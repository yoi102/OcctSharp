namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionChainTests
{
    [Theory]
    [InlineData(.2)]
    [InlineData(-.2)]
    public void MixedClosedChainKeepsItsExactOffsetEdgeAndSuppliedOrder(double distance)
    {
        var curved = SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)])).Offset(distance);
        var start = curved.Evaluate(0).Point;
        var end = curved.Evaluate(1).Point;
        var source = new[] { curved, Line(end, new(10,-4)), Line(new(10,-4),new(-1,-4)), Line(new(-1,-4),start) };
        var chain = SketchExpressionChain2d.CreateOrdered(source,true);
        Assert.True(chain.IsClosed);
        Assert.Same(curved,chain.Curves[0]);
        source[0] = Line(new(0,0),new(1,0));
        Assert.Same(curved,chain.Curves[0]);
        using var wire = chain.BuildWire(SketchPlane.XY);
        Assert.True(wire.IsValid);
        Assert.Equal(4,wire.GetTopologySummary().UniqueCounts.EdgeCount);
        var edges = wire.GetSubShapes(ShapeKind.Edge);
        try { Assert.Equal(1,edges.Count(edge => edge.GetEdgeCurveSnapshot().CurveType == CurveGeometryType.OffsetCurve)); }
        finally { foreach(var edge in edges)edge.Dispose(); }
        var reverse = chain.Reverse();
        Assert.True(reverse.IsClosed);
        Assert.InRange(reverse.Curves[^1].Evaluate(1).Point.DistanceTo(curved.Evaluate(0).Point),0,1e-9);
        using var reversedWire = reverse.BuildWire(SketchPlane.XY);
        Assert.True(reversedWire.IsValid);
        Assert.InRange(Math.Abs(wire.InspectProperties(InspectionPropertyKind.Length).Mass-
            reversedWire.InspectProperties(InspectionPropertyKind.Length).Mass),0,1e-8);
    }

    [Theory]
    [InlineData(false,.2)]
    [InlineData(false,-.2)]
    [InlineData(true,.2)]
    [InlineData(true,-.2)]
    public void ASinglePeriodicOffsetIsAClosedNativeWire(bool mirrored,double distance)
    {
        var curve = SketchCurveExpression2d.FromCurve(SketchCurve2d.Ellipse(new(2,-3),8,3)).Offset(distance);
        if(mirrored)curve = curve.Transform(SketchTransform2d.Mirror(default,SketchDirection2d.XAxis));
        var chain = SketchExpressionChain2d.CreateOrdered([curve],true);
        Assert.True(chain.IsClosed);
        using var wire = chain.BuildWire(new SketchPlane(new(10,20,30),new(0,1,0),new(0,0,1)));
        Assert.True(wire.IsValid);
        Assert.Equal(1,wire.GetTopologySummary().UniqueCounts.EdgeCount);
    }

    [Fact]
    public void OpenChainDoesNotInventAClosingCurveAndShuffledInputIsRejected()
    {
        var first = Line(new(0,0),new(2,0)).Offset(.2);
        var second = Line(first.Evaluate(1).Point,new(3,2));
        var third = Line(new(3,2),new(4,3));
        var chain = SketchExpressionChain2d.CreateOrdered([first,second,third]);
        Assert.False(chain.IsClosed);
        using var wire = chain.BuildWire(SketchPlane.XY);
        Assert.Equal(3,wire.GetTopologySummary().UniqueCounts.EdgeCount);
        var open = Assert.Throws<SketchValidationException>(()=>SketchExpressionChain2d.CreateOrdered([first,second,third],true));
        Assert.Contains(open.Diagnostics,value=>value.Code==SketchDiagnosticCode.OpenChain);
        var gap = Assert.Throws<SketchValidationException>(()=>SketchExpressionChain2d.CreateOrdered([first,third,second]));
        Assert.Contains(gap.Diagnostics,value=>value.Code==SketchDiagnosticCode.Gap);
        Assert.True(wire.IsValid);
    }

    [Fact]
    public void OverlapAndNonAdjacentCrossingsAreRejectedWithoutChangingAnExistingWire()
    {
        var valid = SketchExpressionChain2d.CreateOrdered([Line(new(0,0),new(4,0)),Line(new(4,0),new(4,3))]);
        using var previous = valid.BuildWire(SketchPlane.XY);
        var overlap = Assert.Throws<SketchValidationException>(()=>SketchExpressionChain2d.CreateOrdered([
            Line(new(0,0),new(4,0)),Line(new(4,0),new(2,0))]));
        Assert.Contains(overlap.Diagnostics,value=>value.Code==SketchDiagnosticCode.DuplicateCurve);
        var crossing = Assert.Throws<SketchValidationException>(()=>SketchExpressionChain2d.CreateOrdered([
            Line(new(0,0),new(4,4)),Line(new(4,4),new(0,4)),Line(new(0,4),new(4,0)),Line(new(4,0),new(0,0))],true));
        Assert.Contains(crossing.Diagnostics,value=>value.Code==SketchDiagnosticCode.SelfIntersection);
        Assert.True(previous.IsValid);
        Assert.Equal(2,previous.GetTopologySummary().UniqueCounts.EdgeCount);
    }

    [Fact]
    public void InvalidInputAndNativeFailureLeaveCallerExpressionsAndExistingGeometryUsable()
    {
        var curve = Line(new(0,0),new(4,0));
        var chain = SketchExpressionChain2d.CreateOrdered([curve]);
        using var wire = chain.BuildWire(SketchPlane.XY);
        Assert.Throws<ArgumentNullException>(()=>SketchExpressionChain2d.CreateOrdered(null!));
        Assert.Throws<ArgumentException>(()=>SketchExpressionChain2d.CreateOrdered([]));
        Assert.Throws<ArgumentException>(()=>SketchExpressionChain2d.CreateOrdered([null!]));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionChain2d.CreateOrdered([curve],tolerance:0));
        Assert.Throws<ArgumentNullException>(()=>chain.BuildWire(null!));
        var c0 = SketchCurveExpression2d.FromCurve(SketchCurve2d.BSpline([
            new(0,0),new(1,2),new(2,0),new(3,-2),new(4,0)],[0,.5,1],[3,2,3],2)).Offset(.1);
        Assert.Throws<ArgumentException>(()=>SketchExpressionChain2d.CreateOrdered([c0]));
        Assert.True(wire.IsValid);
        Assert.Equal(new SketchPoint2d(0,0),curve.Evaluate(0).Point);
    }

    private static SketchCurveExpression2d Line(SketchPoint2d first,SketchPoint2d last) =>
        SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(first,last));
}
