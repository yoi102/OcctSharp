#include "Modeling/LocalFeatures.hxx"
#include "Modeling/Repair.hxx"
#include "Geometry/Transforms.hxx"
#include <BRepBuilderAPI_Transform.hxx>
#include <BRep_Builder.hxx>
#include <TopoDS_Compound.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;

namespace {
// The public full-map index is also consumed through RepairSnapshot. Prove
// that copier's exact correspondence and traversal; an unproven map is not a
// successful history contract, even if the geometry remains valid.
bool StableDiagnosticMap(const ShapeMap& original, const TopoDS_Shape& shape) {
  Require(original.Extent() <= 100000, "Result topology exceeds the history limit.");
  std::vector<TopoDS_Shape> correspondence;
  const auto copied = OcctSharp::Native::Repair::Copy(shape, &correspondence);
  const auto final = Map(copied);
  if (final.Extent() != original.Extent() || correspondence.size() != static_cast<size_t>(original.Extent())) return false;
  for (int i = 1; i <= original.Extent(); ++i)
    if (!final(i).IsSame(correspondence[i - 1]) || final(i).Orientation() != original(i).Orientation()) return false;
  return true;
}
bool IsTracked(const TopoDS_Shape& shape) {
  return shape.ShapeType() == TopAbs_FACE || shape.ShapeType() == TopAbs_EDGE || shape.ShapeType() == TopAbs_VERTEX;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform_history(
  const OcctSharp_ShapeHandle* source, const OcctSharp_TrsfHandle* transform, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output != nullptr, "Missing transformed history output.");
    ValidateTransformHandle(transform);
    Require(std::abs(std::abs(transform->Value.ScaleFactor()) - 1.0) <= 1e-12,
      "Topology history supports rigid placement and exact reflection, not scaling.");
    const OcctSharp_ShapeHandle* inputs[] = {source}; InputGraph graph(inputs, 1);
    BRepBuilderAPI_Transform algorithm(graph.At(0), transform->Value, true, false);
    Result result(12); result.Data().Info.ready = 1;
    if (!algorithm.IsDone()) result.Fail("OCCT exact transformation did not complete.");
    else {
      result.Owner->Result = algorithm.Shape(); result.Data().Info.done = 1;
      const auto final = Map(result.Owner->Result);
      if (StableDiagnosticMap(final, result.Owner->Result)) {
        result.Data().Info.group_support |= Evolution;
        const auto& topology = graph.Topology[0];
        for (int i = 1; i <= topology.Extent(); ++i) {
          const auto& before = topology(i); if (!IsTracked(before)) continue;
          const auto after = algorithm.ModifiedShape(before);
          Require(!after.IsNull() && before.ShapeType() == after.ShapeType() && final.Contains(after),
            "Transformation omitted a native source/result correspondence.");
          result.Add(final(final.FindIndex(after)), Modified, 0, i - 1, -1, before.ShapeType());
        }
        result.Owner->Message = "Exact transformation history captured for every face, edge and vertex.";
      } else result.Owner->Message = "Valid transformed geometry; diagnostic copy order unproven, history unavailable.";
    }
    result.Publish(output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_compound_history(
  const OcctSharp_ShapeHandle* const* inputs, int32_t count, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output != nullptr, "Missing compound history output.");
    Require(count >= 1 && count <= 256, "Compound history requires one to 256 inputs.");
    InputGraph graph(inputs, count); TopoDS_Compound compound; BRep_Builder builder; builder.MakeCompound(compound);
    for (int i = 0; i < count; ++i) builder.Add(compound, graph.At(i));
    Result result(13); result.Data().Info.ready = 1; result.Data().Info.done = 1; result.Owner->Result = compound;
    const auto final = Map(compound);
    if (StableDiagnosticMap(final, compound)) {
      result.Data().Info.group_support |= Evolution;
      for (int argument = 0; argument < count; ++argument) {
        const auto& topology = graph.Topology[argument];
        for (int i = 1; i <= topology.Extent(); ++i) {
          const auto& before = topology(i); if (!IsTracked(before)) continue;
          Require(final.Contains(before), "Compound omitted an actual input member.");
          result.Add(final(final.FindIndex(before)), Unchanged, argument, i - 1, -1, before.ShapeType());
        }
      }
      result.Owner->Message = "Compound native membership history captured for every ordered source argument.";
    } else result.Owner->Message = "Valid compound geometry; diagnostic copy order unproven, history unavailable.";
    result.Publish(output);
  });
}
