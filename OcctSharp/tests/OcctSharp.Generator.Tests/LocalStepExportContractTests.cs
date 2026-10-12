using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class LocalStepExportContractTests {
 [Fact]public void PrivateSdkWriterRejectsPinnedSourceIdentityDrift(){string header=CopiedCollectionSupportEmitter.Template("local-step-export-writer-source-header.txt"),native=CopiedCollectionSupportEmitter.Template("local-step-export-writer-source-native.txt");var output=PrivateStepExportWriterEmitter.Transform(header,native);Assert.Contains("canonical leaf filenames",output.Native,StringComparison.Ordinal);Assert.DoesNotContain("Standard_EXPORT",output.Header,StringComparison.Ordinal);Assert.Throws<InvalidDataException>(()=>PrivateStepExportWriterEmitter.Transform(header,native+" "));}
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,"Test.hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Xde};
 [Fact]public void ExactSelectedExportContractRejectsSignatureAndUnsafeReceiverDrift(){var contract=ClosedWorkflowOperationContract.LocalStepExport;foreach(var op in contract.Operations.Values){var d=Declaration(op);Assert.True(contract.Contains(d));Assert.False(contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(contract.Contains(d with{Access=BindingAccess.Protected}));Assert.False(contract.Contains(d with{IsDeleted=true}));Assert.False(contract.Contains(d with{IsVariadic=true}));}}
 [Fact]public void SelectedExportRequiresCompleteClosureAndPrivateCanonicalWriter()
 {
  var contract=ClosedWorkflowOperationContract.LocalStepExport;
  var rows=contract.Operations.Values.Select(Declaration).ToArray();
  var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),contract);
  var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),contract);
  Assert.Equal(6,a.Files.Count);Assert.Equal(15,a.SourceStableIds.Count);
  Assert.Equal(a.Files.Select(x=>x.Content),b.Files.Select(x=>x.Content));
  Assert.Contains(a.Files,x=>x.Content.Contains("CloneShapeLabel",StringComparison.Ordinal));
  Assert.Contains(a.Files,x=>x.Content.Contains("canonical",StringComparison.Ordinal));
  string normalization=Assert.Single(a.Files,x=>x.RelativePath.EndsWith("LocalStepExportNormalization.Generated.hxx",StringComparison.Ordinal)).Content;
  Assert.Contains("placement*location->Get()",normalization,StringComparison.Ordinal);
  Assert.Contains("Assembly clone map is outside its exact source/destination document.",normalization,StringComparison.Ordinal);
  Assert.Contains("summary.occurrences+=descendant.occurrences",normalization,StringComparison.Ordinal);
  Assert.Contains("summary.height>16||summary.occurrences>4096",normalization,StringComparison.Ordinal);
  Assert.Contains(a.Files,x=>x.Content.Contains("ValidatePaths(selected,original->GetData())",StringComparison.Ordinal));
  Assert.Contains(a.Files,x=>x.Content.Contains("OcctSharpSelectedAssembly::Normalize(src,dst,labels);dst->UpdateAssemblies()",StringComparison.Ordinal));
  Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Skip(1)),contract));
 }
}
