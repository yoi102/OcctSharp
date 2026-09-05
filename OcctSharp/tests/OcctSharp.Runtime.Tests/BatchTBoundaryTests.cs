using System.Runtime.InteropServices;
using OcctSharp.Interop;
using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchTPersistenceTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTBoundaryTests
{
    [Theory]
    [InlineData(FeatureBooleanOperation.Cut, 12, 1, 2)]
    [InlineData(FeatureBooleanOperation.Common, 12, 0, 1)]
    [InlineData(FeatureBooleanOperation.Fuse, 24, 0, 2)]
    public void BooleanModesPreserveGeometryAndRelocateActualSourceAssociations(FeatureBooleanOperation operation,
        double volume, double minimumX, double maximumX)
    {
        using var doc = ParametricDocument.Create(); var a = Box("a"); var b = Box("b", 1); doc.Add(a); doc.Add(b);
        var boolean = new ParametricFeatureDefinition(Guid.NewGuid(), "boolean", ParametricFeatureKind.Boolean,
            new Dictionary<string, ParametricParameter>(), [new("a", a.Id, ParametricOutputKind.ExactShape), new("b", b.Id, ParametricOutputKind.ExactShape)],
            new ParametricBooleanRecipe(operation).ToJson()); doc.Add(boolean); Success(doc.Recompute());
        using var result = doc.GetResult(boolean.Id); Assert.Equal(volume, Mass(result.Shape!), 6);
        var bounds = result.Shape!.GetBoundingBox(); Assert.Equal(minimumX, bounds.Minimum.X, 5); Assert.Equal(maximumX, bounds.Maximum.X, 5);
        var copied = doc.Duplicate([a.Id, b.Id, boolean.Id]);
        var copiedHistory = doc.GetAlgorithmHistory(copied[boolean.Id]);
        try { Assert.All(copiedHistory, x => Assert.Contains(x.SourceFeatureId, new[] { copied[a.Id], copied[b.Id] })); }
        finally { foreach (var item in copiedHistory) item.Dispose(); }
        Success(doc.Recompute());
        using var duplicate = doc.GetResult(copied[boolean.Id]); Assert.Equal(volume, Mass(duplicate.Shape!), 6);
    }

    [Fact]
    public unsafe void RawParameterAndGraphBuffersRejectUndersizedOutputsWithoutOverwritingSentinels()
    {
        Assert.Equal(24, Marshal.SizeOf<ParameterInfoRaw>());
        Assert.Equal(16, Marshal.OffsetOf<ParameterInfoRaw>(nameof(ParameterInfoRaw.Real)).ToInt32());
        using var doc = OcafDocument.Create(); var storage = new ParametricStorage(doc.Handle);
        string entry, dependency, first;
        using (var command = doc.BeginTransaction())
        {
            entry = doc.RootLabel.AddChild().Entry; dependency = doc.RootLabel.AddChild().Entry; first = doc.RootLabel.AddChild().Entry;
            storage.SetParameter(entry, ParametricValue.FromIntegers([4, 5, 6]));
            int a = storage.Register(first, Guid.NewGuid()), b = storage.Register(dependency, Guid.NewGuid());
            storage.Rewire(dependency, [a]); Assert.True(b > 0); command.Commit();
        }
        int* values = stackalloc int[3] { 111, 222, 333 };
        Assert.Equal(NativeStatus.InvalidArgument, ParametricNative.GetParameter(doc.Handle, entry, out var info,
            null, 0, out _, values, null, 1));
        Assert.Equal(3, info.Count); Assert.Equal(111, values[0]); Assert.Equal(222, values[1]);
        Assert.Equal(NativeStatus.InvalidArgument, ParametricNative.Links(doc.Handle, dependency, 0, values, 0, out int count, out _, out _));
        Assert.Equal(1, count); Assert.Equal(111, values[0]);
        Assert.Equal(NativeStatus.InvalidArgument, ParametricNative.Rewire(doc.Handle, first, null, -1));
        using (var command = doc.BeginTransaction())
        {
            info = new() { Kind = 2, Real = double.NaN };
            Assert.Equal(NativeStatus.InvalidArgument, ParametricNative.SetParameter(doc.Handle, entry, in info, 0, 0, null, null));
            Assert.Equal([4, 5, 6], storage.GetParameter(entry).Integers);
        }
    }

    [Fact]
    public void ActualExtrusionEvolutionHasOwningGeneratedAndTransactionSelectedHistory()
    {
        using var doc = OcafDocument.Create(); var storage = new ParametricStorage(doc.Handle);
        using var edge = ShapeFactory.CreateEdge(new(0, 0, 0), new(2, 0, 0));
        using var direction = GpVec.Create(0, 0, 3); using var face = edge.Extrude(direction);
        string history;
        using (var command = doc.BeginTransaction())
        {
            history = doc.RootLabel.AddChild().Entry;
            storage.Record(history, ParametricEvolutionKind.Generated, [edge], [face]); command.Commit();
        }
        var copied = storage.History(history);
        try
        {
            var generated = Assert.Single(copied); Assert.Equal(ParametricEvolutionKind.Generated, generated.Kind);
            Assert.Equal(ShapeKind.Edge, generated.Before!.Kind); Assert.Equal(ShapeKind.Face, generated.After!.Kind);
            var beginning = storage.History(history, 0);
            // OCCT's transaction integer is a nesting level, not a durable commit revision.
            try { Assert.Equal(ParametricEvolutionKind.Generated, Assert.Single(beginning).Kind); }
            finally { foreach (var item in beginning) item.Dispose(); }
            Assert.Throws<ArgumentException>(() => storage.History(history, int.MaxValue));
            doc.Dispose(); Assert.Equal(6, generated.After.InspectProperties(InspectionPropertyKind.Area).Mass, 6);
        }
        finally { foreach (var item in copied) item.Dispose(); }
    }

    [Fact]
    public void RelocatedSelectionsStayIndependentAndTamperedTokensAreRejected()
    {
        using var doc = ParametricDocument.Create(); using var shape = ShapeFactory.CreateBox(2, 3, 4);
        var source = Source(doc, "source", shape); Success(doc.Recompute());
        using var result = doc.GetResult(source.Id); var original = doc.Select(result, ShapeKind.Face, 0);
        Assert.Throws<ArgumentException>(() => doc.Resolve(original with { Kind = ShapeKind.Edge }));
        Assert.Throws<ArgumentException>(() => doc.Resolve(original with { Entry = doc.RootEntry }));
        Guid copiedId = doc.Duplicate([source.Id])[source.Id]; Success(doc.Recompute());
        var copied = Assert.Single(doc.GetSelections(copiedId));
        doc.TransformSource(copiedId, ShapeTransform.CreateTranslation(10, 0, 0)); Success(doc.Recompute());
        using var originalFace = doc.Resolve(original); using var copiedFace = doc.Resolve(copied);
        Assert.Equal(ParametricSelectionStatus.Resolved, originalFace.Status);
        Assert.Equal(ParametricSelectionStatus.Resolved, copiedFace.Status);
        Assert.True(originalFace.Shape!.GetBoundingBox().Maximum.X < 2.1);
        Assert.True(copiedFace.Shape!.GetBoundingBox().Minimum.X > 9.9);
        doc.Dispose(); Assert.True(result.Shape!.IsValid); Assert.True(copiedFace.Shape.IsValid);
    }
}
