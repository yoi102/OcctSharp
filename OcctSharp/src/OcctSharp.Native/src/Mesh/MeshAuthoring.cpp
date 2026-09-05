#include "Mesh/MeshAuthoring.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRep_Builder.hxx>
#include <Poly_Polygon3D.hxx>
#include <Poly_PolygonOnTriangulation.hxx>
#include <Poly_Triangle.hxx>
#include <NCollection_Array1.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <gp_GTrsf.hxx>
#include <algorithm>
#include <cmath>

namespace OcctSharp::Native::MeshAuthoring {
void Require(bool condition, const char* message) {
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
void ValidateVertices(const OcctSharp_AuthoredVertex* vertices, int count) {
  Require(count >= 0 && count <= MaximumElements && (count == 0 || vertices), "Invalid or excessive mesh vertex count/buffer.");
  int channels = count == 0 ? 0 : vertices[0].flags & 6;
  for (int i = 0; i < count; ++i) {
    const auto& v = vertices[i];
    Require(v.reserved == 0 && (v.flags & ~7) == 0 && (v.flags & 6) == channels, "Mesh channels must be complete and flags valid.");
    Require(std::isfinite(v.x) && std::isfinite(v.y) && std::isfinite(v.z) && std::isfinite(v.nx) &&
      std::isfinite(v.ny) && std::isfinite(v.nz) && std::isfinite(v.u) && std::isfinite(v.v), "Mesh data must be finite.");
    Require(!(v.flags & 1) || ((v.flags & 4) && (v.nx != 0 || v.ny != 0 || v.nz != 0)), "Defined normal is absent or zero.");
    Require((v.flags & 1) || (v.nx == 0 && v.ny == 0 && v.nz == 0), "Undefined normals must have zero components.");
  }
}
void ValidateTriangles(const OcctSharp_AuthoredTriangle* triangles, int count, int vertexCount) {
  Require(count >= 0 && count <= MaximumElements && (count == 0 || triangles), "Invalid or excessive mesh triangle count/buffer.");
  for (int i = 0; i < count; ++i) {
    const auto& t = triangles[i];
    Require(t.a >= 0 && t.a < vertexCount && t.b >= 0 && t.b < vertexCount && t.c >= 0 && t.c < vertexCount && t.group >= 0,
      "Triangle contains an invalid zero-based vertex or group index.");
  }
}
occ::handle<Poly_Triangulation> Build(const OcctSharp_AuthoredVertex* vertices, int vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int triangleCount) {
  ValidateVertices(vertices, vertexCount); ValidateTriangles(triangles, triangleCount, vertexCount);
  Require(vertexCount > 0 && triangleCount > 0, "An owning triangulated face requires nodes and triangles.");
  bool uv = (vertices[0].flags & 2) != 0, normals = (vertices[0].flags & 4) != 0;
  occ::handle<Poly_Triangulation> mesh = new Poly_Triangulation(vertexCount, triangleCount, uv, normals);
  for (int i = 0; i < vertexCount; ++i) {
    const auto& v = vertices[i]; mesh->SetNode(i + 1, gp_Pnt(v.x, v.y, v.z));
    if (uv) mesh->SetUVNode(i + 1, gp_Pnt2d(v.u, v.v));
    if (normals) {
      gp_XYZ n(v.nx, v.ny, v.nz);
      if (v.flags & 1) { double scale = std::max({std::abs(v.nx), std::abs(v.ny), std::abs(v.nz)}); n /= scale; n.Normalize(); }
      mesh->SetNormal(i + 1, NCollection_Vec3<float>(static_cast<float>(n.X()), static_cast<float>(n.Y()), static_cast<float>(n.Z())));
    }
  }
  for (int i = 0; i < triangleCount; ++i) mesh->SetTriangle(i + 1, Poly_Triangle(triangles[i].a + 1, triangles[i].b + 1, triangles[i].c + 1));
  return mesh;
}
}

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::MeshAuthoring;

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_author_face(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangleCount, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] { Require(output, "Shape output is required.");
    auto mesh = Build(vertices, vertexCount, triangles, triangleCount); TopoDS_Face face; BRep_Builder builder;
    builder.MakeFace(face, mesh); *output = AllocateShape(face);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_transform(const OcctSharp_AuthoredVertex* vertices, int32_t count,
  const double* matrix, int32_t matrixCount, OcctSharp_AuthoredVertex* output, int32_t capacity, double* determinant) {
  if (determinant) *determinant = 0;
  return Guard([&] {
    ValidateVertices(vertices, count); Require(matrix && matrixCount == 12 && determinant && capacity >= count && (count == 0 || output), "Invalid transform buffers.");
    gp_GTrsf transform;
    for (int i = 0; i < 12; ++i) { Require(std::isfinite(matrix[i]), "Affine coefficients must be finite."); transform.SetValue(i / 4 + 1, i % 4 + 1, matrix[i]); }
    gp_Mat linear = transform.VectorialPart(); double det = linear.Determinant();
    Require(std::isfinite(det) && det != 0 && !linear.IsSingular(), "A singular or numerically singular affine transform is not supported.");
    gp_Mat normal = linear.Inverted().Transposed(); std::vector<OcctSharp_AuthoredVertex> result;
    result.reserve(count);
    for (int i = 0; i < count; ++i) {
      auto v = vertices[i]; gp_XYZ p(v.x, v.y, v.z); transform.Transforms(p); v.x = p.X(); v.y = p.Y(); v.z = p.Z();
      if (v.flags & 1) {
        double scale = std::max({std::abs(v.nx), std::abs(v.ny), std::abs(v.nz)});
        gp_XYZ n(v.nx / scale, v.ny / scale, v.nz / scale); n.Multiply(normal); n.Normalize();
        v.nx = n.X(); v.ny = n.Y(); v.nz = n.Z();
      }
      result.push_back(v);
    }
    ValidateVertices(result.data(), count); std::copy(result.begin(), result.end(), output); *determinant = det;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_polyline(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const int32_t* indices, int32_t indexCount, const double* parameters, int32_t parameterCount, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateVertices(vertices, vertexCount);
    Require(output && indices && indexCount >= 2 && indexCount <= MaximumElements &&
      (parameterCount == 0 || (parameters && parameterCount == indexCount)), "Invalid polyline buffers.");
    NCollection_Array1<gp_Pnt> points(1, indexCount); NCollection_Array1<int> nodes(1, indexCount); NCollection_Array1<double> params(1, indexCount);
    for (int i = 0; i < indexCount; ++i) {
      Require(indices[i] >= 0 && indices[i] < vertexCount, "Polyline index is out of range.");
      const auto& v = vertices[indices[i]]; points.SetValue(i + 1, gp_Pnt(v.x, v.y, v.z)); nodes.SetValue(i + 1, indices[i] + 1);
      if (parameterCount) {
        Require(std::isfinite(parameters[i]) && (i == 0 || parameters[i] > parameters[i - 1]), "Polyline parameters must be finite and increasing.");
        params.SetValue(i + 1, parameters[i]);
      }
    }
    // Both native Poly representations are created call-locally; only the independently owning edge escapes.
    occ::handle<Poly_PolygonOnTriangulation> indexed = parameterCount
      ? new Poly_PolygonOnTriangulation(nodes, params) : new Poly_PolygonOnTriangulation(nodes);
    Require(indexed->NbNodes() == indexCount, "Indexed polygon construction failed.");
    occ::handle<Poly_Polygon3D> polygon = parameterCount ? new Poly_Polygon3D(points, params) : new Poly_Polygon3D(points);
    TopoDS_Edge edge; BRep_Builder builder; builder.MakeEdge(edge, polygon); *output = AllocateShape(edge);
  });
}
