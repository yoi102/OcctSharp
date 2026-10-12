using OcctSharp.CopiedCollections;
using OcctSharp.Geometry.Workspaces;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchAMSplineWorkflow {
 static void Near(double e,double a){if(Math.Abs(e-a)>2e-8)throw new InvalidOperationException("AM independent geometry mismatch.");}
 internal static void Run(){
  foreach(bool weighted in new[]{false,true}){
   double[] u=[0,.5,1],v=[0,1];var samples=u.SelectMany(x=>v.SelectMany(y=>new[]{x,y,x*x+y})).ToArray();
   double[]? weights=weighted?u.SelectMany(x=>v.Select(y=>1+x)).ToArray():null;
   var solved=SplineWorkspaceInterpolation.Surface([0,0,0,1,1,1],2,u,[0,0,1,1],1,v,new(samples,6,3),weights);
   var poles=new CopiedGrid<Coordinates3d>(Enumerable.Range(1,6).Select(i=>new Coordinates3d(solved[i,1],solved[i,2],solved[i,3])).ToArray(),3,2);
   using var surface=weighted?GeomBSplineSurfaceCollections.Create(poles,new(Enumerable.Range(1,6).Select(i=>solved[i,4]).ToArray(),3,2),new([0.0,1]),new([0.0,1]),new([3,3]),new([2,2]),2,1,false,false):GeomBSplineSurfaceCollections.Create(poles,new([0.0,1]),new([0.0,1]),new([3,3]),new([2,2]),2,1,false,false);
   using var workspace=SplineWorkspace.Cache(surface,.5,.5);
   foreach(double x in u)foreach(double y in v){var p=surface.EvalD0(x,y);Near(x,p.X);Near(y,p.Y);Near(x*x+y,p.Z);}
   for(int i=0;i<=20;++i){double x=i/20.0,y=1-x;var p=surface.EvalD0(x,y);var d=surface.EvalDN(x,y,1,0);var m=workspace.EvaluateSurface(x,y,2);Near(p.X,m[0,1]);Near(p.Y,m[0,2]);Near(p.Z,m[0,3]);Near(d.X,m[1,1]);Near(d.Z,m[1,3]);}
   using var face=BatchAGSurfaceInterpolationWorkflow.Face(surface);surface.Dispose();var detached=workspace.EvaluateSurface(.3,.7,2);workspace.Dispose();Near(.3,detached[0,1]);
   string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AM."+Guid.NewGuid()+".step");try{ShapeExchange.WriteStep(face,path);using var read=ShapeExchange.ReadStep(path);if(!read.IsValid||read.FaceCount!=1)throw new InvalidOperationException("AM surface STEP readback failed.");}finally{File.Delete(path);}
  }
  double[] t=[0,1.0/3,2.0/3,1];var cp=SplineWorkspaceInterpolation.Curve([0,0,0,0,1,1,1,1],3,t,[0,0,0,0],new(t.SelectMany(x=>new[]{x,x*x,x*x*x}).ToArray(),4,3));
  using var curve=GeomBSplineCurveCollections.Create(new(Enumerable.Range(1,4).Select(i=>new Coordinates3d(cp[i,1],cp[i,2],cp[i,3])).ToArray()),new([0.0,1]),new([4,4]),3,false);
  using var cache=SplineWorkspace.Cache(curve,.5);for(int i=0;i<=20;++i){double x=i/20.0;var m=cache.EvaluateCurve(x,3);Near(x,m[0,1]);Near(x*x,m[0,2]);Near(x*x*x,m[0,3]);Near(6,m[3,3]);}
  using var edge=BatchAGSurfaceInterpolationWorkflow.Edge(curve);if(!edge.IsValid)throw new InvalidOperationException("AM interpolated edge failed.");
 }
}
