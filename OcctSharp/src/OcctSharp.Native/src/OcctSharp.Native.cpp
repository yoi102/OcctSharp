#include "OcctSharp.Native.h"
#include "OcctSharp.Native.Internal.hxx"

#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRep_Tool.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepBndLib.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <AIS_InteractiveContext.hxx>
#include <AIS_Shape.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <BinDrivers.hxx>
#include <BinXCAFDrivers.hxx>
#include <DEGLTF_Provider.hxx>
#include <DEOBJ_ConfigurationNode.hxx>
#include <DEOBJ_Provider.hxx>
#include <DEPLY_ConfigurationNode.hxx>
#include <DEPLY_Provider.hxx>
#include <DEVRML_ConfigurationNode.hxx>
#include <DEVRML_Provider.hxx>
#include <GProp_GProps.hxx>
#include <GProp_PrincipalProps.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Writer.hxx>
#include <IGESControl_Reader.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_DynamicArray.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_StepModelType.hxx>
#include <STEPControl_Writer.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <Standard_TypeDef.hxx>
#include <Standard_Failure.hxx>
#include <Standard_Handle.hxx>
#include <Standard_Type.hxx>
#include <Standard_Version.hxx>
#include <Standard_Transient.hxx>
#include <StlAPI_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_AsciiString.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TDF_TagSource.hxx>
#include <TDF_Tool.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp_Explorer.hxx>
#include <TopExp.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shape.hxx>
#include <Poly_Triangulation.hxx>
#include <Poly_Triangle.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <TCollection_HAsciiString.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc.hxx>
#include <V3d_View.hxx>
#include <V3d_Viewer.hxx>
#include <WNT_Window.hxx>
#include <gp_Ax1.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <gp_Mat.hxx>
#include <gp_XYZ.hxx>
#include <gp_Lin.hxx>
#include <gp_Circ.hxx>
#include <gp_Ax2.hxx>
#include <gp_Ax3.hxx>
#include <gp_Pln.hxx>

#include <cmath>
#include <cstddef>
#include <cstring>
#include <exception>
#include <new>
#include <string>
#include <type_traits>
#include <unordered_set>
#include <unordered_map>
#include <utility>
#include <mutex>
#include <limits>
#include <memory>
#include <vector>
#include <thread>

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
static_assert(sizeof(OcctSharp_Xyz) == 24);
static_assert(alignof(OcctSharp_Xyz) == 8);
static_assert(sizeof(OcctSharp_Line) == 48);
static_assert(sizeof(OcctSharp_Circle) == 56);
static_assert(sizeof(OcctSharp_Ax2) == 96);
static_assert(sizeof(OcctSharp_Ax3) == 96);
static_assert(sizeof(OcctSharp_Plane) == 48);
static_assert(sizeof(OcctSharp_EdgeCurveSnapshot) == 72);
static_assert(alignof(OcctSharp_EdgeCurveSnapshot) == 8);
static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, curve_type) == 0);
static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, first_parameter) == 8);
static_assert(offsetof(OcctSharp_EdgeCurveSnapshot, start_point) == 24);
static_assert(sizeof(OcctSharp_FaceSurfaceSnapshot) == 40);
static_assert(alignof(OcctSharp_FaceSurfaceSnapshot) == 8);
static_assert(offsetof(OcctSharp_FaceSurfaceSnapshot, surface_type) == 0);
static_assert(offsetof(OcctSharp_FaceSurfaceSnapshot, first_u_parameter) == 8);
static_assert(sizeof(OcctSharp_ShapeDistanceResult) == 64);
static_assert(sizeof(OcctSharp_BoundingBox) == 48);
static_assert(alignof(OcctSharp_BoundingBox) == 8);
static_assert(offsetof(OcctSharp_BoundingBox, min_x) == 0);
static_assert(offsetof(OcctSharp_BoundingBox, max_x) == 24);
static_assert(sizeof(OcctSharp_XdeColor) == 32);
static_assert(alignof(OcctSharp_ShapeDistanceResult) == 8);
static_assert(offsetof(OcctSharp_ShapeDistanceResult, distance) == 0);
static_assert(offsetof(OcctSharp_ShapeDistanceResult, point_on_first) == 8);
static_assert(offsetof(OcctSharp_ShapeDistanceResult, point_on_second) == 32);
static_assert(offsetof(OcctSharp_ShapeDistanceResult, solution_count) == 56);
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
struct OcctSharp_GPropsHandle { explicit OcctSharp_GPropsHandle(GProp_GProps value) : Value(std::move(value)) {} GProp_GProps Value; };
struct OcctSharp_OcafDocumentHandle
{
  OcctSharp_OcafDocumentHandle(opencascade::handle<TDocStd_Application> application,
                               opencascade::handle<TDocStd_Document> document)
    : Application(std::move(application)), Document(std::move(document))
  {
  }

  opencascade::handle<TDocStd_Application> Application;
  opencascade::handle<TDocStd_Document> Document;
};
struct OcctSharp_ViewerHandle
{
  opencascade::handle<Aspect_DisplayConnection> Display;
  opencascade::handle<OpenGl_GraphicDriver> Driver;
  opencascade::handle<V3d_Viewer> Viewer;
  opencascade::handle<AIS_InteractiveContext> Context;
  opencascade::handle<V3d_View> View;
  opencascade::handle<WNT_Window> Window;
  std::unordered_map<int64_t, opencascade::handle<AIS_Shape>> Presentations;
  int64_t NextPresentationId = 1;
  std::thread::id OwnerThread;
};

namespace
{
constexpr uint32_t AbiVersion = 0x00010021U;
constexpr const char* BridgeVersion = "0.41.0";
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
std::unordered_set<const OcctSharp_GPropsHandle*> LiveGProps;
std::unordered_set<const OcctSharp_OcafDocumentHandle*> LiveOcafDocuments;
std::unordered_set<const OcctSharp_ViewerHandle*> LiveViewers;

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

void ValidateUsableShape(const OcctSharp_ShapeHandle* shape)
{
  ValidateShape(shape);
  if (shape->Value.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology shape is null.");
  }
}

void ValidateMeshParameters(const double linear_deflection, const double angular_deflection)
{
  if (!std::isfinite(linear_deflection) || linear_deflection <= 0.0
      || !std::isfinite(angular_deflection) || angular_deflection <= 0.0)
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Mesh deflections must be finite and greater than zero.");
  }
}

struct MeshData
{
  std::vector<OcctSharp_MeshVertex> Vertices;
  std::vector<int32_t> Indices;
};

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

