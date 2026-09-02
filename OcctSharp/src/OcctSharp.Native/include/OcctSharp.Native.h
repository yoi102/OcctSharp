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
typedef struct OcctSharp_StepReaderHandle OcctSharp_StepReaderHandle;
typedef struct OcctSharp_FeatureResultHandle OcctSharp_FeatureResultHandle;

typedef struct OcctSharp_FeatureOptions
{
  double fuzzy_tolerance;
  int32_t run_parallel;
  int32_t non_destructive;
  int32_t glue_mode;
  int32_t repair_inputs;
  int32_t unify_result;
} OcctSharp_FeatureOptions;

typedef struct OcctSharp_FeatureResultInfo
{
  int32_t operation;
  int32_t succeeded;
  int32_t recovered;
  int32_t error_count;
  int32_t warning_count;
  int32_t faulty_shape_count;
  int32_t modified_count;
  int32_t generated_count;
  int32_t deleted_count;
  int32_t result_is_valid;
} OcctSharp_FeatureResultInfo;

typedef struct OcctSharp_FeatureHistoryInfo
{
  int32_t source_index;
  int32_t kind;
} OcctSharp_FeatureHistoryInfo;

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

typedef struct OcctSharp_ValidationIssue
{
  int32_t shape_kind;
  int32_t status;
} OcctSharp_ValidationIssue;

typedef struct OcctSharp_StepReadReport
{
  int32_t candidate_root_count;
  int32_t transferred_root_count;
  int32_t shape_count;
  int32_t read_status;
  double system_length_unit;
} OcctSharp_StepReadReport;

typedef struct OcctSharp_Xyz
{
  double x;
  double y;
  double z;
} OcctSharp_Xyz;
typedef struct OcctSharp_FreeformCurveInfo
{
  int32_t kind;
  int32_t degree;
  int32_t periodic;
  int32_t rational;
  int32_t pole_count;
  int32_t knot_count;
  double first_parameter;
  double last_parameter;
} OcctSharp_FreeformCurveInfo;
typedef struct OcctSharp_FreeformSurfaceInfo
{
  int32_t kind;
  int32_t u_degree;
  int32_t v_degree;
  int32_t u_periodic;
  int32_t v_periodic;
  int32_t rational;
  int32_t u_pole_count;
  int32_t v_pole_count;
  int32_t u_knot_count;
  int32_t v_knot_count;
  double first_u;
  double last_u;
  double first_v;
  double last_v;
} OcctSharp_FreeformSurfaceInfo;
typedef struct OcctSharp_FreeformSolution
{
  OcctSharp_Xyz first_point;
  OcctSharp_Xyz second_point;
  double first_parameter;
  double second_parameter;
  double third_parameter;
  double distance;
} OcctSharp_FreeformSolution;
typedef struct OcctSharp_FreeformDiagnostics
{
  int32_t status;
  int32_t input_count;
  int32_t result_count;
  int32_t modified_count;
  int32_t generated_count;
  int32_t deleted_count;
  int32_t is_valid;
  int32_t is_closed;
  double g0_error;
  double g1_error;
  double g2_error;
  double approximation_error;
} OcctSharp_FreeformDiagnostics;
typedef struct OcctSharp_DrawingProjection
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz view_direction;
  OcctSharp_Xyz up_direction;
  int32_t perspective;
  double focus;
} OcctSharp_DrawingProjection;
typedef struct OcctSharp_DrawingPolyline
{
  int32_t point_offset;
  int32_t point_count;
  int32_t closed;
} OcctSharp_DrawingPolyline;
typedef struct OcctSharp_ViewerCamera
{
  OcctSharp_Xyz eye;
  OcctSharp_Xyz target;
  OcctSharp_Xyz up;
  OcctSharp_Xyz projection;
} OcctSharp_ViewerCamera;
typedef struct OcctSharp_ViewerPickRay
{
  OcctSharp_Xyz origin;
  OcctSharp_Xyz direction;
} OcctSharp_ViewerPickRay;
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
typedef struct OcctSharp_ViewerManipulatorState
{
  int32_t attached;
  int32_t active_mode;
  int32_t active_axis;
  int32_t has_active_transformation;
  int32_t activation_on_detection;
  int32_t zoom_persistence;
  int32_t skin;
  int32_t reserved;
  double size;
  double gap;
  OcctSharp_Ax2 position;
} OcctSharp_ViewerManipulatorState;
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

