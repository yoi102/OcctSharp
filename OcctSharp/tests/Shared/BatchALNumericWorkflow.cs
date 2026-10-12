using OcctSharp.CopiedCollections;
using OcctSharp.Numeric;
using OcctSharp.OwnedValues;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchALNumericWorkflow {
 internal static void Run(){
  double[] knots=[0,0,0,0,1,1,1,1],parameters=[0,1.0/3,2.0/3,1];
  var samples=parameters.SelectMany(u=>new[]{u,u*u,u*u*u}).ToArray();
  using var band=SplineMatrixSystem.Create(knots,3,parameters,[0,0,0,0]);
  if(band.Status!=0)throw new InvalidOperationException("AL band factorization failed.");
  var solved=band.Solve(new(samples,4,3));band.Dispose();
  var values=solved.Values??throw new InvalidOperationException("AL band solve failed.");
  Coordinates3d[] poles=Enumerable.Range(1,4).Select(i=>new Coordinates3d(values[i,1],values[i,2],values[i,3])).ToArray();
  using var curve=GeomBSplineCurveCollections.Create(new(poles),new([0.0,1]),new([4,4]),3,false);
  // An overdetermined independently assembled basis and three numerical solvers
  // check the same geometry against analytical polynomial values.
  foreach(var solver in Enum.GetValues<NumericSolver>()){
   int n=solver==NumericSolver.Gauss?4:9;var a=new double[n*4];var b=new double[n*3];
   for(int i=0;i<n;++i){double u=(double)i/(n-1);var basis=SplineMatrixSystem.EvaluateBasis(knots,3,u);for(int j=1;j<=4;++j)a[i*4+j-1]=basis[0,j];b[i*3]=u;b[i*3+1]=u*u;b[i*3+2]=u*u*u;}
   using var matrix=MathMatrix.FromValues(new(a,n,4,-3,7));using var rhs=MathMatrix.FromValues(new(b,n,3,20,-2));
   var result=matrix.Solve(rhs,solver);matrix.Dispose();rhs.Dispose();
   if(!result.IsDone||result.ResidualNorm>1e-10||result.Values is null)throw new InvalidOperationException("AL dense solve failed.");
   for(int i=0;i<4;++i)for(int j=0;j<3;++j)if(Math.Abs(result.Values[7+i,-2+j]-values[i+1,j+1])>1e-9)throw new InvalidOperationException("AL solvers disagree.");
  }
  for(int i=0;i<=40;++i){double u=i/40.0;var p=curve.EvalD0(u);var d=curve.EvalDN(u,1);
   if(Math.Abs(p.X-u)+Math.Abs(p.Y-u*u)+Math.Abs(p.Z-u*u*u)>1e-10||Math.Abs(d.X-1)+Math.Abs(d.Y-2*u)+Math.Abs(d.Z-3*u*u)>1e-9)throw new InvalidOperationException("AL sampled spline error.");}
  using var edge=BatchAGSurfaceInterpolationWorkflow.Edge(curve);string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AL."+Guid.NewGuid()+".step");
  try{ShapeExchange.WriteStep(edge,path);using var restored=ShapeExchange.ReadStep(path);if(!restored.IsValid)throw new InvalidOperationException("AL STEP readback failed.");}finally{File.Delete(path);}
 }
}
