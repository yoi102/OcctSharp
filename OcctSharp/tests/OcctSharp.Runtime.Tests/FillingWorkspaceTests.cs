using OcctSharp.FillingWorkspaces;
using OcctSharp.TopologyAssembly;
using OcctSharp.CopiedCollections;
using OcctSharp.Values;
namespace OcctSharp.Runtime.Tests;
public sealed class FillingWorkspaceTests {
 static readonly double[] SampleParameters=[0,.25,.5,.75,1],UnitKnots=[0,1];
 static readonly int[] CubicMultiplicities=[4,4];
 [Fact]public void LoftRepairAndRealExchangeComposeAcrossModules()=>OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Run();
 [Theory][InlineData(1)][InlineData(2)][InlineData(3)]public void KernelDerivativesMatchAnalyticalPolynomialInAllDimensions(int dimension){
  double[][] p={new[]{0.0,1.0/3,2.0/3,1},new[]{0.0,0,1.0/3,0,2.0/3,1.0/3,1,1},new[]{0.0,0,0,1.0/3,0,0,2.0/3,1.0/3,0,1,1,1}};
  foreach(int order in Enumerable.Range(0,4))foreach(bool bezier in new[]{false,true}){
   if(dimension==1&&bezier)continue;
   var result=SplineKernel.Evaluate(p[dimension-1],dimension,3,SampleParameters,order,bezier?null:UnitKnots,bezier?null:CubicMultiplicities);
   for(int s=0;s<5;++s){double u=s*.25;int offset=s*dimension*(order+1);Assert.Equal(u,result.Values[offset],8);if(dimension>=2)Assert.Equal(u*u,result.Values[offset+1],8);if(dimension==3)Assert.Equal(u*u*u,result.Values[offset+2],8);if(order>=1){Assert.Equal(1,result.Values[offset+dimension],8);if(dimension>=2)Assert.Equal(2*u,result.Values[offset+dimension+1],8);if(dimension==3)Assert.Equal(3*u*u,result.Values[offset+dimension+2],8);}if(order>=2){Assert.Equal(0,result.Values[offset+2*dimension],8);if(dimension>=2)Assert.Equal(2,result.Values[offset+2*dimension+1],8);if(dimension==3)Assert.Equal(6*u,result.Values[offset+2*dimension+2],8);}if(order>=3&&dimension==3)Assert.Equal(6,result.Values[offset+3*dimension+2],8);}
  }
 }
 [Theory][InlineData(2)][InlineData(3)]public void RationalKernelCopiesWeightsAndCircleSamples(int dimension){double[] p=dimension==2?[1,0,1,1,0,1]:[1,0,0,1,1,0,0,1,0];double[] weights=[1,Math.Sqrt(.5),1];var copied=(double[])p.Clone();foreach(bool bezier in new[]{false,true}){var r=SplineKernel.Evaluate(p,dimension,2,[.5],3,bezier?null:[0,1],bezier?null:[3,3],weights);Assert.Equal(Math.Sqrt(.5),r.Values[0],8);Assert.Equal(Math.Sqrt(.5),r.Values[1],8);Assert.All(r.Values,x=>Assert.True(double.IsFinite(x)));}Assert.Equal(copied,p);}
 [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]public void EachLoftStrategyCompletesAndSurfacesAreIndependent(int strategy){using var a=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(0);using var b=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(1);using var c=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(2);using var d=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(3);using var r=SurfaceFilling.Loft(new[]{a,b,c,d},new(Strategy:strategy));Assert.True(r.IsDone);using var surface=r.CreateSurface();var p=surface.EvalD0(.5,.5);r.Dispose();Assert.Equal(p,surface.EvalD0(.5,.5));Assert.Throws<ObjectDisposedException>(()=>r.CreateSurface());Assert.Equal(3,r.CriterionWeights.Count);}
 [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]public void RadiusSweepUsesInitializedPrivateSections(int strategy){using var path=GeomBSplineCurveCollections.Create(new(new[]{new Coordinates3d(0,0,0),new Coordinates3d(0,0,3)}),new([0.0,1]),new([2,2]),1,false);using var r=SurfaceFilling.Sweep(path,.25,new(Strategy:strategy));Assert.True(r.IsDone);using var surface=r.CreateSurface();Assert.True(double.IsFinite(surface.EvalD0(.5,.5).X));Assert.Equal(0,surface.EvalD0(.5,0).Z,5);Assert.Equal(3,surface.EvalD0(.5,1).Z,5);}
 [Fact]public void InvalidBuffersKnotsOptionsAndDisposedInputsAreRejected(){
  Assert.ThrowsAny<Exception>(()=>SplineKernel.Evaluate([0,1],1,1,[2],knots:[0,1],multiplicities:[2,2]));
  Assert.ThrowsAny<Exception>(()=>SplineKernel.Evaluate([0,1],1,1,[.5],knots:[1,0],multiplicities:[2,2]));
  Assert.ThrowsAny<Exception>(()=>SplineKernel.Evaluate([0,1],1,1,[.5],knots:[0,1],multiplicities:[2,2],weights:[0,1]));
  using var path=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(0);Assert.ThrowsAny<Exception>(()=>SurfaceFilling.Sweep(path,-1));Assert.ThrowsAny<Exception>(()=>SurfaceFilling.Loft(new[]{path,path},new(MaximumDegree:100)));
  Assert.ThrowsAny<Exception>(()=>TopologyAssembler.Vertex(double.NaN,0,0));Assert.ThrowsAny<Exception>(()=>TopologyAssembler.Edge(path,1,0));path.Dispose();Assert.Throws<ObjectDisposedException>(()=>SurfaceFilling.Sweep(path,1));
 }
 [Fact]public void NewEdgeAndVertexAreIndependentAndValidated(){using var curve=OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Section(0);var before=curve.EvalD0(.5);using var edge=TopologyAssembler.Edge(curve,0,1);Assert.Equal(before,curve.EvalD0(.5));curve.Dispose();Assert.True(edge.IsValid);Assert.Equal(2,edge.CountSubShapes(ShapeKind.Vertex));using var vertex=TopologyAssembler.Vertex(3,4,5);Assert.True(vertex.IsValid);}
}
