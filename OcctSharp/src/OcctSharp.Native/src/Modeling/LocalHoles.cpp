#include "Modeling/LocalFeatures.hxx"
#include <BRepFeat_MakeCylindricalHole.hxx>
#include <gp_Ax1.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;

OcctSharp_Status OCCTSHARP_CALL occtsharp_local_hole(const OcctSharp_ShapeHandle* source,
  const OcctSharp_LocalHoleOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && output, "Missing local-hole options or output.");
    const auto& o = *options;
    Require(o.reserved1 == 0 && o.reserved2 == 0 && o.reserved3 == 0, "Nonzero local-hole reserved field.");
    Require(o.mode >= 0 && o.mode <= 4, "Unknown local-hole bound mode.");
    Positive(o.radius); Point(o.origin); Direction(o.direction);
    Require(std::isfinite(o.first) && std::isfinite(o.last), "Non-finite axial bounds.");
    if (o.mode == 1) Require(o.last > o.first, "Hole axial bounds must be increasing.");
    if (o.mode == 4) Positive(o.last);
    const OcctSharp_ShapeHandle* inputs[] = {source}; InputGraph graph(inputs, 1);
    Result result(10);
    try {
      BRepFeat_MakeCylindricalHole algorithm;
      algorithm.Init(graph.At(0), gp_Ax1(Point(o.origin), Direction(o.direction)));
      result.Data().Info.ready = 1;
      switch (o.mode) {
        case 0: algorithm.Perform(o.radius); break;
        case 1: algorithm.Perform(o.radius, o.first, o.last, true); break;
        case 2: algorithm.PerformThruNext(o.radius, true); break;
        case 3: algorithm.PerformUntilEnd(o.radius, true); break;
        case 4: algorithm.PerformBlind(o.radius, o.last, true); break;
      }
      // This BOP-derived builder has no MakeShape IsDone(). Status, BOP errors
      // and the explicit Build phase all participate in the success contract.
      if (algorithm.Status() == BRepFeat_NoError && !algorithm.HasErrors()) algorithm.Build();
      result.Data().Info.algorithm_status = static_cast<int>(algorithm.Status());
      if (algorithm.Status() == BRepFeat_NoError && !algorithm.HasErrors() && !algorithm.Shape().IsNull()) {
        result.Data().Info.done = 1; result.Owner->Result = algorithm.Shape();
        History(algorithm, graph, result);
        result.Owner->Message = "Local cylindrical hole completed with native result control enabled.";
      } else result.Fail("Local-hole placement/bounds or result control failed.", result.Data().Info.algorithm_status);
    } catch (const Standard_Failure& error) { result.Fail(error.GetMessageString()); }
    result.Publish(output);
  });
}
