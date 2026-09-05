#include "Mesh/MeshAuthoring.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Documents/Lifecycle.hxx"
#include <BRep_Builder.hxx>
#include <OSD_Path.hxx>
#include <RWMesh_CoordinateSystemConverter.hxx>
#include <RWObj_TriangulationReader.hxx>
#include <RWObj_CafWriter.hxx>
#include <RWGltf_CafWriter.hxx>
#include <RWPly_CafWriter.hxx>
#include <NCollection_IndexedDataMap.hxx>
#include <RWStl.hxx>
#include <TopoDS_Face.hxx>
#include <algorithm>
#include <cmath>
#include <filesystem>
#include <fstream>
#include <set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::MeshAuthoring;

namespace {
// A call-local specialization retains actual channel presence; OCCT's default reader
// substitutes +Z for some missing normals, which must not be reported as authored data.
class CopiedObjReader final : public RWObj_TriangulationReader {
public:
  std::set<int> UvNodes, NormalNodes;
  int Disclosures = 0;
  occ::handle<Poly_Triangulation> GetTriangulation() override {
    // The SDK warns and substitutes (0,0) UVs / +Z normals for invalid indices.
    // Inspect its parsed index map before accepting any such invented channel data.
    for (decltype(myPackedIndices)::Iterator item(myPackedIndices); item.More(); item.Next()) {
      const auto& indices = item.Key();
      Require(indices[1] == -1 || (indices[1] >= myObjVertsUV.Lower() && indices[1] <= myObjVertsUV.Upper()),
        "OBJ UV index is out of range; default UV substitution is not accepted.");
      Require(indices[2] == -1 || (indices[2] >= myObjNorms.Lower() && indices[2] <= myObjNorms.Upper()),
        "OBJ normal index is out of range; default normal substitution is not accepted.");
    }
    auto mesh = RWObj_TriangulationReader::GetTriangulation();
    if (mesh.IsNull()) return mesh;
    if (!UvNodes.empty() && static_cast<int>(UvNodes.size()) != mesh->NbNodes()) { mesh->RemoveUVNodes(); Disclosures |= 1; }
    if (!NormalNodes.empty()) {
      mesh->AddNormals();
      for (int i = 1; i <= mesh->NbNodes(); ++i) {
        NCollection_Vec3<float> normal(0, 0, 0);
        if (NormalNodes.contains(i)) normal = myNormals.Value(i - 1);
        const float scale = std::max({std::abs(normal.x()), std::abs(normal.y()), std::abs(normal.z())});
        if (scale > 0) { normal /= scale; normal.Normalize(); } else Disclosures |= 2;
        mesh->SetNormal(i, normal);
      }
    }
    return mesh;
  }
protected:
  void setNodeNormal(int index, const NCollection_Vec3<float>& normal) override {
    Require(std::isfinite(normal.x()) && std::isfinite(normal.y()) && std::isfinite(normal.z()), "OBJ normal is not finite.");
    NormalNodes.insert(index); RWObj_TriangulationReader::setNodeNormal(index, normal);
  }
  void setNodeUV(int index, const NCollection_Vec2<float>& uv) override {
    Require(std::isfinite(uv.x()) && std::isfinite(uv.y()), "OBJ UV is not finite.");
    UvNodes.insert(index); RWObj_TriangulationReader::setNodeUV(index, uv);
  }
  int addNode(const gp_Pnt& point) override {
    Require(myNodes.Length() < MaximumElements && std::isfinite(point.X()) && std::isfinite(point.Y()) && std::isfinite(point.Z()), "OBJ positions are invalid or excessive.");
    return RWObj_TriangulationReader::addNode(point);
  }
  void addElement(int a, int b, int c, int d) override {
    Require(myTriangles.Length() < MaximumElements - 1, "OBJ triangle count exceeds the mesh limit.");
    RWObj_TriangulationReader::addElement(a, b, c, d);
  }
};
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_read_editable(const char* path, int32_t format, int64_t maximumBytes,
  OcctSharp_ShapeHandle** output, int32_t* disclosures) {
  if (output) *output = nullptr;
  if (disclosures) *disclosures = 0;
  return Guard([&] {
    ValidatePath(path); Require(output && disclosures && (format == 0 || format == 1) && maximumBytes > 0 && maximumBytes <= 268435456,
      "Invalid editable import request or byte limit.");
    auto size = std::filesystem::file_size(path);
    Require(size > 0 && size <= static_cast<uint64_t>(maximumBytes), "Editable mesh file exceeds the caller's byte limit or is empty.");
    occ::handle<Poly_Triangulation> mesh;
    if (format == 0) {
      std::ifstream input(path, std::ios::binary); char header[84]{}; input.read(header, 84);
      if (input.gcount() == 84) {
        uint32_t count = static_cast<unsigned char>(header[80]) | (static_cast<uint32_t>(static_cast<unsigned char>(header[81])) << 8) |
          (static_cast<uint32_t>(static_cast<unsigned char>(header[82])) << 16) | (static_cast<uint32_t>(static_cast<unsigned char>(header[83])) << 24);
        bool binary = 84ULL + 50ULL * count == size || std::string(header, 5) != "solid";
        if (binary) Require(count <= MaximumElements && 84ULL + 50ULL * count == size, "Binary STL has an invalid or excessive facet count.");
      }
      mesh = RWStl::ReadFile(path); *disclosures = 4;
    } else {
      CopiedObjReader reader; reader.SetCreateShapes(false); reader.SetSinglePrecision(false);
      reader.SetMemoryLimit(static_cast<size_t>(std::min<int64_t>(maximumBytes * 8, 1073741824)));
      Require(reader.Read(TCollection_AsciiString(path), Message_ProgressRange()), "OCCT could not read editable OBJ.");
      mesh = reader.GetTriangulation(); *disclosures = reader.Disclosures | 8;
    }
    Require(!mesh.IsNull() && mesh->NbNodes() > 0 && mesh->NbNodes() <= MaximumElements && mesh->NbTriangles() > 0 &&
      mesh->NbTriangles() <= MaximumElements, "Reader returned no usable bounded triangulation.");
    TopoDS_Face face; BRep_Builder builder; builder.MakeFace(face, mesh); *output = AllocateShape(face);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_write_stl(const OcctSharp_AuthoredVertex* vertices, int32_t vertexCount,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangleCount, const char* path, int32_t binary) {
  return Guard([&] {
    ValidatePath(path); Require(binary == 0 || binary == 1, "STL binary flag must be Boolean.");
    auto mesh = Build(vertices, vertexCount, triangles, triangleCount); OSD_Path file(path);
    bool written = binary ? RWStl::WriteBinary(mesh, file) : RWStl::WriteAscii(mesh, file);
    if (!written) throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the authored STL mesh.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_convert_coordinates(const OcctSharp_AuthoredVertex* vertices, int32_t count,
  double sourceUnit, int32_t sourceUp, int32_t sourceLeft, double targetUnit, int32_t targetUp, int32_t targetLeft,
  OcctSharp_AuthoredVertex* output, int32_t capacity) {
  return Guard([&] {
    ValidateVertices(vertices, count);
    Require(std::isfinite(sourceUnit) && sourceUnit > 0 && std::isfinite(targetUnit) && targetUnit > 0 &&
      (sourceUp == 0 || sourceUp == 1) && (targetUp == 0 || targetUp == 1) && (sourceLeft == 0 || sourceLeft == 1) &&
      (targetLeft == 0 || targetLeft == 1) && capacity >= count && (count == 0 || output), "Invalid coordinate conversion request.");
    RWMesh_CoordinateSystemConverter converter;
    converter.SetInputLengthUnit(sourceUnit); converter.SetOutputLengthUnit(targetUnit);
    converter.SetInputCoordinateSystem(sourceUp == 0 ? RWMesh_CoordinateSystem_posYfwd_posZup : RWMesh_CoordinateSystem_negZfwd_posYup);
    converter.SetOutputCoordinateSystem(targetUp == 0 ? RWMesh_CoordinateSystem_posYfwd_posZup : RWMesh_CoordinateSystem_negZfwd_posYup);
    std::vector<OcctSharp_AuthoredVertex> result; result.reserve(count);
    for (int i = 0; i < count; ++i) {
      auto v = vertices[i]; gp_XYZ p(sourceLeft ? -v.x : v.x, v.y, v.z); converter.TransformPosition(p);
      v.x = targetLeft ? -p.X() : p.X(); v.y = p.Y(); v.z = p.Z();
      if (v.flags & 1) {
        double scale = std::max({std::abs(v.nx), std::abs(v.ny), std::abs(v.nz)});
        gp_XYZ n((sourceLeft ? -v.nx : v.nx) / scale, v.ny / scale, v.nz / scale); n.Normalize();
        NCollection_Vec3<float> normal(static_cast<float>(n.X()), static_cast<float>(n.Y()), static_cast<float>(n.Z()));
        converter.TransformNormal(normal); normal.Normalize();
        v.nx = targetLeft ? -normal.x() : normal.x(); v.ny = normal.y(); v.nz = normal.z();
      }
      result.push_back(v);
    }
    ValidateVertices(result.data(), count); std::copy(result.begin(), result.end(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_write_document(const OcctSharp_OcafDocumentHandle* document,
  const char* path, int32_t format, int32_t binary, int32_t channels) {
  return Guard([&] {
    ValidateOcafDocument(document); ValidatePath(path);
    Require(format >= 1 && format <= 3 && (binary == 0 || binary == 1) && channels >= 0 && channels <= 3,
      "Unsupported discrete document format or channel flags.");
    Require(!document->Document->HasOpenCommand(), "Commit the document transaction before discrete export.");
    RWMesh_CoordinateSystemConverter converter; converter.SetInputLengthUnit(0.001);
    converter.SetInputCoordinateSystem(RWMesh_CoordinateSystem_posYfwd_posZup);
    converter.SetOutputLengthUnit(format == 2 ? 1.0 : 0.001);
    converter.SetOutputCoordinateSystem(format == 2 ? RWMesh_CoordinateSystem_negZfwd_posYup : RWMesh_CoordinateSystem_posYfwd_posZup);
    NCollection_IndexedDataMap<TCollection_AsciiString, TCollection_AsciiString> metadata;
    bool written = false;
    if (format == 1) {
      RWObj_CafWriter writer{TCollection_AsciiString(path)}; writer.SetCoordinateSystemConverter(converter);
      written = writer.Perform(document->Document, metadata, Message_ProgressRange());
    } else if (format == 2) {
      RWGltf_CafWriter writer(TCollection_AsciiString(path), binary != 0); writer.SetCoordinateSystemConverter(converter);
      writer.SetForcedUVExport(true);
      written = writer.Perform(document->Document, metadata, Message_ProgressRange());
    } else {
      RWPly_CafWriter writer{TCollection_AsciiString(path)}; writer.SetCoordinateSystemConverter(converter);
      writer.SetDoublePrecision(true); writer.SetNormals((channels & 1) != 0); writer.SetTexCoords((channels & 2) != 0);
      writer.SetColors(true); writer.SetPartId(true);
      written = writer.Perform(document->Document, metadata, Message_ProgressRange());
    }
    if (!written) throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the existing discrete document triangulation.");
  });
}
