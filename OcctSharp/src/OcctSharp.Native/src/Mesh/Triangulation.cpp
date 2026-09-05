// Native Mesh/Triangulation implementation. Public contracts and ownership are unchanged.
#include "Mesh/Triangulation.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRep_Tool.hxx>
#include <IMeshTools_Parameters.hxx>
#include <Poly_Triangle.hxx>
#include <Poly_Triangulation.hxx>
#include <Standard_Handle.hxx>
#include <TopExp_Explorer.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <cmath>
#include <cstddef>
#include <cstring>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <limits>

namespace OcctSharp::Native
{
void ValidateMeshParameters(const double linear_deflection, const double angular_deflection)
{
  if (!std::isfinite(linear_deflection) || linear_deflection <= 0.0
      || !std::isfinite(angular_deflection) || angular_deflection <= 0.0)
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Mesh deflections must be finite and greater than zero.");
  }
}

MeshData BuildMesh(const OcctSharp_ShapeHandle* shape,
                   const double linear_deflection,
                   const double angular_deflection)
{
  ValidateShape(shape);
  ValidateMeshParameters(linear_deflection, angular_deflection);
  BRepMesh_IncrementalMesh mesher(shape->Value, linear_deflection, false, angular_deflection, true);
  MeshData data;
  for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
  {
    const TopoDS_Face face = TopoDS::Face(explorer.Current());
    TopLoc_Location location;
    const opencascade::handle<Poly_Triangulation> triangulation = BRep_Tool::Triangulation(face, location);
    if (triangulation.IsNull())
    {
      continue;
    }

    for (int32_t triangleIndex = 1; triangleIndex <= triangulation->NbTriangles(); ++triangleIndex)
    {
      Poly_Triangle triangle = triangulation->Triangle(triangleIndex);
      int node1 = 0;
      int node2 = 0;
      int node3 = 0;
      triangle.Get(node1, node2, node3);
      gp_Pnt point1 = triangulation->Node(node1);
      gp_Pnt point2 = triangulation->Node(node2);
      gp_Pnt point3 = triangulation->Node(node3);
      const gp_Trsf locationTransform = location.Transformation();
      point1.Transform(locationTransform);
      point2.Transform(locationTransform);
      point3.Transform(locationTransform);

      gp_Vec normal(point1, point2);
      normal = normal.Crossed(gp_Vec(point1, point3));
      if (normal.SquareMagnitude() > 1.0e-24)
      {
        normal.Normalize();
        if (face.Orientation() == TopAbs_REVERSED)
        {
          normal.Reverse();
        }
      }

      const int32_t base = static_cast<int32_t>(data.Vertices.size());
      const auto appendVertex = [&](const gp_Pnt& point)
      {
        data.Vertices.push_back(OcctSharp_MeshVertex{
          point.X(), point.Y(), point.Z(), normal.X(), normal.Y(), normal.Z()});
      };
      appendVertex(point1);
      appendVertex(point2);
      appendVertex(point3);
      if (face.Orientation() == TopAbs_REVERSED)
      {
        data.Indices.insert(data.Indices.end(), {base, base + 2, base + 1});
      }
      else
      {
        data.Indices.insert(data.Indices.end(), {base, base + 1, base + 2});
      }
    }
  }
  return data;
}

