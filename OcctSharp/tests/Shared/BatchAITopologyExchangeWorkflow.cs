using OcctSharp.CopiedCollections;
namespace OcctSharp.Tests.Shared;
internal static class BatchAITopologyExchangeWorkflow {
 internal static void Run(){
  using var box=ShapeFactory.CreateBox(10,10,10);using var shifted=box.Transformed(ShapeTransform.CreateTranslation(5,0,0));using var third=box.Transformed(ShapeTransform.CreateTranslation(0,5,0));
  using var arguments=OwnedShapeSet.Create(box,shifted);using var tools=OwnedShapeSet.Create(third);
  using var result=TopologyCollectionResult.Build(arguments,tools,TopologyCollectionOperation.Fuse);
  using var stored=result.Arguments;using var copied=stored.GetShape(0);
  if(stored.Count!=2 || !copied.IsEqual(box))throw new InvalidOperationException("Ordered argument identity was lost.");
  var faces=box.GetFaces();try{using var modified=result.Modified(faces[0]);using var generated=result.Generated(faces[0]);_ = result.IsDeleted(faces[0]);}finally{foreach(var f in faces)f.Dispose();}
  using var shape=result.Shape;result.Dispose();arguments.Dispose();tools.Dispose();box.Dispose();shifted.Dispose();third.Dispose();
  if(!shape.IsValid || !copied.IsValid)throw new InvalidOperationException("Copied topology depends on its algorithm/input owner.");
  using var inspection=RepairSnapshot.Create(shape);if(!inspection.Metrics.IsValid)throw new InvalidOperationException("Result repair inspection failed.");
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AI."+Guid.NewGuid().ToString("N")+".step");
  try{ShapeExchange.WriteStep(shape,path);using var session=ExchangeCollectionSession.Read(path);using var entities=session.Entities;using var roots=session.GiveList();
   if(entities.Count==0 || roots.Count==0)throw new InvalidOperationException("STEP entity/root collection is empty.");
   using var entity=entities.GetEntity(0);if(session.EntityNumber(entity)!=1 || !entity.TypeName.StartsWith("Step",StringComparison.Ordinal))throw new InvalidOperationException("STEP model indices or RTTI were lost.");
   using var graph=session.CreateGraph();using var graphRoots=graph.RootEntities;using var shared=graph.GetShareds(entity);using var sharing=graph.GetSharings(entity);
   using var iterator=RetainedEntityIterator.Create(entities);using var content=iterator.Content;iterator.Dispose();if(content.Count!=entities.Count)throw new InvalidOperationException("Detached iterator content changed.");
   if(session.Transfer(roots,ExchangeCollectionTransfer.Record)<=0)throw new InvalidOperationException("Selected STEP entities did not transfer.");
   var stats=session.GetStatsTransfer(roots);if(stats.Results<=0 || stats.Failures!=0)throw new InvalidOperationException("STEP transfer stats failed.");
   using var recorded=session.RecordedEntities;using var last=session.LastTransferredEntities;using var shapes=session.ShapeResults(true);using var references=session.EntitiesFromShapes(shapes,ExchangeShapeLookup.Mapped);
   if(recorded.Count==0 || last.Count==0 || shapes.Count==0 || references.Count==0)throw new InvalidOperationException("STEP recorded entity/shape collections failed.");
   using var restored=shapes.GetShape(0);session.Dispose();entities.Dispose();roots.Dispose();shapes.Dispose();
   using var retained=content.GetEntity(0);using var stillRoots=graph.RootEntities;
   if(!restored.IsValid || retained.TypeName.Length==0 || stillRoots.Count==0)throw new InvalidOperationException("Exchange snapshot/graph lifetime failed.");
  }finally{File.Delete(path);}
 }
}
