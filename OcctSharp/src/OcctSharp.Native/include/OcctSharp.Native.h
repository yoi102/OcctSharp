#pragma once

#include <stdint.h>

#if defined(_WIN32)
  #if defined(OCCTSHARP_NATIVE_EXPORTS)
    #define OCCTSHARP_API __declspec(dllexport)
  #else
    #define OCCTSHARP_API __declspec(dllimport)
  #endif
  #define OCCTSHARP_CALL __cdecl
#else
  #define OCCTSHARP_API __attribute__((visibility("default")))
  #define OCCTSHARP_CALL
#endif
#ifdef __cplusplus
extern "C" {
#endif

typedef enum OcctSharp_Status
{
  OCCTSHARP_STATUS_SUCCESS = 0,
  OCCTSHARP_STATUS_INVALID_ARGUMENT = 1,
  OCCTSHARP_STATUS_NULL_HANDLE = 2,
  OCCTSHARP_STATUS_OCCT_FAILURE = 3,
  OCCTSHARP_STATUS_STANDARD_EXCEPTION = 4,
  OCCTSHARP_STATUS_UNKNOWN_EXCEPTION = 5,
  OCCTSHARP_STATUS_FILE_IO_ERROR = 6,
  OCCTSHARP_STATUS_TRANSFER_FAILED = 7,
  OCCTSHARP_STATUS_INVALID_HANDLE = 8,
  OCCTSHARP_STATUS_TYPE_MISMATCH = 9
} OcctSharp_Status;

typedef struct OcctSharp_ShapeHandle OcctSharp_ShapeHandle;
typedef struct OcctSharp_TransientHandle OcctSharp_TransientHandle;
typedef struct OcctSharp_TrsfHandle OcctSharp_TrsfHandle;
typedef struct OcctSharp_LocationHandle OcctSharp_LocationHandle;
typedef struct OcctSharp_VecHandle OcctSharp_VecHandle;
typedef struct OcctSharp_DirHandle OcctSharp_DirHandle;
typedef struct OcctSharp_Ax1Handle OcctSharp_Ax1Handle;
typedef struct OcctSharp_MatHandle OcctSharp_MatHandle;
typedef struct OcctSharp_AsciiStringHandle OcctSharp_AsciiStringHandle;
typedef struct OcctSharp_ExtendedStringHandle OcctSharp_ExtendedStringHandle;
typedef struct OcctSharp_RealSequenceHandle OcctSharp_RealSequenceHandle;
typedef struct OcctSharp_RealArrayHandle OcctSharp_RealArrayHandle;
typedef struct OcctSharp_RealVectorHandle OcctSharp_RealVectorHandle;
typedef struct OcctSharp_IntRealMapHandle OcctSharp_IntRealMapHandle;
typedef struct OcctSharp_IntIndexedMapHandle OcctSharp_IntIndexedMapHandle;
typedef struct OcctSharp_GPropsHandle OcctSharp_GPropsHandle;
typedef struct OcctSharp_OcafDocumentHandle OcctSharp_OcafDocumentHandle;
typedef struct OcctSharp_ViewerHandle OcctSharp_ViewerHandle;

typedef struct OcctSharp_MeshVertex
{
  double x;
  double y;
  double z;
  double normal_x;
  double normal_y;
  double normal_z;
} OcctSharp_MeshVertex;

typedef struct OcctSharp_TopologyCounts
{
  int32_t vertex_count;
  int32_t edge_count;
  int32_t wire_count;
  int32_t face_count;
  int32_t shell_count;
  int32_t solid_count;
  int32_t compsolid_count;
  int32_t compound_count;
} OcctSharp_TopologyCounts;

typedef struct OcctSharp_ShapeTopologySummary
{
  OcctSharp_TopologyCounts unique_counts;
  OcctSharp_TopologyCounts occurrence_counts;
  int32_t is_closed;
  int32_t is_valid;
  double min_vertex_tolerance;
  double max_vertex_tolerance;
  double min_edge_tolerance;
  double max_edge_tolerance;
  double min_face_tolerance;
  double max_face_tolerance;
} OcctSharp_ShapeTopologySummary;

typedef struct OcctSharp_DetailedMeshVertex
{
  double x;
  double y;
  double z;
  double normal_x;
  double normal_y;
  double normal_z;
  double u;
  double v;
  int32_t has_uv;
} OcctSharp_DetailedMeshVertex;

typedef struct OcctSharp_DetailedMeshTriangle
{
  int32_t vertex_a;
  int32_t vertex_b;
  int32_t vertex_c;
  int32_t face_index;
  int32_t is_reversed;
} OcctSharp_DetailedMeshTriangle;

typedef struct OcctSharp_Xyz
{
  double x;
  double y;
  double z;
} OcctSharp_Xyz;
typedef struct OcctSharp_Line
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz direction;
} OcctSharp_Line;
typedef struct OcctSharp_Circle
{
  OcctSharp_Xyz center;
  OcctSharp_Xyz normal;
  double radius;
} OcctSharp_Circle;
typedef struct OcctSharp_Ax2
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz x_direction;
  OcctSharp_Xyz y_direction;
  OcctSharp_Xyz direction;
} OcctSharp_Ax2;
typedef struct OcctSharp_Ax3
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz x_direction;
  OcctSharp_Xyz y_direction;
  OcctSharp_Xyz direction;
} OcctSharp_Ax3;
typedef struct OcctSharp_Plane
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz normal;
} OcctSharp_Plane;
typedef struct OcctSharp_XdeColor
{
  double red;
  double green;
  double blue;
  double alpha;
} OcctSharp_XdeColor;

