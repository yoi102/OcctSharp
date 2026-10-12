using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class PeriodicInteriorDistanceRegressionTests
{
    public static IEnumerable<object[]> Cases
    {
        get
        {
            foreach(string kind in new[]{"circle-circle","ellipse-line","circle-plane","ellipse-plane","line-torus","point-torus","sphere-sphere","torus-torus",
                "nurbs-circle-circle","nurbs-circle-plane","line-nurbs-torus","point-nurbs-torus","nurbs-torus-torus"})
            for(int shift=0;shift<8;shift++)foreach(bool reverse in new[]{false,true})yield return new object[]{kind,shift,reverse};
        }
    }
    [Theory,MemberData(nameof(Cases))]
    public void PeriodicInteriorDistanceRetainsAuthorWitnessesOriginalDomainsAndOwnedSupports(string kind,int shift,bool reverse)
    {
        string fixtures=Path.Combine(AppContext.BaseDirectory,"Fixtures","b18-periodic-interior");
        using var a=ShapeExchange.ReadBrep(Path.Combine(fixtures,$"{kind}-{shift}-a.brep"));
        using var b=ShapeExchange.ReadBrep(Path.Combine(fixtures,$"{kind}-{shift}-b.brep"));
        var first=reverse?b:a;var second=reverse?a:b;
        Assert.True(a.GetValidationReport(true,true).IsValid);Assert.True(b.GetValidationReport(true,true).IsValid);
        using var firstBefore=RepairSnapshot.Create(first);using var secondBefore=RepairSnapshot.Create(second);
        var (pa,pb,ta,tb)=Author(kind,shift);
        if(reverse){(pa,pb)=(pb,pa);(ta,tb)=(tb,ta);}
        EdgeCurveSnapshot? firstEdge=ta==InspectionSupportKind.Edge?first.GetEdgeCurveSnapshot():null;
        EdgeCurveSnapshot? secondEdge=tb==InspectionSupportKind.Edge?second.GetEdgeCurveSnapshot():null;
        FaceSurfaceSnapshot? firstFace=ta==InspectionSupportKind.Face?first.GetFaceSurfaceSnapshot():null;
        FaceSurfaceSnapshot? secondFace=tb==InspectionSupportKind.Face?second.GetFaceSurfaceSnapshot():null;
        double expected=pa.DistanceTo(pb);
        var simple=first.DistanceTo(second);
        using var exact=first.InspectDistanceTo(second);using var repeated=first.InspectDistanceTo(second);
        Assert.InRange(Math.Abs(simple.Distance-expected),0,1e-7);
        Assert.InRange(simple.PointOnFirst.DistanceTo(pa),0,1e-7);Assert.InRange(simple.PointOnSecond.DistanceTo(pb),0,1e-7);
        Assert.Equal(simple.SolutionCount,exact.Solutions.Count);Assert.Equal(exact.Solutions.Count,repeated.Solutions.Count);Assert.NotEmpty(exact.Solutions);
        Assert.All(exact.Solutions,Check);Assert.All(repeated.Solutions,Check);
        using var firstAfter=RepairSnapshot.Create(first);using var secondAfter=RepairSnapshot.Create(second);
        Assert.Equal(firstBefore.Fingerprint,firstAfter.Fingerprint);Assert.Equal(secondBefore.Fingerprint,secondAfter.Fingerprint);
        first.Dispose();second.Dispose();
        Assert.All(exact.Solutions,Check);Assert.All(repeated.Solutions,Check);
        Assert.Throws<ObjectDisposedException>(()=>first.DistanceTo(second));

        void Check(ShapeDistanceSolution solution)
        {
            Assert.InRange(Math.Abs(solution.Distance-expected),0,1e-7);
            Assert.InRange(Math.Abs(solution.PointOnFirst.DistanceTo(solution.PointOnSecond)-expected),0,1e-7);
            Assert.InRange(solution.PointOnFirst.DistanceTo(pa),0,1e-7);Assert.InRange(solution.PointOnSecond.DistanceTo(pb),0,1e-7);
            Assert.Equal(ta,solution.FirstSupportKind);Assert.Equal(tb,solution.SecondSupportKind);
            CheckSupport(solution.FirstSupport,solution.FirstSupportKind,solution.FirstEdgeParameter,solution.FirstFaceParameters,solution.PointOnFirst,firstEdge,firstFace);
            CheckSupport(solution.SecondSupport,solution.SecondSupportKind,solution.SecondEdgeParameter,solution.SecondFaceParameters,solution.PointOnSecond,secondEdge,secondFace);
        }
        void CheckSupport(Shape support,InspectionSupportKind type,double? edgeParameter,(double U,double V)? faceParameters,GpPoint point,EdgeCurveSnapshot? originalEdge,FaceSurfaceSnapshot? originalFace)
        {
            Assert.True(support.GetValidationReport(true,true).IsValid);
            if(type==InspectionSupportKind.Edge)
            {
                Assert.NotNull(edgeParameter);Assert.Null(faceParameters);double parameter=edgeParameter.GetValueOrDefault();
                var bounds=support.GetEdgeCurveSnapshot();var original=originalEdge.GetValueOrDefault();
                Assert.Equal(original.FirstParameter,bounds.FirstParameter);Assert.Equal(original.LastParameter,bounds.LastParameter);
                Assert.InRange(parameter,original.FirstParameter,original.LastParameter);
                Assert.InRange(support.EvaluateEdge(parameter).Point.DistanceTo(point),0,1e-7);
            }
            else if(type==InspectionSupportKind.Face)
            {
                Assert.NotNull(faceParameters);Assert.Null(edgeParameter);var (u,v)=faceParameters.GetValueOrDefault();
                var bounds=support.GetFaceSurfaceSnapshot();var original=originalFace.GetValueOrDefault();Assert.Equal(original,bounds);
                Assert.InRange(u,original.FirstUParameter,original.LastUParameter);Assert.InRange(v,original.FirstVParameter,original.LastVParameter);
                Assert.InRange(support.EvaluateFace(u,v).Point.DistanceTo(point),0,1e-7);
            }
            else
            {
                Assert.Null(edgeParameter);Assert.Null(faceParameters);Assert.Equal(ShapeKind.Vertex,support.Kind);
                var box=support.GetBoundingBox();
                // OCCT expands a vertex box by its tolerance in all axes.
                // Its minimum corner is not the vertex coordinate.
                var center=new GpPoint((box.Minimum.X+box.Maximum.X)/2,(box.Minimum.Y+box.Maximum.Y)/2,(box.Minimum.Z+box.Maximum.Z)/2);
                Assert.InRange(center.DistanceTo(point),0,1e-7);
            }
        }
    }
    private static (GpPoint First,GpPoint Second,InspectionSupportKind FirstType,InspectionSupportKind SecondType) Author(string kind,int shift)
    {
        double phi=shift>=5?.71:0;GpPoint Ray(double distance,double z=0)=>new(distance*Math.Cos(phi),distance*Math.Sin(phi),z);
        if(kind is "circle-circle" or "nurbs-circle-circle")return (Ray(2),Ray(4,.25),InspectionSupportKind.Edge,InspectionSupportKind.Edge);
        if(kind=="ellipse-line")return (Ray(2),Ray(3),InspectionSupportKind.Edge,InspectionSupportKind.Edge);
        if(kind is "circle-plane" or "ellipse-plane" or "nurbs-circle-plane")return (Ray(2),Ray(3),InspectionSupportKind.Edge,InspectionSupportKind.Face);
        if(kind is "line-torus" or "line-nurbs-torus")return (Ray(5),Ray(4),InspectionSupportKind.Edge,InspectionSupportKind.Face);
        if(kind is "point-torus" or "point-nurbs-torus")return (Ray(5),Ray(4),InspectionSupportKind.Vertex,InspectionSupportKind.Face);
        if(kind=="sphere-sphere")return (Ray(2),Ray(4),InspectionSupportKind.Face,InspectionSupportKind.Face);
        return (Ray(4),Ray(6),InspectionSupportKind.Face,InspectionSupportKind.Face);
    }
}
