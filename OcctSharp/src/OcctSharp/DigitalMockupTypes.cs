namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>A stable, order-independent identity for one requested pair.</summary>
public readonly record struct DigitalMockupPairId
{
    public DigitalMockupPairId(string firstId, string secondId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstId);
        ArgumentException.ThrowIfNullOrWhiteSpace(secondId);
        if (string.Equals(firstId, secondId, StringComparison.Ordinal))
            throw new ArgumentException("A digital mock-up pair requires two different item IDs.");
        if (StringComparer.Ordinal.Compare(firstId, secondId) <= 0)
            (FirstId, SecondId) = (firstId, secondId);
        else
            (FirstId, SecondId) = (secondId, firstId);
    }

    public string FirstId { get; }
    public string SecondId { get; }
    public override string ToString() => $"{Uri.EscapeDataString(FirstId)}|{Uri.EscapeDataString(SecondId)}";
}

/// <summary>One caller-owned shape and copied traceability used for a synchronous analysis.</summary>
public sealed class DigitalMockupItem
{
    public DigitalMockupItem(
        string id,
        Shape shape,
        string? definitionId = null,
        IEnumerable<string>? occurrencePath = null,
        IEnumerable<string>? adjacentIds = null,
        string? name = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(shape);
        Id = id;
        Shape = shape;
        DefinitionId = string.IsNullOrWhiteSpace(definitionId) ? null : definitionId;
        OccurrencePath = Array.AsReadOnly(occurrencePath?.ToArray() ?? []);
        AdjacentIds = new HashSet<string>(adjacentIds ?? [], StringComparer.Ordinal);
        Name = string.IsNullOrWhiteSpace(name) ? id : name;
    }

    public string Id { get; }
    public string Name { get; }
    public Shape Shape { get; }
    public string? DefinitionId { get; }
    public IReadOnlyList<string> OccurrencePath { get; }
    public IReadOnlySet<string> AdjacentIds { get; }
}

/// <summary>Finite options for broad/exact-phase digital mock-up analysis.</summary>
public sealed record DigitalMockupPolicy
{
    public double Clearance { get; init; }
    public double ConfusionTolerance { get; init; } = 1e-7;
    public double FuzzyTolerance { get; init; } = 1e-7;
    public double AngularToleranceRadians { get; init; } = 1e-9;
    public bool RunParallel { get; init; }
    public bool NonDestructive { get; init; } = true;
    public bool EarlyExit { get; init; }
    public bool ExactDistanceForAllPairs { get; init; }
    public bool ExcludeSameDefinition { get; init; }
    public bool ExcludeAdjacent { get; init; }
    public bool RunSelfChecks { get; init; } = true;
    public int MaxInterferenceGroupsPerPair { get; init; } = 64;
    public int MaxSelfDiagnosticsPerItem { get; init; } = 64;
    public IReadOnlyCollection<DigitalMockupPairId> ExcludedPairs { get; init; } = Array.Empty<DigitalMockupPairId>();

    internal DigitalMockupPolicy Validated()
    {
        ValidateFiniteNonNegative(Clearance, nameof(Clearance));
        ValidateFiniteNonNegative(ConfusionTolerance, nameof(ConfusionTolerance));
        ValidateFiniteNonNegative(FuzzyTolerance, nameof(FuzzyTolerance));
        ValidateFiniteNonNegative(AngularToleranceRadians, nameof(AngularToleranceRadians));
        if (AngularToleranceRadians > Math.PI) throw new ArgumentOutOfRangeException(nameof(AngularToleranceRadians));
        if (MaxInterferenceGroupsPerPair is < 1 or > 4096) throw new ArgumentOutOfRangeException(nameof(MaxInterferenceGroupsPerPair));
        if (MaxSelfDiagnosticsPerItem is < 1 or > 4096) throw new ArgumentOutOfRangeException(nameof(MaxSelfDiagnosticsPerItem));
        ArgumentNullException.ThrowIfNull(ExcludedPairs);
        return this;
    }

    private static void ValidateFiniteNonNegative(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(name);
    }
}

public enum DigitalMockupPairState
{
    Excluded = 0,
    BroadPhaseClear = 1,
    Clear = 2,
    ClearanceViolation = 3,
    Touching = 4,
    FirstInsideSecond = 5,
    SecondInsideFirst = 6,
    Coincident = 7,
    Interfering = 8,
    Failed = 9,
    SkippedAfterEarlyExit = 10
}

public enum DigitalMockupSeverity
{
    None = 0,
    Information = 1,
    Clearance = 2,
    Contact = 3,
    Critical = 4
}

public enum DigitalMockupFilterReason
{
    None = 0,
    SameDefinition = 1,
    Adjacent = 2,
    ExplicitPair = 3,
    UnusableInput = 4,
    EarlyExit = 5
}

