using System.Security.Cryptography;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ExactOffsetStepRoundTripTests
{
    [Theory]
    [InlineData("arc-g1-mirrored-inward-Bevel",135.3432565745697)]
    [InlineData("spline-g1-mirrored-inward-Bevel",139.58730069490207)]
    [InlineData("spline-g1-normal-outward-Round",157.78143097450942)]
    [InlineData("spline-g2-normal-outward-Round",157.78143097450942)]
    public void ExactOffsetStepRetainsStrictBrepMetricsParametersAndSource(string name,double authorVolume)
    {
        string output=Path.Combine(Path.GetTempPath(),"OcctSharpExactStep",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        using var source=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","b18-exact-step",name+".brep"));
        double sourceArea=Area(source);
        byte[] before=Bytes(source,"source-before");
        string step=ShapeExchange.WriteStep(source,Path.Combine(output,"exact.step"));
        Assert.Equal(before,Bytes(source,"source-after"));
        string graph=File.ReadAllText(step);Assert.DoesNotContain("NUL REF",graph);
        Assert.Equal(12,graph.Split("OFFSET_CURVE_3D",StringSplitOptions.None).Length-1);
        using(var imported=ShapeExchange.ReadStep(step))Check(imported,1);
        using(var report=ShapeExchange.ReadStepWithReport(step))
        {
            Assert.Equal(1,report.Report.CandidateRootCount);Assert.Equal(1,report.Report.TransferredRootCount);Check(report.Shape,1);
        }
        foreach(double unit in new[]{1d,25.4,1000})
        {
            using var session=StepReadSession.Open(step,unit);
            using var first=session.TransferRoot(0);byte[] firstBefore=Bytes(first,"first-before-"+unit);
            using var second=session.TransferRoot(0);
            Assert.Equal(firstBefore,Bytes(first,"first-after-"+unit));Check(first,unit);Check(second,unit);
            session.Dispose();Check(first,unit);Check(second,unit);
        }
        Assert.Equal(before,Bytes(source,"source-final"));

        byte[] Bytes(Shape shape,string label)
        {
            string path=ShapeExchange.WriteBrep(shape,Path.Combine(output,label+".brep"));
            return SHA256.HashData(File.ReadAllBytes(path));
        }
        void Check(Shape shape,double unit)
        {
            Assert.True(shape.GetValidationReport(true,true).IsValid);
            Assert.Equal(1,shape.GetTopologySummary().UniqueCounts.SolidCount);
            double mass=Math.Abs(shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-11).Properties.Mass)*unit*unit*unit;
            Assert.InRange(Math.Abs(mass-authorVolume),0,1e-7);
            Assert.InRange(Math.Abs(Area(shape)*unit*unit-sourceArea),0,1e-7);
        }
    }
    private static double Area(Shape shape)
    {
        var faces=shape.GetSubShapes(ShapeKind.Face);
        try{return faces.Sum(face=>Math.Abs(face.InspectAdaptiveProperties(InspectionPropertyKind.Area,1e-9).Properties.Mass));}
        finally{foreach(var face in faces)face.Dispose();}
    }
}
