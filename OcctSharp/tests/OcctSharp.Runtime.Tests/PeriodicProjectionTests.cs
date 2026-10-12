using OcctSharp.ConstructionWorkspaces;
using OcctSharp.CurveSampling;
using OcctSharp.IntersectionResults;
using OcctSharp.Tests.Shared;
using OcctSharp.Values;
namespace OcctSharp.Runtime.Tests;
public sealed class PeriodicProjectionTests {
    [Fact] public void ProjectedSeamParametersBuildIndependentExactTopologyAndStep()=>BatchBVPeriodicProjectionWorkflow.Run();
    static GeomSurface Surface(AnalyticConstructionSurface kind) {
        using var built=ParameterizedConstruction.AnalyticFace(kind,ConstructionFrame.World,0,1,.1,.5,3,1);
        using var face=built.CopyShape();return RayIntersections.CopySurface(face);
    }
    [Fact] public void CylinderProjectionCrossesTheCanonicalUSeamWithCoupledCopiedParameters() {
        using var surface=Surface(AnalyticConstructionSurface.Cylinder);
        double expected=2*Math.PI+.2;
        var result=CurveComputations.Project(surface,new(4*Math.Cos(expected),4*Math.Sin(expected),.3),6,6.8,0,1);
        Assert.True(result.IsDone);int i=result.UniqueNearestIndex()!.Value;var row=result.Candidates[i];
        Assert.Equal(expected,row.U,7);Assert.Equal(.3,row.V,7);Assert.Equal(1,row.Distance,7);
        var point=BatchBHIntersectionWorkflow.Eval(surface,row.U,row.V);
        Assert.Equal(point.X,row.Point.X,7);Assert.Equal(point.Y,row.Point.Y,7);
        surface.Dispose();Assert.Equal(1,result.MinimumDistance,7);
    }
    [Theory][InlineData(-6.2)][InlineData(6.4)][InlineData(12.7)]
    public void ShiftedUWindowsRetainTheRequestedParameterBranch(double u) {
        using var surface=Surface(AnalyticConstructionSurface.Cylinder);
        var result=CurveComputations.Project(surface,new(4*Math.Cos(u),4*Math.Sin(u),.3),u-.2,u+.2,0,1);
        Assert.True(result.IsDone);var row=result.Candidates[result.UniqueNearestIndex()!.Value];
        Assert.Equal(u,row.U,7);Assert.Equal(1,row.Distance,7);
    }
    [Fact] public void TorusProjectionCrossesBothSeamsWithoutUncouplingThePoint() {
        using var surface=Surface(AnalyticConstructionSurface.Torus);double u=6.4,v=6.5;
        var target=BatchBHIntersectionWorkflow.Eval(surface,u,v);
        var result=CurveComputations.Project(surface,new(target.X,target.Y,target.Z),6,6.8,6,6.8);
        Assert.True(result.IsDone);var row=result.Candidates[result.UniqueNearestIndex()!.Value];
        Assert.Equal(u,row.U,6);Assert.Equal(v,row.V,6);Assert.Equal(0,row.Distance,6);
        Assert.Equal(target.X,row.Point.X,6);Assert.Equal(target.Y,row.Point.Y,6);Assert.Equal(target.Z,row.Point.Z,6);
    }
    [Fact] public void SpherePeriodicUDoesNotRelaxTheNonperiodicLatitudeDomain() {
        using var surface=Surface(AnalyticConstructionSurface.Sphere);
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(4,0,0),6,6.8,-2,2));
        double u=6.4,v=.2;var result=CurveComputations.Project(surface,new(4*Math.Cos(v)*Math.Cos(u),4*Math.Cos(v)*Math.Sin(u),4*Math.Sin(v)),6,6.8,-.5,.5);
        Assert.True(result.IsDone);var row=result.Candidates[result.UniqueNearestIndex()!.Value];Assert.Equal(u,row.U,6);Assert.Equal(v,row.V,6);
    }
    [Fact] public void MoreThanOnePeriodAndInvalidScalarsAreRejectedBeforePublishing() {
        using var surface=Surface(AnalyticConstructionSurface.Torus);
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(3,0,0),0,7,0,1));
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(3,0,0),0,1,0,7));
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(3,0,0),6,6,0,1));
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(3,0,0),double.NaN,7,0,1));
        Assert.Throws<ArgumentException>(()=>CurveComputations.Project(surface,new(double.PositiveInfinity,0,0),6,6.8,0,1));
    }
    [Fact] public void NoOrthogonalSolutionDoesNotInventAWindowEndpoint() {
        using var surface=Surface(AnalyticConstructionSurface.Cylinder);
        var result=CurveComputations.Project(surface,new(0,4,.3),6,6.8,0,1);
        Assert.False(result.IsDone);Assert.Empty(result.Candidates);
    }
    [Fact] public void ConcurrentIndependentSeamWindowsAndClosedOwnersRemainSafe() {
        Parallel.For(0,8,i=>{using var surface=Surface(AnalyticConstructionSurface.Cylinder);double u=6.3+.01*i;
            var result=CurveComputations.Project(surface,new(4*Math.Cos(u),4*Math.Sin(u),.3),6,6.8,0,1);
            surface.Dispose();Assert.Equal(u,result.Candidates[result.UniqueNearestIndex()!.Value].U,7);
            Assert.Throws<ObjectDisposedException>(()=>CurveComputations.Project(surface,new(4,0,0),6,6.8,0,1));});
    }
}
