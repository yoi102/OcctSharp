using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OcctSharp.Interop;
using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchABOwnedTests
{
    static BatchABOwnedTests() => NativeLibraryResolver.EnsureRegistered(typeof(BatchABOwnedTests).Assembly);
    [Fact]
    public void SharedConsumerWorkflowRuns() => OcctSharp.Tests.Shared.BatchABOwnedWorkflow.Run();

    [Fact]
    public void RangeInputsMutationsAndIndependentCopiesCompose()
    {
        using BndRange left = BndRange.Create(1, 5);
        using BndRange right = BndRange.Create(3, 8);
        left.Common(right);
        double min = 0, max = 0;
        Assert.True(left.GetBounds(ref min, ref max));
        Assert.Equal((3d, 5d), (min, max));
        using BndRange shifted = left.Shifted(10);
        left.Dispose(); right.Dispose();
        Assert.True(shifted.GetBounds(ref min, ref max));
        Assert.Equal((13d, 15d), (min, max));
    }

    [Fact]
    public void FalseQueryPreservesInitializedRefValues()
    {
        using BndRange empty = BndRange.Create();
        double min = 11, max = 13;
        Assert.False(empty.GetBounds(ref min, ref max));
        Assert.Equal((11d, 13d), (min, max));
    }

    [Fact]
    public void KernelFailureAndBadInputPreserveAllManagedOutputs()
    {
        using BndBox empty = BndBox.Create();
        double a = 1, b = 2, c = 3, d = 4, e = 5, f = 6;
        Assert.Throws<OcctException>(() => empty.Get(ref a, ref b, ref c, ref d, ref e, ref f));
        Assert.Equal((1d, 2d, 3d, 4d, 5d, 6d), (a, b, c, d, e, f));
        Assert.Throws<OcctException>(() => empty.Add(new Point3d(double.NaN, 0, 0)));
        Assert.True(empty.IsVoid());
        Assert.Throws<OcctException>(() => BndRange.Create(4, 2));
    }

    [Fact]
    public void TypedHandlesRejectDisposedReceiversAndArguments()
    {
        using BndRange range = BndRange.Create(0, 1);
        using BndRange other = BndRange.Create(2, 3);
        other.Dispose(); other.Dispose();
        Assert.Throws<ObjectDisposedException>(() => range.Common(other));
        Assert.Throws<ArgumentNullException>(() => range.Common(null!));
        range.Dispose();
        Assert.Throws<ObjectDisposedException>(() => range.Delta());
    }

    [Fact]
    public void CopiedExtremaPointSurvivesAlgorithmDisposal()
    {
        using ExtremaExtPElC extrema = ExtremaExtPElC.Create(new Point3d(2, 3, 0),
            Line3dOperations.Create(new Point3d(0, 0, 0), new V.Direction3d(1, 0, 0)), 1e-9, -100, 100);
        Assert.True(extrema.IsDone());
        Assert.Equal(9, extrema.SquareDistance(1), 10);
        using ExtremaPOnCurv point = extrema.Point(1);
        extrema.Dispose();
        Assert.Equal(new Point3d(2, 0, 0), point.Value());
        Assert.Equal(2, point.Parameter(), 10);
    }

    [Fact]
    public void StableAddressProjectionRetainsCurveAfterCallerDisposal()
    {
        using GeomAPIProjectPointOnCurve projection = GeomAPIProjectPointOnCurve.Create();
        using (GeomCylindricalSurface cylinder = new(CylinderOperations.Create(Axis3Operations.Create(), 4)))
        using (GeomCurve curve = cylinder.VIso(0)!) projection.Init(new Point3d(7, 0, 0), curve);
        for (int i = 0; i < 100; i++)
        {
            projection.Perform(new Point3d(7 + i, 0, 0));
            Assert.Equal(3 + i, projection.LowerDistance(), 9);
            Assert.Equal(new Point3d(4, 0, 0), projection.NearestPoint());
        }
    }

    [Fact]
    public void MakerReportsNativeConstructionStatus()
    {
        using GceMakeDir invalid = GceMakeDir.Create(0, 0, 0);
        Assert.False(invalid.IsDone());
        Assert.True(invalid.IsError());
        Assert.Throws<OcctException>(() => invalid.Value());
        using GceMakeDir valid = GceMakeDir.Create(0, 3, 4);
        Assert.True(valid.IsDone());
        Assert.Equal(new V.Direction3d(0, 0.6, 0.8), valid.Value());
    }

    [Fact]
    public void TangencyPublishesScalarPointAndEnumOutputs()
    {
        using GccAnaLin2d2Tan line = GccAnaLin2d2Tan.Create(new V.Point2d(0, 0), new V.Point2d(3, 4), 1e-9);
        Assert.True(line.IsDone()); Assert.Equal(1, line.NbSolutions());
        double onLine = 0, onArgument = 0; V.Point2d point = default;
        line.Tangency2(1, ref onLine, ref onArgument, ref point);
        Assert.Equal(new V.Point2d(3, 4), point);
        GccEntPosition first = default, second = default;
        line.WhichQualifier(1, ref first, ref second);
        Assert.True(Enum.IsDefined(first)); Assert.True(Enum.IsDefined(second));
    }

    [Fact]
    public void DefaultSupportingFactoriesAndInheritedMassAreUsable()
    {
        using BndBox2d box2d = BndBox2d.Create();
        Assert.True(box2d.IsVoid());
        using BndOBB obb = BndOBB.Create();
        Assert.True(obb.IsVoid());
        using GPropGProps props = GPropGProps.Create();
        Assert.Equal(0, props.Mass());
        using GPropPrincipalProps principal = props.PrincipalProperties();
        props.Dispose();
        Assert.NotNull(principal);
    }

    [Fact]
    public unsafe void RawHandlesRejectNullStaleWrongTypeAndNeverRepeat()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchABOwnedTests).Assembly);
        int value = 19;
        Assert.Equal(1, BoxIsVoid(0, &value)); Assert.Equal(0, value);
        using BndRange range = BndRange.Create();
        Assert.Equal(1, BoxIsVoid(range.NativeHandle.DangerousGetHandle(), &value));
        nint prior = 0;
        for (int i = 0; i < 1000; i++)
        {
            Assert.Equal(0, BoxCreate(out nint token));
            Assert.True(token > prior); prior = token;
            Assert.Equal(0, BoxIsVoid(token, &value)); Assert.Equal(1, value);
            BoxRelease(token); BoxRelease(token);
            Assert.Equal(1, BoxIsVoid(token, &value)); Assert.Equal(0, value);
        }
    }

    [Fact]
    public unsafe void FinalizerReleasesUnreachableOwnedToken()
    {
        (WeakReference weak, nint token) = Abandon();
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Assert.False(weak.IsAlive);
        int result = 1;
        Assert.Equal(1, BoxIsVoid(token, &result));
    }

    [Fact]
    public unsafe void RawConstructionFailureClearsHandleAndSuccessClearsError()
    {
        Assert.NotEqual(0, BoxCreateBounds(new Point3d(double.NaN, 0, 0), new Point3d(1, 1, 1), out nint failed));
        Assert.Equal(0, failed);
        Assert.NotEqual(string.Empty, Marshal.PtrToStringUTF8(GetLastError()));
        Assert.Equal(0, BoxCreate(out nint token));
        try {
            int value = 0;
            Assert.Equal(0, BoxIsVoid(token, &value));
            Assert.Equal(string.Empty, Marshal.PtrToStringUTF8(GetLastError()));
        } finally { BoxRelease(token); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, nint) Abandon()
    {
        var value = BndBox.Create();
        return (new WeakReference(value), value.NativeHandle.DangerousGetHandle());
    }

    [Fact]
    public async Task DisposeRaceKeepsManagedCallsSafe()
    {
        for (int round = 0; round < 40; round++)
        {
            using BndBox box = BndBox.Create();
            var caller = Task.Run(() => {
                for (int i = 0; i < 100; i++)
                {
                    try { Assert.True(box.IsVoid()); }
                    catch (ObjectDisposedException) { break; }
                }
            });
            box.Dispose();
            await caller;
        }
    }

    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_owned_bnd_box_create_4", CallingConvention = CallingConvention.Cdecl)]
    private static extern int BoxCreate(out nint result);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_owned_bnd_box_create_5", CallingConvention = CallingConvention.Cdecl)]
    private static extern int BoxCreateBounds(Point3d first, Point3d last, out nint result);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_owned_bnd_box_isvoid_37", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern int BoxIsVoid(nint receiver, int* result);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_owned_bnd_box_release", CallingConvention = CallingConvention.Cdecl)]
    private static extern void BoxRelease(nint receiver);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_get_last_error", CallingConvention = CallingConvention.Cdecl)]
    private static extern nint GetLastError();
}
