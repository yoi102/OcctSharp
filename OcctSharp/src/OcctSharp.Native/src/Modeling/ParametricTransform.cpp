#include "Modeling/GuidedAuthoring.hxx"
#include "OcctSharp.Native.Parametric.h"
#include <BRepBuilderAPI_Transform.hxx>
#include <gp_Ax1.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_transform(
  const OcctSharp_ShapeHandle* source, const double* values,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output)
{
  if (output) *output = nullptr;
  if (info) *info = {};
  return Guard([&] {
    Require(values && info && output, "Missing transform input or output."); ValidateUsableShape(source);
    for (int i = 0; i < 7; ++i) Require(std::isfinite(values[i]), "Transform values must be finite.");
    gp_Trsf transform;
    transform.SetRotation(gp_Ax1(gp_Pnt(0, 0, 0), gp_Dir(values[3], values[4], values[5])), values[6]);
    transform.SetTranslationPart(gp_Vec(values[0], values[1], values[2]));
    BRepBuilderAPI_Transform algorithm(source->Value, transform, true, true);
    Require(algorithm.IsDone(), "Source transform did not complete.");
    auto result = std::make_unique<OcctSharp_FeatureResultHandle>(); result->Result = algorithm.Shape();
    auto topology = Map(source->Value);
    for (int i = 1; i <= topology.Extent(); ++i) {
      const auto mapped = algorithm.ModifiedShape(topology(i));
      Require(!mapped.IsNull(), "The transform has no exact topology correspondence.");
      Add(*result, topology(i), 7, 0, i - 1, topology(i).ShapeType());
      Add(*result, mapped, 0, 0, i - 1, topology(i).ShapeType());
    }
    OcctSharp_AuthoringInfo state{}; state.ready = 1; state.done = 1; state.continuity_limit = -1;
    result->Message = "Exact BRepBuilderAPI transform correspondence; no proximity matching.";
    Finish(*result, state); *output = RegisterFeatureResult(std::move(result)); *info = state;
  });
}
