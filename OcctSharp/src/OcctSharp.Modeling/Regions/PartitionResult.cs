namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable partition identities and copied reports with privately owned exact topology.</summary>
public sealed class PartitionResult : IDisposable
{
    private readonly RegionStorage storage;
    private readonly int[] cellItems;
    private readonly Dictionary<int, int> boundaryItems;
    private readonly Dictionary<string, int> outputIndices;
    internal PartitionResult(RegionStorage storage, int inputCount, RegionProgram[] programs, Guid planId)
    {
        this.storage = storage; Revision = Guid.NewGuid(); PlanId = planId;
        var memberships = storage.Find(RegionItemKind.Membership).GroupBy(x => x.Item.A)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Item.B, x => (RegionMembership)x.Item.C));
        var cells = storage.Find(RegionItemKind.Cell).OrderBy(x => x.Item.A).ToArray();
        cellItems = cells.Select(x => x.Index).ToArray();
        Cells = Array.AsReadOnly(cells.Select(x => new RegionCell(new(Revision, x.Item.A), x.Item.B, (ShapeKind)x.Item.C,
            x.Item.Measure, x.Item.Flags != 0, Array.AsReadOnly(Enumerable.Range(0, inputCount)
                .Select(i => memberships.GetValueOrDefault(x.Item.A)?.GetValueOrDefault(i, RegionMembership.Unknown) ?? RegionMembership.Unknown).ToArray()))).ToArray());
        var boundaries = storage.Find(RegionItemKind.Boundary).ToArray();
        boundaryItems = boundaries.ToDictionary(x => x.Item.A, x => x.Index);
        var uses = storage.Find(RegionItemKind.BoundaryUse).GroupBy(x => x.Item.A)
            .ToDictionary(g => g.Key, g => g.Select(x => new RegionBoundaryUse(new(Revision, x.Item.B), x.Item.C)).ToArray());
        Boundaries = Array.AsReadOnly(boundaries.Select(x => new RegionBoundary(new(Revision, x.Item.A), x.Item.B, x.Item.Measure,
            Array.AsReadOnly(uses.GetValueOrDefault(x.Item.A) ?? []))).ToArray());
        outputIndices = programs.Select((p, i) => (p.Key, Index: i)).ToDictionary(p => p.Key, p => p.Index, StringComparer.Ordinal);
        OutputKeys = Array.AsReadOnly(programs.Select(p => p.Key).ToArray());
        var measures = storage.Find(RegionItemKind.InputMeasure).GroupBy(x => (x.Item.A, x.Item.B));
        Conservation = Array.AsReadOnly(measures.Select(g => new RegionConservation(g.Key.A, g.Key.B,
            g.Single(x => x.Item.C == 0).Item.Measure, g.Single(x => x.Item.C == 1).Item.Measure)).ToArray());
        History = Array.AsReadOnly(storage.Find(RegionItemKind.History).Select(x => new RegionHistoryReference(Revision, x.Item.A,
            x.Item.B, x.Item.C, (ShapeKind)x.Item.Flags, (RegionHistoryKind)x.Item.D, x.Index)).ToArray());
        Diagnostics = new(storage.Info.Done != 0, storage.Info.Valid != 0, storage.Info.Warnings != 0, storage.Message,
            Array.AsReadOnly(storage.Find(RegionItemKind.Fault).Select(x => x.Item.A).Distinct().ToArray()));
        Faults = Array.AsReadOnly(storage.Find(RegionItemKind.Fault).Select(x => new RegionArgumentFault(Revision, x.Item.A,
            x.Item.B < 0 ? null : x.Item.B, x.Item.C, x.Index)).ToArray());
    }
    public Guid Revision { get; }
    public Guid PlanId { get; }
    public IReadOnlyList<RegionCell> Cells { get; }
    public IReadOnlyList<RegionBoundary> Boundaries { get; }
    public IReadOnlyList<string> OutputKeys { get; }
    public IReadOnlyList<RegionConservation> Conservation { get; }
    public IReadOnlyList<RegionHistoryReference> History { get; }
    public RegionDiagnostics Diagnostics { get; }
    public IReadOnlyList<RegionArgumentFault> Faults { get; }
    public Shape CopyFaultShape(RegionArgumentFault fault)
    {
        storage.ThrowIfDisposed();
        if (!Faults.Contains(fault)) throw new ArgumentException("Foreign fault record.");
        return storage.Copy(fault.ItemIndex);
    }
    public Shape CopyCell(RegionCellId id) { Validate(id); return storage.Copy(cellItems[id.Index]); }
    public Shape CopyBoundary(RegionBoundaryId id)
    {
        storage.ThrowIfDisposed();
        if (id.Revision != Revision || !boundaryItems.TryGetValue(id.Index, out int item)) throw new ArgumentException("Foreign or invalid boundary ID.");
        return storage.Copy(item);
    }
    public Shape CopyOutput(string key)
    {
        int output = OutputIndex(key);
        if (!Diagnostics.AlgorithmDone || !Diagnostics.IsValid) throw new InvalidOperationException($"Partition is not accepted: {Diagnostics.Message}");
        return storage.Copy(storage.Find(RegionItemKind.Output).Single(x => x.Item.A == output).Index);
    }
    public Shape CopyHistoryShape(RegionHistoryReference item)
    {
        ArgumentNullException.ThrowIfNull(item); storage.ThrowIfDisposed();
        if (!History.Contains(item) || item.Kind is RegionHistoryKind.Deleted or RegionHistoryKind.Unavailable)
            throw new ArgumentException("History is foreign or has no mapped topology.");
        return storage.Copy(item.ItemIndex);
    }
    public IReadOnlyList<RegionAssignment> GetAssignments(string key)
    {
        int output = OutputIndex(key);
        return Array.AsReadOnly(storage.Find(RegionItemKind.Assignment).Where(x => x.Item.A == output)
            .Select(x => new RegionAssignment(new(Revision, x.Item.B), x.Item.C, x.Item.D, x.Item.Measure)).ToArray());
    }
    public IReadOnlyList<RegionRuleEffect> GetRuleEffects(string key)
    {
        int output = OutputIndex(key);
        return Array.AsReadOnly(storage.Find(RegionItemKind.RuleEffect).Where(x => x.Item.A == output)
            .Select(x => new RegionRuleEffect(x.Item.B, new(Revision, x.Item.C), x.Item.D < 0 ? null : x.Item.D,
                x.Item.Flags < 0 ? null : x.Item.Flags)).ToArray());
    }
    /// <summary>Boundaries separating different selected materials; seam repeats do not create adjacency.</summary>
    public IReadOnlyList<RegionBoundary> GetMaterialInterfaces(string key)
    {
        var materials = GetAssignments(key).ToDictionary(x => x.Cell.Index, x => x.Material);
        return Array.AsReadOnly(Boundaries.Where(b => b.Uses.Select(u => u.Cell.Index).Distinct().Where(materials.ContainsKey)
            .Select(i => materials[i]).Distinct().Skip(1).Any()).ToArray());
    }
    /// <summary>External boundaries of the selected union, with material and original-argument provenance.</summary>
    public IReadOnlyList<RegionEnvelopeBoundary> GetEnvelope(string key)
    {
        var materials = GetAssignments(key).ToDictionary(x => x.Cell.Index, x => x.Material);
        List<RegionEnvelopeBoundary> result = [];
        foreach (var boundary in Boundaries)
        {
            var selected = boundary.Uses.Select(u => u.Cell.Index).Distinct().Where(materials.ContainsKey).ToArray();
            // A seam occurs twice on one cell and is not an external boundary.
            if (selected.Length != 1 || boundary.Uses.Count(u => u.Cell.Index == selected[0]) != 1) continue;
            int cell = selected[0];
            result.Add(new(boundary.Id, materials[cell], Array.AsReadOnly(new[] { Cells[cell].Id }),
                Array.AsReadOnly(Cells[cell].InputMembership.Select((m, i) => (m, i)).Where(x => x.m == RegionMembership.Inside).Select(x => x.i).ToArray())));
        }
        return result.AsReadOnly();
    }
    public IReadOnlyList<ConnectedMaterialRegion> GetConnectedRegions(string key)
    {
        var assignments = GetAssignments(key).ToDictionary(x => x.Cell.Index);
        var adjacency = assignments.Keys.ToDictionary(i => i, _ => new HashSet<int>());
        foreach (var boundary in Boundaries)
        {
            var cells = boundary.Uses.Select(u => u.Cell.Index).Distinct().Where(assignments.ContainsKey).ToArray();
            foreach (int a in cells) foreach (int b in cells)
                if (a != b && assignments[a].Material == assignments[b].Material && assignments[a].Dimension == assignments[b].Dimension)
                    adjacency[a].Add(b);
        }
        HashSet<int> visited = []; List<ConnectedMaterialRegion> regions = [];
        foreach (int seed in assignments.Keys.Order())
        {
            if (!visited.Add(seed)) continue; Queue<int> pending = new(); pending.Enqueue(seed); List<int> group = [];
            while (pending.TryDequeue(out int cell))
            {
                group.Add(cell); foreach (int next in adjacency[cell]) if (visited.Add(next)) pending.Enqueue(next);
            }
            group.Sort(); regions.Add(new(assignments[seed].Material, assignments[seed].Dimension,
                Array.AsReadOnly(group.Select(i => Cells[i].Id).ToArray()), group.Sum(i => Cells[i].Measure)));
        }
        return regions.AsReadOnly();
    }
    public Shape CopyCells(IReadOnlyList<RegionCellId> cells)
    {
        ArgumentNullException.ThrowIfNull(cells); storage.ThrowIfDisposed();
        if (cells.Count == 0) throw new ArgumentException("Select at least one cell.");
        foreach (var cell in cells) Validate(cell);
        using var compound = ShapeFactory.CreateCompound(cells.Distinct().Select(c => storage.Get(cellItems[c.Index])).ToArray());
        return RegionStorage.CopyShape(compound);
    }
    /// <summary>Explicit absolute/relative sliver policy. Selecting does not automatically delete or heal cells.</summary>
    public IReadOnlyList<RegionCellId> SelectSlivers(int dimension, double absoluteMaximum, double relativeMaximum = 0)
    {
        storage.ThrowIfDisposed();
        if (dimension is < 0 or > 3 || !double.IsFinite(absoluteMaximum) || absoluteMaximum < 0 ||
            !double.IsFinite(relativeMaximum) || relativeMaximum < 0 || relativeMaximum > 1) throw new ArgumentException("Invalid sliver policy.");
        double total = Cells.Where(c => c.Dimension == dimension).Sum(c => c.Measure);
        double maximum = Math.Max(absoluteMaximum, relativeMaximum * total);
        return Array.AsReadOnly(Cells.Where(c => c.Dimension == dimension && c.Measure <= maximum).Select(c => c.Id).ToArray());
    }
    public RegionPrecisionVerdict EvaluatePrecision(RegionPrecisionPolicy? policy = null)
    {
        storage.ThrowIfDisposed(); policy ??= new();
        if (!double.IsFinite(policy.AbsoluteMeasureError) || policy.AbsoluteMeasureError < 0 ||
            !double.IsFinite(policy.RelativeMeasureError) || policy.RelativeMeasureError < 0 || policy.MaximumCells < 1)
            throw new ArgumentException("Invalid precision acceptance policy.");
        List<string> reasons = [];
        if (!Diagnostics.AlgorithmDone) reasons.Add("Algorithm did not complete.");
        if (policy.RequireValid && !Diagnostics.IsValid) reasons.Add("Partition topology is invalid.");
        if (Cells.Count > policy.MaximumCells) reasons.Add("Cell-growth budget exceeded.");
        foreach (var measure in Conservation)
            if (measure.AbsoluteError > policy.AbsoluteMeasureError + policy.RelativeMeasureError * measure.OriginalMeasure)
                reasons.Add($"Input {measure.InputIndex}, dimension {measure.Dimension}: conservation budget exceeded.");
        return new(reasons.Count == 0, reasons.AsReadOnly());
    }
    private int OutputIndex(string key)
    {
        storage.ThrowIfDisposed(); ArgumentNullException.ThrowIfNull(key);
        if (!outputIndices.TryGetValue(key, out int index)) throw new ArgumentException("Unknown region output key.");
        return index;
    }
    private void Validate(RegionCellId id)
    {
        storage.ThrowIfDisposed();
        if (id.Revision != Revision || (uint)id.Index >= cellItems.Length) throw new ArgumentException("Foreign or invalid cell ID.");
    }
    public void Dispose() => storage.Dispose();
    internal Shape[] CopyCellBoundaryGraph(RegionCellId cell, out RegionBoundaryId[] boundaries)
    {
        Validate(cell);
        boundaries = Boundaries.Where(b => b.Dimension == 2 && b.Uses.Any(u => u.Cell == cell)).Select(b => b.Id).ToArray();
        return AuthoringBridge.CopyInputs(new[] { storage.Get(cellItems[cell.Index]) }
            .Concat(boundaries.Select(b => storage.Get(boundaryItems[b.Index]))).ToArray());
    }
}
