#include "Modeling/LocalFeatures.hxx"
#include <BRepFeat_MakeRevol.hxx>
#include <BRepFeat_MakePipe.hxx>
#include <gp_Ax1.hxx>
#include <numbers>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
namespace {
template<class Algorithm> void AddSlides(Algorithm& algorithm, const InputGraph& graph, const OcctSharp_SlidingPair* sliding, int count) {
  for (int i = 0; i < count; ++i) algorithm.Add(TopoDS::Edge(graph.At(sliding[i].edge_input)), TopoDS::Face(graph.At(sliding[i].face_input)));
}
template<class Algorithm> void Finish(Algorithm& algorithm, const InputGraph& graph, const OcctSharp_LimitedFeatureOptions& o, Result& result) {
  result.Data().Info.done = algorithm.IsDone(); result.Data().Info.algorithm_status = static_cast<int>(algorithm.CurrentStatusError());
  if (algorithm.IsDone()) {
    result.Owner->Result = algorithm.Shape(); History(algorithm, graph, result); algorithm.CopyGroups(result);
    if (o.from_input >= 0) result.Add(graph.At(o.from_input), Limit, o.from_input, 0, 0);
    if (o.until_input >= 0) result.Add(graph.At(o.until_input), Limit, o.until_input, 0, 1);
    result.Owner->Message = "Local BRepFeat sweep built with explicit support and shape limits.";
  } else result.Fail("Local BRepFeat sweep could not satisfy its limiter.", result.Data().Info.algorithm_status);
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_limited_sweep(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, const OcctSharp_SlidingPair* sliding, int32_t slidingCount,
  const OcctSharp_LimitedFeatureOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && output, "Missing limited sweep options/output."); const auto& o = *options;
    Require(o.operation == 2 || o.operation == 3, "Invalid limited sweep kind.");
    InputGraph graph(inputs, count); ValidateLimits(graph, o); ValidateSliding(graph, sliding, slidingCount);
    if (o.operation == 2) {
      Require(o.path_input == -1 && o.limit_mode != 3 && o.limit_mode != 4, "Revolved features do not implement prism end modes.");
      if (o.limit_mode == 0 || o.limit_mode == 6) Require(o.extent <= 2 * std::numbers::pi, "Revolution angle exceeds one turn.");
    } else {
      graph.Typed(o.path_input, TopAbs_WIRE);
      Require(o.limit_mode == 0 || o.limit_mode == 1 || o.limit_mode == 2, "Pipe limits support complete spine, Until or From/Until.");
    }
    Result result(o.operation == 2 ? 6 : 7);
    try {
      if (o.operation == 2) {
        FormAdapter<BRepFeat_MakeRevol> algorithm(graph.At(0), graph.At(1), TopoDS::Face(graph.At(o.support_input)), gp_Ax1(Point(o.origin), Direction(o.direction)), o.fuse, o.modify != 0);
        AddSlides(algorithm, graph, sliding, slidingCount); result.Data().Info.ready = 1;
        switch (o.limit_mode) {
          case 0: algorithm.Perform(o.extent); break;
          case 1: algorithm.Perform(graph.At(o.until_input)); break;
          case 2: algorithm.Perform(graph.At(o.from_input), graph.At(o.until_input)); break;
          case 5: algorithm.PerformThruAll(); break;
          case 6: algorithm.PerformUntilAngle(graph.At(o.until_input), o.extent); break;
        }
        Finish(algorithm, graph, o, result);
      } else {
        FormAdapter<BRepFeat_MakePipe> algorithm(graph.At(0), graph.At(1), TopoDS::Face(graph.At(o.support_input)), TopoDS::Wire(graph.At(o.path_input)), o.fuse, o.modify != 0);
        AddSlides(algorithm, graph, sliding, slidingCount); result.Data().Info.ready = 1;
        if (o.limit_mode == 0) algorithm.Perform();
        else if (o.limit_mode == 1) algorithm.Perform(graph.At(o.until_input));
        else algorithm.Perform(graph.At(o.from_input), graph.At(o.until_input));
        Finish(algorithm, graph, o, result);
      }
    } catch (const Standard_Failure& error) {
      const std::string message = std::string(result.Data().Info.done ? "Local sweep history: " : "Local sweep construction: ")
        + (error.GetMessageString() ? error.GetMessageString() : "OCCT failure");
      result.Fail(message.c_str());
    }
    result.Publish(output);
  });
}
