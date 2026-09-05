#include "Modeling/Repair.hxx"
#include <BRepCheck_Analyzer.hxx>
#include <BRepLib.hxx>
#include <ShapeFix_Wire.hxx>
#include <ShapeFix_Wireframe.hxx>
#include <ShapeFix_Face.hxx>
#include <ShapeFix_Shell.hxx>
#include <ShapeFix_Solid.hxx>
#include <ShapeFix_FixSmallFace.hxx>
#include <ShapeFix_FixSmallSolid.hxx>
#include <ShapeFix_ShapeTolerance.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <cmath>

namespace OcctSharp::Native::Repair {
namespace {
template<class T> void Configure(T& fixer, const OcctSharp_RepairStage& options,
  const occ::handle<ShapeBuild_ReShape>& context) {
  fixer.SetContext(context); fixer.SetPrecision(options.tolerance);
  fixer.SetMinTolerance(options.tolerance); fixer.SetMaxTolerance(options.maximum_tolerance);
}
TopoDS_Face Support(const TopoDS_Shape& shape, const TopoDS_Shape& wire) {
  for (TopExp_Explorer face(shape, TopAbs_FACE); face.More(); face.Next()) {
    auto closure = Map(face.Current()); if (closure.Contains(wire)) return TopoDS::Face(face.Current());
  }
  return {};
}
}
Outcome Fix(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options) {
  Outcome result; result.Shape = shape; result.Context = new ShapeBuild_ReShape();
  auto sourceMap = Map(shape);
  if (options.operation == 2 || options.operation == 3) {
    ShapeFix_Wireframe fixer(shape); Configure(fixer, options, result.Context);
    if (options.operation == 2) {
      fixer.FixWireGaps();
      Require(!fixer.StatusWireGaps(ShapeExtend_FAIL), "Wireframe gap correction failed; no partial result is accepted.");
      OcctSharp_RepairMetrics metrics{};
      OcctSharp_RepairInspectionOptions inspection{options.tolerance, options.tolerance,
        options.tolerance * options.tolerance, options.maximum_tolerance};
      for (const auto& finding : Inspect(fixer.Shape(), inspection, metrics))
        if (finding.kind == 12 || finding.kind == 13) result.Findings.push_back(finding);
    } else {
      Positive(options.threshold, "Small-edge threshold must be positive.");
      fixer.SetPrecision(options.threshold); fixer.ModeDropSmallEdges() = options.mode1 == 1;
      fixer.SetLimitAngle(options.angle); fixer.FixSmallEdges();
      Require(!fixer.StatusSmallEdges(ShapeExtend_FAIL), "Small-edge repair failed.");
    }
    result.Shape = fixer.Shape(); return result;
  }
  if (options.operation == 7) {
    ShapeFix_FixSmallFace fixer; fixer.Init(shape); Configure(fixer, options, result.Context);
    Positive(options.threshold, "Small-face width threshold must be positive.");
    fixer.SetPrecision(options.threshold); fixer.Perform(); result.Shape = fixer.Shape(); return result;
  }
  if (options.operation == 8) {
    ShapeFix_FixSmallSolid fixer; Positive(options.threshold, "Small-solid volume must be positive.");
    fixer.SetFixMode(2); fixer.SetVolumeThreshold(options.threshold);
    result.Shape = fixer.Remove(shape, result.Context); return result;
  }
  if (options.operation == 9) {
    Require(options.mode1 == TopAbs_VERTEX || options.mode1 == TopAbs_EDGE || options.mode1 == TopAbs_FACE,
      "Tolerance normalization requires an explicit vertex, edge or face kind.");
    // Recompute admissible curve/vertex tolerances on the private copy, then clamp
    // and perform exact geometric BRepCheck. Never use clamping alone as proof.
    BRepLib::UpdateTolerances(shape, true);
    ShapeFix_ShapeTolerance fixer;
    fixer.LimitTolerance(shape, options.tolerance, options.maximum_tolerance, static_cast<TopAbs_ShapeEnum>(options.mode1));
    BRepCheck_Analyzer validation(shape, true, false, true);
    Require(validation.IsValid(), "Requested tolerance interval cannot be geometrically verified.");
    return result;
  }
  TopAbs_ShapeEnum kind = options.operation < 2 ? TopAbs_WIRE : options.operation == 4 ? TopAbs_FACE
    : options.operation == 5 ? TopAbs_SHELL : TopAbs_SOLID;
  bool applicable = false;
  for (const auto& item : sourceMap) {
    if (item.ShapeType() != kind) continue;
    applicable = true; auto current = result.Context->Apply(item); TopoDS_Shape fixed;
    if (options.operation < 2) {
      ShapeFix_Wire fixer;
      fixer.Load(TopoDS::Wire(current)); auto face = Support(shape, item);
      if (!face.IsNull()) fixer.SetFace(face);
      Configure(fixer, options, result.Context);
      fixer.ClosedWireMode() = options.mode1 == 1;
      fixer.ModifyTopologyMode() = true; fixer.ModifyGeometryMode() = true;
      if (options.operation == 0) {
        fixer.FixReorder(); Require(!fixer.StatusReorder(ShapeExtend_FAIL), "Wire ordering failed.");
        if (options.mode2 == 1) fixer.FixConnected(options.maximum_tolerance);
      } else {
        fixer.FixConnected(options.maximum_tolerance);
        Require(!fixer.StatusConnected(ShapeExtend_FAIL), "Adjacent endpoint repair failed.");
        if (options.mode1 == 1) fixer.FixClosed(options.maximum_tolerance);
      }
      fixed = fixer.Wire();
    } else if (options.operation == 4) {
      ShapeFix_Face fixer(TopoDS::Face(current)); Configure(fixer, options, result.Context);
      // Hole deletion is a separate opt-in stage, never an implicit face fix.
      fixer.FixSmallAreaWireMode() = 0; fixer.RemoveSmallAreaFaceMode() = 0;
      fixer.FixOrientationMode() = options.mode1; fixer.FixAddNaturalBoundMode() = options.mode2;
      fixer.FixWireMode() = options.mode3; fixer.Perform();
      Require(!fixer.Status(ShapeExtend_FAIL), "Face normalization failed."); fixed = fixer.Result();
    } else if (options.operation == 5) {
      ShapeFix_Shell fixer(TopoDS::Shell(current)); Configure(fixer, options, result.Context);
      fixer.FixFaceMode() = 0;
      fixer.FixFaceOrientation(TopoDS::Shell(current), true, options.mode1 == 1);
      Require(!fixer.Status(ShapeExtend_FAIL), "Shell orientation cannot be normalized."); fixed = fixer.Shape();
    } else {
      ShapeFix_Solid fixer(TopoDS::Solid(current)); Configure(fixer, options, result.Context);
      fixer.FixShellMode() = options.mode1; fixer.FixShellOrientationMode() = 1;
      fixer.CreateOpenSolidMode() = false; fixer.Perform();
      Require(!fixer.Status(ShapeExtend_FAIL), "Solid shell normalization failed."); fixed = fixer.Solid();
    }
    Require(!fixed.IsNull(), "Repair produced null topology.");
    if (!fixed.IsEqual(item)) result.Context->Replace(item, fixed);
  }
  if (!applicable) result.Findings.push_back({17, -1, -1, 0, double(kind), 0});
  result.Shape = result.Context->Apply(shape); return result;
}
}
