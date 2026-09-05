using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchJCompletionTests
{
    [Fact]
    public void ContainerHistoryUsesSupportedDescendantsWithoutNativeAssertions()
    {
        using Shape left = ShapeFactory.CreateBox(10, 10, 10);
        using Shape rightBase = ShapeFactory.CreateBox(10, 10, 10);
        using Shape right = rightBase.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
        using Shape compound = ShapeFactory.CreateCompound([left]);
        using FeatureOperationResult result = FeatureModeling.Boolean(
            FeatureBooleanOperation.Fuse, [compound], [right], Robust);
        Assert.True(result.Diagnostics.Succeeded, result.Diagnostics.StageMessage);
        Assert.True(result.RequireShape().IsValid);
        Assert.NotEmpty(result.History);

        using BooleanOperationResult wireHistory = left.FuseWithHistory(right, ShapeKind.Wire);
        Assert.True(wireHistory.Shape.IsValid);
        Assert.True(wireHistory.History.Left.SourceCount > 0);
        Assert.Equal(0, wireHistory.History.Left.ModifiedResultCount);

        using Shape plane = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(5, -1, -1), new(5, -1, 11), new(5, 11, -1), new(5, 11, 11)]));
        using FreeformShapeResult split = FreeformAuthoring.SplitTopology([compound], [plane]);
        Assert.True(split.Shape.IsValid);
    }

    private static readonly FeatureModelingOptions Robust = new()
    {
        FuzzyTolerance = 1e-7,
        NonDestructive = true,
        RepairInputs = true,
        UnifyResult = true
    };

    [Fact]
    public void SelectedFilletChamferPlanarAndDraftOperationsReturnOwningHistory()
    {
        using Shape box = ShapeFactory.CreateBox(20, 16, 12);
        Shape[] edges = box.GetSubShapes(ShapeKind.Edge);
        Shape[] faces = box.GetSubShapes(ShapeKind.Face);
        try
        {
            using FeatureOperationResult fillet = FeatureModeling.Fillet(box, [edges[0]], 1.0, Robust);
            Assert.True(fillet.Diagnostics.Succeeded, fillet.Diagnostics.StageMessage);
            Assert.True(fillet.RequireShape().IsValid);

            using FeatureOperationResult multipleFillet = FeatureModeling.Fillet(box, [edges[0], edges[4]], 0.5, Robust);
            Assert.True(multipleFillet.Diagnostics.Succeeded, multipleFillet.Diagnostics.StageMessage);

            using FeatureOperationResult variable = FeatureModeling.VariableFillet(box, [edges[1]], 0.5, 1.25, Robust);
            Assert.True(variable.Diagnostics.Succeeded, variable.Diagnostics.StageMessage);
            Assert.NotEmpty(variable.History);
            Assert.Throws<ArgumentException>(() => FeatureModeling.VariableFillet(box, [edges[1]], 0, 1.0, Robust));

            using FeatureOperationResult oversized = FeatureModeling.Fillet(box, [edges[0]], 1000, Robust);
            Assert.False(oversized.Diagnostics.Succeeded);
            Assert.True(oversized.Diagnostics.ErrorCount > 0);
            Assert.False(string.IsNullOrWhiteSpace(oversized.Diagnostics.StageMessage));

            using FeatureOperationResult chamfer = FeatureModeling.Chamfer(box, [edges[2]], 0.75, Robust);
            Assert.True(chamfer.Diagnostics.Succeeded, chamfer.Diagnostics.StageMessage);

            ChamferSelection? adjacent = FindAdjacent(box, edges, faces);
            Assert.NotNull(adjacent);
            using FeatureOperationResult asymmetric = FeatureModeling.Chamfer(box, [adjacent!], 0.5, 1.0, Robust);
            Assert.True(asymmetric.Diagnostics.Succeeded, asymmetric.Diagnostics.StageMessage);

            ChamferSelection? nonAdjacent = FindNonAdjacent(box, edges, faces);
            Assert.NotNull(nonAdjacent);
            Assert.Throws<ArgumentException>(() => FeatureModeling.Chamfer(box, [nonAdjacent!], 0.5, 1.0, Robust));

            using Shape wire = ShapeFactory.CreatePolygonWire(
                [new(0, 0, 0), new(12, 0, 0), new(12, 10, 0), new(0, 10, 0)], close: true);
            using Shape planarFace = ShapeFactory.CreatePlanarFace(wire);
            Shape[] vertices = planarFace.GetSubShapes(ShapeKind.Vertex);
            Shape[] planarEdges = planarFace.GetSubShapes(ShapeKind.Edge);
            try
            {
                using FeatureOperationResult planarFillet = FeatureModeling.FilletPlanarFace(planarFace, [vertices[0]], 1.0);
                Assert.True(planarFillet.Diagnostics.Succeeded, planarFillet.Diagnostics.StageMessage);
                using FeatureOperationResult planarChamfer = FeatureModeling.ChamferPlanarFace(
                    planarFace, [new PlanarChamferSelection(planarEdges[0], planarEdges[1])], 0.6, 0.8);
                Assert.True(planarChamfer.Diagnostics.Succeeded, planarChamfer.Diagnostics.StageMessage);
            }
            finally { DisposeAll(vertices); DisposeAll(planarEdges); }

            FeatureOperationResult? draft = null;
            foreach (Shape face in faces)
            {
                draft?.Dispose();
                draft = FeatureModeling.Draft(box, [face], new GpXyz(0, 0, 1), 0.03,
                    GpPlane.Create(GpXyz.Origin, new GpXyz(0, 0, 1)), Robust);
                if (draft.Diagnostics.Succeeded) break;
            }
            using (draft) Assert.True(draft!.Diagnostics.Succeeded, draft.Diagnostics.StageMessage);

            FeatureOperationResult? negativeDraft = null;
            foreach (Shape face in faces)
            {
                negativeDraft?.Dispose();
                negativeDraft = FeatureModeling.Draft(box, [face], new GpXyz(0, 0, 1), -0.03,
                    GpPlane.Create(GpXyz.Origin, new GpXyz(0, 0, 1)), Robust);
                if (negativeDraft.Diagnostics.Succeeded) break;
            }
            using (negativeDraft)
                Assert.True(negativeDraft!.Diagnostics.Succeeded, negativeDraft.Diagnostics.StageMessage);

            box.Dispose();
            Assert.True(fillet.RequireShape().IsValid);
            Assert.All(fillet.History, item => Assert.True(item.Shape.IsValid));
        }
        finally { DisposeAll(edges); DisposeAll(faces); }
    }

    [Fact]
    public void BossPocketHoleRevolveAndPipeFeaturesComposeWithRobustOptions()
    {
        using Shape baseSolid = ShapeFactory.CreateBox(30, 24, 12);
        using Shape bossWire = ShapeFactory.CreatePolygonWire(
            [new(4, 4, 12), new(10, 4, 12), new(10, 10, 12), new(4, 10, 12)], close: true);
        using Shape bossProfile = ShapeFactory.CreatePlanarFace(bossWire);
        using FeatureOperationResult boss = FeatureModeling.AddBoss(baseSolid, bossProfile, new GpXyz(0, 0, 6), Robust);
        Assert.True(boss.Diagnostics.Succeeded, boss.Diagnostics.StageMessage);

        using Shape pocketWire = ShapeFactory.CreatePolygonWire(
            [new(16, 6, 12), new(22, 6, 12), new(22, 12, 12), new(16, 12, 12)], close: true);
        using Shape pocketProfile = ShapeFactory.CreatePlanarFace(pocketWire);
        using FeatureOperationResult pocket = FeatureModeling.CutPocket(
            boss.RequireShape(), pocketProfile, new GpXyz(0, 0, -8), Robust);
        Assert.True(pocket.Diagnostics.Succeeded, pocket.Diagnostics.StageMessage);

        using FeatureOperationResult blindHole = FeatureModeling.CutHole(
            pocket.RequireShape(), new GpXyz(7, 18, 14), new GpXyz(0, 0, -1), 1.25, 4,
            throughAll: false, Robust);
        Assert.True(blindHole.Diagnostics.Succeeded, blindHole.Diagnostics.StageMessage);

        using FeatureOperationResult hole = FeatureModeling.CutHole(
            pocket.RequireShape(), new GpXyz(7, 18, 20), new GpXyz(0, 0, -1), 2.0, 30,
            throughAll: true, Robust);
        Assert.True(hole.Diagnostics.Succeeded, hole.Diagnostics.StageMessage);

        using Shape revolveWire = ShapeFactory.CreatePolygonWire(
            [new(15, 0, 2), new(18, 0, 2), new(18, 0, 5), new(15, 0, 5)], close: true);
        using Shape revolveProfile = ShapeFactory.CreatePlanarFace(revolveWire);
        using FeatureOperationResult addedRevolve = FeatureModeling.AddRevolvedFeature(
            baseSolid, revolveProfile, GpXyz.Origin, new GpXyz(0, 0, 1), Math.PI * 2, Robust);
        Assert.True(addedRevolve.Diagnostics.Succeeded, addedRevolve.Diagnostics.StageMessage);
        using FeatureOperationResult revolved = FeatureModeling.CutRevolvedFeature(
            hole.RequireShape(), revolveProfile, GpXyz.Origin, new GpXyz(0, 0, 1), Math.PI * 2, Robust);
        Assert.True(revolved.Diagnostics.Succeeded, revolved.Diagnostics.StageMessage);

        using Shape spineEdge = ShapeFactory.CreateEdge(new(2, 2, 12), new(2, 2, 18));
        using Shape spine = ShapeFactory.CreateWire([spineEdge]);
        using Shape pipeWire = ShapeFactory.CreatePolygonWire(
            [new(1, 1, 12), new(3, 1, 12), new(3, 3, 12), new(1, 3, 12)], close: true);
        using Shape pipeProfile = ShapeFactory.CreatePlanarFace(pipeWire);
        using FeatureOperationResult pipe = FeatureModeling.AddPipeFeature(
            revolved.RequireShape(), spine, pipeProfile, Robust);
        Assert.True(pipe.Diagnostics.Succeeded, pipe.Diagnostics.StageMessage);
        Assert.True(pipe.RequireShape().IsValid);

        using Shape cutSpineEdge = ShapeFactory.CreateEdge(new(8, 8, 12), new(8, 8, 0));
        using Shape cutSpine = ShapeFactory.CreateWire([cutSpineEdge]);
        using Shape cutPipeWire = ShapeFactory.CreatePolygonWire(
            [new(7, 7, 12), new(9, 7, 12), new(9, 9, 12), new(7, 9, 12)], close: true);
        using Shape cutPipeProfile = ShapeFactory.CreatePlanarFace(cutPipeWire);
        using FeatureOperationResult cutPipe = FeatureModeling.CutPipeFeature(
            revolved.RequireShape(), cutSpine, cutPipeProfile, Robust);
        Assert.True(cutPipe.Diagnostics.Succeeded, cutPipe.Diagnostics.StageMessage);
        Assert.True(cutPipe.RequireShape().IsValid);
    }

    [Fact]
    public void SplitDefeatureCellsBatchBooleanPreflightDiagnosticsAndRecoveryAreOneClosure()
    {
        using Shape left = ShapeFactory.CreateBox(12, 12, 12);
        using Shape rightBase = ShapeFactory.CreateBox(12, 12, 12);
        using Shape right = rightBase.Transformed(ShapeTransform.CreateTranslation(6, 0, 0));
        using Shape upperBase = ShapeFactory.CreateBox(12, 12, 12);
        using Shape upper = upperBase.Transformed(ShapeTransform.CreateTranslation(0, 6, 0));

        using FeatureOperationResult preflight = FeatureModeling.Preflight(left, right, FeatureBooleanOperation.Fuse);
        Assert.True(preflight.Diagnostics.Succeeded);
        Assert.True(preflight.Diagnostics.FaultyShapeCount >= 0);

        Shape[] breakEdges = left.GetSubShapes(ShapeKind.Edge);
        try
        {
            using Shape broken = left.RemoveSubshape(breakEdges[0]);
            using FeatureOperationResult badPreflight = FeatureModeling.Preflight(broken);
            Assert.True(badPreflight.Diagnostics.FaultyShapeCount > 0);
            using FeatureOperationResult recovered = FeatureModeling.Boolean(
                FeatureBooleanOperation.Fuse, [broken], [right], Robust);
            Assert.True(recovered.Diagnostics.Recovered);
            Assert.True(recovered.Diagnostics.Succeeded, recovered.Diagnostics.StageMessage);
        }
        finally { DisposeAll(breakEdges); }

        using FeatureOperationResult fused = FeatureModeling.Boolean(
            FeatureBooleanOperation.Fuse, [left], [right, upper], Robust with { RunParallel = true });
        Assert.True(fused.Diagnostics.Succeeded, fused.Diagnostics.StageMessage);
        Assert.NotEmpty(fused.History);

        using FeatureOperationResult common = FeatureModeling.Boolean(
            FeatureBooleanOperation.Common, [left], [right], Robust);
        Assert.True(common.Diagnostics.Succeeded, common.Diagnostics.StageMessage);
        using FeatureOperationResult cut = FeatureModeling.Boolean(
            FeatureBooleanOperation.Cut, [left], [right], Robust);
        Assert.True(cut.Diagnostics.Succeeded, cut.Diagnostics.StageMessage);
        using FeatureOperationResult section = FeatureModeling.Boolean(
            FeatureBooleanOperation.Section, [left], [right], Robust);
        Assert.True(section.Diagnostics.Succeeded, section.Diagnostics.StageMessage);

        using Shape splitPlane = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(6, -2, -2), new(6, -2, 14), new(6, 14, -2), new(6, 14, 14)]));
        using Shape splitPlane2 = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(-2, 6, -2), new(-2, 6, 14), new(14, 6, -2), new(14, 6, 14)]));
        using FeatureOperationResult split = FeatureModeling.Split([left, right], [splitPlane, splitPlane2], Robust);
        Assert.True(split.Diagnostics.Succeeded, split.Diagnostics.StageMessage);

        using FeatureOperationResult cells = FeatureModeling.SelectBooleanCells([left, right], [left], [right], 1, Robust);
        Assert.True(cells.Diagnostics.Succeeded, cells.Diagnostics.StageMessage);

        using FeatureOperationResult holed = FeatureModeling.CutHole(
            left, new GpXyz(6, 6, 20), new GpXyz(0, 0, -1), 2, 30, true, Robust);
        Shape[] holeFaces = holed.RequireShape().GetSubShapes(ShapeKind.Face);
        try
        {
            Shape? cylindrical = holeFaces.FirstOrDefault(face => face.GetFaceSurfaceSnapshot().SurfaceType == SurfaceGeometryType.Cylinder);
            Assert.NotNull(cylindrical);
            using FeatureOperationResult defeatured = FeatureModeling.Defeature(holed.RequireShape(), [cylindrical!], Robust);
            Assert.True(defeatured.Diagnostics.Succeeded, defeatured.Diagnostics.StageMessage);
            Assert.True(defeatured.RequireShape().IsValid);
            Assert.Contains(1, defeatured.DeletedSourceIndices);
        }
        finally { DisposeAll(holeFaces); }

        Assert.Contains(fused.History, item => item.Kind is FeatureHistoryKind.Modified or FeatureHistoryKind.Generated);
        left.Dispose(); right.Dispose();
        Assert.True(fused.RequireShape().IsValid);
    }

    [Fact]
    public void FeatureHistorySurvivesStepXdeViewerAndCleanStyleLifetimeWorkflow()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchJ.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory); nint window = CreateTestWindow();
        try
        {
            using Shape source = ShapeFactory.CreateBox(20, 15, 10);
            Shape[] edges = source.GetSubShapes(ShapeKind.Edge);
            FeatureOperationResult feature;
            try { feature = FeatureModeling.VariableFillet(source, [edges[0]], 0.5, 1.0, Robust); }
            finally { DisposeAll(edges); }
            using (feature)
            {
                Assert.True(feature.Diagnostics.Succeeded, feature.Diagnostics.StageMessage);
                string step = Path.Combine(directory, "batch-j.step");
                using (XdeDocument document = XdeDocument.Create())
                {
                    using XdeTransaction transaction = document.BeginTransaction("Batch J feature");
                    XdeLabel label = document.AddShape(feature.RequireShape(), "Batch J Feature Result");
                    label.Color = new XdeColor(0.25, 0.65, 0.9, 1); Assert.True(transaction.Commit());
                    document.WriteStep(step);
                }
                using XdeDocument imported = XdeDocument.ReadStep(step);
                using Shape importedShape = Assert.Single(imported.GetFreeShapes()).Shape;
                Assert.True(importedShape.IsValid);
                using OcctViewer viewer = OcctViewer.Create(window);
                using ViewerPresentation resultPresentation = viewer.Display(importedShape);
                foreach (FeatureHistoryItem item in feature.Generated.Take(2))
                    using (viewer.Display(item.Shape)) { }
                viewer.FitAll(); viewer.Redraw();
                string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-j.png"));
                Assert.True(new FileInfo(screenshot).Length > 0);
            }
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static ChamferSelection? FindAdjacent(Shape source, Shape[] edges, Shape[] faces)
    {
        using TopologyAdjacencyMap map = source.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);
        for (int edgeIndex = 0; edgeIndex < map.Items.Count; ++edgeIndex)
        {
            ReadOnlySpan<int> ancestors = map.GetAncestorIndices(edgeIndex).Span;
            if (!ancestors.IsEmpty) return new ChamferSelection(edges[edgeIndex], faces[ancestors[0]]);
        }
        return null;
    }

    private static ChamferSelection? FindNonAdjacent(Shape source, Shape[] edges, Shape[] faces)
    {
        using TopologyAdjacencyMap map = source.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);
        for (int edgeIndex = 0; edgeIndex < map.Items.Count; ++edgeIndex)
        {
            ReadOnlySpan<int> ancestors = map.GetAncestorIndices(edgeIndex).Span;
            for (int faceIndex = 0; faceIndex < faces.Length; ++faceIndex)
                if (!ancestors.Contains(faceIndex))
                    return new ChamferSelection(edges[edgeIndex], faces[faceIndex]);
        }
        return null;
    }

    private static void DisposeAll(IEnumerable<Shape> shapes) { foreach (Shape shape in shapes) shape.Dispose(); }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch J", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window); _ = NativeWindowMethods.ShowWindow(window, 4); _ = NativeWindowMethods.UpdateWindow(window); return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool DestroyWindow(nint window);
    }
}
