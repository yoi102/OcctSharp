using System.Runtime.InteropServices;
using OcctSharp.Interop;
using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchUFinishingTests
{
    internal static RepairSelection Edge(RepairSnapshot source, int ordinal = 0) => source.Topology.Where(t => t.Kind == ShapeKind.Edge).ElementAt(ordinal).Selection;
    [Fact]
    public void LocalAbiLayoutsAreExplicit()
    {
        Assert.Equal(56, Marshal.SizeOf<LocalFeatureInfoRaw>()); Assert.Equal(56, Marshal.SizeOf<ContourInfoRaw>());
        Assert.Equal(40, Marshal.SizeOf<ContourEdgeRaw>()); Assert.Equal(112, Marshal.SizeOf<FilletSectionRaw>());
        Assert.Equal(16, Marshal.SizeOf<LocalFeatureFaultRaw>()); Assert.Equal(24, Marshal.SizeOf<LocalFeatureHistoryRaw>());
        Assert.Equal(72, Marshal.SizeOf<FilletOptionsRaw>()); Assert.Equal(40, Marshal.SizeOf<FilletProgramRaw>());
        Assert.Equal(96, Marshal.SizeOf<FaceDraftProgramRaw>()); Assert.Equal(72, Marshal.SizeOf<ShellDraftOptionsRaw>());
        Assert.Equal(96, Marshal.SizeOf<LimitedFeatureOptionsRaw>()); Assert.Equal(192, Marshal.SizeOf<RibSlotOptionsRaw>());
        Assert.Equal(88, Marshal.SizeOf<LocalHoleOptionsRaw>());
    }
    [Fact]
    public unsafe void RawSnapshotsValidateEveryCapacityBeforeWritingAndClearFailedOutputs()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        FilletProgramRaw program = new() { Seed = Edge(source).Index, Radius = 1, LawIndex = -1 };
        var options = new ContourFilletOptions().Raw(1);
        Assert.Equal(NativeStatus.Success, NativeMethods.ContourFillet(source.Shape.Handle, &program, 1, null, 0, null, 0, null, 0, in options, out var raw));
        using FeatureResultHandle handle = new(raw);
        Assert.Equal(NativeStatus.Success, NativeMethods.LocalFeatureSnapshot(handle, out var counts, null, 0, null, 0, null, 0, null, 0));
        var contour = new ContourInfoRaw { Index = 0x12345678 };
        var edge = new ContourEdgeRaw { Ordinal = 0x12345678 };
        var section = new FilletSectionRaw { Ordinal = 0x12345678 };
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.LocalFeatureSnapshot(handle, out var empty, &contour, 1, &edge, 1, &section, 0, null, 0));
        Assert.Equal(0x12345678, contour.Index); Assert.Equal(0x12345678, edge.Ordinal); Assert.Equal(0x12345678, section.Ordinal);
        Assert.Equal(0, empty.SectionCount); Assert.True(counts.SectionCount > 0);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.LocalFeatureSnapshot(handle, out _, null, -1, null, 0, null, 0, null, 0));
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.LocalFeatureHistory(handle, -1, out var history, out var shape));
        Assert.Equal(nint.Zero, shape); Assert.Equal(0, history.Kind);
        program.SampleOffset = int.MaxValue; program.SampleCount = 2;
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.ContourFillet(source.Shape.Handle, &program, 1, null, 0, null, 0, null, 0, in options, out raw));
        Assert.Equal(nint.Zero, raw);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.LocalFeatureSourceSubshape(source.Shape.Handle, int.MaxValue, out shape));
        Assert.Equal(nint.Zero, shape);
    }
    [Fact]
    public void ContourDiscoverySimulationAndOwningPatchResults()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var recipe = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(source), 1)]);
        using var discovery = recipe.Discover(source); var contour = Assert.Single(discovery.Contours);
        Assert.Equal(Edge(source), contour.Seed); Assert.Equal(15, contour.Length, 5);
        Assert.NotEmpty(discovery.ContourEdges); Assert.False(discovery.Diagnostics.AlgorithmDone);
        using var simulation = recipe.Simulate(source); Assert.NotEmpty(simulation.SimulatedSections);
        Assert.All(simulation.SimulatedSections, s => Assert.Equal(1, s.Radius, 4));
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid); Assert.NotEmpty(result.GetGroup(LocalFeatureHistoryKind.SurfacePatch));
        Assert.InRange(Mass(result.RequireShape()), 1700, 1800);
        source.Dispose(); box.Dispose(); Assert.True(result.RequireShape().IsValid);
        Assert.All(result.GetGroup(LocalFeatureHistoryKind.SurfacePatch), shape => Assert.True(shape.IsValid));
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NonconstantRadiusProfilesChangeActualGeometry(int mode)
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        var seed = Edge(source);
        var program = mode switch
        {
            0 => FilletContourProgram.FromLaw(seed, ScalarLawDefinition.Linear(new(0, 1), .5, 2)),
            1 => FilletContourProgram.Sampled(seed, [new(0, .5), new(.5, 1.2), new(1, 2)]),
            _ => FilletContourProgram.FromLaw(seed, ScalarLawDefinition.BSpline([.5, 2, .5], [0, 1], [3, 3], 2))
        };
        var recipe = ContourFilletRecipe.Create(source, [program]);
        using var simulation = recipe.Simulate(source);
        Assert.NotEmpty(simulation.SimulatedSections);
        Assert.True(simulation.SimulatedSections.Max(s => s.Radius) - simulation.SimulatedSections.Min(s => s.Radius) > .2);
        using var result = recipe.Build(source); Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid); Assert.InRange(Mass(result.RequireShape()), 1650, 1800);
    }
    [Fact]
    public void RecipesReplayOriginalSourceAndRejectForeignSelections()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box); using var foreign = RepairSnapshot.Create(box);
        var recipe = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(source), 1)]);
        Assert.Throws<ArgumentException>(() => recipe.Build(foreign));
        Assert.Throws<ArgumentException>(() => ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(foreign), 1)]));
        using var first = recipe.Build(source); using var second = recipe.Replace(source, 0, FilletContourProgram.Constant(Edge(source), 2)).Build(source);
        Assert.True(Mass(first.RequireShape()) > Mass(second.RequireShape()));
        using var cleared = recipe.Remove(source, 0).Build(source); Assert.Equal(1800, Mass(cleared.RequireShape()), 7);
        using var same = recipe.Build(source); Assert.Equal(Mass(first.RequireShape()), Mass(same.RequireShape()), 7);
    }
    [Fact]
    public void FailureCannotProduceAnAcceptedRoot()
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        using var result = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(Edge(source), 100)]).Build(source);
        Assert.False(result.Diagnostics.AlgorithmDone); Assert.NotEmpty(result.Faults);
        Assert.Throws<InvalidOperationException>(() => result.RequireShape()); Assert.Null(result.Shape);
        Assert.Equal(1800, Mass(box), 7);
    }
    [Theory]
    [InlineData(LocalHoleMode.BetweenBounds)]
    [InlineData(LocalHoleMode.ThroughNext)]
    [InlineData(LocalHoleMode.UntilEnd)]
    [InlineData(LocalHoleMode.Blind)]
    public void LocalHolesUseNativeBoundedConstruction(LocalHoleMode mode)
    {
        using var box = ShapeFactory.CreateBox(10, 12, 15); using var source = RepairSnapshot.Create(box);
        using var hole = LocalFeatures.Hole(source, new(mode, new(5, 6, -1), new(0, 0, 1), 1, 0, mode == LocalHoleMode.Blind ? 6 : 17));
        Assert.True(hole.Diagnostics.AlgorithmDone, hole.Diagnostics.Message); Assert.True(hole.Diagnostics.ShapeIsValid);
        Assert.Equal(1800 - Math.PI * (mode == LocalHoleMode.Blind ? 5 : 15), Mass(hole.RequireShape()), 5);
    }
}
