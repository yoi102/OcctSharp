using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public sealed record RepairMetadataReview(int MappedLabels, int SharedOccurrences, IReadOnlyList<int> ConflictingSourceIndices)
{
    public bool CanPublish => ConflictingSourceIndices.Count == 0;
}

/// <summary>One shared-definition repair. Geometry/metadata changes and occurrence placements commit as one document command.</summary>
public sealed class RepairDocumentSession : IDisposable
{
    private readonly XdeDocument document;
    private readonly XdeLabel definition;
    private readonly string originalFingerprint;
    private bool disposed, published;
    public RepairSnapshot Source { get; }
    public string DefinitionEntry => definition.Entry;

    public RepairDocumentSession(XdeDocument document, XdeLabel definition, string unit = "mm", long revision = 0,
        RepairInspectionOptions? inspection = null)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(definition);
        if (!ReferenceEquals(definition.Document, document)) throw new ArgumentException("Definition belongs to another document.");
        if (definition.IsAssembly) throw new ArgumentException("Repair a simple shared definition, not an assembly.");
        this.document = document; this.definition = definition;
        using Shape shape = definition.Shape; originalFingerprint = RepairSnapshot.ComputeFingerprint(shape);
        Source = RepairSnapshot.Create(shape, unit, revision, inspection);
    }
    public RepairMetadataReview Review(RepairPreview preview) => ReviewCore(preview, false);
    /// <summary>Creates/reuses metadata on a source subshape inside an already open document transaction.</summary>
    public unsafe XdeLabel GetOrCreateSubshapeLabel(RepairSelection selection)
    {
        ObjectDisposedException.ThrowIf(disposed, this); Source.Validate(selection); ValidateCurrent();
        byte[] buffer = new byte[1024];
        fixed (byte* entry = buffer)
        {
            NativeError.ThrowIfFailed(NativeMethods.RepairXdeSubshapeLabel(document.Handle, definition.Entry,
                selection.Index, entry, buffer.Length, out int count), "repair_xde_subshape_label");
            return document.GetLabel(System.Text.Encoding.UTF8.GetString(buffer, 0, count));
        }
    }
    public RepairMetadataReview Publish(RepairPreview preview, string commandName = "Repair shared definition")
    {
        RepairMetadataReview review = Review(preview);
        if (!review.CanPublish) throw new InvalidOperationException("Metadata mapping has conflicts; the document was not changed.");
        using XdeTransaction transaction = document.BeginTransaction(commandName);
        review = ReviewCore(preview, true);
        transaction.Commit(); preview.MarkAccepted(); published = true;
        return review;
    }
    private unsafe RepairMetadataReview ReviewCore(RepairPreview preview, bool apply)
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(preview); preview.EnsureAcceptable();
        if (published || preview.Plan.Source != Source.Identity) throw new ArgumentException("Foreign or stale document repair preview.");
        ValidateCurrent();
        RepairRelationRaw[] history = preview.History.Select(value => new RepairRelationRaw(value.Source.Index,
            value.Result?.Index ?? -1, (int)value.Kind, 0)).ToArray();
        int[] conflicts = new int[Source.Topology.Count + 1];
        fixed (RepairRelationRaw* relations = history)
        fixed (int* rejected = conflicts)
        {
            NativeError.ThrowIfFailed(NativeMethods.RepairXdeApply(document.Handle, definition.Entry, preview.Result!.Shape.Handle,
                relations, history.Length, apply ? 1 : 0, rejected, conflicts.Length, out int count, out int mapped, out int users), "repair_xde_apply");
            return new(mapped, users, Array.AsReadOnly(conflicts.Take(count).ToArray()));
        }
    }
    private void ValidateCurrent()
    {
        if (published) throw new InvalidOperationException("This document repair session was already published.");
        using Shape current = definition.Shape;
        if (RepairSnapshot.ComputeFingerprint(current) != originalFingerprint)
            throw new InvalidOperationException("The shared definition changed after the repair snapshot.");
    }
    public void Dispose() { if (disposed) return; disposed = true; Source.Dispose(); }
}
