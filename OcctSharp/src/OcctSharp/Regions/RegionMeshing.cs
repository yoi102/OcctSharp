namespace OcctSharp;

#pragma warning disable CS1591
public sealed record RegionMeshGroup(int Group, RegionCellId Cell, RegionBoundaryId? Boundary, int Material);
public sealed record RegionMeshResult(AuthoredMesh Mesh, IReadOnlyList<RegionMeshGroup> Groups);

public static class RegionMeshing
{
    /// <summary>Meshes selected exact cells, carrying exact shared-face IDs in copied triangle groups.</summary>
    public static RegionMeshResult Create(PartitionResult partition, string outputKey, AdvancedMeshOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(partition);
        List<GpPoint> vertices = []; List<MeshNormal> normals = []; List<MeshTriangle> triangles = [];
        List<MeshGroup> meshGroups = []; List<RegionMeshGroup> groups = [];
        foreach (var assignment in partition.GetAssignments(outputKey))
        {
            if (assignment.Dimension != 3) throw new NotSupportedException("Region volume meshing requires solid cells; use existing curve/surface meshing for other dimensions.");
            Shape[] graph = partition.CopyCellBoundaryGraph(assignment.Cell, out var boundaries);
            Shape[] faces = [];
            try
            {
                faces = graph[0].GetFaces(); var mesh = AdvancedMesh.Create(graph[0], options);
                int offset = vertices.Count; vertices.AddRange(mesh.Vertices.Select(v => new GpPoint(v.X, v.Y, v.Z)));
                normals.AddRange(mesh.Vertices.Select(v => v.NormalX == 0 && v.NormalY == 0 && v.NormalZ == 0 ? MeshNormal.Undefined : new MeshNormal(v.NormalX, v.NormalY, v.NormalZ)));
                foreach (var group in mesh.Groups)
                {
                    int key = groups.Count; RegionBoundaryId? boundary = null;
                    if (group.FaceIndex < 0 || group.FaceIndex >= faces.Length) throw new InvalidOperationException("Mesher face index is outside exact cell topology.");
                    for (int i = 0; i < boundaries.Length; i++) if (faces[group.FaceIndex].IsSame(graph[i + 1])) { boundary = boundaries[i]; break; }
                    if (boundary is null) throw new InvalidOperationException("An exact cell face has no interface correspondence.");
                    groups.Add(new(key, assignment.Cell, boundary, assignment.Material));
                    meshGroups.Add(new(key, $"cell-{assignment.Cell.Index}/boundary-{boundary.Value.Index}", $"region-material-{assignment.Material}"));
                    foreach (var triangle in mesh.Triangles.Where(t => t.FaceIndex == group.FaceIndex))
                        triangles.Add(new(triangle.VertexA + offset, triangle.VertexB + offset, triangle.VertexC + offset, key));
                }
            }
            finally { foreach (var face in faces) face.Dispose(); foreach (var shape in graph) shape.Dispose(); }
        }
        return new(new AuthoredMesh(vertices, triangles, normals, groups: meshGroups), groups.AsReadOnly());
    }
}
