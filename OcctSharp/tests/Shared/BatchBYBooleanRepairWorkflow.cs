namespace OcctSharp.Tests.Shared;
internal static class BatchBYBooleanRepairWorkflow {
 static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("BY "+message);}
 static void Close(double actual,double expected)=>Require(double.IsFinite(actual)&&Math.Abs(actual-expected)<1e-5,$"volume differs:{actual:R}/{expected:R}");
 static string Fingerprint(Shape shape){using var snapshot=RepairSnapshot.Create(shape);return snapshot.Fingerprint;}
 static Shape Translate(Shape source,double x,double y,double z){using var transform=GpTrsf.Create(x,y,z);return source.Transformed(transform);}
 internal static void FailureDiagnostics(){
  using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var box=ShapeFactory.CreateBox(1,1,1);using var source=RepairSnapshot.Create(box);using var tool=RepairSnapshot.Create(edge);using var result=BooleanHistoryModeling.Build(TopologyBooleanOperation.Cut,[source,tool]);
  Require(!result.Diagnostics.AlgorithmDone&&!result.Diagnostics.ShapeIsValid&&result.Shape is null&&result.History.Count==0,"SDK failure published geometry/history");
  string text=result.Diagnostics.Message;Require(text.Contains("Errors:",StringComparison.Ordinal)&&text.Contains("BOPAlgo_",StringComparison.Ordinal),"actual SDK diagnostic text missing");
  bool rejected=false;try{result.RequireShape();}catch(InvalidOperationException){rejected=true;}Require(rejected,"failed result accepted");result.Dispose();source.Dispose();tool.Dispose();Require(text.Length>50&&text==result.Diagnostics.Message,"copied diagnostic depends on disposed inputs/result");
 }
 internal static void WarningDiagnostics(){
  using var box=ShapeFactory.CreateBox(1,1,1);using var seed=RepairSnapshot.Create(box);using var emptyResult=BooleanHistoryModeling.Build(TopologyBooleanOperation.Cut,[seed,seed]);using var source=RepairSnapshot.Create(emptyResult.RequireShape());using var tool=RepairSnapshot.Create(box);using var result=BooleanHistoryModeling.Build(TopologyBooleanOperation.Fuse,[source,tool]);
  Close(result.RequireShape().InspectProperties(InspectionPropertyKind.Volume).Mass,1);string text=result.Diagnostics.Message;Require(text.Contains("Warnings:",StringComparison.Ordinal)&&text.Contains("BOPAlgo_AlertEmptyShape",StringComparison.Ordinal),"actual SDK warning text missing");result.Dispose();source.Dispose();tool.Dispose();Require(text==result.Diagnostics.Message,"copied warning depends on disposed native owners");
 }
 internal static void Run(TopologyBooleanOperation operation){
  using var box=ShapeFactory.CreateBox(10,10,10);using var tool=ShapeFactory.CreateBox(2,12,12);using var cutter=Translate(tool,4,-1,-1);using var remote=Translate(box,30,0,0);
  using var a=RepairSnapshot.Create(box);using var b=RepairSnapshot.Create(remote);using var c=RepairSnapshot.Create(cutter);using var d=RepairSnapshot.Create(remote);
  RepairSnapshot[] inputs=operation==TopologyBooleanOperation.Cut?[a,b,c]:[a,b,c,d];string[] fingerprints=inputs.Select(s=>s.Fingerprint).ToArray();
  using var boolean=BooleanHistoryModeling.Build(operation,inputs,2);double finalVolume=operation switch{TopologyBooleanOperation.Fuse=>1088,TopologyBooleanOperation.Cut=>800,_=>200};Close(boolean.RequireShape().InspectProperties(InspectionPropertyKind.Volume).Mass,finalVolume+1000);
  Require(boolean.Diagnostics.GroupSupport.HasFlag(LocalFeatureGroupSupport.Evolution)&&!boolean.Diagnostics.HasComposedHistory,"exact Boolean evolution unavailable");
  for(int i=0;i<inputs.Length;i++){Require(boolean.History.Any(h=>h.Source is {} source&&source.PlanId==boolean.PlanId&&source.ArgumentIndex==i),"argument/tool provenance slot missing");using var copy=inputs[i].CopyShape();Require(Fingerprint(copy)==fingerprints[i],"Boolean changed source snapshot");}
  bool split=boolean.History.Where(h=>h.Source is {ArgumentIndex:0,Kind:ShapeKind.Face}&&h.Kind==LocalFeatureHistoryKind.Modified).GroupBy(h=>h.Source).Any(g=>g.Select(h=>h.ResultTopologyIndex).Where(i=>i.HasValue).Distinct().Count()>1);
  bool shared=boolean.History.Where(h=>h.ResultTopologyIndex.HasValue&&h.Source is {ArgumentIndex:1 or 3}).GroupBy(h=>h.ResultTopologyIndex).Any(g=>g.Select(h=>h.Source!.Value.ArgumentIndex).Distinct().Count()>1);
  bool deleted=boolean.History.Any(h=>h.Kind==LocalFeatureHistoryKind.Deleted);
  if(operation==TopologyBooleanOperation.Cut)Require(split&&deleted,"actual split/deleted history missing");if(operation==TopologyBooleanOperation.Common)Require(shared,"actual shared target history missing");
  foreach(var input in inputs)input.Dispose();box.Dispose();tool.Dispose();cutter.Dispose();remote.Dispose();Require(boolean.RequireShape().IsValid&&boolean.History.Where(h=>h.Shape is not null).All(h=>h.Shape!.IsValid),"Boolean output depends on source lifetime");
  using var source=RepairSnapshot.Create(boolean.RequireShape());RepairSelection? selection=null;
  // This chooses the requested edit in the current snapshot. It does not map an
  // old identity, infer a history path, or select a successor by geometry.
  foreach(var item in source.Topology.Where(t=>t.Kind==ShapeKind.Solid)){using var solid=source.CopySubshape(item.Selection);if(solid.InspectProperties(InspectionPropertyKind.Volume).CenterOfMass.X>20){Require(selection is null,"remote edit selection is ambiguous");selection=item.Selection;}}
  Require(selection.HasValue,"explicit remote solid selection missing");var edit=new TopologyEditRepair([new(selection!.Value,null)]);var plan=new RepairPlan(source,[new("unify",new SameDomainUnificationRepair()),new("remove-explicit-solid",edit)]);
  using var preview=ShapeRepair.Preview(source,plan);Require(preview.Completed&&preview.CanAccept&&preview.Stages.All(s=>s.State==RepairStageState.Completed),"repair/edit plan failed:"+string.Join(";",preview.Stages.Select(s=>s.Message)));Require(preview.History.Any(h=>h.Source==selection.Value&&h.Kind==RepairRelationKind.Deleted&&h.Result is null),"explicit deletion history missing");
  using var current=source.CopyShape();Require(Fingerprint(current)==source.Fingerprint,"repair changed source");using var accepted=preview.Accept();Close(accepted.InspectProperties(InspectionPropertyKind.Volume).Mass,finalVolume);
  using var foreign=RepairSnapshot.Create(accepted);bool foreignRejected=false;try{_ =new RepairPlan(foreign,[new("foreign",edit)]);}catch(ArgumentException){foreignRejected=true;}Require(foreignRejected,"foreign selection accepted");
  var stale=new RepairSelection(source.Identity with{Revision=source.Identity.Revision+1},selection.Value.Index);bool staleRejected=false;try{_ =new RepairPlan(source,[new("stale",new TopologyEditRepair([new(stale,null)]))]);}catch(ArgumentException){staleRejected=true;}Require(staleRejected,"stale revision accepted");
  source.Dispose();boolean.Dispose();preview.Dispose();Require(accepted.IsValid,"accepted edit depends on Boolean/repair owners");string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BY."+Guid.NewGuid()+".step");
  try{ShapeExchange.WriteStep(accepted,path);using var reopened=ShapeExchange.ReadStep(path);double actual=reopened.InspectProperties(InspectionPropertyKind.Volume).Mass;Close(actual,finalVolume);Require(reopened.IsValid&&reopened.CountSubShapes(ShapeKind.Solid)==(operation==TopologyBooleanOperation.Cut?2:1),"actual STEP topology differs");
   using var readSnapshot=RepairSnapshot.Create(reopened);bool readRejected=false;try{_ =new RepairPlan(readSnapshot,[new("old-file-identity",edit)]);}catch(ArgumentException){readRejected=true;}Require(readRejected,"STEP reopen silently rebound an old selection");
   BatchBYRawTopologyViewer.Run(path,operation,actual);
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BY_EVIDENCE") is {} root){string output=Path.Combine(root,operation.ToString().ToLowerInvariant());Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"workflow-result.json"),System.Text.Json.JsonSerializer.Serialize(new{state="PASS",operation=operation.ToString(),inputs=inputs.Length,argumentCount=2,plan=boolean.PlanId,split,shared,deleted,sourcePreserved=true,foreignRejected,staleRejected,reopenedIdentityRejected=readRejected,explicitDeletion=true,independentLifetime=true,stepVolume=actual,stepSolids=reopened.CountSubShapes(ShapeKind.Solid)}));}
  }finally{File.Delete(path);}
  Console.WriteLine($"BY {operation} Boolean/repair workflow passed: exact source slots/all paths, explicit solid deletion, scoped identities, STEP volume{finalVolume}, actual HWND selection/invalidation.");
 }
 internal static void Run(){FailureDiagnostics();WarningDiagnostics();foreach(var operation in new[]{TopologyBooleanOperation.Fuse,TopologyBooleanOperation.Cut,TopologyBooleanOperation.Common})Run(operation);}
}
