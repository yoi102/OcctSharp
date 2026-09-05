#include "Modeling/Regions.hxx"
#include "Modeling/GuidedAuthoring.hxx"
#include "Modeling/Topology.hxx"
#include <BOPAlgo_MakerVolume.hxx>
#include <BOPAlgo_ShellSplitter.hxx>
#include <BRepAlgoAPI_Check.hxx>
#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepClass3d_SolidClassifier.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS_Iterator.hxx>
#include <sstream>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Regions;
using OcctSharp::Native::Authoring::Require;

OcctSharp_Status OCCTSHARP_CALL occtsharp_volume_build(const OcctSharp_ShapeHandle* const* inputs,
  int32_t input_count, const OcctSharp_VolumeOptions* options, OcctSharp_FeatureResultHandle** result) {
  if (result) *result = nullptr;
  return Guard([&] {
    Require(result && options, "Missing volume options or output.");
    Require(std::isfinite(options->fuzzy) && options->fuzzy >= 0 && options->max_solids > 0
      && options->max_solids <= 100000, "Invalid volume precision or capacity.");
    Authoring::Flag(options->intersect); Authoring::Flag(options->avoid_internal); Authoring::Flag(options->parallel);
    Authoring::InputGraph graph(inputs, input_count);
    for (const auto& shape : graph.Shapes) RequireExactFaceSupport(shape);
    auto owner = std::make_unique<OcctSharp_FeatureResultHandle>();
    owner->Regions = std::make_shared<OcctSharp_RegionData>(); auto& data = *owner->Regions;
    if (!CheckInputs(graph.Shapes, *owner)) { Publish(std::move(owner), result); return; }
    NCollection_List<TopoDS_Shape> arguments;
    TopoDS_Compound combined; BRep_Builder builder; builder.MakeCompound(combined);
    for (const auto& shape : graph.Shapes) { arguments.Append(shape); builder.Add(combined, shape); }
    ShapeMap originalFaces; TopExp::MapShapes(combined, TopAbs_FACE, originalFaces);
    if (!originalFaces.IsEmpty()) {
      BOPAlgo_ShellSplitter splitter;
      for (const auto& face : originalFaces) splitter.AddStartElement(face);
      splitter.Perform();
      if (!splitter.HasErrors()) {
        int index = 0;
        for (const auto& shell : splitter.Shells())
          Add(data, ShellCandidate, index++, IsTopologyClosed(shell), BRepCheck_Analyzer(shell).IsValid(),
            -1, shell.Orientation(), Measure(shell, 2), shell);
      }
    }
    if (!options->intersect) {
      // A whole-graph self-interference check includes cross-argument interactions.
      // Shared topology is retained by InputGraph; disconnected coincident faces
      // must be intersected/sewn first, not accepted by a caller trust flag.
      BRepAlgoAPI_Check precondition(combined, true, true);
      Require(precondition.IsValid(), "Non-intersecting volume mode requires verified non-interfering input topology.");
    }
    BOPAlgo_MakerVolume algorithm; algorithm.SetArguments(arguments);
    algorithm.SetNonDestructive(true); algorithm.SetRunParallel(options->parallel != 0);
    algorithm.SetFuzzyValue(options->fuzzy); algorithm.SetIntersect(options->intersect != 0);
    algorithm.SetAvoidInternalShapes(options->avoid_internal != 0); algorithm.Perform();
    if (algorithm.HasErrors()) {
      std::ostringstream errors; algorithm.DumpErrors(errors); owner->Message = errors.str();
      Publish(std::move(owner), result); return;
    }
    ShapeMap solids; TopExp::MapShapes(algorithm.Shape(), TopAbs_SOLID, solids);
    Require(solids.Extent() <= options->max_solids, "Volume solid budget exceeded.");
    ShapeMap helperFaces; if (!algorithm.Box().IsNull()) TopExp::MapShapes(algorithm.Box(), TopAbs_FACE, helperFaces);
    ShapeMap finalFaces; TopExp::MapShapes(algorithm.Shape(), TopAbs_FACE, finalFaces);
    for (const auto& face : helperFaces) Require(!finalFaces.Contains(face), "Construction box face leaked into product topology.");
    Add(data, HelperCheck, helperFaces.Extent(), 0, -1, -1, 1);
    std::vector<TopoDS_Shape> cells;
    for (int i = 1; i <= solids.Extent(); ++i) {
      const auto& solid = solids(i);
      Require(!solid.IsSame(algorithm.Box()), "Construction box solid leaked into product output.");
      const bool valid = BRepCheck_Analyzer(solid).IsValid();
      Add(data, Cell, i - 1, 3, TopAbs_SOLID, -1, valid ? 1 : 0, Measure(solid, 3), solid);
      cells.push_back(solid);
      int shellIndex = 0;
      for (TopExp_Explorer s(solid, TopAbs_SHELL); s.More(); s.Next()) {
        const auto& shell = s.Current(); int role = -1;
        if (IsTopologyClosed(shell)) {
          BRepBuilderAPI_MakeSolid bounded(TopoDS::Shell(shell));
          if (bounded.IsDone()) {
            BRepClass3d_SolidClassifier classifier(bounded.Solid()); classifier.PerformInfinitePoint(1e-7);
            if (classifier.State() == TopAbs_IN) role = 1;
            else if (classifier.State() == TopAbs_OUT) role = 0;
          }
        }
        Add(data, VolumeShell, i - 1, shellIndex++, role, IsTopologyClosed(shell), shell.Orientation(), Measure(shell, 2), shell);
      }
      ShapeMap internals; TopExp::MapShapes(solid, internals);
      for (const auto& item : internals)
        if ((item.ShapeType() == TopAbs_EDGE || item.ShapeType() == TopAbs_VERTEX) && item.Orientation() == TopAbs_INTERNAL)
          Add(data, InternalTopology, i - 1, item.ShapeType(), item.Orientation(), -1, 0, 0, item);
    }
    InspectBoundaries(cells, data);
    for (int input = 0; input < input_count; ++input) {
      ShapeMap faces; TopExp::MapShapes(graph.Shapes[input], TopAbs_FACE, faces);
      for (int index = 1; index <= faces.Extent(); ++index) {
        const auto& source = faces(index); ShapeMap images; images.Add(source);
        if (const auto* mapped = algorithm.Images().Seek(source)) for (const auto& image : *mapped) images.Add(image);
        bool used = false;
        for (int cell = 0; cell < static_cast<int>(cells.size()); ++cell) {
          ShapeMap volumeFaces; TopExp::MapShapes(cells[cell], TopAbs_FACE, volumeFaces);
          for (const auto& image : images) if (volumeFaces.Contains(image)) {
            Add(data, SourceFace, cell, input, index - 1, -1, 0, 0, volumeFaces(volumeFaces.FindIndex(image))); used = true;
          }
        }
        if (!used) Add(data, UnusedFace, input, index - 1, -1, -1, 0, 0, source);
      }
    }
    // Only unused faces contribute unresolved boundaries. Successfully reconstructed
    // independent input faces must not be reported open merely because their original
    // edge TShapes were distinct before intersection.
    ShapeMap sourceFaces;
    for (size_t item = 0; item < data.Items.size(); ++item)
      if (data.Items[item].kind == UnusedFace) sourceFaces.Add(data.Shapes[item]);
    ShapeMap edges; std::vector<int> uses;
    for (const auto& face : sourceFaces) for (TopExp_Explorer e(face, TopAbs_EDGE); e.More(); e.Next()) {
      int edge = edges.FindIndex(e.Current());
      if (!edge) { edge = edges.Add(e.Current()); uses.push_back(0); }
      ++uses[edge - 1];
    }
    for (int i = 1; i <= edges.Extent(); ++i) if (uses[i - 1] == 1)
      Add(data, FreeBoundary, i - 1, -1, -1, -1, 0, Measure(edges(i), 1), edges(i));
    owner->Result = algorithm.Shape(); data.Info.done = 1;
    data.Info.valid = BRepCheck_Analyzer(owner->Result).IsValid();
    data.Info.cell_count = solids.Extent(); data.Info.output_count = 1;
    data.Info.warnings = algorithm.HasWarnings();
    Add(data, Output, 0, -1, -1, -1, data.Info.valid, 0, owner->Result);
    std::ostringstream warnings; algorithm.DumpWarnings(warnings); owner->Message = warnings.str();
    Publish(std::move(owner), result);
  });
}
