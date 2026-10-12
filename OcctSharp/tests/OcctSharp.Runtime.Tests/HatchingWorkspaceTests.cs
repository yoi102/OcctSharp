using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.HatchingWorkspaces;
using OcctSharp.ReferenceOperations;
using OcctSharp.Tests.Shared;
using OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class HatchingWorkspaceTests
{
    static readonly int[] FirstVertexElements = [1, 4];
    static readonly int[] LastVertexElements = [2, 3];
    static HatchingSnapshot RectangleScan(double y = 0, bool points = false, bool segments = false)
    {
        var owners = new List<IDisposable>();
        try
        {
            var boundaries = BatchAQHatchingWorkflow.Rectangle(owners);
            using var line = BatchAQHatchingWorkflow.Line(0, y);
            return GeometricHatching.Scan(boundaries, [new(line, -10, 10)], keepPoints: points, keepSegments: segments);
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }

    [Fact] public void FullCurvedContoursCopiedTrimsAPTopologyAndRealFiles() => BatchAQHatchingWorkflow.Run();

    [Fact]
    public void EmptyScanAndOutsideScanAreSuccessfulEmptyResults()
    {
        using var empty = GeometricHatching.Scan([], []);
        Assert.True(empty.IsDone); Assert.Empty(empty.Curves);
        using var outside = RectangleScan(4);
        Assert.True(outside.IsDone); Assert.True(outside.Curves[0].IsDone);
        Assert.Equal(HatchGenErrorStatus.HatchGen_NoProblem, outside.Curves[0].Status);
        Assert.Empty(outside.Curves[0].Domains); Assert.Empty(outside.Curves[0].Points);
    }

    [Fact]
    public void DetachedEventsPreserveBoundaryIdentityTransitionAndParameter()
    {
        using var snapshot = RectangleScan();
        var result = snapshot.Curves[0]; Assert.Equal(2, result.Points.Count);
        var first = result.Points[0]; var last = result.Points[1];
        Assert.Equal(-4, first.Event.Parameter, 7); Assert.Equal(4, last.Event.Parameter, 7);
        Assert.Equal(4, Assert.Single(first.Elements).SupportIndex); Assert.Equal(2, Assert.Single(last.Elements).SupportIndex);
        Assert.Equal(HatchingRegionState.Outside, first.Event.Before); Assert.Equal(HatchingRegionState.Inside, first.Event.After);
        Assert.Equal(HatchingRegionState.Inside, last.Event.Before); Assert.Equal(HatchingRegionState.Outside, last.Event.After);
        Assert.Null(first.Event.Type); Assert.NotNull(first.Elements[0].Type);
        snapshot.Dispose(); Assert.Equal(3, first.Elements[0].Parameter, 7);
        Assert.Throws<NotSupportedException>(() => ((IList<HatchingCurveResult>)snapshot.Curves)[0] = result);
    }

    [Fact]
    public void NativeInfiniteDomainKeepsAbsentEndpointsAndFiniteClip()
    {
        var owners = new List<IDisposable>();
        try
        {
            var boundaries = BatchAQHatchingWorkflow.Rectangle(owners);
            using var circle = new Geom2dCircle(new Axis2d(new(0, 0), new(1, 0)), 2, true);
            using var snapshot = GeometricHatching.Scan(boundaries, [new(circle, 0, 2 * Math.PI)]);
            var domain = Assert.Single(snapshot.Curves[0].Domains);
            Assert.Null(domain.First); Assert.Null(domain.Last); Assert.Equal(0, domain.ClippedFirst); Assert.Equal(2 * Math.PI, domain.ClippedLast);
            using var trimmed = snapshot.TrimmedCurve(0, 0)!; snapshot.Dispose(); circle.SetRadius(8);
            Point2d p = default; Geom2dCurveReferences.D0(trimmed, Math.PI / 2, ref p);
            Assert.Equal(0, p.X, 7); Assert.Equal(2, p.Y, 7);
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }

    [Fact]
    public void TangencyRetainsEventsAndPointOnlyDomainsDoNotInventCurves()
    {
        using var circle = new Geom2dCircle(new Axis2d(new(0, 0), new(1, 0)), 2, true);
        using var line = BatchAQHatchingWorkflow.Line(0, 2);
        HatchingBoundary[] boundaries = [new(new HatchingCurve(circle, 0, 2 * Math.PI))];
        using var suppressed = GeometricHatching.Scan(boundaries, [new(line, -5, 5)]);
        using var kept = GeometricHatching.Scan(boundaries, [new(line, -5, 5)], keepPoints: true);
        Assert.True(suppressed.IsDone); Assert.True(kept.IsDone);
        Assert.NotEmpty(kept.Curves[0].Points); Assert.Empty(suppressed.Curves[0].Domains);
        var domain = Assert.Single(kept.Curves[0].Domains);
        Assert.Equal(domain.First, domain.Last); Assert.False(domain.HasTrimmedCurve); Assert.Null(kept.TrimmedCurve(0, 0));
        Assert.Contains(kept.Curves[0].Points, p => p.Event.Before == p.Event.After);
    }

    [Fact]
    public void CoincidentBoundarySegmentsPreserveBothSegmentFlags()
    {
        using var snapshot = RectangleScan(3, points: true, segments: true);
        Assert.True(snapshot.IsDone); Assert.True(snapshot.KeepPoints); Assert.True(snapshot.KeepSegments);
        Assert.Contains(snapshot.Curves[0].Points, p => p.Event.SegmentBeginning);
        Assert.Contains(snapshot.Curves[0].Points, p => p.Event.SegmentEnd);
        var domain = Assert.Single(snapshot.Curves[0].Domains);
        Assert.Equal(-4, domain.ClippedFirst, 7); Assert.Equal(4, domain.ClippedLast, 7);
    }

    [Fact]
    public void IncoherentCoincidentContourRetainsNativeFailureWithoutDomains()
    {
        using var boundary = BatchAQHatchingWorkflow.Line(-1, 0, -1, 0);
        using var other = BatchAQHatchingWorkflow.Line(1, 1, 1, -1);
        using var line = BatchAQHatchingWorkflow.Line(0, 0);
        using var snapshot = GeometricHatching.Scan([
            new(new HatchingCurve(boundary, 0, 3), TopAbsOrientation.TopAbs_REVERSED),
            new(new HatchingCurve(other, 0, 2), TopAbsOrientation.TopAbs_REVERSED)], [new(line, -10, 10)], keepPoints: true);
        Assert.False(snapshot.IsDone); Assert.False(snapshot.Curves[0].IsDone);
        Assert.Equal(HatchGenErrorStatus.HatchGen_IncoherentParity, snapshot.Curves[0].Status);
        Assert.True(snapshot.Curves[0].TrimDone); Assert.False(snapshot.Curves[0].TrimFailed);
        Assert.NotEmpty(snapshot.Curves[0].Points); Assert.False(snapshot.Curves[0].HasUsableDomains);
        Assert.Empty(snapshot.Curves[0].Domains);
        Assert.Throws<InvalidOperationException>(() => snapshot.TrimmedCurve(0, 0));
    }

    [Fact]
    public unsafe void NativeDoneFlagWithErrorStatusNeverMarksSnapshotSuccessful()
    {
        using var coincident = BatchAQHatchingWorkflow.Line(-2, 0, -1, 0);
        using var endpoint = BatchAQHatchingWorkflow.Line(0, 0, -1, -1);
        using var crossing = BatchAQHatchingWorkflow.Line(-1, 1, 1, -1);
        using var line = BatchAQHatchingWorkflow.Line(0, 0);
        using var snapshot = GeometricHatching.Scan([
            new(new HatchingCurve(coincident, 0, 3), TopAbsOrientation.TopAbs_REVERSED),
            new(new HatchingCurve(endpoint, 0, 3), TopAbsOrientation.TopAbs_REVERSED),
            new(new HatchingCurve(crossing, 0, 3))], [new(line, -10, 10)], keepPoints: true);
        var result = snapshot.Curves[0];
        Assert.True(result.IsDone); Assert.Equal(HatchGenErrorStatus.HatchGen_IncoherentParity, result.Status);
        Assert.False(snapshot.IsDone); Assert.False(result.HasUsableDomains); Assert.Empty(result.Domains);
        Assert.NotEmpty(result.Points); Assert.Throws<InvalidOperationException>(() => snapshot.TrimmedCurve(0, 0));
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var domain = Marshal.GetDelegateForFunctionPointer<RawDomain>(NativeLibrary.GetExport(library, "occtsharp_hatching_domain"));
            int* flags = stackalloc int[3] { 123, 124, 125 };
            double* values = stackalloc double[4] { 126, 127, 128, 129 };
            Assert.NotEqual(0, domain(Token(snapshot), 0, 0, flags, 3, values, 4));
            Assert.Equal(123, flags[0]); Assert.Equal(125, flags[2]);
            Assert.Equal(126, values[0]); Assert.Equal(129, values[3]);
        }
        finally { NativeLibrary.Free(library); }
    }

    [Fact]
    public void SingleDirectedBoundaryPreservesNativeSemiInfiniteDomain()
    {
        using var boundary = BatchAQHatchingWorkflow.Line(0, -1, 0, 1);
        using var line = BatchAQHatchingWorkflow.Line(0, 0);
        using var snapshot = GeometricHatching.Scan([new(new HatchingCurve(boundary, 0, 2))], [new(line, -3, 3)]);
        Assert.True(snapshot.Curves[0].HasUsableDomains);
        var domain = Assert.Single(snapshot.Curves[0].Domains);
        Assert.Null(domain.First); Assert.Equal(0, domain.Last!.Value, 7);
        Assert.Equal(-3, domain.ClippedFirst); Assert.Equal(0, domain.ClippedLast, 7);
    }

    [Fact]
    public void SharedContourVertexKeepsAllBoundaryAssociations()
    {
        var owners = new List<IDisposable>();
        try
        {
            var boundaries = BatchAQHatchingWorkflow.Rectangle(owners);
            using var line = BatchAQHatchingWorkflow.Line(0, 0, .8, .6);
            using var snapshot = GeometricHatching.Scan(boundaries, [new(line, -10, 10)]);
            Assert.True(snapshot.Curves[0].HasUsableDomains);
            Assert.Equal(2, snapshot.Curves[0].Points.Count);
            Assert.All(snapshot.Curves[0].Points, p => Assert.Equal(2, p.Elements.Count));
            Assert.Equal(FirstVertexElements, snapshot.Curves[0].Points[0].Elements.Select(e => e.SupportIndex).Order());
            Assert.Equal(LastVertexElements, snapshot.Curves[0].Points[1].Elements.Select(e => e.SupportIndex).Order());
            var domain = Assert.Single(snapshot.Curves[0].Domains); Assert.Equal(-5, domain.First!.Value, 7); Assert.Equal(5, domain.Last!.Value, 7);
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }

    [Fact]
    public void BezierGenericAndTrimmedInputsReuseExistingOwningGeometry()
    {
        var owners = new List<IDisposable>();
        try
        {
            var boundaries = BatchAQHatchingWorkflow.Rectangle(owners);
            using var bezier = Geom2dBezierCurveCollections.Create(new([new Point2d(-5, 0), new(5, 0)]));
            Assert.Throws<ArgumentException>(() => GeometricHatching.Scan(boundaries, [new(bezier, -1, 2)]));
            using var scan = GeometricHatching.Scan(boundaries, [new(bezier, 0, 1)]);
            var domain = Assert.Single(scan.Curves[0].Domains); Assert.Equal(.1, domain.First!.Value, 7); Assert.Equal(.9, domain.Last!.Value, 7);
            using var generic = scan.TrimmedCurve(0, 0)!;
            using var trimmed = new Geom2dTrimmedCurve(generic, .2, .8, true, false);
            using var repeated = GeometricHatching.Scan(boundaries, [new HatchingCurve(generic, .15, .85), new(trimmed, .2, .8)]);
            Assert.True(repeated.IsDone); Assert.All(repeated.Curves, c => Assert.True(c.HasUsableDomains));
            Assert.Equal(.15, Assert.Single(repeated.Curves[0].Domains).ClippedFirst);
            Assert.Equal(.8, Assert.Single(repeated.Curves[1].Domains).ClippedLast);
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }

    [Fact]
    public void SplineInputCopyRemainsIndependentOfPoleMutation()
    {
        using var source = Geom2dBSplineCurveCollections.Create(new([new Point2d(-5, 0), new(5, 0)]), new([0.0, 1]), new([2, 2]), 1, false);
        var owners = new List<IDisposable>();
        try
        {
            var boundaries = BatchAQHatchingWorkflow.Rectangle(owners);
            using var snapshot = GeometricHatching.Scan(boundaries, [new(source, 0, 1)]);
            var domain = Assert.Single(snapshot.Curves[0].Domains); Assert.Equal(.1, domain.First!.Value, 7); Assert.Equal(.9, domain.Last!.Value, 7);
            source.SetPole(1, new Point2d(999, 999));
            using var trimmed = snapshot.TrimmedCurve(0, 0)!; snapshot.Dispose(); source.Dispose();
            Point2d p = default; Geom2dCurveReferences.D0(trimmed, .25, ref p); Assert.Equal(-2.5, p.X, 7); Assert.Equal(0, p.Y, 7);
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }

    [Fact]
    public void InvalidRangesOrientationsTolerancesAndIndicesAreRejected()
    {
        using var line = BatchAQHatchingWorkflow.Line(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => new HatchingCurve(line, double.NaN, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HatchingCurve(line, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GeometricHatching.Scan([], [], new(Tangency: 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => GeometricHatching.Scan([new(new HatchingCurve(line, 0, 1), TopAbsOrientation.TopAbs_INTERNAL)], []));
        var input = new HatchingCurve(line, -1, 1); line.Dispose();
        Assert.Throws<ObjectDisposedException>(() => GeometricHatching.Scan([], [input]));
        using var snapshot = RectangleScan();
        Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.TrimmedCurve(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.TrimmedCurve(0, 1));
        Assert.Throws<OcctException>(() => snapshot.TrimmedCurve3d(0, 0, default));
        snapshot.Dispose(); Assert.Throws<ObjectDisposedException>(() => snapshot.TrimmedCurve(0, 0));
    }

    [Fact]
    public void ConcurrentCopiedCurveQueriesAndReleaseRemainSafe()
    {
        using var snapshot = RectangleScan();
        var errors = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
        Parallel.For(0, 80, i =>
        {
            try { if (i == 30) snapshot.Dispose(); else { using var curve = snapshot.TrimmedCurve(0, 0)!; Point2d p = default; Geom2dCurveReferences.D0(curve, 0, ref p); Assert.Equal(0, p.X, 7); } }
            catch (ObjectDisposedException) { }
            catch (Exception e) { errors.Enqueue(e); }
        });
        Assert.Empty(errors); Assert.Single(snapshot.Curves[0].Domains);
    }

    static nint Token(object value) => ((SafeHandle)value.GetType().GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(value)!).DangerousGetHandle();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawInfo(nint token, int index, int* values, int capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawDomain(nint token, int index, int domainIndex, int* flags, int flagCapacity, double* values, int capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawCreate(nint* inputs, int* kinds, double* ranges, int boundaries, int hatchings, int* orientations, double* tolerances, int flags, out nint result, out int done, double* actual, int capacity);
    [Fact]
    public unsafe void RawCreationCapacityAndCountFailuresPublishNothing()
    {
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var create = Marshal.GetDelegateForFunctionPointer<RawCreate>(NativeLibrary.GetExport(library, "occtsharp_hatching_create"));
            double* tolerances = stackalloc double[4] { 1e-8, 1e-8, 1e-8, 1e-8 }; double* actual = stackalloc double[4] { 123, 124, 125, 126 };
            Assert.NotEqual(0, create(null, null, null, 0, 0, null, tolerances, 0, out var token, out var done, actual, 3));
            Assert.Equal(0, token); Assert.Equal(0, done); Assert.Equal(123, actual[0]); Assert.Equal(126, actual[3]);
            Assert.NotEqual(0, create(null, null, null, -1, 0, null, tolerances, 0, out token, out done, actual, 4));
            Assert.Equal(0, token); Assert.Equal(0, done); Assert.Equal(123, actual[0]);
        }
        finally { NativeLibrary.Free(library); }
    }
    [Fact]
    public unsafe void RawIndicesCapacityAndWrongTypedTokensNeverWriteBuffers()
    {
        using var snapshot = RectangleScan(); using var wrong = OcctSharp.ModelingWorkspaces.ModelingBuilders.Edge(new(0, 0, 0), new(1, 0, 0));
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var info = Marshal.GetDelegateForFunctionPointer<RawInfo>(NativeLibrary.GetExport(library, "occtsharp_hatching_info"));
            int* values = stackalloc int[6] { 123, 124, 125, 126, 127, 128 };
            Assert.NotEqual(0, info(Token(snapshot), 0, values, 5)); Assert.Equal(123, values[0]);
            Assert.NotEqual(0, info(Token(snapshot), -1, values, 6)); Assert.Equal(123, values[0]);
            Assert.NotEqual(0, info(Token(wrong), 0, values, 6)); Assert.Equal(128, values[5]);
            var token = Token(snapshot); snapshot.Dispose(); Assert.NotEqual(0, info(token, 0, values, 6)); Assert.Equal(123, values[0]);
        }
        finally { NativeLibrary.Free(library); }
    }
}
