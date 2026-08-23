using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class SupportClassificationPassTests
{
    [Fact]
    public void AssignsStableSkipReasonsInRuleOrder()
    {
        BindingDeclaration candidate = CreateDeclaration("candidate");
        BindingDeclaration unavailableAndPrivate = CreateDeclaration("unavailable") with
        {
            IsUnavailable = true,
            Access = BindingAccess.Private,
        };
        BindingDeclaration deleted = CreateDeclaration("deleted") with { IsDeleted = true };
        BindingDeclaration nonPublic = CreateDeclaration("private") with { Access = BindingAccess.Private };
        BindingDeclaration variadic = CreateDeclaration("variadic") with { IsVariadic = true };
        BindingDeclaration template = CreateDeclaration("template") with { IsTemplated = true };
        BindingDeclaration overloadedOperator = CreateDeclaration("operator") with { IsOverloadedOperator = true };

        BindingModel result = SupportClassificationPass.Apply(new BindingModel(
        [
            overloadedOperator,
            template,
            variadic,
            nonPublic,
            deleted,
            unavailableAndPrivate,
            candidate,
        ]));

        Assert.Equal(BindingSupportState.Pending, Find(result, "candidate").SupportState);
        Assert.Null(Find(result, "candidate").SkipReason);
        Assert.Equal("SK001", Find(result, "unavailable").SkipReason?.Code);
        Assert.Equal("SK002", Find(result, "deleted").SkipReason?.Code);
        Assert.Equal("SK003", Find(result, "private").SkipReason?.Code);
        Assert.Equal("SK004", Find(result, "variadic").SkipReason?.Code);
        Assert.Equal("SK005", Find(result, "template").SkipReason?.Code);
        Assert.Equal("SK006", Find(result, "operator").SkipReason?.Code);
    }

    [Fact]
    public void SummaryIsCompleteAndSortedByStableReasonCode()
    {
        BindingModel result = SupportClassificationPass.Apply(new BindingModel(
        [
            CreateDeclaration("candidate"),
            CreateDeclaration("private-a") with { Access = BindingAccess.Private },
            CreateDeclaration("private-b") with { Access = BindingAccess.Protected },
            CreateDeclaration("variadic") with { IsVariadic = true },
        ]));

        BindingSupportSummary summary = BindingSupportSummary.Create(result);

        Assert.Equal(4, summary.Total);
        Assert.Equal(1, summary.Pending);
        Assert.Equal(3, summary.Skipped);
        Assert.Equal(["SK003", "SK004"], summary.SkipReasons.Keys);
        Assert.Equal(2, summary.SkipReasons["SK003"]);
    }

    [Fact]
    public void PreservesManuallyClassifiedDeclarations()
    {
        BindingDeclaration manual = CreateDeclaration("manual") with
        {
            SupportState = BindingSupportState.Manual,
        };

        BindingModel result = SupportClassificationPass.Apply(new BindingModel([manual]));

        Assert.Equal(BindingSupportState.Manual, Find(result, "manual").SupportState);
        Assert.Null(Find(result, "manual").SkipReason);
    }

    private static BindingDeclaration CreateDeclaration(string name)
    {
        return new BindingDeclaration(
            $"function:{name}",
            name,
            BindingDeclarationKind.Function,
            "Sample.hxx",
            1,
            1);
    }

    private static BindingDeclaration Find(BindingModel model, string name)
    {
        return Assert.Single(model.Declarations, declaration => declaration.NativeName == name);
    }
}
