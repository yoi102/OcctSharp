#include "Modeling/Repair.hxx"
#include <ShapeUpgrade_RemoveInternalWires.hxx>
#include <ShapeUpgrade_RemoveLocations.hxx>
#include <ShapeUpgrade_ShapeDivideContinuity.hxx>
#include <ShapeUpgrade_ShapeDivideAngle.hxx>
#include <ShapeUpgrade_ShapeDivideArea.hxx>
#include <ShapeUpgrade_ShapeDivideClosed.hxx>
#include <ShapeUpgrade_ShapeDivideClosedEdges.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <NCollection_Sequence.hxx>
#include <BRepLib.hxx>
#include <BRepTools.hxx>
#include <ShapeFix_Wire.hxx>
#include <TopoDS.hxx>
#include <TopExp_Explorer.hxx>
#include <cmath>

namespace OcctSharp::Native::Repair {
namespace {
template<class T> Outcome Divide(T& divider, const OcctSharp_RepairStage& options) {
  divider.SetPrecision(options.tolerance); divider.SetMinTolerance(options.tolerance);
  divider.SetMaxTolerance(options.maximum_tolerance); divider.Perform();
  Require(!divider.Status(ShapeExtend_FAIL), "Topology division failed; original retained.");
  Outcome result; result.Shape = divider.Result(); result.Context = divider.GetContext();
  if (!result.Shape.IsNull()) BRepLib::SameParameter(result.Shape, options.tolerance, true);
  return result;
}
}
Outcome Normalize(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options,
  const std::vector<TopoDS_Shape>& protectedShapes, const std::vector<TopoDS_Shape>& selectedHoles) {
  Outcome result;
  switch (options.operation) {
    case 11: {
      Positive(options.threshold, "Internal-hole area threshold must be positive.");
      for (const auto& selected : selectedHoles) {
        bool internal = false;
        for (TopExp_Explorer face(shape, TopAbs_FACE); face.More(); face.Next()) {
          if (Map(face.Current()).Contains(selected) && !BRepTools::OuterWire(TopoDS::Face(face.Current())).IsSame(selected)) internal = true;
        }
        Require(internal, "Selective hole removal requires internal wires on a supporting face.");
      }
      occ::handle<ShapeBuild_ReShape> context = new ShapeBuild_ReShape();
      for (TopExp_Explorer wire(shape, TopAbs_WIRE); wire.More(); wire.Next()) {
        if (!selectedHoles.empty() && std::none_of(selectedHoles.begin(), selectedHoles.end(),
          [&](const TopoDS_Shape& selected) { return selected.IsSame(wire.Current()); })) continue;
        ShapeFix_Wire ordering; ordering.Load(TopoDS::Wire(wire.Current())); ordering.SetPrecision(options.tolerance);
        ordering.FixReorder(); context->Replace(wire.Current(), ordering.Wire());
      }
      auto ordered = context->Apply(shape);
      ShapeUpgrade_RemoveInternalWires remover(ordered); remover.SetContext(context); remover.MinArea() = options.threshold;
      remover.RemoveFaceMode() = false;
      NCollection_Sequence<TopoDS_Shape> faces;
      if (selectedHoles.empty()) {
        for (TopExp_Explorer face(ordered, TopAbs_FACE); face.More(); face.Next()) faces.Append(face.Current());
      } else for (const auto& selected : selectedHoles) faces.Append(context->Apply(selected));
      remover.Perform(faces);
      Require(!remover.Status(ShapeExtend_FAIL), "Internal-wire removal failed.");
      result.Shape = remover.GetResult(); result.Context = remover.Context();
      for (const auto& wire : remover.RemovedWires())
        result.Findings.push_back({18, Map(shape).FindIndex(wire) - 1, -1, 1, 0, options.threshold});
      break;
    }
    case 12: {
      Require(options.mode1 >= TopAbs_COMPOUND && options.mode1 <= TopAbs_FACE, "Unsupported location-removal level.");
      ShapeUpgrade_RemoveLocations remover; remover.SetRemoveLevel(static_cast<TopAbs_ShapeEnum>(options.mode1));
      remover.Remove(shape); result.Shape = remover.GetResult(); result.Context = new ShapeBuild_ReShape();
      for (const auto& item : Map(shape)) {
        auto changed = remover.ModifiedShape(item);
        if (!changed.IsNull() && !changed.IsEqual(item)) result.Context->Replace(item, changed);
      }
      break;
    }
    case 13: {
      Require(options.mode1 == GeomAbs_C0 || options.mode1 == GeomAbs_C1 || options.mode1 == GeomAbs_C2
        || options.mode1 == GeomAbs_C3 || options.mode1 == GeomAbs_CN, "Division accepts C0/C1/C2/C3/CN, not G1/G2.");
      ShapeUpgrade_ShapeDivideContinuity divider(shape);
      auto continuity = static_cast<GeomAbs_Shape>(options.mode1);
      divider.SetBoundaryCriterion(continuity); divider.SetPCurveCriterion(continuity);
      divider.SetSurfaceCriterion(continuity); divider.SetTolerance(options.tolerance);
      divider.SetTolerance2d(options.threshold); return Divide(divider, options);
    }
    case 14: {
      Positive(options.angle, "Angular division bound must be positive.");
      Require(options.angle <= 6.283185307179586, "Angular division bound exceeds one revolution.");
      ShapeUpgrade_ShapeDivideAngle divider(options.angle, shape); return Divide(divider, options);
    }
    case 15: {
      Positive(options.threshold, "Maximum face area must be positive.");
      auto metrics = Metrics(shape);
      Require(metrics.area_available && metrics.area / options.threshold <= options.maximum_topology,
        "Area division cannot fit the requested topology resource bound.");
      ShapeUpgrade_ShapeDivideArea divider(shape); divider.MaxArea() = options.threshold; return Divide(divider, options);
    }
    case 16: {
      Require(options.parts >= 2 && options.parts <= 64, "Closed-face parts must be in [2,64].");
      ShapeUpgrade_ShapeDivideClosed divider(shape); divider.SetNbSplitPoints(options.parts - 1); return Divide(divider, options);
    }
    case 17: {
      Require(options.parts >= 2 && options.parts <= 64, "Closed-edge parts must be in [2,64].");
      ShapeUpgrade_ShapeDivideClosedEdges divider(shape); divider.SetNbSplitPoints(options.parts - 1); return Divide(divider, options);
    }
    case 18: {
      ShapeUpgrade_UnifySameDomain unify(shape, options.mode1 != 0, options.mode2 != 0, false);
      unify.SetSafeInputMode(true); unify.AllowInternalEdges(options.mode3 == 1);
      unify.SetLinearTolerance(options.tolerance); unify.SetAngularTolerance(options.angle);
      for (const auto& protectedShape : protectedShapes) unify.KeepShape(protectedShape);
      unify.Build(); result.Shape = unify.Shape(); result.History = unify.History(); break;
    }
    default: Require(false, "Unknown topology normalization operation.");
  }
  return result;
}
}