void ValidateGProps(const OcctSharp_GPropsHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The GProp_GProps handle is null.");
  if (!IsLiveValue(handle, LiveGProps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The GProp_GProps handle is invalid or already released.");
}

void ValidateOcafDocument(const OcctSharp_OcafDocumentHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The OCAF document handle is null.");
  }
  if (!IsLiveValue(handle, LiveOcafDocuments))
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_HANDLE,
      "The OCAF document handle is invalid or already released.");
  }
  if (handle->Application.IsNull() || handle->Document.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The OCAF document is closed.");
  }
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_default(void)
{
  const gp_XYZ value;
  return { value.X(), value.Y(), value.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_create(const double x, const double y, const double z)
{
  const gp_XYZ value(x, y, z);
  return { value.X(), value.Y(), value.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_copy(const OcctSharp_Xyz value)
{
  return value;
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_added(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  const gp_XYZ result = gp_XYZ(left.x, left.y, left.z).Added(gp_XYZ(right.x, right.y, right.z));
  return { result.X(), result.Y(), result.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_crossed(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  const gp_XYZ result = gp_XYZ(left.x, left.y, left.z).Crossed(gp_XYZ(right.x, right.y, right.z));
  return { result.X(), result.Y(), result.Z() };
}

double OCCTSHARP_CALL occtsharp_gp_xyz_dot(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  return gp_XYZ(left.x, left.y, left.z).Dot(gp_XYZ(right.x, right.y, right.z));
}

double OCCTSHARP_CALL occtsharp_gp_xyz_modulus(const OcctSharp_Xyz value)
{
  return gp_XYZ(value.x, value.y, value.z).Modulus();
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_xyz_normalized(const OcctSharp_Xyz value, OcctSharp_Xyz* result)
{
  if (result == nullptr) { SetLastError("The gp_XYZ normalized output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_XYZ normalized = gp_XYZ(value.x, value.y, value.z).Normalized();
    *result = { normalized.X(), normalized.Y(), normalized.Z() };
  });
}

OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_default(void)
{
  const gp_Lin line;
  return { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_lin_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz direction, OcctSharp_Line* result)
{
  if (result == nullptr) { SetLastError("The gp_Lin output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Lin line(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(direction.x, direction.y, direction.z));
    *result = { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
  });
}

OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_reversed(const OcctSharp_Line value)
{
  const gp_Lin source(gp_Pnt(value.origin.x, value.origin.y, value.origin.z), gp_Dir(value.direction.x, value.direction.y, value.direction.z));
  const gp_Lin line = source.Reversed();
  return { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
}

double OCCTSHARP_CALL occtsharp_gp_lin_distance(const OcctSharp_Line line, const OcctSharp_Xyz point)
{
  return gp_Lin(gp_Pnt(line.origin.x, line.origin.y, line.origin.z), gp_Dir(line.direction.x, line.direction.y, line.direction.z)).Distance(gp_Pnt(point.x, point.y, point.z));
}

double OCCTSHARP_CALL occtsharp_gp_lin_angle(const OcctSharp_Line left, const OcctSharp_Line right)
{
  return gp_Lin(gp_Pnt(left.origin.x, left.origin.y, left.origin.z), gp_Dir(left.direction.x, left.direction.y, left.direction.z)).Angle(gp_Lin(gp_Pnt(right.origin.x, right.origin.y, right.origin.z), gp_Dir(right.direction.x, right.direction.y, right.direction.z)));
}

OcctSharp_Circle OCCTSHARP_CALL occtsharp_gp_circ_default(void)
{
  const gp_Circ circle;
  return { { circle.Location().X(), circle.Location().Y(), circle.Location().Z() }, { circle.Axis().Direction().X(), circle.Axis().Direction().Y(), circle.Axis().Direction().Z() }, circle.Radius() };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_circ_create(const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const double radius, OcctSharp_Circle* result)
{
  if (result == nullptr) { SetLastError("The gp_Circ output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Circ circle(gp_Ax2(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z)), radius);
    *result = { { circle.Location().X(), circle.Location().Y(), circle.Location().Z() }, { circle.Axis().Direction().X(), circle.Axis().Direction().Y(), circle.Axis().Direction().Z() }, circle.Radius() };
  });
}

double OCCTSHARP_CALL occtsharp_gp_circ_area(const OcctSharp_Circle value)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Area(); }

double OCCTSHARP_CALL occtsharp_gp_circ_length(const OcctSharp_Circle value)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Length(); }

double OCCTSHARP_CALL occtsharp_gp_circ_distance(const OcctSharp_Circle value, const OcctSharp_Xyz point)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Distance(gp_Pnt(point.x, point.y, point.z)); }

OcctSharp_Ax2 OCCTSHARP_CALL occtsharp_gp_ax2_default(void)
{
  const gp_Ax2 axis;
  return { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() }, { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() }, { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() }, { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax2_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction, OcctSharp_Ax2* result)
{
  if (result == nullptr) { SetLastError("The gp_Ax2 output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Ax2 axis(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(normal.x, normal.y, normal.z), gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    *result = { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() }, { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() }, { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() }, { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
  });
}

double OCCTSHARP_CALL occtsharp_gp_ax2_angle(const OcctSharp_Ax2 left, const OcctSharp_Ax2 right)
{
  const gp_Ax2 a(gp_Pnt(left.origin.x, left.origin.y, left.origin.z), gp_Dir(left.direction.x, left.direction.y, left.direction.z), gp_Dir(left.x_direction.x, left.x_direction.y, left.x_direction.z));
  const gp_Ax2 b(gp_Pnt(right.origin.x, right.origin.y, right.origin.z), gp_Dir(right.direction.x, right.direction.y, right.direction.z), gp_Dir(right.x_direction.x, right.x_direction.y, right.x_direction.z));
  return a.Angle(b);
}

OcctSharp_Ax3 OCCTSHARP_CALL occtsharp_gp_ax3_default(void)
{
  const gp_Ax3 axis;
  return { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() },
           { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() },
           { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() },
           { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax3_create(
  const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction,
  OcctSharp_Ax3* result)
{
  if (result == nullptr) { SetLastError("The gp_Ax3 output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Ax3 axis(gp_Pnt(origin.x, origin.y, origin.z),
                      gp_Dir(normal.x, normal.y, normal.z),
                      gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    *result = { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() },
                { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() },
                { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() },
                { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
  });
}

int32_t OCCTSHARP_CALL occtsharp_gp_ax3_direct(const OcctSharp_Ax3 value)
{
  const gp_Ax3 axis(gp_Pnt(value.origin.x, value.origin.y, value.origin.z),
                    gp_Dir(value.direction.x, value.direction.y, value.direction.z),
                    gp_Dir(value.x_direction.x, value.x_direction.y, value.x_direction.z));
  return axis.Direct() ? 1 : 0;
}

OcctSharp_Plane OCCTSHARP_CALL occtsharp_gp_pln_default(void)
{ return { { 0., 0., 0. }, { 0., 0., 1. } }; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_pln_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, OcctSharp_Plane* result)
{
  if (result == nullptr) { SetLastError("The gp_Pln output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Pln plane(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(normal.x, normal.y, normal.z));
    *result = { { plane.Location().X(), plane.Location().Y(), plane.Location().Z() }, { plane.Axis().Direction().X(), plane.Axis().Direction().Y(), plane.Axis().Direction().Z() } };
  });
}

double OCCTSHARP_CALL occtsharp_gp_pln_distance(const OcctSharp_Plane plane, const OcctSharp_Xyz point)
{ return gp_Pln(gp_Pnt(plane.origin.x, plane.origin.y, plane.origin.z), gp_Dir(plane.normal.x, plane.normal.y, plane.normal.z)).Distance(gp_Pnt(point.x, point.y, point.z)); }

double OCCTSHARP_CALL occtsharp_gp_pln_signed_distance(const OcctSharp_Plane plane, const OcctSharp_Xyz point)
{ return gp_Pln(gp_Pnt(plane.origin.x, plane.origin.y, plane.origin.z), gp_Dir(plane.normal.x, plane.normal.y, plane.normal.z)).SignedDistance(gp_Pnt(point.x, point.y, point.z)); }

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_create(OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  return Guard([&] { *out_properties = AllocateValue(new OcctSharp_GPropsHandle(GProp_GProps()), LiveGProps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_from_shape(
  const OcctSharp_ShapeHandle* shape, const int32_t mode, const int32_t only_closed,
  OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  if (mode < 0 || mode > 2) { SetLastError("GProp mode must be 0 (linear), 1 (surface), or 2 (volume)."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    GProp_GProps value;
    if (mode == 0)
      BRepGProp::LinearProperties(shape->Value, value);
    else if (mode == 1)
      BRepGProp::SurfaceProperties(shape->Value, value);
    else
      BRepGProp::VolumeProperties(shape->Value, value, only_closed != 0);
    *out_properties = AllocateValue(new OcctSharp_GPropsHandle(std::move(value)), LiveGProps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_clone(
  const OcctSharp_GPropsHandle* source, OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  return Guard([&]
  {
    ValidateGProps(source);
    *out_properties = AllocateValue(new OcctSharp_GPropsHandle(source->Value), LiveGProps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_add(
  OcctSharp_GPropsHandle* target, const OcctSharp_GPropsHandle* item, const double density)
{
  return Guard([&]
  {
    ValidateGProps(target);
    ValidateGProps(item);
    if (!std::isfinite(density) || density <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "GProp density must be finite and greater than zero.");
    target->Value.Add(item->Value, density);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_mass(
  const OcctSharp_GPropsHandle* properties, double* mass)
{
  if (mass == nullptr) { SetLastError("The GProp mass output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *mass = 0.0;
  return Guard([&] { ValidateGProps(properties); *mass = properties->Value.Mass(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_center(
  const OcctSharp_GPropsHandle* properties, OcctSharp_Xyz* center)
{
  if (center == nullptr) { SetLastError("The GProp center output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *center = {};
  return Guard([&]
  {
    ValidateGProps(properties);
    const gp_Pnt value = properties->Value.CentreOfMass();
    *center = { value.X(), value.Y(), value.Z() };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_inertia_value(
  const OcctSharp_GPropsHandle* properties, const int32_t row, const int32_t column, double* value)
{
  if (value == nullptr) { SetLastError("The GProp inertia output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0.0;
  if (row < 1 || row > 3 || column < 1 || column > 3) { SetLastError("GProp inertia indices are 1-based and must be 1..3."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateGProps(properties); *value = properties->Value.MatrixOfInertia().Value(row, column); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_principal_moments(
  const OcctSharp_GPropsHandle* properties, double* first, double* second, double* third)
{
  if (first == nullptr || second == nullptr || third == nullptr) { SetLastError("The principal-moment output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *first = *second = *third = 0.0;
  return Guard([&]
  {
    ValidateGProps(properties);
    properties->Value.PrincipalProperties().Moments(*first, *second, *third);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_symmetry(
  const OcctSharp_GPropsHandle* properties, int32_t* axis, int32_t* point)
{
  if (axis == nullptr || point == nullptr) { SetLastError("The symmetry output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *axis = *point = 0;
  return Guard([&]
  {
    ValidateGProps(properties);
    const GProp_PrincipalProps principal = properties->Value.PrincipalProperties();
    *axis = principal.HasSymmetryAxis() ? 1 : 0;
    *point = principal.HasSymmetryPoint() ? 1 : 0;
  });
}

void OCCTSHARP_CALL occtsharp_gprops_release(OcctSharp_GPropsHandle* properties)
{
  if (properties != nullptr && UnregisterValue(properties, LiveGProps)) delete properties;
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_curve_snapshot(
  const OcctSharp_ShapeHandle* edge, OcctSharp_EdgeCurveSnapshot* out_snapshot)
{
  if (out_snapshot == nullptr)
  {
    SetLastError("The edge curve snapshot output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_snapshot = {};
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A BRep curve snapshot requires an edge shape.");

    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (!std::isfinite(first) || !std::isfinite(last))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge curve does not have finite parameter bounds.");

    const gp_Pnt start = curve.Value(first);
    const gp_Pnt end = curve.Value(last);
    *out_snapshot = {
      static_cast<int32_t>(curve.GetType()),
      first,
      last,
      { start.X(), start.Y(), start.Z() },
      { end.X(), end.Y(), end.Z() }
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_surface_snapshot(
  const OcctSharp_ShapeHandle* face, const int32_t restrict_to_face,
  OcctSharp_FaceSurfaceSnapshot* out_snapshot)
{
  if (out_snapshot == nullptr)
  {
    SetLastError("The face surface snapshot output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_snapshot = {};
  if (restrict_to_face != 0 && restrict_to_face != 1)
  {
    SetLastError("The face surface restriction flag must be zero or one.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A BRep surface snapshot requires a face shape.");

    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), restrict_to_face != 0);
    *out_snapshot = {
      static_cast<int32_t>(surface.GetType()),
      surface.FirstUParameter(),
      surface.LastUParameter(),
      surface.FirstVParameter(),
      surface.LastVParameter()
    };
  });
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_null(
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The null shape output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&] { *out_shape = AllocateShape(TopoDS_Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_sphere(
  const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The output shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("Sphere radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { *out_shape = AllocateShape(BRepPrimAPI_MakeSphere(radius).Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cylinder(
  const double radius, const double height, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The output shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || !std::isfinite(height) || radius <= 0.0 || height <= 0.0)
  { SetLastError("Cylinder radius and height must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { *out_shape = AllocateShape(BRepPrimAPI_MakeCylinder(radius, height).Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cone(
  const double bottom_radius, const double top_radius, const double height,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The cone output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(bottom_radius) || !std::isfinite(top_radius) || !std::isfinite(height)
      || bottom_radius < 0.0 || top_radius < 0.0 || height <= 0.0
      || (bottom_radius == 0.0 && top_radius == 0.0) || bottom_radius == top_radius)
  {
    SetLastError("Cone radii must be finite, non-negative, different, and not both zero; height must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeCone builder(bottom_radius, top_radius, height);
    TopoDS_Shape result = builder.Shape();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT cone construction did not complete.");
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT cone construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_torus(
  const double major_radius, const double minor_radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The torus output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(major_radius) || !std::isfinite(minor_radius)
      || major_radius <= 0.0 || minor_radius <= 0.0 || major_radius <= minor_radius)
  {
    SetLastError("Torus radii must be finite and greater than zero, and the major radius must exceed the minor radius.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeTorus builder(major_radius, minor_radius);
    TopoDS_Shape result = builder.Shape();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT torus construction did not complete.");
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT torus construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_edge(
  const OcctSharp_Xyz start, const OcctSharp_Xyz end, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFinite(start.x, "Edge start X must be finite."); ValidateFinite(start.y, "Edge start Y must be finite."); ValidateFinite(start.z, "Edge start Z must be finite.");
    ValidateFinite(end.x, "Edge end X must be finite."); ValidateFinite(end.y, "Edge end Y must be finite."); ValidateFinite(end.z, "Edge end Z must be finite.");
    if (start.x == end.x && start.y == end.y && start.z == end.z)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Edge endpoints must be distinct.");
    BRepBuilderAPI_MakeEdge builder(gp_Pnt(start.x, start.y, start.z), gp_Pnt(end.x, end.y, end.z));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_polygon_wire(
  const OcctSharp_Xyz* points, const int32_t count, const int32_t close,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wire output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (count < 2 || points == nullptr) { SetLastError("A polygon wire requires at least two points."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_MakePolygon builder;
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(points[index].x, "Wire point X must be finite.");
      ValidateFinite(points[index].y, "Wire point Y must be finite.");
      ValidateFinite(points[index].z, "Wire point Z must be finite.");
      builder.Add(gp_Pnt(points[index].x, points[index].y, points[index].z));
    }
    if (close != 0) builder.Close();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT polygon wire construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_planar_face(
  const OcctSharp_ShapeHandle* wire, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(wire);
    if (wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar face construction requires a wire shape.");
    BRepBuilderAPI_MakeFace builder(TopoDS::Wire(wire->Value));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar face construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_snapshot(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_faces,
  const int32_t capacity,
  int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The face snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  if (capacity < 0 || (capacity > 0 && out_faces == nullptr))
  { SetLastError("The face snapshot capacity or output buffer is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    int32_t required = 0;
    for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next()) ++required;
    *out_written = required;
    if (capacity < required)
    { throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The face snapshot buffer is too small."); }
    int32_t index = 0;
    try
    {
      for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
      {
        out_faces[index++] = AllocateShape(TopoDS::Face(explorer.Current()));
      }
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(out_faces[cleanup]);
      *out_written = 0;
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_snapshot(
  const OcctSharp_ShapeHandle* shape,
  const int32_t kind,
  OcctSharp_ShapeHandle** out_shapes,
  const int32_t capacity,
  int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The subshape snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  if (kind < 0 || kind > 7) { SetLastError("The subshape kind must be a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (capacity < 0 || (capacity > 0 && out_shapes == nullptr))
  { SetLastError("The subshape snapshot capacity or output buffer is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    const TopAbs_ShapeEnum targetKind = static_cast<TopAbs_ShapeEnum>(kind);
    int32_t required = 0;
    for (TopExp_Explorer explorer(shape->Value, targetKind); explorer.More(); explorer.Next()) ++required;
    *out_written = required;
    if (capacity < required)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The subshape snapshot buffer is too small.");
    int32_t index = 0;
    try
    {
      for (TopExp_Explorer explorer(shape->Value, targetKind); explorer.More(); explorer.Next())
        out_shapes[index++] = AllocateShape(explorer.Current());
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(out_shapes[cleanup]);
      *out_written = 0;
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_count(
  const OcctSharp_ShapeHandle* shape, const int32_t kind, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The subshape count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_count = 0;
  if (kind < 0 || kind > 7) { SetLastError("The subshape kind must be a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    for (TopExp_Explorer explorer(shape->Value, static_cast<TopAbs_ShapeEnum>(kind)); explorer.More(); explorer.Next()) ++*out_count;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_extrude(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_VecHandle* direction,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The extrusion output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateVector(direction);
    if (direction->Value.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The extrusion direction must be non-zero.");
    BRepPrimAPI_MakePrism builder(shape->Value, direction->Value, false, false);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT extrusion did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT extrusion produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_revolve(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_Ax1Handle* axis,
  const double angle_radians, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The revolution output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  const double full_turn = 2.0 * std::acos(-1.0);
  if (!std::isfinite(angle_radians) || angle_radians == 0.0 || std::abs(angle_radians) > full_turn)
  {
    SetLastError("The revolution angle must be finite, non-zero, and no greater than one full turn in magnitude.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateAxis(axis);
    BRepPrimAPI_MakeRevol builder(shape->Value, axis->Value, angle_radians, false);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT revolution did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT revolution produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_all(
  const OcctSharp_ShapeHandle* shape, const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The fillet output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("The fillet radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> edges;
    TopExp::MapShapes(shape->Value, TopAbs_EDGE, edges);
    if (edges.IsEmpty()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The source shape has no edges to fillet.");
    BRepFilletAPI_MakeFillet builder(shape->Value);
    for (Standard_Integer index = 1; index <= edges.Extent(); ++index) builder.Add(radius, TopoDS::Edge(edges(index)));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The fillet output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("The fillet radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Fillet construction requires an edge shape.");
    BRepFilletAPI_MakeFillet builder(shape->Value);
    builder.Add(radius, TopoDS::Edge(edge->Value));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_all(
  const OcctSharp_ShapeHandle* shape, const double distance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The chamfer output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || distance <= 0.0) { SetLastError("The chamfer distance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> edges;
    TopExp::MapShapes(shape->Value, TopAbs_EDGE, edges);
    if (edges.IsEmpty()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The source shape has no edges to chamfer.");
    BRepFilletAPI_MakeChamfer builder(shape->Value);
    for (Standard_Integer index = 1; index <= edges.Extent(); ++index) builder.Add(distance, TopoDS::Edge(edges(index)));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  const double distance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The chamfer output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || distance <= 0.0) { SetLastError("The chamfer distance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Chamfer construction requires an edge shape.");
    BRepFilletAPI_MakeChamfer builder(shape->Value);
    builder.Add(distance, TopoDS::Edge(edge->Value));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_offset(
  const OcctSharp_ShapeHandle* shape, const double offset, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The offset output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(offset) || offset == 0.0 || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The offset must be finite and non-zero, and tolerance must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    BRepOffsetAPI_MakeOffsetShape builder;
    builder.PerformByJoin(shape->Value, offset, tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT offset construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT offset construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_section(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The section output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Section builder(left->Value, right->Value, false);
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_bounding_box(
  const OcctSharp_ShapeHandle* shape, OcctSharp_BoundingBox* out_bounds)
{
  if (out_bounds == nullptr) { SetLastError("The bounding-box output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_bounds = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    Bnd_Box box;
    BRepBndLib::AddOptimal(shape->Value, box, false, true);
    if (box.IsVoid() || box.IsOpen())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT did not produce finite shape bounds.");
    box.Get(out_bounds->min_x, out_bounds->min_y, out_bounds->min_z,
            out_bounds->max_x, out_bounds->max_y, out_bounds->max_z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_is_valid(
  const OcctSharp_ShapeHandle* shape, int32_t* out_is_valid)
{
  if (out_is_valid == nullptr) { SetLastError("The validity output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_is_valid = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    BRepCheck_Analyzer analyzer(shape->Value);
    *out_is_valid = analyzer.IsValid() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_fuse(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean fuse output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Fuse operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean fuse did not complete.");
    *out_shape = AllocateShape(operation.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_cut(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean cut output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Cut operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean cut did not complete.");
    *out_shape = AllocateShape(operation.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_common(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean common output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Common operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean common did not complete.");
    TopoDS_Shape result = operation.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean common produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_distance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_ShapeDistanceResult* out_result)
{
  if (out_result == nullptr) { SetLastError("The shape distance output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateUsableShape(first); ValidateUsableShape(second);
    BRepExtrema_DistShapeShape operation(first->Value, second->Value);
    if (!operation.IsDone() || operation.NbSolution() <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT shape distance did not produce a solution.");

    const gp_Pnt point1 = operation.PointOnShape1(1);
    const gp_Pnt point2 = operation.PointOnShape2(1);
    *out_result = {
      operation.Value(),
      { point1.X(), point1.Y(), point1.Z() },
      { point2.X(), point2.Y(), point2.Z() },
      operation.NbSolution()
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fix(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The shape fix output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ShapeFix_Shape fixer(shape->Value);
    fixer.Perform();
    TopoDS_Shape fixed = fixer.Shape();
    if (fixed.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "ShapeFix_Shape produced a null result.");
    }
    *out_shape = AllocateShape(std::move(fixed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_unify_same_domain(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The unify-same-domain output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ShapeUpgrade_UnifySameDomain operation(shape->Value, true, true, false);
    operation.Build();
    TopoDS_Shape unified = operation.Shape();
    if (unified.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "ShapeUpgrade_UnifySameDomain produced a null result.");
    }
    *out_shape = AllocateShape(std::move(unified));
  });
}

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

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_snapshot(
  const OcctSharp_RealSequenceHandle* sequence, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real sequence snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealSequence(sequence);
    const int32_t length = sequence->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real sequence snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = sequence->Value.Value(index + 1);
    *written = length;
  });
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_snapshot(
  const OcctSharp_RealArrayHandle* array, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real array snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealArray(array);
    const int32_t length = array->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real array snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = array->Value.Value(array->Value.Lower() + index);
    *written = length;
  });
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_snapshot(
  const OcctSharp_RealVectorHandle* vector, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real vector snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealVector(vector);
    const int32_t length = vector->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real vector snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = vector->Value.Value(index);
    *written = length;
  });
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_snapshot(
  const OcctSharp_IntRealMapHandle* map, int32_t* keys, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The integer-real map snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateIntRealMap(map);
    const int32_t extent = map->Value.Extent();
    if (capacity < extent || (extent > 0 && (keys == nullptr || values == nullptr))) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The integer-real map snapshot buffers are too small or null.");
    int32_t index = 0;
    for (NCollection_DataMap<int32_t, double>::Iterator iterator(map->Value); iterator.More(); iterator.Next())
    {
      keys[index] = iterator.Key();
      values[index] = iterator.Value();
      ++index;
    }
    *written = extent;
  });
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_snapshot(
  const OcctSharp_IntIndexedMapHandle* map, int32_t* keys, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The indexed map snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateIntIndexedMap(map);
    const int32_t extent = map->Value.Extent();
    if (capacity < extent || (extent > 0 && keys == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The indexed map snapshot buffer is too small or null.");
    for (int32_t index = 0; index < extent; ++index) keys[index] = map->Value.FindKey(index + 1);
    *written = extent;
  });
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

namespace
{
TCollection_ExtendedString MakeExtendedUtf8(const char* utf8, const int32_t length)
{
  ValidateUtf8Input(utf8, length);
  return TCollection_ExtendedString(MakeAsciiString(utf8, length), true);
}

std::string ExtendedToUtf8(const TCollection_ExtendedString& value)
{
  const int32_t capacity = value.LengthOfCString() + 1;
  std::string result(static_cast<size_t>(capacity), '\0');
  Standard_PCharacter output = result.data();
  const int32_t written = value.ToUTF8CString(output);
  result.resize(static_cast<size_t>(written));
  return result;
}

void CopyUtf8Result(const std::string& value, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output length pointer is null.");
  }
  *written = 0;
  ValidateOutputBuffer(buffer, capacity, static_cast<int32_t>(value.size()) + 1);
  if (!value.empty())
  {
    std::memcpy(buffer, value.data(), value.size());
  }
  buffer[value.size()] = '\0';
  *written = static_cast<int32_t>(value.size());
}

TDF_Label ResolveOcafLabel(const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  ValidateOcafDocument(document);
  if (entry == nullptr || entry[0] == '\0')
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label entry is null or empty.");
  }
  TDF_Label label;
  TDF_Tool::Label(document->Document->GetData(), entry, label, false);
  if (label.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label entry does not exist.");
  }
  return label;
}

void RequireOpenOcafCommand(const OcctSharp_OcafDocumentHandle* document)
{
  if (!document->Document->HasOpenCommand())
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "An OCAF transaction must be open before modifying labels.");
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_create(
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output OCAF document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    BinDrivers::DefineFormat(application);
    opencascade::handle<TDocStd_Document> document;
    application->NewDocument(TCollection_ExtendedString("BinOcaf"), document);
    if (document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null OCAF document.");
    }
    document->SetUndoLimit(10);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output OCAF document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    BinDrivers::DefineFormat(application);
    opencascade::handle<TDocStd_Document> document;
    const PCDM_ReaderStatus status = application->Open(
      TCollection_ExtendedString(file_path, true), document);
    if (status != PCDM_RS_OK || document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not open the binary OCAF document.");
    }
    document->SetUndoLimit(10);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_save(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "The OCAF transaction must be committed or aborted before saving.");
    }
    const PCDM_StoreStatus status = document->Application->SaveAs(
      document->Document, TCollection_ExtendedString(file_path, true));
    if (status != PCDM_SS_OK)
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not save the binary OCAF document.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_has_open_command(
  const OcctSharp_OcafDocumentHandle* document, int32_t* has_open_command)
{
  if (has_open_command == nullptr)
  {
    SetLastError("The OCAF command-state output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_open_command = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    *has_open_command = document->Document->HasOpenCommand() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_begin_command(
  OcctSharp_OcafDocumentHandle* document)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An OCAF transaction is already open.");
    }
    document->Document->NewCommand();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_commit_command(
  OcctSharp_OcafDocumentHandle* document, int32_t* changed)
{
  if (changed == nullptr)
  {
    SetLastError("The OCAF commit result pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *changed = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    *changed = document->Document->CommitCommand() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_abort_command(
  OcctSharp_OcafDocumentHandle* document)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    document->Document->AbortCommand();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_main_entry(
  const OcctSharp_OcafDocumentHandle* document,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    TCollection_AsciiString entry;
    TDF_Tool::Entry(document->Document->Main(), entry);
    CopyUtf8Result(std::string(entry.ToCString()), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_add_child(
  OcctSharp_OcafDocumentHandle* document, const char* parent_entry, int32_t* child_tag)
{
  if (child_tag == nullptr)
  {
    SetLastError("The OCAF child-tag output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *child_tag = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const TDF_Label parent = ResolveOcafLabel(document, parent_entry);
    const TDF_Label child = TDF_TagSource::NewChild(parent);
    if (child.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null OCAF child label.");
    }
    *child_tag = child.Tag();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_child_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The OCAF child-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&] { *count = ResolveOcafLabel(document, entry).NbChildren(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_set_name(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* utf8,
  const int32_t length)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    TDataStd_Name::Set(ResolveOcafLabel(document, entry), MakeExtendedUtf8(utf8, length));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_name,
  int32_t* length)
{
  if (has_name == nullptr || length == nullptr)
  {
    SetLastError("An OCAF name metadata output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_name = 0;
  *length = 0;
  return Guard([&]
  {
    opencascade::handle<TDataStd_Name> name;
    if (ResolveOcafLabel(document, entry).FindAttribute(TDataStd_Name::GetID(), name))
    {
      *has_name = 1;
      *length = name->Get().LengthOfCString();
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    opencascade::handle<TDataStd_Name> name;
    if (!ResolveOcafLabel(document, entry).FindAttribute(TDataStd_Name::GetID(), name))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label has no name attribute.");
    }
    CopyUtf8Result(ExtendedToUtf8(name->Get()), buffer, capacity, written);
  });
}

void OCCTSHARP_CALL occtsharp_ocaf_document_release(OcctSharp_OcafDocumentHandle* document)
{
  if (document != nullptr && UnregisterValue(document, LiveOcafDocuments))
  {
    if (!document->Application.IsNull() && !document->Document.IsNull())
    {
      if (document->Document->HasOpenCommand())
      {
        document->Document->AbortCommand();
      }
      document->Application->Close(document->Document);
    }
    delete document;
  }
}

namespace
{
OcctSharp_OcafDocumentHandle* CreateOwnedXdeDocument()
{
  opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
  BinXCAFDrivers::DefineFormat(application);
  opencascade::handle<TDocStd_Document> document;
  application->NewDocument(TCollection_ExtendedString("BinXCAF"), document);
  if (document.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null XDE document.");
  }
  document->SetUndoLimit(10);
  InitializeXdeTools(document);
  return AllocateValue(
    new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
    LiveOcafDocuments);
}

void CopyLabelEntry(const TDF_Label& label, char* buffer, const int32_t capacity, int32_t* written)
{
  if (label.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT returned a null XDE label.");
  }
  TCollection_AsciiString entry;
  TDF_Tool::Entry(label, entry);
  CopyUtf8Result(std::string(entry.ToCString()), buffer, capacity, written);
}

opencascade::handle<XCAFDoc_ShapeTool> GetXdeShapeTool(
  const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ShapeTool(document->Document->Main());
}

bool GetAssignedMaterial(
  const TDF_Label& label,
  opencascade::handle<TCollection_HAsciiString>& name,
  opencascade::handle<TCollection_HAsciiString>& description,
  double& density,
  opencascade::handle<TCollection_HAsciiString>& densityName,
  opencascade::handle<TCollection_HAsciiString>& densityType)
{
  opencascade::handle<TDataStd_TreeNode> reference;
  if (!label.FindAttribute(XCAFDoc::MaterialRefGUID(), reference) || !reference->HasFather())
  {
    return false;
  }
  return XCAFDoc_MaterialTool::GetMaterial(
    reference->Father()->Label(), name, description, density, densityName, densityType);
}

std::string MaterialFieldUtf8(const TDF_Label& label, const int32_t field)
{
  opencascade::handle<TCollection_HAsciiString> name;
  opencascade::handle<TCollection_HAsciiString> description;
  opencascade::handle<TCollection_HAsciiString> densityName;
  opencascade::handle<TCollection_HAsciiString> densityType;
  double density = 0.0;
  if (!GetAssignedMaterial(label, name, description, density, densityName, densityType))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label has no material assignment.");
  }
  const opencascade::handle<TCollection_HAsciiString>* selected = nullptr;
  switch (field)
  {
    case 0: selected = &name; break;
    case 1: selected = &description; break;
    case 2: selected = &densityName; break;
    case 3: selected = &densityType; break;
    default:
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE material field index is invalid.");
  }
  return selected->IsNull() ? std::string() : std::string((*selected)->ToCString());
}

opencascade::handle<NCollection_HSequence<TCollection_ExtendedString>> GetXdeLayers(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry)
{
  const TDF_Label label = ResolveOcafLabel(document, entry);
  return XCAFDoc_DocumentTool::LayerTool(document->Document->Main())->GetLayers(label);
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_create(
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&] { *out_document = CreateOwnedXdeDocument(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    BinXCAFDrivers::DefineFormat(application);
    opencascade::handle<TDocStd_Document> document;
    const PCDM_ReaderStatus status = application->Open(
      TCollection_ExtendedString(file_path, true), document);
    if (status != PCDM_RS_OK || document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not open the binary XDE document.");
    }
    document->SetUndoLimit(10);
    InitializeXdeTools(document);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      STEPCAFControl_Reader reader;
      ConfigureXdeReader(reader);
      if (reader.ReadFile(file_path) != IFSelect_RetDone || !reader.Transfer(result->Document))
      {
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer STEP into the XDE document.");
      }
      *out_document = result;
    }
    catch (...)
    {
      occtsharp_ocaf_document_release(result);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before STEP export.");
    }
    STEPCAFControl_Writer writer;
    ConfigureXdeWriter(writer);
    if (!writer.Transfer(document->Document, STEPControl_AsIs)
        || writer.Write(file_path) != IFSelect_RetDone)
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not write the XDE STEP document.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_shape(
  OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_ShapeHandle* shape,
  const char* name_utf8,
  const int32_t name_length,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateUsableShape(shape);
    TDF_Label label = GetXdeShapeTool(document)->AddShape(shape->Value, false, false);
    TDataStd_Name::Set(label, MakeExtendedUtf8(name_utf8, name_length));
    CopyLabelEntry(label, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_assembly(
  OcctSharp_OcafDocumentHandle* document,
  const char* name_utf8,
  const int32_t name_length,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    TDF_Label label = GetXdeShapeTool(document)->NewShape();
    TDataStd_Name::Set(label, MakeExtendedUtf8(name_utf8, name_length));
    CopyLabelEntry(label, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_component(
  OcctSharp_OcafDocumentHandle* document,
  const char* assembly_entry,
  const char* part_entry,
  const OcctSharp_LocationHandle* location,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateLocationHandle(location);
    const TDF_Label assembly = ResolveOcafLabel(document, assembly_entry);
    const TDF_Label part = ResolveOcafLabel(document, part_entry);
    TDF_Label occurrence = GetXdeShapeTool(document)->AddComponent(assembly, part, location->Value);
    GetXdeShapeTool(document)->UpdateAssemblies();
    CopyLabelEntry(occurrence, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_shape(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output XDE shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    TopoDS_Shape shape;
    if (!XCAFDoc_ShapeTool::GetShape(ResolveOcafLabel(document, entry), shape) || shape.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The XDE label does not contain a shape.");
    }
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_is_assembly(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* is_assembly)
{
  if (is_assembly == nullptr)
  {
    SetLastError("The XDE assembly-state output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *is_assembly = 0;
  return Guard([&] { *is_assembly = XCAFDoc_ShapeTool::IsAssembly(ResolveOcafLabel(document, entry)) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE component-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&] { *count = XCAFDoc_ShapeTool::NbComponents(ResolveOcafLabel(document, entry), false); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> components;
    if (!XCAFDoc_ShapeTool::GetComponents(ResolveOcafLabel(document, entry), components, false)
        || index < 1 || index > components.Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE component index is out of range.");
    }
    CopyLabelEntry(components.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_referred_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    TDF_Label referred;
    if (!XCAFDoc_ShapeTool::GetReferredShape(ResolveOcafLabel(document, entry), referred))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a component occurrence.");
    }
    CopyLabelEntry(referred, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_location(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output XDE location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_location = nullptr;
  return Guard([&]
  {
    *out_location = AllocateLocation(XCAFDoc_ShapeTool::GetLocation(ResolveOcafLabel(document, entry)));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_count(
  const OcctSharp_OcafDocumentHandle* document, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE free-shape count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    GetXdeShapeTool(document)->GetFreeShapes(labels);
    *count = labels.Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    GetXdeShapeTool(document)->GetFreeShapes(labels);
    if (index < 1 || index > labels.Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE free-shape index is out of range.");
    }
    CopyLabelEntry(labels.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_color(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_XdeColor color)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const double values[] = {color.red, color.green, color.blue, color.alpha};
    for (const double value : values)
    {
      if (!std::isfinite(value) || value < 0.0 || value > 1.0)
      {
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE color channels must be finite values from zero through one.");
      }
    }
    const Quantity_ColorRGBA nativeColor(
      static_cast<float>(color.red),
      static_cast<float>(color.green),
      static_cast<float>(color.blue),
      static_cast<float>(color.alpha));
    const TDF_Label label = ResolveOcafLabel(document, entry);
    const opencascade::handle<XCAFDoc_ColorTool> colorTool =
      XCAFDoc_DocumentTool::ColorTool(document->Document->Main());
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorGen);
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorSurf);
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorCurv);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_color(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_color,
  OcctSharp_XdeColor* color)
{
  if (has_color == nullptr || color == nullptr)
  {
    SetLastError("An XDE color output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_color = 0;
  *color = {};
  return Guard([&]
  {
    Quantity_ColorRGBA nativeColor;
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (XCAFDoc_ColorTool::GetColor(label, XCAFDoc_ColorGen, nativeColor)
        || XCAFDoc_ColorTool::GetColor(label, XCAFDoc_ColorSurf, nativeColor)
        || XCAFDoc_ColorTool::GetColor(label, XCAFDoc_ColorCurv, nativeColor))
    {
      *has_color = 1;
      *color = {
        nativeColor.GetRGB().Red(), nativeColor.GetRGB().Green(),
        nativeColor.GetRGB().Blue(), nativeColor.Alpha()};
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_layer(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* layer_utf8,
  const int32_t layer_length,
  const int32_t replace_existing)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    XCAFDoc_DocumentTool::LayerTool(document->Document->Main())->SetLayer(
      ResolveOcafLabel(document, entry), MakeExtendedUtf8(layer_utf8, layer_length), replace_existing != 0);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE layer count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    *count = layers.IsNull() ? 0 : layers->Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  int32_t* length)
{
  if (length == nullptr)
  {
    SetLastError("The XDE layer-name length pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *length = 0;
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    if (layers.IsNull() || index < 1 || index > layers->Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE layer index is out of range.");
    }
    *length = layers->Value(index).LengthOfCString();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    if (layers.IsNull() || index < 1 || index > layers->Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE layer index is out of range.");
    }
    CopyUtf8Result(ExtendedToUtf8(layers->Value(index)), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_material(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* name,
  const int32_t name_length,
  const char* description,
  const int32_t description_length,
  const double density,
  const char* density_name,
  const int32_t density_name_length,
  const char* density_type,
  const int32_t density_type_length)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateFinite(density, "XDE material density must be finite.");
    if (density < 0.0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE material density cannot be negative.");
    }
    ValidateUtf8Input(name, name_length);
    ValidateUtf8Input(description, description_length);
    ValidateUtf8Input(density_name, density_name_length);
    ValidateUtf8Input(density_type, density_type_length);
    auto makeString = [](const char* value, const int32_t length)
    {
      return opencascade::handle<TCollection_HAsciiString>(
        new TCollection_HAsciiString(MakeAsciiString(value, length)));
    };
    XCAFDoc_DocumentTool::MaterialTool(document->Document->Main())->SetMaterial(
      ResolveOcafLabel(document, entry),
      makeString(name, name_length),
      makeString(description, description_length),
      density,
      makeString(density_name, density_name_length),
      makeString(density_type, density_type_length));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_info(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_material,
  double* density)
{
  if (has_material == nullptr || density == nullptr)
  {
    SetLastError("An XDE material output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *density = 0.0;
  return Guard([&]
  {
    opencascade::handle<TCollection_HAsciiString> name;
    opencascade::handle<TCollection_HAsciiString> description;
    opencascade::handle<TCollection_HAsciiString> densityName;
    opencascade::handle<TCollection_HAsciiString> densityType;
    *has_material = GetAssignedMaterial(
      ResolveOcafLabel(document, entry), name, description, *density, densityName, densityType) ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t field,
  int32_t* length)
{
  if (length == nullptr)
  {
    SetLastError("The XDE material field length pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *length = 0;
  return Guard([&] { *length = static_cast<int32_t>(MaterialFieldUtf8(ResolveOcafLabel(document, entry), field).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t field,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    CopyUtf8Result(MaterialFieldUtf8(ResolveOcafLabel(document, entry), field), buffer, capacity, written);
  });
}

namespace
{
void ValidateViewer(const OcctSharp_ViewerHandle* viewer)
{
  if (viewer == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The viewer handle is null.");
  }
  if (!IsLiveValue(viewer, LiveViewers))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The viewer handle is invalid or already released.");
  }
}

void ValidateViewerThread(const OcctSharp_ViewerHandle* viewer)
{
  ValidateViewer(viewer);
  if (viewer->OwnerThread != std::this_thread::get_id())
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Viewer operations must run on the thread that created the viewer.");
  }
}

opencascade::handle<AIS_Shape> FindPresentation(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t presentationId)
{
  const auto iterator = viewer->Presentations.find(presentationId);
  if (iterator == viewer->Presentations.end())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer presentation ID does not exist.");
  }
  return iterator->second;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create(
  const intptr_t window_handle, OcctSharp_ViewerHandle** out_viewer)
{
  if (out_viewer == nullptr)
  {
    SetLastError("The output viewer pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_viewer = nullptr;
  return Guard([&]
  {
    if (window_handle == 0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A non-zero native window handle is required.");
    }
    std::unique_ptr<OcctSharp_ViewerHandle> viewer(new OcctSharp_ViewerHandle());
    viewer->OwnerThread = std::this_thread::get_id();
    viewer->Display = new Aspect_DisplayConnection();
    viewer->Driver = new OpenGl_GraphicDriver(viewer->Display);
    viewer->Viewer = new V3d_Viewer(viewer->Driver);
    viewer->Viewer->SetDefaultLights();
    viewer->Viewer->SetLightOn();
    viewer->Context = new AIS_InteractiveContext(viewer->Viewer);
    viewer->View = viewer->Viewer->CreateView();
    viewer->Window = new WNT_Window(reinterpret_cast<Aspect_Handle>(window_handle));
    viewer->View->SetWindow(viewer->Window);
    viewer->View->MustBeResized();
    *out_viewer = AllocateValue(viewer.release(), LiveViewers);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_shape(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ShapeHandle* shape,
  int64_t* presentation_id)
{
  if (presentation_id == nullptr)
  {
    SetLastError("The viewer presentation ID output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *presentation_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateUsableShape(shape);
    opencascade::handle<AIS_Shape> presentation = new AIS_Shape(shape->Value);
    const int64_t id = viewer->NextPresentationId++;
    viewer->Presentations.emplace(id, presentation);
    viewer->Context->Display(presentation, false);
    *presentation_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_visible(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const int32_t visible)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_Shape> presentation = FindPresentation(viewer, presentation_id);
    if (visible != 0) viewer->Context->Display(presentation, false);
    else viewer->Context->Erase(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_presentation(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_Shape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->Remove(presentation, false);
    viewer->Presentations.erase(presentation_id);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_all(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->FitAll(0.01, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_redraw(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_resize(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->MustBeResized();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_move_to(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  int32_t* detected)
{
  if (detected == nullptr)
  {
    SetLastError("The viewer detection output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *detected = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->MoveTo(x, y, viewer->View, false);
    *detected = viewer->Context->HasDetected() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  int32_t* selected_count)
{
  if (selected_count == nullptr)
  {
    SetLastError("The viewer selected-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *selected_count = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->MoveTo(x, y, viewer->View, false);
    viewer->Context->SelectDetected(AIS_SelectionScheme_Replace);
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_snapshot(
  OcctSharp_ViewerHandle* viewer,
  int64_t* presentation_ids,
  const int32_t capacity,
  int32_t* written)
{
  if (written == nullptr)
  {
    SetLastError("The viewer selection snapshot count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *written = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    std::vector<int64_t> ids;
    for (viewer->Context->InitSelected(); viewer->Context->MoreSelected(); viewer->Context->NextSelected())
    {
      const opencascade::handle<AIS_InteractiveObject> selected = viewer->Context->SelectedInteractive();
      for (const auto& presentation : viewer->Presentations)
      {
        if (presentation.second == selected)
        {
          ids.push_back(presentation.first);
          break;
        }
      }
    }
    if (capacity < static_cast<int32_t>(ids.size()) || (!ids.empty() && presentation_ids == nullptr))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selection snapshot buffer is too small or null.");
    }
    for (size_t index = 0; index < ids.size(); ++index) presentation_ids[index] = ids[index];
    *written = static_cast<int32_t>(ids.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_count(
  OcctSharp_ViewerHandle* viewer,
  int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The viewer selected-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    *count = viewer->Context->NbSelected();
  });
}

void OCCTSHARP_CALL occtsharp_viewer_release(OcctSharp_ViewerHandle* viewer)
{
  if (viewer != nullptr && UnregisterValue(viewer, LiveViewers))
  {
    if (!viewer->Context.IsNull()) viewer->Context->RemoveAll(false);
    viewer->Presentations.clear();
    viewer->View.Nullify();
    viewer->Context.Nullify();
    viewer->Viewer.Nullify();
    viewer->Driver.Nullify();
    viewer->Window.Nullify();
    viewer->Display.Nullify();
    delete viewer;
  }
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
