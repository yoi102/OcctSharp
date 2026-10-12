#include "Modeling/LocalFeatures.hxx"
#include "Modeling/Repair.hxx"
#include "../../generated/Modeling/OcctSharp.Modeling.BooleanRepairAdapters.Generated.hxx"
#include <NCollection_List.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;

OcctSharp_Status OCCTSHARP_CALL occtsharp_boolean_topology_history(
  const OcctSharp_ShapeHandle* const* inputs, int32_t count, int32_t argument_count,
  int32_t operation, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output != nullptr, "Missing Boolean history output.");
    Require(count >= 2 && count <= 256 && argument_count > 0 && argument_count < count,
      "Boolean history requires two to 256 inputs and nonempty argument/tool groups.");
    Require(operation >= 0 && operation <= 2, "Unsupported Boolean history operation.");
    // One private graph copy preserves each original input's full-map order.
    // The caller's input geometry and topology flags remain untouched.
    InputGraph graph(inputs, count);
    NCollection_List<TopoDS_Shape> arguments, tools;
    for (int i = 0; i < count; ++i)
      (i < argument_count ? arguments : tools).Append(graph.At(i));
    OcctSharpGenerated::BooleanHistoryBuilder algorithm(operation);
    algorithm.SetArguments(arguments); algorithm.SetTools(tools);
    algorithm.SetNonDestructive(true); algorithm.SetRunParallel(false);
    algorithm.SetToFillHistory(true);
    Result result(11); result.Data().Info.ready = 1;
    algorithm.Build();
    const auto diagnostics = algorithm.Diagnostics();
    std::string diagnosticText;
    if (diagnostics.hasErrors) diagnosticText += "\nErrors:\n" + diagnostics.errors;
    if (diagnostics.hasWarnings) diagnosticText += "\nWarnings:\n" + diagnostics.warnings;
    if (!algorithm.IsDone() || diagnostics.hasErrors) {
      const auto message = "OCCT Boolean history operation did not complete." + diagnosticText;
      result.Fail(message.c_str());
    }
    else {
      result.Owner->Result = algorithm.Shape(); result.Data().Info.done = 1;
      const auto finalMap = Map(result.Owner->Result);
      // RepairSnapshot copies the result before diagnostics/persistence. Prove
      // that the actual copier preserves every full-map slot via its exact
      // ModifiedShape correspondence, never standalone BRep byte similarity.
      std::vector<TopoDS_Shape> correspondence;
      const auto diagnosticCopy = OcctSharp::Native::Repair::Copy(result.Owner->Result, &correspondence);
      const auto diagnosticMap = Map(diagnosticCopy);
      bool stable = diagnosticMap.Extent() == finalMap.Extent()
        && correspondence.size() == static_cast<size_t>(finalMap.Extent());
      for (int i = 1; stable && i <= finalMap.Extent(); ++i)
        stable = diagnosticMap(i).IsSame(correspondence[i - 1])
          && diagnosticMap(i).Orientation() == finalMap(i).Orientation();
      // Modified/Generated/Deleted are read while this exact builder is alive;
      // Unchanged uses TShape/location identity in the final output graph.
      if (stable) History(algorithm, graph, result);
      for (auto& item : result.Owner->History)
        if (!item.Shape.IsNull() && finalMap.Contains(item.Shape))
          item.Shape = finalMap(finalMap.FindIndex(item.Shape));
      result.Owner->Message = stable ? "Boolean topology history captured from the final output."
        : "Valid Boolean geometry; diagnostic copy order could not be verified, history unavailable.";
      result.Owner->Message += diagnosticText;
    }
    result.Publish(output);
  });
}
