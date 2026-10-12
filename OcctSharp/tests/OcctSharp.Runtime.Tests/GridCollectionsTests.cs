using System.Runtime.InteropServices;
using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Values;
using Workflow = OcctSharp.Tests.Shared.BatchAGSurfaceInterpolationWorkflow;

namespace OcctSharp.Runtime.Tests;

public sealed class GridCollectionsTests
{
    private static readonly double[] UnitWeights = [1, 1, 1, 1, 1, 1];
    private static readonly double[] RationalWeights = [1, 2, 3, 2, 1, 4];
    private static readonly double[] Knots = [0, 1];
    private static readonly int[] UMults = [2, 2], VMults = [3, 3];
    private static GeomBSplineSurface Spline(bool rational = false) => rational
        ? GeomBSplineSurfaceCollections.Create(Workflow.Grid(2, 3), new(RationalWeights, 2, 3), new(Knots), new(Knots), new(UMults), new(VMults), 1, 2, false, false)
        : GeomBSplineSurfaceCollections.Create(Workflow.Grid(2, 3), new(Knots), new(Knots), new(UMults), new(VMults), 1, 2, false, false);
    [Fact] public void CompletePackageWorkflow() => Workflow.Run();
    [Fact]
    public void ManagedGridChecksBothAxesCopiesDataAndRejectsOverflow()
    {
        double[] values = [1, 2, 3, 4, 5, 6]; var grid = new CopiedGrid<double>(values, 2, 3, -2, 4);
        values[0] = 99; var output = grid.ToArray(); output[1] = 98;
        Assert.Equal(1, grid[-2, 4]); Assert.Equal(6, grid[-1, 6]);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid[-2, 7]);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid[-3, 4]);
        Assert.Throws<ArgumentException>(() => new CopiedGrid<double>(values, 3, 3));
        Assert.Throws<ArgumentException>(() => new CopiedGrid<double>([], 0, 0));
        Assert.Throws<ArgumentException>(() => new CopiedGrid<double>(values, int.MaxValue, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedGrid<double>(values, 2, 3, int.MaxValue - 2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedGrid<double>(values, 2, 3, 1, int.MinValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CopiedGrid<double>(values, 2, 3, int.MinValue + 1, 1));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void NonSquareSurfaceDefinitionsAndNativePoleReadsAgree(bool rational)
    {
        var input = Workflow.Grid(2, 3, -2, 4);
        using var bezier = rational ? GeomBezierSurfaceCollections.Create(input, new(RationalWeights, 2, 3, -2, 4)) : GeomBezierSurfaceCollections.Create(input);
        using var spline = Spline(rational);
        var saved = GeomBezierSurfaceCollections.Poles(bezier);
        Assert.Equal(input.ToArray(), saved.ToArray()); Assert.Equal(input.ToArray(), GeomBSplineSurfaceCollections.Poles(spline).ToArray());
        for (int u = 1; u <= 2; ++u) foreach (int v in new[] { 1, 3 })
        {
            var native = bezier.EvalD0(u - 1, (v - 1) / 2.0); var expected = saved[u, v];
            Assert.Equal(expected.X, native.X); Assert.Equal(expected.Y, native.Y); Assert.Equal(expected.Z, native.Z);
        }
        Assert.Equal(rational ? RationalWeights : UnitWeights, GeomBezierSurfaceCollections.WeightsArray(bezier).ToArray());
        Assert.Equal(rational ? RationalWeights : UnitWeights, GeomBSplineSurfaceCollections.WeightsArray(spline).ToArray());
        bezier.Dispose(); spline.Dispose(); Assert.Equal(new Coordinates3d(2, 6, .3), saved[2, 3]);
    }
    [Fact]
    public void GridCopyOutReportsKernelReplacementBoundsAndPublishesOnlyOnSuccess()
    {
        using var surface = Spline(true);
        var poles = Workflow.Grid(2, 3, -4, 7); var original = poles;
        GeomBSplineSurfaceCollections.Poles(surface, ref poles);
        Assert.NotSame(original, poles); Assert.Equal(1, poles.RowLower); Assert.Equal(1, poles.ColumnLower);
        Assert.Equal(Workflow.Grid(2, 3).ToArray(), poles.ToArray());
        CopiedGrid<double> weights = new(UnitWeights, 2, 3, -4, 7);
        GeomBSplineSurfaceCollections.Weights(surface, ref weights); Assert.Equal(RationalWeights, weights.ToArray());
        var wrong = Workflow.Grid(3, 2); var unchanged = wrong;
        Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.Poles(surface, ref wrong)); Assert.Same(unchanged, wrong);
        using var bezier = GeomBezierSurfaceCollections.Create(Workflow.Grid(2, 3), new(RationalWeights, 2, 3));
        GeomBezierSurfaceCollections.Poles(bezier, ref poles); GeomBezierSurfaceCollections.Weights(bezier, ref weights);
        Assert.Equal(RationalWeights, weights.ToArray());
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void PartialRowAndColumnEditsPreserveNativeIndicesAndWeights(bool spline)
    {
        using var s = Spline(); using var b = GeomBezierSurfaceCollections.Create(Workflow.Grid(2, 3));
        CopiedArray<Coordinates3d> row = new([new(10, 20, 30)], 2), col = new([new(11, 21, 31)], 2);
        if (spline)
        {
            GeomBSplineSurfaceCollections.SetPoleRow(s, 1, row, new([2.0], 2));
            GeomBSplineSurfaceCollections.SetPoleCol(s, 3, col); GeomBSplineSurfaceCollections.SetWeightCol(s, 3, new([3.0], 2));
            GeomBSplineSurfaceCollections.SetPoleRow(s, 2, row); GeomBSplineSurfaceCollections.SetPoleCol(s, 1, col, new([4.0], 2));
            GeomBSplineSurfaceCollections.SetWeightRow(s, 1, new([5.0], 2));
            Assert.Equal(new Coordinates3d(10, 20, 30), GeomBSplineSurfaceCollections.Poles(s)[1, 2]);
            Assert.Equal(5, GeomBSplineSurfaceCollections.WeightsArray(s)[1, 2]);
            Assert.Equal(new Coordinates3d(11, 21, 31), GeomBSplineSurfaceCollections.Poles(s)[2, 3]);
            Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.SetPoleCol(s, 4, col));
            Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.SetPoleRow(s, 1, row, new([0.0], 2)));
        }
        else
        {
            GeomBezierSurfaceCollections.SetPoleRow(b, 1, row, new([2.0], 2));
            GeomBezierSurfaceCollections.SetPoleCol(b, 3, col); GeomBezierSurfaceCollections.SetWeightCol(b, 3, new([1.0, 3]));
            GeomBezierSurfaceCollections.SetPoleRow(b, 2, row); GeomBezierSurfaceCollections.SetPoleCol(b, 1, new([new Coordinates3d(0, 0, 0), new(11, 21, 31)]), new([1.0, 4]));
            GeomBezierSurfaceCollections.SetWeightRow(b, 1, new([5.0], 2));
            Assert.Equal(new Coordinates3d(10, 20, 30), GeomBezierSurfaceCollections.Poles(b)[1, 2]);
            Assert.Equal(5, GeomBezierSurfaceCollections.WeightsArray(b)[1, 2]);
            Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.SetPoleRow(b, 1, new([new Coordinates3d(0, 0, 0)], 4)));
            Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.SetPoleCol(b, 1, col, new([1.0], 2)));
        }
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void BezierRowAndColumnInsertionClosesAllBeforeAfterOverloads(bool rational)
    {
        using var surface = GeomBezierSurfaceCollections.Create(Workflow.Grid(2, 3));
        CopiedArray<Coordinates3d> row = new([new(4, 0, 0), new(4, 3, 0), new(4, 6, 0)]);
        if (rational) { GeomBezierSurfaceCollections.InsertPoleRowBefore(surface, 2, row, new([1.0, 2, 1])); GeomBezierSurfaceCollections.InsertPoleRowAfter(surface, 3, row, new([1.0, 1, 2])); }
        else { GeomBezierSurfaceCollections.InsertPoleRowBefore(surface, 2, row); GeomBezierSurfaceCollections.InsertPoleRowAfter(surface, 3, row); }
        CopiedArray<Coordinates3d> col = new([new(0, 9, 0), new(2, 9, 0), new(4, 9, 0), new(6, 9, 0)]);
        if (rational) { GeomBezierSurfaceCollections.InsertPoleColBefore(surface, 2, col, new([1.0, 2, 1, 2])); GeomBezierSurfaceCollections.InsertPoleColAfter(surface, 4, col, new([2.0, 1, 2, 1])); }
        else { GeomBezierSurfaceCollections.InsertPoleColBefore(surface, 2, col); GeomBezierSurfaceCollections.InsertPoleColAfter(surface, 4, col); }
        Assert.Equal(4, surface.NbUPoles()); Assert.Equal(5, surface.NbVPoles());
        Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.InsertPoleColBefore(surface, 0, col));
        Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.InsertPoleRowAfter(surface, 1, row));
        Assert.Equal(Knots, GeomBezierSurfaceCollections.UKnots(surface).ToArray()); Assert.Equal(Knots, GeomBezierSurfaceCollections.VKnots(surface).ToArray());
        Assert.Equal(2, GeomBezierSurfaceCollections.UMultiplicities(surface).Count); Assert.Equal(2, GeomBezierSurfaceCollections.VMultiplicities(surface).Count);
        Assert.Equal(8, GeomBezierSurfaceCollections.UKnotSequence(surface).Count); Assert.Equal(10, GeomBezierSurfaceCollections.VKnotSequence(surface).Count);
    }
    [Fact]
    public void SplineKnotReadsOutputBuffersInsertionAndReparameterizationAgree()
    {
        using var surface = Spline();
        var uk = new CopiedArray<double>([0, 0]); var vk = new CopiedArray<double>([0, 0]);
        var um = new CopiedArray<int>([0, 0]); var vm = new CopiedArray<int>([0, 0]);
        var uf = new CopiedArray<double>(new double[4]); var vf = new CopiedArray<double>(new double[6]);
        GeomBSplineSurfaceCollections.UKnots(surface, ref uk); GeomBSplineSurfaceCollections.VKnots(surface, ref vk);
        GeomBSplineSurfaceCollections.UMultiplicities(surface, ref um); GeomBSplineSurfaceCollections.VMultiplicities(surface, ref vm);
        GeomBSplineSurfaceCollections.UKnotSequence(surface, ref uf); GeomBSplineSurfaceCollections.VKnotSequence(surface, ref vf);
        Assert.Equal(Knots, uk.ToArray()); Assert.Equal(Knots, vk.ToArray()); Assert.Equal(UMults, um.ToArray()); Assert.Equal(VMults, vm.ToArray());
        Assert.Equal(uf.ToArray(), GeomBSplineSurfaceCollections.UKnotSequence(surface).ToArray()); Assert.Equal(vf.ToArray(), GeomBSplineSurfaceCollections.VKnotSequence(surface).ToArray());
        GeomBSplineSurfaceCollections.InsertUKnots(surface, new([.4]), new([1]), 1e-9, false);
        GeomBSplineSurfaceCollections.InsertVKnots(surface, new([.6]), new([1]), 1e-9, false);
        Assert.Equal(3, surface.NbUKnots()); Assert.Equal(3, surface.NbVKnots());
        GeomBSplineSurfaceCollections.SetUKnots(surface, new([2.0, 3, 4])); GeomBSplineSurfaceCollections.SetVKnots(surface, new([5.0, 6, 7]));
        Assert.Equal(2, GeomBSplineSurfaceCollections.UKnots(surface)[1]); Assert.Equal(5, GeomBSplineSurfaceCollections.VKnots(surface)[1]);
        var old = uk; Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.UKnots(surface, ref uk)); Assert.Same(old, uk);
        Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.SetUKnots(surface, new([4.0, 3, 2])));
    }
    [Fact]
    public void PeriodicSurfaceHasMatchingSeamAndIndependentFace()
    {
        var values = new Coordinates3d[12];
        for (int u = 0; u < 4; ++u) for (int v = 0; v < 3; ++v) values[u*3+v] = new(Math.Cos(u*Math.PI/2), Math.Sin(u*Math.PI/2), v);
        using var surface = GeomBSplineSurfaceCollections.Create(new(values, 4, 3), new([0.0, 1, 2, 3, 4]), new(Knots), new([1, 1, 1, 1, 1]), new(VMults), 2, 2, true, false);
        Assert.True(surface.IsUPeriodic()); var a = surface.EvalD0(0, .5); var b = surface.EvalD0(4, .5);
        Assert.Equal(a.X, b.X, 8); Assert.Equal(a.Y, b.Y, 8); Assert.Equal(a.Z, b.Z, 8);
        using Shape face = Workflow.Face(surface); surface.Dispose(); Assert.True(face.IsValid);
    }
    [Fact]
    public void InvalidSurfaceDefinitionsFailBeforeKernelAccess()
    {
        Assert.Throws<ArgumentException>(() => GeomBSplineSurfaceCollections.Create(Workflow.Grid(2, 3), new(Knots), new(Knots), new([3, 3]), new(VMults), 1, 2, false, false));
        Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.Create(Workflow.Grid(2, 3), new(UnitWeights, 3, 2)));
        Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.Create(Workflow.Grid(1, 3)));
        Assert.Throws<ArgumentException>(() => GeomBezierSurfaceCollections.Create(Workflow.Grid(2, 3), new([1.0, 1, 1, 1, 0, 1], 2, 3)));
        Assert.Throws<OcctException>(() => GeomBezierSurfaceCollections.Create(new([new Coordinates3d(double.NaN, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)], 2, 2)));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void ParameterizedPeriodicInterpolationAndTangentCopiesSurviveOwnerDisposal(bool twoDimensions)
    {
        if (twoDimensions)
        {
            using var algorithm = Geom2dAPIInterpolateCollections.Create(new([new Point2d(1, 0), new Point2d(0, 1), new Point2d(-1, 0), new Point2d(0, -1)]), new([0.0, 1, 2, 3, 4]), true, 1e-7);
            algorithm.Load(new CopiedArray<Vector2d>([new(0, 1), new(-1, 0), new(0, -1), new(1, 0)]), new([true, false, true, false]), false); algorithm.Perform();
            Assert.True(algorithm.IsDone()); using var curve = algorithm.Curve(); algorithm.Dispose(); Assert.True(curve.IsPeriodic());
            var a = curve.EvalD0(curve.FirstParameter()); var b = curve.EvalD0(curve.LastParameter()); Assert.Equal(a.X, b.X, 8); Assert.Equal(a.Y, b.Y, 8);
        }
        else
        {
            using var algorithm = GeomAPIInterpolateCollections.Create(new([new Coordinates3d(1, 0, 0), new(0, 1, 0), new(-1, 0, 0), new(0, -1, 0)]), new([0.0, 1, 2, 3, 4]), true, 1e-7);
            algorithm.Load(new Vector3d(0, 1, 0), new Vector3d(1, 0, 0), false); algorithm.Perform();
            Assert.True(algorithm.IsDone()); using var curve = algorithm.Curve(); algorithm.Dispose(); Assert.True(curve.IsPeriodic());
            using Shape edge = Workflow.Edge(curve); Assert.True(edge.IsValid);
        }
    }
    [Fact]
    public void InterpolationRejectsMalformedParallelInputsAndDisposedOwners()
    {
        CopiedArray<Coordinates3d> points = new([new(0, 0, 0), new(1, 2, 0), new(3, 0, 0)]);
        Assert.Throws<ArgumentException>(() => GeomAPIInterpolateCollections.Create(points, new([0.0, 1]), false, 1e-7));
        Assert.Throws<ArgumentException>(() => GeomAPIInterpolateCollections.Create(points, new([0.0, 1, 2]), true, 1e-7));
        Assert.Throws<ArgumentException>(() => GeomAPIInterpolateCollections.Create(new(points.ToArray(), -1), false, 1e-7));
        Assert.Throws<ArgumentException>(() => GeomAPIInterpolateCollections.Create(points, false, 0));
        Assert.Throws<ArgumentException>(() => GeomAPIInterpolateCollections.Create(new([new Coordinates3d(0, 0, 0), new(0, 0, 0)]), false, 1e-7));
        using var algorithm = GeomAPIInterpolateCollections.Create(points, false, 1e-7);
        Assert.False(algorithm.IsDone()); Assert.Throws<ArgumentException>(() => algorithm.Curve());
        Assert.Throws<ArgumentException>(() => algorithm.Load(new CopiedArray<Vector3d>([new(1, 0, 0)]), new([true]), false));
        Assert.Throws<ArgumentException>(() => algorithm.Load(new CopiedArray<Vector3d>([new(0, 0, 0), new(1, 0, 0), new(1, 0, 0)]), new([true, false, false]), false));
        Assert.Throws<ArgumentException>(() => algorithm.Load(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), false));
        algorithm.Perform(); using var curve = algorithm.Curve(); algorithm.Dispose(); algorithm.Dispose();
        Assert.Throws<ObjectDisposedException>(() => algorithm.Perform()); Assert.True(curve.NbPoles() >= 2);
    }
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void PointGridFittingConstructorsReinitializationAndInterpolation(int mode)
    {
        var points = Workflow.Grid(4, 5);
        using var algorithm = mode switch {
            0 => GeomAPIPointsToBSplineSurfaceCollections.Create(points, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5),
            1 => GeomAPIPointsToBSplineSurfaceCollections.Create(points, ApproxParametrizationType.Approx_ChordLength, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5),
            _ => GeomAPIPointsToBSplineSurfaceCollections.Create(points, 1.0, .1, .01, 8, GeomAbsShape.GeomAbs_C2, 1e-5)
        };
        Assert.True(algorithm.IsDone()); using var first = algorithm.Surface();
        if (mode == 0) algorithm.Init(points, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        else if (mode == 1) algorithm.Init(points, ApproxParametrizationType.Approx_ChordLength, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5, false);
        else algorithm.Init(points, 1.0, .1, .01, 8, GeomAbsShape.GeomAbs_C2, 1e-5);
        Assert.True(algorithm.IsDone());
        if (mode == 0) algorithm.Interpolate(points, false); else algorithm.Interpolate(points, ApproxParametrizationType.Approx_ChordLength, false);
        Assert.True(algorithm.IsDone()); using var result = algorithm.Surface(); algorithm.Dispose();
        using Shape face = Workflow.Face(result); Assert.True(face.IsValid); Assert.True(first.NbUPoles() >= 2);
    }
    [Fact]
    public void HeightFieldConstructorInitAndInterpolationKeepGridOrientation()
    {
        var points = new CopiedGrid<double>(new double[20], 4, 5);
        using var algorithm = GeomAPIPointsToBSplineSurfaceCollections.Create(points, 2, 3, 7, 4, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        Assert.True(algorithm.IsDone()); algorithm.Init(points, 2, 3, 7, 4, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        algorithm.Interpolate(points, 2, 3, 7, 4); Assert.True(algorithm.IsDone());
        using var result = algorithm.Surface(); algorithm.Dispose(); var p = result.EvalD0(2, 7);
        Assert.Equal(0, p.Z, 7); using Shape face = Workflow.Face(result); Assert.True(face.IsValid);
    }
    [Fact]
    public void FittingRejectsUnsafeBoundsEnumsDegreesAndSteps()
    {
        Assert.Throws<ArgumentException>(() => GeomAPIPointsToBSplineSurfaceCollections.Create(Workflow.Grid(4, 5, -2, 1), 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5));
        Assert.Throws<ArgumentException>(() => GeomAPIPointsToBSplineSurfaceCollections.Create(Workflow.Grid(), (ApproxParametrizationType)99, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5));
        Assert.Throws<ArgumentException>(() => GeomAPIPointsToBSplineSurfaceCollections.Create(Workflow.Grid(), 5, 3, GeomAbsShape.GeomAbs_C2, 1e-5));
        Assert.Throws<ArgumentException>(() => GeomAPIPointsToBSplineSurfaceCollections.Create(new CopiedGrid<double>(new double[20], 4, 5), 0, 0, 0, 1, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5));
    }
    [Fact]
    public void FlatHeightFallbackReinitializesIntoNonflatAndPointGridResults()
    {
        using var algorithm = GeomAPIPointsToBSplineSurfaceCollections.Create(new CopiedGrid<double>(new double[20], 4, 5), 2, 3, 7, 4, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        using var flat = algorithm.Surface(); Assert.Equal(3, flat.UDegree());
        var heights = new double[20]; for (int u=0; u<4; ++u) for (int v=0; v<5; ++v) heights[u*5+v] = .1*u + .2*v;
        algorithm.Init(new CopiedGrid<double>(heights, 4, 5), 2, 3, 7, 4, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        Assert.True(algorithm.IsDone()); using var nonflat = algorithm.Surface();
        Assert.True(double.IsFinite(nonflat.EvalD0(6, 12).Z));
        algorithm.Interpolate(new CopiedGrid<double>(heights, 4, 5), 2, 3, 7, 4);
        Assert.True(algorithm.IsDone()); using var interpolated = algorithm.Surface();
        Assert.True(double.IsFinite(interpolated.EvalD0(6, 12).Z));
        algorithm.Init(Workflow.Grid(4, 5), 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        Assert.True(algorithm.IsDone()); using var points = algorithm.Surface();
        Assert.True(double.IsFinite(points.EvalD0(.4, .6).Z));
        algorithm.Dispose(); Assert.Equal(0, flat.EvalD0(6, 12).Z, 8);
        Assert.Throws<ArgumentException>(() => GeomAPIPointsToBSplineSurfaceCollections.Create(new CopiedGrid<double>(new double[20], 4, 5), double.MaxValue, double.MaxValue, 0, 1, 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5));
    }
    [Fact]
    public void RetainedInterpolationPointsParametersAndFlagsSurviveGarbageCollection()
    {
        static GeomAPIInterpolateCollections CreateTemporaryInputs()
        {
            var result = GeomAPIInterpolateCollections.Create(new([new Coordinates3d(0, 0, 0), new(1, 2, 0), new(3, 0, 0)]), new([0.0, 1, 2]), false, 1e-7);
            result.Load(new CopiedArray<Vector3d>([new(1, 1, 0), new(0, 0, 0), new(1, -1, 0)]), new([true, false, true]), false);
            return result;
        }
        using var algorithm = CreateTemporaryInputs(); GC.Collect(); GC.WaitForPendingFinalizers();
        algorithm.Perform(); Assert.True(algorithm.IsDone()); using var curve = algorithm.Curve(); algorithm.Dispose();
        Assert.Equal(0, curve.EvalD0(0).X, 8); Assert.Equal(3, curve.EvalD0(2).X, 8);
    }
    [Fact]
    public async Task AlgorithmSafeHandleKeepsConcurrentCallsAndDisposalSafe()
    {
        for (int i=0; i<32; ++i)
        {
            using var algorithm = GeomAPIInterpolateCollections.Create(new([new Coordinates3d(0, 0, 0), new(1, 1, 0), new(2, 0, 0)]), false, 1e-7);
            var calls = Task.Run(() => { for (int n=0; n<16; ++n) { try { Assert.False(algorithm.IsDone()); } catch (ObjectDisposedException) { return; } } });
            algorithm.Dispose(); await calls;
        }
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int CreateGrid(double* data, int row, int rows, int col, int cols, nint* result);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int GridInfo(nint token, int* row, int* rows, int* col, int* cols);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int CopyGrid(nint token, double* data, int capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int CreateFlags(byte* data, int lower, int count, nint* result);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Release(nint token);
    [Fact]
    public unsafe void RawGridAbiRejectsOverflowCapacityWrongTypesStaleTokensAndMalformedBoolean()
    {
        _ = OcctRuntime.Info; nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
            var create = Export<CreateGrid>("occtsharp_copy_realgrid_create"); var info = Export<GridInfo>("occtsharp_copy_realgrid_info");
            var wrong = Export<GridInfo>("occtsharp_copy_point3dgrid_info"); var copy = Export<CopyGrid>("occtsharp_copy_realgrid_copy");
            var release = Export<Release>("occtsharp_copy_realgrid_release"); var wrongRelease = Export<Release>("occtsharp_copy_point3dgrid_release");
            var flags = Export<CreateFlags>("occtsharp_copy_booleanarray_create");
            double* data = stackalloc double[6] { 1, 2, 3, 4, 5, 6 }; nint token = 123;
            Assert.Equal(1, create(data, 1, 2, 1, 3, null)); Assert.Equal(1, create(null, 1, 2, 1, 3, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, 1, int.MaxValue, 1, 2, &token)); Assert.Equal(0, token);
            Assert.Equal(1, create(data, int.MaxValue - 2, 2, 1, 3, &token));
            Assert.Equal(1, create(data, int.MinValue + 1, 2, 1, 3, &token));
            Assert.Equal(1, create(data, 1, 2, int.MaxValue - 3, 3, &token));
            Assert.Equal(1, create(data, 1, 0, 1, 3, &token));
            Assert.Equal(0, create(data, -2, 2, 4, 3, &token)); nint first = token;
            try
            {
                data[0] = 99; int row = 0, rows = 0, col = 0, cols = 0;
                Assert.Equal(0, info(token, &row, &rows, &col, &cols)); Assert.Equal(-2, row); Assert.Equal(4, col); Assert.Equal(2, rows); Assert.Equal(3, cols);
                Assert.Equal(1, wrong(token, &row, &rows, &col, &cols)); wrongRelease(token);
                double* output = stackalloc double[6] { -7, -7, -7, -7, -7, -7 };
                Assert.Equal(1, copy(token, output, 5)); Assert.Equal(-7, output[0]); Assert.Equal(-7, output[5]);
                Assert.Equal(1, copy(token, null, 6)); Assert.Equal(0, copy(token, output, 6)); Assert.Equal(1, output[0]); Assert.Equal(6, output[5]);
            }
            finally { release(token); release(token); }
            int r=0, n=0, c=0, m=0; Assert.Equal(1, info(first, &r, &n, &c, &m));
            Assert.Equal(0, create(data, 1, 2, 1, 3, &token)); Assert.NotEqual(first, token); release(token);
            byte* invalid = stackalloc byte[2] { 0, 2 }; Assert.Equal(1, flags(invalid, 1, 2, &token)); Assert.Equal(0, token);
            invalid[1] = 1; Assert.Equal(0, flags(invalid, 1, 2, &token)); Export<Release>("occtsharp_copy_booleanarray_release")(token);
        }
        finally { NativeLibrary.Free(library); }
    }
}
