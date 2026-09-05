#pragma once
#include "OcctSharp.Native.Authoring.h"

#ifdef __cplusplus
extern "C" {
#endif

/* Fixed copied contracts. All topology indices are zero-based in the exact input
   copy correspondence, never addresses or geometric nearest-neighbour matches. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_source_subshape(
  const OcctSharp_ShapeHandle* source, int32_t index, OcctSharp_ShapeHandle** output);

typedef struct OcctSharp_LocalFeatureInfo {
  int32_t operation, ready, done, valid, partial, algorithm_status, composed, group_support;
  int32_t contour_count, edge_count, section_count, fault_count, history_count, reserved;
} OcctSharp_LocalFeatureInfo;
typedef struct OcctSharp_ContourInfo {
  int32_t index, program, seed, first_vertex, last_vertex, closed, tangent, reserved;
  double length;
  double law_probe_error;
  int32_t law_sample_count, law_approximated;
} OcctSharp_ContourInfo;
typedef struct OcctSharp_ContourEdge {
  int32_t contour, ordinal, source_index, first_vertex, last_vertex, reserved;
  double first_parameter, last_parameter;
} OcctSharp_ContourEdge;
typedef struct OcctSharp_FilletSection {
  int32_t contour, patch, ordinal, reserved;
  OcctSharp_Xyz center, normal, x_direction;
  double radius, first_parameter, last_parameter;
} OcctSharp_FilletSection;
typedef struct OcctSharp_LocalFeatureFault {
  int32_t kind, contour, source_index, status;
} OcctSharp_LocalFeatureFault;
typedef struct OcctSharp_LocalFeatureHistory {
  int32_t argument_index, topology_index, source_kind, kind, group, result_topology_index;
} OcctSharp_LocalFeatureHistory;
typedef struct OcctSharp_RadiusSample { double parameter, radius; } OcctSharp_RadiusSample;
typedef struct OcctSharp_VertexRadius { int32_t vertex, reserved; double radius; } OcctSharp_VertexRadius;
typedef struct OcctSharp_FilletProgram {
  int32_t seed, mode, sample_offset, sample_count, law_index, vertex_offset, vertex_count, reserved;
  double radius;
} OcctSharp_FilletProgram;
typedef struct OcctSharp_FilletOptions {
  int32_t action, representation, continuity, reserved;
  double tangent_tolerance, tolerance_3d, tolerance_2d, approximation_3d, approximation_2d, deflection, angular_tolerance;
} OcctSharp_FilletOptions;
typedef struct OcctSharp_ChamferProgram {
  int32_t seed, support, method, reserved;
  double first, second;
} OcctSharp_ChamferProgram;
typedef struct OcctSharp_FaceDraftProgram {
  int32_t face, propagation, reserved1, reserved2;
  double angle;
  OcctSharp_Xyz direction, plane_origin, plane_normal;
} OcctSharp_FaceDraftProgram;
typedef struct OcctSharp_ShellDraftOptions {
  int32_t limit_kind, keep, internal_draft, transition;
  double angle, length, angle_minimum, angle_maximum;
  OcctSharp_Xyz direction;
} OcctSharp_ShellDraftOptions;
typedef struct OcctSharp_SlidingPair { int32_t edge_input, face_input; } OcctSharp_SlidingPair;
typedef struct OcctSharp_LimitedFeatureOptions {
  int32_t operation, limit_mode, fuse, modify, support_input, from_input, until_input, path_input;
  double extent, draft_angle;
  OcctSharp_Xyz origin, direction;
} OcctSharp_LimitedFeatureOptions;
typedef struct OcctSharp_RibSlotOptions {
  int32_t revolution, fuse, sliding, angular_limit;
  OcctSharp_Xyz plane_origin, plane_normal, direction1, direction2, axis_origin, axis_direction;
  double thickness1, thickness2, angle_first, angle_last;
} OcctSharp_RibSlotOptions;
typedef struct OcctSharp_LocalHoleOptions {
  int32_t mode, reserved1, reserved2, reserved3;
  OcctSharp_Xyz origin, direction;
  double radius, first, last;
} OcctSharp_LocalHoleOptions;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_contour_fillet(
  const OcctSharp_ShapeHandle* source, const OcctSharp_FilletProgram* programs, int32_t program_count,
  const OcctSharp_RadiusSample* samples, int32_t sample_count, const OcctSharp_VertexRadius* vertices, int32_t vertex_count,
  const OcctSharp_LawInput* laws, int32_t law_count, const OcctSharp_FilletOptions* options,
  OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_contour_chamfer(
  const OcctSharp_ShapeHandle* source, const OcctSharp_ChamferProgram* programs, int32_t program_count,
  int32_t mode, int32_t build, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_face_draft(
  const OcctSharp_ShapeHandle* source, const OcctSharp_FaceDraftProgram* programs, int32_t program_count,
  int32_t build, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shell_draft(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count, const OcctSharp_ShellDraftOptions* options,
  OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_limited_prism(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count, const OcctSharp_SlidingPair* sliding, int32_t sliding_count,
  const OcctSharp_LimitedFeatureOptions* options, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_limited_sweep(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count, const OcctSharp_SlidingPair* sliding, int32_t sliding_count,
  const OcctSharp_LimitedFeatureOptions* options, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_rib_slot(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count, const OcctSharp_SlidingPair* sliding, int32_t sliding_count,
  const OcctSharp_RibSlotOptions* options, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_local_hole(
  const OcctSharp_ShapeHandle* source, const OcctSharp_LocalHoleOptions* options, OcctSharp_FeatureResultHandle** output);
/* All-zero null buffers query counts. Otherwise every capacity is checked before
   any array is written, including arrays whose corresponding count is zero. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_snapshot(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_LocalFeatureInfo* info,
  OcctSharp_ContourInfo* contours, int32_t contour_capacity, OcctSharp_ContourEdge* edges, int32_t edge_capacity,
  OcctSharp_FilletSection* sections, int32_t section_capacity, OcctSharp_LocalFeatureFault* faults, int32_t fault_capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_history(
  const OcctSharp_FeatureResultHandle* result, int32_t index, OcctSharp_LocalFeatureHistory* info, OcctSharp_ShapeHandle** shape);

#ifdef __cplusplus
}
#endif
