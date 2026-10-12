using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class GuidedOffsetPreparationTests
{
    private static readonly bool[] Switches = [false, true];
    private static readonly double[] Offsets = [-.03, .03];
    public static IEnumerable<object[]> Inputs() => from spline in Switches
        from reverse in Switches from located in Switches
        from distance in Offsets select new object[] { spline, reverse, located, distance };

    [Theory, MemberData(nameof(Inputs))]
    public void PreparationRetainsSourceBytesDirectionFiniteBoundsAndIndependentResult(
        bool spline, bool reverse, bool located, double distance)
    {
        var primitive = spline ? SketchCurve2d.BSpline([new(-.5, 0), new(-1d/6, 2d/3), new(1d/6, 2d/3), new(.5, 0)],
            [0, 1], [4, 4], 3) : SketchCurve2d.Bezier([new(-.5, 0), new(0, 1), new(.5, 0)]);
        var expression = SketchCurveExpression2d.FromCurve(primitive).Offset(distance).Trim(.1, .9);
        if (reverse) expression = expression.Reverse();
        string definition = System.Text.Json.JsonSerializer.Serialize(expression);
        using var original = SketchExpressionChain2d.CreateOrdered([expression]).BuildWire(SketchPlane.XY);
        using var transform = GpTrsf.Create(located ? 11 : 0, located ? -7 : 0, located ? 3 : 0, 1, 2, 3, located ? .7 : 0);
        var source = original.Transformed(transform);
        using var files = new TempFiles(); string before = files.PathFor("before.brep"), after = files.PathFor("after.brep");
        ShapeExchange.WriteBrep(source, before);
        using var prepared = GuidedSweepPlan.PrepareOffsetCurves(source);
        ShapeExchange.WriteBrep(source, after);
        Assert.Equal(File.ReadAllBytes(before), File.ReadAllBytes(after));
        Assert.Equal(definition, System.Text.Json.JsonSerializer.Serialize(expression));
        Assert.True(prepared.Diagnostics.Ready); Assert.True(prepared.Diagnostics.AlgorithmDone);
        Assert.True(prepared.Diagnostics.ShapeIsValid); Assert.False(prepared.Diagnostics.IsSolid);
        Assert.InRange(prepared.Diagnostics.ApproximationError!.Value, 0, 1e-10);
        Assert.True(prepared.RequireShape().GetValidationReport(true, true).IsValid);
        Assert.Equal(source.GetTopologySummary().UniqueCounts, prepared.RequireShape().GetTopologySummary().UniqueCounts);
        Assert.Contains(prepared.History, h => h.Kind == AuthoringHistoryKind.InputSnapshot);
        Assert.All(prepared.History.Where(h => h.Kind == AuthoringHistoryKind.Modified), h =>
        {
            Assert.NotNull(h.Source); Assert.Equal(0, h.Source.Value.ArgumentIndex);
            Assert.Equal(prepared.PlanId, h.Source.Value.PlanId); Assert.NotNull(h.Shape);
        });
        source.Dispose();
        Assert.True(prepared.RequireShape().GetValidationReport(true, true).IsValid);
    }

    [Fact]
    public void NonOffsetWireRemainsAnIndependentExactCopy()
    {
        using var input = SketchCurveChain2d.Create([SketchCurve2d.Segment(new(0, 0), new(2, 0))]).BuildWire(SketchPlane.XY);
        using var prepared = GuidedSweepPlan.PrepareOffsetCurves(input);
        Assert.Equal(0, prepared.Diagnostics.ApproximationError);
        Assert.Equal(input.InspectProperties(InspectionPropertyKind.Length).Mass,
            prepared.RequireShape().InspectProperties(InspectionPropertyKind.Length).Mass);
        Assert.True(prepared.RequireShape().GetValidationReport(true, true).IsValid);
    }

    [Fact]
    public void InvalidControlsKindsAndDisposedInputAreRejected()
    {
        using var input = SketchCurveChain2d.Create([SketchCurve2d.Segment(new(0, 0), new(2, 0))]).BuildWire(SketchPlane.XY);
        Assert.Throws<ArgumentNullException>(() => GuidedSweepPlan.PrepareOffsetCurves(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => GuidedSweepPlan.PrepareOffsetCurves(input, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => GuidedSweepPlan.PrepareOffsetCurves(input, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GuidedSweepPlan.PrepareOffsetCurves(input, maximumSegments: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GuidedSweepPlan.PrepareOffsetCurves(input, maximumSegments: 513));
        Assert.Throws<ArgumentOutOfRangeException>(() => GuidedSweepPlan.PrepareOffsetCurves(input, maximumDegree: 1));
        using var box = ShapeFactory.CreateBox(1, 2, 3);
        Assert.Throws<ArgumentException>(() => GuidedSweepPlan.PrepareOffsetCurves(box));
        input.Dispose(); Assert.Throws<ObjectDisposedException>(() => GuidedSweepPlan.PrepareOffsetCurves(input));
    }

    private sealed class TempFiles : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.GuidedPreparation", Guid.NewGuid().ToString("N"));
        public TempFiles() => Directory.CreateDirectory(directory);
        public string PathFor(string file) => Path.Combine(directory, file);
        public void Dispose() => Directory.Delete(directory, true);
    }
}
