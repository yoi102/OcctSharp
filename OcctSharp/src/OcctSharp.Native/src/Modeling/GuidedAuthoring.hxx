#pragma once
#include "OcctSharp.Native.Authoring.h"
#include "Modeling/Features.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRep_Tool.hxx>
#include <BRep_Builder.hxx>
#include <TopExp.hxx>
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <cmath>
#include <string>
#include <vector>

namespace OcctSharp::Native::Authoring {
using ShapeMap = NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher>;
inline void Require(bool condition, const char* message) {
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
inline void Positive(double value) { Require(std::isfinite(value) && value > 0, "Authoring tolerances must be finite and positive."); }
inline void Flag(int flag) { Require(flag == 0 || flag == 1, "A Boolean flag is invalid."); }
inline gp_Pnt Point(OcctSharp_Xyz value) {
  Require(std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z), "Authoring coordinates must be finite.");
  return gp_Pnt(value.x, value.y, value.z);
}
inline gp_Dir Direction(OcctSharp_Xyz value) { return gp_Dir(Point(value).XYZ()); }
inline ShapeMap Map(const TopoDS_Shape& shape) {
  ShapeMap result; TopExp::MapShapes(shape, result); return result;
}
// A single copy operation preserves shared TShapes and face/pcurve dependencies.
struct InputGraph {
  std::vector<TopoDS_Shape> Shapes;
  explicit InputGraph(const OcctSharp_ShapeHandle* const* inputs, int count) {
    Require(inputs != nullptr && count >= 1 && count <= 512, "Authoring requires one to 512 input shapes.");
    TopoDS_Compound compound; BRep_Builder builder; builder.MakeCompound(compound);
    for (int i = 0; i < count; ++i) {
      ValidateUsableShape(inputs[i]);
      const auto& source = inputs[i]->Value;
      const bool wasFree = source.Free();
      // TopoDS_Builder::Add freezes the component TShape, even in a temporary
      // compound. Restore that flag on both paths so capturing never edits inputs.
      try { builder.Add(compound, source); }
      catch (...) { source.TShape()->Free(wasFree); throw; }
      source.TShape()->Free(wasFree);
    }
    BRepBuilderAPI_Copy copy(compound, true, false);
    Require(copy.IsDone(), "The authoring input graph could not be copied.");
    for (int i = 0; i < count; ++i) {
      auto shape = copy.ModifiedShape(inputs[i]->Value);
      Require(!shape.IsNull(), "An input has no exact copy correspondence.");
      shape.Orientation(inputs[i]->Value.Orientation()); Shapes.push_back(shape);
    }
  }
  const TopoDS_Shape& At(int index) const {
    Require(index >= 0 && index < static_cast<int>(Shapes.size()), "An input shape index is out of range."); return Shapes[index];
  }
  const TopoDS_Shape& Typed(int index, TopAbs_ShapeEnum kind) const {
    const auto& shape = At(index); Require(shape.ShapeType() == kind, "An authoring shape has an incorrect topology kind."); return shape;
  }
};
inline void Add(OcctSharp_FeatureResultHandle& output, const TopoDS_Shape& shape, int kind,
  int source = -1, int subshape = -1, int sourceKind = -1) {
  Require(output.History.size() < 1000000, "Authoring history exceeds the bounded result limit.");
  output.History.push_back({source, kind, shape, subshape, sourceKind});
}
template<class Algorithm> void History(Algorithm& algorithm, const InputGraph& graph,
  OcctSharp_FeatureResultHandle& output) {
  for (int source = 0; source < static_cast<int>(graph.Shapes.size()); ++source) {
    const auto topology = Map(graph.Shapes[source]);
    for (int i = 1; i <= topology.Extent(); ++i) {
      const auto& shape = topology(i); bool mapped = false;
      for (const auto& generated : algorithm.Generated(shape)) {
        Add(output, generated, 1, source, i - 1, shape.ShapeType()); mapped = true;
      }
      for (const auto& modified : algorithm.Modified(shape)) {
        Add(output, modified, 0, source, i - 1, shape.ShapeType()); mapped = true;
      }
      if (!mapped) Add(output, {}, 5, source, i - 1, shape.ShapeType());
    }
  }
}
inline void Finish(OcctSharp_FeatureResultHandle& output, OcctSharp_AuthoringInfo& info) {
  info.history_count = static_cast<int>(output.History.size());
  info.valid = !output.Result.IsNull() && BRepCheck_Analyzer(output.Result).IsValid();
  info.solid = !output.Result.IsNull() && output.Result.ShapeType() == TopAbs_SOLID;
  output.Info.succeeded = info.done; output.Info.result_is_valid = info.valid;
  output.Info.generated_count = info.history_count;
}
}
