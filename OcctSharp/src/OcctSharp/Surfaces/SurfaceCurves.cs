using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public static partial class SurfaceModeling
{
    public static SurfaceCurveDefinition GetCurveDefinition(Shape face, Shape edge, int seamBranch = 0, double tolerance = 1e-7) =>
        ReadEdgeCurve(face, edge, seamBranch, false, tolerance);

    /// <summary>Derives UV geometry for a 3D edge already on the surface; off-surface curves fail.</summary>
    public static SurfaceCurveDefinition DeriveCurveDefinition(Shape face, Shape edge, double tolerance = 1e-6) =>
        ReadEdgeCurve(face, edge, 0, true, tolerance);

    private static unsafe SurfaceCurveDefinition ReadEdgeCurve(Shape face, Shape edge, int branch, bool derive, double tolerance)
    {
        ValidateShape(face); ValidateShape(edge); ValidateTolerance(tolerance);
        if (branch is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(branch));
        return ReadCurve((out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles, double* weights, int poleCount,
            double* knots, int* multiplicities, int knotCount) => NativeMethods.SurfaceCurveDefinition(
                face.Handle, edge.Handle, branch, derive ? 1 : 0, tolerance, out info,
                poles, weights, poleCount, knots, multiplicities, knotCount), branch);
    }

    public static SurfaceCurveDefinition InterpolateUv(IReadOnlyList<SketchPoint2d> points, bool periodic = false, double tolerance = 1e-7) =>
        FitUv(points, true, periodic, 2, 8, FreeformContinuity.C2, tolerance);

    public static SurfaceCurveDefinition ApproximateUv(
        IReadOnlyList<SketchPoint2d> points, int minimumDegree = 3, int maximumDegree = 8,
        double tolerance = 1e-5, FreeformContinuity continuity = FreeformContinuity.C2) =>
        FitUv(points, false, false, minimumDegree, maximumDegree, continuity, tolerance);

    private static unsafe SurfaceCurveDefinition FitUv(
        IReadOnlyList<SketchPoint2d> points, bool interpolate, bool periodic, int minimumDegree, int maximumDegree,
        FreeformContinuity continuity, double tolerance)
    {
        ValidateTolerance(tolerance); SketchPoint2dRaw[] inputs = CopyUv(points);
        if (inputs.Length < 3) throw new ArgumentException("Smooth UV fitting needs at least three points.", nameof(points));
        fixed (SketchPoint2dRaw* pinned = inputs)
        {
            SketchPoint2dRaw* input = pinned;
            return ReadCurve((out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles, double* weights, int poleCount,
                double* knots, int* multiplicities, int knotCount) => NativeMethods.SurfaceFitUv(
                    input, inputs.Length, interpolate ? 1 : 0, periodic ? 1 : 0, minimumDegree, maximumDegree, (int)continuity, tolerance,
                    out info, poles, weights, poleCount, knots, multiplicities, knotCount));
        }
    }

    /// <summary>Offsets in UV parameter units, not world-space distance. Approximation residual is in UV units.</summary>
    public static unsafe SurfaceCurveDefinition OffsetUv(SketchCurve2d curve, double distance, double tolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(curve); ValidateTolerance(tolerance);
        if (!double.IsFinite(distance) || distance == 0) throw new ArgumentOutOfRangeException(nameof(distance));
        return curve.WithRaw(raw => ReadCurve((out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles,
            double* weights, int poleCount, double* knots, int* multiplicities, int knotCount) =>
                NativeMethods.SurfaceOffsetUv(raw, distance, tolerance, out info, poles, weights, poleCount, knots, multiplicities, knotCount)));
    }

    public static unsafe Shape LiftCurve(Shape face, SketchCurve2d curve, bool build3d = true, double tolerance = 1e-7)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(curve); ValidateTolerance(tolerance);
        return curve.WithRaw(raw =>
        {
            Check(NativeMethods.SurfaceLiftCurve(face.Handle, raw, build3d ? 1 : 0, tolerance, out nint shape), "lift_curve");
            return Own(shape, "lift_curve");
        });
    }

    public static unsafe IReadOnlyList<SurfaceCurveSample> SampleCurve(Shape face, Shape edge, int count, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateShape(edge); ValidateTolerance(tolerance);
        if (count is < 2 or > 1_000_000) throw new ArgumentOutOfRangeException(nameof(count));
        SurfaceCurveSampleRaw[] samples = new SurfaceCurveSampleRaw[count];
        fixed (SurfaceCurveSampleRaw* output = samples)
            Check(NativeMethods.SurfaceSampleCurve(face.Handle, edge.Handle, count, tolerance, output), "sample_curve");
        return Array.AsReadOnly(samples.Select(value => new SurfaceCurveSample(
            value.Parameter, ToUv(value.Uv), ToPoint(value.Point), ToVector(value.Tangent))).ToArray());
    }

    public static unsafe IReadOnlyList<SurfaceCurveIntersection> IntersectCurve(Shape face, Shape edge, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateShape(edge); ValidateTolerance(tolerance);
        Check(NativeMethods.SurfaceIntersectCurve(face.Handle, edge.Handle, tolerance, null, 0, out int count), "intersect_curve_count");
        SurfaceIntersectionRaw[] records = new SurfaceIntersectionRaw[count];
        fixed (SurfaceIntersectionRaw* output = records)
            Check(NativeMethods.SurfaceIntersectCurve(face.Handle, edge.Handle, tolerance, output, records.Length, out count), "intersect_curve");
        return Array.AsReadOnly(records.Take(count).Select(value => new SurfaceCurveIntersection(
            (SurfaceIntersectionKind)value.Kind, new(value.FirstParameter, value.LastParameter),
            ToPoint(value.FirstPoint), ToPoint(value.LastPoint), ToUv(value.FirstUv), ToUv(value.LastUv), (SurfaceDomainState)value.State)).ToArray());
    }

    private unsafe delegate NativeStatus CurveReadOperation(out SurfaceCurveInfoRaw info,
        SketchPoint2dRaw* poles, double* weights, int poleCount, double* knots, int* multiplicities, int knotCount);

    private static unsafe SurfaceCurveDefinition ReadCurve(CurveReadOperation operation, int branch = 0)
    {
        Check(operation(out SurfaceCurveInfoRaw info, null, null, 0, null, null, 0), "curve_definition_count");
        if (info.PoleCount < 2 || info.PoleCount > 1_000_000 || info.KnotCount < 2 || info.KnotCount > 1_000_000)
            throw new InvalidOperationException("The native curve definition returned invalid array sizes.");
        SketchPoint2dRaw[] poles = new SketchPoint2dRaw[info.PoleCount]; double[] weights = new double[poles.Length];
        double[] knots = new double[info.KnotCount]; int[] multiplicities = new int[knots.Length];
        fixed (SketchPoint2dRaw* polePointer = poles)
        fixed (double* weightPointer = weights)
        fixed (double* knotPointer = knots)
        fixed (int* multiplicityPointer = multiplicities)
            Check(operation(out info, polePointer, weightPointer, poles.Length, knotPointer, multiplicityPointer, knots.Length), "curve_definition");
        SketchCurve2d curve = SketchCurve2d.BSpline(poles.Select(ToUv).ToArray(), knots, multiplicities,
            info.Degree, info.Periodic != 0, weights.All(value => value == 1) ? null : weights);
        if (info.First > curve.FirstParameter || info.Last < curve.LastParameter)
            curve = curve.Trim((info.First - curve.FirstParameter) / (curve.LastParameter - curve.FirstParameter),
                (info.Last - curve.FirstParameter) / (curve.LastParameter - curve.FirstParameter));
        if (info.Reversed != 0) curve = curve.Reverse();
        return new(curve, new(info.SourceFirst, info.SourceLast), info.Exact != 0, info.ParameterPreserved != 0, info.Residual, branch);
    }
}
