using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class SplineWorkspacesContractTests {
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Geometry};
 [Fact]public void ExactAccessibleSignatureRequired(){var d=Declaration(SplineWorkspacesOperationContract.Operations.Values.First());Assert.True(SplineWorkspacesOperationContract.Contains(d));Assert.False(SplineWorkspacesOperationContract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(SplineWorkspacesOperationContract.Contains(d with{IsDeleted=true}));Assert.False(SplineWorkspacesOperationContract.Contains(d with{Access=BindingAccess.Private}));Assert.False(SplineWorkspacesOperationContract.Contains(d with{SupportState=BindingSupportState.Manual}));}
 [Fact]public void IncompleteContractCannotPublishManifest(){var d=Declaration(SplineWorkspacesOperationContract.Operations.Values.First());Assert.Throws<InvalidDataException>(()=>SplineWorkspacesOperationEmitter.Emit("8.0.1",new BindingModel([d])));}
 [Fact]public void ReorderedInputEmitsIdenticalModuleAndIds(){var rows=SplineWorkspacesOperationContract.Operations.Values.Select(Declaration).ToArray();var a=SplineWorkspacesOperationEmitter.Emit("8.0.1",new BindingModel(rows));var b=SplineWorkspacesOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()));Assert.Equal(31,a.SourceStableIds.Count);Assert.Equal(a.Files.Select(f=>f.Content),b.Files.Select(f=>f.Content));Assert.All(a.Files,f=>Assert.Equal(OcctProductModule.Geometry,f.ProductModule));}
 [Fact]public void UnsafeSurfaceInlineAndPackedLayoutsRemainExcluded(){Assert.DoesNotContain(SplineWorkspacesOperationContract.Operations.Values,o=>o.NativeName.StartsWith("BSplSLib::CoefsD",StringComparison.Ordinal)||o.NativeName=="BSplCLib::FlatBezierKnots"||o.Signature.Contains("EvaluatorFunction",StringComparison.Ordinal));}
}
