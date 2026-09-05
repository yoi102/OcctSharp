#pragma once
#include "Modeling/LocalFeatures.hxx"
#include <BRepFilletAPI_LocalOperation.hxx>
#include <TopExp.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Vertex.hxx>

namespace OcctSharp::Native::LocalFeatures {
inline void CopyContours(BRepFilletAPI_LocalOperation& algorithm, const InputGraph& graph,
  const std::vector<int>& seeds, Result& result) {
  const int count = algorithm.NbContours(); Require(count >= 0 && count <= 256, "Too many finishing contours.");
  for (int ic = 1; ic <= count; ++ic) {
    int program = -1;
    for (int p = 0; p < static_cast<int>(seeds.size()); ++p)
      if (algorithm.Contour(TopoDS::Edge(graph.Subshape(0, seeds[p], TopAbs_EDGE))) == ic) { program = p; break; }
    Require(program >= 0, "An unassigned contour has no source provenance.");
    result.Data().Contours.push_back({ic - 1, program, seeds[program], graph.Index(0, algorithm.FirstVertex(ic)),
      graph.Index(0, algorithm.LastVertex(ic)), algorithm.Closed(ic), algorithm.ClosedAndTangent(ic), 0, algorithm.Length(ic)});
    const int edges = algorithm.NbEdges(ic); Require(edges > 0 && edges <= 100000, "Invalid contour edge count.");
    for (int j = 1; j <= edges; ++j) {
      const auto& edge = algorithm.Edge(ic, j); TopoDS_Vertex first, last; TopExp::Vertices(edge, first, last, true);
      const int index = graph.Index(0, edge); Require(index >= 0, "A contour edge is outside the source correspondence.");
      double a = first.IsNull() ? -1 : algorithm.RelativeAbscissa(ic, first);
      double b = last.IsNull() ? -1 : algorithm.RelativeAbscissa(ic, last);
      if (j == edges && algorithm.Closed(ic) && !last.IsNull() && last.IsSame(algorithm.FirstVertex(ic))) b = 1;
      result.Data().Edges.push_back({ic - 1, j - 1, index, graph.Index(0, first), graph.Index(0, last), 0, a, b});
      result.Add(edge, ContourEdge, 0, index, ic - 1, TopAbs_EDGE);
    }
  }
}
}
