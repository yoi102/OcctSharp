using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchTPersistenceTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTAuthoringTests
{
    [Fact]
    public void RecomputedSharedDefinitionsExchangeAndRealHwndReview() => OcctSharp.Validation.BatchTParametricWorkflow.Run();
    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false)]
    [InlineData(DocumentStorageFormat.XmlOcaf, false)]
    [InlineData(DocumentStorageFormat.BinXcaf, true)]
    [InlineData(DocumentStorageFormat.XmlXcaf, true)]
    public void PersistedGuidedLawAndConstrainedFillRecipesReexecuteRealKernels(DocumentStorageFormat format, bool xde)
    {
        using var doc = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create(); using var wire = BatchSAuthoringTests.Square(); using var spine = BatchSAuthoringTests.Spine();
        var w = Source(doc, "profile", wire); var s = Source(doc, "spine", spine);
        var sweep = new ParametricFeatureDefinition(Guid.NewGuid(), "sweep", ParametricFeatureKind.GuidedSweep,
            new Dictionary<string, ParametricParameter>(), [new("profile", w.Id, ParametricOutputKind.ExactShape), new("spine", s.Id, ParametricOutputKind.ExactShape)],
            new ParametricSweepRecipe("spine", [new("profile")], new() { SolidPolicy = SweepSolidPolicy.RequireSolid },
                ScaleLaw: new(ScalarLawKind.Linear, new(0, 1), 1, 2)).ToJson());
        doc.Add(sweep);
        Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        var inputs = new List<ParametricInput>();
        try
        {
            for (int i = 0; i < edges.Length; i++) inputs.Add(new("edge" + i, Source(doc, "edge" + i, edges[i]).Id, ParametricOutputKind.ExactShape));
        }
        finally { foreach (var edge in edges) edge.Dispose(); }
        var constraints = inputs.Select(x => new ParametricFillConstraint(x.Name, SurfaceConstraintKind.Edge, x.Name)).ToArray();
        var fill = new ParametricFeatureDefinition(Guid.NewGuid(), "fill", ParametricFeatureKind.ConstrainedFill,
            new Dictionary<string, ParametricParameter>(), inputs, new ParametricFillRecipe(constraints).ToJson());
        doc.Add(fill); Success(doc.Recompute());
        using (var swept = doc.GetResult(sweep.Id)) Assert.Equal(280.0 / 3, Mass(swept.Shape!), 3);
        using (var filled = doc.GetResult(fill.Id)) Assert.Equal(4, filled.Shape!.InspectProperties(InspectionPropertyKind.Area).Mass, 5);
        VerifyReopened(doc, format, xde, reopened =>
        {
            Success(reopened.Recompute(ParametricRecomputeMode.Full));
            using var swept = reopened.GetResult(sweep.Id); Assert.Equal(280.0 / 3, Mass(swept.Shape!), 3);
            using var filled = reopened.GetResult(fill.Id); Assert.Equal(4, filled.Shape!.InspectProperties(InspectionPropertyKind.Area).Mass, 5);
            Assert.NotEmpty(reopened.GetDiagnostics(fill.Id));
            var history = reopened.GetAlgorithmHistory(sweep.Id);
            try
            {
                Assert.NotEmpty(history); Assert.Contains(history, x => x.SourceFeatureId == w.Id && x.Shape is not null);
                Assert.All(history, x => Assert.False(x.HasExactSourceSubshape));
                reopened.Dispose(); Assert.Contains(history, x => x.Shape?.IsValid == true);
            }
            finally { foreach (var item in history) item.Dispose(); }
        });
        var bad = new ParametricFeatureDefinition(fill.Id, fill.Name, fill.Kind, fill.Parameters, fill.Inputs,
            new ParametricFillRecipe([.. constraints, new("conflict", SurfaceConstraintKind.Point, Point: new(0, 0, 10))],
                new() { MaximumSegments = 1, MaximumDegree = 2, Iterations = 1 }).ToJson());
        doc.Update(bad); Assert.False(doc.Recompute().Succeeded);
        using var last = doc.GetResult(fill.Id, true); Assert.True(last.IsStale);
        Assert.Equal(4, last.Shape!.InspectProperties(InspectionPropertyKind.Area).Mass, 5);
    }

    [Fact]
    public void ExtrusionAndRevolutionUseTypedParametersAndReferencedProfiles()
    {
        using var doc = ParametricDocument.Create(); using var wire = BatchSAuthoringTests.Square();
        using var face = ShapeFactory.CreatePlanarFace(wire); var source = Source(doc, "face", face);
        var extrusion = Derived("extrusion", ParametricFeatureKind.Extrusion, source.Id,
            new Dictionary<string, ParametricParameter> { ["z"] = Length(5) }, slot: "profile"); doc.Add(extrusion);
        using var section = ShapeFactory.CreatePolygonWire([new(1, 0, 0), new(2, 0, 0), new(2, 0, 3), new(1, 0, 3)], true);
        using var profile = ShapeFactory.CreatePlanarFace(section); var profileId = Source(doc, "revolution profile", profile).Id;
        var revolution = Derived("revolution", ParametricFeatureKind.Revolution, profileId,
            new Dictionary<string, ParametricParameter> { ["angle"] = ParametricParameter.FromValue(ParametricValue.FromReal(360, ParametricUnit.Degree)) }, slot: "profile");
        doc.Add(revolution); Success(doc.Recompute());
        using var a = doc.GetResult(extrusion.Id); Assert.Equal(20, Mass(a.Shape!), 6);
        using var b = doc.GetResult(revolution.Id); Assert.Equal(Math.PI * 9, Mass(b.Shape!), 5);
    }
}
