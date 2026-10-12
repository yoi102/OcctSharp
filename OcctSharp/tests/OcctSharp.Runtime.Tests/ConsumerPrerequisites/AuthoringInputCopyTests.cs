using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class AuthoringInputCopyTests
{
    [Fact]
    public void CopyReportHasIndependentVersionedLayoutsAndKeepsOldHistoryLayout()
    {
        Assert.Equal(16, Marshal.SizeOf<AuthoringCopyInfoRaw>());
        Assert.Equal(16, Marshal.SizeOf<AuthoringCopyArgumentRaw>());
        Assert.Equal(32, Marshal.SizeOf<AuthoringCopyRowRaw>());
        Assert.Equal(16, Marshal.SizeOf<AuthoringHistoryRaw>());
        Assert.Equal(7, (int)AuthoringHistoryKind.InputSnapshot);
        Assert.Equal(9, (int)AuthoringHistoryKind.SurfacePatch);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualBoxCopyRowsAddressNativeMembersAndNeverReuseOriginalIdentity(bool located)
    {
        using var box = ShapeFactory.CreateBox(1, 2, 3);
        using var transform = GpTrsf.Create(located ? 13 : 0, located ? -7 : 0, located ? 5 : 0,
            1, 2, 3, located ? .7 : 0);
        using var source = box.Transformed(transform);
        byte[] before = Bytes(source);
        using var copy = AuthoringInputCopy.Create([source]);
        Assert.Equal(before, Bytes(source));
        var report = copy.Report;
        Assert.Equal(1, report.SchemaVersion); Assert.NotEqual(Guid.Empty, report.SourceDomainId);
        Assert.NotEqual(report.SourceDomainId, report.TargetDomainId);
        Assert.False(report.ProvesCompleteOccurrencePaths); Assert.False(report.ProvesFinalResultIdentity);
        VerifyDomain(report, [source], copy.Inputs);
        Assert.All(report.Rows, row => Assert.True(row.HasUniqueArgumentMapping));
        Assert.Equal(6, copy.Inputs[0].InspectProperties(InspectionPropertyKind.Volume).Mass, 10);
        var rows = report.Rows.ToArray();
        source.Dispose(); copy.Dispose();
        Assert.Equal(rows, report.Rows.ToArray());
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void RepeatedArgumentsExposeAllMembershipsAndRefuseUniqueArgumentMapping(int count)
    {
        using var wire = BatchSAuthoringTests.Square();
        Shape[] sources = Enumerable.Repeat(wire, count).ToArray();
        byte[] before = Bytes(wire);
        using var copy = AuthoringInputCopy.Create(sources);
        Assert.Equal(before, Bytes(wire));
        VerifyDomain(copy.Report, sources, copy.Inputs);
        Assert.All(copy.Report.Rows, row =>
        {
            Assert.Equal(count, row.SourceArgumentMemberships); Assert.Equal(count, row.TargetArgumentMemberships);
            Assert.Equal(AuthoringInputCopyIssue.MultipleSourceArguments | AuthoringInputCopyIssue.MultipleTargetArguments, row.Issues);
            Assert.False(row.HasUniqueArgumentMapping);
        });
    }

    [Fact]
    public void SharedWireAndActualEdgeArgumentAreAmbiguousOnlyOnTheirSharedNativeMembers()
    {
        using var wire = BatchSAuthoringTests.Square();
        Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        try
        {
            using var copy = AuthoringInputCopy.Create([wire, edges[0]]);
            VerifyDomain(copy.Report, [wire, edges[0]], copy.Inputs);
            Assert.Contains(copy.Report.Rows, row => row.ArgumentIndex == 0 && row.HasUniqueArgumentMapping);
            Assert.All(copy.Report.Rows.Where(row => row.ArgumentIndex == 1), row =>
            {
                Assert.Equal(2, row.SourceArgumentMemberships); Assert.Equal(2, row.TargetArgumentMemberships);
                Assert.False(row.HasUniqueArgumentMapping);
            });
            Assert.Contains(copy.Report.Rows, row => row.ArgumentIndex == 0 && row.SourceKind == ShapeKind.Edge &&
                row.SourceArgumentMemberships == 2);
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Fact]
    public void CoincidentIndependentShapesDoNotAcquireSharedIdentityFromCoordinatesOrCounts()
    {
        using var first = BatchSAuthoringTests.Square(); using var second = BatchSAuthoringTests.Square();
        using var copy = AuthoringInputCopy.Create([first, second]);
        VerifyDomain(copy.Report, [first, second], copy.Inputs);
        Assert.All(copy.Report.Rows, row =>
        { Assert.Equal(1, row.SourceArgumentMemberships); Assert.Equal(1, row.TargetArgumentMemberships); Assert.True(row.HasUniqueArgumentMapping); });
        Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(copy.Inputs[0], copy.Inputs[1]));
    }

    [Theory]
    [InlineData(GuidedSweepFrame.CorrectedFrenet)]
    [InlineData(GuidedSweepFrame.FixedFrame)]
    public void PlanCaptureAndEachActualRunHaveSeparateDomainsWithoutInventingHistoryAddressProof(GuidedSweepFrame frame)
    {
        using var spine = BatchSAuthoringTests.Spine(); using var section = BatchSAuthoringTests.Square();
        byte[] spineBefore = Bytes(spine), sectionBefore = Bytes(section);
        using var plan = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = frame, SolidPolicy = SweepSolidPolicy.RequireSolid });
        AuthoringInputCopyReport capture = plan.InputCopy;
        Assert.Equal(2, capture.Arguments.Count);
        Assert.Equal(spineBefore, Bytes(spine)); Assert.Equal(sectionBefore, Bytes(section));
        spine.Dispose(); section.Dispose();
        using var preview = plan.Simulate(3); using var built = plan.Build();
        Assert.True(preview.Diagnostics.AlgorithmDone, preview.Diagnostics.Message);
        Assert.True(built.Diagnostics.ShapeIsValid, built.Diagnostics.Message);
        var previewCopy = Assert.IsType<AuthoringInputCopyReport>(preview.InputCopy);
        var runCopy = Assert.IsType<AuthoringInputCopyReport>(built.InputCopy);
        Assert.Equal(capture.TargetDomainId, previewCopy.SourceDomainId);
        Assert.Equal(capture.TargetDomainId, runCopy.SourceDomainId);
        Assert.NotEqual(previewCopy.TargetDomainId, runCopy.TargetDomainId);
        Assert.All(runCopy.Rows, row => Assert.True(row.HasUniqueArgumentMapping));
        Assert.False(runCopy.ProvesHistorySourceAddresses);
        using var input0 = plan.CopyInput(0); using var input1 = plan.CopyInput(1);
        Assert.Equal(10, input0.InspectProperties(InspectionPropertyKind.Length).Mass, 10);
        Assert.Equal(8, input1.InspectProperties(InspectionPropertyKind.Length).Mass, 10);
        var rows = runCopy.Rows.ToArray();
        plan.Dispose(); preview.Dispose(); built.Dispose();
        Assert.Equal(rows, runCopy.Rows.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LoftRunReportsActualCopiedInputDomainWithoutClaimingCompatibleOrFinalIdentity(bool compatibility)
    {
        using var a = BatchSAuthoringTests.Square(); using var b = BatchSAuthoringTests.Square(5, 3);
        byte[] beforeA = Bytes(a), beforeB = Bytes(b);
        using var built = GuidedLoft.Build([a, b], new() { Solid = true, CorrectCompatibility = compatibility });
        Assert.True(built.Diagnostics.ShapeIsValid, built.Diagnostics.Message);
        Assert.Equal(beforeA, Bytes(a)); Assert.Equal(beforeB, Bytes(b));
        var copy = Assert.IsType<AuthoringInputCopyReport>(built.InputCopy);
        Assert.Equal(2, copy.Arguments.Count); Assert.False(copy.ProvesHistorySourceAddresses);
        Assert.All(copy.Rows, row => Assert.True(row.HasUniqueArgumentMapping));
        Assert.False(copy.ProvesFinalResultIdentity); Assert.False(copy.ProvesCompleteOccurrencePaths);
        a.Dispose(); b.Dispose(); built.Dispose(); Assert.NotEmpty(copy.Rows);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreparationKeepsOriginalCopyDomainSeparateFromPreparedHistoryAndFinalMembership(bool reverse)
    {
        var curve = SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(-.5, 0), new(0, 1), new(.5, 0)]))
            .Offset(.03).Trim(.1, .9);
        if (reverse) curve = curve.Reverse();
        using var source = SketchExpressionChain2d.CreateOrdered([curve]).BuildWire(SketchPlane.XY);
        byte[] before = Bytes(source);
        using var prepared = GuidedSweepPlan.PrepareOffsetCurves(source);
        Assert.True(prepared.Diagnostics.ShapeIsValid, prepared.Diagnostics.Message);
        Assert.InRange(prepared.Diagnostics.ApproximationError!.Value, 0, 1e-10);
        Assert.Equal(before, Bytes(source));
        var copy = Assert.IsType<AuthoringInputCopyReport>(prepared.InputCopy);
        Assert.All(copy.Rows, row => Assert.True(row.HasUniqueArgumentMapping));
        Assert.False(copy.ProvesHistorySourceAddresses);
        var snapshot = Assert.Single(prepared.History, item => item.Kind == AuthoringHistoryKind.InputSnapshot);
        Assert.NotNull(snapshot.Shape); Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(prepared.RequireShape(), snapshot.Shape));
        Assert.False(copy.ProvesFinalResultIdentity);
    }

    [Fact]
    public void InvalidEmptyAndDisposedInputNeverReturnPartialCopies()
    {
        Assert.Throws<ArgumentNullException>(() => AuthoringInputCopy.Create(null!));
        Assert.Throws<ArgumentException>(() => AuthoringInputCopy.Create([]));
        using var input = BatchSAuthoringTests.Square(); input.Dispose();
        Assert.Throws<ObjectDisposedException>(() => AuthoringInputCopy.Create([input]));
    }

    [Fact]
    public unsafe void NativeReportIndicesRejectOutsideDomainsAndResetOutputWhileOwnerRemainsUsable()
    {
        using var input = BatchSAuthoringTests.Square(); byte[] before = Bytes(input);
        AuthoringBridge.WithInputs([input], (pointers, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.AuthoringCopyInputs(pointers, count, out nint raw), "test_copy_inputs");
            using FeatureResultHandle result = new(raw);
            NativeError.ThrowIfFailed(NativeMethods.AuthoringCopyReportInfo(result, out var info), "test_copy_info");
            Assert.Equal(1, info.SchemaVersion); Assert.Equal(1, info.ArgumentCount); Assert.True(info.RowCount > 0);
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringCopyReportArgument(result, -1, out var argument));
            Assert.Equal(default, argument);
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringCopyReportArgument(result, info.ArgumentCount, out argument));
            Assert.Equal(default, argument);
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringCopyReportRow(result, -1, out var row));
            Assert.Equal(default, row);
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringCopyReportRow(result, info.RowCount, out row));
            Assert.Equal(default, row);
            Assert.NotNull(AuthoringInputCopyReader.Read(result, Guid.NewGuid()));
            return 0;
        });
        Assert.Equal(before, Bytes(input));
    }

    private static void VerifyDomain(AuthoringInputCopyReport report, Shape[] sources, IReadOnlyList<Shape> targets)
    {
        Assert.Equal(sources.Length, report.Arguments.Count); Assert.Equal(sources.Length, targets.Count);
        foreach (var argument in report.Arguments)
        {
            Shape source = sources[argument.ArgumentIndex], target = targets[argument.ArgumentIndex];
            var sourceTopology = Topology(source); var targetTopology = Topology(target);
            Assert.Equal(sourceTopology.Length, argument.SourceTopologyCount);
            Assert.Equal(targetTopology.Length, argument.TargetTopologyCount);
            var rows = report.Rows.Where(row => row.ArgumentIndex == argument.ArgumentIndex).ToArray();
            Assert.Equal(Enumerable.Range(0, argument.SourceTopologyCount), rows.Select(row => row.SourceTopologyIndex));
            foreach (var row in rows)
            {
                using var original = Member(source, row.SourceTopologyIndex, row.SourceKind);
                using var copied = Member(target, row.TargetTopologyIndex, row.TargetKind!.Value);
                Assert.Equal(row.SourceKind, (ShapeKind)sourceTopology[row.SourceTopologyIndex].Kind);
                Assert.Equal(row.TargetKind, (ShapeKind)targetTopology[row.TargetTopologyIndex].Kind);
                Assert.Equal(row.SourceTopologyIndex, RepairSnapshot.FindTopologyIndex(source, original));
                Assert.Equal(row.TargetTopologyIndex, RepairSnapshot.FindTopologyIndex(target, copied));
                Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(source, copied));
                Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(target, original));
                Assert.Equal(sources.Count(candidate => RepairSnapshot.FindTopologyIndex(candidate, original) >= 0), row.SourceArgumentMemberships);
                Assert.Equal(targets.Count(candidate => RepairSnapshot.FindTopologyIndex(candidate, copied) >= 0), row.TargetArgumentMemberships);
            }
        }
    }
    private static unsafe RepairTopologyRaw[] Topology(Shape shape)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairTopology(shape.Handle, null, 0, out int count), "test_copy_topology_count");
        RepairTopologyRaw[] result = new RepairTopologyRaw[count];
        fixed (RepairTopologyRaw* data = result)
            NativeError.ThrowIfFailed(NativeMethods.RepairTopology(shape.Handle, data, count, out _), "test_copy_topology");
        return result;
    }
    private static Shape Member(Shape shape, int index, ShapeKind kind)
    {
        Shape[] explored = shape.GetSubShapes(kind); Shape? selected = null;
        try
        {
            selected = explored.First(candidate => RepairSnapshot.FindTopologyIndex(shape, candidate) == index);
            return selected;
        }
        finally { foreach (Shape candidate in explored) if (!ReferenceEquals(candidate, selected)) candidate.Dispose(); }
    }
    private static unsafe byte[] Bytes(Shape shape)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count), "test_copy_bytes_count");
        byte[] bytes = new byte[count];
        fixed (byte* data = bytes) NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, data, bytes.Length, out _), "test_copy_bytes");
        return bytes;
    }
}
