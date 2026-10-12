using OcctSharp.CopiedCollections;
using OcctSharp.ModelingWorkspaces;
using OcctSharp.RepairWorkspaces;
namespace OcctSharp.Tests.Shared;
internal static class BatchAPModelingWorkflow {
 internal static void Run(){
  using var a=ModelingBuilders.Edge(new(0,0,0),new(4,0,0));using var b=ModelingBuilders.Edge(new(4,0,0),new(4,3,0));using var c=ModelingBuilders.Edge(new(4,3,0),new(0,3,0));using var d=ModelingBuilders.Edge(new(0,3,0),new(0,0,0));
  using var e1=a.Shape;using var e2=b.Shape;using var e3=c.Shape;using var e4=d.Shape;using var set=OwnedShapeSet.Create(e1,e2,e3,e4);using var result=ModelingBuilders.Wire(set);using var wire=result.Shape;
  if(!result.IsDone||result.Error!=0||wire.CountSubShapes(ShapeKind.Edge)!=4)throw new InvalidOperationException("AP private wire construction failed.");
  set.Dispose();e1.Dispose();e2.Dispose();e3.Dispose();e4.Dispose();result.Dispose();
  using var face=ShapeFactory.CreatePlanarFace(wire);using var contents=ModelingBuilders.Contents(face);if(contents.Counts["NbFaces"]!=1||contents.Counts["NbWires"]!=1)throw new InvalidOperationException("AP contents count failed.");
  var bounds=face.GetSubShapes(ShapeKind.Wire);try{var local=ModelingBuilders.CheckLacking(bounds[0],face,1);if(local.Found)throw new InvalidOperationException("Valid AP wire unexpectedly lacks a pcurve connection.");}finally{foreach(var bound in bounds)bound.Dispose();}
  using var repair=TopologyRepairSession.Create(face,RepairSessionKind.Face);using var loops=repair.FixFaceLoops();using var snapshot=repair.Snapshot();using var repaired=snapshot.Shape;
  face.Dispose();repair.Dispose();snapshot.Dispose();contents.Dispose();if(!repaired.IsValid||Math.Abs(repaired.InspectProperties(InspectionPropertyKind.Area).Mass-12)>1e-8)throw new InvalidOperationException("AP repair composition failed.");
  foreach(bool step in new[]{true,false}){string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AP."+Guid.NewGuid()+(step?".step":".iges"));try{if(step)ShapeExchange.WriteStep(repaired,path);else ShapeExchange.WriteIges(repaired,path);using var read=step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);if(!read.IsValid||Math.Abs(read.InspectProperties(InspectionPropertyKind.Area).Mass-12)>1e-5)throw new InvalidOperationException("AP independent readback failed.");}finally{File.Delete(path);}}
 }
}
