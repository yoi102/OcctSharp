#include "Modeling/ContourPrograms.hxx"
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <numbers>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
OcctSharp_Status OCCTSHARP_CALL occtsharp_contour_chamfer(const OcctSharp_ShapeHandle* source,
  const OcctSharp_ChamferProgram* programs, int32_t count, int32_t mode, int32_t build, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output && count >= 0 && count <= 256 && (!count || programs), "Invalid chamfer request.");
    Require(mode >= 0 && mode <= 2, "Invalid chamfer mode."); Flag(build); InputGraph graph(&source, 1); Result result(1);
    BRepFilletAPI_MakeChamfer algorithm(graph.At(0)); algorithm.SetMode(static_cast<ChFiDS_ChamfMode>(mode));
    std::vector<int> seeds;
    for (int i = 0; i < count; ++i) {
      const auto& p = programs[i]; const auto& edge = TopoDS::Edge(graph.Subshape(0, p.seed, TopAbs_EDGE));
      const auto& face = TopoDS::Face(graph.Subshape(0, p.support, TopAbs_FACE));
      Require(Map(face).Contains(edge), "The chamfer support face must contain its seed edge.");
      Require(p.reserved == 0 && p.method >= 0 && p.method <= 2, "Invalid chamfer dimension method."); Positive(p.first);
      Require(mode == 0 || (mode == 1 && p.method == 0) || (mode == 2 && p.method == 1), "Throat modes require their own symmetric/penetration dimension programs.");
      if (p.method == 1) Positive(p.second);
      if (p.method == 2) Require(std::isfinite(p.second) && p.second > 0 && p.second < std::numbers::pi / 2, "Chamfer angle must be in (0,pi/2) radians.");
      Require(algorithm.Contour(edge) == 0, "Conflicting chamfer programs on one tangent contour.");
      if (p.method == 0) algorithm.Add(p.first, edge);
      else if (p.method == 1) algorithm.Add(p.first, p.second, edge, face);
      else algorithm.AddDA(p.first, p.second, edge, face);
      Require(algorithm.Contour(edge) > 0, "The seed is not an eligible chamfer contour."); seeds.push_back(p.seed);
    }
    CopyContours(algorithm, graph, seeds, result); result.Data().Info.ready = 1;
    try {
      if (build) {
        if (!count) { result.Owner->Result = graph.At(0); result.Data().Info.done = 1; }
        else {
          algorithm.Build(); result.Data().Info.done = algorithm.IsDone();
          if (algorithm.IsDone()) { result.Owner->Result = algorithm.Shape(); History(algorithm, graph, result); result.Owner->Message = "Chamfer uses the explicit support and OCCT dimension mode."; }
          else result.Fail("The chamfer dimensions failed on this source.");
        }
      }
    } catch (const Standard_Failure& error) { result.Fail(error.GetMessageString()); }
    result.Publish(output);
  });
}
