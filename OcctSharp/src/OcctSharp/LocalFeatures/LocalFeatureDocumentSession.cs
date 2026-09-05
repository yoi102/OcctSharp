using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Edits a shared definition in definition coordinates through an explicit occurrence path.
/// All repeated placements remain unchanged; ambiguous subshape metadata blocks publication.</summary>
public sealed class LocalFeatureDocumentSession : IDisposable
{
    private readonly XdeDocument document;
    private readonly XdeLabel assembly, definition;
    private readonly string originalFingerprint, contextFingerprint;
    private bool disposed, published;
    public RepairSnapshot Source { get; }
    public IReadOnlyList<string> OccurrencePath { get; }
    public string DefinitionEntry => definition.Entry;

    public LocalFeatureDocumentSession(XdeDocument document, XdeLabel assembly, IReadOnlyList<string> occurrencePath)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(assembly);
        this.document = document; this.assembly = assembly;
        OccurrencePath = Array.AsReadOnly(ScalarLawDefinition.Copy(occurrencePath, 256));
        using var resolved = document.ResolveOccurrencePath(assembly, OccurrencePath);
        definition = resolved.Definition;
        if (definition.IsAssembly) throw new ArgumentException("Local features require a simple shared part definition.");
        using var shape = definition.Shape;
        originalFingerprint = RepairSnapshot.ComputeFingerprint(shape);
        contextFingerprint = RepairSnapshot.ComputeFingerprint(resolved.LocatedShape);
        Source = RepairSnapshot.Create(shape);
    }
    public RepairMetadataReview Review(LocalFeatureResult result, RepairBudget? budget = null, IEnumerable<RepairSelection>? protectedTopology = null)
    {
        ValidateCurrent();
        using var acceptance = LocalFeatureAcceptance.Inspect(Source, result, budget, protectedTopology);
        if (!acceptance.CanAccept) throw new InvalidOperationException("Local-feature geometry violates its acceptance policy.");
        return Map(result, false);
    }
    /// <summary>Creates source subshape metadata inside the caller's open document transaction.</summary>
    public unsafe XdeLabel GetOrCreateSubshapeLabel(RepairSelection selection)
    {
        ValidateCurrent(); Source.Validate(selection); byte[] bytes = new byte[1024];
        fixed (byte* buffer = bytes)
        {
            NativeError.ThrowIfFailed(NativeMethods.RepairXdeSubshapeLabel(document.Handle, definition.Entry,
                selection.Index, buffer, bytes.Length, out int count), "local_feature_subshape_label");
            return document.GetLabel(System.Text.Encoding.UTF8.GetString(bytes, 0, count));
        }
    }
    public RepairMetadataReview Publish(LocalFeatureResult result, RepairBudget? budget = null, IEnumerable<RepairSelection>? protectedTopology = null)
    {
        var review = Review(result, budget, protectedTopology);
        if (!review.CanPublish) throw new InvalidOperationException("Ambiguous or missing exact subshape history prevents metadata publication.");
        using var command = document.BeginTransaction("Publish local feature shared definition");
        ValidateCurrent(); review = Map(result, true); command.Commit(); published = true; return review;
    }
    private unsafe RepairMetadataReview Map(LocalFeatureResult result, bool apply)
    {
        var relations = result.History.Where(h => h.Source is { ArgumentIndex: 0 } && h.Kind is
            LocalFeatureHistoryKind.Unchanged or LocalFeatureHistoryKind.Modified or LocalFeatureHistoryKind.Generated
            or LocalFeatureHistoryKind.Deleted or LocalFeatureHistoryKind.Unmapped).Select(h => new RepairRelationRaw(
                h.Source!.Value.TopologyIndex, h.ResultTopologyIndex ?? -1, h.Kind switch
                {
                    LocalFeatureHistoryKind.Unchanged => 0, LocalFeatureHistoryKind.Modified => 1,
                    LocalFeatureHistoryKind.Generated => 2, LocalFeatureHistoryKind.Deleted => 3, _ => 4
                }, 0)).Distinct().ToArray();
        int[] conflicts = new int[Source.Topology.Count + 1];
        fixed (RepairRelationRaw* history = relations) fixed (int* rejected = conflicts)
        {
            NativeError.ThrowIfFailed(NativeMethods.RepairXdeApply(document.Handle, definition.Entry, result.RequireShape().Handle,
                history, relations.Length, apply ? 1 : 0, rejected, conflicts.Length, out int count, out int mapped, out int users), "local_feature_publication");
            return new(mapped, users, Array.AsReadOnly(conflicts.Take(count).ToArray()));
        }
    }
    private void ValidateCurrent()
    {
        ObjectDisposedException.ThrowIf(disposed, this); document.ThrowIfDisposed();
        if (published) throw new InvalidOperationException("This local-feature session has already published.");
        using var resolved = document.ResolveOccurrencePath(assembly, OccurrencePath); using var current = definition.Shape;
        if (resolved.Definition.Entry != definition.Entry || RepairSnapshot.ComputeFingerprint(current) != originalFingerprint
            || RepairSnapshot.ComputeFingerprint(resolved.LocatedShape) != contextFingerprint)
            throw new InvalidOperationException("The occurrence context or shared definition changed; create a new session.");
    }
    public void Dispose() { if (disposed) return; disposed = true; Source.Dispose(); }
}

public sealed record LocalFeatureExchangeResult(string Path, string Disclosure);
public static class LocalFeatureDelivery
{
    /// <summary>Writes valid exact output and supported product metadata, not an executable recipe or history.</summary>
    public static LocalFeatureExchangeResult Export(LocalFeatureResult result, string path, XdeExchangeFormat format,
        string name = "Local feature", XdeColor? color = null)
    {
        ArgumentNullException.ThrowIfNull(result); if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        using var document = XdeDocument.Create();
        using (var command = document.BeginTransaction("Stage local-feature exchange"))
        {
            var label = document.AddShape(result.RequireShape(), name); if (color is not null) label.Color = color; command.Commit();
        }
        return new(document.WriteExchange(path, format), "Exact geometry and supported name/color. No executable recipe, diagnostic sections or persistent feature history.");
    }
}
#pragma warning restore CS1591
