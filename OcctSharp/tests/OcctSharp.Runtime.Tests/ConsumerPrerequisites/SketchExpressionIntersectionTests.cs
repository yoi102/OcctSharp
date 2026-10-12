using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionIntersectionTests
{
    [Theory, MemberData(nameof(SketchCurveExpressionTests.FreeformCases),MemberType=typeof(SketchCurveExpressionTests))]
    public void OrthogonalProjectionReturnsCopiedDirectedFiniteParameters(SketchCurve2d curve,double distance)
    {
        var expression=SketchCurveExpression2d.FromCurve(curve).Offset(distance).Reverse().Trim(.1,.9);
        foreach(var transform in new[] {SketchTransform2d.Identity,
            SketchTransform2d.Mirror(new(3,-2),SketchDirection2d.Create(2,1))})
        {
            var target=expression.Transform(transform);
            var onCurve=target.Evaluate(.37);
            double length=onCurve.FirstDerivative.Magnitude;
            var query=new SketchPoint2d(onCurve.Point.X+.05*onCurve.FirstDerivative.Y/length,
                onCurve.Point.Y-.05*onCurve.FirstDerivative.X/length);
            var result=target.Project(query);
            Assert.NotEmpty(result);
            var closest=result[0];
            Assert.InRange(closest.Point.DistanceTo(onCurve.Point),0,1e-7);
            Assert.InRange(Math.Abs(closest.Distance-.05),0,1e-8);
            Assert.InRange(Math.Abs(closest.NormalizedParameter-.37),0,1e-7);
            Assert.All(result,value=>Assert.InRange(value.NativeParameter,target.FirstParameter,target.LastParameter));
            using(var edge=target.ToEdge(SketchPlane.XY))edge.Dispose();
            Assert.InRange(closest.Point.DistanceTo(target.Evaluate(closest.NativeParameter,false).Point),0,1e-8);
        }
    }

    [Fact]
    public void TangencyMultipleContactsAndOverlapIntervalsRemainDistinct()
    {
        var curve=SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(-2,0),new(0,4),new(2,0)])).Offset(.2);
        var tangent=curve.InspectIntersections(Line(new(-3,1.8),new(3,1.8)));
        Assert.Equal(1,tangent.PointCount);
        Assert.Equal(0,tangent.OverlapIntervalCount);
        Assert.Single(tangent.Endpoints);
        var crossings=curve.InspectIntersections(Line(new(-3,1),new(3,1)));
        Assert.Equal(2,crossings.PointCount);
        Assert.Equal(0,crossings.OverlapIntervalCount);
        Assert.Equal(2,crossings.Endpoints.Count);
        foreach(var second in new[]{curve,curve.Trim(.2,.8),curve.Reverse()})
        {
            var overlap=curve.InspectIntersections(second);
            Assert.True(overlap.HasOverlappingIntervals);
            Assert.Equal(0,overlap.PointCount);
            Assert.Equal(2,overlap.Endpoints.Count);
            foreach(var endpoint in overlap.Endpoints)
            {
                Assert.InRange(endpoint.Point.DistanceTo(curve.Evaluate(endpoint.FirstNativeParameter,false).Point),0,1e-7);
                Assert.InRange(endpoint.Point.DistanceTo(second.Evaluate(endpoint.SecondNativeParameter,false).Point),0,1e-7);
            }
        }
        Assert.Empty(curve.InspectSelfIntersections().Endpoints);
        Assert.False(curve.InspectSelfIntersections().HasOverlappingIntervals);
    }

    [Fact]
    public void ReversedAndMirroredEllipseIntersectionsRetainAuthorParameters()
    {
        var source=SketchCurveExpression2d.FromCurve(SketchCurve2d.EllipticArc(new(2,-3),8,3,.25,1.4)).Offset(-.2).Reverse();
        var point=source.Evaluate(.37).Point;
        var line=Line(new(point.X,-10),new(point.X,10));
        var transform=SketchTransform2d.Mirror(new(3,-2),SketchDirection2d.Create(2,1));
        foreach(var pair in new[]{(Curve:source,Line:line),(Curve:source.Transform(transform),Line:line.Transform(transform))})
        {
            var result=pair.Curve.InspectIntersections(pair.Line);
            Assert.Equal(1,result.PointCount);
            Assert.Equal(0,result.OverlapIntervalCount);
            var value=Assert.Single(result.Endpoints);
            Assert.InRange(Math.Abs(value.FirstNormalizedParameter-.37),0,1e-7);
            Assert.InRange(value.Point.DistanceTo(pair.Curve.Evaluate(value.FirstNativeParameter,false).Point),0,1e-7);
            Assert.InRange(value.Point.DistanceTo(pair.Line.Evaluate(value.SecondNativeParameter,false).Point),0,1e-7);
        }
    }

    [Fact]
    public void NegativeConicBrepBoundsPreserveFiniteAuthorDomain()
    {
        var source=SketchCurveExpression2d.FromCurve(SketchCurve2d.EllipticArc(new(2,-3),8,3,.25,1.4)).Offset(.2);
        var reflected=source.Transform(SketchTransform2d.Mirror(default,SketchDirection2d.XAxis));
        using var edge=reflected.ToEdge(SketchPlane.XY);
        var snapshot=edge.GetEdgeCurveSnapshot();
        Assert.Equal(reflected.FirstParameter,snapshot.FirstParameter,10);
        Assert.Equal(reflected.LastParameter,snapshot.LastParameter,10);
        double u=reflected.FirstParameter+.37*(reflected.LastParameter-reflected.FirstParameter);
        var actual=edge.EvaluateEdge(u).Point;
        var expected=reflected.Reverse().Evaluate(u,false).Point;
        Assert.InRange(expected.DistanceTo(new(actual.X,actual.Y)),0,1e-9);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int ProjectRaw(SketchExpressionNodeRaw* nodes,int count,SketchPoint2dRaw point,
        SketchProjectionRaw* results,int capacity,int* resultCount);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int IntersectRaw(SketchExpressionNodeRaw* first,int firstCount,
        SketchExpressionNodeRaw* second,int secondCount,double tolerance,SketchIntersectionRaw* results,
        int capacity,int* resultCount,int* pointCount,int* intervalCount);

    [Theory]
    [InlineData(false,.2)]
    [InlineData(false,-.2)]
    [InlineData(true,.2)]
    [InlineData(true,-.2)]
    public void ReflectedPeriodicEdgesKeepBothClosedVertexParametersThroughBrep(bool ellipse,double distance)
    {
        var primitive=ellipse?SketchCurve2d.Ellipse(new(2,-3),8,3):SketchCurve2d.Circle(new(2,-3),4);
        var source=SketchCurveExpression2d.FromCurve(primitive).Offset(distance)
            .Transform(SketchTransform2d.Mirror(default,SketchDirection2d.XAxis));
        string directory=Path.Combine(Path.GetTempPath(),"occtsharp-b18-periodic-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path;
            using(var edge=source.ToEdge(SketchPlane.XY))
            {
                Assert.True(edge.IsValid);
                path=ShapeExchange.WriteBrep(edge,Path.Combine(directory,"periodic.brep"));
            }
            using var restored=ShapeExchange.ReadBrep(path);
            Assert.True(restored.IsValid);
            var snapshot=restored.GetEdgeCurveSnapshot();
            Assert.Equal(source.FirstParameter,snapshot.FirstParameter,10);
            Assert.Equal(source.LastParameter,snapshot.LastParameter,10);
            Assert.InRange(snapshot.StartPoint.DistanceTo(snapshot.EndPoint),0,1e-9);
            foreach(double t in new[]{0,.17,.65,1})
            {
                double u=source.FirstParameter+t*(source.LastParameter-source.FirstParameter);
                var expected=source.Reverse().Evaluate(u,false).Point;
                var actual=restored.EvaluateEdge(u).Point;
                Assert.InRange(expected.DistanceTo(new(actual.X,actual.Y)),0,1e-9);
            }
        }
        finally{Directory.Delete(directory,true);}
    }

    [Fact]
    public unsafe void NativeResultBuffersRejectAliasesAndSmallCapacityBeforePublication()
    {
        _=OcctRuntime.Info;
        nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        try
        {
            var project=Marshal.GetDelegateForFunctionPointer<ProjectRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_project"));
            var intersect=Marshal.GetDelegateForFunctionPointer<IntersectRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_intersect"));
            SketchPoint2dRaw* poles=stackalloc SketchPoint2dRaw[]{new(-2,0),new(0,4),new(2,0)};
            SketchPoint2dRaw* linePoles=stackalloc SketchPoint2dRaw[]{new(-3,1),new(3,1)};
            SketchExpressionNodeRaw* curve=stackalloc SketchExpressionNodeRaw[2];
            curve[0]=new(){Kind=1,BasisIndex=-1,Curve=new(){Kind=4,Degree=2,PoleCount=3,FirstParameter=0,LastParameter=1,Poles=poles}};
            curve[1]=new(){Kind=2,BasisIndex=0,SignedOffset=.2,Curve=new(){FirstParameter=0,LastParameter=1}};
            SketchExpressionNodeRaw line=new(){Kind=1,BasisIndex=-1,Curve=new(){Kind=1,PoleCount=2,FirstParameter=0,LastParameter=6,Poles=linePoles}};
            var beforeCurve=new ReadOnlySpan<byte>(curve,240).ToArray();
            var beforePoles=new ReadOnlySpan<byte>(poles,48).ToArray();
            int count=81,points=82,intervals=83;
            Assert.Equal(1,project(curve,2,new(0,2),null,0,(int*)curve));
            Assert.Equal(beforeCurve,new ReadOnlySpan<byte>(curve,240).ToArray());
            Assert.Equal(1,project(curve,2,new(0,2),(SketchProjectionRaw*)poles,1,&count));
            Assert.Equal(81,count);
            Assert.Equal(beforePoles,new ReadOnlySpan<byte>(poles,48).ToArray());
            Assert.Equal(1,intersect(curve,2,&line,1,1e-7,null,0,&count,&count,&intervals));
            Assert.Equal(81,count);Assert.Equal(83,intervals);
            var sentinel=new SketchIntersectionRaw(new(91,92),93,94);
            var output=sentinel;
            Assert.Equal(1,intersect(curve,2,&line,1,1e-7,&output,1,&count,&points,&intervals));
            Assert.Equal(sentinel,output);
            Assert.Equal(81,count);Assert.Equal(82,points);Assert.Equal(83,intervals);
            Assert.Equal(1,intersect(curve,2,&line,1,1e-7,null,0,(int*)linePoles,&points,&intervals));
            Assert.Equal(new(-3,1),linePoles[0]);
            Assert.Equal(0,intersect(curve,2,&line,1,1e-7,null,0,&count,&points,&intervals));
            Assert.Equal(2,count);Assert.Equal(2,points);Assert.Equal(0,intervals);
        }
        finally { NativeLibrary.Free(library); }
    }

    [Fact]
    public void InvalidPublicInputsLeaveAnExistingResultUsable()
    {
        var curve=Line(new(0,0),new(10,0)).Offset(.2);
        var result=Assert.Single(curve.Project(new(3,2)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>curve.Project(new(double.NaN,1)));
        Assert.Throws<ArgumentNullException>(()=>curve.InspectIntersections(null!));
        Assert.Throws<ArgumentOutOfRangeException>(()=>curve.InspectIntersections(curve,0));
        Assert.Equal(result,Assert.Single(curve.Project(new(3,2))));
    }

    private static SketchCurveExpression2d Line(SketchPoint2d a,SketchPoint2d b)=>
        SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(a,b));
}
