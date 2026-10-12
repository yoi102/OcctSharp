using OcctSharp.AssemblyDiagnostics;
using OcctSharp.CopiedCollections;
using OcctSharp.RepairWorkspaces;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchBAAssemblyWorkflow {
 internal static GeomBSplineSurface Surface()=>GeomBSplineSurfaceCollections.Create(new(new Coordinates3d[]{new(0,0,0),new(0,3,0),new(4,0,0),new(4,3,1)},2,2),new([0.0,1]),new([0.0,1]),new([2,2]),new([2,2]),1,1,false,false);
 internal static readonly Point2d[] Boundary=[new(0,0),new(1,0),new(1,1),new(0,1),new(0,0)];
 internal static void Run(){
  using var surface=Surface();using var result=AssemblyWorkflows.WireOnSurface(surface,Boundary);if(!result.IsDone||result.EdgeCount!=4)throw new InvalidOperationException("BA private pcurve/wire/face assembly failed.");
  using var face=result.Shape;surface.Dispose();result.Dispose();var validation=AssemblyWorkflows.Validate(face);if(!validation.IsValid||!validation.UsesExactMethod||validation.InvalidSubshapeCount!=0)throw new InvalidOperationException("BA copied assembly validation failed.");
  var area=AssemblyWorkflows.WireArea(face);if(area.Found||area.StatusMask==0)throw new InvalidOperationException("BA initialized face/wire area diagnostic failed.");
  using var repair=TopologyRepairSession.Create(face,RepairSessionKind.Face);using var loops=repair.FixFaceLoops();using var snapshot=repair.Snapshot();using var restored=snapshot.Shape;
  foreach(bool step in new[]{true,false}){string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BA."+Guid.NewGuid()+(step?".step":".iges"));try{if(step)ShapeExchange.WriteStep(restored,path);else ShapeExchange.WriteIges(restored,path);using var read=step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);if(!read.IsValid||read.FaceCount!=1)throw new InvalidOperationException("BA repaired assembly real-file readback failed.");}finally{File.Delete(path);}}
 }
}
