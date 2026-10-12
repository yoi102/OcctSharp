using OcctSharp.ModelingResults;
using OcctSharp.TopologyAssembly;
namespace OcctSharp.Tests.Shared;
internal static class BatchBGModelingWorkflow {
 internal static Shape Square(double z=0)=>ShapeFactory.CreatePolygonWire([new(0,0,z),new(4,0,z),new(4,4,z),new(0,4,z)],true);
 internal static void Run(){
  using var curve=BatchAWGeometryWorkflow.Section(0);using var edge=TopologyAssembler.Edge(curve,0,1);using var lower=ShapeFactory.CreateWire([edge]);using var moved=edge.Transformed(ShapeTransform.CreateTranslationAndRotationZ(0,0,3,0));using var upper=ShapeFactory.CreateWire([moved]);
  using var loft=ModelingResultComputations.Loft([lower,upper],new(Ruled:true));if(!loft.IsDone||loft.LoftConfiguration!.MutableInput)throw new InvalidOperationException("BG curve-wire loft configuration failed: "+loft.Message);
  using var sheet=loft.CopyResult();loft.Dispose();curve.Dispose();lower.Dispose();upper.Dispose();if(!sheet.IsValid||sheet.FaceCount!=1)throw new InvalidOperationException("BG copied loft lifetime failed.");
  using var baseWire=Square();using var raised=Square(3);using var solid=ModelingResultComputations.Loft([baseWire,raised],new(Solid:true,Ruled:true));if(!solid.IsDone)throw new InvalidOperationException("BG solid loft failed: "+solid.Message);using var body=solid.CopyResult();
  using var otherBase=ShapeFactory.CreateBox(3,3,6);using var other=otherBase.Transformed(ShapeTransform.CreateTranslationAndRotationZ(2,1,-1,0));using var section=ModelingResultComputations.Section(body,other);if(!section.IsDone||section.ResultEdgeCount==0)throw new InvalidOperationException("BG exact-source section failed: "+section.Message);
  foreach(var link in section.Sources.Where(x=>x.State==ModelingSourceState.Exact)){using var left=section.CopyInput(0,link.SourceTopologyOrdinal);using var right=section.CopyInput(1,link.SecondSourceTopologyOrdinal);if(left.Kind!=ShapeKind.Face||right.Kind!=ShapeKind.Face)throw new InvalidOperationException("BG exact ancestor domain failed.");}
  using var offset=ModelingResultComputations.Offset([baseWire],.5,new(Join:2));if(!offset.IsDone)throw new InvalidOperationException("BG configured offset failed: "+offset.Message);
  foreach(bool step in new[]{true,false}){string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BG."+Guid.NewGuid()+(step?".step":".iges"));try{if(step)ShapeExchange.WriteStep(body,path);else ShapeExchange.WriteIges(body,path);using var read=step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);if(!read.IsValid||read.FaceCount!=6)throw new InvalidOperationException("BG independent actual file readback failed.");}finally{File.Delete(path);}}
 }
}
