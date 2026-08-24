using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class ManualBindingPassTests
{
    [Fact]
    public void MarksConfiguredStableIdAsManualAndPreservesOtherDeclarations()
    {
        BindingModel model = new(
        [
            Declaration("manual"),
            Declaration("pending"),
        ]);

        BindingModel result = ManualBindingPass.Apply(
            model,
            [new ManualBindingConfiguration { StableId = "manual", SpecialCaseId = "SC-032" }]);

        Assert.Equal(BindingSupportState.Manual, Find(result, "manual").SupportState);
        Assert.Equal(BindingSupportState.Pending, Find(result, "pending").SupportState);
    }

    [Fact]
    public void RejectsMissingStableIdsAndInvalidSpecialCaseReferences()
    {
        BindingModel model = new([Declaration("known")]);

        Assert.Throws<InvalidDataException>(() => ManualBindingPass.Apply(
            model,
            [new ManualBindingConfiguration { StableId = "missing", SpecialCaseId = "SC-032" }]));
        Assert.Throws<InvalidDataException>(() => ManualBindingPass.Apply(
            model,
            [new ManualBindingConfiguration { StableId = "known", SpecialCaseId = "ADR-0052" }]));
    }

    private static BindingDeclaration Declaration(string stableId) => new(
        stableId,
        stableId,
        BindingDeclarationKind.Method,
        "Test.hxx",
        1,
        1)
    {
        Access = BindingAccess.Public,
    };

    private static BindingDeclaration Find(BindingModel model, string stableId) =>
        Assert.Single(model.Declarations, declaration => declaration.StableId == stableId);
}
