using OcctSharp.CopiedCollections;
using OcctSharp.FillingWorkspaces;
using OcctSharp.TopologyAssembly;
using OcctSharp.Values;
using OcctSharp.RepairWorkspaces;
namespace OcctSharp.Tests.Shared;
internal static class BatchAWGeometryWorkflow {
 internal static GeomBSplineCurve Section(double z)=>GeomBSplineCurveCollections.Create(new(new[]{new Coordinates3d(0,0,z),new Coordinates3d(1,0,z),new Coordinates3d(2,0,z),new Coordinates3d(3,0,z)}),new([0.0,1]),new([4,4]),3,false);
 internal static void Run(){
  using var a=Section(0);using var b=Section(1);using var c=Section(2);using var d=Section(3);
  using var loft=SurfaceFilling.Loft(new[]{a,b,c,d});if(!loft.IsDone)throw new InvalidOperationException("AW section loft did not complete.");
  using var surface=loft.CreateSurface();using var independent=loft.CreateSurface();using var face=BatchAGSurfaceInterpolationWorkflow.Face(surface);var sample=surface.EvalD0(.5,.5);if(!double.IsFinite(sample.X)||!face.IsValid)throw new InvalidOperationException("AW independent loft geometry invalid.");
  loft.Dispose();a.Dispose();b.Dispose();c.Dispose();d.Dispose();surface.Dispose();if(!double.IsFinite(independent.EvalD0(.5,.5).Z))throw new InvalidOperationException("AW surface depended on disposed owners.");
  using var repair=TopologyRepairSession.Create(face,RepairSessionKind.Face);using var loops=repair.FixFaceLoops();using var snapshot=repair.Snapshot();using var restored=snapshot.Shape;
  foreach(bool step in new[]{true,false}){string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AW."+Guid.NewGuid()+(step?".step":".iges"));try{if(step)ShapeExchange.WriteStep(restored,path);else ShapeExchange.WriteIges(restored,path);using var read=step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);if(!read.IsValid||read.FaceCount!=1)throw new InvalidOperationException("AW actual surface readback failed.");}finally{File.Delete(path);}}
  using var pathCurve=Section(0);using var edge=TopologyAssembler.Edge(pathCurve,0,1);pathCurve.Dispose();if(!edge.IsValid||edge.CountSubShapes(ShapeKind.Vertex)!=2)throw new InvalidOperationException("AW controlled edge assembly failed.");
  using var vertex=TopologyAssembler.Vertex(1,2,3);if(!vertex.IsValid)throw new InvalidOperationException("AW initialized vertex failed.");
 }
}
