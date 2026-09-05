#pragma once
#include "OcctSharp.Native.LocalFeatures.h"
#include "Modeling/Features.hxx"
#include "Modeling/Topology.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <cmath>
#include <memory>
#include <vector>

static_assert(sizeof(OcctSharp_LocalFeatureInfo) == 56);
static_assert(sizeof(OcctSharp_ContourInfo) == 56);
static_assert(sizeof(OcctSharp_ContourEdge) == 40);
static_assert(sizeof(OcctSharp_FilletSection) == 112);
static_assert(sizeof(OcctSharp_LocalFeatureFault) == 16);
static_assert(sizeof(OcctSharp_LocalFeatureHistory) == 24);
static_assert(sizeof(OcctSharp_FilletOptions) == 72);
static_assert(sizeof(OcctSharp_FilletProgram) == 40);
static_assert(sizeof(OcctSharp_FaceDraftProgram) == 96);
static_assert(sizeof(OcctSharp_ShellDraftOptions) == 72);
static_assert(sizeof(OcctSharp_LimitedFeatureOptions) == 96);
static_assert(sizeof(OcctSharp_RibSlotOptions) == 192);
static_assert(sizeof(OcctSharp_LocalHoleOptions) == 88);

struct OcctSharp_LocalFeatureData {
  OcctSharp_LocalFeatureInfo Info{};
  std::vector<OcctSharp_ContourInfo> Contours;
  std::vector<OcctSharp_ContourEdge> Edges;
  std::vector<OcctSharp_FilletSection> Sections;
  std::vector<OcctSharp_LocalFeatureFault> Faults;
  std::vector<int> HistoryGroups;
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> FinalTopology;
};

namespace OcctSharp::Native::LocalFeatures {
using ShapeMap = NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher>;
enum HistoryKind { Modified, Generated, FirstCap, LastCap, Lateral, Contact, TangentContact,
  SurfacePatch, ContourEdge, AffectedFace, ProblemShape, Partial, Unchanged, Deleted, Unmapped, Limit, PreLimitShape };
enum GroupSupport { Caps = 1, Laterals = 2, Contacts = 4, Patches = 8, Evolution = 16 };
inline void Require(bool value, const char* message) {
  if (!value) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
inline void Positive(double value) { Require(std::isfinite(value) && value > 0, "A local-feature value must be finite and positive."); }
inline void Flag(int value) { Require(value == 0 || value == 1, "Invalid local-feature Boolean flag."); }
inline gp_Pnt Point(OcctSharp_Xyz value) {
  Require(std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z), "Non-finite local-feature coordinates.");
  return gp_Pnt(value.x, value.y, value.z);
}
inline gp_Dir Direction(OcctSharp_Xyz value) {
  const auto p = Point(value); Require(p.XYZ().Modulus() > 1e-12, "A direction must be nonzero."); return gp_Dir(p.XYZ());
}
inline OcctSharp_Xyz Xyz(const gp_XYZ& value) { return {value.X(), value.Y(), value.Z()}; }
ShapeMap Map(const TopoDS_Shape& shape);
// Captures the entire graph in one copy. Correspondence arrays retain the original
// input topology order, even if the native copier chooses a different traversal.
struct InputGraph {
  std::vector<TopoDS_Shape> Shapes;
  std::vector<ShapeMap> Topology;
  InputGraph(const OcctSharp_ShapeHandle* const* inputs, int count);
  const TopoDS_Shape& At(int index) const;
  const TopoDS_Shape& Typed(int index, TopAbs_ShapeEnum kind) const;
  const TopoDS_Shape& Subshape(int argument, int index, TopAbs_ShapeEnum kind) const;
  int Index(int argument, const TopoDS_Shape& shape) const;
};
struct Result {
  std::unique_ptr<OcctSharp_FeatureResultHandle> Owner;
  explicit Result(int operation);
  OcctSharp_LocalFeatureData& Data() { return *Owner->LocalFeature; }
  void Add(const TopoDS_Shape& shape, int kind, int argument = -1, int topology = -1, int group = -1, int sourceKind = -1);
  void Fail(const char* message, int status = -1);
  void Publish(OcctSharp_FeatureResultHandle** output);
};
void ValidateSliding(const InputGraph& graph, const OcctSharp_SlidingPair* sliding, int count);
void ValidateLimits(const InputGraph& graph, const OcctSharp_LimitedFeatureOptions& options);

template<class Algorithm> void History(Algorithm& algorithm, const InputGraph& graph, Result& result) {
  result.Data().Info.group_support |= Evolution;
  const auto finalMap = Map(result.Owner->Result);
  for (int argument = 0; argument < static_cast<int>(graph.Shapes.size()); ++argument) {
    const auto& topology = graph.Topology[argument];
    for (int i = 1; i <= topology.Extent(); ++i) {
      const auto& shape = topology(i);
      if (shape.ShapeType() != TopAbs_FACE && shape.ShapeType() != TopAbs_EDGE && shape.ShapeType() != TopAbs_VERTEX) continue;
      bool mapped = false;
      for (const auto& generated : algorithm.Generated(shape)) {
        result.Add(generated, Generated, argument, i - 1, -1, shape.ShapeType()); mapped = true;
      }
      for (const auto& modified : algorithm.Modified(shape)) {
        result.Add(modified, Modified, argument, i - 1, -1, shape.ShapeType()); mapped = true;
      }
      if (finalMap.Contains(shape)) result.Add(shape, Unchanged, argument, i - 1, -1, shape.ShapeType());
      else if (!mapped) result.Add({}, algorithm.IsDeleted(shape) ? Deleted : Unmapped, argument, i - 1, -1, shape.ShapeType());
    }
  }
}
// BRepFeat_Form cap getters do not guard absent keys after a limiting cut.
// Keep the builder local and copy only relations that reach its final topology.
template<class Algorithm> class FormAdapter final : public Algorithm {
public:
  using Algorithm::Algorithm;
  void CopyGroups(Result& result) const {
    result.Data().Info.group_support |= Caps | Contacts | Laterals;
    const auto finalMap = Map(result.Owner->Result);
    const auto cap = [&](const TopoDS_Shape& source, int kind) {
      if (source.IsNull()) return;
      const auto* shapes = this->myMap.Seek(source);
      if (shapes) for (const auto& shape : *shapes)
        if (finalMap.Contains(shape)) result.Add(finalMap(finalMap.FindIndex(shape)), kind);
    };
    cap(this->myFShape, FirstCap); cap(this->myLShape, LastCap);
    for (const auto& shape : this->myNewEdges) if (finalMap.Contains(shape)) result.Add(finalMap(finalMap.FindIndex(shape)), Contact);
    for (const auto& shape : this->myTgtEdges) if (finalMap.Contains(shape)) result.Add(finalMap(finalMap.FindIndex(shape)), TangentContact);
    // Profile-edge-generated final faces are exact laterals, even when a limit
    // removes the original caps. Preserve final traversal orientation as well as
    // TShape/location identity. Never infer a new cap from geometric proximity.
    ShapeMap laterals;
    for (const auto& item : result.Owner->History)
      if (item.SourceIndex == 1 && item.SourceKind == TopAbs_EDGE && item.Kind == Generated
        && !item.Shape.IsNull() && item.Shape.ShapeType() == TopAbs_FACE && finalMap.Contains(item.Shape))
        laterals.Add(finalMap(finalMap.FindIndex(item.Shape)));
    for (const auto& shape : laterals) result.Add(shape, Lateral);
  }
};
}
