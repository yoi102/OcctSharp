using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class LocalizedMomentInspectionTests
{
    public static IEnumerable<object[]> OriginalOffsetFiles()
    {
        foreach(double scale in new[]{.01,1d})
        foreach(bool reverse in new[]{false,true})
        foreach(string variant in new[]{"original","translated","reflected"})
            yield return [scale,reverse,variant];
    }

    [Theory]
    [MemberData(nameof(OriginalOffsetFiles))]
    public void OriginalLargeCoordinateRoundOffsetsConvergeWithoutRebuildingTopology(double scale,bool reverse,string variant)
    {
        string name=$"translated-rotated-Round-{scale.ToString(System.Globalization.CultureInfo.InvariantCulture)}-{reverse}-{variant}.brep";
        using var shape=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","b17-large-offset",name));
        Assert.True(shape.IsValid);var topology=shape.GetTopologySummary();using var before=RepairSnapshot.Create(shape);
        // Independent Minkowski-sum moments: 20 x 12 rectangle, radius-one
        // round offset, rigid XY rotation .73, author scale and height three.
        // These are the actual retained .10 failure BReps, including the exact
        // translations/reflections that did not repair the old inspection.
        double area=(304+Math.PI)*scale*scale,mass=3*area,c=Math.Cos(.73),s=Math.Sin(.73);
        double x=(10*c-6*s)*scale,y=(10*s+6*c)*scale;
        if(variant=="original"){x+=1_000_000;y-=1_000_000;}
        if(variant=="reflected")x=-x;
        double ix=(4877d+1d/3+36.25*Math.PI)*Math.Pow(scale,4),
            iy=(12008+100.25*Math.PI)*Math.Pow(scale,4);
        double i11=3*(ix*c*c+iy*s*s)+mass*9/12,
            i22=3*(ix*s*s+iy*c*c)+mass*9/12,
            i12=3*c*s*(ix-iy)*(variant=="reflected"?-1:1),i33=3*(ix+iy);
        for(int repeat=0;repeat<2;repeat++)
        {
            var measured=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
            Assert.InRange(measured.EstimatedRelativeError,0,1e-10);
            Near(mass,measured.Properties.Mass);
            Assert.InRange(Math.Abs(x-measured.Properties.CenterOfMass.X),0,1e-8);
            Assert.InRange(Math.Abs(y-measured.Properties.CenterOfMass.Y),0,1e-8);
            Assert.InRange(Math.Abs(1.5-measured.Properties.CenterOfMass.Z),0,1e-8);
            var m=measured.Properties.Inertia;
            Near(i11,m.I11);Near(i22,m.I22);Near(i33,m.I33);Near(i12,m.I12);Near(i12,m.I21);
            Near(0,m.I13);Near(0,m.I31);Near(0,m.I23);Near(0,m.I32);
        }
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
        Assert.Equal(topology,shape.GetTopologySummary());
    }

    [Theory]
    [InlineData(.01,0)][InlineData(.01,1)][InlineData(.01,2)][InlineData(.01,3)]
    [InlineData(1d,0)][InlineData(1d,1)][InlineData(1d,2)][InlineData(1d,3)]
    public void LocatedTiltedAndReflectedCircularSupportsPreserveAllMoments(double radius,int mode)
    {
        using var edge=ShapeFactory.CreateCircleEdge(new(1_000_000,-1_000_000,0),new(0,0,1),radius);
        using var wire=ShapeFactory.CreateWire([edge]);using var face=ShapeFactory.CreatePlanarFace(wire);
        using var direction=GpVec.Create(0,0,3);using var solid=face.Extrude(direction);
        using var transform=mode switch
        {
            0=>GpTrsf.Identity,1=>GpTrsf.Create(-1_000_000,1_000_000,7),
            2=>GpTrsf.Create(7,-3,2,2,1,3,.413),
            _=>GpTrsf.CreatePlaneMirror(3,-2,1,2,1,3)
        };
        using var shape=solid.Transformed(transform);using var before=RepairSnapshot.Create(shape);
        double mass=3*Math.PI*radius*radius;
        double[] center=[1_000_000,-1_000_000,1.5],diagonal=[mass*(radius*radius/4+.75),mass*(radius*radius/4+.75),mass*radius*radius/2];
        var result=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
        Assert.InRange(result.EstimatedRelativeError,0,1e-10);
        Assert.InRange(Math.Abs(mass-result.Properties.Mass)/mass,0,1e-8);
        double[] actualCenter=[result.Properties.CenterOfMass.X,result.Properties.CenterOfMass.Y,result.Properties.CenterOfMass.Z];
        var m=result.Properties.Inertia;
        double[,] actual={{m.I11,m.I12,m.I13},{m.I21,m.I22,m.I23},{m.I31,m.I32,m.I33}};
        for(int row=0;row<3;row++)
        {
            double expectedCenter=transform.Value(row+1,4);
            for(int k=0;k<3;k++)expectedCenter+=transform.Value(row+1,k+1)*center[k];
            Assert.InRange(Math.Abs(actualCenter[row]-expectedCenter),0,2e-9);
            for(int col=0;col<3;col++)
            {
                double expected=0;
                for(int k=0;k<3;k++)expected+=transform.Value(row+1,k+1)*diagonal[k]*transform.Value(col+1,k+1);
                Assert.InRange(Math.Abs(actual[row,col]-expected),0,Math.Max(1e-12,Math.Abs(expected)*1e-8));
            }
        }
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
    }

    private static void Near(double expected,double actual)=>
        Assert.InRange(Math.Abs(expected-actual),0,Math.Max(1e-8,Math.Abs(expected)*1e-8));
}
