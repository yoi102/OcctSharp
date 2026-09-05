using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchULocalFeatureTests
{
    internal static Shape FaceAt(double z, double x0 = 2, double y0 = 2, double x1 = 4, double y1 = 4)
    {
        using var wire = ShapeFactory.CreatePolygonWire([new(x0, y0, z), new(x1, y0, z), new(x1, y1, z), new(x0, y1, z)], true);
        return ShapeFactory.CreatePlanarFace(wire);
    }
    internal static Shape TopFace(Shape box)
    {
        var faces = box.GetFaces(); var top = faces.OrderByDescending(f => f.GetBoundingBox().Minimum.Z).First();
        foreach (var face in faces) if (!ReferenceEquals(face, top)) face.Dispose(); return top;
    }
    [Theory]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.Extent)]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.Until)]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.FromUntil)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.Extent)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.Until)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.FromUntil)]
    public void LocalPrismsUseRealSupportAndLimits(LimitedFeatureKind kind, LocalFeatureLimit limit)
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var support = TopFace(box);
        using var profile = FaceAt(5); using var from = FaceAt(5, -10, -10, 20, 20); using var until = FaceAt(8, -10, -10, 20, 20);
        using var plan = LimitedFeaturePlan.Create(box, profile, support, new() { Kind = kind, Limit = limit, Extent = 3, DraftAngle = .08 },
            from: limit == LocalFeatureLimit.FromUntil ? from : null, until: limit == LocalFeatureLimit.Extent ? null : until);
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid); Assert.True(Mass(result.RequireShape()) > 500);
        if (kind == LimitedFeatureKind.Prism) Assert.Equal(512, Mass(result.RequireShape()), 5);
        Assert.True(result.Diagnostics.GroupSupport.HasFlag(LocalFeatureGroupSupport.Caps));
        Assert.Contains(result.History, h => h.Kind == LocalFeatureHistoryKind.Generated);
        var groups = result.History.Where(h => h.Kind is LocalFeatureHistoryKind.FirstCap or LocalFeatureHistoryKind.LastCap
            or LocalFeatureHistoryKind.Contact or LocalFeatureHistoryKind.TangentContact or LocalFeatureHistoryKind.Lateral).ToArray();
        Assert.NotEmpty(groups);
        using var final = RepairSnapshot.Create(result.RequireShape());
        Assert.All(groups, item =>
        {
            Assert.NotNull(item.ResultTopologyIndex); Assert.NotNull(item.Shape);
            using var corresponding = final.CopySubshape(final.Select(item.ResultTopologyIndex!.Value));
            using var standalone = RepairSnapshot.Create(item.Shape!);
            Assert.Equal(RepairSnapshot.ComputeFingerprint(corresponding), standalone.Fingerprint);
        });
    }
    [Theory]
    [InlineData(ShellDraftLimit.Length)]
    [InlineData(ShellDraftLimit.UnderlyingSurface)]
    [InlineData(ShellDraftLimit.Shape)]
    public void DraftShellExtentAndStopsCreateIndependentShells(ShellDraftLimit limit)
    {
        using var circle = ShapeFactory.CreateCircleEdge(new(2, 2, 0), new(0, 0, 1), 2);
        using var wire = limit == ShellDraftLimit.Length
            ? ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(4, 0, 0), new(4, 4, 0), new(0, 4, 0)], true)
            : ShapeFactory.CreateWire([circle]);
        using var stop = FaceAt(3, -20, -20, 20, 20);
        using var plan = ShellDraftPlan.Create(wire, new() { Limit = limit, Length = 3, Angle = .1 }, limit == ShellDraftLimit.Length ? null : stop);
        wire.Dispose(); stop.Dispose(); using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        double expectedHeight = limit == ShellDraftLimit.Length ? 3 * Math.Cos(.1) : 3;
        Assert.InRange(result.RequireShape().GetBoundingBox().Maximum.Z, expectedHeight - 1e-5, expectedHeight + 1e-5);
        Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.Lateral));
        Assert.Contains(result.History, h => h.Kind == LocalFeatureHistoryKind.Generated && h.Shape is not null);
        if (limit != ShellDraftLimit.Length) Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.Limit));
    }
    [Theory]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RightCorner, false)]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RightCorner, true)]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RoundCorner, false)]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RoundCorner, true)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RightCorner, false)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RightCorner, true)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RoundCorner, false)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RoundCorner, true)]
    public void AnalyticEdgeShellLimitsReportMeasuredGeometryAndEdgeOnlyHistory(ShellDraftLimit limit, ShellDraftTransition transition, bool circular)
    {
        using var edge = circular ? ShapeFactory.CreateCircleEdge(new(2, 2, 0), new(0, 0, 1), 2)
            : ShapeFactory.CreateEdge(new(0, 0, 0), new(4, 0, 0));
        using var stop = FaceAt(3, -20, -20, 20, 20);
        using var plan = ShellDraftPlan.Create(edge, new() { Limit = limit, Transition = transition }, stop);
        edge.Dispose(); stop.Dispose();
        using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        // This SDK completes the open-line/unbounded-surface case but returns
        // invalid topology. Keep the independently measured validity visible;
        // a completed algorithm must not be confused with an accepted result.
        if (!circular && limit == ShellDraftLimit.UnderlyingSurface)
        {
            Assert.False(result.Diagnostics.ShapeIsValid);
            Assert.NotNull(result.Shape);
            Assert.False(result.Shape.IsValid);
            Assert.Throws<InvalidOperationException>(() => result.RequireShape());
            return;
        }
        Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.InRange(result.RequireShape().GetBoundingBox().Maximum.Z, 3 - 1e-5, 3 + 1e-5);
        Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.Generated));
        Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.Limit));
        var lateral = result.History.Where(h => h.Kind == LocalFeatureHistoryKind.Lateral).ToArray();
        Assert.NotEmpty(lateral);
        Assert.All(lateral, h => Assert.NotNull(h.ResultTopologyIndex));
    }
    [Theory]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RightCorner)]
    [InlineData(ShellDraftLimit.UnderlyingSurface, ShellDraftTransition.RoundCorner)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RightCorner)]
    [InlineData(ShellDraftLimit.Shape, ShellDraftTransition.RoundCorner)]
    public void CorneredShellLimitProfilesRejectBeforeEnteringTheUnsafeSdkPath(ShellDraftLimit limit, ShellDraftTransition transition)
    {
        using var wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(4, 0, 0), new(4, 4, 0), new(0, 4, 0)], true);
        using var stop = FaceAt(3, -20, -20, 20, 20);
        using var plan = ShellDraftPlan.Create(wire, new() { Limit = limit, Transition = transition }, stop);
        Assert.ThrowsAny<ArgumentException>(() => plan.Build());
        using var lengthPlan = ShellDraftPlan.Create(wire, new() { Length = 3, Transition = transition });
        using var lengthResult = lengthPlan.Build();
        Assert.True(lengthResult.Diagnostics.AlgorithmDone, lengthResult.Diagnostics.Message);
        Assert.True(lengthResult.Diagnostics.ShapeIsValid);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LinearRibsAndSlotsChangeMaterialThroughNativeForms(bool add)
    {
        using var floor = ShapeFactory.CreateBox(10, 10, 2); using var wall = ShapeFactory.CreateBox(2, 10, 10);
        using var basis = add ? floor.Fuse(wall) : ShapeFactory.CreateBox(10, 10, 10);
        using var wire = ShapeFactory.CreatePolygonWire(add ? [new(2, 5, 8), new(8, 5, 2)] : [new(2, 5, 10), new(10, 5, 2)]);
        double before = Mass(basis);
        using var plan = RibSlotPlan.Create(basis, wire, new() { AddMaterial = add, PlaneOrigin = new(0, 5, 0),
            ThicknessDirection1 = new(0, 1, 0), ThicknessDirection2 = new(0, -1, 0) });
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, $"Invalid result: {result.Diagnostics.Message}");
        Assert.True(add ? Mass(result.RequireShape()) > before : Mass(result.RequireShape()) < before, $"Before {before}; after {Mass(result.RequireShape())}");
        Assert.True(result.Diagnostics.GroupSupport.HasFlag(LocalFeatureGroupSupport.Contacts));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevolutionRibsAndSlotsUseLinearThickness(bool add)
    {
        using var lower = ShapeFactory.CreateCylinder(2, 5); using var upper = ShapeFactory.CreateCylinder(1, 3);
        using var moved = upper.Transformed(ShapeTransform.CreateTranslation(0, 0, 5));
        using var basis = add ? lower.Fuse(moved) : ShapeFactory.CreateCylinder(2, 8);
        using var wire = ShapeFactory.CreatePolygonWire(add ? [new(-2, 0, 5), new(-1, 0, 8)] : [new(-.8, 0, 9), new(-.8, 0, -1)]);
        double before = Mass(basis);
        using var plan = RibSlotPlan.Create(basis, wire, new() { Revolution = true, AddMaterial = add, PlaneOrigin = add ? new(-2, 0, 5) : new(-.8, 0, 9), Thickness1 = .2, Thickness2 = .2 });
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, $"Invalid result: {result.Diagnostics.Message}");
        Assert.True(add ? Mass(result.RequireShape()) > before : Mass(result.RequireShape()) < before, $"Before {before}; after {Mass(result.RequireShape())}");
    }
}
