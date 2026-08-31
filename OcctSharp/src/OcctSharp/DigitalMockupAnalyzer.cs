using System.Diagnostics;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Runs one occurrence-aware broad/exact-phase digital mock-up analysis.</summary>
public static class DigitalMockupAnalyzer
{
    /// <summary>Analyzes caller-owned shapes synchronously; returned topology is independent.</summary>
    public static DigitalMockupReport Analyze(
        IReadOnlyList<DigitalMockupItem> items,
        DigitalMockupPolicy? policy = null) =>
        AnalyzeCore(items, (policy ?? new DigitalMockupPolicy()).Validated(), null, null);

    /// <summary>
    /// Recomputes pairs involving changed IDs and independently clones unchanged results.
    /// The previous report must remain alive for the duration of this call.
    /// </summary>
    public static DigitalMockupReport AnalyzeIncremental(
        DigitalMockupReport previous,
        IReadOnlyList<DigitalMockupItem> items,
        IEnumerable<string> changedItemIds,
        DigitalMockupPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(changedItemIds);
        previous.ThrowIfDisposed();
        HashSet<string> changed = new(changedItemIds, StringComparer.Ordinal);
        if (changed.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Changed item IDs cannot be null or whitespace.", nameof(changedItemIds));
        return AnalyzeCore(items, (policy ?? previous.Policy).Validated(), previous, changed);
    }

    /// <summary>Expands world-located leaf occurrences from one or more XDE assembly roots.</summary>
    public static DigitalMockupReport AnalyzeAssembly(
        IReadOnlyList<XdeLabel> assemblyRoots,
        DigitalMockupPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(assemblyRoots);
        if (assemblyRoots.Count == 0) throw new ArgumentException("At least one assembly root is required.", nameof(assemblyRoots));
        List<XdeOccurrence> occurrences = [];
        List<Shape> shapes = [];
        try
        {
            List<DigitalMockupItem> items = [];
            foreach (XdeLabel root in assemblyRoots)
            {
                ArgumentNullException.ThrowIfNull(root);
                if (!root.IsAssembly) throw new ArgumentException($"XDE root '{root.Entry}' is not an assembly.", nameof(assemblyRoots));
                foreach (XdeOccurrence occurrence in root.GetOccurrences(recursive: true))
                {
                    occurrences.Add(occurrence);
                    if (occurrence.IsAssembly) continue;
                    Shape located = occurrence.GetLocatedShape();
                    shapes.Add(located);
                    string id = $"{root.Entry}/{string.Join('/', occurrence.Path)}";
                    string? name = occurrence.OccurrenceLabel.Name ?? occurrence.ReferredLabel.Name;
                    items.Add(new DigitalMockupItem(id, located, occurrence.ReferredLabel.Entry,
                        occurrence.Path, name: name));
                }
            }
            if (items.Count == 0) throw new ArgumentException("The selected XDE roots contain no leaf occurrences.", nameof(assemblyRoots));
            return Analyze(items, policy);
        }
        finally
        {
            foreach (Shape shape in shapes) shape.Dispose();
            foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose();
        }
    }

    /// <summary>Expands and analyzes one XDE assembly root.</summary>
    public static DigitalMockupReport AnalyzeAssembly(XdeLabel assemblyRoot, DigitalMockupPolicy? policy = null) =>
        AnalyzeAssembly([assemblyRoot], policy);

    private static DigitalMockupReport AnalyzeCore(
        IReadOnlyList<DigitalMockupItem> sourceItems,
        DigitalMockupPolicy policy,
        DigitalMockupReport? previous,
        HashSet<string>? changedIds)
    {
        ArgumentNullException.ThrowIfNull(sourceItems);
        if (sourceItems.Count == 0) throw new ArgumentException("At least one digital mock-up item is required.", nameof(sourceItems));
        DigitalMockupItem[] items = sourceItems.OrderBy(item => item?.Id, StringComparer.Ordinal).ToArray()!;
        if (items.Any(item => item is null)) throw new ArgumentException("The item collection contains null.", nameof(sourceItems));
        if (items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != items.Length)
            throw new ArgumentException("Digital mock-up item IDs must be unique.", nameof(sourceItems));

        long totalStart = Stopwatch.GetTimestamp();
        List<DigitalMockupDiagnostic> diagnostics = [];
        long boundsStart = Stopwatch.GetTimestamp();
        DigitalMockupItemSnapshot[] snapshots = new DigitalMockupItemSnapshot[items.Length];
        List<int> usableIndices = [];
        for (int index = 0; index < items.Length; ++index)
        {
            DigitalMockupItem item = items[index];
            try
            {
                BoundingBox3d aabb = item.Shape.GetBoundingBox();
                OrientedBoundingBox3d obb = item.Shape.GetOrientedBoundingBox();
                snapshots[index] = new(item.Id, item.Name, item.DefinitionId,
                    Array.AsReadOnly(item.OccurrencePath.ToArray()), aabb, obb, true);
                usableIndices.Add(index);
            }
            catch (Exception exception) when (exception is OcctException or ObjectDisposedException or InvalidOperationException)
            {
                snapshots[index] = new(item.Id, item.Name, item.DefinitionId,
                    Array.AsReadOnly(item.OccurrencePath.ToArray()), null, null, false);
                diagnostics.Add(new(DigitalMockupStage.Bounds, DigitalMockupDiagnosticSeverity.Error,
                    "DMU001", exception.Message, item.Id));
            }
        }
        TimeSpan boundsElapsed = Stopwatch.GetElapsedTime(boundsStart);

        long broadStart = Stopwatch.GetTimestamp();
        (HashSet<DigitalMockupPairId> candidates, int axisComparisons) =
            GetCandidates(items, usableIndices, policy.Clearance + Math.Max(policy.ConfusionTolerance, policy.FuzzyTolerance));
        if (policy.ExactDistanceForAllPairs)
            for (int first = 0; first < items.Length; ++first)
                for (int second = first + 1; second < items.Length; ++second)
                    if (snapshots[first].IsUsable && snapshots[second].IsUsable)
                        candidates.Add(new(items[first].Id, items[second].Id));
        TimeSpan broadElapsed = Stopwatch.GetElapsedTime(broadStart);

        HashSet<DigitalMockupPairId> explicitExclusions = new(policy.ExcludedPairs);
        List<PairWork> work = [];
        for (int first = 0; first < items.Length; ++first)
        {
            for (int second = first + 1; second < items.Length; ++second)
            {
                DigitalMockupItem left = items[first];
                DigitalMockupItem right = items[second];
                DigitalMockupPairId id = new(left.Id, right.Id);
                DigitalMockupFilterReason filter = GetFilter(left, right, snapshots[first], snapshots[second], id, explicitExclusions, policy);
                double lowerBound = snapshots[first].AxisAlignedBounds is { } firstBounds
                    && snapshots[second].AxisAlignedBounds is { } secondBounds
                    ? BoundsDistance(firstBounds, secondBounds)
                    : double.NaN;
                work.Add(new(id, left, right, filter, candidates.Contains(id), lowerBound));
            }
        }

        bool canReuse = previous is not null && changedIds is not null && PolicyEquivalent(previous.Policy, policy);
        HashSet<string> previousItemIds = previous?.Items.Select(item => item.Id).ToHashSet(StringComparer.Ordinal) ?? [];
        DigitalMockupPairResult?[] pairResults = new DigitalMockupPairResult?[work.Count];
        int reusedCount = 0;
        for (int index = 0; index < work.Count; ++index)
        {
            PairWork pair = work[index];
            if (canReuse
                && !changedIds!.Contains(pair.Id.FirstId)
                && !changedIds.Contains(pair.Id.SecondId)
                && previousItemIds.Contains(pair.Id.FirstId)
                && previousItemIds.Contains(pair.Id.SecondId)
                && previous!.PairById.TryGetValue(pair.Id, out DigitalMockupPairResult? old))
            {
                pairResults[index] = old.CloneOwned();
                ++reusedCount;
            }
        }

        long exactStart = Stopwatch.GetTimestamp();
        IEnumerable<int> pending = Enumerable.Range(0, work.Count).Where(index => pairResults[index] is null);
        if (policy.RunParallel && !policy.EarlyExit)
        {
            Parallel.ForEach(pending, index => pairResults[index] = AnalyzePair(work[index], policy));
        }
        else
        {
            bool stop = false;
            foreach (int index in pending)
            {
                if (stop && work[index].Filter == DigitalMockupFilterReason.None && work[index].IsCandidate)
                {
                    pairResults[index] = Terminal(work[index], DigitalMockupPairState.SkippedAfterEarlyExit,
                        DigitalMockupSeverity.Information, DigitalMockupFilterReason.EarlyExit);
                    continue;
                }
                pairResults[index] = AnalyzePair(work[index], policy);
                if (policy.EarlyExit && pairResults[index]!.Severity == DigitalMockupSeverity.Critical) stop = true;
            }
        }
        TimeSpan exactElapsed = Stopwatch.GetElapsedTime(exactStart);

        long selfStart = Stopwatch.GetTimestamp();
        List<DigitalMockupSelfCheckResult> selfChecks = [];
        if (policy.RunSelfChecks)
        {
            Dictionary<string, DigitalMockupSelfCheckResult> previousChecks = previous?.SelfChecks
                .ToDictionary(item => item.ItemId, StringComparer.Ordinal) ?? [];
            for (int index = 0; index < items.Length; ++index)
            {
                DigitalMockupItem item = items[index];
                if (canReuse && !changedIds!.Contains(item.Id) && previousChecks.TryGetValue(item.Id, out DigitalMockupSelfCheckResult? copied))
                {
                    selfChecks.Add(copied with { ValidationIssues = Array.AsReadOnly(copied.ValidationIssues.ToArray()) });
                    continue;
                }
                selfChecks.Add(SelfCheck(item, snapshots[index], policy, diagnostics));
            }
        }
        TimeSpan selfElapsed = Stopwatch.GetElapsedTime(selfStart);

        DigitalMockupPairResult[] completedPairs = pairResults.Select(result => result!).ToArray();
        foreach (DigitalMockupPairResult pair in completedPairs) diagnostics.AddRange(pair.Diagnostics);
        int requestedCount = checked(items.Length * (items.Length - 1) / 2);
        TimeSpan totalElapsed = Stopwatch.GetElapsedTime(totalStart);
        DigitalMockupSummary summary = new(
            items.Length,
            requestedCount,
            candidates.Count,
            completedPairs.Count(pair => pair.IsExact),
            completedPairs.Count(pair => pair.State == DigitalMockupPairState.Excluded),
            completedPairs.Count(pair => pair.State == DigitalMockupPairState.BroadPhaseClear),
            completedPairs.Count(pair => pair.State == DigitalMockupPairState.Failed),
            completedPairs.Count(pair => pair.IsIssue),
            selfChecks.Count(item => !item.IsValid || item.FaultyShapeCount > 0),
            reusedCount,
            axisComparisons,
            new(boundsElapsed, broadElapsed, exactElapsed, selfElapsed, totalElapsed));
        diagnostics.Add(new(DigitalMockupStage.Aggregation, DigitalMockupDiagnosticSeverity.Information,
            "DMU000", $"Analyzed {items.Length} items, {requestedCount} requested pairs, {candidates.Count} broad-phase candidates, and {summary.ExactPairCount} exact pairs."));
        return new DigitalMockupReport(snapshots, completedPairs, selfChecks, diagnostics.ToArray(), summary, policy);
    }

    private static DigitalMockupPairResult AnalyzePair(PairWork work, DigitalMockupPolicy policy)
    {
        if (work.Filter != DigitalMockupFilterReason.None)
        {
            DigitalMockupPairState state = work.Filter == DigitalMockupFilterReason.UnusableInput
                ? DigitalMockupPairState.Failed : DigitalMockupPairState.Excluded;
            DigitalMockupSeverity severity = state == DigitalMockupPairState.Failed
                ? DigitalMockupSeverity.Critical : DigitalMockupSeverity.None;
            return Terminal(work, state, severity, work.Filter);
        }
        if (!work.IsCandidate)
            return Terminal(work, DigitalMockupPairState.BroadPhaseClear, DigitalMockupSeverity.None);

        List<DigitalMockupDiagnostic> diagnostics = [];
        Shape? issue = null;
        ExactDistanceResult? exact = null;
        try
        {
            NativeError.ThrowIfFailed(NativeMethods.AnalyzeDigitalMockupPair(
                work.First.Shape.Handle, work.Second.Shape.Handle,
                policy.ConfusionTolerance, policy.FuzzyTolerance,
                policy.RunParallel ? 1 : 0, policy.NonDestructive ? 1 : 0,
                out int classification, out double distance, out double overlapVolume, out nint issueHandle),
                "digital_mockup_pair_analyze");
            if (issueHandle != 0) issue = ShapeFactory.FromNativeHandle(issueHandle, "digital_mockup_issue_shape");
            try
            {
                exact = work.First.Shape.InspectDistanceTo(work.Second.Shape);
            }
            catch (Exception exception) when (exception is OcctException or InvalidOperationException)
            {
                diagnostics.Add(new(DigitalMockupStage.ExactDistance, DigitalMockupDiagnosticSeverity.Warning,
                    "DMU102", $"Pair classification succeeded but witness extraction failed: {exception.Message}", PairId: work.Id));
            }
            DigitalMockupPairState state = classification switch
            {
                0 => distance <= policy.Clearance + policy.ConfusionTolerance
                    ? DigitalMockupPairState.ClearanceViolation : DigitalMockupPairState.Clear,
                1 => DigitalMockupPairState.Touching,
                2 => DigitalMockupPairState.FirstInsideSecond,
                3 => DigitalMockupPairState.SecondInsideFirst,
                4 => DigitalMockupPairState.Coincident,
                5 => DigitalMockupPairState.Interfering,
                _ => throw new InvalidOperationException($"Unknown native pair classification {classification}.")
            };
            DigitalMockupSeverity severity = state switch
            {
                DigitalMockupPairState.ClearanceViolation => DigitalMockupSeverity.Clearance,
                DigitalMockupPairState.Touching => DigitalMockupSeverity.Contact,
                DigitalMockupPairState.FirstInsideSecond or DigitalMockupPairState.SecondInsideFirst
                    or DigitalMockupPairState.Coincident or DigitalMockupPairState.Interfering => DigitalMockupSeverity.Critical,
                _ => DigitalMockupSeverity.None
            };
            IReadOnlyList<DigitalMockupInterferenceGroup> groups = BuildGroups(exact?.Solutions ?? [], policy.MaxInterferenceGroupsPerPair);
            return new(work.Id, state, severity, DigitalMockupFilterReason.None, true, distance,
                work.LowerBound, overlapVolume, exact, groups, issue, diagnostics);
        }
        catch (Exception exception) when (exception is OcctException or InvalidOperationException or ObjectDisposedException)
        {
            exact?.Dispose();
            issue?.Dispose();
            diagnostics.Add(new(DigitalMockupStage.Classification, DigitalMockupDiagnosticSeverity.Error,
                "DMU101", exception.Message, PairId: work.Id));
            return new(work.Id, DigitalMockupPairState.Failed, DigitalMockupSeverity.Critical,
                DigitalMockupFilterReason.None, false, null, work.LowerBound, 0, null, [], null, diagnostics);
        }
    }

    private static DigitalMockupPairResult Terminal(
        PairWork work,
        DigitalMockupPairState state,
        DigitalMockupSeverity severity,
        DigitalMockupFilterReason filter = DigitalMockupFilterReason.None) =>
        new(work.Id, state, severity, filter, false, null, work.LowerBound, 0, null, [], null, []);

    private static DigitalMockupInterferenceGroup[] BuildGroups(
        IReadOnlyList<ShapeDistanceSolution> witnesses,
        int maximum)
    {
        return witnesses.Select((witness, index) => (witness, index))
            .GroupBy(item => (item.witness.FirstSupportKind, item.witness.SecondSupportKind))
            .Take(maximum)
            .Select(group => new DigitalMockupInterferenceGroup(
                ToInterferenceKind(group.Key.FirstSupportKind, group.Key.SecondSupportKind),
                group.Key.FirstSupportKind, group.Key.SecondSupportKind,
                Array.AsReadOnly(group.Select(item => item.index).ToArray())))
            .ToArray();
    }

    private static DigitalMockupInterferenceKind ToInterferenceKind(InspectionSupportKind first, InspectionSupportKind second) =>
        (first, second) switch
        {
            (InspectionSupportKind.Face, InspectionSupportKind.Face) => DigitalMockupInterferenceKind.FaceFace,
            (InspectionSupportKind.Edge, InspectionSupportKind.Edge) => DigitalMockupInterferenceKind.EdgeEdge,
            (InspectionSupportKind.Face, InspectionSupportKind.Edge) or (InspectionSupportKind.Edge, InspectionSupportKind.Face) => DigitalMockupInterferenceKind.FaceEdge,
            (InspectionSupportKind.Face, InspectionSupportKind.Vertex) or (InspectionSupportKind.Vertex, InspectionSupportKind.Face) => DigitalMockupInterferenceKind.FaceVertex,
            (InspectionSupportKind.Edge, InspectionSupportKind.Vertex) or (InspectionSupportKind.Vertex, InspectionSupportKind.Edge) => DigitalMockupInterferenceKind.EdgeVertex,
            _ => DigitalMockupInterferenceKind.VertexVertex
        };

    private static DigitalMockupSelfCheckResult SelfCheck(
        DigitalMockupItem item,
        DigitalMockupItemSnapshot snapshot,
        DigitalMockupPolicy policy,
        List<DigitalMockupDiagnostic> diagnostics)
    {
        if (!snapshot.IsUsable)
            return new(item.Id, false, 1, [], "The input shape is unavailable.");
        try
        {
            ShapeValidationReport validation = item.Shape.GetValidationReport(geometryChecks: true, exact: true);
            using FeatureOperationResult preflight = FeatureModeling.Preflight(item.Shape);
            ShapeValidationIssue[] issues = validation.Issues.Take(policy.MaxSelfDiagnosticsPerItem).ToArray();
            bool valid = validation.IsValid && preflight.Diagnostics.ResultIsValid;
            return new(item.Id, valid, preflight.Diagnostics.FaultyShapeCount,
                Array.AsReadOnly(issues), preflight.Diagnostics.StageMessage);
        }
        catch (Exception exception) when (exception is OcctException or InvalidOperationException or ObjectDisposedException)
        {
            diagnostics.Add(new(DigitalMockupStage.SelfCheck, DigitalMockupDiagnosticSeverity.Error,
                "DMU201", exception.Message, item.Id));
            return new(item.Id, false, 1, [], exception.Message);
        }
    }

    private static DigitalMockupFilterReason GetFilter(
        DigitalMockupItem first,
        DigitalMockupItem second,
        DigitalMockupItemSnapshot firstSnapshot,
        DigitalMockupItemSnapshot secondSnapshot,
        DigitalMockupPairId pair,
        HashSet<DigitalMockupPairId> explicitExclusions,
        DigitalMockupPolicy policy)
    {
        if (!firstSnapshot.IsUsable || !secondSnapshot.IsUsable) return DigitalMockupFilterReason.UnusableInput;
        if (explicitExclusions.Contains(pair)) return DigitalMockupFilterReason.ExplicitPair;
        if (policy.ExcludeSameDefinition && first.DefinitionId is not null
            && string.Equals(first.DefinitionId, second.DefinitionId, StringComparison.Ordinal))
            return DigitalMockupFilterReason.SameDefinition;
        if (policy.ExcludeAdjacent && (first.AdjacentIds.Contains(second.Id) || second.AdjacentIds.Contains(first.Id)))
            return DigitalMockupFilterReason.Adjacent;
        return DigitalMockupFilterReason.None;
    }

    private static double BoundsDistance(BoundingBox3d first, BoundingBox3d second)
    {
        double dx = Math.Max(0, Math.Max(first.Minimum.X - second.Maximum.X, second.Minimum.X - first.Maximum.X));
        double dy = Math.Max(0, Math.Max(first.Minimum.Y - second.Maximum.Y, second.Minimum.Y - first.Maximum.Y));
        double dz = Math.Max(0, Math.Max(first.Minimum.Z - second.Maximum.Z, second.Minimum.Z - first.Maximum.Z));
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static unsafe (HashSet<DigitalMockupPairId> Pairs, int AxisComparisons) GetCandidates(
        DigitalMockupItem[] items,
        List<int> usableIndices,
        double expansion)
    {
        if (usableIndices.Count < 2) return ([], 0);
        nint[] handles = new nint[usableIndices.Count];
        bool[] references = new bool[usableIndices.Count];
        int acquired = 0;
        try
        {
            for (; acquired < usableIndices.Count; ++acquired)
            {
                Shape shape = items[usableIndices[acquired]].Shape;
                shape.Handle.DangerousAddRef(ref references[acquired]);
                handles[acquired] = shape.Handle.DangerousGetHandle();
            }
            int count;
            int comparisons;
            fixed (nint* shapePointer = handles)
            {
                NativeError.ThrowIfFailed(NativeMethods.GetDigitalMockupCandidatePairs(
                    shapePointer, handles.Length, expansion, null, 0, out count, out comparisons),
                    "digital_mockup_candidate_pair_count");
                int[] nativePairs = new int[checked(count * 2)];
                fixed (int* pairPointer = nativePairs)
                    NativeError.ThrowIfFailed(NativeMethods.GetDigitalMockupCandidatePairs(
                        shapePointer, handles.Length, expansion, pairPointer, count,
                        out int written, out int repeatedComparisons), "digital_mockup_candidate_pairs");
                HashSet<DigitalMockupPairId> result = [];
                for (int index = 0; index < count; ++index)
                    result.Add(new(items[usableIndices[nativePairs[index * 2]]].Id,
                        items[usableIndices[nativePairs[index * 2 + 1]]].Id));
                return (result, comparisons);
            }
        }
        finally
        {
            for (int index = acquired - 1; index >= 0; --index)
                if (references[index]) items[usableIndices[index]].Shape.Handle.DangerousRelease();
        }
    }

    private static bool PolicyEquivalent(DigitalMockupPolicy first, DigitalMockupPolicy second) =>
        first with { ExcludedPairs = Array.Empty<DigitalMockupPairId>() }
            == second with { ExcludedPairs = Array.Empty<DigitalMockupPairId>() }
        && first.ExcludedPairs.ToHashSet().SetEquals(second.ExcludedPairs);

    private sealed record PairWork(
        DigitalMockupPairId Id,
        DigitalMockupItem First,
        DigitalMockupItem Second,
        DigitalMockupFilterReason Filter,
        bool IsCandidate,
        double LowerBound);
}