typedef struct OcctSharp_EdgeCurveSnapshot
{
  int32_t curve_type;
  double first_parameter;
  double last_parameter;
  OcctSharp_Xyz start_point;
  OcctSharp_Xyz end_point;
} OcctSharp_EdgeCurveSnapshot;

typedef struct OcctSharp_FaceSurfaceSnapshot
{
  int32_t surface_type;
  double first_u_parameter;
  double last_u_parameter;
  double first_v_parameter;
  double last_v_parameter;
} OcctSharp_FaceSurfaceSnapshot;

typedef struct OcctSharp_CurveEvaluation
{
  double parameter;
  OcctSharp_Xyz point;
  OcctSharp_Xyz tangent;
} OcctSharp_CurveEvaluation;

typedef struct OcctSharp_CurveProjection
{
  double parameter;
  OcctSharp_Xyz point;
  double distance;
  int32_t solution_count;
} OcctSharp_CurveProjection;

typedef struct OcctSharp_SurfaceEvaluation
{
  double u_parameter;
  double v_parameter;
  OcctSharp_Xyz point;
  OcctSharp_Xyz normal;
} OcctSharp_SurfaceEvaluation;

typedef struct OcctSharp_SurfaceProjection
{
  double u_parameter;
  double v_parameter;
  OcctSharp_Xyz point;
  double distance;
  int32_t solution_count;
} OcctSharp_SurfaceProjection;

typedef struct OcctSharp_BooleanHistorySummary
{
  int32_t left_source_count;
  int32_t left_modified_source_count;
  int32_t left_generated_source_count;
  int32_t left_deleted_source_count;
  int32_t left_modified_result_count;
  int32_t left_generated_result_count;
  int32_t right_source_count;
  int32_t right_modified_source_count;
  int32_t right_generated_source_count;
  int32_t right_deleted_source_count;
  int32_t right_modified_result_count;
  int32_t right_generated_result_count;
} OcctSharp_BooleanHistorySummary;

typedef struct OcctSharp_ShapeDistanceResult
{
  double distance;
  OcctSharp_Xyz point_on_first;
  OcctSharp_Xyz point_on_second;
  int32_t solution_count;
} OcctSharp_ShapeDistanceResult;

typedef struct OcctSharp_BoundingBox
{
  double min_x;
  double min_y;
  double min_z;
  double max_x;
  double max_y;
  double max_z;
} OcctSharp_BoundingBox;

typedef struct OcctSharp_StepAssemblyInput
{
  const char* file_path;
  double translation_x;
  double translation_y;
  double translation_z;
  double rotation_axis_x;
  double rotation_axis_y;
  double rotation_axis_z;
  double rotation_angle_radians;
} OcctSharp_StepAssemblyInput;

OCCTSHARP_API uint32_t OCCTSHARP_CALL occtsharp_get_abi_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_bridge_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_occt_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_last_error(void);

