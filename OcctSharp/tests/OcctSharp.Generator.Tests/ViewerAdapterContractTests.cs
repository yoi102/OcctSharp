using System.Text.Json;
using System.Text.Json.Serialization;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class ViewerAdapterContractTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
    private static BindingDeclaration[] Rows()
    {
        using var stream = typeof(ViewerAdapterContractTests).Assembly.GetManifestResourceStream("OcctSharp.Generator.Tests.Fixtures.viewer-adapter-model.json")!;
        return JsonSerializer.Deserialize<BindingDeclaration[]>(stream, Options)!;
    }
    [Fact]
    public void ViewerProfileRejectsSignatureAccessAndReceiverDrift()
    {
        var contract = ClosedWorkflowOperationContract.ViewerAdapters;
        var rows = Rows().Where(contract.Contains).ToArray(); Assert.Equal(36, rows.Length);
        foreach (var row in rows)
        {
            Assert.False(contract.Contains(row with { NativeSignature = row.NativeSignature + " const" }));
            Assert.False(contract.Contains(row with { Access = BindingAccess.Private }));
            Assert.False(contract.Contains(row with { IsDeleted = true }));
            Assert.False(contract.Contains(row with { IsPureVirtual = true }));
            Assert.False(contract.Contains(row with { SupportState = BindingSupportState.Manual }));
        }
        var first = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows), contract);
        var second = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Reverse()), contract);
        Assert.Equal(first.Files.Select(f => f.Content), second.Files.Select(f => f.Content));
        string source = Assert.Single(first.Files).Content;
        Assert.Contains("inline Graphic3d_ClipPlane* ConstructGraphic3dClipPlane", source);
        Assert.Contains("return new Graphic3d_ClipPlane", source);
        Assert.Contains("decltype(auto)", source); Assert.Contains("_Bind", source);
        Assert.Contains("const AIS_Manipulator::OptionsForAttach &", source);
        Assert.DoesNotContain("class PrivateGraphic3dClipPlane", source);
        Assert.DoesNotContain("OCCTSHARP_CALL", source);
        Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Skip(1)), contract));
    }
    [Fact]
    public void DocumentIdentityUsesOriginalGuidValueAndExactVirtualReceiver()
    {
        var contract = ClosedWorkflowOperationContract.DocumentIdentityAdapters;
        var rows = Rows().Where(contract.Contains).ToArray(); Assert.Equal(4, rows.Length);
        string source = Assert.Single(ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows), contract).Files).Content;
        Assert.Contains("inline Standard_GUID ConstructStandardGUID", source);
        Assert.Contains("TDF_Attribute_DynamicType_Bind", source);
        Assert.Contains("#include <TDF_Attribute.hxx>", source);
        Assert.DoesNotContain("class PrivateStandardGUID", source);
        Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Skip(1)), contract));
    }
}
