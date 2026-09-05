namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Immutable mesh editing. Every result carries exact revision-scoped correspondence.</summary>
public static partial class MeshEditing
{
    public static MeshEditResult SetPositions(AuthoredMesh source, MeshVertexSelection selection, IEnumerable<GpPoint> positions)
    {
        Validate(source, selection); GpPoint[] values = MeshDataValidation.Copy(positions, nameof(positions));
        if (values.Length != selection.Indices.Count) throw new ArgumentException("Position count must match the selected vertices.");
        GpPoint[] points = [.. source.Positions];
        for (int i = 0; i < values.Length; ++i) points[selection.Indices[i]] = values[i];
        HashSet<int> selected = [.. selection.Indices];
        int[] affected = Enumerable.Range(0, source.Triangles.Count).Where(i =>
            selected.Contains(source.Triangles[i].A) || selected.Contains(source.Triangles[i].B) || selected.Contains(source.Triangles[i].C)).ToArray();
        AuthoredMesh mesh = Create(source, points, source.Triangles, null, source.UVs);
        return IdentityResult(source, mesh, affected, ["Derived normals invalidated by position editing."]);
    }

    public static MeshEditResult SetTriangles(AuthoredMesh source, MeshTriangleSelection selection, IEnumerable<MeshTriangle> triangles)
    {
        Validate(source, selection); MeshTriangle[] values = MeshDataValidation.Copy(triangles, nameof(triangles));
        if (values.Length != selection.Indices.Count) throw new ArgumentException("Connectivity count must match the triangle selection.");
        MeshTriangle[] facets = [.. source.Triangles];
        for (int i = 0; i < values.Length; ++i) facets[selection.Indices[i]] = values[i];
        AuthoredMesh mesh = Create(source, source.Positions, facets, null, source.UVs);
        return IdentityResult(source, mesh, selection.Indices, ["Derived normals invalidated by connectivity editing."]);
    }

    public static MeshEditResult DeleteTriangles(AuthoredMesh source, MeshTriangleSelection selection)
    {
        Validate(source, selection); HashSet<int> deleted = [.. selection.Indices];
        int[] retained = Enumerable.Range(0, source.Triangles.Count).Where(i => !deleted.Contains(i)).ToArray();
        AuthoredMesh mesh = Create(source, source.Positions, retained.Select(i => source.Triangles[i]), source.Normals, source.UVs);
        return new(mesh, new(source.Revision, mesh.Revision, MeshIndexMap.Identity(source.Positions.Count),
            MeshIndexMap.FromResultSources(source.Triangles.Count, retained)), []);
    }

    public static MeshEditResult Compact(AuthoredMesh source)
    {
        ArgumentNullException.ThrowIfNull(source);
        HashSet<int> used = [];
        foreach (MeshTriangle triangle in source.Triangles) { used.Add(triangle.A); used.Add(triangle.B); used.Add(triangle.C); }
        foreach (MeshPolyline line in source.Polylines) used.UnionWith(line.Indices);
        int[] vertices = used.Order().ToArray();
        int[] inverse = Enumerable.Repeat(-1, source.Positions.Count).ToArray();
        for (int i = 0; i < vertices.Length; ++i) inverse[vertices[i]] = i;
        AuthoredMesh mesh = Create(source, vertices.Select(i => source.Positions[i]),
            source.Triangles.Select(t => new MeshTriangle(inverse[t.A], inverse[t.B], inverse[t.C], t.Group)),
            source.Normals is null ? null : vertices.Select(i => source.Normals[i]),
            source.UVs is null ? null : vertices.Select(i => source.UVs[i]),
            polylines: source.Polylines.Select(line => new MeshPolyline(line.Indices.Select(i => inverse[i]), line.IsClosed, line.Parameters)),
            origins: vertices.Select(i => source.VertexOrigins[i]));
        return new(mesh, new(source.Revision, mesh.Revision, MeshIndexMap.FromResultSources(source.Positions.Count, vertices),
            MeshIndexMap.Identity(source.Triangles.Count)), []);
    }

