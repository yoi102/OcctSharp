using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Reporting;

namespace OcctSharp.Generator.Tests;

public sealed class GeneratedDependencyClosureTests
{
    [Fact]
    public void ReportsObservedCycleAndTargetGraphViolationsWithoutLosingStableIdEvidence()
    {
        BindingModel model = new(
        [
            Record("foundation-record", "Foundation_Type", OcctProductModule.Foundation),
            Record("geometry-record", "Geometry_Type", OcctProductModule.Geometry),
            Method("foundation-to-geometry", "Foundation_Type::Geometry", OcctProductModule.Foundation, Handle("Geometry_Type")),
            Method("geometry-to-foundation", "Geometry_Type::Foundation", OcctProductModule.Geometry, Handle("Foundation_Type")),
        ]);
        GeneratedDependencyClosureReport report = Create(
            model,
            ["foundation-to-geometry", "geometry-to-foundation"]);

        Assert.True(report.IsComplete);
        Assert.False(report.ManagedProjectSplitReady);
        Assert.False(report.NativeDllSplitReady);
        Assert.Equal("KeepSingleManagedProjectAndNativeDll", report.RecommendedDecision);
        Assert.Contains(report.DirectDependencies, edge =>
            edge.SourceModule == "Foundation"
            && edge.TargetModule == "Geometry"
            && !edge.AllowedByTargetGraph
            && edge.SourceStableIds.SequenceEqual(["foundation-to-geometry"]));
        Assert.Contains(report.CyclicGroups, group =>
            group.Modules.SequenceEqual(["Foundation", "Geometry"]));
        Assert.Contains(report.Issues, issue => issue.Code == "SD002");
    }

    [Fact]
    public void FailsClosureCompletenessForAnUnresolvedEmittedHandleTarget()
    {
        BindingModel model = new(
        [
            Record("modeling-record", "Modeling_Type", OcctProductModule.Modeling),
            Method("missing-target", "Modeling_Type::Missing", OcctProductModule.Modeling, Handle("Missing_Type")),
        ]);
        GeneratedDependencyClosureReport report = Create(model, ["missing-target"]);

        Assert.False(report.IsComplete);
        GeneratedDependencyIssue issue = Assert.Single(report.Issues, item => item.Code == "SD001");
        Assert.Equal("missing-target", issue.SourceStableId);
        Assert.Equal("Missing_Type", issue.NativeType);
    }

    [Fact]
    public void MarksAcyclicTargetGraphCompatibleManagedClosureEligibleButKeepsNativeSplitDeferred()
    {
        BindingModel model = new(
        [
            Record("geometry-record", "Geometry_Type", OcctProductModule.Geometry),
            Record("modeling-record", "Modeling_Type", OcctProductModule.Modeling),
            Method("modeling-to-geometry", "Modeling_Type::Geometry", OcctProductModule.Modeling, Handle("Geometry_Type")),
        ]);
        GeneratedDependencyClosureReport report = Create(model, ["modeling-to-geometry"]);

        Assert.True(report.IsComplete);
        Assert.True(report.ManagedProjectSplitReady);
        Assert.False(report.NativeDllSplitReady);
        Assert.Equal("ManagedSplitEligibleNativeSplitDeferred", report.RecommendedDecision);
        Assert.Empty(report.CyclicGroups);
        Assert.Empty(report.Issues);
    }

    private static GeneratedDependencyClosureReport Create(BindingModel model, IReadOnlyList<string> emittedIds) =>
        GeneratedDependencyClosureAnalyzer.Create(
            "test",
            "1.3",
            model,
            new GeneratedBindingSet("test", emittedIds, []));

    private static BindingDeclaration Record(string id, string name, OcctProductModule module) =>
        new(id, name, BindingDeclarationKind.Record, name + ".hxx", 1, 1)
        {
            SourcePackage = name.Split('_')[0],
            ProductModule = module,
        };

    private static BindingDeclaration Method(
        string id,
        string name,
        OcctProductModule module,
        BindingType returnType) =>
        new(id, name, BindingDeclarationKind.Method, name.Split(':')[0] + ".hxx", 2, 1)
        {
            SourcePackage = name.Split('_')[0],
            ProductModule = module,
            Access = BindingAccess.Public,
            ReturnType = returnType,
        };

    private static BindingType Handle(string target) => new(
        $"opencascade::handle<{target}>",
        $"opencascade::handle<{target}>",
        $"opencascade::handle<{target}>",
        $"opencascade::handle<{target}>",
        [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
        "opencascade::handle",
        [new BindingTemplateArgument("Type", target)],
        true,
        target);
}
