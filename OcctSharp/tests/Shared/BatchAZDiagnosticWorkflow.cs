using OcctSharp.CopiedCollections;
using OcctSharp.FoundationValues;
namespace OcctSharp.Tests.Shared;
internal static class BatchAZDiagnosticWorkflow {
 internal static void Run(){
  var grid=new CopiedGrid<double>(new double[]{4,0,0,9},2,2,-3,7);
  var root=SdkDiagnostics.MatrixSquareRoot(grid);
  if(!root.IsDone||root.Matrix is null||Math.Abs(root.Matrix.ToArray()[0]-2)>1e-12||Math.Abs(root.Matrix.ToArray()[3]-3)>1e-12)throw new InvalidOperationException("Copied SDK square root failed.");
  var polynomial=SdkDiagnostics.EvaluatePolynomial(new double[]{-4,0,1},root.Matrix.ToArray()[0]);
  var quadrature=SdkDiagnostics.GaussQuadrature(5);
  if(Math.Abs(polynomial.Value)>1e-12||!quadrature.IsDone||Math.Abs(quadrature.Weights.Sum()-2)>1e-12)throw new InvalidOperationException("Foundation diagnostics failed.");
  var message=SdkDiagnostics.Message("诊断 %s: %d / %.3f",["已复制"],[5],[quadrature.Weights.Sum()]);
  if(message.Complete!="诊断 已复制: 5 / 2.000"||message.UnresolvedArguments!=0)throw new InvalidOperationException("Private Unicode message failed.");
  if(SdkDiagnostics.FirstLocationInSet(message.Complete,"已",1,message.Complete.Length)!=4)throw new InvalidOperationException("Copied Unicode set diagnostic failed.");
  BatchAVFoundationWorkflow.Run();
 }
}
