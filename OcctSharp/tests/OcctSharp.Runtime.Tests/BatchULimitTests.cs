using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchULocalFeatureTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchULimitTests
{
    [Theory]
    [InlineData(LimitedFeatureKind.Prism)]
    [InlineData(LimitedFeatureKind.DraftedPrism)]
    public void UnreachableLimiterIsNotReplacedByAnUnboundedFeature(LimitedFeatureKind kind)
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var top = TopFace(box); using var profile = FaceAt(5);
        // A plane parallel to the feature direction is unreachable even when
        // the kernel uses its unbounded support instead of its trimmed face.
        using var stopWire = ShapeFactory.CreatePolygonWire([new(100, 0, 0), new(100, 10, 0), new(100, 10, 10), new(100, 0, 10)], true);
        using var unreachable = ShapeFactory.CreatePlanarFace(stopWire);
        using var plan = LimitedFeaturePlan.Create(box, profile, top, new() { Kind = kind, Limit = LocalFeatureLimit.Until, DraftAngle = .08 }, until: unreachable);
        using var result = plan.Build(); Assert.False(result.Diagnostics.AlgorithmDone); Assert.Null(result.Shape);
        Assert.Throws<InvalidOperationException>(() => result.RequireShape()); Assert.Equal(500, Mass(box), 6);
    }

    [Fact]
    public void DraftedCutHeightMeasuresTheActualTaper()
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var profile = FaceAt(0);
        var faces = box.GetFaces();
        try
        {
            var bottom = faces.Single(f => Math.Abs(f.GetBoundingBox().Maximum.Z) < 1e-5);
            using var plan = LimitedFeaturePlan.Create(box, profile, bottom,
                new() { Kind = LimitedFeatureKind.DraftedPrism, AddMaterial = false, Extent = 3, DraftAngle = .08 });
            using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
            double tangent = Math.Tan(.08); Assert.Equal(500 - (12 - 36 * tangent + 36 * tangent * tangent), Mass(result.RequireShape()), 5);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }

    [Fact]
    public void LimitedLineSpineReportsThePinnedSdkCurveConversionFailure()
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var top = TopFace(box); using var profile = FaceAt(5);
        using var spine = ShapeFactory.CreatePolygonWire([new(3, 3, 5), new(3, 3, 10)]);
        using var until = FaceAt(8, -10, -10, 20, 20);
        using var plan = LimitedFeaturePlan.Create(box, profile, top, new() { Kind = LimitedFeatureKind.Pipe, Limit = LocalFeatureLimit.Until }, until: until, spine: spine);
        using var result = plan.Build(); Assert.False(result.Diagnostics.AlgorithmDone); Assert.Null(result.Shape);
        Assert.Contains("curve", result.Diagnostics.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SlidingProfileEdgesRetainSharedMembershipAndRejectForeignPairs()
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var support = TopFace(box);
        using var profile = FaceAt(5, 0, 2, 4, 4);
        var edges = profile.GetSubShapes(ShapeKind.Edge); var faces = box.GetFaces();
        try
        {
            var edge = edges.Single(e => Math.Abs(e.GetBoundingBox().Maximum.X) < 1e-5);
            var face = faces.Single(f => Math.Abs(f.GetBoundingBox().Maximum.X) < 1e-5);
            LocalSlidingConstraint pair = new(edge, face);
            LimitedFeatureOptions options = new() { Extent = 3, AddMaterial = false, Direction = new(0, 0, -1) };
            using var plan = LimitedFeaturePlan.Create(box, profile, support, options, sliding: [pair]);
            using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
            Assert.Equal(476, Mass(result.RequireShape()), 5);
            using var duplicate = LimitedFeaturePlan.Create(box, profile, support, options, sliding: [pair, pair]);
            Assert.Throws<ArgumentException>(() => duplicate.Build());
            using var foreign = FaceAt(5, 0, 2, 4, 4);
            using var invalid = LimitedFeaturePlan.Create(box, profile, support, options, sliding: [new(edge, foreign)]);
            Assert.Throws<ArgumentException>(() => invalid.Build());
            Assert.Equal(500, Mass(box), 6);
        }
        finally { foreach (var edge in edges) edge.Dispose(); foreach (var face in faces) face.Dispose(); }
    }

    [Fact]
    public void DraftedUntilEndInsufficientAxialReachRemainsADiagnosticFailure()
    {
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var profile = FaceAt(0);
        var faces = box.GetFaces();
        try
        {
            var bottom = faces.Single(f => Math.Abs(f.GetBoundingBox().Maximum.Z) < 1e-5);
            using var plan = LimitedFeaturePlan.Create(box, profile, bottom,
                new() { Kind = LimitedFeatureKind.DraftedPrism, Limit = LocalFeatureLimit.UntilEnd, AddMaterial = false, DraftAngle = .001 });
            using var result = plan.Build(); Assert.False(result.Diagnostics.AlgorithmDone); Assert.Null(result.Shape);
            Assert.Contains("Prism construction", result.Diagnostics.Message, StringComparison.Ordinal);
            Assert.Throws<InvalidOperationException>(() => result.RequireShape()); Assert.Equal(1000, Mass(box), 6);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }

    [Theory]
    [InlineData(LocalFeatureLimit.Extent)]
    [InlineData(LocalFeatureLimit.Until)]
    [InlineData(LocalFeatureLimit.FromUntil)]
    public void RevolvedFeaturesUseRadialProfileAndPlanarStop(LocalFeatureLimit limit)
    {
        using var unplaced = ShapeFactory.CreateBox(10, 20, 5);
        using var box = unplaced.Transformed(ShapeTransform.CreateTranslation(0, -10, 0)); using var support = TopFace(box); using var profile = FaceAt(5);
        using var stopWire = ShapeFactory.CreatePolygonWire([new(-10, 0, -10), new(-10, 0, 20), new(20, 0, 20), new(20, 0, -10)], true);
        using var stop = ShapeFactory.CreatePlanarFace(stopWire);
        using var plan = LimitedFeaturePlan.Create(box, profile, support,
            new() { Kind = LimitedFeatureKind.Revolved, Limit = limit, AxisOrigin = new(0, 0, 5), Direction = new(1, 0, 0), Extent = .7 },
            from: limit == LocalFeatureLimit.FromUntil ? support : null, until: limit == LocalFeatureLimit.Extent ? null : stop);
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid);
        double expected = 1000 + 12 * (limit == LocalFeatureLimit.Extent ? .7 : Math.PI / 2);
        Assert.True(Math.Abs(Mass(result.RequireShape()) - expected) < 1e-4,
            $"Expected {expected}; actual {Mass(result.RequireShape())}; bounds {result.RequireShape().GetBoundingBox()}; topology {result.RequireShape().GetTopologySummary()}");
        Assert.Equal(1, result.RequireShape().GetTopologySummary().UniqueCounts.SolidCount);
        if (limit != LocalFeatureLimit.Extent)
        {
            using var material = result.RequireShape().Cut(box);
            Assert.InRange(material.GetBoundingBox().Maximum.Y, -1e-5, 1e-5);
            Assert.InRange(material.GetBoundingBox().Minimum.Y, -4.00001, -3.99999);
        }
    }

    [Theory]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.UntilEnd)]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.FromEnd)]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.ThroughAll)]
    [InlineData(LimitedFeatureKind.Prism, LocalFeatureLimit.UntilAndExtent)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.UntilEnd)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.FromEnd)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.ThroughAll)]
    [InlineData(LimitedFeatureKind.DraftedPrism, LocalFeatureLimit.UntilAndExtent)]
    public void SemiInfinitePrismModesHaveFiniteCutResults(LimitedFeatureKind kind, LocalFeatureLimit limit)
    {
        // MakeDPrism's UntilEnd uses the largest box dimension as a slanted
        // generatrix length. Give it enough axial reach beyond the opposite cap.
        using var box = ShapeFactory.CreateBox(20, 10, 10); using var top = TopFace(box);
        using var profile = kind == LimitedFeatureKind.DraftedPrism && limit == LocalFeatureLimit.UntilEnd
            ? FaceAt(0) : FaceAt(10);
        var baseFaces = box.GetFaces();
        using var bottom = baseFaces.Single(f => Math.Abs(f.GetBoundingBox().Maximum.Z) < 1e-5);
        foreach (var face in baseFaces) if (!ReferenceEquals(face, bottom)) face.Dispose();
        using var until = FaceAt(3, -10, -10, 20, 20);
        using var plan = LimitedFeaturePlan.Create(box, profile, kind == LimitedFeatureKind.DraftedPrism && limit == LocalFeatureLimit.UntilEnd ? bottom : top,
            new() { Kind = kind, Limit = limit, AddMaterial = false, Direction = new(0, 0, -1), DraftAngle = limit == LocalFeatureLimit.UntilEnd ? .001 : .05, Extent = 7 },
            until: limit is LocalFeatureLimit.FromEnd or LocalFeatureLimit.UntilAndExtent ? until : null);
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid); Assert.InRange(Mass(result.RequireShape()), 1800, 1999.9);
        Assert.Equal(2000, Mass(box), 6);
    }

    [Theory]
    [InlineData(LocalFeatureLimit.Extent)]
    [InlineData(LocalFeatureLimit.Until)]
    [InlineData(LocalFeatureLimit.FromUntil)]
    public void PipeFeatureStopsUseTheSharedSupportGraph(LocalFeatureLimit limit)
    {
        using var box = ShapeFactory.CreateBox(10, 10, 5); using var support = TopFace(box); using var profile = FaceAt(5);
        using var spineEdge = ShapeFactory.CreateBezierEdge([new(3, 3, 5), new(3, 3, 7.5), new(3, 3, 10)]);
        using var spine = ShapeFactory.CreateWire([spineEdge]);
        using var from = FaceAt(5, -10, -10, 20, 20); using var until = FaceAt(8, -10, -10, 20, 20);
        using var plan = LimitedFeaturePlan.Create(box, profile, support, new() { Kind = LimitedFeatureKind.Pipe, Limit = limit },
            from: limit == LocalFeatureLimit.FromUntil ? from : null, until: limit == LocalFeatureLimit.Extent ? null : until, spine: spine);
        using var result = plan.Build(); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid); Assert.Equal(limit == LocalFeatureLimit.Extent ? 520 : 512, Mass(result.RequireShape()), 5);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AngularRibClippingPreservesBaseOutsideTheSelectedInterval(bool add)
    {
        using var lower = ShapeFactory.CreateCylinder(2, 5); using var upper = ShapeFactory.CreateCylinder(1, 3);
        using var moved = upper.Transformed(ShapeTransform.CreateTranslation(0, 0, 5));
        using var basis = add ? lower.Fuse(moved) : ShapeFactory.CreateCylinder(2, 8);
        using var wire = ShapeFactory.CreatePolygonWire(add ? [new(-2, 0, 5), new(-1, 0, 8)] : [new(-.8, 0, 9), new(-.8, 0, -1)]);
        RibSlotOptions options = new() { Revolution = true, AddMaterial = add, PlaneOrigin = add ? new(-2, 0, 5) : new(-.8, 0, 9), Thickness1 = .2, Thickness2 = .2 };
        using var wholePlan = RibSlotPlan.Create(basis, wire, options); using var whole = wholePlan.Build();
        using var plan = RibSlotPlan.Create(basis, wire, options with { AngularLimit = new(Math.PI / 2, Math.PI) });
        using var clipped = plan.Build(); Assert.True(clipped.Diagnostics.AlgorithmDone, clipped.Diagnostics.Message); Assert.True(clipped.Diagnostics.ShapeIsValid);
        double baseMass = Mass(basis), wholeChange = Math.Abs(Mass(whole.RequireShape()) - baseMass), clippedChange = Math.Abs(Mass(clipped.RequireShape()) - baseMass);
        Assert.True(clippedChange > 1e-4 && clippedChange < wholeChange - 1e-4, $"Changes whole={wholeChange}, clipped={clippedChange}");
        Assert.True(clipped.Diagnostics.HasComposedHistory); Assert.NotEmpty(clipped.GetGroup(LocalFeatureHistoryKind.PreLimitShape));
        using var lostBase = basis.Cut(clipped.RequireShape());
        if (add) Assert.InRange(Mass(lostBase), -1e-8, 1e-8);
        Assert.DoesNotContain(clipped.History, h => h.Kind is LocalFeatureHistoryKind.Generated or LocalFeatureHistoryKind.Modified);
    }
}
