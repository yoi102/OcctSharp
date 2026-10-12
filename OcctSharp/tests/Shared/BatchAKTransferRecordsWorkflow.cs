using OcctSharp.CopiedCollections;
using OcctSharp.TransferRecords;
namespace OcctSharp.Tests.Shared;
internal static class BatchAKTransferRecordsWorkflow {
 static readonly int[] ResultCounts=[1,2];
 internal static void Run(){foreach(var format in new[]{ExchangeCollectionFormat.Step,ExchangeCollectionFormat.Iges})Run(format);}
 internal static void Run(ExchangeCollectionFormat format){
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AK."+Guid.NewGuid().ToString("N")+(format==ExchangeCollectionFormat.Step?".step":".iges"));
  try{
   using(var box=ShapeFactory.CreateBox(2,3,4)){if(format==ExchangeCollectionFormat.Step)ShapeExchange.WriteStep(box,path);else ShapeExchange.WriteIges(box,path);}
   using var session=TransferRecordSession.Read(path,format);using var snapshot=session.TransferRoots();using var repeat=session.Snapshot();
   if(snapshot.TransferredCount<=0 || snapshot.Records.Count==0 || snapshot.Mappings.Count==0 || snapshot.ShapeCount==0)throw new InvalidOperationException("Empty file transfer records.");
   if(!snapshot.ReaderDone || !snapshot.ReaderCheckOk || snapshot.Mappings.Count!=repeat.Mappings.Count)throw new InvalidOperationException("Unstable transfer status.");
   if(snapshot.Records.Where(x=>x.Recorded).Any(x=>!x.ResultCounts.SequenceEqual(ResultCounts)))throw new InvalidOperationException("Native level-one duplicate main record was lost.");
   var mapped=snapshot.Mappings.First(x=>x.HasShape);using var shape=snapshot.GetShape(mapped.MapIndex)!;using var entity=snapshot.GetEntity(mapped.ModelNumber);
   using var shapes=snapshot.Shapes;using var first=shapes.GetShape(0);using var foreign=ShapeFactory.CreateBox(2,3,4);
   if(!snapshot.FindShape(shape).Contains(mapped.ModelNumber) || snapshot.FindShape(foreign).Count!=0)throw new InvalidOperationException("Exact identity lookup inferred geometric equality.");
   session.Dispose();File.Delete(path);shapes.Dispose();snapshot.Dispose();
   if(!shape.IsValid || !first.IsValid || entity.TypeName.Length==0 || repeat.Mappings.Count==0)throw new InvalidOperationException("Detached result lifetime failed.");
   using var list=OwnedShapeSet.Create(shape,shape);using var binder=TransferShapeBinder.Create(list);using var warning=binder.WithMessage("复核 \"warning\"\n",true);using var failure=warning.WithMessage("failure\\path");
   using var copied=failure.Shapes;binder.Dispose();warning.Dispose();failure.Dispose();list.Dispose();
   if(copied.Count!=2)throw new InvalidOperationException("Binder duplicates or independent storage were lost.");
  }finally{File.Delete(path);}
 }
}
