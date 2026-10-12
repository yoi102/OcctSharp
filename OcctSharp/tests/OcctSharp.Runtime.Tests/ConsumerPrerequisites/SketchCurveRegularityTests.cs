using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchCurveRegularityTests
{
    public static IEnumerable<object[]> RegularCurves()
    {
        var curves = new[]
        {
            SketchCurve2d.Segment(new(2,3),new(8,11)),
            SketchCurve2d.CircularArc(default,5,5.5,.5),
            SketchCurve2d.EllipticArc(default,8,3,5.5,1.1),
            SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)]),
            SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)],[1,1.2,.9]),
            SketchCurve2d.BSpline([new(0,0),new(2,1),new(5,.5),new(8,0)],[0,.5,1],[3,1,3],2)
        };
        foreach(var curve in curves) foreach(double distance in new[]{.2,-.2}) foreach(bool reverse in new[]{false,true})
            yield return [curve,distance,reverse];
    }
    [Theory,MemberData(nameof(RegularCurves))]
    public void FiniteAndContinuedDomainsCertifyWithoutChangingAuthorValues(SketchCurve2d curve,double distance,bool reverse)
    {
        var source=SketchCurveExpression2d.FromCurve(reverse?curve.Reverse():curve).Offset(reverse?-distance:distance);
        var expression=source.ContinueNativeRange(source.FirstParameter-.05,source.LastParameter+.05);
        var before=JsonSerializer.Serialize(expression);var result=SketchCurveRegularity2d.Inspect(expression);
        Assert.Equal(SketchCurveRegularityState.Certified,result.State);Assert.True(result.MinimumTangentialScale>0);
        Assert.Equal(expression.FirstParameter,result.FirstParameter);Assert.Equal(expression.LastParameter,result.LastParameter);
        Assert.InRange(result.IntervalsChecked,1,SketchCurveRegularity2d.MaximumIntervals);
        // Independent native points check the certificate's bound and the
        // declared analytic native geometry; these points are not its proof.
        for(int i=0;i<=16;i++)
        {
            double u=expression.FirstParameter+i/16.0*(expression.LastParameter-expression.FirstParameter);
            var basis=SketchCurveExpression2d.FromCurve(curve).ContinueNativeRange(expression.FirstParameter,expression.LastParameter);
            var d1=basis.Derivative(u,1,false);var d2=basis.Derivative(u,2,false);
            double curvature=(d1.X*d2.Y-d1.Y*d2.X)/Math.Pow(d1.Magnitude,3);
            Assert.True(result.MinimumTangentialScale<=1+distance*curvature);
        }
        using var edge=expression.ToEdge(SketchPlane.XY);Assert.True(edge.IsValid);
        Assert.Equal(before,JsonSerializer.Serialize(expression));
    }
    [Theory]
    [InlineData(.37123456789,1.0)] [InlineData(.0000001,1.0)] [InlineData(.9999999,1.0)]
    [InlineData(.37123456789,.01)] [InlineData(.37123456789,100.0)]
    public void InteriorEvenMultiplicityCuspCannotPassBetweenDisplaySamples(double center,double scale)
    {
        // r(u)=(scale*u,scale*(u-center)^2), kappa(center)=2/scale.
        // At d=-scale/2 the scale factor touches zero without changing sign.
        var curve=Parabola(center,scale);var expression=SketchCurveExpression2d.FromCurve(curve).Offset(-scale/2);
        var before=JsonSerializer.Serialize(expression);var proof=SketchCurveRegularity2d.Inspect(expression);
        Assert.False(proof.IsCertified);Assert.Equal(SketchCurveRegularityState.NotCertified,proof.State);
        Assert.Contains("unresolved",proof.Diagnostic);Assert.Equal(1,proof.ExpressionNodeIndex);
        Assert.Equal(before,JsonSerializer.Serialize(expression));
        Assert.True(SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(curve).Offset(-scale*.49)).IsCertified);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FoldedIntervalRejectsAtomicallyAndLocatesSourceCurve(bool reverse)
    {
        var expression=SketchCurveExpression2d.FromCurve(Parabola(.37123456789,1));
        var chain=SketchExpressionChain2d.CreateOrdered([reverse?expression.Reverse():expression]);
        var before=JsonSerializer.Serialize(chain);
        var proof=SketchCurveRegularity2d.Inspect(expression.Offset(-.75));Assert.False(proof.IsCertified);
        var error=Assert.Throws<SketchValidationException>(()=>SketchExpressionOffset2d.Create(chain,reverse?.75:-.75));
        Assert.All(error.Diagnostics,d=>{Assert.Equal(SketchDiagnosticCode.OffsetRegularityFailure,d.Code);Assert.Equal(0,d.FirstCurveIndex);Assert.Equal(0,d.SecondCurveIndex);Assert.Contains("native interval",d.Message);});
        Assert.Equal(before,JsonSerializer.Serialize(chain));
        var valid=SketchExpressionOffset2d.Create(chain,reverse?.1:-.1);using var wire=valid.Chain.BuildWire(SketchPlane.XY);Assert.True(wire.IsValid);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CancelledFinalDistanceCannotHideUnsafeIntermediateStage(bool reversedIntermediate)
    {
        var source=SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(default,5)).Offset(-6);
        var expression=reversedIntermediate?source.Reverse().Offset(-6):source.Offset(6);
        var proof=SketchCurveRegularity2d.Inspect(expression);
        Assert.Equal(SketchCurveRegularityState.Folded,proof.State);Assert.Equal(1,proof.ExpressionNodeIndex);
        Assert.Equal(SketchCurveRegularityState.Certified,SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(default,5))).State);
    }
    [Fact]
    public void FiniteEllipseAvoidingHighCurvatureCertifiesButWholeEllipseDoesNot()
    {
        // Minimum global radius of curvature is b*b/a=1.125. Around pi/2
        // it is a*a/b=21.333..., so -2 is safe only on the finite arc.
        var finite=SketchCurveExpression2d.FromCurve(SketchCurve2d.EllipticArc(default,8,3,1.4,.3)).Offset(-2);
        Assert.True(SketchCurveRegularity2d.Inspect(finite).IsCertified);
        var whole=SketchCurveExpression2d.FromCurve(SketchCurve2d.Ellipse(default,8,3)).Offset(-2);
        Assert.False(SketchCurveRegularity2d.Inspect(whole).IsCertified);
        var reflected=finite.Transform(SketchTransform2d.Mirror(default,SketchDirection2d.YAxis));
        Assert.True(SketchCurveRegularity2d.Inspect(reflected).IsCertified);
    }
    [Theory]
    [InlineData(2,false)] [InlineData(2,true)] [InlineData(3,false)] [InlineData(3,true)]
    [InlineData(4,false)] [InlineData(4,true)] [InlineData(5,false)] [InlineData(5,true)]
    public void NonuniformRepeatedKnotSplineContinuesExactAuthorTerminalSpans(int degree,bool rational)
    {
        int count=2*degree+1;
        var points=Enumerable.Range(0,count).Select(i=>new SketchPoint2d(i*2,Math.Sin(i*.3))).ToArray();
        var weights=rational?Enumerable.Range(0,count).Select(i=>1+.05*(i%3)).ToArray():null;
        var primitive=SketchCurve2d.BSpline(points,[0,.2,.7,1],[degree+1,degree-1,1,degree+1],degree,false,weights);
        var expression=SketchCurveExpression2d.FromCurve(primitive.Trim(.05,.95)).Offset(.01).ContinueNativeRange(-.01,1.01);
        var result=SketchCurveRegularity2d.Inspect(expression);Assert.True(result.IsCertified,result.Diagnostic);
        for(int i=0;i<=32;i++)
        {
            double u=-.01+i/32.0*1.02;
            var basis=SketchCurveExpression2d.FromCurve(primitive).ContinueNativeRange(-.01,1.01);
            var a=basis.Derivative(u,1,false);var b=basis.Derivative(u,2,false);
            Assert.True(result.MinimumTangentialScale<=1+.01*(a.X*b.Y-a.Y*b.X)/Math.Pow(a.Magnitude,3));
            var independent=IndependentSplinePoint(primitive,u);
            Assert.InRange(basis.Evaluate(u,false).Point.DistanceTo(independent),0,1e-9);
        }
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PeriodicNonuniformRationalSplineCertificateCoversNegativeAndMultipleSeams(bool rational)
    {
        var primitive=SketchCurve2d.BSpline([new(0,0),new(4,0),new(4,3),new(0,3)],
            [0,.3,.9,2.1,4],[1,1,1,1,1],2,true,rational?[1,1.2,.8,1.1]:null);
        var expression=SketchCurveExpression2d.FromCurve(primitive).Offset(.01).ContinueNativeRange(-4,8);
        var before=JsonSerializer.Serialize(expression);var result=SketchCurveRegularity2d.Inspect(expression);
        Assert.True(result.IsCertified,result.Diagnostic);Assert.Contains("principal period",result.Diagnostic);
        for(int i=0;i<=64;i++)
        {
            double u=i/64.0*4;var basis=SketchCurveExpression2d.FromCurve(primitive);
            var a=basis.Derivative(u,1,false);var b=basis.Derivative(u,2,false);
            Assert.True(result.MinimumTangentialScale<=1+.01*(a.X*b.Y-a.Y*b.X)/Math.Pow(a.Magnitude,3));
        }
        Assert.Equal(before,JsonSerializer.Serialize(expression));
    }
    [Theory]
    [InlineData(2,1,false)] [InlineData(2,1,true)] [InlineData(3,1,false)] [InlineData(3,1,true)]
    [InlineData(3,2,false)] [InlineData(3,2,true)] [InlineData(4,2,false)] [InlineData(4,2,true)]
    [InlineData(4,3,false)] [InlineData(4,3,true)] [InlineData(5,3,false)] [InlineData(5,3,true)]
    public void PeriodicAuthorControlAndKnotExpansionMatchesIndependentNativeGeometry(int degree,int multiplicity,bool rational)
    {
        int count=2*multiplicity+2;
        var points=Enumerable.Range(0,count).Select(i=>new SketchPoint2d(10*Math.Cos(i*Math.Tau/count),7*Math.Sin(i*Math.Tau/count))).ToArray();
        var primitive=SketchCurve2d.BSpline(points,[0,.3,.9,2.1,4],[multiplicity,1,multiplicity,1,multiplicity],degree,true,
            rational?Enumerable.Range(0,count).Select(i=>1+.05*(i%3)).ToArray():null);
        var expression=SketchCurveExpression2d.FromCurve(primitive).Offset(.01).ContinueNativeRange(-4,8);
        var result=SketchCurveRegularity2d.Inspect(expression);Assert.True(result.IsCertified,result.Diagnostic);
        // The source native evaluator independently checks expanded control
        // indexing,including nonuniform knots and repeated periodic seams.
        var flat=primitive.Knots.SelectMany((k,i)=>Enumerable.Repeat(k,primitive.Multiplicities[i])).ToList();
        int extra=degree+1-multiplicity;var cycle=flat.Take(flat.Count-multiplicity).ToArray();
        var expanded=Enumerable.Range(-extra,extra).Select(j=>cycle[((j%cycle.Length)+cycle.Length)%cycle.Length]+4*Math.Floor((double)j/cycle.Length)).ToList();
        expanded.AddRange(flat);
        expanded.AddRange(Enumerable.Range(multiplicity,extra).Select(j=>cycle[j%cycle.Length]+4*(1+j/cycle.Length)));
        var controls=Enumerable.Range(0,count+extra).Select(i=>points[i%count]).ToArray();
        var weights=Enumerable.Range(0,count+extra).Select(i=>rational?primitive.Weights[i%count]:1).ToArray();
        var nativeBasis=SketchCurveExpression2d.FromCurve(primitive);
        for(int i=0;i<=64;i++)
        {
            double u=i/64.0*4;int span=degree;while(span<controls.Length-1&&u>=expanded[span+1])span++;
            double Scalar(int component)
            {
                var values=Enumerable.Range(span-degree,degree+1).Select(j=>weights[j]*(component==0?controls[j].X:component==1?controls[j].Y:1)).ToArray();
                for(int r=1;r<=degree;r++)for(int j=degree;j>=r;j--)
                {
                    int k=span-degree+j;double width=expanded[k+degree-r+1]-expanded[k];
                    double a=width==0?0:(u-expanded[k])/width;values[j]=(1-a)*values[j-1]+a*values[j];
                }
                return values[^1];
            }
            var expected=new SketchPoint2d(Scalar(0)/Scalar(2),Scalar(1)/Scalar(2));
            Assert.InRange(expected.DistanceTo(nativeBasis.Evaluate(u,false).Point),0,1e-9);
            var a=nativeBasis.Derivative(u,1,false);var b=nativeBasis.Derivative(u,2,false);
            Assert.True(result.MinimumTangentialScale<=1+.01*(a.X*b.Y-a.Y*b.X)/Math.Pow(a.Magnitude,3));
        }
    }
    [Fact]
    public void InteriorC0AndSingularBasisAreUncertifiedWhileTheirFiniteSmoothPartWorks()
    {
        var spline=SketchCurve2d.BSpline([new(0,0),new(1,2),new(2,0),new(3,-2),new(4,0)],[0,.5,1],[3,2,3],2);
        var entire=SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(spline).Offset(.01));
        Assert.False(entire.IsCertified);Assert.Contains("C1",entire.Diagnostic);Assert.Equal(.5,entire.FirstParameter);
        Assert.True(SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(spline.Trim(.1,.4)).Offset(.01)).IsCertified);
        var singular=SketchCurve2d.Bezier([new(0,0),new(1,0),new(0,0)]);
        Assert.False(SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(singular).Offset(.01)).IsCertified);
    }
    [Fact]
    public void RationalDenominatorCrossingInContinuationCannotCertify()
    {
        var primitive=SketchCurve2d.Bezier([new(0,0),new(1,1)],[1,2]);
        var expression=SketchCurveExpression2d.FromCurve(primitive).Offset(.01).ContinueNativeRange(-2,1);
        Assert.False(SketchCurveRegularity2d.Inspect(expression).IsCertified);
        Assert.True(SketchCurveRegularity2d.Inspect(SketchCurveExpression2d.FromCurve(primitive).Offset(.01)).IsCertified);
    }
    [Fact]
    public void NullInputIsRejected() => Assert.Throws<ArgumentNullException>(()=>SketchCurveRegularity2d.Inspect(null!));
    [Fact]
    public void AdjacentBinary64KnotSpanCannotBeSkippedByRoundedMidpoint()
    {
        double first=Math.BitIncrement(.25),last=Math.BitIncrement(first);
        Assert.Equal(last,first+(last-first)/2);
        var primitive=SketchCurve2d.BSpline([new(0,0),new(1,0),new(1,1),new(2,1),new(2,0)],
            [0,first,last,1],[3,1,1,3],2);
        var expression=SketchCurveExpression2d.FromCurve(primitive).Offset(.01);
        var result=SketchCurveRegularity2d.Inspect(expression);
        Assert.False(result.IsCertified);
        Assert.True(result.FirstParameter<=last&&result.LastParameter>=first);
    }
    private static SketchCurve2d Parabola(double center,double scale) => SketchCurve2d.Bezier([
        new(0,scale*center*center),new(scale/2,scale*(center*center-center)),new(scale,scale*(1-center)*(1-center))]);
    private static SketchPoint2d IndependentSplinePoint(SketchCurve2d curve,double u)
    {
        var flat=curve.Knots.SelectMany((k,i)=>Enumerable.Repeat(k,curve.Multiplicities[i])).ToArray();
        int degree=curve.Degree,n=curve.Poles.Count-1,span=degree;
        while(span<n&&u>=flat[span+1])span++;
        double[] Scalar(int dimension)
        {
            var values=new double[degree+1];
            for(int j=0;j<=degree;j++)
            {
                int i=span-degree+j;double weight=curve.Weights.Count==0?1:curve.Weights[i];
                values[j]=dimension==2?weight:(dimension==0?curve.Poles[i].X:curve.Poles[i].Y)*weight;
            }
            for(int r=1;r<=degree;r++)for(int j=degree;j>=r;j--)
            {
                int i=span-degree+j;double width=flat[i+degree-r+1]-flat[i];
                double a=width==0?0:(u-flat[i])/width;values[j]=(1-a)*values[j-1]+a*values[j];
            }
            return values;
        }
        double denominator=Scalar(2)[^1];return new(Scalar(0)[^1]/denominator,Scalar(1)[^1]/denominator);
    }
}
