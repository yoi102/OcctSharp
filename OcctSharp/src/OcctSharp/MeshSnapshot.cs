namespace OcctSharp;

/// <summary>A copied vertex and unit face normal from an OCCT triangulation.</summary>
public readonly record struct MeshVertex(
    double X,
    double Y,
    double Z,
    double NormalX,
    double NormalY,
    double NormalZ);

/// <summary>Owns caller-side copies of a triangulated shape's vertices and triangle indices.</summary>
public sealed class MeshSnapshot
{
    internal MeshSnapshot(MeshVertex[] vertices, int[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }

    /// <summary>Gets the copied vertex records. Every three vertices form one triangle.</summary>
    public IReadOnlyList<MeshVertex> Vertices { get; }

    /// <summary>Gets triangle indices into <see cref="Vertices"/>.</summary>
    public IReadOnlyList<int> Indices { get; }

    /// <summary>Gets the number of triangles in this snapshot.</summary>
    public int TriangleCount => Indices.Count / 3;
}
