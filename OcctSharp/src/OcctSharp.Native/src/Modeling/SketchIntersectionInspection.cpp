#include "OcctSharp.SketchInspection.h"
#include "Modeling/Sketch.hxx"
#include "Runtime/Error.hxx"
#include <Geom2dAPI_InterCurveCurve.hxx>
#include <cmath>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_intersection_counts(
  const OcctSharp_SketchCurve* first, const OcctSharp_SketchCurve* second,
  const double tolerance, int32_t* point_count, int32_t* overlap_interval_count)
{
  if (point_count == nullptr || overlap_interval_count == nullptr)
  { SetLastError("A sketch intersection count output is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *point_count = 0;
  *overlap_interval_count = 0;
  if (first == nullptr || second == nullptr || !std::isfinite(tolerance) || tolerance <= 0)
  { SetLastError("Sketch intersection inputs and tolerance must be valid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const auto firstCurve = BuildSketchCurve(*first);
    const auto secondCurve = BuildSketchCurve(*second);
    Geom2dAPI_InterCurveCurve intersection(firstCurve, secondCurve, tolerance);
    if (!intersection.Intersector().IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Exact sketch intersection classification failed.");
    *point_count = intersection.NbPoints();
    *overlap_interval_count = intersection.NbSegments();
  });
}
