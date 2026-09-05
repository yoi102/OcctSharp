#pragma once
#include "OcctSharp.Native.Surface.h"
#include "OcctSharp.Native.Internal.hxx"
#include <BRep_Tool.hxx>
#include <BRepTools.hxx>
#include <BRepClass_FaceClassifier.hxx>
#include <Geom_Surface.hxx>
#include <Geom2d_Curve.hxx>
#include <Standard_Failure.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Pln.hxx>
#include <algorithm>
#include <cmath>
#include <limits>
#include <memory>
#include <stdexcept>
#include <vector>

// Implemented in the original bridge: one curve builder and one ownership registry.
OcctSharp_Status OcctSharp_Internal_BuildSketchCurve(
  const OcctSharp_SketchCurve&, opencascade::handle<Geom2d_Curve>&);

namespace OcctSharp::SurfaceBridge {
class Failure final : public std::runtime_error {
public:
  Failure(OcctSharp_Status status, const char* message) : std::runtime_error(message), Status(status) {}
  OcctSharp_Status Status;
};
inline void Require(bool condition, const char* message) {
  if (!condition) throw Failure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
inline void Check(OcctSharp_Status status) {
  if (status != OCCTSHARP_STATUS_SUCCESS) throw Failure(status, occtsharp_get_last_error());
}
template<class Action> OcctSharp_Status Invoke(Action&& action) {
  OcctSharp_Internal_SetLastError("");
  try { action(); return OCCTSHARP_STATUS_SUCCESS; }
  catch (const Failure& error) { OcctSharp_Internal_SetLastError(error.what()); return error.Status; }
  catch (const Standard_Failure& error) { OcctSharp_Internal_SetLastError(error.GetMessageString()); return OCCTSHARP_STATUS_OCCT_FAILURE; }
  catch (const std::exception& error) { OcctSharp_Internal_SetLastError(error.what()); return OCCTSHARP_STATUS_STANDARD_EXCEPTION; }
  catch (...) { OcctSharp_Internal_SetLastError("Unknown surface operation failure."); return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION; }
}
inline void Tolerance(double value) { Require(std::isfinite(value) && value > 0, "Tolerance must be finite and positive."); }
inline TopoDS_Shape Shape(const OcctSharp_ShapeHandle* handle) {
  const TopoDS_Shape* shape = nullptr;
  Check(OcctSharp_Internal_TryGetShape(handle, &shape));
  Require(!shape->IsNull(), "The topology is null.");
  return *shape;
}
inline TopoDS_Shape TypedShape(const OcctSharp_ShapeHandle* handle, TopAbs_ShapeEnum kind) {
  auto shape = Shape(handle);
  if (shape.ShapeType() != kind) throw Failure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The topology has the wrong kind for this surface operation.");
  return shape;
}
struct FaceData {
  TopoDS_Face face;
  TopLoc_Location location;
  opencascade::handle<Geom_Surface> surface;
  double u0, u1, v0, v1;
  explicit FaceData(const OcctSharp_ShapeHandle* handle) : face(TopoDS::Face(TypedShape(handle, TopAbs_FACE))) {
    surface = BRep_Tool::Surface(face, location);
    Require(!surface.IsNull(), "The face has no surface.");
    BRepTools::UVBounds(face, u0, u1, v0, v1);
    Require(std::isfinite(u0) && std::isfinite(u1) && std::isfinite(v0) && std::isfinite(v1)
      && u0 < u1 && v0 < v1, "A surface operation requires finite increasing UV bounds.");
  }
  int State(double u, double v, double tolerance) const {
    BRepClass_FaceClassifier classifier(face, gp_Pnt2d(u, v), tolerance);
    return static_cast<int>(classifier.State());
  }
  gp_Pnt WorldPoint(double u, double v) const {
    return surface->Value(u, v).Transformed(location.Transformation());
  }
};
inline gp_Pnt Point(OcctSharp_Xyz p) {
  Require(std::isfinite(p.x) && std::isfinite(p.y) && std::isfinite(p.z), "A point must be finite.");
  return {p.x, p.y, p.z};
}
inline gp_Pnt2d Point2d(OcctSharp_SketchPoint2d p) {
  Require(std::isfinite(p.x) && std::isfinite(p.y), "A UV point must be finite.");
  return {p.x, p.y};
}
template<class Xyz> OcctSharp_Xyz Copy(const Xyz& p) { return {p.X(), p.Y(), p.Z()}; }
inline OcctSharp_SketchPoint2d Copy2d(const gp_Pnt2d& p) { return {p.X(), p.Y()}; }
inline void Result(const TopoDS_Shape& shape, OcctSharp_ShapeHandle** output) {
  Require(output != nullptr, "The shape output is null.");
  if (shape.IsNull()) throw Failure(OCCTSHARP_STATUS_OCCT_FAILURE, "The surface operation returned null topology.");
  *output = OcctSharp_Internal_AllocateShape(shape);
}
template<class Value> void CopyValues(const std::vector<Value>& values, Value* output, int capacity, int32_t* count) {
  Require(count != nullptr && capacity >= 0 && values.size() <= INT32_MAX, "Invalid result count or capacity.");
  *count = static_cast<int32_t>(values.size());
  if (output == nullptr && capacity == 0) return;
  Require(output != nullptr && capacity >= *count, "The result buffer is too small.");
  std::copy(values.begin(), values.end(), output);
}
using ShapeOwner = std::unique_ptr<OcctSharp_ShapeHandle, decltype(&occtsharp_shape_release)>;
inline auto SketchCurve(const OcctSharp_SketchCurve& input) {
  opencascade::handle<Geom2d_Curve> curve;
  Check(OcctSharp_Internal_BuildSketchCurve(input, curve));
  return curve;
}
}
