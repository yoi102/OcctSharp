using OcctSharp.IntersectionResults;
using OcctSharp.ModelingResults;
using OcctSharp.TopologyAssembly;
namespace OcctSharp.Tests.Shared;
internal static class BatchBHIntersectionWorkflow {
 internal static Shape Horizontal(double z=0){using var wire=ShapeFactory.CreatePolygonWire([new(-5,-5,z),new(5,-5,z),new(5,5,z),new(-5,5,z)],true);return ShapeFactory.CreatePlanarFace(wire);}
 internal static Shape Vertical(){using var wire=ShapeFactory.CreatePolygonWire([new(1,-5,-5),new(1,5,-5),new(1,5,5),new(1,-5,5)],true);return ShapeFactory.CreatePlanarFace(wire);}
 internal static GeomSurface Spherical(){using var sphere=ShapeFactory.CreateSphere(2);var faces=sphere.GetFaces();try{return RayIntersections.CopySurface(faces[0]);}finally{foreach(var face in faces)face.Dispose();}}
 internal static void Run(){
  using var a=Horizontal(.5);using var first=Spherical();using var second=RayIntersections.CopySurface(a);using var result=SurfaceIntersections.Compute(first,second);if(!result.IsDone||result.Lines.Count!=1)throw new InvalidOperationException("BH coupled surface intersection failed.");using var tuple=result.CopyLine(1);using var source1=result.CopySourceSurface(0);using var source2=result.CopySourceSurface(1);if(tuple.FirstPCurve is null||tuple.SecondPCurve is null)throw new InvalidOperationException("BH coupled pcurves missing for sphere/plane fixture.");
  foreach(double t in new[]{.5,1,1.5}){var p=tuple.Curve.Value(t);var u=BatchBHIntersectionWorkflow.Eval(tuple.FirstPCurve,t);var v=BatchBHIntersectionWorkflow.Eval(tuple.SecondPCurve,t);var x=BatchBHIntersectionWorkflow.Eval(source1,u.X,u.Y);var y=BatchBHIntersectionWorkflow.Eval(source2,v.X,v.Y);if(Distance(p.X,p.Y,p.Z,x.X,x.Y,x.Z)>1e-6||Distance(p.X,p.Y,p.Z,y.X,y.Y,y.Z)>1e-6)throw new InvalidOperationException("BH paired pcurve residual failed.");}
  using var edge=TopologyAssembler.Edge(tuple.Curve,.5,1.5);result.Dispose();first.Dispose();second.Dispose();a.Dispose();if(!edge.IsValid)throw new InvalidOperationException("BH intersection topology lifetime failed.");
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BH."+Guid.NewGuid()+".step");try{ShapeExchange.WriteStep(edge,path);using var restored=ShapeExchange.ReadStep(path);if(!restored.IsValid)throw new InvalidOperationException("BH actual STEP readback failed.");}finally{File.Delete(path);}
  using var lower=BatchBGModelingWorkflow.Square();using var upper=BatchBGModelingWorkflow.Square(3);using var loft=ModelingResultComputations.Loft([lower,upper],new(Solid:true,Ruled:true));using var solid=loft.CopyResult();using var hit=RayIntersections.Line(solid,new(-1,2,1),new(1,0,0),0,6);if(!hit.IsDone||hit.Hits.Count!=2)throw new InvalidOperationException("BH finite ray through BG loft failed.");foreach(var h in hit.Hits){using var face=hit.CopyFace(h.FaceOrdinal);using var surface=RayIntersections.CopySurface(face);var p=BatchBHIntersectionWorkflow.Eval(surface,h.U,h.V);if(Distance(p.X,p.Y,p.Z,h.Point.X,h.Point.Y,h.Point.Z)>1e-6)throw new InvalidOperationException("BH exact copied hit UV back-evaluation failed.");}
 }
 internal static OcctSharp.Values.Point2d Eval(Geom2dCurve curve,double t){OcctSharp.Values.Point2d point=default;OcctSharp.ReferenceOperations.Geom2dCurveReferences.D0(curve,t,ref point);return point;}
 internal static Point3d Eval(GeomSurface surface,double u,double v){Point3d point=default;OcctSharp.ReferenceOperations.GeomSurfaceReferences.D0(surface,u,v,ref point);return point;}
 internal static double Distance(double x,double y,double z,double a,double b,double c)=>Math.Sqrt((x-a)*(x-a)+(y-b)*(y-b)+(z-c)*(z-c));
}
