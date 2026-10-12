using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class AdaptiveInspectionTests
{
    private static SketchCurve2d Curve() => SketchCurve2d.BSpline(
        [new(8,0),new(20,2),new(16,4),new(18,6),new(16,9)], [0,.2,1], [4,1,4], 3);
    private static Shape Face(SketchCurve2d curve) => SketchProfile2d.Create(
        SketchCurveChain2d.Create([curve,SketchCurve2d.Segment(curve.Evaluate(1).Point,curve.Evaluate(0).Point)],true))
        .CreateFace(SketchPlane.XY);
    private static void Near(double expected,double actual,double tolerance=1e-6) =>
        Assert.InRange(actual,expected-tolerance,expected+tolerance);

    // Green's theorem, six-point Gauss on each authored polynomial span.
    // The highest integrand degree is 11, so this independently integrates all moments exactly.
    private static double[] Moments(SketchCurve2d curve)
    {
        double[] nodes=[-.9324695142031521,-.6612093864662645,-.2386191860831969,
            .2386191860831969,.6612093864662645,.9324695142031521];
        double[] weights=[.1713244923791704,.3607615730481386,.4679139345726910,
            .4679139345726910,.3607615730481386,.1713244923791704];
        var result=new double[6];
        var chord=SketchCurve2d.Segment(curve.Evaluate(1).Point,curve.Evaluate(0).Point);
        foreach(var piece in new[]{(curve,0d,.2),(curve,.2,1d),(chord,0d,1d)})
        for(int i=0;i<nodes.Length;i++)
        {
            double half=(piece.Item3-piece.Item2)/2,at=(piece.Item2+piece.Item3)/2+half*nodes[i];
            var e=piece.Item1.Evaluate(at);double x=e.Point.X,y=e.Point.Y;
            // OCCT supplies derivatives in the native domain (segment length for the chord).
            double domain=piece.Item1.LastParameter-piece.Item1.FirstParameter;
            double dx=e.FirstDerivative.X*domain,dy=e.FirstDerivative.Y*domain,w=half*weights[i];
            result[0]+=w*x*dy; result[1]+=w*x*x*dy/2; result[2]-=w*y*y*dx/2;
            result[3]+=w*x*x*x*dy/3; result[4]-=w*y*y*y*dx/3; result[5]+=w*x*x*y*dy/2;
        }
        return result;
    }

    [Fact] public void NonuniformAreaVolumeCentroidAndInertiaMatchIndependentPolynomialMoments()
    {
        var curve=Curve();using var face=Face(curve);using var direction=GpVec.Create(0,0,3);using var solid=face.Extrude(direction);
        var m=Moments(curve);Near(40.208,m[0]);double x=m[1]/m[0],y=m[2]/m[0];
        var area=face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10);
        var volume=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
        Near(m[0],area.Properties.Mass);Near(m[0]*3,volume.Properties.Mass);
        foreach(var props in new[]{area.Properties,volume.Properties})
        {Near(x,props.CenterOfMass.X);Near(y,props.CenterOfMass.Y);}
        Near(1.5,volume.Properties.CenterOfMass.Z);
        double ix=m[4]-m[0]*y*y,iy=m[3]-m[0]*x*x,product=m[5]-m[0]*x*y;
        Near(ix,area.Properties.Inertia.I11);Near(iy,area.Properties.Inertia.I22);
        Near(-product,area.Properties.Inertia.I12);Near(ix+iy,area.Properties.Inertia.I33);
        Near(ix*3+m[0]*3*9/12,volume.Properties.Inertia.I11);
        Near(iy*3+m[0]*3*9/12,volume.Properties.Inertia.I22);
        Near(-product*3,volume.Properties.Inertia.I12);Near((ix+iy)*3,volume.Properties.Inertia.I33);
        Assert.InRange(area.EstimatedRelativeError,0,1e-10);Assert.InRange(volume.EstimatedRelativeError,0,1e-10);
        Near(m[0],face.InspectProperties(InspectionPropertyKind.Area).Mass);
        Near(m[0]*3,solid.InspectProperties(InspectionPropertyKind.Volume).Mass);
    }

    [Fact] public void CopiedPartitionPreservesOriginalTopologyFingerprintLocationAndLifetime()
    {
        using var face=Face(Curve());using var direction=GpVec.Create(0,0,3);using var solid=face.Extrude(direction);
        using var transform=GpTrsf.Create(125,-42,9);using var located=solid.Transformed(transform);
        using var before=RepairSnapshot.Create(located);var topology=located.GetTopologySummary();
        AdaptiveShapeInspection copied=default;
        for(int i=0;i<3;i++)copied=located.InspectAdaptiveProperties(InspectionPropertyKind.Volume);
        using var after=RepairSnapshot.Create(located);
        Assert.Equal(before.Fingerprint,after.Fingerprint);Assert.Equal(topology,located.GetTopologySummary());
        Near(120.624,copied.Properties.Mass);Near(10.5,copied.Properties.CenterOfMass.Z);
        var original=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume).Properties;
        Near(original.CenterOfMass.X+125,copied.Properties.CenterOfMass.X);
        Near(original.CenterOfMass.Y-42,copied.Properties.CenterOfMass.Y);
        Near(original.Inertia.I11,copied.Properties.Inertia.I11,1e-5);
        located.Dispose();Near(120.624,copied.Properties.Mass);
        Assert.Throws<ObjectDisposedException>(()=>located.InspectAdaptiveProperties(InspectionPropertyKind.Volume));
    }

    [Fact] public void ReversedCurveAndStepKeepSameProperties()
    {
        var curve=Curve();using var face=Face(curve);using var reverse=Face(curve.Reverse());
        Near(40.208,reverse.InspectAdaptiveProperties(InspectionPropertyKind.Area).Properties.Mass);
        using var direction=GpVec.Create(0,0,3);using var solid=face.Extrude(direction);
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        try{ShapeExchange.WriteStep(solid,path);using var loaded=ShapeExchange.ReadStep(path);
            Near(120.624,loaded.InspectAdaptiveProperties(InspectionPropertyKind.Volume).Properties.Mass);}
        finally{File.Delete(path);}
    }

    [Theory]
    [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)] [InlineData(0)]
    [InlineData(-1)] [InlineData(1e-13)] [InlineData(.00101)]
    public void InvalidToleranceRejectsBeforeInspection(double tolerance)
    {using var shape=ShapeFactory.CreateBox(2,3,4);Assert.Throws<ArgumentOutOfRangeException>(()=>shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,tolerance));}

    [Fact] public void TypeBoundariesUnitsAndAnalyticalSolidsArePreserved()
    {
        using var box=ShapeFactory.CreateBox(2,3,4);using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>box.InspectAdaptiveProperties(InspectionPropertyKind.Length));
        Assert.Throws<InvalidCastException>(()=>box.InspectAdaptiveProperties(InspectionPropertyKind.Area));
        Assert.Throws<InvalidCastException>(()=>edge.InspectAdaptiveProperties(InspectionPropertyKind.Volume));
        Near(24,box.InspectAdaptiveProperties(InspectionPropertyKind.Volume).Properties.Mass);
        using var cylinder=ShapeFactory.CreateCylinder(2,5);
        Near(Math.PI*20,cylinder.InspectAdaptiveProperties(InspectionPropertyKind.Volume).Properties.Mass);
    }

    [Fact] public void BooleanWithExtrudedLinearSplineAndCircularHoleKeepsExactMoments()
    {
        var bottom=SketchCurve2d.Segment(new(0,0),new(20,0));
        var right=SketchCurve2d.Bezier([new(20,0),new(20,5),new(20,10)]);
        var top=SketchCurve2d.BSpline([new(20,10),new(10,10),new(0,10)],[0,1,2],[2,1,2],1);
        var left=SketchCurve2d.Segment(new(0,10),new(0,0));
        var outer=SketchCurveChain2d.Create([bottom,right,top,left],true);
        var hole=SketchCurveChain2d.Create([SketchCurve2d.Circle(new(10,5),2)],true);
        var profile=SketchProfile2d.Classify([outer,hole]);
        using var box=ShapeFactory.CreateBox(30,20,4);
        using var added=profile.AddTo(box,SketchPlane.XY,5);
        using var cut=profile.CutFrom(box,SketchPlane.XY,5);
        using var before=RepairSnapshot.Create(added);
        double area=200-4*Math.PI,volume=2400+area;
        for(int repeat=0;repeat<3;repeat++)
        {
            var result=added.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
            Near(volume,result.Properties.Mass);
            Near((2400*15+area*10)/volume,result.Properties.CenterOfMass.X);
            Near((2400*10+area*5)/volume,result.Properties.CenterOfMass.Y);
            Near((2400*2+area*4.5)/volume,result.Properties.CenterOfMass.Z);
            Near(2400-area*4,cut.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass);
            Assert.InRange(result.EstimatedRelativeError,0,1e-10);
        }
        using var after=RepairSnapshot.Create(added);
        Assert.Equal(before.Fingerprint,after.Fingerprint);
    }

    [Fact] public void ExtrudedCircularWirePreservesAnalyticalVolumeAndCentroid()
    {
        using var edge=ShapeFactory.CreateCircleEdge(new(2,3,0),new(0,0,1),5);
        using var wire=ShapeFactory.CreateWire([edge]);
        using var face=ShapeFactory.CreatePlanarFace(wire);
        using var direction=GpVec.Create(0,0,8);using var solid=face.Extrude(direction);
        var measured=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
        Near(Math.PI*200,measured.Properties.Mass);Near(2,measured.Properties.CenterOfMass.X);
        Near(3,measured.Properties.CenterOfMass.Y);Near(4,measured.Properties.CenterOfMass.Z);
        Near(Math.PI*200,solid.InspectProperties(InspectionPropertyKind.Volume).Mass);
        Near(Math.PI*200*(75+64)/12,measured.Properties.Inertia.I11,1e-5);
        Near(Math.PI*200*25/2,measured.Properties.Inertia.I33,1e-5);
    }
}
