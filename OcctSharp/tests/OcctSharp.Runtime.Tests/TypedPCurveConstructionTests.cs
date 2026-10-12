using OcctSharp.ConstructionWorkspaces;
using OcctSharp.Tests.Shared;
using OcctSharp.TopologyAssembly;
using OcctSharp.Values;
namespace OcctSharp.Runtime.Tests;
public sealed class TypedPCurveConstructionTests {
 [Theory][InlineData(0,0)][InlineData(0,1)][InlineData(0,2)][InlineData(1,0)][InlineData(1,1)][InlineData(1,2)][InlineData(2,0)][InlineData(2,1)][InlineData(2,2)]
 public void AllTypedAndMixedProfilesKeepCoupledParametersAndNativeOrientation(int inputs,int profile){
  using var curve=BatchBXTypedTopologyWorkflow.Segment();using var surface=BatchBXTypedTopologyWorkflow.Plane();using var baseCurve=ParameterizedConstruction.CopyPCurve(curve);using var baseSurface=ParameterizedConstruction.CopySurface(surface);
  var first=new GpPoint(.8,1.2,0);var last=new GpPoint(3.2,1.2,0);using var a=TopologyAssembler.Vertex(first.X,first.Y,first.Z);using var b=TopologyAssembler.Vertex(last.X,last.Y,last.Z);
  using var result=(inputs,profile) switch {
   (0,0)=>ParameterizedConstruction.PCurveEdge(curve,baseSurface,0,1),(0,1)=>ParameterizedConstruction.PCurveEdge(curve,baseSurface,first,last,0,1),(0,2)=>ParameterizedConstruction.PCurveEdgeWithVertices(curve,baseSurface,a,b,0,1),
   (1,0)=>ParameterizedConstruction.PCurveEdge(baseCurve,surface,0,1),(1,1)=>ParameterizedConstruction.PCurveEdge(baseCurve,surface,first,last,0,1),(1,2)=>ParameterizedConstruction.PCurveEdgeWithVertices(baseCurve,surface,a,b,0,1),
   (2,0)=>ParameterizedConstruction.PCurveEdge(curve,surface,0,1),(2,1)=>ParameterizedConstruction.PCurveEdge(curve,surface,first,last,0,1),_=>ParameterizedConstruction.PCurveEdgeWithVertices(curve,surface,a,b,0,1)};
  Assert.True(result.IsDone);Assert.True(result.HasStoredPCurve);Assert.Equal(0,result.NativeOrientation);Assert.Equal(0,result.NativeBounds!.First);Assert.Equal(1,result.NativeBounds.Last);
  var p=result.EvaluatePCurve(.4);Assert.Equal(.44,p.UV.X,7);Assert.Equal(.3,p.UV.Y,7);Assert.Equal(1.76,p.Point.X,6);Assert.Equal(1.2,p.Point.Y,6);
  curve.SetPole(1,new Point2d(8,8));surface.SetPole(1,1,new Point3d(80,80,80));curve.Dispose();surface.Dispose();baseCurve.Dispose();baseSurface.Dispose();Assert.Equal(1.76,result.EvaluatePCurve(.4).Point.X,6);
 }
 [Fact] public void TypedReversedIntervalAndIndependentBaseCopiesRetainTheirActualDomains(){
  using var curve=BatchBXTypedTopologyWorkflow.Segment();using var surface=BatchBXTypedTopologyWorkflow.Plane();using var copiedCurve=ParameterizedConstruction.CopyPCurve(curve);using var copiedSurface=ParameterizedConstruction.CopySurface(surface);using var result=ParameterizedConstruction.PCurveEdge(curve,surface,1,0);
  Assert.True(result.IsDone);Assert.Equal(1,result.NativeOrientation);Assert.Equal(0,result.NativeBounds!.First);Assert.Equal(1,result.NativeBounds.Last);
  curve.SetPole(1,new Point2d(8,8));surface.SetPole(1,1,new Point3d(80,80,80));curve.Dispose();surface.Dispose();using var independent=ParameterizedConstruction.PCurveEdge(copiedCurve,copiedSurface,0,1);Assert.Equal(2,independent.EvaluatePCurve(.5).Point.X,6);
 }
 [Fact] public void TypedNullDisposedAndInvalidIntervalsRejectWithoutPublishingTopology(){
  using var curve=BatchBXTypedTopologyWorkflow.Segment();using var surface=BatchBXTypedTopologyWorkflow.Plane();Assert.Throws<ArgumentNullException>(()=>ParameterizedConstruction.CopyPCurve(null!));Assert.Throws<ArgumentNullException>(()=>ParameterizedConstruction.CopySurface(null!));Assert.Throws<ArgumentException>(()=>ParameterizedConstruction.PCurveEdge(curve,surface,0,double.NaN));Assert.Throws<ArgumentException>(()=>ParameterizedConstruction.PCurveEdge(curve,surface,1,1));curve.Dispose();Assert.Throws<ObjectDisposedException>(()=>ParameterizedConstruction.CopyPCurve(curve));surface.Dispose();Assert.Throws<ObjectDisposedException>(()=>ParameterizedConstruction.CopySurface(surface));
 }
 [Fact] public void TypedAssemblyHolesAndActualStepReadback()=>BatchBXTypedTopologyWorkflow.Run();
}
