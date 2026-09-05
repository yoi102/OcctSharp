// Native Modeling/Features implementation. Public contracts and ownership are unchanged.
#include "Modeling/Features.hxx"
#include "Modeling/Topology.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BOPAlgo_ArgumentAnalyzer.hxx>
#include <BOPAlgo_CellsBuilder.hxx>
#include <BOPAlgo_GlueEnum.hxx>
#include <BRepAlgoAPI_BooleanOperation.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Defeaturing.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepAlgoAPI_Splitter.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepFilletAPI_MakeFillet2d.hxx>
#include <BRepOffsetAPI_DraftAngle.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <NCollection_List.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <Standard_Failure.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <cstring>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>
#include <gp_XYZ.hxx>
#include <memory>
#include <mutex>
#include <string>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
OcctSharp_FeatureResultHandle* AllocateFeatureResult()
{
  auto* result = new OcctSharp_FeatureResultHandle();
  try
  {
    std::lock_guard<std::mutex> lock(LiveShapesMutex);
    LiveFeatureResults.insert(result);
    return result;
  }
  catch (...)
  {
    delete result;
    throw;
  }
}

OcctSharp_FeatureResultHandle* RegisterFeatureResult(
  std::unique_ptr<OcctSharp_FeatureResultHandle> result)
{
  OcctSharp_FeatureResultHandle* raw = result.get();
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveFeatureResults.insert(raw);
  result.release();
  return raw;
}

bool IsLiveFeatureResult(const OcctSharp_FeatureResultHandle* result)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveFeatureResults.contains(result);
}

bool UnregisterFeatureResult(const OcctSharp_FeatureResultHandle* result)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveFeatureResults.erase(result) != 0;
}

