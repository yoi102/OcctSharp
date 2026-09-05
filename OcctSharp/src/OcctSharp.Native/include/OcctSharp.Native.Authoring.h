#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

/* All inputs are call-duration borrowed buffers; output values are copied.
   Span kinds: constant=0, linear=1, interpolation=2, BSpline=3, smooth=4.
   Offsets are zero-based into the shared numeric buffers. */
typedef struct OcctSharp_LawSpan {
  int32_t kind, degree, tangents, value_offset, value_count, parameter_offset, parameter_count, multiplicity_offset;
  double first, last, value_first, value_last, derivative_first, derivative_last;
  double active_first, active_last;
} OcctSharp_LawSpan;
typedef struct OcctSharp_LawInput {
  const OcctSharp_LawSpan* spans;
  const double* values;
  const int32_t* multiplicities;
  int32_t span_count, value_count, multiplicity_count, reserved;
  double first, last;
} OcctSharp_LawInput;
typedef struct OcctSharp_LawSample {
  double parameter, value, first_derivative, second_derivative;
  int32_t defined, reserved;
} OcctSharp_LawSample;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_law_evaluate(
  const OcctSharp_LawInput* input, const double* parameters, int32_t count,
  OcctSharp_LawSample* samples, int32_t capacity, double* conservative_lower_bound);

typedef struct OcctSharp_AuthoringInfo {
  int32_t ready, done, valid, solid, algorithm_status, history_count, section_count, continuity_limit;
  double approximation_error;
  int32_t error_available, reserved;
} OcctSharp_AuthoringInfo;
typedef struct OcctSharp_AuthoringHistoryInfo {
  int32_t source_index, subshape_index, source_kind, kind;
} OcctSharp_AuthoringHistoryInfo;
typedef struct OcctSharp_SweepSection { int32_t shape_index, location_index, contact, correction; } OcctSharp_SweepSection;
typedef struct OcctSharp_SweepOptions {
  int32_t frame, secondary_index, curvilinear, contact, transition, maximum_degree, maximum_segments, force_c1;
  int32_t solid_policy, simulation_count, operation, reserved;
  double tolerance_3d, tolerance_boundary, tolerance_angular;
  OcctSharp_Xyz origin, direction, x_direction;
} OcctSharp_SweepOptions;
typedef struct OcctSharp_LoftOptions {
  int32_t solid, ruled, compatibility, smoothing, maximum_degree, continuity, parameterization, reserved;
  double tolerance, weight_1, weight_2, weight_3;
} OcctSharp_LoftOptions;

/* FeatureResult is reused only as a temporary registered result owner. New history
   kinds: 0 modified, 1 generated, 2 first, 3 last, 4 simulated, 5 unmapped,
   6 compatible section, 7 input snapshot. source/subshape indices are exact, not geometric matches. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_copy_inputs(
  const OcctSharp_ShapeHandle* const* inputs, int32_t count, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_history(
  const OcctSharp_FeatureResultHandle* result, int32_t index,
  OcctSharp_AuthoringHistoryInfo* info, OcctSharp_ShapeHandle** shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_guided_sweep(
  const OcctSharp_ShapeHandle* const* inputs, int32_t count,
  const OcctSharp_SweepSection* sections, int32_t section_count,
  const OcctSharp_SweepOptions* options, const OcctSharp_LawInput* law,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_guided_loft(
  const OcctSharp_ShapeHandle* const* inputs, int32_t count, const OcctSharp_LoftOptions* options,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output);

typedef struct OcctSharp_FillConstraint {
  int32_t kind, shape_index, support_index, order, boundary, required, id, reserved;
  double u, v;
  OcctSharp_Xyz point;
} OcctSharp_FillConstraint;
typedef struct OcctSharp_FillOptions {
  int32_t degree, points_per_curve, iterations, anisotropic, maximum_degree, maximum_segments, seed_index, verification_samples;
  double tolerance_2d, tolerance_3d, tolerance_angular, tolerance_curvature;
} OcctSharp_FillOptions;
typedef struct OcctSharp_ConstraintResidual {
  int32_t id, kernel_index, defined, accepted;
  double position, angle, curvature;
  int32_t sample_count, required;
} OcctSharp_ConstraintResidual;
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_constrained_fill(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count,
  const OcctSharp_FillConstraint* constraints, int32_t constraint_count,
  const OcctSharp_FillOptions* options, OcctSharp_ConstraintResidual* residuals, int32_t capacity,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output);

typedef struct OcctSharp_PatchOptions {
  int32_t operation, style, with_ratio, minimum_multiplicity, bezier, reserved;
  double first, last, first_u, last_u, first_v, last_v, tolerance;
} OcctSharp_PatchOptions;
typedef struct OcctSharp_PatchSpan {
  int32_t source_index, u_index, v_index, orientation;
  double first, last, first_v, last_v, result_first, result_last;
} OcctSharp_PatchSpan;
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_patch_convert(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count, const OcctSharp_PatchOptions* options,
  OcctSharp_PatchSpan* spans, int32_t capacity, int32_t* span_count,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_surface_join(
  const OcctSharp_ShapeHandle* boundary, const OcctSharp_ShapeHandle* first_face, const OcctSharp_ShapeHandle* second_face,
  int32_t count, double tolerance, OcctSharp_ConstraintResidual* residuals, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_curve_join(
  const OcctSharp_ShapeHandle* first_curve, const OcctSharp_ShapeHandle* second_curve,
  double first_parameter, double second_parameter, int32_t reverse_second,
  OcctSharp_ConstraintResidual* residual);

#ifdef __cplusplus
}
static_assert(sizeof(OcctSharp_LawSpan) == 96);
static_assert(sizeof(OcctSharp_LawInput) == 56);
static_assert(sizeof(OcctSharp_LawSample) == 40);
static_assert(sizeof(OcctSharp_AuthoringInfo) == 48);
static_assert(sizeof(OcctSharp_AuthoringHistoryInfo) == 16);
static_assert(sizeof(OcctSharp_SweepSection) == 16);
static_assert(sizeof(OcctSharp_SweepOptions) == 144);
static_assert(sizeof(OcctSharp_LoftOptions) == 64);
static_assert(sizeof(OcctSharp_FillConstraint) == 72);
static_assert(sizeof(OcctSharp_FillOptions) == 64);
static_assert(sizeof(OcctSharp_ConstraintResidual) == 48);
static_assert(sizeof(OcctSharp_PatchOptions) == 80);
static_assert(sizeof(OcctSharp_PatchSpan) == 64);
#endif
