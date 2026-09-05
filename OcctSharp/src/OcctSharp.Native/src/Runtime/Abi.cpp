// Native Runtime/Abi implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <Standard_Version.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <cstddef>
#include <type_traits>

namespace OcctSharp::Native
{
static_assert(sizeof(Standard_Integer) == sizeof(int32_t));

static_assert(sizeof(Standard_Real) == sizeof(double));

static_assert(sizeof(Standard_Boolean) == sizeof(bool));

static_assert(sizeof(std::underlying_type_t<TopAbs_ShapeEnum>) == sizeof(int32_t));

static_assert(sizeof(OcctSharp_StepAssemblyInput) == 64);

static_assert(alignof(OcctSharp_StepAssemblyInput) == 8);

static_assert(sizeof(OcctSharp_Xyz) == 24);

static_assert(alignof(OcctSharp_Xyz) == 8);

static_assert(sizeof(OcctSharp_DrawingProjection) == 88);

static_assert(sizeof(OcctSharp_DrawingPolyline) == 12);

static_assert(sizeof(OcctSharp_ViewerCamera) == 96);

static_assert(sizeof(OcctSharp_ViewerPickRay) == 48);

static_assert(sizeof(OcctSharp_Line) == 48);

static_assert(sizeof(OcctSharp_Circle) == 56);

static_assert(sizeof(OcctSharp_Ax2) == 96);

static_assert(sizeof(OcctSharp_ViewerManipulatorState) == 144);

static_assert(sizeof(OcctSharp_Ax3) == 96);

static_assert(sizeof(OcctSharp_Plane) == 48);

static_assert(sizeof(OcctSharp_EdgeCurveSnapshot) == 72);

static_assert(alignof(OcctSharp_EdgeCurveSnapshot) == 8);

static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, curve_type) == 0);

static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, first_parameter) == 8);

static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, start_point) == 24);

static_assert(sizeof(OcctSharp_FaceSurfaceSnapshot) == 40);

static_assert(alignof(OcctSharp_FaceSurfaceSnapshot) == 8);

static_assert(offsetof(OcctSharp_FaceSurfaceSnapshot, surface_type) == 0);

static_assert(offsetof(OcctSharp_FaceSurfaceSnapshot, first_u_parameter) == 8);

static_assert(sizeof(OcctSharp_CurveEvaluation) == 56);

static_assert(sizeof(OcctSharp_CurveDerivativeEvaluation) == 80);

static_assert(sizeof(OcctSharp_Xy) == 16);

static_assert(sizeof(OcctSharp_PcurveSnapshot) == 48);

static_assert(sizeof(OcctSharp_PcurveEvaluation) == 40);

static_assert(sizeof(OcctSharp_CurveProjection) == 48);

static_assert(sizeof(OcctSharp_SurfaceEvaluation) == 64);

static_assert(sizeof(OcctSharp_SurfaceDerivativeEvaluation) == 112);

static_assert(sizeof(OcctSharp_SurfaceProjection) == 56);

static_assert(sizeof(OcctSharp_BooleanHistorySummary) == 48);

static_assert(offsetof(OcctSharp_CurveEvaluation, point) == 8);

static_assert(offsetof(OcctSharp_CurveProjection, solution_count) == 40);

static_assert(offsetof(OcctSharp_SurfaceEvaluation, point) == 16);

static_assert(offsetof(OcctSharp_SurfaceProjection, solution_count) == 48);

static_assert(sizeof(OcctSharp_ShapeDistanceResult) == 64);

static_assert(sizeof(OcctSharp_BoundingBox) == 48);

static_assert(alignof(OcctSharp_BoundingBox) == 8);

static_assert(offsetof(OcctSharp_BoundingBox, min_x) == 0);

static_assert(offsetof(OcctSharp_BoundingBox, max_x) == 24);

static_assert(sizeof(OcctSharp_XdeColor) == 32);

static_assert(sizeof(OcctSharp_XdePresentationStyle) == 112);

static_assert(alignof(OcctSharp_XdePresentationStyle) == 8);

