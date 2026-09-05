using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchSAuthoringTests
{
    internal static Shape Square(double z = 0, double side = 2) => ShapeFactory.CreatePolygonWire(
        [new(0, 0, z), new(side, 0, z), new(side, side, z), new(0, side, z)], true);
    internal static Shape Spine() => ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 10)]);

    [Fact]
    public void ScalarDefinitionsAreCopiedAndDomainDerivativesAreExplicit()
    {
        Assert.Equal(96, Marshal.SizeOf<LawSpanRaw>()); Assert.Equal(56, Marshal.SizeOf<LawInputRaw>());
        Assert.Equal(40, Marshal.SizeOf<LawSampleRaw>()); Assert.Equal(48, Marshal.SizeOf<AuthoringInfoRaw>());
        Assert.Equal(144, Marshal.SizeOf<SweepOptionsRaw>());
        var constant = ScalarLawDefinition.Constant(new(2, 6), 3);
        Assert.All(constant.Sample(9).Samples, s => { Assert.Equal(3, s.Value); Assert.Equal(0, s.FirstDerivative); Assert.Equal(0, s.SecondDerivative); });
        var linear = ScalarLawDefinition.Linear(new(2, 6), 1, 5);
        Assert.Equal(3, linear.Evaluate(4).Value, 10); Assert.Equal(1, linear.Evaluate(4).FirstDerivative!.Value, 10);
        var mapped = linear.Trim(new(3, 5)).MapDomain(new(10, 14));
        Assert.Equal(3, mapped.Evaluate(12).Value, 10); Assert.Equal(0.5, mapped.Evaluate(12).FirstDerivative!.Value, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => mapped.Evaluate(9));
        Assert.Equal(2, mapped.Evaluate(9, LawDomainPolicy.Clamp).Value, 10);
        double[] x = [0, 0.5, 1], y = [1, 2, 3]; var interpolation = ScalarLawDefinition.Interpolate(x, y, 2, 2);
        x[1] = 0; y[1] = 99;
        Assert.Equal(2, interpolation.Evaluate(0.5).Value, 8); Assert.Equal(2, interpolation.Evaluate(0).FirstDerivative!.Value, 8);
        Assert.Equal(2, interpolation.Evaluate(1).FirstDerivative!.Value, 8);
        Assert.Throws<ArgumentException>(() => ScalarLawDefinition.Interpolate([0, 0, 1], [1, 2, 3]));
        Assert.Throws<ArgumentException>(() => ScalarLawDefinition.BSpline([1, 2, 3], [0, 1], [2, 2], 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScalarLawDefinition.Constant(new(0, 1), double.NaN));
        Assert.Equal("OcctSharp.Geometry", typeof(ScalarLawDefinition).Assembly.GetName().Name);
    }
    [Fact]
    public void BSplineSmoothCompositeAndSamplingDoNotInventGlobalBounds()
    {
        var spline = ScalarLawDefinition.BSpline([1, 3, 1], [0, 1], [3, 3], 2);
        Assert.Equal(2, spline.Evaluate(0.5).Value, 10); Assert.Equal(-8, spline.Evaluate(0.5).SecondDerivative!.Value, 10);
        var smooth = ScalarLawDefinition.Smooth(new(0, 2), 1, 3, 0.2, 0.4);
        Assert.Equal(1, smooth.Evaluate(0).Value, 8); Assert.Equal(0.2, smooth.Evaluate(0).FirstDerivative!.Value, 8);
        Assert.Equal(3, smooth.Evaluate(2).Value, 8); Assert.Equal(0.4, smooth.Evaluate(2).FirstDerivative!.Value, 8);
        var mapped = smooth.MapDomain(new(0, 4)); Assert.Equal(0.1, mapped.Evaluate(0).FirstDerivative!.Value, 8);
        var composite = ScalarLawDefinition.Composite([ScalarLawDefinition.Linear(new(0, 1), 1, 2), ScalarLawDefinition.Linear(new(1, 2), 4, 5)]);
        Assert.Equal(2, composite.InspectJoins()[0].ValueJump, 10); Assert.Null(composite.Evaluate(1).FirstDerivative);
        Assert.Throws<ArgumentException>(() => ScalarLawDefinition.Composite([ScalarLawDefinition.Constant(new(0, 1), 1), ScalarLawDefinition.Constant(new(2, 3), 2)]));
        // Positive endpoints are not sufficient: this quadratic actually dips below zero.
        var overshoot = ScalarLawDefinition.BSpline([1, -3, 1], [0, 1], [3, 3], 2);
        Assert.False(overshoot.Sample(3).SamplesArePositive); Assert.False(overshoot.Sample(2).HasGlobalPositivityProof);
        Assert.True(overshoot.Sample(2).SamplesArePositive); Assert.True(spline.Sample(3).HasGlobalPositivityProof);
    }
    [Theory]
    [InlineData(GuidedSweepFrame.FixedFrame)]
    [InlineData(GuidedSweepFrame.FixedBinormal)]
    [InlineData(GuidedSweepFrame.Discrete)]
    [InlineData(GuidedSweepFrame.CorrectedFrenet)]
    public void SweepFramesSimulateBuildAndRetainOwningHistory(GuidedSweepFrame frame)
    {
        using Shape spine = Spine(); using Shape section = Square();
        using var plan = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = frame,
            FrameDirection = frame == GuidedSweepFrame.FixedBinormal ? new(0, 1, 0) : new(0, 0, 1), SolidPolicy = SweepSolidPolicy.RequireSolid });
        Assert.True(plan.Preflight().Ready);
        using var preview = plan.Simulate(5); Assert.True(preview.Diagnostics.AlgorithmDone, preview.Diagnostics.Message);
        Assert.Equal(5, preview.SimulatedSections.Count); Assert.All(preview.SimulatedSections, s => Assert.Equal(1, s.CountSubShapes(ShapeKind.Wire)));
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message); Assert.True(result.Diagnostics.IsSolid);
        Assert.NotNull(result.FirstSection); Assert.NotNull(result.LastSection); Assert.NotNull(result.Diagnostics.ApproximationError);
        Assert.Contains(result.History, h => h.Kind == AuthoringHistoryKind.Generated && h.Source.HasValue);
        Assert.Contains(result.History, h => h.Kind == AuthoringHistoryKind.Unmapped && h.Shape is null);
        spine.Dispose(); section.Dispose(); plan.Dispose();
        Assert.Equal(6, result.RequireShape().FaceCount); Assert.Throws<ObjectDisposedException>(() => plan.Build());
    }
    [Fact]
    public void LawSweepRejectsGuideConflictAndUsesPositiveControlHull()
    {
        using Shape spine = Spine(); using Shape section = Square();
        using var plan = GuidedSweepPlan.Create(spine, [new(section)], scaleLaw: ScalarLawDefinition.Linear(new(0, 1), 1, 2));
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        using var bad = GuidedSweepPlan.Create(spine, [new(section)], scaleLaw: ScalarLawDefinition.BSpline([1, -3, 1], [0, 1], [3, 3], 2));
        Assert.Throws<ArgumentException>(() => bad.Build());
        Assert.Throws<ArgumentException>(() => GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = GuidedSweepFrame.AuxiliarySpine }, spine,
            ScalarLawDefinition.Constant(new(0, 1), 1)));
        using var degenerate = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = GuidedSweepFrame.FixedBinormal, FrameDirection = new(0, 0, 1) });
        Assert.Throws<ArgumentException>(() => degenerate.Build());
    }
    [Fact]
    public void LoftCompatibilityReportsSectionsAndEndpointProvenance()
    {
        using Shape a = Square(); using Shape b = Square(5, 3); using Shape c = Square(10, 2);
        using var result = GuidedLoft.Build([a, b, c], new() { Solid = true, CorrectCompatibility = true });
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.NotNull(result.FirstSection); Assert.NotNull(result.LastSection);
        Assert.Contains(result.History, h => h.Kind == AuthoringHistoryKind.Generated);
        Assert.Equal(3, result.History.Count(h => h.Kind == AuthoringHistoryKind.CompatibleSection));
        a.Dispose(); b.Dispose(); c.Dispose(); Assert.NotNull(result.RequireShape());
    }
    [Theory]
    [InlineData(SurfaceConstraintContinuity.G0)]
    [InlineData(SurfaceConstraintContinuity.G1)]
    [InlineData(SurfaceConstraintContinuity.G2)]
    public void SupportedBoundaryAndUvConstraintsAreMeasuredOnFinalSurface(SurfaceConstraintContinuity continuity)
    {
        using Shape wire = Square(); using Shape support = ShapeFactory.CreatePlanarFace(wire);
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            List<SurfaceConstraint> constraints = edges.Select((edge, i) => (SurfaceConstraint)new SurfaceEdgeConstraint($"edge-{i}", edge, continuity, SupportFace: support)).ToList();
            // Use UVs measured from the support's own surface domain, not world XY assumptions.
            var domain = SurfaceModeling.Describe(support).Bounds;
            constraints.Add(new SurfaceUvConstraint("center", support, (domain.FirstU + domain.LastU) / 2, (domain.FirstV + domain.LastV) / 2, continuity));
            using var plan = ConstrainedFillPlan.Create(constraints, initialSurface: support);
            support.Dispose(); foreach (Shape edge in edges) edge.Dispose();
            using var result = plan.Build(); Assert.True(result.Result.Diagnostics.AlgorithmDone, result.Result.Diagnostics.Message);
            Assert.True(result.Accepted, string.Join("; ", result.Constraints.Select(c => $"{c.Id}: {c.PositionResidual}, {c.AngularResidual}, {c.CurvatureResidual}, {c.Accepted}")));
            Assert.All(result.Constraints, c => Assert.True(c.Accepted)); Assert.NotNull(result.RequireFace());
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }
}
