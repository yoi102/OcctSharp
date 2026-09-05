using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns a private input graph. Each build creates a fresh partition revision and native-local builder.</summary>
public sealed class PartitionPlan : IDisposable
{
    private readonly Shape[] inputs;
    private bool disposed;
    private PartitionPlan(Shape[] inputs, PartitionOptions options) { this.inputs = inputs; Options = options; }
    /// <summary>Copied build options; tolerance never escalates automatically.</summary>
    public PartitionOptions Options { get; }
    /// <summary>Identifies this captured input graph, separately from each result revision.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>Captures all arguments in one copy while preserving existing shared topology.</summary>
    public static PartitionPlan Create(IReadOnlyList<Shape> inputs, PartitionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(inputs); options ??= new();
        if (inputs.Count is < 1 or > 128 || !double.IsFinite(options.FuzzyTolerance) || options.FuzzyTolerance < 0 || options.MaximumCells is < 1 or > 100000)
            throw new ArgumentException("Invalid partition input count, precision or capacity.");
        return new(AuthoringBridge.CopyInputs(inputs), options);
    }
    /// <summary>Returns all unselected parts plus any requested independent region outputs in one build.</summary>
    public unsafe PartitionResult Build(IReadOnlyList<RegionProgram>? programs = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this); var selected = programs?.ToArray() ?? [];
        if (selected.Length > 128 || selected.Any(p => p is null) || selected.Select(p => p.Key).Distinct(StringComparer.Ordinal).Count() != selected.Length)
            throw new ArgumentException("Output programs require unique keys and at most 128 outputs.");
        List<RegionRuleRaw> rules = []; List<int> expressions = []; RegionOutputRaw[] outputs = new RegionOutputRaw[selected.Length];
        for (int output = 0; output < selected.Length; output++)
        {
            var program = selected[output]; outputs[output] = new() { RemoveBoundaries = program.RemoveInternalBoundaries ? 1 : 0, Containers = program.MakeContainers ? 1 : 0 };
            foreach (var rule in program.Rules)
            {
                if (rules.Count == 4096 || rule.Expression.Tokens.Length > 100000 - expressions.Count)
                    throw new ArgumentException("Combined region program exceeds its bounds.");
                foreach (int token in rule.Expression.Tokens) if (token >= inputs.Length) throw new ArgumentException("Expression refers to a missing input.");
                rules.Add(new() { Output = output, Action = (int)rule.Action, Material = rule.Material, Offset = expressions.Count,
                    Count = rule.Expression.Tokens.Length, Dimension = rule.Dimension ?? -1, MaximumMeasure = rule.MaximumMeasure ?? -1 });
                expressions.AddRange(rule.Expression.Tokens);
            }
        }
        if (rules.Count > 4096 || expressions.Count > 100000) throw new ArgumentException("Combined region program exceeds its bounds.");
        RegionRuleRaw[] rawRules = rules.ToArray(); int[] rawExpressions = expressions.ToArray();
        PartitionOptionsRaw rawOptions = new() { Fuzzy = Options.FuzzyTolerance, Parallel = Options.RunParallel ? 1 : 0,
            CheckInputs = Options.CheckInputs ? 1 : 0, MaxCells = Options.MaximumCells };
        RegionStorage storage = AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            fixed (RegionRuleRaw* r = rawRules) fixed (int* e = rawExpressions) fixed (RegionOutputRaw* o = outputs)
            {
                NativeError.ThrowIfFailed(NativeMethods.PartitionBuild(p, count, in rawOptions, r, rawRules.Length,
                    e, rawExpressions.Length, o, outputs.Length, out nint result), "partition_build");
                return RegionStorage.Read(result);
            }
        });
        try { return new(storage, inputs.Length, selected, Id); } catch { storage.Dispose(); throw; }
    }
    /// <summary>Releases only this plan's private inputs; independent results survive.</summary>
    public void Dispose() { if (disposed) return; disposed = true; foreach (var input in inputs) input.Dispose(); }
}
