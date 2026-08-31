using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchLCompletionTests
{
    [Fact]
    public void InputsBoundsBroadPhaseFiltersDistanceWitnessAndClearanceAreOneClosure()
    {
        using Shape slender = ShapeFactory.CreateBox(20, 2, 1);
        using Shape rotated = slender.Transformed(ShapeTransform.CreateTranslationAndRotationZ(0, 0, 0, 45));
        OrientedBoundingBox3d oriented = rotated.GetOrientedBoundingBox();
        double[] sizes = [oriented.SizeX, oriented.SizeY, oriented.SizeZ];
        Array.Sort(sizes);
        Assert.Equal(1, sizes[0], 5);
        Assert.Equal(2, sizes[1], 5);
        Assert.Equal(20, sizes[2], 5);
        Assert.True(rotated.GetBoundingBox().SizeX > 15);
        Assert.InRange(oriented.XDirection.Modulus, 0.999999, 1.000001);

        using Shape first = ShapeFactory.CreateBox(2, 2, 2);
        using Shape secondBase = ShapeFactory.CreateBox(2, 2, 2);
        using Shape second = secondBase.Transformed(ShapeTransform.CreateTranslation(3, 0, 0));
        using Shape farBase = ShapeFactory.CreateBox(1, 1, 1);
        using Shape far = farBase.Transformed(ShapeTransform.CreateTranslation(100, 0, 0));
        using Shape sameDefinition = ShapeFactory.CreateSphere(1);
        using Shape adjacent = ShapeFactory.CreateCylinder(1, 2);
        using Shape adjacentFirst = ShapeFactory.CreateBox(1, 2, 3);
        using Shape adjacentSecond = ShapeFactory.CreateSphere(2);

        DigitalMockupItem[] items =
        [
            new("A", first, "box"),
            new("B", second, "box-copy"),
            new("C", far, "far"),
            new("D", sameDefinition, "shared", adjacentIds: ["E"]),
            new("E", adjacent, "shared"),
            new("F", adjacentFirst, "adjacent-first", adjacentIds: ["G"]),
            new("G", adjacentSecond, "adjacent-second")
        ];
        DigitalMockupPolicy policy = new()
        {
            Clearance = 1,
            ExcludeSameDefinition = true,
            ExcludeAdjacent = true,
            ExcludedPairs = [new("A", "C")]
        };
        using DigitalMockupReport report = DigitalMockupAnalyzer.Analyze(items.Reverse().ToArray(), policy);
        Assert.Equal(7, report.Summary.InputCount);
        Assert.Equal(21, report.Summary.RequestedPairCount);
        Assert.True(report.Summary.CandidatePairCount < report.Summary.RequestedPairCount);
        Assert.True(report.Summary.ExactPairCount < report.Summary.RequestedPairCount);
        DigitalMockupPairResult measured = report.PairById[new("A", "B")];
        Assert.Equal(DigitalMockupPairState.ClearanceViolation, measured.State);
        Assert.True(measured.IsExact);
        Assert.Equal(1, measured.MinimumDistance!.Value, 6);
        Assert.NotEmpty(measured.Witnesses);
        Assert.All(measured.Witnesses, witness => Assert.Equal(1, witness.Distance, 6));
        Assert.Equal(DigitalMockupFilterReason.ExplicitPair, report.PairById[new("A", "C")].FilterReason);
        Assert.Equal(DigitalMockupFilterReason.SameDefinition, report.PairById[new("D", "E")].FilterReason);
        Assert.Equal(DigitalMockupFilterReason.Adjacent, report.PairById[new("F", "G")].FilterReason);

        using DigitalMockupReport above = DigitalMockupAnalyzer.Analyze(
            items.Take(2).ToArray(), policy with { Clearance = 0.5, ExactDistanceForAllPairs = true });
        Assert.Equal(DigitalMockupPairState.Clear, Assert.Single(above.Pairs).State);
        Assert.Throws<ArgumentOutOfRangeException>(() => DigitalMockupAnalyzer.Analyze(
            items.Take(2).ToArray(), policy with { FuzzyTolerance = double.NaN }));
    }

    [Fact]
    public void ContactPenetrationContainmentSelfCheckToleranceAndRobustOptionsAreDeterministic()
    {
        using Shape outer = ShapeFactory.CreateBox(10, 10, 10);
        using Shape innerBase = ShapeFactory.CreateBox(2, 2, 2);
        using Shape inner = innerBase.Transformed(ShapeTransform.CreateTranslation(2, 2, 2));
        using Shape coincident = ShapeFactory.CreateBox(10, 10, 10);
        using Shape touchingBase = ShapeFactory.CreateBox(2, 2, 2);
        using Shape touching = touchingBase.Transformed(ShapeTransform.CreateTranslation(10, 0, 0));
        using Shape overlapBase = ShapeFactory.CreateBox(2, 2, 2);
        using Shape overlap = overlapBase.Transformed(ShapeTransform.CreateTranslation(9, 0, 0));
        Shape[] edges = outer.GetSubShapes(ShapeKind.Edge);
        using Shape broken = outer.RemoveSubshape(edges[0]);
        DisposeAll(edges);

        DigitalMockupItem[] items =
        [
            new("outer", outer), new("inner", inner), new("coincident", coincident),
            new("touching", touching), new("overlap", overlap), new("broken", broken)
        ];
        DigitalMockupPolicy serialPolicy = new()
        {
            ExactDistanceForAllPairs = true,
            ConfusionTolerance = 1e-7,
            FuzzyTolerance = 1e-7,
            AngularToleranceRadians = 1e-8,
            NonDestructive = true
        };
        using DigitalMockupReport serial = DigitalMockupAnalyzer.Analyze(items, serialPolicy);
        using DigitalMockupReport parallel = DigitalMockupAnalyzer.Analyze(items, serialPolicy with { RunParallel = true });
        Assert.Equal(DigitalMockupPairState.FirstInsideSecond, serial.PairById[new("inner", "outer")].State);
        Assert.Equal(DigitalMockupPairState.Coincident, serial.PairById[new("coincident", "outer")].State);
        Assert.Equal(DigitalMockupPairState.Touching, serial.PairById[new("outer", "touching")].State);
        DigitalMockupPairResult penetration = serial.PairById[new("outer", "overlap")];
        Assert.Equal(DigitalMockupPairState.Interfering, penetration.State);
        Assert.True(penetration.OverlapVolume > 0);
        Assert.NotNull(penetration.IssueTopology);
        Assert.NotEmpty(penetration.InterferenceGroups);

        using Shape firstFaceWire = ShapeFactory.CreatePolygonWire(
            [new(0, 0, 0), new(10, 0, 0), new(10, 10, 0), new(0, 10, 0)], close: true);
        using Shape firstFace = ShapeFactory.CreatePlanarFace(firstFaceWire);
        using Shape firstFaceSupport = ShapeFactory.CreateSphere(1);
        using Shape secondFaceSupportBase = ShapeFactory.CreateSphere(1);
        using Shape secondFaceSupport = secondFaceSupportBase.Transformed(ShapeTransform.CreateTranslation(5, 4, 3));
        using Shape firstEdge = ShapeFactory.CreateEdge(new(0, 2, 3), new(10, 2, 3));
        using Shape secondEdge = ShapeFactory.CreateEdge(new(5, 0, 4), new(5, 10, 4));
        using Shape mixedEdge = ShapeFactory.CreateEdge(new(-1, 8, 1), new(7, -4, 1));
        using DigitalMockupReport contactGroups = DigitalMockupAnalyzer.Analyze(
            [new("edge-a", firstEdge), new("edge-b", secondEdge), new("face-a", firstFace),
                new("mixed", mixedEdge), new("sphere-a", firstFaceSupport), new("sphere-b", secondFaceSupport)],
            serialPolicy);
        AssertGroup(contactGroups.PairById[new("sphere-a", "sphere-b")], DigitalMockupInterferenceKind.FaceFace);
        AssertGroup(contactGroups.PairById[new("edge-a", "edge-b")], DigitalMockupInterferenceKind.EdgeEdge);
        AssertGroup(contactGroups.PairById[new("mixed", "sphere-a")], DigitalMockupInterferenceKind.FaceEdge);
        Assert.Contains(serial.SelfChecks, item => item.ItemId == "broken" && item.FaultyShapeCount > 0);
        Assert.Equal(
            serial.Pairs.Select(pair => (pair.Id, pair.State)).ToArray(),
            parallel.Pairs.Select(pair => (pair.Id, pair.State)).ToArray());

        using DigitalMockupReport early = DigitalMockupAnalyzer.Analyze(items, serialPolicy with { EarlyExit = true });
        Assert.Contains(early.Pairs, pair => pair.State == DigitalMockupPairState.SkippedAfterEarlyExit);
    }

    [Fact]
    public void PairMatrixAggregationOwnershipIncrementalAndStepXdeTraceabilityRemainComplete()
    {
        using Shape first = ShapeFactory.CreateBox(3, 3, 3);
        using Shape movedBase = ShapeFactory.CreateBox(3, 3, 3);
        using Shape moved = movedBase.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
        using Shape farBase = ShapeFactory.CreateBox(2, 2, 2);
        using Shape far = farBase.Transformed(ShapeTransform.CreateTranslation(40, 0, 0));
        DigitalMockupItem[] initial = [new("A", first, "first"), new("B", moved, "second"), new("C", far, "far")];
        DigitalMockupPolicy policy = new() { Clearance = 3, ExactDistanceForAllPairs = true };
        using DigitalMockupReport baseline = DigitalMockupAnalyzer.Analyze(initial, policy);
        Assert.Equal(3, baseline.Pairs.Count);
        Assert.Equal(3, baseline.PairById.Count);
        Assert.Contains(baseline.Aggregation.ByOccurrence, group => group.Key == "A");
        Assert.Contains(baseline.Diagnostics, item => item.Stage == DigitalMockupStage.Aggregation);

        using Shape relocated = movedBase.Transformed(ShapeTransform.CreateTranslation(2, 0, 0));
        DigitalMockupItem[] changed = [new("A", first, "first"), new("B", relocated, "second"), new("C", far, "far")];
        using DigitalMockupReport incremental = DigitalMockupAnalyzer.AnalyzeIncremental(baseline, changed, ["B"]);
        Assert.Equal(1, incremental.Summary.ReusedPairCount);
        Assert.Equal(baseline.PairById[new("A", "C")].Id, incremental.PairById[new("A", "C")].Id);
        Assert.Equal(DigitalMockupPairState.Interfering, incremental.PairById[new("A", "B")].State);
        first.Dispose(); moved.Dispose(); far.Dispose(); relocated.Dispose();
        Assert.True(incremental.PairById[new("A", "B")].IssueTopology!.IsValid);

        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchL.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string step = Path.Combine(directory, "batch-l.step");
            using Shape partShape = ShapeFactory.CreateBox(4, 4, 4);
            using (XdeDocument source = XdeDocument.Create())
            {
                using XdeTransaction transaction = source.BeginTransaction("Batch L XDE assembly");
                XdeLabel part = source.AddShape(partShape, "Repeated Part");
                XdeLabel root = source.AddAssembly("DMU Root");
                using TopLocLocation firstLocation = Location(0, 0, 0);
                using TopLocLocation secondLocation = Location(3, 0, 0);
                source.AddComponent(root, part, firstLocation);
                source.AddComponent(root, part, secondLocation);
                Assert.True(transaction.Commit());
                using DigitalMockupReport xde = DigitalMockupAnalyzer.AnalyzeAssembly(root,
                    new DigitalMockupPolicy { ExactDistanceForAllPairs = true });
                Assert.Equal(2, xde.Items.Count);
                Assert.All(xde.Items, item => Assert.Equal(part.Entry, item.DefinitionId));
                Assert.Equal(DigitalMockupPairState.Interfering, Assert.Single(xde.Pairs).State);
                source.WriteStep(step);
            }
            using XdeDocument imported = XdeDocument.ReadStep(step);
            using DigitalMockupReport reread = DigitalMockupAnalyzer.AnalyzeAssembly(Assert.Single(imported.GetFreeShapes()),
                new DigitalMockupPolicy { ExactDistanceForAllPairs = true });
            Assert.Equal(2, reread.Items.Count);
            Assert.All(reread.Items, item => Assert.NotEmpty(item.OccurrencePath));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void RealHwndReviewColorsIsolatesEnablesSelectionAndSurvivesSourceDisposal()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchL.Viewer.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            DigitalMockupReport report;
            using (Shape first = ShapeFactory.CreateBox(10, 10, 10))
            using (Shape secondBase = ShapeFactory.CreateBox(4, 4, 4))
            using (Shape second = secondBase.Transformed(ShapeTransform.CreateTranslation(8, 0, 0)))
                report = DigitalMockupAnalyzer.Analyze(
                    [new("first", first), new("second", second)],
                    new DigitalMockupPolicy { ExactDistanceForAllPairs = true });
            using (report)
            using (OcctViewer viewer = OcctViewer.Create(window))
            using (DigitalMockupReviewSession review = DigitalMockupReviewSession.Display(report, viewer))
            {
                DigitalMockupPairId issue = Assert.Single(review.IssueIds);
                Assert.NotEmpty(review.GetPresentations(issue));
                review.EnableSelection(issue, ShapeKind.Face);
                review.Isolate([issue]);
                string screenshot = review.SaveKeyedScreenshot(Path.Combine(directory, "batch-l.png"), [issue]);
                Assert.True(new FileInfo(screenshot).Length > 0);
                review.ShowAll();
            }
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void DisposeAll(IEnumerable<Shape> shapes)
    {
        foreach (Shape shape in shapes) shape.Dispose();
    }

    private static void AssertGroup(DigitalMockupPairResult pair, DigitalMockupInterferenceKind expected)
    {
        Assert.Contains(pair.InterferenceGroups, group => group.Kind == expected);
    }

    private static TopLocLocation Location(double x, double y, double z)
    {
        using GpTrsf transform = GpTrsf.Create(x, y, z);
        return TopLocLocation.FromTransform(transform);
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch L", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
