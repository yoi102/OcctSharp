namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable, bounded set expression over input membership; no user-code evaluation.</summary>
public sealed class RegionExpression
{
    private readonly int[] tokens;
    private RegionExpression(int[] tokens) => this.tokens = tokens;
    internal ReadOnlySpan<int> Tokens => tokens;
    /// <summary>Copies the finite postfix recipe for persistence.</summary>
    public int[] CopyTokens() => (int[])tokens.Clone();
    /// <summary>Validates a copied finite postfix recipe; input indices are checked again at build time.</summary>
    public static RegionExpression FromTokens(IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count is < 1 or > 4096) throw new ArgumentException("Expression token count is outside bounds.");
        int[] copy = values.ToArray(); int depth = 0;
        foreach (int token in copy)
        {
            if (token is >= 0 and < 128 or -1 or -2) depth++;
            else if (token is >= -5 and <= -3 && depth >= 2) depth--;
            else throw new ArgumentException("Malformed region expression.");
        }
        if (depth != 1) throw new ArgumentException("Expression must leave exactly one result.");
        return new(copy);
    }
    public static RegionExpression All { get; } = new([-1]);
    public static RegionExpression None { get; } = new([-2]);
    public static RegionExpression Input(int index)
    {
        if (index is < 0 or >= 128) throw new ArgumentOutOfRangeException(nameof(index));
        return new([index]);
    }
    public RegionExpression Union(RegionExpression other) => Combine(other, -3);
    public RegionExpression Intersect(RegionExpression other) => Combine(other, -4);
    public RegionExpression Except(RegionExpression other) => Combine(other, -5);
    private RegionExpression Combine(RegionExpression other, int op)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (tokens.Length + other.tokens.Length + 1 > 4096) throw new ArgumentException("Region expression exceeds 4096 tokens.");
        return new([.. tokens, .. other.tokens, op]);
    }
}
public enum RegionRuleAction { Add, Remove }
public enum RegionMembership { Unknown = -1, Outside, Inside }
public readonly record struct RegionCellId(Guid Revision, int Index);
public readonly record struct RegionBoundaryId(Guid Revision, int Index);
public sealed record RegionRule(RegionExpression Expression, int Material = 0,
    RegionRuleAction Action = RegionRuleAction.Add, int? Dimension = null, double? MaximumMeasure = null);

/// <summary>One independent output. All ordered rules precede boundary removal and container assembly.</summary>
public sealed class RegionProgram
{
    public RegionProgram(string key, IReadOnlyList<RegionRule> rules, bool removeInternalBoundaries = false, bool makeContainers = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key); ArgumentNullException.ThrowIfNull(rules);
        if (rules.Count > 4096) throw new ArgumentException("A program exceeds 4096 rules.");
        var copied = rules.ToArray();
        foreach (var rule in copied)
        {
            ArgumentNullException.ThrowIfNull(rule); ArgumentNullException.ThrowIfNull(rule.Expression);
            if (rule.Material < 0 || !Enum.IsDefined(rule.Action) || rule.Dimension is < 0 or > 3 ||
                rule.MaximumMeasure is double measure && (!double.IsFinite(measure) || measure < 0))
                throw new ArgumentException("Invalid region rule material, dimension or measure.");
        }
        Key = key; Rules = Array.AsReadOnly(copied); RemoveInternalBoundaries = removeInternalBoundaries; MakeContainers = makeContainers;
    }
    public string Key { get; }
    public IReadOnlyList<RegionRule> Rules { get; }
    public bool RemoveInternalBoundaries { get; }
    public bool MakeContainers { get; }
}
public sealed record PartitionOptions(double FuzzyTolerance = 0, bool RunParallel = false, bool CheckInputs = true, int MaximumCells = 100000);
public sealed record RegionPrecisionPolicy(double AbsoluteMeasureError = 1e-7, double RelativeMeasureError = 1e-8,
    int MaximumCells = 100000, bool RequireValid = true);
public sealed record RegionCell(RegionCellId Id, int Dimension, ShapeKind Kind, double Measure, bool MembershipKnown,
    IReadOnlyList<RegionMembership> InputMembership);
public sealed record RegionBoundaryUse(RegionCellId Cell, int Orientation);
public sealed record RegionBoundary(RegionBoundaryId Id, int Dimension, double Measure, IReadOnlyList<RegionBoundaryUse> Uses);
public sealed record RegionAssignment(RegionCellId Cell, int Material, int Dimension, double Measure);
public sealed record RegionRuleEffect(int RuleIndex, RegionCellId Cell, int? PreviousMaterial, int? NewMaterial);
public sealed record RegionConservation(int InputIndex, int Dimension, double OriginalMeasure, double PartitionMeasure)
{
    public double AbsoluteError => Math.Abs(OriginalMeasure - PartitionMeasure);
}
public enum RegionHistoryKind { Modified, Unchanged, Deleted, Unavailable }
public sealed record RegionHistoryReference(Guid Revision, int OutputIndex, int InputIndex, int TopologyIndex, ShapeKind SourceKind,
    RegionHistoryKind Kind, int ItemIndex);
public sealed record RegionDiagnostics(bool AlgorithmDone, bool IsValid, bool HasWarnings, string Message, IReadOnlyList<int> InvalidInputs);
public sealed record RegionArgumentFault(Guid Revision, int InputIndex, int? TopologyIndex, int NativeStatus, int ItemIndex);
public sealed record RegionPrecisionVerdict(bool Accepted, IReadOnlyList<string> Reasons);
public sealed record RegionEnvelopeBoundary(RegionBoundaryId Boundary, int Material, IReadOnlyList<RegionCellId> Cells,
    IReadOnlyList<int> InputIndices);
public sealed record ConnectedMaterialRegion(int Material, int Dimension, IReadOnlyList<RegionCellId> Cells, double Measure);
