using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public readonly record struct MeshEdgeUse(int Triangle, int Corner, int From, int To);
public sealed class MeshEdgeIncidence
{
    internal MeshEdgeIncidence(int a, int b, IEnumerable<MeshEdgeUse> uses) =>
        (A, B, Uses) = (a, b, Array.AsReadOnly(uses.ToArray()));
    public int A { get; }
    public int B { get; }
    public IReadOnlyList<MeshEdgeUse> Uses { get; }
    public bool IsBoundary => A != B && Uses.Count == 1;
    public bool IsNonManifold => A == B || Uses.Count > 2 || Uses.Select(u => u.Triangle).Distinct().Count() != Uses.Count;
}
public sealed class MeshBoundaryChain
{
    internal MeshBoundaryChain(IEnumerable<int> vertices, IEnumerable<int> triangles, bool closed) =>
        (Vertices, Triangles, IsClosed) = (Array.AsReadOnly(vertices.ToArray()), Array.AsReadOnly(triangles.ToArray()), closed);
    public IReadOnlyList<int> Vertices { get; }
    public IReadOnlyList<int> Triangles { get; }
    public bool IsClosed { get; }
}
public sealed class MeshBoundaryResult
{
    internal MeshBoundaryResult(MeshRevision revision, IEnumerable<MeshBoundaryChain> chains, IEnumerable<int> branches) =>
        (Revision, Chains, BranchVertices) = (revision, Array.AsReadOnly(chains.ToArray()), Array.AsReadOnly(branches.ToArray()));
    public MeshRevision Revision { get; }
    public IReadOnlyList<MeshBoundaryChain> Chains { get; }
    public IReadOnlyList<int> BranchVertices { get; }
}
/// <summary>Full indexed incidence, including all uses at non-manifold edges. No positional welding is implied.</summary>
public sealed class MeshConnectivity
{
    private readonly AuthoredMesh source;
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<IReadOnlyList<int>> triangleEdges;
    private MeshConnectivity(AuthoredMesh mesh)
    {
        source = mesh; Revision = mesh.Revision;
        Dictionary<(int, int), List<MeshEdgeUse>> edges = [];
        List<int>[] incident = Enumerable.Range(0, mesh.Positions.Count).Select(_ => new List<int>()).ToArray();
        for (int i = 0; i < mesh.Triangles.Count; ++i)
        {
            MeshTriangle triangle = mesh.Triangles[i];
            foreach (int v in new[] { triangle.A, triangle.B, triangle.C }.Distinct()) incident[v].Add(i);
            for (int corner = 0; corner < 3; ++corner)
            {
                int a = triangle[corner], b = triangle[(corner + 1) % 3];
                (int, int) key = (Math.Min(a, b), Math.Max(a, b));
                if (!edges.TryGetValue(key, out List<MeshEdgeUse>? uses)) edges.Add(key, uses = []);
                uses.Add(new(i, corner, a, b));
            }
        }
        MeshEdgeIncidence[] records = edges.OrderBy(e => e.Key).Select(e => new MeshEdgeIncidence(e.Key.Item1, e.Key.Item2, e.Value)).ToArray();
        List<int>[] byTriangle = Enumerable.Range(0, mesh.Triangles.Count).Select(_ => new List<int>()).ToArray();
        for (int i = 0; i < records.Length; ++i)
            foreach (int t in records[i].Uses.Select(u => u.Triangle).Distinct()) byTriangle[t].Add(i);
        Edges = Array.AsReadOnly(records);
        IncidentTriangles = Array.AsReadOnly(incident.Select(row => (IReadOnlyList<int>)Array.AsReadOnly(row.ToArray())).ToArray());
        triangleEdges = Array.AsReadOnly(byTriangle.Select(row => (IReadOnlyList<int>)Array.AsReadOnly(row.ToArray())).ToArray());
        // Poly_Connect supplies the manifold adjacency. Full incidence above retains
        // non-manifold and inconsistent-orientation inputs that Poly cannot represent.
        if (records.All(e => !e.IsNonManifold && (e.Uses.Count != 2 || e.Uses[0].From != e.Uses[1].From)))
            VerifyPolyConnectivity(mesh);
    }
    public MeshRevision Revision { get; }
    public IReadOnlyList<MeshEdgeIncidence> Edges { get; }
    public IReadOnlyList<IReadOnlyList<int>> IncidentTriangles { get; }
    public static MeshConnectivity Create(AuthoredMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh); return new(mesh);
    }
    private unsafe void VerifyPolyConnectivity(AuthoredMesh mesh)
    {
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(mesh); AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(mesh.Triangles);
        int[] neighbors = new int[checked(triangles.Length * 3)]; OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
        fixed (int* n = neighbors)
            NativeError.ThrowIfFailed(NativeMethods.MeshPolyConnect(v, vertices.Length, t, triangles.Length, n, neighbors.Length), "mesh_poly_connect");
        for (int i = 0; i < triangles.Length; ++i)
        {
            int[] actual = neighbors.Skip(3 * i).Take(3).Where(n => n >= 0).Distinct().Order().ToArray();
            if (!actual.SequenceEqual(Neighbors(i))) throw new OcctException("InvalidMeshResult", "OCCT manifold adjacency disagrees with complete copied edge incidence.");
        }
    }
    /// <summary>Returns all distinct edge-neighbors, not a truncated three-neighbor manifold assumption.</summary>
    public IReadOnlyList<int> Neighbors(int triangle)
    {
        MeshDataValidation.Indices([triangle], source.Triangles.Count);
        return Array.AsReadOnly(triangleEdges[triangle].SelectMany(i => Edges[i].Uses).Select(u => u.Triangle)
            .Where(i => i != triangle).Distinct().Order().ToArray());
    }
    public IReadOnlyList<MeshTriangleSelection> Components()
    {
        bool[] visited = new bool[source.Triangles.Count]; List<MeshTriangleSelection> result = [];
        for (int i = 0; i < visited.Length; ++i)
        {
            if (visited[i]) continue;
            Queue<int> queue = new(); List<int> component = []; queue.Enqueue(i); visited[i] = true;
            while (queue.TryDequeue(out int current))
            {
                component.Add(current);
                foreach (int next in Neighbors(current))
                    if (!visited[next]) { visited[next] = true; queue.Enqueue(next); }
            }
            result.Add(source.SelectTriangles(component));
        }
        return result.AsReadOnly();
    }
    public IReadOnlyList<MeshEditResult> ExtractComponents() =>
        Array.AsReadOnly(Components().Select(selection => MeshEditing.Extract(source, selection)).ToArray());

    public MeshTriangleSelection Expand(MeshTriangleSelection selection, int rings, bool stopAtGroupBoundary = true,
        bool stopAtMaterialBoundary = true, bool stopAtAttributeSeam = true)
    {
        ArgumentNullException.ThrowIfNull(selection); selection.Validate(source);
        ArgumentOutOfRangeException.ThrowIfNegative(rings);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rings, source.Triangles.Count);
        HashSet<int> selected = [.. selection.Indices], frontier = [.. selection.Indices];
        Dictionary<int, MeshGroup> groups = source.Groups.ToDictionary(g => g.Key);
        for (int ring = 0; ring < rings && frontier.Count > 0; ++ring)
        {
            HashSet<int> next = [];
            foreach (int triangle in frontier)
                foreach (int edgeIndex in triangleEdges[triangle])
                {
                    MeshEdgeIncidence edge = Edges[edgeIndex];
                    if (edge.IsNonManifold) continue;
                    foreach (MeshEdgeUse use in edge.Uses)
                    {
                        int neighbor = use.Triangle;
                        if (selected.Contains(neighbor)) continue;
                        MeshTriangle a = source.Triangles[triangle], b = source.Triangles[neighbor];
                        if (stopAtGroupBoundary && a.Group != b.Group) continue;
                        if (stopAtMaterialBoundary && groups[a.Group].MaterialKey != groups[b.Group].MaterialKey) continue;
                        // Duplicated seam vertices are not index-adjacent; they are never implicitly welded.
                        // Undefined endpoint normals are an explicit attribute barrier when requested.
                        if (stopAtAttributeSeam && source.Normals is not null &&
                            (!source.Normals[edge.A].IsDefined || !source.Normals[edge.B].IsDefined)) continue;
                        next.Add(neighbor);
                    }
                }
            selected.UnionWith(next); frontier = next;
        }
        return source.SelectTriangles(selected);
    }

    public MeshBoundaryResult Boundaries()
    {
        MeshEdgeUse[] uses = Edges.Where(e => e.IsBoundary).Select(e => e.Uses[0]).ToArray();
        Dictionary<int, List<int>> outgoing = [], incoming = [];
        for (int i = 0; i < uses.Length; ++i)
        {
            if (!outgoing.TryGetValue(uses[i].From, out List<int>? from)) outgoing.Add(uses[i].From, from = []);
            if (!incoming.TryGetValue(uses[i].To, out List<int>? to)) incoming.Add(uses[i].To, to = []);
            from.Add(i); to.Add(i);
        }
        int[] branches = outgoing.Keys.Concat(incoming.Keys).Distinct().Where(v =>
            outgoing.GetValueOrDefault(v)?.Count != 1 || incoming.GetValueOrDefault(v)?.Count != 1).Order().ToArray();
        HashSet<int> branchSet = [.. branches]; bool[] visited = new bool[uses.Length]; List<MeshBoundaryChain> chains = [];
        foreach (int start in Enumerable.Range(0, uses.Length).OrderBy(i => branchSet.Contains(uses[i].From) ? 0 : 1))
        {
            if (visited[start]) continue;
            List<int> vertices = [uses[start].From], triangles = []; int current = start;
            while (!visited[current])
            {
                visited[current] = true; MeshEdgeUse edge = uses[current]; vertices.Add(edge.To); triangles.Add(edge.Triangle);
                if (branchSet.Contains(edge.To) || !outgoing.TryGetValue(edge.To, out List<int>? next)) break;
                current = next[0];
            }
            chains.Add(new(vertices, triangles, vertices[0] == vertices[^1] && !branchSet.Contains(vertices[0])));
        }
        return new(Revision, chains, branches);
    }
}

