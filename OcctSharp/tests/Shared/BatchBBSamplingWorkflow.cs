using OcctSharp.CopiedCollections;
using OcctSharp.CurveSampling;
using OcctSharp.TopologyAssembly;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchBBSamplingWorkflow {
 internal static GeomBSplineCurve Arc()=>GeomBSplineCurveCollections.Create(new(new Coordinates3d[]{new(0,0,0),new(0,2,0),new(2,2,0),new(2,0,0)}),new([0.0,1]),new([4,4]),3,false);
 internal static GeomBSplineSurface Plane()=>GeomBSplineSurfaceCollections.Create(new(new Coordinates3d[]{new(0,0,0),new(0,3,0),new(4,0,0),new(4,3,0)},2,2),new([0.0,1]),new([0.0,1]),new([2,2]),new([2,2]),1,1,false,false);
 internal static void Run(){using var curve=Arc();var samples=CurveComputations.Uniform(curve,0,1,17);if(!samples.IsDone||samples.Points.Count!=17||samples.Length<=2)throw new InvalidOperationException("BB uniform copied samples failed.");var deflection=CurveComputations.Deflection(curve,0,1,.01);if(!deflection.IsDone||deflection.Points.Count<3)throw new InvalidOperationException("BB bounded SDK deflection failed.");using var edge=TopologyAssembler.Edge(curve,0,1);curve.Dispose();if(!edge.IsValid||!double.IsFinite(samples.Points[8].Y))throw new InvalidOperationException("BB sampling/modeling copy lifetime failed.");using var surface=Plane();var projection=CurveComputations.Project(surface,new Coordinates3d(2,1.5,2),.1,.9,.1,.9);surface.Dispose();if(!projection.IsDone||projection.UniqueNearestIndex() is null||Math.Abs(projection.MinimumDistance-2)>1e-7)throw new InvalidOperationException("BB copied rectangle projection failed.");}
}
