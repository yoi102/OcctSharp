using OcctSharp.ConstructionWorkspaces;
using OcctSharp.IntersectionResults;
using OcctSharp.TopologyAssembly;
namespace OcctSharp.Tests.Shared;
internal static class BatchBLConstructionWorkflow {
 internal sealed class PCurveFixture:IDisposable {
  readonly CoupledIntersectionLine line;
  readonly ConstructedPCurve? explicitlyConstructed;
  internal GeomCurve Curve=>line.Curve;
  internal Geom2dCurve PCurve=>line.SecondPCurve??explicitlyConstructed!.Curve;
  internal GeomSurface Surface{get;}
  internal PCurveFixture(bool periodic=true){using var face=BatchBHIntersectionWorkflow.Horizontal(.5);using var other=periodic?null:BatchBHIntersectionWorkflow.Vertical();using var first=periodic?BatchBHIntersectionWorkflow.Spherical():RayIntersections.CopySurface(other!);using var plane=RayIntersections.CopySurface(face);using var result=SurfaceIntersections.Compute(first,plane);if(!result.IsDone||result.Lines.Count!=1)throw new InvalidOperationException("BL pcurve fixture failed.");line=result.CopyLine(1);Surface=result.CopySourceSurface(1);if(line.SecondPCurve is null){if(periodic)throw new InvalidOperationException("BL fixture pcurve missing.");explicitlyConstructed=SurfaceIntersections.BuildPCurve(Surface,line.Curve,0,2);}}
  public void Dispose(){line.Dispose();explicitlyConstructed?.Dispose();Surface.Dispose();}
 }
 internal static GpPoint Point(GeomCurve curve,double t){var p=curve.Value(t);return new(p.X,p.Y,p.Z);}
 internal static void Run(){
  var frame=ConstructionFrame.World;var edges=new List<Shape>();
  try{
   foreach(var f in new[]{frame,new ConstructionFrame(new(4,0,0),new(0,0,1),new(0,1,0)),new ConstructionFrame(new(4,4,0),new(0,0,1),new(-1,0,0)),new ConstructionFrame(new(0,4,0),new(0,0,1),new(0,-1,0))}){using var edge=ParameterizedConstruction.AnalyticEdge(AnalyticConstructionCurve.Line,f,0,4);if(!edge.IsDone)throw new InvalidOperationException("BL analytic segment failed.");edges.Add(edge.CopyShape());}
   using var wire=ShapeFactory.CreateWire(edges);using var assembled=ShapeFactory.CreatePlanarFace(wire);
   using var built=ParameterizedConstruction.AnalyticFace(AnalyticConstructionSurface.Plane,frame,0,4,0,4);using var face=built.CopyShape();
   if(!face.IsValid||!assembled.IsValid||Math.Abs(face.InspectProperties(InspectionPropertyKind.Area).Mass-16)>1e-6)throw new InvalidOperationException("BL finite native face/wire assembly failed.");
   using var fixture=new PCurveFixture();using var onSurface=ParameterizedConstruction.PCurveEdge(fixture.PCurve,fixture.Surface,.5,1.5);using var pshape=onSurface.CopyShape();
   var p=onSurface.Evaluate(.9);var paired=onSurface.EvaluatePCurve(.9);if(!pshape.IsValid||BatchBHIntersectionWorkflow.Distance(p.X,p.Y,p.Z,paired.Point.X,paired.Point.Y,paired.Point.Z)>1e-6)throw new InvalidOperationException("BL native constructed pcurve residual failed.");
   fixture.Dispose();onSurface.Dispose();if(!pshape.IsValid)throw new InvalidOperationException("BL pcurve topology retained source lifetime.");
   foreach(bool step in new[]{true,false}){string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BL."+Guid.NewGuid()+(step?".step":".iges"));try{if(step)ShapeExchange.WriteStep(face,path);else ShapeExchange.WriteIges(face,path);using var read=step?ShapeExchange.ReadStep(path):ShapeExchange.ReadIges(path);if(!read.IsValid||read.FaceCount!=1||Math.Abs(read.InspectProperties(InspectionPropertyKind.Area).Mass-16)>1e-4)throw new InvalidOperationException("BL actual STEP/IGES finite face readback failed.");}finally{File.Delete(path);}}
  }finally{foreach(var edge in edges)edge.Dispose();}
 }
}
