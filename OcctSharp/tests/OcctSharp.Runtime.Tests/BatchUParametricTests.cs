using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchUFinishingTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchUParametricTests
{
    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false)]
    [InlineData(DocumentStorageFormat.XmlOcaf, false)]
    [InlineData(DocumentStorageFormat.BinXcaf, true)]
    [InlineData(DocumentStorageFormat.XmlXcaf, true)]
    public void LimitedRecipeReopenRebuildsWithExactSupportSharing(DocumentStorageFormat format, bool xde)
    {
        using var doc = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create();
        using var basis = ShapeFactory.CreateBox(10, 10, 5); using var profile = BatchULocalFeatureTests.FaceAt(5);
        using var stop = BatchULocalFeatureTests.FaceAt(8, -10, -10, 20, 20);
        Guid Add(string name, Shape shape)
        { var feature = new ParametricFeatureDefinition(Guid.NewGuid(), name, ParametricFeatureKind.SourceShape, new Dictionary<string, ParametricParameter>(), []); return doc.Add(feature, shape); }
        var baseId = Add("base", basis); var profileId = Add("profile", profile); var stopId = Add("stop", stop); Success(doc.Recompute());
        using var current = doc.GetResult(baseId); using var source = RepairSnapshot.Create(current.Shape!);
        var top = BatchUProgramTests.Select(source, ShapeKind.Face, b => b.Minimum.Z > 4.99);
        var recipe = new ParametricLimitedFeatureRecipe("base", "profile", ParametricLocalSelection.Bind("base", source, top),
            new() { Kind = LimitedFeatureKind.Prism, Limit = LocalFeatureLimit.Until }, Until: "stop");
        var feature = new ParametricFeatureDefinition(Guid.NewGuid(), "limited", ParametricFeatureKind.LimitedFeature, new Dictionary<string, ParametricParameter>(),
            [new("base", baseId, ParametricOutputKind.ExactShape), new("profile", profileId, ParametricOutputKind.ExactShape), new("stop", stopId, ParametricOutputKind.ExactShape)], recipe.ToJson());
        doc.Add(feature); Success(doc.Recompute()); using var initial = doc.GetResult(feature.Id); Assert.Equal(512, Mass(initial.Shape!), 5);
        BatchTPersistenceTests.VerifyReopened(doc, format, xde, reopened =>
        {
            Success(reopened.Recompute(ParametricRecomputeMode.Full)); using var rebuilt = reopened.GetResult(feature.Id);
            Assert.Equal(512, Mass(rebuilt.Shape!), 5); Assert.True(rebuilt.Shape!.IsValid);
            var unreachable = recipe with { Support = recipe.Support with { Index = int.MaxValue } };
            reopened.Update(new(feature.Id, feature.Name, feature.Kind, feature.Parameters, feature.Inputs, unreachable.ToJson()));
            Assert.False(reopened.Recompute().Succeeded); using var last = reopened.GetResult(feature.Id, true);
            Assert.True(last.IsStale); Assert.Equal(512, Mass(last.Shape!), 5);
        });
    }

    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false)]
    [InlineData(DocumentStorageFormat.XmlOcaf, false)]
    [InlineData(DocumentStorageFormat.BinXcaf, true)]
    [InlineData(DocumentStorageFormat.XmlXcaf, true)]
    public void PersistedContourRecipeReexecutesAndFailedParametersKeepLastGood(DocumentStorageFormat format, bool xde)
    {
        using var doc = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create();
        var box = Box("base", 10); doc.Add(box); Success(doc.Recompute());
        using var baseResult = doc.GetResult(box.Id); using var source = RepairSnapshot.Create(baseResult.Shape!);
        var selector = ParametricLocalSelection.Bind("source", source, Edge(source));
        var recipe = new ParametricFilletRecipe("source", [new(selector, RadiusParameter: "radius")]);
        var fillet = Derived("fillet", ParametricFeatureKind.ContourFillet, box.Id,
            new Dictionary<string, ParametricParameter> { ["radius"] = Length(.2) }, recipe.ToJson());
        doc.Add(fillet); Success(doc.Recompute());
        using var initial = doc.GetResult(fillet.Id); double initialMass = Mass(initial.Shape!);
        var larger = fillet.WithParameter("radius", Length(.4)); Success(doc.EditAndRecompute(larger));
        using var edited = doc.GetResult(fillet.Id); double changedMass = Mass(edited.Shape!); Assert.True(changedMass < initialMass);
        doc.Update(larger.WithParameter("radius", Length(100))); Assert.False(doc.Recompute().Succeeded);
        Assert.Throws<InvalidOperationException>(() => doc.GetResult(fillet.Id));
        using var stale = doc.GetResult(fillet.Id, true); Assert.True(stale.IsStale); Assert.Equal(changedMass, Mass(stale.Shape!), 6);
        doc.Update(larger); Success(doc.Recompute());
        BatchTPersistenceTests.VerifyReopened(doc, format, xde, reopened =>
        {
            Success(reopened.Recompute(ParametricRecomputeMode.Full));
            using var rebuilt = reopened.GetResult(fillet.Id); Assert.Equal(changedMass, Mass(rebuilt.Shape!), 6);
            reopened.Update(box.WithParameter("x", Length(11)));
            var failure = reopened.Recompute(); Assert.False(failure.Succeeded);
            Assert.Contains(failure.Issues, i => i.Message.Contains("rebind", StringComparison.OrdinalIgnoreCase));
            using var previous = reopened.GetResult(fillet.Id, true); Assert.True(previous.IsStale);
            Assert.Equal(changedMass, Mass(previous.Shape!), 6);
        });
    }

    [Theory]
    [InlineData(ParametricFeatureKind.ContourChamfer)]
    [InlineData(ParametricFeatureKind.FaceDraft)]
    public void AdditionalFinishingRecipesExecuteThroughTheFacade(ParametricFeatureKind kind)
    {
        using var doc = ParametricDocument.Create(); var box = Box("base", 10); doc.Add(box); Success(doc.Recompute());
        using var basis = doc.GetResult(box.Id); using var source = RepairSnapshot.Create(basis.Shape!);
        var face = BatchUProgramTests.Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var edge = BatchUProgramTests.Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.X) < 1e-5 && Math.Abs(b.Maximum.Y) < 1e-5);
        var support = ParametricLocalSelection.Bind("source", source, face);
        ParametricRecipe recipe = kind == ParametricFeatureKind.ContourChamfer
            ? new ParametricChamferRecipe("source", [new(ParametricLocalSelection.Bind("source", source, edge), support, ChamferDimensions.DistanceAngle, .2, .4)])
            : new ParametricDraftRecipe("source", [new(support, .08, new(0, 0, 1), new(0, 0, 0), new(0, 0, 1))]);
        var feature = Derived("local", kind, box.Id, recipe: recipe.ToJson()); doc.Add(feature); Success(doc.Recompute());
        using var output = doc.GetResult(feature.Id); Assert.True(output.Shape!.IsValid); Assert.NotEqual(Mass(basis.Shape!), Mass(output.Shape));
    }
}
