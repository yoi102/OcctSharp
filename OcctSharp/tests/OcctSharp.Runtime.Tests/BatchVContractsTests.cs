using OcctSharp.Interop;
using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchVContractsTests
{
    [Fact]
    public void CopiedProgramsStayBoundedAndDoNotMutateSourceGeometryOnSuccessOrFailure()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6); string fingerprint = RepairSnapshot.ComputeFingerprint(box);
        using var plan = PartitionPlan.Create([box]);
        int[] tokens = [-1]; var expression = RegionExpression.FromTokens(tokens); tokens[0] = -2;
        RegionRule[] rules = [new(expression)]; var program = new RegionProgram("one", rules); rules[0] = new(RegionExpression.None);
        using var result = plan.Build([program]); Assert.Single(result.Cells); Assert.Single(result.GetAssignments("one"));
        Assert.Single(result.Conservation); Assert.Equal(0, result.Conservation[0].AbsoluteError, 6);
        using var copied = result.CopyOutput("one"); Assert.Equal(120, Mass(copied), 6);
        RegionRule[] many = Enumerable.Repeat(new RegionRule(RegionExpression.All), 4096).ToArray();
        Assert.Throws<ArgumentException>(() => plan.Build([new("first", many), new("second", many)]));
        var longExpression = RegionExpression.All;
        for (int i = 0; i < 10; i++) longExpression = longExpression.Union(longExpression);
        Assert.Throws<ArgumentException>(() => plan.Build([new("bounded", Enumerable.Repeat(new RegionRule(longExpression), 100).ToArray())]));
        Assert.Equal(fingerprint, RepairSnapshot.ComputeFingerprint(box));
        using var volumePlan = VolumeConstructionPlan.Create([box]); using var volume = volumePlan.Build();
        Assert.Single(volume.Volumes); Assert.Equal(fingerprint, RepairSnapshot.ComputeFingerprint(box));
    }

    [Fact]
    public void EscapedRegionManifestRejectsForeignNamedShapeAndUndoRestoresIt()
    {
        using var owner = OcafDocument.Create(); using var document = ParametricDocument.Attach(owner);
        var box = Box("source", 10); document.Add(box);
        var feature = Derived("regions", ParametricFeatureKind.Partition, box.Id,
            recipe: new ParametricPartitionRecipe([new("one", [new(RegionExpression.All.CopyTokens())])]).ToJson());
        document.Add(feature); Success(document.Recompute()); using var original = document.GetRegionOutput(feature.Id, "one");
        var storage = new ParametricStorage(owner.Handle);
        string entry = document.Features.Single(f => f.Definition.Id == feature.Id).Entry;
        var stored = System.Text.Json.Nodes.JsonNode.Parse(storage.GetText(entry, "feature")!)!;
        string resultEntry = stored["ResultEntry"]!.GetValue<string>();
        using (var command = owner.BeginTransaction())
        {
            storage.SetText(resultEntry, "regionOutputs", "{\"one\":\"0:1\"}"); command.Commit();
        }
        Assert.Throws<InvalidDataException>(() => document.GetRegionOutput(feature.Id, "one"));
        Assert.True(owner.Undo()); using var restored = document.GetRegionOutput(feature.Id, "one");
        Assert.Equal(original.Revision, restored.Revision); Assert.Equal(120, Mass(restored.Shape!), 6);
    }

    [Fact]
    public void PointSelectionOwnsOnlyMatchingVolumesWithExplicitBoundaryAndForeignIdRejection()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6);
        using var other = box.Transformed(ShapeTransform.CreateTranslation(10, 0, 0));
        using var plan = VolumeConstructionPlan.Create([box, other]); using var result = plan.Build();
        using var selected = result.SelectPoint(new(1, 1, 1), VolumeBoundaryPolicy.Reject);
        Assert.Single(selected.Selected); Assert.Equal(2, selected.Hits.Count); Assert.Equal(120, Mass(selected.Shape), 6);
        using var excluded = result.SelectPoint(new(0, 1, 1), VolumeBoundaryPolicy.Exclude); Assert.Empty(excluded.Selected);
        using var included = result.SelectPoint(new(0, 1, 1), VolumeBoundaryPolicy.Include); Assert.Single(included.Selected);
        Assert.Throws<InvalidOperationException>(() => result.SelectPoint(new(0, 1, 1), VolumeBoundaryPolicy.Reject));
        Assert.Throws<ArgumentOutOfRangeException>(() => result.SelectPoint(new(1, 1, 1), (VolumeBoundaryPolicy)99));
        Assert.Throws<ArgumentException>(() => result.CopyVolume(new(Guid.NewGuid(), 0)));
        Assert.Throws<ArgumentException>(() => result.CopyVolume(new(result.Revision, -1)));
        result.Dispose(); Assert.True(selected.Shape.IsValid); Assert.Equal(120, Mass(selected.Shape), 6);
        Assert.Throws<ObjectDisposedException>(() => result.ClassifyPoint(new(1, 1, 1)));
    }

    [Fact]
    public void ProductBinXcafReopenRetainsRegionKeysAssignmentsAndSourceMetadata()
    {
        string folder = Path.Combine(Path.GetTempPath(), "OcctSharp-BatchV-metadata-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(4, 5, 6);
            using var other = box.Transformed(ShapeTransform.CreateTranslation(2, 0, 0));
            using var plan = PartitionPlan.Create([box, other]); using var result = plan.Build([new("regions", [new(RegionExpression.All, 17)])]);
            RegionAssemblyInput[] sources = [new(0, "occurrence-a", "definition-a", "rule-a", "source", null, "fingerprint")];
            var products = RegionProducts.Create(document, result, [new("part-key", "regions", new(.2, .4, .8))], sources: sources);
            var original = products.Products[0]; string path = document.Save(Path.Combine(folder, "regions.xbf"));
            using var reopened = XdeDocument.Open(path); var restored = reopened.GetLabel(original.Label.Entry);
            Assert.Equal(original.Label.Comment, restored.Comment); Assert.Equal("part-key", restored.Name); Assert.NotNull(restored.Color);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<RegionProductMetadata>(restored.Comment!)!;
            Assert.Equal(result.Revision, metadata.PartitionRevision); Assert.Equal("regions", metadata.OutputKey);
            Assert.Equal("part-key", metadata.PartKey); Assert.Equal(3, metadata.Cells.Count);
            Assert.All(metadata.Cells, c => Assert.Equal(17, c.Material));
            Assert.Equal("occurrence-a", Assert.Single(metadata.Sources).OccurrenceKey);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void CancellationAfterMultiOutputCandidateAndUndoNeverPublishPartialGenerations()
    {
        using var document = ParametricDocument.Create(); var box = Box("box", 10); document.Add(box);
        var recipe = new ParametricPartitionRecipe([new("one", [new(RegionExpression.All.CopyTokens())]),
            new("two", [new(RegionExpression.All.CopyTokens(), 2)])]);
        var feature = Derived("regions", ParametricFeatureKind.Partition, box.Id, recipe: recipe.ToJson()); document.Add(feature);
        Success(document.Recompute()); using var first = document.GetRegionOutput(feature.Id, "one");
        using var cancel = new CancellationTokenSource();
        var report = document.RecomputeCore(box.WithParameter("x", Length(20)), ParametricRecomputeMode.Full, null,
            cancel.Token, id => { if (id == feature.Id) cancel.Cancel(); });
        Assert.True(report.Cancelled); Assert.Contains(feature.Id, report.Executed);
        foreach (string key in new[] { "one", "two" })
        {
            using var retained = document.GetRegionOutput(feature.Id, key, true);
            Assert.Equal(first.Revision, retained.Revision); Assert.Equal(120, Mass(retained.Shape!), 6);
        }
        Success(document.EditAndRecompute(box.WithParameter("x", Length(20))));
        using var changed = document.GetRegionOutput(feature.Id, "two"); Assert.Equal(240, Mass(changed.Shape!), 6);
        Assert.True(document.Undo()); using var undone = document.GetRegionOutput(feature.Id, "two", true);
        Assert.Equal(first.Revision, undone.Revision); Assert.Equal(120, Mass(undone.Shape!), 6);
        Assert.True(document.Redo()); using var redone = document.GetRegionOutput(feature.Id, "one"); Assert.Equal(changed.Revision, redone.Revision);
        var mapping = document.Duplicate([box.Id, feature.Id]);
        Assert.Throws<InvalidOperationException>(() => document.GetRegionOutputKeys(mapping[feature.Id]));
        Success(document.Recompute()); using var copied = document.GetRegionOutput(mapping[feature.Id], "two");
        Assert.Equal(240, Mass(copied.Shape!), 6); Assert.NotEqual(changed.Revision, copied.Revision);
    }

    [Fact]
    public unsafe void RawRegionBuffersRejectMalformedRulesCapacitiesAndStaleOwnersWithoutWritingSentinels()
    {
        using var a = ShapeFactory.CreateBox(3, 3, 3); using var b = a.Transformed(ShapeTransform.CreateTranslation(1, 0, 0));
        PartitionOptionsRaw options = new() { CheckInputs = 1, MaxCells = 1000 };
        AuthoringBridge.WithInputs([a, b], (p, count) =>
        {
            Assert.Equal(NativeStatus.Success, NativeMethods.PartitionBuild(p, count, in options, null, 0, null, 0, null, 0, out nint raw));
            var owner = new FeatureResultHandle(raw);
            try
            {
                Assert.Equal(NativeStatus.Success, NativeMethods.RegionSnapshot(owner, out var info, null, 0));
                Assert.True(info.ItemCount > 1); RegionItemRaw sentinel = new() { Kind = 123456, A = 789 };
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RegionSnapshot(owner, out _, &sentinel, 1));
                Assert.Equal(123456, sentinel.Kind); Assert.Equal(789, sentinel.A);
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RegionSnapshot(owner, out _, null, -1));
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RegionItemShape(owner, -1, out nint absent)); Assert.Equal(0, absent);
                RegionRuleRaw bad = new() { Output = 0, Offset = 0, Count = 1, Dimension = -1, MaximumMeasure = -1 };
                RegionOutputRaw output = default; int expression = -5;
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.PartitionBuild(p, count, in options, &bad, 1, &expression, 1, &output, 1, out nint failed));
                Assert.Equal(0, failed);
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.PartitionBuild(null, count, in options, null, 0, null, 0, null, 0, out failed));
                Assert.Equal(0, failed);
                RegionOutputRaw* requiredOutputs = stackalloc RegionOutputRaw[2];
                requiredOutputs[0] = default; requiredOutputs[1] = default;
                RegionRuleRaw* requiredRules = stackalloc RegionRuleRaw[3];
                for (int i = 0; i < 3; i++) requiredRules[i] = new() { Output = i == 0 ? 0 : 1, Offset = 0, Count = 1,
                    Material = i, Dimension = -1, MaximumMeasure = -1 };
                expression = -1;
                // The first output succeeds; conflicting second-output rules must still publish no owner.
                Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.PartitionBuild(p, count, in options,
                    requiredRules, 3, &expression, 1, requiredOutputs, 2, out failed)); Assert.Equal(0, failed);
                using var recovered = PartitionPlan.Create([a, b]); using var accepted = recovered.Build([new("all", [new(RegionExpression.All)])]);
                using var copied = accepted.CopyOutput("all"); Assert.Equal(36, Mass(copied), 6);
            }
            finally { owner.Dispose(); }
            using var stale = new FeatureResultHandle(raw);
            Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.RegionSnapshot(stale, out _, null, 0));
            return 0;
        });
    }
    [Fact]
    public void SelfInterferingArgumentsExposeSourceFaultsAndNoSelectedOutput()
    {
        using var a = ShapeFactory.CreateBox(3, 3, 3); using var b = a.Transformed(ShapeTransform.CreateTranslation(1, 0, 0));
        using var invalid = ShapeFactory.CreateCompound([a, b]); using var other = ShapeFactory.CreateBox(10, 10, 10);
        using var plan = PartitionPlan.Create([invalid, other]); using var result = plan.Build([new("all", [new(RegionExpression.All)])]);
        Assert.False(result.Diagnostics.AlgorithmDone); Assert.Contains(0, result.Diagnostics.InvalidInputs); Assert.NotEmpty(result.Faults);
        Assert.Throws<InvalidOperationException>(() => result.CopyOutput("all"));
        using var fault = result.CopyFaultShape(result.Faults[0]); Assert.True(fault.FaceCount > 0);
        Assert.Contains(result.Faults, f => f.TopologyIndex.HasValue);
    }
    [Fact]
    public void FaceAndEdgePartitionsExposeCorrectDimensionalMeasuresAndSharedEdges()
    {
        using var wireA = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(10, 0, 0), new(10, 10, 0), new(0, 10, 0)], true);
        using var faceA = ShapeFactory.CreatePlanarFace(wireA); using var faceB = faceA.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
        using var plan = PartitionPlan.Create([faceA, faceB]); using var result = plan.Build([new("sheet", [new(RegionExpression.All)], makeContainers: true)]);
        Assert.Equal(3, result.Cells.Count); Assert.All(result.Cells, c => { Assert.Equal(2, c.Dimension); Assert.Equal(50, c.Measure, 6); });
        using var sheet = result.CopyOutput("sheet"); Assert.Equal(1, sheet.GetTopologySummary().UniqueCounts.ShellCount);
        Assert.Contains(result.Boundaries, b => b.Dimension == 1 && b.Uses.Select(u => u.Cell).Distinct().Count() == 2);
        using var edgeA = ShapeFactory.CreateEdge(new(0, 0, 0), new(10, 0, 0)); using var edgeB = ShapeFactory.CreateEdge(new(5, 0, 0), new(15, 0, 0));
        using var linePlan = PartitionPlan.Create([edgeA, edgeB]); using var lines = linePlan.Build([new("wire", [new(RegionExpression.All)], makeContainers: true)]);
        Assert.Equal(3, lines.Cells.Count); Assert.All(lines.Cells, c => Assert.Equal(5, c.Measure, 6));
        using var output = lines.CopyOutput("wire"); Assert.Equal(1, output.GetTopologySummary().UniqueCounts.WireCount);
    }
    [Fact]
    public void InternalEdgePolicyHasObservableInclusionAndExclusion()
    {
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var edge = ShapeFactory.CreateEdge(new(2, 2, 2), new(4, 2, 2));
        using var include = VolumeConstructionPlan.Create([box, edge]); using var exclude = VolumeConstructionPlan.Create([box, edge], new(AvoidInternalShapes: true));
        using var included = include.Build(); using var excluded = exclude.Build();
        Assert.Single(included.Volumes); Assert.Single(excluded.Volumes); Assert.Equal(1000, included.Volumes[0].Volume, 6);
        Assert.NotEmpty(included.InternalTopologyItems); Assert.Empty(excluded.InternalTopologyItems);
    }
    [Fact]
    public void ClosedShellCandidatesAndOutputCopiesSurviveInputAndResultDisposal()
    {
        var box = ShapeFactory.CreateBox(4, 5, 6); var faces = box.GetFaces();
        var plan = VolumeConstructionPlan.Create(faces, new(IntersectInputs: false));
        foreach (var face in faces) face.Dispose(); box.Dispose(); var result = plan.Build();
        Assert.Contains(result.ShellCandidates, s => s.IsClosed && s.IsValid);
        using var shape = result.CopyVolume(result.Volumes[0].Id); plan.Dispose(); result.Dispose();
        Assert.Equal(120, Mass(shape), 6); Assert.True(shape.IsValid);
    }
    [Fact]
    public void BoundedRepeatedRegionLifecyclesKeepIndependentSnapshotsAndRejectForeignHistory()
    {
        using var a = ShapeFactory.CreateBox(3, 3, 3); using var b = a.Transformed(ShapeTransform.CreateTranslation(1, 0, 0));
        using var plan = PartitionPlan.Create([a, b]); using var first = plan.Build([new("all", [new(RegionExpression.All)])]);
        var history = first.History.First(h => h.Kind == RegionHistoryKind.Modified);
        for (int i = 0; i < 32; i++)
        {
            using var next = plan.Build([new("all", [new(RegionExpression.All)])]);
            Assert.Throws<ArgumentException>(() => next.CopyHistoryShape(history));
            using var snapshot = next.CopyOutput("all"); Assert.Equal(36, Mass(snapshot), 6);
            snapshot.Dispose(); using var again = next.CopyOutput("all"); Assert.True(again.IsValid);
        }
    }
    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false)]
    [InlineData(DocumentStorageFormat.XmlOcaf, false)]
    [InlineData(DocumentStorageFormat.BinXcaf, true)]
    [InlineData(DocumentStorageFormat.XmlXcaf, true)]
    public void RegionAndVolumeRecipesReexecuteAfterFourFormatReopen(DocumentStorageFormat format, bool xde)
    {
        using var document = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create(); var a = Box("a", 10); document.Add(a);
        var b = Derived("b", ParametricFeatureKind.Placement, a.Id, new Dictionary<string, ParametricParameter> { ["x"] = Length(5) }); document.Add(b);
        var partitionRecipe = new ParametricPartitionRecipe([new("union", [new(RegionExpression.All.CopyTokens(), 1)])]);
        var regions = new ParametricFeatureDefinition(Guid.NewGuid(), "regions", ParametricFeatureKind.Partition,
            new Dictionary<string, ParametricParameter>(), [new("a", a.Id, ParametricOutputKind.ExactShape), new("b", b.Id, ParametricOutputKind.ExactShape)], partitionRecipe.ToJson());
        document.Add(regions);
        var volumes = Derived("volumes", ParametricFeatureKind.VolumeConstruction, regions.Id, recipe: new ParametricVolumeRecipe().ToJson()); document.Add(volumes);
        Success(document.Recompute());
        BatchTPersistenceTests.VerifyReopened(document, format, xde, reopened =>
        {
            Success(reopened.Recompute(ParametricRecomputeMode.Full));
            using var output = reopened.GetRegionOutput(regions.Id, "union"); Assert.Equal(180, Mass(output.Shape!), 6);
            Assert.NotEmpty(reopened.GetRegionOutputKeys(volumes.Id));
            using var solid = reopened.GetRegionOutput(volumes.Id, "volume-0"); Assert.True(solid.Shape!.IsValid);
        });
    }
}
