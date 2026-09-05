using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public static partial class SurfaceModeling
{
    public static unsafe Shape CreateAnalyticFace(AnalyticSurfaceKind kind, SketchPlane frame,
        SurfaceParameterBounds bounds, double radius = 1, double secondary = 0.5, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(frame); bounds.Validate(nameof(bounds)); ValidateTolerance(tolerance);
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        double[] range = [bounds.FirstU, bounds.LastU, bounds.FirstV, bounds.LastV]; SketchPlaneRaw raw = frame.ToRaw();
        fixed (double* parameters = range)
        {
            Check(NativeMethods.SurfaceCreateAnalytic((int)kind, in raw, radius, secondary, parameters, tolerance, out nint shape), "create_analytic");
            return Own(shape, "create_analytic");
        }
    }

    /// <summary>Returns owning compound topology; use GetSubShapes to obtain independently owning projected pieces.</summary>
    public static Shape ProjectShape(Shape face, Shape edgeOrWire, SurfaceProjectionOptions? options = null)
    {
        ValidateShape(face); ValidateShape(edgeOrWire); options ??= new();
        SurfaceProjectionOptionsRaw raw = new(options.Tolerance3d, options.Tolerance2d, options.MaximumDistance,
            options.LimitToFace ? 1 : 0, options.MaximumDegree, options.MaximumSegments, (int)options.Continuity);
        Check(NativeMethods.SurfaceProjectShape(face.Handle, edgeOrWire.Handle, in raw, out nint shape), "project_shape");
        return Own(shape, "project_shape");
    }

    public static unsafe Shape CreateWire(Shape face, IReadOnlyList<Shape> edges, double tolerance = 1e-7)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(edges); ValidateTolerance(tolerance);
        return ShapeFactory.WithBorrowedShapeHandles(edges, (handles, count) =>
        {
            Check(NativeMethods.SurfaceMakeWire(face.Handle, handles, count, tolerance, out nint shape), "make_wire");
            return Own(shape, "make_wire");
        });
    }

    public static Shape LiftLoop(Shape face, SketchCurveChain2d loop, double tolerance = 1e-7)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(loop); List<Shape> edges = [];
        try
        {
            foreach (SketchCurve2d curve in loop.Curves) edges.Add(LiftCurve(face, curve, tolerance: tolerance));
            return CreateWire(face, edges, tolerance);
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    /// <summary>Uses a validated UV outer loop and holes on the face's supporting surface.</summary>
    public static unsafe Shape CreateTrimmedFace(Shape face, SketchProfile2d profile, double tolerance = 1e-7)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(profile); ValidateTolerance(tolerance);
        List<Shape> wires = [];
        try
        {
            wires.Add(LiftLoop(face, profile.Outer, tolerance));
            foreach (SketchCurveChain2d hole in profile.Holes) wires.Add(LiftLoop(face, hole, tolerance));
            return ShapeFactory.WithBorrowedShapeHandles(wires, (handles, count) =>
            {
                Check(NativeMethods.SurfaceMakeFace(face.Handle, handles, count, tolerance, out nint shape), "make_face");
                return Own(shape, "make_face");
            });
        }
        finally { foreach (Shape wire in wires) wire.Dispose(); }
    }

    public static SurfaceRepairResult Repair(Shape shape, double tolerance = 1e-7, double maximumTolerance = 1e-3) =>
        RepairCore(shape, true, tolerance, maximumTolerance);

    public static SurfaceRepairDiagnostics InspectRepresentations(Shape shape)
    {
        using SurfaceRepairResult inspection = RepairCore(shape, false, 1e-7, 1e-3);
        return inspection.Diagnostics;
    }

    private static SurfaceRepairResult RepairCore(Shape shape, bool perform, double tolerance, double maximumTolerance)
    {
        ValidateShape(shape); ValidateTolerance(tolerance); ValidateTolerance(maximumTolerance);
        Check(NativeMethods.SurfaceRepair(shape.Handle, perform ? 1 : 0, tolerance, maximumTolerance, out SurfaceRepairInfoRaw info, out nint result), "repair");
        return new(Own(result, "repair"), new(info.ValidBefore != 0, info.ValidAfter != 0,
            info.EdgesBefore, info.EdgesAfter, info.MissingBefore, info.MissingAfter,
            info.InconsistentBefore, info.InconsistentAfter, info.ToleranceBefore, info.ToleranceAfter));
    }

    public static unsafe IReadOnlyList<SurfaceBoundaryLoop> GetBoundaryLoops(Shape face, double tolerance = 1e-7)
    {
        ValidateShape(face); ValidateTolerance(tolerance);
        Check(NativeMethods.SurfaceBoundary(face.Handle, null, null, 0, out int count), "boundary_count");
        SurfaceBoundaryInfoRaw[] info = new SurfaceBoundaryInfoRaw[count]; nint[] handles = new nint[count];
        try
        {
            fixed (SurfaceBoundaryInfoRaw* records = info)
            fixed (nint* edges = handles)
                Check(NativeMethods.SurfaceBoundary(face.Handle, records, edges, count, out count), "boundary");
            List<(SurfaceBoundaryInfoRaw Info, SurfaceBoundarySegment Segment)> segments = [];
            for (int index = 0; index < count; ++index)
            {
                using Shape edge = Own(handles[index], "boundary_edge"); handles[index] = 0;
                int branch = info[index].Seam != 0 && info[index].Orientation == 1 ? 1 : 0;
                SurfaceCurveDefinition definition = GetCurveDefinition(face, edge, branch, tolerance);
                segments.Add((info[index], new(definition, info[index].Length, info[index].Seam != 0, info[index].Degenerate != 0)));
            }
            return Array.AsReadOnly(segments.GroupBy(value => value.Info.LoopIndex).Select(group =>
            {
                SurfaceBoundarySegment[] values = group.Select(value => value.Segment).ToArray(); double? area = null;
                try { area = SketchCurveChain2d.Create(values.Select(value => value.Definition.Curve).ToArray(), true, tolerance).Measure().SignedArea; }
                catch (SketchValidationException) { /* Explicit null area represents a non-simple or seam-ambiguous UV loop. */ }
                return new SurfaceBoundaryLoop(group.Key, group.First().Info.Outer != 0, values, area);
            }).ToArray());
        }
        finally { foreach (nint handle in handles) if (handle != 0) NativeMethods.ReleaseShape(handle); }
    }

    public static unsafe Shape SplitFace(Shape face, IReadOnlyList<Shape> tools)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(tools);
        return ShapeFactory.WithBorrowedShapeHandles(tools, (handles, count) =>
        {
            Check(NativeMethods.SurfaceSplit(face.Handle, handles, count, out nint result), "split");
            return Own(result, "split");
        });
    }

    public static Shape IntersectFaces(Shape first, Shape second, double tolerance = 1e-7)
    {
        ValidateShape(first); ValidateShape(second); ValidateTolerance(tolerance);
        Check(NativeMethods.SurfaceSection(first.Handle, second.Handle, tolerance, out nint result), "section");
        return Own(result, "section");
    }

    /// <summary>Splits on copied topology and reports copied validity, piece count and area diagnostics.</summary>
    public static SurfaceSplitResult SplitFaceWithDiagnostics(Shape face, IReadOnlyList<Shape> tools)
    {
        ValidateShape(face); ArgumentNullException.ThrowIfNull(tools);
        double sourceArea = face.InspectProperties(InspectionPropertyKind.Area).Mass;
        Shape result = SplitFace(face, tools);
        try
        {
            return new(result, new(tools.Count, result.FaceCount, result.IsValid,
                sourceArea, result.InspectProperties(InspectionPropertyKind.Area).Mass));
        }
        catch { result.Dispose(); throw; }
    }

    public static Shape IntersectPlane(Shape face, SketchPlane plane, SurfaceParameterBounds planeBounds, double tolerance = 1e-7)
    {
        using Shape tool = CreateAnalyticFace(AnalyticSurfaceKind.Plane, plane, planeBounds, tolerance: tolerance);
        return IntersectFaces(face, tool, tolerance);
    }
}
