#include "Modeling/LocalFeatures.hxx"
#include <BRepFeat_MakePrism.hxx>
#include <BRepFeat_MakeDPrism.hxx>
#include <numbers>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
namespace {
template<class Algorithm> void Execute(Algorithm& algorithm, const InputGraph& graph,
  const OcctSharp_LimitedFeatureOptions& o, const OcctSharp_SlidingPair* sliding, int slidingCount, Result& result) {
  for (int i = 0; i < slidingCount; ++i)
    algorithm.Add(TopoDS::Edge(graph.At(sliding[i].edge_input)), TopoDS::Face(graph.At(sliding[i].face_input)));
  result.Data().Info.ready = 1;
  switch (o.limit_mode) {
    case 0: algorithm.Perform(o.extent); break;
    case 1: algorithm.Perform(graph.At(o.until_input)); break;
    case 2: algorithm.Perform(graph.At(o.from_input), graph.At(o.until_input)); break;
    case 3: algorithm.PerformUntilEnd(); break;
    case 4: algorithm.PerformFromEnd(graph.At(o.until_input)); break;
    case 5: algorithm.PerformThruAll(); break;
    case 6: algorithm.PerformUntilHeight(graph.At(o.until_input), o.extent); break;
  }
  result.Data().Info.done = algorithm.IsDone(); result.Data().Info.algorithm_status = static_cast<int>(algorithm.CurrentStatusError());
  if (algorithm.IsDone()) {
    result.Owner->Result = algorithm.Shape(); History(algorithm, graph, result); algorithm.CopyGroups(result);
    if (o.from_input >= 0) result.Add(graph.At(o.from_input), Limit, o.from_input, 0, 0);
    if (o.until_input >= 0) result.Add(graph.At(o.until_input), Limit, o.until_input, 0, 1);
    result.Owner->Message = "BRepFeat prism uses explicit support/sliding/limiter semantics; no Boolean fallback.";
  } else result.Fail("The BRepFeat prism limiter construction failed.", result.Data().Info.algorithm_status);
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_limited_prism(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, const OcctSharp_SlidingPair* sliding, int32_t slidingCount,
  const OcctSharp_LimitedFeatureOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && output, "Missing local prism options/output."); const auto& o = *options;
    Require(o.operation == 0 || o.operation == 1, "A prism request cannot invoke a different local feature.");
    Require(o.path_input == -1, "A prism does not accept a pipe spine.");
    InputGraph graph(inputs, count); ValidateLimits(graph, o); ValidateSliding(graph, sliding, slidingCount);
    if (o.operation == 1) { graph.Typed(1, TopAbs_FACE); Require(std::abs(o.draft_angle) < std::numbers::pi / 2, "Invalid drafted-prism angle."); }
    Result result(4 + o.operation);
    try {
      if (o.operation == 0) {
        FormAdapter<BRepFeat_MakePrism> algorithm(graph.At(0), graph.At(1), TopoDS::Face(graph.At(o.support_input)), Direction(o.direction), o.fuse, o.modify != 0);
        Execute(algorithm, graph, o, sliding, slidingCount, result);
      } else {
        FormAdapter<BRepFeat_MakeDPrism> algorithm(graph.At(0), TopoDS::Face(graph.At(1)), TopoDS::Face(graph.At(o.support_input)), o.draft_angle, o.fuse, o.modify != 0);
        Execute(algorithm, graph, o, sliding, slidingCount, result);
      }
    } catch (const Standard_Failure& error) {
      const std::string message = std::string(result.Data().Info.done ? "Prism history: " : "Prism construction: ")
        + (error.GetMessageString() ? error.GetMessageString() : "OCCT failure");
      result.Fail(message.c_str());
    }
    result.Publish(output);
  });
}
