namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>
/// Immutable managed-owned mesh data. No native resource needs disposal. Optional channels are
/// absent or full-cardinality; a zero normal is valid only when explicitly undefined.
/// </summary>
public sealed class AuthoredMesh
{
    public AuthoredMesh(IEnumerable<GpPoint> positions, IEnumerable<MeshTriangle> triangles,
        IEnumerable<MeshNormal>? normals = null, IEnumerable<MeshUv>? uvs = null,
        IEnumerable<MeshGroup>? groups = null, IEnumerable<MeshPolyline>? polylines = null,
        MeshCoordinates? coordinates = null)
        : this(positions, triangles, normals, uvs, groups, polylines, coordinates,
            new MeshRevision(Guid.NewGuid(), 0), null) { }

    internal AuthoredMesh(IEnumerable<GpPoint> positions, IEnumerable<MeshTriangle> triangles,
        IEnumerable<MeshNormal>? normals, IEnumerable<MeshUv>? uvs, IEnumerable<MeshGroup>? groups,
        IEnumerable<MeshPolyline>? polylines, MeshCoordinates? coordinates, MeshRevision revision,
        IEnumerable<MeshVertexOrigin>? origins)
    {
        GpPoint[] points = MeshDataValidation.Copy(positions, nameof(positions));
        MeshTriangle[] facets = MeshDataValidation.Copy(triangles, nameof(triangles));
        _ = checked(facets.Length * 3);
        foreach (GpPoint point in points) MeshDataValidation.Point(point);
        MeshNormal[]? normalData = normals is null ? null : MeshDataValidation.Copy(normals, nameof(normals));
        MeshUv[]? uvData = uvs is null ? null : MeshDataValidation.Copy(uvs, nameof(uvs));
        if ((normalData is not null && normalData.Length != points.Length) ||
            (uvData is not null && uvData.Length != points.Length))
            throw new ArgumentException("Normals and UVs must be absent or have exactly one value per mesh vertex.");
        if (normalData is not null)
            foreach (MeshNormal normal in normalData)
            {
                if (!double.IsFinite(normal.X) || !double.IsFinite(normal.Y) || !double.IsFinite(normal.Z))
                    throw new ArgumentException("Normals must be finite, including explicitly undefined values.");
                if (normal.IsDefined) _ = normal.Normalized();
                else if (normal.X != 0 || normal.Y != 0 || normal.Z != 0)
                    throw new ArgumentException("Undefined normals use zero coordinates and an explicit flag.");
            }
        if (uvData is not null && uvData.Any(uv => !double.IsFinite(uv.U) || !double.IsFinite(uv.V)))
            throw new ArgumentException("UV coordinates must be finite.");
        MeshGroup[] groupData = groups is null ? [new(0, "Default")] : MeshDataValidation.Copy(groups, nameof(groups));
        HashSet<int> groupKeys = [];
        foreach (MeshGroup group in groupData)
            if (group is null || group.Key < 0 || !groupKeys.Add(group.Key) || group.Name is null)
                throw new ArgumentException("Mesh group keys must be unique nonnegative integers with a copied name.");
        foreach (MeshTriangle triangle in facets)
        {
            MeshDataValidation.Indices([triangle.A, triangle.B, triangle.C], points.Length);
            if (!groupKeys.Contains(triangle.Group)) throw new ArgumentException("Every triangle must reference an existing group.");
        }
        MeshPolyline[] lines = polylines is null ? [] : MeshDataValidation.Copy(polylines, nameof(polylines));
        int totalLineIndices = 0;
        foreach (MeshPolyline line in lines)
        {
            ArgumentNullException.ThrowIfNull(line);
            totalLineIndices = checked(totalLineIndices + line.Indices.Count);
            if (totalLineIndices > MeshDataValidation.MaximumElements)
                throw new ArgumentException("The total polyline index count exceeds the bounded mesh limit.");
            MeshDataValidation.Indices(line.Indices, points.Length);
        }
        MeshVertexOrigin[] source = origins is null
            ? Enumerable.Range(0, points.Length).Select(i => new MeshVertexOrigin(revision.Id, i)).ToArray()
            : MeshDataValidation.Copy(origins, nameof(origins));
        if (source.Length != points.Length || source.Any(o => o.Index < 0 || o.SourceId == Guid.Empty))
            throw new ArgumentException("Logical source-vertex correspondence must be complete.");
        Coordinates = coordinates ?? new(); Coordinates.Validate();
        Positions = Array.AsReadOnly(points); Triangles = Array.AsReadOnly(facets);
        Normals = normalData is null ? null : Array.AsReadOnly(normalData);
        UVs = uvData is null ? null : Array.AsReadOnly(uvData);
        Groups = Array.AsReadOnly(groupData); Polylines = Array.AsReadOnly(lines);
        VertexOrigins = Array.AsReadOnly(source); Revision = revision;
    }

