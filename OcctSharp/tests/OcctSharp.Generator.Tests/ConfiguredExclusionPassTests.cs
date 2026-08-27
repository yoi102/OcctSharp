using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class ConfiguredExclusionPassTests
{
    [Fact]
    public void AppliesNarrowArtifactUnavailableDisposition()
    {
        BindingDeclaration available = Declaration("available");
        BindingDeclaration unavailable = Declaration("unavailable");
        BindingModel result = ConfiguredExclusionPass.Apply(
            new BindingModel([available, unavailable]),
            new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal)
            {
                ["unavailable"] = new BindingSkipReason(
                    "SK008",
                    "ArtifactUnavailable",
                    "The pinned import library does not export this symbol."),
            });

        Assert.Equal(BindingSupportState.Supported, result.Declarations[0].SupportState);
        Assert.Equal(BindingSupportState.Skipped, result.Declarations[1].SupportState);
        Assert.Equal("SK008", result.Declarations[1].SkipReason?.Code);
        Assert.Equal("ArtifactUnavailable", result.Declarations[1].SkipReason?.Category);
    }

    [Fact]
    public void RejectsUnknownStableId()
    {
        BindingModel model = new([Declaration("known")]);

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            ConfiguredExclusionPass.Apply(
                model,
                new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal)
                {
                    ["missing"] = new BindingSkipReason("SK007", "SuppressedConstruction", "test"),
                }));

        Assert.Contains("missing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AppliesWholePackageDispositionWithoutHidingOtherPackages()
    {
        BindingDeclaration excluded = Declaration("draw") with { SourcePackage = "Draw" };
        BindingDeclaration retained = Declaration("geom") with { SourcePackage = "Geom" };

        BindingModel result = ConfiguredExclusionPass.Apply(
            new BindingModel([excluded, retained]),
            new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal),
            new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal)
            {
                ["Draw"] = new BindingSkipReason(
                    "SK009",
                    "TestHarness",
                    "Draw is not part of the core runtime package."),
            });

        Assert.Equal(BindingSupportState.Skipped, result.Declarations[0].SupportState);
        Assert.Equal("SK009", result.Declarations[0].SkipReason?.Code);
        Assert.Equal(BindingSupportState.Supported, result.Declarations[1].SupportState);
    }

    private static BindingDeclaration Declaration(string stableId) => new(
        stableId,
        "Example::Value",
        BindingDeclarationKind.Method,
        "Example.hxx",
        1,
        1)
    {
        Access = BindingAccess.Public,
        SupportState = BindingSupportState.Supported,
    };
}
