namespace OcctSharp;

/// <summary>Selects one built-in OCAF/XCAF persistence format.</summary>
public enum DocumentStorageFormat
{
    /// <summary>Binary generic OCAF document storage.</summary>
    BinOcaf,
    /// <summary>XML generic OCAF document storage.</summary>
    XmlOcaf,
    /// <summary>Binary XCAF document storage.</summary>
    BinXcaf,
    /// <summary>XML XCAF document storage.</summary>
    XmlXcaf
}

/// <summary>Identifies supported copied document attributes.</summary>
public enum DocumentAttributeKind
{
    /// <summary>An attribute outside the typed Batch I surface.</summary>
    Unknown,
    /// <summary>A Unicode label name.</summary>
    Name,
    /// <summary>A Unicode label comment.</summary>
    Comment,
    /// <summary>An ASCII string value.</summary>
    AsciiString,
    /// <summary>A 32-bit integral value.</summary>
    IntegralValue,
    /// <summary>A double-precision real value.</summary>
    Real,
    /// <summary>A bounded array of 32-bit integral values.</summary>
    IntegerArray,
    /// <summary>A bounded array of double-precision real values.</summary>
    RealArray,
    /// <summary>A direct same-document label reference.</summary>
    Reference,
    /// <summary>An ordered array of same-document label references.</summary>
    ReferenceArray,
    /// <summary>An application-tree node relation.</summary>
    TreeNode,
    /// <summary>An independently copied named topology value.</summary>
    NamedShape
}

/// <summary>Copies one bounded integer-array attribute.</summary>
public sealed record DocumentIntegerArray(int LowerBound, IReadOnlyList<int> Values)
{
    /// <summary>Gets the inclusive logical upper bound.</summary>
    public int UpperBound => checked(LowerBound + Values.Count - 1);
}

/// <summary>Copies one bounded real-array attribute.</summary>
public sealed record DocumentRealArray(int LowerBound, IReadOnlyList<double> Values)
{
    /// <summary>Gets the inclusive logical upper bound.</summary>
    public int UpperBound => checked(LowerBound + Values.Count - 1);
}

/// <summary>Copies one application tree relation.</summary>
public sealed record DocumentTreeSnapshot(string? ParentEntry, IReadOnlyList<string> ChildEntries);

/// <summary>Copies one label attribute without retaining native attribute state.</summary>
public sealed record DocumentAttributeSnapshot(
    DocumentAttributeKind Kind,
    string Id,
    string NativeType,
    string? TextValue,
    int? IntegerValue,
    double? RealValue,
    DocumentIntegerArray? IntegerArray,
    DocumentRealArray? RealArray,
    string? ReferenceEntry,
    IReadOnlyList<string> ReferenceEntries,
    DocumentTreeSnapshot? Tree,
    Shape? NamedShape)
{
    /// <summary>Gets whether the snapshot owns copied named topology.</summary>
    public bool HasNamedShape => NamedShape is not null;
}

/// <summary>Copies one stable-entry label and its supported attributes.</summary>
public sealed record DocumentLabelSnapshot(
    string Entry,
    int Tag,
    int Depth,
    bool IsRoot,
    string? ParentEntry,
    IReadOnlyList<string> ChildEntries,
    IReadOnlyList<DocumentAttributeSnapshot> Attributes) : IDisposable
{
    /// <summary>Releases independently owning named-topology copies in this label.</summary>
    public void Dispose()
    {
        foreach (DocumentAttributeSnapshot attribute in Attributes) attribute.NamedShape?.Dispose();
    }
}

/// <summary>Copies one complete document label/attribute table.</summary>
public sealed record DocumentSnapshot(IReadOnlyList<DocumentLabelSnapshot> Labels) : IDisposable
{
    /// <summary>Gets the single root label.</summary>
    public DocumentLabelSnapshot Root => Labels.Single(static label => label.IsRoot);