void ValidateFeatureResult(const OcctSharp_FeatureResultHandle* result)
{
  if (result == nullptr)
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The feature result handle is null.");
  if (!IsLiveFeatureResult(result))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The feature result handle is invalid or already released.");
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_execute(
  const int32_t operation,
  const OcctSharp_ShapeHandle* const* shapes, const int32_t shape_count,
  const int32_t primary_count, const int32_t secondary_count,
  const double* parameters, const int32_t parameter_count,
  const OcctSharp_Xyz* vectors, const int32_t vector_count,
  const OcctSharp_FeatureOptions options,
  OcctSharp_FeatureResultHandle** out_result)
{
  if (out_result == nullptr)
  {
    SetLastError("The feature result output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_result = nullptr;
  if (operation < 0 || operation > 21 || shape_count < 1 || shapes == nullptr
      || primary_count < 0 || secondary_count < 0 || parameter_count < 0
      || vector_count < 0 || (parameter_count > 0 && parameters == nullptr)
      || (vector_count > 0 && vectors == nullptr))
  {
    SetLastError("The feature operation request is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  if (!std::isfinite(options.fuzzy_tolerance) || options.fuzzy_tolerance < 0.0
      || options.glue_mode < 0 || options.glue_mode > 2)
  {
    SetLastError("Feature fuzzy tolerance and glue mode are invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    auto snapshot = std::make_unique<OcctSharp_FeatureResultHandle>();
    snapshot->Info.operation = operation;
    std::vector<TopoDS_Shape> inputs;
    inputs.reserve(static_cast<size_t>(shape_count));
    for (int32_t index = 0; index < shape_count; ++index)
    {
      ValidateUsableShape(shapes[index]);
      TopoDS_Shape value = shapes[index]->Value;
      if (options.repair_inputs != 0 && !BRepCheck_Analyzer(value).IsValid())
      {
        ShapeFix_Shape fixer(value);
        fixer.Perform();
        if (!fixer.Shape().IsNull()) value = fixer.Shape();
        snapshot->Info.recovered = 1;
      }
      inputs.push_back(std::move(value));
    }

    const auto requireShapes = [&](const int32_t minimum, const char* message)
    {
      if (shape_count < minimum) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
    };
    const auto parameter = [&](const int32_t index) -> double
    {
      if (index < 0 || index >= parameter_count || !std::isfinite(parameters[index]))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A required feature parameter is missing or non-finite.");
      return parameters[index];
    };
    const auto xyz = [&](const int32_t index) -> gp_XYZ
    {
      if (index < 0 || index >= vector_count
          || !std::isfinite(vectors[index].x) || !std::isfinite(vectors[index].y)
          || !std::isfinite(vectors[index].z))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A required feature vector is missing or non-finite.");
      return gp_XYZ(vectors[index].x, vectors[index].y, vectors[index].z);
    };
    const auto fail = [&](const std::string& message, const int32_t errors = 1)
    {
      snapshot->Info.succeeded = 0;
      snapshot->Info.error_count = std::max(errors, 1);
      snapshot->Message = message;
    };
    const auto finalize = [&](TopoDS_Shape result, const std::string& stage)
    {
      if (result.IsNull())
      {
        fail(stage + " produced a null result.");
        return;
      }
      if (options.unify_result != 0)
      {
        ShapeUpgrade_UnifySameDomain unify(result, true, true, true);
        unify.Build();
        if (!unify.Shape().IsNull()) result = unify.Shape();
      }
      snapshot->Result = std::move(result);
      snapshot->Info.succeeded = 1;
      snapshot->Info.result_is_valid = BRepCheck_Analyzer(snapshot->Result).IsValid() ? 1 : 0;
      snapshot->Message = stage;
    };
    const auto captureHistory = [&](auto& builder, const std::vector<int32_t>& source_indices)
    {
      for (const int32_t source_index : source_indices)
      {
        if (source_index < 0 || source_index >= shape_count) continue;
        const TopoDS_Shape& source = inputs[static_cast<size_t>(source_index)];
        std::vector<TopoDS_Shape> candidates{source};
        for (int kind = static_cast<int>(TopAbs_COMPOUND); kind <= static_cast<int>(TopAbs_VERTEX); ++kind)
        {
          for (TopExp_Explorer explorer(source, static_cast<TopAbs_ShapeEnum>(kind)); explorer.More(); explorer.Next())
          {
            const TopoDS_Shape& candidate = explorer.Current();
            if (std::none_of(candidates.begin(), candidates.end(), [&](const TopoDS_Shape& existing)
              { return existing.IsSame(candidate); }))
              candidates.push_back(candidate);
          }
        }
        std::vector<TopoDS_Shape> modifiedSeen;
        std::vector<TopoDS_Shape> generatedSeen;
        const auto appendHistory = [&](const int32_t kind, const TopoDS_Shape& value,
          std::vector<TopoDS_Shape>& seen)
        {
          if (std::none_of(seen.begin(), seen.end(), [&](const TopoDS_Shape& existing)
            { return existing.IsSame(value); }))
          {
            seen.push_back(value);
            snapshot->History.push_back({source_index, kind, value});
          }
        };
        for (const TopoDS_Shape& candidate : candidates)
        {
          // BRepTools_History rejects container types with a CRT assertion in
          // Debug OCCT; this cannot be caught as a Standard_Failure exception.
          if (!SupportsShapeHistory(candidate)) continue;
          try
          {
            const auto& modified = builder.Modified(candidate);
            for (NCollection_List<TopoDS_Shape>::Iterator it(modified); it.More(); it.Next())
              appendHistory(0, it.Value(), modifiedSeen);
          }
          catch (const Standard_Failure&)
          {
            // Some OCCT builders throw when topology is absent from their history map.
          }
          try
          {
            const auto& generated = builder.Generated(candidate);
            for (NCollection_List<TopoDS_Shape>::Iterator it(generated); it.More(); it.Next())
              appendHistory(1, it.Value(), generatedSeen);
          }
          catch (const Standard_Failure&)
          {
            // An absent history-map key is equivalent to an empty generated list.
          }
        }
        try
        {
          if (SupportsShapeHistory(source) && builder.IsDeleted(source)) snapshot->Deleted.push_back(source_index);
        }
        catch (const Standard_Failure&)
        {
          // Builders without a deletion entry report no deletion for this request.
        }
      }
    };
    const auto applyBopOptions = [&](auto& builder)
    {
      builder.SetRunParallel(options.run_parallel != 0);
      builder.SetFuzzyValue(options.fuzzy_tolerance);
      builder.SetNonDestructive(options.non_destructive != 0);
      builder.SetGlue(static_cast<BOPAlgo_GlueEnum>(options.glue_mode));
    };

    if (operation == 0 || operation == 1)
    {
      requireShapes(2, "Selected fillet requires a source and at least one edge.");
      const double first = parameter(0);
      const double second = operation == 1 ? parameter(1) : first;
      if (first <= 0.0 || second <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Fillet radii must be greater than zero.");
      BRepFilletAPI_MakeFillet builder(inputs[0]);
      std::vector<int32_t> sources{0};
      for (int32_t index = 1; index < shape_count; ++index)
      {
        if (inputs[index].ShapeType() != TopAbs_EDGE)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Every fillet selection must be an edge.");
        builder.Add(first, second, TopoDS::Edge(inputs[index]));
        sources.push_back(index);
      }
      builder.Build();
      if (!builder.IsDone()) fail("OCCT selected fillet construction did not complete.");
      else { captureHistory(builder, sources); finalize(builder.Shape(), operation == 0 ? "selected fillet" : "variable fillet"); }
    }
    else if (operation == 2 || operation == 3)
    {
      requireShapes(2, "Selected chamfer requires a source and edge selections.");
      const double first = parameter(0);
      const double second = operation == 3 ? parameter(1) : first;
      if (first <= 0.0 || second <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Chamfer distances must be greater than zero.");
      BRepFilletAPI_MakeChamfer builder(inputs[0]);
      std::vector<int32_t> sources{0};
      if (operation == 2)
      {
        for (int32_t index = 1; index < shape_count; ++index)
        {
          if (inputs[index].ShapeType() != TopAbs_EDGE)
            throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Every chamfer selection must be an edge.");
          builder.Add(first, TopoDS::Edge(inputs[index]));
          sources.push_back(index);
        }
      }
      else
      {
        if ((shape_count - 1) % 2 != 0)
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Two-distance chamfer requires edge/face pairs.");
        for (int32_t index = 1; index < shape_count; index += 2)
        {
          if (inputs[index].ShapeType() != TopAbs_EDGE || inputs[index + 1].ShapeType() != TopAbs_FACE)
            throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Two-distance chamfer selections must be edge/face pairs.");
          bool isSupport = false;
          for (TopExp_Explorer explorer(inputs[index + 1], TopAbs_EDGE); explorer.More(); explorer.Next())
          {
            if (explorer.Current().IsSame(inputs[index])) { isSupport = true; break; }
          }
          if (!isSupport)
            throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,
              "Each two-distance chamfer support face must contain its selected edge.");
          builder.Add(first, second, TopoDS::Edge(inputs[index]), TopoDS::Face(inputs[index + 1]));
          sources.push_back(index); sources.push_back(index + 1);
        }
      }
      builder.Build();
      if (!builder.IsDone()) fail("OCCT selected chamfer construction did not complete.");
      else { captureHistory(builder, sources); finalize(builder.Shape(), operation == 2 ? "selected chamfer" : "two-distance chamfer"); }
    }
    else if (operation == 4 || operation == 5)
    {
      requireShapes(operation == 4 ? 2 : 3, "Planar finishing requires a face and vertex or edge selections.");
      if (inputs[0].ShapeType() != TopAbs_FACE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar finishing requires a face source.");
      BRepFilletAPI_MakeFillet2d builder(TopoDS::Face(inputs[0]));
      std::vector<int32_t> sources{0};
      if (operation == 4)
      {
        const double radius = parameter(0);
        if (radius <= 0.0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Planar fillet radius must be greater than zero.");
        for (int32_t index = 1; index < shape_count; ++index)
        {
          if (inputs[index].ShapeType() != TopAbs_VERTEX)
            throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar fillet selections must be vertices.");
          builder.AddFillet(TopoDS::Vertex(inputs[index]), radius); sources.push_back(index);
        }
      }
      else
      {
        const double first = parameter(0), second = parameter(1);
        if (first <= 0.0 || second <= 0.0 || (shape_count - 1) % 2 != 0)
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Planar chamfer requires positive distances and edge pairs.");
        for (int32_t index = 1; index < shape_count; index += 2)
        {
          if (inputs[index].ShapeType() != TopAbs_EDGE || inputs[index + 1].ShapeType() != TopAbs_EDGE)
            throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar chamfer selections must be edge pairs.");
          builder.AddChamfer(TopoDS::Edge(inputs[index]), TopoDS::Edge(inputs[index + 1]), first, second);
          sources.push_back(index); sources.push_back(index + 1);
        }
      }
      builder.Build();
      if (!builder.IsDone()) fail("OCCT planar finishing did not complete.");
      else { captureHistory(builder, sources); finalize(builder.Shape(), operation == 4 ? "planar fillet" : "planar chamfer"); }
    }
    else if (operation == 6)
    {
      requireShapes(2, "Draft requires a source and selected faces.");
      const double angle = parameter(0);
      const gp_XYZ directionValue = xyz(0), originValue = xyz(1), normalValue = xyz(2);
      if (directionValue.SquareModulus() <= 0.0 || normalValue.SquareModulus() <= 0.0 || angle == 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Draft direction, plane normal, and angle must be non-zero.");
      BRepOffsetAPI_DraftAngle builder(inputs[0]);
      std::vector<int32_t> sources{0};
      const gp_Dir direction(directionValue); const gp_Pln plane{gp_Pnt(originValue), gp_Dir(normalValue)};
      for (int32_t index = 1; index < shape_count; ++index)
      {
        if (inputs[index].ShapeType() != TopAbs_FACE)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Draft selections must be faces.");
        builder.Add(TopoDS::Face(inputs[index]), direction, angle, plane, true);
        if (!builder.AddDone()) { fail("OCCT could not add a selected draft face."); break; }
        sources.push_back(index);
      }
      if (snapshot->Info.error_count == 0)
      {
        builder.Build();
        if (!builder.IsDone()) fail("OCCT selected draft construction did not complete.");
        else { captureHistory(builder, sources); finalize(builder.Shape(), "selected draft"); }
      }
    }
    else if (operation == 7 || operation == 8)
    {
      requireShapes(2, "Linear local feature requires a base and profile.");
      const gp_XYZ directionValue = xyz(0);
      if (directionValue.SquareModulus() <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Linear feature direction must be non-zero.");
      BRepPrimAPI_MakePrism prism(inputs[1], gp_Vec(directionValue), false, false);
      if (!prism.IsDone() || prism.Shape().IsNull()) fail("OCCT could not construct the linear feature tool.");
      else
      {
        std::unique_ptr<BRepAlgoAPI_BooleanOperation> builder;
        if (operation == 7) builder = std::make_unique<BRepAlgoAPI_Fuse>(inputs[0], prism.Shape());
        else builder = std::make_unique<BRepAlgoAPI_Cut>(inputs[0], prism.Shape());
        applyBopOptions(*builder); builder->Build();
        if (!builder->IsDone()) fail(operation == 7 ? "OCCT boss construction did not complete." : "OCCT pocket construction did not complete.");
        else { captureHistory(*builder, {0}); finalize(builder->Shape(), operation == 7 ? "prismatic boss" : "prismatic pocket"); }
      }
    }
    else if (operation == 9)
    {
      requireShapes(1, "Hole construction requires a base shape.");
      const double radius = parameter(0), depth = parameter(1);
      const gp_XYZ originValue = xyz(0), directionValue = xyz(1);
      if (radius <= 0.0 || depth <= 0.0 || directionValue.SquareModulus() <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Hole radius, depth, and direction must be positive/non-zero.");
      BRepPrimAPI_MakeCylinder cylinder(gp_Ax2(gp_Pnt(originValue), gp_Dir(directionValue)), radius, depth);
      BRepAlgoAPI_Cut builder(inputs[0], cylinder.Shape()); applyBopOptions(builder); builder.Build();
      if (!builder.IsDone()) fail("OCCT hole construction did not complete.");
      else { captureHistory(builder, {0}); finalize(builder.Shape(), secondary_count != 0 ? "through hole" : "blind hole"); }
    }
    else if (operation == 10 || operation == 11)
    {
      requireShapes(2, "Revolved local feature requires a base and profile.");
      const double angle = parameter(0);
      const gp_XYZ originValue = xyz(0), directionValue = xyz(1);
      if (angle == 0.0 || directionValue.SquareModulus() <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Revolved feature angle and axis must be non-zero.");
      BRepPrimAPI_MakeRevol revol(inputs[1], gp_Ax1(gp_Pnt(originValue), gp_Dir(directionValue)), angle, false);
      std::unique_ptr<BRepAlgoAPI_BooleanOperation> builder;
      if (operation == 10) builder = std::make_unique<BRepAlgoAPI_Fuse>(inputs[0], revol.Shape());
      else builder = std::make_unique<BRepAlgoAPI_Cut>(inputs[0], revol.Shape());
      applyBopOptions(*builder); builder->Build();
      if (!builder->IsDone()) fail("OCCT revolved local feature did not complete.");
      else { captureHistory(*builder, {0}); finalize(builder->Shape(), operation == 10 ? "additive revolved feature" : "subtractive revolved feature"); }
    }
    else if (operation == 12 || operation == 13)
    {
      requireShapes(3, "Pipe local feature requires a base, spine, and profile.");
      if (inputs[1].ShapeType() != TopAbs_WIRE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pipe local feature requires a wire spine.");
      BRepOffsetAPI_MakePipe pipe(TopoDS::Wire(inputs[1]), inputs[2]);
      if (!pipe.IsDone() || pipe.Shape().IsNull()) fail("OCCT could not construct the pipe feature tool.");
      else
      {
        std::unique_ptr<BRepAlgoAPI_BooleanOperation> builder;
        if (operation == 12) builder = std::make_unique<BRepAlgoAPI_Fuse>(inputs[0], pipe.Shape());
        else builder = std::make_unique<BRepAlgoAPI_Cut>(inputs[0], pipe.Shape());
        applyBopOptions(*builder); builder->Build();
        if (!builder->IsDone()) fail("OCCT pipe local feature did not complete.");
        else { captureHistory(*builder, {0}); finalize(builder->Shape(), operation == 12 ? "additive pipe feature" : "subtractive pipe feature"); }
      }
    }
    else if (operation == 14)
    {
      if (primary_count < 1 || primary_count >= shape_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Split requires arguments followed by tools.");
      NCollection_List<TopoDS_Shape> arguments, tools;
      std::vector<int32_t> sources;
      for (int32_t index = 0; index < primary_count; ++index) { arguments.Append(inputs[index]); sources.push_back(index); }
      for (int32_t index = primary_count; index < shape_count; ++index) tools.Append(inputs[index]);
      BRepAlgoAPI_Splitter builder; builder.SetArguments(arguments); builder.SetTools(tools); applyBopOptions(builder); builder.Build();
      if (!builder.IsDone()) fail("OCCT multi-argument split did not complete.");
      else { captureHistory(builder, sources); finalize(builder.Shape(), "multi-argument split"); }
    }
    else if (operation == 15)
    {
      requireShapes(2, "Defeaturing requires a source and selected faces.");
      BRepAlgoAPI_Defeaturing builder; builder.SetShape(inputs[0]);
      builder.SetRunParallel(options.run_parallel != 0); builder.SetToFillHistory(true);
      std::vector<int32_t> sources{0};
      for (int32_t index = 1; index < shape_count; ++index)
      {
        if (inputs[index].ShapeType() != TopAbs_FACE)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Defeaturing selections must be faces.");
        builder.AddFaceToRemove(inputs[index]); sources.push_back(index);
      }
      builder.Build();
      snapshot->Info.error_count = builder.HasErrors() ? 1 : 0;
      snapshot->Info.warning_count = builder.HasWarnings() ? 1 : 0;
      if (!builder.IsDone() || builder.HasErrors()) fail("OCCT defeaturing did not complete.");
      else { captureHistory(builder, sources); finalize(builder.Shape(), "defeaturing"); }
    }
    else if (operation == 16)
    {
      if (primary_count < 2 || primary_count > shape_count || secondary_count < 0
          || primary_count + secondary_count > shape_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Cell selection requires arguments followed by take and avoid lists.");
      NCollection_List<TopoDS_Shape> arguments, take, avoid;
      for (int32_t index = 0; index < primary_count; ++index) arguments.Append(inputs[index]);
      for (int32_t index = primary_count; index < primary_count + secondary_count; ++index) take.Append(inputs[index]);
      for (int32_t index = primary_count + secondary_count; index < shape_count; ++index) avoid.Append(inputs[index]);
      BOPAlgo_CellsBuilder builder; builder.SetArguments(arguments); builder.SetRunParallel(options.run_parallel != 0);
      builder.SetFuzzyValue(options.fuzzy_tolerance); builder.SetNonDestructive(options.non_destructive != 0);
      builder.SetGlue(static_cast<BOPAlgo_GlueEnum>(options.glue_mode)); builder.Perform();
      if (builder.HasErrors()) fail("OCCT Boolean cell decomposition did not complete.");
      else
      {
        if (take.IsEmpty()) builder.AddAllToResult(parameter_count > 0 ? static_cast<int>(parameter(0)) : 0, false);
        else builder.AddToResult(take, avoid, parameter_count > 0 ? static_cast<int>(parameter(0)) : 0, false);
        if (parameter_count > 0 && parameter(0) != 0.0) builder.RemoveInternalBoundaries();
        snapshot->Info.warning_count = builder.HasWarnings() ? 1 : 0;
        std::vector<int32_t> sources; for (int32_t index = 0; index < primary_count; ++index) sources.push_back(index);
        captureHistory(builder, sources); finalize(builder.Shape(), "Boolean cell selection");
      }
    }
    else if (operation >= 17 && operation <= 20)
    {
      if (primary_count < 1 || primary_count > shape_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Batch Boolean requires one or more arguments and optional tools.");
      if (operation != 17 && primary_count == shape_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Cut/common/section require at least one tool.");
      NCollection_List<TopoDS_Shape> arguments, tools;
      std::vector<int32_t> sources;
      for (int32_t index = 0; index < primary_count; ++index) { arguments.Append(inputs[index]); sources.push_back(index); }
      for (int32_t index = primary_count; index < shape_count; ++index) { tools.Append(inputs[index]); sources.push_back(index); }
      std::unique_ptr<BRepAlgoAPI_BooleanOperation> builder;
      if (operation == 17) builder = std::make_unique<BRepAlgoAPI_Fuse>();
      else if (operation == 18) builder = std::make_unique<BRepAlgoAPI_Cut>();
      else if (operation == 19) builder = std::make_unique<BRepAlgoAPI_Common>();
      else builder = std::make_unique<BRepAlgoAPI_Section>();
      builder->SetArguments(arguments); builder->SetTools(tools); applyBopOptions(*builder); builder->Build();
      if (!builder->IsDone()) fail("OCCT batch Boolean operation did not complete.");
      else { captureHistory(*builder, sources); finalize(builder->Shape(), "batch Boolean"); }
    }
    else
    {
      requireShapes(1, "Boolean preflight requires at least one shape.");
      BOPAlgo_ArgumentAnalyzer analyzer;
      analyzer.SetShape1(inputs[0]); if (shape_count > 1) analyzer.SetShape2(inputs[1]);
      analyzer.OperationType() = primary_count >= 0 && primary_count <= 4
        ? static_cast<BOPAlgo_Operation>(primary_count) : BOPAlgo_UNKNOWN;
      analyzer.StopOnFirstFaulty() = false;
      analyzer.ArgumentTypeMode() = true; analyzer.SelfInterMode() = true;
      analyzer.SmallEdgeMode() = true; analyzer.RebuildFaceMode() = true;
      analyzer.TangentMode() = true; analyzer.MergeVertexMode() = true;
      analyzer.MergeEdgeMode() = true; analyzer.ContinuityMode() = true;
      analyzer.CurveOnSurfaceMode() = true; analyzer.Perform();
      snapshot->Result = inputs[0]; snapshot->Info.succeeded = 1;
      snapshot->Info.faulty_shape_count = analyzer.GetCheckResult().Size();
      snapshot->Info.result_is_valid = BRepCheck_Analyzer(snapshot->Result).IsValid() ? 1 : 0;
      snapshot->Message = analyzer.HasFaulty() ? "Boolean preflight found one or more risks." : "Boolean preflight passed.";
    }

    snapshot->Info.modified_count = static_cast<int32_t>(std::count_if(
      snapshot->History.begin(), snapshot->History.end(), [](const auto& item) { return item.Kind == 0; }));
    snapshot->Info.generated_count = static_cast<int32_t>(snapshot->History.size()) - snapshot->Info.modified_count;
    snapshot->Info.deleted_count = static_cast<int32_t>(snapshot->Deleted.size());
    *out_result = RegisterFeatureResult(std::move(snapshot));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_info(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_FeatureResultInfo* out_info)
{
  if (out_info == nullptr) { SetLastError("The feature result info pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_info = {};
  return Guard([&] { ValidateFeatureResult(result); *out_info = result->Info; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_shape(
  const OcctSharp_FeatureResultHandle* result, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The feature result shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFeatureResult(result);
    if (!result->Result.IsNull()) *out_shape = AllocateShape(result->Result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_history(
  const OcctSharp_FeatureResultHandle* result, const int32_t index,
  OcctSharp_FeatureHistoryInfo* out_info, OcctSharp_ShapeHandle** out_shape)
{
  if (out_info == nullptr || out_shape == nullptr)
  { SetLastError("A feature history output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_info = {}; *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFeatureResult(result);
    if (index < 0 || index >= static_cast<int32_t>(result->History.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The feature history index is out of range.");
    const auto& item = result->History[static_cast<size_t>(index)];
    *out_info = {item.SourceIndex, item.Kind}; *out_shape = AllocateShape(item.Shape);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_deleted(
  const OcctSharp_FeatureResultHandle* result, const int32_t index, int32_t* out_source_index)
{
  if (out_source_index == nullptr) { SetLastError("The deleted source index pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_source_index = 0;
  return Guard([&]
  {
    ValidateFeatureResult(result);
    if (index < 0 || index >= static_cast<int32_t>(result->Deleted.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The deleted source index is out of range.");
    *out_source_index = result->Deleted[static_cast<size_t>(index)];
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_feature_result_message(
  const OcctSharp_FeatureResultHandle* result, char* buffer, const int32_t capacity,
  int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The feature message written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    ValidateFeatureResult(result);
    const int32_t required = static_cast<int32_t>(result->Message.size()) + 1;
    *out_written = required;
    if (capacity == 0 && buffer == nullptr) return;
    ValidateOutputBuffer(buffer, capacity, required);
    std::memcpy(buffer, result->Message.c_str(), static_cast<size_t>(required));
  });
}

void OCCTSHARP_CALL occtsharp_feature_result_release(OcctSharp_FeatureResultHandle* result)
{
  if (result == nullptr || !UnregisterFeatureResult(result)) return;
  delete result;
}
