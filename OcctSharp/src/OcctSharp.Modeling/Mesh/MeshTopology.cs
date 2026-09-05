using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public sealed class ExistingMeshSnapshot
{
    internal ExistingMeshSnapshot(AuthoredMesh mesh, IEnumerable<string> disclosures) =>
        (Mesh, Disclosures) = (mesh, Array.AsReadOnly(disclosures.ToArray()));
    public AuthoredMesh Mesh { get; }
    public IReadOnlyList<string> Disclosures { get; }
}

/// <summary>Owns a discrete face and copied group/provenance metadata; never claims to be an exact CAD solid.</summary>
public sealed class DiscreteMeshModel : IDisposable
{
    private readonly Shape shape;
    private readonly AuthoredMesh metadata;
    internal DiscreteMeshModel(Shape shape, AuthoredMesh metadata) => (this.shape, this.metadata) = (shape, metadata);
    public MeshRevision Revision => metadata.Revision;
    public bool IsDisposed => shape.Handle.IsClosed;
    /// <summary>Returns an independent topology/triangulation copy, not a mutable alias of this model.</summary>
    public Shape CopyShape() => MeshTopology.CopyWithTriangulation(shape);
    public AuthoredMesh Snapshot()
    {
        ExistingMeshSnapshot read = MeshTopology.SnapshotExisting(shape, metadata.Coordinates);
        AuthoredMesh mesh = read.Mesh;
        if (mesh.Positions.Count != metadata.Positions.Count || mesh.Triangles.Count != metadata.Triangles.Count)
            throw new OcctException("InvalidMeshResult", "Discrete model topology changed unexpectedly.");
        return new(mesh.Positions, mesh.Triangles.Select((t, i) => t with { Group = metadata.Triangles[i].Group }),
            mesh.Normals, mesh.UVs, metadata.Groups, metadata.Polylines, metadata.Coordinates, metadata.Revision, metadata.VertexOrigins);
    }
    public void Dispose() => shape.Dispose();
}

