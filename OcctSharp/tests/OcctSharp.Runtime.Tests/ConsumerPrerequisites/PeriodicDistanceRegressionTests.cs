using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class PeriodicDistanceRegressionTests
{
    public static TheoryData<string,double,bool> Cases => new()
    {
        {"arc-g1",0,false},{"arc-g1",0,true},{"arc-g1",.02,false},{"arc-g1",.02,true},
        {"spline-g1",0,false},{"spline-g1",0,true},{"spline-g1",.02,false},{"spline-g1",.02,true},
        {"spline-g2",0,false},{"spline-g2",0,true},{"spline-g2",.02,false},{"spline-g2",.02,true}
    };

    [Theory,MemberData(nameof(Cases))]
    public void NegativePeriodicDistancePreservesOriginalParametersAndOwnedSupports(string kind,double offset,bool reverse)
    {
        using var source=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","b18-periodic-distance",
            kind+"-normal-outward-Round.brep"));
        using var before=RepairSnapshot.Create(source);
        var edges=source.GetSubShapes(ShapeKind.Edge);
        try
        {
            var edge=edges.First(e=>e.GetEdgeCurveSnapshot() is {CurveType:CurveGeometryType.Ellipse,FirstParameter:<0});
            var bounds=edge.GetEdgeCurveSnapshot();
            var originalPoint=edge.EvaluateEdge((bounds.FirstParameter+bounds.LastParameter)/2).Point;
            var point=originalPoint with {Z=originalPoint.Z+offset};
            using var line=ShapeFactory.CreateEdge(point,point with {Z=point.Z+1});
            var vertices=line.GetSubShapes(ShapeKind.Vertex);
            try
            {
                var vertex=vertices.First(v=>v.GetBoundingBox().Minimum.DistanceTo(point)<1e-6);
                var first=reverse?edge:vertex;var second=reverse?vertex:edge;
                var simple=first.DistanceTo(second);
                Assert.InRange(Math.Abs(simple.Distance-offset),0,1e-10);
                Assert.InRange(Math.Abs(simple.PointOnFirst.DistanceTo(simple.PointOnSecond)-offset),0,1e-10);
                using var exact=first.InspectDistanceTo(second);
                Assert.Equal(simple.SolutionCount,exact.Solutions.Count);
                using var repeated=first.InspectDistanceTo(second);
                Assert.Equal(exact.Solutions.Count,repeated.Solutions.Count);
                Assert.NotEmpty(exact.Solutions);
                Assert.All(exact.Solutions,solution=>
                {
                    Assert.InRange(Math.Abs(solution.Distance-offset),0,1e-10);
                    Assert.InRange(Math.Abs(solution.PointOnFirst.DistanceTo(solution.PointOnSecond)-offset),0,1e-10);
                    Assert.Equal(InspectionSupportKind.Edge,reverse?solution.FirstSupportKind:solution.SecondSupportKind);
                    Assert.Equal(InspectionSupportKind.Vertex,reverse?solution.SecondSupportKind:solution.FirstSupportKind);
                    double parameter=(reverse?solution.FirstEdgeParameter:solution.SecondEdgeParameter)!.Value;
                    Assert.InRange(parameter,bounds.FirstParameter,bounds.LastParameter);
                    var support=reverse?solution.FirstSupport:solution.SecondSupport;
                    Assert.InRange(support.EvaluateEdge(parameter).Point.DistanceTo(reverse?solution.PointOnFirst:solution.PointOnSecond),0,1e-10);
                    Assert.Equal(bounds.FirstParameter,support.GetEdgeCurveSnapshot().FirstParameter);
                    Assert.Equal(bounds.LastParameter,support.GetEdgeCurveSnapshot().LastParameter);
                });
                using var after=RepairSnapshot.Create(source);Assert.Equal(before.Fingerprint,after.Fingerprint);
                // Every returned support is owned independently of input owners.
                source.Dispose();foreach(var item in edges)item.Dispose();foreach(var item in vertices)item.Dispose();line.Dispose();
                foreach(var solution in exact.Solutions)
                {
                    var support=reverse?solution.FirstSupport:solution.SecondSupport;
                    double parameter=(reverse?solution.FirstEdgeParameter:solution.SecondEdgeParameter)!.Value;
                    Assert.InRange(support.EvaluateEdge(parameter).Point.DistanceTo(originalPoint),0,1e-10);
                }
                Assert.Throws<ObjectDisposedException>(()=>first.DistanceTo(second));
            }
            finally{foreach(var vertex in vertices)vertex.Dispose();}
        }
        finally{foreach(var edge in edges)edge.Dispose();}
    }
}
