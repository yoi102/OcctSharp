using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Copied-definition, native-local freeform curve/surface and profile-to-solid workflows.</summary>
public static class FreeformAuthoring
{
    public static unsafe Shape CreateCurve(FreeformCurveDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        XyzRaw[] poles = CopyPoints(definition.CopyPoles());
        double[] weights = definition.CopyWeights(), knots = definition.CopyKnots();
        int[] multiplicities = definition.CopyMultiplicities();
        fixed (XyzRaw* polePointer = poles)
        fixed (double* weightPointer = weights)
        fixed (double* knotPointer = knots)
        fixed (int* multiplicityPointer = multiplicities)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformCurveCreate(
                (int)definition.Kind, polePointer, weights.Length == 0 ? null : weightPointer,
                poles.Length, knots.Length == 0 ? null : knotPointer,
                multiplicities.Length == 0 ? null : multiplicityPointer, knots.Length,
                definition.Degree, definition.Periodic ? 1 : 0, out nint edge), "freeform_curve_create");
            Shape result = ShapeFactory.FromNativeHandle(edge, "freeform_curve_create");
            if (definition.ParameterRange is not { } range) return result;
            try { Shape trimmed = SegmentCurve(result, range); result.Dispose(); return trimmed; }
            catch { result.Dispose(); throw; }
        }
    }

    public static unsafe Shape InterpolateCurve(
        IReadOnlyList<GpPoint> points, GpXyz? initialTangent = null, GpXyz? finalTangent = null,
        bool periodic = false, double tolerance = 1e-7)
    {
        XyzRaw[] rawPoints = CopyPoints(points, periodic ? 3 : 2);
        if (initialTangent.HasValue != finalTangent.HasValue)
            throw new ArgumentException("Initial and final tangents must be supplied together.");
        XyzRaw[] tangents = initialTangent is null ? [] : [ToRaw(initialTangent.Value), ToRaw(finalTangent!.Value)];
        ValidateTolerance(tolerance, nameof(tolerance));
        fixed (XyzRaw* pointPointer = rawPoints)
        fixed (XyzRaw* tangentPointer = tangents)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformCurveInterpolate(
                pointPointer, rawPoints.Length, tangents.Length == 0 ? null : tangentPointer,
                tangents.Length, periodic ? 1 : 0, tolerance, out nint edge), "freeform_curve_interpolate");
            return ShapeFactory.FromNativeHandle(edge, "freeform_curve_interpolate");
        }
    }

    public static unsafe Shape ApproximateCurve(
        IReadOnlyList<GpPoint> points, int minimumDegree = 3, int maximumDegree = 8,
        FreeformContinuity continuity = FreeformContinuity.C2, double tolerance = 1e-5)
    {
        XyzRaw[] rawPoints = CopyPoints(points, 2); ValidateDegreeRange(minimumDegree, maximumDegree); ValidateTolerance(tolerance, nameof(tolerance));
        fixed (XyzRaw* pointer = rawPoints)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformCurveApproximate(
                pointer, rawPoints.Length, minimumDegree, maximumDegree, (int)continuity,
                tolerance, out nint edge), "freeform_curve_approximate");
            return ShapeFactory.FromNativeHandle(edge, "freeform_curve_approximate");
        }
    }

    public static unsafe FreeformCurveDefinition GetCurveDefinition(Shape edge)
    {
        ValidateShape(edge);
        NativeError.ThrowIfFailed(NativeMethods.FreeformCurveInfo(edge.Handle, out FreeformCurveInfoRaw info), "freeform_curve_info");
        XyzRaw[] rawPoles = new XyzRaw[info.PoleCount]; double[] weights = new double[info.PoleCount];
        double[] knots = new double[info.KnotCount]; int[] multiplicities = new int[info.KnotCount];
        fixed (XyzRaw* polePointer = rawPoles)
        fixed (double* weightPointer = weights)
        fixed (double* knotPointer = knots)
        fixed (int* multiplicityPointer = multiplicities)
            NativeError.ThrowIfFailed(NativeMethods.FreeformCurveCopyDefinition(
                edge.Handle, polePointer, rawPoles.Length, weightPointer, weights.Length,
                info.KnotCount == 0 ? null : knotPointer, knots.Length,
                info.KnotCount == 0 ? null : multiplicityPointer, multiplicities.Length), "freeform_curve_copy_definition");
        GpPoint[] poles = rawPoles.Select(ToPoint).ToArray();
        double[]? rationalWeights = info.Rational == 0 ? null : weights;
        ParameterRange range = new(info.FirstParameter, info.LastParameter);
        return info.Kind == (int)FreeformGeometryKind.Bezier
            ? FreeformCurveDefinition.Bezier(poles, rationalWeights, range)
            : FreeformCurveDefinition.BSpline(poles, knots, multiplicities, info.Degree, info.Periodic != 0, rationalWeights, range);
    }

    public static Shape ElevateCurveDegree(Shape edge, int degree) => EditCurve(edge, 1, degree, 0.0, 0.0, "freeform_curve_elevate_degree");
    public static Shape ReverseCurve(Shape edge) => EditCurve(edge, 2, 0, 0.0, 0.0, "freeform_curve_reverse");
    public static Shape SegmentCurve(Shape edge, ParameterRange range)
    {
        range.Validate(nameof(range)); return EditCurve(edge, 3, 0, range.First, range.Last, "freeform_curve_segment");
    }

    public static unsafe IReadOnlyList<Shape> SplitCurve(Shape edge, IReadOnlyList<double> parameters)
    {
        ValidateShape(edge); ArgumentNullException.ThrowIfNull(parameters);
        double[] values = parameters.ToArray();
        for (int index = 0; index < values.Length; ++index)
            if (!double.IsFinite(values[index]) || index > 0 && values[index] <= values[index - 1])
                throw new ArgumentException("Split parameters must be finite and strictly increasing.", nameof(parameters));
        nint[] handles = new nint[values.Length + 1];
        fixed (double* parameterPointer = values)
        fixed (nint* handlePointer = handles)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformCurveSplit(edge.Handle,
                values.Length == 0 ? null : parameterPointer, values.Length, handlePointer,
                handles.Length, out int written), "freeform_curve_split");
            return OwnShapes(handles, written, "freeform_curve_split");
        }
    }

    public static IReadOnlyList<FreeformSolution> ProjectPoint(Shape edge, GpPoint point) =>
        CopySolutions(edge, null, point, 1);
    public static IReadOnlyList<FreeformSolution> CurveExtrema(Shape first, Shape second) =>
        CopySolutions(first, second, default, 2);
    public static IReadOnlyList<FreeformSolution> IntersectCurveWithFace(Shape edge, Shape face) =>
        CopySolutions(edge, face, default, 3);

    public static unsafe Shape CreateLocatedPlanarProfile(
        IReadOnlyList<GpPoint> localPoints, GpPoint origin, GpXyz normal, GpXyz xDirection,
        bool interpolate = false, double tolerance = 1e-7)
    {
        XyzRaw[] points = CopyPoints(localPoints, 3); ValidateTolerance(tolerance, nameof(tolerance));
        fixed (XyzRaw* pointer = points)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformPlanarProfile(pointer, points.Length,
                ShapeFactory.ToRaw(origin), ToRaw(normal), ToRaw(xDirection), interpolate ? 1 : 0,
                tolerance, out nint wire), "freeform_planar_profile");
            return ShapeFactory.FromNativeHandle(wire, "freeform_planar_profile");
        }
    }

    public static Shape OffsetPlanarWire(
        Shape wire, double distance, double altitude = 0.0,
        PlanarOffsetJoin join = PlanarOffsetJoin.Arc)
    {
        ValidateShape(wire); ValidateFinite(distance, nameof(distance)); ValidateFinite(altitude, nameof(altitude));
        NativeError.ThrowIfFailed(NativeMethods.FreeformPlanarOffset(wire.Handle, distance, altitude, (int)join, out nint result), "freeform_planar_offset");
        return ShapeFactory.FromNativeHandle(result, "freeform_planar_offset");
    }

    public static unsafe Shape CreateSurfaceFace(FreeformSurfaceDefinition definition, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(definition); ValidateTolerance(tolerance, nameof(tolerance));
        XyzRaw[] poles = CopyPoints(definition.CopyPoles()); double[] weights = definition.CopyWeights();
        double[] uKnots = definition.CopyUKnots(), vKnots = definition.CopyVKnots();
        int[] uMultiplicities = definition.CopyUMultiplicities(), vMultiplicities = definition.CopyVMultiplicities();
        double[] bounds = definition.Bounds is { } b ? [b.FirstU, b.LastU, b.FirstV, b.LastV] : [];
        fixed (XyzRaw* polePointer = poles)
        fixed (double* weightPointer = weights)
        fixed (double* uKnotPointer = uKnots)
        fixed (double* vKnotPointer = vKnots)
        fixed (int* uMultiplicityPointer = uMultiplicities)
        fixed (int* vMultiplicityPointer = vMultiplicities)
        fixed (double* boundsPointer = bounds)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformSurfaceCreate(
                (int)definition.Kind, polePointer, weights.Length == 0 ? null : weightPointer,
                definition.UPoleCount, definition.VPoleCount,
                uKnots.Length == 0 ? null : uKnotPointer, uMultiplicities.Length == 0 ? null : uMultiplicityPointer, uKnots.Length,
                vKnots.Length == 0 ? null : vKnotPointer, vMultiplicities.Length == 0 ? null : vMultiplicityPointer, vKnots.Length,
                definition.UDegree, definition.VDegree, definition.UPeriodic ? 1 : 0, definition.VPeriodic ? 1 : 0,
                bounds.Length == 0 ? null : boundsPointer, tolerance, out nint face), "freeform_surface_create");
            return ShapeFactory.FromNativeHandle(face, "freeform_surface_create");
        }
    }

    public static Shape InterpolateSurface(IReadOnlyList<IReadOnlyList<GpPoint>> grid, double tolerance = 1e-7) =>
        FitSurface(grid, 3, 8, -1, tolerance, "freeform_surface_interpolate");

    public static Shape ApproximateSurface(
        IReadOnlyList<IReadOnlyList<GpPoint>> grid, int minimumDegree = 3, int maximumDegree = 8,
        FreeformContinuity continuity = FreeformContinuity.C2, double tolerance = 1e-5) =>
        FitSurface(grid, minimumDegree, maximumDegree, (int)continuity, tolerance, "freeform_surface_approximate");

    public static unsafe FreeformSurfaceDefinition GetSurfaceDefinition(Shape face)
    {
        ValidateShape(face);
        NativeError.ThrowIfFailed(NativeMethods.FreeformSurfaceInfo(face.Handle, out FreeformSurfaceInfoRaw info), "freeform_surface_info");
        int poleCount = checked(info.UPoleCount * info.VPoleCount);
        XyzRaw[] rawPoles = new XyzRaw[poleCount]; double[] weights = new double[poleCount];
        double[] uKnots = new double[info.UKnotCount], vKnots = new double[info.VKnotCount];
        int[] uMultiplicities = new int[info.UKnotCount], vMultiplicities = new int[info.VKnotCount];
        fixed (XyzRaw* polePointer = rawPoles)
        fixed (double* weightPointer = weights)
        fixed (double* uKnotPointer = uKnots)
        fixed (double* vKnotPointer = vKnots)
        fixed (int* uMultiplicityPointer = uMultiplicities)
        fixed (int* vMultiplicityPointer = vMultiplicities)
            NativeError.ThrowIfFailed(NativeMethods.FreeformSurfaceCopyDefinition(face.Handle,
                polePointer, poleCount, weightPointer, poleCount,
                info.UKnotCount == 0 ? null : uKnotPointer, info.UKnotCount,
                info.UKnotCount == 0 ? null : uMultiplicityPointer, info.UKnotCount,
                info.VKnotCount == 0 ? null : vKnotPointer, info.VKnotCount,
                info.VKnotCount == 0 ? null : vMultiplicityPointer, info.VKnotCount), "freeform_surface_copy_definition");
        GpPoint[] poles = rawPoles.Select(ToPoint).ToArray(); double[]? rationalWeights = info.Rational == 0 ? null : weights;
        SurfaceParameterBounds bounds = new(info.FirstU, info.LastU, info.FirstV, info.LastV);
        return info.Kind == (int)FreeformGeometryKind.Bezier
            ? FreeformSurfaceDefinition.Bezier(info.UPoleCount, info.VPoleCount, poles, rationalWeights, bounds)
            : FreeformSurfaceDefinition.BSpline(info.UPoleCount, info.VPoleCount, poles,
                uKnots, uMultiplicities, vKnots, vMultiplicities, info.UDegree, info.VDegree,
                info.UPeriodic != 0, info.VPeriodic != 0, rationalWeights, bounds);
    }

    public static Shape ElevateSurfaceDegree(Shape face, int uDegree, int vDegree, double tolerance = 1e-7) =>
        EditSurface(face, 1, uDegree, vDegree, null, tolerance, "freeform_surface_elevate_degree");
    public static Shape ReverseSurfaceU(Shape face, double tolerance = 1e-7) =>
        EditSurface(face, 2, 0, 0, null, tolerance, "freeform_surface_reverse_u");
    public static Shape ReverseSurfaceV(Shape face, double tolerance = 1e-7) =>
        EditSurface(face, 3, 0, 0, null, tolerance, "freeform_surface_reverse_v");
    public static Shape TrimSurface(Shape face, SurfaceParameterBounds bounds, double tolerance = 1e-7) =>
        EditSurface(face, 4, 0, 0, bounds, tolerance, "freeform_surface_trim");

    public static Shape CreateRuledFace(Shape firstEdge, Shape secondEdge)
    {
        ValidateShape(firstEdge); ValidateShape(secondEdge);
        NativeError.ThrowIfFailed(NativeMethods.FreeformRuledFace(firstEdge.Handle, secondEdge.Handle, out nint face), "freeform_ruled_face");
        return ShapeFactory.FromNativeHandle(face, "freeform_ruled_face");
    }

    public static unsafe FreeformShapeResult FillBoundary(
        IReadOnlyList<Shape> boundaryEdges, IReadOnlyList<GpPoint>? interiorPoints = null,
        FreeformContinuity continuity = FreeformContinuity.C0, double tolerance = 1e-5)
    {
        ArgumentNullException.ThrowIfNull(boundaryEdges); if (boundaryEdges.Count < 2) throw new ArgumentException("Boundary filling requires at least two edges.", nameof(boundaryEdges));
        ValidateTolerance(tolerance, nameof(tolerance)); XyzRaw[] points = CopyPoints(interiorPoints ?? [], 0);
        return WithShapeHandles(boundaryEdges, (handles, count) =>
        {
            fixed (XyzRaw* pointPointer = points)
            {
                NativeError.ThrowIfFailed(NativeMethods.FreeformFill(handles, count,
                    points.Length == 0 ? null : pointPointer, points.Length, (int)continuity,
                    tolerance, out FreeformDiagnosticsRaw diagnostics, out nint face), "freeform_fill");
                return new FreeformShapeResult(ShapeFactory.FromNativeHandle(face, "freeform_fill"), FromRaw(diagnostics));
            }
        });
    }

    public static unsafe FreeformShapeResult SplitTopology(IReadOnlyList<Shape> objects, IReadOnlyList<Shape> tools)
    {
        ArgumentNullException.ThrowIfNull(objects); ArgumentNullException.ThrowIfNull(tools);
        if (objects.Count == 0 || tools.Count == 0) throw new ArgumentException("Topology splitting requires object and tool shapes.");
        return WithShapeHandles(objects, (objectHandles, objectCount) =>
            WithShapeHandles(tools, (toolHandles, toolCount) =>
            {
                NativeError.ThrowIfFailed(NativeMethods.FreeformSplit(objectHandles, objectCount, toolHandles, toolCount,
                    out FreeformDiagnosticsRaw diagnostics, out nint result), "freeform_split");
                return new FreeformShapeResult(ShapeFactory.FromNativeHandle(result, "freeform_split"), FromRaw(diagnostics));
            }));
    }

    public static unsafe FreeformShapeResult CreatePipeShell(
        Shape spine, IReadOnlyList<Shape> profiles, bool makeSolid = false, bool frenet = false,
        PipeTransition transition = PipeTransition.Transformed, double tolerance = 1e-5,
        int maximumDegree = 8, int maximumSegments = 30)
    {
        ValidateShape(spine); ArgumentNullException.ThrowIfNull(profiles); if (profiles.Count == 0) throw new ArgumentException("A pipe shell requires at least one profile.", nameof(profiles));
        ValidateTolerance(tolerance, nameof(tolerance));
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumDegree, 1); ArgumentOutOfRangeException.ThrowIfGreaterThan(maximumDegree, 25);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumSegments, 1);
        return WithShapeHandles(profiles, (handles, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformPipeShell(spine.Handle, handles, count,
                makeSolid ? 1 : 0, frenet ? 1 : 0, (int)transition, tolerance, maximumDegree,
                maximumSegments, out FreeformDiagnosticsRaw diagnostics, out nint result), "freeform_pipe_shell");
            return new FreeformShapeResult(ShapeFactory.FromNativeHandle(result, "freeform_pipe_shell"), FromRaw(diagnostics));
        });
    }

    public static unsafe FreeformShapeResult CreateLoft(
        IReadOnlyList<Shape> sections, bool makeSolid = false, bool ruled = false,
        bool smoothing = true, FreeformContinuity continuity = FreeformContinuity.C2,
        int maximumDegree = 8, double tolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(sections); if (sections.Count < 2) throw new ArgumentException("A loft requires at least two sections.", nameof(sections));
        ValidateTolerance(tolerance, nameof(tolerance)); if (maximumDegree is < 1 or > 25) throw new ArgumentOutOfRangeException(nameof(maximumDegree));
        return WithShapeHandles(sections, (handles, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformLoft(handles, count, makeSolid ? 1 : 0,
                ruled ? 1 : 0, smoothing ? 1 : 0, (int)continuity, maximumDegree, tolerance,
                out FreeformDiagnosticsRaw diagnostics, out nint result), "freeform_loft");
            return new FreeformShapeResult(ShapeFactory.FromNativeHandle(result, "freeform_loft"), FromRaw(diagnostics));
        });
    }

    public static FreeformShapeResult Heal(Shape shape, double tolerance = 1e-6)
    {
        ValidateShape(shape); ValidateTolerance(tolerance, nameof(tolerance));
        NativeError.ThrowIfFailed(NativeMethods.FreeformHeal(shape.Handle, tolerance,
            out FreeformDiagnosticsRaw diagnostics, out nint result), "freeform_heal");
        return new FreeformShapeResult(ShapeFactory.FromNativeHandle(result, "freeform_heal"), FromRaw(diagnostics));
    }

    public static FreeformShapeResult OffsetFaceOrShell(Shape shape, double distance, double tolerance = 1e-6)
    {
        ValidateShape(shape); Shape offset = shape.Offset(distance, tolerance);
        ShapeValidationReport report = offset.GetValidationReport();
        return new FreeformShapeResult(offset, new FreeformDiagnostics(0, 1, 1, 0, 0, 0,
            report.IsValid, offset.GetTopologySummary().IsClosed, 0, 0, 0, 0));
    }

    public static FreeformShapeResult SewHealValidate(IReadOnlyList<Shape> faces, double tolerance = 1e-6)
    {
        using Shape sewn = ShapeFactory.Sew(faces, tolerance);
        return Heal(sewn, tolerance);
    }

    private static Shape EditCurve(Shape edge, int operation, int degree, double first, double last, string name)
    {
        ValidateShape(edge); NativeError.ThrowIfFailed(NativeMethods.FreeformCurveEdit(edge.Handle, operation, degree, first, last, out nint result), name);
        return ShapeFactory.FromNativeHandle(result, name);
    }

    private static unsafe FreeformSolution[] CopySolutions(Shape first, Shape? second, GpPoint point, int operation)
    {
        ValidateShape(first); if (second is not null) ValidateShape(second);
        int count; NativeStatus countStatus = operation switch
        {
            1 => NativeMethods.FreeformCurveProjectCount(first.Handle, ShapeFactory.ToRaw(point), out count),
            2 => NativeMethods.FreeformCurveExtremaCount(first.Handle, second!.Handle, out count),
            _ => NativeMethods.FreeformCurveFaceIntersectionCount(first.Handle, second!.Handle, out count)
        };
        NativeError.ThrowIfFailed(countStatus, "freeform_solution_count");
        if (count == 0) return [];
        FreeformSolutionRaw[] raw = new FreeformSolutionRaw[count];
        fixed (FreeformSolutionRaw* pointer = raw)
        {
            NativeStatus copyStatus = operation switch
            {
                1 => NativeMethods.FreeformCurveProjectCopy(first.Handle, ShapeFactory.ToRaw(point), pointer, count, out count),
                2 => NativeMethods.FreeformCurveExtremaCopy(first.Handle, second!.Handle, pointer, count, out count),
                _ => NativeMethods.FreeformCurveFaceIntersectionCopy(first.Handle, second!.Handle, pointer, count, out count)
            };
            NativeError.ThrowIfFailed(copyStatus, "freeform_solution_copy");
        }
        return raw.Take(count).Select(value => new FreeformSolution(ToPoint(value.FirstPoint), ToPoint(value.SecondPoint),
            value.FirstParameter, value.SecondParameter, value.ThirdParameter, value.Distance)).ToArray();
    }

    private static unsafe Shape FitSurface(
        IReadOnlyList<IReadOnlyList<GpPoint>> grid, int minimumDegree, int maximumDegree,
        int continuity, double tolerance, string operation)
    {
        ArgumentNullException.ThrowIfNull(grid); if (grid.Count < 2) throw new ArgumentException("A surface grid requires at least two U rows.", nameof(grid));
        int vCount = grid[0]?.Count ?? 0; if (vCount < 2 || grid.Any(row => row is null || row.Count != vCount)) throw new ArgumentException("A surface grid must be rectangular with at least two V columns.", nameof(grid));
        ValidateDegreeRange(minimumDegree, maximumDegree); ValidateTolerance(tolerance, nameof(tolerance));
        XyzRaw[] points = new XyzRaw[checked(grid.Count * vCount)];
        for (int u = 0; u < grid.Count; ++u) for (int v = 0; v < vCount; ++v) points[u * vCount + v] = ShapeFactory.ToRaw(grid[u][v]);
        fixed (XyzRaw* pointer = points)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformSurfaceApproximate(pointer, grid.Count, vCount,
                minimumDegree, maximumDegree, continuity, tolerance, out nint face), operation);
            return ShapeFactory.FromNativeHandle(face, operation);
        }
    }

    private static unsafe Shape EditSurface(
        Shape face, int operation, int uDegree, int vDegree, SurfaceParameterBounds? bounds,
        double tolerance, string name)
    {
        ValidateShape(face); ValidateTolerance(tolerance, nameof(tolerance)); bounds?.Validate(nameof(bounds));
        double[] values = bounds is { } b ? [b.FirstU, b.LastU, b.FirstV, b.LastV] : [];
        fixed (double* pointer = values)
        {
            NativeError.ThrowIfFailed(NativeMethods.FreeformSurfaceEdit(face.Handle, operation, uDegree, vDegree,
                values.Length == 0 ? null : pointer, tolerance, out nint result), name);
            return ShapeFactory.FromNativeHandle(result, name);
        }
    }

    private static FreeformDiagnostics FromRaw(FreeformDiagnosticsRaw value) => new(
        value.Status, value.InputCount, value.ResultCount, value.ModifiedCount, value.GeneratedCount,
        value.DeletedCount, value.IsValid != 0, value.IsClosed != 0, value.G0Error, value.G1Error,
        value.G2Error, value.ApproximationError);

    private static XyzRaw[] CopyPoints(IReadOnlyList<GpPoint> points, int minimum = 0)
    {
        ArgumentNullException.ThrowIfNull(points); if (points.Count < minimum) throw new ArgumentException($"At least {minimum} points are required.", nameof(points));
        XyzRaw[] result = new XyzRaw[points.Count]; for (int index = 0; index < points.Count; ++index) result[index] = ShapeFactory.ToRaw(points[index]); return result;
    }
    private static XyzRaw[] CopyPoints(GpPoint[] points) => points.Select(ShapeFactory.ToRaw).ToArray();
    private static XyzRaw ToRaw(GpXyz value) { ValidateFinite(value.X, nameof(value)); ValidateFinite(value.Y, nameof(value)); ValidateFinite(value.Z, nameof(value)); return new(value.X, value.Y, value.Z); }
    private static GpPoint ToPoint(XyzRaw value) => new(value.X, value.Y, value.Z);
    private static void ValidateShape(Shape shape) { ArgumentNullException.ThrowIfNull(shape); ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape); }
    private static void ValidateTolerance(double value, string name) { if (!double.IsFinite(value) || value <= 0.0) throw new ArgumentOutOfRangeException(name, "Tolerance must be finite and greater than zero."); }
    private static void ValidateFinite(double value, string name) { if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name, "The value must be finite."); }
    private static void ValidateDegreeRange(int minimum, int maximum) { if (minimum < 1 || maximum < minimum || maximum > 25) throw new ArgumentOutOfRangeException(nameof(maximum), "Degree range must be within 1 through 25."); }

    private unsafe delegate TResult ShapeHandlesAction<TResult>(nint* handles, int count);
    private static unsafe TResult WithShapeHandles<TResult>(IReadOnlyList<Shape> shapes, ShapeHandlesAction<TResult> action)
    {
        nint[] pointers = new nint[shapes.Count]; bool[] references = new bool[shapes.Count]; int acquired = 0;
        try
        {
            for (; acquired < shapes.Count; ++acquired)
            {
                Shape shape = shapes[acquired] ?? throw new ArgumentException("A shape collection contains null.", nameof(shapes)); ValidateShape(shape);
                shape.Handle.DangerousAddRef(ref references[acquired]); pointers[acquired] = shape.Handle.DangerousGetHandle();
            }
            fixed (nint* pointer = pointers) return action(pointer, pointers.Length);
        }
        finally { for (int index = acquired - 1; index >= 0; --index) if (references[index]) shapes[index].Handle.DangerousRelease(); }
    }

    private static List<Shape> OwnShapes(nint[] handles, int written, string operation)
    {
        List<Shape> result = new(written);
        try { for (int index = 0; index < written; ++index) result.Add(ShapeFactory.FromNativeHandle(handles[index], operation)); return result; }
        catch { foreach (Shape shape in result) shape.Dispose(); throw; }
    }
}
#pragma warning restore CS1591
