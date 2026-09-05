#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct OcctSharp_RepairResultHandle OcctSharp_RepairResultHandle;

/* All indices are zero-based in a snapshot-wide TopExp map, never addresses. */
typedef struct OcctSharp_RepairTopology {
  int32_t index, kind, orientation, parent_index;
  double tolerance;
} OcctSharp_RepairTopology;

typedef struct OcctSharp_RepairFinding {
  int32_t kind, source_index, related_index, status;
  double value, limit;
} OcctSharp_RepairFinding;

typedef struct OcctSharp_RepairMetrics {
  int32_t valid, topology_count, area_available, volume_available;
  double maximum_tolerance, area, volume, maximum_gap;
} OcctSharp_RepairMetrics;

typedef struct OcctSharp_RepairInspectionOptions {
  double tolerance, small_length, small_area, tolerance_outlier;
} OcctSharp_RepairInspectionOptions;

typedef struct OcctSharp_RepairStage {
  int32_t operation, mode1, mode2, mode3, parts, maximum_topology;
  double tolerance, maximum_tolerance, threshold, angle;
} OcctSharp_RepairStage;

typedef struct OcctSharp_RepairRelation {
  int32_t source_index, result_index, kind, reserved;
} OcctSharp_RepairRelation;

typedef struct OcctSharp_RepairBoundary {
  int32_t closed, area_available, edge_count, reserved;
  double length, area, endpoint_gap;
} OcctSharp_RepairBoundary;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_copy(
  const OcctSharp_ShapeHandle* source, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_serialized(
  const OcctSharp_ShapeHandle* source, uint8_t* output, int32_t capacity, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_topology(
  const OcctSharp_ShapeHandle* source, OcctSharp_RepairTopology* output, int32_t capacity, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_subshape(
  const OcctSharp_ShapeHandle* source, int32_t index, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_inspect(
  const OcctSharp_ShapeHandle* source, const OcctSharp_RepairInspectionOptions* options,
  OcctSharp_RepairMetrics* metrics, OcctSharp_RepairFinding* findings, int32_t capacity, int32_t* count);
/* Index -1 queries the count without creating a wire; each other call creates one owner. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_boundary(
  const OcctSharp_ShapeHandle* source, double tolerance, int32_t index,
  OcctSharp_RepairBoundary* info, int32_t* source_edges, int32_t capacity,
  int32_t* edge_count, int32_t* boundary_count, OcctSharp_ShapeHandle** wire);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_execute(
  const OcctSharp_ShapeHandle* source, const OcctSharp_RepairStage* stage,
  const int32_t* selected, int32_t selected_count, const int32_t* protected_indices, int32_t protected_count,
  const OcctSharp_ShapeHandle* const* replacements, int32_t replacement_count,
  OcctSharp_RepairResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_shape(
  const OcctSharp_RepairResultHandle* result, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_history(
  const OcctSharp_RepairResultHandle* result, OcctSharp_RepairRelation* output, int32_t capacity, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_findings(
  const OcctSharp_RepairResultHandle* result, OcctSharp_RepairFinding* output, int32_t capacity, int32_t* count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_release(OcctSharp_RepairResultHandle* result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_xde_apply(
  OcctSharp_OcafDocumentHandle* document, const char* definition_entry, const OcctSharp_ShapeHandle* candidate,
  const OcctSharp_RepairRelation* history, int32_t history_count, int32_t apply,
  int32_t* conflicts, int32_t capacity, int32_t* conflict_count, int32_t* mapped_count, int32_t* occurrence_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_viewer_select(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_xde_subshape_label(
  OcctSharp_OcafDocumentHandle* document, const char* definition_entry, int32_t index,
  char* entry, int32_t capacity, int32_t* written);

#ifdef __cplusplus
}
#endif
