using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchVIntegrationTests
{
    [Fact]
    public void SeparateRegionExchangeAndRealHwndCellInterfaceVoidReview() => Validation.BatchVRegionWorkflow.Run();
    [Fact]
    public void BoundedVoidsRespectUserEnvelopeAndExcludeOutsideOccupiedMaterial()
    {
        using var envelope = ShapeFactory.CreateBox(10, 10, 10); using var occupied = ShapeFactory.CreateBox(5, 10, 10);
        using var outside = occupied.Transformed(ShapeTransform.CreateTranslation(50, 0, 0));
        using var plan = BoundedVoidPlan.Create(envelope, [occupied, outside]); using var result = plan.Build();
        using var voids = result.CopyOutput("voids"); Assert.Equal(500, Mass(voids), 6);
        Assert.All(result.GetAssignments("voids"), a => Assert.Equal(RegionMembership.Inside, result.Cells[a.Cell.Index].InputMembership[0]));
        Assert.Equal(5, voids.GetBoundingBox().Minimum.X, 5); Assert.Equal(10, voids.GetBoundingBox().Maximum.X, 5);
    }
    [Fact]
    public void NestedFaceSetsProduceActualCavityShellClassificationAndPointOutcomes()
    {
        using var outer = ShapeFactory.CreateBox(10, 10, 10); using var inner0 = ShapeFactory.CreateBox(2, 2, 2);
        using var inner = inner0.Transformed(ShapeTransform.CreateTranslation(4, 4, 4));
        using var hollow = outer.Cut(inner); using var plan = VolumeConstructionPlan.Create([hollow]); using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.HelperBoxExcluded);
        Assert.Contains(result.Shells, s => s.Role == VolumeShellRole.Cavity && s.IsClosed);
        Assert.Contains(result.Shells, s => s.Role == VolumeShellRole.Exterior && s.IsClosed);
        Assert.Contains(result.ClassifyPoint(new(1, 1, 1)), h => h.State == VolumePointState.Inside);
        Assert.All(result.ClassifyPoint(new(20, 20, 20)), h => Assert.Equal(VolumePointState.Outside, h.State));
        Assert.Contains(result.ClassifyPoint(new(0, 1, 1)), h => h.State == VolumePointState.OnBoundary);
        Assert.All(result.ShellCandidates, s => Assert.True(s.IsValid));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IntersectingFaceVolumesHaveSourceCorrespondenceAndIndependentContainers(bool avoidInternal)
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = a.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
        using var plan = VolumeConstructionPlan.Create([a, b], new(AvoidInternalShapes: avoidInternal)); using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.Equal(3, result.Volumes.Count);
        Assert.All(result.Volumes, v => Assert.Equal(500, v.Volume, 6)); Assert.True(result.HelperBoxExcluded);
        Assert.All(result.Volumes, v => Assert.Contains(result.SourceFaces, f => f.Volume == v.Id));
        using var containers = result.CopyAdjacencyContainers(); Assert.Equal(1500, Mass(containers), 6);
        Assert.Equal(1, containers.GetTopologySummary().UniqueCounts.CompSolidCount);
        result.Dispose(); Assert.True(containers.IsValid);
    }
    [Fact]
    public void ExactRegionMeshGroupsPreserveSharedInterfaceIdsAndMaterialKeys()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = a.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
        using var plan = PartitionPlan.Create([a, b]); using var result = plan.Build([new("all", [new(RegionExpression.All, 7)])]);
        var mesh = RegionMeshing.Create(result, "all"); Assert.NotEmpty(mesh.Mesh.Triangles);
        Assert.All(mesh.Groups, g => { Assert.NotNull(g.Boundary); Assert.Equal(result.Revision, g.Cell.Revision); Assert.Equal(7, g.Material); });
        Assert.Contains(mesh.Groups.GroupBy(g => g.Boundary), g => g.Select(x => x.Cell).Distinct().Count() == 2);
        Assert.All(mesh.Mesh.Triangles, t => Assert.Contains(mesh.Groups, g => g.Group == t.Group));
        result.Dispose(); Assert.NotEmpty(mesh.Mesh.Positions);
    }
    [Theory]
    [InlineData(RegionAssemblyRulePolicy.PerOccurrence)]
    [InlineData(RegionAssemblyRulePolicy.SharedDefinitionRules)]
    public void LocatedInstancesRemainDistinctAndPlacementRefreshInvalidatesOldCells(RegionAssemblyRulePolicy policy)
    {
        using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(10, 10, 10);
        var (root, first, second) = Assembly(document, box);
        using var capture = AssemblyPartitionPlan.Capture(document, root, [[first.Entry], [second.Entry]], policy);
        Assert.Equal(2, capture.Inputs.Count);
        Assert.Equal(policy == RegionAssemblyRulePolicy.PerOccurrence ? 2 : 1, capture.Inputs.Select(i => i.RuleKey).Distinct().Count());
        var expression = capture.ExpressionFor(capture.Inputs[0].RuleKey);
        using var before = capture.Build([new("region", [new(expression)])]);
        using (var shape = before.CopyOutput("region")) Assert.Equal(policy == RegionAssemblyRulePolicy.PerOccurrence ? 1000 : 1500, Mass(shape), 5);
        using (var tx = document.BeginTransaction("move occurrence"))
        {
            using var transform = ShapeTransform.CreateTranslation(20, 0, 0).ToGpTrsf(); using var location = TopLocLocation.FromTransform(transform);
            document.RelocateOccurrence(second, location); tx.Commit();
        }
        Assert.False(capture.IsCurrent());
        Assert.Throws<InvalidOperationException>(() => capture.Publish(document, before, [new("stale", "region")]));
        using var refreshed = capture.Refresh(); using var after = refreshed.Build([new("all", [new(RegionExpression.All)])]);
        Assert.Equal(2, after.Cells.Count); Assert.Throws<ArgumentException>(() => after.CopyCell(before.Cells[0].Id));
        Assert.Equal(2, refreshed.Inputs.Count);
    }
    [Fact]
    public void MultiRegionXdeProductsKeepExplicitKeysColorsProvenanceAndUndo()
    {
        using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(10, 10, 10);
        var (root, first, second) = Assembly(document, box);
        using var capture = AssemblyPartitionPlan.Capture(document, root, [[first.Entry], [second.Entry]]);
        using var result = capture.Build([new("left", [new(RegionExpression.Input(0).Except(RegionExpression.Input(1)), 4)]),
            new("right", [new(RegionExpression.Input(1), 5)])]);
        var published = capture.Publish(document, result, [new("left-part", "left", new(.9, .1, .2)), new("right-part", "right", new(.1, .2, .9))]);
        Assert.Equal(2, published.Products.Count);
        Assert.All(published.Products, p => { Assert.NotNull(p.Label.Color); Assert.NotNull(p.Label.Comment); Assert.NotEmpty(p.Metadata.Sources); });
        Assert.Equal(2, published.Root.GetComponents().Count);
        Assert.True(document.Undo()); Assert.True(document.Redo());
    }
    [Fact]
    public void RepairVolumeWorkflowConsumesQAcceptanceOnlyAfterSolidSuccess()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6); using var source = RepairSnapshot.Create(box);
        using var preview = ShapeRepair.Preview(source, new(source, [new("sew", new SewingRepair())]));
        Assert.True(preview.CanAccept);
        using var result = RepairToVolume.Build([preview]); Assert.True(preview.IsAccepted);
        Assert.Single(result.Result.Volumes); Assert.Equal(120, result.Result.Volumes[0].Volume, 6);
        Assert.Equal(preview.Result!.Identity, result.Sources[0].RepairedSnapshot); Assert.Equal(0, result.Sources[0].VolumeInput);
    }
    [Fact]
    public void ParametricRegionOutputsPublishAtomicallyAndFailedEditPreservesEveryGeneration()
    {
        using var document = ParametricDocument.Create(); var a = Box("a", 10); document.Add(a);
        var b = Derived("b", ParametricFeatureKind.Placement, a.Id, new Dictionary<string, ParametricParameter> { ["x"] = Length(5) }); document.Add(b);
        var recipe = new ParametricPartitionRecipe([new("union", [new(RegionExpression.All.CopyTokens(), 1)]),
            new("common", [new(RegionExpression.Input(0).Intersect(RegionExpression.Input(1)).CopyTokens())])]);
        var feature = new ParametricFeatureDefinition(Guid.NewGuid(), "regions", ParametricFeatureKind.Partition,
            new Dictionary<string, ParametricParameter>(), [new("a", a.Id, ParametricOutputKind.ExactShape), new("b", b.Id, ParametricOutputKind.ExactShape)], recipe.ToJson());
        document.Add(feature); Success(document.Recompute());
        Assert.Equal(new[] { "common", "union" }, document.GetRegionOutputKeys(feature.Id));
        using var union = document.GetRegionOutput(feature.Id, "union"); using var common = document.GetRegionOutput(feature.Id, "common");
        Assert.Equal(180, Mass(union.Shape!), 6); Assert.Equal(60, Mass(common.Shape!), 6); Assert.Equal(union.Revision, common.Revision);
        var invalid = new ParametricPartitionRecipe([new("union", [new(RegionExpression.All.CopyTokens(), 1), new(RegionExpression.All.CopyTokens(), 2)])]);
        var edit = new ParametricFeatureDefinition(feature.Id, feature.Name, feature.Kind, feature.Parameters, feature.Inputs, invalid.ToJson());
        Assert.False(document.EditAndRecompute(edit).Succeeded);
        using var oldUnion = document.GetRegionOutput(feature.Id, "union", true); using var oldCommon = document.GetRegionOutput(feature.Id, "common", true);
        Assert.Equal(union.Revision, oldUnion.Revision); Assert.Equal(common.Revision, oldCommon.Revision);
        Assert.Equal(180, Mass(oldUnion.Shape!), 6); Assert.Equal(60, Mass(oldCommon.Shape!), 6);
    }
    private static (XdeLabel Root, XdeLabel First, XdeLabel Second) Assembly(XdeDocument document, Shape box)
    {
        using var tx = document.BeginTransaction("region fixture"); var definition = document.AddShape(box, "shared part");
        var root = document.AddAssembly("region fixture"); using var identity = TopLocLocation.Identity;
        var first = document.AddComponent(root, definition, identity);
        using var transform = ShapeTransform.CreateTranslation(5, 0, 0).ToGpTrsf(); using var location = TopLocLocation.FromTransform(transform);
        var second = document.AddComponent(root, definition, location); tx.Commit(); return (root, first, second);
    }
}