/// <summary>Owning discrete-face and independent exact-face mesh-cache adapters. Snapshots never invoke remeshing.</summary>
public static class MeshTopology
{
    public static DiscreteMeshModel Create(AuthoredMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh); return new(CreateFace(mesh), mesh);
    }
    public static unsafe Shape CreateFace(AuthoredMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (mesh.Positions.Count == 0 || mesh.Triangles.Count == 0) throw new ArgumentException("A discrete face requires vertices and triangles.");
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(mesh); AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(mesh.Triangles);
        OcctRuntime.EnsureCompatible(); nint shape;
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
            NativeError.ThrowIfFailed(NativeMethods.MeshAuthorFace(v, vertices.Length, t, triangles.Length, out shape), "mesh_author_face");
        return ShapeFactory.FromNativeHandle(shape, "mesh_author_face");
    }
    public static Shape CopyWithTriangulation(Shape source)
    {
        Validate(source); NativeError.ThrowIfFailed(NativeMethods.MeshCopyShape(source.Handle, out nint output), "mesh_copy_shape");
        return ShapeFactory.FromNativeHandle(output, "mesh_copy_shape");
    }
    public static bool IsSurfaceBacked(Shape source)
    {
        Validate(source); NativeError.ThrowIfFailed(NativeMethods.MeshIsExact(source.Handle, out int exact), "mesh_is_exact"); return exact != 0;
    }
    public static void RequireSurfaceBacked(Shape source)
    {
        if (!IsSurfaceBacked(source)) throw new NotSupportedException("Triangulation-only geometry cannot be used as an exact surface-backed BRep.");
    }
    public static unsafe ExistingMeshSnapshot SnapshotExisting(Shape source, MeshCoordinates? coordinates = null)
    {
        Validate(source);
        NativeError.ThrowIfFailed(NativeMethods.MeshExistingSnapshot(source.Handle, null, 0, out int vertexCount,
            null, 0, out int triangleCount, out int faceCount), "mesh_existing_snapshot_count");
        if ((uint)vertexCount > MeshDataValidation.MaximumElements || (uint)triangleCount > MeshDataValidation.MaximumElements || faceCount < 0)
            throw new OcctException("InvalidMeshResult", "Existing mesh count is invalid or excessive.");
        AuthoredVertexRaw[] vertices = new AuthoredVertexRaw[vertexCount]; AuthoredTriangleRaw[] triangles = new AuthoredTriangleRaw[triangleCount];
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
        {
            NativeError.ThrowIfFailed(NativeMethods.MeshExistingSnapshot(source.Handle, v, vertices.Length, out int vc,
                t, triangles.Length, out int tc, out int fc), "mesh_existing_snapshot");
            if (vc != vertexCount || tc != triangleCount || fc != faceCount)
                throw new OcctException("InvalidMeshResult", "Existing triangulation changed during bounded snapshot.");
        }
        List<string> disclosures = [];
        bool anyUv = vertices.Any(v => (v.Flags & 2) != 0), allUv = vertices.Length > 0 && vertices.All(v => (v.Flags & 2) != 0);
        bool anyNormal = vertices.Any(v => (v.Flags & 4) != 0);
        if (anyUv && !allUv) disclosures.Add("UV channel omitted because some source faces have no UVs.");
        if (anyNormal && vertices.Any(v => (v.Flags & 1) == 0)) disclosures.Add("Missing or zero source normals are explicitly undefined.");
        AuthoredMesh mesh = new(MeshBuffers.Positions(vertices), MeshBuffers.Triangles(triangles),
            anyNormal ? MeshBuffers.Normals(vertices) : null, allUv ? MeshBuffers.Uvs(vertices) : null,
            Enumerable.Range(0, faceCount).Select(i => new MeshGroup(i, $"Face {i}", SourceKey: $"face:{i}")), coordinates: coordinates);
        return new(mesh, disclosures);
    }
    /// <summary>Replaces one exact face cache on a private copy. Replacement nodes are in that face's local coordinate system.</summary>
    public static unsafe Shape ReplaceTriangulation(Shape source, int faceIndex, AuthoredMesh replacement)
    {
        Validate(source); ArgumentNullException.ThrowIfNull(replacement); ArgumentOutOfRangeException.ThrowIfNegative(faceIndex);
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(replacement); AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(replacement.Triangles);
        nint result;
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
            NativeError.ThrowIfFailed(NativeMethods.MeshReplaceFace(source.Handle, faceIndex, v, vertices.Length, t, triangles.Length, out result), "mesh_replace_face");
        return ShapeFactory.FromNativeHandle(result, "mesh_replace_face");
    }
    public static unsafe Shape Remesh(Shape source, IEnumerable<int> faceIndices, double linearDeflection = 0.1, double angularDeflection = 0.5)
    {
        Validate(source); int[] indices = MeshDataValidation.Copy(faceIndices, nameof(faceIndices));
        if (indices.Length == 0 || indices.Any(i => i < 0) || indices.Distinct().Count() != indices.Length ||
            !double.IsFinite(linearDeflection) || linearDeflection <= 0 || !double.IsFinite(angularDeflection) || angularDeflection <= 0 || angularDeflection > Math.PI)
            throw new ArgumentException("Remeshing requires unique nonnegative face indices and finite positive tolerances.");
        nint result;
        fixed (int* i = indices)
            NativeError.ThrowIfFailed(NativeMethods.MeshRemeshFaces(source.Handle, i, indices.Length, linearDeflection, angularDeflection, out result), "mesh_remesh_faces");
        return ShapeFactory.FromNativeHandle(result, "mesh_remesh_faces");
    }
    public static Shape CreatePolyline(MeshPolyline3d polyline)
    {
        ArgumentNullException.ThrowIfNull(polyline);
        AuthoredMesh mesh = new(polyline.Points, []);
        return CreatePolyline(mesh, new MeshPolyline(Enumerable.Range(0, polyline.Points.Count)
            .Select(i => polyline.IsClosed && i == polyline.Points.Count - 1 ? 0 : i), polyline.IsClosed, polyline.Parameters));
    }
    public static unsafe Shape CreatePolyline(AuthoredMesh mesh, MeshPolyline polyline)
    {
        ArgumentNullException.ThrowIfNull(mesh); ArgumentNullException.ThrowIfNull(polyline);
        MeshDataValidation.Indices(polyline.Indices, mesh.Positions.Count);
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(mesh); int[] indices = [.. polyline.Indices]; double[] parameters = polyline.Parameters?.ToArray() ?? [];
        OcctRuntime.EnsureCompatible(); nint result;
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (int* i = indices)
        fixed (double* p = parameters)
            NativeError.ThrowIfFailed(NativeMethods.MeshPolyline(v, vertices.Length, i, indices.Length, p, parameters.Length, out result), "mesh_polyline");
        return ShapeFactory.FromNativeHandle(result, "mesh_polyline");
    }
    private static void Validate(Shape source)
    {
        ArgumentNullException.ThrowIfNull(source); ObjectDisposedException.ThrowIf(source.Handle.IsClosed, source); OcctRuntime.EnsureCompatible();
    }
}
