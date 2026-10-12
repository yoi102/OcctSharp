namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionProfileTests
{
    private static readonly int[] NestedDepths=[2,0,3,1];
    private static readonly int[] NestedParents=[3,-1,0,1];
    private static readonly int[] RootDepths=[0,0];
    private static readonly int[] RootParents=[-1,-1];
    [Theory]
    [InlineData(.2)]
    [InlineData(-.2)]
    public void CircularOffsetProfileHasAnalyticalAreaAndExtrudedVolume(double distance)
    {
        var loop=Circle(new(2,-3),4,distance);
        var measured=loop.Measure();
        double expected=Math.PI*Math.Pow(4+distance,2);
        Assert.InRange(Math.Abs(measured.SignedArea-expected),0,1e-9);
        Assert.InRange(Math.Abs(loop.Reverse().Measure().SignedArea+expected),0,1e-9);
        var profile=SketchExpressionProfile2d.Create(loop.Reverse());
        foreach(var plane in new[]{SketchPlane.XY,new SketchPlane(new(10,20,30),new(0,1,0),new(0,0,1))})
        {
            using var face=profile.CreateFace(plane);
            Assert.True(face.IsValid);
            Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-expected),0,1e-8);
            using var solid=profile.Extrude(plane,3);
            Assert.True(solid.IsValid);
            Assert.Equal(1,solid.GetTopologySummary().UniqueCounts.SolidCount);
            Assert.InRange(Math.Abs(solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass-3*expected),0,1e-7);
            CheckOffsetPcurves(face);
        }
    }

    [Theory]
    [InlineData(.2)]
    [InlineData(-.2)]
    public void EllipseOffsetKeepsItsNativeCurveAndPcurveOnTheFace(double distance)
    {
        var expression=SketchCurveExpression2d.FromCurve(SketchCurve2d.Ellipse(new(2,-3),8,3)).Offset(distance);
        var loop=SketchExpressionChain2d.CreateOrdered([expression],true);
        var profile=SketchExpressionProfile2d.Create(loop);
        using var face=profile.CreateFace(SketchPlane.XY);
        Assert.True(face.IsValid);
        double expected=distance>0?82.797259948932339252310652725657:68.250514835664919652973241142421;
        Assert.InRange(Math.Abs(loop.Measure().SignedArea-expected),0,1e-9);
        Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-expected),0,1e-8);
        Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-loop.Measure().SignedArea),0,1e-8);
        CheckOffsetPcurves(face);
    }

    [Fact]
    public void ProfileHolesAreNormalizedAndMultipleIslandDepthsRemainExplicit()
    {
        var outer=Circle(default,10,.2).Reverse();
        var hole=Circle(default,6,.1);
        var island=Circle(default,3,.05).Reverse();
        var innerHole=Circle(default,1,.02);
        var region=SketchExpressionRegion2d.Classify([island,outer,innerHole,hole]);
        Assert.Equal(NestedDepths,region.Depths);
        Assert.Equal(NestedParents,region.ParentIndices);
        Assert.Equal(2,region.Profiles.Count);
        Assert.All(region.Profiles,p=>Assert.Single(p.Holes));
        Assert.True(region.Loops[1].Measure().SignedArea>0);
        Assert.True(region.Loops[3].Measure().SignedArea<0);
        using var shape=region.CreateShape(SketchPlane.XY);
        Assert.True(shape.IsValid);
        Assert.Equal(2,shape.GetTopologySummary().UniqueCounts.FaceCount);
        double expected=Math.PI*(10.2*10.2-6.1*6.1+3.05*3.05-1.02*1.02);
        var faces=region.CreateFaces(SketchPlane.XY);
        try
        {
            double area=faces.Sum(face=>face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass);
            Assert.InRange(Math.Abs(area-expected),0,1e-8);
        }
        finally{foreach(var face in faces)face.Dispose();}
        Assert.Throws<SketchValidationException>(()=>SketchExpressionProfile2d.Create(outer,[hole,island]));
    }

    [Fact]
    public void DisjointRootsAreFilledAndIntersectingTouchingDuplicateLoopsAreRejected()
    {
        var first=Circle(default,2,.1);
        var second=Circle(new(10,0),1,-.1);
        var region=SketchExpressionRegion2d.Classify([first,second]);
        Assert.Equal(RootDepths,region.Depths);
        Assert.Equal(RootParents,region.ParentIndices);
        Assert.Equal(2,region.Profiles.Count);
        using var valid=region.CreateShape(SketchPlane.XY);
        var crossing=Circle(new(2,0),2,.1);
        var tangent=Circle(new(4.2,0),2,.1);
        Assert.Throws<SketchValidationException>(()=>SketchExpressionRegion2d.Classify([first,crossing]));
        Assert.Throws<SketchValidationException>(()=>SketchExpressionRegion2d.Classify([first,tangent]));
        Assert.Throws<SketchValidationException>(()=>SketchExpressionRegion2d.Classify([first,first.Reverse()]));
        Assert.True(valid.IsValid);
        Assert.Equal(2,valid.GetTopologySummary().UniqueCounts.FaceCount);
    }

    [Fact]
    public void OutsideHoleOpenInputAndNullArgumentsDoNotChangeExistingFaces()
    {
        var outer=Circle(default,3,.1);
        var profile=SketchExpressionProfile2d.Create(outer);
        using var face=profile.CreateFace(SketchPlane.XY);
        Assert.Throws<SketchValidationException>(()=>SketchExpressionProfile2d.Create(outer,[Circle(new(10,0),1,.1)]));
        var open=SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(default,new(4,0)))]);
        Assert.Throws<SketchValidationException>(()=>SketchExpressionRegion2d.Classify([open]));
        Assert.Throws<ArgumentNullException>(()=>SketchExpressionRegion2d.Classify(null!));
        Assert.Throws<ArgumentException>(()=>SketchExpressionRegion2d.Classify([]));
        Assert.Throws<ArgumentNullException>(()=>profile.CreateFace(null!));
        Assert.Throws<ArgumentOutOfRangeException>(()=>profile.Extrude(SketchPlane.XY,0));
        Assert.True(face.IsValid);
    }

    private static SketchExpressionChain2d Circle(SketchPoint2d center,double radius,double distance) =>
        SketchExpressionChain2d.CreateOrdered([SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(center,radius)).Offset(distance)],true);

    [Theory]
    [InlineData(0,.2,false)]
    [InlineData(0,-.2,true)]
    [InlineData(1,.2,true)]
    [InlineData(1,-.2,false)]
    [InlineData(2,.2,false)]
    [InlineData(2,-.2,true)]
    public void MixedFreeformOffsetAndHoleRetainExactFaceGeometryThroughBrep(int kind,double distance,bool mirror)
    {
        var basis=kind switch
        {
            0=>SketchCurve2d.Bezier([new(0,0),new(4,5),new(9,0)]).Trim(.1,.9),
            1=>SketchCurve2d.Bezier([new(0,0),new(2,4),new(7,5),new(10,0)]).Trim(.1,.9),
            _=>SketchCurve2d.BSpline([new(0,0),new(2,3),new(5,4),new(8,3),new(10,0)],
                [0,.4,1],[4,1,4],3,false,[1,1.2,.8,1.1,1]).Trim(.1,.9)
        };
        var curve=SketchCurveExpression2d.FromCurve(basis).Offset(distance);
        var start=curve.Evaluate(0).Point;var end=curve.Evaluate(1).Point;
        var right=new SketchPoint2d(end.X,-10);var left=new SketchPoint2d(start.X,-10);
        var source=new[]{curve,Line(end,right),Line(right,left),Line(left,start)};
        var holeCurve=SketchCurveExpression2d.FromCurve(SketchCurve2d.Circle(new(4,-5),1)).Offset(.05);
        if(mirror)
        {
            var reflection=SketchTransform2d.Mirror(default,SketchDirection2d.XAxis);
            source=source.Select(value=>value.Transform(reflection)).ToArray();holeCurve=holeCurve.Transform(reflection);
        }
        var outer=SketchExpressionChain2d.CreateOrdered(source,true);
        var hole=SketchExpressionChain2d.CreateOrdered([holeCurve],true);
        var profile=SketchExpressionProfile2d.Create(outer,[hole]);
        double expected=Math.Abs(outer.Measure().SignedArea)-Math.PI*1.05*1.05;
        var plane=new SketchPlane(new(10,20,30),new(0,1,0),new(0,0,1));
        string directory=Path.Combine(Path.GetTempPath(),"b18-mixed-face-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path;
            using(var face=profile.CreateFace(plane))
            {
                Assert.True(face.IsValid);
                Assert.Equal(5,face.GetTopologySummary().UniqueCounts.EdgeCount);
                Assert.InRange(Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-expected),0,1e-8);
                CheckOffsetPcurves(face);
                path=ShapeExchange.WriteBrep(face,Path.Combine(directory,"face.brep"));
            }
            using var restored=ShapeExchange.ReadBrep(path);
            Assert.True(restored.IsValid);
            Assert.Equal(5,restored.GetTopologySummary().UniqueCounts.EdgeCount);
            Assert.InRange(Math.Abs(restored.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10).Properties.Mass-expected),0,1e-8);
            CheckOffsetPcurves(restored);
        }
        finally{Directory.Delete(directory,true);}
    }

    [Theory]
    [InlineData(.2)]
    [InlineData(-.2)]
    public void OffsetCircularProfileRevolvesToAnAnalyticalTorus(double distance)
    {
        var profile=SketchExpressionProfile2d.Create(Circle(new(12,0),2,distance));
        using var torus=profile.Revolve(SketchPlane.XY,default,SketchDirection2d.YAxis);
        Assert.True(torus.IsValid);
        Assert.Equal(1,torus.GetTopologySummary().UniqueCounts.SolidCount);
        double expected=2*Math.PI*Math.PI*12*Math.Pow(2+distance,2);
        Assert.InRange(Math.Abs(torus.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass-expected),0,1e-7);
    }

    private static SketchCurveExpression2d Line(SketchPoint2d first,SketchPoint2d last) =>
        SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(first,last));

    private static void CheckOffsetPcurves(Shape face)
    {
        var edges=face.GetSubShapes(ShapeKind.Edge);
        try
        {
            Assert.Contains(edges,e=>e.GetEdgeCurveSnapshot().CurveType==CurveGeometryType.OffsetCurve);
            foreach(var edge in edges)
            {
                var snapshot=edge.GetPcurveSnapshot(face);
                foreach(double t in new[]{0,.17,.53,.91,1})
                {
                    double parameter=snapshot.FirstParameter+t*(snapshot.LastParameter-snapshot.FirstParameter);
                    var uv=edge.EvaluatePcurve(face,parameter).Point;
                    var surface=face.EvaluateFace(uv.X,uv.Y).Point;
                    var curve=edge.EvaluateEdge(parameter).Point;
                    Assert.InRange(surface.DistanceTo(curve),0,1e-9);
                }
            }
        }
        finally {foreach(var edge in edges)edge.Dispose();}
    }
}
