using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class WriterDiagnosticsContractTests {
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.DataExchange};
 [Fact]public void ExactAccessibleSignatureRequired(){var d=Declaration(WriterDiagnosticsOperationContract.Operations.Values.First());Assert.True(WriterDiagnosticsOperationContract.Contains(d));Assert.False(WriterDiagnosticsOperationContract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(WriterDiagnosticsOperationContract.Contains(d with{IsDeleted=true}));Assert.False(WriterDiagnosticsOperationContract.Contains(d with{Access=BindingAccess.Private}));Assert.False(WriterDiagnosticsOperationContract.Contains(d with{SupportState=BindingSupportState.Manual}));}
 [Fact]public void IncompleteContractCannotPublishManifest(){var d=Declaration(WriterDiagnosticsOperationContract.Operations.Values.First());Assert.Throws<InvalidDataException>(()=>WriterDiagnosticsOperationEmitter.Emit("8.0.1",new BindingModel([d])));}
 [Fact]public void ReorderedInputEmitsIdenticalModuleAndIds(){var rows=WriterDiagnosticsOperationContract.Operations.Values.Select(Declaration).ToArray();var a=WriterDiagnosticsOperationEmitter.Emit("8.0.1",new BindingModel(rows));var b=WriterDiagnosticsOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()));Assert.Equal(16,a.SourceStableIds.Count);Assert.Equal(a.Files.Select(f=>f.Content),b.Files.Select(f=>f.Content));Assert.All(a.Files,f=>Assert.Equal(OcctProductModule.DataExchange,f.ProductModule));}
 [Fact]public void MutationIsExcluded(){Assert.DoesNotContain(WriterDiagnosticsOperationContract.Operations.Values,o=>o.NativeName=="TransferBRep::SetTransientFromShape");}
}
