using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns a native OCCT topology shape.</summary>
public partial class Shape : IDisposable
{
    private readonly ShapeHandle handle;

    internal Shape(ShapeHandle handle)
    {
        this.handle = handle;
    }

    internal ShapeHandle Handle => handle;

    /// <summary>Gets the number of faces contained in this shape.</summary>
    public int FaceCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetFaceCount(handle, out int faceCount),
                "shape_get_face_count");
            return faceCount;
        }
    }

    /// <summary>Returns independent owned face copies in deterministic explorer order.</summary>
    public unsafe Shape[] GetFaces()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.GetFaceCount(handle, out int count), "shape_get_face_count");
        if (count == 0)
        {
            return [];
        }

        nint[] nativeFaces = new nint[count];
        fixed (nint* facePointer = nativeFaces)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetFaceSnapshot(handle, facePointer, nativeFaces.Length, out int written),
                "shape_face_snapshot");
            if (written != nativeFaces.Length)
            {
                for (int index = 0; index < written; ++index)
                {
                    NativeMethods.ReleaseShape(nativeFaces[index]);
                }

                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native face snapshot count changed during enumeration.");
            }
        }

        Shape[] result = new Shape[nativeFaces.Length];
        int created = 0;
        try
        {
            for (; created < nativeFaces.Length; ++created)
            {
                result[created] = ShapeFactory.FromNativeHandle(nativeFaces[created], "shape_face_snapshot");
            }

            return result;
        }
        catch
        {
            for (int index = created; index < nativeFaces.Length; ++index)
            {
                NativeMethods.ReleaseShape(nativeFaces[index]);
            }

            throw;
        }
    }

    /// <summary>Returns independent owned copies of a supported topological subshape kind.</summary>
    public unsafe Shape[] GetSubShapes(ShapeKind kind)
    {
        if (kind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(kind), "Only Compound through Vertex can be explored.");
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.GetSubshapeCount(handle, (int)kind, out int expected), "shape_subshape_count");
        if (expected == 0) return [];
        nint[] nativeShapes = new nint[expected];
        fixed (nint* shapePointer = nativeShapes)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetSubshapeSnapshot(handle, (int)kind, shapePointer, nativeShapes.Length, out int written),
                "shape_subshape_snapshot");
            if (written != nativeShapes.Length)
                throw new OcctException(NativeStatus.UnknownException.ToString(), "The native subshape snapshot count changed during enumeration.");
        }
        Shape[] result = new Shape[nativeShapes.Length];
        int created = 0;
        try
        {
            for (; created < nativeShapes.Length; ++created)
                result[created] = ShapeFactory.FromNativeHandle(nativeShapes[created], "shape_subshape_snapshot");
            return result;
        }
        catch
        {
            for (int index = created; index < nativeShapes.Length; ++index) NativeMethods.ReleaseShape(nativeShapes[index]);
            throw;
        }
    }

    /// <summary>Counts occurrences of one supported topological subshape kind.</summary>
    public int CountSubShapes(ShapeKind kind)
    {
        if (kind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(kind), "Only Compound through Vertex can be explored.");
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetSubshapeCount(handle, (int)kind, out int count),
            "shape_subshape_count");
        return count;
    }

    /// <summary>Copies the adapted curve type, parameter range, and endpoint values for an edge.</summary>
    public EdgeCurveSnapshot GetEdgeCurveSnapshot()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetEdgeCurveSnapshot(handle, out EdgeCurveSnapshotRaw snapshot),
            "shape_edge_curve_snapshot");
        return new EdgeCurveSnapshot(
            (CurveGeometryType)snapshot.CurveType,
            snapshot.FirstParameter,
            snapshot.LastParameter,
            new GpPoint(snapshot.StartPoint.X, snapshot.StartPoint.Y, snapshot.StartPoint.Z),
            new GpPoint(snapshot.EndPoint.X, snapshot.EndPoint.Y, snapshot.EndPoint.Z));
    }

    /// <summary>Copies the adapted surface type and UV bounds for a face.</summary>
    public FaceSurfaceSnapshot GetFaceSurfaceSnapshot(bool restrictToFace = true)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetFaceSurfaceSnapshot(handle, restrictToFace ? 1 : 0, out FaceSurfaceSnapshotRaw snapshot),
            "shape_face_surface_snapshot");
        return new FaceSurfaceSnapshot(
            (SurfaceGeometryType)snapshot.SurfaceType,
            snapshot.FirstUParameter,
            snapshot.LastUParameter,
            snapshot.FirstVParameter,
            snapshot.LastVParameter);
    }

    /// <summary>Evaluates a copied point and oriented unit tangent at an edge parameter.</summary>
    public CurveEvaluation EvaluateEdge(double parameter)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.EvaluateEdge(handle, parameter, out CurveEvaluationRaw result),
            "shape_edge_evaluate");
        return new CurveEvaluation(
            result.Parameter,
            ToPoint(result.Point),
            ToPoint(result.Tangent));
    }

    /// <summary>Evaluates copied first and second 3D derivatives at an edge parameter.</summary>
    public CurveDerivativeEvaluation EvaluateEdgeDerivatives(double parameter)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.EvaluateEdgeDerivatives(handle, parameter, out CurveDerivativeEvaluationRaw result),
            "shape_edge_evaluate_derivatives");
        return new CurveDerivativeEvaluation(
            result.Parameter,
            ToPoint(result.Point),
            ToPoint(result.FirstDerivative),
            ToPoint(result.SecondDerivative));
    }

    /// <summary>Copies the bounded 2D pcurve for this edge on a supplied owning face.</summary>
    public PcurveSnapshot GetPcurveSnapshot(Shape face)
    {
        ArgumentNullException.ThrowIfNull(face);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(face.handle.IsClosed, face);
        NativeError.ThrowIfFailed(
            NativeMethods.GetEdgePcurveSnapshot(handle, face.handle, out PcurveSnapshotRaw result),
            "shape_edge_pcurve_snapshot");
        return new PcurveSnapshot(
            result.FirstParameter,
            result.LastParameter,
            new GpPoint2d(result.StartPoint.X, result.StartPoint.Y),
            new GpPoint2d(result.EndPoint.X, result.EndPoint.Y));
    }

    /// <summary>Evaluates a copied UV point and unit tangent on this edge's pcurve for a face.</summary>
    public PcurveEvaluation EvaluatePcurve(Shape face, double parameter)
    {
        ArgumentNullException.ThrowIfNull(face);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(face.handle.IsClosed, face);
        NativeError.ThrowIfFailed(
            NativeMethods.EvaluateEdgePcurve(handle, face.handle, parameter, out PcurveEvaluationRaw result),
            "shape_edge_pcurve_evaluate");
        return new PcurveEvaluation(
            result.Parameter,
            new GpPoint2d(result.Point.X, result.Point.Y),
            new GpPoint2d(result.Tangent.X, result.Tangent.Y));
    }

    /// <summary>Computes the finite length of an edge over its complete parameter range.</summary>
    public double GetEdgeLength()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.GetEdgeLength(handle, out double length), "shape_edge_length");
        return length;
    }

    /// <summary>Projects a point onto the bounded 3D curve of an edge.</summary>
    public CurveProjection ProjectPointOnEdge(GpPoint point)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.ProjectPointOnEdge(handle, ShapeFactory.ToRaw(point), out CurveProjectionRaw result),
            "shape_edge_project_point");
        return new CurveProjection(result.Parameter, ToPoint(result.Point), result.Distance, result.SolutionCount);
    }

    /// <summary>Returns an independent edge restricted to a finite subinterval of its curve.</summary>
    public Shape TrimEdge(double firstParameter, double lastParameter)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.TrimEdge(handle, firstParameter, lastParameter, out nint result),
            "shape_edge_trim");
        return ShapeFactory.FromNativeHandle(result, "shape_edge_trim");
    }

    /// <summary>Evaluates a copied point and oriented unit normal at bounded face parameters.</summary>
    public SurfaceEvaluation EvaluateFace(double uParameter, double vParameter)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.EvaluateFace(handle, uParameter, vParameter, out SurfaceEvaluationRaw result),
            "shape_face_evaluate");
        return new SurfaceEvaluation(
            result.UParameter,
            result.VParameter,
            ToPoint(result.Point),
            ToPoint(result.Normal));
    }

    /// <summary>Evaluates copied U/V derivatives and the oriented unit normal on a face.</summary>
    public SurfaceDerivativeEvaluation EvaluateFaceDerivatives(double uParameter, double vParameter)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.EvaluateFaceDerivatives(
                handle, uParameter, vParameter, out SurfaceDerivativeEvaluationRaw result),
            "shape_face_evaluate_derivatives");
        return new SurfaceDerivativeEvaluation(
            result.UParameter,
            result.VParameter,
            ToPoint(result.Point),
            ToPoint(result.UDerivative),
            ToPoint(result.VDerivative),
            ToPoint(result.Normal));
    }

    /// <summary>Projects a point onto the bounded surface domain of a face.</summary>
    public SurfaceProjection ProjectPointOnFace(GpPoint point, double tolerance = 1e-7)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.ProjectPointOnFace(
                handle, ShapeFactory.ToRaw(point), tolerance, out SurfaceProjectionRaw result),
            "shape_face_project_point");
        return new SurfaceProjection(
            result.UParameter,
            result.VParameter,
            ToPoint(result.Point),
            result.Distance,
            result.SolutionCount);
    }

    /// <summary>Returns an independent rectangular face restricted to finite UV bounds.</summary>
    public Shape TrimFace(
        double firstUParameter,
        double lastUParameter,
        double firstVParameter,
        double lastVParameter,
        double tolerance = 1e-7)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.TrimFace(
                handle,
                firstUParameter,
                lastUParameter,
                firstVParameter,
                lastVParameter,
                tolerance,
                out nint result),
            "shape_face_trim");
        return ShapeFactory.FromNativeHandle(result, "shape_face_trim");
    }

    /// <summary>
    /// Copies unique subshapes, unique ancestors, and their zero-based adjacency indices.
    /// The returned map owns every copied topology value.
    /// </summary>
    public unsafe TopologyAdjacencyMap GetTopologyAdjacency(ShapeKind itemKind, ShapeKind ancestorKind)
    {
        if (itemKind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(itemKind));
        if (ancestorKind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(ancestorKind));
        if (itemKind <= ancestorKind)
            throw new ArgumentException("The item kind must be lower-level than the ancestor kind.", nameof(itemKind));
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetTopologyAdjacencyCount(
                handle, (int)itemKind, (int)ancestorKind,
                out int itemCount, out int ancestorCount, out int relationCount),
            "shape_topology_adjacency_count");

        nint[] nativeItems = new nint[itemCount];
        nint[] nativeAncestors = new nint[ancestorCount];
        int[] offsets = new int[itemCount + 1];
        int[] ancestorIndices = new int[relationCount];
        fixed (nint* itemPointer = nativeItems)
        fixed (nint* ancestorPointer = nativeAncestors)
        fixed (int* offsetPointer = offsets)
        fixed (int* indexPointer = ancestorIndices)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetTopologyAdjacencySnapshot(
                    handle, (int)itemKind, (int)ancestorKind,
                    itemPointer, nativeItems.Length,
                    ancestorPointer, nativeAncestors.Length,
                    offsetPointer, offsets.Length,
                    indexPointer, ancestorIndices.Length,
                    out int itemsWritten, out int ancestorsWritten, out int relationsWritten),
                "shape_topology_adjacency_snapshot");
            if (itemsWritten != itemCount || ancestorsWritten != ancestorCount || relationsWritten != relationCount)
            {
                for (int index = 0; index < itemsWritten; ++index) NativeMethods.ReleaseShape(nativeItems[index]);
                for (int index = 0; index < ancestorsWritten; ++index) NativeMethods.ReleaseShape(nativeAncestors[index]);
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The topology adjacency snapshot counts changed during enumeration.");
            }
        }

        Shape[] items = new Shape[itemCount];
        Shape[] ancestors = new Shape[ancestorCount];
        int createdItems = 0;
        int createdAncestors = 0;
        try
        {
            for (; createdItems < itemCount; ++createdItems)
                items[createdItems] = ShapeFactory.FromNativeHandle(nativeItems[createdItems], "shape_topology_adjacency_snapshot");
            for (; createdAncestors < ancestorCount; ++createdAncestors)
                ancestors[createdAncestors] = ShapeFactory.FromNativeHandle(nativeAncestors[createdAncestors], "shape_topology_adjacency_snapshot");
            return new TopologyAdjacencyMap(itemKind, ancestorKind, items, ancestors, offsets, ancestorIndices);
        }
        catch
        {
            for (int index = 0; index < createdItems; ++index) items[index].Dispose();
            for (int index = createdItems; index < itemCount; ++index) NativeMethods.ReleaseShape(nativeItems[index]);
            for (int index = 0; index < createdAncestors; ++index) ancestors[index].Dispose();
            for (int index = createdAncestors; index < ancestorCount; ++index) NativeMethods.ReleaseShape(nativeAncestors[index]);
            throw;
        }
    }

    /// <summary>Returns an independent topology graph with one contained subshape replaced.</summary>
    public Shape ReplaceSubshape(Shape target, Shape replacement)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(replacement);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(target.handle.IsClosed, target);
        ObjectDisposedException.ThrowIf(replacement.handle.IsClosed, replacement);
        NativeError.ThrowIfFailed(
            NativeMethods.ReplaceSubshape(handle, target.handle, replacement.handle, out nint result),
            "shape_replace_subshape");
        return ShapeFactory.FromNativeHandle(result, "shape_replace_subshape");
    }

    /// <summary>Returns an independent topology graph with one contained subshape removed.</summary>
    public Shape RemoveSubshape(Shape target)
    {
        ArgumentNullException.ThrowIfNull(target);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(target.handle.IsClosed, target);
        NativeError.ThrowIfFailed(
            NativeMethods.RemoveSubshape(handle, target.handle, out nint result),
            "shape_remove_subshape");
        return ShapeFactory.FromNativeHandle(result, "shape_remove_subshape");
    }

    /// <summary>Creates an owned shape with the supplied rigid transform applied.</summary>
    public Shape Transformed(ShapeTransform transform)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();
        NativeStatus status = NativeMethods.TransformShape(
            handle,
            transform.TranslationX,
            transform.TranslationY,
            transform.TranslationZ,
            transform.RotationAxisX,
            transform.RotationAxisY,
            transform.RotationAxisZ,
            transform.RotationAngleRadians,
            out nint transformedShape);
        NativeError.ThrowIfFailed(status, "shape_transform");
        return ShapeFactory.FromNativeHandle(transformedShape, "shape_transform");
    }

    /// <summary>Creates an owned shape by applying an OCCT <c>gp_Trsf</c> value.</summary>
    public Shape Transformed(GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return transform.Apply(this);
    }

    /// <summary>Returns an independently owned shape with an absolute location.</summary>
    public Shape Located(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Locate(this);
    }

    /// <summary>Returns an independently owned shape moved by a location.</summary>
    public Shape Moved(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Move(this);
    }

    /// <summary>Extrudes this topology along an OCCT vector and returns an independently owned result.</summary>
    public Shape Extrude(GpVec direction)
    {
        ArgumentNullException.ThrowIfNull(direction);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(direction.Handle.IsClosed, direction);
        NativeError.ThrowIfFailed(
            NativeMethods.ExtrudeShape(handle, direction.Handle, out nint result),
            "shape_extrude");
        return ShapeFactory.FromNativeHandle(result, "shape_extrude");
    }

    /// <summary>Revolves this topology around an OCCT axis by at most one full turn.</summary>
    public Shape Revolve(GpAx1 axis, double angleRadians)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(axis.Handle.IsClosed, axis);
        NativeError.ThrowIfFailed(
            NativeMethods.RevolveShape(handle, axis.Handle, angleRadians, out nint result),
            "shape_revolve");
        return ShapeFactory.FromNativeHandle(result, "shape_revolve");
    }

    /// <summary>Applies one radius to every unique edge and returns an independently owned fillet result.</summary>
    public Shape Fillet(double radius)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.FilletAllEdges(handle, radius, out nint result),
            "shape_fillet_all");
        return ShapeFactory.FromNativeHandle(result, "shape_fillet_all");
    }

    /// <summary>Applies a radius to one edge belonging to this shape.</summary>
    public Shape Fillet(Shape edge, double radius)
    {
        ArgumentNullException.ThrowIfNull(edge);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(edge.handle.IsClosed, edge);
        NativeError.ThrowIfFailed(
            NativeMethods.FilletEdge(handle, edge.handle, radius, out nint result),
            "shape_fillet_edge");
        return ShapeFactory.FromNativeHandle(result, "shape_fillet_edge");
    }

    /// <summary>Applies one distance to every unique edge and returns an independently owned chamfer result.</summary>
    public Shape Chamfer(double distance)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.ChamferAllEdges(handle, distance, out nint result),
            "shape_chamfer_all");
        return ShapeFactory.FromNativeHandle(result, "shape_chamfer_all");
    }

    /// <summary>Applies a chamfer distance to one edge belonging to this shape.</summary>
    public Shape Chamfer(Shape edge, double distance)
    {
        ArgumentNullException.ThrowIfNull(edge);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(edge.handle.IsClosed, edge);
        NativeError.ThrowIfFailed(
            NativeMethods.ChamferEdge(handle, edge.handle, distance, out nint result),
            "shape_chamfer_edge");
        return ShapeFactory.FromNativeHandle(result, "shape_chamfer_edge");
    }

    /// <summary>Builds an offset shape using OCCT's skin/join algorithm.</summary>
    public Shape Offset(double distance, double tolerance = 1e-6)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.OffsetShape(handle, distance, tolerance, out nint result),
            "shape_offset");
        return ShapeFactory.FromNativeHandle(result, "shape_offset");
    }

    /// <summary>Hollows this solid by removing copied closing faces and offsetting the remaining walls.</summary>
    public unsafe Shape MakeThickSolid(
        IReadOnlyList<Shape> closingFaces,
        double offset,
        double tolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(closingFaces);
        if (closingFaces.Count == 0)
            throw new ArgumentException("A thick solid requires at least one closing face.", nameof(closingFaces));
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return ShapeFactory.WithBorrowedShapeHandles(closingFaces, (pointers, count) =>
        {
            NativeError.ThrowIfFailed(
                NativeMethods.MakeThickSolid(handle, pointers, count, offset, tolerance, out nint result),
                "shape_make_thick_solid");
            return ShapeFactory.FromNativeHandle(result, "shape_make_thick_solid");
        });
    }

    /// <summary>Computes section curves between this shape and another shape.</summary>
    public Shape Section(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.SectionShapes(handle, other.handle, out nint result),
            "shape_section");
        return ShapeFactory.FromNativeHandle(result, "shape_section");
    }

    /// <summary>Copies finite axis-aligned bounds from OCCT.</summary>
    public BoundingBox3d GetBoundingBox()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetBoundingBox(handle, out BoundingBoxRaw bounds),
            "shape_bounding_box");
        return new BoundingBox3d(
            new GpPoint(bounds.MinX, bounds.MinY, bounds.MinZ),
            new GpPoint(bounds.MaxX, bounds.MaxY, bounds.MaxZ));
    }

    /// <summary>Gets whether OCCT reports the complete topology as valid.</summary>
    public bool IsValid
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.IsShapeValid(handle, out int isValid),
                "shape_is_valid");
            return isValid != 0;
        }
    }

    /// <summary>Copies unique/occurrence counts, closedness, validity, and common tolerance ranges.</summary>
    public ShapeTopologySummary GetTopologySummary()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetShapeTopologySummary(handle, out ShapeTopologySummaryRaw summary),
            "shape_topology_summary");
        return new ShapeTopologySummary(
            ToTopologyCounts(summary.UniqueCounts),
            ToTopologyCounts(summary.OccurrenceCounts),
            summary.IsClosed != 0,
            summary.IsValid != 0,
            new ToleranceRange(summary.MinVertexTolerance, summary.MaxVertexTolerance),
            new ToleranceRange(summary.MinEdgeTolerance, summary.MaxEdgeTolerance),
            new ToleranceRange(summary.MinFaceTolerance, summary.MaxFaceTolerance));
    }

    /// <summary>Copies detailed BRepCheck statuses for every unique subshape.</summary>
    public unsafe ShapeValidationReport GetValidationReport(
        bool geometryChecks = true,
        bool exact = false)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetShapeValidationIssueCount(
                handle, geometryChecks ? 1 : 0, exact ? 1 : 0,
                out int isValid, out int issueCount),
            "shape_validation_issue_count");
        ValidationIssueRaw[] nativeIssues = new ValidationIssueRaw[issueCount];
        fixed (ValidationIssueRaw* issuePointer = nativeIssues)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetShapeValidationIssues(
                    handle, geometryChecks ? 1 : 0, exact ? 1 : 0,
                    issuePointer, nativeIssues.Length,
                    out int writtenValid, out int writtenCount),
                "shape_validation_issues");
            if (writtenValid != isValid || writtenCount != nativeIssues.Length)
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native validation issue count changed during extraction.");
        }
        ShapeValidationIssue[] issues = new ShapeValidationIssue[nativeIssues.Length];
        for (int index = 0; index < issues.Length; ++index)
            issues[index] = new ShapeValidationIssue(
                (ShapeKind)nativeIssues[index].ShapeKind,
                (ShapeValidationStatus)nativeIssues[index].Status);
        return new ShapeValidationReport(isValid != 0, issues);
    }

    /// <summary>Computes an OCCT boolean union and returns an independently owned result.</summary>
    public Shape Fuse(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(NativeMethods.BooleanFuse(handle, other.handle, out nint result), "shape_boolean_fuse");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_fuse");
    }

    /// <summary>Subtracts <paramref name="tool"/> from this shape through OCCT BRepAlgoAPI.</summary>
    public Shape Cut(Shape tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(tool.handle.IsClosed, tool);
        NativeError.ThrowIfFailed(NativeMethods.BooleanCut(handle, tool.handle, out nint result), "shape_boolean_cut");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_cut");
    }

    /// <summary>Computes the OCCT Boolean intersection and returns an independently owned result.</summary>
    public Shape Common(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.BooleanCommon(handle, other.handle, out nint result),
            "shape_boolean_common");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_common");
    }

    /// <summary>Computes a union and copies history counts for one unique topology kind.</summary>
    public BooleanOperationResult FuseWithHistory(Shape other, ShapeKind trackedKind = ShapeKind.Face) =>
        RunBooleanWithHistory(other, BooleanOperationKind.Fuse, trackedKind);

    /// <summary>Computes a subtraction and copies history counts for one unique topology kind.</summary>
    public BooleanOperationResult CutWithHistory(Shape tool, ShapeKind trackedKind = ShapeKind.Face) =>
        RunBooleanWithHistory(tool, BooleanOperationKind.Cut, trackedKind);

    /// <summary>Computes an intersection and copies history counts for one unique topology kind.</summary>
    public BooleanOperationResult CommonWithHistory(Shape other, ShapeKind trackedKind = ShapeKind.Face) =>
        RunBooleanWithHistory(other, BooleanOperationKind.Common, trackedKind);

    private BooleanOperationResult RunBooleanWithHistory(
        Shape other,
        BooleanOperationKind operation,
        ShapeKind trackedKind)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (trackedKind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(trackedKind));
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.BooleanWithHistory(
                handle, other.handle, (int)operation, (int)trackedKind,
                out nint result, out BooleanHistorySummaryRaw history),
            "shape_boolean_with_history");
        Shape resultShape = ShapeFactory.FromNativeHandle(result, "shape_boolean_with_history");
        BooleanHistorySideSummary left = new(
            history.LeftSourceCount,
            history.LeftModifiedSourceCount,
            history.LeftGeneratedSourceCount,
            history.LeftDeletedSourceCount,
            history.LeftModifiedResultCount,
            history.LeftGeneratedResultCount);
        BooleanHistorySideSummary right = new(
            history.RightSourceCount,
            history.RightModifiedSourceCount,
            history.RightGeneratedSourceCount,
            history.RightDeletedSourceCount,
            history.RightModifiedResultCount,
            history.RightGeneratedResultCount);
        return new BooleanOperationResult(
            operation,
            resultShape,
            new BooleanHistorySummary(trackedKind, left, right));
    }

    /// <summary>Copies the minimum distance and one corresponding point pair from OCCT.</summary>
    public ShapeDistanceResult DistanceTo(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.GetShapeDistance(handle, other.handle, out ShapeDistanceResultRaw result),
            "shape_distance");
        return new ShapeDistanceResult(
            result.Distance,
            new GpPoint(result.PointOnFirst.X, result.PointOnFirst.Y, result.PointOnFirst.Z),
            new GpPoint(result.PointOnSecond.X, result.PointOnSecond.Y, result.PointOnSecond.Z),
            result.SolutionCount);
    }

    /// <summary>Runs OCCT's safe ShapeFix_Shape pass and returns an owned result.</summary>
    public Shape Fixed()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.FixShape(handle, out nint result), "shape_fix");
        return ShapeFactory.FromNativeHandle(result, "shape_fix");
    }

    /// <summary>Runs ShapeFix and returns copied validation snapshots from before and after repair.</summary>
    public ShapeRepairResult RepairWithReport(bool geometryChecks = true, bool exact = false)
    {
        ShapeValidationReport before = GetValidationReport(geometryChecks, exact);
        Shape repaired = Fixed();
        try
        {
            return new ShapeRepairResult(
                repaired,
                before,
                repaired.GetValidationReport(geometryChecks, exact));
        }
        catch
        {
            repaired.Dispose();
            throw;
        }
    }

    /// <summary>Unifies compatible neighbouring faces and edges into an owned result.</summary>
    public Shape UnifiedSameDomain()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.UnifySameDomain(handle, out nint result),
            "shape_unify_same_domain");
        return ShapeFactory.FromNativeHandle(result, "shape_unify_same_domain");
    }

    /// <summary>Builds an owned managed copy of the shape's triangle mesh.</summary>
    public unsafe MeshSnapshot CreateMesh(double linearDeflection = 0.1, double angularDeflection = 0.5)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetMeshCount(handle, linearDeflection, angularDeflection, out int vertexCount, out int indexCount),
            "shape_mesh_count");
        MeshVertexRaw[] nativeVertices = new MeshVertexRaw[vertexCount];
        int[] indices = new int[indexCount];
        fixed (MeshVertexRaw* vertexPointer = nativeVertices)
        fixed (int* indexPointer = indices)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetMeshSnapshot(
                    handle,
                    linearDeflection,
                    angularDeflection,
                    vertexPointer,
                    nativeVertices.Length,
                    out int writtenVertices,
                    indexPointer,
                    indices.Length,
                    out int writtenIndices),
                "shape_mesh_snapshot");
            if (writtenVertices != nativeVertices.Length || writtenIndices != indices.Length)
            {
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native mesh snapshot count changed during extraction.");
            }
        }

        MeshVertex[] vertices = new MeshVertex[nativeVertices.Length];
        for (int index = 0; index < vertices.Length; ++index)
        {
            MeshVertexRaw value = nativeVertices[index];
            vertices[index] = new MeshVertex(value.X, value.Y, value.Z, value.NormalX, value.NormalY, value.NormalZ);
        }

        return new MeshSnapshot(vertices, indices);
    }

    /// <summary>
    /// Copies OCCT triangulation nodes, transformed node normals, UVs, triangle winding,
    /// and zero-based source-face mappings into caller-owned managed arrays.
    /// </summary>
    public unsafe DetailedMeshSnapshot CreateDetailedMesh(
        double linearDeflection = 0.1,
        double angularDeflection = 0.5)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetDetailedMeshCount(
                handle, linearDeflection, angularDeflection,
                out int vertexCount, out int triangleCount, out int faceCount),
            "shape_detailed_mesh_count");
        DetailedMeshVertexRaw[] nativeVertices = new DetailedMeshVertexRaw[vertexCount];
        DetailedMeshTriangleRaw[] nativeTriangles = new DetailedMeshTriangleRaw[triangleCount];
        fixed (DetailedMeshVertexRaw* vertexPointer = nativeVertices)
        fixed (DetailedMeshTriangleRaw* trianglePointer = nativeTriangles)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetDetailedMeshSnapshot(
                    handle, linearDeflection, angularDeflection,
                    vertexPointer, nativeVertices.Length, out int writtenVertices,
                    trianglePointer, nativeTriangles.Length, out int writtenTriangles,
                    out int writtenFaces),
                "shape_detailed_mesh_snapshot");
            if (writtenVertices != nativeVertices.Length
                || writtenTriangles != nativeTriangles.Length
                || writtenFaces != faceCount)
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native detailed-mesh snapshot count changed during extraction.");
        }

        DetailedMeshVertex[] vertices = new DetailedMeshVertex[nativeVertices.Length];
        for (int index = 0; index < vertices.Length; ++index)
        {
            DetailedMeshVertexRaw value = nativeVertices[index];
            vertices[index] = new DetailedMeshVertex(
                value.X, value.Y, value.Z,
                value.NormalX, value.NormalY, value.NormalZ,
                value.U, value.V, value.HasUv != 0);
        }
        DetailedMeshTriangle[] triangles = new DetailedMeshTriangle[nativeTriangles.Length];
        for (int index = 0; index < triangles.Length; ++index)
        {
            DetailedMeshTriangleRaw value = nativeTriangles[index];
            triangles[index] = new DetailedMeshTriangle(
                value.VertexA, value.VertexB, value.VertexC,
                value.FaceIndex, value.IsReversed != 0);
        }
        return new DetailedMeshSnapshot(vertices, triangles, faceCount);
    }

    private static TopologyCounts ToTopologyCounts(TopologyCountsRaw counts) => new(
        counts.VertexCount,
        counts.EdgeCount,
        counts.WireCount,
        counts.FaceCount,
        counts.ShellCount,
        counts.SolidCount,
        counts.CompSolidCount,
        counts.CompoundCount);

    private static GpPoint ToPoint(XyzRaw value) => new(value.X, value.Y, value.Z);

    /// <summary>Releases the owned native shape.</summary>
    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }
}
