using System.Runtime.InteropServices;
using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class CopiedCollectionsTests
{
    private static readonly int[] ExpectedMultiplicities = [3, 3];
    private static readonly int[] ExtremeLowerBounds = [int.MinValue + 1, int.MaxValue - 2];
    private static readonly int[] ExpectedReversedMultiplicities = [3, 2];
    private static readonly double[] ExpectedReparameterizedLimits = [10, 20];
    [Fact]
    public void PackageWorkflow() => global::OcctSharp.Tests.Shared.BatchAFCopiedCollectionsWorkflow.Run();

    [Fact]
    public void ManagedSnapshotsCopyInputAndExportAndValidateBounds()
    {
        double[] input = [1, 2]; var snapshot = new CopiedArray<double>(input, int.MaxValue - 2);
        input[0] = 99; var exported = snapshot.ToArray(); exported[0] = 98;
        Assert.Equal(1, snapshot[int.MaxValue - 2]); Assert.Equal(int.MaxValue - 1, snapshot.Upper);
        Assert.Throws<ArgumentException>(() => new CopiedArray<int>([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedArray<int>([1, 2], int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedArray<int>([1], int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedArray<int>([1], int.MinValue));
        Assert.Throws<IndexOutOfRangeException>(() => snapshot[int.MaxValue - 3]);
        Assert.Equal(0, new CopiedSequence<int>([]).Count);
    }

    [Fact]
    public void StaticCopiesPreserveBoundsAndOriginalSnapshot()
    {
        CopiedArray<Coordinates3d> data = new([new(0, 1, 2), new(3, 4, 5)], -3); var original = data;
        BSplCLibCollections.Reverse(ref data, data.Upper);
        Assert.Equal(new Coordinates3d(3, 4, 5), data[-3]); Assert.Equal(new Coordinates3d(0, 1, 2), original[-3]);
        CopiedArray<double> knots = new([0, 0.5, 1], 3);
        BSplCLibCollections.Reparametrize(10, 20, ref knots);
        Assert.Equal(new double[] { 10, 15, 20 }, knots.ToArray()); Assert.Equal(3, knots.Lower);
        CopiedArray<int> mults = new([1, 3, 2], 4);
        Assert.Equal(3, BSplCLibCollections.MaxKnotMult(mults, 4, 6));
        Assert.Throws<ArgumentException>(() => BSplCLibCollections.MaxKnotMult(mults, 3, 6));
        foreach (int lower in ExtremeLowerBounds)
        {
            CopiedArray<int> extreme = new([2, 3], lower);
            Assert.Equal(3, BSplCLibCollections.MaxKnotMult(extreme, lower, lower + 1));
            BSplCLibCollections.Reverse(ref extreme);
            Assert.Equal(ExpectedReversedMultiplicities, extreme.ToArray()); Assert.Equal(lower, extreme.Lower);
            CopiedArray<double> parameters = new([0, 1], lower);
            BSplCLibCollections.Reparametrize(10, 20, ref parameters);
            Assert.Equal(ExpectedReparameterizedLimits, parameters.ToArray());
        }
    }

    [Fact]
    public void FailedMutableCopiesLeaveManagedReferenceUnchanged()
    {
        CopiedArray<double> data = new([0, 0.5, 1]); var original = data;
        Assert.Throws<ArgumentException>(() => BSplCLibCollections.Reparametrize(1, 1, ref data)); Assert.Same(original, data);
        Assert.Throws<ArgumentException>(() => BSplCLibCollections.Reverse(ref data, 0)); Assert.Same(original, data);
        CopiedArray<double> nonfinite = new([0, double.NaN, 1]); var bad = nonfinite;
        Assert.Throws<OcctException>(() => BSplCLibCollections.Reverse(ref nonfinite)); Assert.Same(bad, nonfinite);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplineDefinitionsRoundTripIn2dAnd3d(bool rational)
    {
        CopiedArray<Coordinates3d> p3 = new([new(0, 0, 0), new(1, 2, 0), new(3, 0, 0)]);
        CopiedArray<Point2d> p2 = new([new(0, 0), new(1, 2), new(3, 0)]);
        CopiedArray<double> weights = new([1, 2, 1]), knots = new([0, 1]); CopiedArray<int> mults = new([3, 3]);
        using var c3 = rational ? GeomBSplineCurveCollections.Create(p3, weights, knots, mults, 2, false, true)
            : GeomBSplineCurveCollections.Create(p3, knots, mults, 2, false);
        using var c2 = rational ? Geom2dBSplineCurveCollections.Create(p2, weights, knots, mults, 2, false)
            : Geom2dBSplineCurveCollections.Create(p2, knots, mults, 2, false);
        Assert.Equal(p3.ToArray(), GeomBSplineCurveCollections.Poles(c3).ToArray()); Assert.Equal(p2.ToArray(), Geom2dBSplineCurveCollections.Poles(c2).ToArray());
        Assert.Equal(new double[] { 0, 1 }, GeomBSplineCurveCollections.Knots(c3).ToArray());
        Assert.Equal(ExpectedMultiplicities, Geom2dBSplineCurveCollections.Multiplicities(c2).ToArray());
        Assert.Equal(rational ? 2 : 1, GeomBSplineCurveCollections.WeightsArray(c3)[2]);
        var saved = GeomBSplineCurveCollections.Knots(c3);
        GeomBSplineCurveCollections.SetKnots(c3, new([2, 4])); Assert.Equal(0, saved[1]); Assert.Equal(2, GeomBSplineCurveCollections.Knots(c3)[1]);
        Geom2dBSplineCurveCollections.InsertKnots(c2, new([0.5]), new([1]), 1e-9, false); Assert.Equal(3, c2.NbKnots());
    }

    [Fact]
    public void CurveOutputBuffersAreReplacedOnlyOnSuccess()
    {
        using var c = GeomBezierCurveCollections.Create(new([new Coordinates3d(0, 0, 0), new Coordinates3d(1, 2, 3)]));
        CopiedArray<Coordinates3d> output = new([new(9, 9, 9)]); var original = output;
        Assert.Throws<ArgumentException>(() => GeomBezierCurveCollections.Poles(c, ref output)); Assert.Same(original, output);
        output = new([new(9, 9, 9), new(8, 8, 8)]); GeomBezierCurveCollections.Poles(c, ref output);
        Assert.Equal(new Coordinates3d(0, 0, 0), output[1]);
        c.Dispose(); Assert.Equal(new Coordinates3d(1, 2, 3), output[2]);
        Assert.Throws<ObjectDisposedException>(() => GeomBezierCurveCollections.Poles(c));
    }

    [Fact]
    public void ConstructorsAndBulkEditsRejectMismatchedAndInvalidData()
    {
        CopiedArray<Coordinates3d> poles = new([new(0, 0, 0), new(1, 2, 0), new(2, 0, 0)]);
        Assert.Throws<ArgumentException>(() => GeomBSplineCurveCollections.Create(poles, new([0, 1]), new([2, 2]), 2, false));
        Assert.Throws<ArgumentException>(() => GeomBSplineCurveCollections.Create(poles, new([0, 1]), new([3, 3]), 0, false));
        Assert.Throws<ArgumentException>(() => GeomBezierCurveCollections.Create(poles, new([1, 0, 1])));
        Assert.Throws<OcctException>(() => GeomBezierCurveCollections.Create(new([new Coordinates3d(double.NaN, 0, 0), new Coordinates3d(0, 0, 0)])));
        using var c = GeomBSplineCurveCollections.Create(poles, new([0, 1]), new([3, 3]), 2, false);
        Assert.Throws<ArgumentException>(() => GeomBSplineCurveCollections.SetKnots(c, new([1, 0])));
        Assert.Throws<ArgumentException>(() => GeomBSplineCurveCollections.InsertKnots(c, new([0.5]), new([1, 1]), 1e-9, false));
        Assert.Equal(2, c.NbKnots());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FittingInitializesReinitializesAndRetainsIndependentResults(int mode)
    {
        CopiedArray<Coordinates3d> p3 = new([new(0, 0, 0), new(1, 1, 0), new(2, 0, 0), new(3, 1, 0)]);
        CopiedArray<Point2d> p2 = new([new(0, 0), new(1, 1), new(2, 0), new(3, 1)]);
        using var f3 = GeomAPIPointsToBSplineCollections.Create(); using var f2 = Geom2dAPIPointsToBSplineCollections.Create();
        Assert.False(f3.IsDone()); Assert.Throws<ArgumentException>(() => f3.Curve());
        f3.Init(p3, (ApproxParametrizationType)mode, 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4);
        f2.Init(p2, (ApproxParametrizationType)mode, 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4);
        Assert.True(f3.IsDone()); Assert.True(f2.IsDone());
        using var c3 = f3.Curve(); using var c2 = f2.Curve();
        f3.Init(p3, new CopiedArray<double>([0, 1, 2, 3]), 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4);
        f2.Init(p2, new CopiedArray<double>([0, 1, 2, 3]), 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4);
        f3.Dispose(); f3.Dispose(); f2.Dispose();
        Assert.True(GeomBSplineCurveCollections.Poles(c3).Count >= 2); Assert.True(Geom2dBSplineCurveCollections.Poles(c2).Count >= 2);
        Assert.Throws<ObjectDisposedException>(() => f3.IsDone());
    }

    [Fact]
    public void FittingRejectsUnsafeParameterArraysBeforeKernelAccess()
    {
        using var fitting = GeomAPIPointsToBSplineCollections.Create();
        CopiedArray<Coordinates3d> points = new([new(0, 0, 0), new(1, 1, 0), new(2, 0, 0)]);
        Assert.Throws<ArgumentException>(() => fitting.Init(points, new CopiedArray<double>([0, 1]), 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4));
        Assert.Throws<ArgumentException>(() => fitting.Init(points, new CopiedArray<double>([0, 0, 1]), 2, 3, GeomAbsShape.GeomAbs_C1, 1e-4));
        Assert.Throws<ArgumentException>(() => fitting.Init(points, 4, 2, GeomAbsShape.GeomAbs_C1, 1e-4));
        Assert.False(fitting.IsDone());
    }

    [Fact]
    public void FittingConstructorsCoverParametersSmoothingAndSampledOrdinates()
    {
        CopiedArray<Coordinates3d> p3 = new([new(0, 0, 0), new(1, 1, 0), new(2, 0, 0), new(3, 1, 0), new(4, 0, 0)]);
        CopiedArray<Point2d> p2 = new([new(0, 0), new(1, 1), new(2, 0), new(3, 1), new(4, 0)]);
        CopiedArray<double> parameters = new([0, 1, 2, 3, 4]);
        using var parameterized3 = GeomAPIPointsToBSplineCollections.Create(p3, ApproxParametrizationType.Approx_ChordLength, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        using var explicit3 = GeomAPIPointsToBSplineCollections.Create(p3, parameters, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        using var smooth3 = GeomAPIPointsToBSplineCollections.Create(p3, 1.0, 0.1, 0.1, 8, GeomAbsShape.GeomAbs_C1, 0.1);
        using var parameterized2 = Geom2dAPIPointsToBSplineCollections.Create(p2, ApproxParametrizationType.Approx_Centripetal, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        using var explicit2 = Geom2dAPIPointsToBSplineCollections.Create(p2, parameters, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        using var smooth2 = Geom2dAPIPointsToBSplineCollections.Create(p2, 1.0, 0.1, 0.1, 8, GeomAbsShape.GeomAbs_C1, 0.1);
        using var ordinates = Geom2dAPIPointsToBSplineCollections.Create(new CopiedArray<double>([0, 1, 0, 1, 0]), 0.0, 1.0, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        Assert.True(parameterized3.IsDone()); Assert.True(explicit3.IsDone()); Assert.True(smooth3.IsDone());
        Assert.True(parameterized2.IsDone()); Assert.True(explicit2.IsDone()); Assert.True(smooth2.IsDone()); Assert.True(ordinates.IsDone());
        smooth3.Init(p3, 1.0, 0.1, 0.1, 8, GeomAbsShape.GeomAbs_C1, 0.1);
        smooth2.Init(p2, 1.0, 0.1, 0.1, 8, GeomAbsShape.GeomAbs_C1, 0.1);
        ordinates.Init(new CopiedArray<double>([0, 1, 0, 1, 0]), 10.0, 2.0, 2, 4, GeomAbsShape.GeomAbs_C1, 1e-3);
        using var sampled = ordinates.Curve(); Assert.Equal(10, sampled.StartPoint().X, 6);
    }

    [Fact]
    public void BothCurveFamiliesExposeAllDefinitionAndRefCopyVariants()
    {
        CopiedArray<Coordinates3d> p3 = new([new(0, 0, 0), new(1, 2, 0), new(3, 0, 0)]);
        CopiedArray<Point2d> p2 = new([new(0, 0), new(1, 2), new(3, 0)]);
        using var b3 = GeomBezierCurveCollections.Create(p3, new([1, 2, 1]));
        using var b2 = Geom2dBezierCurveCollections.Create(p2);
        Assert.Equal(new double[] { 0, 1 }, GeomBezierCurveCollections.Knots(b3).ToArray());
        Assert.Equal(ExpectedMultiplicities, GeomBezierCurveCollections.Multiplicities(b3).ToArray());
        Assert.Equal(6, GeomBezierCurveCollections.KnotSequence(b3).Count);
        Assert.Equal(6, Geom2dBezierCurveCollections.KnotSequence(b2).Count);
        Assert.Equal(2, Geom2dBezierCurveCollections.Knots(b2).Count);
        Assert.Equal(ExpectedMultiplicities, Geom2dBezierCurveCollections.Multiplicities(b2).ToArray());
        Assert.Equal(1, Geom2dBezierCurveCollections.WeightsArray(b2)[2]);
        Geom2dBezierCurveCollections.Poles(b2, ref p2); Assert.Equal(3, p2.Count);
        using var s3 = GeomBSplineCurveCollections.Create(p3, new([0, 1]), new([3, 3]), 2, false);
        using var s2 = Geom2dBSplineCurveCollections.Create(p2, new([0, 1]), new([3, 3]), 2, false);
        CopiedArray<double> knots3 = new([9, 9]), knots2 = new([9, 9]), flat3 = new([0, 0, 0, 0, 0, 0]), flat2 = new([0, 0, 0, 0, 0, 0]);
        CopiedArray<int> mult3 = new([0, 0]), mult2 = new([0, 0]);
        GeomBSplineCurveCollections.Knots(s3, ref knots3); Geom2dBSplineCurveCollections.Knots(s2, ref knots2);
        GeomBSplineCurveCollections.Multiplicities(s3, ref mult3); Geom2dBSplineCurveCollections.Multiplicities(s2, ref mult2);
        GeomBSplineCurveCollections.KnotSequence(s3, ref flat3); Geom2dBSplineCurveCollections.KnotSequence(s2, ref flat2);
        GeomBSplineCurveCollections.Poles(s3, ref p3); Geom2dBSplineCurveCollections.Poles(s2, ref p2);
        Assert.Equal(knots3.ToArray(), knots2.ToArray()); Assert.Equal(mult3.ToArray(), mult2.ToArray()); Assert.Equal(flat3.ToArray(), flat2.ToArray());
        Geom2dBSplineCurveCollections.SetKnots(s2, new([4, 8])); Assert.Equal(4, Geom2dBSplineCurveCollections.Knots(s2)[1]);
        CopiedArray<Point2d> reversed2 = new([new(0, 0), new(1, 1)]); BSplCLibCollections.Reverse(ref reversed2, 2); Assert.Equal(new Point2d(1, 1), reversed2[1]);
        CopiedArray<double> weights = new([1, 2, 3]); BSplCLibCollections.Reverse(ref weights, 3); Assert.Equal(3, weights[1]);
        CopiedArray<double> reversedKnots = new([0, 0.25, 1]); BSplCLibCollections.Reverse(ref reversedKnots); Assert.Equal(0.75, reversedKnots[2]);
    }

    [Fact]
    public void SequenceMergingCopiesEmptyAndSeededInputsAndPreservesFailedReference()
    {
        CopiedSequence<double> merged = new([]); var empty = merged;
        GeomLibCollections.FuseIntervals(new([0, 0.5, 1]), new([0, 0.25, 1]), ref merged, 1e-9, false);
        Assert.Equal(new double[] { 0, 0.25, 0.5, 1 }, merged.ToArray()); Assert.Empty(empty.ToArray());
        var saved = merged;
        Assert.Throws<ArgumentException>(() => GeomLibCollections.FuseIntervals(new([1, 0]), new([0, 1]), ref merged, 1e-9, false)); Assert.Same(saved, merged);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int CreateBuffer(double* data, int lower, int count, nint* output);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int Info(nint token, int* lower, int* count);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int Copy(nint token, double* data, int capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Release(nint token);

    [Fact]
    public unsafe void RawAbiChecksCountsCapacityTypeTagsStaleTokensAndNonReuse()
    {
        _ = OcctRuntime.Info; nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
            var create = Export<CreateBuffer>("occtsharp_copy_realarray_create"); var copy = Export<Copy>("occtsharp_copy_realarray_copy");
            var info = Export<Info>("occtsharp_copy_realarray_info"); var wrongInfo = Export<Info>("occtsharp_copy_integerarray_info");
            var release = Export<Release>("occtsharp_copy_realarray_release"); var wrongRelease = Export<Release>("occtsharp_copy_integerarray_release");
            double* data = stackalloc double[2] { 2, 3 }; nint token = 99;
            Assert.Equal(1, create(data, 1, 2, null)); Assert.Equal(1, create(null, 1, 2, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, int.MaxValue, 2, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, int.MaxValue, 1, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, int.MinValue, 1, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, 1, -1, &token)); Assert.Equal(1, create(null, 1, 0, &token));
            Assert.Equal(0, create(data, -2, 2, &token)); nint first = token;
            try
            {
                data[0] = 99; int lower = 0, count = 0;
                Assert.Equal(0, info(token, &lower, &count)); Assert.Equal(-2, lower); Assert.Equal(2, count);
                Assert.Equal(1, wrongInfo(token, &lower, &count)); wrongRelease(token);
                double* output = stackalloc double[2] { -7, -8 };
                Assert.Equal(1, copy(token, output, 1)); Assert.Equal(-7, output[0]); Assert.Equal(-8, output[1]);
                Assert.Equal(1, copy(token, null, 2)); Assert.Equal(0, copy(token, output, 2)); Assert.Equal(2, output[0]);
            }
            finally { release(token); release(token); }
            int l = 0, n = 0; Assert.Equal(1, info(first, &l, &n)); Assert.Equal(0, create(data, 1, 2, &token)); Assert.NotEqual(first, token); release(token);
        }
        finally { NativeLibrary.Free(library); }
    }
}
