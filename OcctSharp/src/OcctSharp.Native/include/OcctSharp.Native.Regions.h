#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

/* All indices are zero-based in this result only. No algorithm or borrowed
   iterator escapes; the temporary snapshot uses the existing feature owner. */
typedef struct OcctSharp_RegionInfo {
  int32_t done, valid, item_count, cell_count, output_count, warnings, reserved1, reserved2;
} OcctSharp_RegionInfo;
typedef struct OcctSharp_RegionItem {
  int32_t kind, a, b, c, d, flags;
  double measure;
} OcctSharp_RegionItem;
typedef struct OcctSharp_PartitionOptions {
  double fuzzy;
  int32_t parallel, check_inputs, max_cells, reserved;
} OcctSharp_PartitionOptions;
typedef struct OcctSharp_RegionRule {
  int32_t output, action, material, expression_offset, expression_count, dimension;
  double maximum_measure;
} OcctSharp_RegionRule;
typedef struct OcctSharp_RegionOutput {
  int32_t remove_boundaries, containers, reserved1, reserved2;
} OcctSharp_RegionOutput;
typedef struct OcctSharp_VolumeOptions {
  double fuzzy;
  int32_t intersect, avoid_internal, parallel, max_solids;
} OcctSharp_VolumeOptions;

/* Postfix expression: >=0 input index; -1 true, -2 false, -3 union,
   -4 intersection, -5 difference. No callbacks or arbitrary code. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_partition_build(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count,
  const OcctSharp_PartitionOptions* options, const OcctSharp_RegionRule* rules, int32_t rule_count,
  const int32_t* expressions, int32_t expression_count,
  const OcctSharp_RegionOutput* outputs, int32_t output_count, OcctSharp_FeatureResultHandle** result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_volume_build(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count,
  const OcctSharp_VolumeOptions* options, OcctSharp_FeatureResultHandle** result);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_region_snapshot(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_RegionInfo* info,
  OcctSharp_RegionItem* items, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_region_item_shape(
  const OcctSharp_FeatureResultHandle* result, int32_t index, OcctSharp_ShapeHandle** shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_region_classify_solid(
  const OcctSharp_ShapeHandle* solid, OcctSharp_Xyz point, double tolerance, int32_t* state);

#ifdef __cplusplus
}
#endif
