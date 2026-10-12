using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class AuthoringHistoryDomainTests
{
    private static readonly bool[] Switches = [false, true];
    private static readonly GuidedSweepFrame[] Frames = [GuidedSweepFrame.CorrectedFrenet, GuidedSweepFrame.FixedFrame,
        GuidedSweepFrame.FixedBinormal, GuidedSweepFrame.Discrete];
    public static IEnumerable<object[]> Sweeps() => from frame in Frames from solid in Switches from located in Switches
        select new object[] { frame, solid, located };
    public static IEnumerable<object[]> Preparations() => from spline in Switches from reverse in Switches
        from located in Switches from negative in Switches select new object[] { spline, reverse, located, negative };
    public static IEnumerable<object[]> Lofts() => from compatibility in Switches from solid in Switches from located in Switches
        select new object[] { compatibility, solid, located };

    [Fact]
    public void IndependentDomainLayoutsKeepOldHistoryAndCopyLayoutsUnchanged()
    {
        Assert.Equal(16, Marshal.SizeOf<AuthoringHistoryDomainInfoRaw>());
        Assert.Equal(48, Marshal.SizeOf<AuthoringHistoryDomainRowRaw>());
        Assert.Equal(16, Marshal.SizeOf<AuthoringHistoryRaw>());
        Assert.Equal(32, Marshal.SizeOf<AuthoringCopyRowRaw>());
        Assert.Equal(5, (int)AuthoringHistoryKind.Unmapped); Assert.Equal(7, (int)AuthoringHistoryKind.InputSnapshot);
    }

    [Theory, MemberData(nameof(Sweeps))]
    public void ActualSweepReportsSourceCopyMembershipAndFinalTargetsAfterSolidification(
        GuidedSweepFrame frame, bool solid, bool located)
    {
        using var rawSpine = BatchSAuthoringTests.Spine(); using var rawSection = BatchSAuthoringTests.Square();
        using var transform = Transform(located);
        using var spine = rawSpine.Transformed(transform); using var section = rawSection.Transformed(transform);
        byte[] spineBefore = Bytes(spine), sectionBefore = Bytes(section);
        using var plan = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = frame,
            FrameOrigin = located ? new(11, -7, 3) : new(0, 0, 0),
            FrameDirection = frame == GuidedSweepFrame.FixedBinormal ? new(0, 1, 0) : new(0, 0, 1),
            SolidPolicy = solid ? SweepSolidPolicy.RequireSolid : SweepSolidPolicy.Shell });
        using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
        Assert.Equal(solid, result.Diagnostics.IsSolid);
        if (solid) Assert.InRange(Math.Abs(result.RequireShape().InspectProperties(InspectionPropertyKind.Volume).Mass - 40), 0, 1e-7);
        Assert.Equal(spineBefore, Bytes(spine)); Assert.Equal(sectionBefore, Bytes(section));
        var domains = Verify(result);
        Assert.Contains(domains.Rows, row => row.Kind == AuthoringHistoryKind.Generated && row.HasUniqueNativeCopySourceMembership && row.IsNativeMemberOfFinalResult);
        Assert.All(domains.Rows.Where(row => result.History[row.HistoryIndex].Source.HasValue), row => Assert.True(row.HasUniqueNativeCopySourceMembership));
        Assert.All(domains.Rows.Where(row => row.Kind is AuthoringHistoryKind.FirstSection or AuthoringHistoryKind.LastSection), row =>
        { Assert.True(row.SourceIssues.HasFlag(AuthoringHistorySourceIssue.NoSourceIdentity)); Assert.False(row.HasUniqueNativeCopySourceMembership); });
        var values = domains.Rows.ToArray(); var copyId = result.InputCopy!.TargetDomainId;
        spine.Dispose(); section.Dispose(); plan.Dispose(); result.Dispose();
        Assert.Equal(values, domains.Rows.ToArray()); Assert.Equal(copyId, domains.InputDomainId);
    }

    [Theory, MemberData(nameof(Preparations))]
    public void ActualOffsetPreparationRetainsSeparateSnapshotAndProvesOnlyActualSourceAndFinalMembership(
        bool spline, bool reverse, bool located, bool negative)
    {
        var primitive = spline ? SketchCurve2d.BSpline([new(-.5, 0), new(-1d / 6, 2d / 3), new(1d / 6, 2d / 3), new(.5, 0)],
            [0, 1], [4, 4], 3) : SketchCurve2d.Bezier([new(-.5, 0), new(0, 1), new(.5, 0)]);
        var expression = SketchCurveExpression2d.FromCurve(primitive).Offset(negative ? -.03 : .03).Trim(.1, .9);
        if (reverse) expression = expression.Reverse();
        using var raw = SketchExpressionChain2d.CreateOrdered([expression]).BuildWire(SketchPlane.XY);
        using var transform = Transform(located); using var source = raw.Transformed(transform);
        byte[] before = Bytes(source);
        using var prepared = GuidedSweepPlan.PrepareOffsetCurves(source);
        Assert.True(prepared.Diagnostics.ShapeIsValid, prepared.Diagnostics.Message);
        Assert.InRange(prepared.Diagnostics.ApproximationError!.Value, 0, 1e-10);
        Assert.Equal(before, Bytes(source));
        var domains = Verify(prepared);
        Assert.All(domains.Rows.Where(row => row.Kind == AuthoringHistoryKind.Modified), row =>
        { Assert.True(row.HasUniqueNativeCopySourceMembership); Assert.True(row.IsNativeMemberOfFinalResult); });
        var snapshot = Assert.Single(domains.Rows, row => row.Kind == AuthoringHistoryKind.InputSnapshot);
        Assert.Equal(AuthoringHistorySourceIssue.NoSourceIdentity, snapshot.SourceIssues);
        Assert.Equal(AuthoringHistoryTargetIssue.OutsideFinalResult, snapshot.TargetIssues);
        Assert.Equal(-1, snapshot.FinalTargetTopologyIndex); Assert.False(snapshot.IsNativeMemberOfFinalResult);
        source.Dispose(); prepared.Dispose(); Assert.NotEmpty(domains.Rows);
    }

    [Theory, MemberData(nameof(Lofts))]
    public void ActualCompatibleLoftTargetsAreCheckedOnTheFinalResultAndSourceIndicesAreNotAssumed(
        bool compatibility, bool solid, bool located)
    {
        using var rawA = BatchSAuthoringTests.Square(); using var rawB = BatchSAuthoringTests.Square(5, 3);
        using var rawC = BatchSAuthoringTests.Square(10, 2); using var transform = Transform(located);
        using var a = rawA.Transformed(transform); using var b = rawB.Transformed(transform); using var d = rawC.Transformed(transform);
        byte[] beforeA = Bytes(a), beforeB = Bytes(b), beforeC = Bytes(d);
        using var result = GuidedLoft.Build([a, b, d], new() { Solid = solid, CorrectCompatibility = compatibility });
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
        Assert.Equal(solid, result.Diagnostics.IsSolid);
        Assert.Equal(beforeA, Bytes(a)); Assert.Equal(beforeB, Bytes(b)); Assert.Equal(beforeC, Bytes(d));
        var domains = Verify(result);
        Assert.Contains(domains.Rows, row => row.Kind == AuthoringHistoryKind.Generated && row.HasUniqueNativeCopySourceMembership && row.IsNativeMemberOfFinalResult);
        Assert.Equal(3, domains.Rows.Count(row => row.Kind == AuthoringHistoryKind.CompatibleSection));
        Assert.All(domains.Rows.Where(row => row.Kind == AuthoringHistoryKind.CompatibleSection), row =>
        { Assert.True(row.HasUniqueNativeCopySourceMembership); Assert.Equal(ShapeKind.Wire, row.SourceKind); });
        a.Dispose(); b.Dispose(); d.Dispose(); result.Dispose(); Assert.NotEmpty(domains.Rows);
    }

    [Fact]
    public void PreflightAndSimulationKeepNoFinalResultAndMissingSourceDiagnosticsInsteadOfInventingTargets()
    {
        using var spine = BatchSAuthoringTests.Spine(); using var section = BatchSAuthoringTests.Square();
        using var plan = GuidedSweepPlan.Create(spine, [new(section)]);
        Assert.True(plan.Preflight().Ready);
        using var simulation = plan.Simulate(3);
        Assert.True(simulation.Diagnostics.AlgorithmDone, simulation.Diagnostics.Message);
        Assert.Null(simulation.Shape);
        var domains = Verify(simulation); Assert.Equal(0, domains.FinalTopologyCount);
        Assert.Equal(3, domains.Rows.Count);
        Assert.All(domains.Rows, row =>
        {
            Assert.Equal(AuthoringHistorySourceIssue.NoSourceIdentity, row.SourceIssues);
            Assert.Equal(AuthoringHistoryTargetIssue.NoFinalResult, row.TargetIssues);
            Assert.Equal(-1, row.CopiedSourceTopologyIndex); Assert.Equal(-1, row.FinalTargetTopologyIndex);
            Assert.False(row.HasUniqueNativeCopySourceMembership); Assert.False(row.IsNativeMemberOfFinalResult);
        });
    }

    [Fact]
    public void RepeatedSectionArgumentsRetainMultipleNativeMembershipsAndNeverClaimUniqueSource()
    {
        using var spine = BatchSAuthoringTests.Spine(); using var section = BatchSAuthoringTests.Square();
        using var plan = GuidedSweepPlan.Create(spine, [new(section), new(section)], new() { SolidPolicy = SweepSolidPolicy.Shell });
        using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        var domains = Verify(result);
        var shared = domains.Rows.Where(row => row.SourceArgumentIndex is 1 or 2 &&
            !row.SourceIssues.HasFlag(AuthoringHistorySourceIssue.NoSourceIdentity)).ToArray();
        Assert.NotEmpty(shared);
        Assert.All(shared, row =>
        {
            Assert.Equal(2, row.SourceArgumentMemberships);
            Assert.True(row.SourceIssues.HasFlag(AuthoringHistorySourceIssue.MultipleArguments));
            Assert.False(row.HasUniqueNativeCopySourceMembership);
        });
    }

    [Fact]
    public unsafe void AbsentDomainAndOutOfRangeRowsRejectWithoutManufacturingAReportOrChangingInputs()
    {
        using var wire = BatchSAuthoringTests.Square(); byte[] before = Bytes(wire);
        AuthoringBridge.WithInputs([wire], (pointers, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.AuthoringCopyInputs(pointers, count, out nint raw), "domain_test_copy");
            using FeatureResultHandle result = new(raw);
            NativeError.ThrowIfFailed(NativeMethods.AuthoringHistoryDomainInfo(result, out var info), "domain_test_absent_info");
            Assert.Equal(default, info);
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringHistoryDomainRow(result, 0, out var row));
            Assert.Equal(default, row);
            return 0;
        });
        Assert.Equal(before, Bytes(wire));
    }

    private static AuthoringHistoryDomainReport Verify(AuthoringResult result)
    {
        var copy = Assert.IsType<AuthoringInputCopyReport>(result.InputCopy);
        var domains = Assert.IsType<AuthoringHistoryDomainReport>(result.HistoryDomains);
        Assert.Equal(1, domains.SchemaVersion); Assert.Equal(copy.TargetDomainId, domains.InputDomainId);
        Assert.NotEqual(domains.InputDomainId, domains.FinalDomainId);
        Assert.False(domains.ProvesCompleteOccurrencePaths); Assert.False(domains.ProvesComposedReferenceIdentity);
        Assert.Equal(result.History.Count, domains.Rows.Count);
        Assert.Equal(Enumerable.Range(0, result.History.Count), domains.Rows.Select(row => row.HistoryIndex));
        Assert.Equal(result.Shape is null ? 0 : TopologyCount(result.Shape), domains.FinalTopologyCount);
        foreach (var row in domains.Rows)
        {
            var item = result.History[row.HistoryIndex]; Assert.Equal(item.Kind, row.Kind);
            if (item.Source is { } source)
            {
                Assert.Equal(source.ArgumentIndex, row.SourceArgumentIndex);
                Assert.Equal(source.TopologyIndex, row.LegacySourceTopologyIndex); Assert.Equal(source.Kind, row.SourceKind);
            }
            if (row.HasUniqueNativeCopySourceMembership)
            {
                Assert.Contains(copy.Rows, mapping => mapping.ArgumentIndex == row.SourceArgumentIndex &&
                    mapping.TargetTopologyIndex == row.CopiedSourceTopologyIndex && mapping.TargetKind == row.SourceKind);
                Assert.Equal(row.CopiedSourceTopologyIndex != row.LegacySourceTopologyIndex,
                    row.SourceIssues.HasFlag(AuthoringHistorySourceIssue.LegacyIndexDiffers));
            }
            int index = result.Shape is not null && item.Shape is not null ? RepairSnapshot.FindTopologyIndex(result.Shape, item.Shape) : -1;
            Assert.Equal(index, row.FinalTargetTopologyIndex);
            Assert.Equal(index >= 0, row.IsNativeMemberOfFinalResult);
            if (item.Shape is null) Assert.True(row.TargetIssues.HasFlag(AuthoringHistoryTargetIssue.NoShape));
            if (result.Shape is null) Assert.True(row.TargetIssues.HasFlag(AuthoringHistoryTargetIssue.NoFinalResult));
            if (result.Shape is not null && item.Shape is not null && index < 0)
                Assert.True(row.TargetIssues.HasFlag(AuthoringHistoryTargetIssue.OutsideFinalResult));
        }
        return domains;
    }
    private static GpTrsf Transform(bool located) => GpTrsf.Create(located ? 11 : 0, located ? -7 : 0, located ? 3 : 0,
        0, 0, 1, located ? .7 : 0);
    private static unsafe int TopologyCount(Shape shape)
    { NativeError.ThrowIfFailed(NativeMethods.RepairTopology(shape.Handle, null, 0, out int count), "domain_test_topology"); return count; }
    private static unsafe byte[] Bytes(Shape shape)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count), "domain_test_bytes_count");
        byte[] bytes = new byte[count];
        fixed (byte* data = bytes) NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, data, bytes.Length, out _), "domain_test_bytes");
        return bytes;
    }
}