public enum DigitalMockupStage
{
    Input = 0,
    Bounds = 1,
    BroadPhase = 2,
    Filter = 3,
    ExactDistance = 4,
    Classification = 5,
    SelfCheck = 6,
    Aggregation = 7,
    Incremental = 8,
    Viewer = 9
}

public enum DigitalMockupDiagnosticSeverity { Information = 0, Warning = 1, Error = 2 }

public sealed record DigitalMockupDiagnostic(
    DigitalMockupStage Stage,
    DigitalMockupDiagnosticSeverity Severity,
    string Code,
    string Message,
    string? ItemId = null,
    DigitalMockupPairId? PairId = null);

public sealed record DigitalMockupItemSnapshot(
    string Id,
    string Name,
    string? DefinitionId,
    IReadOnlyList<string> OccurrencePath,
    BoundingBox3d? AxisAlignedBounds,
    OrientedBoundingBox3d? OrientedBounds,
    bool IsUsable);

public enum DigitalMockupInterferenceKind
{
    FaceFace = 0,
    EdgeEdge = 1,
    FaceEdge = 2,
    FaceVertex = 3,
    EdgeVertex = 4,
    VertexVertex = 5
}

public sealed record DigitalMockupInterferenceGroup(
    DigitalMockupInterferenceKind Kind,
    InspectionSupportKind FirstSupportKind,
    InspectionSupportKind SecondSupportKind,
    IReadOnlyList<int> WitnessIndices);

public sealed record DigitalMockupSelfCheckResult(
    string ItemId,
    bool IsValid,
    int FaultyShapeCount,
    IReadOnlyList<ShapeValidationIssue> ValidationIssues,
    string Message);

/// <summary>One complete terminal pair result with independently owning support/issue topology.</summary>
public sealed class DigitalMockupPairResult : IDisposable
{
    private readonly ExactDistanceResult? _exactDistance;
    private bool _disposed;

    internal DigitalMockupPairResult(
        DigitalMockupPairId id,
        DigitalMockupPairState state,
        DigitalMockupSeverity severity,
        DigitalMockupFilterReason filterReason,
        bool isExact,
        double? minimumDistance,
        double broadPhaseDistanceLowerBound,
        double overlapVolume,
        ExactDistanceResult? exactDistance,
        IReadOnlyList<DigitalMockupInterferenceGroup> interferenceGroups,
        Shape? issueTopology,
        IReadOnlyList<DigitalMockupDiagnostic> diagnostics)
    {
        Id = id;
        State = state;
        Severity = severity;
        FilterReason = filterReason;
        IsExact = isExact;
        MinimumDistance = minimumDistance;
        BroadPhaseDistanceLowerBound = broadPhaseDistanceLowerBound;
        OverlapVolume = overlapVolume;
        _exactDistance = exactDistance;
        InterferenceGroups = interferenceGroups;
        IssueTopology = issueTopology;
        Diagnostics = diagnostics;
    }

    public DigitalMockupPairId Id { get; }
    public string FirstId => Id.FirstId;
    public string SecondId => Id.SecondId;
    public DigitalMockupPairState State { get; }
    public DigitalMockupSeverity Severity { get; }
    public DigitalMockupFilterReason FilterReason { get; }
    public bool IsExact { get; }
    public double? MinimumDistance { get; }
    public double BroadPhaseDistanceLowerBound { get; }
    public double OverlapVolume { get; }
    public IReadOnlyList<ShapeDistanceSolution> Witnesses => _exactDistance?.Solutions ?? [];
    public IReadOnlyList<DigitalMockupInterferenceGroup> InterferenceGroups { get; }
    public Shape? IssueTopology { get; }
    public IReadOnlyList<DigitalMockupDiagnostic> Diagnostics { get; }
    public bool IsIssue => Severity >= DigitalMockupSeverity.Clearance;

