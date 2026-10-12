#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif
// Additive copied classification. Existing intersection endpoint enumeration is unchanged.
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_intersection_counts(
  const OcctSharp_SketchCurve* first, const OcctSharp_SketchCurve* second,
  double tolerance, int32_t* point_count, int32_t* overlap_interval_count);
#ifdef __cplusplus
}
#endif
