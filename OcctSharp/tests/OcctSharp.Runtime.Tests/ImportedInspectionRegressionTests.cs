using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ImportedInspectionRegressionTests
{
    [Fact]
    public void DetailedImportedSolidBudgetsEachExactSpanInsteadOfRejectingNormalAccumulatedWork()
    {
        using var shape=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","minipc3-many-faces-solid.brep"));
        Assert.True(shape.IsValid);using var before=RepairSnapshot.Create(shape);
        var measured=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-9);
        Assert.True(double.IsFinite(measured.Properties.Mass)&&measured.Properties.Mass>0);
        Assert.InRange(measured.EstimatedRelativeError,0,1e-9);
        // Independent SDK-only Gauss at 1e-12 on the ORIGINAL unpartitioned
        // fixture. GK mass-only differs by 1.2e-10; all values are preserved in
        // artifacts/cadoryx-inspection-mass-reference-many-faces-1.txt.
        Assert.InRange(Math.Abs(measured.Properties.Mass-5277.5345493580207),0,1e-5);
        double[] actualCenter=[measured.Properties.CenterOfMass.X,measured.Properties.CenterOfMass.Y,measured.Properties.CenterOfMass.Z];
        double[] expectedCenter=[.0057391598634751262,1.6494542465544584,-.0060364794645721611];
        for(int i=0;i<3;i++)Assert.InRange(Math.Abs(actualCenter[i]-expectedCenter[i]),0,1e-7);
        var matrix=measured.Properties.Inertia;
        double[,] actual={{matrix.I11,matrix.I12,matrix.I13},{matrix.I21,matrix.I22,matrix.I23},{matrix.I31,matrix.I32,matrix.I33}};
        double[,] expected={{708134.92561548017,-26.93735518296257,-624.95065268648591},
            {-26.93735518296257,1406685.744015669,29.519719672163802},
            {-624.95065268648591,29.519719672163802,708126.14216468809}};
        for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            Assert.InRange(Math.Abs(actual[row,col]-expected[row,col])/Math.Max(1,Math.Abs(expected[row,col])),0,1e-7);
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
    }
    [Fact]
    public void ImportedDenseTrimSpansPreserveConvergedMassCenterAndAllInertiaComponents()
    {
        using var shape=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","minipc3-difficult-trim-solid.brep"));
        using var before=RepairSnapshot.Create(shape);Assert.True(shape.IsValid);
        // Independent SDK-only fixed Legendre reference: 4096 outer / 256
        // inner nodes on ORIGINAL geometry, long-double accumulation. It
        // agrees with 2048/256 in mass within 3.2e-8. Ordinary adaptive GProp
        // does not converge its off-diagonal inertia on this fixture.
        foreach(double tolerance in new[]{1e-9,1e-11})
        {
            var measured=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,tolerance);
            Assert.InRange(measured.EstimatedRelativeError,0,tolerance);
            Assert.InRange(Math.Abs(measured.Properties.Mass-223.84908567032869),0,1e-7);
            Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.X-.0015797600917485111),0,1e-8);
            Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.Y-.0010306625888025361),0,1e-8);
            Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.Z-3.5072873825301185),0,1e-8);
            var m=measured.Properties.Inertia;
            double[,] actual={{m.I11,m.I12,m.I13},{m.I21,m.I22,m.I23},{m.I31,m.I32,m.I33}};
            double[,] reference={{2707.7244964757961,.08812804478892744,-.1049137341652866},
                {.08812804478892744,2707.6208404158597,-1.8104408146209767},
                {-.1049137341652866,-1.8104408146209767,1106.8727003863924}};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                Assert.InRange(Math.Abs(actual[row,col]-reference[row,col]),0,1e-6);
        }
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
    }
    [Fact]
    public void ImportedSmallMomentContributionsMeetAccuracyWithoutRelativeZeroSubdivision()
    {
        using var shape=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","minipc3-moment-cancellation-solid.brep"));
        Assert.True(shape.IsValid);
        using var before=RepairSnapshot.Create(shape);
        var timer=System.Diagnostics.Stopwatch.StartNew();
        var measured=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
        timer.Stop();
        // The frozen .4 import did not finish this original small solid in the
        // remaining 118 seconds of its five-minute diagnostic deadline.
        Assert.True(timer.Elapsed<TimeSpan.FromSeconds(10),$"Small-solid inspection took {timer.Elapsed}.");
        Assert.InRange(measured.EstimatedRelativeError,0,1e-10);
        // Independent SDK-only executable integrates the ORIGINAL fixture with
        // adaptive Gauss at 1e-12. GK mass-only agrees within 3.3e-13. The SDK's
        // default fixed-order GProp is inaccurate here (mass .1599529516).
        Assert.InRange(Math.Abs(measured.Properties.Mass-.18952421132937064),0,1e-9);
        Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.X-79.100677565953035),0,1e-8);
        Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.Y-21.409822439018534),0,1e-8);
        Assert.InRange(Math.Abs(measured.Properties.CenterOfMass.Z-7.8984311373664502),0,1e-8);
        var matrix=measured.Properties.Inertia;
        double[,] actual={{matrix.I11,matrix.I12,matrix.I13},{matrix.I21,matrix.I22,matrix.I23},{matrix.I31,matrix.I32,matrix.I33}};
        double[,] expected={{.16666991267774733,0,0},{0,.0065840136701353913,-.0052768219160270746},{0,-.0052768219160270746,.16079661195522021}};
        for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            Assert.InRange(Math.Abs(actual[row,col]-expected[row,col]),0,1e-8);
        using var after=RepairSnapshot.Create(shape);
        Assert.Equal(before.Fingerprint,after.Fingerprint);
    }
    [Fact]
    public void PeriodicPcurveImportedSolidUsesNativeSpanIntegrationWithoutContinuitySplitting()
    {
        string path=Path.Combine(AppContext.BaseDirectory,"Fixtures","minipc3-periodic-pcurve-solid.brep");
        using var shape=ShapeExchange.ReadBrep(path);Assert.True(shape.IsValid);
        using var before=RepairSnapshot.Create(shape);var topology=shape.GetTopologySummary();
        using var independent=GPropProperties.FromShape(shape,GPropMode.Volume,onlyClosed:true);
        for(int i=0;i<3;i++)
        {
            var adaptive=shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10);
            var ordinary=shape.InspectProperties(InspectionPropertyKind.Volume);
            Assert.True(double.IsFinite(adaptive.Properties.Mass)&&adaptive.Properties.Mass>0);
            Assert.InRange(Math.Abs(adaptive.Properties.Mass-ordinary.Mass)/ordinary.Mass,0,1e-8);
            Assert.InRange(adaptive.EstimatedRelativeError,0,1e-10);
            Assert.InRange(Math.Abs(adaptive.Properties.Mass-independent.Mass)/independent.Mass,0,1e-6);
            Assert.InRange(Math.Abs(adaptive.Properties.CenterOfMass.X-independent.CenterOfMass.X),0,1e-6);
            Assert.InRange(Math.Abs(adaptive.Properties.CenterOfMass.Y-independent.CenterOfMass.Y),0,1e-6);
            Assert.InRange(Math.Abs(adaptive.Properties.CenterOfMass.Z-independent.CenterOfMass.Z),0,1e-6);
            Assert.InRange(Math.Abs(adaptive.Properties.Inertia.I11-independent.InertiaValue(1,1)),0,1e-6*Math.Max(1,Math.Abs(independent.InertiaValue(1,1))));
            Assert.InRange(Math.Abs(adaptive.Properties.Inertia.I22-independent.InertiaValue(2,2)),0,1e-6*Math.Max(1,Math.Abs(independent.InertiaValue(2,2))));
            Assert.InRange(Math.Abs(adaptive.Properties.Inertia.I33-independent.InertiaValue(3,3)),0,1e-6*Math.Max(1,Math.Abs(independent.InertiaValue(3,3))));
        }
        foreach(var face in shape.GetFaces())
        using(face)
        {
            var measured=face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-10);
            using var independentArea=GPropProperties.FromShape(face,GPropMode.Surface);
            Assert.InRange(Math.Abs(measured.Properties.Mass-independentArea.Mass),0,1e-6*Math.Max(1,independentArea.Mass));
        }
        using var after=RepairSnapshot.Create(shape);Assert.Equal(before.Fingerprint,after.Fingerprint);
        Assert.Equal(topology,shape.GetTopologySummary());
    }

    [Theory][InlineData(false)][InlineData(true)]
    public void C0PeriodicFiniteKnotSpansMeasureAndExtrudeInBothDirections(bool reversed)
    {
        using var basis=FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline(
            [new(8,0,0),new(10,4,0),new(12,7,0),new(16,0,0),new(12,-4,0),new(10,-2,0)],
            [5,5.7,7],[3,3,3],3,true,[1,.7,1.1,.8,1.2,1]));
        using var segment=FreeformAuthoring.SegmentCurve(basis,new(7.5,9));
        var g=FreeformAuthoring.GetCurveDefinition(segment);
        var curve=SketchCurve2d.BSpline(g.Poles.Select(p=>new SketchPoint2d(p.X,p.Y)).ToArray(),
            g.Knots,g.Multiplicities,g.Degree,g.Periodic,g.Weights);
        if(reversed)curve=curve.Reverse();
        var chain=SketchCurveChain2d.Create([curve,SketchCurve2d.Segment(curve.Evaluate(1).Point,curve.Evaluate(0).Point)],true);
        double area=Math.Abs(chain.Measure().SignedArea);Assert.True(area>0);
        // Independent positional polygon quadrature: no derivatives, knot removal,
        // or reuse of the production 8/16-point Gauss area integrator.
        const int samples=32768;
        double positionalArea=0;
        var previous=curve.Evaluate(0).Point;
        for(int i=1;i<=samples;i++)
        {
            var next=curve.Evaluate((double)i/samples).Point;
            positionalArea+=previous.X*next.Y-next.X*previous.Y;
            previous=next;
        }
        var first=curve.Evaluate(0).Point;
        positionalArea+=previous.X*first.Y-first.X*previous.Y;
        Assert.InRange(Math.Abs(Math.Abs(positionalArea/2)-area)/area,0,1e-6);
        var profile=SketchProfile2d.Create(chain);using var solid=profile.Extrude(SketchPlane.XY,3);
        Assert.True(solid.IsValid);double volume=solid.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass;
        Assert.InRange(Math.Abs(volume-3*area)/(3*area),0,1e-7);
        Assert.InRange(Math.Abs(Math.Abs(chain.Reverse().Measure().SignedArea)-area),0,1e-9);
    }
}