public sealed record MeshOrientationIssue(string Reason, int Triangle, int RelatedTriangle);
public sealed class MeshOrientationResult
{
    internal MeshOrientationResult(MeshEditResult? edit, IEnumerable<MeshOrientationIssue> issues) =>
        (Edit, Issues) = (edit, Array.AsReadOnly(issues.ToArray()));
    public bool IsOrientable => Edit is not null;
    public MeshEditResult? Edit { get; }
    public IReadOnlyList<MeshOrientationIssue> Issues { get; }
}

public static partial class MeshEditing
{
    public static MeshOrientationResult OrientComponents(AuthoredMesh source)
    {
        ArgumentNullException.ThrowIfNull(source); MeshConnectivity graph = MeshConnectivity.Create(source);
        List<MeshOrientationIssue> issues = [];
        List<(int Next, bool Toggle)>[] relations = Enumerable.Range(0, source.Triangles.Count).Select(_ => new List<(int, bool)>()).ToArray();
        foreach (MeshEdgeIncidence edge in graph.Edges)
        {
            if (edge.IsNonManifold) { issues.Add(new("Non-manifold or degenerate edge; no orientation chosen.", edge.Uses[0].Triangle, -1)); continue; }
            if (edge.Uses.Count != 2) continue;
            MeshEdgeUse a = edge.Uses[0], b = edge.Uses[1]; bool toggle = a.From == b.From;
            relations[a.Triangle].Add((b.Triangle, toggle)); relations[b.Triangle].Add((a.Triangle, toggle));
        }
        bool?[] reversed = new bool?[source.Triangles.Count];
        for (int i = 0; i < reversed.Length; ++i)
        {
            if (reversed[i].HasValue) continue;
            Queue<int> queue = new(); reversed[i] = false; queue.Enqueue(i);
            while (queue.TryDequeue(out int current))
                foreach ((int next, bool toggle) in relations[current])
                {
                    bool expected = reversed[current]!.Value ^ toggle;
                    if (reversed[next].HasValue)
                    {
                        if (reversed[next] != expected) issues.Add(new("Non-orientable adjacency cycle.", current, next));
                    }
                    else { reversed[next] = expected; queue.Enqueue(next); }
                }
        }
        if (issues.Count > 0) return new(null, issues);
        MeshTriangle[] triangles = source.Triangles.Select((t, i) => reversed[i]!.Value ? t.Reversed() : t).ToArray();
        AuthoredMesh mesh = Create(source, source.Positions, triangles, null, source.UVs);
        MeshEditResult rebuilt = RebuildNormals(mesh);
        MeshEditMap initial = new(source.Revision, mesh.Revision, MeshIndexMap.Identity(source.Positions.Count), MeshIndexMap.Identity(source.Triangles.Count));
        return new(new(rebuilt.Mesh, initial.Then(rebuilt.Map), Enumerable.Range(0, source.Triangles.Count)), []);
    }

    /// <summary>Only complete edge-connected components may be reversed; shared-vertex normals are rebuilt.</summary>
    public static MeshEditResult ReverseComponents(AuthoredMesh source, MeshTriangleSelection selection)
    {
        Validate(source, selection); HashSet<int> selected = [.. selection.Indices];
        MeshConnectivity graph = MeshConnectivity.Create(source);
        foreach (MeshTriangleSelection component in graph.Components())
            if (component.Indices.Any(selected.Contains) && !component.Indices.All(selected.Contains))
                throw new ArgumentException("Winding reversal requires complete components, not part of a shared edge boundary.");
        AuthoredMesh mesh = Create(source, source.Positions, source.Triangles.Select((t, i) => selected.Contains(i) ? t.Reversed() : t), null, source.UVs);
        MeshEditResult rebuilt = RebuildNormals(mesh);
        MeshEditMap initial = new(source.Revision, mesh.Revision, MeshIndexMap.Identity(source.Positions.Count), MeshIndexMap.Identity(source.Triangles.Count));
        return new(rebuilt.Mesh, initial.Then(rebuilt.Map), selection.Indices);
    }
}
