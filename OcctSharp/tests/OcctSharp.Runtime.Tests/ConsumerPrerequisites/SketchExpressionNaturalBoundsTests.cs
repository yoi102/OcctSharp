using Xunit;
namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionNaturalBoundsTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void CompleteAndOneSidedTrimsRetainExactAuthoredFiniteBounds(bool reverse)
    {
        var primitive=SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(new(2,-3),4,.38,Math.PI));
        var offset=primitive.Offset(.2);
        if(reverse)offset=offset.Reverse();
        // This finite domain reproduces the old subtraction underflow.
        Assert.True(offset.LastParameter-(offset.LastParameter-offset.FirstParameter)<offset.FirstParameter);
        Assert.Same(offset,offset.Trim(0,1));
        var start=offset.Trim(0,.37);var end=offset.Trim(.37,1);
        Assert.Equal(reverse?offset.LastParameter:offset.FirstParameter,reverse?start.LastParameter:start.FirstParameter);
        Assert.Equal(reverse?offset.FirstParameter:offset.LastParameter,reverse?end.FirstParameter:end.LastParameter);
        foreach(var trimmed in new[]{start,end})
        {
            Assert.Same(offset.Basis,trimmed.Basis);Assert.Equal(offset.Reversed,trimmed.Reversed);
            Assert.Equal(offset.SignedOffset,trimmed.SignedOffset);
            Assert.InRange(trimmed.FirstParameter,offset.FirstParameter,offset.LastParameter);
            Assert.InRange(trimmed.LastParameter,offset.FirstParameter,offset.LastParameter);
        }
        double span=offset.LastParameter-offset.FirstParameter;
        Assert.Equal(reverse?offset.LastParameter-.37*span:offset.FirstParameter+.37*span,
            reverse?start.FirstParameter:start.LastParameter);
        Assert.Equal(reverse?offset.LastParameter-.37*span:offset.FirstParameter+.37*span,
            reverse?end.LastParameter:end.FirstParameter);
        var nested=offset.Trim(.13,.89).Offset(-.05);
        Assert.Same(nested,nested.Trim(0,1));
        Assert.Equal(nested.FirstParameter,nested.Trim(0,1).FirstParameter);
        Assert.Equal(nested.LastParameter,nested.Trim(0,1).LastParameter);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public void ExactSelectionDoesNotClampNearBoundsOrAllowInvalidNormalizedRanges(bool reverse)
    {
        var expression=SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(new(0,0),3,.38,Math.PI)).Offset(.1);
        if(reverse)expression=expression.Reverse();
        double inside=Math.BitDecrement(1d);
        var trimmed=expression.Trim(0,inside);
        Assert.NotSame(expression,trimmed);
        double span=expression.LastParameter-expression.FirstParameter;
        Assert.Equal(reverse?expression.LastParameter-inside*span:expression.FirstParameter,
            trimmed.FirstParameter);
        Assert.Equal(reverse?expression.LastParameter:expression.FirstParameter+inside*span,
            trimmed.LastParameter);
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Trim(Math.BitDecrement(0d),1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Trim(0,Math.BitIncrement(1d)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Trim(1,1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Trim(double.NaN,1));
        var continued=expression.ContinueNativeRange(-1,5);
        Assert.Same(continued,continued.Trim(0,1));
        Assert.Equal(-1,continued.Trim(0,1).FirstParameter);Assert.Equal(5,continued.Trim(0,1).LastParameter);
    }

    [Theory][InlineData(false,.1)][InlineData(true,.1)][InlineData(false,-.1)][InlineData(true,-.1)]
    public void RotatedClosedNaturalEndpointsKeepExactBoundsAndIndependentArea(bool reverse,double distance)
    {
        const double angle=.38;
        SketchPoint2d P(double x,double y)=>new(3+x*Math.Cos(angle)-y*Math.Sin(angle),-2+x*Math.Sin(angle)+y*Math.Cos(angle));
        var curves=new[]{
            SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(P(0,-2),P(8,-2))),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(P(8,0),2,angle-Math.PI/2,Math.PI)),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(P(8,2),P(0,2))),
            SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(P(0,0),2,angle+Math.PI/2,Math.PI))};
        if(reverse)curves=curves.Reverse().Select(c=>c.Reverse()).ToArray();
        var source=SketchExpressionChain2d.CreateOrdered(curves,true);
        var result=SketchExpressionOffset2d.CreateClosed(source,distance);
        Assert.Equal(4,result.Joints.Count);Assert.Equal(4,result.Segments.Count);
        for(int i=0;i<4;i++)
        {
            var output=result.Segments[i].Curve;var original=curves[i];
            Assert.Equal(SketchExpressionOffsetJointKind.SourceEndpoints,result.Joints[i].Kind);
            Assert.Same(original,output.Basis);
            Assert.Equal(original.FirstParameter,output.FirstParameter);
            Assert.Equal(original.LastParameter,output.LastParameter);
            Assert.Equal(original.Reversed,output.Reversed);
        }
        double area=32+4*Math.PI+distance*(16+4*Math.PI)+distance*distance*Math.PI;
        Assert.InRange(Math.Abs(Math.Abs(result.Chain.Measure().SignedArea)-area),0,1e-8);
        Assert.InRange(Math.Abs(result.Chain.Measure().Perimeter-(16+4*Math.PI+Math.Tau*distance)),0,1e-8);
    }
}
