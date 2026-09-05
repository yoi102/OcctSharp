#include "Mesh/MeshAuthoring.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <BRep_Builder.hxx>
#include <BRep_Tool.hxx>
#include <NCollection_IndexedMap.hxx>
#include <Poly_Connect.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Face.hxx>
#include <algorithm>
#include <cmath>
#include <set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::MeshAuthoring;

namespace {
using FaceMap = NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher>;
FaceMap Faces(const TopoDS_Shape& source) { FaceMap map; TopExp::MapShapes(source, TopAbs_FACE, map); return map; }
bool IsExact(const TopoDS_Shape& shape) {
  auto faces = Faces(shape);
  if (faces.IsEmpty()) return false;
  for (const auto& face : faces) if (BRep_Tool::Surface(TopoDS::Face(face)).IsNull()) return false;
  return true;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_is_exact(const OcctSharp_ShapeHandle* source, int32_t* exact) {
  if (exact) *exact = 0;
  return Guard([&] { ValidateUsableShape(source); Require(exact, "Exactness output is required."); *exact = IsExact(source->Value) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_copy_shape(const OcctSharp_ShapeHandle* source, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateUsableShape(source); Require(output, "Shape output is required.");
    BRepBuilderAPI_Copy copy(source->Value, true, true); Require(copy.IsDone(), "Independent geometry/triangulation copy failed.");
    *output = AllocateShape(copy.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_existing_snapshot(const OcctSharp_ShapeHandle* shape,
  OcctSharp_AuthoredVertex* vertices, int32_t vertexCapacity, int32_t* vertexCount,
  OcctSharp_AuthoredTriangle* triangles, int32_t triangleCapacity, int32_t* triangleCount, int32_t* faceCount) {
  if (vertexCount) *vertexCount = 0;
  if (triangleCount) *triangleCount = 0;
  if (faceCount) *faceCount = 0;
  return Guard([&] {
    ValidateUsableShape(shape);
    Require(vertexCount && triangleCount && faceCount && vertexCapacity >= 0 && triangleCapacity >= 0, "Invalid snapshot counts/capacities.");
    auto faces = Faces(shape->Value); *faceCount = faces.Extent();
    std::vector<OcctSharp_AuthoredVertex> points; std::vector<OcctSharp_AuthoredTriangle> facets;
    for (int faceIndex = 1; faceIndex <= faces.Extent(); ++faceIndex) {
      auto face = TopoDS::Face(faces(faceIndex)); TopLoc_Location location;
      const auto& mesh = BRep_Tool::Triangulation(face, location);
      Require(!mesh.IsNull(), "Face has no existing triangulation; snapshot does not implicitly mesh it.");
      Require(mesh->NbNodes() <= MaximumElements - static_cast<int>(points.size()) &&
        mesh->NbTriangles() <= MaximumElements - static_cast<int>(facets.size()), "Existing triangulation exceeds snapshot limits.");
      int offset = static_cast<int>(points.size()); const auto& transform = location.Transformation();
      bool reversed = face.Orientation() == TopAbs_REVERSED;
      gp_Mat normalTransform = transform.VectorialPart().Inverted().Transposed();
      for (int i = 1; i <= mesh->NbNodes(); ++i) {
        gp_Pnt p = mesh->Node(i).Transformed(transform); OcctSharp_AuthoredVertex value{};
        value.x = p.X(); value.y = p.Y(); value.z = p.Z();
        if (mesh->HasUVNodes()) { const auto uv = mesh->UVNode(i); value.u = uv.X(); value.v = uv.Y(); value.flags |= 2; }
        if (mesh->HasNormals()) {
          value.flags |= 4; NCollection_Vec3<float> raw; mesh->Normal(i, raw);
          gp_XYZ n(raw.x(), raw.y(), raw.z());
          if (n.SquareModulus() > 0) {
            n.Multiply(normalTransform); n.Normalize(); if (reversed) n.Reverse();
            value.nx = n.X(); value.ny = n.Y(); value.nz = n.Z(); value.flags |= 1;
          }
        }
        points.push_back(value);
      }
      bool reverseTriangles = reversed != transform.IsNegative();
      for (int i = 1; i <= mesh->NbTriangles(); ++i) {
        int a, b, c; mesh->Triangle(i).Get(a, b, c); if (reverseTriangles) std::swap(b, c);
        facets.push_back({a - 1 + offset, b - 1 + offset, c - 1 + offset, faceIndex - 1});
      }
    }
    *vertexCount = static_cast<int>(points.size()); *triangleCount = static_cast<int>(facets.size());
    if (!vertices && !triangles && vertexCapacity == 0 && triangleCapacity == 0) return;
    Require(vertexCapacity >= *vertexCount && triangleCapacity >= *triangleCount &&
      (points.empty() || vertices) && (facets.empty() || triangles), "Existing mesh snapshot buffers are too small.");
    std::copy(points.begin(), points.end(), vertices); std::copy(facets.begin(), facets.end(), triangles);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_replace_face(const OcctSharp_ShapeHandle* source, int32_t faceIndex,
  const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount, const OcctSharp_AuthoredTriangle* triangles,
  int32_t triangleCount, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateUsableShape(source); Require(output, "Shape output is required.");
    auto faces = Faces(source->Value); Require(faceIndex >= 0 && faceIndex < faces.Extent(), "Replacement face index is out of range.");
    Require(!BRep_Tool::Surface(TopoDS::Face(faces(faceIndex + 1))).IsNull(), "Cache replacement requires an exact surface-backed face.");
    auto mesh = Build(vertices, vertexCount, triangles, triangleCount);
    BRepBuilderAPI_Copy copy(source->Value, true, true); Require(copy.IsDone(), "Independent mesh-cache copy failed.");
    TopoDS_Face face = TopoDS::Face(copy.ModifiedShape(faces(faceIndex + 1)));
    Require(!face.IsNull(), "Copied face identity could not be resolved.");
    BRep_Builder builder; builder.UpdateFace(face, mesh, true); *output = AllocateShape(copy.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_remesh_faces(const OcctSharp_ShapeHandle* source, const int32_t* selected,
  int32_t faceCount, double linear, double angular, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateUsableShape(source);
    Require(output && faceCount > 0 && faceCount <= MaximumElements && selected && std::isfinite(linear) && linear > 0 &&
      std::isfinite(angular) && angular > 0 && angular <= 3.14159265358979323846, "Invalid selected remeshing request.");
    auto faces = Faces(source->Value); std::set<int> unique;
    for (int i = 0; i < faceCount; ++i) {
      Require(selected[i] >= 0 && selected[i] < faces.Extent() && unique.insert(selected[i]).second, "Invalid or duplicate remesh face index.");
      Require(!BRep_Tool::Surface(TopoDS::Face(faces(selected[i] + 1))).IsNull(), "Cannot remesh a triangulation-only face without an exact surface.");
    }
    BRepBuilderAPI_Copy copy(source->Value, true, true); Require(copy.IsDone(), "Independent remeshing copy failed.");
    for (int index : unique) {
      TopoDS_Face face = TopoDS::Face(copy.ModifiedShape(faces(index + 1))); Require(!face.IsNull(), "Copied face mapping is unavailable.");
      BRepTools::Clean(face, true); BRepMesh_IncrementalMesh mesher(face, linear, false, angular, false);
      Require(mesher.IsDone(), "Selected exact-face remeshing failed."); TopLoc_Location location;
      Require(!BRep_Tool::Triangulation(face, location).IsNull(), "Selected remeshing produced no triangulation.");
    }
    *output = AllocateShape(copy.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_poly_connect(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangleCount, int32_t* neighbors, int32_t capacity) {
  return Guard([&] {
    ValidateVertices(vertices, vertexCount); ValidateTriangles(triangles, triangleCount, vertexCount);
    Require(capacity >= triangleCount * 3 && (triangleCount == 0 || neighbors), "Connectivity output capacity is too small.");
    if (triangleCount == 0) return;
    ValidateCoherent(triangles, triangleCount); Poly_Connect connect(Build(vertices, vertexCount, triangles, triangleCount));
    std::vector<int> result(triangleCount * 3);
    for (int i = 1; i <= triangleCount; ++i) {
      int a, b, c; connect.Triangles(i, a, b, c);
      result[(i - 1) * 3] = a - 1; result[(i - 1) * 3 + 1] = b - 1; result[(i - 1) * 3 + 2] = c - 1;
    }
    std::copy(result.begin(), result.end(), neighbors);
  });
}
