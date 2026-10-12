using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;

public sealed class XdeAdapterContractTests
{
    static BindingType Value(string name) => new(name,name,name,name,[new(BindingTypeLayerKind.Value,false)],null,[],false,null);
    static BindingDeclaration Row(string name, string signature, BindingDeclarationKind kind) =>
        new("test",name,kind,name.Split("::")[0]+".hxx",1,1)
        { NativeSignature=signature,Access=BindingAccess.Public,SupportState=BindingSupportState.Pending };

    [Fact] public void SemanticPointerRetainsNoexceptAndBorrowedReturn()
    {
        var row=Row("Quantity_ColorRGBA::GetRGB","Quantity_ColorRGBA::GetRGB: const Quantity_Color &() const noexcept",BindingDeclarationKind.Method)
            with { ReturnType=Value("const Quantity_Color &"),IsConst=true };
        string source=Assert.Single(PrivateDocumentOperationEmitter.Emit("8.0.1",[row],ClosedWorkflowOperationContract.XdeAdapters,"").Files).Content;
        Assert.Contains("const Quantity_Color & (Quantity_ColorRGBA::*)() const noexcept",source);
        Assert.Contains("inline decltype(auto) Quantity_ColorRGBA_GetRGB(const Quantity_ColorRGBA& receiver",source);
        Assert.Contains("inline auto Quantity_ColorRGBA_GetRGB_Bind(const Quantity_ColorRGBA& receiver)",source);
        Assert.Contains("[address=&receiver](auto&&... args)->decltype(auto)",source);
        Assert.DoesNotContain("OCCTSHARP_CALL",source);
    }

    [Fact] public void ValueConstructionReturnsExactSdkTypeWithoutChangingCopyAndReturnTypes()
    {
        var pbr=Row("XCAFDoc_VisMaterialPBR::XCAFDoc_VisMaterialPBR","XCAFDoc_VisMaterialPBR::XCAFDoc_VisMaterialPBR: void ()",BindingDeclarationKind.Constructor);
        var rgba=Row("Quantity_ColorRGBA::Quantity_ColorRGBA","Quantity_ColorRGBA::Quantity_ColorRGBA: void (float, float, float, float)",BindingDeclarationKind.Constructor)
            with { Parameters=Enumerable.Range(0,4).Select(i=>new BindingParameter(i,"arg"+i,Value("float"),false)).ToArray() };
        string source=Assert.Single(PrivateDocumentOperationEmitter.Emit("8.0.1",[pbr,rgba],ClosedWorkflowOperationContract.XdeAdapters,"").Files).Content;
        Assert.Contains("inline XCAFDoc_VisMaterialPBR ConstructXCAFDocVisMaterialPBR(){return XCAFDoc_VisMaterialPBR();}",source);
        Assert.Contains("inline Quantity_ColorRGBA ConstructQuantityColorRGBA(float arg0,float arg1,float arg2,float arg3)",source);
        Assert.DoesNotContain("class PrivateQuantityColorRGBA",source);
        Assert.DoesNotContain("class PrivateXCAFDocVisMaterialPBR",source);
    }

    [Fact] public void ExactXdeContractRejectsPartialClosureAndSignatureDrift()
    {
        var contract=ClosedWorkflowOperationContract.XdeAdapters;
        foreach(var op in contract.Operations.Values)
        {
            var row=Row(op.NativeName,op.Signature,BindingDeclarationKind.Method) with {StableId=op.StableId};
            Assert.True(contract.Contains(row));
            Assert.False(contract.Contains(row with{NativeSignature=row.NativeSignature+" "}));
            Assert.False(contract.Contains(row with{IsTemplated=true}));
            Assert.False(contract.Contains(row with{SupportState=BindingSupportState.Manual}));
        }
        var first=contract.Operations.Values.First();
        var admitted=Row(first.NativeName,first.Signature,BindingDeclarationKind.Method) with {StableId=first.StableId};
        Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new([admitted]),contract));
    }
}
