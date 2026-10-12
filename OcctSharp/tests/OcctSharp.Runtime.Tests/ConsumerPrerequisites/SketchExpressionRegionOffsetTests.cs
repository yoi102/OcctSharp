using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionRegionOffsetTests
{
    private static readonly bool[] Directions=[false,true];
    private static readonly int[] Kinds=[1,2,3];
    private static readonly double[] Distances=[.2,-.2];
    private static readonly int[] CircleDepths=[2,0,3,1,0];
    private static readonly int[] MixedDepths=[0,1,2,3];
    public static IEnumerable<object[]> CircleCases()=>from policy in Enum.GetValues<SketchExpressionJoin>()
        from distance in Distances from reversed in Directions select new object[]{policy,distance,reversed};
    [Theory,MemberData(nameof(CircleCases))]
    public void FourLevelHolesIslandsAndSeparateRootKeepAuthorOrderAndIndependentArea(SketchExpressionJoin join,double distance,bool reversed)
    {
        var inputs=new[]{Circle(default,8),Circle(default,20),Circle(default,3),Circle(default,14),Circle(new(60,0),6)};
        if(reversed)inputs=inputs.Select(loop=>loop.Reverse()).ToArray();
        var originalInputs=JsonSerializer.Serialize(inputs);var source=SketchExpressionRegion2d.Classify(inputs);
        var sourceBefore=JsonSerializer.Serialize(source);
        Assert.Equal(new[]{3,-1,0,1,-1},source.ParentIndices);Assert.Equal(CircleDepths,source.Depths);
        var result=SketchExpressionRegionOffset2d.Create(source,distance,join);
        Assert.Equal(source.ParentIndices,result.Region.ParentIndices);Assert.Equal(source.Depths,result.Region.Depths);
        Assert.Equal(Enumerable.Range(0,5),result.Loops.Select(loop=>loop.SourceLoopIndex));
        Assert.Equal(new[]{distance,distance,-distance,-distance,distance},result.Loops.Select(loop=>loop.OutwardLoopDistance));
        double expected=Math.PI*(Sq(20+distance)-Sq(14-distance)+Sq(8+distance)-Sq(3-distance)+Sq(6+distance));
        Assert.InRange(Math.Abs(result.FilledArea-expected),0,1e-8);
        var plane=new SketchPlane(new(100,-200,30),new(0,1,0),new(0,0,1));
        double area=0,volume=0;var faces=result.Region.CreateFaces(plane);
        try
        {
            Assert.Equal(3,faces.Length);
            foreach(var face in faces){Assert.True(face.IsValid);area+=face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass;}
            foreach(var profile in result.Region.Profiles){using var solid=profile.Extrude(plane,3);Assert.True(solid.IsValid);volume+=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass;}
        }
        finally{foreach(var face in faces)face.Dispose();}
        Assert.InRange(Math.Abs(area-expected),0,1e-8);Assert.InRange(Math.Abs(volume-3*expected),0,1e-7);
        foreach(var loop in result.Loops)
        {
            Assert.Single(loop.Offset.Segments);Assert.Equal(0,loop.Offset.Segments[0].SourceIndices.Single());
            using var edge=loop.Offset.Segments[0].Curve.ToEdge(plane);Assert.Equal(CurveGeometryType.OffsetCurve,edge.GetEdgeCurveSnapshot().CurveType);
        }
        Assert.Equal(sourceBefore,JsonSerializer.Serialize(source));Assert.Equal(originalInputs,JsonSerializer.Serialize(inputs));
    }
    public static IEnumerable<object[]> MixedCases()=>from policy in Enum.GetValues<SketchExpressionJoin>()
        from kind in Kinds from distance in Distances select new object[]{policy,kind,distance};
    [Theory,MemberData(nameof(MixedCases))]
    public void MixedFreeformRegionPreservesNestedTopologyAndNativeGeometryAfterBrep(SketchExpressionJoin join,int kind,double distance)
    {
        var source=SketchExpressionRegion2d.Classify([FreeformBox(kind,0,0,40,30),FreeformBox(kind,8,-5,24,20).Reverse(),
            FreeformBox(kind,14,-10,12,12),Circle(new(20,-16),2).Reverse()]);
        var before=JsonSerializer.Serialize(source);var result=SketchExpressionRegionOffset2d.Create(source,distance,join);
        Assert.Equal(new[]{-1,0,1,2},result.Region.ParentIndices);Assert.Equal(MixedDepths,result.Region.Depths);
        var directory=Path.Combine(Path.GetTempPath(),"b18-region-offset-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            double area=0,restoredArea=0,volume=0,restoredVolume=0;
            for(int i=0;i<result.Region.Profiles.Count;i++)
            {
                var profile=result.Region.Profiles[i];string facePath,solidPath;
                using(var face=profile.CreateFace(SketchPlane.XZ))
                {
                    Assert.True(face.IsValid);area+=face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass;
                    facePath=ShapeExchange.WriteBrep(face,Path.Combine(directory,$"face-{i}.brep"));
                    var edges=face.GetSubShapes(ShapeKind.Edge);
                    try
                    {
                        Assert.Contains(edges,e=>e.GetEdgeCurveSnapshot().CurveType==CurveGeometryType.OffsetCurve);
                        foreach(var edge in edges)
                        {
                            var pcurve=edge.GetPcurveSnapshot(face);
                            foreach(double t in new[]{0,.17,.53,.91,1})
                            {
                                double u=pcurve.FirstParameter+t*(pcurve.LastParameter-pcurve.FirstParameter);
                                var uv=edge.EvaluatePcurve(face,u).Point;
                                Assert.InRange(face.EvaluateFace(uv.X,uv.Y).Point.DistanceTo(edge.EvaluateEdge(u).Point),0,1e-9);
                            }
                        }
                    }
                    finally{foreach(var edge in edges)edge.Dispose();}
                }
                using(var solid=profile.Extrude(SketchPlane.XZ,3))
                {Assert.True(solid.IsValid);volume+=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass;solidPath=ShapeExchange.WriteBrep(solid,Path.Combine(directory,$"solid-{i}.brep"));}
                using(var face=ShapeExchange.ReadBrep(facePath)){Assert.True(face.IsValid);restoredArea+=face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass;}
                using(var solid=ShapeExchange.ReadBrep(solidPath)){Assert.True(solid.IsValid);restoredVolume+=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass;}
            }
            Assert.InRange(Math.Abs(area-result.FilledArea),0,1e-8);Assert.InRange(Math.Abs(restoredArea-result.FilledArea),0,1e-8);
            Assert.InRange(Math.Abs(volume-3*result.FilledArea),0,1e-7);Assert.InRange(Math.Abs(restoredVolume-3*result.FilledArea),0,1e-7);
            foreach(var loop in result.Loops)
                Assert.Equal(Enumerable.Range(0,source.Loops[loop.SourceLoopIndex].Curves.Count),
                    loop.Offset.Segments.Where(s=>s.Kind==SketchExpressionOffsetSegmentKind.Parallel).Select(s=>s.SourceIndices.Single()));
        }
        finally{Directory.Delete(directory,true);}
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Theory]
    [InlineData(1.0)] [InlineData(1.2)]
    public void MultipleRootsTouchOrMergeRefusesWithoutDroppingOrRebindingContours(double distance)
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,5),Circle(new(12,0),5)]);
        var before=JsonSerializer.Serialize(source);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,distance));
        Assert.Equal(SketchExpressionRegionOffsetFailure.ResultBoundary,error.Failure);
        Assert.Equal(0,error.FirstLoopIndex);Assert.Equal(1,error.SecondLoopIndex);
        Assert.Empty(error.CurveDiagnostics);
        Assert.Equal(before,JsonSerializer.Serialize(source));
        var valid=SketchExpressionRegionOffset2d.Create(source,.2);Assert.Equal(2,valid.Region.Profiles.Count);
    }
    [Theory]
    [InlineData(-1.0)] [InlineData(-1.2)]
    public void HoleMeetsOuterBoundaryRefusesAtomicallyWithSourceLoopIndices(double distance)
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,10),Circle(new(6,0),2)]);var before=JsonSerializer.Serialize(source);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,distance));
        Assert.Equal(SketchExpressionRegionOffsetFailure.ResultBoundary,error.Failure);Assert.Equal(0,error.FirstLoopIndex);Assert.Equal(1,error.SecondLoopIndex);
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Fact]
    public void FailedLaterHoleOffsetKeepsEarlierLoopsAndCurveDiagnostics()
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,10),Circle(default,1)]);var before=JsonSerializer.Serialize(source);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,1.2));
        Assert.Equal(SketchExpressionRegionOffsetFailure.CurveOffset,error.Failure);Assert.Equal(1,error.FirstLoopIndex);
        Assert.NotEmpty(error.CurveDiagnostics);Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Fact]
    public void DisjointFinalLoopsWithSwappedContainmentAreRejectedRatherThanRenamed()
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,10),Circle(new(6,0),2)]);var before=JsonSerializer.Serialize(source);
        // Each final loop is regular and their final boundaries are disjoint,
        // but the expanding hole now contains the shrinking original outer.
        var outer=SketchExpressionOffset2d.CreateClosed(source.Loops[0],-8).Chain;
        var hole=SketchExpressionOffset2d.CreateClosed(source.Loops[1],8).Chain;
        var wrong=SketchExpressionRegion2d.Classify([outer,hole]);Assert.Equal(new[]{1,-1},wrong.ParentIndices);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,-8));
        Assert.Equal(SketchExpressionRegionOffsetFailure.ChangedContainment,error.Failure);Assert.Equal(0,error.FirstLoopIndex);Assert.Equal(1,error.SecondLoopIndex);
        Assert.Empty(error.CurveDiagnostics);
        Assert.Contains("split/merge/reparenting",error.Message);Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Fact]
    public void CollapsedConnectingNeckRequiresExplicitSplitInsteadOfPublishingTwoPieces()
    {
        SketchPoint2d[] points=[new(0,0),new(4,0),new(4,1),new(8,1),new(8,0),new(12,0),
            new(12,4),new(8,4),new(8,3),new(4,3),new(4,4),new(0,4)];
        var loop=SketchExpressionChain2d.CreateOrdered(Enumerable.Range(0,points.Length).Select(i=>Line(points[i],points[(i+1)%points.Length])).ToArray(),true);
        var source=SketchExpressionRegion2d.Classify([loop]);var before=JsonSerializer.Serialize(source);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,-1.2));
        Assert.Equal(SketchExpressionRegionOffsetFailure.CurveOffset,error.Failure);Assert.Equal(0,error.FirstLoopIndex);Assert.NotEmpty(error.CurveDiagnostics);
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Fact]
    public void NearContactUsesOperationToleranceRatherThanEarlierRegionTolerance()
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,5),Circle(new(10.00001,0),5)],1e-9);var before=JsonSerializer.Serialize(source);
        var error=Assert.Throws<SketchExpressionRegionOffsetException>(()=>SketchExpressionRegionOffset2d.Create(source,.001,tolerance:.0001));
        Assert.Equal(SketchExpressionRegionOffsetFailure.SourceBoundary,error.Failure);Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    [Fact]
    public void InvalidArgumentsHaveNoNativeOrAuthorEffects()
    {
        var source=SketchExpressionRegion2d.Classify([Circle(default,5)]);var before=JsonSerializer.Serialize(source);
        Assert.Throws<ArgumentNullException>(()=>SketchExpressionRegionOffset2d.Create(null!,.2));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionRegionOffset2d.Create(source,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionRegionOffset2d.Create(source,double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionRegionOffset2d.Create(source,.2,(SketchExpressionJoin)999));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionRegionOffset2d.Create(source,.2,miterLimit:21));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SketchExpressionRegionOffset2d.Create(source,.2,tolerance:0));
        Assert.Equal(before,JsonSerializer.Serialize(source));
    }
    private static double Sq(double value)=>value*value;
    private static SketchExpressionChain2d Circle(SketchPoint2d center,double radius)=>
        SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(center,radius))],true);
    private static SketchCurveExpression2d Line(SketchPoint2d a,SketchPoint2d b)=>SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(a,b));
    private static SketchExpressionChain2d FreeformBox(int kind,double x,double y,double width,double height)
    {
        var curve=kind switch
        {
            1=>SketchCurve2d.Bezier([new(x,y),new(x+width/2,y+1),new(x+width,y)]),
            2=>SketchCurve2d.Bezier([new(x,y),new(x+width/2,y+1),new(x+width,y)],[1,1.2,.9]),
            _=>SketchCurve2d.BSpline([new(x,y),new(x+width*.25,y+1),new(x+width*.65,y+.5),new(x+width,y)],
                [0,.5,1],[3,1,3],2,false,[1,1.2,.9,1.1])
        };
        return SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(curve),Line(new(x+width,y),new(x+width,y-height)),
            Line(new(x+width,y-height),new(x,y-height)),Line(new(x,y-height),new(x,y))],true);
    }
}
