namespace OcctSharp;

/// <summary>One flattened assembly occurrence with an independently owned world location.</summary>
public sealed class XdeOccurrence : IDisposable
{
    private readonly TopLocLocation worldLocation;

    internal XdeOccurrence(
        XdeLabel occurrenceLabel,
        XdeLabel referredLabel,
        IReadOnlyList<string> path,
        TopLocLocation worldLocation)
    {
        OccurrenceLabel = occurrenceLabel;
        ReferredLabel = referredLabel;
        Path = Array.AsReadOnly([.. path]);
        this.worldLocation = worldLocation;
    }

    /// <summary>Gets the parent-bound component occurrence label.</summary>
    public XdeLabel OccurrenceLabel { get; }
    /// <summary>Gets the parent-bound part or subassembly definition label.</summary>
    public XdeLabel ReferredLabel { get; }
    /// <summary>Gets the root-to-leaf sequence of occurrence entries.</summary>
    public IReadOnlyList<string> Path { get; }
    /// <summary>Gets the occurrence depth below the queried assembly root.</summary>
    public int Depth => Path.Count;
    /// <summary>Gets whether the referred definition is itself an assembly.</summary>
    public bool IsAssembly => ReferredLabel.IsAssembly;

    /// <summary>Returns an independent copy of the composed root-to-occurrence location.</summary>
    public TopLocLocation GetWorldLocation() => worldLocation.Clone();

    /// <summary>Returns an independently owned shape placed at the composed world location.</summary>
    public Shape GetLocatedShape()
    {
        using Shape shape = ReferredLabel.Shape;
        return worldLocation.Locate(shape);
    }

    /// <summary>Releases the owned composed world location.</summary>
    public void Dispose()
    {
        worldLocation.Dispose();
        GC.SuppressFinalize(this);
    }
}
