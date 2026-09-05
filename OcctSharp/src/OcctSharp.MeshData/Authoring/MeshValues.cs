namespace OcctSharp;

#pragma warning disable CS1591

public readonly record struct MeshRevision(Guid Id, long Number);
public readonly record struct MeshVertexOrigin(Guid SourceId, int Index);
public readonly record struct MeshUv(double U, double V);
public readonly record struct MeshNormal(double X, double Y, double Z, bool IsDefined = true)
{
    public static MeshNormal Undefined => new(0, 0, 0, false);
    public MeshNormal Normalized()
    {
        if (!IsDefined) return Undefined;
        double scale = Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));
        if (!double.IsFinite(scale) || scale == 0) throw new ArgumentException("A defined normal must be finite and nonzero.");
        double x = X / scale, y = Y / scale, z = Z / scale;
        double length = Math.Sqrt(x * x + y * y + z * z);
        return new(x / length, y / length, z / length);
    }
}
public readonly record struct MeshTriangle(int A, int B, int C, int Group = 0)
{
    public int this[int corner] => corner switch { 0 => A, 1 => B, 2 => C, _ => throw new ArgumentOutOfRangeException(nameof(corner)) };
    public MeshTriangle Reversed() => new(A, C, B, Group);
}
/// <summary>Opaque copied material/provenance references; no XDE or Modeling dependency.</summary>
public sealed record MeshGroup(int Key, string Name, string? MaterialKey = null, string? SourceKey = null);
public enum MeshUpAxis { Z, Y }
public enum MeshHandedness { Right, Left }
/// <summary>Units are metres per coordinate unit; conversion changes this metadata once.</summary>
public sealed record MeshCoordinates(double MetresPerUnit = 0.001, MeshUpAxis UpAxis = MeshUpAxis.Z,
    MeshHandedness Handedness = MeshHandedness.Right)
{
    internal void Validate()
    {
        if (!double.IsFinite(MetresPerUnit) || MetresPerUnit <= 0 ||
            !Enum.IsDefined(UpAxis) || !Enum.IsDefined(Handedness))
            throw new ArgumentException("Mesh coordinates require positive finite units and known axes/handedness.");
    }
}
/// <summary>Copied row-major 3x4 affine transform; translation uses the mesh coordinate unit.</summary>
public readonly record struct AuthoredMeshTransform(
    double M11, double M12, double M13, double M14,
    double M21, double M22, double M23, double M24,
    double M31, double M32, double M33, double M34)
{
    public static AuthoredMeshTransform Identity => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
    public static AuthoredMeshTransform Translation(double x, double y, double z) => Identity with { M14 = x, M24 = y, M34 = z };
    public static AuthoredMeshTransform Scale(double scale) => new(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0);
    public double Determinant => M11 * (M22 * M33 - M23 * M32) - M12 * (M21 * M33 - M23 * M31) + M13 * (M21 * M32 - M22 * M31);
    internal double[] Values => [M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34];
}
public readonly record struct MeshUvTransform(double M11, double M12, double M13, double M21, double M22, double M23)
{
    public static MeshUvTransform Identity => new(1, 0, 0, 0, 1, 0);
}
public readonly record struct AuthoredMeshBounds(GpPoint Minimum, GpPoint Maximum);
public sealed record MeshRegionStatistics(MeshRevision Revision, int TriangleCount, int VertexCount,
    double SurfaceArea, AuthoredMeshBounds? Bounds);

/// <summary>Copied whole-mesh statistics for authored data; closedness is indexed mesh incidence, not exact solid validity.</summary>
public sealed record AuthoredMeshStatistics(MeshRevision Revision, int VertexCount, int TriangleCount, int GroupCount,
    double SurfaceArea, AuthoredMeshBounds? Bounds, int ComponentCount, int BoundaryEdgeCount,
    int NonManifoldEdgeCount, int DegenerateTriangleCount)
{
    public bool IsClosedManifold => TriangleCount > 0 && BoundaryEdgeCount == 0 && NonManifoldEdgeCount == 0 && DegenerateTriangleCount == 0;
}

internal static class MeshDataValidation
{
    internal const int MaximumElements = 5_000_000;
    internal static T[] Copy<T>(IEnumerable<T> values, string name, int maximum = MaximumElements)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.TryGetNonEnumeratedCount(out int count) && count > maximum)
            throw new ArgumentException($"{name} exceeds the bounded mesh element limit ({maximum}).", name);
        List<T> result = new(Math.Min(count, maximum));
        foreach (T value in values)
        {
            if (result.Count == maximum) throw new ArgumentException($"{name} exceeds the bounded mesh element limit ({maximum}).", name);
            result.Add(value);
        }
        return result.ToArray();
    }
    internal static void Point(GpPoint point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            throw new ArgumentException("Mesh positions must be finite.");
    }
    internal static void Indices(IEnumerable<int> indices, int count)
    {
        foreach (int index in indices)
            if ((uint)index >= (uint)count) throw new ArgumentOutOfRangeException(nameof(indices), "Mesh indices must be zero-based and in range.");
    }
    internal static void Parameters(double[]? parameters, int count)
    {
        if (parameters is null) return;
        if (parameters.Length != count) throw new ArgumentException("Polyline parameter cardinality must match its nodes.");
        for (int i = 0; i < count; ++i)
            if (!double.IsFinite(parameters[i]) || (i > 0 && parameters[i] <= parameters[i - 1]))
                throw new ArgumentException("Polyline parameters must be finite and strictly increasing.");
    }
}