    public MeshRevision Revision { get; }
    public IReadOnlyList<GpPoint> Positions { get; }
    public IReadOnlyList<MeshTriangle> Triangles { get; }
    public IReadOnlyList<MeshNormal>? Normals { get; }
    public IReadOnlyList<MeshUv>? UVs { get; }
    public IReadOnlyList<MeshGroup> Groups { get; }
    public IReadOnlyList<MeshPolyline> Polylines { get; }
    public IReadOnlyList<MeshVertexOrigin> VertexOrigins { get; }
    public MeshCoordinates Coordinates { get; }
    public MeshTriangleSelection SelectTriangles(IEnumerable<int> indices) => new(this, indices);
    public MeshVertexSelection SelectVertices(IEnumerable<int> indices) => new(this, indices);

    /// <summary>Expands per-corner attributes into separate vertices while retaining logical origins.</summary>
    public static AuthoredMesh FromCorners(IEnumerable<GpPoint> positions, IEnumerable<MeshTriangle> triangles,
        IEnumerable<MeshNormal>? cornerNormals = null, IEnumerable<MeshUv>? cornerUvs = null,
        IEnumerable<MeshGroup>? groups = null, MeshCoordinates? coordinates = null)
    {
        AuthoredMesh source = new(positions, triangles, groups: groups, coordinates: coordinates);
        int count = checked(source.Triangles.Count * 3);
        if (count > MeshDataValidation.MaximumElements) throw new ArgumentException("Expanded corner count exceeds the mesh element limit.");
        MeshNormal[]? normals = cornerNormals is null ? null : MeshDataValidation.Copy(cornerNormals, nameof(cornerNormals));
        MeshUv[]? uvs = cornerUvs is null ? null : MeshDataValidation.Copy(cornerUvs, nameof(cornerUvs));
        if ((normals is not null && normals.Length != count) || (uvs is not null && uvs.Length != count))
            throw new ArgumentException("Per-corner channels require exactly three values per triangle.");
        GpPoint[] points = new GpPoint[count]; MeshVertexOrigin[] origins = new MeshVertexOrigin[count];
        MeshTriangle[] facets = new MeshTriangle[source.Triangles.Count];
        for (int t = 0; t < facets.Length; ++t)
        {
            for (int c = 0; c < 3; ++c)
            {
                int index = source.Triangles[t][c]; points[3 * t + c] = source.Positions[index]; origins[3 * t + c] = source.VertexOrigins[index];
            }
            facets[t] = new(3 * t, 3 * t + 1, 3 * t + 2, source.Triangles[t].Group);
        }
        return new(points, facets, normals, uvs, source.Groups, null, source.Coordinates, new(Guid.NewGuid(), 0), origins);
    }
}

public sealed class MeshTriangleSelection
{
    internal MeshTriangleSelection(AuthoredMesh mesh, IEnumerable<int> indices)
    {
        int[] data = MeshDataValidation.Copy(indices, nameof(indices)).Distinct().Order().ToArray();
        MeshDataValidation.Indices(data, mesh.Triangles.Count);
        Revision = mesh.Revision; Indices = Array.AsReadOnly(data);
    }
    public MeshRevision Revision { get; }
    public IReadOnlyList<int> Indices { get; }
    internal void Validate(AuthoredMesh mesh)
    {
        if (Revision != mesh.Revision) throw new ArgumentException("Triangle selection belongs to a foreign or stale mesh revision.");
    }
}
public sealed class MeshVertexSelection
{
    internal MeshVertexSelection(AuthoredMesh mesh, IEnumerable<int> indices)
    {
        int[] data = MeshDataValidation.Copy(indices, nameof(indices)).Distinct().Order().ToArray();
        MeshDataValidation.Indices(data, mesh.Positions.Count);
        Revision = mesh.Revision; Indices = Array.AsReadOnly(data);
    }
    public MeshRevision Revision { get; }
    public IReadOnlyList<int> Indices { get; }
    internal void Validate(AuthoredMesh mesh)
    {
        if (Revision != mesh.Revision) throw new ArgumentException("Vertex selection belongs to a foreign or stale mesh revision.");
    }
}
