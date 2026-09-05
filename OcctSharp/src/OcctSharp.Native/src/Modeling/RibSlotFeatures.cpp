#include "Modeling/LocalFeatures.hxx"
#include <BRepFeat_MakeLinearForm.hxx>
#include <BRepFeat_MakeRevolutionForm.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepBndLib.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <Bnd_Box.hxx>
#include <Geom_Plane.hxx>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Pln.hxx>
#include <numbers>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
namespace {
// OCCT RibSlot history getters assume map membership and reuse mutable/static
// scratch lists. Read its protected maps locally instead, preserving absence.
template<class Algorithm> class RibHistoryAdapter final : public Algorithm {
public:
  using Algorithm::Algorithm;
  void CopyHistory(const InputGraph& graph, Result& result) const {
    const auto finalMap = Map(result.Owner->Result);
    result.Data().Info.group_support |= Evolution | Caps | Contacts;
    for (int argument = 0; argument < static_cast<int>(graph.Shapes.size()); ++argument) {
      const auto& topology = graph.Topology[argument];
      for (int i = 1; i <= topology.Extent(); ++i) {
        const auto& source = topology(i); const auto kind = source.ShapeType();
        if (kind != TopAbs_FACE && kind != TopAbs_EDGE && kind != TopAbs_VERTEX) continue;
        bool mapped = false;
        const auto append = [&](const auto& shapes, int relation) {
          for (const auto& target : shapes) {
            if (!target.IsSame(source) && finalMap.Contains(target)) {
              result.Add(target, relation, argument, i - 1, -1, kind); mapped = true;
            }
          }
        };
        const auto* descendants = this->myMap.Seek(source);
        if (descendants) append(*descendants, Modified);
        if (kind != TopAbs_FACE) {
          const auto* generated = this->myLFMap.Seek(source);
          if (generated) {
            for (const auto& intermediate : *generated) {
              const auto* targets = this->myMap.Seek(intermediate);
              if (targets) append(*targets, Generated);
            }
          } else if (descendants) append(*descendants, Generated);
        }
        if (finalMap.Contains(source)) result.Add(source, Unchanged, argument, i - 1, -1, kind);
        else if (!mapped) result.Add({}, descendants && descendants->IsEmpty() ? Deleted : Unmapped,
          argument, i - 1, -1, kind);
      }
    }
    const auto cap = [&](const TopoDS_Shape& source, int kind) {
      if (source.IsNull()) return;
      const auto* shapes = this->myMap.Seek(source);
      if (shapes) for (const auto& shape : *shapes)
        if (finalMap.Contains(shape)) result.Add(finalMap(finalMap.FindIndex(shape)), kind);
    };
    cap(this->myFShape, FirstCap); cap(this->myLShape, LastCap);
    for (const auto& edge : this->myNewEdges) if (finalMap.Contains(edge)) result.Add(finalMap(finalMap.FindIndex(edge)), Contact);
    for (const auto& edge : this->myTgtEdges) if (finalMap.Contains(edge)) result.Add(finalMap(finalMap.FindIndex(edge)), TangentContact);
  }
};

template<class Algorithm> TopoDS_Shape Boolean(const TopoDS_Shape& left, const TopoDS_Shape& right) {
  Algorithm operation; NCollection_List<TopoDS_Shape> arguments, tools;
  arguments.Append(left); tools.Append(right);
  operation.SetArguments(arguments); operation.SetTools(tools);
  operation.SetNonDestructive(true); operation.Build();
  if (!operation.IsDone() || operation.HasErrors() || operation.Shape().IsNull())
    throw Standard_Failure("Explicit rib angular clipping failed.");
  return operation.Shape();
}

void ClipMaterial(const InputGraph& graph, const OcctSharp_RibSlotOptions& o, Result& result) {
  // A distinct, requested stage AFTER a successful native rib/slot construction.
  // Clip only the difference material, never intersect the entire base with a wedge.
  const auto preLimit = result.Owner->Result;
  const auto material = o.fuse ? Boolean<BRepAlgoAPI_Cut>(preLimit, graph.At(0))
                              : Boolean<BRepAlgoAPI_Cut>(graph.At(0), preLimit);
  Bnd_Box bounds; BRepBndLib::Add(material, bounds);
  if (bounds.IsVoid() || bounds.IsOpen()) throw Standard_Failure("Rib material has no finite clipping domain.");
  double x0, y0, z0, x1, y1, z1; bounds.Get(x0, y0, z0, x1, y1, z1);
  const auto origin = Point(o.axis_origin); const auto direction = Direction(o.axis_direction);
  double radius = 1, first = 0, last = 0;
  for (double x : {x0, x1}) for (double y : {y0, y1}) for (double z : {z0, z1}) {
    const gp_Vec delta(origin, gp_Pnt(x, y, z)); const double height = delta.Dot(gp_Vec(direction));
    first = std::min(first, height); last = std::max(last, height);
    radius = std::max(radius, delta.Crossed(gp_Vec(direction)).Magnitude());
  }
  const double margin = std::max(1.0, radius * 0.01);
  // Angular zero is the plane's positive radial direction, n x axis.
  gp_Dir radial = Direction(o.plane_normal).Crossed(direction);
  radial.Rotate(gp_Ax1(origin, direction), o.angle_first);
  const gp_Pnt bottom = origin.Translated(gp_Vec(direction) * (first - margin));
  BRepPrimAPI_MakeCylinder wedge(gp_Ax2(bottom, direction, radial), radius + margin,
    last - first + 2 * margin, o.angle_last - o.angle_first);
  const auto clipped = Boolean<BRepAlgoAPI_Common>(material, wedge.Shape());
  const auto finalShape = o.fuse ? Boolean<BRepAlgoAPI_Fuse>(graph.At(0), clipped)
                               : Boolean<BRepAlgoAPI_Cut>(graph.At(0), clipped);
  // Original native evolution points into preLimit, not into the composed final.
  // Do not silently present those relations as exact final-result correspondence.
  result.Owner->History.clear(); result.Data().HistoryGroups.clear();
  result.Data().Info.group_support = 0; result.Data().Info.composed = 1;
  result.Add(preLimit, PreLimitShape); result.Add(wedge.Shape(), Limit);
  result.Owner->Result = finalShape;
  const auto finalMap = Map(finalShape);
  for (int i = 1; i <= graph.Topology[0].Extent(); ++i) {
    const auto& source = graph.Topology[0](i);
    if (source.ShapeType() != TopAbs_FACE && source.ShapeType() != TopAbs_EDGE && source.ShapeType() != TopAbs_VERTEX) continue;
    if (finalMap.Contains(source)) result.Add(source, Unchanged, 0, i - 1, -1, source.ShapeType());
    else result.Add({}, Unmapped, 0, i - 1, -1, source.ShapeType());
  }
  result.Owner->Message = "Native revolution rib/slot followed by explicit angular material clipping; composed history is not exact native final evolution.";
}

template<class Algorithm> void Execute(Algorithm& algorithm, const InputGraph& graph,
  const OcctSharp_SlidingPair* sliding, int count, Result& result) {
  result.Data().Info.ready = algorithm.IsDone();
  if (!algorithm.IsDone()) { result.Fail("Rib/slot initialization failed.", algorithm.CurrentStatusError()); return; }
  for (int i = 0; i < count; ++i)
    algorithm.Add(TopoDS::Edge(graph.At(sliding[i].edge_input)), TopoDS::Face(graph.At(sliding[i].face_input)));
  algorithm.Perform(); result.Data().Info.algorithm_status = algorithm.CurrentStatusError();
  if (!algorithm.IsDone()) { result.Fail("Rib/slot reconstruction failed.", algorithm.CurrentStatusError()); return; }
  result.Data().Info.done = 1; result.Owner->Result = algorithm.Shape();
  algorithm.CopyHistory(graph, result);
  result.Data().Info.group_support |= Laterals;
  const auto finalMap = Map(result.Owner->Result);
  for (const auto& face : algorithm.FacesForDraft())
    if (finalMap.Contains(face)) result.Add(finalMap(finalMap.FindIndex(face)), Lateral);
  result.Owner->Message = "Native rib/slot construction with copied cap, contact and draft-face provenance.";
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_rib_slot(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, const OcctSharp_SlidingPair* sliding, int32_t slidingCount,
  const OcctSharp_RibSlotOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && output && count >= 2, "Missing rib/slot input graph or options."); const auto& o = *options;
    Flag(o.revolution); Flag(o.fuse); Flag(o.sliding); Flag(o.angular_limit);
    InputGraph graph(inputs, count); graph.Typed(1, TopAbs_WIRE); ValidateSliding(graph, sliding, slidingCount);
    const auto origin = Point(o.plane_origin); const auto normal = Direction(o.plane_normal);
    const occ::handle<Geom_Plane> plane = new Geom_Plane(gp_Pln(origin, normal));
    if (o.revolution) {
      Require(std::isfinite(o.thickness1) && std::isfinite(o.thickness2)
        && o.thickness1 >= 0 && o.thickness2 >= 0 && o.thickness1 + o.thickness2 > 0, "Rib thicknesses must be nonnegative distances with positive total.");
      Point(o.axis_origin); Direction(o.axis_direction);
      if (o.angular_limit) {
        Require(std::isfinite(o.angle_first) && std::isfinite(o.angle_last)
          && o.angle_last > o.angle_first && o.angle_last - o.angle_first <= 2 * std::numbers::pi,
          "Angular clipping requires a finite increasing interval of at most one turn.");
        Require(std::abs(normal.Dot(Direction(o.axis_direction))) < 1e-8, "The angular reference plane must contain the revolution direction.");
      }
    } else {
      Require(!o.angular_limit, "Angular clipping is only defined for revolution ribs.");
      Point(o.direction1); Point(o.direction2);
      Require(Point(o.direction1).XYZ().Modulus() + Point(o.direction2).XYZ().Modulus() > 1e-12, "Linear thickness directions cannot both be zero.");
    }
    Result result(o.revolution ? 9 : 8);
    try {
      if (o.revolution) {
        bool actualSliding = o.sliding != 0;
        RibHistoryAdapter<BRepFeat_MakeRevolutionForm> algorithm(graph.At(0), TopoDS::Wire(graph.At(1)), plane,
          gp_Ax1(Point(o.axis_origin), Direction(o.axis_direction)), o.thickness1, o.thickness2, o.fuse, actualSliding);
        Execute(algorithm, graph, sliding, slidingCount, result);
        if (result.Data().Info.done && o.angular_limit) ClipMaterial(graph, o, result);
      } else {
        RibHistoryAdapter<BRepFeat_MakeLinearForm> algorithm(graph.At(0), TopoDS::Wire(graph.At(1)), plane,
          gp_Vec(Point(o.direction1).XYZ()), gp_Vec(Point(o.direction2).XYZ()), o.fuse, o.sliding != 0);
        Execute(algorithm, graph, sliding, slidingCount, result);
      }
    } catch (const Standard_Failure& error) { result.Owner->Result.Nullify(); result.Fail(error.GetMessageString()); }
    result.Publish(output);
  });
}
