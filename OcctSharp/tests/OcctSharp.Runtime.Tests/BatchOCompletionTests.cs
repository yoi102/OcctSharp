using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchOCompletionTests
{
    [Fact]
    public void TrimAndSimilarityTransformsPreserveTheParametricPointSequence()
    {
        SketchCurve2d[] curves = [
            SketchCurve2d.Segment(new(2, 1), new(8, 3)),
            SketchCurve2d.CircularArc(new(3, 4), 2, 0.3, 1.7),
            SketchCurve2d.EllipticArc(new(3, 4), 4, 2, 0.3, -1.7, 0.2),
            SketchCurve2d.Bezier([new(1, 2), new(3, 7), new(8, 1)], [1.0, 0.8, 1.0]),
            SketchCurve2d.Interpolate([new(1, 2), new(3, 7), new(8, 1)])];
        SketchTransform2d[] transforms = [SketchTransform2d.Translation(2, 3),
            SketchTransform2d.Rotation(0.7), SketchTransform2d.Scale(2),
            SketchTransform2d.Scale(-2), SketchTransform2d.Mirror(new(1, 2), SketchDirection2d.YAxis)];
        foreach (SketchCurve2d original in curves)
        foreach (SketchCurve2d curve in new[] { original, original.Reverse() })
        {
            SketchCurve2d trimmed = curve.Trim(0.2, 0.7);
            AssertPoint(curve.Evaluate(0.2).Point, trimmed.Evaluate(0).Point);
            AssertPoint(curve.Evaluate(0.7).Point, trimmed.Evaluate(1).Point);
            foreach (SketchTransform2d transform in transforms)
            {
                SketchCurve2d transformed = curve.Transform(transform);
                foreach (double parameter in new[] { 0.0, 0.2, 0.7, 1.0 })
                    AssertPoint(transform.Apply(curve.Evaluate(parameter).Point), transformed.Evaluate(parameter).Point);
                SketchPoint2d pointOnCurve = transformed.Evaluate(0.4).Point;
                IReadOnlyList<SketchProjection> projections = transformed.Project(pointOnCurve);
                Assert.True(projections.Count > 0, $"Projection missing for {curve.Kind}, reversed={curve.Reversed}, transform={transform}.");
                SketchProjection projection = projections[0];
                Assert.InRange(projection.NormalizedParameter, 0, 1);
                AssertPoint(pointOnCurve, transformed.Evaluate(projection.NormalizedParameter).Point);
                using Shape edge = transformed.ToEdge(SketchPlane.YZ);
                Assert.True(edge.IsValid);
            }
        }
        Assert.Throws<ArgumentException>(() => SketchCurve2d.BSpline([new(0, 0), new(1, 0)], [], [], 1));
        Assert.Throws<InvalidOperationException>(() => curves[0].Transform(default));
        Assert.Throws<ArgumentException>(() => SketchTransform2d.Mirror(default, default));
        Assert.Equal(1, SketchDirection2d.Create(1e200, 1e200).X * Math.Sqrt(2), 12);
        Assert.Equal(1, SketchDirection2d.Create(double.MaxValue, double.MaxValue).X * Math.Sqrt(2), 12);
        SketchCurve2d unclamped = SketchCurve2d.BSpline(
            [new(0, 0), new(1, 2), new(3, 2), new(4, 0)],
            [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0], [1, 1, 1, 1, 1, 1, 1], 2);
        Assert.Equal(0, unclamped.FirstParameter);
        Assert.Equal(2, unclamped.LastParameter);
        using Shape unclampedEdge = unclamped.ToEdge(SketchPlane.XY);
        Assert.True(unclampedEdge.IsValid);
    }

    [Fact]
    public void ShuffledOpenChainsAndCrossingHolesHaveDeterministicValidation()
    {
        SketchCurveChain2d open = SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(1, 0), new(2, 0)),
            SketchCurve2d.Segment(new(0, 0), new(1, 0)),
            SketchCurve2d.Segment(new(2, 0), new(3, 0))]);
        Assert.False(open.IsClosed);
        Assert.Equal(3, open.Measure().Perimeter, 8);
        SketchCurveChain2d outer = SketchCurveChain2d.Create([SketchCurve2d.Circle(new(0, 0), 20)], true);
        SketchCurveChain2d horizontal = SketchCurveChain2d.Create([
            SketchCurve2d.Ellipse(new(0, 0), 8, 1, Math.PI)], true);
        SketchCurveChain2d vertical = SketchCurveChain2d.Create([
            SketchCurve2d.Ellipse(new(0, 0), 8, 1, Math.PI / 2)], true);
        Assert.Throws<SketchValidationException>(() => SketchProfile2d.Create(outer, [horizontal, vertical]));
    }

    private static void AssertPoint(SketchPoint2d expected, SketchPoint2d actual)
    {
        Assert.Equal(expected.X, actual.X, 8);
        Assert.Equal(expected.Y, actual.Y, 8);
    }

    [Fact]
    public void CurvedAreaBoundaryNestingSelfIntersectionsAndGapToleranceAreGeometric()
    {
        SketchCurveChain2d circle = SketchCurveChain2d.Create([SketchCurve2d.Circle(default, 10, 0.023)], true);
        Assert.Equal(100 * Math.PI, circle.Measure().SignedArea, 8);
        SketchBounds2d bounds = circle.Measure().Bounds;
        Assert.True(bounds.Minimum.X <= -10 && bounds.Maximum.X >= 10);
        double angle = Math.PI / 64;
        SketchPoint2d center = new(9.995 * Math.Cos(angle), 9.995 * Math.Sin(angle));
        SketchCurveChain2d smallHole = SketchCurveChain2d.Create([SketchCurve2d.Circle(center, 0.001)], true);
        using Shape narrow = SketchProfile2d.Create(circle, [smallHole]).CreateFace(SketchPlane.XY);
        Assert.True(narrow.IsValid);

        SketchCurve2d selfCrossing = SketchCurve2d.Interpolate([new(0, 0), new(4, 4), new(0, 4), new(4, 0)]);
        Assert.Throws<SketchValidationException>(() => SketchCurveChain2d.Create([selfCrossing]));
        SketchCurve2d line = SketchCurve2d.Segment(new(0, 0), new(10, 0));
        Assert.NotEmpty(line.Intersect(SketchCurve2d.Segment(new(5, 0), new(15, 0))));
        Assert.NotEmpty(SketchCurve2d.Circle(default, 2).Intersect(SketchCurve2d.Segment(new(-3, 2), new(3, 2))));

        SketchCurve2d near = SketchCurve2d.Segment(new(10.00001, 0), new(20, 0));
        Assert.Throws<SketchValidationException>(() => SketchCurveChain2d.Create([line, near]));
        SketchCurveChain2d connected = SketchCurveChain2d.Create([near, line], tolerance: 1e-4);
        using Shape wire = connected.BuildWire(SketchPlane.XY);
        Assert.True(wire.IsValid);
        using Shape originalEdge = line.ToEdge(SketchPlane.XY);
        Assert.True(originalEdge.GetTopologySummary().VertexTolerance.Maximum < 1e-4);
        using Shape negativeOffset = circle.Offset(SketchPlane.XY, -1);
        Assert.True(negativeOffset.IsValid);
        Assert.Throws<ArgumentOutOfRangeException>(() => circle.Offset(SketchPlane.XY, 0));
    }

    [Fact]
    public void CopiedValuesPlaneAndAllCurveFamiliesInspectAndEditIndependently()
    {
        Assert.Equal(16, Marshal.SizeOf<OcctSharp.Interop.SketchPoint2dRaw>());
        Assert.Equal(72, Marshal.SizeOf<OcctSharp.Interop.SketchPlaneRaw>());
        Assert.Equal(104, Marshal.SizeOf<OcctSharp.Interop.SketchCurveRaw>());

        SketchDirection2d direction = SketchDirection2d.Create(3, 4);
        Assert.Equal(1.0, Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y), 12);
        SketchPlane plane = new(new GpPoint(4, 5, 6), new GpXyz(0, 1, 0), new GpXyz(0, 0, 1));
        SketchPoint2d local = new(7, 8);
        Assert.Equal(local, plane.ToLocal(plane.ToWorld(local)));
        Assert.Throws<ArgumentException>(() => new SketchPlane(new(0, 0, 0), new(1, 0, 0), new(1, 0, 0)));

        SketchCurve2d segment = SketchCurve2d.Segment(new(0, 0), new(10, 0));
        SketchEvaluation middle = segment.Evaluate(0.5);
        Assert.Equal(new SketchPoint2d(5, 0), middle.Point);
        Assert.Equal(0.5, middle.NormalizedParameter, 12);

        SketchCurve2d circle = SketchCurve2d.Circle(new(5, 5), 3);
        SketchProjection nearest = circle.Project(new(9, 5))[0];
        Assert.Equal(1, nearest.Distance, 8);
        SketchCurve2d ellipse = SketchCurve2d.EllipticArc(new(0, 0), 6, 3, 0, Math.PI);
        Assert.Equal(6, ellipse.Evaluate(0).Point.X, 8);

        SketchCurve2d bezier = SketchCurve2d.Bezier(
            [new(0, 0), new(5, 4), new(10, 0)], [1.0, 0.8, 1.0]);
        Assert.True(bezier.Evaluate(0.5).Point.Y > 0);
        SketchCurve2d spline = SketchCurve2d.Interpolate([new(0, -1), new(5, -1), new(10, -1)]);
        Assert.Equal(-1, spline.Evaluate(0.5).Point.Y, 8);

        SketchCurve2d vertical = SketchCurve2d.Segment(new(5, -5), new(5, 5));
        SketchIntersection crossing = Assert.Single(segment.Intersect(vertical));
        Assert.Equal(new SketchPoint2d(5, 0), crossing.Point);
        Assert.Empty(segment.Intersect(SketchCurve2d.Segment(new(0, 2), new(10, 2))));

        IReadOnlyList<SketchCurve2d> split = bezier.Split([0.25, 0.75]);
        Assert.Equal(3, split.Count);
        Assert.Equal(bezier.Evaluate(0.25).Point, split[0].Evaluate(1).Point);
        SketchCurve2d transformed = ellipse.Transform(SketchTransform2d.Translation(2, 3));
        Assert.Equal(8, transformed.Evaluate(0).Point.X, 8);
        Assert.Equal(3, transformed.Evaluate(0).Point.Y, 8);
        SketchCurve2d reversed = segment.Reverse();
        Assert.Equal(segment.Evaluate(1).Point, reversed.Evaluate(0).Point);
        Assert.Equal(segment.Evaluate(0).Point, reversed.Evaluate(1).Point);
        Assert.Equal(-segment.Evaluate(0.25).FirstDerivative.X, reversed.Evaluate(0.75).FirstDerivative.X, 12);
    }

    [Fact]
    public void MixedLoopsDiagnosticsHolesOffsetsAndFeaturesFormOneOwningClosure()
    {
        SketchCurve2d bottom = SketchCurve2d.Segment(new(0, 0), new(20, 0));
        SketchCurve2d right = SketchCurve2d.Bezier([new(20, 0), new(20, 5), new(20, 10)]);
        SketchCurve2d top = SketchCurve2d.BSpline(
            [new(20, 10), new(10, 10), new(0, 10)], [0.0, 1.0, 2.0], [2, 1, 2], 1);
        SketchCurve2d left = SketchCurve2d.Segment(new(0, 10), new(0, 0));
        SketchCurveChain2d outer = SketchCurveChain2d.Create([top, bottom, left, right], requireClosed: true);
        SketchLoopMeasurement measurement = outer.Measure();
        Assert.Equal(60, measurement.Perimeter, 6);
        Assert.Equal(200, Math.Abs(measurement.SignedArea), 5);
        Assert.Empty(outer.Inspect());

        Assert.Throws<SketchValidationException>(() => SketchCurveChain2d.Create([bottom, bottom], requireClosed: true));
        Assert.Throws<SketchValidationException>(() => SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(0, 0), new(10, 10)),
            SketchCurve2d.Segment(new(10, 10), new(0, 10)),
            SketchCurve2d.Segment(new(0, 10), new(10, 0)),
            SketchCurve2d.Segment(new(10, 0), new(0, 0))], requireClosed: true));

        SketchCurveChain2d hole = SketchCurveChain2d.Create([SketchCurve2d.Circle(new(10, 5), 2)], requireClosed: true);
        SketchProfile2d profile = SketchProfile2d.Classify([hole, outer]);
        using Shape face = profile.CreateFace(SketchPlane.XY);
        Assert.True(face.IsValid);
        Assert.Equal(200 - Math.PI * 4, face.InspectProperties(InspectionPropertyKind.Area).Mass, 3);
        using Shape offset = outer.Offset(SketchPlane.XY, 1, PlanarOffsetJoin.Arc);
        Assert.True(offset.IsValid);
        using Shape prism = profile.Extrude(SketchPlane.XY, 5);
        Assert.True(prism.IsValid);
        Assert.Equal((200 - Math.PI * 4) * 5, prism.InspectProperties(InspectionPropertyKind.Volume).Mass, 2);

        SketchCurveChain2d revolveLoop = SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(10, 0), new(12, 0)),
            SketchCurve2d.Segment(new(12, 0), new(12, 4)),
            SketchCurve2d.Segment(new(12, 4), new(10, 4)),
            SketchCurve2d.Segment(new(10, 4), new(10, 0))], requireClosed: true);
        SketchProfile2d revolveProfile = SketchProfile2d.Create(revolveLoop);
        using Shape revolved = revolveProfile.Revolve(SketchPlane.XY, new(0, 0), SketchDirection2d.YAxis);
        Assert.True(revolved.IsValid);
        Assert.True(revolved.InspectProperties(InspectionPropertyKind.Volume).Mass > 0);

        using Shape baseSolid = ShapeFactory.CreateBox(30, 20, 4);
        using Shape added = profile.AddTo(baseSolid, SketchPlane.XY, 5);
        using Shape cut = profile.CutFrom(baseSolid, SketchPlane.XY, 5);
        Assert.True(added.InspectProperties(InspectionPropertyKind.Volume).Mass > baseSolid.InspectProperties(InspectionPropertyKind.Volume).Mass);
        Assert.True(cut.InspectProperties(InspectionPropertyKind.Volume).Mass < baseSolid.InspectProperties(InspectionPropertyKind.Volume).Mass);
    }

    [Fact]
    public void PlanarFeatureMetadataSurvivesStepAndIgesRoundTrips()
    {
        string directory = CreateDirectory();
        try
        {
            SketchProfile2d profile = RectangleProfile(12, 8);
            using Shape feature = profile.Extrude(SketchPlane.XY, 6);
            XdePartMetadata metadata = new("Batch O planar feature", new XdeColor(0.15, 0.65, 0.3), ["Sketch", "Feature"]);
            string step = SketchProfile2d.WriteStep(feature, Path.Combine(directory, "batch-o.step"), metadata);
            string iges = SketchProfile2d.WriteIges(feature, Path.Combine(directory, "batch-o.iges"), metadata);
            Assert.True(new FileInfo(step).Length > 0); Assert.True(new FileInfo(iges).Length > 0);
            using XdeDocument restoredStep = XdeDocument.ReadStep(step);
            using XdeDocument restoredIges = XdeDocument.ReadIges(iges);
            foreach (XdeDocument document in new[] { restoredStep, restoredIges })
            {
                XdeLabel root = Assert.Single(document.GetFreeShapes());
                Assert.Contains("Batch O", root.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                using Shape shape = root.Shape; Assert.True(shape.IsValid);
                IReadOnlyList<XdePresentationStyle> styles = root.GetPresentationStyles();
                try
                {
                    Assert.Contains(styles, style => style.EffectiveColor is { } color
                        && Math.Abs(color.Red - 0.15) < 0.02
                        && Math.Abs(color.Green - 0.65) < 0.02
                        && Math.Abs(color.Blue - 0.3) < 0.02);
                    using DocumentSnapshot snapshot = document.CreateSnapshot();
                    Assert.NotEmpty(snapshot.Labels.SelectMany(label => document.GetLabel(label.Entry).Layers));
                }
                finally { foreach (XdePresentationStyle style in styles) style.Dispose(); }
            }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void PlanarFeatureDisplaysAndSelectsThroughRealHwndAfterDefinitionLifetimeEnds()
    {
        string directory = CreateDirectory(); nint window = CreateTestWindow();
        try
        {
            Shape feature;
            {
                SketchProfile2d profile = RectangleProfile(10, 6);
                feature = profile.Extrude(new SketchPlane(new(2, 3, 4), new(1, 0, 0), new(0, 1, 0)), 5);
            }
            using (feature)
            using (OcctViewer viewer = OcctViewer.Create(window))
            using (ViewerPresentation presentation = viewer.Display(feature))
            {
                viewer.FitAll(); viewer.Redraw();
                viewer.MoveTo(128, 128);
                Assert.Single(viewer.SelectAt(128, 128, ViewerSelectionMode.Replace));
                string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-o.png"));
                Assert.True(new FileInfo(screenshot).Length > 0);
                Assert.True(feature.IsValid);
            }
        }
        finally { Assert.True(NativeWindowMethods.DestroyWindow(window)); Directory.Delete(directory, recursive: true); }
    }

    private static SketchProfile2d RectangleProfile(double width, double height)
    {
        SketchCurveChain2d loop = SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(0, 0), new(width, 0)),
            SketchCurve2d.Segment(new(width, 0), new(width, height)),
            SketchCurve2d.Segment(new(width, height), new(0, height)),
            SketchCurve2d.Segment(new(0, height), new(0, 0))], requireClosed: true);
        return SketchProfile2d.Create(loop);
    }

    private static string CreateDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchO.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory); return directory;
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch O", 0x80000000u,
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
