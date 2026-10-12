using System.Collections;
using System.Globalization;
using OcctSharp.FoundationValues;
using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class UnitDiagnosticWorkflowTests {
 private static readonly UnitConversionInput Length=new(1,"m","mm",UnitDimensionKind.Length);
 [Fact] public void FullUnitNumericConstraintGeometryAndStepConsumer()=>BatchBUUnitWorkflow.Run();
 [Fact] public void UnitsDimensionsAndInvariantDiagnosticsRemainDetached(){
  var prior=CultureInfo.CurrentCulture;CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("fr-FR");
  try{
   var input=new[]{new UnitConversionInput(.0254,"m","mm",UnitDimensionKind.Length),new(180,"deg","rad",UnitDimensionKind.PlaneAngle),new(10,"s","s",UnitDimensionKind.Time)};
   var rows=UnitDiagnosticWorkflow.Convert(input);Array.Fill(input,Length);
   Assert.Equal(25.4,rows[0].Converted.Value,10);Assert.Equal(Math.PI,rows[1].Converted.Value,10);
   Assert.Equal(1,rows[0].Converted.Exponents[1]);Assert.Equal(1,rows[1].Converted.Exponents[7]);
   Assert.Contains("25.4",rows[0].Diagnostic.Complete,StringComparison.Ordinal);Assert.DoesNotContain("25,4",rows[0].Diagnostic.Complete,StringComparison.Ordinal);
   Assert.Equal(0,rows[0].Diagnostic.UnresolvedArguments);Assert.Equal("deg",rows[1].Input.FromUnit);
   Assert.Throws<NotSupportedException>(()=>((IList<UnitConversionSnapshot>)rows)[0]=rows[1]);
  }finally{CultureInfo.CurrentCulture=prior;}
 }
 [Fact] public void DimensionNumericAndSdkFailuresPublishNoBatchAndIdentifyRow(){
  Check(Length with{ExpectedDimension=UnitDimensionKind.Time},UnitWorkflowFailure.DimensionMismatch);
  Check(Length with{Minimum=1001},UnitWorkflowFailure.ValueOutOfRange);
  Check(Length with{Maximum=999},UnitWorkflowFailure.ValueOutOfRange);
  Check(Length with{FromUnit="s"},UnitWorkflowFailure.ConversionFailure);
  Check(Length with{ToUnit="notAUnit"},UnitWorkflowFailure.ConversionFailure);
  void Check(UnitConversionInput bad,UnitWorkflowFailure expected){IReadOnlyList<UnitConversionSnapshot>? output=null;
   var error=Assert.Throws<UnitWorkflowException>(()=>output=UnitDiagnosticWorkflow.Convert([Length,bad]));Assert.Null(output);Assert.Equal(2,error.Ordinal);Assert.Equal(expected,error.Failure);}
 }
 [Fact] public void InvalidFiniteUnitsKindsAndLimitsRejectAtomically(){
  foreach(var bad in new[]{null!,Length with{Value=double.NaN},Length with{Value=double.PositiveInfinity},Length with{FromUnit=""},Length with{ToUnit="米"},Length with{FromUnit="m\0"},Length with{ToUnit=new string('a',257)},Length with{ExpectedDimension=(UnitDimensionKind)99},Length with{Minimum=double.NaN},Length with{Maximum=double.PositiveInfinity},Length with{Minimum=2,Maximum=1}}){
   var error=Assert.Throws<UnitWorkflowException>(()=>UnitDiagnosticWorkflow.Convert([Length,bad]));Assert.Equal(2,error.Ordinal);Assert.Equal(UnitWorkflowFailure.InvalidInput,error.Failure);
  }
  Assert.Throws<ArgumentNullException>(()=>UnitDiagnosticWorkflow.Convert(null!));Assert.Empty(UnitDiagnosticWorkflow.Convert([]));
 }
 private sealed class Tracked(Func<IEnumerable<UnitConversionInput>> values):IEnumerable<UnitConversionInput>{
  internal int ReadCount,DisposeCount;
  public IEnumerator<UnitConversionInput> GetEnumerator(){try{foreach(var row in values()){ReadCount++;yield return row;}}finally{DisposeCount++;}}
  IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
 }
 [Fact] public void InfiniteAndFailedInputsAreBoundedAndDisposeBeforeNativeProcessing(){
  var infinite=new Tracked(Endless);var error=Assert.Throws<UnitWorkflowException>(()=>UnitDiagnosticWorkflow.Convert(infinite));
  Assert.Equal(UnitWorkflowFailure.InvalidInput,error.Failure);Assert.Equal(4097,infinite.ReadCount);Assert.Equal(1,infinite.DisposeCount);
  var failed=new Tracked(Failed);error=Assert.Throws<UnitWorkflowException>(()=>UnitDiagnosticWorkflow.Convert(failed));
  Assert.Equal(UnitWorkflowFailure.EnumerationFailure,error.Failure);Assert.Equal(2,error.Ordinal);Assert.Equal(1,failed.DisposeCount);
  var nested=new Tracked(NestedUnitFailure);error=Assert.Throws<UnitWorkflowException>(()=>UnitDiagnosticWorkflow.Convert(nested));
  Assert.Equal(UnitWorkflowFailure.EnumerationFailure,error.Failure);Assert.Equal(2,error.Ordinal);Assert.IsType<UnitWorkflowException>(error.InnerException);Assert.Equal(1,nested.DisposeCount);
  static IEnumerable<UnitConversionInput> Endless(){while(true)yield return Length;}
  static IEnumerable<UnitConversionInput> Failed(){yield return Length;throw new InvalidOperationException("enumeration failed");}
  static IEnumerable<UnitConversionInput> NestedUnitFailure(){yield return Length;_ = UnitDiagnosticWorkflow.Convert([Length with{Value=double.NaN}]);}
 }
 [Fact] public void ExactRowBudgetAndSuccessEnumeratorCleanupAreAccepted(){
  var input=new Tracked(()=>Enumerable.Repeat(Length,4096));var result=UnitDiagnosticWorkflow.Convert(input);
  Assert.Equal(4096,result.Count);Assert.Equal(4096,result[^1].Ordinal);Assert.Equal(1,input.DisposeCount);Assert.Equal(1000,result[^1].Converted.Value);
 }
 [Fact] public void IndependentConcurrentConversionsPreserveDimensionsAndCopies(){
  Parallel.For(0,40,index=>{var row=UnitDiagnosticWorkflow.Convert([Length with{Value=index+1}]).Single();Assert.Equal((index+1)*1000,row.Converted.Value);Assert.Equal(1,row.Converted.Exponents[1]);Assert.Equal(0,row.Diagnostic.UnresolvedArguments);});
 }
}
