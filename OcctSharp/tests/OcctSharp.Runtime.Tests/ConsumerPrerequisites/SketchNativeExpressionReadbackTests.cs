using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchNativeExpressionReadbackTests
{
    public static IEnumerable<object[]> Curves()
    {
        foreach(var row in SketchCurveExpressionTests.FreeformCases())yield return row;
        foreach(double distance in new[]{.2,-.2})
        {
            yield return [SketchCurve2d.Bezier([new(0,0),new(3,5),new(9,0)],[1,1.4,.8]).Trim(.1,.9),distance];
            yield return [SketchCurve2d.BSpline([new(2,0),new(0,2),new(-2,0),new(0,-2)],
                [0,1,2,3,4],[1,1,1,1,1],2,true),distance];
        }
    }

    [Theory,MemberData(nameof(Curves))]
    public void CompleteNativeBasisSurvivesBrepReloadSourceDisposalAndOwnedEvaluation(SketchCurve2d curve,double distance)
    {
        var expression=SketchCurveExpression2d.FromCurve(curve).Offset(distance).Reverse().Trim(.1,.85);
        var plane=new SketchPlane(new(2,3,4),new(0,1,0),new(0,0,1));
        string directory=Path.Combine(Path.GetTempPath(),"b18-readback-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            SketchNativeExpressionSnapshot2d before,after;
            string path;
            using(var edge=expression.ToEdge(plane))
            {
                path=ShapeExchange.WriteBrep(edge,Path.Combine(directory,"source.brep"));
                var sourceHash=SHA256.HashData(File.ReadAllBytes(path));
                before=SketchNativeExpressionSnapshot2d.Read(edge,plane);
                var unchanged=ShapeExchange.WriteBrep(edge,Path.Combine(directory,"after-read.brep"));
                Assert.Equal(sourceHash,SHA256.HashData(File.ReadAllBytes(unchanged)));
            }
            using(var reopened=ShapeExchange.ReadBrep(path))after=SketchNativeExpressionSnapshot2d.Read(reopened,plane);
            EqualNativeValues(before,after);
            Assert.Equal(expression.FirstParameter,after.FirstParameter,12);Assert.Equal(expression.LastParameter,after.LastParameter,12);
            Assert.Equal(expression.Reversed,after.Reversed);
            Assert.Equal(SketchNativeCurveKind.Offset,after.Curve.Kind);Assert.Equal(distance,after.Curve.SignedOffset,12);
            var leaf=after.Nodes[0];Assert.Equal((int)curve.Kind,(int)leaf.Kind);
            if(curve.Kind is SketchCurveKind.Bezier or SketchCurveKind.BSpline)
            {
                Assert.Equal(curve.Degree,leaf.Degree);Assert.Equal(curve.Periodic,leaf.IsPeriodic);Assert.Equal(curve.IsRational,leaf.IsRational);
                Assert.Equal(curve.Poles.Count,leaf.Poles.Count);
                for(int i=0;i<leaf.Poles.Count;i++)Near(curve.Poles[i],leaf.Poles[i]);
                Assert.Equal(curve.IsRational?curve.Weights:Enumerable.Repeat(1.0,curve.Poles.Count),leaf.Weights);
                Assert.Equal(curve.Knots,leaf.Knots);Assert.Equal(curve.Multiplicities,leaf.Multiplicities);
            }
            else
            {
                Near(curve.Poles[0],leaf.Origin);Assert.Equal(curve.MajorRadius,leaf.MajorRadius,12);Assert.Equal(curve.MinorRadius,leaf.MinorRadius,12);
                Assert.InRange(Math.Abs(leaf.XDirection.X*leaf.YDirection.Y-leaf.XDirection.Y*leaf.YDirection.X-1),0,1e-12);
            }
            foreach(double t in new[]{0,.13,.5,.83,1})
            {
                Near(expression.Evaluate(t).Point,after.Evaluate(t).Point);
                for(int order=1;order<=3;order++)Near(expression.Derivative(t,order),after.Derivative(t,order),1e-8);
            }
            Assert.Throws<NotSupportedException>(()=>((IList<SketchNativeCurveNode2d>)after.Nodes)[0]=after.Curve);
            Assert.Throws<NotSupportedException>(()=>((IList<SketchPoint2d>)leaf.Poles).Add(default));
        }
        finally{Directory.Delete(directory,true);}
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void ReflectionAndOppositePlaneNormalKeepNativeParameterAxesAndDistance(int kind)
    {
        var curve=kind switch
        {
            1=>SketchCurve2d.Segment(new(2,3),new(7,8)).Trim(.2,.8),
            2=>SketchCurve2d.Circle(new(2,-3),5,.3),
            3=>SketchCurve2d.Ellipse(new(2,-3),8,3,.3),
            4=>SketchCurve2d.Bezier([new(0,0),new(3,4),new(9,0)],[1,1.4,.8]).Trim(.1,.9),
            _=>SketchCurve2d.BSpline([new(0,0),new(2,3),new(5,4),new(8,3),new(10,0)],[0,.4,1],[4,1,4],3,false,[1,1.2,.8,1.1,1]).Trim(.1,.9)
        };
        var expression=SketchCurveExpression2d.FromCurve(curve).Offset(.2).Reverse()
            .Transform(SketchTransform2d.Mirror(new(3,-2),SketchDirection2d.Create(2,1)));
        var plane=new SketchPlane(new(0,0,0),new(1,0,0),new(0,-1,0));
        SketchNativeExpressionSnapshot2d snapshot;
        using(var edge=expression.ToEdge(SketchPlane.XY))snapshot=SketchNativeExpressionSnapshot2d.Read(edge,plane);
        Assert.Equal(-expression.SignedOffset*(expression.Basis!.Reversed?-1:1),snapshot.Curve.SignedOffset,12);
        if(kind is 2 or 3)
        {
            Assert.True(snapshot.FirstParameter<0);
            var leaf=snapshot.Nodes[0];Assert.InRange(leaf.XDirection.X*leaf.YDirection.Y-leaf.XDirection.Y*leaf.YDirection.X,-1.000000000001,-.999999999999);
        }
        foreach(double t in new[]{0,.2,.6,1})Near(plane.ToLocal(SketchPlane.XY.ToWorld(expression.Evaluate(t).Point)),snapshot.Evaluate(t).Point);
    }

    [Fact]
    public void TrimmedNativeLineKeepsItsParameterZeroRatherThanReplacingItWithTheFiniteEndpoint()
    {
        var expression=SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(new(2,3),new(8,11))).Trim(.2,.8).Reverse();
        SketchNativeExpressionSnapshot2d snapshot;
        using(var edge=expression.ToEdge(SketchPlane.XY))snapshot=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        Assert.Equal(2,snapshot.FirstParameter,12);Assert.Equal(8,snapshot.LastParameter,12);
        Assert.True(snapshot.Reversed);Assert.Equal(SketchNativeCurveKind.Line,snapshot.Nodes[0].Kind);
        Near(new(2,3),snapshot.Nodes[0].Origin);Near(new SketchVector2d(.6,.8),snapshot.Nodes[0].XDirection);
        Near(new(6.8,9.4),snapshot.Evaluate(0).Point);Near(new(3.2,4.6),snapshot.Evaluate(1).Point);
        Near(new SketchVector2d(-.6,-.8),snapshot.Derivative(.5,1));
        Assert.Single(snapshot.Nodes);Assert.Equal(SketchNativeCurveKind.Line,snapshot.Curve.Kind);
    }

    [Fact]
    public void NativeOffsetSimplificationIsReportedWithoutInventingTheAuthorTree()
    {
        var author=SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)]))
            .Offset(.2).Reverse().Offset(.1);
        Assert.Equal(3,author.Depth);
        SketchNativeExpressionSnapshot2d snapshot;
        using(var edge=author.ToEdge(SketchPlane.XY))snapshot=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        Assert.Single(snapshot.Nodes,n=>n.Kind==SketchNativeCurveKind.Offset);
        Assert.Equal(.1,snapshot.Curve.SignedOffset,12);
        foreach(double t in new[]{0,.4,1})Near(author.Evaluate(t).Point,snapshot.Evaluate(t).Point);
        Assert.Equal(.2,author.Basis!.SignedOffset);Assert.Equal(.1,author.SignedOffset);
    }

    [Fact]
    public void DisposedWrongShapeWrongPlaneAndInvalidEvaluationAreRejected()
    {
        var expression=SketchCurveExpression2d.FromCurve(SketchCurve2d.Ellipse(default,8,3)).Offset(.2);
        using var edge=expression.ToEdge(SketchPlane.XY);
        var snapshot=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
        Assert.Throws<ArgumentException>(()=>SketchNativeExpressionSnapshot2d.Read(edge,new(new(0,0,1),new(1,0,0),new(0,1,0))));
        using var box=ShapeFactory.CreateBox(1,2,3);Assert.Throws<ArgumentException>(()=>SketchNativeExpressionSnapshot2d.Read(box,SketchPlane.XY));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>snapshot.Evaluate(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(()=>snapshot.Evaluate(-.1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>snapshot.Derivative(.5,4));
        edge.Dispose();Assert.Throws<ObjectDisposedException>(()=>SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY));
        Assert.True(double.IsFinite(snapshot.Evaluate(.5).Point.X));
        Assert.Equal(120,Marshal.SizeOf<SketchNativeNodeRaw>());Assert.Equal(32,Marshal.SizeOf<SketchNativeSummaryRaw>());
        Assert.Equal(120,Marshal.SizeOf<SketchExpressionNodeRaw>());Assert.Equal(104,Marshal.SizeOf<SketchCurveRaw>());
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int ReadRaw(nint edge,SketchPlaneRaw* plane,double tolerance,SketchNativeSummaryRaw* summary,
        SketchNativeNodeRaw* nodes,int nodeCapacity,SketchPoint2dRaw* poles,double* weights,int poleCapacity,double* knots,int* mults,int knotCapacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int EvaluateRaw(SketchNativeSummaryRaw* summary,SketchNativeNodeRaw* nodes,SketchPoint2dRaw* poles,
        double* weights,double* knots,int* mults,double parameter,int order,SketchEvaluationRaw* output);

    [Fact]
    public unsafe void NativeReadAndEvaluateRejectAliasingAndMalformedTablesWithoutWritingAnyOutput()
    {
        var expression=SketchCurveExpression2d.FromCurve(SketchCurve2d.BSpline([new(0,0),new(2,3),new(5,4),new(8,3),new(10,0)],
            [0,.4,1],[4,1,4],3,false,[1,1.2,.8,1.1,1])).Offset(.2);
        using var edge=expression.ToEdge(SketchPlane.XY);
        nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        try
        {
            var read=Marshal.GetDelegateForFunctionPointer<ReadRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_read_edge"));
            var evaluate=Marshal.GetDelegateForFunctionPointer<EvaluateRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_snapshot_evaluate"));
            nint handle=edge.Handle.DangerousGetHandle();var plane=SketchPlane.XY.ToRaw();var beforePlane=plane;
            Assert.Equal(1,read(handle,&plane,1e-9,(SketchNativeSummaryRaw*)&plane,null,0,null,null,0,null,null,0));Assert.Equal(beforePlane,plane);
            Assert.Equal(1,read(handle,&plane,1e-9,(SketchNativeSummaryRaw*)handle,null,0,null,null,0,null,null,0));
            SketchNativeSummaryRaw summary=new(){NodeCount=73};
            Assert.Equal(0,read(handle,&plane,1e-9,&summary,null,0,null,null,0,null,null,0));
            var expectedSummary=summary;
            var nodes=new SketchNativeNodeRaw[summary.NodeCount];var poles=new SketchPoint2dRaw[summary.PoleCount];
            var weights=new double[summary.PoleCount];var knots=new double[summary.KnotCount];var mults=new int[summary.KnotCount];
            fixed(SketchNativeNodeRaw* n=nodes)
            fixed(SketchPoint2dRaw* p=poles)
            fixed(double* w=weights,k=knots)
            fixed(int* m=mults)
            {
                Assert.Equal(1,read(handle,&plane,1e-9,&summary,n,1,p,w,poles.Length,k,m,knots.Length));
                Assert.Equal(expectedSummary,summary);Assert.All(nodes,row=>Assert.Equal(0,row.Kind));Assert.All(weights,row=>Assert.Equal(0,row));
                Assert.Equal(1,read(handle,&plane,1e-9,&summary,n,nodes.Length,p,(double*)p,poles.Length,k,m,knots.Length));
                Assert.Equal(expectedSummary,summary);
                Assert.Equal(0,read(handle,&plane,1e-9,&summary,n,nodes.Length,p,w,poles.Length,k,m,knots.Length));
                Assert.Equal(expectedSummary,summary);
                byte[] beforeNodes=new ReadOnlySpan<byte>(n,nodes.Length*120).ToArray();
                Assert.Equal(1,evaluate(&summary,n,p,w,k,m,.5,1,(SketchEvaluationRaw*)n));
                Assert.Equal(beforeNodes,new ReadOnlySpan<byte>(n,nodes.Length*120).ToArray());
                SketchEvaluationRaw output=new(){Parameter=73};var unchanged=output;
                n[nodes.Length-1].BasisIndex=nodes.Length-1;
                Assert.Equal(1,evaluate(&summary,n,p,w,k,m,.5,1,&output));Assert.Equal(unchanged,output);
                n[nodes.Length-1].BasisIndex=nodes.Length-2;
                double originalWeight=w[0];w[0]=double.NaN;
                Assert.Equal(1,evaluate(&summary,n,p,w,k,m,.5,1,&output));Assert.Equal(unchanged,output);w[0]=originalWeight;
                double originalKnot=k[1];k[1]=k[0];
                Assert.Equal(1,evaluate(&summary,n,p,w,k,m,.5,1,&output));Assert.Equal(unchanged,output);k[1]=originalKnot;
                summary.PoleCount=1000001;
                Assert.Equal(1,evaluate(&summary,n,p,w,k,m,.5,1,&output));Assert.Equal(unchanged,output);summary=expectedSummary;
                Assert.Equal(0,evaluate(&summary,n,p,w,k,m,.5,1,&output));Near(expression.Evaluate(.5).Point,new(output.Point.X,output.Point.Y));
            }
            Assert.True(edge.IsValid);
        }
        finally{NativeLibrary.Free(library);}
    }

    private static void Near(SketchPoint2d expected,SketchPoint2d actual,double tolerance=1e-9)=>Assert.InRange(expected.DistanceTo(actual),0,tolerance);
    private static void Near(SketchVector2d expected,SketchVector2d actual,double tolerance=1e-9)=>Assert.InRange(double.Hypot(expected.X-actual.X,expected.Y-actual.Y),0,tolerance);
    private static void EqualNativeValues(SketchNativeExpressionSnapshot2d expected,SketchNativeExpressionSnapshot2d actual)
    {
        // BRep text serializes finite bounds with decimal roundoff; verify every
        // field at the existing parameter/control-point tolerances, not JSON spelling.
        Assert.Equal(expected.FirstParameter,actual.FirstParameter,12);Assert.Equal(expected.LastParameter,actual.LastParameter,12);
        Assert.Equal(expected.Reversed,actual.Reversed);Assert.Equal(expected.Nodes.Count,actual.Nodes.Count);
        for(int i=0;i<expected.Nodes.Count;i++)
        {
            var a=expected.Nodes[i];var b=actual.Nodes[i];Assert.Equal(a.Kind,b.Kind);Assert.Equal(a.Degree,b.Degree);
            Assert.Equal(a.IsRational,b.IsRational);Assert.Equal(a.IsPeriodic,b.IsPeriodic);
            Assert.Equal(a.FirstParameter,b.FirstParameter,12);Assert.Equal(a.LastParameter,b.LastParameter,12);
            Assert.Equal(a.SignedOffset,b.SignedOffset,12);Assert.Equal(a.MajorRadius,b.MajorRadius,12);Assert.Equal(a.MinorRadius,b.MinorRadius,12);
            Near(a.Origin,b.Origin);Near(a.XDirection,b.XDirection,1e-12);Near(a.YDirection,b.YDirection,1e-12);
            Assert.Equal(a.Poles.Count,b.Poles.Count);Assert.Equal(a.Weights.Count,b.Weights.Count);Assert.Equal(a.Knots.Count,b.Knots.Count);
            for(int j=0;j<a.Poles.Count;j++){Near(a.Poles[j],b.Poles[j]);Assert.Equal(a.Weights[j],b.Weights[j],12);}
            for(int j=0;j<a.Knots.Count;j++)Assert.Equal(a.Knots[j],b.Knots[j],12);
            Assert.Equal(a.Multiplicities,b.Multiplicities);
            Assert.Equal(a.Basis is null,b.Basis is null);
            if(a.Basis is not null)Assert.Same(actual.Nodes[i-1],b.Basis);
        }
    }
}
