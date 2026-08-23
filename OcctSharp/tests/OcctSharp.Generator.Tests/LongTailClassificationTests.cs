using OcctSharp.Generator.Inventory;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class LongTailClassificationTests
{
    [Fact]
    public void ClassifiesEveryDeclarationAndHeaderWithoutPendingState()
    {
        BindingDeclaration supported = Create("supported", BindingDeclarationKind.Method) with
        {
            SupportState = BindingSupportState.Supported,
        };
        BindingDeclaration instance = Create("instance", BindingDeclarationKind.Method);
        BindingDeclaration skipped = Create("skipped", BindingDeclarationKind.Record) with
        {
            SupportState = BindingSupportState.Skipped,
            SkipReason = new BindingSkipReason("SK005", "Template", "test"),
        };
        OcctInventoryFailure failure = new(
            "IVtk_Types.hxx",
            "error: 'vtkType.h' file not found");

        OcctFinalClassification result = LongTailClassification.Create(
            [supported, instance, skipped],
            ["Good.hxx", "IVtk_Types.hxx"],
            new HashSet<string>(["Good.hxx"], StringComparer.Ordinal),
            [failure]);

        Assert.True(result.IsComplete);
        Assert.Equal(3, result.DeclarationClassified);
        Assert.Equal(0, result.DeclarationPending);
        Assert.Equal("SupportedUnselected", Find(result, "supported").State);
        Assert.Equal("LT002", Find(result, "instance").Code);
        Assert.Equal("SK005", Find(result, "skipped").Code);
        Assert.Contains(result.Headers, item => item.Header == "IVtk_Types.hxx"
            && item.Code == "HD001"
            && item.State == "BlockedExternalDependency");
    }

    [Fact]
    public void MarksManifestOwnedStableIdsAsEmittedInsteadOfUnselected()
    {
        BindingDeclaration supported = Create("supported", BindingDeclarationKind.Method) with
        {
            SupportState = BindingSupportState.Supported,
        };

        OcctFinalClassification result = LongTailClassification.Create(
            [supported],
            ["Good.hxx"],
            new HashSet<string>(["Good.hxx"], StringComparer.Ordinal),
            [],
            new HashSet<string>([supported.StableId], StringComparer.Ordinal));

        OcctDeclarationDisposition disposition = Assert.Single(result.Declarations);
        Assert.Equal("Emitted", disposition.State);
        Assert.Equal("EM001", disposition.Code);
        Assert.Contains(result.DeclarationStates, item => item.State == "Emitted" && item.Count == 1);
        Assert.DoesNotContain(result.DeclarationStates, item => item.State == "SupportedUnselected");
    }

    [Theory]
    [InlineData("OpenGl_GLESExtensions.hxx", "HD002")]
    [InlineData("RWGltf_GltfOStreamWriter.hxx", "HD003")]
    [InlineData("NCollection_Haft.h", "HD004")]
    [InlineData("BOPDS_Map.hxx", "HD005")]
    public void AssignsStableHeaderFailureCodes(string header, string expectedCode)
    {
        OcctFinalClassification result = LongTailClassification.Create(
            [],
            [header],
            new HashSet<string>(StringComparer.Ordinal),
            [new OcctInventoryFailure(header, "error: dependency file not found")]);

        Assert.True(result.IsComplete);
        Assert.Equal(expectedCode, Assert.Single(result.Headers).Code);
    }

    private static BindingDeclaration Create(string id, BindingDeclarationKind kind) => new(
        id,
        id,
        kind,
        "Test.hxx",
        1,
        1)
    {
        Access = BindingAccess.Public,
    };

    private static OcctDeclarationDisposition Find(OcctFinalClassification result, string id) =>
        Assert.Single(result.Declarations, item => item.StableId == id);
}
