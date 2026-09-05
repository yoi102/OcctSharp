using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public static class SketchModeling
{
    public static unsafe SketchEvaluation Evaluate(SketchCurve2d curve, double parameter, bool normalized = true)
    {
        ArgumentNullException.ThrowIfNull(curve);
        if (!double.IsFinite(parameter) || normalized && (parameter < 0.0 || parameter > 1.0))
            throw new ArgumentOutOfRangeException(nameof(parameter));
        double native = normalized ? curve.ToNative(parameter) : parameter;
        if (native < curve.FirstParameter || native > curve.LastParameter)
            throw new ArgumentOutOfRangeException(nameof(parameter), "The parameter is outside the bounded curve.");
        return curve.WithRaw(raw =>
        {
            NativeError.ThrowIfFailed(NativeMethods.SketchCurveEvaluate(raw, native, out SketchEvaluationRaw result), "sketch_curve_evaluate");
            return new SketchEvaluation(
                new(result.Point.X, result.Point.Y), new(result.Derivative.X, result.Derivative.Y),
                result.Parameter, curve.ToNormalized(result.Parameter));
        });
    }

    public static unsafe IReadOnlyList<SketchProjection> Project(SketchCurve2d curve, SketchPoint2d point)
    {
        ArgumentNullException.ThrowIfNull(curve); point.Validate(nameof(point));
        return curve.WithRaw(raw =>
        {
            NativeError.ThrowIfFailed(NativeMethods.SketchCurveProject(raw, new(point.X, point.Y), null, 0, out int count), "sketch_curve_project_count");
            SketchProjectionRaw[] values = new SketchProjectionRaw[count];
            fixed (SketchProjectionRaw* pointer = values)
                NativeError.ThrowIfFailed(NativeMethods.SketchCurveProject(raw, new(point.X, point.Y), pointer, values.Length, out count), "sketch_curve_project");
            return values.Take(count).Select(value => new SketchProjection(
                new(value.Point.X, value.Point.Y), value.Parameter,
                curve.ToNormalized(value.Parameter), value.Distance)).ToArray();
        });
    }

    public static unsafe IReadOnlyList<SketchIntersection> Intersect(
        SketchCurve2d first, SketchCurve2d second, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(first); ArgumentNullException.ThrowIfNull(second);
        ValidateTolerance(tolerance, nameof(tolerance));
        return ReferenceEquals(first, second)
            ? first.WithRaw(firstRaw => ReadIntersections(firstRaw, null))
            : first.WithRaw(firstRaw => second.WithRaw(secondRaw => ReadIntersections(firstRaw, secondRaw)));

        SketchIntersection[] ReadIntersections(SketchCurveRaw* firstRaw, SketchCurveRaw* secondRaw)
        {
            NativeError.ThrowIfFailed(NativeMethods.SketchCurveIntersect(firstRaw, secondRaw, tolerance, null, 0, out int count), "sketch_curve_intersect_count");
            SketchIntersectionRaw[] values = new SketchIntersectionRaw[count];
            fixed (SketchIntersectionRaw* pointer = values)
                NativeError.ThrowIfFailed(NativeMethods.SketchCurveIntersect(firstRaw, secondRaw, tolerance, pointer, values.Length, out count), "sketch_curve_intersect");
            return values.Take(count).Select(value => new SketchIntersection(
                new(value.Point.X, value.Point.Y), value.FirstParameter, value.SecondParameter,
                first.ToNormalized(value.FirstParameter), second.ToNormalized(value.SecondParameter))).ToArray();
        }
    }

    public static unsafe Shape CreateEdge(SketchCurve2d curve, SketchPlane plane)
    {
        ArgumentNullException.ThrowIfNull(curve); ArgumentNullException.ThrowIfNull(plane);
        return curve.WithRaw(raw =>
        {
            SketchPlaneRaw planeRaw = plane.ToRaw();
            NativeError.ThrowIfFailed(NativeMethods.SketchCurveMakeEdge(raw, &planeRaw, out nint shape), "sketch_curve_make_edge");
            return ShapeFactory.FromNativeHandle(shape, "sketch_curve_make_edge");
        });
    }

    internal static void ValidateTolerance(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0.0) throw new ArgumentOutOfRangeException(name, "Tolerance must be finite and positive.");
    }
}

