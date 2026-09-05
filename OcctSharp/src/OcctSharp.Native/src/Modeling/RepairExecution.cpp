#include "Modeling/Repair.hxx"
#include "Modeling/Topology.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include <BRepTools.hxx>
#include <BRep_Builder.hxx>
#include <TopoDS_Compound.hxx>
#include <cmath>
#include <sstream>
#include <unordered_set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Repair;
namespace {
std::string Fingerprint(const TopoDS_Shape& shape) {
  std::ostringstream stream; BRepTools::Write(shape, stream, false, false, TopTools_FormatVersion_CURRENT);
  return stream.str();
}
void ValidateIndices(const int* values, int count, int maximum) {
  Require(count >= 0 && count <= maximum && (count == 0 || values), "Invalid topology selection buffer.");
  std::unordered_set<int> unique;
  for (int i = 0; i < count; ++i)
    Require(values[i] >= 0 && values[i] < maximum && unique.insert(values[i]).second,
      "Topology selections must be unique and within the source snapshot.");
}
TopoDS_Shape NonNull(TopoDS_Shape value) {
  if (!value.IsNull()) return value;
  TopoDS_Compound empty; BRep_Builder().MakeCompound(empty); return empty;
}
std::vector<std::pair<TopoDS_Shape, int>> Relations(const TopoDS_Shape& from, const Outcome& outcome) {
  std::vector<std::pair<TopoDS_Shape, int>> result;
  if (!outcome.History.IsNull() && SupportsShapeHistory(from)) {
    for (const auto& item : outcome.History->Modified(from)) result.emplace_back(item, 1);
    for (const auto& item : outcome.History->Generated(from)) result.emplace_back(item, 2);
    if (!result.empty()) return result;
    if (outcome.History->IsRemoved(from)) { result.emplace_back(TopoDS_Shape(), 3); return result; }
  }
  if (!outcome.Context.IsNull()) {
    if (outcome.Context->IsRecorded(from) && outcome.Context->Value(from).IsNull()) {
      result.emplace_back(TopoDS_Shape(), 3); return result;
    }
    auto changed = outcome.Context->Apply(from);
    if (!changed.IsNull() && !changed.IsEqual(from)) {
      result.emplace_back(changed, 1); return result;
    }
  }
  result.emplace_back(from, 0); return result;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_execute(
  const OcctSharp_ShapeHandle* source, const OcctSharp_RepairStage* stage,
  const int32_t* selected, int32_t selected_count, const int32_t* protected_indices, int32_t protected_count,
  const OcctSharp_ShapeHandle* const* replacements, int32_t replacement_count,
  OcctSharp_RepairResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateUsableShape(source); Require(stage && output, "Missing repair stage or output.");
    Require(stage->operation >= 0 && stage->operation <= 19, "Unknown repair operation.");
    Positive(stage->tolerance, "Repair tolerance must be positive.");
    Positive(stage->maximum_tolerance, "Maximum repair tolerance must be positive.");
    Require(stage->maximum_tolerance >= stage->tolerance && stage->maximum_topology > 0
      && stage->maximum_topology <= 1000000, "Invalid repair tolerance or growth limit.");
    Require(std::isfinite(stage->threshold) && std::isfinite(stage->angle), "Non-finite repair options.");
    const auto flag = [](int value) { return value == 0 || value == 1; };
    const auto control = [](int value) { return value >= -1 && value <= 1; };
    switch (stage->operation) {
      case 0: Require(flag(stage->mode1) && flag(stage->mode2), "Invalid reorder flags."); break;
      case 1: case 3: case 5: Require(flag(stage->mode1), "Invalid repair Boolean mode."); break;
      case 4: Require(control(stage->mode1) && control(stage->mode2) && control(stage->mode3), "Invalid face repair controls."); break;
      case 6: Require(control(stage->mode1), "Invalid solid repair control."); break;
      case 10: Require(flag(stage->mode1) && flag(stage->mode2), "Invalid sewing flags."); break;
      case 18: Require(flag(stage->mode1) && flag(stage->mode2) && flag(stage->mode3)
        && (stage->mode1 || stage->mode2), "Invalid unification flags."); break;
    }
    auto sourceMap = Map(source->Value);
    Require(sourceMap.Extent() <= stage->maximum_topology, "Source exceeds the repair topology resource bound.");
    ValidateIndices(selected, selected_count, sourceMap.Extent());
    ValidateIndices(protected_indices, protected_count, sourceMap.Extent());
    Require(replacement_count >= 0 && (replacement_count == 0 || replacements), "Invalid replacements.");
    Require(stage->operation == 19 ? (selected_count > 0 && replacement_count == selected_count)
      : replacement_count == 0, "Only an explicit edit stage accepts one replacement slot per selection.");
    if (stage->operation == 19) {
      for (int i = 0; i < replacement_count; ++i) {
        if (!replacements[i]) continue; ValidateUsableShape(replacements[i]);
        Require(replacements[i]->Value.ShapeType() == sourceMap(selected[i] + 1).ShapeType(), "Replacement topology kinds differ.");
        auto replacementMap = Map(replacements[i]->Value);
        for (int j = 0; j < selected_count; ++j)
          Require(!replacementMap.Contains(sourceMap(selected[j] + 1)), "Replacement edit cycle or conflicting target.");
      }
    }
    std::vector<TopoDS_Shape> copies; auto privateSource = Copy(source->Value, &copies);
    std::vector<TopoDS_Shape> scopes, protectedShapes;
    std::vector<std::string> before;
    before.reserve(copies.size()); for (const auto& copy : copies) before.push_back(Fingerprint(copy));
    if (!selected_count) scopes.push_back(privateSource);
    else for (int i = 0; i < selected_count; ++i) scopes.push_back(copies[selected[i]]);
    for (size_t i = 0; i < scopes.size(); ++i) {
      auto closure = Map(scopes[i]);
      for (size_t j = 0; j < scopes.size(); ++j)
        Require(i == j || !closure.Contains(scopes[j]), "Nested/conflicting selections are not permitted.");
    }
    for (int i = 0; i < protected_count; ++i) protectedShapes.push_back(copies[protected_indices[i]]);
    std::vector<TopoDS_Shape> requestedScopes = scopes, selectedHoles;
    if (stage->operation == 11 && selected_count && scopes[0].ShapeType() == TopAbs_WIRE) {
      for (const auto& scope : scopes) Require(scope.ShapeType() == TopAbs_WIRE, "Do not mix faces and internal wires in one hole selection.");
      selectedHoles = scopes; scopes = {privateSource};
    }
    auto result = std::make_unique<OcctSharp_RepairResultHandle>();
    occ::handle<ShapeBuild_ReShape> combined = new ShapeBuild_ReShape();
    std::vector<Outcome> outcomes;
    std::vector<ShapeMap> scopeMaps;
    for (size_t i = 0; i < scopes.size(); ++i) {
      const auto& scope = scopes[i]; Outcome outcome;
      if (stage->operation == 19) {
        outcome.Context = new ShapeBuild_ReShape();
        if (replacements[i]) { outcome.Shape = Copy(replacements[i]->Value); outcome.Context->Replace(scope, outcome.Shape); }
        else { outcome.Context->Remove(scope); outcome.Shape = NonNull({}); }
      } else if (stage->operation <= 9) outcome = Fix(scope, *stage);
      else if (stage->operation == 10) outcome = Sew(scope, *stage);
      else outcome = Normalize(scope, *stage, protectedShapes, selectedHoles);
      outcome.Shape = NonNull(outcome.Shape);
      Require(Map(outcome.Shape).Extent() <= stage->maximum_topology, "Repair exceeded the topology growth bound.");
      if (stage->operation == 19 && !replacements[i]) combined->Remove(scope);
      else if (!outcome.Shape.IsEqual(scope)) combined->Replace(scope, outcome.Shape);
      scopeMaps.push_back(Map(scope)); outcomes.push_back(std::move(outcome));
    }
    result->Shape = NonNull(combined->Apply(privateSource)); auto afterMap = Map(result->Shape);
    Require(afterMap.Extent() <= stage->maximum_topology, "Repair exceeded the total topology bound.");
    for (size_t i = 0; i < copies.size(); ++i) {
      bool reported = false;
      for (size_t scopeIndex = 0; scopeIndex < scopes.size(); ++scopeIndex) {
        if (!scopeMaps[scopeIndex].Contains(copies[i])) continue;
        for (const auto& [target, kind] : Relations(copies[i], outcomes[scopeIndex])) {
          if (kind == 3) { result->History.push_back({int(i), -1, 3, 0}); reported = true; continue; }
          auto targets = Map(target);
          for (const auto& candidate : targets) {
            // Container splits use same-kind pieces; position and cardinality never
            // establish persistent identity across an algorithm.
            if (candidate.ShapeType() != copies[i].ShapeType()) continue;
            int index = afterMap.FindIndex(candidate);
            if (index) {
              int actual = kind == 0 && before[i] != Fingerprint(candidate) ? 1 : kind;
              result->History.push_back({int(i), index - 1, actual, 0}); reported = true;
            }
          }
        }
      }
      if (!reported) {
        auto mapped = combined->Apply(copies[i]); int index = mapped.IsNull() ? 0 : afterMap.FindIndex(mapped);
        if (index) {
          int kind = before[i] == Fingerprint(mapped) ? 0 : 1;
          result->History.push_back({int(i), index - 1, kind, 0});
        } else result->History.push_back({int(i), -1, 4, 0});
      }
    }
    for (size_t i = 0; i < copies.size(); ++i) {
      bool inScope = false, ancestor = false;
      auto closure = Map(copies[i]);
      for (const auto& scope : requestedScopes) {
        inScope |= Map(scope).Contains(copies[i]); ancestor |= closure.Contains(scope);
      }
      if (selected_count && !inScope && !ancestor) {
        int index = afterMap.FindIndex(copies[i]);
        if (!index || before[i] != Fingerprint(afterMap(index))) {
          auto message = "Repair would alter topology outside the selected closure at index " + std::to_string(i)
            + " (kind " + std::to_string(copies[i].ShapeType()) + ").";
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message.c_str());
        }
      }
    }
    for (int i = 0; i < protected_count; ++i) {
      int index = protected_indices[i], matches = 0;
      for (const auto& relation : result->History) if (relation.source_index == index && relation.result_index >= 0) {
        ++matches; Require(before[index] == Fingerprint(afterMap(relation.result_index + 1)), "A protected subshape would change.");
      }
      Require(matches == 1, "Protected topology would be removed, split, merged or lose verified correspondence.");
    }
    for (size_t i = 0; i < outcomes.size(); ++i) {
      auto inputMap = Map(scopes[i]), outputMap = Map(outcomes[i].Shape);
      for (auto finding : outcomes[i].Findings) {
        bool resultIndex = finding.kind >= 12 && finding.kind <= 16;
        auto& local = resultIndex ? outputMap : inputMap;
        const auto remap = [&](int index) {
          if (index < 0 || index >= local.Extent()) return -1;
          const auto& found = local(index + 1);
          if (resultIndex) return afterMap.FindIndex(found) - 1;
          for (size_t j = 0; j < copies.size(); ++j) if (copies[j].IsSame(found)) return int(j);
          return -1;
        };
        finding.source_index = remap(finding.source_index);
        finding.related_index = remap(finding.related_index);
        result->Findings.push_back(finding);
      }
    }
    auto* raw = result.get(); RegisterValue(raw, LiveRepairResults); result.release(); *output = raw;
  });
}
