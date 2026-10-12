using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class MeshAdapterContractTests {
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.MeshAdapters;
 static BindingDeclaration Row(CopiedCollectionOperationContract.Operation op){string owner=op.NativeName.Split("::")[0];return new(op.StableId,op.NativeName,op.NativeName.EndsWith("::"+owner,StringComparison.Ordinal)?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,owner+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,SupportState=BindingSupportState.Pending};}
 [Fact] public void ExactClosureRejectsSignatureAccessAndUnsafeDrift(){foreach(var op in Contract.Operations.Values){var row=Row(op);Assert.True(Contract.Contains(row));Assert.False(Contract.Contains(row with{NativeSignature=row.NativeSignature+" "}));Assert.False(Contract.Contains(row with{IsTemplated=true}));Assert.False(Contract.Contains(row with{Access=BindingAccess.Private}));if(!op.Supporting)Assert.False(Contract.Contains(row with{SupportState=BindingSupportState.Manual}));}Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new([Row(Contract.Operations.Values.First(o=>!o.Supporting))]),Contract));}
 [Fact] public void DeterministicPrivateHeaderCopiesReadbackAndContainsNoPublicReceiver(){var rows=Contract.Operations.Values.Select(Row).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),Contract);Assert.Single(a.Files);Assert.Equal(a.Files[0].Content,b.Files[0].Content);Assert.Equal(30,a.SourceStableIds.Count);Assert.Contains("CopiedMeshTriangle",a.Files[0].Content);Assert.Contains("CopiedCoherentTriangle",a.Files[0].Content);Assert.Contains("ClearOutput(){value.ChangeOutput().Nullify();}",a.Files[0].Content);Assert.DoesNotContain("OCCTSHARP_CALL",a.Files[0].Content);Assert.DoesNotContain("ChangeOutput()const",a.Files[0].Content);}
 [Fact] public void SupportingSignaturesAloneCannotPublishAdapters(){var output=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(Contract.Operations.Values.Where(o=>o.Supporting).Select(Row)),Contract);Assert.Empty(output.Files);Assert.Empty(output.SourceStableIds);}
 [Fact]
 public void GenericSharedEmitterExcludesPrivateOperationsButPreservesReusedSupport()
 {
  static BindingType Value(string name) => new(name,name,name,name,[new(BindingTypeLayerKind.Value,false)],null,[],false,null);
  var privateOperations=Contract.Operations.Values.Where(o=>o.NativeName is "Poly_Triangulation::Normal" or "Poly_Triangulation::UVNode" && !o.Signature.Contains("void",StringComparison.Ordinal)).ToArray();
  var support=Contract.Operations.Values.Single(o=>o.NativeName=="Poly_CoherentTriangulation::NTriangles");
  var rows=privateOperations.Select(o=>Row(o) with {
   SourcePackage="Poly",SupportState=BindingSupportState.Supported,IsConst=true,
   ReturnType=Value(o.NativeName.EndsWith("Normal",StringComparison.Ordinal)?"gp_Dir":"gp_Pnt2d"),
   Parameters=[new(0,"index",Value("int"),false)]
  }).Append(Row(support) with {SourcePackage="Poly",SupportState=BindingSupportState.Supported,IsConst=true,ReturnType=Value("int")}).ToArray();
  SharedHandleScopeConfiguration Scope(string name)=>new(){SourcePackage="Poly",NativeType=name,Header=name+".hxx",ManagedTypeName=name.Replace("_","",StringComparison.Ordinal),ExportNamePrefix=name.ToLowerInvariant(),SuppressConstructors=true};
  var scopes=new[]{Scope("Poly_Triangulation"),Scope("Poly_CoherentTriangulation")};
  var emitted=SharedHandleBindingEmitter.Emit("8.0.1",new(rows),scopes);
  Assert.Equal(new[]{support.StableId},emitted.SourceStableIds);
  var baseline=SharedHandleBindingEmitter.Emit("8.0.1",new(rows.Where(r=>r.StableId==support.StableId)),scopes);
  Assert.Equal(baseline.Files.Select(f=>f.Content),emitted.Files.Select(f=>f.Content));
  Assert.Equal(3,rows.Length);
 }
}