    /// <summary>Gets the label with the specified stable entry.</summary>
    public DocumentLabelSnapshot GetLabel(string entry) =>
        Labels.Single(label => string.Equals(label.Entry, entry, StringComparison.Ordinal));

    /// <summary>Releases all independently owning named-topology copies.</summary>
    public void Dispose()
    {
        foreach (DocumentLabelSnapshot label in Labels) label.Dispose();
    }
}

/// <summary>Identifies a copied document dependency edge.</summary>
public enum DocumentDependencyEdgeKind
{
    /// <summary>A direct TDF reference.</summary>
    DirectReference,
    /// <summary>An element of an ordered reference array.</summary>
    ReferenceArray,
    /// <summary>An application-tree parent-to-child relation.</summary>
    TreeNode,
    /// <summary>An XDE assembly occurrence-to-referred-shape relation.</summary>
    XdeOccurrence
}

/// <summary>Copies one directed dependency between stable entries.</summary>
public sealed record DocumentDependencyEdge(
    string SourceEntry,
    string TargetEntry,
    DocumentDependencyEdgeKind Kind,
    int Ordinal = 0);

/// <summary>Provides deterministic graph diagnostics over copied document edges.</summary>
public sealed class DocumentDependencyGraph
{
    internal DocumentDependencyGraph(IEnumerable<string> nodes, IEnumerable<DocumentDependencyEdge> edges)
    {
        Nodes = nodes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Edges = edges
            .Distinct()
            .OrderBy(static edge => edge.SourceEntry, StringComparer.Ordinal)
            .ThenBy(static edge => edge.TargetEntry, StringComparer.Ordinal)
            .ThenBy(static edge => edge.Kind)
            .ThenBy(static edge => edge.Ordinal)
            .ToArray();

        Dictionary<string, List<string>> outgoing = Nodes.ToDictionary(
            static node => node, static _ => new List<string>(), StringComparer.Ordinal);
        Dictionary<string, int> incoming = Nodes.ToDictionary(
            static node => node, static _ => 0, StringComparer.Ordinal);
        foreach (DocumentDependencyEdge edge in Edges)
        {
            if (!outgoing.ContainsKey(edge.SourceEntry)) outgoing[edge.SourceEntry] = [];
            if (!outgoing.ContainsKey(edge.TargetEntry)) outgoing[edge.TargetEntry] = [];
            if (!incoming.ContainsKey(edge.SourceEntry)) incoming[edge.SourceEntry] = 0;
            if (!incoming.ContainsKey(edge.TargetEntry)) incoming[edge.TargetEntry] = 0;
            if (!outgoing[edge.SourceEntry].Contains(edge.TargetEntry, StringComparer.Ordinal))
            {
                outgoing[edge.SourceEntry].Add(edge.TargetEntry);
                incoming[edge.TargetEntry]++;
            }
        }
        foreach (List<string> targets in outgoing.Values) targets.Sort(StringComparer.Ordinal);

        Roots = incoming.Where(static item => item.Value == 0).Select(static item => item.Key)
            .Order(StringComparer.Ordinal).ToArray();
        Leaves = outgoing.Where(static item => item.Value.Count == 0).Select(static item => item.Key)
            .Order(StringComparer.Ordinal).ToArray();
        StronglyConnectedGroups = FindStronglyConnected(outgoing);
        CyclicGroups = StronglyConnectedGroups
            .Where(group => group.Count > 1 || outgoing[group[0]].Contains(group[0], StringComparer.Ordinal))
            .ToArray();
        TopologicalOrder = CyclicGroups.Count == 0 ? SortTopologically(outgoing, incoming) : [];
    }