typedef struct OcctSharp_XdePresentationStyle
{
  int32_t is_visible;
  int32_t has_surface_color;
  int32_t has_curve_color;
  int32_t has_material_color;
  OcctSharp_XdeColor surface_color;
  OcctSharp_XdeColor curve_color;
  OcctSharp_XdeColor material_color;
} OcctSharp_XdePresentationStyle;

typedef struct OcctSharp_XdeValidationProperties
{
  double area;
  double volume;
  OcctSharp_Xyz centroid;
  int32_t has_area;
  int32_t has_volume;
  int32_t has_centroid;
} OcctSharp_XdeValidationProperties;

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

typedef struct OcctSharp_CurveDerivativeEvaluation
{
  double parameter;
  OcctSharp_Xyz point;
  OcctSharp_Xyz first_derivative;
  OcctSharp_Xyz second_derivative;
} OcctSharp_CurveDerivativeEvaluation;

typedef struct OcctSharp_Xy
{
  double x;
  double y;
} OcctSharp_Xy;

typedef struct OcctSharp_PcurveSnapshot
{
  double first_parameter;
  double last_parameter;
  OcctSharp_Xy start_point;
  OcctSharp_Xy end_point;
} OcctSharp_PcurveSnapshot;

typedef struct OcctSharp_PcurveEvaluation
{
  double parameter;
  OcctSharp_Xy point;
  OcctSharp_Xy tangent;
} OcctSharp_PcurveEvaluation;

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

typedef struct OcctSharp_SurfaceDerivativeEvaluation
{
  double u_parameter;
  double v_parameter;
  OcctSharp_Xyz point;
  OcctSharp_Xyz u_derivative;
  OcctSharp_Xyz v_derivative;
  OcctSharp_Xyz normal;
} OcctSharp_SurfaceDerivativeEvaluation;

typedef struct OcctSharp_StepReaderInfo
{
  int32_t candidate_root_count;
  int32_t read_status;
  double system_length_unit;
  int32_t length_unit_count;
  int32_t angle_unit_count;
  int32_t solid_angle_unit_count;
} OcctSharp_StepReaderInfo;

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

typedef struct OcctSharp_ExtremaSolution
{
  double distance;
  OcctSharp_Xyz point_on_first;
  OcctSharp_Xyz point_on_second;
  int32_t first_support_kind;
  int32_t second_support_kind;
  int32_t has_first_edge_parameter;
  double first_edge_parameter;
  int32_t has_second_edge_parameter;
  double second_edge_parameter;
  int32_t has_first_face_parameters;
  double first_face_u;
  double first_face_v;
  int32_t has_second_face_parameters;
  double second_face_u;
  double second_face_v;
  int32_t is_inner_solution;
  OcctSharp_ShapeHandle* first_support;
  OcctSharp_ShapeHandle* second_support;
} OcctSharp_ExtremaSolution;

typedef struct OcctSharp_InspectionProperties
{
  double mass;
  OcctSharp_Xyz center;
  double i11;
  double i12;
  double i13;
  double i21;
  double i22;
  double i23;
  double i31;
  double i32;
  double i33;
} OcctSharp_InspectionProperties;

typedef struct OcctSharp_RadialMeasurement
{
  int32_t geometry_kind;
  double radius;
  double diameter;
  double semi_angle;
} OcctSharp_RadialMeasurement;

typedef struct OcctSharp_PmiDimension
{
  int32_t type;
  int32_t has_qualifier;
  int32_t qualifier;
  int32_t has_angular_qualifier;
  int32_t angular_qualifier;
  int32_t has_class_of_tolerance;
  int32_t is_hole;
  int32_t form_variance;
  int32_t grade;
  int32_t left_decimal_places;
  int32_t right_decimal_places;
  int32_t has_direction;
  OcctSharp_Xyz direction;
  int32_t has_plane;
  OcctSharp_Ax2 plane;
  int32_t has_first_point;
  OcctSharp_Xyz first_point;
  int32_t has_second_point;
  OcctSharp_Xyz second_point;
  int32_t has_text_point;
  OcctSharp_Xyz text_point;
} OcctSharp_PmiDimension;

typedef struct OcctSharp_PmiTolerance
{
  int32_t type;
  int32_t type_of_value;
  double value;
  int32_t material_requirement;
  int32_t zone_modifier;
  double zone_modifier_value;
  double maximum_value_modifier;
  int32_t has_axis;
  OcctSharp_Ax2 axis;
  int32_t has_plane;
  OcctSharp_Ax2 plane;
  int32_t has_point;
  OcctSharp_Xyz point;
  int32_t has_text_point;
  OcctSharp_Xyz text_point;
  int32_t affected_plane_type;
  OcctSharp_Plane affected_plane;
} OcctSharp_PmiTolerance;

