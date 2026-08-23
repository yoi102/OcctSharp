#include "OcctSharp.Native.h"
#include "OcctSharp.Native.Internal.hxx"

#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Writer.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_DynamicArray.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_StepModelType.hxx>
#include <STEPControl_Writer.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <Standard_TypeDef.hxx>
#include <Standard_Failure.hxx>
#include <Standard_Handle.hxx>
#include <Standard_Type.hxx>
#include <Standard_Version.hxx>
#include <Standard_Transient.hxx>
#include <StlAPI_Writer.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_AsciiString.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TDocStd_Document.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp_Explorer.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc.hxx>
#include <gp_Ax1.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <gp_Mat.hxx>

#include <cmath>
#include <cstddef>
#include <cstring>
#include <exception>
#include <new>
#include <string>
#include <type_traits>
#include <unordered_set>
#include <utility>
#include <mutex>

class OcctSharp_TransientDerived final : public Standard_Transient
{
  DEFINE_STANDARD_RTTI_INLINE(OcctSharp_TransientDerived, Standard_Transient)
};

static_assert(sizeof(Standard_Integer) == sizeof(int32_t));
static_assert(sizeof(Standard_Real) == sizeof(double));
static_assert(sizeof(Standard_Boolean) == sizeof(bool));
static_assert(sizeof(std::underlying_type_t<TopAbs_ShapeEnum>) == sizeof(int32_t));
static_assert(sizeof(OcctSharp_StepAssemblyInput) == 64);
static_assert(alignof(OcctSharp_StepAssemblyInput) == 8);
static_assert(offsetof(OcctSharp_StepAssemblyInput, file_path) == 0);
static_assert(offsetof(OcctSharp_StepAssemblyInput, translation_x) == 8);
static_assert(offsetof(OcctSharp_StepAssemblyInput, rotation_angle_radians) == 56);

struct OcctSharp_ShapeHandle
{
  explicit OcctSharp_ShapeHandle(TopoDS_Shape shape)
    : Value(std::move(shape))
  {
  }

  TopoDS_Shape Value;
};

struct OcctSharp_TransientHandle
{
  explicit OcctSharp_TransientHandle(opencascade::handle<Standard_Transient> value)
    : Value(std::move(value))
  {
  }

  opencascade::handle<Standard_Transient> Value;
};

struct OcctSharp_TrsfHandle
{
  explicit OcctSharp_TrsfHandle(gp_Trsf value)
    : Value(std::move(value))
  {
  }

  gp_Trsf Value;
};

struct OcctSharp_LocationHandle
{
  explicit OcctSharp_LocationHandle(TopLoc_Location value)
    : Value(std::move(value))
  {
  }

  TopLoc_Location Value;
};

struct OcctSharp_VecHandle { explicit OcctSharp_VecHandle(gp_Vec value) : Value(std::move(value)) {} gp_Vec Value; };
struct OcctSharp_DirHandle { explicit OcctSharp_DirHandle(gp_Dir value) : Value(std::move(value)) {} gp_Dir Value; };
struct OcctSharp_Ax1Handle { explicit OcctSharp_Ax1Handle(gp_Ax1 value) : Value(std::move(value)) {} gp_Ax1 Value; };
struct OcctSharp_MatHandle { explicit OcctSharp_MatHandle(gp_Mat value) : Value(std::move(value)) {} gp_Mat Value; };
struct OcctSharp_AsciiStringHandle { explicit OcctSharp_AsciiStringHandle(TCollection_AsciiString value) : Value(std::move(value)) {} TCollection_AsciiString Value; };
struct OcctSharp_ExtendedStringHandle { explicit OcctSharp_ExtendedStringHandle(TCollection_ExtendedString value) : Value(std::move(value)) {} TCollection_ExtendedString Value; };
struct OcctSharp_RealSequenceHandle { explicit OcctSharp_RealSequenceHandle(NCollection_Sequence<double> value) : Value(std::move(value)) {} NCollection_Sequence<double> Value; };
struct OcctSharp_RealArrayHandle { explicit OcctSharp_RealArrayHandle(NCollection_Array1<double> value) : Value(std::move(value)) {} NCollection_Array1<double> Value; };
struct OcctSharp_RealVectorHandle { explicit OcctSharp_RealVectorHandle(NCollection_DynamicArray<double> value) : Value(std::move(value)) {} NCollection_DynamicArray<double> Value; };
struct OcctSharp_IntRealMapHandle { explicit OcctSharp_IntRealMapHandle(NCollection_DataMap<int32_t, double> value) : Value(std::move(value)) {} NCollection_DataMap<int32_t, double> Value; };
struct OcctSharp_IntIndexedMapHandle { explicit OcctSharp_IntIndexedMapHandle(NCollection_IndexedMap<int32_t> value) : Value(std::move(value)) {} NCollection_IndexedMap<int32_t> Value; };

namespace
{
constexpr uint32_t AbiVersion = 0x00010011U;
constexpr const char* BridgeVersion = "0.18.0";
thread_local std::string LastError;
std::mutex LiveShapesMutex;
std::unordered_set<const OcctSharp_ShapeHandle*> LiveShapes;
std::unordered_set<const OcctSharp_TransientHandle*> LiveTransients;
std::unordered_set<const OcctSharp_TrsfHandle*> LiveTransforms;
std::unordered_set<const OcctSharp_LocationHandle*> LiveLocations;
std::unordered_set<const OcctSharp_VecHandle*> LiveVectors;
std::unordered_set<const OcctSharp_DirHandle*> LiveDirections;
std::unordered_set<const OcctSharp_Ax1Handle*> LiveAxes;
std::unordered_set<const OcctSharp_MatHandle*> LiveMatrices;
std::unordered_set<const OcctSharp_AsciiStringHandle*> LiveAsciiStrings;
std::unordered_set<const OcctSharp_ExtendedStringHandle*> LiveExtendedStrings;
std::unordered_set<const OcctSharp_RealSequenceHandle*> LiveRealSequences;
std::unordered_set<const OcctSharp_RealArrayHandle*> LiveRealArrays;
std::unordered_set<const OcctSharp_RealVectorHandle*> LiveRealVectors;
std::unordered_set<const OcctSharp_IntRealMapHandle*> LiveIntRealMaps;
std::unordered_set<const OcctSharp_IntIndexedMapHandle*> LiveIntIndexedMaps;

class OperationFailure final : public std::runtime_error
{
public:
  OperationFailure(const OcctSharp_Status status, const char* message)
    : std::runtime_error(message), Status(status)
  {
  }

