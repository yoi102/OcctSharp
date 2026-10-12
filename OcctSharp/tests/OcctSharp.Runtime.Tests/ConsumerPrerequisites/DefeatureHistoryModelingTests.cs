using OcctSharp;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class DefeatureHistoryModelingTests
{
    private static readonly FeatureModelingOptions Robust = new()
    {
        FuzzyTolerance = 1e-7, NonDestructive = true, RepairInputs = false, UnifyResult = true
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovedBoreHasDeletedFaceAndCompleteFinalSourceSubshapeHistory(bool unify)
    {
        using var shape = HoledBox(1);
        using var source = RepairSnapshot.Create(shape);
        var faces = CylindricalFaces(source);
        Assert.Single(faces);
        using var result = DefeatureHistoryModeling.Build(source, faces, unify);
        CheckHistory(result, source, faces, unify);
        Assert.Equal(1728, Volume(result.RequireShape()), 5);
        using var unchanged = RepairSnapshot.Create(shape);
        Assert.Equal(source.Fingerprint, unchanged.Fingerprint);
        using var expected = ShapeFactory.CreateBox(12, 12, 12);
        using var extra = result.RequireShape().Cut(expected);
        using var missing = expected.Cut(result.RequireShape());
        Assert.Equal(0, extra.GetTopologySummary().UniqueCounts.SolidCount);
        Assert.Equal(0, missing.GetTopologySummary().UniqueCounts.SolidCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TwoSelectedBoresMustBothBeNativelyDeleted(bool unify)
    {
        using var shape = HoledBox(2);
        using var source = RepairSnapshot.Create(shape);
        var faces = CylindricalFaces(source);
        Assert.Equal(2, faces.Length);
        using var result = DefeatureHistoryModeling.Build(source, faces, unify);
        CheckHistory(result, source, faces, unify);
        Assert.Equal(1728, Volume(result.RequireShape()), 5);
        using var unchanged = RepairSnapshot.Create(shape);
        Assert.Equal(source.Fingerprint, unchanged.Fingerprint);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TiltedLocatedCircularSeamsRetainExactFinalMembership(bool unify)
    {
        using var box = HoledBox(1);
        using var original = RepairSnapshot.Create(box);
        using var rotation = GpTrsf.Create(4, 2, -1, 2, 1, 3, .413);
        using var located = ShapeHistoryModeling.Transform(original, rotation);
        using var source = RepairSnapshot.Create(located.RequireShape());
        var faces = CylindricalFaces(source);
        Assert.Single(faces);
        using var result = DefeatureHistoryModeling.Build(source, faces, unify);
        CheckHistory(result, source, faces, unify);
        Assert.Equal(1728, Volume(result.RequireShape()), 5);
        using var unchanged = RepairSnapshot.Create(located.RequireShape());
        Assert.Equal(source.Fingerprint, unchanged.Fingerprint);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResultAndOwnedHistorySurviveInputDisposalAndAreReleasedTogether(bool unify)
    {
        LocalFeatureResult result;
        using (var shape = HoledBox(1))
        using (var source = RepairSnapshot.Create(shape))
        {
            result = DefeatureHistoryModeling.Build(source, CylindricalFaces(source), unify);
            CheckHistory(result, source, CylindricalFaces(source), unify);
        }
        var owned = result.History.Where(row => row.Shape is not null).Select(row => row.Shape!).ToArray();
        using (result)
        {
            Assert.True(result.RequireShape().GetValidationReport(true, true).IsValid);
            Assert.Equal(1728, Volume(result.RequireShape()), 5);
            Assert.NotEmpty(owned);
            Assert.All(owned, shape => Assert.True(shape.IsValid));
        }
        Assert.Throws<ObjectDisposedException>(() => result.RequireShape());
        Assert.All(owned, shape => Assert.Throws<ObjectDisposedException>(() => shape.GetTopologySummary()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcceptedDefeatureResultCanFeedIndependentSubsequentHistory(bool unify)
    {
        using var shape = HoledBox(1);
        using var source = RepairSnapshot.Create(shape);
        using var result = DefeatureHistoryModeling.Build(source, CylindricalFaces(source), unify);
        using var middle = RepairSnapshot.Create(result.RequireShape());
        using var mirror = GpTrsf.CreatePlaneMirror(3, 2, -1, 2, 1, 3);
        using var subsequent = ShapeHistoryModeling.Transform(middle, mirror);
        Assert.True(subsequent.Diagnostics.AlgorithmDone);
        Assert.True(subsequent.Diagnostics.ShapeIsValid);
        Assert.False(subsequent.Diagnostics.HasComposedHistory);
        Assert.Equal(1728, Volume(subsequent.RequireShape()), 5);
        var slots = Tracked(middle).Select(t => t.Selection.Index).Order().ToArray();
        Assert.Equal(slots, subsequent.History.Select(row => row.Source!.Value.TopologyIndex).Order().ToArray());
        Assert.All(subsequent.History, row => Assert.NotNull(row.ResultTopologyIndex));
    }

    [Fact]
    public void InvalidStaleWrongKindDuplicateAndDisposedSelectionsNeverPublish()
    {
        using var shape = HoledBox(1);
        using var source = RepairSnapshot.Create(shape);
        var face = Assert.Single(CylindricalFaces(source));
        Assert.Throws<ArgumentNullException>(() => DefeatureHistoryModeling.Build(null!, [face]));
        Assert.Throws<ArgumentNullException>(() => DefeatureHistoryModeling.Build(source, null!));
        Assert.Throws<ArgumentException>(() => DefeatureHistoryModeling.Build(source, []));
        Assert.Throws<ArgumentException>(() => DefeatureHistoryModeling.Build(source, Enumerable.Repeat(face, 257).ToArray()));
        Assert.Throws<ArgumentException>(() => DefeatureHistoryModeling.Build(source, [face, face]));
        using var foreign = RepairSnapshot.Create(shape, revision: 2);
        Assert.Throws<ArgumentException>(() => DefeatureHistoryModeling.Build(source, [foreign.Select(face.Index)]));
        var edge = source.Topology.First(t => t.Kind == ShapeKind.Edge).Selection;
        Assert.Throws<ArgumentException>(() => DefeatureHistoryModeling.Build(source, [edge]));
        Assert.Throws<ArgumentOutOfRangeException>(() => DefeatureHistoryModeling.Build(source, [new(source.Identity, source.Topology.Count)]));
        source.Dispose();
        Assert.Throws<ObjectDisposedException>(() => DefeatureHistoryModeling.Build(source, [face]));
    }

    [Fact]
    public void RemovingAnOuterBoxFaceCannotBeAcceptedAsAConfirmedFeatureDeletion()
    {
        using var shape = ShapeFactory.CreateBox(12, 12, 12);
        using var source = RepairSnapshot.Create(shape);
        var face = source.Topology.First(t => t.Kind == ShapeKind.Face).Selection;
        using var result = DefeatureHistoryModeling.Build(source, [face]);
        Assert.False(result.Diagnostics.AlgorithmDone);
        Assert.Throws<InvalidOperationException>(() => result.RequireShape());
        Assert.Empty(result.History);
        using var unchanged = RepairSnapshot.Create(shape);
        Assert.Equal(source.Fingerprint, unchanged.Fingerprint);
    }

    private static Shape HoledBox(int holes)
    {
        using var box = ShapeFactory.CreateBox(12, 12, 12);
        using var first = FeatureModeling.CutHole(box, new(holes == 1 ? 6 : 3, 6, 20), new(0, 0, -1), 1, 30, true, Robust);
        if (holes == 1) return first.RequireShape().Clone();
        using var second = FeatureModeling.CutHole(first.RequireShape(), new(9, 6, 20), new(0, 0, -1), 1, 30, true, Robust);
        return second.RequireShape().Clone();
    }

    private static RepairSelection[] CylindricalFaces(RepairSnapshot source)
        => source.Topology.Where(t =>
        {
            if (t.Kind != ShapeKind.Face) return false;
            using var face = source.CopySubshape(t.Selection);
            return face.GetFaceSurfaceSnapshot().SurfaceType == SurfaceGeometryType.Cylinder;
        }).Select(t => t.Selection).ToArray();

    private static IEnumerable<RepairTopologyItem> Tracked(RepairSnapshot source)
        => source.Topology.Where(t => t.Kind is ShapeKind.Face or ShapeKind.Edge or ShapeKind.Vertex);

    private static double Volume(Shape shape)
        => shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume, 1e-10).Properties.Mass;

    private static void CheckHistory(LocalFeatureResult result, RepairSnapshot source, RepairSelection[] deleted, bool unify)
    {
        Assert.Equal(LocalFeatureOperation.Defeature, result.Diagnostics.Operation);
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
        Assert.Equal(unify, result.Diagnostics.HasComposedHistory);
        Assert.True(result.Diagnostics.GroupSupport.HasFlag(LocalFeatureGroupSupport.Evolution));
        Assert.Equal(source.Identity, result.Source);
        Assert.Equal(source.Fingerprint, result.SourceFingerprint);
        using var final = RepairSnapshot.Create(result.RequireShape());
        Assert.NotEqual(source.Fingerprint, final.Fingerprint);
        Assert.All(result.History, row =>
        {
            Assert.NotNull(row.Source);
            Assert.Equal(result.PlanId, row.Source!.Value.PlanId);
            Assert.Equal(0, row.Source.Value.ArgumentIndex);
        });
        foreach (var slot in Tracked(source))
        {
            var rows = result.History.Where(row => row.Source!.Value.TopologyIndex == slot.Selection.Index).ToArray();
            Assert.NotEmpty(rows);
            foreach (var row in rows)
            {
                Assert.Equal(slot.Kind, row.Source!.Value.Kind);
                Assert.True(row.Kind is LocalFeatureHistoryKind.Unchanged or LocalFeatureHistoryKind.Modified
                    or LocalFeatureHistoryKind.Generated or LocalFeatureHistoryKind.Deleted, row.Kind.ToString());
                if (row.Kind == LocalFeatureHistoryKind.Deleted)
                {
                    Assert.Null(row.Shape);
                    Assert.Null(row.ResultTopologyIndex);
                }
                else
                {
                    Assert.NotNull(row.Shape);
                    Assert.NotNull(row.ResultTopologyIndex);
                    var target = final.Topology[row.ResultTopologyIndex!.Value];
                    Assert.Equal(row.Shape!.Kind, target.Kind);
                    using var member = final.CopySubshape(target.Selection);
                    using var expected = RepairSnapshot.Create(member);
                    using var copied = RepairSnapshot.Create(row.Shape);
                    Assert.Equal(expected.Fingerprint, copied.Fingerprint);
                }
            }
        }
        foreach (var face in deleted)
        {
            var rows = result.History.Where(row => row.Source!.Value.TopologyIndex == face.Index).ToArray();
            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Equal(LocalFeatureHistoryKind.Deleted, row.Kind));
        }
    }
}
