namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTRecomputeTests
{
    internal static ParametricParameter Length(double value) => ParametricParameter.FromValue(ParametricValue.FromReal(value, ParametricUnit.Millimeter));
    internal static ParametricFeatureDefinition Box(string name, double x = 2) => new(Guid.NewGuid(), name, ParametricFeatureKind.Box,
        new Dictionary<string, ParametricParameter> { ["x"] = Length(x), ["y"] = Length(3), ["z"] = Length(4) }, []);
    internal static ParametricFeatureDefinition Derived(string name, ParametricFeatureKind kind, Guid input,
        IReadOnlyDictionary<string, ParametricParameter>? parameters = null, string? recipe = null, string slot = "source") =>
        new(Guid.NewGuid(), name, kind, parameters ?? new Dictionary<string, ParametricParameter>(), [new(slot, input, ParametricOutputKind.ExactShape)], recipe);
    internal static void Success(ParametricRecomputeReport report) => Assert.True(report.Succeeded, string.Join("; ", report.Issues.Select(x => x.Message)));
    private static double Volume(ParametricDocument doc, Guid id)
    {
        using var result = doc.GetResult(id);
        return Mass(result.Shape!);
    }
    internal static double Mass(Shape shape) { using var properties = GPropProperties.FromShape(shape); return properties.Mass; }

    [Fact]
    public void IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo()
    {
        using var doc = ParametricDocument.Create();
        var box = Box("box"); var independent = Box("independent", 5);
        doc.Add(box); doc.Add(independent);
        var moved = Derived("moved", ParametricFeatureKind.Placement, box.Id,
            new Dictionary<string, ParametricParameter> { ["x"] = Length(10) });
        doc.Add(moved);
        Success(doc.Recompute());
        Assert.Equal(24, Volume(doc, moved.Id), 6);
        Guid untouched = doc.Features.Single(x => x.Definition.Id == independent.Id).ResultRevision!.Value;
        var edit = box.WithParameter("x", Length(4));
        Success(doc.EditAndRecompute(edit));
        Assert.Equal(48, Volume(doc, moved.Id), 6);
        Assert.True(doc.GetLogbook(box.Id).Touched); Assert.True(doc.GetLogbook(box.Id).Valid);
        Assert.True(doc.GetLogbook(moved.Id).Impacted); Assert.True(doc.GetLogbook(moved.Id).Valid);
        Assert.False(doc.GetLogbook(moved.Id).Touched); Assert.False(doc.GetLogbook(independent.Id).Touched);
        Assert.Equal(untouched, doc.Features.Single(x => x.Definition.Id == independent.Id).ResultRevision);
        Assert.True(doc.Undo()); Assert.Equal(24, Volume(doc, moved.Id), 6);
        Assert.Equal(2, doc.ReadParameter(box.Id, "x").Real);
        Assert.True(doc.Redo()); Assert.Equal(48, Volume(doc, moved.Id), 6);
        doc.Update(edit.WithParameter("x", Length(6)));
        doc.Update(independent.WithParameter("x", Length(7)));
        Assert.False(doc.GetLogbook(box.Id).Valid); Assert.False(doc.GetLogbook(box.Id).Done);
        Assert.False(doc.GetLogbook(moved.Id).Valid);
        var targeted = doc.Recompute(ParametricRecomputeMode.Targeted, [moved.Id]);
        Success(targeted); Assert.Contains(independent.Id, targeted.Pending);
        Assert.Equal(72, Volume(doc, moved.Id), 6);
        Assert.Throws<InvalidOperationException>(() => doc.GetResult(independent.Id));
        using (var stale = doc.GetResult(independent.Id, true)) Assert.True(stale.IsStale);
        Success(doc.Recompute(ParametricRecomputeMode.Full));
        Assert.Equal(84, Volume(doc, independent.Id), 6);
        Assert.Empty(doc.Recompute().Executed);
    }

    [Fact]
    public void FailedMiddleNodeKeepsAllLastGoodResultsAndBlocksDependants()
    {
        using var doc = ParametricDocument.Create();
        var box = Box("box"); doc.Add(box);
        var moved = Derived("moved", ParametricFeatureKind.Placement, box.Id); doc.Add(moved);
        Success(doc.Recompute());
        Guid oldRevision = doc.Features.Single(x => x.Definition.Id == box.Id).ResultRevision!.Value;
        doc.Update(box.WithParameter("x", Length(-1)));
        var failed = doc.Recompute(); Assert.False(failed.Succeeded);
        Assert.Equal(oldRevision, doc.Features.Single(x => x.Definition.Id == box.Id).ResultRevision);
        Assert.Equal(ParametricExecutionState.Failed, doc.Features.Single(x => x.Definition.Id == box.Id).State);
        Assert.Equal(ParametricExecutionState.Blocked, doc.Features.Single(x => x.Definition.Id == moved.Id).State);
        using var previous = doc.GetResult(moved.Id, true);
        Assert.True(previous.IsStale); Assert.Equal(24, Mass(previous.Shape!), 6);
        Assert.False(doc.GetLogbook(box.Id).Valid); Assert.True(doc.GetLogbook(box.Id).Touched);
        doc.Update(box); Success(doc.Recompute());
        Assert.True(doc.GetLogbook(box.Id).Valid); Assert.True(doc.GetLogbook(box.Id).Done);
    }

    [Fact]
    public void CancellationAndFailedCombinedEditDoNotPublishCandidates()
    {
        using var doc = ParametricDocument.Create(); var box = Box("box"); doc.Add(box); Success(doc.Recompute());
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        var cancelled = doc.EditAndRecompute(box.WithParameter("x", Length(8)), cancel.Token);
        Assert.True(cancelled.Cancelled); Assert.Equal(2, doc.ReadParameter(box.Id, "x").Real);
        var failed = doc.EditAndRecompute(box.WithParameter("x", Length(-2)));
        Assert.False(failed.Succeeded); Assert.Equal(2, doc.ReadParameter(box.Id, "x").Real);
        using var last = doc.GetResult(box.Id, true); Assert.Equal(24, Mass(last.Shape!), 6);
    }

    [Fact]
    public void PrimitiveExpressionUnitsScalarAndDisposedDocumentAreEnforced()
    {
        var doc = ParametricDocument.Create();
        var cylinder = new ParametricFeatureDefinition(Guid.NewGuid(), "cylinder", ParametricFeatureKind.Cylinder,
            new Dictionary<string, ParametricParameter> { ["radius"] = Length(2), ["height"] = Length(5) }, []);
        doc.Add(cylinder);
        var scalar = new ParametricFeatureDefinition(Guid.NewGuid(), "scalar", ParametricFeatureKind.Scalar,
            new Dictionary<string, ParametricParameter> { ["value"] = ParametricParameter.FromExpression(ParametricExpression.Parameter(cylinder.Id, "height")) }, []);
        doc.Add(scalar); Success(doc.Recompute());
        Assert.Equal(Math.PI * 20, Volume(doc, cylinder.Id), 5);
        using var output = doc.GetResult(scalar.Id); Assert.Equal(5, output.Scalar!.Value.Value);
        using var owning = doc.GetResult(cylinder.Id);
        doc.Update(cylinder.WithParameter("radius", ParametricParameter.FromValue(ParametricValue.FromReal(2, ParametricUnit.Degree))));
        Assert.False(doc.Recompute().Succeeded);
        doc.Dispose(); Assert.Throws<ObjectDisposedException>(() => doc.GetResult(cylinder.Id));
        Assert.Equal(Math.PI * 20, Mass(owning.Shape!), 5);
    }

    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false)]
    [InlineData(DocumentStorageFormat.XmlOcaf, false)]
    [InlineData(DocumentStorageFormat.BinXcaf, true)]
    [InlineData(DocumentStorageFormat.XmlXcaf, true)]
    public void TypedBooleanRepairAndMeshRecipesExecuteWithoutChangingExactInputs(DocumentStorageFormat format, bool xde)
    {
        using var doc = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create(); var a = Box("a"); var b = Box("b", 1);
        doc.Add(a); doc.Add(b);
        var boolean = new ParametricFeatureDefinition(Guid.NewGuid(), "cut", ParametricFeatureKind.Boolean,
            new Dictionary<string, ParametricParameter>(), [new("a", a.Id, ParametricOutputKind.ExactShape), new("b", b.Id, ParametricOutputKind.ExactShape)],
            new ParametricBooleanRecipe(FeatureBooleanOperation.Cut).ToJson());
        doc.Add(boolean);
        var repair = Derived("repair", ParametricFeatureKind.Repair, boolean.Id,
            recipe: new ParametricRepairRecipe([new SameDomainUnificationRepair()]).ToJson()); doc.Add(repair);
        var mesh = Derived("mesh", ParametricFeatureKind.Mesh, repair.Id, recipe: new ParametricMeshRecipe(RecomputeNormals: true).ToJson()); doc.Add(mesh);
        Success(doc.Recompute());
        Assert.Equal(12, Volume(doc, boolean.Id), 6); Assert.Equal(12, Volume(doc, repair.Id), 6); Assert.Equal(24, Volume(doc, a.Id), 6);
        using var discrete = doc.GetResult(mesh.Id); Assert.Equal(ParametricOutputKind.Mesh, discrete.Kind);
        Assert.False(MeshTopology.IsSurfaceBacked(discrete.Shape!));
        Assert.True(MeshTopology.SnapshotExisting(discrete.Shape!).Mesh.Triangles.Count >= 12);
        Assert.Throws<ArgumentException>(() => doc.Add(Derived("invalid", ParametricFeatureKind.Placement, mesh.Id)));
        BatchTPersistenceTests.VerifyReopened(doc, format, xde, reopened =>
        {
            Success(reopened.Recompute(ParametricRecomputeMode.Full));
            Assert.Equal(12, Volume(reopened, repair.Id), 6); Assert.NotEmpty(reopened.GetDiagnostics(repair.Id));
            using var output = reopened.GetResult(mesh.Id); Assert.False(MeshTopology.IsSurfaceBacked(output.Shape!));
            Assert.True(MeshTopology.SnapshotExisting(output.Shape!).Mesh.Triangles.Count >= 12);
            var history = reopened.GetAlgorithmHistory(boolean.Id);
            try
            {
                Assert.NotEmpty(history); Assert.All(history, x => Assert.Contains(x.SourceFeatureId, new[] { a.Id, b.Id }));
                Assert.All(history, x => Assert.False(x.HasExactSourceSubshape));
                reopened.Dispose(); Assert.Contains(history, x => x.Shape?.IsValid == true);
            }
            finally { foreach (var item in history) item.Dispose(); }
        });
    }
}
