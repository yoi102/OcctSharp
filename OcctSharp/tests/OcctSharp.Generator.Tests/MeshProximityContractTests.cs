using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class MeshProximityContractTests {
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,"Test.hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Mesh};
 [Fact]public void ExactPrivateMeshContractRejectsSignatureAndUnsafeReceiverDrift(){var contract=ClosedWorkflowOperationContract.MeshProximity;foreach(var op in contract.Operations.Values){var d=Declaration(op);Assert.True(contract.Contains(d));Assert.False(contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(contract.Contains(d with{Access=BindingAccess.Protected}));Assert.False(contract.Contains(d with{IsDeleted=true}));Assert.False(contract.Contains(d with{IsVariadic=true}));}}
 [Fact]public void MeshProximityRequiresCompleteClosureAndIndependentCopiedOutputs(){var contract=ClosedWorkflowOperationContract.MeshProximity;var rows=contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),contract);Assert.Equal(3,a.Files.Count);Assert.Equal(22,a.SourceStableIds.Count);Assert.Equal(a.Files.Select(x=>x.Content),b.Files.Select(x=>x.Content));Assert.Contains(a.Files,x=>x.Content.Contains("ProximityCopy",StringComparison.Ordinal));Assert.Contains(a.Files,x=>x.Content.Contains("ProximityValueBudget",StringComparison.Ordinal));Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Skip(1)),contract));}
}
