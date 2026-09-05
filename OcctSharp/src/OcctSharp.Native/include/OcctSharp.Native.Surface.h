#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct OcctSharp_SurfaceInfo {
  int32_t kind, orientation, closed_u, closed_v, periodic_u, periodic_v;
  double first_u, last_u, first_v, last_v, period_u, period_v;
} OcctSharp_SurfaceInfo;

typedef struct OcctSharp_SurfaceSample {
  int32_t state, normal_defined, curvature_defined, reserved;
  OcctSharp_SketchPoint2d uv;
  OcctSharp_Xyz point, du, dv, normal;
  double minimum_curvature, maximum_curvature, mean_curvature, gaussian_curvature;
} OcctSharp_SurfaceSample;

typedef struct OcctSharp_SurfacePointSolution {
  int32_t source_index, state;
  OcctSharp_SketchPoint2d uv;
  OcctSharp_Xyz point;
  double distance;
} OcctSharp_SurfacePointSolution;

typedef struct OcctSharp_SurfaceCurveInfo {
  int32_t degree, periodic, pole_count, knot_count, reversed, exact, parameter_preserved, reserved;
  double first, last, source_first, source_last, residual;
} OcctSharp_SurfaceCurveInfo;

typedef struct OcctSharp_SurfaceProjectionOptions {
  double tolerance_3d, tolerance_2d, maximum_distance;
  int32_t limit_to_face, maximum_degree, maximum_segments, continuity;
} OcctSharp_SurfaceProjectionOptions;

typedef struct OcctSharp_SurfaceRepairInfo {
  int32_t valid_before, valid_after, edges_before, edges_after;
  int32_t missing_3d_before, missing_3d_after, inconsistent_before, inconsistent_after;
  double tolerance_before, tolerance_after;
} OcctSharp_SurfaceRepairInfo;

typedef struct OcctSharp_SurfaceBoundaryInfo {
  int32_t loop_index, outer, orientation, seam;
  int32_t degenerate, reserved;
  double length;
} OcctSharp_SurfaceBoundaryInfo;

typedef struct OcctSharp_SurfaceIntersection {
  int32_t kind, state;
  double first_parameter, last_parameter;
  OcctSharp_Xyz first_point, last_point;
  OcctSharp_SketchPoint2d first_uv, last_uv;
} OcctSharp_SurfaceIntersection;

typedef struct OcctSharp_SurfaceCurveSample {
  double parameter;
  OcctSharp_SketchPoint2d uv;
  OcctSharp_Xyz point, tangent;
} OcctSharp_SurfaceCurveSample;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_describe(
  const OcctSharp_ShapeHandle*, OcctSharp_SurfaceInfo*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_evaluate(
  const OcctSharp_ShapeHandle*, const OcctSharp_SketchPoint2d*, int32_t, double, OcctSharp_SurfaceSample*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_classify(
  const OcctSharp_ShapeHandle*, const OcctSharp_SketchPoint2d*, int32_t, double, int32_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_sample_curve(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle*, int32_t, double, OcctSharp_SurfaceCurveSample*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_project_points(
  const OcctSharp_ShapeHandle*, const OcctSharp_Xyz*, int32_t, int32_t, double,
  OcctSharp_SurfacePointSolution*, int32_t, int32_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_iso(
  const OcctSharp_ShapeHandle*, int32_t, double, double, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_curve_definition(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle*, int32_t, int32_t, double,
  OcctSharp_SurfaceCurveInfo*, OcctSharp_SketchPoint2d*, double*, int32_t, double*, int32_t*, int32_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_fit_uv(
  const OcctSharp_SketchPoint2d*, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, double,
  OcctSharp_SurfaceCurveInfo*, OcctSharp_SketchPoint2d*, double*, int32_t, double*, int32_t*, int32_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_offset_uv(
  const OcctSharp_SketchCurve*, double, double, OcctSharp_SurfaceCurveInfo*,
  OcctSharp_SketchPoint2d*, double*, int32_t, double*, int32_t*, int32_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_lift_curve(
  const OcctSharp_ShapeHandle*, const OcctSharp_SketchCurve*, int32_t, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_project_shape(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle*,
  const OcctSharp_SurfaceProjectionOptions*, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_make_wire(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle* const*, int32_t, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_make_face(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle* const*, int32_t, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_repair(
  const OcctSharp_ShapeHandle*, int32_t, double, double, OcctSharp_SurfaceRepairInfo*, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_boundary(
  const OcctSharp_ShapeHandle*, OcctSharp_SurfaceBoundaryInfo*, OcctSharp_ShapeHandle**, int32_t, int32_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_split(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle* const*, int32_t, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_create_analytic(
  int32_t, const OcctSharp_SketchPlane*, double, double, const double*, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_section(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle*, double, OcctSharp_ShapeHandle**);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_intersect_curve(
  const OcctSharp_ShapeHandle*, const OcctSharp_ShapeHandle*, double,
  OcctSharp_SurfaceIntersection*, int32_t, int32_t*);

#ifdef __cplusplus
}
#endif
