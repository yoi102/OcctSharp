#pragma once

// Private native Exchange/ShapeExchange contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepMesh_IncrementalMesh.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <STEPControl_Reader.hxx>
#include <TCollection_AsciiString.hxx>
#include <TopoDS_Shape.hxx>
#include <string>
#include <utility>
#include <vector>

struct OcctSharp_StepReaderHandle
{
  STEPControl_Reader Reader;
  IFSelect_ReturnStatus ReadStatus = IFSelect_RetVoid;
  std::vector<std::string> LengthUnits;
  std::vector<std::string> AngleUnits;
  std::vector<std::string> SolidAngleUnits;
};

namespace OcctSharp::Native
{
void ValidateStepReader(const OcctSharp_StepReaderHandle* reader);

const std::vector<std::string>& StepReaderUnitList(
  const OcctSharp_StepReaderHandle* reader, const int32_t unit_kind);

template <typename TProvider>
OcctSharp_Status ReadMeshExchangeShape(
  const char* file_path, OcctSharp_ShapeHandle** out_shape, TProvider& provider,
  const char* failure_message)
{
  if (out_shape == nullptr) { SetLastError("The mesh exchange read output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    TopoDS_Shape shape;
    if (!provider.Read(TCollection_AsciiString(file_path), shape) || shape.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, failure_message);
    *out_shape = AllocateShape(std::move(shape));
  });
}

template <typename TProvider>
OcctSharp_Status WriteMeshExchangeShape(
  const OcctSharp_ShapeHandle* shape, const char* file_path, TProvider& provider,
  const char* failure_message)
{
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidatePath(file_path);
    BRepMesh_IncrementalMesh mesh(shape->Value, 0.1, false, 0.5, true);
    if (!mesh.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT meshing did not complete before mesh exchange.");
    if (!provider.Write(TCollection_AsciiString(file_path), shape->Value))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, failure_message);
  });
}
}
