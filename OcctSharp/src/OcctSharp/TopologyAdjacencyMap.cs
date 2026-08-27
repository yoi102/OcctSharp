namespace OcctSharp;

/// <summary>
/// Owns copied unique topology items and ancestors plus a compact zero-based adjacency map.
/// </summary>
public sealed class TopologyAdjacencyMap : IDisposable
{
    private readonly Shape[] items;
    private readonly Shape[] ancestors;
    private readonly int[] offsets;
    private readonly int[] ancestorIndices;
    private bool disposed;

    internal TopologyAdjacencyMap(
        ShapeKind itemKind,
        ShapeKind ancestorKind,
        Shape[] items,
        Shape[] ancestors,
        int[] offsets,
        int[] ancestorIndices)
    {
        ItemKind = itemKind;
        AncestorKind = ancestorKind;
        this.items = items;
        this.ancestors = ancestors;
        this.offsets = offsets;
        this.ancestorIndices = ancestorIndices;
        Items = Array.AsReadOnly(items);
        Ancestors = Array.AsReadOnly(ancestors);
    }

    /// <summary>Gets the unique mapped item kind.</summary>
    public ShapeKind ItemKind { get; }

    /// <summary>Gets the unique ancestor kind.</summary>
    public ShapeKind AncestorKind { get; }

    /// <summary>Gets the independently owned unique item copies.</summary>
    public IReadOnlyList<Shape> Items { get; }

    /// <summary>Gets the independently owned unique ancestor copies.</summary>
    public IReadOnlyList<Shape> Ancestors { get; }

    /// <summary>Gets the number of item-to-ancestor relations.</summary>
    public int RelationCount => ancestorIndices.Length;

    /// <summary>Returns the ancestor indices associated with one item index.</summary>
    public ReadOnlyMemory<int> GetAncestorIndices(int itemIndex)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(itemIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(itemIndex, items.Length);
        int start = offsets[itemIndex];
        return ancestorIndices.AsMemory(start, offsets[itemIndex + 1] - start);
    }

    /// <summary>Releases every copied topology owner. Disposal is idempotent.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (Shape item in items) item.Dispose();
        foreach (Shape ancestor in ancestors) ancestor.Dispose();
        GC.SuppressFinalize(this);
    }
}
