using OcctSharp.FoundationValues;
using OcctSharp.Geometry.Constraints;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchBUUnitWorkflow {
 internal static void Run(){
  UnitConversionInput[] input=[new(1,"cm","mm",UnitDimensionKind.Length,1,100),new(.02,"m","mm",UnitDimensionKind.Length,1,100),new(30,"mm","mm",UnitDimensionKind.Length,1,100)];
  var rows=UnitDiagnosticWorkflow.Convert(input);
  var dimensions=new Coordinates3d(rows[0].Converted.Value,rows[1].Converted.Value,rows[2].Converted.Value);
  var copy=CoupledConstraints.Copy([new(new([new(0,0,0),dimensions],Lower:-4))]).Single();
  Array.Fill(input,new(999,"s","s",UnitDimensionKind.Time));
  var size=copy.Positions3d![-3];
  using var shape=ShapeFactory.CreateBox(size.X,size.Y,size.Z);
  if(!shape.IsValid||shape.CountSubShapes(ShapeKind.Face)!=6)throw new InvalidOperationException("BU normalized box topology failed.");
  var path=Path.Combine(Path.GetTempPath(),$"OcctSharp.BU.{Guid.NewGuid():N}.step");
  try{
   ShapeExchange.WriteStep(shape,path);using var reread=ShapeExchange.ReadStep(path);var box=reread.GetBoundingBox();
   if(!reread.IsValid||Math.Abs(box.SizeX-10)>1e-5||Math.Abs(box.SizeY-20)>1e-5||Math.Abs(box.SizeZ-30)>1e-5)
    throw new InvalidOperationException("BU converted STEP geometry differs.");
  }finally{File.Delete(path);}
  shape.Dispose();
  if(rows[0].Input.FromUnit!="cm"||rows[0].Converted.Value!=10||rows[0].Diagnostic.Complete!="row=1; value=10; unit=mm"
     ||copy.Positions3d[-3]!=new Coordinates3d(10,20,30))throw new InvalidOperationException("BU detached values/diagnostics changed.");
  Console.WriteLine("BU unit workflow passed: explicit dimensions/numeric bounds, copied diagnostics, BT constraints and exact STEP box sizes.");
 }
}
