namespace OcctSharp.Runtime.Tests;
public sealed class SketchExpressionDerivativeRegressionTests
{
    [Theory]
    [InlineData(.2,false)] [InlineData(-.2,false)] [InlineData(.2,true)] [InlineData(-.2,true)]
    public void RationalSplineOffsetThirdDerivativeMatchesIndependentAnalyticSeriesAfterBrep(double distance,bool translated)
    {
        var definition=SketchCurve2d.BSpline([new(0,0),new(2,3),new(5,4),new(8,3),new(10,0)],
            [0,.4,1],[4,1,4],3,false,[1,1.2,.8,1.1,1]).Trim(.1,.9);
        var source=SketchCurveExpression2d.FromCurve(definition).Offset(distance).ContinueNativeRange(-.06,1.06);
        var plane=translated?new SketchPlane(new(100,-200,30),new(0,1,0),new(0,0,1)):SketchPlane.XY;
        string directory=Path.Combine(Path.GetTempPath(),"b18-offset-d3-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string path;using(var edge=source.ToEdge(plane))path=ShapeExchange.WriteBrep(edge,Path.Combine(directory,"exact.brep"));
            SketchNativeExpressionSnapshot2d copied;
            using(var edge=ShapeExchange.ReadBrep(path))copied=SketchNativeExpressionSnapshot2d.Read(edge,plane);
            foreach(double u in new[]{-.06,0,.13,.5,.8,1,1.06})
            {
                var expected=IndependentOffsetD3(definition,u,distance);
                var original=source.Derivative(u,3,false);var actual=copied.Derivative(u,3,false);
                Assert.InRange(double.Hypot(expected.X-original.X,expected.Y-original.Y),0,1e-8);
                Assert.InRange(double.Hypot(expected.X-actual.X,expected.Y-actual.Y),0,1e-8);
            }
            if(distance>0)
            {
                var expected=source.Derivative(-.06,3,false);
                Assert.InRange(Math.Abs(expected.X-4086.0788942318754),0,1e-8);
                Assert.InRange(Math.Abs(expected.Y-6913.58367212158),0,1e-8);
            }
        }
        finally{Directory.Delete(directory,true);}
    }
private static SketchVector2d IndependentOffsetD3(SketchCurve2d curve,double u,double offset)
{
    int p=curve.Degree,n=curve.Poles.Count-1,span=p;
    var flat=curve.Knots.SelectMany((k,i)=>Enumerable.Repeat(k,curve.Multiplicities[i])).ToArray();
    while(span<n&&u>=flat[span+1])span++;
    var x=new double[p+1,5];var y=new double[p+1,5];var w=new double[p+1,5];
    for(int j=0;j<=p;j++)
    {
        int index=span-p+j;w[j,0]=curve.IsRational?curve.Weights[index]:1;x[j,0]=curve.Poles[index].X*w[j,0];y[j,0]=curve.Poles[index].Y*w[j,0];
    }
    for(int r=1;r<=p;r++)for(int j=p;j>=r;j--)
    {
        int index=span-p+j;double den=flat[index+p-r+1]-flat[index],a=(u-flat[index])/den,da=1/den;
        for(int k=4;k>=0;k--)
        {
            x[j,k]=x[j-1,k]+a*(x[j,k]-x[j-1,k])+(k>0?da*(x[j,k-1]-x[j-1,k-1]):0);
            y[j,k]=y[j-1,k]+a*(y[j,k]-y[j-1,k])+(k>0?da*(y[j,k-1]-y[j-1,k-1]):0);
            w[j,k]=w[j-1,k]+a*(w[j,k]-w[j-1,k])+(k>0?da*(w[j,k-1]-w[j-1,k-1]):0);
        }
    }
    var px=new double[5];var py=new double[5];
    for(int k=0;k<=4;k++)
    {
        double ax=x[p,k],ay=y[p,k];for(int j=1;j<=k;j++){ax-=w[p,j]*px[k-j];ay-=w[p,j]*py[k-j];}
        px[k]=ax/w[p,0];py[k]=ay/w[p,0];
    }
    var vx=new double[4];var vy=new double[4];var speedSquared=new double[4];var speed=new double[4];var reciprocal=new double[4];
    for(int k=0;k<=3;k++){vx[k]=(k+1)*px[k+1];vy[k]=(k+1)*py[k+1];}
    for(int k=0;k<=3;k++)for(int j=0;j<=k;j++)speedSquared[k]+=vx[j]*vx[k-j]+vy[j]*vy[k-j];
    speed[0]=Math.Sqrt(speedSquared[0]);reciprocal[0]=1/speed[0];
    for(int k=1;k<=3;k++)
    {
        double remainder=speedSquared[k];for(int j=1;j<k;j++)remainder-=speed[j]*speed[k-j];speed[k]=remainder/(2*speed[0]);
        for(int j=1;j<=k;j++)reciprocal[k]-=speed[j]*reciprocal[k-j]/speed[0];
    }
    double nx=0,ny=0;for(int j=0;j<=3;j++){nx+=vy[j]*reciprocal[3-j];ny-=vx[j]*reciprocal[3-j];}
    return new(6*(px[3]+offset*nx),6*(py[3]+offset*ny));
}
}
