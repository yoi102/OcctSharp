namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>An immutable polygon referencing mesh nodes. Closed polygons repeat the first node.</summary>
public sealed class MeshPolyline
{
    public MeshPolyline(IEnumerable<int> indices, bool closed = false, IEnumerable<double>? parameters = null)
    {
        int[] nodes = MeshDataValidation.Copy(indices, nameof(indices));
        double[]? values = parameters is null ? null : MeshDataValidation.Copy(parameters, nameof(parameters));
        if (nodes.Length < (closed ? 4 : 2) || nodes.Any(i => i < 0) ||
            (closed && nodes[0] != nodes[^1]) || (!closed && nodes[0] == nodes[^1]))
            throw new ArgumentException("Polyline closedness and repeated endpoint must agree; at least two points or three distinct closed vertices are required.");
        if (closed && nodes.Distinct().Count() < 3) throw new ArgumentException("A closed polygon needs three distinct node indices.");
        MeshDataValidation.Parameters(values, nodes.Length);
        Indices = Array.AsReadOnly(nodes); Parameters = values is null ? null : Array.AsReadOnly(values); IsClosed = closed;
    }
    public IReadOnlyList<int> Indices { get; }
    public IReadOnlyList<double>? Parameters { get; }
    public bool IsClosed { get; }
}
/// <summary>An independent copied 3D polyline with explicit parameterization and endpoint contract.</summary>
public sealed class MeshPolyline3d
{
    public MeshPolyline3d(IEnumerable<GpPoint> points, bool closed = false, IEnumerable<double>? parameters = null)
    {
        GpPoint[] data = MeshDataValidation.Copy(points, nameof(points));
        foreach (GpPoint point in data) MeshDataValidation.Point(point);
        double[]? values = parameters is null ? null : MeshDataValidation.Copy(parameters, nameof(parameters));
        if (data.Length < (closed ? 4 : 2) || (closed != (data[0] == data[^1])))
            throw new ArgumentException("The repeated polyline endpoint must agree with closedness.");
        if (closed && data.Distinct().Count() < 3) throw new ArgumentException("A closed polyline needs three distinct points.");
        MeshDataValidation.Parameters(values, data.Length);
        Points = Array.AsReadOnly(data); Parameters = values is null ? null : Array.AsReadOnly(values); IsClosed = closed;
    }
    public IReadOnlyList<GpPoint> Points { get; }
    public IReadOnlyList<double>? Parameters { get; }
    public bool IsClosed { get; }
}
