using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class SelectionWorkspaceContractTests {
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.SelectionWorkspaces;
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Visualization};
 [Fact]public void ExactSelectionSignaturesAndAccessAreRequired(){foreach(var op in Contract.Operations.Values){var d=Declaration(op);Assert.True(Contract.Contains(d));Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(Contract.Contains(d with{Access=BindingAccess.Private}));Assert.False(Contract.Contains(d with{IsUnavailable=true}));}}
 [Fact]public void SupportAloneDoesNotPublishSelectionWorkflow(){var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(Contract.Operations.Values.Where(o=>o.Supporting).Select(Declaration)),Contract);Assert.Empty(a.Files);Assert.Empty(a.SourceStableIds);}
 [Fact]public void PartialSelectionClosureIsRejected(){Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel([Declaration(Contract.Operations.Values.First(o=>!o.Supporting))]),Contract));}
 [Fact]public void SelectionOutputIsDeterministicAndUnsafeDeclarationsStayDeferred(){var rows=Contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()),Contract);Assert.Equal(a.Files.Select(f=>f.Content),b.Files.Select(f=>f.Content));Assert.Equal(3,a.Files.Count);Assert.All(a.Files,f=>Assert.Equal(OcctProductModule.Visualization,f.ProductModule));Assert.DoesNotContain(Contract.Operations.Values,o=>o.NativeName.Contains("SetAnimation")||o.NativeName.Contains("SetActiveVolume")||o.NativeName.Contains("UpdateMouse")||o.Signature.Contains("std::istream")||o.Signature.Contains("&&"));}
}
