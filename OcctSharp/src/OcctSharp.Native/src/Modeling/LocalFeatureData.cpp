#include "Modeling/LocalFeatures.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRep_Builder.hxx>
#include <TopExp.hxx>
#include <TopoDS_Compound.hxx>
#include <algorithm>
#include <set>

namespace OcctSharp::Native::LocalFeatures {
ShapeMap Map(const TopoDS_Shape& shape) { ShapeMap map; if (!shape.IsNull()) TopExp::MapShapes(shape, map); return map; }
InputGraph::InputGraph(const OcctSharp_ShapeHandle* const* inputs, int count) {
  Require(inputs && count > 0 && count <= 512, "Local features require one to 512 inputs.");
  TopoDS_Compound compound; BRep_Builder builder; builder.MakeCompound(compound);
  std::vector<ShapeMap> original;
  for (int i = 0; i < count; ++i) {
    ValidateUsableShape(inputs[i]); const auto& shape = inputs[i]->Value; RequireExactFaceSupport(shape);
    auto map = Map(shape); Require(map.Extent() <= 100000, "Local feature topology exceeds the bounded limit.");
    original.push_back(map); const bool free = shape.Free();
    try { builder.Add(compound, shape); } catch (...) { shape.TShape()->Free(free); throw; }
    shape.TShape()->Free(free);
  }
  BRepBuilderAPI_Copy copy(compound, true, false); Require(copy.IsDone(), "Local input graph copy failed.");
  for (int i = 0; i < count; ++i) {
    auto shape = copy.ModifiedShape(inputs[i]->Value); Require(!shape.IsNull(), "Missing input copy correspondence.");
    shape.Orientation(inputs[i]->Value.Orientation()); Shapes.push_back(shape); ShapeMap mapped;
    for (const auto& item : original[i]) {
      auto value = copy.ModifiedShape(item); Require(!value.IsNull(), "Missing topology copy correspondence.");
      value.Orientation(item.Orientation()); mapped.Add(value);
    }
    Require(mapped.Extent() == original[i].Extent(), "Input topology correspondence is not one-to-one.");
    Topology.push_back(mapped);
  }
}
const TopoDS_Shape& InputGraph::At(int index) const {
  Require(index >= 0 && index < static_cast<int>(Shapes.size()), "Input index is out of range."); return Shapes[index];
}
const TopoDS_Shape& InputGraph::Typed(int index, TopAbs_ShapeEnum kind) const {
  const auto& shape = At(index); Require(shape.ShapeType() == kind, "Wrong input topology kind."); return shape;
}
const TopoDS_Shape& InputGraph::Subshape(int argument, int index, TopAbs_ShapeEnum kind) const {
  At(argument); const auto& map = Topology[argument];
  Require(index >= 0 && index < map.Extent(), "A topology selection is out of range.");
  const auto& shape = map(index + 1); Require(shape.ShapeType() == kind, "Wrong selected topology kind."); return shape;
}
int InputGraph::Index(int argument, const TopoDS_Shape& shape) const {
  At(argument); return shape.IsNull() ? -1 : Topology[argument].FindIndex(shape) - 1;
}
Result::Result(int operation) : Owner(std::make_unique<OcctSharp_FeatureResultHandle>()) {
  Owner->LocalFeature = std::make_shared<OcctSharp_LocalFeatureData>(); Data().Info.operation = operation;
}
void Result::Add(const TopoDS_Shape& shape, int kind, int argument, int topology, int group, int sourceKind) {
  Require(Owner->History.size() < 1000000, "Local feature history exceeds the bounded limit.");
  Owner->History.push_back({argument, kind, shape, topology, sourceKind}); Data().HistoryGroups.push_back(group);
}
void Result::Fail(const char* message, int status) {
  Owner->Result.Nullify(); Data().Info.done = 0; Data().Info.valid = 0;
  Data().Info.algorithm_status = status; Owner->Message = message ? message : "OCCT local-feature failure.";
}
void Result::Publish(OcctSharp_FeatureResultHandle** output) {
  auto& info = Data().Info;
  Data().FinalTopology = Map(Owner->Result);
  info.valid = !Owner->Result.IsNull() && BRepCheck_Analyzer(Owner->Result).IsValid();
  info.contour_count = static_cast<int>(Data().Contours.size()); info.edge_count = static_cast<int>(Data().Edges.size());
  info.section_count = static_cast<int>(Data().Sections.size()); info.fault_count = static_cast<int>(Data().Faults.size());
  info.history_count = static_cast<int>(Owner->History.size());
  Owner->Info.succeeded = info.done; Owner->Info.result_is_valid = info.valid;
  Owner->Info.generated_count = info.history_count; *output = RegisterFeatureResult(std::move(Owner));
}
void ValidateSliding(const InputGraph& graph, const OcctSharp_SlidingPair* sliding, int count) {
  Require(count >= 0 && count <= 256 && (!count || sliding), "Invalid sliding constraint buffer.");
  std::set<std::pair<int, int>> seen;
  for (int i = 0; i < count; ++i) {
    const auto& pair = sliding[i]; const auto& edge = graph.Typed(pair.edge_input, TopAbs_EDGE);
    const auto& face = graph.Typed(pair.face_input, TopAbs_FACE);
    Require(graph.Topology[1].Contains(edge) && graph.Topology[0].Contains(face), "Sliding requires a profile edge and a base face.");
    Require(seen.emplace(graph.Index(1, edge), graph.Index(0, face)).second, "Duplicate sliding constraint.");
  }
}
void ValidateLimits(const InputGraph& graph, const OcctSharp_LimitedFeatureOptions& o) {
  Require(graph.Shapes.size() >= 2 && o.limit_mode >= 0 && o.limit_mode <= 6, "Invalid limit mode or input count.");
  Flag(o.fuse); Flag(o.modify);
  const auto& support = graph.Typed(o.support_input, TopAbs_FACE);
  Require(graph.Topology[0].Contains(support) || graph.Topology[1].Contains(support), "The support face must belong to the base or profile.");
  if (o.limit_mode == 2) graph.At(o.from_input); else Require(o.from_input == -1, "Unexpected From limiter.");
  if (o.limit_mode == 1 || o.limit_mode == 2 || o.limit_mode == 4 || o.limit_mode == 6) graph.At(o.until_input);
  else Require(o.until_input == -1, "Unexpected Until limiter.");
  if (o.limit_mode == 0 || o.limit_mode == 6) Positive(o.extent);
  Require(std::isfinite(o.draft_angle), "Non-finite drafted-prism angle.");
  Direction(o.direction); Point(o.origin);
}
}

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_source_subshape(
  const OcctSharp_ShapeHandle* source, int32_t index, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output != nullptr, "Missing selected subshape output."); ValidateUsableShape(source);
    const auto topology = Map(source->Value);
    Require(index >= 0 && index < topology.Extent(), "Selected source topology is out of range.");
    // Owning TopoDS value sharing the private snapshot, not an independent deep
    // copy: the next complete-graph copy must retain support/sliding identity.
    *output = AllocateShape(topology(index + 1));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_snapshot(const OcctSharp_FeatureResultHandle* result,
  OcctSharp_LocalFeatureInfo* info, OcctSharp_ContourInfo* contours, int32_t cc, OcctSharp_ContourEdge* edges, int32_t ec,
  OcctSharp_FilletSection* sections, int32_t sc, OcctSharp_LocalFeatureFault* faults, int32_t fc) {
  if (info) *info = {};
  return Guard([&] {
    Require(info != nullptr, "The local feature info output is null."); ValidateFeatureResult(result);
    Require(result->LocalFeature != nullptr, "The result is not a local-feature snapshot."); const auto& data = *result->LocalFeature;
    Require(cc >= 0 && ec >= 0 && sc >= 0 && fc >= 0, "A snapshot capacity is negative.");
    const bool query = !contours && !edges && !sections && !faults && cc == 0 && ec == 0 && sc == 0 && fc == 0;
    if (!query) {
      Require(cc >= data.Info.contour_count && (!cc || contours) && ec >= data.Info.edge_count && (!ec || edges)
        && sc >= data.Info.section_count && (!sc || sections) && fc >= data.Info.fault_count && (!fc || faults), "A local feature snapshot buffer is too small or null.");
      if (!data.Contours.empty()) std::copy(data.Contours.begin(), data.Contours.end(), contours);
      if (!data.Edges.empty()) std::copy(data.Edges.begin(), data.Edges.end(), edges);
      if (!data.Sections.empty()) std::copy(data.Sections.begin(), data.Sections.end(), sections);
      if (!data.Faults.empty()) std::copy(data.Faults.begin(), data.Faults.end(), faults);
    }
    *info = data.Info;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_local_feature_history(const OcctSharp_FeatureResultHandle* result,
  int32_t index, OcctSharp_LocalFeatureHistory* info, OcctSharp_ShapeHandle** shape) {
  if (shape) *shape = nullptr; if (info) *info = {};
  return Guard([&] {
    Require(info && shape, "Missing local history output."); ValidateFeatureResult(result);
    Require(result->LocalFeature && index >= 0 && index < static_cast<int>(result->History.size()), "Invalid local history index or result kind.");
    const auto& item = result->History[index]; auto* copy = item.Shape.IsNull() ? nullptr : AllocateShape(item.Shape);
    const int targetIndex = item.Shape.IsNull() ? -1 : result->LocalFeature->FinalTopology.FindIndex(item.Shape) - 1;
    *info = {item.SourceIndex, item.SubshapeIndex, item.SourceKind, item.Kind, result->LocalFeature->HistoryGroups[index], targetIndex}; *shape = copy;
  });
}