static_assert(offsetof(OcctSharp_XdePresentationStyle, surface_color) == 16);

static_assert(offsetof(OcctSharp_XdePresentationStyle, material_color) == 80);

static_assert(sizeof(OcctSharp_XdeValidationProperties) == 56);

static_assert(alignof(OcctSharp_XdeValidationProperties) == 8);

static_assert(offsetof(OcctSharp_XdeValidationProperties, area) == 0);

static_assert(offsetof(OcctSharp_XdeValidationProperties, centroid) == 16);

static_assert(offsetof(OcctSharp_XdeValidationProperties, has_area) == 40);

static_assert(sizeof(OcctSharp_TopologyCounts) == 32);

static_assert(sizeof(OcctSharp_ShapeTopologySummary) == 120);

static_assert(alignof(OcctSharp_ShapeTopologySummary) == 8);

static_assert(offsetof(OcctSharp_ShapeTopologySummary, unique_counts) == 0);

static_assert(offsetof(OcctSharp_ShapeTopologySummary, occurrence_counts) == 32);

static_assert(offsetof(OcctSharp_ShapeTopologySummary, is_closed) == 64);

static_assert(offsetof(OcctSharp_ShapeTopologySummary, min_vertex_tolerance) == 72);

static_assert(sizeof(OcctSharp_DetailedMeshVertex) == 72);

static_assert(alignof(OcctSharp_DetailedMeshVertex) == 8);

static_assert(offsetof(OcctSharp_DetailedMeshVertex, has_uv) == 64);

static_assert(sizeof(OcctSharp_DetailedMeshTriangle) == 20);

static_assert(sizeof(OcctSharp_ValidationIssue) == 8);

static_assert(sizeof(OcctSharp_StepReadReport) == 24);

static_assert(offsetof(OcctSharp_StepReadReport, system_length_unit) == 16);

static_assert(sizeof(OcctSharp_IgesReadReport) == 32);

static_assert(offsetof(OcctSharp_IgesReadReport, source_length_unit_meters) == 16);

static_assert(sizeof(OcctSharp_SketchPoint2d) == 16);

static_assert(sizeof(OcctSharp_SketchPlane) == 72);

static_assert(sizeof(OcctSharp_SketchCurve) == 104);

static_assert(offsetof(OcctSharp_SketchCurve, first_parameter) == 32);

static_assert(offsetof(OcctSharp_SketchCurve, poles) == 72);

static_assert(sizeof(OcctSharp_SketchEvaluation) == 40);

static_assert(sizeof(OcctSharp_SketchProjection) == 32);

static_assert(sizeof(OcctSharp_SketchIntersection) == 32);

static_assert(sizeof(OcctSharp_StepReaderInfo) == 32);

static_assert(offsetof(OcctSharp_StepReaderInfo, system_length_unit) == 8);

static_assert(alignof(OcctSharp_ShapeDistanceResult) == 8);

static_assert(offsetof(OcctSharp_ShapeDistanceResult, distance) == 0);

static_assert(offsetof(OcctSharp_ShapeDistanceResult, point_on_first) == 8);

static_assert(offsetof(OcctSharp_ShapeDistanceResult, point_on_second) == 32);

static_assert(offsetof(OcctSharp_ShapeDistanceResult, solution_count) == 56);

static_assert(offsetof(OcctSharp_StepAssemblyInput, file_path) == 0);

static_assert(offsetof(OcctSharp_StepAssemblyInput, translation_x) == 8);

static_assert(offsetof(OcctSharp_StepAssemblyInput, rotation_angle_radians) == 56);

constexpr uint32_t AbiVersion = 0x0001003BU;

constexpr const char* BridgeVersion = "0.67.0";
}

using namespace OcctSharp::Native;

uint32_t OCCTSHARP_CALL occtsharp_get_abi_version(void)
{
  return AbiVersion;
}

const char* OCCTSHARP_CALL occtsharp_get_bridge_version(void)
{
  return BridgeVersion;
}

const char* OCCTSHARP_CALL occtsharp_get_occt_version(void)
{
  return OCC_VERSION_COMPLETE;
}
