using Xunit;
namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionOffsetJointTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void TangentCapsuleRetainsExplicitSourceEndpointsWithoutClaimingAnIntersectionChart(bool reverse)
    {
        var curves=new[]{
            SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(new(0,-2),new(8,-2))),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(new(8,0),2,-Math.PI/2,Math.PI)),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(new(8,2),new(0,2))),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(new(0,0),2,Math.PI/2,Math.PI))};
        if(reverse)curves=curves.Reverse().Select(c=>c.Reverse()).ToArray();
        var source=SketchExpressionChain2d.CreateOrdered(curves,true);var result=SketchExpressionOffset2d.CreateClosed(source,.1);
        Assert.Equal(4,result.Joints.Count);Assert.Equal(4,result.Segments.Count);
        for(int i=0;i<4;i++)
        {
            var joint=result.Joints[i];Assert.Equal(i,joint.PreviousSourceIndex);Assert.Equal((i+1)%4,joint.NextSourceIndex);
            Assert.Equal(SketchExpressionOffsetJointKind.SourceEndpoints,joint.Kind);
            Assert.Equal(curves[i].Reversed?curves[i].FirstParameter:curves[i].LastParameter,joint.PreviousParameter);
            var next=curves[(i+1)%4];Assert.Equal(next.Reversed?next.LastParameter:next.FirstParameter,joint.NextParameter);
        }
        double area=32+4*Math.PI+.1*(16+4*Math.PI)+.01*Math.PI;
        Assert.InRange(Math.Abs(Math.Abs(result.Chain.Measure().SignedArea)-area),0,1e-8);
    }
    [Theory][InlineData(SketchExpressionJoin.Miter)][InlineData(SketchExpressionJoin.Round)][InlineData(SketchExpressionJoin.Bevel)]
    public void RectangleDistinguishesActualIntersectionsFromEachExplicitBridge(SketchExpressionJoin join)
    {
        SketchPoint2d[] points=[new(0,0),new(8,0),new(8,4),new(0,4)];
        var source=SketchExpressionChain2d.CreateOrdered(Enumerable.Range(0,4).Select(i=>SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(points[i],points[(i+1)%4]))).ToArray(),true);
        var result=SketchExpressionOffset2d.CreateClosed(source,.2,join);
        var expected=join==SketchExpressionJoin.Miter?SketchExpressionOffsetJointKind.NativeIntersection:
            join==SketchExpressionJoin.Round?SketchExpressionOffsetJointKind.RoundBridge:SketchExpressionOffsetJointKind.BevelBridge;
        Assert.Equal(4,result.Joints.Count);Assert.All(result.Joints,j=>Assert.Equal(expected,j.Kind));
        Assert.Equal(join==SketchExpressionJoin.Miter?4:8,result.Segments.Count);
        Assert.All(result.Joints,j=>{Assert.Equal((j.PreviousSourceIndex+1)%4,j.NextSourceIndex);Assert.True(double.IsFinite(j.PreviousParameter)&&double.IsFinite(j.NextParameter));});
    }
}
