using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class ModelingResultContractTests {
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.ModelingResults;
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,op.NativeName.EndsWith("::BRepOffsetAPI_MakeOffset",StringComparison.Ordinal)?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,"Test.hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Modeling};
 [Fact]public void ExactCopiedModelingContractRejectsChangedSignaturesAndInvalidReceivers(){foreach(var op in Contract.Operations.Values){var d=Declaration(op);Assert.True(Contract.Contains(d));Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(Contract.Contains(d with{IsUnavailable=true}));Assert.False(Contract.Contains(d with{Access=BindingAccess.Protected}));Assert.False(Contract.Contains(d with{SupportState=BindingSupportState.Manual}));}}
 [Fact]public void ModelingResultEmissionIsDeterministicAndRequiresCompleteExactClosure(){var rows=Contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),Contract);Assert.Equal(3,a.Files.Count);Assert.Equal(a.Files.Select(x=>x.Content),b.Files.Select(x=>x.Content));Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Skip(1)),Contract));Assert.Equal(27,a.SourceStableIds.Count);}
}
