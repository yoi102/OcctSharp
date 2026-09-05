// Native Exchange/ShapeExchange implementation. Public contracts and ownership are unchanged.
#include "Exchange/ShapeExchange.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <BRep_Builder.hxx>
#include <DEGLTF_Provider.hxx>
#include <DEOBJ_ConfigurationNode.hxx>
#include <DEOBJ_Provider.hxx>
#include <DEPLY_ConfigurationNode.hxx>
#include <DEPLY_Provider.hxx>
#include <DEVRML_ConfigurationNode.hxx>
#include <DEVRML_Provider.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <NCollection_Sequence.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
#include <Standard_Handle.hxx>
#include <StlAPI_Reader.hxx>
#include <StlAPI_Writer.hxx>
#include <TCollection_AsciiString.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <cstring>
#include <limits>
#include <memory>
#include <string>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
void ValidateStepReader(const OcctSharp_StepReaderHandle* reader)
{
  if (reader == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The STEP reader handle is null.");
  if (!IsLiveValue(reader, LiveStepReaders))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The STEP reader handle is invalid or already released.");
}

const std::vector<std::string>& StepReaderUnitList(
  const OcctSharp_StepReaderHandle* reader, const int32_t unit_kind)
{
  switch (unit_kind)
  {
    case 0: return reader->LengthUnits;
    case 1: return reader->AngleUnits;
    case 2: return reader->SolidAngleUnits;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP unit kind is outside the supported range.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_brep(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The BREP output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    BRep_Builder builder;
    TopoDS_Shape shape;
    if (!BRepTools::Read(shape, file_path, builder) || shape.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read the BREP file.");
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    STEPControl_Reader reader;
    if (reader.ReadFile(file_path) != IFSelect_RetDone)
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read the STEP file.");
    }

    if (reader.TransferRoots() <= 0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The STEP file produced no transferable roots.");
    }

    TopoDS_Shape shape = reader.OneShape();
    if (shape.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The STEP transfer produced a null shape.");
    }

    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_iges(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The IGES output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    IGESControl_Reader reader;
    if (reader.ReadFile(file_path) != IFSelect_RetDone)
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read the IGES file.");
    }
    if (reader.TransferRoots() <= 0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The IGES file produced no transferable roots.");
    }
    TopoDS_Shape shape = reader.OneShape();
    if (shape.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The IGES transfer produced a null shape.");
    }
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step_report(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape,
  OcctSharp_StepReadReport* out_report)
{
  if (out_shape == nullptr || out_report == nullptr)
  {
    SetLastError("A STEP report output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  *out_report = {};
  return Guard([&]
  {
    ValidatePath(file_path);
    STEPControl_Reader reader;
    const IFSelect_ReturnStatus readStatus = reader.ReadFile(file_path);
    out_report->read_status = static_cast<int32_t>(readStatus);
    if (readStatus != IFSelect_RetDone)
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read the STEP file.");
    out_report->candidate_root_count = reader.NbRootsForTransfer();
    out_report->system_length_unit = reader.SystemLengthUnit();
    out_report->transferred_root_count = reader.TransferRoots();
    out_report->shape_count = reader.NbShapes();
    if (out_report->transferred_root_count <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The STEP file produced no transferable roots.");
    TopoDS_Shape shape = reader.OneShape();
    if (shape.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The STEP transfer produced a null shape.");
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_open(
  const char* file_path, const double target_system_length_unit,
  OcctSharp_StepReaderHandle** out_reader, OcctSharp_StepReaderInfo* out_info)
{
  if (out_reader == nullptr || out_info == nullptr)
  { SetLastError("The STEP reader output pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_reader = nullptr;
  *out_info = {};
  if (!std::isfinite(target_system_length_unit) || target_system_length_unit < 0.0)
  { SetLastError("The target system length unit must be zero or a positive finite value."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidatePath(file_path);
    std::unique_ptr<OcctSharp_StepReaderHandle> reader(new OcctSharp_StepReaderHandle());
    reader->ReadStatus = reader->Reader.ReadFile(file_path);
    if (reader->ReadStatus != IFSelect_RetDone)
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not open the STEP reader session.");
    if (target_system_length_unit > 0.0)
      reader->Reader.SetSystemLengthUnit(target_system_length_unit);

    NCollection_Sequence<TCollection_AsciiString> length_units;
    NCollection_Sequence<TCollection_AsciiString> angle_units;
    NCollection_Sequence<TCollection_AsciiString> solid_angle_units;
    reader->Reader.FileUnits(length_units, angle_units, solid_angle_units);
    for (NCollection_Sequence<TCollection_AsciiString>::Iterator iterator(length_units); iterator.More(); iterator.Next())
      reader->LengthUnits.emplace_back(iterator.Value().ToCString());
    for (NCollection_Sequence<TCollection_AsciiString>::Iterator iterator(angle_units); iterator.More(); iterator.Next())
      reader->AngleUnits.emplace_back(iterator.Value().ToCString());
    for (NCollection_Sequence<TCollection_AsciiString>::Iterator iterator(solid_angle_units); iterator.More(); iterator.Next())
      reader->SolidAngleUnits.emplace_back(iterator.Value().ToCString());

    *out_info = {
      reader->Reader.NbRootsForTransfer(),
      static_cast<int32_t>(reader->ReadStatus),
      reader->Reader.SystemLengthUnit(),
      static_cast<int32_t>(reader->LengthUnits.size()),
      static_cast<int32_t>(reader->AngleUnits.size()),
      static_cast<int32_t>(reader->SolidAngleUnits.size()) };
    *out_reader = AllocateValue(reader.release(), LiveStepReaders);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_unit_utf8_length(
  const OcctSharp_StepReaderHandle* reader, const int32_t unit_kind, const int32_t unit_index,
  int32_t* out_length)
{
  if (out_length == nullptr) { SetLastError("The STEP unit length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_length = 0;
  return Guard([&]
  {
    ValidateStepReader(reader);
    const std::vector<std::string>& units = StepReaderUnitList(reader, unit_kind);
    if (unit_index < 0 || unit_index >= static_cast<int32_t>(units.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP unit index is outside the available range.");
    if (units[unit_index].size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP unit name exceeds the supported buffer size.");
    *out_length = static_cast<int32_t>(units[unit_index].size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_unit_to_utf8(
  const OcctSharp_StepReaderHandle* reader, const int32_t unit_kind, const int32_t unit_index,
  char* buffer, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The STEP unit written output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    ValidateStepReader(reader);
    const std::vector<std::string>& units = StepReaderUnitList(reader, unit_kind);
    if (unit_index < 0 || unit_index >= static_cast<int32_t>(units.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP unit index is outside the available range.");
    const int32_t required = static_cast<int32_t>(units[unit_index].size());
    ValidateOutputBuffer(buffer, capacity, required);
    if (required > 0) std::memcpy(buffer, units[unit_index].data(), required);
    *out_written = required;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_reader_transfer_root(
  OcctSharp_StepReaderHandle* reader, const int32_t root_index,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The STEP root output shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateStepReader(reader);
    const int32_t root_count = reader->Reader.NbRootsForTransfer();
    if (root_index < 0 || root_index >= root_count)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The zero-based STEP root index is outside the candidate range.");
    reader->Reader.ClearShapes();
    if (!reader->Reader.TransferRoot(root_index + 1))
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer the selected STEP root.");
    TopoDS_Shape shape = reader->Reader.OneShape();
    if (shape.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The selected STEP root produced a null shape.");
    *out_shape = AllocateShape(std::move(shape));
  });
}

void OCCTSHARP_CALL occtsharp_step_reader_release(OcctSharp_StepReaderHandle* reader)
{
  if (reader != nullptr && UnregisterValue(reader, LiveStepReaders)) delete reader;
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_stl(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The STL output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    StlAPI_Reader reader;
    TopoDS_Shape shape;
    if (!reader.Read(shape, file_path))
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read the STL file.");
    }
    if (shape.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The STL transfer produced a null shape.");
    }
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_obj(
  const char* file_path, OcctSharp_ShapeHandle** out_shape)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return ReadMeshExchangeShape(file_path, out_shape, provider, "OCCT OBJ transfer failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_gltf(
  const char* file_path, OcctSharp_ShapeHandle** out_shape)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return ReadMeshExchangeShape(file_path, out_shape, provider, "OCCT glTF transfer failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_vrml(
  const char* file_path, OcctSharp_ShapeHandle** out_shape)
{
  occ::handle<DEVRML_ConfigurationNode> node = new DEVRML_ConfigurationNode();
  DEVRML_Provider provider(node);
  return ReadMeshExchangeShape(file_path, out_shape, provider, "OCCT VRML transfer failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_step(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path)
{
  return Guard([&]
  {
    ValidateShape(shape);
    ValidatePath(file_path);
    STEPControl_Writer writer;
    if (writer.Transfer(shape->Value, STEPControl_AsIs) != IFSelect_RetDone)
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer the shape to STEP.");
    }

    if (writer.Write(file_path) != IFSelect_RetDone)
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the STEP file.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_brep(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path)
{
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidatePath(file_path);
    if (!BRepTools::Write(shape->Value, file_path))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the BREP file.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_stl(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path,
  const double linear_deflection,
  const double angular_deflection,
  const int32_t binary)
{
  if (!std::isfinite(linear_deflection) || linear_deflection <= 0.0
      || !std::isfinite(angular_deflection) || angular_deflection <= 0.0)
  {
    SetLastError("STL deflections must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateShape(shape);
    ValidatePath(file_path);
    BRepMesh_IncrementalMesh mesh(shape->Value, linear_deflection, false, angular_deflection, true);
    StlAPI_Writer writer;
    writer.ASCIIMode() = binary == 0;
    if (!writer.Write(shape->Value, file_path))
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the STL file.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_iges(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path)
{
  return Guard([&]
  {
    ValidateShape(shape);
    ValidatePath(file_path);
    IGESControl_Writer writer("MM", 1);
    if (!writer.AddShape(shape->Value))
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer the shape to IGES.");
    }

    if (!writer.Write(file_path))
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the IGES file.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_obj(
  const OcctSharp_ShapeHandle* shape, const char* file_path)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return WriteMeshExchangeShape(shape, file_path, provider, "OCCT OBJ write failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_ply(
  const OcctSharp_ShapeHandle* shape, const char* file_path)
{
  occ::handle<DEPLY_ConfigurationNode> node = new DEPLY_ConfigurationNode();
  DEPLY_Provider provider(node);
  return WriteMeshExchangeShape(shape, file_path, provider, "OCCT PLY write failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_gltf(
  const OcctSharp_ShapeHandle* shape, const char* file_path)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return WriteMeshExchangeShape(shape, file_path, provider, "OCCT glTF write failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_vrml(
  const OcctSharp_ShapeHandle* shape, const char* file_path)
{
  occ::handle<DEVRML_ConfigurationNode> node = new DEVRML_ConfigurationNode();
  DEVRML_Provider provider(node);
  return WriteMeshExchangeShape(shape, file_path, provider, "OCCT VRML write failed.");
}
