using OcctSharp.FoundationValues;
using OcctSharp.Generated;
using OcctSharp.OwnedValues;
namespace OcctSharp.Tests.Shared;
internal static class BatchAVFoundationWorkflow {
 internal static void Run(){
  var copied=SdkValues.Text("零件😀.step");if(copied.Text!="零件😀.step"||copied.Length!=9)throw new InvalidOperationException("AV Unicode projection failed.");
  var ascii=SdkValues.Text(" part ",TextOperation.TrimStart,unicode:false);ascii=SdkValues.Text(ascii.Text,TextOperation.TrimEnd,unicode:false);
  var path=SdkValues.Path("C:\\models\\part.step",PathOperation.SetName,ascii.Text,system:9);
  if(path.Name!="part"||path.Extension!=".step")throw new InvalidOperationException("AV pure path projection failed.");
  var units=SdkValues.ConvertUnits(25.4,"mm","m");if(Math.Abs(units.Value-0.0254)>1e-12||units.Exponents[1]!=1)throw new InvalidOperationException("AV explicit unit conversion failed.");
  var color=SdkValues.ColorFromHex("#336699");using(var owner=color.CreateOwner()){
   if(Math.Abs(owner.Red()-color.Red)>1e-6)throw new InvalidOperationException("AV independent color owner failed.");
  }
  var rgb=SdkValues.ConvertColor(ColorConversion.LinearToSrgb,color.Red,color.Green,color.Blue);
  if(Math.Abs(rgb[0]-0.2)>1e-5||Math.Abs(rgb[1]-0.4)>1e-5||Math.Abs(rgb[2]-0.6)>1e-5)throw new InvalidOperationException("AV color-space round trip failed.");
  using var shape=ShapeFactory.CreateBox(units.Value,units.Value*2,units.Value*3);if(!shape.IsValid)throw new InvalidOperationException("AV normalized modeling inputs failed.");
  if(copied.Text!="零件😀.step"||path.Name!="part"||color.Hex.Length==0)throw new InvalidOperationException("AV copied results changed after owner disposal.");
 }
}
