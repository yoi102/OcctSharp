namespace OcctSharp;

/// <summary>Controls removal of a reusable XDE definition that still has occurrences.</summary>
public enum AssemblyDefinitionRemovalPolicy
{
    /// <summary>Reject removal while any occurrence still refers to the definition.</summary>
    RejectIfUsed = 0,
    /// <summary>Remove referring occurrences before removing the definition.</summary>
    RemoveOccurrences = 1,
}

/// <summary>Classifies copied product-structure nodes.</summary>
public enum AssemblyStructureNodeKind
{
    /// <summary>A reusable non-assembly shape definition.</summary>
    PartDefinition = 0,
    /// <summary>A reusable assembly definition.</summary>
    AssemblyDefinition = 1,
    /// <summary>A path-qualified use of a reusable definition.</summary>
    Occurrence = 2,
}

/// <summary>Classifies copied product-structure edges.</summary>
public enum AssemblyStructureLinkKind
{
    /// <summary>An assembly definition contains an occurrence.</summary>
    ContainsOccurrence = 0,
    /// <summary>An occurrence refers to a reusable definition.</summary>
    RefersToDefinition = 1,
}

/// <summary>Classifies deterministic product-structure diagnostics.</summary>
public enum AssemblyDiagnosticCode
{
    /// <summary>A definition is active more than once in one occurrence path.</summary>
    Cycle = 0,
    /// <summary>An occurrence cannot resolve its referred definition.</summary>
    DanglingReference = 1,
    /// <summary>A definition is not reachable from the selected assembly root.</summary>
    OrphanDefinition = 2,
    /// <summary>An assembly exposes the same direct occurrence more than once.</summary>
    DuplicateOccurrence = 3,
    /// <summary>A definition does not provide readable topology.</summary>
    MissingShape = 4,
    /// <summary>An occurrence path cannot be resolved.</summary>
    InvalidPath = 5,
}

/// <summary>One copied definition or path-qualified occurrence node.</summary>
public sealed record AssemblyStructureNode(
    string Id,
    string Entry,
    string? DefinitionEntry,
    string? ParentId,
    string? Name,
    AssemblyStructureNodeKind Kind,
    IReadOnlyList<string> Path,
    int Depth);

/// <summary>One copied product-structure relationship.</summary>
public sealed record AssemblyStructureLink(string SourceId, string TargetId, AssemblyStructureLinkKind Kind);

/// <summary>One copied product-structure issue.</summary>
public sealed record AssemblyDiagnostic(
    AssemblyDiagnosticCode Code,
    string Entry,
    IReadOnlyList<string> Path,
    string Message);

/// <summary>A deterministic managed-owned structure graph with no document lifetime.</summary>
public sealed record AssemblyStructureSnapshot(
    IReadOnlyList<AssemblyStructureNode> Nodes,
    IReadOnlyList<AssemblyStructureLink> Links,
    IReadOnlyList<AssemblyDiagnostic> Diagnostics);

/// <summary>One structured or flattened bill-of-material item.</summary>
public sealed record AssemblyBomItem(
    string DefinitionEntry,
    string? Name,
    int Quantity,
    IReadOnlyList<string> Path,
    int Depth,
    bool IsAssembly);

/// <summary>A copied BOM generated from one assembly root.</summary>
public sealed record AssemblyBomReport(bool IsFlattened, IReadOnlyList<AssemblyBomItem> Items);

/// <summary>One reverse-usage record for a reusable definition.</summary>
public sealed record AssemblyWhereUsedItem(
    string DefinitionEntry,
    string OccurrenceEntry,
    string? ParentAssemblyEntry,
    IReadOnlyList<string> Path);

/// <summary>A copied assembly-item reference attribute.</summary>
public sealed record AssemblyItemReference(string FormattedPath, int? SubshapeIndex, bool IsOrphan)
{
    /// <summary>Gets the copied stable-entry path segments.</summary>
    public IReadOnlyList<string> Path => FormattedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
}

/// <summary>A copied SHUO graph-node view.</summary>
public sealed record AssemblyShuo(
    string Entry,
    IReadOnlyList<string> OccurrencePath,
    IReadOnlyList<string> UpperUsageEntries,
    IReadOnlyList<string> NextUsageEntries);

/// <summary>Effective occurrence metadata after definition fallback.</summary>
public sealed record AssemblyEffectiveMetadata(
    string? Name,
    XdeColor? Color,
    IReadOnlyList<string> Layers,
    XdeMaterial? Material,
    XdeVisualMaterial? VisualMaterial);

/// <summary>World-space physical rollup for one grouped definition.</summary>
public sealed record AssemblyPropertyGroup(
    string DefinitionEntry,
    string? Name,
    int Quantity,
    double Mass,
    GpPoint CenterOfMass,
    BoundingBox3d Bounds);

/// <summary>World-space physical rollup for a complete assembly occurrence tree.</summary>
public sealed record AssemblyPropertyRollup(
    int OccurrenceCount,
    double Mass,
    GpPoint CenterOfMass,
    BoundingBox3d Bounds,
    IReadOnlyList<AssemblyPropertyGroup> Groups);

/// <summary>Owns the resolved world location and topology for one occurrence path.</summary>
public sealed class AssemblyOccurrenceResolution : IDisposable
{
    internal AssemblyOccurrenceResolution(
        XdeLabel occurrence,
        XdeLabel definition,
        IReadOnlyList<string> path,
        TopLocLocation worldLocation,
        Shape locatedShape)
    {
        Occurrence = occurrence;
        Definition = definition;
        Path = path;
        WorldLocation = worldLocation;
        LocatedShape = locatedShape;
    }

    /// <summary>Gets the resolved parent-bound occurrence label.</summary>
    public XdeLabel Occurrence { get; }
    /// <summary>Gets the resolved parent-bound reusable definition label.</summary>
    public XdeLabel Definition { get; }
    /// <summary>Gets the copied stable-entry occurrence path.</summary>
    public IReadOnlyList<string> Path { get; }
    /// <summary>Gets an independently owned world location.</summary>
    public TopLocLocation WorldLocation { get; }
    /// <summary>Gets an independently owned located topology value.</summary>
    public Shape LocatedShape { get; }

    /// <summary>Releases the owned world location and located topology.</summary>
    public void Dispose()
    {
        LocatedShape.Dispose();
        WorldLocation.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>Associates a viewer-owned presentation with a copied occurrence path.</summary>
public sealed class AssemblyViewerPresentation : IDisposable
{
    internal AssemblyViewerPresentation(IReadOnlyList<string> path, ViewerPresentation presentation)
    {
        Path = path;
        Presentation = presentation;
    }

    /// <summary>Gets the copied stable-entry occurrence path.</summary>
    public IReadOnlyList<string> Path { get; }
    /// <summary>Gets the viewer-owned presentation.</summary>
    public ViewerPresentation Presentation { get; }
    /// <summary>Removes and releases the viewer-owned presentation.</summary>
    public void Dispose() => Presentation.Dispose();
}
