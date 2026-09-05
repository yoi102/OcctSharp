namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Exact copied one-to-many map; an empty result list denotes deletion.</summary>
public sealed class MeshIndexMap
{
    internal MeshIndexMap(IEnumerable<IEnumerable<int>> targets, int resultCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(resultCount);
        IReadOnlyList<int>[] data = targets.Select(row =>
        {
            int[] copy = row.Distinct().Order().ToArray(); MeshDataValidation.Indices(copy, resultCount);
            return (IReadOnlyList<int>)Array.AsReadOnly(copy);
        }).ToArray();
        Targets = Array.AsReadOnly(data); ResultCount = resultCount;
    }
    public int SourceCount => Targets.Count;
    public int ResultCount { get; }
    public IReadOnlyList<IReadOnlyList<int>> Targets { get; }
    public IReadOnlyList<int> Deleted => Array.AsReadOnly(Enumerable.Range(0, SourceCount).Where(i => Targets[i].Count == 0).ToArray());
    internal static MeshIndexMap Identity(int count) => new(Enumerable.Range(0, count).Select(i => new[] { i }), count);
    internal static MeshIndexMap FromResultSources(int sourceCount, IReadOnlyList<int> resultSources)
    {
        List<int>[] rows = Enumerable.Range(0, sourceCount).Select(_ => new List<int>()).ToArray();
        for (int i = 0; i < resultSources.Count; ++i)
        {
            int source = resultSources[i];
            if (source < -1 || source >= sourceCount) throw new ArgumentOutOfRangeException(nameof(resultSources));
            if (source >= 0) rows[source].Add(i);
        }
        return new(rows, resultSources.Count);
    }
    internal MeshIndexMap Compose(MeshIndexMap next)
    {
        if (ResultCount != next.SourceCount) throw new ArgumentException("Map cardinalities do not match.");
        return new(Targets.Select(row => row.SelectMany(i => next.Targets[i])), next.ResultCount);
    }
}
public sealed class MeshEditMap
{
    internal MeshEditMap(MeshRevision source, MeshRevision result, MeshIndexMap vertices, MeshIndexMap triangles) =>
        (Source, Result, Vertices, Triangles) = (source, result, vertices, triangles);
    public MeshRevision Source { get; }
    public MeshRevision Result { get; }
    public MeshIndexMap Vertices { get; }
    public MeshIndexMap Triangles { get; }
    public MeshEditMap Then(MeshEditMap next)
    {
        ArgumentNullException.ThrowIfNull(next);
        if (Result != next.Source) throw new ArgumentException("Maps cannot be composed across foreign or stale intermediate revisions.", nameof(next));
        return new(Source, next.Result, Vertices.Compose(next.Vertices), Triangles.Compose(next.Triangles));
    }
    public MeshTriangleSelection MapSelection(MeshTriangleSelection selection, AuthoredMesh result)
    {
        ArgumentNullException.ThrowIfNull(selection); ArgumentNullException.ThrowIfNull(result);
        if (selection.Revision != Source || result.Revision != Result) throw new ArgumentException("Selection/map/result revisions do not match.");
        return result.SelectTriangles(selection.Indices.SelectMany(i => Triangles.Targets[i]));
    }
}
public sealed class MeshEditResult
{
    internal MeshEditResult(AuthoredMesh mesh, MeshEditMap map, IEnumerable<int> affectedTriangles, IEnumerable<string>? diagnostics = null)
    {
        Mesh = mesh; Map = map; AffectedTriangles = mesh.SelectTriangles(affectedTriangles);
        Diagnostics = Array.AsReadOnly(diagnostics?.ToArray() ?? []);
    }
    public AuthoredMesh Mesh { get; }
    public MeshEditMap Map { get; }
    public MeshTriangleSelection AffectedTriangles { get; }
    public IReadOnlyList<string> Diagnostics { get; }
}
public sealed class MeshConcatenationResult
{
    internal MeshConcatenationResult(AuthoredMesh mesh, MeshEditMap[] maps) => (Mesh, SourceMaps) = (mesh, Array.AsReadOnly(maps));
    public AuthoredMesh Mesh { get; }
    public IReadOnlyList<MeshEditMap> SourceMaps { get; }
}
