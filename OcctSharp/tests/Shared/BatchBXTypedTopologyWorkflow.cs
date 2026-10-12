using OcctSharp.ConstructionWorkspaces;
using OcctSharp.CopiedCollections;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchBXTypedTopologyWorkflow {
 internal static GeomBSplineSurface Plane()=>GeomBSplineSurfaceCollections.Create(new CopiedGrid<Coordinates3d>([new(0,0,0),new(0,4,0),new(4,0,0),new(4,4,0)],2,2),new([0.0,1]),new([0.0,1]),new([2,2]),new([2,2]),1,1,false,false);
 internal static Geom2dBSplineCurve Segment()=>Geom2dBSplineCurveCollections.Create(new([new Point2d(.2,.3),new Point2d(.8,.3)]),new([0.0,1]),new([2,2]),1,false);
#if !DIRECT_MODULE_CONSUMER
 internal static SketchCurveChain2d Rectangle(double a,double b,double c,double d)=>SketchCurveChain2d.Create([SketchCurve2d.Segment(new(a,b),new(c,b)),SketchCurve2d.Segment(new(c,b),new(c,d)),SketchCurve2d.Segment(new(c,d),new(a,d)),SketchCurve2d.Segment(new(a,d),new(a,b))],true);
#endif
 internal static void Run(){
  using var surface=Plane();using var curve=Segment();using var result=ParameterizedConstruction.PCurveEdge(curve,surface,0,1);using var edge=result.CopyShape();
  var paired=result.EvaluatePCurve(.5);if(!result.IsDone||!result.HasStoredPCurve||Math.Abs(paired.Point.X-2)>1e-6||Math.Abs(paired.Point.Y-1.2)>1e-6||Math.Abs(paired.UV.X-.5)>1e-6)throw new InvalidOperationException("BX typed pcurve mapping failed");
  using var face=BatchAGSurfaceInterpolationWorkflow.Face(surface);
#if DIRECT_MODULE_CONSUMER
  using var holed=BatchBXRawTopologyViewer.TrimFace(face);
#else
  var profile=SketchProfile2d.Create(Rectangle(0,0,1,1),[Rectangle(.25,.25,.75,.75)]);using var holed=SurfaceModeling.CreateTrimmedFace(face,profile);
#endif
  surface.SetPole(1,1,new Point3d(90,90,90));curve.SetPole(1,new Point2d(9,9));surface.Dispose();curve.Dispose();face.Dispose();
  if(Math.Abs(result.EvaluatePCurve(.5).Point.X-2)>1e-6||!edge.IsValid||!holed.IsValid||Math.Abs(holed.InspectProperties(InspectionPropertyKind.Area).Mass-12)>1e-5)throw new InvalidOperationException("BX source mutation/disposal changed independent geometry");
  var wires=holed.GetSubShapes(ShapeKind.Wire);try{if(wires.Length!=2)throw new InvalidOperationException("BX outer/hole topology missing");}finally{foreach(var wire in wires)wire.Dispose();}
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BX."+Guid.NewGuid()+".step");try{ShapeExchange.WriteStep(holed,path);using var read=ShapeExchange.ReadStep(path);if(!read.IsValid||read.FaceCount!=1||Math.Abs(read.InspectProperties(InspectionPropertyKind.Area).Mass-12)>1e-4)throw new InvalidOperationException("BX holed STEP geometry readback failed");BatchBXRawTopologyViewer.Run(path);}finally{File.Delete(path);}
  Console.WriteLine("BX typed topology workflow passed: independent pcurve/face, source mutation/closure, two wires, STEP area12.");
 }
}
