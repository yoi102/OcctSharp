using OcctSharp.OwnedValues;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchADOwnedDataTests
{
    [Fact]
    public void CleanConsumerWorkflowKeepsCopiedResultsAlive() => global::OcctSharp.Tests.Shared.BatchADOwnedDataWorkflow.Run();

    [Fact]
    public void HatchEndpointAndElementCopiesDoNotShareSequenceNodes()
    {
        using HatchGenPointOnElement element = HatchGenPointOnElement.Create();
        element.SetIndex(4);
        element.SetParameter(0.5);
        using HatchGenPointOnHatching first = HatchGenPointOnHatching.Create();
        first.SetParameter(2);
        first.AddPoint(element, 1e-9);
        using HatchGenPointOnHatching second = HatchGenPointOnHatching.Create();
        second.SetParameter(8);
        using HatchGenDomain domain = HatchGenDomain.Create(first, second);
        using HatchGenPointOnHatching copy = domain.FirstPoint();
        copy.RemPoint(1);
        using HatchGenPointOnHatching stored = domain.FirstPoint();
        Assert.Equal(1, stored.NbPoints());
        using HatchGenPointOnElement item = stored.Point(1);
        item.SetIndex(90);
        using HatchGenPointOnElement original = stored.Point(1);
        Assert.Equal(4, original.Index());
        first.ClrPoints();
        first.Dispose();
        second.Dispose();
        domain.Dispose();
        Assert.Equal(1, stored.NbPoints());
        Assert.Equal(0, copy.NbPoints());
        Assert.Equal(0.5, original.Parameter());
    }

    [Fact]
    public void HatchMissingEndpointsAndInvalidIndicesReportNativeErrors()
    {
        using HatchGenDomain domain = HatchGenDomain.Create();
        Assert.False(domain.HasFirstPoint());
        Assert.False(domain.HasSecondPoint());
        Assert.Throws<OcctException>(() => domain.FirstPoint());
        Assert.Throws<OcctException>(() => domain.SecondPoint());
        using HatchGenPointOnHatching point = HatchGenPointOnHatching.Create();
        Assert.Throws<OcctException>(() => point.Point(1));
        Assert.Throws<OcctException>(() => point.Point(0));
        Assert.Throws<OcctException>(() => point.Point(int.MaxValue));
        Assert.Throws<OcctException>(() => point.RemPoint(-1));
        Assert.Equal(0, point.NbPoints());
        Assert.Throws<ArgumentNullException>(() => domain.SetFirstPoint(null!));
        point.Dispose();
        Assert.Throws<ObjectDisposedException>(() => domain.SetFirstPoint(point));
    }

    [Fact]
    public void IgesBuilderCopiesCoordinatesIntoIndependentEntities()
    {
        using IGESConvGeomGeomBuilder builder = IGESConvGeomGeomBuilder.Create();
        Assert.True(builder.IsIdentity());
        builder.AddXYZ(new V.Coordinates3d(1, 2, 3));
        builder.AddXYZ(new V.Coordinates3d(4, 5, 6));
        using IGESGeomCopiousData data = Assert.IsType<IGESGeomCopiousData>(builder.MakeCopiousData(2, false));
        using IGESGeomTransformationMatrix transform = Assert.IsType<IGESGeomTransformationMatrix>(builder.MakeTransformation(1));
        builder.Clear();
        builder.Dispose();
        Assert.Equal(2, data.NbPoints());
        Assert.Equal(2, data.DataType());
        Assert.Equal(new Point3d(4, 5, 6), data.Point(2));
        Assert.Equal(1, transform.Value().M11);
    }

    [Fact]
    public void IgesBuilderRejectsInvalidDataRequestsAndRemainsUsable()
    {
        using IGESConvGeomGeomBuilder builder = IGESConvGeomGeomBuilder.Create();
        Assert.Throws<OcctException>(() => builder.MakeCopiousData(2, false));
        builder.AddXY(new V.Coordinates2d(4, 5));
        Assert.Throws<OcctException>(() => builder.Point(0));
        Assert.Throws<OcctException>(() => builder.Point(int.MaxValue));
        Assert.Throws<OcctException>(() => builder.MakeCopiousData(9, false));
        Assert.Throws<OcctException>(() => builder.MakeCopiousData(3, true));
        double x = -1, y = -2, z = -3;
        builder.EvalXYZ(new V.Coordinates3d(7, 8, 9), ref x, ref y, ref z);
        Assert.Equal((7d, 8d, 9d), (x, y, z));
        using IGESGeomCopiousData data = Assert.IsType<IGESGeomCopiousData>(builder.MakeCopiousData(1, true));
        Assert.Equal(new Point3d(4, 5, 0), data.Point(1));
    }

    [Fact]
    public void IgesDirectoryCriteriaConfigureAndRejectNullEntity()
    {
        using IGESDataDirChecker checker = IGESDataDirChecker.Create(100, 2);
        checker.BlankStatusRequired(1);
        checker.UseFlagRequired(0);
        checker.SubordinateStatusRequired(0);
        checker.HierarchyStatusRequired(0);
        checker.SetDefault();
        Assert.Throws<ArgumentNullException>(() => checker.Correct(null!));
        checker.Dispose();
        Assert.Throws<ObjectDisposedException>(() => checker.BlankStatusRequired(1));
    }

    [Fact]
    public void CommonPartPublishesIndependentRangeAndBoundingPoints()
    {
        using IntToolsCommonPrt common = IntToolsCommonPrt.Create();
        common.SetRange1(2, 7);
        common.SetBoundingPoints(new Point3d(1, 2, 3), new Point3d(4, 5, 6));
        double first = -1, last = -1;
        common.Range1(ref first, ref last);
        Assert.Equal((2d, 7d), (first, last));
        Point3d a = default, b = default;
        common.BoundingPoints(ref a, ref b);
        common.Dispose();
        Assert.Equal(new Point3d(1, 2, 3), a);
        Assert.Equal(new Point3d(4, 5, 6), b);
        Assert.Throws<ObjectDisposedException>(() => common.BoundingPoints(ref a, ref b));
        Assert.Equal(new Point3d(1, 2, 3), a);
    }

    [Fact]
    public void EmptyIntersectionCurveHasNullHandlesAndUnchangedBounds()
    {
        using IntToolsCurve curve = IntToolsCurve.Create();
        Assert.Null(curve.Curve());
        Assert.Null(curve.FirstCurve2d());
        Assert.Null(curve.SecondCurve2d());
        Assert.False(curve.HasBounds());
        double first = 11, last = 22;
        Point3d a = new(1, 2, 3), b = new(4, 5, 6);
        Assert.False(curve.Bounds(ref first, ref last, ref a, ref b));
        Assert.Equal((11d, 22d), (first, last));
        Assert.Equal(new Point3d(1, 2, 3), a);
        curve.SetTolerance(0.125);
        curve.SetTangentialTolerance(0.25);
        Assert.Equal(0.125, curve.Tolerance());
        Assert.Equal(0.25, curve.TangentialTolerance());
        Assert.Throws<ArgumentNullException>(() => curve.SetCurve(null!));
    }

    [Fact]
    public void RootInitializesAndRoundTripsAllIntervalOutputs()
    {
        using IntToolsRoot root = IntToolsRoot.Create(3, 1);
        double t1 = 9, t2 = 9, f1 = 9, f2 = 9;
        root.Interval(ref t1, ref t2, ref f1, ref f2);
        Assert.Equal((0d, 0d, 0d, 0d), (t1, t2, f1, f2));
        root.SetInterval(1, 5, -2, 4);
        root.Interval(ref t1, ref t2, ref f1, ref f2);
        Assert.Equal((1d, 5d, -2d, 4d), (t1, t2, f1, f2));
        root.SetStateBefore(TopAbsState.TopAbs_OUT);
        root.SetStateAfter(TopAbsState.TopAbs_IN);
        Assert.True(root.IsValid());
        Assert.Equal(3, root.Root());
    }

    [Fact]
    public void StartPointResolvesTriangleEdgesAndPreservesNoEdgeOutputs()
    {
        using IntPolyhTriangle triangle = IntPolyhTriangle.Create(10, 20, 30);
        using IntPolyhStartPoint point = IntPolyhStartPoint.Create();
        // OCCT uses -1 as the no-edge sentinel; the default record stores -2.
        point.SetEdge1(-1);
        point.SetEdge2(-1);
        int first = -11, second = -22, last = -33;
        Assert.Equal(0, point.GetEdgePoints(triangle, ref first, ref second, ref last));
        Assert.Equal((-11, -22, -33), (first, second, last));
        point.SetEdge1(2);
        Assert.Equal(1, point.GetEdgePoints(triangle, ref first, ref second, ref last));
        Assert.Equal((20, 30, 10), (first, second, last));
        point.SetXYZ(1, 2, 3);
        point.SetUV1(0.2, 0.3);
        Assert.Equal((1d, 2d, 3d, 0.2, 0.3), (point.X(), point.Y(), point.Z(), point.U1(), point.V1()));
        triangle.Dispose();
        Assert.Throws<ObjectDisposedException>(() => point.GetEdgePoints(triangle, ref first, ref second, ref last));
    }

    [Fact]
    public void SectionPointPublishesCopiedPositionAndEnumReferenceOutputs()
    {
        using IntfSectionPoint point = IntfSectionPoint.Create(new Point3d(2, 4, 6),
            IntfPIType.Intf_EDGE, 5, 6, 0.25, IntfPIType.Intf_FACE, 7, 8, 0.75, 0.5);
        IntfPIType type = default;
        int first = 0, second = 0;
        double parameter = 0;
        point.InfoFirst(ref type, ref first, ref second, ref parameter);
        Assert.Equal((IntfPIType.Intf_EDGE, 5, 6, 0.25), (type, first, second, parameter));
        Point3d copy = point.Pnt();
        point.Dispose();
        Assert.Equal(new Point3d(2, 4, 6), copy);
    }

    [Fact]
    public void ClipRangeCopiesInputAndComputesNearestUnclippedDepth()
    {
        using SelectMgrViewClipRange clip = SelectMgrViewClipRange.Create();
        using BndRange removed = BndRange.Create(2, 5);
        clip.AddClipSubRange(removed);
        removed.Shift(100);
        removed.Dispose();
        Assert.True(clip.IsClipped(3));
        Assert.False(clip.IsClipped(7));
        using BndRange query = BndRange.Create(3, 9);
        double depth = -1;
        Assert.True(clip.GetNearestDepth(query, ref depth));
        Assert.Equal(5, depth);
        using BndRange covered = BndRange.Create(3, 4);
        Assert.False(clip.GetNearestDepth(covered, ref depth));
        clip.SetVoid();
        Assert.False(clip.IsClipped(3));
    }

    [Fact]
    public void PermissionFlagsUseCopiedEnumOutputsWithoutFilesystemAccess()
    {
        using OSDProtection permissions = OSDProtection.Create(OSDSingleProtection.OSD_R,
            OSDSingleProtection.OSD_RW, OSDSingleProtection.OSD_RX, OSDSingleProtection.OSD_None);
        OSDSingleProtection system = default, user = default, group = default, world = default;
        permissions.Values(ref system, ref user, ref group, ref world);
        Assert.Equal((OSDSingleProtection.OSD_R, OSDSingleProtection.OSD_RW,
            OSDSingleProtection.OSD_RX, OSDSingleProtection.OSD_None), (system, user, group, world));
        permissions.Add(ref world, OSDSingleProtection.OSD_R);
        permissions.Add(ref world, OSDSingleProtection.OSD_W);
        Assert.Equal(OSDSingleProtection.OSD_RW, world);
        permissions.Sub(ref world, OSDSingleProtection.OSD_W);
        Assert.Equal(OSDSingleProtection.OSD_R, world);
        Assert.Equal(OSDSingleProtection.OSD_None, permissions.World());
    }

    [Fact]
    public void ExecutionStatusCombinesIndependentGroupsAndRejectsDisposedOperands()
    {
        using MessageExecStatus left = MessageExecStatus.Create();
        using MessageExecStatus right = MessageExecStatus.Create();
        left.SetAllDone();
        right.SetAllWarn();
        left.Add(right);
        Assert.True(left.IsDone());
        Assert.True(left.IsWarn());
        Assert.False(left.IsFail());
        left.And(right);
        Assert.False(left.IsDone());
        Assert.True(left.IsWarn());
        right.Clear();
        Assert.True(left.IsWarn());
        left.ClearAllWarn();
        Assert.False(left.IsWarn());
        right.Dispose();
        Assert.Throws<ObjectDisposedException>(() => left.Add(right));
        Assert.Throws<ArgumentNullException>(() => left.And(null!));
    }
}
