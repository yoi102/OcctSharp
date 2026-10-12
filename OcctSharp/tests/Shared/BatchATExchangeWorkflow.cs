using System.Text.Json;
using OcctSharp.CopiedCollections;
using OcctSharp.ExchangeSessions;
using OcctSharp.TransferRecords;
namespace OcctSharp.Tests.Shared;
internal static class BatchATExchangeWorkflow {
 internal static void Run(){Run(false);Run(true);}
 internal static void Run(bool iges){
  string folder=Path.Combine(Path.GetTempPath(),"OcctSharp.AT.Consumer."+Guid.NewGuid());Directory.CreateDirectory(folder);
  string source=Path.Combine(folder,iges?"source.iges":"source.step"),output=Path.Combine(folder,iges?"output.iges":"output.step");
  try{
   using var box=ShapeFactory.CreateBox(2,3,4);using var set=OwnedShapeSet.Create(box);using var writer=WriterTransferSession.Create(set,iges?WriterTransferFormat.Iges:WriterTransferFormat.Step);
   if(!writer.TransferAndWrite(source).WriteSucceeded)throw new InvalidOperationException("AT fixture export failed.");
   using var session=iges?ExchangeFileSession.ReadIges(source):ExchangeFileSession.ReadStep(source);var snapshot=session.Snapshot();string frozen=JsonSerializer.Serialize(snapshot);
   var id=session.EntityId(1);if(snapshot.Entities.Count!=session.ModelCount||session.SelectAll().Entities.Count!=session.ModelCount||session.SelectEntities(id,id).Entities.Count!=1||!session.SelectType(id).Entities.Contains(id)||session.ResolveLabel("1").Unique!=id)throw new InvalidOperationException("AT identity/selection projection failed.");
   File.Delete(source);var write=iges?session.WriteIges(output,new(authorName:"OcctSharp AT",companyName:"copied header")):session.WriteStep(output,new(name:"OcctSharp AT",authors:["copied header"]));
   if(!write.Done||JsonSerializer.Serialize(session.Snapshot())!=frozen)throw new InvalidOperationException("AT export/restoration failed.");
   using var other=iges?ExchangeFileSession.ReadIges(output):ExchangeFileSession.ReadStep(output);var reread=other.Snapshot();
   if(iges?reread.IgesHeader!.AuthorName!="OcctSharp AT":reread.StepHeader!.Name!="OcctSharp AT")throw new InvalidOperationException("AT actual metadata readback failed.");
   using var shape=iges?ShapeExchange.ReadIges(output):ShapeExchange.ReadStep(output);if(!shape.IsValid||shape.FaceCount!=6)throw new InvalidOperationException("AT geometric readback failed.");
   using var transfer=TransferRecordSession.Read(output,iges?ExchangeCollectionFormat.Iges:ExchangeCollectionFormat.Step);using var result=transfer.TransferRoots();if(!result.ReaderDone||result.ShapeCount<1)throw new InvalidOperationException("AT AK transfer reuse failed.");
   session.Dispose();if(snapshot.Entities.Count<1)throw new InvalidOperationException("AT detached snapshot lost data.");
  }finally{Directory.Delete(folder,true);}
 }
}