    public static MeshEditResult Extract(AuthoredMesh source, MeshTriangleSelection selection)
    {
        Validate(source, selection); HashSet<int> used = [];
        foreach (int i in selection.Indices)
        {
            MeshTriangle triangle = source.Triangles[i]; used.Add(triangle.A); used.Add(triangle.B); used.Add(triangle.C);
        }
        MeshPolyline[] lines = source.Polylines.Where(line => line.Indices.All(used.Contains)).ToArray();
        MeshTriangle[] triangles = selection.Indices.Select(i => source.Triangles[i]).ToArray();
        HashSet<int> groups = triangles.Select(t => t.Group).ToHashSet();
        AuthoredMesh intermediate = Create(source, source.Positions, triangles, source.Normals, source.UVs,
            groups: source.Groups.Where(g => groups.Contains(g.Key)), polylines: lines);
        MeshEditMap first = new(source.Revision, intermediate.Revision, MeshIndexMap.Identity(source.Positions.Count),
            MeshIndexMap.FromResultSources(source.Triangles.Count, selection.Indices));
        MeshEditResult compact = Compact(intermediate);
        return new(compact.Mesh, first.Then(compact.Map), Enumerable.Range(0, compact.Mesh.Triangles.Count));
    }

    /// <summary>Concatenates without welding. All sources must use identical coordinates and channel presence.</summary>
    public static MeshConcatenationResult Concatenate(IEnumerable<AuthoredMesh> sources)
    {
        AuthoredMesh[] meshes = MeshDataValidation.Copy(sources, nameof(sources), 100_000);
        if (meshes.Length == 0 || meshes.Any(m => m is null)) throw new ArgumentException("At least one non-null mesh is required.");
        AuthoredMesh first = meshes[0];
        long vertexCount = 0, triangleCount = 0;
        foreach (AuthoredMesh mesh in meshes)
        {
            vertexCount = checked(vertexCount + mesh.Positions.Count); triangleCount = checked(triangleCount + mesh.Triangles.Count);
            if (vertexCount > MeshDataValidation.MaximumElements || triangleCount > MeshDataValidation.MaximumElements)
                throw new ArgumentException("Concatenation exceeds the bounded element limit.");
            if (mesh.Coordinates != first.Coordinates || (mesh.Normals is null) != (first.Normals is null) || (mesh.UVs is null) != (first.UVs is null))
                throw new ArgumentException("Convert coordinates and explicitly reconcile absent channels before concatenation.");
        }
        List<GpPoint> positions = []; List<MeshTriangle> triangles = []; List<MeshNormal>? normals = first.Normals is null ? null : [];
        List<MeshUv>? uvs = first.UVs is null ? null : []; List<MeshGroup> groups = []; List<MeshPolyline> lines = [];
        List<MeshVertexOrigin> origins = []; List<(int Vertex, int Triangle)> offsets = [];
        foreach (AuthoredMesh mesh in meshes)
        {
            int vertexOffset = positions.Count, triangleOffset = triangles.Count; offsets.Add((vertexOffset, triangleOffset));
            Dictionary<int, int> groupMap = [];
            foreach (MeshGroup group in mesh.Groups)
            {
                int key = groups.Count; groupMap.Add(group.Key, key);
                groups.Add(group with { Key = key, SourceKey = group.SourceKey ?? $"{mesh.Revision.Id:N}:{group.Key}" });
            }
            positions.AddRange(mesh.Positions); origins.AddRange(mesh.VertexOrigins);
            if (normals is not null) normals.AddRange(mesh.Normals!);
            if (uvs is not null) uvs.AddRange(mesh.UVs!);
            triangles.AddRange(mesh.Triangles.Select(t => new MeshTriangle(t.A + vertexOffset, t.B + vertexOffset, t.C + vertexOffset, groupMap[t.Group])));
            lines.AddRange(mesh.Polylines.Select(line => new MeshPolyline(line.Indices.Select(i => i + vertexOffset), line.IsClosed, line.Parameters)));
        }
        AuthoredMesh result = Create(first, positions, triangles, normals, uvs, groups, lines, origins);
        MeshEditMap[] maps = meshes.Select((mesh, i) => new MeshEditMap(mesh.Revision, result.Revision,
            new MeshIndexMap(Enumerable.Range(0, mesh.Positions.Count).Select(v => new[] { v + offsets[i].Vertex }), result.Positions.Count),
            new MeshIndexMap(Enumerable.Range(0, mesh.Triangles.Count).Select(t => new[] { t + offsets[i].Triangle }), result.Triangles.Count))).ToArray();
        return new(result, maps);
    }

