using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Copied surface/UV inspection and independently owning curve/topology workflows.</summary>
public static partial class SurfaceModeling
{
    public static SurfaceDescriptor Describe(Shape face)
    {
        ValidateShape(face);
        Check(NativeMethods.SurfaceDescribe(face.Handle, out SurfaceInfoRaw raw), "describe");
        return new((SurfaceGeometryType)raw.Kind, new(raw.FirstU, raw.LastU, raw.FirstV, raw.LastV),
            raw.Orientation == 1, raw.ClosedU != 0, raw.ClosedV != 0, raw.PeriodicU != 0, raw.PeriodicV != 0,
            raw.PeriodU, raw.PeriodV);
    }

    public static SurfaceEvaluationPoint Evaluate(Shape face, SketchPoint2d uv, bool normalized = false, double tolerance = 1e-7) =>
        EvaluateMany(face, [uv], normalized, tolerance)[0];

    public static unsafe IReadOnlyList<SurfaceEvaluationPoint> EvaluateMany(
        Shape face, IReadOnlyList<SketchPoint2d> points, bool normalized = false, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateTolerance(tolerance);
        SketchPoint2dRaw[] inputs = CopyUv(points);
        if (normalized)
        {
            SurfaceParameterBounds bounds = Describe(face).Bounds;
            for (int index = 0; index < inputs.Length; ++index)
            {
                SketchPoint2dRaw value = inputs[index];
                if (value.X < 0 || value.X > 1 || value.Y < 0 || value.Y > 1)
                    throw new ArgumentOutOfRangeException(nameof(points), "Normalized UV values must be in [0,1].");
                inputs[index] = new(bounds.FirstU + value.X * (bounds.LastU - bounds.FirstU),
                    bounds.FirstV + value.Y * (bounds.LastV - bounds.FirstV));
            }
        }
        SurfaceSampleRaw[] output = new SurfaceSampleRaw[inputs.Length];
        fixed (SketchPoint2dRaw* input = inputs)
        fixed (SurfaceSampleRaw* results = output)
            Check(NativeMethods.SurfaceEvaluate(face.Handle, input, inputs.Length, tolerance, results), "evaluate");
        return Array.AsReadOnly(output.Select(raw => new SurfaceEvaluationPoint(
            ToUv(raw.Uv), ToPoint(raw.Point), ToVector(raw.Du), ToVector(raw.Dv),
            raw.NormalDefined == 0 ? null : ToVector(raw.Normal), raw.Singular != 0, (SurfaceDomainState)raw.State,
            raw.CurvatureDefined == 0 ? null : raw.MinimumCurvature, raw.CurvatureDefined == 0 ? null : raw.MaximumCurvature,
            raw.CurvatureDefined == 0 ? null : raw.MeanCurvature, raw.CurvatureDefined == 0 ? null : raw.GaussianCurvature)).ToArray());
    }

    public static SurfaceDomainState Classify(Shape face, SketchPoint2d uv, double tolerance = 1e-7) => ClassifyMany(face, [uv], tolerance)[0];

    public static unsafe IReadOnlyList<SurfaceDomainState> ClassifyMany(Shape face, IReadOnlyList<SketchPoint2d> points, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateTolerance(tolerance); SketchPoint2dRaw[] inputs = CopyUv(points);
        int[] states = new int[inputs.Length];
        fixed (SketchPoint2dRaw* input = inputs)
        fixed (int* output = states)
            Check(NativeMethods.SurfaceClassify(face.Handle, input, inputs.Length, tolerance, output), "classify");
        return Array.AsReadOnly(states.Select(value => (SurfaceDomainState)value).ToArray());
    }

    public static SurfaceGrid SampleGrid(Shape face, int uCount, int vCount, double tolerance = 1e-7)
    {
        if (uCount < 2 || vCount < 2 || (long)uCount * vCount > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(uCount), "A grid needs at least 2x2 and at most one million samples.");
        SketchPoint2d[] points = new SketchPoint2d[uCount * vCount];
        for (int v = 0; v < vCount; ++v)
        for (int u = 0; u < uCount; ++u)
            points[v * uCount + u] = new((double)u / (uCount - 1), (double)v / (vCount - 1));
        return new(uCount, vCount, EvaluateMany(face, points, normalized: true, tolerance));
    }

    public static IReadOnlyList<SurfacePointSolution> ProjectPoint(Shape face, GpPoint point, bool limitToFace = true, double tolerance = 1e-7) =>
        ProjectPoints(face, [point], limitToFace, tolerance);

    public static unsafe IReadOnlyList<SurfacePointSolution> ProjectPoints(
        Shape face, IReadOnlyList<GpPoint> points, bool limitToFace = true, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateTolerance(tolerance); ArgumentNullException.ThrowIfNull(points);
        if (points.Count > 100_000) throw new ArgumentOutOfRangeException(nameof(points));
        XyzRaw[] inputs = points.Select(point => { ValidatePoint(point); return ShapeFactory.ToRaw(point); }).ToArray();
        fixed (XyzRaw* input = inputs)
        {
            Check(NativeMethods.SurfaceProjectPoints(face.Handle, input, inputs.Length, limitToFace ? 1 : 0, tolerance, null, 0, out int count), "project_points_count");
            SurfacePointSolutionRaw[] values = new SurfacePointSolutionRaw[count];
            fixed (SurfacePointSolutionRaw* output = values)
                Check(NativeMethods.SurfaceProjectPoints(face.Handle, input, inputs.Length, limitToFace ? 1 : 0, tolerance, output, values.Length, out count), "project_points");
            return Array.AsReadOnly(values.Take(count).Select(value => new SurfacePointSolution(
                value.SourceIndex, ToUv(value.Uv), ToPoint(value.Point), value.Distance, (SurfaceDomainState)value.State)).ToArray());
        }
    }

    public static Shape CreateIsoEdge(Shape face, SurfaceIsoDirection direction, double parameter, ParameterRange range)
    {
        ValidateShape(face); range.Validate(nameof(range));
        if (!Enum.IsDefined(direction) || !double.IsFinite(parameter)) throw new ArgumentOutOfRangeException(nameof(direction));
        Check(NativeMethods.SurfaceIso(face.Handle, (int)direction, parameter, range.First, range.Last, out nint shape), "iso");
        return Own(shape, "iso");
    }

    private static void ValidateShape(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape); ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
    }
    private static void ValidateTolerance(double tolerance) => SketchModeling.ValidateTolerance(tolerance, nameof(tolerance));
    private static void ValidatePoint(GpPoint point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z)) throw new ArgumentOutOfRangeException(nameof(point));
    }
    private static SketchPoint2dRaw[] CopyUv(IReadOnlyList<SketchPoint2d> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count > 1_000_000) throw new ArgumentOutOfRangeException(nameof(points));
        return points.Select(point => { point.Validate(nameof(points)); return new SketchPoint2dRaw(point.X, point.Y); }).ToArray();
    }
    private static GpPoint ToPoint(XyzRaw point) => new(point.X, point.Y, point.Z);
    private static GpXyz ToVector(XyzRaw point) => new(point.X, point.Y, point.Z);
    private static SketchPoint2d ToUv(SketchPoint2dRaw point) => new(point.X, point.Y);
    private static void Check(NativeStatus status, string operation) => NativeError.ThrowIfFailed(status, "surface_" + operation);
    private static Shape Own(nint shape, string operation) => ShapeFactory.FromNativeHandle(shape, "surface_" + operation);
}
