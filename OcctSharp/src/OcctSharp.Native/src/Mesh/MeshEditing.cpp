#include "Mesh/MeshAuthoring.hxx"
#include "Runtime/Error.hxx"
#include <Poly_CoherentTriangulation.hxx>
#include <Poly_MergeNodesTool.hxx>
#include <algorithm>
#include <cmath>
#include <limits>
#include <map>
#include <set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::MeshAuthoring;

namespace OcctSharp::Native::MeshAuthoring {
void ValidateCoherent(const OcctSharp_AuthoredTriangle* triangles, int count) {
  std::map<std::pair<int, int>, std::vector<std::pair<int, int>>> edges;
  for (int i = 0; i < count; ++i) {
    const auto& t = triangles[i]; int nodes[3] = {t.a, t.b, t.c};
    Require(t.a != t.b && t.b != t.c && t.a != t.c, "A coherent patch cannot contain repeated triangle nodes.");
    for (int c = 0; c < 3; ++c) {
      int a = nodes[c], b = nodes[(c + 1) % 3]; auto key = std::minmax(a, b);
      auto& uses = edges[{key.first, key.second}];
      Require(uses.size() < 2 && (uses.empty() || uses[0].first != a), "Patch adjacency is non-manifold or inconsistently oriented.");
      uses.emplace_back(a, b);
    }
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_weld_nodes(const OcctSharp_AuthoredVertex* vertices, int32_t count,
  const int32_t* partitions, int32_t partitionCount, double tolerance, int32_t* representatives, int32_t capacity) {
  return Guard([&] {
    ValidateVertices(vertices, count);
    Require(std::isfinite(tolerance) && tolerance >= 0 && tolerance <= std::numeric_limits<float>::max() &&
      (tolerance == 0 || tolerance >= std::numeric_limits<float>::min()), "Weld tolerance is outside OCCT's supported float hashing range.");
    Require(partitionCount == count && capacity >= count && (count == 0 || (partitions && representatives)), "Invalid weld buffers.");
    struct Partition { occ::handle<Poly_MergeNodesTool> tool; std::vector<int> representatives; };
    std::map<int, Partition> tools; std::vector<int> result(count);
    for (int i = 0; i < count; ++i) {
      const auto& v = vertices[i];
      Require(partitions[i] >= 0 && partitions[i] < std::max(count, 1), "Invalid weld partition.");
      Require(std::abs(v.x) <= std::numeric_limits<float>::max() && std::abs(v.y) <= std::numeric_limits<float>::max() &&
        std::abs(v.z) <= std::numeric_limits<float>::max(), "Mesh positions exceed the OCCT weld hash range.");
      auto& partition = tools[partitions[i]];
      if (partition.tool.IsNull()) {
        partition.tool = new Poly_MergeNodesTool(3.14159265358979323846, tolerance);
        partition.tool->ChangeOutput().Nullify(); partition.tool->SetDropDegenerative(false);
      }
      // OCCT explicitly supports use as a merge map with no output triangulation.
      // A three-point element inserts one position without deriving an artificial surface normal.
      gp_XYZ p(v.x, v.y, v.z); gp_XYZ nodes[3] = {p, p, p}; partition.tool->AddTriangle(nodes);
      int local = partition.tool->ElementNodeIndex(0);
      Require(local >= 0 && local <= static_cast<int>(partition.representatives.size()), "OCCT returned an inconsistent weld index.");
      if (local == static_cast<int>(partition.representatives.size())) partition.representatives.push_back(i);
      int representative = partition.representatives[local]; const auto& old = vertices[representative];
      double distance = std::hypot(std::hypot(v.x - old.x, v.y - old.y), v.z - old.z);
      Require(distance <= tolerance, "OCCT float hash merged nodes outside the declared double-precision tolerance; use a larger tolerance or local coordinates.");
      result[i] = representative;
    }
    std::copy(result.begin(), result.end(), representatives);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_coherent_patch(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangleCount,
  const int32_t* replaced, const OcctSharp_AuthoredTriangle* replacements, int32_t replacementCount,
  const OcctSharp_AuthoredTriangle* appended, int32_t appendedCount, OcctSharp_AuthoredTriangle* output, int32_t capacity) {
  return Guard([&] {
    ValidateVertices(vertices, vertexCount); ValidateTriangles(triangles, triangleCount, vertexCount);
    ValidateTriangles(replacements, replacementCount, vertexCount); ValidateTriangles(appended, appendedCount, vertexCount);
    Require(replacementCount == 0 || replaced, "Replacement indices are required.");
    Require(appendedCount <= MaximumElements - triangleCount && capacity >= triangleCount + appendedCount &&
      (triangleCount + appendedCount == 0 || output), "Invalid patch output capacity.");
    ValidateCoherent(triangles, triangleCount);
    std::vector<OcctSharp_AuthoredTriangle> result;
    if (triangleCount) result.assign(triangles, triangles + triangleCount);
    std::set<int> changed;
    for (int i = 0; i < replacementCount; ++i) {
      Require(replaced[i] >= 0 && replaced[i] < triangleCount && changed.insert(replaced[i]).second, "Invalid or duplicate replacement index.");
      result[replaced[i]] = replacements[i];
    }
    if (appendedCount) result.insert(result.end(), appended, appended + appendedCount);
    ValidateCoherent(result.data(), static_cast<int>(result.size()));
    Poly_CoherentTriangulation coherent;
    for (int i = 0; i < vertexCount; ++i) coherent.SetNode(gp_XYZ(vertices[i].x, vertices[i].y, vertices[i].z));
    std::vector<Poly_CoherentTriangle*> sourceTriangles;
    for (int i = 0; i < triangleCount; ++i) {
      auto* triangle = coherent.AddTriangle(triangles[i].a, triangles[i].b, triangles[i].c);
      Require(triangle, "OCCT could not create coherent source connectivity."); sourceTriangles.push_back(triangle);
    }
    for (int i = 0; i < replacementCount; ++i) {
      const auto& t = replacements[i];
      Require(coherent.ReplaceNodes(*sourceTriangles[replaced[i]], t.a, t.b, t.c), "Coherent replacement failed; no source was mutated.");
    }
    for (int i = 0; i < appendedCount; ++i) {
      const auto& t = appended[i]; Require(coherent.AddTriangle(t.a, t.b, t.c), "Coherent patch insertion failed.");
    }
    Require(coherent.NTriangles() == static_cast<int>(result.size()), "Coherent patch cardinality differs from requested connectivity.");
    // Node IDs in the coherent owner remain original indices; GetTriangulation compacts them,
    // so copy the actual coherent triangle nodes without substituting compacted insertion IDs.
    for (int i = 0; i < static_cast<int>(result.size()); ++i) {
      const auto& t = coherent.Triangle(i); result[i].a = t.Node(0); result[i].b = t.Node(1); result[i].c = t.Node(2);
    }
    std::copy(result.begin(), result.end(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_remove_degenerate(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangleCount, double minimumArea, double minimumLength,
  int32_t* removed, int32_t capacity, int32_t* removedCount) {
  if (removedCount) *removedCount = 0;
  return Guard([&] {
    ValidateVertices(vertices, vertexCount); ValidateTriangles(triangles, triangleCount, vertexCount);
    Require(std::isfinite(minimumArea) && minimumArea >= 0 && std::isfinite(minimumLength) && minimumLength >= 0 &&
      removedCount && capacity >= 0, "Invalid degeneration policy or output.");
    std::vector<int> result;
    for (int i = 0; i < triangleCount; ++i) {
      const auto& t = triangles[i]; const auto& a = vertices[t.a]; const auto& b = vertices[t.b]; const auto& c = vertices[t.c];
      gp_XYZ pa(a.x, a.y, a.z), pb(b.x, b.y, b.z), pc(c.x, c.y, c.z);
      double area = (pb - pa).Crossed(pc - pa).Modulus() * 0.5;
      double length = std::min({(pb - pa).Modulus(), (pc - pb).Modulus(), (pa - pc).Modulus()});
      Require(std::isfinite(area) && std::isfinite(length), "Degeneration query exceeds numeric range.");
      if (area > minimumArea && length > minimumLength) continue;
      // Removal-only policy deliberately avoids RemoveDegenerated's implicit vertex collapse.
      // Isolate each removed facet so even a non-manifold input is filtered without rewiring survivors.
      Poly_CoherentTriangulation facet;
      facet.SetNode(pa); facet.SetNode(pb); facet.SetNode(pc);
      auto* item = facet.AddTriangle(0, 1, 2);
      Require(item && facet.RemoveTriangle(*item) && facet.NTriangles() == 0, "OCCT coherent triangle removal failed.");
      result.push_back(i);
    }
    *removedCount = static_cast<int>(result.size());
    if (!removed && capacity == 0) return;
    Require(capacity >= *removedCount && (result.empty() || removed), "Degenerate triangle output buffer is too small.");
    std::copy(result.begin(), result.end(), removed);
  });
}