    public static MeshEditResult RemoveDuplicates(AuthoredMesh source, bool ignoreWinding = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        HashSet<(int, int, int, int)> seen = []; List<int> deleted = [];
        for (int i = 0; i < source.Triangles.Count; ++i)
        {
            MeshTriangle t = source.Triangles[i]; int a = t.A, b = t.B, c = t.C;
            if (ignoreWinding) { int[] sorted = [a, b, c]; Array.Sort(sorted); (a, b, c) = (sorted[0], sorted[1], sorted[2]); }
            else if (b <= a && b <= c) (a, b, c) = (b, c, a);
            else if (c <= a && c <= b) (a, b, c) = (c, a, b);
            // Group identity intentionally distinguishes overlapping material surfaces.
            if (!seen.Add((a, b, c, t.Group))) deleted.Add(i);
        }
        return DeleteTriangles(source, source.SelectTriangles(deleted));
    }

    public static MeshEditResult AssignGroup(AuthoredMesh source, MeshTriangleSelection selection, MeshGroup group)
    {
        Validate(source, selection); ArgumentNullException.ThrowIfNull(group);
        MeshGroup? existing = source.Groups.FirstOrDefault(g => g.Key == group.Key);
        if (existing is not null && existing != group) throw new ArgumentException("An existing group key cannot silently replace another material/provenance definition.");
        MeshGroup[] groups = existing is null ? [.. source.Groups, group] : [.. source.Groups];
        MeshTriangle[] triangles = [.. source.Triangles];
        foreach (int index in selection.Indices) triangles[index] = triangles[index] with { Group = group.Key };
        AuthoredMesh result = Create(source, source.Positions, triangles, source.Normals, source.UVs, groups);
        return IdentityResult(source, result, selection.Indices);
    }

    private static AuthoredMesh Create(AuthoredMesh source, IEnumerable<GpPoint> positions, IEnumerable<MeshTriangle> triangles,
        IEnumerable<MeshNormal>? normals, IEnumerable<MeshUv>? uvs, IEnumerable<MeshGroup>? groups = null,
        IEnumerable<MeshPolyline>? polylines = null, IEnumerable<MeshVertexOrigin>? origins = null, MeshCoordinates? coordinates = null) =>
        new(positions, triangles, normals, uvs, groups ?? source.Groups, polylines ?? source.Polylines,
            coordinates ?? source.Coordinates, new(Guid.NewGuid(), checked(source.Revision.Number + 1)), origins ?? source.VertexOrigins);

    private static MeshEditResult IdentityResult(AuthoredMesh source, AuthoredMesh result, IEnumerable<int>? affected = null,
        IEnumerable<string>? diagnostics = null) => new(result,
        new(source.Revision, result.Revision, MeshIndexMap.Identity(source.Positions.Count), MeshIndexMap.Identity(source.Triangles.Count)),
        affected ?? Enumerable.Range(0, result.Triangles.Count), diagnostics);

    private static void Validate(AuthoredMesh source, MeshTriangleSelection selection)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(selection); selection.Validate(source);
    }
    private static void Validate(AuthoredMesh source, MeshVertexSelection selection)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(selection); selection.Validate(source);
    }
}
