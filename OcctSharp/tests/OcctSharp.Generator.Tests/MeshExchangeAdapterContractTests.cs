using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;

public sealed class MeshExchangeAdapterContractTests
{
    static readonly System.Text.Json.JsonSerializerOptions ModelOptions=new(){PropertyNameCaseInsensitive=true,Converters={new System.Text.Json.Serialization.JsonStringEnumConverter()}};
    [Fact] public void ResourceOwnersRequireAllExactOperationsAndPreserveSdkFactoryTypes()
    {
        var contract=ClosedWorkflowOperationContract.MeshResourceAdapters;Assert.Equal(9,contract.Operations.Count);
        using var stream=typeof(MeshExchangeAdapterContractTests).Assembly.GetManifestResourceStream("OcctSharp.Generator.Tests.Fixtures.mesh-resource-model.json")!;
        var rows=System.Text.Json.JsonSerializer.Deserialize<BindingDeclaration[]>(stream,ModelOptions)!.Where(contract.Contains).ToArray();Assert.Equal(9,rows.Length);
        foreach(var row in rows){Assert.True(contract.Contains(row));Assert.False(contract.Contains(row with{NativeSignature=row.NativeSignature+" const"}));Assert.False(contract.Contains(row with{Access=BindingAccess.Private}));}
        var first=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows),contract);var second=ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Reverse()),contract);
        Assert.Equal(first.Files.Select(x=>x.Content),second.Files.Select(x=>x.Content));Assert.Single(first.Files);Assert.Contains("inline NCollection_Buffer ConstructNCollectionBuffer",first.Files[0].Content);Assert.Contains("inline Image_Texture ConstructImageTexture",first.Files[0].Content);
        Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new(rows.Skip(1)),contract));
    }
    [Fact] public void ExactAdmissionRejectsDriftAndMutableSessionOverloads()
    {
        var contract=ClosedWorkflowOperationContract.MeshExchangeAdapters;
        Assert.Equal(35,contract.Operations.Count);
        foreach(var op in contract.Operations.Values)
        {
            var row=new BindingDeclaration(op.StableId,op.NativeName,BindingDeclarationKind.Method,"test.hxx",1,1)
            {Access=BindingAccess.Public,SupportState=BindingSupportState.Pending,NativeSignature=op.Signature};
            Assert.True(contract.Contains(row));Assert.False(contract.Contains(row with{NativeSignature=op.Signature+" const"}));Assert.False(contract.Contains(row with{IsDeleted=true}));
            Assert.DoesNotContain("XSControl_WorkSession",op.Signature);
            if(op.Supporting)Assert.Empty(ClosedWorkflowOperationEmitter.Emit("8.0.1",new([row]),contract).Files);
            else Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new([row]),contract));
        }
    }
    [Fact] public void WriterFactoriesReturnOriginalSdkTypeWithoutDerivedReceiver()
    {
        var row=new BindingDeclaration("test","IGESCAFControl_Writer::IGESCAFControl_Writer",BindingDeclarationKind.Constructor,"IGESCAFControl_Writer.hxx",1,1)
        {NativeSignature="IGESCAFControl_Writer::IGESCAFControl_Writer: void ()"};
        var source=Assert.Single(PrivateDocumentOperationEmitter.Emit("8.0.1",[row],ClosedWorkflowOperationContract.MeshExchangeAdapters,"").Files).Content;
        Assert.Contains("inline IGESCAFControl_Writer ConstructIGESCAFControlWriter(){return IGESCAFControl_Writer();}",source);
        Assert.DoesNotContain("class PrivateIGESCAFControlWriter",source);
        Assert.DoesNotContain("OCCTSHARP_CALL",source);
    }
}