public enum SketchDiagnosticCode
{
    Gap = 1,
    DuplicateCurve = 2,
    ZeroLength = 3,
    SelfIntersection = 4,
    OpenChain = 5,
    AmbiguousNesting = 6
}

public readonly record struct SketchDiagnostic(
    SketchDiagnosticCode Code, string Message, int FirstCurveIndex = -1, int SecondCurveIndex = -1);

public sealed class SketchValidationException : ArgumentException
{
    internal SketchValidationException(IReadOnlyList<SketchDiagnostic> diagnostics)
        : base(string.Join(" ", diagnostics.Select(value => value.Message))) => Diagnostics = diagnostics;
    public IReadOnlyList<SketchDiagnostic> Diagnostics { get; }
}

public readonly record struct SketchBounds2d(SketchPoint2d Minimum, SketchPoint2d Maximum);

public readonly record struct SketchLoopMeasurement(
    double Perimeter, double SignedArea, bool IsCounterClockwise, SketchBounds2d Bounds);

/// <summary>An ordered immutable mixed-curve chain with copied validation diagnostics.</summary>
public sealed class SketchCurveChain2d
{
    private readonly SketchCurve2d[] curves;
    private SketchCurveChain2d(SketchCurve2d[] curves, bool isClosed, double tolerance)
        => (this.curves, IsClosed, Tolerance) = (curves, isClosed, tolerance);

    public IReadOnlyList<SketchCurve2d> Curves => Array.AsReadOnly((SketchCurve2d[])curves.Clone());
    public bool IsClosed { get; }
    public double Tolerance { get; }

    public static SketchCurveChain2d Create(
        IReadOnlyList<SketchCurve2d> curves, bool requireClosed = false, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(curves); SketchModeling.ValidateTolerance(tolerance, nameof(tolerance));
        if (curves.Count == 0) throw new ArgumentException("A sketch chain requires at least one curve.", nameof(curves));
        SketchCurve2d[] source = curves.Select(value => value ?? throw new ArgumentException("A sketch curve is null.", nameof(curves))).ToArray();
        List<SketchDiagnostic> diagnostics = FindLocalDefects(source, tolerance);
        if (diagnostics.Count != 0) throw new SketchValidationException(diagnostics);

        List<SketchCurve2d> ordered = [source[0]];
        List<SketchCurve2d> remaining = source.Skip(1).ToList();
        while (remaining.Count > 0)
        {
            SketchPoint2d end = ordered[^1].Evaluate(1.0).Point;
            int match = -1; bool reverse = false;
            for (int index = 0; index < remaining.Count; ++index)
            {
                SketchPoint2d start = remaining[index].Evaluate(0.0).Point;
                SketchPoint2d candidateEnd = remaining[index].Evaluate(1.0).Point;
                if (end.DistanceTo(start) <= tolerance) { match = index; break; }
                if (end.DistanceTo(candidateEnd) <= tolerance) { match = index; reverse = true; break; }
            }
            if (match < 0)
            {
                // A shuffled open chain can start in its interior. Extend the other end
                // before concluding that the input contains a gap.
                SketchPoint2d start = ordered[0].Evaluate(0.0).Point;
                for (int index = 0; index < remaining.Count; ++index)
                {
                    SketchCurve2d candidate = remaining[index];
                    if (start.DistanceTo(candidate.Evaluate(1).Point) <= tolerance)
                    { match = index; reverse = false; break; }
                    if (start.DistanceTo(candidate.Evaluate(0).Point) <= tolerance)
                    { match = index; reverse = true; break; }
                }
                if (match < 0)
                    throw new SketchValidationException([new(SketchDiagnosticCode.Gap, "The mixed sketch curves cannot be ordered into one connected chain.")]);
                SketchCurve2d previous = remaining[match]; remaining.RemoveAt(match);
                ordered.Insert(0, reverse ? previous.Reverse() : previous);
                continue;
            }
            SketchCurve2d next = remaining[match]; remaining.RemoveAt(match);
            ordered.Add(reverse ? next.Reverse() : next);
        }

        bool closed = ordered.Count == 1
            ? ordered[0].Evaluate(0.0).Point.DistanceTo(ordered[0].Evaluate(1.0).Point) <= tolerance
            : ordered[0].Evaluate(0.0).Point.DistanceTo(ordered[^1].Evaluate(1.0).Point) <= tolerance;
        if (requireClosed && !closed)
            throw new SketchValidationException([new(SketchDiagnosticCode.OpenChain, "The ordered sketch chain is not closed.")]);
        diagnostics = FindSelfIntersections(ordered.ToArray(), closed, tolerance);
        if (diagnostics.Count != 0) throw new SketchValidationException(diagnostics);
        return new(ordered.ToArray(), closed, tolerance);
    }

