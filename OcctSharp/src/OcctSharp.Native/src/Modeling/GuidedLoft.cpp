#include "Modeling/GuidedAuthoring.hxx"
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepFill_CompatibleWires.hxx>
#include <GeomAbs_Shape.hxx>
#include <Approx_ParametrizationType.hxx>
#include <TopoDS_Wire.hxx>
#include <TopoDS_Vertex.hxx>
#include <TopoDS_Edge.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

OcctSharp_Status OCCTSHARP_CALL occtsharp_guided_loft(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, const OcctSharp_LoftOptions* options, OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && info && output, "A guided loft argument is null."); const auto& o = *options;
    Flag(o.solid); Flag(o.ruled); Flag(o.compatibility); Flag(o.smoothing); Positive(o.tolerance);
    Require(o.reserved == 0 && count >= 2 && count <= 128 && o.maximum_degree >= 2 && o.maximum_degree <= 25
      && o.continuity >= 0 && o.continuity <= 2 && o.parameterization >= 0 && o.parameterization <= 2, "Invalid loft controls.");
    Positive(o.weight_1); Positive(o.weight_2); Positive(o.weight_3);
    InputGraph graph(inputs, count); auto result = std::make_unique<OcctSharp_FeatureResultHandle>();
    OcctSharp_AuthoringInfo state{}; state.continuity_limit = -1;
    try {
      BRepOffsetAPI_ThruSections builder(o.solid != 0, o.ruled != 0, o.tolerance);
      // Explicitly retain the compatibility operation's working wires: ThruSections
      // Wires() returns myInputWires, not its corrected private myWires sequence.
      builder.SetMutableInput(true); builder.CheckCompatibility(false);
      builder.SetSmoothing(o.smoothing != 0); builder.SetMaxDegree(o.maximum_degree);
      builder.SetContinuity(o.continuity == 0 ? GeomAbs_C0 : o.continuity == 1 ? GeomAbs_C1 : GeomAbs_C2);
      builder.SetParType(static_cast<Approx_ParametrizationType>(o.parameterization));
      builder.SetCriteriumWeight(o.weight_1, o.weight_2, o.weight_3);
      NCollection_Sequence<TopoDS_Shape> working;
      std::vector<ShapeMap> originalTopology;
      for (int i = 0; i < count; ++i) {
        const auto& section = graph.At(i);
        originalTopology.push_back(Map(section));
        if (section.ShapeType() == TopAbs_WIRE) working.Append(section);
        else {
          Require((i == 0 || i == count - 1) && section.ShapeType() == TopAbs_VERTEX, "Only loft endpoints can be vertices.");
          BRep_Builder b; TopoDS_Edge edge; TopoDS_Wire wire; b.MakeEdge(edge);
          b.Add(edge, section.Oriented(TopAbs_FORWARD)); b.Add(edge, section.Oriented(TopAbs_REVERSED)); b.Degenerated(edge, true);
          b.MakeWire(wire); b.Add(wire, edge); wire.Closed(true); working.Append(wire);
        }
      }
      BRepFill_CompatibleWires compatibility(working);
      if (o.compatibility) {
        compatibility.Perform();
        Require(compatibility.IsDone(), "OCCT section compatibility failed.");
        working = compatibility.Shape();
      }
      for (int i = 1; i <= working.Length(); ++i) builder.AddWire(TopoDS::Wire(working(i)));
      state.ready = 1; builder.Build(); state.done = builder.IsDone(); state.algorithm_status = static_cast<int>(builder.GetStatus());
      if (state.done) {
        result->Result = builder.Shape();
        // OCCT FirstShape/LastShape describe caps and may be null at punctual
        // endpoints. Retain the exact supplied endpoint (or compatible wire),
        // without inventing a cap or matching a vertex by geometric proximity.
        const auto first = builder.FirstShape();
        const auto last = builder.LastShape();
        Add(*result, !first.IsNull() ? first : graph.At(0).ShapeType() == TopAbs_VERTEX ? graph.At(0) : working(1), 2);
        Add(*result, !last.IsNull() ? last : graph.At(count - 1).ShapeType() == TopAbs_VERTEX ? graph.At(count - 1) : working(count), 3);
        for (int source = 0; source < count; ++source) {
          for (int i = 1; i <= originalTopology[source].Extent(); ++i) {
            const auto& original = originalTopology[source](i); bool mapped = false;
            NCollection_List<TopoDS_Shape> candidates;
            const auto* corrected = o.compatibility ? compatibility.Generated().Seek(original) : nullptr;
            if (corrected) candidates = *corrected; else candidates.Append(original);
            for (const auto& candidate : candidates) {
              for (const auto& generated : builder.Generated(candidate)) { Add(*result, generated, 1, source, i - 1, original.ShapeType()); mapped = true; }
              for (const auto& modified : builder.Modified(candidate)) { Add(*result, modified, 0, source, i - 1, original.ShapeType()); mapped = true; }
            }
            if (!mapped) Add(*result, {}, 5, source, i - 1, original.ShapeType());
          }
          Add(*result, working(source + 1), 6, source, 0, graph.At(source).ShapeType());
        }
        state.section_count = working.Length(); result->Message = "Explicit compatible working sections and composed exact edge history are copied; original inputs are unchanged.";
      } else result->Message = "OCCT loft did not complete.";
    } catch (const Standard_Failure& error) { state.done = 0; result->Message = error.GetMessageString(); }
    Finish(*result, state); *output = RegisterFeatureResult(std::move(result)); *info = state;
  });
}
