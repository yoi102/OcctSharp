namespace OcctSharp;

#pragma warning disable CS1591

public static partial class SurfaceModeling
{
    public static SketchPoint2d NormalizeUv(SurfaceDescriptor surface, SketchPoint2d uv)
    {
        ArgumentNullException.ThrowIfNull(surface); uv.Validate(nameof(uv));
        return new(surface.IsUPeriodic ? Normalize(uv.X, surface.Bounds.FirstU, surface.UPeriod) : uv.X,
            surface.IsVPeriodic ? Normalize(uv.Y, surface.Bounds.FirstV, surface.VPeriod) : uv.Y);
    }

    public static SketchPoint2d ShiftUv(SurfaceDescriptor surface, SketchPoint2d uv, int uPeriods, int vPeriods)
    {
        ArgumentNullException.ThrowIfNull(surface); uv.Validate(nameof(uv));
        if (uPeriods != 0 && !surface.IsUPeriodic || vPeriods != 0 && !surface.IsVPeriodic)
            throw new ArgumentException("A non-periodic surface direction cannot be shifted by a period.");
        SketchPoint2d result = new(uv.X + uPeriods * surface.UPeriod, uv.Y + vPeriods * surface.VPeriod);
        result.Validate(nameof(uv)); return result;
    }

    public static IReadOnlyList<SurfaceUvShift> UnwrapUv(SurfaceDescriptor surface, IReadOnlyList<SketchPoint2d> points)
    {
        ArgumentNullException.ThrowIfNull(surface); ArgumentNullException.ThrowIfNull(points);
        SketchPoint2d[] copied = points.ToArray(); SurfaceUvShift[] results = new SurfaceUvShift[copied.Length];
        for (int index = 0; index < copied.Length; ++index)
        {
            copied[index].Validate(nameof(points));
            SketchPoint2d value = copied[index]; double uShift = 0, vShift = 0;
            if (index != 0)
            {
                if (surface.IsUPeriodic) uShift = NearestShift(results[index - 1].Uv.X, value.X, surface.UPeriod);
                if (surface.IsVPeriodic) vShift = NearestShift(results[index - 1].Uv.Y, value.Y, surface.VPeriod);
                value = new(value.X + uShift * surface.UPeriod, value.Y + vShift * surface.VPeriod);
                value.Validate(nameof(points));
            }
            results[index] = new(value, uShift, vShift);
        }
        return Array.AsReadOnly(results);
    }

    /// <summary>Projects every input point and unwraps the nearest solutions in sequence; a missing group fails atomically.</summary>
    public static IReadOnlyList<SurfaceTracePoint> TracePoints(Shape face, IReadOnlyList<GpPoint> points,
        double maximumDistance = double.PositiveInfinity, bool limitToFace = true, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (double.IsNaN(maximumDistance) || maximumDistance < 0) throw new ArgumentOutOfRangeException(nameof(maximumDistance));
        GpPoint[] copied = points.ToArray(); SurfaceDescriptor descriptor = Describe(face);
        IReadOnlyList<SurfacePointSolution> solutions = ProjectPoints(face, copied, limitToFace, tolerance);
        Dictionary<int, SurfacePointSolution[]> groups = solutions.GroupBy(value => value.SourceIndex)
            .ToDictionary(group => group.Key, group => group.OrderBy(value => value.Distance).ToArray());
        SurfaceTracePoint[] result = new SurfaceTracePoint[copied.Length];
        for (int index = 0; index < result.Length; ++index)
        {
            if (!groups.TryGetValue(index, out SurfacePointSolution[]? group) || group[0].Distance > maximumDistance)
                throw new InvalidOperationException($"Surface tracing has no acceptable projection for input point {index}.");
            SurfacePointSolution selected = group[0]; SketchPoint2d uv = selected.Uv;
            if (index != 0)
            {
                SurfaceTracePoint previous = result[index - 1]; double best = double.PositiveInfinity;
                foreach (SurfacePointSolution candidate in group.Where(value => value.Distance <= group[0].Distance + tolerance))
                {
                    SketchPoint2d unwrapped = UnwrapUv(descriptor, [previous.Uv, candidate.Uv])[1].Uv;
                    double distance = unwrapped.DistanceTo(previous.Uv);
                    if (distance < best) { best = distance; selected = candidate; uv = unwrapped; }
                }
            }
            result[index] = new(index, uv, selected.Point, selected.Distance);
        }
        return Array.AsReadOnly(result);
    }

    private static double Normalize(double value, double origin, double period)
    {
        ValidateTolerance(period);
        double remainder = (value % period - origin % period) % period;
        if (remainder < 0) remainder += period;
        double result = origin + remainder;
        if (!double.IsFinite(result)) throw new ArgumentOutOfRangeException(nameof(value));
        return result;
    }

    private static double NearestShift(double previous, double value, double period)
    {
        ValidateTolerance(period); double shift = Math.Round((previous - value) / period, MidpointRounding.ToEven);
        if (!double.IsFinite(shift)) throw new ArgumentOutOfRangeException(nameof(value));
        return shift;
    }
}
