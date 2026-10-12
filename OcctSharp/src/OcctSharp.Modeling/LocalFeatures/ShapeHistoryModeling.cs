using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Independent exact topology transforms and compounds with complete native provenance.</summary>
public static class ShapeHistoryModeling
{
    /// <summary>Copies and transforms one snapshot by a rigid placement or exact reflection.
    /// Every face, edge and vertex is related through the actual builder's ModifiedShape.
    /// Source indices belong to this snapshot; result indices belong to the returned full map.
    /// The input and transform may be disposed independently after the call.</summary>
    public static LocalFeatureResult Transform(RepairSnapshot source, GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(transform);
        transform.ThrowIfDisposedForLocation();
        NativeError.ThrowIfFailed(NativeMethods.ShapeTransformHistory(source.Shape.Handle, transform.Handle, out nint result),
            "shape_transform_history");
        return LocalFeatureBridge.Read(Guid.NewGuid(), result, source.Identity, source.Fingerprint);
    }

    /// <summary>Copies one to 256 snapshots as one private graph and creates a compound.
    /// Native membership proves every face/edge/vertex correspondence for each ordered input.
    /// Shared targets retain each source argument; no first-source or geometric matching is used.
    /// Results and their history shapes outlive every supplied snapshot.</summary>
    public static unsafe LocalFeatureResult Compound(IReadOnlyList<RepairSnapshot> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        if (inputs.Count is < 1 or > 256) throw new ArgumentException("Expected one to 256 snapshots.", nameof(inputs));
        var shapes = new Shape[inputs.Count];
        for (int i = 0; i < shapes.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(inputs[i]);
            shapes[i] = inputs[i].Shape;
        }
        return AuthoringBridge.WithInputs(shapes, (p, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.ShapeCompoundHistory(p, count, out nint result), "shape_compound_history");
            return LocalFeatureBridge.Read(Guid.NewGuid(), result);
        });
    }
}
