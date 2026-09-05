using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchPCompletionTests
{
    [Fact]
    public void AnalyticFramesDerivativesCurvatureAndSingularChartsAreCopied()
    {
        Assert.Equal(72, Marshal.SizeOf<OcctSharp.Interop.SurfaceInfoRaw>());
        Assert.Equal(160, Marshal.SizeOf<OcctSharp.Interop.SurfaceSampleRaw>());
        SketchPlane frame = new(new(10, 20, 30), new(0, 1, 0), new(0, 0, 1));
        foreach (AnalyticSurfaceKind kind in Enum.GetValues<AnalyticSurfaceKind>())
        {
            SurfaceParameterBounds bounds = kind == AnalyticSurfaceKind.Sphere ? new(0, Math.Tau, -Math.PI / 2, Math.PI / 2)
                : kind == AnalyticSurfaceKind.Torus ? new(0, Math.Tau, 0, Math.Tau) : new(0, Math.Tau, 0, 5);
            using Shape face = SurfaceModeling.CreateAnalyticFace(kind, frame, bounds, 4, 0.5);
            Assert.True(face.IsValid);
            SurfaceDescriptor descriptor = SurfaceModeling.Describe(face);
            SurfaceEvaluationPoint value = SurfaceModeling.Evaluate(face, new(0.3, 0.4), normalized: true);
            Assert.NotNull(value.Normal); Assert.NotNull(value.GaussianCurvature);
            Assert.Equal(SurfaceDomainState.Inside, value.DomainState);
            using Shape reversed = face.Reversed();
            SurfaceEvaluationPoint opposite = SurfaceModeling.Evaluate(reversed, value.Uv);
            Assert.Equal(-value.Normal!.Value.X, opposite.Normal!.Value.X, 8);
            Assert.Equal(-value.MinimumCurvature!.Value, opposite.MaximumCurvature!.Value, 8);
            if (kind == AnalyticSurfaceKind.Plane) Assert.Equal(0, value.GaussianCurvature!.Value, 8);
            if (kind == AnalyticSurfaceKind.Cylinder)
            {
                Assert.True(descriptor.IsUPeriodic);
                Assert.Equal(0.25, Math.Max(Math.Abs(value.MinimumCurvature!.Value), Math.Abs(value.MaximumCurvature!.Value)), 8);
            }
            if (kind == AnalyticSurfaceKind.Sphere)
                Assert.True(SurfaceModeling.Evaluate(face, new(1, Math.PI / 2)).IsParameterSingular);
        }
    }

    [Fact]
    public void HolesBatchProjectionGridMasksAndBoundaryMetricsUseTheTrimmedDomain()
    {
        using Shape basis = Plane();
        using Shape face = SurfaceModeling.CreateTrimmedFace(basis, HoledProfile());
        Assert.Equal(96, face.InspectProperties(InspectionPropertyKind.Area).Mass, 6);
        Assert.Equal(SurfaceDomainState.Inside, SurfaceModeling.Classify(face, new(2, 2)));
        Assert.Equal(SurfaceDomainState.Outside, SurfaceModeling.Classify(face, new(5, 5)));
        Assert.Equal(SurfaceDomainState.Boundary, SurfaceModeling.Classify(face, new(4, 5)));
        Assert.Equal(SurfaceDomainState.Outside, SurfaceModeling.Classify(face, new(20, 20)));
        SurfaceGrid grid = SurfaceModeling.SampleGrid(face, 11, 11);
        Assert.Equal(SurfaceDomainState.Outside, grid.Samples[5 * 11 + 5].DomainState);
        IReadOnlyList<SurfacePointSolution> values = SurfaceModeling.ProjectPoints(face, [new(2, 2, 3), new(5, 5, 3)]);
        Assert.All(values, value => Assert.Equal(0, value.SourceIndex));
        Assert.Single(SurfaceModeling.ProjectPoint(face, new(5, 5, 3), limitToFace: false));
        Assert.Empty(SurfaceModeling.ProjectPoints(face, []));
        IReadOnlyList<SurfaceBoundaryLoop> loops = SurfaceModeling.GetBoundaryLoops(face);
        Assert.Equal(2, loops.Count); Assert.Single(loops, loop => loop.IsOuter);
        Assert.Equal(48, loops.Sum(loop => loop.Length), 6);
        Assert.All(loops, loop => Assert.NotNull(loop.SignedUvArea));
        face.Dispose();
        Assert.Equal(121, grid.Samples.Count); Assert.Equal(8, loops.Sum(loop => loop.Segments.Count));
    }

    [Fact]
    public void LiftedCurvesDefinitionsProjectionAndThreeDimensionalArcLengthCompose()
    {
        using Shape cylinder = Cylinder();
        SketchCurve2d uv = SketchCurve2d.Segment(new(0.2, 1), new(5.5, 7));
        using Shape edge = SurfaceModeling.LiftCurve(cylinder, uv);
        Assert.True(edge.IsValid);
        SurfaceCurveDefinition copied = SurfaceModeling.GetCurveDefinition(cylinder, edge);
        Assert.Equal(uv.Evaluate(0).Point.X, copied.Curve.Evaluate(0).Point.X, 6);
        SurfaceCurveDefinition derived = SurfaceModeling.DeriveCurveDefinition(cylinder, edge, 1e-5);
        Assert.InRange(derived.Residual, 0, 1e-5);
        IReadOnlyList<SurfaceCurveSample> samples = SurfaceModeling.SampleCurve(cylinder, edge, 11);
        Assert.Equal(11, samples.Count);
        Assert.Equal(uv.Evaluate(1).Point.Y, samples[^1].Uv.Y, 6);
        Assert.Equal(Math.Sqrt(Math.Pow(3 * 5.3, 2) + 36), edge.InspectProperties(InspectionPropertyKind.Length).Mass, 5);
        using Shape reverse = SurfaceModeling.LiftCurve(cylinder, uv.Reverse());
        Assert.Equal(samples[^1].Point.X, SurfaceModeling.SampleCurve(cylinder, reverse, 11)[0].Point.X, 6);
        using Shape iso = SurfaceModeling.CreateIsoEdge(cylinder, SurfaceIsoDirection.ConstantU, 1, new(0, 8));
        Assert.Equal(8, iso.InspectProperties(InspectionPropertyKind.Length).Mass, 6);
        using Shape offSurface = ShapeFactory.CreateEdge(new(20, 0, 0), new(20, 0, 4));
        Assert.Throws<ArgumentException>(() => SurfaceModeling.DeriveCurveDefinition(cylinder, offSurface));
        edge.Dispose(); cylinder.Dispose();
        Assert.True(copied.Curve.Evaluate(0.5).Point.X > 0);
    }

    [Fact]
    public void SeamBranchesPeriodicShiftsAndContinuousPointTracesAreExplicit()
    {
        using Shape cylinder = Cylinder(); SurfaceDescriptor descriptor = SurfaceModeling.Describe(cylinder);
        IReadOnlyList<SurfaceBoundaryLoop> loops = SurfaceModeling.GetBoundaryLoops(cylinder);
        Assert.Equal(2, loops.Sum(loop => loop.SeamOccurrenceCount));
        SurfaceBoundarySegment[] seams = loops.SelectMany(loop => loop.Segments).Where(segment => segment.IsSeam).ToArray();
        Assert.Equal(Math.Tau, Math.Abs(seams[0].Definition.Curve.Evaluate(0.5).Point.X - seams[1].Definition.Curve.Evaluate(0.5).Point.X), 6);
        Assert.Equal(0.3, SurfaceModeling.NormalizeUv(descriptor, new(-Math.Tau + 0.3, 2)).X, 8);
        IReadOnlyList<SurfaceUvShift> unwrapped = SurfaceModeling.UnwrapUv(descriptor, [new(6.1, 2), new(0.1, 2), new(0.3, 2)]);
        Assert.True(unwrapped[1].Uv.X > 6.1); Assert.Equal(1, unwrapped[1].UPeriodShift);
        double[] angles = [6.1, 6.2, 0.05, 0.15];
        GpPoint[] points = angles.Select(angle => new GpPoint(3.1 * Math.Cos(angle), 3.1 * Math.Sin(angle), 4)).ToArray();
        IReadOnlyList<SurfaceTracePoint> trace = SurfaceModeling.TracePoints(cylinder, points, maximumDistance: 0.2);
        Assert.Equal(4, trace.Count); Assert.True(trace[2].Uv.X > trace[1].Uv.X);
        Assert.Throws<InvalidOperationException>(() => SurfaceModeling.TracePoints(cylinder, [new(50, 0, 3)], maximumDistance: 1));
        using Shape plane = Plane();
        Assert.Throws<ArgumentException>(() => SurfaceModeling.ShiftUv(SurfaceModeling.Describe(plane), new(1, 1), 1, 0));
    }

    [Fact]
    public void RebuildingMissingCurvesRepairsCopiesWithoutMutatingInputs()
    {
        using Shape cylinder = Cylinder();
        using Shape incomplete = SurfaceModeling.LiftCurve(cylinder, SketchCurve2d.Segment(new(1, 1), new(2, 7)), build3d: false);
        SurfaceRepairDiagnostics before = SurfaceModeling.InspectRepresentations(incomplete);
        Assert.Equal(1, before.MissingCurveCountBefore);
        using SurfaceRepairResult repaired = SurfaceModeling.Repair(incomplete, 1e-6);
        Assert.Equal(0, repaired.Diagnostics.MissingCurveCountAfter);
        Assert.True(repaired.Shape.IsValid);
        Assert.Equal(1, SurfaceModeling.InspectRepresentations(incomplete).MissingCurveCountBefore);
        incomplete.Dispose(); cylinder.Dispose(); Assert.True(repaired.Shape.IsValid);
    }

    [Fact]
    public void WireOrderingNonPlanarHolesAndFaceSplitsRetainOwningTopology()
    {
        using Shape cylinder = Cylinder();
        SketchProfile2d profile = SketchProfile2d.Create(Rectangle(0.4, 1, 5, 7), [Rectangle(1, 3, 2, 4)]);
        using Shape curved = SurfaceModeling.CreateTrimmedFace(cylinder, profile);
        Assert.True(curved.IsValid);
        Assert.Equal(3 * ((5 - 0.4) * 6 - 1), curved.InspectProperties(InspectionPropertyKind.Area).Mass, 4);
        using Shape cut = SurfaceModeling.LiftCurve(cylinder, SketchCurve2d.Segment(new(0, 4), new(Math.Tau, 4)));
        using Shape split = SurfaceModeling.SplitFace(cylinder, [cut]);
        Assert.Equal(2, split.FaceCount);
        Assert.Equal(cylinder.InspectProperties(InspectionPropertyKind.Area).Mass, split.InspectProperties(InspectionPropertyKind.Area).Mass, 4);
        Shape[] edges = Rectangle(0.4, 1, 2, 3).Curves.Select(curve => SurfaceModeling.LiftCurve(cylinder, curve)).ToArray();
        try { using Shape wire = SurfaceModeling.CreateWire(cylinder, [edges[2], edges[0], edges[3], edges[1]]); Assert.True(wire.IsValid); }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Fact]
    public void SmoothUvFittingOffsetAndFreeformFacesHaveRealGeometry()
    {
        SketchPoint2d[] points = [new(1, 1), new(2, 3), new(4, 2), new(6, 5)];
        SurfaceCurveDefinition interpolated = SurfaceModeling.InterpolateUv(points);
        SurfaceCurveDefinition approximated = SurfaceModeling.ApproximateUv(points);
        Assert.True(interpolated.Curve.Degree > 1); Assert.True(approximated.Curve.Degree > 1);
        foreach (SketchPoint2d point in points) Assert.InRange(interpolated.Curve.Project(point)[0].Distance, 0, 1e-6);
        SurfaceCurveDefinition offset = SurfaceModeling.OffsetUv(SketchCurve2d.Segment(new(1, 1), new(5, 1)), 0.2);
        Assert.Equal(0.2, Math.Abs(offset.Curve.Evaluate(0.5).Point.Y - 1), 6);
        FreeformSurfaceDefinition surface = FreeformSurfaceDefinition.Bezier(2, 2,
            [new(0, 0, 0), new(10, 0, 1), new(0, 10, 2), new(10, 10, 4)]);
        using Shape face = FreeformAuthoring.CreateSurfaceFace(surface);
        Assert.Equal(SurfaceGeometryType.BezierSurface, SurfaceModeling.Describe(face).Kind);
        using Shape edge = SurfaceModeling.LiftCurve(face, SketchCurve2d.Bezier([new(0.1, 0.1), new(0.4, 0.8), new(0.9, 0.7)]));
        Assert.True(edge.IsValid); Assert.NotNull(SurfaceModeling.Evaluate(face, new(0.5, 0.5)).Normal);
        using Shape trimmed = SurfaceModeling.CreateTrimmedFace(face, SketchProfile2d.Create(Rectangle(0.1, 0.1, 0.9, 0.9), [Rectangle(0.3, 0.3, 0.5, 0.5)]));
        Assert.True(trimmed.IsValid);
    }

    [Fact]
    public void NormalProjectionSectionsAndCurveSurfaceIntersectionsAreBounded()
    {
        using Shape basis = Plane(); using Shape face = SurfaceModeling.CreateTrimmedFace(basis, HoledProfile());
        using Shape input = ShapeFactory.CreateEdge(new(-2, 5, 3), new(12, 5, 3));
        using Shape projected = SurfaceModeling.ProjectShape(face, input);
        Assert.Equal(2, projected.CountSubShapes(ShapeKind.Edge));
        using Shape cylinder = Cylinder();
        using Shape section = SurfaceModeling.IntersectPlane(cylinder, new(new(0, 0, 4), new(1, 0, 0), new(0, 1, 0)), new(-5, 5, -5, 5));
        Assert.True(section.CountSubShapes(ShapeKind.Edge) > 0);
        using Shape vertical = ShapeFactory.CreateEdge(new(2, 2, -2), new(2, 2, 2));
        Assert.Single(SurfaceModeling.IntersectCurve(face, vertical));
        using Shape throughHole = ShapeFactory.CreateEdge(new(5, 5, -2), new(5, 5, 2));
        Assert.Empty(SurfaceModeling.IntersectCurve(face, throughHole));
        using Shape coplanar = ShapeFactory.CreateEdge(new(1, 2, 0), new(9, 2, 0));
        Assert.Contains(SurfaceModeling.IntersectCurve(face, coplanar), hit => hit.Kind == SurfaceIntersectionKind.CoincidentInterval);
        using Shape acrossHole = ShapeFactory.CreateEdge(new(-2, 5, 0), new(12, 5, 0));
        SurfaceCurveIntersection[] intervals = SurfaceModeling.IntersectCurve(face, acrossHole)
            .Where(hit => hit.Kind == SurfaceIntersectionKind.CoincidentInterval).ToArray();
        Assert.Equal(2, intervals.Length);
        Assert.Equal(8, intervals.Sum(hit => hit.CurveParameters.Last - hit.CurveParameters.First), 6);
    }

    [Fact]
    public void InvalidInputsAndDisposedParentsFailBeforeUnsafeNativeUse()
    {
        using Shape face = Plane();
        Assert.Throws<ArgumentOutOfRangeException>(() => SurfaceModeling.SampleGrid(face, int.MaxValue, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SurfaceModeling.Evaluate(face, new(1.1, 0.5), normalized: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => SurfaceModeling.Classify(face, new(double.NaN, 0)));
        using Shape box = ShapeFactory.CreateBox(1, 1, 1);
        Assert.Throws<InvalidCastException>(() => SurfaceModeling.Describe(box));
        face.Dispose(); Assert.Throws<ObjectDisposedException>(() => SurfaceModeling.Describe(face));
    }

    [Fact]
    public void LocatedTopologyAndCopiedSplitResultsRetainWorldCoordinates()
    {
        using Shape source = Cylinder();
        using GpTrsf translation = GpTrsf.Create(10, 20, 30);
        using TopLocLocation location = TopLocLocation.FromTransform(translation);
        using Shape located = source.Located(location);
        SurfaceEvaluationPoint sample = SurfaceModeling.Evaluate(located, new(1, 2));
        Assert.Equal(30 + 2, sample.Point.Z, 8);
        Assert.Equal(10 + 3 * Math.Cos(1), sample.Point.X, 8);
        Assert.InRange(SurfaceModeling.ProjectPoint(located, sample.Point)[0].Distance, 0, 1e-6);
        using Shape edge = SurfaceModeling.LiftCurve(located, SketchCurve2d.Segment(new(0, 4), new(Math.Tau, 4)));
        Assert.InRange(SurfaceModeling.DeriveCurveDefinition(located, edge, 1e-5).Residual, 0, 1e-5);
        SurfaceRepairDiagnostics before = SurfaceModeling.InspectRepresentations(located);
        using SurfaceSplitResult result = SurfaceModeling.SplitFaceWithDiagnostics(located, [edge]);
        Assert.Equal(2, result.Diagnostics.FaceCount);
        Assert.Equal(result.Diagnostics.SourceArea, result.Diagnostics.ResultArea, 5);
        Assert.Equal(before, SurfaceModeling.InspectRepresentations(located));
        located.Dispose(); edge.Dispose(); Assert.True(result.Shape.IsValid);
    }

    [Fact]
    public void RepresentationFlagsAreRepairedOnCopiesAndSourceFilesRemainUnchanged()
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.BatchP.Flags." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using Shape face = Cylinder();
            using Shape edge = SurfaceModeling.LiftCurve(face, SketchCurve2d.Segment(new(1, 1), new(2, 6)));
            string path = ShapeExchange.WriteBrep(edge, Path.Combine(directory, "edge.brep"));
            string original = File.ReadAllText(path);
            var flags = new System.Text.RegularExpressions.Regex(@"(?m)(^Ed\r?\n[^\r\n]*?)[ \t]+1[ \t]+1[ \t]+0[ \t]*\r?$");
            Assert.Single(flags.Matches(original));
            string damaged = flags.Replace(original, "$1 0 0 0");
            File.WriteAllText(path, damaged);
            using Shape inconsistent = ShapeExchange.ReadBrep(path);
            Assert.Equal(1, SurfaceModeling.InspectRepresentations(inconsistent).InconsistentEdgeCountBefore);
            using SurfaceRepairResult result = SurfaceModeling.Repair(inconsistent);
            Assert.Equal(0, result.Diagnostics.InconsistentEdgeCountAfter);
            Assert.Equal(1, SurfaceModeling.InspectRepresentations(inconsistent).InconsistentEdgeCountBefore);
            Assert.Equal(damaged, File.ReadAllText(path));
            Assert.Throws<ArgumentException>(() => SurfaceModeling.Repair(inconsistent, 1e-5, 1e-7));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void SurfaceWorkflowSurvivesMetadataExchangeAndRealHwndReview()
        => OcctSharp.Validation.BatchPSurfaceWorkflow.Run();

    [Fact]
    public void PeriodicFitsDegenerateBoundariesAndProjectionControlsAreExplicit()
    {
        SurfaceCurveDefinition periodic = SurfaceModeling.InterpolateUv([new(1, 1), new(3, 1), new(3, 3), new(1, 3)], periodic: true);
        Assert.True(periodic.Curve.Periodic);
        SurfaceCurveDefinition c1 = SurfaceModeling.ApproximateUv([new(1, 1), new(2, 4), new(3, 2), new(4, 5)],
            minimumDegree: 2, continuity: FreeformContinuity.C1);
        Assert.InRange(c1.Residual, 0, 1e-5);
        Assert.Throws<ArgumentException>(() => SurfaceModeling.ApproximateUv([new(1, 1), new(2, 4), new(3, 2)],
            continuity: (FreeformContinuity)99));
        using Shape sphere = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Sphere,
            SketchPlane.XY, new(0, Math.Tau, -Math.PI / 2, Math.PI / 2), 3);
        Assert.Equal(2, SurfaceModeling.GetBoundaryLoops(sphere).SelectMany(loop => loop.Segments).Count(segment => segment.IsDegenerate));
        using Shape plane = Plane();
        using Shape far = ShapeFactory.CreateEdge(new(1, 2, 10), new(8, 2, 10));
        using Shape empty = SurfaceModeling.ProjectShape(plane, far, new() { MaximumDistance = 1 });
        Assert.Equal(0, empty.CountSubShapes(ShapeKind.Edge));
        using Shape cylinder = Cylinder();
        Assert.True(SurfaceModeling.ProjectPoint(cylinder, new(4, 0, 4), limitToFace: false).Count >= 2);
        SurfaceCurveDefinition offset = SurfaceModeling.OffsetUv(
            SketchCurve2d.Bezier([new(1, 1), new(1.5, 3), new(2, 5)]), 0.1);
        using Shape lifted = SurfaceModeling.LiftCurve(cylinder, offset.Curve);
        Assert.True(lifted.IsValid);
        Assert.Throws<ArgumentException>(() => SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Sphere,
            SketchPlane.XY, new(0, Math.Tau, -2, 2), 3));
        using Shape reversed = cylinder.Reversed();
        using Shape trimmed = SurfaceModeling.CreateTrimmedFace(reversed, SketchProfile2d.Create(Rectangle(1, 1, 2, 3)));
        Assert.True(SurfaceModeling.Describe(trimmed).IsReversed);
        using Shape disjoint = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane,
            new(new(0, 0, 20), new(1, 0, 0), new(0, 1, 0)), new(0, 10, 0, 10));
        using Shape noSection = SurfaceModeling.IntersectFaces(plane, disjoint);
        Assert.Equal(0, noSection.CountSubShapes(ShapeKind.Edge));
    }

    private static Shape Plane() => SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(0, 10, 0, 10));
    private static Shape Cylinder() => SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Cylinder, SketchPlane.XY, new(0, Math.Tau, 0, 8), 3);
    private static SketchProfile2d HoledProfile() => SketchProfile2d.Create(Rectangle(0, 0, 10, 10), [Rectangle(4, 4, 6, 6)]);
    private static SketchCurveChain2d Rectangle(double x0, double y0, double x1, double y1) => SketchCurveChain2d.Create([
        SketchCurve2d.Segment(new(x0, y0), new(x1, y0)), SketchCurve2d.Segment(new(x1, y0), new(x1, y1)),
        SketchCurve2d.Segment(new(x1, y1), new(x0, y1)), SketchCurve2d.Segment(new(x0, y1), new(x0, y0))], true);
}
