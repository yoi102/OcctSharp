#pragma once

// Private native Mesh/Triangulation contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include <vector>

namespace OcctSharp::Native
{
struct MeshData
{
  std::vector<OcctSharp_MeshVertex> Vertices;
  std::vector<int32_t> Indices;
};

struct DetailedMeshData
{
  std::vector<OcctSharp_DetailedMeshVertex> Vertices;
  std::vector<OcctSharp_DetailedMeshTriangle> Triangles;
  int32_t FaceCount = 0;
};

void ValidateMeshParameters(const double linear_deflection, const double angular_deflection);

MeshData BuildMesh(const OcctSharp_ShapeHandle* shape,
                   const double linear_deflection,
                   const double angular_deflection);

DetailedMeshData BuildDetailedMesh(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection);

DetailedMeshData BuildAdvancedMesh(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  const double minimum_size,
  const bool relative,
  const bool parallel,
  const bool internal_vertices,
  const bool control_surface_deflection);
}
