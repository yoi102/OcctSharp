#pragma once
#include "OcctSharp.Native.Mesh.h"
#include <Poly_Triangulation.hxx>
#include <vector>

namespace OcctSharp::Native::MeshAuthoring {
inline constexpr int MaximumElements = 5000000;
void Require(bool condition, const char* message);
void ValidateVertices(const OcctSharp_AuthoredVertex* vertices, int count);
void ValidateTriangles(const OcctSharp_AuthoredTriangle* triangles, int count, int vertexCount);
occ::handle<Poly_Triangulation> Build(const OcctSharp_AuthoredVertex* vertices, int vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int triangleCount);
void ValidateCoherent(const OcctSharp_AuthoredTriangle* triangles, int triangleCount);
}
