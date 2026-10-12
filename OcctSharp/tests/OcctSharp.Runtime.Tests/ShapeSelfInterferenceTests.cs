using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ShapeSelfInterferenceTests
{
    [Fact] public void ValidSolidRemainsClearAndInputFingerprintIsUnchanged()
    {
        using var shape=ShapeFactory.CreateBox(2,3,4);using var before=RepairSnapshot.Create(shape);
        var report=shape.GetSelfInterferenceReport();Assert.True(report.IsClear);Assert.Equal(0,report.SelfInterferenceFaultCount);Assert.Equal(0,report.OtherFaultCount);
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
        Assert.True(shape.IsValid);Assert.Equal(24,shape.InspectProperties(InspectionPropertyKind.Volume).Mass,8);
    }
    [Fact] public void OverlappingValidCompoundAndCrossingWireAreNotClear()
    {
        using var a=ShapeFactory.CreateBox(2,2,2);using var transform=GpTrsf.Create(1,0,0);using var b=a.Transformed(transform);
        using var overlap=ShapeFactory.CreateCompound([a,b]);Assert.True(overlap.GetValidationReport(true,true).IsValid);
        var report=overlap.GetSelfInterferenceReport();Assert.False(report.IsClear);Assert.True(report.SelfInterferenceFaultCount>0);
        using var crossing=ShapeFactory.CreatePolygonWire([new(-2,0,0),new(2,0,0),new(0,-2,0),new(0,2,0)]);
        Assert.False(crossing.GetSelfInterferenceReport().IsClear);
    }
    [Fact] public void SeparatedCompoundAndProperAdjacentWireStayClear()
    {
        using var a=ShapeFactory.CreateBox(2,2,2);using var transform=GpTrsf.Create(5,0,0);using var b=a.Transformed(transform);
        using var separated=ShapeFactory.CreateCompound([a,b]);Assert.True(separated.GetSelfInterferenceReport().IsClear);
        using var wire=ShapeFactory.CreatePolygonWire([new(0,0,0),new(0,0,5),new(5,0,5)]);Assert.True(wire.GetSelfInterferenceReport().IsClear);
    }
    [Fact] public void ReportHasNoNativeLifetimeAndDisposedInputRejects()
    {
        ShapeSelfInterferenceReport report;
        var shape=ShapeFactory.CreateBox(2,3,4);try{report=shape.GetSelfInterferenceReport();}finally{shape.Dispose();}
        Assert.True(report.IsClear);Assert.Throws<ObjectDisposedException>(()=>shape.GetSelfInterferenceReport());
    }
    [Fact] public void CurvedAndCornerSweepAndSolidAndSheetLoftAreClear()
    {
        using var edge=ShapeFactory.CreateCircleEdge(new(0,0,0),new(0,0,1),1);using var profile=ShapeFactory.CreateWire([edge]);
        using var arc=ShapeFactory.CreateArcEdge(new(0,0,0),new(10-10/Math.Sqrt(2),0,10/Math.Sqrt(2)),new(10,0,10));using var spine=ShapeFactory.CreateWire([arc]);
        using var plan=GuidedSweepPlan.Create(spine,[new(profile)],new(){SolidPolicy=SweepSolidPolicy.RequireSolid});using var sweep=plan.Build();
        Assert.True(sweep.RequireShape().GetSelfInterferenceReport().IsClear);
        using var cornerSpine=ShapeFactory.CreatePolygonWire([new(0,0,0),new(0,0,10),new(10,0,10)]);
        using var cornerPlan=GuidedSweepPlan.Create(cornerSpine,[new(profile)],new(){SolidPolicy=SweepSolidPolicy.RequireSolid,Transition=GuidedSweepTransition.RightCorner});using var corner=cornerPlan.Build();
        Assert.True(corner.RequireShape().GetSelfInterferenceReport().IsClear);
        using var lower=ShapeFactory.CreatePolygonWire([new(-1,-1,0),new(1,-1,0),new(1,1,0),new(-1,1,0)],true);
        using var upper=ShapeFactory.CreatePolygonWire([new(-2,-2,5),new(2,-2,5),new(2,2,5),new(-2,2,5)],true);
        using var solid=GuidedLoft.Build([lower,upper],new(){Solid=true,Ruled=true});using var sheet=GuidedLoft.Build([lower,upper],new(){Solid=false,Ruled=true});
        Assert.True(solid.RequireShape().GetSelfInterferenceReport().IsClear);Assert.True(sheet.RequireShape().GetSelfInterferenceReport().IsClear);
    }
}
