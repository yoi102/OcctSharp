namespace OcctSharp;

/// <summary>A copied OCCT triangulation node with its transformed normal and optional UV.</summary>
public readonly record struct DetailedMeshVertex(
    double X,
    double Y,
    double Z,
    double NormalX,
    double NormalY,
    double NormalZ,
    double U,
    double V,
    bool HasUv);

/// <summary>A copied triangle mapped to its zero-based source face.</summary>
public readonly record struct DetailedMeshTriangle(
    int VertexA,
    int VertexB,
    int VertexC,
    int FaceIndex,
    bool IsReversed);

/// <summary>Owns a complete caller-side copy of OCCT's face triangulations.</summary>
public sealed class DetailedMeshSnapshot
{
    internal DetailedMeshSnapshot(
        DetailedMeshVertex[] vertices,
        DetailedMeshTriangle[] triangles,
        int faceCount)
    {
        Vertices = vertices;
        Triangles = triangles;
        FaceCount = faceCount;
    }

    /// <summary>Gets the copied triangulation nodes.</summary>
    public IReadOnlyList<DetailedMeshVertex> Vertices { get; }

    /// <summary>Gets copied triangles with their source-face mappings.</summary>
    public IReadOnlyList<DetailedMeshTriangle> Triangles { get; }

    /// <summary>Gets the number of source faces, including faces without triangulation.</summary>
    public int FaceCount { get; }

    /// <summary>Gets the number of copied triangles.</summary>
    public int TriangleCount => Triangles.Count;

    /// <summary>Gets whether at least one copied node has an OCCT UV value.</summary>
    public bool HasUv => Vertices.Any(static vertex => vertex.HasUv);
}
