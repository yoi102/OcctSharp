using System.Runtime.InteropServices;
using System.Text.Json;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchCurveExpressionTests
{
    public static IEnumerable<object[]> FreeformCases()
    {
        var curves = new[]
        {
            SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)]).Trim(.1,.9),
            SketchCurve2d.Bezier([new(0,0),new(2,4),new(7,5),new(10,0)]).Trim(.1,.9),
            SketchCurve2d.BSpline([new(0,0),new(2,3),new(5,4),new(8,3),new(10,0)],
                [0,.4,1],[4,1,4],3,false,[1,1.2,.8,1.1,1]).Trim(.1,.9),
            SketchCurve2d.EllipticArc(new(2,-3),8,3,.25,1.4),
            SketchCurve2d.EllipticArc(new(2,-3),8,3,5.7,1.1)
        };
        foreach (var curve in curves)
            foreach (double distance in new[] { .2, -.2 })
                yield return [curve, distance];
    }

    [Theory, MemberData(nameof(FreeformCases))]
    public void SignedOffsetAndFiniteEdgeRetainExactGeometry(SketchCurve2d curve, double distance)
    {
        string before = JsonSerializer.Serialize(curve);
        var expression = SketchCurveExpression2d.FromCurve(curve).Offset(distance);
        Assert.Equal(SketchCurveExpressionKind.Offset, expression.Kind);
        Assert.Same(curve, expression.Basis!.Primitive);
        var plane = new SketchPlane(new(2,3,4), new(0,1,0), new(0,0,1));
        using var edge = expression.ToEdge(plane);
        Assert.True(edge.IsValid);
        var snapshot = edge.GetEdgeCurveSnapshot();
        Assert.Equal(CurveGeometryType.OffsetCurve, snapshot.CurveType);
        Assert.Equal(curve.FirstParameter, snapshot.FirstParameter, 10);
        Assert.Equal(curve.LastParameter, snapshot.LastParameter, 10);
        foreach (double t in new[] { 0, .13, .5, .83, 1 })
        {
            var basis = curve.Evaluate(t);
            double length = basis.FirstDerivative.Magnitude;
            var expected = new SketchPoint2d(
                basis.Point.X + distance * basis.FirstDerivative.Y / length,
                basis.Point.Y - distance * basis.FirstDerivative.X / length);
            var evaluation = expression.Evaluate(t);
            Near(expected, evaluation.Point);
            var world = plane.ToWorld(expected);
            var actual = edge.EvaluateEdge(evaluation.NativeParameter).Point;
            Assert.InRange(world.DistanceTo(actual), 0, 1e-9);
        }
        Assert.Equal(before, JsonSerializer.Serialize(curve));
    }

    [Theory, MemberData(nameof(FreeformCases))]
    public void ReversalTrimAndNestedOffsetsRespectDirectedBasis(SketchCurve2d curve, double distance)
    {
        var original = SketchCurveExpression2d.FromCurve(curve).Offset(distance);
        var reversed = original.Reverse();
        var trim = reversed.Trim(.2,.8);
        var opposite = SketchCurveExpression2d.FromCurve(curve.Reverse()).Offset(distance);
        var expectedOpposite = SketchCurveExpression2d.FromCurve(curve).Offset(-distance).Reverse();
        var nested = original.Offset(.1);
        var combined = SketchCurveExpression2d.FromCurve(curve).Offset(distance + .1);
        foreach (double t in new[] { 0, .3, .65, 1 })
        {
            Near(original.Evaluate(1-t).Point, reversed.Evaluate(t).Point);
            Near(reversed.Evaluate(.2 + .6*t).Point, trim.Evaluate(t).Point);
            Near(expectedOpposite.Evaluate(t).Point, opposite.Evaluate(t).Point);
            Near(combined.Evaluate(t).Point, nested.Evaluate(t).Point);
        }
        // Reverse changes traversal, not the offset geometry. A later offset
        // follows the newly directed basis and therefore changes its normal.
        var nestedReversed = reversed.Offset(.1);
        var combinedReversed = SketchCurveExpression2d.FromCurve(curve).Offset(distance-.1).Reverse();
        Near(combinedReversed.Evaluate(.37).Point, nestedReversed.Evaluate(.37).Point);
    }

    [Theory, MemberData(nameof(FreeformCases))]
    public void SimilarityTransformsCommuteWithOwnedOffsetGeometry(SketchCurve2d curve, double distance)
    {
        var source = SketchCurveExpression2d.FromCurve(curve).Offset(distance).Reverse().Trim(.1,.85).Offset(.07);
        foreach (var transform in new[]
        {
            SketchTransform2d.Translation(13,-7),
            SketchTransform2d.Rotation(.73,new(4,-2)),
            SketchTransform2d.Scale(2.3,new(3,1)),
            SketchTransform2d.Scale(-1.7),
            SketchTransform2d.Mirror(new(3,-2),SketchDirection2d.Create(2,1))
        })
        {
            var transformed = source.Transform(transform);
            foreach (double t in new[] { 0, .25, .7, 1 })
                Near(transform.Apply(source.Evaluate(t).Point), transformed.Evaluate(t).Point, 2e-9);
        }
    }

    [Fact]
    public void PrimitiveAndScaledSegmentKeepTheirOriginalParameterContracts()
    {
        var curve = SketchCurve2d.Segment(new(1,2),new(8,4)).Reverse().Trim(.1,.8);
        var primitive = SketchCurveExpression2d.FromCurve(curve);
        foreach (double t in new[] { 0, .2, .6, 1 })
            Assert.Equal(curve.Evaluate(t), primitive.Evaluate(t));
        var offset = primitive.Offset(.3);
        var transform = SketchTransform2d.Scale(2);
        var result = offset.Transform(transform);
        Assert.Equal(offset.FirstParameter * 2, result.FirstParameter);
        Assert.Equal(offset.LastParameter * 2, result.LastParameter);
        foreach (double t in new[] { 0, .2, .6, 1 })
            Near(transform.Apply(offset.Evaluate(t).Point), result.Evaluate(t).Point);
    }

    [Fact]
    public void CircleDerivativesUseExactDirectedNativeParameterUnits()
    {
        double radius = 4.3;
        var expression = SketchCurveExpression2d.FromCurve(SketchCurve2d.CircularArc(new(1,2),4,.2,1.6)).Offset(.3);
        double u = .71;
        var expected = new[]
        {
            new SketchVector2d(-radius*Math.Sin(u),radius*Math.Cos(u)),
            new SketchVector2d(-radius*Math.Cos(u),-radius*Math.Sin(u)),
            new SketchVector2d(radius*Math.Sin(u),-radius*Math.Cos(u))
        };
        double normalized = (u-expression.FirstParameter)/(expression.LastParameter-expression.FirstParameter);
        for (int order=1;order<=3;++order)
        {
            Near(expected[order-1], expression.Derivative(u,order,false));
            var reversed = expression.Reverse().Derivative(1-normalized,order);
            double sign = order % 2 == 0 ? 1 : -1;
            Near(new(expected[order-1].X*sign,expected[order-1].Y*sign),reversed);
        }
        Near(expression.Evaluate(normalized).FirstDerivative, expression.Derivative(normalized,1));
    }

    [Fact]
    public void QuadraticOffsetFirstDerivativeMatchesAnalyticalNormalDerivative()
    {
        var expression = SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)])).Offset(.2);
        foreach (double u in new[] { .1, .35, .7, .9 })
        {
            double dx=8+2*u,dy=10-20*u,ddx=2,ddy=-20;
            double speed=double.Hypot(dx,dy),speedDerivative=(dx*ddx+dy*ddy)/speed;
            var expected = new SketchVector2d(
                dx+.2*(ddy/speed-dy*speedDerivative/(speed*speed)),
                dy-.2*(ddx/speed-dx*speedDerivative/(speed*speed)));
            Near(expected,expression.Derivative(u,1));
        }
    }

    [Fact]
    public void FiniteSmoothPartOfC0SplineWorksButAnInteriorC0KnotIsRejected()
    {
        var basis = SketchCurve2d.BSpline([new(0,0),new(1,2),new(2,0),new(3,-2),new(4,0)],
            [0,.5,1],[3,2,3],2);
        var smooth = SketchCurveExpression2d.FromCurve(basis.Trim(.1,.4)).Offset(.1);
        using var edge = smooth.ToEdge(SketchPlane.XY);
        Assert.True(edge.IsValid);
        Assert.True(double.IsFinite(smooth.Evaluate(.5).Point.X));
        var invalid = SketchCurveExpression2d.FromCurve(basis).Offset(.1);
        Assert.Throws<ArgumentException>(() => invalid.Evaluate(.3));
        Assert.Throws<ArgumentException>(() => invalid.ToEdge(SketchPlane.XY));
        Assert.True(edge.IsValid);
        Assert.True(double.IsFinite(smooth.Evaluate(.5).Point.X));
    }

    [Fact]
    public void OffsetBrepRoundTripPreservesNativeTypeBoundsAndIndependentLifetime()
    {
        var expression = SketchCurveExpression2d.FromCurve(SketchCurve2d.EllipticArc(new(2,-3),8,3,.25,1.4)).Offset(-.2).Reverse();
        string directory = Path.Combine(Path.GetTempPath(),"occtsharp-b18-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path;
            using (var source = expression.ToEdge(SketchPlane.XY))
                path = ShapeExchange.WriteBrep(source,Path.Combine(directory,"offset.brep"));
            using var restored = ShapeExchange.ReadBrep(path);
            var snapshot = restored.GetEdgeCurveSnapshot();
            Assert.Equal(CurveGeometryType.OffsetCurve,snapshot.CurveType);
            Assert.Equal(expression.FirstParameter,snapshot.FirstParameter,10);
            Assert.Equal(expression.LastParameter,snapshot.LastParameter,10);
            double native = expression.FirstParameter + .37*(expression.LastParameter-expression.FirstParameter);
            var expected = expression.Reverse().Evaluate(native,false).Point;
            var actual = restored.EvaluateEdge(native).Point;
            Near(expected,new(actual.X,actual.Y));
            Assert.True(restored.IsValid);
        }
        finally { Directory.Delete(directory,true); }
    }

    [Fact]
    public void OwnedTablesAndResourceBoundAreImmutableAndErrorsDoNotMutateTheSource()
    {
        SketchPoint2d[] poles = [new(0,0),new(4,5),new(9,0)];
        var basis = SketchCurve2d.Bezier(poles);
        var expression = SketchCurveExpression2d.FromCurve(basis).Offset(.2);
        var before = expression.Evaluate(.37);
        poles[1]=new(100,-100);
        Assert.Equal(before,expression.Evaluate(.37));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Offset(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Trim(.8,.2));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Evaluate(1.1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>expression.Derivative(.5,4));
        Assert.Throws<ArgumentNullException>(()=>expression.ToEdge(null!));
        Assert.Equal(before,expression.Evaluate(.37));
        var deep = SketchCurveExpression2d.FromCurve(basis);
        for (int i=1;i<SketchCurveExpression2d.MaximumDepth;++i) deep=deep.Offset(.001);
        Assert.Equal(64,deep.Depth);
        Assert.Throws<ArgumentException>(()=>deep.Offset(.001));
        Assert.True(double.IsFinite(deep.Evaluate(.5).Point.X));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int EvaluateRaw(SketchExpressionNodeRaw* nodes,int count,double parameter,SketchEvaluationRaw* output);

    [Fact]
    public unsafe void NativeLayoutAndAliasRejectionsPreserveInputsAndFailureOutputs()
    {
        Assert.Equal(104,Marshal.SizeOf<SketchCurveRaw>());
        Assert.Equal(120,Marshal.SizeOf<SketchExpressionNodeRaw>());
        Assert.Equal(16,Marshal.OffsetOf<SketchExpressionNodeRaw>(nameof(SketchExpressionNodeRaw.Curve)).ToInt32());
        _ = OcctRuntime.Info;
        nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        try
        {
            var evaluate=Marshal.GetDelegateForFunctionPointer<EvaluateRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_evaluate"));
            SketchPoint2dRaw* poles=stackalloc SketchPoint2dRaw[] {new(0,0),new(4,5),new(9,0)};
            SketchExpressionNodeRaw* nodes=stackalloc SketchExpressionNodeRaw[2];
            nodes[0]=new(){Kind=1,BasisIndex=-1,Curve=new(){Kind=4,Degree=2,PoleCount=3,FirstParameter=0,LastParameter=1,Poles=poles}};
            nodes[1]=new(){Kind=2,BasisIndex=0,SignedOffset=.2,Curve=new(){FirstParameter=0,LastParameter=1}};
            byte[] beforeNodes=new ReadOnlySpan<byte>(nodes,240).ToArray();
            byte[] beforePoles=new ReadOnlySpan<byte>(poles,48).ToArray();
            Assert.Equal(1,evaluate(nodes,2,.5,(SketchEvaluationRaw*)nodes));
            Assert.Equal(beforeNodes,new ReadOnlySpan<byte>(nodes,240).ToArray());
            Assert.Equal(1,evaluate(nodes,2,.5,(SketchEvaluationRaw*)poles));
            Assert.Equal(beforePoles,new ReadOnlySpan<byte>(poles,48).ToArray());
            SketchEvaluationRaw sentinel=new(new(91,92),new(93,94),95);
            var output=sentinel;
            nodes[1].BasisIndex=1;
            Assert.Equal(1,evaluate(nodes,2,.5,&output));
            Assert.Equal(sentinel,output);
            nodes[1].BasisIndex=0;
            Assert.Equal(0,evaluate(nodes,2,.5,&output));
            Assert.NotEqual(sentinel,output);
        }
        finally { NativeLibrary.Free(library); }
    }

    private static void Near(SketchPoint2d expected,SketchPoint2d actual,double tolerance=1e-9) =>
        Assert.InRange(expected.DistanceTo(actual),0,tolerance);
    private static void Near(SketchVector2d expected,SketchVector2d actual,double tolerance=1e-9) =>
        Assert.InRange(double.Hypot(expected.X-actual.X,expected.Y-actual.Y),0,tolerance);
}
