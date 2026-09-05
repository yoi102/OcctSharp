using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchUFinishingTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchUProgramTests
{
    [Fact]
    public void TangentChainLawUsesTheWholeOrderedContourDomain()
    {
        using var lower = ShapeFactory.CreateBox(10, 12, 7.5); using var upper = lower.Transformed(ShapeTransform.CreateTranslation(0, 0, 7.5));
        using var joined = lower.Fuse(upper); using var source = RepairSnapshot.Create(joined);
        var edge = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.X) < 1e-5 && Math.Abs(b.Maximum.Y) < 1e-5);
        var recipe = ContourFilletRecipe.Create(source, [FilletContourProgram.FromLaw(edge, ScalarLawDefinition.Linear(new(0, 1), .5, 1.5))]);
        using var discovery = recipe.Discover(source); var contour = Assert.Single(discovery.Contours);
        Assert.Equal(15, contour.Length, 5); Assert.True(discovery.ContourEdges.Count >= 2);
        using var start = source.CopySubshape(contour.FirstVertex!.Value); double startZ = start.GetBoundingBox().Minimum.Z;
        using var end = source.CopySubshape(contour.LastVertex!.Value); double endZ = end.GetBoundingBox().Minimum.Z;
        using var simulation = recipe.Simulate(source); Assert.NotEmpty(simulation.SimulatedSections);
        Assert.All(simulation.SimulatedSections, s => Assert.InRange(s.Radius - (.5 + (s.Center.Z - startZ) / (endZ - startZ)), -.001, .001));
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.InRange(Mass(result.RequireShape()), 1790, 1800);
        var second = discovery.ContourEdges[1].Edge;
        Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source,
            [FilletContourProgram.Constant(edge, 1), FilletContourProgram.Constant(second, 1)]).Build(source));
    }

    [Fact]
    public void ClosedCircularContourHasExplicitSeamCorrespondence()
    {
        using var cylinder = ShapeFactory.CreateCylinder(10, 15); using var source = RepairSnapshot.Create(cylinder);
        var edge = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.Z) < 1e-5);
        var recipe = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(edge, 1)]);
        using var discovery = recipe.Discover(source); var contour = Assert.Single(discovery.Contours);
        Assert.True(contour.IsClosed); Assert.Equal(20 * Math.PI, contour.Length, 5);
        Assert.Equal(1, discovery.ContourEdges[^1].LastParameter);
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.InRange(Mass(result.RequireShape()), 4650, 1500 * Math.PI);
    }

    internal static RepairSelection Select(RepairSnapshot source, ShapeKind kind, Func<BoundingBox3d, bool> predicate)
    {
        foreach (var item in source.Topology.Where(t => t.Kind == kind))
        {
            using var shape = source.CopySubshape(item.Selection);
            if (predicate(shape.GetBoundingBox())) return item.Selection;
        }
        throw new InvalidOperationException("Fixture topology not found.");
    }

    [Fact]
    public void ClosedLawsRejectValueAndDerivativeSeamsBeforeKernelBuild()
    {
        using var cylinder = ShapeFactory.CreateCylinder(10, 15); using var source = RepairSnapshot.Create(cylinder);
        var seed = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.Z) < 1e-5);
        foreach (var law in new[] { ScalarLawDefinition.Linear(new(0, 1), 1, 2), ScalarLawDefinition.BSpline([1, 2, 1], [0, 1], [3, 3], 2) })
            Assert.Contains("seam", Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source,
                [FilletContourProgram.FromLaw(seed, law)]).Build(source)).Message, StringComparison.OrdinalIgnoreCase);
        var program = FilletContourProgram.Sampled(seed, [new(0, 1), new(.5, 1.2), new(1, 1)]);
        using var result = ContourFilletRecipe.Create(source, [program]).Build(source);
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.InRange(Mass(result.RequireShape()), 4650, 1500 * Math.PI);
    }

    [Fact]
    public void LawAnchorsAreConsistencyConstraintsAndSharedJunctionsCannotConflict()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var seed = Edge(source);
        using var discovery = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(seed, 1)]).Discover(source);
        var contour = Assert.Single(discovery.Contours); var vertex = contour.FirstVertex!.Value;
        var law = FilletContourProgram.FromLaw(seed, ScalarLawDefinition.Linear(new(0, 1), .5, 1.5));
        var anchored = law.WithVertexRadii([new(vertex, .5), new(contour.LastVertex!.Value, 1.5)]);
        using var simulation = ContourFilletRecipe.Create(source, [anchored]).Simulate(source);
        Assert.True(simulation.SimulatedSections.Max(s => s.Radius) - simulation.SimulatedSections.Min(s => s.Radius) > .9);
        using var built = ContourFilletRecipe.Create(source, [anchored]).Build(source);
        Assert.True(built.Diagnostics.AlgorithmDone, built.Diagnostics.Message); Assert.True(built.Diagnostics.ShapeIsValid);
        Assert.Contains("conflicts", Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source,
            [law.WithVertexRadii([new(vertex, 2)])]).Build(source)).Message, StringComparison.OrdinalIgnoreCase);
        RepairSelection adjacent = default;
        foreach (var candidate in source.Topology.Where(t => t.Kind == ShapeKind.Edge && t.Selection != seed))
        {
            using var other = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(candidate.Selection, 1)]).Discover(source);
            if (other.Contours.Any(c => c.FirstVertex == vertex || c.LastVertex == vertex)) { adjacent = candidate.Selection; break; }
        }
        Assert.NotEqual(default, adjacent);
        Assert.Contains("junction", Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source,
            [FilletContourProgram.Constant(seed, 1).WithVertexRadii([new(vertex, 1)]),
             FilletContourProgram.Constant(adjacent, 2).WithVertexRadii([new(vertex, 2)])]).Build(source)).Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(FilletRepresentation.Rational, FilletContinuity.C0)]
    [InlineData(FilletRepresentation.Rational, FilletContinuity.C1)]
    [InlineData(FilletRepresentation.Rational, FilletContinuity.C2)]
    [InlineData(FilletRepresentation.QuasiAngular, FilletContinuity.C0)]
    [InlineData(FilletRepresentation.QuasiAngular, FilletContinuity.C1)]
    [InlineData(FilletRepresentation.QuasiAngular, FilletContinuity.C2)]
    [InlineData(FilletRepresentation.Polynomial, FilletContinuity.C0)]
    [InlineData(FilletRepresentation.Polynomial, FilletContinuity.C1)]
    [InlineData(FilletRepresentation.Polynomial, FilletContinuity.C2)]
    public void RepresentationAndContinuityHaveIndependentNumericValidation(FilletRepresentation representation, FilletContinuity continuity)
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        using var result = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(source), 1)],
            new() { Representation = representation, Continuity = continuity }).Build(source);
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        double expected = 1800 - 15 * (1 - Math.PI / 4);
        Assert.InRange(Mass(result.RequireShape()), expected - .01, expected + .01);
    }

    [Fact]
    public void IndependentContoursAndVertexProgramsRejectAmbiguousAssignments()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var edge = Edge(source);
        using var discovery = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(edge, 1)]).Discover(source);
        var contour = Assert.Single(discovery.Contours);
        Assert.NotNull(contour.FirstVertex); Assert.NotNull(contour.LastVertex);
        var constrained = FilletContourProgram.Constant(edge, 1).WithVertexRadii([new(contour.FirstVertex!.Value, .5), new(contour.LastVertex!.Value, 1.5)]);
        using var simulation = ContourFilletRecipe.Create(source, [constrained]).Simulate(source);
        Assert.True(simulation.SimulatedSections.Max(s => s.Radius) - simulation.SimulatedSections.Min(s => s.Radius) > .5);
        using var built = ContourFilletRecipe.Create(source, [constrained]).Build(source);
        Assert.True(built.Diagnostics.AlgorithmDone, built.Diagnostics.Message); Assert.True(built.Diagnostics.ShapeIsValid);
        Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source,
            [FilletContourProgram.Constant(edge, 1), FilletContourProgram.Constant(edge, 2)]).Build(source));
        var opposite = Select(source, ShapeKind.Edge, b => b.Minimum.X > 9.99 && b.Minimum.Y > 11.99 && b.Maximum.Z - b.Minimum.Z > 14.99);
        using var both = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(edge, 1), FilletContourProgram.Constant(opposite, 2)]).Build(source);
        Assert.True(both.Diagnostics.AlgorithmDone, both.Diagnostics.Message); Assert.Equal(2, both.Contours.Count);
        Assert.Equal(1800 - 15 * (1 - Math.PI / 4) * 5, Mass(both.RequireShape()), 4);
    }

    [Theory]
    [InlineData(ContourChamferMode.Classic, ChamferDimensions.DistanceAngle, 1, .5235987755982988)]
    [InlineData(ContourChamferMode.ConstantThroat, ChamferDimensions.Symmetric, 1, 0)]
    [InlineData(ContourChamferMode.ConstantThroatPenetration, ChamferDimensions.TwoDistances, .5, 1)]
    public void ChamferModesMeasureTheActualSupportingSection(ContourChamferMode mode, ChamferDimensions dimensions, double first, double second)
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var edge = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.X) < 1e-5 && Math.Abs(b.Maximum.Y) < 1e-5);
        var support = Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var recipe = ContourChamferRecipe.Create(source, [new(edge, support, dimensions, first, second)], mode);
        using var discovery = recipe.Discover(source); Assert.Single(discovery.Contours); Assert.NotEmpty(discovery.ContourEdges);
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid);
        var patches = result.History.Where(h => h.Kind == LocalFeatureHistoryKind.Generated && h.Shape?.Kind == ShapeKind.Face).Select(h => h.Shape!).ToArray();
        Assert.NotEmpty(patches);
        var bounds = patches[0].GetBoundingBox(); double x = bounds.Maximum.X, y = bounds.Maximum.Y;
        Assert.True(x > .01 && y > .01); double sectionArea = x * y / 2;
        Assert.Equal(1800 - 15 * sectionArea, Mass(result.RequireShape()), 4);
        if (mode == ContourChamferMode.ConstantThroat) Assert.InRange(x * y / Math.Sqrt(x * x + y * y), .99999, 1.00001);
        if (dimensions == ChamferDimensions.DistanceAngle)
        { Assert.InRange(y, first - 1e-5, first + 1e-5); Assert.InRange(x / y, Math.Tan(second) - 1e-5, Math.Tan(second) + 1e-5); }
        if (mode == ContourChamferMode.ConstantThroatPenetration)
        {
            // For perpendicular supports OCCT's penetration construction has
            // setbacks b*b/a-a and sqrt(b*b-a*a), not the supplied distances.
            double a = Math.Min(first, second), b = Math.Max(first, second);
            Assert.InRange(x, b * b / a - a - 1e-5, b * b / a - a + 1e-5);
            Assert.InRange(y, Math.Sqrt(b * b - a * a) - 1e-5, Math.Sqrt(b * b - a * a) + 1e-5);
        }
    }

    [Fact]
    public void PerFaceDraftPreflightAndIndependentAnglesPreserveTheOriginal()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var left = Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var right = Select(source, ShapeKind.Face, b => b.Minimum.X > 9.99);
        var program = new FaceDraftProgram(left, .05, new(0, 0, 1), new(0, 0, 0), new(0, 0, 1), DraftPropagation.SelectedFaceOnly);
        var recipe = FaceDraftRecipe.Create(source, [program, program with { Face = right, Angle = .1 }]);
        using var preflight = recipe.Preflight(source); Assert.True(preflight.Diagnostics.Ready, preflight.Diagnostics.Message);
        Assert.Equal(2, preflight.GetGroup(LocalFeatureHistoryKind.AffectedFace).Count);
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.True(Math.Abs(Mass(result.RequireShape()) - 1800) > 10); Assert.Equal(1800, Mass(box), 6);
        Assert.Throws<ArgumentException>(() => FaceDraftRecipe.Create(source, [program, program]).Build(source));
        Assert.Throws<ArgumentOutOfRangeException>(() => FaceDraftRecipe.Create(source, [program with { Angle = 1e-5 }]));
    }

    [Fact]
    public void ProtectedAcceptanceEnforcesExactHistoryAndBudgetsBeforeSingleConsumption()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        using var result = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(source), 1)]).Build(source);
        var protectedFace = Select(source, ShapeKind.Face, b => b.Minimum.X > 9.99);
        using var accepted = LocalFeatureAcceptance.Inspect(source, result, protectedTopology: [protectedFace]);
        Assert.True(accepted.CanAccept, string.Join(",", accepted.Checks));
        using var copy = accepted.Accept(); Assert.False(accepted.CanAccept); Assert.Throws<InvalidOperationException>(() => accepted.Accept());
        using var rejected = LocalFeatureAcceptance.Inspect(source, result, new(MaximumRelativeVolumeChange: 0));
        Assert.False(rejected.CanAccept); Assert.Throws<InvalidOperationException>(() => rejected.Accept());
        using var changed = LocalFeatureAcceptance.Inspect(source, result, protectedTopology: [Edge(source)]);
        Assert.False(changed.CanAccept);
        using var foreign = RepairSnapshot.Create(box); Assert.Throws<ArgumentException>(() => LocalFeatureAcceptance.Inspect(foreign, result));
        result.Dispose(); source.Dispose(); Assert.True(copy.IsValid);
    }

    [Fact]
    public void IndependentChamferContoursKeepTheirOwnDimensions()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var first = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.X) < 1e-5 && Math.Abs(b.Maximum.Y) < 1e-5);
        var second = Select(source, ShapeKind.Edge, b => b.Minimum.X > 9.99 && b.Minimum.Y > 11.99);
        var left = Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var right = Select(source, ShapeKind.Face, b => b.Minimum.X > 9.99);
        var recipe = ContourChamferRecipe.Create(source,
            [new(first, left, ChamferDimensions.Symmetric, 1), new(second, right, ChamferDimensions.Symmetric, 2)]);
        using var result = recipe.Build(source);
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.Equal(2, result.Contours.Count); Assert.Equal(1800 - 15 * (1 + 4) / 2.0, Mass(result.RequireShape()), 5);
        Assert.All(result.Contours, c => Assert.Contains(result.ContourEdges, e => e.ContourIndex == c.Index));
    }

    [Fact]
    public void TangentDraftReportsTheEffectiveChainAndRejectsSelectedFaceOnly()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var original = RepairSnapshot.Create(box);
        var vertical = original.Topology.Where(t => t.Kind == ShapeKind.Edge).Where(t =>
        { using var edge = original.CopySubshape(t.Selection); return edge.GetBoundingBox().Maximum.Z - edge.GetBoundingBox().Minimum.Z > 14.99; });
        using var rounded = ContourFilletRecipe.Create(original, vertical.Select(t => FilletContourProgram.Constant(t.Selection, 1))).Build(original);
        using var source = RepairSnapshot.Create(rounded.RequireShape());
        var face = Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var program = new FaceDraftProgram(face, .05, new(0, 0, 1), new(0, 0, 0), new(0, 0, 1));
        using var preflight = FaceDraftRecipe.Create(source, [program]).Preflight(source);
        Assert.True(preflight.Diagnostics.Ready, preflight.Diagnostics.Message);
        Assert.True(preflight.GetGroup(LocalFeatureHistoryKind.AffectedFace).Count >= 2);
        using var result = FaceDraftRecipe.Create(source, [program]).Build(source);
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.InRange(Math.Abs(Mass(result.RequireShape()) - 1800), 1, 500);
        Assert.Throws<ArgumentException>(() => FaceDraftRecipe.Create(source, [program with { Propagation = DraftPropagation.SelectedFaceOnly }]).Build(source));
    }

    [Fact]
    public void DraftFailurePreservesTheActualProblemFace()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var top = Select(source, ShapeKind.Face, b => b.Minimum.Z > 14.99);
        using var result = FaceDraftRecipe.Create(source, [new(top, .2, new(0, 0, 1), new(0, 0, 0), new(0, 0, 1))]).Build(source);
        Assert.False(result.Diagnostics.AlgorithmDone); Assert.Null(result.Shape); Assert.NotEmpty(result.Faults);
        Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.ProblemShape));
        Assert.Contains(result.History, h => h.Kind == LocalFeatureHistoryKind.ProblemShape && h.Source?.TopologyIndex == top.Index);
        Assert.Equal(1800, Mass(box), 6);
    }
}
