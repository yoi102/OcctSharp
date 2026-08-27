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
        Assert.Equal("BL101", Find(result, "instance").Code);
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

    [Fact]
    public void MarksConfiguredManualStableIdsAndRejectsManifestOverlap()
    {
        BindingDeclaration declaration = Create("manual", BindingDeclarationKind.Method);
        HashSet<string> manual = new([declaration.StableId], StringComparer.Ordinal);

        OcctFinalClassification result = LongTailClassification.Create(
            [declaration],
            ["Good.hxx"],
            new HashSet<string>(["Good.hxx"], StringComparer.Ordinal),
            [],
            emittedStableIds: null,
            manualStableIds: manual);

        OcctDeclarationDisposition disposition = Assert.Single(result.Declarations);
        Assert.Equal("Manual", disposition.State);
        Assert.Equal("MN001", disposition.Code);
        Assert.Throws<InvalidDataException>(() => LongTailClassification.Create(
            [declaration],
            ["Good.hxx"],
            new HashSet<string>(["Good.hxx"], StringComparer.Ordinal),
            [],
            emittedStableIds: manual,
            manualStableIds: manual));
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

    [Fact]
    public void AssignsConfiguredPackageDisposition()
    {
        BindingDeclaration declaration = Create("draw", BindingDeclarationKind.Method) with
        {
            SourcePackage = "Draw",
            SupportState = BindingSupportState.Supported,
        };

        OcctFinalClassification result = LongTailClassification.Create(
            [declaration],
            ["Draw.hxx"],
            new HashSet<string>(["Draw.hxx"], StringComparer.Ordinal),
            [],
            excludedPackages: new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal)
            {
                ["Draw"] = new BindingSkipReason("SK009", "TestHarness", "test-only"),
            });

        OcctDeclarationDisposition disposition = Assert.Single(result.Declarations);
        Assert.Equal("Skipped", disposition.State);
        Assert.Equal("SK009", disposition.Code);
        Assert.Equal("TestHarness", disposition.Category);
    }

    [Fact]
    public void ReplacesBroadLongTailBucketsWithStructuralDispositions()
    {
        BindingType pointer = new(
            "double *",
            "double *",
            "double",
            "double",
            [
                new BindingTypeLayer(BindingTypeLayerKind.PointerIndirection, false),
                new BindingTypeLayer(BindingTypeLayerKind.Value, false),
            ],
            null,
            [],
            false,
            null);
        BindingDeclaration record = Create("record", BindingDeclarationKind.Record);
        BindingDeclaration function = Create("function", BindingDeclarationKind.Function) with
        {
            SourcePackage = "Standard",
            SourceToolkit = "TKernel",
            ReturnType = pointer,
        };

        OcctFinalClassification result = LongTailClassification.Create(
            [record, function],
            ["Good.hxx"],
            new HashSet<string>(["Good.hxx"], StringComparer.Ordinal),
            []);

        Assert.Equal("SK012", Find(result, "record").Code);
        Assert.Equal("BL202", Find(result, "function").Code);
        Assert.DoesNotContain(result.DeclarationReasons, item =>
            item.Code is "LT001" or "LT002" or "LT003" or "LT004");
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
