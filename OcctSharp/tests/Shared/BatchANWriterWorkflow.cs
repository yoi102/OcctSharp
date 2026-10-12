using OcctSharp.CopiedCollections;
using OcctSharp.TransferRecords;
namespace OcctSharp.Tests.Shared;
internal static class BatchANWriterWorkflow {
 internal static void Run(){Run(WriterTransferFormat.Step);Run(WriterTransferFormat.Iges);}
 internal static void Run(WriterTransferFormat format){
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AN."+Guid.NewGuid()+(format==WriterTransferFormat.Step?".step":".iges"));
  try{
   using var box=ShapeFactory.CreateBox(2,3,4);using var moved=box.Transformed(ShapeTransform.CreateTranslation(8,0,0));using var set=OwnedShapeSet.Create(box,moved);
   using var writer=WriterTransferSession.Create(set,format);box.Dispose();moved.Dispose();set.Dispose();
   var snapshot=writer.TransferAndWrite(path);var repeat=writer.Snapshot();writer.Dispose();
   if(!snapshot.WriteSucceeded || snapshot.Sources.Count!=2 || snapshot.Sources.Any(x=>!x.TransferSucceeded) || snapshot.EntityTypes.Count==0 || !File.Exists(path) || new FileInfo(path).Length==0)throw new InvalidOperationException("Writer export failed.");
   if(System.Text.Json.JsonSerializer.Serialize(snapshot)!=System.Text.Json.JsonSerializer.Serialize(repeat))throw new InvalidOperationException("Unstable writer snapshot.");
   foreach(var source in snapshot.Sources){if(source.MapIndex is {} index && snapshot.GetMapping(index).MapIndex!=index)throw new InvalidOperationException("Foreign finder map.");foreach(var number in source.Results.ModelNumbers)if(snapshot.GetEntityType(number).Length==0)throw new InvalidOperationException("Missing exact model entity.");}
   using var read=format==WriterTransferFormat.Step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);
   if(!read.IsValid || read.FaceCount<12)throw new InvalidOperationException("Independent writer file readback failed.");
  }finally{File.Delete(path);}
 }
}
