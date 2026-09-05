using OcctSharp.Interop;
using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTPersistenceTests
{
    internal static void VerifyReopened(ParametricDocument document, DocumentStorageFormat format, bool xde, Action<ParametricDocument> verify)
    {
        string folder = Path.Combine(Path.GetTempPath(), "OcctSharp-BatchT-recipes-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string path = document.Save(Path.Combine(folder, "参数"), format);
            using var reopened = ParametricDocument.Open(path, document.RootEntry, xde);
            verify(reopened);
        }
        finally { Directory.Delete(folder, true); }
    }
    internal static ParametricFeatureDefinition Source(ParametricDocument doc, string name, Shape source)
    {
        var definition = new ParametricFeatureDefinition(Guid.NewGuid(), name, ParametricFeatureKind.SourceShape,
            new Dictionary<string, ParametricParameter>(), []);
        doc.Add(definition, source); return definition;
    }

    [Fact]
    public void HistorySelectsDurableResultGenerationsAcrossUndoRedoAndReopen()
    {
        using var doc = ParametricDocument.Create(); var box = Box("history"); doc.Add(box); Success(doc.Recompute());
        Guid first = doc.Features.Single().ResultRevision!.Value;
        Success(doc.EditAndRecompute(box.WithParameter("x", Length(8))));
        Guid second = doc.Features.Single().ResultRevision!.Value;
        using var old = doc.GetHistory(box.Id, first); using var current = doc.GetHistory(box.Id, second);
        Assert.Equal(first, old.ResultRevision); Assert.Equal(24, Mass(Assert.Single(old.Evolutions).After!), 6);
        Assert.Contains(current.Evolutions, x => x.Kind == ParametricEvolutionKind.Modified && Math.Abs(Mass(x.After!) - 96) < 1e-6);
        Assert.True(doc.Undo()); Assert.Equal(first, doc.Features.Single().ResultRevision);
        Assert.Throws<ArgumentException>(() => doc.GetHistory(box.Id, second)); Assert.True(doc.Redo());
        VerifyReopened(doc, DocumentStorageFormat.BinOcaf, false, reopened =>
        {
            using var restored = reopened.GetHistory(box.Id, first); Assert.Equal(24, Mass(Assert.Single(restored.Evolutions).After!), 6);
            using var latest = reopened.GetHistory(box.Id, second); Assert.Equal(current.Evolutions.Count, latest.Evolutions.Count);
            Assert.Throws<ArgumentException>(() => reopened.GetHistory(box.Id, Guid.NewGuid()));
        });
        doc.Dispose(); Assert.Equal(24, Mass(Assert.Single(old.Evolutions).After!), 6);
    }

    [Fact]
    public void DedicatedSelectionTracksActualTransformHistoryAndRejectsForeignStaleContexts()
    {
        using var doc = ParametricDocument.Create(); using var box = ShapeFactory.CreateBox(2, 3, 4);
        var source = Source(doc, "source", box); Success(doc.Recompute());
        using var original = doc.GetResult(source.Id);
        var selection = doc.Select(original, ShapeKind.Face, 0);
        using (var result = doc.Resolve(selection)) Assert.Equal(ParametricSelectionStatus.Resolved, result.Status);
        using var foreign = ParametricDocument.Create();
        Assert.Throws<ArgumentException>(() => foreign.Resolve(selection));
        Assert.Throws<ArgumentException>(() => foreign.Select(original, ShapeKind.Face, 0));
        doc.TransformSource(source.Id, ShapeTransform.CreateTranslation(10, 0, 0));
        Assert.Throws<InvalidOperationException>(() => doc.Select(original, ShapeKind.Face, 0));
        Success(doc.Recompute());
        using var moved = doc.Resolve(selection);
        Assert.Equal(ParametricSelectionStatus.Resolved, moved.Status);
        Assert.True(moved.Shape!.GetBoundingBox().Minimum.X > 9.9);
        using var history = doc.GetHistory(source.Id);
        Assert.Contains(history.Evolutions, x => x.Kind == ParametricEvolutionKind.Modified);
        Assert.Single(doc.GetSelections(source.Id));
        doc.Delete([source.Id]);
        using (var missing = doc.Resolve(selection)) Assert.Equal(ParametricSelectionStatus.Deleted, missing.Status);
        Assert.True(doc.Undo());
        using var restored = doc.Resolve(selection); Assert.Equal(ParametricSelectionStatus.Resolved, restored.Status);
        doc.Dispose(); Assert.True(moved.Shape.GetBoundingBox().Minimum.X > 9.9);
        Assert.All(history.Evolutions, x => Assert.NotNull(x.After));
    }

    [Fact]
    public void SubgraphRelocationRewritesIdsExpressionsAndNativeDependencies()
    {
        using var doc = ParametricDocument.Create(); var box = Box("box"); doc.Add(box);
        var related = Box("related").WithParameter("x", ParametricParameter.FromExpression(ParametricExpression.Parameter(box.Id, "x")));
        doc.Add(related); Success(doc.Recompute());
        Assert.Throws<InvalidOperationException>(() => doc.Duplicate([related.Id]));
        var mapping = doc.Duplicate([box.Id, related.Id]);
        Assert.Equal(2, mapping.Count); Assert.Equal(4, doc.Features.Count);
        Assert.Equal(4, doc.Features.Select(x => x.FunctionId).Distinct().Count());
        Success(doc.Recompute());
        var copiedBox = doc.Features.Single(x => x.Definition.Id == mapping[box.Id]).Definition;
        doc.Update(copiedBox.WithParameter("x", Length(5))); Success(doc.Recompute());
        using var copied = doc.GetResult(mapping[related.Id]); Assert.Equal(60, Mass(copied.Shape!), 6);
        using var untouched = doc.GetResult(related.Id); Assert.Equal(24, Mass(untouched.Shape!), 6);
        Assert.Throws<InvalidOperationException>(() => doc.Delete([box.Id]));
        Assert.Equal(2, doc.Delete([box.Id], ParametricDeletePolicy.Cascade).Count);
        Assert.Equal(2, doc.Features.Count); Assert.True(doc.Undo()); Assert.Equal(4, doc.Features.Count);
        var retained = doc.Duplicate([related.Id], ParametricExternalReferencePolicy.Retain);
        Success(doc.Recompute()); using var retainedResult = doc.GetResult(retained[related.Id]); Assert.Equal(24, Mass(retainedResult.Shape!), 6);
    }

    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf, false, "cbf")]
    [InlineData(DocumentStorageFormat.XmlOcaf, false, "xml")]
    [InlineData(DocumentStorageFormat.BinXcaf, true, "xbf")]
    [InlineData(DocumentStorageFormat.XmlXcaf, true, "xml")]
    public void FourFormatsReopenFunctionsParametersAndSelectionsThenReallyRecompute(DocumentStorageFormat format, bool xde, string extension)
    {
        string folder = Path.Combine(Path.GetTempPath(), "OcctSharp-BatchT-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder); string path = Path.Combine(folder, "参数." + extension);
        string root; Guid id; ParametricSelection selection;
        try
        {
            using (var doc = xde ? ParametricDocument.CreateXde() : ParametricDocument.Create())
            using (var source = ShapeFactory.CreateBox(2, 3, 4))
            {
                root = doc.RootEntry; id = Source(doc, "源形状", source).Id; Success(doc.Recompute());
                using var result = doc.GetResult(id); selection = doc.Select(result, ShapeKind.Face, 0);
                doc.Save(path, format);
            }
            using var reopened = ParametricDocument.Open(path, root, xde);
            Assert.Equal("源形状", reopened.Features.Single().Definition.Name);
            using (var selected = reopened.Resolve(selection)) Assert.Equal(ParametricSelectionStatus.Resolved, selected.Status);
            reopened.TransformSource(id, ShapeTransform.CreateTranslation(8, 0, 0)); Success(reopened.Recompute());
            using var moved = reopened.Resolve(selection);
            Assert.Equal(ParametricSelectionStatus.Resolved, moved.Status); Assert.True(moved.Shape!.GetBoundingBox().Minimum.X > 7.9);
            using var owning = reopened.GetResult(id); Assert.Equal(24, Mass(owning.Shape!), 6);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void UnknownSchemaAndEscapedFeaturePathsRejectWithoutMutationAndAttachedOwnerRemainsOwnedByCaller()
    {
        using var owner = OcafDocument.Create(); var graph = ParametricDocument.Attach(owner); var box = Box("box"); graph.Add(box);
        string root = graph.RootEntry;
        var storage = new ParametricStorage(owner.Handle);
        string original = storage.GetText(root, "manifest")!;
        using (var transaction = owner.BeginTransaction())
        {
            storage.SetText(root, "manifest", original.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":999", StringComparison.Ordinal)); transaction.Commit();
        }
        Assert.Throws<NotSupportedException>(() => ParametricDocument.Attach(owner, root));
        Assert.Contains("999", storage.GetText(root, "manifest"));
        owner.Undo();
        graph.Dispose(); Assert.NotNull(owner.RootLabel);
        using var attached = ParametricDocument.Attach(owner, root); Success(attached.Recompute());
        owner.Dispose(); Assert.Throws<ObjectDisposedException>(() => attached.CreatePlan());
    }
}
