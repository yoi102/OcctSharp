using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;

public sealed class DocumentAdapterContractTests
{
 static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.DocumentAdapters;
 static BindingType Value(string name)=>new(name,name,name,name,[new(BindingTypeLayerKind.Value,false)],null,[],false,null);
 static BindingDeclaration Row(CopiedCollectionOperationContract.Operation o)=>new(o.StableId,o.NativeName,
  o.NativeName.Split("::") is var words&&words[0]==words[1]?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,
  o.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=o.Signature,Access=BindingAccess.Public,SupportState=BindingSupportState.Pending};
 [Fact] public void ExactAdmissionRejectsUnsafeDriftAndRequiresTheFullClosure()
 {
  Assert.Equal(125,Contract.Operations.Count);
  foreach(var op in Contract.Operations.Values){var row=Row(op);Assert.True(Contract.Contains(row));Assert.False(Contract.Contains(row with{NativeSignature=row.NativeSignature+" "}));Assert.False(Contract.Contains(row with{IsTemplated=true}));Assert.False(Contract.Contains(row with{Access=BindingAccess.Private}));Assert.False(Contract.Contains(row with{SupportState=BindingSupportState.Manual}));}
  Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new([Row(Contract.Operations.Values.First())]),Contract));
 }
 [Fact] public void SemanticDelegationRetainsTypedLookupShortCircuitAndQualifiedDriverBaseCalls()
 {
  var lookup=Row(Contract.Operations.Values.Single(o=>o.NativeName=="TDF_Label::FindAttribute")) with{ReturnType=Value("bool"),IsConst=true,Parameters=[new(0,"id",Value("const Standard_GUID&"),false),new(1,"result",Value("occ::handle<TDF_Attribute>&"),false)]};
  var driver=Row(Contract.Operations.Values.First(o=>o.NativeName=="BinMDataStd_NamedDataDriver::Paste")) with{ReturnType=Value("void"),IsConst=true};
  var ctor=Row(Contract.Operations.Values.Single(o=>o.NativeName=="TNaming_Builder::TNaming_Builder")) with{Parameters=[new(0,"label",Value("const TDF_Label&"),false)]};
  var output=PrivateDocumentOperationEmitter.Emit("8.0.1",[lookup,driver,ctor],Contract,"");
  string source=Assert.Single(output.Files).Content;
  Assert.Contains("static_cast<Signature",source);
  Assert.Contains("TDF_Label_FindAttribute(receiver,id,base)&&!(result=occ::down_cast<T>(base)).IsNull()",source);
  Assert.Contains("receiver.BinMDataStd_NamedDataDriver::Paste",source);
  Assert.Contains("class PrivateTNamingBuilder : public TNaming_Builder",source);
  Assert.DoesNotContain("OCCTSHARP_CALL",source);
  Assert.DoesNotContain("occtsharp_",source);
 }
 [Fact] public void OverloadsWithDifferentReceiverContractsFailClosed()
 {
  var op=Contract.Operations.Values.First(o=>o.NativeName=="TDataStd_NamedData::GetID");var first=Row(op) with{ReturnType=Value("const Standard_GUID&"),IsStatic=true};
  Assert.Throws<InvalidDataException>(()=>PrivateDocumentOperationEmitter.Emit("8.0.1",[first,first with{StableId="drift",IsStatic=false}],Contract,""));
 }
}