    /// <summary>Gets all graph nodes in stable-entry order.</summary>
    public IReadOnlyList<string> Nodes { get; }
    /// <summary>Gets all directed dependency edges in deterministic order.</summary>
    public IReadOnlyList<DocumentDependencyEdge> Edges { get; }
    /// <summary>Gets nodes with no incoming dependency.</summary>
    public IReadOnlyList<string> Roots { get; }
    /// <summary>Gets nodes with no outgoing dependency.</summary>
    public IReadOnlyList<string> Leaves { get; }
    /// <summary>Gets every strongly connected group.</summary>
    public IReadOnlyList<IReadOnlyList<string>> StronglyConnectedGroups { get; }
    /// <summary>Gets strongly connected groups that form cycles.</summary>
    public IReadOnlyList<IReadOnlyList<string>> CyclicGroups { get; }
    /// <summary>Gets a deterministic topological order, or an empty list when cyclic.</summary>
    public IReadOnlyList<string> TopologicalOrder { get; }
    /// <summary>Gets whether the dependency graph contains no cycle.</summary>
    public bool IsAcyclic => CyclicGroups.Count == 0;

    /// <summary>Copies outgoing dependency edges for one stable entry.</summary>
    public IReadOnlyList<DocumentDependencyEdge> GetOutgoing(string entry) =>
        Edges.Where(edge => string.Equals(edge.SourceEntry, entry, StringComparison.Ordinal)).ToArray();

    /// <summary>Copies incoming dependency edges for one stable entry.</summary>
    public IReadOnlyList<DocumentDependencyEdge> GetIncoming(string entry) =>
        Edges.Where(edge => string.Equals(edge.TargetEntry, entry, StringComparison.Ordinal)).ToArray();

    private static List<string> SortTopologically(
        Dictionary<string, List<string>> outgoing,
        Dictionary<string, int> originalIncoming)
    {
        Dictionary<string, int> incoming = originalIncoming.ToDictionary(
            static item => item.Key, static item => item.Value, StringComparer.Ordinal);
        SortedSet<string> ready = new(incoming.Where(static item => item.Value == 0).Select(static item => item.Key), StringComparer.Ordinal);
        List<string> result = [];
        while (ready.Count > 0)
        {
            string node = ready.Min!;
            ready.Remove(node);
            result.Add(node);
            foreach (string target in outgoing[node])
                if (--incoming[target] == 0) ready.Add(target);
        }
        return result;
    }

    private static IReadOnlyList<string>[] FindStronglyConnected(
        Dictionary<string, List<string>> outgoing)
    {
        Dictionary<string, int> indices = new(StringComparer.Ordinal);
        Dictionary<string, int> lowLinks = new(StringComparer.Ordinal);
        HashSet<string> onStack = new(StringComparer.Ordinal);
        Stack<string> stack = new();
        List<IReadOnlyList<string>> groups = [];
        int nextIndex = 0;
        foreach (string node in outgoing.Keys.Order(StringComparer.Ordinal))
            if (!indices.TryGetValue(node, out _)) Visit(node);
        return groups.OrderBy(static group => group[0], StringComparer.Ordinal).ToArray();

        void Visit(string node)
        {
            indices[node] = lowLinks[node] = nextIndex++;
            stack.Push(node);
            onStack.Add(node);
            foreach (string target in outgoing[node])
            {
                if (!indices.TryGetValue(target, out int targetIndex)) { Visit(target); lowLinks[node] = Math.Min(lowLinks[node], lowLinks[target]); }
                else if (onStack.Contains(target)) lowLinks[node] = Math.Min(lowLinks[node], targetIndex);
            }
            if (lowLinks[node] != indices[node]) return;
            List<string> group = [];
            string current;
            do { current = stack.Pop(); onStack.Remove(current); group.Add(current); } while (current != node);
            group.Sort(StringComparer.Ordinal);
            groups.Add(group);
        }
    }
}

/// <summary>Copies current undo/redo and dirty state.</summary>
public sealed record DocumentHistoryState(int UndoLimit, int AvailableUndos, int AvailableRedos, bool IsChanged);

/// <summary>Copies one native document delta for diagnostics.</summary>
public sealed record DocumentHistoryEntry(
    string Name,
    int BeginTime,
    int EndTime,
    int AttributeDeltaCount,
    IReadOnlyList<string> ChangedLabelEntries);
