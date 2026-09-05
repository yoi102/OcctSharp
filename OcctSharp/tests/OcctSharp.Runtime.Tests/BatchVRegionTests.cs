namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchVRegionTests
{
    private static double Volume(Shape shape) => shape.InspectProperties(InspectionPropertyKind.Volume).Mass;
    private static Shape Shift(Shape shape, double x) => shape.Transformed(ShapeTransform.CreateTranslation(x, 0, 0));
    private static RegionProgram Union(string key = "union", int material = 1, bool remove = false, bool containers = false) =>
        new(key, [new(RegionExpression.All, material)], remove, containers);

    [Fact]
    public void FullPartitionPrecedesSelectionAndReportsExactMembershipAndConservation()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = Shift(a, 5);
        using var plan = PartitionPlan.Create([a, b]); using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.IsValid);
        Assert.Equal(3, result.Cells.Count); Assert.Empty(result.OutputKeys);
        Assert.All(result.Cells, c => { Assert.Equal(3, c.Dimension); Assert.Equal(500, c.Measure, 6); Assert.True(c.MembershipKnown); });
        Assert.Single(result.Cells, c => c.InputMembership.All(m => m == RegionMembership.Inside));
        Assert.Equal(2, result.Conservation.Count); Assert.All(result.Conservation, c => { Assert.Equal(1000, c.OriginalMeasure, 6); Assert.Equal(0, c.AbsoluteError, 6); });
        Assert.True(result.EvaluatePrecision().Accepted);
    }
    [Fact]
    public void MultiOutputExpressionProgramsRetainMaterialsAndOrientedSharedInterfaces()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = Shift(a, 5);
        var input0 = RegionExpression.Input(0); var input1 = RegionExpression.Input(1);
        RegionProgram materials = new("materials", [new(input0.Except(input1), 4), new(input1, 8)], true, true);
        using var plan = PartitionPlan.Create([a, b]);
        using var result = plan.Build([materials, Union(), new("intersection", [new(input0.Intersect(input1))])]);
        using var union = result.CopyOutput("union"); using var intersection = result.CopyOutput("intersection");
        using var regions = result.CopyOutput("materials");
        Assert.Equal(1500, Volume(union), 6); Assert.Equal(500, Volume(intersection), 6); Assert.Equal(1500, Volume(regions), 6);
        Assert.Equal(3, result.GetAssignments("materials").Count);
        Assert.Single(result.GetMaterialInterfaces("materials"));
        var face = result.GetMaterialInterfaces("materials")[0];
        Assert.Equal(2, face.Uses.Select(u => u.Cell).Distinct().Count());
        Assert.Equal(2, face.Uses.Select(u => u.Orientation).Distinct().Count());
        Assert.Equal(2, result.GetConnectedRegions("materials").Count);
        Assert.All(result.GetEnvelope("materials"), e => { Assert.NotEmpty(e.InputIndices); Assert.Single(e.Cells); });
        Assert.Contains(result.History, h => h.SourceKind == ShapeKind.Solid && h.Kind == RegionHistoryKind.Unavailable);
    }
    [Fact]
    public void OrderedRemovalClearsAssignmentBeforeReassignmentAndConflictsFailAtomically()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = Shift(a, 5);
        using var plan = PartitionPlan.Create([a, b]);
        RegionProgram rules = new("ordered", [new(RegionExpression.All, 2),
            new(RegionExpression.Input(1), Action: RegionRuleAction.Remove), new(RegionExpression.Input(1), 3)]);
        using var result = plan.Build([rules]); Assert.Equal(7, result.GetRuleEffects("ordered").Count);
        Assert.Equal(2, result.GetAssignments("ordered").Count(x => x.Material == 3));
        Assert.Throws<ArgumentException>(() => plan.Build([Union(), new("conflict", [new(RegionExpression.All, 1), new(RegionExpression.Input(1), 2)])]));
        using var rerun = plan.Build([Union()]); Assert.Equal(3, rerun.Cells.Count);
    }
    [Fact]
    public void CellAndBoundaryIdsRejectForeignRevisionAndPublicCopiesOutliveEveryOwner()
    {
        var a = ShapeFactory.CreateBox(10, 10, 10); var b = Shift(a, 5); var plan = PartitionPlan.Create([a, b]);
        a.Dispose(); b.Dispose(); var first = plan.Build([Union()]); var second = plan.Build([Union()]);
        Assert.NotEqual(first.Revision, second.Revision);
        Assert.Throws<ArgumentException>(() => second.CopyCell(first.Cells[0].Id));
        Assert.Throws<ArgumentException>(() => second.CopyBoundary(first.Boundaries[0].Id));
        using var cell = first.CopyCell(first.Cells[0].Id); using var output = first.CopyOutput("union");
        plan.Dispose(); first.Dispose(); second.Dispose();
        Assert.True(cell.IsValid); Assert.Equal(500, Volume(cell), 6); Assert.Equal(1500, Volume(output), 6);
        Assert.Throws<ObjectDisposedException>(() => plan.Build()); Assert.Throws<ObjectDisposedException>(() => first.CopyCell(first.Cells[0].Id));
    }
    [Fact]
    public void InternalRemovalAndTypedContainersRespectMaterialZero()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = Shift(a, 5); using var plan = PartitionPlan.Create([a, b]);
        using var result = plan.Build([Union("zero", 0, true), Union("merged", 2, true), Union("containers", 0, false, true)]);
        using var zero = result.CopyOutput("zero"); using var merged = result.CopyOutput("merged"); using var containers = result.CopyOutput("containers");
        Assert.Equal(3, zero.GetTopologySummary().UniqueCounts.SolidCount); Assert.Equal(1, merged.GetTopologySummary().UniqueCounts.SolidCount);
        var groups = containers.GetSubShapes(ShapeKind.CompSolid);
        try { Assert.Single(groups); Assert.Equal(1500, Volume(groups[0]), 6); } finally { foreach (var g in groups) g.Dispose(); }
    }
    [Fact]
    public void SmallCellsAreExplicitSelectionsAndGrowthPolicyCanRejectValidPartition()
    {
        using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = Shift(a, .01); using var plan = PartitionPlan.Create([a, b]);
        using var result = plan.Build([new("slivers", [new(RegionExpression.All, MaximumMeasure: 2)])]);
        var selected = result.SelectSlivers(3, 2); Assert.Equal(2, selected.Count);
        using var shape = result.CopyCells(selected); Assert.Equal(2, Volume(shape), 5);
        using var filtered = result.CopyOutput("slivers"); Assert.Equal(2, Volume(filtered), 5);
        Assert.False(result.EvaluatePrecision(new(MaximumCells: 2)).Accepted); Assert.True(result.Diagnostics.IsValid);
    }
    [Fact]
    public void MixedDimensionsKeepTheirMeasuresAndRejectCrossDimensionRemoval()
    {
        using var solid = ShapeFactory.CreateBox(10, 10, 10);
        using var edge = ShapeFactory.CreateEdge(new GpPoint(20, 0, 0), new GpPoint(25, 0, 0));
        using var plan = PartitionPlan.Create([solid, edge]); using var result = plan.Build([Union("mixed", 0)]);
        Assert.Contains(result.Cells, c => c.Dimension == 3 && Math.Abs(c.Measure - 1000) < 1e-6);
        Assert.Contains(result.Cells, c => c.Dimension == 1 && Math.Abs(c.Measure - 5) < 1e-6);
        Assert.Throws<ArgumentException>(() => plan.Build([Union("bad", 1, true)]));
        using var isolated = ShapeFactory.CreateEdge(new(40, 0, 0), new(45, 0, 0));
        var vertices = isolated.GetSubShapes(ShapeKind.Vertex);
        try
        {
            using var mixed = PartitionPlan.Create([solid, edge, vertices[0]]);
            using var parts = mixed.Build([Union("all-dimensions", 0)]);
            Assert.Contains(parts.Cells, c => c.Dimension == 0 && c.Measure == 1);
            Assert.Contains(parts.Cells, c => c.Dimension == 1 && Math.Abs(c.Measure - 5) < 1e-6);
            Assert.Contains(parts.Cells, c => c.Dimension == 3 && Math.Abs(c.Measure - 1000) < 1e-6);
        }
        finally { foreach (var vertex in vertices) vertex.Dispose(); }
    }
    [Fact]
    public void VolumeConstructionBuildsFiniteBoxAndMapsEverySourceFace()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6); var faces = box.GetFaces();
        try
        {
            using var plan = VolumeConstructionPlan.Create(faces); using var result = plan.Build();
            Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.HelperBoxExcluded);
            Assert.Single(result.Volumes); Assert.Equal(120, result.Volumes[0].Volume, 6); Assert.True(result.Volumes[0].IsValid);
            Assert.Equal(6, result.SourceFaces.Select(f => f.InputIndex).Distinct().Count());
            using var shape = result.CopyResult(); Assert.Equal(120, Volume(shape), 6);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }
    [Fact]
    public void OpenVolumeReportsZeroSolidsAndUnresolvedTopologyWithoutInventingBox()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6); var faces = box.GetFaces();
        try
        {
            using var plan = VolumeConstructionPlan.Create(faces.Take(5).ToArray()); using var result = plan.Build();
            Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.Empty(result.Volumes);
            Assert.True(result.HelperBoxExcluded); Assert.NotEmpty(result.UnusedFaceItems); Assert.NotEmpty(result.FreeBoundaryItems);
            using var boundary = result.CopyDiagnosticShape(result.FreeBoundaryItems[0]); Assert.True(boundary.IsValid);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }
    [Fact]
    public void VerifiedFastVolumeModeAcceptsSharedShellAndRejectsIntersectingArguments()
    {
        using var box = ShapeFactory.CreateBox(4, 5, 6); using var other = Shift(box, 2);
        using var eligible = VolumeConstructionPlan.Create([box], new(IntersectInputs: false)); using var result = eligible.Build();
        Assert.Single(result.Volumes); Assert.Equal(120, result.Volumes[0].Volume, 6);
        using var unsafePlan = VolumeConstructionPlan.Create([box, other], new(IntersectInputs: false));
        Assert.Throws<ArgumentException>(() => unsafePlan.Build());
    }
    [Fact]
    public void RegionProgramsRejectUnboundedInvalidOrMissingInputExpressions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RegionExpression.Input(-1));
        Assert.Throws<ArgumentException>(() => new RegionProgram("bad", [new(RegionExpression.All, -1)]));
        using var a = ShapeFactory.CreateBox(1, 1, 1); using var b = Shift(a, .5); using var plan = PartitionPlan.Create([a, b]);
        Assert.Throws<ArgumentException>(() => plan.Build([new("bad", [new(RegionExpression.Input(2))])]));
        Assert.Throws<ArgumentException>(() => plan.Build([Union(), Union()]));
        Assert.Throws<ArgumentException>(() => PartitionPlan.Create([a, b], new(FuzzyTolerance: double.NaN)));
    }
}
