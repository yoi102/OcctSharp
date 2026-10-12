using OcctSharp.FillingWorkspaces;
using OcctSharp.Geometry.Workspaces;
namespace OcctSharp.Tests.Shared;
internal static class BatchBWNonuniformFittingWorkflow {
 internal static void Run(){
  double[] parameters=[0,.05,.3,1];var sections=parameters.Select(p=>BatchAWGeometryWorkflow.Section(3*p)).ToArray();
  try{
   using var result=SurfaceFilling.LoftWithParameters(sections,parameters);if(!result.IsDone)throw new InvalidOperationException("BW explicit loft failed");
   using var surface=result.CreateSurface();using var cache=SplineWorkspace.Cache(surface,.5,.5);using var face=BatchAGSurfaceInterpolationWorkflow.Face(surface);
   foreach(double v in new[]{0,.05,.3,.5,1})if(Math.Abs(surface.EvalD0(.5,v).Z-3*v)>1e-5||Math.Abs(surface.EvalDN(.5,v,0,1).Z-3)>1e-5)throw new InvalidOperationException("BW explicit parameter mapping/derivative incorrect");
   var copied=cache.EvaluateSurface(.5,.5,2).ToArray();foreach(var source in sections)source.Dispose();surface.Dispose();result.Dispose();parameters[1]=99;
   if(!face.IsValid||Math.Abs(face.InspectProperties(InspectionPropertyKind.Area).Mass-9)>1e-5||!copied.SequenceEqual(cache.EvaluateSurface(.5,.5,2).ToArray())||result.RequestedSectionParameters[1]!=.05)throw new InvalidOperationException("BW copies retained mutable source state");
   string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BW."+Guid.NewGuid()+".step");try{ShapeExchange.WriteStep(face,path);using var read=ShapeExchange.ReadStep(path);var faces=read.GetFaces();try{if(!read.IsValid||faces.Length!=1||Math.Abs(faces[0].InspectProperties(InspectionPropertyKind.Area).Mass-9)>1e-4)throw new InvalidOperationException("BW loft STEP readback geometry incorrect");}finally{foreach(var f in faces)f.Dispose();}}finally{File.Delete(path);}
   Console.WriteLine("BW nonuniform fitting workflow passed: explicit section mapping/derivatives, independent surface/cache/face and STEP area9.");
  }finally{foreach(var section in sections)section.Dispose();}
 }
}
