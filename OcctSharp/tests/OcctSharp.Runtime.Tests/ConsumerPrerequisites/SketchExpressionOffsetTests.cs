using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionOffsetTests
{
    private static readonly bool[] Directions=[false,true];
    private static readonly double[] Distances=[.2,-.2];
    private static readonly int[] Kinds=[1,2,3];
    private static readonly int[] SourceIndices=[0,1];
    public static IEnumerable<object[]> Corners()=>from policy in Enum.GetValues<SketchExpressionJoin>()
        from kind in Kinds from distance in Distances from reverse in Directions select new object[]{policy,kind,distance,reverse};

    [Theory,MemberData(nameof(Corners))]
    public void FreeformCornerKeepsExactOffsetAndBridgeContractsAcrossReverseAndBrep(SketchExpressionJoin policy,int kind,double distance,bool reverse)
    {
        var leaf=kind switch
        {
            1=>SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)]),
            2=>SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)],[1,1.2,.9]),
            _=>SketchCurve2d.BSpline([new(0,0),new(2,1),new(5,.5),new(8,0)],[0,.5,1],[3,1,3],2)
        };
        var source=SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(leaf),Line(new(8,0),new(8,8))]);
        if(reverse)source=source.Reverse();var before=JsonSerializer.Serialize(source);
        var result=SketchExpressionOffset2d.Create(source,reverse?-distance:distance,policy);
        bool bridge=policy!=SketchExpressionJoin.Miter&&distance>0;
        Assert.Equal(bridge?3:2,result.Segments.Count);Assert.False(result.Chain.IsClosed);
        Assert.Equal(SourceIndices,result.Segments.Where(s=>s.Kind==SketchExpressionOffsetSegmentKind.Parallel).Select(s=>s.SourceIndices.Single()));
        foreach(var segment in result.Segments.Where(s=>s.Kind==SketchExpressionOffsetSegmentKind.Parallel))
        {
            using var edge=segment.Curve.ToEdge(SketchPlane.XY);Assert.True(edge.IsValid);
            Assert.Equal(CurveGeometryType.OffsetCurve,edge.GetEdgeCurveSnapshot().CurveType);
            var native=SketchNativeExpressionSnapshot2d.Read(edge,SketchPlane.XY);
            foreach(double t in new[]{0,.25,.7,1})Near(segment.Curve.Evaluate(t).Point,native.Evaluate(t).Point);
        }
        if(bridge)
        {
            var joined=result.Segments.Single(s=>s.Kind!=SketchExpressionOffsetSegmentKind.Parallel);
            Assert.Equal(SourceIndices,joined.SourceIndices);
            Assert.Equal(policy==SketchExpressionJoin.Round?SketchCurveKind.Circle:SketchCurveKind.Segment,joined.Curve.Primitive!.Kind);
            if(policy==SketchExpressionJoin.Round)
            {
                Assert.Equal(Math.Abs(distance),joined.Curve.Primitive.MajorRadius,12);
                int index=result.Segments.ToList().IndexOf(joined);
                Tangent(result.Segments[index-1].Curve.Derivative(1,1),joined.Curve.Derivative(0,1));
                Tangent(joined.Curve.Derivative(1,1),result.Segments[index+1].Curve.Derivative(0,1));
            }
        }
        else if(policy==SketchExpressionJoin.Miter&&distance>0)
        {
            var curve=result.Segments[reverse?1:0].Curve;
            Assert.Equal(SketchCurveExpressionKind.Continuation,curve.Kind);
            Assert.True(curve.LastParameter>1);
            Assert.NotNull(curve.Basis!.Basis!.Primitive);
        }
        string directory=Path.Combine(Path.GetTempPath(),"b18-offset-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string path;using(var wire=result.Chain.BuildWire(SketchPlane.XZ)){Assert.True(wire.IsValid);path=ShapeExchange.WriteBrep(wire,Path.Combine(directory,"offset.brep"));}
            using var reopened=ShapeExchange.ReadBrep(path);Assert.True(reopened.IsValid);
            Assert.Equal(result.Segments.Count,reopened.GetTopologySummary().UniqueCounts.EdgeCount);
        }
        finally{Directory.Delete(directory,true);}
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }

    public static IEnumerable<object[]> Rectangles()=>from policy in Enum.GetValues<SketchExpressionJoin>()
        from distance in Distances from reverse in Directions select new object[]{policy,distance,reverse};
    [Theory,MemberData(nameof(Rectangles))]
    public void ClosedRectanglePreservesB17FormulaAndWindingIndependentDistance(SketchExpressionJoin policy,double distance,bool reverse)
    {
        var chain=SketchExpressionChain2d.CreateOrdered([Line(new(0,0),new(20,0)),Line(new(20,0),new(20,12)),
            Line(new(20,12),new(0,12)),Line(new(0,12),new(0,0))],true);
        if(reverse)chain=chain.Reverse();var before=JsonSerializer.Serialize(chain);
        var result=SketchExpressionOffset2d.CreateClosed(chain,distance,policy);
        double expected=240+64*distance+(distance<0||policy==SketchExpressionJoin.Miter?4:policy==SketchExpressionJoin.Round?Math.PI:2)*distance*distance;
        Assert.InRange(Math.Abs(Math.Abs(result.Chain.Measure().SignedArea)-expected),0,1e-8);
        using var face=SketchExpressionProfile2d.Create(result.Chain).CreateFace(SketchPlane.XY);Assert.True(face.IsValid);
        Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-expected),0,1e-8);
        Assert.Equal(before,JsonSerializer.Serialize(chain));
    }

    [Fact]
    public void CollapseMiterLimitAndInvalidArgumentsPublishNoPartialAuthorResult()
    {
        var circle=SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(default,5))],true);
        var before=JsonSerializer.Serialize(circle);
        Assert.Throws<SketchValidationException>(()=>SketchExpressionOffset2d.CreateClosed(circle,-6));
        Assert.Equal(before,JsonSerializer.Serialize(circle));
        var corner=SketchExpressionChain2d.CreateOrdered([Line(new(0,0),new(10,0)),Line(new(10,0),new(0,.01))]);
        var error=Assert.Throws<SketchValidationException>(()=>SketchExpressionOffset2d.Create(corner,1));
        Assert.All(error.Diagnostics,item=>{Assert.Equal(0,item.FirstCurveIndex);Assert.Equal(1,item.SecondCurveIndex);Assert.Equal(SketchDiagnosticCode.OffsetCornerFailure,item.Code);});
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionOffset2d.Create(circle,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionOffset2d.Create(circle,1,miterLimit:21));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionOffset2d.Create(circle,1,(SketchExpressionJoin)99));
        var open=SketchExpressionChain2d.CreateOrdered([Line(default,new(1,0))]);
        Assert.Throws<ArgumentException>(()=>SketchExpressionOffset2d.CreateClosed(open,1));
    }
    public static IEnumerable<object[]> MixedProfiles()=>from policy in Enum.GetValues<SketchExpressionJoin>()
        from kind in Kinds from distance in Distances select new object[]{policy,kind,distance};
    [Theory,MemberData(nameof(MixedProfiles))]
    public void ClosedFreeformOffsetBuildsExactFaceAndSolidWithoutChangingAuthorData(SketchExpressionJoin policy,int kind,double distance)
    {
        var leaf=kind switch
        {
            1=>SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)]),
            2=>SketchCurve2d.Bezier([new(0,0),new(4,1),new(8,0)],[1,1.2,.9]),
            _=>SketchCurve2d.BSpline([new(0,0),new(2,1),new(5,.5),new(8,0)],[0,.5,1],[3,1,3],2)
        };
        var source=SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(leaf),Line(new(8,0),new(8,-5)),
            Line(new(8,-5),new(0,-5)),Line(new(0,-5),new(0,0))],true);
        var before=JsonSerializer.Serialize(source);var offset=SketchExpressionOffset2d.CreateClosed(source,distance,policy);
        var profile=SketchExpressionProfile2d.Create(offset.Chain);
        double area=Math.Abs(offset.Chain.Measure().SignedArea);
        var plane=new SketchPlane(new(3,4,5),new(0,1,0),new(0,0,1));
        using var face=profile.CreateFace(plane);Assert.True(face.IsValid);
        Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-area),0,1e-8);
        using var solid=profile.Extrude(plane,3);Assert.True(solid.IsValid);
        Assert.InRange(Math.Abs(solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass-3*area),0,1e-7);
        var edges=face.GetSubShapes(ShapeKind.Edge);
        try
        {
            Assert.Equal(4,edges.Count(e=>e.GetEdgeCurveSnapshot().CurveType==CurveGeometryType.OffsetCurve));
            foreach(var edge in edges)
            {
                var pcurve=edge.GetPcurveSnapshot(face);
                foreach(double t in new[]{0,.17,.53,.91,1})
                {
                    // Use the actual copied bounds for exact endpoint samples;
                    // reconstructing first+(last-first) can overshoot by one ulp.
                    double u=t==0?pcurve.FirstParameter:t==1?pcurve.LastParameter:
                        pcurve.FirstParameter+t*(pcurve.LastParameter-pcurve.FirstParameter);
                    Assert.InRange(u,pcurve.FirstParameter,pcurve.LastParameter);
                    var uv=edge.EvaluatePcurve(face,u).Point;
                    Assert.InRange(face.EvaluateFace(uv.X,uv.Y).Point.DistanceTo(edge.EvaluateEdge(u).Point),0,1e-9);
                }
            }
        }
        finally{foreach(var edge in edges)edge.Dispose();}
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    private static SketchCurveExpression2d Line(SketchPoint2d a,SketchPoint2d b)=>SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(a,b));
    private static void Near(SketchPoint2d a,SketchPoint2d b)=>Assert.InRange(a.DistanceTo(b),0,1e-9);
    private static void Tangent(SketchVector2d a,SketchVector2d b)
    {double product=a.Magnitude*b.Magnitude;Assert.InRange(Math.Abs((a.X*b.Y-a.Y*b.X)/product),0,1e-9);Assert.True(a.X*b.X+a.Y*b.Y>0);}
}