typedef struct OcctSharp_PmiDatum
{
  int32_t position;
  int32_t is_datum_target;
  int32_t target_type;
  double target_length;
  double target_width;
  int32_t target_number;
  int32_t has_target_axis;
  OcctSharp_Ax2 target_axis;
  int32_t has_plane;
  OcctSharp_Ax2 plane;
  int32_t has_point;
  OcctSharp_Xyz point;
  int32_t has_text_point;
  OcctSharp_Xyz text_point;
  int32_t has_modifier_with_value;
  int32_t modifier_with_value;
  double modifier_value;
} OcctSharp_PmiDatum;

typedef struct OcctSharp_SavedView
{
  int32_t projection_type;
  OcctSharp_Xyz projection_point;
  OcctSharp_Xyz view_direction;
  OcctSharp_Xyz up_direction;
  double zoom_factor;
  double window_horizontal_size;
  double window_vertical_size;
  int32_t has_front_clipping;
  double front_clipping_distance;
  int32_t has_back_clipping;
  double back_clipping_distance;
  int32_t has_view_volume_sides_clipping;
} OcctSharp_SavedView;

typedef struct OcctSharp_PlaneEquation
{
  double a;
  double b;
  double c;
  double d;
  int32_t capping;
} OcctSharp_PlaneEquation;

typedef struct OcctSharp_BoundingBox
{
  double min_x;
  double min_y;
  double min_z;
  double max_x;
  double max_y;
  double max_z;
} OcctSharp_BoundingBox;