    public IReadOnlyList<SketchDiagnostic> Inspect()
    {
        List<SketchDiagnostic> result = FindLocalDefects(curves, Tolerance);
        if (!IsClosed) result.Add(new(SketchDiagnosticCode.OpenChain, "The sketch chain is open."));
        result.AddRange(FindSelfIntersections(curves, IsClosed, Tolerance));
        return result;
    }

    public unsafe Shape BuildWire(SketchPlane plane)
    {
        ArgumentNullException.ThrowIfNull(plane);
        List<Shape> edges = new(curves.Length);
        try
        {
            foreach (SketchCurve2d curve in curves) edges.Add(curve.ToEdge(plane));
            return ShapeFactory.WithBorrowedShapeHandles(edges, (handles, count) =>
            {
                NativeError.ThrowIfFailed(NativeMethods.SketchMakeWire(handles, count, Tolerance, out nint wire), "sketch_make_wire");
                return ShapeFactory.FromNativeHandle(wire, "sketch_make_wire");
            });
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    public Shape Offset(
        SketchPlane plane, double distance, PlanarOffsetJoin join = PlanarOffsetJoin.Arc)
    {
        if (!double.IsFinite(distance) || Math.Abs(distance) <= Tolerance)
            throw new ArgumentOutOfRangeException(nameof(distance));
        using Shape wire = BuildWire(plane);
        return FreeformAuthoring.OffsetPlanarWire(wire, distance, 0.0, join);
    }

    public SketchLoopMeasurement Measure()
    {
        using Shape wire = BuildWire(SketchPlane.XY);
        double perimeter = wire.InspectProperties(InspectionPropertyKind.Length).Mass;
        BoundingBox3d bounds = wire.GetBoundingBox();
        double signedArea = IsClosed ? curves.Sum(IntegrateArea) : 0.0;
        return new(perimeter, signedArea, signedArea > 0.0,
            new(new(bounds.Minimum.X, bounds.Minimum.Y), new(bounds.Maximum.X, bounds.Maximum.Y)));
    }

    internal bool Contains(SketchPoint2d point, double tolerance)
    {
        using Shape wire = BuildWire(SketchPlane.XY);
        NativeError.ThrowIfFailed(NativeMethods.SketchWireContains(wire.Handle,
            new(point.X, point.Y), tolerance, out int inside), "sketch_wire_contains");
        return inside != 0;
    }

    internal SketchCurveChain2d Reverse()
    {
        SketchCurve2d[] reversed = curves.Reverse().Select(value => value.Reverse()).ToArray();
        return new(reversed, IsClosed, Tolerance);
    }

    private static List<SketchDiagnostic> FindLocalDefects(SketchCurve2d[] curves, double tolerance)
    {
        List<SketchDiagnostic> result = [];
        for (int index = 0; index < curves.Length; ++index)
            if (curves[index].Kind is SketchCurveKind.Bezier or SketchCurveKind.BSpline
                && curves[index].Intersect(curves[index], tolerance).Any(value =>
                    Math.Abs(value.FirstNormalizedParameter - value.SecondNormalizedParameter) > 1e-7
                    && !(Math.Min(value.FirstNormalizedParameter, value.SecondNormalizedParameter) <= 1e-7
                        && Math.Max(value.FirstNormalizedParameter, value.SecondNormalizedParameter) >= 1 - 1e-7)))
                result.Add(new(SketchDiagnosticCode.SelfIntersection, $"Curve {index} intersects itself.", index, index));
        for (int first = 0; first < curves.Length; ++first)
        {
            SketchPoint2d firstStart = curves[first].Evaluate(0.0).Point;
            SketchPoint2d firstEnd = curves[first].Evaluate(1.0).Point;
            if (curves[first].Kind == SketchCurveKind.Segment && firstStart.DistanceTo(firstEnd) <= tolerance)
                result.Add(new(SketchDiagnosticCode.ZeroLength, $"Curve {first} has zero length.", first));
            for (int second = first + 1; second < curves.Length; ++second)
            {
                SketchPoint2d secondStart = curves[second].Evaluate(0.0).Point;
                SketchPoint2d secondEnd = curves[second].Evaluate(1.0).Point;
                bool sameEnds = firstStart.DistanceTo(secondStart) <= tolerance && firstEnd.DistanceTo(secondEnd) <= tolerance;
                bool reversedEnds = firstStart.DistanceTo(secondEnd) <= tolerance && firstEnd.DistanceTo(secondStart) <= tolerance;
                if ((sameEnds || reversedEnds)
                    && curves[first].Evaluate(0.5).Point.DistanceTo(curves[second].Evaluate(0.5).Point) <= tolerance)
                    result.Add(new(SketchDiagnosticCode.DuplicateCurve, $"Curves {first} and {second} are duplicates.", first, second));
            }
        }
        return result;
    }

    private static List<SketchDiagnostic> FindSelfIntersections(
        SketchCurve2d[] curves, bool closed, double tolerance)
    {
        List<SketchDiagnostic> result = [];
        for (int first = 0; first < curves.Length; ++first)
        for (int second = first + 1; second < curves.Length; ++second)
        {
            bool adjacent = second == first + 1 || closed && first == 0 && second == curves.Length - 1;
            foreach (SketchIntersection intersection in curves[first].Intersect(curves[second], tolerance))
            {
                bool sharedEnd = adjacent &&
                    (intersection.FirstNormalizedParameter <= 1e-7 || intersection.FirstNormalizedParameter >= 1.0 - 1e-7) &&
                    (intersection.SecondNormalizedParameter <= 1e-7 || intersection.SecondNormalizedParameter >= 1.0 - 1e-7);
                if (!sharedEnd)
                {
                    result.Add(new(SketchDiagnosticCode.SelfIntersection,
                        $"Curves {first} and {second} intersect inside the chain.", first, second));
                    break;
                }
            }
        }
        return result;
    }

    private static double IntegrateArea(SketchCurve2d curve)
    {
        // Green's theorem on the curve, subdivided at every B-spline knot. Fixed
        // display tessellation is not an area measurement or a nesting classifier.
        double[] breaks = curve.Knots.Select(curve.ToNormalized)
            .Select(value => curve.Reversed ? 1 - value : value)
            .Where(value => value > 0 && value < 1).Append(0).Append(1).Order().Distinct().ToArray();
        double total = 0;
        for (int index = 0; index + 1 < breaks.Length; ++index)
        {
            double a = breaks[index], b = breaks[index + 1], middle = (a + b) / 2;
            // Evaluate one-sided at knot breaks so a piecewise-linear spline's
            // derivative on the next span does not pollute the previous span.
            double fa = Value(a == 0 ? a : Math.BitIncrement(a));
            double fb = Value(b == 1 ? b : Math.BitDecrement(b));
            double fm = Value(middle);
            double whole = (b - a) * (fa + 4 * fm + fb) / 6;
            total += Integrate(a, b, fa, fm, fb, whole, 1e-10 * Math.Max(1, Math.Abs(whole)), 20);
        }
        return total;

        double Value(double parameter)
        {
            SketchEvaluation value = curve.Evaluate(parameter);
            return 0.5 * (value.Point.X * value.FirstDerivative.Y - value.Point.Y * value.FirstDerivative.X)
                * (curve.LastParameter - curve.FirstParameter);
        }

        double Integrate(double a, double b, double fa, double fm, double fb,
            double whole, double tolerance, int depth)
        {
            double middle = (a + b) / 2;
            double leftMiddle = Value((a + middle) / 2), rightMiddle = Value((middle + b) / 2);
            double left = (middle - a) * (fa + 4 * leftMiddle + fm) / 6;
            double right = (b - middle) * (fm + 4 * rightMiddle + fb) / 6;
            double error = left + right - whole;
            if (Math.Abs(error) <= 15 * tolerance) return left + right + error / 15;
            if (depth == 0) throw new InvalidOperationException("Sketch area integration did not converge.");
            return Integrate(a, middle, fa, leftMiddle, fm, left, tolerance / 2, depth - 1)
                + Integrate(middle, b, fm, rightMiddle, fb, right, tolerance / 2, depth - 1);
        }
    }
}

/// <summary>One closed outer loop and zero or more nested hole loops.</summary>
public sealed class SketchProfile2d
{
    private readonly SketchCurveChain2d[] holes;
    private SketchProfile2d(SketchCurveChain2d outer, SketchCurveChain2d[] holes)
        => (Outer, this.holes) = (outer, holes);
    public SketchCurveChain2d Outer { get; }
    public IReadOnlyList<SketchCurveChain2d> Holes => Array.AsReadOnly((SketchCurveChain2d[])holes.Clone());

    public static SketchProfile2d Create(
        SketchCurveChain2d outer, IReadOnlyList<SketchCurveChain2d>? holes = null,
        double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(outer); SketchModeling.ValidateTolerance(tolerance, nameof(tolerance));
        if (!outer.IsClosed) throw new SketchValidationException([new(SketchDiagnosticCode.OpenChain, "The profile outer loop is open.")]);
        SketchCurveChain2d[] copiedHoles = holes?.ToArray() ?? [];
        if (copiedHoles.Any(value => value is null || !value.IsClosed))
            throw new SketchValidationException([new(SketchDiagnosticCode.OpenChain, "Every profile hole must be a closed loop.")]);
        SketchCurveChain2d normalizedOuter = outer.Measure().SignedArea < 0.0 ? outer.Reverse() : outer;
        for (int index = 0; index < copiedHoles.Length; ++index)
            if (copiedHoles[index].Measure().SignedArea > 0.0) copiedHoles[index] = copiedHoles[index].Reverse();
        ValidateNesting(normalizedOuter, copiedHoles, tolerance);
        return new(normalizedOuter, copiedHoles);
    }

    public static SketchProfile2d Classify(
        IReadOnlyList<SketchCurveChain2d> loops, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(loops);
        SketchModeling.ValidateTolerance(tolerance, nameof(tolerance));
        if (loops.Count == 0) throw new ArgumentException("At least one loop is required.", nameof(loops));
        if (loops.Any(value => value is null)) throw new ArgumentException("A profile loop is null.", nameof(loops));
        SketchCurveChain2d[] ordered = loops.OrderByDescending(value => Math.Abs(value.Measure().SignedArea)).ToArray();
        return Create(ordered[0], ordered.Skip(1).ToArray(), tolerance);
    }

    public unsafe Shape CreateFace(SketchPlane plane)
    {
        ArgumentNullException.ThrowIfNull(plane);
        using Shape outerWire = Outer.BuildWire(plane);
        List<Shape> innerWires = [];
        try
        {
            foreach (SketchCurveChain2d hole in holes) innerWires.Add(hole.BuildWire(plane));
            nint[] handles = new nint[innerWires.Count]; bool[] references = new bool[innerWires.Count];
            try
            {
                for (int index = 0; index < innerWires.Count; ++index)
                {
                    innerWires[index].Handle.DangerousAddRef(ref references[index]);
                    handles[index] = innerWires[index].Handle.DangerousGetHandle();
                }
                fixed (nint* pointer = handles)
                {
                    NativeError.ThrowIfFailed(NativeMethods.SketchProfileMakeFace(
                        outerWire.Handle, handles.Length == 0 ? null : pointer, handles.Length, out nint face), "sketch_profile_make_face");
                    return ShapeFactory.FromNativeHandle(face, "sketch_profile_make_face");
                }
            }
            finally { for (int index = references.Length - 1; index >= 0; --index) if (references[index]) innerWires[index].Handle.DangerousRelease(); }
        }
        finally { foreach (Shape wire in innerWires) wire.Dispose(); }
    }

    public Shape Extrude(SketchPlane plane, double distance)
    {
        if (!double.IsFinite(distance) || Math.Abs(distance) <= 1e-12) throw new ArgumentOutOfRangeException(nameof(distance));
        using Shape face = CreateFace(plane);
        using GpVec direction = GpVec.Create(plane.Normal.X * distance, plane.Normal.Y * distance, plane.Normal.Z * distance);
        return face.Extrude(direction);
    }

    public Shape Revolve(
        SketchPlane plane, SketchPoint2d axisOrigin, SketchDirection2d axisDirection,
        double angleRadians = Math.Tau)
    {
        ArgumentNullException.ThrowIfNull(plane); axisDirection.Validate();
        if (!double.IsFinite(angleRadians) || Math.Abs(angleRadians) <= 1e-12 || Math.Abs(angleRadians) > Math.Tau + 1e-12)
            throw new ArgumentOutOfRangeException(nameof(angleRadians));
        GpPoint origin = plane.ToWorld(axisOrigin); GpXyz direction = plane.ToWorldDirection(axisDirection);
        using Shape face = CreateFace(plane);
        using GpAx1 axis = GpAx1.Create(origin.X, origin.Y, origin.Z, direction.X, direction.Y, direction.Z);
        return face.Revolve(axis, angleRadians);
    }

    public Shape AddTo(Shape target, SketchPlane plane, double distance)
    {
        ArgumentNullException.ThrowIfNull(target); using Shape feature = Extrude(plane, distance); return target.Fuse(feature);
    }
    public Shape CutFrom(Shape target, SketchPlane plane, double distance)
    {
        ArgumentNullException.ThrowIfNull(target); using Shape feature = Extrude(plane, distance); return target.Cut(feature);
    }

    public static XdeLabel AddToXde(XdeDocument document, Shape result, XdePartMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(result); ArgumentNullException.ThrowIfNull(metadata);
        return document.AddPart(result, metadata);
    }

    public static string WriteStep(Shape result, string filePath, XdePartMetadata metadata) =>
        WriteExchange(result, filePath, metadata, XdeExchangeFormat.Step);
    public static string WriteIges(Shape result, string filePath, XdePartMetadata metadata) =>
        WriteExchange(result, filePath, metadata, XdeExchangeFormat.Iges);

    private static string WriteExchange(Shape result, string filePath, XdePartMetadata metadata, XdeExchangeFormat format)
    {
        ArgumentNullException.ThrowIfNull(result); ArgumentNullException.ThrowIfNull(metadata);
        using XdeDocument document = XdeDocument.Create();
        using (XdeTransaction transaction = document.BeginTransaction("Create planar feature"))
        { document.AddPart(result, metadata); transaction.Commit(); }
        return format == XdeExchangeFormat.Step ? document.WriteStep(filePath) : document.WriteIges(filePath);
    }

    private static void ValidateNesting(
        SketchCurveChain2d outer, SketchCurveChain2d[] holes, double tolerance)
    {
        for (int index = 0; index < holes.Length; ++index)
        {
            SketchPoint2d testPoint = holes[index].Curves[0].Evaluate(0).Point;
            if (!outer.Contains(testPoint, tolerance))
                throw new SketchValidationException([new(SketchDiagnosticCode.AmbiguousNesting, $"Hole loop {index} is not inside the outer loop.")]);
            for (int other = index + 1; other < holes.Length; ++other)
            {
                foreach (SketchCurve2d firstCurve in holes[index].Curves)
                foreach (SketchCurve2d secondCurve in holes[other].Curves)
                    if (firstCurve.Intersect(secondCurve, tolerance).Count != 0)
                        throw new SketchValidationException([new(SketchDiagnosticCode.AmbiguousNesting, "Hole loops intersect or touch.", index, other)]);
                SketchPoint2d otherPoint = holes[other].Curves[0].Evaluate(0).Point;
                if (holes[index].Contains(otherPoint, tolerance) || holes[other].Contains(testPoint, tolerance))
                    throw new SketchValidationException([new(SketchDiagnosticCode.AmbiguousNesting, "Nested or overlapping hole loops are ambiguous.", index, other)]);
            }
            foreach (SketchCurve2d outerCurve in outer.Curves)
            foreach (SketchCurve2d holeCurve in holes[index].Curves)
                if (outerCurve.Intersect(holeCurve, tolerance).Count != 0)
                    throw new SketchValidationException([new(SketchDiagnosticCode.AmbiguousNesting, $"Hole loop {index} intersects the outer loop.")]);
        }
    }

}

#pragma warning restore CS1591
