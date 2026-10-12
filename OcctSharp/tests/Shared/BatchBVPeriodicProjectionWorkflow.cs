using OcctSharp.ConstructionWorkspaces;
using OcctSharp.CurveSampling;
using OcctSharp.IntersectionResults;
namespace OcctSharp.Tests.Shared;
internal static class BatchBVPeriodicProjectionWorkflow {
 internal static void Run() {
  using var built=ParameterizedConstruction.AnalyticFace(AnalyticConstructionSurface.Cylinder,ConstructionFrame.World,6,6.8,0,1,3);
  using var face=built.CopyShape();using var surface=RayIntersections.CopySurface(face);
  var rows=new List<ProjectionCandidate>();
  foreach(double u in new[]{6.1,6.7}) {
   var result=CurveComputations.Project(surface,new(4*Math.Cos(u),4*Math.Sin(u),.3),6,6.8,0,1);
   if(!result.IsDone||result.UniqueNearestIndex() is not int i)throw new InvalidOperationException("BV unique finite projection missing");
   var row=result.Candidates[i];if(Math.Abs(row.U-u)>1e-7||Math.Abs(row.Distance-1)>1e-7)throw new InvalidOperationException("BV seam branch/distance incorrect");rows.Add(row);
  }
  using var edgeResult=ParameterizedConstruction.AnalyticEdge(AnalyticConstructionCurve.Circle,new(new(0,0,rows[0].V),new(0,0,1),new(1,0,0)),rows[0].U,rows[1].U,3);using var edge=edgeResult.CopyShape();
  if(!edge.IsValid||Math.Abs(edge.InspectProperties(InspectionPropertyKind.Length).Mass-1.8)>1e-6)throw new InvalidOperationException("BV projected seam UV did not build the expected independent arc");
  surface.Dispose();face.Dispose();built.Dispose();edgeResult.Dispose();
  if(!edge.IsValid||Math.Abs(rows[0].Distance-1)>1e-7)throw new InvalidOperationException("BV retained source ownership");
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BV."+Guid.NewGuid()+".step");
  try {ShapeExchange.WriteStep(edge,path);using var read=ShapeExchange.ReadStep(path);var edges=read.GetSubShapes(ShapeKind.Edge);
   try {if(!read.IsValid||edges.Length!=1||Math.Abs(edges[0].InspectProperties(InspectionPropertyKind.Length).Mass-1.8)>1e-5)throw new InvalidOperationException("BV exact seam arc STEP readback failed");}
   finally {foreach(var item in edges)item.Dispose();}
  } finally {File.Delete(path);}
  Console.WriteLine("BV periodic projection workflow passed: copied UV across seam, independent exact circular edge and STEP arc length.");
 }
}