OCCTSHARP_API OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_default(void);
OCCTSHARP_API OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_create(double x, double y, double z);
OCCTSHARP_API OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_copy(OcctSharp_Xyz value);
OCCTSHARP_API OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_added(OcctSharp_Xyz left, OcctSharp_Xyz right);
OCCTSHARP_API OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_crossed(OcctSharp_Xyz left, OcctSharp_Xyz right);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_xyz_dot(OcctSharp_Xyz left, OcctSharp_Xyz right);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_xyz_modulus(OcctSharp_Xyz value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_xyz_normalized(
  OcctSharp_Xyz value, OcctSharp_Xyz* result);
OCCTSHARP_API OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_default(void);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_lin_create(
  OcctSharp_Xyz origin, OcctSharp_Xyz direction, OcctSharp_Line* result);
OCCTSHARP_API OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_reversed(OcctSharp_Line value);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_lin_distance(OcctSharp_Line line, OcctSharp_Xyz point);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_lin_angle(OcctSharp_Line left, OcctSharp_Line right);
OCCTSHARP_API OcctSharp_Circle OCCTSHARP_CALL occtsharp_gp_circ_default(void);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_circ_create(
  OcctSharp_Xyz center, OcctSharp_Xyz normal, double radius, OcctSharp_Circle* result);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_circ_area(OcctSharp_Circle value);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_circ_length(OcctSharp_Circle value);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_circ_distance(OcctSharp_Circle value, OcctSharp_Xyz point);
OCCTSHARP_API OcctSharp_Ax2 OCCTSHARP_CALL occtsharp_gp_ax2_default(void);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax2_create(
  OcctSharp_Xyz origin, OcctSharp_Xyz normal, OcctSharp_Xyz x_direction, OcctSharp_Ax2* result);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_ax2_angle(OcctSharp_Ax2 left, OcctSharp_Ax2 right);
OCCTSHARP_API OcctSharp_Ax3 OCCTSHARP_CALL occtsharp_gp_ax3_default(void);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax3_create(
  OcctSharp_Xyz origin, OcctSharp_Xyz normal, OcctSharp_Xyz x_direction, OcctSharp_Ax3* result);
OCCTSHARP_API int32_t OCCTSHARP_CALL occtsharp_gp_ax3_direct(OcctSharp_Ax3 value);
OCCTSHARP_API OcctSharp_Plane OCCTSHARP_CALL occtsharp_gp_pln_default(void);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_pln_create(
  OcctSharp_Xyz origin, OcctSharp_Xyz normal, OcctSharp_Plane* result);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_pln_distance(OcctSharp_Plane plane, OcctSharp_Xyz point);
OCCTSHARP_API double OCCTSHARP_CALL occtsharp_gp_pln_signed_distance(OcctSharp_Plane plane, OcctSharp_Xyz point);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_create(
  OcctSharp_GPropsHandle** out_properties);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_from_shape(
  const OcctSharp_ShapeHandle* shape, int32_t mode, int32_t only_closed, OcctSharp_GPropsHandle** out_properties);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_clone(
  const OcctSharp_GPropsHandle* source, OcctSharp_GPropsHandle** out_properties);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_add(
  OcctSharp_GPropsHandle* target, const OcctSharp_GPropsHandle* item, double density);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_mass(
  const OcctSharp_GPropsHandle* properties, double* mass);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_center(
  const OcctSharp_GPropsHandle* properties, OcctSharp_Xyz* center);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_inertia_value(
  const OcctSharp_GPropsHandle* properties, int32_t row, int32_t column, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_principal_moments(
  const OcctSharp_GPropsHandle* properties, double* first, double* second, double* third);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_symmetry(
  const OcctSharp_GPropsHandle* properties, int32_t* axis, int32_t* point);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_gprops_release(OcctSharp_GPropsHandle* properties);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_curve_snapshot(
  const OcctSharp_ShapeHandle* edge, OcctSharp_EdgeCurveSnapshot* out_snapshot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_surface_snapshot(
  const OcctSharp_ShapeHandle* face, int32_t restrict_to_face,
  OcctSharp_FaceSurfaceSnapshot* out_snapshot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate(
  const OcctSharp_ShapeHandle* edge, double parameter, OcctSharp_CurveEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_length(
  const OcctSharp_ShapeHandle* edge, double* out_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_project_point(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz point, OcctSharp_CurveProjection* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate(
  const OcctSharp_ShapeHandle* face, double u_parameter, double v_parameter,
  OcctSharp_SurfaceEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_project_point(
  const OcctSharp_ShapeHandle* face, OcctSharp_Xyz point, double tolerance,
  OcctSharp_SurfaceProjection* out_result);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_box(
  double size_x,
  double size_y,
  double size_z,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_null(
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_sphere(
  double radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cylinder(
  double radius, double height, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cone(
  double bottom_radius, double top_radius, double height, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_torus(
  double major_radius, double minor_radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wedge(
  double size_x, double size_y, double size_z, double top_x_length,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_edge(
  OcctSharp_Xyz start, OcctSharp_Xyz end, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_circle_edge(
  OcctSharp_Xyz center, OcctSharp_Xyz normal, double radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_arc_edge(
  OcctSharp_Xyz start, OcctSharp_Xyz middle, OcctSharp_Xyz end, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_ellipse_edge(
  OcctSharp_Xyz center, OcctSharp_Xyz normal, OcctSharp_Xyz x_direction,
  double major_radius, double minor_radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_bezier_edge(
  const OcctSharp_Xyz* poles, int32_t count, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_interpolated_edge(
  const OcctSharp_Xyz* points, int32_t count, int32_t periodic, double tolerance,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_loft(
  const OcctSharp_ShapeHandle* const* sections, int32_t count,
  int32_t make_solid, int32_t ruled, double tolerance, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_pipe(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* profile,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_sew(
  const OcctSharp_ShapeHandle* const* shapes, int32_t count, double tolerance,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_polygon_wire(
  const OcctSharp_Xyz* points, int32_t count, int32_t close,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_planar_face(
  const OcctSharp_ShapeHandle* wire, OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_get_face_count(
  const OcctSharp_ShapeHandle* shape,
  int32_t* out_face_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_snapshot(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_faces,
  int32_t capacity,
  int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_snapshot(
  const OcctSharp_ShapeHandle* shape,
  int32_t kind,
  OcctSharp_ShapeHandle** out_shapes,
  int32_t capacity,
  int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_count(
  const OcctSharp_ShapeHandle* shape, int32_t kind, int32_t* out_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_count(
  const OcctSharp_ShapeHandle* shape, int32_t item_kind, int32_t ancestor_kind,
  int32_t* out_item_count, int32_t* out_ancestor_count, int32_t* out_relation_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_snapshot(
  const OcctSharp_ShapeHandle* shape, int32_t item_kind, int32_t ancestor_kind,
  OcctSharp_ShapeHandle** out_items, int32_t item_capacity,
  OcctSharp_ShapeHandle** out_ancestors, int32_t ancestor_capacity,
  int32_t* out_offsets, int32_t offset_capacity,
  int32_t* out_ancestor_indices, int32_t relation_capacity,
  int32_t* out_items_written, int32_t* out_ancestors_written, int32_t* out_relations_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_extrude(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_VecHandle* direction,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_revolve(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_Ax1Handle* axis,
  double angle_radians, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_all(
  const OcctSharp_ShapeHandle* shape, double radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  double radius, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_all(
  const OcctSharp_ShapeHandle* shape, double distance, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  double distance, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_offset(
  const OcctSharp_ShapeHandle* shape, double offset, double tolerance,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_make_thick_solid(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_ShapeHandle* const* closing_faces, int32_t face_count,
  double offset, double tolerance, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_section(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_bounding_box(
  const OcctSharp_ShapeHandle* shape, OcctSharp_BoundingBox* out_bounds);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_is_valid(
  const OcctSharp_ShapeHandle* shape, int32_t* out_is_valid);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_summary(
  const OcctSharp_ShapeHandle* shape, OcctSharp_ShapeTopologySummary* out_summary);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_fuse(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_cut(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_common(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_with_history(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  int32_t operation, int32_t tracked_kind,
  OcctSharp_ShapeHandle** out_shape, OcctSharp_BooleanHistorySummary* out_history);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_distance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_ShapeDistanceResult* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fix(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_unify_same_domain(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection,
  double angular_deflection,
  int32_t* out_vertex_count,
  int32_t* out_index_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection,
  double angular_deflection,
  OcctSharp_MeshVertex* vertices,
  int32_t vertex_capacity,
  int32_t* out_vertex_count,
  int32_t* indices,
  int32_t index_capacity,
  int32_t* out_index_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_detailed_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection,
  double angular_deflection,
  int32_t* out_vertex_count,
  int32_t* out_triangle_count,
  int32_t* out_face_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_detailed_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection,
  double angular_deflection,
  OcctSharp_DetailedMeshVertex* vertices,
  int32_t vertex_capacity,
  int32_t* out_vertex_count,
  OcctSharp_DetailedMeshTriangle* triangles,
  int32_t triangle_capacity,
  int32_t* out_triangle_count,
  int32_t* out_face_count);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_brep(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_iges(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_stl(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_obj(
  const char* file_path, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_gltf(
  const char* file_path, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_vrml(
  const char* file_path, OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_step(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_brep(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_stl(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path,
  double linear_deflection,
  double angular_deflection,
  int32_t binary);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_iges(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_obj(
  const OcctSharp_ShapeHandle* shape, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_ply(
  const OcctSharp_ShapeHandle* shape, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_gltf(
  const OcctSharp_ShapeHandle* shape, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_vrml(
  const OcctSharp_ShapeHandle* shape, const char* file_path);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform(
  const OcctSharp_ShapeHandle* shape,
  double translation_x,
  double translation_y,
  double translation_z,
  double rotation_axis_x,
  double rotation_axis_y,
  double rotation_axis_z,
  double rotation_angle_radians,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_identity(
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_rotation(
  double translation_x,
  double translation_y,
  double translation_z,
  double rotation_axis_x,
  double rotation_axis_y,
  double rotation_axis_z,
  double rotation_angle_radians,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_clone(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_inverted(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_multiplied(
  const OcctSharp_TrsfHandle* left,
  const OcctSharp_TrsfHandle* right,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_value(
  const OcctSharp_TrsfHandle* transform,
  int32_t row,
  int32_t column,
  double* out_value);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_trsf_release(OcctSharp_TrsfHandle* transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform_trsf(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_identity(
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_from_trsf(
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_clone(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_inverted(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_multiplied(
  const OcctSharp_LocationHandle* left,
  const OcctSharp_LocationHandle* right,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_is_identity(
  const OcctSharp_LocationHandle* location,
  int32_t* out_is_identity);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_to_trsf(
  const OcctSharp_LocationHandle* location,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_location_release(OcctSharp_LocationHandle* location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_located(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_LocationHandle* location,
  int32_t moved,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_create(
  double x, double y, double z, OcctSharp_VecHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_clone(
  const OcctSharp_VecHandle* source, OcctSharp_VecHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_components(
  const OcctSharp_VecHandle* vector, double* x, double* y, double* z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_magnitude(
  const OcctSharp_VecHandle* vector, double* magnitude);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_dot(
  const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, double* dot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_crossed(
  const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, OcctSharp_VecHandle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_vec_release(OcctSharp_VecHandle* vector);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_create(
  double x, double y, double z, OcctSharp_DirHandle** out_direction);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_clone(
  const OcctSharp_DirHandle* source, OcctSharp_DirHandle** out_direction);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_components(
  const OcctSharp_DirHandle* direction, double* x, double* y, double* z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_dot(
  const OcctSharp_DirHandle* left, const OcctSharp_DirHandle* right, double* dot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_reversed(
  const OcctSharp_DirHandle* source, OcctSharp_DirHandle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_dir_release(OcctSharp_DirHandle* direction);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_create(
  double origin_x, double origin_y, double origin_z,
  double direction_x, double direction_y, double direction_z,
  OcctSharp_Ax1Handle** out_axis);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_clone(
  const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** out_axis);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_components(
  const OcctSharp_Ax1Handle* axis,
  double* origin_x, double* origin_y, double* origin_z,
  double* direction_x, double* direction_y, double* direction_z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_reversed(
  const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_ax1_release(OcctSharp_Ax1Handle* axis);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_create(
  const double* values, OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_identity(
  OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_clone(
  const OcctSharp_MatHandle* source, OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_value(
  const OcctSharp_MatHandle* matrix, int32_t row, int32_t column, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_determinant(
  const OcctSharp_MatHandle* matrix, double* determinant);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_mat_release(OcctSharp_MatHandle* matrix);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_vec(
  const OcctSharp_VecHandle* vector, OcctSharp_TrsfHandle** out_transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_rotation_axis(
  const OcctSharp_Ax1Handle* axis, double angle_radians, OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_create(
  const char* utf8, int32_t length, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_clone(
  const OcctSharp_AsciiStringHandle* source, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_length(
  const OcctSharp_AsciiStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_append(
  OcctSharp_AsciiStringHandle* string, const char* utf8, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_utf8(
  const OcctSharp_AsciiStringHandle* string, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_extended(
  const OcctSharp_AsciiStringHandle* string, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_ascii_release(OcctSharp_AsciiStringHandle* string);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_create_utf8(
  const char* utf8, int32_t length, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_clone(
  const OcctSharp_ExtendedStringHandle* source, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_length(
  const OcctSharp_ExtendedStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_utf8_length(
  const OcctSharp_ExtendedStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_append_utf8(
  OcctSharp_ExtendedStringHandle* string, const char* utf8, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_utf8(
  const OcctSharp_ExtendedStringHandle* string, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_value(
  const OcctSharp_ExtendedStringHandle* string, int32_t index, uint16_t* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_ascii(
  const OcctSharp_ExtendedStringHandle* string, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_extended_release(OcctSharp_ExtendedStringHandle* string);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_create(
  const double* values, int32_t count, OcctSharp_RealSequenceHandle** out_sequence);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_clone(
  const OcctSharp_RealSequenceHandle* source, OcctSharp_RealSequenceHandle** out_sequence);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_length(
  const OcctSharp_RealSequenceHandle* sequence, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_value(
  const OcctSharp_RealSequenceHandle* sequence, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_append(
  OcctSharp_RealSequenceHandle* sequence, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_set_value(
  OcctSharp_RealSequenceHandle* sequence, int32_t index, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_remove(
  OcctSharp_RealSequenceHandle* sequence, int32_t index);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_snapshot(
  const OcctSharp_RealSequenceHandle* sequence, double* values, int32_t capacity, int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_sequence_release(OcctSharp_RealSequenceHandle* sequence);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_create(
  const double* values, int32_t count, OcctSharp_RealArrayHandle** out_array);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_clone(
  const OcctSharp_RealArrayHandle* source, OcctSharp_RealArrayHandle** out_array);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_length(
  const OcctSharp_RealArrayHandle* array, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_lower(
  const OcctSharp_RealArrayHandle* array, int32_t* lower);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_value(
  const OcctSharp_RealArrayHandle* array, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_set_value(
  OcctSharp_RealArrayHandle* array, int32_t index, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_snapshot(
  const OcctSharp_RealArrayHandle* array, double* values, int32_t capacity, int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_array_release(OcctSharp_RealArrayHandle* array);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_create(
  const double* values, int32_t count, OcctSharp_RealVectorHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_clone(
  const OcctSharp_RealVectorHandle* source, OcctSharp_RealVectorHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_length(
  const OcctSharp_RealVectorHandle* vector, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_value(
  const OcctSharp_RealVectorHandle* vector, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_append(
  OcctSharp_RealVectorHandle* vector, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_set_value(
  OcctSharp_RealVectorHandle* vector, int32_t index, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_snapshot(
  const OcctSharp_RealVectorHandle* vector, double* values, int32_t capacity, int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_vector_release(OcctSharp_RealVectorHandle* vector);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_create(
  const int32_t* keys, const double* values, int32_t count, OcctSharp_IntRealMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_clone(
  const OcctSharp_IntRealMapHandle* source, OcctSharp_IntRealMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_extent(
  const OcctSharp_IntRealMapHandle* map, int32_t* extent);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_is_bound(
  const OcctSharp_IntRealMapHandle* map, int32_t key, int32_t* is_bound);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_find(
  const OcctSharp_IntRealMapHandle* map, int32_t key, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_bind(
  OcctSharp_IntRealMapHandle* map, int32_t key, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_unbind(
  OcctSharp_IntRealMapHandle* map, int32_t key, int32_t* removed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_snapshot(
  const OcctSharp_IntRealMapHandle* map, int32_t* keys, double* values, int32_t capacity, int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_int_real_map_release(OcctSharp_IntRealMapHandle* map);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_create(
  const int32_t* keys, int32_t count, OcctSharp_IntIndexedMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_clone(
  const OcctSharp_IntIndexedMapHandle* source, OcctSharp_IntIndexedMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_extent(
  const OcctSharp_IntIndexedMapHandle* map, int32_t* extent);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_add(
  OcctSharp_IntIndexedMapHandle* map, int32_t key, int32_t* index, int32_t* added);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_key(
  const OcctSharp_IntIndexedMapHandle* map, int32_t index, int32_t* key);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_find_index(
  const OcctSharp_IntIndexedMapHandle* map, int32_t key, int32_t* index);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_remove_last(
  OcctSharp_IntIndexedMapHandle* map, int32_t* removed_key);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_snapshot(
  const OcctSharp_IntIndexedMapHandle* map, int32_t* keys, int32_t capacity, int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_int_indexed_map_release(OcctSharp_IntIndexedMapHandle* map);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_compound(
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_compound_add(
  OcctSharp_ShapeHandle* compound,
  const OcctSharp_ShapeHandle* child);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_merge_xde(
  const OcctSharp_StepAssemblyInput* inputs,
  int32_t input_count,
  const char* output_path);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_create(
  OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_save(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_has_open_command(
  const OcctSharp_OcafDocumentHandle* document, int32_t* has_open_command);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_begin_command(
  OcctSharp_OcafDocumentHandle* document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_commit_command(
  OcctSharp_OcafDocumentHandle* document, int32_t* changed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_abort_command(
  OcctSharp_OcafDocumentHandle* document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_main_entry(
  const OcctSharp_OcafDocumentHandle* document,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_add_child(
  OcctSharp_OcafDocumentHandle* document, const char* parent_entry, int32_t* child_tag);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_child_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_set_name(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* utf8,
  int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_name,
  int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_ocaf_document_release(
  OcctSharp_OcafDocumentHandle* document);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_create(
  OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_import_step(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, int32_t* out_root_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_shape(
  OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_ShapeHandle* shape,
  const char* name_utf8,
  int32_t name_length,
  char* entry_buffer,
  int32_t entry_capacity,
  int32_t* entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_assembly(
  OcctSharp_OcafDocumentHandle* document,
  const char* name_utf8,
  int32_t name_length,
  char* entry_buffer,
  int32_t entry_capacity,
  int32_t* entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_component(
  OcctSharp_OcafDocumentHandle* document,
  const char* assembly_entry,
  const char* part_entry,
  const OcctSharp_LocationHandle* location,
  char* entry_buffer,
  int32_t entry_capacity,
  int32_t* entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_shape(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_is_assembly(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* is_assembly);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_referred_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_location(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_LocationHandle** out_location);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_count(
  const OcctSharp_OcafDocumentHandle* document, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_entry(
  const OcctSharp_OcafDocumentHandle* document,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_color(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_XdeColor color);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_color(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_color,
  OcctSharp_XdeColor* color);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_layer(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* layer_utf8,
  int32_t layer_length,
  int32_t replace_existing);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t index,
  int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_material(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* name,
  int32_t name_length,
  const char* description,
  int32_t description_length,
  double density,
  const char* density_name,
  int32_t density_name_length,
  const char* density_type,
  int32_t density_type_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_info(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_material,
  double* density);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t field,
  int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t field,
  char* buffer,
  int32_t capacity,
  int32_t* written);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create(
  intptr_t window_handle, OcctSharp_ViewerHandle** out_viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_shape(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ShapeHandle* shape,
  int64_t* presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_visible(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  int32_t visible);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_color(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  double red,
  double green,
  double blue);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_transparency(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  double transparency);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_display_mode(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  int32_t display_mode);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_presentation(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_all(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_redraw(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_resize(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_projection(
  OcctSharp_ViewerHandle* viewer,
  int32_t projection);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_zoom(
  OcctSharp_ViewerHandle* viewer,
  double factor);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pan(
  OcctSharp_ViewerHandle* viewer,
  int32_t delta_x,
  int32_t delta_y);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_move_to(
  OcctSharp_ViewerHandle* viewer,
  int32_t x,
  int32_t y,
  int32_t* detected);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at(
  OcctSharp_ViewerHandle* viewer,
  int32_t x,
  int32_t y,
  int32_t* selected_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at_mode(
  OcctSharp_ViewerHandle* viewer,
  int32_t x,
  int32_t y,
  int32_t selection_mode,
  int32_t* selected_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_selection(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_snapshot(
  OcctSharp_ViewerHandle* viewer,
  int64_t* presentation_ids,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_count(
  OcctSharp_ViewerHandle* viewer,
  int32_t* count);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_viewer_release(OcctSharp_ViewerHandle* viewer);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_shape_release(OcctSharp_ShapeHandle* shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_null(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_derived(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_clone(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_try_cast_derived(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_null(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_is_null);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_ref_count(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_ref_count);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_type_name(
  const OcctSharp_TransientHandle* handle,
  const char** out_type_name);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_kind(
  const OcctSharp_TransientHandle* handle,
  const char* type_name,
  int32_t* out_is_kind);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_transient_release(OcctSharp_TransientHandle* handle);

#ifdef __cplusplus
}
#endif
