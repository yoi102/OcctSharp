using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Geometry.Workspaces;
using OcctSharp.Values;
namespace OcctSharp.Runtime.Tests;
public sealed class SplineWorkspacesTests {
 [Theory][InlineData(2)][InlineData(3)]
 public void NonconstantWeightsRecoverRationalControlPoints(int dimension){
  double[] data=dimension==2?[0,0,2,2.0/3,4,0]:[0,0,0,2,2.0/3,0,4,0,0];
  var result=SplineWorkspaceInterpolation.Curve([0,0,0,1,1,1],2,[0,.5,1],[0,0,0],new(data,3,dimension),[1,1.5,1]);Near(2,result[2,1]);Near(1,result[2,2]);Near(2,result[2,dimension+1]);
  Assert.Throws<ArgumentException>(()=>SplineWorkspaceInterpolation.Curve([0,0,0,1,1,1],2,[0,.5,1],[0,0,0],new(data,3,dimension),[1,.5,1]));
  Assert.Throws<ArgumentException>(()=>SplineWorkspaceInterpolation.Curve([0,0,0,1,1,1],2,[0,.5,1],[0,1,0],new(data,3,dimension),[1,1,1]));
 }
 [Theory][InlineData(false)][InlineData(true)]
 public void VPeriodicUnequalDegreesAndCopiedCoefficientReconstruction(bool rational){
  var p=new CopiedGrid<Coordinates3d>(Enumerable.Range(0,2).SelectMany(i=>Enumerable.Range(0,4).Select(j=>new Coordinates3d(Math.Cos(j*Math.PI/2),Math.Sin(j*Math.PI/2),i))).ToArray(),2,4);var w=new CopiedGrid<double>([1,2,1,2,1,2,1,2],2,4);
  using var s=rational?GeomBSplineSurfaceCollections.Create(p,w,new([0.0,1]),new([0.0,1,2,3,4]),new([2,2]),new([1,1,1,1,1]),1,2,false,true):GeomBSplineSurfaceCollections.Create(p,new([0.0,1]),new([0.0,1,2,3,4]),new([2,2]),new([1,1,1,1,1]),1,2,false,true);
  using var cache=SplineWorkspace.Cache(s,.5,8.2);for(int i=0;i<=8;++i){double v=i/8.0;Point(s.EvalD0(.4,v),cache.EvaluateSurface(.4,v-8,2));}
  using var b=rational?GeomBezierSurfaceCollections.Create(p,w):GeomBezierSurfaceCollections.Create(p);using var c=SplineWorkspace.Coefficients(b);var coeff=c.Snapshot();
  for(int sample=0;sample<=10;++sample){double u=sample/10.0,v=1-u;double[] sums=new double[coeff.Columns];for(int i=0;i<2;++i)for(int j=0;j<4;++j)for(int k=0;k<sums.Length;++k)sums[k]+=coeff[i*4+j+1,k+1]*Math.Pow(u,i)*Math.Pow(v,j);double weight=rational?sums[3]:1;var actual=b.EvalD0(u,v);Near(actual.X,sums[0]/weight);Near(actual.Y,sums[1]/weight);Near(actual.Z,sums[2]/weight);}
 }
 [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]delegate int RawValues(nint token,int operation,double u,double v,int order,out nint result);
 [Fact]public void WrongAndStaleRawTokensNeverPublishResults(){
  using var b=GeomBezierCurveCollections.Create(new([new Coordinates3d(0,0,0),new(1,1,0)]));var s=SplineWorkspace.Coefficients(b);
  var field=typeof(SplineWorkspace).GetField("handle",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;nint token=((System.Runtime.InteropServices.SafeHandle)field.GetValue(s)!).DangerousGetHandle();
  nint lib=System.Runtime.InteropServices.NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));try{var call=System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<RawValues>(System.Runtime.InteropServices.NativeLibrary.GetExport(lib,"occtsharp_spline_workspace_values"));Assert.NotEqual(0,call(token,1,2,0,0,out var p));Assert.Equal(0,p);Assert.NotEqual(0,call(token,7,0,0,0,out p));Assert.Equal(0,p);s.Dispose();Assert.NotEqual(0,call(token,0,0,0,0,out p));Assert.Equal(0,p);Assert.NotEqual(0,call(0,0,0,0,0,out p));Assert.Equal(0,p);}finally{s.Dispose();System.Runtime.InteropServices.NativeLibrary.Free(lib);}
 }
 static void Near(double e,double a)=>Assert.InRange(Math.Abs(e-a),0,2e-8);
 static void Point(Point3d p,CopiedGrid<double> m,int row=0){Near(p.X,m[row,1]);Near(p.Y,m[row,2]);Near(p.Z,m[row,3]);}
 static Coordinates3d[] Poles(int nu,int nv)=>Enumerable.Range(0,nu).SelectMany(i=>Enumerable.Range(0,nv).Select(j=>new Coordinates3d(i,j,Math.Sin(i+j*.7)))).ToArray();
 [Theory][InlineData(false)][InlineData(true)]
 public void CurveCoefficientsAndCacheAgreeWithOwningDerivativesAfterSourceDisposal(bool rational){
  var poles=new CopiedArray<Coordinates3d>([new(0,1,0),new(1,2,1),new(3,-1,2),new(4,1,-1)]);var weights=new CopiedArray<double>([1,2,1,3]);
  using var b=rational?GeomBezierCurveCollections.Create(poles,weights):GeomBezierCurveCollections.Create(poles);
  using var c=rational?GeomBSplineCurveCollections.Create(poles,weights,new([2.0,5]),new([4,4]),3,false,true):GeomBSplineCurveCollections.Create(poles,new([2.0,5]),new([4,4]),3,false);
  using var coeff=SplineWorkspace.Coefficients(b);using var cache=SplineWorkspace.Cache(c,3.1);var copy=coeff.Snapshot();Assert.Equal(4,copy.Rows);Assert.Equal(rational?4:3,copy.Columns);
  for(int i=0;i<=12;++i){double t=i/12.0;for(int order=0;order<=3;++order){var x=coeff.EvaluateCurve(t,order);var y=cache.EvaluateCurve(2+t*3,order);Point(b.EvalD0(t),x);Point(c.EvalD0(2+t*3),y);for(int d=1;d<=order;++d){var a=b.EvalDN(t,d);var z=c.EvalDN(2+t*3,d);Near(a.X,x[d,1]);Near(a.Y,x[d,2]);Near(a.Z,x[d,3]);Near(z.X,y[d,1]);Near(z.Y,y[d,2]);Near(z.Z,y[d,3]);}}}
  var before=cache.EvaluateCurve(3,3).ToArray();b.Dispose();c.Dispose();Assert.Equal(before,cache.EvaluateCurve(3,3).ToArray());coeff.Dispose();Assert.Equal(4,copy.Rows);Assert.Throws<ObjectDisposedException>(()=>coeff.Snapshot());
 }
 [Theory][InlineData(false)][InlineData(true)]
 public void TwoDimensionalCoefficientAndCachePaths(bool rational){
  var p=new CopiedArray<Point2d>([new(1,0),new(1,1),new(0,1)]);var w=new CopiedArray<double>([1,Math.Sqrt(.5),1]);
  using var b=rational?Geom2dBezierCurveCollections.Create(p,w):Geom2dBezierCurveCollections.Create(p);
  using var c=rational?Geom2dBSplineCurveCollections.Create(p,w,new([0.0,1]),new([3,3]),2,false):Geom2dBSplineCurveCollections.Create(p,new([0.0,1]),new([3,3]),2,false);
  using var x=SplineWorkspace.Coefficients(b);using var y=SplineWorkspace.Cache(c,.5);
  for(int i=0;i<=10;++i)for(int order=0;order<=3;++order){double t=i/10.0;var a=x.EvaluateCurve(t,order);var z=y.EvaluateCurve(t,order);Near(b.EvalD0(t).X,a[0,1]);Near(b.EvalD0(t).Y,a[0,2]);for(int d=1;d<=order;++d){Near(b.EvalDN(t,d).X,a[d,1]);Near(b.EvalDN(t,d).Y,a[d,2]);}for(int j=0;j<a.Rows;++j)for(int k=1;k<=2;++k)Near(a[j,k],z[j,k]);}
 }
 [Theory][InlineData(2,3,false)][InlineData(2,3,true)][InlineData(3,2,false)][InlineData(3,2,true)][InlineData(3,3,true)]
 public void SurfaceUnequalDegreeLayoutsAndAllDerivatives(int nu,int nv,bool rational){
  var p=new CopiedGrid<Coordinates3d>(Poles(nu,nv),nu,nv);var w=new CopiedGrid<double>(Enumerable.Range(0,nu*nv).Select(i=>1.0+i%3).ToArray(),nu,nv);
  using var b=rational?GeomBezierSurfaceCollections.Create(p,w):GeomBezierSurfaceCollections.Create(p);
  using var s=rational?GeomBSplineSurfaceCollections.Create(p,w,new([2.0,5]),new([3.0,7]),new([nu,nu]),new([nv,nv]),nu-1,nv-1,false,false):GeomBSplineSurfaceCollections.Create(p,new([2.0,5]),new([3.0,7]),new([nu,nu]),new([nv,nv]),nu-1,nv-1,false,false);
  using var coeff=SplineWorkspace.Coefficients(b);using var cache=SplineWorkspace.Cache(s,3,4);Assert.Equal(nu*nv,coeff.Snapshot().Rows);
  for(int i=0;i<=8;++i){double u=i/8.0,v=1-u;for(int order=0;order<=2;++order){var x=coeff.EvaluateSurface(u,v,order);var y=cache.EvaluateSurface(2+3*u,3+4*v,order);Point(b.EvalD0(u,v),x);Point(s.EvalD0(2+3*u,3+4*v),y);
   (int a,int z)[] ds=order==0?[]:order==1?[(1,0),(0,1)]:[(1,0),(0,1),(2,0),(1,1),(0,2)];for(int k=0;k<ds.Length;++k){var d=ds[k];var bv=b.EvalDN(u,v,d.a,d.z);var sv=s.EvalDN(2+3*u,3+4*v,d.a,d.z);Near(bv.X,x[k+1,1]);Near(bv.Y,x[k+1,2]);Near(bv.Z,x[k+1,3]);Near(sv.X,y[k+1,1]);Near(sv.Y,y[k+1,2]);Near(sv.Z,y[k+1,3]);}}}
  var expected=cache.EvaluateSurface(3,4,2).ToArray();b.Dispose();s.Dispose();Assert.Equal(expected,cache.EvaluateSurface(3,4,2).ToArray());
 }
 [Theory][InlineData(false)][InlineData(true)]
 public void PeriodicCurveAndSurfaceNormalizeButRejectOtherSpans(bool rational){
  Coordinates3d[] ring=[new(1,0,0),new(0,1,0),new(-1,0,0),new(0,-1,0)];var weights=new CopiedArray<double>([1,2,1,2]);
  using var c=rational?GeomBSplineCurveCollections.Create(new(ring),weights,new([0.0,1,2,3,4]),new([1,1,1,1,1]),2,true,true):GeomBSplineCurveCollections.Create(new(ring),new([0.0,1,2,3,4]),new([1,1,1,1,1]),2,true);
  using var cache=SplineWorkspace.Cache(c,8.25);for(int i=0;i<=8;++i){double t=i/8.0;Point(c.EvalD0(t),cache.EvaluateCurve(t+8,2));}Assert.Throws<ArgumentException>(()=>cache.EvaluateCurve(1.5));
  var points=ring.SelectMany(p=>Enumerable.Range(0,3).Select(j=>new Coordinates3d(p.X,p.Y,j))).ToArray();var grid=new CopiedGrid<Coordinates3d>(points,4,3);var wg=new CopiedGrid<double>(Enumerable.Range(0,12).Select(i=>1.0+(i/3)%2).ToArray(),4,3);
  using var s=rational?GeomBSplineSurfaceCollections.Create(grid,wg,new([0.0,1,2,3,4]),new([0.0,1]),new([1,1,1,1,1]),new([3,3]),2,2,true,false):GeomBSplineSurfaceCollections.Create(grid,new([0.0,1,2,3,4]),new([0.0,1]),new([1,1,1,1,1]),new([3,3]),2,2,true,false);
  using var sc=SplineWorkspace.Cache(s,-3.75,.4);for(int i=0;i<=8;++i){double t=i/8.0;var x=sc.EvaluateSurface(t+8,.4,2);Point(s.EvalD0(t,.4),x);var d=s.EvalDN(t,.4,1,1);Near(d.Z,x[4,3]);}Assert.Throws<ArgumentException>(()=>sc.EvaluateSurface(2,.4));
 }
 [Theory][InlineData(2,false)][InlineData(3,false)][InlineData(2,true)][InlineData(3,true)]
 public void ContactInterpolationReturnsIndependentPolesAndWeights(int dimension,bool weighted){
  double[] p=[0,.3,.7,1];var data=p.SelectMany(t=>dimension==2?new[]{t,t*t}:new[]{t,t*t,t*t*t}).ToArray();var grid=new CopiedGrid<double>(data,4,dimension);var weights=weighted?new double[]{1,1,1,1}:null;
  var result=SplineWorkspaceInterpolation.Curve([0,0,0,0,1,1,1,1],3,p,[0,0,0,0],grid,weights);Assert.Equal(data,grid.ToArray());if(weights!=null)Assert.Equal(new double[]{1,1,1,1},weights);
  for(int i=1;i<=4;++i){Near((i-1)/3.0,result[i,1]);if(weighted)Near(1,result[i,dimension+1]);}
  var hermite=new CopiedGrid<double>([0,0,1,0,1,2,1,1],4,2);var h=SplineWorkspaceInterpolation.Curve([0,0,0,0,1,1,1,1],3,[0,0,1,1],[0,1,1,0],hermite);Near(1.0/3,h[2,1]);Near(0,h[2,2]);Near(2.0/3,h[3,1]);Near(1.0/3,h[3,2]);
 }
 [Fact]public void InvalidOptionsInversionAndWorkspaceRequestsPublishNothing(){
  var points=new CopiedGrid<double>([0,0,1,1],2,2);Assert.Throws<ArgumentException>(()=>SplineWorkspaceInterpolation.Curve([0,0,1,1],1,[0,1],[0,0],points,[]));Assert.Throws<ArgumentException>(()=>SplineWorkspaceInterpolation.Curve([0,0,1,1],1,[0,1],[0,0],points,[1,0]));
  Assert.ThrowsAny<Exception>(()=>SplineWorkspaceInterpolation.Curve([0,0,1,1],1,[0,0],[0,0],points));Assert.Throws<ArgumentException>(()=>SplineWorkspaceInterpolation.Curve([0,0,1,1],1,[1,0],[0,0],points));
  using var b=GeomBezierCurveCollections.Create(new([new Coordinates3d(0,0,0),new(1,1,0)]));using var s=SplineWorkspace.Coefficients(b);Assert.Throws<ArgumentException>(()=>s.EvaluateCurve(double.NaN));Assert.Throws<ArgumentException>(()=>s.EvaluateCurve(1.1));Assert.Throws<ArgumentException>(()=>s.EvaluateCurve(.5,4));Assert.Throws<InvalidOperationException>(()=>s.EvaluateSurface(.5,.5));
 }
 [Fact]public async Task WorkspaceDisposalRacesRetainActiveCalls(){
  for(int repeat=0;repeat<30;++repeat){using var b=GeomBezierCurveCollections.Create(new([new Coordinates3d(0,0,0),new(1,1,0)]));var s=SplineWorkspace.Coefficients(b);var task=Task.Run(()=>{for(int i=0;i<50;++i){try{Near(.5,s.EvaluateCurve(.5)[0,1]);}catch(ObjectDisposedException){return;}}});s.Dispose();await task;}
 }
 [Fact]public void CompleteGeometryAndStepWorkflow()=>OcctSharp.Tests.Shared.BatchAMSplineWorkflow.Run();
}
