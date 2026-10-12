using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class FittingAdapterContractTests {
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.FittingAdapters;
 static BindingDeclaration Row(CopiedCollectionOperationContract.Operation op){string owner=op.NativeName.Split("::")[0];return new(op.StableId,op.NativeName,op.NativeName.EndsWith("::"+owner,StringComparison.Ordinal)?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,owner+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public};}
 [Fact] public void ExactInitializedFittingClosureRejectsDriftUnsafeAndManual() {
  foreach(var op in Contract.Operations.Values){var d=Row(op);Assert.True(Contract.Contains(d));Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(Contract.Contains(d with{IsTemplated=true}));Assert.False(Contract.Contains(d with{Access=BindingAccess.Private}));Assert.False(Contract.Contains(d with{SupportState=BindingSupportState.Manual}));}
  Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel([Row(Contract.Operations.Values.First())]),Contract));
 }
 [Fact] public void CompleteFittingClosureEmitsNativePrivateOwningHandleCopiesDeterministically() {
  var rows=Contract.Operations.Values.Select(Row).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()),Contract);
  Assert.Equal(18,a.SourceStableIds.Count);Assert.Single(a.Files);Assert.Equal(a.Files[0].Content,b.Files[0].Content);Assert.DoesNotContain("OCCTSHARP_CALL",a.Files[0].Content);Assert.Contains("occ::handle<Geom_BSplineSurface> Surface() const",a.Files[0].Content);
 }
}
