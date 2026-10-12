using OcctSharp.FillingWorkspaces;
using OcctSharp.Geometry.Workspaces;
using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class NonuniformFillingTests {
 [Fact] public void NonuniformFittingCreatesIndependentFaceAndExactStepReadback()=>BatchBWNonuniformFittingWorkflow.Run();
 [Theory][InlineData(0)][InlineData(1)][InlineData(3)]
 public void NonuniformPhysicalSectionsPreservePositionDerivativesAndIndependentCopies(int strategy) {
  double[] parameters=[2,2.3,3.8,8];var sections=parameters.Select(p=>BatchAWGeometryWorkflow.Section((p-2)*.5)).ToArray();
  try {
   using var result=SurfaceFilling.LoftWithParameters(sections,parameters,new(Strategy:strategy));Assert.True(result.IsDone);using var surface=result.CreateSurface();
   foreach(double v in new[]{2,2.3,3.8,5.5,8}){Assert.Equal((v-2)*.5,surface.EvalD0(.5,v).Z,5);Assert.Equal(.5,surface.EvalDN(.5,v,0,1).Z,5);Assert.Equal(0,surface.EvalDN(.5,v,0,2).Z,5);}
   using var cache=SplineWorkspace.Cache(surface,.5,5.5);var copied=cache.EvaluateSurface(.5,5.5,2).ToArray();parameters[1]=99;
   foreach(var section in sections)section.Dispose();result.Dispose();surface.Dispose();Assert.Equal(copied,cache.EvaluateSurface(.5,5.5,2).ToArray());Assert.Equal(2.3,result.RequestedSectionParameters[1]);Assert.Throws<ArgumentException>(()=>cache.EvaluateSurface(.5,9));
  } finally {foreach(var section in sections)section.Dispose();}
 }
 [Fact] public void InvalidExplicitDomainsAndIgnoredSmoothingAreRejectedBeforePublication() {
  using var a=BatchAWGeometryWorkflow.Section(0);using var b=BatchAWGeometryWorkflow.Section(1);
  foreach(var parameters in new double[][]{[],[0],[0,0],[1,0],[0,double.NaN],[0,double.PositiveInfinity],[0,1e-12]})Assert.Throws<ArgumentException>(()=>SurfaceFilling.LoftWithParameters([a,b],parameters,new(Strategy:1)));
  Assert.Throws<ArgumentException>(()=>SurfaceFilling.LoftWithParameters([a,b],[0,1],new(Strategy:2)));
  Assert.Throws<ArgumentException>(()=>SurfaceFilling.LoftWithParameters([a,null!],[0,1],new(Strategy:1)));
  a.Dispose();Assert.Throws<ObjectDisposedException>(()=>SurfaceFilling.LoftWithParameters([a,b],[0,1],new(Strategy:1)));
 }
 [Fact] public void LegacyUniformSmoothingRetainsThePreviouslySupportedFourSectionProfile() {
  using var a=BatchAWGeometryWorkflow.Section(0);using var b=BatchAWGeometryWorkflow.Section(1);using var c=BatchAWGeometryWorkflow.Section(2);using var d=BatchAWGeometryWorkflow.Section(3);using var uniform=SurfaceFilling.Loft([a,b,c,d],new(Strategy:2));Assert.True(uniform.IsDone);Assert.Empty(uniform.RequestedSectionParameters);
  using var surface=uniform.CreateSurface();Assert.Equal(1.5,surface.EvalD0(.5,.5).Z,5);
 }
 [Fact] public void ParameterRowsAreReadonlyAndNativeOutputDoesNotRetainTheCallerArray() {
  using var a=BatchAWGeometryWorkflow.Section(0);using var b=BatchAWGeometryWorkflow.Section(3);double[] p=[0,3];using var result=SurfaceFilling.LoftWithParameters([a,b],p,new(Strategy:1));p[1]=30;
  Assert.True(result.IsDone);Assert.Equal(3,result.RequestedSectionParameters[1]);Assert.Throws<NotSupportedException>(()=>((IList<double>)result.RequestedSectionParameters)[0]=99);
  foreach(int strategy in new[]{0,3})Assert.Throws<ArgumentException>(()=>SurfaceFilling.LoftWithParameters([a,b],[0,3],new(Strategy:strategy)));
 }
}
