#include "Modeling/GuidedAuthoring.hxx"

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_copy_inputs(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output != nullptr, "The authoring copy output is null."); InputGraph graph(inputs, count);
    auto result = std::make_unique<OcctSharp_FeatureResultHandle>();
    for (int i = 0; i < count; ++i) Add(*result, graph.Shapes[i], 7, i);
    result->Info.generated_count = count; result->Info.succeeded = 1;
    *output = RegisterFeatureResult(std::move(result));
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_history(const OcctSharp_FeatureResultHandle* result,
  int32_t index, OcctSharp_AuthoringHistoryInfo* info, OcctSharp_ShapeHandle** shape) {
  if (shape) *shape = nullptr;
  return Guard([&] {
    Require(info != nullptr && shape != nullptr, "An authoring history output is null."); ValidateFeatureResult(result);
    Require(index >= 0 && index < static_cast<int>(result->History.size()), "History index is out of range.");
    const auto& item = result->History[index];
    auto* value = item.Shape.IsNull() ? nullptr : AllocateShape(item.Shape);
    *info = {item.SourceIndex, item.SubshapeIndex, item.SourceKind, item.Kind}; *shape = value;
  });
}
