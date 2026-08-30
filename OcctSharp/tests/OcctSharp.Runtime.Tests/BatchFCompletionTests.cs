using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchFCompletionTests
{
    [Fact]
    public void CurveDefinitionsInterpolationEditingAndMultiSolutionsAreOneCopiedClosure()
    {
        GpPoint[] sourcePoles = [new(0, 0, 0), new(3, 6, 0), new(7, -4, 0), new(10, 0, 0)];
        double[] sourceWeights = [1, 0.65, 0.8, 1];
        FreeformCurveDefinition rationalBezier = FreeformCurveDefinition.Bezier(sourcePoles, sourceWeights);
        sourcePoles[1] = new GpPoint(999, 999, 999); sourceWeights[1] = 99;
        using Shape bezier = FreeformAuthoring.CreateCurve(rationalBezier);
        FreeformCurveDefinition bezierSnapshot = FreeformAuthoring.GetCurveDefinition(bezier);
        Assert.Equal(FreeformGeometryKind.Bezier, bezierSnapshot.Kind);
        Assert.True(bezierSnapshot.IsRational);
        Assert.Equal(new GpPoint(3, 6, 0), bezierSnapshot.Poles[1]);
        Assert.Equal(0.65, bezierSnapshot.Weights[1], 10);

        FreeformCurveDefinition explicitSpline = FreeformCurveDefinition.BSpline(
            [new(0, 4, 2), new(3, 8, 2), new(7, 0, 2), new(10, 4, 2)],
            [0, 1], [4, 4], 3, weights: [1, 0.75, 0.75, 1]);
        using Shape spline = FreeformAuthoring.CreateCurve(explicitSpline);
        FreeformCurveDefinition splineSnapshot = FreeformAuthoring.GetCurveDefinition(spline);
        Assert.Equal(3, splineSnapshot.Degree); Assert.Equal([0d, 1d], splineSnapshot.Knots);

        using Shape interpolated = FreeformAuthoring.InterpolateCurve(
            [new(0, 0, 4), new(3, 2, 4), new(7, -2, 4), new(10, 0, 4)],
            new GpXyz(1, 0, 0), new GpXyz(1, 0, 0));
        using Shape periodic = FreeformAuthoring.InterpolateCurve(
            [new(0, 0, 6), new(5, 0, 6), new(5, 5, 6), new(0, 5, 6)], periodic: true);
        using Shape approximated = FreeformAuthoring.ApproximateCurve(
            [new(0, 0, 8), new(2, 1, 8), new(4, -1, 8), new(6, 2, 8), new(8, 0, 8)]);
        Assert.Equal(FreeformGeometryKind.BSpline, FreeformAuthoring.GetCurveDefinition(interpolated).Kind);
        Assert.True(FreeformAuthoring.GetCurveDefinition(periodic).Periodic);
        Assert.Equal(FreeformGeometryKind.BSpline, FreeformAuthoring.GetCurveDefinition(approximated).Kind);

        using Shape elevated = FreeformAuthoring.ElevateCurveDegree(bezier, 5);
        using Shape reversed = FreeformAuthoring.ReverseCurve(spline);
        using Shape segment = FreeformAuthoring.SegmentCurve(bezier, new ParameterRange(0.2, 0.8));
        Assert.Equal(5, FreeformAuthoring.GetCurveDefinition(elevated).Degree);
        Assert.Equal(splineSnapshot.Poles[^1], FreeformAuthoring.GetCurveDefinition(reversed).Poles[0]);
        IReadOnlyList<Shape> pieces = FreeformAuthoring.SplitCurve(bezier, [0.25, 0.6]);
        try { Assert.Equal(3, pieces.Count); Assert.All(pieces, piece => Assert.Equal(ShapeKind.Edge, piece.Kind)); }
        finally { DisposeAll(pieces); }

        Assert.NotEmpty(FreeformAuthoring.ProjectPoint(bezier, new GpPoint(5, 3, 0)));
        Assert.NotEmpty(FreeformAuthoring.CurveExtrema(bezier, spline));
        using Shape plane = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(0, -10, -10), new(0, -10, 10), new(0, 10, -10), new(0, 10, 10)]));
        using Shape crossing = ShapeFactory.CreateEdge(new GpPoint(-5, 0, 0), new GpPoint(5, 0, 0));
        Assert.Single(FreeformAuthoring.IntersectCurveWithFace(crossing, plane));

        Assert.Throws<ArgumentException>(() => FreeformCurveDefinition.BSpline(
            [new(0, 0, 0), new(1, 0, 0)], [0, 1], [3, 3], 2));
        bezier.Dispose();
        Assert.Throws<ObjectDisposedException>(() => FreeformAuthoring.GetCurveDefinition(bezier));
    }

    [Fact]
    public void SurfaceDefinitionsFittingEditingFillRuledAndOffsetRemainOwning()
    {
        GpPoint[] grid = CreateWaveGrid(3, 3, 0.0);
        using Shape bezierFace = FreeformAuthoring.CreateSurfaceFace(
            FreeformSurfaceDefinition.Bezier(3, 3, grid,
                [1, 0.8, 1, 0.9, 0.6, 0.9, 1, 0.8, 1]));
        FreeformSurfaceDefinition snapshot = FreeformAuthoring.GetSurfaceDefinition(bezierFace);
        Assert.Equal(3, snapshot.UPoleCount); Assert.Equal(3, snapshot.VPoleCount); Assert.True(snapshot.IsRational);

        IReadOnlyList<IReadOnlyList<GpPoint>> fitGrid =
        [
            [new(0, 0, 0), new(0, 3, 1), new(0, 6, 0), new(0, 9, -1)],
            [new(3, 0, 1), new(3, 3, 3), new(3, 6, 1), new(3, 9, 0)],
            [new(6, 0, 0), new(6, 3, 2), new(6, 6, 0), new(6, 9, -1)],
            [new(9, 0, -1), new(9, 3, 0), new(9, 6, -1), new(9, 9, -2)]
        ];
        using Shape interpolated = FreeformAuthoring.InterpolateSurface(fitGrid);
        using Shape approximated = FreeformAuthoring.ApproximateSurface(fitGrid);
        Assert.Equal(FreeformGeometryKind.BSpline, FreeformAuthoring.GetSurfaceDefinition(interpolated).Kind);
        Assert.Equal(FreeformGeometryKind.BSpline, FreeformAuthoring.GetSurfaceDefinition(approximated).Kind);

        using Shape elevated = FreeformAuthoring.ElevateSurfaceDegree(bezierFace, 4, 4);
        using Shape reversedU = FreeformAuthoring.ReverseSurfaceU(bezierFace);
        using Shape reversedV = FreeformAuthoring.ReverseSurfaceV(bezierFace);
        using Shape trimmed = FreeformAuthoring.TrimSurface(bezierFace, new SurfaceParameterBounds(0.15, 0.85, 0.2, 0.8));
        Assert.Equal(4, FreeformAuthoring.GetSurfaceDefinition(elevated).UDegree);
        Assert.Equal(ShapeKind.Face, reversedU.Kind); Assert.Equal(ShapeKind.Face, reversedV.Kind); Assert.Equal(ShapeKind.Face, trimmed.Kind);

        using Shape lower = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier(
            [new(0, 0, 0), new(4, 2, 0), new(8, 0, 0)]));
        using Shape upper = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier(
            [new(0, 0, 4), new(4, -2, 5), new(8, 0, 4)]));
        using Shape ruled = FreeformAuthoring.CreateRuledFace(lower, upper);
        Assert.Equal(ShapeKind.Face, ruled.Kind);

        Shape[] boundary =
        [
            ShapeFactory.CreateEdge(new(0, 0, 0), new(10, 0, 0)),
            ShapeFactory.CreateEdge(new(10, 0, 0), new(10, 10, 1)),
            ShapeFactory.CreateEdge(new(10, 10, 1), new(0, 10, 0)),
            ShapeFactory.CreateEdge(new(0, 10, 0), new(0, 0, 0))
        ];
        try
        {
            using FreeformShapeResult filled = FreeformAuthoring.FillBoundary(boundary, [new GpPoint(5, 5, 2)]);
            Assert.Equal(ShapeKind.Face, filled.Shape.Kind); Assert.True(filled.Diagnostics.G0Error >= 0);
            using FreeformShapeResult offset = FreeformAuthoring.OffsetFaceOrShell(filled.Shape, 0.5);
            Assert.True(offset.Shape.IsValid);
        }
        finally { DisposeAll(boundary); }

        Assert.Throws<ArgumentException>(() => FreeformSurfaceDefinition.Bezier(3, 3, [new GpPoint(0, 0, 0)]));
        bezierFace.Dispose();
        Assert.True(interpolated.IsValid);
    }

    [Fact]
    public void LocatedProfilesOffsetSplitPipeLoftSewAndHealFormOneTopologyClosure()
    {
        GpPoint[] square = [new(-2, -2, 0), new(2, -2, 0), new(2, 2, 0), new(-2, 2, 0)];
        using Shape lower = FreeformAuthoring.CreateLocatedPlanarProfile(square, new GpPoint(0, 0, 0), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0));
        using Shape upper = FreeformAuthoring.CreateLocatedPlanarProfile(square, new GpPoint(0, 0, 8), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0), interpolate: true);
        using Shape planarOffset = FreeformAuthoring.OffsetPlanarWire(lower, 1.0);
        Assert.True(planarOffset.CountSubShapes(ShapeKind.Edge) >= 4);

        using FreeformShapeResult smoothLoft = FreeformAuthoring.CreateLoft([lower, upper], makeSolid: true, smoothing: true);
        using FreeformShapeResult ruledLoft = FreeformAuthoring.CreateLoft([lower, upper], makeSolid: true, ruled: true, smoothing: false);
        Assert.True(smoothLoft.Diagnostics.IsValid); Assert.True(ruledLoft.Diagnostics.IsValid);

        using Shape spineEdge = ShapeFactory.CreateEdge(new GpPoint(0, 0, 0), new GpPoint(0, 0, 12));
        using Shape spine = ShapeFactory.CreateWire([spineEdge]);
        using FreeformShapeResult pipe = FreeformAuthoring.CreatePipeShell(spine, [lower], makeSolid: true, maximumDegree: 8, maximumSegments: 24);
        Assert.True(pipe.Shape.IsValid);

        using Shape box = ShapeFactory.CreateBox(10, 10, 10);
        using Shape tool = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(-1, -1, 5), new(-1, 11, 5), new(11, -1, 5), new(11, 11, 5)]));
        using FreeformShapeResult split = FreeformAuthoring.SplitTopology([box], [tool]);
        Assert.True(split.Diagnostics.ResultCount >= 2); Assert.True(split.Diagnostics.ModifiedCount > 0);
        using FreeformShapeResult healed = FreeformAuthoring.Heal(split.Shape);
        Assert.True(healed.Diagnostics.IsValid);

        Shape[] loftFaces = smoothLoft.Shape.GetSubShapes(ShapeKind.Face);
        try
        {
            using FreeformShapeResult sewn = FreeformAuthoring.SewHealValidate(loftFaces);
            Assert.True(sewn.Diagnostics.IsValid);
        }
        finally { DisposeAll(loftFaces); }
    }

    [Fact]
    public void FreeformStepXdeViewerMeasurementMeshAndScreenshotRunFromOneWorkflow()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchF.{Guid.NewGuid():N}"); Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            using Shape lower = FreeformAuthoring.CreateLocatedPlanarProfile(
                [new(-4, -3, 0), new(4, -3, 0), new(4, 3, 0), new(-4, 3, 0)],
                new GpPoint(0, 0, 0), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0), interpolate: true);
            using Shape upper = FreeformAuthoring.CreateLocatedPlanarProfile(
                [new(-2, -2, 0), new(2, -2, 0), new(2, 2, 0), new(-2, 2, 0)],
                new GpPoint(0, 0, 10), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0), interpolate: true);
            using FreeformShapeResult authored = FreeformAuthoring.CreateLoft([lower, upper], makeSolid: true);
            string step = Path.Combine(directory, "batch-f-freeform.step");
            using (XdeDocument document = XdeDocument.Create())
            {
                using XdeTransaction transaction = document.BeginTransaction();
                XdeLabel label = document.AddShape(authored.Shape, "Batch F Freeform Loft");
                label.Color = new XdeColor(0.18, 0.55, 0.86, 1.0); Assert.True(transaction.Commit());
                document.WriteStep(step);
            }
            using XdeDocument imported = XdeDocument.ReadStep(step);
            XdeLabel importedLabel = Assert.Single(imported.GetFreeShapes());
            using Shape importedShape = importedLabel.Shape;
            Assert.True(importedShape.IsValid); Assert.True(importedShape.CountSubShapes(ShapeKind.Face) >= 3);
            DetailedMeshSnapshot mesh = importedShape.CreateDetailedMesh(0.25, 0.5); Assert.NotEmpty(mesh.Vertices); Assert.NotEmpty(mesh.Triangles);
            ShapeInspectionProperties mass = importedShape.InspectProperties(InspectionPropertyKind.Volume); Assert.True(mass.Mass > 0);

            using OcctViewer viewer = OcctViewer.Create(window); using ViewerPresentation presentation = viewer.Display(importedShape);
            presentation.SetSelectionKind(ShapeKind.Face); viewer.FitAll(); viewer.Redraw();
            Assert.NotEmpty(viewer.SelectRectangle(0, 0, 255, 255));
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-f.png"), ViewerScreenshotBuffer.Rgb);
            Assert.True(new FileInfo(screenshot).Length > 0);
        }
        finally { Assert.True(NativeWindowMethods.DestroyWindow(window)); Directory.Delete(directory, recursive: true); }
    }

    private static GpPoint[] CreateWaveGrid(int uCount, int vCount, double z)
    {
        GpPoint[] result = new GpPoint[uCount * vCount];
        for (int u = 0; u < uCount; ++u) for (int v = 0; v < vCount; ++v)
            result[u * vCount + v] = new GpPoint(u * 4, v * 4, z + (u == 1 && v == 1 ? 3 : 0));
        return result;
    }

    private static void DisposeAll(IEnumerable<Shape> shapes) { foreach (Shape shape in shapes) shape.Dispose(); }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch F", 0x80000000u,
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
