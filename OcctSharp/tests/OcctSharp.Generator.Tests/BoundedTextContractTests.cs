using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class BoundedTextContractTests {
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,"Test.hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Foundation};
 [Fact]public void ExactLengthAndTransformContractRejectsSignatureAndUnsafeReceiverDrift(){var contract=ClosedWorkflowOperationContract.BoundedTextTransforms;foreach(var op in contract.Operations.Values){var d=Declaration(op);Assert.True(contract.Contains(d));Assert.False(contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(contract.Contains(d with{Access=BindingAccess.Protected}));Assert.False(contract.Contains(d with{IsDeleted=true}));Assert.False(contract.Contains(d with{IsVariadic=true}));}}
 [Fact]public void BoundedTextRequiresCompleteClosureAndAtomicCopiedOutputs(){var contract=ClosedWorkflowOperationContract.BoundedTextTransforms;var rows=contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),contract);Assert.Equal(3,a.Files.Count);Assert.Equal(14,a.SourceStableIds.Count);Assert.Equal(a.Files.Select(x=>x.Content),b.Files.Select(x=>x.Content));Assert.Contains(a.Files,x=>x.Content.Contains("TransformBoundary",StringComparison.Ordinal));Assert.Contains(a.Files,x=>x.Content.Contains("reservation",StringComparison.Ordinal));Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Skip(1)),contract));}
}
