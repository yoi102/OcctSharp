#include "Modeling/Regions.hxx"
#include "Modeling/GuidedAuthoring.hxx"
#include "Modeling/Topology.hxx"
#include <BOPAlgo_CellsBuilder.hxx>
#include <TopoDS_Iterator.hxx>
#include <map>
#include <set>
#include <sstream>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Regions;
using OcctSharp::Native::Authoring::Require;

namespace {
// Only this pinned-SDK adapter sees protected material/index storage. Every part
// is checked against GetAllParts and every assignment is resolved before mutation.
// No OCCT algorithm object survives the call.
class Cells final : public BOPAlgo_CellsBuilder {
public:
  const NCollection_List<TopoDS_Shape>* Origins(const TopoDS_Shape& part) const {
    const int index = myIndex.FindIndex(part);
    return index ? &myIndex.FindFromIndex(index) : nullptr;
  }
  void Select(const std::vector<TopoDS_Shape>& parts, const std::vector<int>& materials) {
    RemoveAllFromResult();
    ShapeMap allowed; for (TopoDS_Iterator it(GetAllParts()); it.More(); it.Next()) allowed.Add(it.Value());
    Require(parts.size() == materials.size(), "Selection cardinalities differ.");
    for (size_t i = 0; i < parts.size(); ++i) {
      if (materials[i] < 0) continue;
      Require(allowed.Contains(parts[i]), "Foreign cell reached native selection.");
      BRep_Builder().Add(myShape, parts[i]);
      if (materials[i] != 0) {
        myShapeMaterial.Bind(parts[i], materials[i]);
        auto* group = myMaterials.ChangeSeek(materials[i]);
        if (!group) group = myMaterials.Bound(materials[i], NCollection_List<TopoDS_Shape>());
        group->Append(parts[i]);
      }
    }
    PrepareHistory(Message_ProgressRange());
  }
};
void ValidateExpression(const int* values, int count, int inputs) {
  int depth = 0;
  Require(count > 0 && count <= 4096, "Region expressions require one to 4096 tokens.");
  for (int i = 0; i < count; ++i) {
    const int token = values[i];
    if (token >= 0) { Require(token < inputs, "Region expression input is out of range."); ++depth; }
    else if (token == -1 || token == -2) ++depth;
    else { Require(token >= -5 && token <= -3 && depth >= 2, "Malformed region postfix expression."); --depth; }
  }
  Require(depth == 1, "Region expression leaves more than one result.");
}
bool Evaluate(const int* values, int count, const std::vector<bool>& membership) {
  std::vector<bool> stack; stack.reserve(count);
  for (int i = 0; i < count; ++i) {
    const int token = values[i];
    if (token >= 0) stack.push_back(membership[token]);
    else if (token == -1 || token == -2) stack.push_back(token == -1);
    else {
      const bool right = stack.back(); stack.pop_back(); const bool left = stack.back(); stack.pop_back();
      stack.push_back(token == -3 ? left || right : token == -4 ? left && right : left && !right);
    }
  }
  return stack.back();
}
void CopyHistory(Cells& cells, const std::vector<TopoDS_Shape>& inputs,
  int output, OcctSharp_RegionData& data) {
  ShapeMap finalTopology; TopExp::MapShapes(cells.Shape(), finalTopology);
  for (int source = 0; source < static_cast<int>(inputs.size()); ++source) {
    ShapeMap map; TopExp::MapShapes(inputs[source], map);
    for (int i = 1; i <= map.Extent(); ++i) {
      const auto& shape = map(i);
      const auto kind = shape.ShapeType();
      if (kind == TopAbs_SOLID) { Add(data, History, output, source, i - 1, 3, kind); continue; }
      if (kind != TopAbs_FACE && kind != TopAbs_EDGE && kind != TopAbs_VERTEX) continue;
      bool mapped = false;
      for (const auto& image : cells.Modified(shape)) {
        Add(data, History, output, source, i - 1, 0, kind, 0, image); mapped = true;
      }
      if (!mapped) {
        const bool deleted = cells.IsDeleted(shape);
        const bool unchanged = !deleted && finalTopology.Contains(shape);
        Add(data, History, output, source, i - 1, deleted ? 2 : unchanged ? 1 : 3, kind, 0,
          unchanged ? finalTopology(finalTopology.FindIndex(shape)) : TopoDS_Shape());
      }
    }
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_partition_build(
  const OcctSharp_ShapeHandle* const* inputs, int32_t input_count,
  const OcctSharp_PartitionOptions* options, const OcctSharp_RegionRule* rules, int32_t rule_count,
  const int32_t* expressions, int32_t expression_count,
  const OcctSharp_RegionOutput* outputs, int32_t output_count, OcctSharp_FeatureResultHandle** result) {
  if (result) *result = nullptr;
  return Guard([&] {
    Require(result && options, "Missing partition options or output.");
    Require(input_count >= 1 && input_count <= 128, "Partition input count must be one to 128.");
    Require(std::isfinite(options->fuzzy) && options->fuzzy >= 0 && options->max_cells >= 1
      && options->max_cells <= 100000 && options->reserved == 0, "Invalid partition precision or capacity.");
    Authoring::Flag(options->parallel); Authoring::Flag(options->check_inputs);
    Require(rule_count >= 0 && rule_count <= 4096 && (!rule_count || rules), "Invalid rule buffer.");
    Require(expression_count >= 0 && expression_count <= 100000 && (!expression_count || expressions), "Invalid expression buffer.");
    Require(output_count >= 0 && output_count <= 128 && (!output_count || outputs), "Invalid output buffer.");
    for (int i = 0; i < output_count; ++i) {
      Authoring::Flag(outputs[i].remove_boundaries); Authoring::Flag(outputs[i].containers);
      Require(outputs[i].reserved1 == 0 && outputs[i].reserved2 == 0, "Reserved output fields must be zero.");
    }
    for (int i = 0; i < rule_count; ++i) {
      const auto& rule = rules[i];
      Require(rule.output >= 0 && rule.output < output_count && rule.action >= 0 && rule.action <= 1
        && rule.material >= 0 && rule.dimension >= -1 && rule.dimension <= 3
        && std::isfinite(rule.maximum_measure) && rule.maximum_measure >= -1,
        "Invalid region selection rule.");
      Require(rule.expression_offset >= 0 && rule.expression_count > 0
        && rule.expression_offset <= expression_count - rule.expression_count, "Expression slice is out of range.");
      ValidateExpression(expressions + rule.expression_offset, rule.expression_count, input_count);
    }
    Authoring::InputGraph graph(inputs, input_count);
    for (const auto& shape : graph.Shapes) RequireExactFaceSupport(shape);
    auto owner = std::make_unique<OcctSharp_FeatureResultHandle>();
    owner->Regions = std::make_shared<OcctSharp_RegionData>(); auto& data = *owner->Regions;
    if (options->check_inputs && !CheckInputs(graph.Shapes, *owner)) { Publish(std::move(owner), result); return; }
    NCollection_List<TopoDS_Shape> arguments; for (const auto& shape : graph.Shapes) arguments.Append(shape);
    // General Fuse requires two argument identities. For a single input, the second
    // argument is a container alias of exactly the same copied topology, not new
    // geometry. Fold that explicit alias back into input zero's membership below.
    TopoDS_Compound singleAlias;
    if (input_count == 1) {
      BRep_Builder builder; builder.MakeCompound(singleAlias); builder.Add(singleAlias, graph.Shapes[0]);
      arguments.Append(singleAlias);
    }
    Cells algorithm; algorithm.SetArguments(arguments); algorithm.SetNonDestructive(true);
    algorithm.SetRunParallel(options->parallel != 0); algorithm.SetFuzzyValue(options->fuzzy); algorithm.Perform();
    if (algorithm.HasErrors()) {
      std::ostringstream errors; algorithm.DumpErrors(errors); owner->Message = errors.str();
      Publish(std::move(owner), result); return;
    }
    std::vector<TopoDS_Shape> cells;
    std::vector<std::vector<bool>> membership;
    std::vector<bool> known;
    std::vector<double> measures;
    for (TopoDS_Iterator it(algorithm.GetAllParts()); it.More(); it.Next()) {
      Require(static_cast<int>(cells.size()) < options->max_cells, "Partition cell budget exceeded.");
      const auto& cell = it.Value(); const int index = static_cast<int>(cells.size()); const int dimension = Dimension(cell);
      Require(dimension >= 0, "Unexpected non-basic partition part.");
      cells.push_back(cell); membership.emplace_back(input_count, false);
      const auto* origins = algorithm.Origins(cell); bool mapped = origins && !origins->IsEmpty();
      if (origins) for (const auto& origin : *origins) {
        bool found = false;
        for (int input = 0; input < input_count; ++input) if (origin.IsSame(graph.Shapes[input])) {
          membership.back()[input] = true; found = true;
        }
        if (input_count == 1 && origin.IsSame(singleAlias)) { membership.back()[0] = true; found = true; }
        mapped = mapped && found;
      }
      known.push_back(mapped); measures.push_back(Measure(cell, dimension));
      Add(data, Cell, index, dimension, static_cast<int>(cell.ShapeType()), -1,
        mapped ? 1 : 0, measures.back(), cell);
      for (int input = 0; input < input_count; ++input)
        Add(data, Membership, index, input, mapped ? membership.back()[input] ? 1 : 0 : -1);
    }
    InspectBoundaries(cells, data);
    for (int input = 0; input < input_count; ++input) for (int dim = 0; dim <= 3; ++dim) {
      const auto kind = dim == 0 ? TopAbs_VERTEX : dim == 1 ? TopAbs_EDGE : dim == 2 ? TopAbs_FACE : TopAbs_SOLID;
      ShapeMap components; TopExp::MapShapes(graph.Shapes[input], kind, components);
      double original = 0; for (const auto& component : components) original += Measure(component, dim);
      double split = 0; for (size_t i = 0; i < cells.size(); ++i)
        if (Dimension(cells[i]) == dim && membership[i][input]) split += measures[i];
      // Conservation applies to basic input dimension, not every embedded boundary.
      if (Dimension(graph.Shapes[input]) == dim) {
        Add(data, InputMeasure, input, dim, 0, -1, 0, original);
        Add(data, InputMeasure, input, dim, 1, -1, 0, split);
      }
    }
    for (int output = 0; output < output_count; ++output) {
      std::vector<int> materials(cells.size(), -1);
      for (int r = 0; r < rule_count; ++r) {
        const auto& rule = rules[r]; if (rule.output != output) continue;
        for (int i = 0; i < static_cast<int>(cells.size()); ++i) {
          Require(known[i], "Selection cannot guess unknown cell membership.");
          if (rule.dimension >= 0 && Dimension(cells[i]) != rule.dimension) continue;
          if (rule.maximum_measure >= 0 && measures[i] > rule.maximum_measure) continue;
          if (!Evaluate(expressions + rule.expression_offset, rule.expression_count, membership[i])) continue;
          const int before = materials[i];
          if (rule.action == 0) {
            Require(before < 0 || before == rule.material, "Conflicting material assignments to the same cell.");
            materials[i] = rule.material;
          } else materials[i] = -1;
          Add(data, RuleEffect, output, r, i, before, materials[i]);
        }
      }
      if (outputs[output].remove_boundaries) {
        std::map<int, int> dimensions;
        for (size_t i = 0; i < cells.size(); ++i) if (materials[i] > 0) {
          const auto [at, inserted] = dimensions.emplace(materials[i], Dimension(cells[i]));
          Require(inserted || at->second == Dimension(cells[i]), "Internal-boundary removal across dimensions is unsupported.");
        }
      }
      algorithm.Select(cells, materials);
      if (outputs[output].remove_boundaries) algorithm.RemoveInternalBoundaries();
      if (outputs[output].containers) algorithm.MakeContainers();
      if (algorithm.HasErrors() || (outputs[output].remove_boundaries && algorithm.HasWarnings())) {
        std::ostringstream message; algorithm.DumpErrors(message); algorithm.DumpWarnings(message);
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, ("Required region finalization failed: " + message.str()).c_str());
      }
      const bool valid = BRepCheck_Analyzer(algorithm.Shape()).IsValid();
      Require(valid, "Required region output is invalid; no partial output is published.");
      Add(data, Output, output, outputs[output].containers, outputs[output].remove_boundaries, -1, 1, 0, algorithm.Shape());
      for (int i = 0; i < static_cast<int>(cells.size()); ++i) if (materials[i] >= 0)
        Add(data, Assignment, output, i, materials[i], Dimension(cells[i]), 0, measures[i]);
      CopyHistory(algorithm, graph.Shapes, output, data);
    }
    owner->Result = algorithm.GetAllParts(); data.Info.done = 1;
    data.Info.valid = BRepCheck_Analyzer(owner->Result).IsValid();
    data.Info.cell_count = static_cast<int>(cells.size()); data.Info.output_count = output_count;
    data.Info.warnings = algorithm.HasWarnings();
    std::ostringstream message; algorithm.DumpWarnings(message); owner->Message = message.str();
    Publish(std::move(owner), result);
  });
}
