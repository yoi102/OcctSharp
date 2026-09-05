namespace OcctSharp;

#pragma warning disable CS1591

public static partial class MeshEditing
{
    public static MeshEditResult SetNormals(AuthoredMesh source, MeshVertexSelection selection, IEnumerable<MeshNormal> normals,
        bool normalize = true)
    {
        Validate(source, selection); MeshNormal[] values = MeshDataValidation.Copy(normals, nameof(normals));
        if (values.Length != selection.Indices.Count) throw new ArgumentException("Normal count must match selection.");
        MeshNormal[] result = source.Normals?.ToArray() ?? Enumerable.Repeat(MeshNormal.Undefined, source.Positions.Count).ToArray();
        for (int i = 0; i < values.Length; ++i) result[selection.Indices[i]] = normalize ? values[i].Normalized() : values[i];
        AuthoredMesh mesh = Create(source, source.Positions, source.Triangles, result, source.UVs);
        HashSet<int> selected = [.. selection.Indices];
        return IdentityResult(source, mesh, Enumerable.Range(0, source.Triangles.Count).Where(i =>
            selected.Contains(source.Triangles[i].A) || selected.Contains(source.Triangles[i].B) || selected.Contains(source.Triangles[i].C)));
    }

    /// <summary>Transforms selected complete UV charts. Shared vertices outside the selection require an explicit split first.</summary>
    public static MeshEditResult TransformUvs(AuthoredMesh source, MeshTriangleSelection selection, MeshUvTransform transform)
    {
        Validate(source, selection);
        if (source.UVs is null) throw new ArgumentException("The mesh has no UV channel.");
        double[] values = [transform.M11, transform.M12, transform.M13, transform.M21, transform.M22, transform.M23];
        if (values.Any(v => !double.IsFinite(v))) throw new ArgumentException("UV transform must be finite.");
        HashSet<int> selected = [.. selection.Indices], vertices = [];
        foreach (int i in selected)
        {
            MeshTriangle t = source.Triangles[i]; vertices.UnionWith([t.A, t.B, t.C]);
        }
        for (int i = 0; i < source.Triangles.Count; ++i)
            if (!selected.Contains(i))
            {
                MeshTriangle t = source.Triangles[i];
                if (vertices.Contains(t.A) || vertices.Contains(t.B) || vertices.Contains(t.C))
                    throw new ArgumentException("Selected UV charts share vertices with unselected triangles; split their seam explicitly first.");
            }
        MeshUv[] uvs = [.. source.UVs];
        foreach (int v in vertices)
        {
            MeshUv uv = uvs[v]; uvs[v] = new(transform.M11 * uv.U + transform.M12 * uv.V + transform.M13,
                transform.M21 * uv.U + transform.M22 * uv.V + transform.M23);
        }
        return IdentityResult(source, Create(source, source.Positions, source.Triangles, source.Normals, uvs), selected);
    }

