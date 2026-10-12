using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class BatchBAContractTests {
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.AssemblyDiagnostics;
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public};
 [Fact]public void ExactAdmissionRejectsDriftAndUninitializedIncompleteClosure(){foreach(var op in Contract.Operations.Values){var d=Declaration(op);Assert.True(Contract.Contains(d));Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(Contract.Contains(d with{Access=BindingAccess.Private}));Assert.False(Contract.Contains(d with{IsPureVirtual=true}));}Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel([Declaration(Contract.Operations.Values.First(x=>!x.Supporting))]),Contract));}
 [Fact]public void SupportAloneCannotPublishAndCompleteEmissionIsDeterministic(){Assert.Empty(ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(Contract.Operations.Values.Where(x=>x.Supporting).Select(Declaration)),Contract).Files);var rows=Contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()),Contract);Assert.Equal(a.Files.Select(f=>f.Content),b.Files.Select(f=>f.Content));Assert.Equal(3,a.Files.Count);}
}