    internal DigitalMockupPairResult CloneOwned()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ExactDistanceResult? exact = _exactDistance is null ? null : CloneExact(_exactDistance);
        Shape? topology = IssueTopology is null ? null : CloneShape(IssueTopology);
        return new(Id, State, Severity, FilterReason, IsExact, MinimumDistance,
            BroadPhaseDistanceLowerBound, OverlapVolume, exact,
            InterferenceGroups.Select(group => group with { WitnessIndices = Array.AsReadOnly(group.WitnessIndices.ToArray()) }).ToArray(),
            topology, Diagnostics.ToArray());
    }

    private static ExactDistanceResult CloneExact(ExactDistanceResult source)
    {
        List<ShapeDistanceSolution> copies = [];
        try
        {
            foreach (ShapeDistanceSolution item in source.Solutions)
                copies.Add(new ShapeDistanceSolution(item.Distance, item.PointOnFirst, item.PointOnSecond,
                    item.FirstSupportKind, item.SecondSupportKind, CloneShape(item.FirstSupport), CloneShape(item.SecondSupport),
                    item.FirstEdgeParameter, item.SecondEdgeParameter, item.FirstFaceParameters,
                    item.SecondFaceParameters, item.IsInnerSolution));
            return new ExactDistanceResult(copies, source.Units);
        }
        catch
        {
            foreach (ShapeDistanceSolution copy in copies) copy.Dispose();
            throw;
        }
    }

    private static Shape CloneShape(Shape shape)
    {
        using TopLocLocation identity = TopLocLocation.Identity;
        return shape.Located(identity);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _exactDistance?.Dispose();
        IssueTopology?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed record DigitalMockupIssueGroup(
    string Key,
    DigitalMockupSeverity Severity,
    IReadOnlyList<DigitalMockupPairId> PairIds);

public sealed record DigitalMockupAggregation(
    IReadOnlyList<DigitalMockupIssueGroup> BySeverity,
    IReadOnlyList<DigitalMockupIssueGroup> ByOccurrence,
    IReadOnlyList<DigitalMockupIssueGroup> ByDefinition);

public sealed record DigitalMockupTiming(
    TimeSpan Bounds,
    TimeSpan BroadPhase,
    TimeSpan ExactPhase,
    TimeSpan SelfCheck,
    TimeSpan Total);

public sealed record DigitalMockupSummary(
    int InputCount,
    int RequestedPairCount,
    int CandidatePairCount,
    int ExactPairCount,
    int FilteredPairCount,
    int BroadPhaseRejectedPairCount,
    int FailedPairCount,
    int IssuePairCount,
    int SelfIssueCount,
    int ReusedPairCount,
    int AxisComparisonCount,
    DigitalMockupTiming Timing);

/// <summary>Copied report plus independently owning pair topology.</summary>
public sealed class DigitalMockupReport : IDisposable
{
    private bool _disposed;

    internal DigitalMockupReport(
        IReadOnlyList<DigitalMockupItemSnapshot> items,
        IReadOnlyList<DigitalMockupPairResult> pairs,
        IReadOnlyList<DigitalMockupSelfCheckResult> selfChecks,
        IReadOnlyList<DigitalMockupDiagnostic> diagnostics,
        DigitalMockupSummary summary,
        DigitalMockupPolicy policy)
    {
        Items = items;
        Pairs = pairs;
        SelfChecks = selfChecks;
        Diagnostics = diagnostics;
        Summary = summary;
        Policy = policy;
        PairById = pairs.ToDictionary(pair => pair.Id);
        Aggregation = BuildAggregation(items, pairs);
    }

    public IReadOnlyList<DigitalMockupItemSnapshot> Items { get; }
    public IReadOnlyList<DigitalMockupPairResult> Pairs { get; }
    public IReadOnlyDictionary<DigitalMockupPairId, DigitalMockupPairResult> PairById { get; }
    public IReadOnlyList<DigitalMockupSelfCheckResult> SelfChecks { get; }
    public IReadOnlyList<DigitalMockupDiagnostic> Diagnostics { get; }
    public DigitalMockupSummary Summary { get; }
    public DigitalMockupPolicy Policy { get; }
    public DigitalMockupAggregation Aggregation { get; }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private static DigitalMockupAggregation BuildAggregation(
        IReadOnlyList<DigitalMockupItemSnapshot> items,
        IReadOnlyList<DigitalMockupPairResult> pairs)
    {
        Dictionary<string, DigitalMockupItemSnapshot> byId = items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        DigitalMockupPairResult[] issues = pairs.Where(pair => pair.IsIssue).ToArray();
        DigitalMockupIssueGroup[] severity = issues
            .GroupBy(pair => pair.Severity)
            .OrderByDescending(group => group.Key)
            .Select(group => new DigitalMockupIssueGroup(group.Key.ToString(), group.Key,
                group.Select(pair => pair.Id).OrderBy(id => id.ToString(), StringComparer.Ordinal).ToArray()))
            .ToArray();
        DigitalMockupIssueGroup[] occurrence = issues
            .SelectMany(pair => new[] { (pair.FirstId, pair), (pair.SecondId, pair) })
            .GroupBy(item => item.Item1, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new DigitalMockupIssueGroup(group.Key, group.Max(item => item.pair.Severity),
                group.Select(item => item.pair.Id).Distinct().OrderBy(id => id.ToString(), StringComparer.Ordinal).ToArray()))
            .ToArray();
        DigitalMockupIssueGroup[] definition = issues
            .SelectMany(pair => new[] { pair.FirstId, pair.SecondId }
                .Select(id => (Definition: byId[id].DefinitionId, Pair: pair)))
            .Where(item => item.Definition is not null)
            .GroupBy(item => item.Definition!, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new DigitalMockupIssueGroup(group.Key, group.Max(item => item.Pair.Severity),
                group.Select(item => item.Pair.Id).Distinct().OrderBy(id => id.ToString(), StringComparer.Ordinal).ToArray()))
            .ToArray();
        return new(severity, occurrence, definition);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (DigitalMockupPairResult pair in Pairs) pair.Dispose();
        GC.SuppressFinalize(this);
    }
}

#pragma warning restore CS1591