    /// <summary>Splits normal creases/material discontinuities touching the selected region. Positions never move.</summary>
    public static MeshEditResult SplitBoundaryVertices(AuthoredMesh source, MeshTriangleSelection selection,
        double creaseAngle = Math.PI / 4, bool splitMaterials = true)
    {
        Validate(source, selection);
        if (!double.IsFinite(creaseAngle) || creaseAngle < 0 || creaseAngle > Math.PI) throw new ArgumentOutOfRangeException(nameof(creaseAngle));
        if (selection.Indices.Count == 0)
            return IdentityResult(source, Create(source, source.Positions, source.Triangles, source.Normals, source.UVs), []);
        int corners = checked(source.Triangles.Count * 3);
        if (corners > MeshDataValidation.MaximumElements) throw new ArgumentException("Corner splitting exceeds the bounded mesh limit.");
        MeshConnectivity graph = MeshConnectivity.Create(source); HashSet<int> selected = [.. selection.Indices];
        HashSet<int> touchedVertices = [];
        foreach (int i in selected)
        {
            MeshTriangle triangle = source.Triangles[i];
            touchedVertices.UnionWith([triangle.A, triangle.B, triangle.C]);
        }
        int[] parents = Enumerable.Range(0, corners).ToArray();
        MeshNormal[] faceNormals = source.Triangles.Select(t => Normal(Cross(source, t))).ToArray();
        Dictionary<int, MeshGroup> groups = source.Groups.ToDictionary(g => g.Key);
        foreach (MeshEdgeIncidence edge in graph.Edges)
        {
            if (edge.Uses.Count != 2 || edge.IsNonManifold) continue;
            int a = edge.Uses[0].Triangle, b = edge.Uses[1].Triangle;
            bool scoped = selected.Contains(a) || selected.Contains(b);
            MeshNormal na = faceNormals[a], nb = faceNormals[b];
            bool barrier = scoped && (!na.IsDefined || !nb.IsDefined ||
                na.X * nb.X + na.Y * nb.Y + na.Z * nb.Z < Math.Cos(creaseAngle) - 1e-12 ||
                (splitMaterials && groups[source.Triangles[a].Group].MaterialKey != groups[source.Triangles[b].Group].MaterialKey));
            if (barrier) continue;
            foreach (int v in new[] { edge.A, edge.B })
            {
                int ca = Enumerable.Range(0, 3).First(c => source.Triangles[a][c] == v);
                int cb = Enumerable.Range(0, 3).First(c => source.Triangles[b][c] == v);
                parents[Find(3 * b + cb)] = Find(3 * a + ca);
            }
        }
        List<GpPoint> points = [.. source.Positions]; List<MeshUv>? uvs = source.UVs is null ? null : [.. source.UVs];
        List<MeshVertexOrigin> origins = [.. source.VertexOrigins]; List<int> sources = Enumerable.Range(0, source.Positions.Count).ToList();
        Dictionary<int, int> representatives = []; HashSet<int> occupied = []; MeshTriangle[] triangles = new MeshTriangle[source.Triangles.Count];
        for (int i = 0; i < source.Triangles.Count; ++i)
        {
            int[] indices = new int[3];
            for (int c = 0; c < 3; ++c)
            {
                int v = source.Triangles[i][c];
                // Preserve disconnected vertex fans outside the requested region exactly.
                int root = touchedVertices.Contains(v) ? Find(3 * i + c) : -v - 1;
                if (!representatives.TryGetValue(root, out int target))
                {
                    target = v;
                    if (!occupied.Add(v))
                    {
                        target = points.Count; points.Add(source.Positions[v]); origins.Add(source.VertexOrigins[v]); sources.Add(v); uvs?.Add(source.UVs![v]);
                    }
                    representatives.Add(root, target);
                }
                indices[c] = target;
            }
            triangles[i] = new(indices[0], indices[1], indices[2], source.Triangles[i].Group);
        }
        AuthoredMesh result = Create(source, points, triangles, null, uvs, origins: origins);
        return new(result, new(source.Revision, result.Revision, MeshIndexMap.FromResultSources(source.Positions.Count, sources),
            MeshIndexMap.Identity(source.Triangles.Count)), Enumerable.Range(0, triangles.Length),
            ["Normals invalidated after vertex splitting; polyline references retain their original nodes."]);

        int Find(int item)
        {
            while (parents[item] != item) { parents[item] = parents[parents[item]]; item = parents[item]; }
            return item;
        }
    }

