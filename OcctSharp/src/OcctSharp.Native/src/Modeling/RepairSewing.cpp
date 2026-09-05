#include "Modeling/Repair.hxx"
#include <BRepBuilderAPI_Sewing.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>

namespace OcctSharp::Native::Repair {
Outcome Sew(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options) {
  BRepBuilderAPI_Sewing sewing(options.tolerance, true, true, true, options.mode1 == 1);
  sewing.SetMinTolerance(options.tolerance); sewing.SetMaxTolerance(options.maximum_tolerance);
  sewing.SetLocalTolerancesMode(options.mode2 == 1); sewing.SetSameParameterMode(true);
  bool hasFaces = false;
  for (TopExp_Explorer face(shape, TopAbs_FACE); face.More(); face.Next()) { sewing.Add(face.Current()); hasFaces = true; }
  Require(hasFaces, "Sewing requires faces or shells."); sewing.Perform();
  Outcome result; result.Shape = sewing.SewedShape();
  Require(!result.Shape.IsNull(), "Sewing produced no topology.");
  result.Context = new ShapeBuild_ReShape();
  auto before = Map(shape), after = Map(result.Shape);
  for (const auto& item : before) {
    if (sewing.IsModifiedSubShape(item)) {
      auto changed = sewing.ModifiedSubShape(item);
      if (!changed.IsNull() && !changed.IsEqual(item)) result.Context->Replace(item, changed);
    } else if (sewing.IsModified(item)) {
      auto changed = sewing.Modified(item);
      if (!changed.IsNull() && !changed.IsEqual(item)) result.Context->Replace(item, changed);
    }
  }
  for (int i = 1; i <= sewing.NbDeletedFaces(); ++i) result.Context->Remove(sewing.DeletedFace(i));
  for (int i = 1; i <= sewing.NbFreeEdges(); ++i)
    result.Findings.push_back({14, after.FindIndex(sewing.FreeEdge(i)) - 1, -1, 1, 0, 0});
  for (int i = 1; i <= sewing.NbMultipleEdges(); ++i)
    result.Findings.push_back({15, after.FindIndex(sewing.MultipleEdge(i)) - 1, -1, 1, 0, 0});
  for (int i = 1; i <= sewing.NbContigousEdges(); ++i)
    result.Findings.push_back({16, after.FindIndex(sewing.ContigousEdge(i)) - 1, -1, 1, 0, 0});
  return result;
}
}
