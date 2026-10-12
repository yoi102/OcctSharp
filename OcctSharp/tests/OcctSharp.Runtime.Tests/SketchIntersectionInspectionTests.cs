using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchIntersectionInspectionTests
{
    private static SketchCurve2d Curve()=>SketchCurve2d.BSpline([new(8,0),new(20,2),new(16,4),new(18,6),new(16,9)],[0,.2,1],[4,1,4],3);
    [Fact] public void SameObjectAndCopiedPartialAndReversedSplineReportOverlapIntervals()
    {
        var curve=Curve();foreach(var second in new[]{curve,Curve(),curve.Trim(.2,.8),curve.Reverse()})
        {var result=SketchModeling.InspectIntersections(curve,second);Assert.True(result.HasOverlappingIntervals);Assert.True(result.OverlapIntervalCount>0);}
        // The legacy same-object call intentionally means self-intersection and stays unchanged.
        Assert.Empty(curve.Intersect(curve));
    }
    [Fact] public void PartialCollinearSegmentsAndIsolatedCrossingsRemainDistinct()
    {
        var first=SketchCurve2d.Segment(new(0,0),new(10,0));
        var overlap=SketchModeling.InspectIntersections(first,SketchCurve2d.Segment(new(2,0),new(8,0)));
        Assert.True(overlap.HasOverlappingIntervals);Assert.Equal(0,overlap.PointCount);Assert.Equal(2,overlap.Endpoints.Count);
        var crossing=SketchModeling.InspectIntersections(first,SketchCurve2d.Segment(new(5,-5),new(5,5)));
        Assert.False(crossing.HasOverlappingIntervals);Assert.Equal(1,crossing.PointCount);Assert.Single(crossing.Endpoints);
    }
    [Fact] public void TangentAndMultiplePointContactsHaveNoInventedInterval()
    {
        var curve=SketchCurve2d.Bezier([new(-2,0),new(0,4),new(2,0)]);
        var tangent=SketchModeling.InspectIntersections(curve,SketchCurve2d.Segment(new(-3,2),new(3,2)));
        Assert.False(tangent.HasOverlappingIntervals);Assert.Equal(1,tangent.PointCount);
        var multiple=SketchModeling.InspectIntersections(curve,SketchCurve2d.Segment(new(-3,1),new(3,1)));
        Assert.False(multiple.HasOverlappingIntervals);Assert.Equal(2,multiple.PointCount);
    }
    [Fact] public void InvalidArgumentsAndResultLifetimeAreCopiedOnly()
    {
        var curve=Curve();Assert.Throws<ArgumentNullException>(()=>SketchModeling.InspectIntersections(curve,null!));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchModeling.InspectIntersections(curve,curve,0));
        var result=SketchModeling.InspectIntersections(curve,SketchCurve2d.Segment(new(0,4),new(30,4)));
        using(var edge=SketchModeling.CreateEdge(curve,SketchPlane.XY)){edge.Dispose();Assert.Single(result.Endpoints);}
    }
}