    /// <summary>Reconstructs area-weighted normals, optionally splitting crease/material boundaries first.</summary>
    public static MeshEditResult RebuildNormals(AuthoredMesh source, double creaseAngle = Math.PI, bool splitMaterials = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!double.IsFinite(creaseAngle) || creaseAngle < 0 || creaseAngle > Math.PI) throw new ArgumentOutOfRangeException(nameof(creaseAngle));
        MeshEditResult? split = creaseAngle < Math.PI || splitMaterials
            ? SplitBoundaryVertices(source, source.SelectTriangles(Enumerable.Range(0, source.Triangles.Count)), creaseAngle, splitMaterials) : null;
        AuthoredMesh working = split?.Mesh ?? source;
        (double X, double Y, double Z)[] accumulated = new (double, double, double)[working.Positions.Count];
        foreach (MeshTriangle t in working.Triangles)
        {
            (double x, double y, double z) = Cross(working, t);
            foreach (int v in new[] { t.A, t.B, t.C })
            {
                var old = accumulated[v]; accumulated[v] = (old.X + x, old.Y + y, old.Z + z);
            }
        }
        MeshNormal[] normals = accumulated.Select(Normal).ToArray();
        AuthoredMesh result = Create(working, working.Positions, working.Triangles, normals, working.UVs);
        MeshEditResult edit = IdentityResult(working, result);
        return split is null ? edit : new(result, split.Map.Then(edit.Map), Enumerable.Range(0, result.Triangles.Count));
    }

    public static AuthoredMeshStatistics Inspect(AuthoredMesh source)
    {
        ArgumentNullException.ThrowIfNull(source);
        MeshConnectivity graph = MeshConnectivity.Create(source);
        MeshRegionStatistics region = Measure(source, source.SelectTriangles(Enumerable.Range(0, source.Triangles.Count)));
        return new(source.Revision, source.Positions.Count, source.Triangles.Count, source.Groups.Count,
            region.SurfaceArea, region.Bounds, graph.Components().Count, graph.Edges.Count(e => e.IsBoundary),
            graph.Edges.Count(e => e.IsNonManifold), source.Triangles.Count(t => Length(Cross(source, t)) == 0));
    }

    public static MeshRegionStatistics Measure(AuthoredMesh source, MeshTriangleSelection selection)
    {
        Validate(source, selection); HashSet<int> vertices = []; double area = 0;
        foreach (int index in selection.Indices)
        {
            MeshTriangle t = source.Triangles[index]; vertices.UnionWith([t.A, t.B, t.C]);
            var cross = Cross(source, t); area += 0.5 * Length(cross);
        }
        if (!double.IsFinite(area)) throw new ArgumentException("Mesh area exceeds the finite numeric range.");
        AuthoredMeshBounds? bounds = null;
        if (vertices.Count > 0)
        {
            GpPoint[] points = vertices.Select(i => source.Positions[i]).ToArray();
            bounds = new(new(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z)),
                new(points.Max(p => p.X), points.Max(p => p.Y), points.Max(p => p.Z)));
        }
        return new(source.Revision, selection.Indices.Count, vertices.Count, area, bounds);
    }

    private static (double X, double Y, double Z) Cross(AuthoredMesh mesh, MeshTriangle triangle)
    {
        GpPoint a = mesh.Positions[triangle.A], b = mesh.Positions[triangle.B], c = mesh.Positions[triangle.C];
        double ux = b.X - a.X, uy = b.Y - a.Y, uz = b.Z - a.Z, vx = c.X - a.X, vy = c.Y - a.Y, vz = c.Z - a.Z;
        var result = (uy * vz - uz * vy, uz * vx - ux * vz, ux * vy - uy * vx);
        if (!double.IsFinite(result.Item1) || !double.IsFinite(result.Item2) || !double.IsFinite(result.Item3))
            throw new ArgumentException("Mesh geometry exceeds the finite numeric range.");
        return result;
    }
    private static double Length((double X, double Y, double Z) value)
    {
        double scale = Math.Max(Math.Abs(value.X), Math.Max(Math.Abs(value.Y), Math.Abs(value.Z)));
        if (scale == 0) return 0;
        double x = value.X / scale, y = value.Y / scale, z = value.Z / scale;
        return scale * Math.Sqrt(x * x + y * y + z * z);
    }
    private static MeshNormal Normal((double X, double Y, double Z) value) =>
        value == (0d, 0d, 0d) ? MeshNormal.Undefined : new MeshNormal(value.X, value.Y, value.Z).Normalized();
}
