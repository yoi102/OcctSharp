using OcctSharp.CopiedCollections;
using OcctSharp.NumericResults;
using OcctSharp.Numeric;
using OcctSharp.Values;
using OcctSharp.TopologyAssembly;
namespace OcctSharp.Tests.Shared;
internal static class BatchBFNumericWorkflow {
 internal static void Run(){
  double[] parameters=[0,1.0/3,2.0/3,1];double[] knots=[0,0,0,0,1,1,1,1];var basis=new double[16];var samples=new double[12];
  for(int i=0;i<4;i++){double u=parameters[i];var row=SplineMatrixSystem.EvaluateBasis(knots,3,u);for(int j=0;j<4;j++)basis[i*4+j]=row[0,j+1];samples[i*3]=u;samples[i*3+1]=u*u;samples[i*3+2]=u*u*u;}
  var a=new CopiedGrid<double>(basis,4,4,-2,7);var b=new CopiedGrid<double>(samples,4,3,13,-5);var lu=NumericComputations.DecomposeLu(a);var svd=NumericComputations.DecomposeSvd(a);var solved=NumericComputations.SolveMultiple(a,b);
  if(!lu.IsDone||!svd.IsDone||svd.Rank!=4||!solved.IsDone||solved.Matrix is null)throw new InvalidOperationException("BF copied aggregate interpolation failed.");
  var values=solved.Matrix.ToArray();var poles=Enumerable.Range(0,4).Select(i=>new Coordinates3d(values[i*3],values[i*3+1],values[i*3+2])).ToArray();
  using var curve=GeomBSplineCurveCollections.Create(new(poles),new([0.0,1]),new([4,4]),3,false);
  for(int i=0;i<=40;i++){double u=i/40.0;var p=curve.EvalD0(u);double[] residual=[p.X-u,p.Y-u*u,p.Z-u*u*u];var norm=NumericComputations.InspectVectors(residual,residual);if(norm.EuclideanNorm>1e-10)throw new InvalidOperationException("BF independent curve residual failed.");}
  using var edge=TopologyAssembler.Edge(curve,0,1);string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BF."+Guid.NewGuid()+".step");
  try{ShapeExchange.WriteStep(edge,path);using var restored=ShapeExchange.ReadStep(path);if(!restored.IsValid)throw new InvalidOperationException("BF independent STEP readback failed.");}finally{File.Delete(path);}
 }
}
