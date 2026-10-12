using System.Text.Json;
using System.Text.Json.Serialization;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class RenderResourceAdapterContractTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
    [Fact]
    public void ExactResourceContractPreservesAllocationCopyDefaultsAndBorrowedReturns()
    {
        using var stream = typeof(RenderResourceAdapterContractTests).Assembly.GetManifestResourceStream("OcctSharp.Generator.Tests.Fixtures.render-resource-adapter-model.json")!;
        var rows = JsonSerializer.Deserialize<BindingDeclaration[]>(stream, Options)!;
        var contract = ClosedWorkflowOperationContract.RenderResourceAdapters;
        Assert.Equal(80, rows.Length);
        foreach (var row in rows)
        {
            Assert.True(contract.Contains(row));
            Assert.False(contract.Contains(row with { NativeSignature = row.NativeSignature + " const" }));
            Assert.False(contract.Contains(row with { Access = BindingAccess.Private }));
            Assert.False(contract.Contains(row with { SupportState = BindingSupportState.Manual }));
        }
        var first = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows), contract);
        var reversed = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Reverse()), contract);
        Assert.Equal(first.Files.Select(f => f.Content), reversed.Files.Select(f => f.Content));
        string source = Assert.Single(first.Files).Content;
        Assert.Contains("inline Graphic3d_CubeMapPacked* ConstructGraphic3dCubeMapPacked(const occ::handle<Image_PixMap> & arg0){return new Graphic3d_CubeMapPacked(arg0);}", source);
        Assert.Contains("inline Graphic3d_ZLayerSettings ConstructGraphic3dZLayerSettings(const Graphic3d_ZLayerSettings & arg0){return Graphic3d_ZLayerSettings(arg0);}", source);
        Assert.Contains("return new PrsDim_LengthDimension", source);
        Assert.Contains("OpenGl_GraphicDriver_GetSharedContext_Bind", source);
        Assert.Contains("decltype(auto)", source);
        Assert.DoesNotContain("class Private", source);
        Assert.DoesNotContain("OCCTSHARP_CALL", source);
        Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Skip(1)), contract));
    }
}