  OcctSharp_Status Status;
};

void SetLastError(const char* message)
{
  LastError = message == nullptr ? "Unknown native error." : message;
}

void RegisterShape(OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveShapes.insert(shape);
}

bool IsLiveShape(const OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveShapes.contains(shape);
}

bool UnregisterShape(const OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveShapes.erase(shape) != 0;
}

OcctSharp_ShapeHandle* AllocateShape(TopoDS_Shape shape)
{
  OcctSharp_ShapeHandle* handle = new OcctSharp_ShapeHandle(std::move(shape));
  try
  {
    RegisterShape(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void RegisterTransient(OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveTransients.insert(handle);
}

bool IsLiveTransient(const OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransients.contains(handle);
}

bool UnregisterTransient(const OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransients.erase(handle) != 0;
}

OcctSharp_TransientHandle* AllocateTransient(opencascade::handle<Standard_Transient> value)
{
  OcctSharp_TransientHandle* handle = new OcctSharp_TransientHandle(std::move(value));
  try
  {
    RegisterTransient(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void RegisterTransform(OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveTransforms.insert(handle);
}

bool IsLiveTransform(const OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransforms.contains(handle);
}

bool UnregisterTransform(const OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransforms.erase(handle) != 0;
}

OcctSharp_TrsfHandle* AllocateTransform(gp_Trsf value)
{
  OcctSharp_TrsfHandle* handle = new OcctSharp_TrsfHandle(std::move(value));
  try
  {
    RegisterTransform(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void RegisterLocation(OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveLocations.insert(handle);
}

bool IsLiveLocation(const OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveLocations.contains(handle);
}

bool UnregisterLocation(const OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveLocations.erase(handle) != 0;
}

OcctSharp_LocationHandle* AllocateLocation(TopLoc_Location value)
{
  OcctSharp_LocationHandle* handle = new OcctSharp_LocationHandle(std::move(value));
  try
  {
    RegisterLocation(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

template <typename T>
void RegisterValue(T* handle, std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  live.insert(handle);
}

template <typename T>
bool IsLiveValue(const T* handle, const std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return live.contains(handle);
}

template <typename T>
bool UnregisterValue(const T* handle, std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return live.erase(handle) != 0;
}

template <typename T>
T* AllocateValue(T* handle, std::unordered_set<const T*>& live)
{
  try
  {
    RegisterValue(handle, live);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void ValidateVector(const OcctSharp_VecHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The vector handle is null.");
  if (!IsLiveValue(handle, LiveVectors)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The vector handle is invalid or already released.");
}

void ValidateDirection(const OcctSharp_DirHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The direction handle is null.");
  if (!IsLiveValue(handle, LiveDirections)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The direction handle is invalid or already released.");
}

void ValidateAxis(const OcctSharp_Ax1Handle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The axis handle is null.");
  if (!IsLiveValue(handle, LiveAxes)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The axis handle is invalid or already released.");
}

void ValidateMatrix(const OcctSharp_MatHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The matrix handle is null.");
  if (!IsLiveValue(handle, LiveMatrices)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The matrix handle is invalid or already released.");
}

void ValidateAsciiString(const OcctSharp_AsciiStringHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The ASCII string handle is null.");
  if (!IsLiveValue(handle, LiveAsciiStrings)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The ASCII string handle is invalid or already released.");
}

void ValidateExtendedString(const OcctSharp_ExtendedStringHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The extended string handle is null.");
  if (!IsLiveValue(handle, LiveExtendedStrings)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The extended string handle is invalid or already released.");
}

void ValidateRealSequence(const OcctSharp_RealSequenceHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real sequence handle is null.");
  if (!IsLiveValue(handle, LiveRealSequences)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real sequence handle is invalid or already released.");
}

void ValidateRealArray(const OcctSharp_RealArrayHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real array handle is null.");
  if (!IsLiveValue(handle, LiveRealArrays)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real array handle is invalid or already released.");
}

void ValidateRealVector(const OcctSharp_RealVectorHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real vector handle is null.");
  if (!IsLiveValue(handle, LiveRealVectors)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real vector handle is invalid or already released.");
}

void ValidateIntRealMap(const OcctSharp_IntRealMapHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The integer-real map handle is null.");
  if (!IsLiveValue(handle, LiveIntRealMaps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The integer-real map handle is invalid or already released.");
}

void ValidateIntIndexedMap(const OcctSharp_IntIndexedMapHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The integer indexed map handle is null.");
  if (!IsLiveValue(handle, LiveIntIndexedMaps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The integer indexed map handle is invalid or already released.");
}

void ValidateUtf8Input(const char* utf8, const int32_t length)
{
  if (length < 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "UTF-8 length cannot be negative.");
  if (length > 0 && utf8 == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "UTF-8 input is null for a non-empty string.");
}

TCollection_AsciiString MakeAsciiString(const char* utf8, const int32_t length)
{
  return length == 0 ? TCollection_AsciiString() : TCollection_AsciiString(utf8, length);
}

void ValidateOutputBuffer(char* buffer, const int32_t capacity, const int32_t required)
{
  if (capacity < required) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output buffer is too small.");
  if (required > 0 && buffer == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output buffer is null.");
}

void ValidateSequenceIndex(const OcctSharp_RealSequenceHandle* sequence, const int32_t index)
{
  const int32_t length = sequence->Value.Length();
  if (index < 1 || index > length) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sequence indices are 1-based and must be within the sequence length.");
}

void ValidateArrayIndex(const OcctSharp_RealArrayHandle* array, const int32_t index)
{
  if (index < array->Value.Lower() || index > array->Value.Upper()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Array indices are outside the native lower and upper bounds.");
}

void ValidateVectorIndex(const OcctSharp_RealVectorHandle* vector, const int32_t index)
{
  if (index < 0 || index >= vector->Value.Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Vector indices are zero-based and must be within the vector length.");
}
template <typename TAction>
OcctSharp_Status Guard(TAction&& action)
{
  LastError.clear();

  try
  {
    action();
    return OCCTSHARP_STATUS_SUCCESS;
  }
  catch (const Standard_Failure& error)
  {
    SetLastError(error.GetMessageString());
    return OCCTSHARP_STATUS_OCCT_FAILURE;
  }
  catch (const OperationFailure& error)
  {
    SetLastError(error.what());
    return error.Status;
  }
  catch (const std::exception& error)
  {
    SetLastError(error.what());
    return OCCTSHARP_STATUS_STANDARD_EXCEPTION;
  }
  catch (...)
  {
    SetLastError("Unknown C++ exception.");
    return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION;
  }
}

void ValidatePath(const char* filePath)
{
  if (filePath == nullptr || filePath[0] == '\0')
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The file path is null or empty.");
  }
}

void ValidateShape(const OcctSharp_ShapeHandle* shape)
{
  if (shape == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The shape handle is null.");
  }

  if (!IsLiveShape(shape))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The shape handle is invalid or already released.");
  }
}

void ValidateTransient(const OcctSharp_TransientHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The transient handle is null.");
  }

  if (!IsLiveTransient(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The transient handle is invalid or already released.");
  }
}

void ValidateTransformHandle(const OcctSharp_TrsfHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The transform handle is null.");
  }

  if (!IsLiveTransform(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The transform handle is invalid or already released.");
  }
}

void ValidateLocationHandle(const OcctSharp_LocationHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The location handle is null.");
  }

  if (!IsLiveLocation(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The location handle is invalid or already released.");
  }
}

void ValidateFinite(double value, const char* name)
{
  if (!std::isfinite(value))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, name);
  }
}

gp_Trsf CreateTranslationRotation(
  const double translationX,
  const double translationY,
  const double translationZ,
  const double axisX,
  const double axisY,
  const double axisZ,
  const double angle)
{
  ValidateFinite(translationX, "Translation X must be finite.");
  ValidateFinite(translationY, "Translation Y must be finite.");
  ValidateFinite(translationZ, "Translation Z must be finite.");
  ValidateFinite(axisX, "Rotation axis X must be finite.");
  ValidateFinite(axisY, "Rotation axis Y must be finite.");
  ValidateFinite(axisZ, "Rotation axis Z must be finite.");
  ValidateFinite(angle, "Rotation angle must be finite.");
  const double axisMagnitudeSquared = axisX * axisX + axisY * axisY + axisZ * axisZ;
  if (!std::isfinite(axisMagnitudeSquared) || axisMagnitudeSquared <= 0.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Rotation axis must be finite and non-zero.");
  }

  gp_Trsf transform;
  if (angle != 0.0)
  {
    transform.SetRotation(
      gp_Ax1(gp_Pnt(0.0, 0.0, 0.0), gp_Dir(axisX, axisY, axisZ)), angle);
  }
  transform.SetTranslationPart(gp_Vec(translationX, translationY, translationZ));
  return transform;
}

void ValidateTransform(const OcctSharp_StepAssemblyInput& input)
{
  const double axisMagnitudeSquared = input.rotation_axis_x * input.rotation_axis_x
    + input.rotation_axis_y * input.rotation_axis_y
    + input.rotation_axis_z * input.rotation_axis_z;
  if (!std::isfinite(input.translation_x)
      || !std::isfinite(input.translation_y)
      || !std::isfinite(input.translation_z)
      || !std::isfinite(input.rotation_angle_radians)
      || !std::isfinite(axisMagnitudeSquared)
      || axisMagnitudeSquared <= 0.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "STEP assembly transforms must be finite and use a non-zero rotation axis.");
  }
}

gp_Trsf CreateTransform(const OcctSharp_StepAssemblyInput& input)
{
  ValidateTransform(input);
  gp_Trsf transform;
  if (input.rotation_angle_radians != 0.0)
  {
    transform.SetRotation(
      gp_Ax1(
        gp_Pnt(0.0, 0.0, 0.0),
        gp_Dir(input.rotation_axis_x, input.rotation_axis_y, input.rotation_axis_z)),
      input.rotation_angle_radians);
  }
  transform.SetTranslationPart(
    gp_Vec(input.translation_x, input.translation_y, input.translation_z));
  return transform;
}

occ::handle<TDocStd_Document> CreateXdeDocument()
{
  occ::handle<TDocStd_Document> document =
    new TDocStd_Document(TCollection_ExtendedString("BinXCAF"));
  XCAFDoc_DocumentTool::Set(document->Main());
  return document;
}

void InitializeXdeTools(const occ::handle<TDocStd_Document>& document)
{
  const TDF_Label& main = document->Main();
  XCAFDoc_DocumentTool::ShapeTool(main);
  XCAFDoc_DocumentTool::ColorTool(main);
  XCAFDoc_DocumentTool::LayerTool(main);
  XCAFDoc_DocumentTool::DimTolTool(main);
  XCAFDoc_DocumentTool::MaterialTool(main);
  XCAFDoc_DocumentTool::VisMaterialTool(main);
  XCAFDoc_DocumentTool::ViewTool(main);
}

void ConfigureXdeReader(STEPCAFControl_Reader& reader)
{
  reader.SetColorMode(true);
  reader.SetNameMode(true);
  reader.SetLayerMode(true);
  reader.SetPropsMode(true);
  reader.SetMetaMode(true);
  reader.SetProductMetaMode(true);
  reader.SetSHUOMode(true);
  reader.SetGDTMode(true);
  reader.SetMatMode(true);
  reader.SetViewMode(true);
}

void ConfigureXdeWriter(STEPCAFControl_Writer& writer)
{
  writer.SetColorMode(true);
  writer.SetNameMode(true);
  writer.SetLayerMode(true);
  writer.SetPropsMode(true);
  writer.SetMetadataMode(true);
  writer.SetSHUOMode(true);
  writer.SetDimTolMode(true);
  writer.SetMaterialMode(true);
  writer.SetVisualMaterialMode(true);
}
}

void OcctSharp_Internal_SetLastError(const char* message)
{
  SetLastError(message);
}

OcctSharp_Status OcctSharp_Internal_TryGetShape(
  const OcctSharp_ShapeHandle* handle,
  const TopoDS_Shape** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The internal topology output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  if (handle == nullptr)
  {
    SetLastError("The shape handle is null.");
    return OCCTSHARP_STATUS_NULL_HANDLE;
  }

  if (!IsLiveShape(handle))
  {
    SetLastError("The shape handle is invalid or already released.");
    return OCCTSHARP_STATUS_INVALID_HANDLE;
  }

  *out_shape = &handle->Value;
  return OCCTSHARP_STATUS_SUCCESS;
}

OcctSharp_ShapeHandle* OcctSharp_Internal_AllocateShape(TopoDS_Shape shape)
{
  return AllocateShape(std::move(shape));
}

uint32_t OCCTSHARP_CALL occtsharp_get_abi_version(void)
{
  return AbiVersion;
}

const char* OCCTSHARP_CALL occtsharp_get_bridge_version(void)
{
  return BridgeVersion;
}

const char* OCCTSHARP_CALL occtsharp_get_occt_version(void)
{
  return OCC_VERSION_COMPLETE;
}

const char* OCCTSHARP_CALL occtsharp_get_last_error(void)
{
  return LastError.c_str();
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_box(
  const double size_x,
  const double size_y,
  const double size_z,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;

  if (!std::isfinite(size_x) || !std::isfinite(size_y) || !std::isfinite(size_z)
      || size_x <= 0.0 || size_y <= 0.0 || size_z <= 0.0)
  {
    SetLastError("Box dimensions must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    TopoDS_Shape shape = BRepPrimAPI_MakeBox(size_x, size_y, size_z).Shape();
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_get_face_count(
  const OcctSharp_ShapeHandle* shape,
  int32_t* out_face_count)
{
  if (shape == nullptr)
  {
    SetLastError("The shape handle is null.");
    return OCCTSHARP_STATUS_NULL_HANDLE;
  }

  if (out_face_count == nullptr)
  {
    SetLastError("The output face-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateShape(shape);
    int32_t count = 0;
    for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
    {
      ++count;
    }

    *out_face_count = count;
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform(
  const OcctSharp_ShapeHandle* shape,
  const double translation_x,
  const double translation_y,
  const double translation_z,
  const double rotation_axis_x,
  const double rotation_axis_y,
  const double rotation_axis_z,
  const double rotation_angle_radians,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  const double axisMagnitudeSquared = rotation_axis_x * rotation_axis_x
    + rotation_axis_y * rotation_axis_y
    + rotation_axis_z * rotation_axis_z;
  if (!std::isfinite(translation_x) || !std::isfinite(translation_y) || !std::isfinite(translation_z)
      || !std::isfinite(rotation_angle_radians) || !std::isfinite(axisMagnitudeSquared)
      || axisMagnitudeSquared <= 0.0)
  {
    SetLastError("Transform values must be finite and the rotation axis must be non-zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateShape(shape);
    gp_Trsf transform = CreateTranslationRotation(
      translation_x,
      translation_y,
      translation_z,
      rotation_axis_x,
      rotation_axis_y,
      rotation_axis_z,
      rotation_angle_radians);
    TopoDS_Shape transformed = BRepBuilderAPI_Transform(shape->Value, transform, false, false).Shape();
    *out_shape = AllocateShape(std::move(transformed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_identity(
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    *out_transform = AllocateTransform(gp_Trsf());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_rotation(
  const double translation_x,
  const double translation_y,
  const double translation_z,
  const double rotation_axis_x,
  const double rotation_axis_y,
  const double rotation_axis_z,
  const double rotation_angle_radians,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    *out_transform = AllocateTransform(CreateTranslationRotation(
      translation_x,
      translation_y,
      translation_z,
      rotation_axis_x,
      rotation_axis_y,
      rotation_axis_z,
      rotation_angle_radians));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_clone(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(source);
    *out_transform = AllocateTransform(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_inverted(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(source);
    *out_transform = AllocateTransform(source->Value.Inverted());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_multiplied(
  const OcctSharp_TrsfHandle* left,
  const OcctSharp_TrsfHandle* right,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(left);
    ValidateTransformHandle(right);
    *out_transform = AllocateTransform(left->Value.Multiplied(right->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_value(
  const OcctSharp_TrsfHandle* transform,
  const int32_t row,
  const int32_t column,
  double* out_value)
{
  if (out_value == nullptr)
  {
    SetLastError("The output transform value pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_value = 0.0;
  return Guard([&]
  {
    ValidateTransformHandle(transform);
    if (row < 1 || row > 3 || column < 1 || column > 4)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Transform matrix indices must be row 1..3 and column 1..4.");
    }
    *out_value = transform->Value.Value(row, column);
  });
}

void OCCTSHARP_CALL occtsharp_trsf_release(OcctSharp_TrsfHandle* transform)
{
  if (transform != nullptr && UnregisterTransform(transform))
  {
    delete transform;
  }
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform_trsf(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_TrsfHandle* transform,
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
    ValidateShape(shape);
    ValidateTransformHandle(transform);
    TopoDS_Shape transformed = BRepBuilderAPI_Transform(shape->Value, transform->Value, false, false).Shape();
    *out_shape = AllocateShape(std::move(transformed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_identity(
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    *out_location = AllocateLocation(TopLoc_Location());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_from_trsf(
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(transform);
    *out_location = AllocateLocation(TopLoc_Location(transform->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_clone(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(source);
    *out_location = AllocateLocation(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_inverted(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(source);
    *out_location = AllocateLocation(source->Value.Inverted());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_multiplied(
  const OcctSharp_LocationHandle* left,
  const OcctSharp_LocationHandle* right,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(left);
    ValidateLocationHandle(right);
    *out_location = AllocateLocation(left->Value.Multiplied(right->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_is_identity(
  const OcctSharp_LocationHandle* location,
  int32_t* out_is_identity)
{
  if (out_is_identity == nullptr)
  {
    SetLastError("The output identity pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_is_identity = 0;
  return Guard([&]
  {
    ValidateLocationHandle(location);
    *out_is_identity = location->Value.IsIdentity() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_to_trsf(
  const OcctSharp_LocationHandle* location,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(location);
    *out_transform = AllocateTransform(location->Value.Transformation());
  });
}

void OCCTSHARP_CALL occtsharp_location_release(OcctSharp_LocationHandle* location)
{
  if (location != nullptr && UnregisterLocation(location))
  {
    delete location;
  }
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_located(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_LocationHandle* location,
  const int32_t moved,
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
    ValidateShape(shape);
    ValidateLocationHandle(location);
    TopoDS_Shape result = moved == 0
      ? shape->Value.Located(location->Value, false)
      : shape->Value.Moved(location->Value, false);
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_vec(
  const OcctSharp_VecHandle* vector,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr) { SetLastError("The output transform pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateVector(vector);
    *out_transform = AllocateTransform(gp_Trsf());
    (*out_transform)->Value.SetTranslation(vector->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_rotation_axis(
  const OcctSharp_Ax1Handle* axis,
  const double angle_radians,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr) { SetLastError("The output transform pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateAxis(axis);
    ValidateFinite(angle_radians, "Rotation angle must be finite.");
    gp_Trsf value;
    value.SetRotation(axis->Value, angle_radians);
    *out_transform = AllocateTransform(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_create(
  const double x, const double y, const double z, OcctSharp_VecHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&]
  {
    ValidateFinite(x, "Vector X must be finite."); ValidateFinite(y, "Vector Y must be finite."); ValidateFinite(z, "Vector Z must be finite.");
    *out_vector = AllocateValue(new OcctSharp_VecHandle(gp_Vec(x, y, z)), LiveVectors);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_clone(const OcctSharp_VecHandle* source, OcctSharp_VecHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr; return Guard([&] { ValidateVector(source); *out_vector = AllocateValue(new OcctSharp_VecHandle(source->Value), LiveVectors); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_components(const OcctSharp_VecHandle* vector, double* x, double* y, double* z)
{
  if (x == nullptr || y == nullptr || z == nullptr) { SetLastError("The vector component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(vector); *x = vector->Value.X(); *y = vector->Value.Y(); *z = vector->Value.Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_magnitude(const OcctSharp_VecHandle* vector, double* magnitude)
{
  if (magnitude == nullptr) { SetLastError("The vector magnitude output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(vector); *magnitude = vector->Value.Magnitude(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_dot(const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, double* dot)
{
  if (dot == nullptr) { SetLastError("The vector dot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(left); ValidateVector(right); *dot = left->Value.Dot(right->Value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_crossed(const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, OcctSharp_VecHandle** result)
{
  if (result == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateVector(left); ValidateVector(right); *result = AllocateValue(new OcctSharp_VecHandle(left->Value.Crossed(right->Value)), LiveVectors); });
}

void OCCTSHARP_CALL occtsharp_vec_release(OcctSharp_VecHandle* vector)
{ if (vector != nullptr && UnregisterValue(vector, LiveVectors)) delete vector; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_create(const double x, const double y, const double z, OcctSharp_DirHandle** out_direction)
{
  if (out_direction == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_direction = nullptr; return Guard([&] { ValidateFinite(x, "Direction X must be finite."); ValidateFinite(y, "Direction Y must be finite."); ValidateFinite(z, "Direction Z must be finite."); *out_direction = AllocateValue(new OcctSharp_DirHandle(gp_Dir(x, y, z)), LiveDirections); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_clone(const OcctSharp_DirHandle* source, OcctSharp_DirHandle** out_direction)
{
  if (out_direction == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_direction = nullptr; return Guard([&] { ValidateDirection(source); *out_direction = AllocateValue(new OcctSharp_DirHandle(source->Value), LiveDirections); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_components(const OcctSharp_DirHandle* direction, double* x, double* y, double* z)
{
  if (x == nullptr || y == nullptr || z == nullptr) { SetLastError("The direction component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateDirection(direction); *x = direction->Value.X(); *y = direction->Value.Y(); *z = direction->Value.Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_dot(const OcctSharp_DirHandle* left, const OcctSharp_DirHandle* right, double* dot)
{
  if (dot == nullptr) { SetLastError("The direction dot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateDirection(left); ValidateDirection(right); *dot = left->Value.Dot(right->Value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_reversed(const OcctSharp_DirHandle* source, OcctSharp_DirHandle** result)
{
  if (result == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateDirection(source); *result = AllocateValue(new OcctSharp_DirHandle(source->Value.Reversed()), LiveDirections); });
}

void OCCTSHARP_CALL occtsharp_dir_release(OcctSharp_DirHandle* direction)
{ if (direction != nullptr && UnregisterValue(direction, LiveDirections)) delete direction; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_create(
  const double origin_x, const double origin_y, const double origin_z,
  const double direction_x, const double direction_y, const double direction_z,
  OcctSharp_Ax1Handle** out_axis)
{
  if (out_axis == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_axis = nullptr; return Guard([&] { ValidateFinite(origin_x, "Axis origin X must be finite."); ValidateFinite(origin_y, "Axis origin Y must be finite."); ValidateFinite(origin_z, "Axis origin Z must be finite."); ValidateFinite(direction_x, "Axis direction X must be finite."); ValidateFinite(direction_y, "Axis direction Y must be finite."); ValidateFinite(direction_z, "Axis direction Z must be finite."); *out_axis = AllocateValue(new OcctSharp_Ax1Handle(gp_Ax1(gp_Pnt(origin_x, origin_y, origin_z), gp_Dir(direction_x, direction_y, direction_z))), LiveAxes); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_clone(const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** out_axis)
{
  if (out_axis == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_axis = nullptr; return Guard([&] { ValidateAxis(source); *out_axis = AllocateValue(new OcctSharp_Ax1Handle(source->Value), LiveAxes); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_components(const OcctSharp_Ax1Handle* axis, double* ox, double* oy, double* oz, double* dx, double* dy, double* dz)
{
  if (ox == nullptr || oy == nullptr || oz == nullptr || dx == nullptr || dy == nullptr || dz == nullptr) { SetLastError("The axis component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateAxis(axis); *ox = axis->Value.Location().X(); *oy = axis->Value.Location().Y(); *oz = axis->Value.Location().Z(); *dx = axis->Value.Direction().X(); *dy = axis->Value.Direction().Y(); *dz = axis->Value.Direction().Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_reversed(const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** result)
{
  if (result == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateAxis(source); *result = AllocateValue(new OcctSharp_Ax1Handle(source->Value.Reversed()), LiveAxes); });
}

void OCCTSHARP_CALL occtsharp_ax1_release(OcctSharp_Ax1Handle* axis)
{ if (axis != nullptr && UnregisterValue(axis, LiveAxes)) delete axis; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_create(const double* values, OcctSharp_MatHandle** out_matrix)
{
  if (values == nullptr || out_matrix == nullptr) { SetLastError("The matrix input or output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { for (int i = 0; i < 9; ++i) ValidateFinite(values[i], "Matrix values must be finite."); *out_matrix = AllocateValue(new OcctSharp_MatHandle(gp_Mat(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8])), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_identity(OcctSharp_MatHandle** out_matrix)
{
  if (out_matrix == nullptr) { SetLastError("The output matrix pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { *out_matrix = AllocateValue(new OcctSharp_MatHandle(gp_Mat(1,0,0,0,1,0,0,0,1)), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_clone(const OcctSharp_MatHandle* source, OcctSharp_MatHandle** out_matrix)
{
  if (out_matrix == nullptr) { SetLastError("The output matrix pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { ValidateMatrix(source); *out_matrix = AllocateValue(new OcctSharp_MatHandle(source->Value), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_value(const OcctSharp_MatHandle* matrix, const int32_t row, const int32_t column, double* value)
{
  if (value == nullptr) { SetLastError("The matrix value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0; return Guard([&] { ValidateMatrix(matrix); if (row < 1 || row > 3 || column < 1 || column > 3) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Matrix indices must be row 1..3 and column 1..3."); *value = matrix->Value.Value(row, column); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_determinant(const OcctSharp_MatHandle* matrix, double* determinant)
{
  if (determinant == nullptr) { SetLastError("The matrix determinant output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateMatrix(matrix); *determinant = matrix->Value.Determinant(); });
}

void OCCTSHARP_CALL occtsharp_mat_release(OcctSharp_MatHandle* matrix)
{ if (matrix != nullptr && UnregisterValue(matrix, LiveMatrices)) delete matrix; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_create(
  const char* utf8, const int32_t length, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateUtf8Input(utf8, length);
    *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(MakeAsciiString(utf8, length)), LiveAsciiStrings);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_clone(
  const OcctSharp_AsciiStringHandle* source, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateAsciiString(source); *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(source->Value), LiveAsciiStrings); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_length(const OcctSharp_AsciiStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The ASCII string length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateAsciiString(string); *length = string->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_append(
  OcctSharp_AsciiStringHandle* string, const char* utf8, const int32_t length)
{
  return Guard([&] { ValidateAsciiString(string); ValidateUtf8Input(utf8, length); if (length > 0) string->Value.AssignCat(utf8, length); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_utf8(
  const OcctSharp_AsciiStringHandle* string, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The ASCII string output length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateAsciiString(string);
    const int32_t length = string->Value.Length();
    ValidateOutputBuffer(buffer, capacity, length + 1);
    if (length > 0) std::memcpy(buffer, string->Value.ToCString(), static_cast<size_t>(length));
    buffer[length] = '\0';
    *written = length;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_extended(
  const OcctSharp_AsciiStringHandle* string, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateAsciiString(string);
    *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(TCollection_ExtendedString(string->Value, true)), LiveExtendedStrings);
  });
}

void OCCTSHARP_CALL occtsharp_ascii_release(OcctSharp_AsciiStringHandle* string)
{ if (string != nullptr && UnregisterValue(string, LiveAsciiStrings)) delete string; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_create_utf8(
  const char* utf8, const int32_t length, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateUtf8Input(utf8, length);
    TCollection_AsciiString ascii = MakeAsciiString(utf8, length);
    *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(TCollection_ExtendedString(ascii, true)), LiveExtendedStrings);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_clone(
  const OcctSharp_ExtendedStringHandle* source, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateExtendedString(source); *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(source->Value), LiveExtendedStrings); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_length(const OcctSharp_ExtendedStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The extended string length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateExtendedString(string); *length = string->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_utf8_length(const OcctSharp_ExtendedStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The extended string UTF-8 length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateExtendedString(string); *length = string->Value.LengthOfCString(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_append_utf8(
  OcctSharp_ExtendedStringHandle* string, const char* utf8, const int32_t length)
{
  return Guard([&]
  {
    ValidateExtendedString(string);
    ValidateUtf8Input(utf8, length);
    TCollection_AsciiString ascii = MakeAsciiString(utf8, length);
    string->Value.AssignCat(TCollection_ExtendedString(ascii, true));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_utf8(
  const OcctSharp_ExtendedStringHandle* string, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The extended string output length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateExtendedString(string);
    const int32_t length = string->Value.LengthOfCString();
    ValidateOutputBuffer(buffer, capacity, length + 1);
    std::string converted(static_cast<size_t>(length) + 1, '\0');
    Standard_PCharacter output = converted.data();
    const int32_t convertedLength = string->Value.ToUTF8CString(output);
    if (convertedLength > 0) std::memcpy(buffer, converted.data(), static_cast<size_t>(convertedLength));
    buffer[convertedLength] = '\0';
    *written = convertedLength;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_value(
  const OcctSharp_ExtendedStringHandle* string, const int32_t index, uint16_t* value)
{
  if (value == nullptr) { SetLastError("The extended string value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateExtendedString(string); if (index < 1 || index > string->Value.Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Extended string indices are 1-based and must be within the string length."); *value = static_cast<uint16_t>(string->Value.Value(index)); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_ascii(
  const OcctSharp_ExtendedStringHandle* string, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateExtendedString(string); *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(TCollection_AsciiString(string->Value)), LiveAsciiStrings); });
}

void OCCTSHARP_CALL occtsharp_extended_release(OcctSharp_ExtendedStringHandle* string)
{ if (string != nullptr && UnregisterValue(string, LiveExtendedStrings)) delete string; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_create(
  const double* values, const int32_t count, OcctSharp_RealSequenceHandle** out_sequence)
{
  if (out_sequence == nullptr) { SetLastError("The output real sequence pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_sequence = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sequence count or input values are invalid.");
    NCollection_Sequence<double> sequence;
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Sequence values must be finite."); sequence.Append(values[index]); }
    *out_sequence = AllocateValue(new OcctSharp_RealSequenceHandle(std::move(sequence)), LiveRealSequences);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_clone(
  const OcctSharp_RealSequenceHandle* source, OcctSharp_RealSequenceHandle** out_sequence)
{
  if (out_sequence == nullptr) { SetLastError("The output real sequence pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_sequence = nullptr;
  return Guard([&] { ValidateRealSequence(source); *out_sequence = AllocateValue(new OcctSharp_RealSequenceHandle(source->Value), LiveRealSequences); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_length(const OcctSharp_RealSequenceHandle* sequence, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real sequence length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealSequence(sequence); *length = sequence->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_value(const OcctSharp_RealSequenceHandle* sequence, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real sequence value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); *value = sequence->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_append(OcctSharp_RealSequenceHandle* sequence, const double value)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateFinite(value, "Sequence values must be finite."); sequence->Value.Append(value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_set_value(OcctSharp_RealSequenceHandle* sequence, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); ValidateFinite(value, "Sequence values must be finite."); sequence->Value.SetValue(index, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_remove(OcctSharp_RealSequenceHandle* sequence, const int32_t index)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); sequence->Value.Remove(index); });
}

void OCCTSHARP_CALL occtsharp_real_sequence_release(OcctSharp_RealSequenceHandle* sequence)
{ if (sequence != nullptr && UnregisterValue(sequence, LiveRealSequences)) delete sequence; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_create(
  const double* values, const int32_t count, OcctSharp_RealArrayHandle** out_array)
{
  if (out_array == nullptr) { SetLastError("The output real array pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_array = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Array count or input values are invalid.");
    NCollection_Array1<double> array(1, count);
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Array values must be finite."); array.SetValue(index + 1, values[index]); }
    *out_array = AllocateValue(new OcctSharp_RealArrayHandle(std::move(array)), LiveRealArrays);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_clone(
  const OcctSharp_RealArrayHandle* source, OcctSharp_RealArrayHandle** out_array)
{
  if (out_array == nullptr) { SetLastError("The output real array pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_array = nullptr;
  return Guard([&] { ValidateRealArray(source); *out_array = AllocateValue(new OcctSharp_RealArrayHandle(source->Value), LiveRealArrays); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_length(const OcctSharp_RealArrayHandle* array, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real array length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealArray(array); *length = array->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_lower(const OcctSharp_RealArrayHandle* array, int32_t* lower)
{
  if (lower == nullptr) { SetLastError("The real array lower-bound output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealArray(array); *lower = array->Value.Lower(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_value(const OcctSharp_RealArrayHandle* array, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real array value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealArray(array); ValidateArrayIndex(array, index); *value = array->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_set_value(OcctSharp_RealArrayHandle* array, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealArray(array); ValidateArrayIndex(array, index); ValidateFinite(value, "Array values must be finite."); array->Value.SetValue(index, value); });
}

void OCCTSHARP_CALL occtsharp_real_array_release(OcctSharp_RealArrayHandle* array)
{ if (array != nullptr && UnregisterValue(array, LiveRealArrays)) delete array; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_create(
  const double* values, const int32_t count, OcctSharp_RealVectorHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output real vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Vector count or input values are invalid.");
    NCollection_DynamicArray<double> vector;
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Vector values must be finite."); vector.Append(values[index]); }
    *out_vector = AllocateValue(new OcctSharp_RealVectorHandle(std::move(vector)), LiveRealVectors);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_clone(
  const OcctSharp_RealVectorHandle* source, OcctSharp_RealVectorHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output real vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&] { ValidateRealVector(source); *out_vector = AllocateValue(new OcctSharp_RealVectorHandle(source->Value), LiveRealVectors); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_length(const OcctSharp_RealVectorHandle* vector, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real vector length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealVector(vector); *length = vector->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_value(const OcctSharp_RealVectorHandle* vector, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real vector value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealVector(vector); ValidateVectorIndex(vector, index); *value = vector->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_append(OcctSharp_RealVectorHandle* vector, const double value)
{
  return Guard([&] { ValidateRealVector(vector); ValidateFinite(value, "Vector values must be finite."); vector->Value.Append(value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_set_value(OcctSharp_RealVectorHandle* vector, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealVector(vector); ValidateVectorIndex(vector, index); ValidateFinite(value, "Vector values must be finite."); vector->Value.SetValue(index, value); });
}

void OCCTSHARP_CALL occtsharp_real_vector_release(OcctSharp_RealVectorHandle* vector)
{ if (vector != nullptr && UnregisterValue(vector, LiveRealVectors)) delete vector; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_create(
  const int32_t* keys, const double* values, const int32_t count, OcctSharp_IntRealMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output integer-real map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && (keys == nullptr || values == nullptr))) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Map count or input arrays are invalid.");
    NCollection_DataMap<int32_t, double> map;
    for (int32_t i = 0; i < count; ++i) { ValidateFinite(values[i], "Map values must be finite."); if (!map.Bind(keys[i], values[i])) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Map keys must be unique."); }
    *out_map = AllocateValue(new OcctSharp_IntRealMapHandle(std::move(map)), LiveIntRealMaps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_clone(const OcctSharp_IntRealMapHandle* source, OcctSharp_IntRealMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output integer-real map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&] { ValidateIntRealMap(source); *out_map = AllocateValue(new OcctSharp_IntRealMapHandle(source->Value), LiveIntRealMaps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_extent(const OcctSharp_IntRealMapHandle* map, int32_t* extent)
{
  if (extent == nullptr) { SetLastError("The integer-real map extent output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntRealMap(map); *extent = map->Value.Extent(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_is_bound(const OcctSharp_IntRealMapHandle* map, const int32_t key, int32_t* is_bound)
{
  if (is_bound == nullptr) { SetLastError("The integer-real map bound output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntRealMap(map); *is_bound = map->Value.IsBound(key) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_find(const OcctSharp_IntRealMapHandle* map, const int32_t key, double* value)
{
  if (value == nullptr) { SetLastError("The integer-real map value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateIntRealMap(map); if (!map->Value.Find(key, *value)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The map key is not bound."); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_bind(OcctSharp_IntRealMapHandle* map, const int32_t key, const double value)
{
  return Guard([&] { ValidateIntRealMap(map); ValidateFinite(value, "Map values must be finite."); map->Value.Bind(key, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_unbind(OcctSharp_IntRealMapHandle* map, const int32_t key, int32_t* removed)
{
  if (removed == nullptr) { SetLastError("The integer-real map removal output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *removed = 0;
  return Guard([&] { ValidateIntRealMap(map); *removed = map->Value.UnBind(key) ? 1 : 0; });
}

void OCCTSHARP_CALL occtsharp_int_real_map_release(OcctSharp_IntRealMapHandle* map)
{ if (map != nullptr && UnregisterValue(map, LiveIntRealMaps)) delete map; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_create(
  const int32_t* keys, const int32_t count, OcctSharp_IntIndexedMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output indexed map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && keys == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map count or input keys are invalid.");
    NCollection_IndexedMap<int32_t> map;
    for (int32_t i = 0; i < count; ++i) { const int before = map.Extent(); const int index = map.Add(keys[i]); if (index != before + 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map keys must be unique."); }
    *out_map = AllocateValue(new OcctSharp_IntIndexedMapHandle(std::move(map)), LiveIntIndexedMaps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_clone(const OcctSharp_IntIndexedMapHandle* source, OcctSharp_IntIndexedMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output indexed map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&] { ValidateIntIndexedMap(source); *out_map = AllocateValue(new OcctSharp_IntIndexedMapHandle(source->Value), LiveIntIndexedMaps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_extent(const OcctSharp_IntIndexedMapHandle* map, int32_t* extent)
{
  if (extent == nullptr) { SetLastError("The indexed map extent output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntIndexedMap(map); *extent = map->Value.Extent(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_add(OcctSharp_IntIndexedMapHandle* map, const int32_t key, int32_t* index, int32_t* added)
{
  if (index == nullptr || added == nullptr) { SetLastError("The indexed map add output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *index = 0; *added = 0;
  return Guard([&] { ValidateIntIndexedMap(map); const int before = map->Value.Extent(); *index = map->Value.Add(key); *added = *index == before + 1 ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_key(const OcctSharp_IntIndexedMapHandle* map, const int32_t index, int32_t* key)
{
  if (key == nullptr) { SetLastError("The indexed map key output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *key = 0;
  return Guard([&] { ValidateIntIndexedMap(map); if (index < 1 || index > map->Value.Extent()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map indices are 1-based and must be within the extent."); *key = map->Value.FindKey(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_find_index(const OcctSharp_IntIndexedMapHandle* map, const int32_t key, int32_t* index)
{
  if (index == nullptr) { SetLastError("The indexed map index output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *index = 0;
  return Guard([&] { ValidateIntIndexedMap(map); *index = map->Value.FindIndex(key); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_remove_last(OcctSharp_IntIndexedMapHandle* map, int32_t* removed_key)
{
  if (removed_key == nullptr) { SetLastError("The indexed map removed-key output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *removed_key = 0;
  return Guard([&] { ValidateIntIndexedMap(map); const int extent = map->Value.Extent(); if (extent <= 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Cannot remove the last key from an empty indexed map."); *removed_key = map->Value.FindKey(extent); map->Value.RemoveLast(); });
}

void OCCTSHARP_CALL occtsharp_int_indexed_map_release(OcctSharp_IntIndexedMapHandle* map)
{ if (map != nullptr && UnregisterValue(map, LiveIntIndexedMaps)) delete map; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_compound(OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  return Guard([&]
  {
    TopoDS_Compound compound;
    BRep_Builder builder;
    builder.MakeCompound(compound);
    *out_shape = AllocateShape(std::move(compound));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_compound_add(
  OcctSharp_ShapeHandle* compound,
  const OcctSharp_ShapeHandle* child)
{
  return Guard([&]
  {
    ValidateShape(compound);
    ValidateShape(child);
    if (compound->Value.ShapeType() != TopAbs_COMPOUND)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The target shape is not a compound.");
    }

    TopoDS_Compound target = TopoDS::Compound(compound->Value);
    BRep_Builder builder;
    builder.Add(target, child->Value);
    compound->Value = target;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_merge_xde(
  const OcctSharp_StepAssemblyInput* inputs,
  const int32_t input_count,
  const char* output_path)
{
  return Guard([&]
  {
    if (inputs == nullptr)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP input array is null.");
    }
    if (input_count <= 0)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "At least one STEP input is required.");
    }
    ValidatePath(output_path);

    occ::handle<TDocStd_Document> outputDocument = CreateXdeDocument();
    InitializeXdeTools(outputDocument);
    occ::handle<XCAFDoc_ShapeTool> outputShapeTool =
      XCAFDoc_DocumentTool::ShapeTool(outputDocument->Main());
    TDF_Label outputAssembly = outputShapeTool->NewShape();
    TDataStd_Name::Set(outputAssembly, TCollection_ExtendedString("OcctSharp Assembly"));

    int32_t rootCount = 0;
    for (int32_t inputIndex = 0; inputIndex < input_count; ++inputIndex)
    {
      const OcctSharp_StepAssemblyInput& input = inputs[inputIndex];
      ValidatePath(input.file_path);
      gp_Trsf transform = CreateTransform(input);

      occ::handle<TDocStd_Document> sourceDocument = CreateXdeDocument();
      InitializeXdeTools(sourceDocument);
      STEPCAFControl_Reader reader;
      ConfigureXdeReader(reader);
      if (reader.ReadFile(input.file_path) != IFSelect_RetDone)
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_FILE_IO_ERROR,
          "OCCT could not read a STEP input through STEPCAF.");
      }
      if (!reader.Transfer(sourceDocument))
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_TRANSFER_FAILED,
          "A STEP input could not be transferred into an XDE document.");
      }

      occ::handle<XCAFDoc_ShapeTool> sourceShapeTool =
        XCAFDoc_DocumentTool::ShapeTool(sourceDocument->Main());
      NCollection_Sequence<TDF_Label> sourceRoots;
      sourceShapeTool->GetFreeShapes(sourceRoots);
      if (sourceRoots.IsEmpty())
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_TRANSFER_FAILED,
          "A STEP input produced no free XDE shape roots.");
      }

      NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>, occ::handle<XCAFDoc_VisMaterial>>
        visualMaterialMap;
      for (NCollection_Sequence<TDF_Label>::Iterator rootIterator(sourceRoots); rootIterator.More();
           rootIterator.Next())
      {
        NCollection_DataMap<TDF_Label, TDF_Label> labelMap;
        TDF_Label clonedRoot = XCAFDoc_Editor::CloneShapeLabel(
          rootIterator.Value(), sourceShapeTool, outputShapeTool, labelMap);
        if (clonedRoot.IsNull())
        {
          throw OperationFailure(
            OCCTSHARP_STATUS_TRANSFER_FAILED,
            "An XDE shape tree could not be cloned into the output document.");
        }

        for (NCollection_DataMap<TDF_Label, TDF_Label>::Iterator labelIterator(labelMap);
             labelIterator.More(); labelIterator.Next())
        {
          occ::handle<TDataStd_TreeNode> materialReference;
          const bool hasMaterialReference =
            labelIterator.Key().FindAttribute(XCAFDoc::MaterialRefGUID(), materialReference)
            && materialReference->HasFather();
          XCAFDoc_Editor::CloneMetaData(
            labelIterator.Key(),
            labelIterator.Value(),
            &visualMaterialMap,
            true,
            true,
            true,
            true,
            true);
          if (hasMaterialReference && labelIterator.Value() != clonedRoot)
          {
            // STEPCAF material export operates on top-level part labels. Preserve a
            // subshape assignment in its original cloned label and also promote it
            // to the corresponding part root for round-trip STEP material export.
            XCAFDoc_Editor::CloneMetaData(
              labelIterator.Key(),
              clonedRoot,
              &visualMaterialMap,
              false,
              false,
              true,
              false,
              false);
          }
        }

        TDF_Label component = outputShapeTool->AddComponent(
          outputAssembly,
          clonedRoot,
          TopLoc_Location(transform));
        if (component.IsNull())
        {
          throw OperationFailure(
            OCCTSHARP_STATUS_TRANSFER_FAILED,
            "A cloned XDE root could not be placed in the output assembly.");
        }
        ++rootCount;
      }
    }

    if (rootCount == 0)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TRANSFER_FAILED,
        "No STEP roots were added to the output XDE assembly.");
    }
    outputShapeTool->UpdateAssemblies();

    STEPCAFControl_Writer writer;
    ConfigureXdeWriter(writer);
    if (!writer.Transfer(outputDocument, STEPControl_AsIs))
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TRANSFER_FAILED,
        "The output XDE document could not be transferred to STEP.");
    }
    if (writer.Write(output_path) != IFSelect_RetDone)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_FILE_IO_ERROR,
        "OCCT could not write the XDE STEP assembly.");
    }
  });
}

void OCCTSHARP_CALL occtsharp_shape_release(OcctSharp_ShapeHandle* shape)
{
  if (shape != nullptr && UnregisterShape(shape))
  {
    delete shape;
  }
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    opencascade::handle<Standard_Transient> value = new Standard_Transient();
    *out_handle = AllocateTransient(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_null(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    *out_handle = AllocateTransient(opencascade::handle<Standard_Transient>());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_derived(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    opencascade::handle<Standard_Transient> value = new OcctSharp_TransientDerived();
    *out_handle = AllocateTransient(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_clone(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    ValidateTransient(source);
    *out_handle = AllocateTransient(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_try_cast_derived(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    ValidateTransient(source);
    if (source->Value.IsNull()
        || !source->Value->IsKind("OcctSharp_TransientDerived"))
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TYPE_MISMATCH,
        "The transient handle is not an OcctSharp_TransientDerived instance.");
    }

    // Copying the native handle retains the same OCCT object. The dynamic type
    // was checked above; no C++ object pointer or layout crosses the ABI.
    *out_handle = AllocateTransient(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_null(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_is_null)
{
  if (out_is_null == nullptr)
  {
    SetLastError("The output null-state pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_is_null = handle->Value.IsNull() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_ref_count(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_ref_count)
{
  if (out_ref_count == nullptr)
  {
    SetLastError("The output reference-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_ref_count = handle->Value.IsNull() ? 0 : handle->Value->GetRefCount();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_type_name(
  const OcctSharp_TransientHandle* handle,
  const char** out_type_name)
{
  if (out_type_name == nullptr)
  {
    SetLastError("The output transient type-name pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_type_name = nullptr;
  return Guard([&]
  {
    ValidateTransient(handle);
    if (handle->Value.IsNull())
    {
      *out_type_name = "";
      return;
    }

    *out_type_name = handle->Value->DynamicType()->Name();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_kind(
  const OcctSharp_TransientHandle* handle,
  const char* type_name,
  int32_t* out_is_kind)
{
  if (type_name == nullptr || type_name[0] == '\0')
  {
    SetLastError("The transient type name is null or empty.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  if (out_is_kind == nullptr)
  {
    SetLastError("The output transient kind-state pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_is_kind = !handle->Value.IsNull() && handle->Value->IsKind(type_name) ? 1 : 0;
  });
}

void OCCTSHARP_CALL occtsharp_transient_release(OcctSharp_TransientHandle* handle)
{
  if (handle != nullptr && UnregisterTransient(handle))
  {
    delete handle;
  }
}