DetailedMeshData BuildDetailedMesh(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection)
{
  ValidateUsableShape(shape);
  ValidateMeshParameters(linear_deflection, angular_deflection);
  BRepMesh_IncrementalMesh mesher(shape->Value, linear_deflection, false, angular_deflection, true);
  DetailedMeshData data;
  for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
  {
    if (data.FaceCount == std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The face count exceeds the 32-bit ABI.");
    const int32_t faceIndex = data.FaceCount++;
    const TopoDS_Face face = TopoDS::Face(explorer.Current());
    TopLoc_Location location;
    opencascade::handle<Poly_Triangulation> triangulation = BRep_Tool::Triangulation(face, location);
    if (triangulation.IsNull()) continue;
    if (!triangulation->HasNormals()) triangulation->ComputeNormals();

    const size_t baseValue = data.Vertices.size();
    if (baseValue + static_cast<size_t>(triangulation->NbNodes())
        > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The detailed mesh exceeds the 32-bit ABI.");
    const int32_t base = static_cast<int32_t>(baseValue);
    const gp_Trsf locationTransform = location.Transformation();
    const bool isReversed = face.Orientation() == TopAbs_REVERSED;
    const bool hasUv = triangulation->HasUVNodes();
    for (int32_t nodeIndex = 1; nodeIndex <= triangulation->NbNodes(); ++nodeIndex)
    {
      gp_Pnt point = triangulation->Node(nodeIndex);
      point.Transform(locationTransform);
      gp_Dir normal = triangulation->Normal(nodeIndex);
      normal.Transform(locationTransform);
      if (isReversed) normal.Reverse();
      double u = 0.0;
      double v = 0.0;
      if (hasUv)
      {
        const gp_Pnt2d uv = triangulation->UVNode(nodeIndex);
        u = uv.X();
        v = uv.Y();
      }
      data.Vertices.push_back({
        point.X(), point.Y(), point.Z(),
        normal.X(), normal.Y(), normal.Z(),
        u, v, hasUv ? 1 : 0 });
    }

    for (int32_t triangleIndex = 1; triangleIndex <= triangulation->NbTriangles(); ++triangleIndex)
    {
      int node1 = 0;
      int node2 = 0;
      int node3 = 0;
      triangulation->Triangle(triangleIndex).Get(node1, node2, node3);
      int32_t vertexA = base + node1 - 1;
      int32_t vertexB = base + node2 - 1;
      int32_t vertexC = base + node3 - 1;
      if (isReversed) std::swap(vertexB, vertexC);
      data.Triangles.push_back({ vertexA, vertexB, vertexC, faceIndex, isReversed ? 1 : 0 });
    }
  }
  return data;
}

DetailedMeshData BuildAdvancedMesh(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  const double minimum_size,
  const bool relative,
  const bool parallel,
  const bool internal_vertices,
  const bool control_surface_deflection)
{
  ValidateUsableShape(shape);
  ValidateMeshParameters(linear_deflection, angular_deflection);
  if (!std::isfinite(minimum_size) || minimum_size < 0.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The advanced-mesh minimum size must be finite and non-negative.");

  BRepBuilderAPI_Copy copier(shape->Value, true, false);
  const TopoDS_Shape working_shape = copier.Shape();
  if (working_shape.IsNull())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not copy the shape for independent advanced meshing.");

  IMeshTools_Parameters parameters;
  parameters.Deflection = linear_deflection;
  parameters.Angle = angular_deflection;
  parameters.MinSize = minimum_size > 0.0 ? minimum_size : -1.0;
  parameters.Relative = relative;
  parameters.InParallel = parallel;
  parameters.InternalVerticesMode = internal_vertices;
  parameters.ControlSurfaceDeflection = control_surface_deflection;
  parameters.AllowQualityDecrease = true;
  BRepMesh_IncrementalMesh mesher(working_shape, parameters);
  if (!mesher.IsDone())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT advanced meshing did not complete.");

  DetailedMeshData data;
  for (TopExp_Explorer explorer(working_shape, TopAbs_FACE); explorer.More(); explorer.Next())
  {
    if (data.FaceCount == std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The face count exceeds the 32-bit ABI.");
    const int32_t face_index = data.FaceCount++;
    const TopoDS_Face face = TopoDS::Face(explorer.Current());
    TopLoc_Location location;
    opencascade::handle<Poly_Triangulation> triangulation = BRep_Tool::Triangulation(face, location);
    if (triangulation.IsNull()) continue;
    if (!triangulation->HasNormals()) triangulation->ComputeNormals();

    const size_t base_value = data.Vertices.size();
    if (base_value + static_cast<size_t>(triangulation->NbNodes())
        > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The advanced mesh exceeds the 32-bit ABI.");
    const int32_t base = static_cast<int32_t>(base_value);
    const gp_Trsf location_transform = location.Transformation();
    const bool is_reversed = face.Orientation() == TopAbs_REVERSED;
    const bool has_uv = triangulation->HasUVNodes();
    for (int32_t node_index = 1; node_index <= triangulation->NbNodes(); ++node_index)
    {
      gp_Pnt point = triangulation->Node(node_index);
      point.Transform(location_transform);
      gp_Dir normal = triangulation->Normal(node_index);
      normal.Transform(location_transform);
      if (is_reversed) normal.Reverse();
      double u = 0.0;
      double v = 0.0;
      if (has_uv)
      {
        const gp_Pnt2d uv = triangulation->UVNode(node_index);
        u = uv.X();
        v = uv.Y();
      }
      data.Vertices.push_back({
        point.X(), point.Y(), point.Z(), normal.X(), normal.Y(), normal.Z(),
        u, v, has_uv ? 1 : 0 });
    }

    for (int32_t triangle_index = 1; triangle_index <= triangulation->NbTriangles(); ++triangle_index)
    {
      int node1 = 0;
      int node2 = 0;
      int node3 = 0;
      triangulation->Triangle(triangle_index).Get(node1, node2, node3);
      int32_t vertex_a = base + node1 - 1;
      int32_t vertex_b = base + node2 - 1;
      int32_t vertex_c = base + node3 - 1;
      if (is_reversed) std::swap(vertex_b, vertex_c);
      data.Triangles.push_back({ vertex_a, vertex_b, vertex_c, face_index, is_reversed ? 1 : 0 });
    }
  }
  return data;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  int32_t* out_vertex_count,
  int32_t* out_index_count)
{
  if (out_vertex_count == nullptr || out_index_count == nullptr)
  {
    SetLastError("The mesh count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_index_count = 0;
  return Guard([&]
  {
    MeshData data = BuildMesh(shape, linear_deflection, angular_deflection);
    if (data.Vertices.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max())
        || data.Indices.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The mesh is too large for the 32-bit ABI.");
    }
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_index_count = static_cast<int32_t>(data.Indices.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  OcctSharp_MeshVertex* vertices,
  const int32_t vertex_capacity,
  int32_t* out_vertex_count,
  int32_t* indices,
  const int32_t index_capacity,
  int32_t* out_index_count)
{
  if (out_vertex_count == nullptr || out_index_count == nullptr)
  {
    SetLastError("The mesh snapshot count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_index_count = 0;
  if (vertex_capacity < 0 || index_capacity < 0
      || (vertex_capacity > 0 && vertices == nullptr)
      || (index_capacity > 0 && indices == nullptr))
  {
    SetLastError("The mesh snapshot capacity or output buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    MeshData data = BuildMesh(shape, linear_deflection, angular_deflection);
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_index_count = static_cast<int32_t>(data.Indices.size());
    if (vertex_capacity < *out_vertex_count || index_capacity < *out_index_count)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The mesh snapshot buffer is too small.");
    }
    if (*out_vertex_count > 0)
    {
      std::memcpy(vertices, data.Vertices.data(), data.Vertices.size() * sizeof(OcctSharp_MeshVertex));
    }
    if (*out_index_count > 0)
    {
      std::memcpy(indices, data.Indices.data(), data.Indices.size() * sizeof(int32_t));
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_detailed_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  int32_t* out_vertex_count,
  int32_t* out_triangle_count,
  int32_t* out_face_count)
{
  if (out_vertex_count == nullptr || out_triangle_count == nullptr || out_face_count == nullptr)
  {
    SetLastError("A detailed-mesh count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_triangle_count = 0;
  *out_face_count = 0;
  return Guard([&]
  {
    DetailedMeshData data = BuildDetailedMesh(shape, linear_deflection, angular_deflection);
    if (data.Vertices.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max())
        || data.Triangles.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The detailed mesh exceeds the 32-bit ABI.");
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_triangle_count = static_cast<int32_t>(data.Triangles.size());
    *out_face_count = data.FaceCount;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_detailed_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection,
  const double angular_deflection,
  OcctSharp_DetailedMeshVertex* vertices,
  const int32_t vertex_capacity,
  int32_t* out_vertex_count,
  OcctSharp_DetailedMeshTriangle* triangles,
  const int32_t triangle_capacity,
  int32_t* out_triangle_count,
  int32_t* out_face_count)
{
  if (out_vertex_count == nullptr || out_triangle_count == nullptr || out_face_count == nullptr)
  {
    SetLastError("A detailed-mesh snapshot count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_triangle_count = 0;
  *out_face_count = 0;
  if (vertex_capacity < 0 || triangle_capacity < 0
      || (vertex_capacity > 0 && vertices == nullptr)
      || (triangle_capacity > 0 && triangles == nullptr))
  {
    SetLastError("The detailed-mesh snapshot capacity or output buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    DetailedMeshData data = BuildDetailedMesh(shape, linear_deflection, angular_deflection);
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_triangle_count = static_cast<int32_t>(data.Triangles.size());
    *out_face_count = data.FaceCount;
    if (vertex_capacity < *out_vertex_count || triangle_capacity < *out_triangle_count)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The detailed-mesh snapshot buffer is too small.");
    if (*out_vertex_count > 0)
      std::memcpy(vertices, data.Vertices.data(), data.Vertices.size() * sizeof(OcctSharp_DetailedMeshVertex));
    if (*out_triangle_count > 0)
      std::memcpy(triangles, data.Triangles.data(), data.Triangles.size() * sizeof(OcctSharp_DetailedMeshTriangle));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_advanced_mesh_count(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection, const double angular_deflection, const double minimum_size,
  const int32_t relative, const int32_t parallel, const int32_t internal_vertices,
  const int32_t control_surface_deflection,
  int32_t* out_vertex_count, int32_t* out_triangle_count, int32_t* out_face_count)
{
  if (out_vertex_count == nullptr || out_triangle_count == nullptr || out_face_count == nullptr)
  {
    SetLastError("An advanced-mesh count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_triangle_count = 0;
  *out_face_count = 0;
  return Guard([&]
  {
    DetailedMeshData data = BuildAdvancedMesh(
      shape, linear_deflection, angular_deflection, minimum_size,
      relative != 0, parallel != 0, internal_vertices != 0,
      control_surface_deflection != 0);
    if (data.Vertices.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max())
        || data.Triangles.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The advanced mesh exceeds the 32-bit ABI.");
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_triangle_count = static_cast<int32_t>(data.Triangles.size());
    *out_face_count = data.FaceCount;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_advanced_mesh_snapshot(
  const OcctSharp_ShapeHandle* shape,
  const double linear_deflection, const double angular_deflection, const double minimum_size,
  const int32_t relative, const int32_t parallel, const int32_t internal_vertices,
  const int32_t control_surface_deflection,
  OcctSharp_DetailedMeshVertex* vertices, const int32_t vertex_capacity,
  int32_t* out_vertex_count,
  OcctSharp_DetailedMeshTriangle* triangles, const int32_t triangle_capacity,
  int32_t* out_triangle_count, int32_t* out_face_count)
{
  if (out_vertex_count == nullptr || out_triangle_count == nullptr || out_face_count == nullptr)
  {
    SetLastError("An advanced-mesh snapshot count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_vertex_count = 0;
  *out_triangle_count = 0;
  *out_face_count = 0;
  if (vertex_capacity < 0 || triangle_capacity < 0
      || (vertex_capacity > 0 && vertices == nullptr)
      || (triangle_capacity > 0 && triangles == nullptr))
  {
    SetLastError("The advanced-mesh snapshot capacity or output buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    DetailedMeshData data = BuildAdvancedMesh(
      shape, linear_deflection, angular_deflection, minimum_size,
      relative != 0, parallel != 0, internal_vertices != 0,
      control_surface_deflection != 0);
    *out_vertex_count = static_cast<int32_t>(data.Vertices.size());
    *out_triangle_count = static_cast<int32_t>(data.Triangles.size());
    *out_face_count = data.FaceCount;
    if (vertex_capacity < *out_vertex_count || triangle_capacity < *out_triangle_count)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The advanced-mesh snapshot buffer is too small.");
    if (*out_vertex_count > 0)
      std::memcpy(vertices, data.Vertices.data(), data.Vertices.size() * sizeof(OcctSharp_DetailedMeshVertex));
    if (*out_triangle_count > 0)
      std::memcpy(triangles, data.Triangles.data(), data.Triangles.size() * sizeof(OcctSharp_DetailedMeshTriangle));
  });
}
