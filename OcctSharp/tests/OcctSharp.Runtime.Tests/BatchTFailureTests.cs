using System.Text.Json.Nodes;
using OcctSharp.Interop;
using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTFailureTests
{
    [Fact]
    public void CancellationAfterSuccessfulCandidateAbortsBeforeTheNextKernelAndPreservesRevisions()
    {
        using var doc = ParametricDocument.Create(); var box = Box("box"); doc.Add(box);
        var move = Derived("move", ParametricFeatureKind.Placement, box.Id); doc.Add(move); Success(doc.Recompute());
        var revisions = doc.Features.ToDictionary(x => x.Definition.Id, x => x.ResultRevision);
        using var cancel = new CancellationTokenSource();
        var report = doc.RecomputeCore(box.WithParameter("x", Length(8)), ParametricRecomputeMode.Incremental,
            null, cancel.Token, id => { Assert.Equal(box.Id, id); cancel.Cancel(); });
        Assert.True(report.Cancelled); Assert.False(report.Succeeded); Assert.Equal([box.Id], report.Executed);
        Assert.Equal(2, doc.ReadParameter(box.Id, "x").Real);
        Assert.All(doc.Features, x => Assert.Equal(revisions[x.Definition.Id], x.ResultRevision));
        using var original = doc.GetResult(move.Id, true); Assert.True(original.IsStale); Assert.Equal(24, Mass(original.Shape!), 6);
        Success(doc.Recompute());
    }

    [Fact]
    public void NativeDependencyAndCurrentResultRevisionCorruptionRejectBeforeUse()
    {
        using var owner = OcafDocument.Create(); using var graph = ParametricDocument.Attach(owner);
        var box = Box("box"); graph.Add(box); var move = Derived("move", ParametricFeatureKind.Placement, box.Id); graph.Add(move);
        Success(graph.Recompute()); var storage = new ParametricStorage(owner.Handle);
        string entry = graph.Features.Single(x => x.Definition.Id == move.Id).Entry;
        using (var command = owner.BeginTransaction())
        {
            var json = JsonNode.Parse(storage.GetText(entry, "feature")!)!; json["InputRevisions"] = new JsonObject();
            storage.SetText(entry, "feature", json.ToJsonString()); command.Commit();
        }
        Assert.Throws<InvalidDataException>(() => graph.GetResult(move.Id)); Assert.True(owner.Undo());
        using (var command = owner.BeginTransaction()) { storage.Rewire(entry, []); command.Commit(); }
        Assert.Throws<InvalidDataException>(() => graph.CreatePlan()); Assert.True(owner.Undo());
        using var result = graph.GetResult(move.Id); Assert.Equal(24, Mass(result.Shape!), 6);
    }

    [Fact]
    public void LaterKernelFailureRollsBackEarlierSuccessfulCandidates()
    {
        using var doc = ParametricDocument.Create(); var box = Box("box"); doc.Add(box);
        var move = Derived("move", ParametricFeatureKind.Placement, box.Id); doc.Add(move); Success(doc.Recompute());
        Guid old = doc.Features.Single(x => x.Definition.Id == box.Id).ResultRevision!.Value;
        doc.Update(box.WithParameter("x", Length(8)));
        var zero = ParametricParameter.FromValue(ParametricValue.FromReal(0));
        doc.Update(move.WithParameter("axisX", zero).WithParameter("axisY", zero).WithParameter("axisZ", zero));
        var failed = doc.Recompute(); Assert.False(failed.Succeeded); Assert.Contains(box.Id, failed.Executed);
        Assert.Equal(old, doc.Features.Single(x => x.Definition.Id == box.Id).ResultRevision);
        using var last = doc.GetResult(box.Id, true); Assert.True(last.IsStale); Assert.Equal(24, Mass(last.Shape!), 6);
    }

    [Fact]
    public void NativeNamingReportsMultipleSuccessorsAsAmbiguousAndMissingContextAsDeleted()
    {
        using var doc = OcafDocument.Create(); using var box = ShapeFactory.CreateBox(2, 3, 4);
        var storage = new ParametricStorage(doc.Handle);
        string context, selector, history;
        Shape[] faces = box.GetFaces();
        try
        {
            using (var command = doc.BeginTransaction())
            {
                context = doc.RootLabel.AddChild().Entry; selector = doc.RootLabel.AddChild().Entry; history = doc.RootLabel.AddChild().Entry;
                storage.Record(context, ParametricEvolutionKind.Primitive, [], [box]);
                Assert.True(storage.Select(selector, context, faces[0], ShapeKind.Face)); command.Commit();
            }
            using var first = faces[0].Transformed(ShapeTransform.CreateTranslation(4, 0, 0));
            using var second = faces[0].Transformed(ShapeTransform.CreateTranslation(8, 0, 0));
            using var compound = ShapeFactory.CreateCompound([first, second]);
            using (var command = doc.BeginTransaction())
            {
                storage.Record(history, ParametricEvolutionKind.Modified, [faces[0], faces[0]], [first, second]);
                storage.Record(context, ParametricEvolutionKind.Modified, [box], [compound]); command.Commit();
            }
            using (var command = doc.BeginTransaction())
            {
                var result = storage.Resolve(selector, ShapeKind.Face); using var owned = result.Shape;
                Assert.Equal(ParametricSelectionStatus.Ambiguous, result.Status); Assert.Null(owned); command.Commit();
            }
            using (var command = doc.BeginTransaction())
            {
                storage.Record(context, ParametricEvolutionKind.Deleted, [compound], []); command.Commit();
            }
            using (var command = doc.BeginTransaction())
            {
                var result = storage.Resolve(selector, ShapeKind.Face); using var owned = result.Shape;
                Assert.Equal(ParametricSelectionStatus.Deleted, result.Status); Assert.Null(owned); command.Commit();
            }
            var copied = storage.History(history);
            try
            {
                Assert.Equal(2, copied.Count); doc.Dispose();
                Assert.All(copied, x => { Assert.Equal(ParametricEvolutionKind.Modified, x.Kind); Assert.True(x.After!.IsValid); });
            }
            finally { foreach (var item in copied) item.Dispose(); }
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }

    [Fact]
    public void MalformedPersistedPathsFailBeforeMutationAndInterruptedStatesRecoverDownstream()
    {
        using var owner = OcafDocument.Create(); using var graph = ParametricDocument.Attach(owner);
        var box = Box("box"); graph.Add(box);
        var move = Derived("move", ParametricFeatureKind.Placement, box.Id); graph.Add(move); Success(graph.Recompute());
        string entry = graph.Features.Single(x => x.Definition.Id == box.Id).Entry;
        var storage = new ParametricStorage(owner.Handle); string original = storage.GetText(entry, "feature")!;
        using (var command = owner.BeginTransaction())
        {
            var json = JsonNode.Parse(original)!; json["ParametersEntry"] = "0:1:999";
            storage.SetText(entry, "feature", json.ToJsonString()); command.Commit();
        }
        string malformed = storage.GetText(entry, "feature")!;
        Assert.Throws<InvalidDataException>(() => ParametricDocument.Attach(owner, graph.RootEntry));
        Assert.Equal(malformed, storage.GetText(entry, "feature")); Assert.True(owner.Undo());
        using (var command = owner.BeginTransaction())
        {
            var json = JsonNode.Parse(original)!; json["State"] = (int)ParametricExecutionState.Executing;
            storage.SetText(entry, "feature", json.ToJsonString()); storage.State(entry, ParametricExecutionState.Executing); command.Commit();
        }
        using var recovered = ParametricDocument.Attach(owner, graph.RootEntry);
        Assert.All(recovered.Features, x => { Assert.True(x.Dirty); Assert.Equal(ParametricExecutionState.NotExecuted, x.State); });
        Success(recovered.Recompute());
    }

    [Fact]
    public void SubshapeMetadataConflictsPreventPublicationWithoutChangingOccurrences()
    {
        using var graph = ParametricDocument.CreateXde(); var box = Box("box"); graph.Add(box); Success(graph.Recompute());
        XdeDocument doc = graph.Xde; XdeLabel definition;
        using (var transaction = doc.BeginTransaction())
        using (var result = graph.GetResult(box.Id))
        {
            definition = doc.AddShape(result.Shape!, "part"); transaction.Commit();
        }
        using (var repair = new RepairDocumentSession(doc, definition))
        using (var transaction = doc.BeginTransaction())
        {
            var face = repair.Source.Topology.First(x => x.Kind == ShapeKind.Face);
            repair.GetOrCreateSubshapeLabel(face.Selection).Color = new(1, 0, 0); transaction.Commit();
        }
        Success(graph.EditAndRecompute(box.WithParameter("x", Length(9))));
        var review = graph.ReviewDefinition(box.Id, doc, definition);
        Assert.False(review.CanPublish); Assert.NotEmpty(review.ConflictingSubshapes);
        Assert.Throws<InvalidOperationException>(() => graph.PublishDefinition(box.Id, doc, definition));
        using var unchanged = definition.Shape; Assert.Equal(24, Mass(unchanged), 6);
    }
}
