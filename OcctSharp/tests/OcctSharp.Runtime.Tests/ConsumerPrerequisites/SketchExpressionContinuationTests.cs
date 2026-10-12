using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionContinuationTests
{
    private static readonly int[] Degrees=[2,3,4,5],Multiplicities=[1,2];
    private static readonly bool[] RationalValues=[false,true];
    public static IEnumerable<object[]> SplineCases()=>from degree in Degrees
        from multiplicity in Multiplicities where multiplicity<degree
        from rational in RationalValues select new object[]{degree,multiplicity,rational};

    [Theory,MemberData(nameof(SplineCases))]
    public void PolarContinuationPreservesNonuniformRationalSplineSpansAndRepeatedKnots(int degree,int multiplicity,bool rational)
    {
        var points=Enumerable.Range(0,degree+multiplicity+2).Select(i=>new SketchPoint2d(2*i,2*Math.Sin(.4*i))).ToArray();
        var curve=SketchCurve2d.BSpline(points,[0,.2,.7,1],[degree+1,multiplicity,1,degree+1],degree,false,
            rational?points.Select((_,i)=>1+.03*Math.Cos(i)).ToArray():null);
        var source=SketchCurveExpression2d.FromCurve(curve).Offset(.1);var before=JsonSerializer.Serialize(source);
        var continued=source.ContinueNativeRange(-.07,1.09);
        using var edge=continued.ToEdge(SketchPlane.XY);Assert.True(edge.IsValid);
        var snapshot=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        foreach(double u in new[]{-.07,-.03,0,.13,.37,.62,.85,1,1.04,1.09})
        {
            var(reference,tangent)=DeBoor(curve,u);var target=new SketchPoint2d(reference.X+.1*tangent.Y/tangent.Magnitude,
                reference.Y-.1*tangent.X/tangent.Magnitude);
            Near(target,continued.Evaluate(u,false).Point);Near(target,snapshot.Evaluate(u,false).Point);
            if(u>0&&u<1)for(int order=1;order<=3;order++)Near(source.Derivative(u,order,false),continued.Derivative(u,order,false),1e-8);
        }
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Theory]
    [InlineData(false,.2)] [InlineData(false,-.2)] [InlineData(true,.2)] [InlineData(true,-.2)]
    public void BernsteinContinuationKeepsOriginalParameterAndExactNormalDistance(bool rational,double distance)
    {
        var leaf=SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)],rational?[1,1.3,.9]:null);
        var source=SketchCurveExpression2d.FromCurve(leaf).Offset(distance);var before=JsonSerializer.Serialize(source);
        var continued=source.ContinueNativeRange(-.15,1.15);
        Assert.Same(source,continued.Basis);Assert.Equal(SketchCurveExpressionKind.Continuation,continued.Kind);
        var plane=new SketchPlane(new(3,4,5),new(0,1,0),new(0,0,1));
        using var edge=continued.ToEdge(plane);Assert.True(edge.IsValid);
        var snapshot=SketchNativeExpressionSnapshot2d.Read(edge,plane);
        Assert.Equal(SketchNativeCurveKind.Offset,snapshot.Curve.Kind);Assert.Equal(SketchNativeCurveKind.BSpline,snapshot.Nodes[0].Kind);
        Assert.Equal(-.15,edge.GetEdgeCurveSnapshot().FirstParameter,12);Assert.Equal(1.15,edge.GetEdgeCurveSnapshot().LastParameter,12);
        foreach(double u in new[]{-.15,-.07,0,.4,1,1.09,1.15})
        {
            var value=Bernstein(u,rational);var derivative=BernsteinD1(u,rational);double speed=derivative.Magnitude;
            var expected=new SketchPoint2d(value.X+distance*derivative.Y/speed,value.Y-distance*derivative.X/speed);
            Near(expected,continued.Evaluate(u,false).Point);
            Near(expected,snapshot.Evaluate(u,false).Point);
            Assert.InRange(edge.EvaluateEdge(u).Point.DistanceTo(plane.ToWorld(expected)),0,1e-9);
            if(u>=0&&u<=1)for(int order=1;order<=3;order++)Near(source.Derivative(u,order,false),continued.Derivative(u,order,false),1e-8);
        }
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void NonuniformSplineContinuationPreservesItsTerminalPolynomialAndOriginalInterior(bool rational)
    {
        // Degree2 span at the right has a nonzero quadratic term. Continuing
        // its polynomial produces(1+4u*u,1+4u-6u*u),not its endpoint tangent.
        var curve=SketchCurve2d.BSpline([new(0,0),new(1,2),new(3,1),new(5,-1)],
            [0,.5,1],[3,1,3],2,false,rational?[1,1.2,.8,1.1]:null);
        var source=SketchCurveExpression2d.FromCurve(curve).Offset(.1);
        var continued=source.ContinueNativeRange(-.1,1.1);
        using var edge=continued.ToEdge(SketchPlane.XY);Assert.True(edge.IsValid);
        var snapshot=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        foreach(double u in new[]{0,.1,.4,.6,.9,1})
        {
            Near(source.Evaluate(u,false).Point,continued.Evaluate(u,false).Point);
            for(int order=1;order<=3;order++)Near(source.Derivative(u,order,false),continued.Derivative(u,order,false),1e-8);
        }
        foreach(double u in new[]{-.1,1.1})Near(continued.Evaluate(u,false).Point,snapshot.Evaluate(u,false).Point);
        if(!rational)
        {
            double u=1.1;
            // Independent exact right-span reference obtained by de Boor's
            // degree2 recurrence on knots[0,0,0,.5,1,1,1].
            double a=(u-.5)/.5,b=u;
            var first=new SketchPoint2d(1*(1-b)+3*b,2*(1-b)+1*b);
            var second=new SketchPoint2d(3*(1-a)+5*a,1*(1-a)-1*a);
            var point=new SketchPoint2d(first.X*(1-a)+second.X*a,first.Y*(1-a)+second.Y*a);
            var tangent=new SketchVector2d(8*u,4-12*u);
            var expected=new SketchPoint2d(point.X+.1*tangent.Y/tangent.Magnitude,point.Y-.1*tangent.X/tangent.Magnitude);
            Near(expected,continued.Evaluate(u,false).Point);
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void AnalyticContinuationRetainsExactSupportAndReversalAcrossPeriodicSeams(int kind)
    {
        var leaf=kind switch{1=>SketchCurve2d.Segment(new(2,3),new(8,11)),2=>SketchCurve2d.CircularArc(default,5,5.5,.5),
            _=>SketchCurve2d.EllipticArc(default,8,3,5.5,.5)};
        var source=SketchCurveExpression2d.FromCurve(leaf).Offset(.2).Reverse();
        var continued=source.ContinueNativeRange(source.FirstParameter-.2,source.LastParameter+.2);
        using var edge=continued.ToEdge(SketchPlane.XY);Assert.True(edge.IsValid);
        var read=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        Assert.Equal((int)leaf.Kind,(int)read.Nodes[0].Kind);
        foreach(double t in new[]{0,.3,1})Near(continued.Evaluate(t).Point,read.Evaluate(t).Point);
        var reflected=continued.Transform(SketchTransform2d.Mirror(default,SketchDirection2d.YAxis));
        foreach(double t in new[]{0,.3,1})Near(new(-continued.Evaluate(t).Point.X,continued.Evaluate(t).Point.Y),reflected.Evaluate(t).Point);
    }

    [Fact]
    public void ContinuationRejectsLossOfFiniteC1OrInvalidRangesWithoutChangingTheAuthor()
    {
        var leaf=SketchCurve2d.BSpline([new(0,0),new(1,2),new(2,0),new(3,-2),new(4,0)],[0,.5,1],[3,2,3],2).Trim(.1,.4);
        var source=SketchCurveExpression2d.FromCurve(leaf).Offset(.1);var before=JsonSerializer.Serialize(source);
        using var valid=source.ToEdge(SketchPlane.XY);Assert.True(valid.IsValid);
        var invalid=source.ContinueNativeRange(0,1);Assert.Throws<ArgumentException>(()=>invalid.ToEdge(SketchPlane.XY));
        Assert.Equal(before,JsonSerializer.Serialize(source));
        Assert.Throws<ArgumentOutOfRangeException>(()=>source.ContinueNativeRange(source.FirstParameter+.01,1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>source.ContinueNativeRange(double.NaN,1));
    }

    private static SketchPoint2d Bernstein(double u,bool rational)
    {
        double a=(1-u)*(1-u),b=2*u*(1-u)*(rational?1.3:1),c=u*u*(rational?.9:1),sum=a+b+c;
        return new((4*b+9*c)/sum,5*b/sum);
    }
    private static SketchVector2d BernsteinD1(double u,bool rational)
    {
        double b=2*u*(1-u)*(rational?1.3:1),c=u*u*(rational?.9:1),s=(1-u)*(1-u)+b+c;
        double db=2*(1-2*u)*(rational?1.3:1),dc=2*u*(rational?.9:1),ds=-2*(1-u)+db+dc;
        return new(((4*db+9*dc)*s-(4*b+9*c)*ds)/(s*s),(5*db*s-5*b*ds)/(s*s));
    }
    private static (SketchPoint2d Point,SketchVector2d Derivative) DeBoor(SketchCurve2d curve,double u)
    {
        // Independent scalar de Boor recurrence with forward automatic D1;
        // extrapolation uses the actual first/last polynomial/rational span.
        var flat=curve.Knots.SelectMany((k,i)=>Enumerable.Repeat(k,curve.Multiplicities[i])).ToArray();
        int p=curve.Degree,n=curve.Poles.Count-1,span=p;
        while(span<n&&u>=flat[span+1])span++;
        var x=new double[p+1];var y=new double[p+1];var w=new double[p+1];
        var dx=new double[p+1];var dy=new double[p+1];var dw=new double[p+1];
        for(int j=0;j<=p;j++)
        {
            int index=span-p+j;w[j]=curve.IsRational?curve.Weights[index]:1;x[j]=curve.Poles[index].X*w[j];y[j]=curve.Poles[index].Y*w[j];
        }
        for(int r=1;r<=p;r++)for(int j=p;j>=r;j--)
        {
            int index=span-p+j;double denominator=flat[index+p-r+1]-flat[index];
            double a=(u-flat[index])/denominator,da=1/denominator;
            dx[j]=dx[j-1]+a*(dx[j]-dx[j-1])+da*(x[j]-x[j-1]);
            dy[j]=dy[j-1]+a*(dy[j]-dy[j-1])+da*(y[j]-y[j-1]);
            dw[j]=dw[j-1]+a*(dw[j]-dw[j-1])+da*(w[j]-w[j-1]);
            x[j]=x[j-1]+a*(x[j]-x[j-1]);y[j]=y[j-1]+a*(y[j]-y[j-1]);w[j]=w[j-1]+a*(w[j]-w[j-1]);
        }
        return(new(x[p]/w[p],y[p]/w[p]),new((dx[p]*w[p]-x[p]*dw[p])/(w[p]*w[p]),(dy[p]*w[p]-y[p]*dw[p])/(w[p]*w[p])));
    }
    private static void Near(SketchPoint2d a,SketchPoint2d b,double tolerance=1e-9)=>Assert.InRange(a.DistanceTo(b),0,tolerance);
    private static void Near(SketchVector2d a,SketchVector2d b,double tolerance=1e-9)=>Assert.InRange(double.Hypot(a.X-b.X,a.Y-b.Y),0,tolerance);
}
