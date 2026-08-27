using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    private const string LibraryName = "OcctSharp.Native";

    static NativeMethods()
    {
        NativeLibraryResolver.EnsureRegistered();
    }

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_abi_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint GetAbiVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_bridge_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetBridgeVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_occt_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetOcctVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetLastError();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial XyzRaw CreateXyzDefault();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial XyzRaw CreateXyz(double x, double y, double z);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial XyzRaw CopyXyz(XyzRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_added")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial XyzRaw AddXyz(XyzRaw left, XyzRaw right);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_crossed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial XyzRaw CrossXyz(XyzRaw left, XyzRaw right);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_dot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double DotXyz(XyzRaw left, XyzRaw right);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_modulus")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetXyzModulus(XyzRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_xyz_normalized")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus NormalizeXyz(XyzRaw value, out XyzRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_lin_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial LineRaw CreateLineDefault();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_lin_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateLine(XyzRaw origin, XyzRaw direction, out LineRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_lin_reversed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial LineRaw ReverseLine(LineRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_lin_distance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetLineDistance(LineRaw line, XyzRaw point);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_lin_angle")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetLineAngle(LineRaw left, LineRaw right);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_circ_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial CircleRaw CreateCircleDefault();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_circ_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCircle(XyzRaw center, XyzRaw normal, double radius, out CircleRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_circ_area")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetCircleArea(CircleRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_circ_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetCircleLength(CircleRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_circ_distance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetCircleDistance(CircleRaw value, XyzRaw point);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax2_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Ax2Raw CreateAx2Default();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax2_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAx2(XyzRaw origin, XyzRaw normal, XyzRaw xDirection, out Ax2Raw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax2_angle")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetAx2Angle(Ax2Raw left, Ax2Raw right);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax3_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Ax3Raw CreateAx3Default();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax3_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAx3(XyzRaw origin, XyzRaw normal, XyzRaw xDirection, out Ax3Raw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_ax3_direct")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int IsAx3Direct(Ax3Raw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_pln_default")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial PlaneRaw CreatePlaneDefault();
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_pln_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreatePlane(XyzRaw origin, XyzRaw normal, out PlaneRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_pln_distance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetPlaneDistance(PlaneRaw plane, XyzRaw point);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gp_pln_signed_distance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double GetPlaneSignedDistance(PlaneRaw plane, XyzRaw point);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateGProps(out nint properties);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_from_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateGPropsFromShape(ShapeHandle shape, int mode, int onlyClosed, out nint properties);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneGProps(GPropsHandle source, out nint properties);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddGProps(GPropsHandle target, GPropsHandle item, double density);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_mass")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetGPropsMass(GPropsHandle properties, out double mass);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_center")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetGPropsCenter(GPropsHandle properties, out XyzRaw center);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_inertia_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetGPropsInertiaValue(GPropsHandle properties, int row, int column, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_principal_moments")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetGPropsPrincipalMoments(GPropsHandle properties, out double first, out double second, out double third);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_symmetry")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetGPropsSymmetry(GPropsHandle properties, out int axis, out int point);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_gprops_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseGProps(nint properties);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_edge_curve_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetEdgeCurveSnapshot(
        ShapeHandle edge,
        out EdgeCurveSnapshotRaw snapshot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_face_surface_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFaceSurfaceSnapshot(
        ShapeHandle face,
        int restrictToFace,
        out FaceSurfaceSnapshotRaw snapshot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_edge_evaluate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus EvaluateEdge(
        ShapeHandle edge, double parameter, out CurveEvaluationRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_edge_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetEdgeLength(ShapeHandle edge, out double length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_edge_project_point")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ProjectPointOnEdge(
        ShapeHandle edge, XyzRaw point, out CurveProjectionRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_face_evaluate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus EvaluateFace(
        ShapeHandle face, double uParameter, double vParameter, out SurfaceEvaluationRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_face_project_point")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ProjectPointOnFace(
        ShapeHandle face, XyzRaw point, double tolerance, out SurfaceProjectionRaw result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_box")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateBox(
        double sizeX,
        double sizeY,
        double sizeZ,
        out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_null")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateNullShape(out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_sphere")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateSphere(double radius, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_cylinder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCylinder(double radius, double height, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_cone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCone(double bottomRadius, double topRadius, double height, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_torus")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTorus(double majorRadius, double minorRadius, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_wedge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateWedge(
        double sizeX, double sizeY, double sizeZ, double topXLength, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateEdge(XyzRaw start, XyzRaw end, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_circle_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCircleEdge(
        XyzRaw center, XyzRaw normal, double radius, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_arc_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateArcEdge(
        XyzRaw start, XyzRaw middle, XyzRaw end, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_ellipse_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateEllipseEdge(
        XyzRaw center, XyzRaw normal, XyzRaw xDirection,
        double majorRadius, double minorRadius, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_bezier_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CreateBezierEdge(
        XyzRaw* poles, int count, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_interpolated_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CreateInterpolatedEdge(
        XyzRaw* points, int count, int periodic, double tolerance, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_loft")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CreateLoft(
        nint* sections, int count, int makeSolid, int ruled, double tolerance, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_pipe")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreatePipe(ShapeHandle spine, ShapeHandle profile, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_sew")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SewShapes(
        nint* shapes, int count, double tolerance, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_polygon_wire")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CreatePolygonWire(
        XyzRaw* points, int count, int close, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_planar_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreatePlanarFace(ShapeHandle wire, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_get_face_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFaceCount(ShapeHandle shape, out int faceCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_face_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetFaceSnapshot(
        ShapeHandle shape, nint* faces, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_subshape_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetSubshapeSnapshot(
        ShapeHandle shape, int kind, nint* shapes, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_subshape_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetSubshapeCount(ShapeHandle shape, int kind, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_topology_adjacency_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTopologyAdjacencyCount(
        ShapeHandle shape, int itemKind, int ancestorKind,
        out int itemCount, out int ancestorCount, out int relationCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_topology_adjacency_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetTopologyAdjacencySnapshot(
        ShapeHandle shape, int itemKind, int ancestorKind,
        nint* items, int itemCapacity,
        nint* ancestors, int ancestorCapacity,
        int* offsets, int offsetCapacity,
        int* ancestorIndices, int relationCapacity,
        out int itemsWritten, out int ancestorsWritten, out int relationsWritten);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_extrude")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ExtrudeShape(ShapeHandle shape, VectorHandle direction, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_revolve")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RevolveShape(ShapeHandle shape, AxisHandle axis, double angleRadians, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_fillet_all")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FilletAllEdges(ShapeHandle shape, double radius, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_fillet_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FilletEdge(ShapeHandle shape, ShapeHandle edge, double radius, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_chamfer_all")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ChamferAllEdges(ShapeHandle shape, double distance, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_chamfer_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ChamferEdge(ShapeHandle shape, ShapeHandle edge, double distance, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus OffsetShape(ShapeHandle shape, double offset, double tolerance, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_make_thick_solid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MakeThickSolid(
        ShapeHandle shape, nint* closingFaces, int faceCount,
        double offset, double tolerance, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_section")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SectionShapes(ShapeHandle left, ShapeHandle right, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_bounding_box")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetBoundingBox(ShapeHandle shape, out BoundingBoxRaw bounds);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_is_valid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsShapeValid(ShapeHandle shape, out int isValid);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_boolean_fuse")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BooleanFuse(ShapeHandle left, ShapeHandle right, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_boolean_cut")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BooleanCut(ShapeHandle left, ShapeHandle right, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_boolean_common")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BooleanCommon(ShapeHandle left, ShapeHandle right, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_boolean_with_history")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BooleanWithHistory(
        ShapeHandle left,
        ShapeHandle right,
        int operation,
        int trackedKind,
        out nint shape,
        out BooleanHistorySummaryRaw history);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_distance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetShapeDistance(
        ShapeHandle first,
        ShapeHandle second,
        out ShapeDistanceResultRaw result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_fix")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FixShape(ShapeHandle shape, out nint fixedShape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_unify_same_domain")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus UnifySameDomain(ShapeHandle shape, out nint unifiedShape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_mesh_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetMeshCount(
        ShapeHandle shape,
        double linearDeflection,
        double angularDeflection,
        out int vertexCount,
        out int indexCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_mesh_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetMeshSnapshot(
        ShapeHandle shape,
        double linearDeflection,
        double angularDeflection,
        MeshVertexRaw* vertices,
        int vertexCapacity,
        out int vertexCount,
        int* indices,
        int indexCapacity,
        out int indexCount);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_read_step",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadStep(string filePath, out nint shape);
    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_read_iges",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadIges(string filePath, out nint shape);
    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_read_stl",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadStl(string filePath, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_read_obj", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadObj(string filePath, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_read_gltf", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadGltf(string filePath, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_read_vrml", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadVrml(string filePath, out nint shape);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_step",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteStep(ShapeHandle shape, string filePath);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_stl",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteStl(
        ShapeHandle shape,
        string filePath,
        double linearDeflection,
        double angularDeflection,
        int binary);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_iges",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteIges(ShapeHandle shape, string filePath);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_write_obj", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteObj(ShapeHandle shape, string filePath);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_write_ply", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WritePly(ShapeHandle shape, string filePath);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_write_gltf", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteGltf(ShapeHandle shape, string filePath);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_write_vrml", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteVrml(ShapeHandle shape, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TransformShape(
        ShapeHandle shape,
        double translationX,
        double translationY,
        double translationZ,
        double rotationAxisX,
        double rotationAxisY,
        double rotationAxisZ,
        double rotationAngleRadians,
        out nint transformedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransformIdentity(out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_translation_rotation")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransform(
        double translationX,
        double translationY,
        double translationZ,
        double rotationAxisX,
        double rotationAxisY,
        double rotationAxisZ,
        double rotationAngleRadians,
        out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneTransform(TransformHandle source, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_inverted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus InvertTransform(TransformHandle source, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_multiplied")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MultiplyTransforms(
        TransformHandle left,
        TransformHandle right,
        out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransformValue(
        TransformHandle transform,
        int row,
        int column,
        out double value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseTransform(nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_transform_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TransformShapeWithTransform(
        ShapeHandle shape,
        TransformHandle transform,
        out nint transformedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_create_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateLocationIdentity(out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_create_from_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateLocation(TransformHandle transform, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneLocation(LocationHandle source, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_inverted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus InvertLocation(LocationHandle source, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_multiplied")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MultiplyLocations(
        LocationHandle left,
        LocationHandle right,
        out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_is_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsLocationIdentity(LocationHandle location, out int isIdentity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_to_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocationToTransform(LocationHandle location, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseLocation(nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_located")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocateShape(
        ShapeHandle shape,
        LocationHandle location,
        int moved,
        out nint locatedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateVector(double x, double y, double z, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneVector(VectorHandle source, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorComponents(VectorHandle vector, out double x, out double y, out double z);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_magnitude")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorMagnitude(VectorHandle vector, out double magnitude);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_dot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorDot(VectorHandle left, VectorHandle right, out double dot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_crossed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CrossVectors(VectorHandle left, VectorHandle right, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseVector(nint vector);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateDirection(double x, double y, double z, out nint direction);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneDirection(DirectionHandle source, out nint direction);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetDirectionComponents(DirectionHandle direction, out double x, out double y, out double z);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_dot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetDirectionDot(DirectionHandle left, DirectionHandle right, out double dot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_reversed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReverseDirection(DirectionHandle source, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseDirection(nint direction);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAxis(double ox, double oy, double oz, double dx, double dy, double dz, out nint axis);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneAxis(AxisHandle source, out nint axis);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetAxisComponents(AxisHandle axis, out double ox, out double oy, out double oz, out double dx, out double dy, out double dz);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_reversed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReverseAxis(AxisHandle source, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseAxis(nint axis);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateMatrix(nint values, out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIdentityMatrix(out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneMatrix(MatrixHandle source, out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetMatrixValue(MatrixHandle matrix, int row, int column, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_determinant")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetMatrixDeterminant(MatrixHandle matrix, out double determinant);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseMatrix(nint matrix);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_translation_vec")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTranslationTransform(VectorHandle vector, out nint transform);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_rotation_axis")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRotationTransform(AxisHandle axis, double angleRadians, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAscii(nint utf8, int length, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneAscii(AsciiStringHandle source, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetAsciiLength(AsciiStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendAscii(AsciiStringHandle value, nint utf8, int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_to_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyAsciiUtf8(AsciiStringHandle value, nint buffer, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_to_extended")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ConvertAsciiToExtended(AsciiStringHandle value, out nint extended);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseAscii(nint value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_create_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateExtended(nint utf8, int length, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneExtended(ExtendedStringHandle source, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedLength(ExtendedStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_utf8_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedUtf8Length(ExtendedStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_append_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendExtendedUtf8(ExtendedStringHandle value, nint utf8, int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_to_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyExtendedUtf8(ExtendedStringHandle value, nint buffer, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedValue(ExtendedStringHandle value, int index, out ushort character);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_to_ascii")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ConvertExtendedToAscii(ExtendedStringHandle value, out nint ascii);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseExtended(nint value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealSequence(nint values, int count, out nint sequence);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealSequence(RealSequenceHandle source, out nint sequence);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealSequenceLength(RealSequenceHandle sequence, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealSequenceValue(RealSequenceHandle sequence, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendRealSequence(RealSequenceHandle sequence, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealSequenceValue(RealSequenceHandle sequence, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveRealSequence(RealSequenceHandle sequence, int index);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SnapshotRealSequence(RealSequenceHandle sequence, nint values, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealSequence(nint sequence);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealArray(nint values, int count, out nint array);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealArray(RealArrayHandle source, out nint array);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayLength(RealArrayHandle array, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_lower")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayLower(RealArrayHandle array, out int lower);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayValue(RealArrayHandle array, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealArrayValue(RealArrayHandle array, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SnapshotRealArray(RealArrayHandle array, nint values, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealArray(nint array);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealVector(nint values, int count, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealVector(RealVectorHandle source, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealVectorLength(RealVectorHandle vector, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealVectorValue(RealVectorHandle vector, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendRealVector(RealVectorHandle vector, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealVectorValue(RealVectorHandle vector, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SnapshotRealVector(RealVectorHandle vector, nint values, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealVector(nint vector);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIntRealMap(nint keys, nint values, int count, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneIntRealMap(IntRealMapHandle source, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_extent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntRealMapExtent(IntRealMapHandle map, out int extent);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_is_bound")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsIntRealMapBound(IntRealMapHandle map, int key, out int isBound);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_find")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FindIntRealMap(IntRealMapHandle map, int key, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_bind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BindIntRealMap(IntRealMapHandle map, int key, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_unbind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus UnbindIntRealMap(IntRealMapHandle map, int key, out int removed);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SnapshotIntRealMap(IntRealMapHandle map, nint keys, nint values, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseIntRealMap(nint map);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIntIndexedMap(nint keys, int count, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneIntIndexedMap(IntIndexedMapHandle source, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_extent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntIndexedMapExtent(IntIndexedMapHandle map, out int extent);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddIntIndexedMap(IntIndexedMapHandle map, int key, out int index, out int added);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_key")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntIndexedMapKey(IntIndexedMapHandle map, int index, out int key);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_find_index")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FindIntIndexedMapIndex(IntIndexedMapHandle map, int key, out int index);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_remove_last")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveLastIntIndexedMap(IntIndexedMapHandle map, out int removedKey);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SnapshotIntIndexedMap(IntIndexedMapHandle map, nint keys, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseIntIndexedMap(nint map);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_compound")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCompound(out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_compound_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddToCompound(ShapeHandle compound, ShapeHandle child);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_step_merge_xde",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MergeStepXde(
        nint inputs,
        int inputCount,
        string outputPath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateOcafDocument(out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus OpenOcafDocument(string filePath, out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_save", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SaveOcafDocument(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_has_open_command")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus HasOpenOcafCommand(OcafDocumentHandle document, out int hasOpenCommand);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_begin_command")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BeginOcafCommand(OcafDocumentHandle document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_commit_command")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CommitOcafCommand(OcafDocumentHandle document, out int changed);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_abort_command")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AbortOcafCommand(OcafDocumentHandle document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_main_entry")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetOcafMainEntry(OcafDocumentHandle document, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_label_add_child", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddOcafChild(OcafDocumentHandle document, string parentEntry, out int childTag);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_label_child_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetOcafChildCount(OcafDocumentHandle document, string entry, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_label_set_name", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetOcafLabelName(OcafDocumentHandle document, string entry, nint utf8, int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_label_name_utf8_length", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetOcafLabelNameLength(OcafDocumentHandle document, string entry, out int hasName, out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_label_name_to_utf8", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetOcafLabelName(OcafDocumentHandle document, string entry, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ocaf_document_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseOcafDocument(nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateXdeDocument(out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_import_step", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ImportStepIntoXdeDocument(
        OcafDocumentHandle document, string filePath, out int rootCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus OpenXdeDocument(string filePath, out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_read_step", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadStepXdeDocument(string filePath, out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_write_step", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteStepXdeDocument(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_add_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddXdeShape(OcafDocumentHandle document, ShapeHandle shape, nint nameUtf8, int nameLength, nint entryBuffer, int entryCapacity, out int entryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_add_assembly")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddXdeAssembly(OcafDocumentHandle document, nint nameUtf8, int nameLength, nint entryBuffer, int entryCapacity, out int entryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_add_component", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddXdeComponent(OcafDocumentHandle document, string assemblyEntry, string partEntry, LocationHandle location, nint entryBuffer, int entryCapacity, out int entryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_get_shape", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeShape(OcafDocumentHandle document, string entry, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_is_assembly", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsXdeAssembly(OcafDocumentHandle document, string entry, out int isAssembly);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_component_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeComponentCount(OcafDocumentHandle document, string entry, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_component_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeComponentEntry(OcafDocumentHandle document, string entry, int index, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_referred_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeReferredEntry(OcafDocumentHandle document, string entry, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_get_location", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeLocation(OcafDocumentHandle document, string entry, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_free_shape_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeFreeShapeCount(OcafDocumentHandle document, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_free_shape_entry")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeFreeShapeEntry(OcafDocumentHandle document, int index, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_color", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeColor(OcafDocumentHandle document, string entry, XdeColorRaw color);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_get_color", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeColor(OcafDocumentHandle document, string entry, out int hasColor, out XdeColorRaw color);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_layer", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeLayer(OcafDocumentHandle document, string entry, nint layerUtf8, int layerLength, int replaceExisting);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_layer_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeLayerCount(OcafDocumentHandle document, string entry, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_layer_name_utf8_length", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeLayerNameLength(OcafDocumentHandle document, string entry, int index, out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_layer_name_to_utf8", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeLayerName(OcafDocumentHandle document, string entry, int index, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_material", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeMaterial(OcafDocumentHandle document, string entry, nint name, int nameLength, nint description, int descriptionLength, double density, nint densityName, int densityNameLength, nint densityType, int densityTypeLength);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_material_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeMaterialInfo(OcafDocumentHandle document, string entry, out int hasMaterial, out double density);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_material_field_utf8_length", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeMaterialFieldLength(OcafDocumentHandle document, string entry, int field, out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_material_field_to_utf8", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeMaterialField(OcafDocumentHandle document, string entry, int field, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateViewer(nint windowHandle, out nint viewer);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_display_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus DisplayViewerShape(ViewerHandle viewer, ShapeHandle shape, out long presentationId);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_set_presentation_visible")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetViewerPresentationVisible(ViewerHandle viewer, long presentationId, int visible);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_remove_presentation")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveViewerPresentation(ViewerHandle viewer, long presentationId);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_fit_all")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FitAllViewer(ViewerHandle viewer);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_redraw")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RedrawViewer(ViewerHandle viewer);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_resize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ResizeViewer(ViewerHandle viewer);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_move_to")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MoveViewerTo(ViewerHandle viewer, int x, int y, out int detected);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_select_at")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SelectViewerAt(ViewerHandle viewer, int x, int y, out int selectedCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_selected_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SnapshotViewerSelection(ViewerHandle viewer, long* presentationIds, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_selected_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetViewerSelectedCount(ViewerHandle viewer, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseViewer(nint viewer);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseShape(nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create_null")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateNullTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create_derived")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateDerivedTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneTransient(
        SharedTransientHandle source,
        out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_try_cast_derived")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TryCastDerivedTransient(
        SharedTransientHandle source,
        out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_is_null")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsTransientNull(
        SharedTransientHandle handle,
        out int isNull);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_get_ref_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransientRefCount(
        SharedTransientHandle handle,
        out int referenceCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_get_type_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransientTypeName(
        SharedTransientHandle handle,
        out nint typeName);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_transient_is_kind",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsTransientKind(
        SharedTransientHandle handle,
        string typeName,
        out int isKind);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseTransient(nint handle);
}
