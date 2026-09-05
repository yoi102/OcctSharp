using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public sealed record ParametricPublicationReview(Guid FeatureId, Guid ResultRevision, int SharedOccurrences,
    IReadOnlyList<int> ConflictingSubshapes)
{
    public bool CanPublish => ConflictingSubshapes.Count == 0;
}
public sealed record ParametricExchangeResult(string Path, Guid ResultRevision, string Disclosure);

public sealed partial class ParametricDocument
{
    /// <summary>Checks shared-definition replacement. Unmapped subshape metadata is a conflict, never silently discarded.</summary>
    public ParametricPublicationReview ReviewDefinition(Guid feature, XdeDocument document, XdeLabel definition)
    {
        using var result = ExactResult(feature);
        return Publication(result, document, definition, false);
    }

    public ParametricPublicationReview PublishDefinition(Guid feature, XdeDocument document, XdeLabel definition)
    {
        using var result = ExactResult(feature);
        var review = Publication(result, document, definition, false);
        if (!review.CanPublish) throw new InvalidOperationException("Subshape metadata has no verified topology mapping; publication was not applied.");
        using var transaction = document.BeginTransaction("Publish recomputed shared definition");
        review = Publication(result, document, definition, true);
        transaction.Commit(); return review;
    }

    /// <summary>Delivers supported exact shape/name/color only; STEP/IGES does not persist the parametric graph.</summary>
    public ParametricExchangeResult Export(Guid feature, string path, XdeExchangeFormat format, string? name = null, XdeColor? color = null)
    {
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        using var result = ExactResult(feature);
        using var document = XdeDocument.Create();
        using (var command = document.BeginTransaction("Stage recomputed exact exchange"))
        {
            var label = document.AddShape(result.Shape!, name ?? Get(ReadFeatures(), feature).Definition.Name);
            if (color is not null) label.Color = color;
            command.Commit();
        }
        return new(document.WriteExchange(path, format), result.Revision,
            "Exact geometry and supported name/color only. No executable feature graph, naming history or discrete-only output guarantee.");
    }

    private ParametricResult ExactResult(Guid feature)
    {
        var result = GetResult(feature);
        try
        {
            if (result.Kind != ParametricOutputKind.ExactShape) throw new NotSupportedException("This operation requires a current exact topology output.");
            RequireExactTopology(result.Shape!); return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void RequireExactTopology(Shape shape)
    {
        shape.ThrowIfDisposed();
        if (shape.IsNull) throw new ArgumentException("Null topology is not an exact parametric source.");
        if (shape.FaceCount > 0) { MeshTopology.RequireSurfaceBacked(shape); return; }
        // Exact wires/edges/vertices are valid profile and guide inputs without faces.
        var edges = shape.GetSubShapes(ShapeKind.Edge);
        try { foreach (var edge in edges) _ = edge.GetEdgeCurveSnapshot(); }
        finally { foreach (var edge in edges) edge.Dispose(); }
    }

    private static unsafe ParametricPublicationReview Publication(ParametricResult result, XdeDocument document, XdeLabel definition, bool apply)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(definition);
        document.ThrowIfDisposed();
        if (!ReferenceEquals(document, definition.Document)) throw new ArgumentException("Definition belongs to another document.");
        NativeError.ThrowIfFailed(NativeMethods.RepairXdeApply(document.Handle, definition.Entry, result.Shape!.Handle,
            null, 0, 0, null, 0, out int count, out _, out int users), "parametric_publication_review");
        if (count is < 0 or > 1_000_000) throw new InvalidOperationException("Metadata mapping is excessive.");
        int[] conflicts = new int[count];
        fixed (int* p = conflicts)
            NativeError.ThrowIfFailed(NativeMethods.RepairXdeApply(document.Handle, definition.Entry, result.Shape.Handle,
                null, 0, apply ? 1 : 0, p, count, out _, out _, out users), "parametric_publication");
        return new(result.FeatureId, result.Revision, users, Array.AsReadOnly(conflicts));
    }
}
#pragma warning restore CS1591