typedef struct OcctSharp_OrientedBoundingBox
{
  OcctSharp_Xyz center;
  OcctSharp_Xyz x_direction;
  OcctSharp_Xyz y_direction;
  OcctSharp_Xyz z_direction;
  double half_size_x;
  double half_size_y;
  double half_size_z;
} OcctSharp_OrientedBoundingBox;

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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate_derivatives(
  const OcctSharp_ShapeHandle* edge, double parameter,
  OcctSharp_CurveDerivativeEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_snapshot(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_PcurveSnapshot* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_evaluate(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, double parameter,
  OcctSharp_PcurveEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_length(
  const OcctSharp_ShapeHandle* edge, double* out_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_project_point(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz point, OcctSharp_CurveProjection* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate(
  const OcctSharp_ShapeHandle* face, double u_parameter, double v_parameter,
  OcctSharp_SurfaceEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate_derivatives(
  const OcctSharp_ShapeHandle* face, double u_parameter, double v_parameter,
  OcctSharp_SurfaceDerivativeEvaluation* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_project_point(
  const OcctSharp_ShapeHandle* face, OcctSharp_Xyz point, double tolerance,
  OcctSharp_SurfaceProjection* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_trim(
  const OcctSharp_ShapeHandle* edge, double first_parameter, double last_parameter,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_trim(
  const OcctSharp_ShapeHandle* face,
  double first_u_parameter, double last_u_parameter,
  double first_v_parameter, double last_v_parameter,
  double tolerance, OcctSharp_ShapeHandle** out_shape);

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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wire(
  const OcctSharp_ShapeHandle* const* edges, int32_t count,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_planar_face(
  const OcctSharp_ShapeHandle* wire, OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_create(
  int32_t kind, const OcctSharp_Xyz* poles, const double* weights, int32_t pole_count,
  const double* knots, const int32_t* multiplicities, int32_t knot_count,
  int32_t degree, int32_t periodic, OcctSharp_ShapeHandle** out_edge);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_interpolate(
  const OcctSharp_Xyz* points, int32_t point_count, const OcctSharp_Xyz* endpoint_tangents,
  int32_t tangent_count, int32_t periodic, double tolerance, OcctSharp_ShapeHandle** out_edge);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_approximate(
  const OcctSharp_Xyz* points, int32_t point_count, int32_t minimum_degree,
  int32_t maximum_degree, int32_t continuity, double tolerance, OcctSharp_ShapeHandle** out_edge);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_info(
  const OcctSharp_ShapeHandle* edge, OcctSharp_FreeformCurveInfo* out_info);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_copy_definition(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz* poles, int32_t pole_capacity,
  double* weights, int32_t weight_capacity, double* knots, int32_t knot_capacity,
  int32_t* multiplicities, int32_t multiplicity_capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_edit(
  const OcctSharp_ShapeHandle* edge, int32_t operation, int32_t degree,
  double first_parameter, double last_parameter, OcctSharp_ShapeHandle** out_edge);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_split(
  const OcctSharp_ShapeHandle* edge, const double* parameters, int32_t parameter_count,
  OcctSharp_ShapeHandle** out_edges, int32_t capacity, int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_count(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz point, int32_t* out_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_copy(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz point,
  OcctSharp_FreeformSolution* solutions, int32_t capacity, int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, int32_t* out_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_copy(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_FreeformSolution* solutions, int32_t capacity, int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_count(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, int32_t* out_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_copy(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_FreeformSolution* solutions, int32_t capacity, int32_t* out_written);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_profile(
  const OcctSharp_Xyz* points, int32_t point_count, OcctSharp_Xyz origin,
  OcctSharp_Xyz normal, OcctSharp_Xyz x_direction, int32_t interpolate,
  double tolerance, OcctSharp_ShapeHandle** out_wire);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_offset(
  const OcctSharp_ShapeHandle* wire, double distance, double altitude,
  int32_t join_type, OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_create(
  int32_t kind, const OcctSharp_Xyz* poles, const double* weights,
  int32_t u_pole_count, int32_t v_pole_count,
  const double* u_knots, const int32_t* u_multiplicities, int32_t u_knot_count,
  const double* v_knots, const int32_t* v_multiplicities, int32_t v_knot_count,
  int32_t u_degree, int32_t v_degree, int32_t u_periodic, int32_t v_periodic,
  const double* bounds, double tolerance, OcctSharp_ShapeHandle** out_face);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_approximate(
  const OcctSharp_Xyz* points, int32_t u_count, int32_t v_count,
  int32_t minimum_degree, int32_t maximum_degree, int32_t continuity,
  double tolerance, OcctSharp_ShapeHandle** out_face);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_info(
  const OcctSharp_ShapeHandle* face, OcctSharp_FreeformSurfaceInfo* out_info);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_copy_definition(
  const OcctSharp_ShapeHandle* face, OcctSharp_Xyz* poles, int32_t pole_capacity,
  double* weights, int32_t weight_capacity,
  double* u_knots, int32_t u_knot_capacity, int32_t* u_multiplicities, int32_t u_multiplicity_capacity,
  double* v_knots, int32_t v_knot_capacity, int32_t* v_multiplicities, int32_t v_multiplicity_capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_edit(
  const OcctSharp_ShapeHandle* face, int32_t operation, int32_t u_degree, int32_t v_degree,
  const double* bounds, double tolerance, OcctSharp_ShapeHandle** out_face);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_ruled_face(
  const OcctSharp_ShapeHandle* first_edge, const OcctSharp_ShapeHandle* second_edge,
  OcctSharp_ShapeHandle** out_face);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_fill(
  const OcctSharp_ShapeHandle* const* edges, int32_t edge_count,
  const OcctSharp_Xyz* points, int32_t point_count, int32_t continuity,
  double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics,
  OcctSharp_ShapeHandle** out_face);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_split(
  const OcctSharp_ShapeHandle* const* objects, int32_t object_count,
  const OcctSharp_ShapeHandle* const* tools, int32_t tool_count,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_pipe_shell(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* const* profiles,
  int32_t profile_count, int32_t make_solid, int32_t frenet, int32_t transition_mode,
  double tolerance, int32_t maximum_degree, int32_t maximum_segments,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_loft(
  const OcctSharp_ShapeHandle* const* sections, int32_t section_count,
  int32_t make_solid, int32_t ruled, int32_t smoothing, int32_t continuity,
  int32_t maximum_degree, double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_heal(
  const OcctSharp_ShapeHandle* shape, double tolerance,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_compute(
  const OcctSharp_ShapeHandle* const* shapes, int32_t shape_count,
  OcctSharp_DrawingProjection projection, int32_t exact, int32_t iso_count,
  double deflection, OcctSharp_ShapeHandle** out_layers, int32_t layer_capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_section(
  const OcctSharp_ShapeHandle* shape, OcctSharp_Xyz plane_origin,
  OcctSharp_Xyz plane_normal, int32_t approximate, OcctSharp_ShapeHandle** out_section);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_count(
  const OcctSharp_ShapeHandle* shape, int32_t samples_per_curve,
  int32_t* out_polyline_count, int32_t* out_point_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_copy(
  const OcctSharp_ShapeHandle* shape, int32_t samples_per_curve,
  OcctSharp_DrawingPolyline* polylines, int32_t polyline_capacity,
  OcctSharp_Xyz* points, int32_t point_capacity,
  int32_t* out_polylines_written, int32_t* out_points_written);

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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_replace_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  const OcctSharp_ShapeHandle* replacement, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_remove_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  OcctSharp_ShapeHandle** out_shape);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_oriented_bounding_box(
  const OcctSharp_ShapeHandle* shape, OcctSharp_OrientedBoundingBox* out_bounds);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_digital_mockup_candidate_pairs(
  const OcctSharp_ShapeHandle* const* shapes, int32_t shape_count, double expansion,
  int32_t* pairs, int32_t pair_capacity, int32_t* out_pair_count,
  int32_t* out_axis_comparison_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_is_valid(
  const OcctSharp_ShapeHandle* shape, int32_t* out_is_valid);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_summary(
  const OcctSharp_ShapeHandle* shape, OcctSharp_ShapeTopologySummary* out_summary);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issue_count(
  const OcctSharp_ShapeHandle* shape, int32_t geometry_checks, int32_t exact,
  int32_t* out_is_valid, int32_t* out_issue_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issues(
  const OcctSharp_ShapeHandle* shape, int32_t geometry_checks, int32_t exact,
  OcctSharp_ValidationIssue* issues, int32_t capacity,
  int32_t* out_is_valid, int32_t* out_issue_count);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_execute(
  int32_t operation,
  const OcctSharp_ShapeHandle* const* shapes, int32_t shape_count,
  int32_t primary_count, int32_t secondary_count,
  const double* parameters, int32_t parameter_count,
  const OcctSharp_Xyz* vectors, int32_t vector_count,
  OcctSharp_FeatureOptions options,
  OcctSharp_FeatureResultHandle** out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_info(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_FeatureResultInfo* out_info);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_shape(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_history(
  const OcctSharp_FeatureResultHandle* result, int32_t index,
  OcctSharp_FeatureHistoryInfo* out_info, OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_deleted(
  const OcctSharp_FeatureResultHandle* result, int32_t index, int32_t* out_source_index);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_message(
  const OcctSharp_FeatureResultHandle* result, char* buffer, int32_t capacity,
  int32_t* out_written);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_feature_result_release(
  OcctSharp_FeatureResultHandle* result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_distance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_ShapeDistanceResult* out_result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_solution(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  int32_t index, OcctSharp_ExtremaSolution* solution);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_pair_classify(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  double tolerance, int32_t* classification, double* distance,
  double* overlap_volume, OcctSharp_ShapeHandle** overlap_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_digital_mockup_pair_analyze(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  double confusion_tolerance, double fuzzy_tolerance, int32_t run_parallel,
  int32_t non_destructive, int32_t* classification, double* distance,
  double* overlap_volume, OcctSharp_ShapeHandle** issue_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_inspection_properties(
  const OcctSharp_ShapeHandle* shape, int32_t property_kind,
  OcctSharp_InspectionProperties* properties);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_angle(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  double* radians);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_radial_measurement(
  const OcctSharp_ShapeHandle* shape, OcctSharp_RadialMeasurement* measurement);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_advanced_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection, double angular_deflection, double minimum_size,
  int32_t relative, int32_t parallel, int32_t internal_vertices,
  int32_t control_surface_deflection,
  int32_t* out_vertex_count, int32_t* out_triangle_count, int32_t* out_face_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_advanced_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  double linear_deflection, double angular_deflection, double minimum_size,
  int32_t relative, int32_t parallel, int32_t internal_vertices,
  int32_t control_surface_deflection,
  OcctSharp_DetailedMeshVertex* vertices, int32_t vertex_capacity,
  int32_t* out_vertex_count,
  OcctSharp_DetailedMeshTriangle* triangles, int32_t triangle_capacity,
  int32_t* out_triangle_count, int32_t* out_face_count);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_brep(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step_report(
  const char* file_path, OcctSharp_ShapeHandle** out_shape,
  OcctSharp_StepReadReport* out_report);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_open(
  const char* file_path, double target_system_length_unit,
  OcctSharp_StepReaderHandle** out_reader, OcctSharp_StepReaderInfo* out_info);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_unit_utf8_length(
  const OcctSharp_StepReaderHandle* reader, int32_t unit_kind, int32_t unit_index,
  int32_t* out_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_unit_to_utf8(
  const OcctSharp_StepReaderHandle* reader, int32_t unit_kind, int32_t unit_index,
  char* buffer, int32_t capacity, int32_t* out_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_transfer_root(
  OcctSharp_StepReaderHandle* reader, int32_t root_index,
  OcctSharp_ShapeHandle** out_shape);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_step_reader_release(
  OcctSharp_StepReaderHandle* reader);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_save_format(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, int32_t xde, int32_t xml);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_label_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* tag, int32_t* depth, int32_t* is_root, int32_t* has_parent,
  char* parent_buffer, int32_t parent_capacity, int32_t* parent_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_child_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t index,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t index,
  int32_t* kind, char* id_buffer, int32_t id_capacity, int32_t* id_written,
  char* type_buffer, int32_t type_capacity, int32_t* type_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  int32_t* has_value, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  const char* utf8, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_scalar_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  int32_t* has_value, int32_t* integer_value, double* real_value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_scalar_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  int32_t integer_value, double real_value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_remove(
  OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_array_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  int32_t* has_value, int32_t* lower, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_integer_array_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* values, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_real_array_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  double* values, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_integer_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t lower,
  const int32_t* values, int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_real_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t lower,
  const double* values, int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t array,
  int32_t* has_value, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t array,
  int32_t index, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const char* target_entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const char* const* target_entries, int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_node, int32_t* has_parent, int32_t* child_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t parent,
  int32_t index, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_reparent(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const char* parent_entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_detach(
  OcctSharp_OcafDocumentHandle* document, const char* entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_named_shape_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_shape, OcctSharp_ShapeHandle** shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_named_shape_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const OcctSharp_ShapeHandle* shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_commit_named_command(
  OcctSharp_OcafDocumentHandle* document, const char* utf8, int32_t length, int32_t* changed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_state(
  const OcctSharp_OcafDocumentHandle* document, int32_t* undo_limit,
  int32_t* undo_count, int32_t* redo_count, int32_t* is_changed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_set_limit(
  OcctSharp_OcafDocumentHandle* document, int32_t undo_limit);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_action(
  OcctSharp_OcafDocumentHandle* document, int32_t action, int32_t* changed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_info(
  const OcctSharp_OcafDocumentHandle* document, int32_t redo, int32_t index,
  int32_t* begin_time, int32_t* end_time, int32_t* delta_count,
  int32_t* label_count, int32_t* name_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_name(
  const OcctSharp_OcafDocumentHandle* document, int32_t redo, int32_t index,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_label(
  const OcctSharp_OcafDocumentHandle* document, int32_t redo, int32_t index,
  int32_t label_index, char* buffer, int32_t capacity, int32_t* written);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_gltf(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_obj(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step_options(
  const char* file_path,
  int32_t read_names,
  int32_t read_colors,
  int32_t read_layers,
  int32_t read_validation_properties,
  int32_t read_materials,
  int32_t read_gdt,
  int32_t read_views,
  OcctSharp_OcafDocumentHandle** out_document);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_gltf(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_obj(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_ply(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_vrml(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step_options(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  int32_t model_type,
  int32_t schema,
  int32_t write_names,
  int32_t write_colors,
  int32_t write_layers,
  int32_t write_validation_properties,
  int32_t write_materials,
  int32_t write_gdt);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_count(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_entry(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, int32_t index,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDimension* data,
  const double* values, int32_t value_count, const int32_t* modifiers, int32_t modifier_count,
  const char* semantic_name, const char* presentation_name,
  const char* description, const char* description_name,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDimension* data,
  const double* values, int32_t value_count, const int32_t* modifiers, int32_t modifier_count,
  const char* semantic_name, const char* presentation_name,
  const char* description, const char* description_name);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, OcctSharp_PmiDimension* data,
  int32_t* value_count, int32_t* modifier_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, int32_t modifier_count,
  const char* semantic_name, const char* presentation_name,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, int32_t modifier_count,
  const char* semantic_name, const char* presentation_name);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, OcctSharp_PmiTolerance* data,
  int32_t* modifier_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, int32_t modifier_count,
  const char* name, const char* description, const char* identification,
  const char* semantic_name, const char* presentation_name,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, int32_t modifier_count,
  const char* name, const char* description, const char* identification,
  const char* semantic_name, const char* presentation_name);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, OcctSharp_PmiDatum* data,
  int32_t* modifier_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_numeric_item(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  int32_t field, int32_t index, double* real_value, int32_t* integer_value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  int32_t field, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  int32_t field, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_aux_shape(
  OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  int32_t role, const OcctSharp_ShapeHandle* shape, const char* name);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_clear_aux_shape(
  OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry, int32_t role);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_get_aux_shape(
  const OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  int32_t role, int32_t* has_shape, OcctSharp_ShapeHandle** shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_references(
  OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry,
  const char* first_entries, const char* second_entries);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_count(
  const OcctSharp_OcafDocumentHandle* document, int32_t relation,
  const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_entry(
  const OcctSharp_OcafDocumentHandle* document, int32_t relation,
  const char* entry, int32_t index, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_remove(
  OcctSharp_OcafDocumentHandle* document, int32_t kind, const char* entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, int32_t plane_count,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, int32_t plane_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_SavedView* data, int32_t* plane_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_plane(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t index, OcctSharp_PlaneEquation* plane);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_remove(
  OcctSharp_OcafDocumentHandle* document, const char* entry);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_shape(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_ShapeHandle* shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_location(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_LocationHandle* location,
  char* result_entry_buffer,
  int32_t result_entry_capacity,
  int32_t* result_entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_remove_component(
  OcctSharp_OcafDocumentHandle* document, const char* entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_remove_shape(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t remove_completely,
  int32_t* removed);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_user_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t recursive,
  int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_user_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t recursive,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_clone_subtree(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* result_entry_buffer,
  int32_t result_entry_capacity,
  int32_t* result_entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_external_references(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* const* references,
  int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t index,
  int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_assembly_item_reference(
  OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  const char* item_path,
  int32_t subshape_index);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_assembly_item_reference_info(
  const OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  int32_t* has_reference,
  int32_t* is_orphan,
  int32_t* subshape_index,
  int32_t* path_length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_assembly_item_reference_path(
  const OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  char* buffer,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_create(
  OcctSharp_OcafDocumentHandle* document,
  const char* const* occurrence_entries,
  int32_t count,
  char* result_entry_buffer,
  int32_t result_entry_capacity,
  int32_t* result_entry_written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_link_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* shuo_entry,
  int32_t upper,
  int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_link_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* shuo_entry,
  int32_t upper,
  int32_t index,
  char* buffer,
  int32_t capacity,
  int32_t* written);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_presentation_style_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_presentation_style_snapshot(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_ShapeHandle** shapes,
  OcctSharp_XdePresentationStyle* styles,
  int32_t capacity,
  int32_t* written);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_visual_material(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const char* name, int32_t name_length,
  double red, double green, double blue, double alpha,
  double metallic, double roughness,
  double emissive_red, double emissive_green, double emissive_blue,
  double refraction_index, int32_t alpha_mode, double alpha_cutoff);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material,
  double* red, double* green, double* blue, double* alpha,
  double* metallic, double* roughness,
  double* emissive_red, double* emissive_green, double* emissive_blue,
  double* refraction_index, int32_t* alpha_mode, double* alpha_cutoff);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_validation_properties(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_XdeValidationProperties* properties);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_validation_properties(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_XdeValidationProperties* properties);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create(
  intptr_t window_handle, OcctSharp_ViewerHandle** out_viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_shape(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ShapeHandle* shape,
  int64_t* presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_xde_label(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int64_t* presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_create(
  OcctSharp_ViewerHandle* viewer,
  int32_t kind,
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_Xyz* points,
  int32_t point_count,
  const OcctSharp_PlaneEquation* plane,
  const char* model_units,
  const char* display_units,
  int32_t has_custom_value,
  double custom_value,
  double flyout,
  double red,
  double green,
  double blue,
  double line_width,
  int64_t* dimension_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_update_style(
  OcctSharp_ViewerHandle* viewer,
  int64_t dimension_id,
  const char* model_units,
  const char* display_units,
  int32_t has_custom_value,
  double custom_value,
  double flyout,
  double red,
  double green,
  double blue,
  double line_width);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_visible(
  OcctSharp_ViewerHandle* viewer, int64_t dimension_id, int32_t visible);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_selected(
  OcctSharp_ViewerHandle* viewer, int64_t dimension_id, int32_t selected);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_remove(
  OcctSharp_ViewerHandle* viewer, int64_t dimension_id);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_selection_kind(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  int32_t shape_kind);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_presentation(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_get_transform(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id, OcctSharp_TrsfHandle** transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_set_transform(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id, const OcctSharp_TrsfHandle* transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_reset_transform(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_attach(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id, int32_t adjust_position,
  int32_t adjust_size, int32_t enable_modes, int64_t* manipulator_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_part(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t axis,
  int32_t mode, int32_t enabled);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_enable_mode(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t mode);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_activation_on_detection(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t enabled);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_position(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, const OcctSharp_Ax2* position);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_appearance(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, double size, double gap, int32_t skin);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_zoom_persistence(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t enabled);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_start(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t x, int32_t y);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_transform_mouse(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t x, int32_t y,
  OcctSharp_TrsfHandle** transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_transform_custom(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, const OcctSharp_TrsfHandle* transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_stop(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id, int32_t apply);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_get_state(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id,
  OcctSharp_ViewerManipulatorState* state);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_detach(
  OcctSharp_ViewerHandle* viewer, int64_t manipulator_id);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_start_rotation(
  OcctSharp_ViewerHandle* viewer, int32_t x, int32_t y, double z_rotation_threshold);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_rotate(
  OcctSharp_ViewerHandle* viewer, int32_t x, int32_t y);
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
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer,
  int64_t* presentation_ids,
  OcctSharp_ShapeHandle** shapes,
  int32_t capacity,
  int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_count(
  OcctSharp_ViewerHandle* viewer,
  int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_detected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer,
  int64_t* presentation_id,
  OcctSharp_ShapeHandle** shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_rectangle(
  OcctSharp_ViewerHandle* viewer,
  int32_t min_x,
  int32_t min_y,
  int32_t max_x,
  int32_t max_y,
  int32_t selection_mode,
  int32_t* selected_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_polygon(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_Xy* points,
  int32_t point_count,
  int32_t selection_mode,
  int32_t* selected_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_pixel_tolerance(
  OcctSharp_ViewerHandle* viewer,
  int32_t tolerance);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_shape_filter(
  OcctSharp_ViewerHandle* viewer,
  int32_t shape_kind);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_filters(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selection_bounds(
  OcctSharp_ViewerHandle* viewer,
  int32_t* has_bounds,
  OcctSharp_BoundingBox* bounds);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_selected(
  OcctSharp_ViewerHandle* viewer,
  double margin,
  int32_t* fitted);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_color(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape,
  double red,
  double green,
  double blue);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_transparency(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape,
  double transparency);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_width(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape,
  double width);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_subshape_overrides(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_all_subshape_overrides(
  OcctSharp_ViewerHandle* viewer,
  int64_t presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_get_camera(
  OcctSharp_ViewerHandle* viewer,
  OcctSharp_ViewerCamera* camera);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_camera(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ViewerCamera* camera);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_screen_to_world(
  OcctSharp_ViewerHandle* viewer,
  int32_t x,
  int32_t y,
  OcctSharp_Xyz* point);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_world_to_screen(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_Xyz* point,
  int32_t* x,
  int32_t* y);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pick_ray(
  OcctSharp_ViewerHandle* viewer,
  int32_t x,
  int32_t y,
  OcctSharp_ViewerPickRay* ray);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_window_fit(
  OcctSharp_ViewerHandle* viewer,
  int32_t min_x,
  int32_t min_y,
  int32_t max_x,
  int32_t max_y);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_background_color(
  OcctSharp_ViewerHandle* viewer,
  double red,
  double green,
  double blue);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create_clip_plane(
  OcctSharp_ViewerHandle* viewer,
  double a,
  double b,
  double c,
  double d,
  int64_t* clip_plane_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_update_clip_plane(
  OcctSharp_ViewerHandle* viewer,
  int64_t clip_plane_id,
  double a,
  double b,
  double c,
  double d);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_clip_plane_enabled(
  OcctSharp_ViewerHandle* viewer,
  int64_t clip_plane_id,
  int32_t enabled);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_clip_plane(
  OcctSharp_ViewerHandle* viewer,
  int64_t clip_plane_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_computed_mode(
  OcctSharp_ViewerHandle* viewer,
  int32_t enabled);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_show_trihedron(
  OcctSharp_ViewerHandle* viewer,
  int32_t position,
  double red,
  double green,
  double blue,
  double scale);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_hide_trihedron(
  OcctSharp_ViewerHandle* viewer);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dump(
  OcctSharp_ViewerHandle* viewer,
  const char* file_path,
  int32_t buffer_type);
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
