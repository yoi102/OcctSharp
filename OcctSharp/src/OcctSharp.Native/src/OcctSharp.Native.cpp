#include "OcctSharp.Native.h"
#include "OcctSharp.Native.Internal.hxx"

#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepAlgoAPI_Splitter.hxx>
#include <BRepFill.hxx>
#include <BRepFill_Filling.hxx>
#include <BRep_Tool.hxx>
#include <BRepTools.hxx>
#include <BRepTools_ReShape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <IMeshTools_Parameters.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_BooleanOperation.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepBndLib.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepCheck_Result.hxx>
#include <BRepCheck_Status.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_MakePipeShell.hxx>
#include <BRepOffsetAPI_MakeOffset.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GCPnts_AbscissaPoint.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <GeomAPI_PointsToBSpline.hxx>
#include <GeomAPI_PointsToBSplineSurface.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <GeomAPI_ExtremaCurveCurve.hxx>
#include <GeomAPI_IntCS.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_Curve.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Geom_Surface.hxx>
#include <Geom2d_Curve.hxx>
#include <HLRAlgo_Projector.hxx>
#include <HLRBRep_Algo.hxx>
#include <HLRBRep_HLRToShape.hxx>
#include <HLRBRep_PolyAlgo.hxx>
#include <HLRBRep_PolyHLRToShape.hxx>
#include <AIS_InteractiveContext.hxx>
#include <AIS_SelectionScheme.hxx>
#include <AIS_ColoredShape.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Bnd_Box.hxx>
#include <BinDrivers.hxx>
#include <BinXCAFDrivers.hxx>
#include <DEGLTF_Provider.hxx>
#include <DEOBJ_ConfigurationNode.hxx>
#include <DEOBJ_Provider.hxx>
#include <DEPLY_ConfigurationNode.hxx>
#include <DEPLY_Provider.hxx>
#include <DEVRML_ConfigurationNode.hxx>
#include <DEVRML_Provider.hxx>
#include <DESTEP_Parameters.hxx>
#include <GProp_GProps.hxx>
#include <GProp_PrincipalProps.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Writer.hxx>
#include <IGESControl_Reader.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_Array2.hxx>
#include <NCollection_DynamicArray.hxx>
#include <NCollection_IndexedDataMap.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_HArray1.hxx>
#include <NCollection_List.hxx>
#include <NCollection_Sequence.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <Graphic3d_BufferType.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_ClipPlane.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_StepModelType.hxx>
#include <STEPControl_Writer.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeFix_Face.hxx>
#include <ShapeFix_Shell.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <Standard_TypeDef.hxx>
#include <Standard_Failure.hxx>
#include <Standard_Handle.hxx>
#include <Standard_Type.hxx>
#include <Standard_Version.hxx>
#include <Standard_Transient.hxx>
#include <StdSelect_ShapeTypeFilter.hxx>
#include <SelectMgr_Filter.hxx>
#include <StlAPI_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_AsciiString.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_RealArray.hxx>
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
#include <TopoDS_Edge.hxx>
#include <TopoDS_Shape.hxx>
#include <Poly_Triangulation.hxx>
#include <Poly_Triangle.hxx>
#include <Precision.hxx>
#include <PrsDim_AngleDimension.hxx>
#include <PrsDim_DiameterDimension.hxx>
#include <PrsDim_Dimension.hxx>
#include <PrsDim_LengthDimension.hxx>
#include <PrsDim_RadiusDimension.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Quantity_Color.hxx>
#include <TCollection_HAsciiString.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_ClippingPlaneTool.hxx>
#include <XCAFDoc_Datum.hxx>
#include <XCAFDoc_Dimension.hxx>
#include <XCAFDoc_DimTolTool.hxx>
#include <XCAFDoc_GeomTolerance.hxx>
#include <XCAFDoc_View.hxx>
#include <XCAFDoc_ViewTool.hxx>
#include <XCAFDoc_Area.hxx>
#include <XCAFDoc_Centroid.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_Volume.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc_VisMaterialTool.hxx>
#include <XCAFDoc_VisMaterialPBR.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_GraphNode.hxx>
#include <XCAFDimTolObjects_DatumObject.hxx>
#include <XCAFDimTolObjects_DimensionObject.hxx>
#include <XCAFDimTolObjects_GeomToleranceObject.hxx>
#include <XCAFView_Object.hxx>
#include <V3d_View.hxx>
#include <V3d_TypeOfOrientation.hxx>
#include <V3d_TypeOfVisualization.hxx>
#include <V3d_Viewer.hxx>
#include <WNT_Window.hxx>
#include <gp_Ax1.hxx>
#include <gp.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <gp_Mat.hxx>
#include <gp_XYZ.hxx>
#include <gp_Lin.hxx>
#include <gp_Circ.hxx>
#include <gp_Elips.hxx>
#include <gp_Ax2.hxx>
#include <gp_Ax3.hxx>
#include <gp_Pln.hxx>

#include <algorithm>
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
static_assert(sizeof(OcctSharp_DrawingProjection) == 88);
static_assert(sizeof(OcctSharp_DrawingPolyline) == 12);
static_assert(sizeof(OcctSharp_ViewerCamera) == 96);
static_assert(sizeof(OcctSharp_ViewerPickRay) == 48);
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
static_assert(sizeof(OcctSharp_CurveEvaluation) == 56);
static_assert(sizeof(OcctSharp_CurveDerivativeEvaluation) == 80);
static_assert(sizeof(OcctSharp_Xy) == 16);
static_assert(sizeof(OcctSharp_PcurveSnapshot) == 48);
static_assert(sizeof(OcctSharp_PcurveEvaluation) == 40);
static_assert(sizeof(OcctSharp_CurveProjection) == 48);
static_assert(sizeof(OcctSharp_SurfaceEvaluation) == 64);
static_assert(sizeof(OcctSharp_SurfaceDerivativeEvaluation) == 112);
static_assert(sizeof(OcctSharp_SurfaceProjection) == 56);
static_assert(sizeof(OcctSharp_BooleanHistorySummary) == 48);
static_assert(offsetof(OcctSharp_CurveEvaluation, point) == 8);
static_assert(offsetof(OcctSharp_CurveProjection, solution_count) == 40);
static_assert(offsetof(OcctSharp_SurfaceEvaluation, point) == 16);
static_assert(offsetof(OcctSharp_SurfaceProjection, solution_count) == 48);
static_assert(sizeof(OcctSharp_ShapeDistanceResult) == 64);
static_assert(sizeof(OcctSharp_BoundingBox) == 48);
static_assert(alignof(OcctSharp_BoundingBox) == 8);
static_assert(offsetof(OcctSharp_BoundingBox, min_x) == 0);
static_assert(offsetof(OcctSharp_BoundingBox, max_x) == 24);
static_assert(sizeof(OcctSharp_XdeColor) == 32);
static_assert(sizeof(OcctSharp_XdeValidationProperties) == 56);
static_assert(alignof(OcctSharp_XdeValidationProperties) == 8);
static_assert(offsetof(OcctSharp_XdeValidationProperties, area) == 0);
static_assert(offsetof(OcctSharp_XdeValidationProperties, centroid) == 16);
static_assert(offsetof(OcctSharp_XdeValidationProperties, has_area) == 40);
static_assert(sizeof(OcctSharp_TopologyCounts) == 32);
static_assert(sizeof(OcctSharp_ShapeTopologySummary) == 120);
static_assert(alignof(OcctSharp_ShapeTopologySummary) == 8);
static_assert(offsetof(OcctSharp_ShapeTopologySummary, unique_counts) == 0);
static_assert(offsetof(OcctSharp_ShapeTopologySummary, occurrence_counts) == 32);
static_assert(offsetof(OcctSharp_ShapeTopologySummary, is_closed) == 64);
static_assert(offsetof(OcctSharp_ShapeTopologySummary, min_vertex_tolerance) == 72);
static_assert(sizeof(OcctSharp_DetailedMeshVertex) == 72);
static_assert(alignof(OcctSharp_DetailedMeshVertex) == 8);
static_assert(offsetof(OcctSharp_DetailedMeshVertex, has_uv) == 64);
static_assert(sizeof(OcctSharp_DetailedMeshTriangle) == 20);
static_assert(sizeof(OcctSharp_ValidationIssue) == 8);
static_assert(sizeof(OcctSharp_StepReadReport) == 24);
static_assert(offsetof(OcctSharp_StepReadReport, system_length_unit) == 16);
static_assert(sizeof(OcctSharp_StepReaderInfo) == 32);
static_assert(offsetof(OcctSharp_StepReaderInfo, system_length_unit) == 8);
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
  std::unordered_map<int64_t, opencascade::handle<AIS_ColoredShape>> Presentations;
  std::unordered_map<int64_t, opencascade::handle<PrsDim_Dimension>> Dimensions;
  std::unordered_map<int64_t, opencascade::handle<Graphic3d_ClipPlane>> ClipPlanes;
  opencascade::handle<SelectMgr_Filter> ActiveFilter;
  int64_t NextPresentationId = 1;
  int64_t NextDimensionId = 1;
  int64_t NextClipPlaneId = 1;
  std::thread::id OwnerThread;
};

struct OcctSharp_StepReaderHandle
{
  STEPControl_Reader Reader;
  IFSelect_ReturnStatus ReadStatus = IFSelect_RetVoid;
  std::vector<std::string> LengthUnits;
  std::vector<std::string> AngleUnits;
  std::vector<std::string> SolidAngleUnits;
};

namespace
{
constexpr uint32_t AbiVersion = 0x00010032U;
constexpr const char* BridgeVersion = "0.58.0";
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
std::unordered_set<const OcctSharp_StepReaderHandle*> LiveStepReaders;

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

void ValidateStepReader(const OcctSharp_StepReaderHandle* reader)
{
  if (reader == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The STEP reader handle is null.");
  if (!IsLiveValue(reader, LiveStepReaders))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The STEP reader handle is invalid or already released.");
}

void ValidateUsableShape(const OcctSharp_ShapeHandle* shape)
{
  ValidateShape(shape);
  if (shape->Value.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology shape is null.");
  }
}

gp_Pnt ToPoint(const OcctSharp_Xyz& value, const char* message)
{
  if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  return gp_Pnt(value.x, value.y, value.z);
}

gp_Vec ToVector(const OcctSharp_Xyz& value, const char* message)
{
  if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  const gp_Vec result(value.x, value.y, value.z);
  if (result.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  return result;
}

OcctSharp_Xyz FromPoint(const gp_Pnt& value)
{
  return {value.X(), value.Y(), value.Z()};
}

struct DrawingPolylineData
{
  std::vector<OcctSharp_DrawingPolyline> Polylines;
  std::vector<OcctSharp_Xyz> Points;
};

TopoDS_Shape NonNullDrawingLayer(TopoDS_Shape shape)
{
  if (!shape.IsNull()) return shape;
  TopoDS_Compound compound;
  BRep_Builder builder;
  builder.MakeCompound(compound);
  return compound;
}

HLRAlgo_Projector MakeDrawingProjector(const OcctSharp_DrawingProjection& projection)
{
  const gp_Pnt origin = ToPoint(projection.origin, "Drawing projection origin must be finite.");
  const gp_Vec view = ToVector(projection.view_direction, "Drawing view direction must be finite and non-zero.");
  const gp_Vec up = ToVector(projection.up_direction, "Drawing up direction must be finite and non-zero.");
  gp_Vec right = up.Crossed(view);
  if (right.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing up and view directions must not be parallel.");
  if ((projection.perspective != 0 && projection.perspective != 1)
      || !std::isfinite(projection.focus) || projection.focus <= 0.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing perspective flag or focus is invalid.");
  const gp_Ax2 coordinate_system(origin, gp_Dir(view), gp_Dir(right));
  return projection.perspective == 0
    ? HLRAlgo_Projector(coordinate_system)
    : HLRAlgo_Projector(coordinate_system, projection.focus);
}

DrawingPolylineData BuildDrawingPolylines(const TopoDS_Shape& shape, const int32_t samples_per_curve)
{
  if (samples_per_curve < 2 || samples_per_curve > 4096)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing curve samples must be between 2 and 4096.");

  DrawingPolylineData data;
  for (TopExp_Explorer explorer(shape, TopAbs_EDGE); explorer.More(); explorer.Next())
  {
    const TopoDS_Edge edge = TopoDS::Edge(explorer.Current());
    BRepAdaptor_Curve curve(edge);
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (!std::isfinite(first) || !std::isfinite(last) || last < first) continue;
    const int32_t count = curve.GetType() == GeomAbs_Line ? 2 : samples_per_curve;
    if (data.Points.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max() - count))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing polyline point count exceeds the 32-bit ABI.");
    const int32_t offset = static_cast<int32_t>(data.Points.size());
    for (int32_t index = 0; index < count; ++index)
    {
      const double parameter = count == 1 ? first : first + (last - first) * index / (count - 1);
      data.Points.push_back(FromPoint(curve.Value(parameter)));
    }
    const bool closed = count > 2
      && curve.Value(first).SquareDistance(curve.Value(last)) <= Precision::SquareConfusion();
    data.Polylines.push_back({offset, count, closed ? 1 : 0});
  }
  return data;
}

opencascade::handle<Geom_Curve> GetEdgeCurve(const OcctSharp_ShapeHandle* edge)
{
  ValidateUsableShape(edge);
  if (edge->Value.ShapeType() != TopAbs_EDGE)
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The freeform operation requires an edge.");
  double first = 0.0;
  double last = 0.0;
  opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), first, last);
  if (curve.IsNull())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no usable 3D curve.");
  return curve;
}

opencascade::handle<Geom_Surface> GetFaceSurface(const OcctSharp_ShapeHandle* face)
{
  ValidateUsableShape(face);
  if (face->Value.ShapeType() != TopAbs_FACE)
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The freeform operation requires a face.");
  opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(TopoDS::Face(face->Value));
  if (surface.IsNull())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no usable surface.");
  return surface;
}

void ValidateOutputCapacity(const int32_t capacity, const int32_t required, const void* buffer, const char* message)
{
  if (capacity < required || (required > 0 && buffer == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}

GeomAbs_Shape ToContinuity(const int32_t value)
{
  switch (value)
  {
    case 0: return GeomAbs_C0;
    case 1: return GeomAbs_C1;
    case 2: return GeomAbs_C2;
    case 3: return GeomAbs_C3;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Continuity must be between C0 and C3.");
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

int32_t CheckedTopologyCount(const TopoDS_Shape& shape, const TopAbs_ShapeEnum kind, const bool unique)
{
  size_t count = 0;
  if (unique)
  {
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shapes;
    TopExp::MapShapes(shape, kind, shapes);
    count = static_cast<size_t>(shapes.Extent());
  }
  else
  {
    if (shape.ShapeType() == kind) ++count;
    for (TopExp_Explorer explorer(shape, kind); explorer.More(); explorer.Next()) ++count;
  }
  if (count > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology count exceeds the 32-bit ABI.");
  }
  return static_cast<int32_t>(count);
}

OcctSharp_TopologyCounts BuildTopologyCounts(const TopoDS_Shape& shape, const bool unique)
{
  return {
    CheckedTopologyCount(shape, TopAbs_VERTEX, unique),
    CheckedTopologyCount(shape, TopAbs_EDGE, unique),
    CheckedTopologyCount(shape, TopAbs_WIRE, unique),
    CheckedTopologyCount(shape, TopAbs_FACE, unique),
    CheckedTopologyCount(shape, TopAbs_SHELL, unique),
    CheckedTopologyCount(shape, TopAbs_SOLID, unique),
    CheckedTopologyCount(shape, TopAbs_COMPSOLID, unique),
    CheckedTopologyCount(shape, TopAbs_COMPOUND, unique)
  };
}

void BuildToleranceRange(
  const TopoDS_Shape& shape,
  const TopAbs_ShapeEnum kind,
  double& minimum,
  double& maximum)
{
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shapes;
  TopExp::MapShapes(shape, kind, shapes);
  minimum = 0.0;
  maximum = 0.0;
  if (shapes.IsEmpty()) return;

  minimum = std::numeric_limits<double>::infinity();
  for (int32_t index = 1; index <= shapes.Extent(); ++index)
  {
    const TopoDS_Shape& item = shapes(index);
    double tolerance = 0.0;
    switch (kind)
    {
      case TopAbs_VERTEX: tolerance = BRep_Tool::Tolerance(TopoDS::Vertex(item)); break;
      case TopAbs_EDGE: tolerance = BRep_Tool::Tolerance(TopoDS::Edge(item)); break;
      case TopAbs_FACE: tolerance = BRep_Tool::Tolerance(TopoDS::Face(item)); break;
      default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance is available only for vertices, edges, and faces.");
    }
    minimum = std::min(minimum, tolerance);
    maximum = std::max(maximum, tolerance);
  }
}

bool IsTopologyClosed(const TopoDS_Shape& shape)
{
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shells;
  TopExp::MapShapes(shape, TopAbs_SHELL, shells);
  if (!shells.IsEmpty())
  {
    for (int32_t index = 1; index <= shells.Extent(); ++index)
      if (!BRep_Tool::IsClosed(shells(index))) return false;
    return true;
  }
  return BRep_Tool::IsClosed(shape);
}

struct ValidationData
{
  bool IsValid = false;
  std::vector<OcctSharp_ValidationIssue> Issues;
};

ValidationData BuildValidationData(
  const OcctSharp_ShapeHandle* shape,
  const bool geometryChecks,
  const bool exact)
{
  ValidateUsableShape(shape);
  BRepCheck_Analyzer analyzer(shape->Value, geometryChecks, false, exact);
  ValidationData data;
  data.IsValid = analyzer.IsValid();
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> subshapes;
  TopExp::MapShapes(shape->Value, subshapes);
  for (int32_t index = 1; index <= subshapes.Extent(); ++index)
  {
    const TopoDS_Shape& subshape = subshapes(index);
    const opencascade::handle<BRepCheck_Result>& result = analyzer.Result(subshape);
    if (result.IsNull()) continue;
    const NCollection_List<BRepCheck_Status>& statuses = result->Status();
    for (NCollection_List<BRepCheck_Status>::Iterator iterator(statuses); iterator.More(); iterator.Next())
    {
      const BRepCheck_Status status = iterator.Value();
      if (status == BRepCheck_NoError) continue;
      if (data.Issues.size() == static_cast<size_t>(std::numeric_limits<int32_t>::max()))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The validation issue count exceeds the 32-bit ABI.");
      data.Issues.push_back({
        static_cast<int32_t>(subshape.ShapeType()),
        static_cast<int32_t>(status) });
    }
  }
  return data;
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

struct DetailedMeshData
{
  std::vector<OcctSharp_DetailedMeshVertex> Vertices;
  std::vector<OcctSharp_DetailedMeshTriangle> Triangles;
  int32_t FaceCount = 0;
};

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
  XCAFDoc_DocumentTool::ClippingPlaneTool(main);
}

void ConfigureXdeReader(
  STEPCAFControl_Reader& reader,
  const bool read_names = true,
  const bool read_colors = true,
  const bool read_layers = true,
  const bool read_validation_properties = true,
  const bool read_materials = true,
  const bool read_gdt = true,
  const bool read_views = true)
{
  reader.SetColorMode(read_colors);
  reader.SetNameMode(read_names);
  reader.SetLayerMode(read_layers);
  reader.SetPropsMode(read_validation_properties);
  reader.SetMetaMode(true);
  reader.SetProductMetaMode(true);
  reader.SetSHUOMode(true);
  reader.SetGDTMode(read_gdt);
  reader.SetMatMode(read_materials);
  reader.SetViewMode(read_views);
}

void ConfigureXdeWriter(
  STEPCAFControl_Writer& writer,
  const bool write_names = true,
  const bool write_colors = true,
  const bool write_layers = true,
  const bool write_validation_properties = true,
  const bool write_materials = true,
  const bool write_gdt = true)
{
  writer.SetColorMode(write_colors);
  writer.SetNameMode(write_names);
  writer.SetLayerMode(write_layers);
  writer.SetPropsMode(write_validation_properties);
  writer.SetMetadataMode(true);
  writer.SetSHUOMode(true);
  writer.SetDimTolMode(write_gdt);
  writer.SetMaterialMode(write_materials);
  writer.SetVisualMaterialMode(true);
}

std::vector<TDF_Label> ImportStepRootsIntoXdeDocument(
  const char* file_path,
  const occ::handle<TDocStd_Document>& output_document)
{
  ValidatePath(file_path);
  occ::handle<TDocStd_Document> source_document = CreateXdeDocument();
  InitializeXdeTools(source_document);
  STEPCAFControl_Reader reader;
  ConfigureXdeReader(reader);
  if (reader.ReadFile(file_path) != IFSelect_RetDone)
    throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read a STEP input through STEPCAF.");
  if (!reader.Transfer(source_document))
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "A STEP input could not be transferred into an XDE document.");

  occ::handle<XCAFDoc_ShapeTool> source_shape_tool =
    XCAFDoc_DocumentTool::ShapeTool(source_document->Main());
  occ::handle<XCAFDoc_ShapeTool> output_shape_tool =
    XCAFDoc_DocumentTool::ShapeTool(output_document->Main());
  NCollection_Sequence<TDF_Label> source_roots;
  source_shape_tool->GetFreeShapes(source_roots);
  if (source_roots.IsEmpty())
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "A STEP input produced no free XDE shape roots.");

  std::vector<TDF_Label> imported_roots;
  imported_roots.reserve(static_cast<size_t>(source_roots.Size()));
  NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>, occ::handle<XCAFDoc_VisMaterial>>
    visual_material_map;
  for (NCollection_Sequence<TDF_Label>::Iterator root_iterator(source_roots); root_iterator.More();
       root_iterator.Next())
  {
    NCollection_DataMap<TDF_Label, TDF_Label> label_map;
    TDF_Label cloned_root = XCAFDoc_Editor::CloneShapeLabel(
      root_iterator.Value(), source_shape_tool, output_shape_tool, label_map);
    if (cloned_root.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "An XDE shape tree could not be cloned into the destination document.");

    for (NCollection_DataMap<TDF_Label, TDF_Label>::Iterator label_iterator(label_map);
         label_iterator.More(); label_iterator.Next())
    {
      occ::handle<TDataStd_TreeNode> material_reference;
      const bool has_material_reference =
        label_iterator.Key().FindAttribute(XCAFDoc::MaterialRefGUID(), material_reference)
        && material_reference->HasFather();
      XCAFDoc_Editor::CloneMetaData(
        label_iterator.Key(), label_iterator.Value(), &visual_material_map,
        true, true, true, true, true);
      if (has_material_reference && label_iterator.Value() != cloned_root)
      {
        XCAFDoc_Editor::CloneMetaData(
          label_iterator.Key(), cloned_root, &visual_material_map,
          false, false, true, false, false);
      }
    }
    imported_roots.push_back(cloned_root);
  }
  return imported_roots;
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate(
  const OcctSharp_ShapeHandle* edge, const double parameter, OcctSharp_CurveEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The curve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve evaluation requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (parameter < first || parameter > last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve parameter is outside the edge range.");
    gp_Pnt point;
    gp_Vec derivative;
    curve.D1(parameter, point, derivative);
    if (derivative.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no defined tangent at the requested parameter.");
    const gp_Dir tangent(derivative);
    *out_result = { parameter,
      { point.X(), point.Y(), point.Z() },
      { tangent.X(), tangent.Y(), tangent.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate_derivatives(
  const OcctSharp_ShapeHandle* edge, const double parameter,
  OcctSharp_CurveDerivativeEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve derivative output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The curve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve derivative evaluation requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    if (parameter < curve.FirstParameter() || parameter > curve.LastParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve parameter is outside the edge range.");
    gp_Pnt point;
    gp_Vec first_derivative;
    gp_Vec second_derivative;
    curve.D2(parameter, point, first_derivative, second_derivative);
    *out_result = { parameter,
      { point.X(), point.Y(), point.Z() },
      { first_derivative.X(), first_derivative.Y(), first_derivative.Z() },
      { second_derivative.X(), second_derivative.Y(), second_derivative.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_snapshot(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_PcurveSnapshot* out_result)
{
  if (out_result == nullptr) { SetLastError("The pcurve snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateUsableShape(edge);
    ValidateUsableShape(face);
    if (edge->Value.ShapeType() != TopAbs_EDGE || face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A pcurve snapshot requires an edge and a face.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom2d_Curve> curve = BRep_Tool::CurveOnSurface(
      TopoDS::Edge(edge->Value), TopoDS::Face(face->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no pcurve on the supplied face.");
    const gp_Pnt2d start = curve->Value(first);
    const gp_Pnt2d end = curve->Value(last);
    *out_result = { first, last, { start.X(), start.Y() }, { end.X(), end.Y() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_evaluate(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, const double parameter,
  OcctSharp_PcurveEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The pcurve evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The pcurve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    ValidateUsableShape(face);
    if (edge->Value.ShapeType() != TopAbs_EDGE || face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pcurve evaluation requires an edge and a face.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom2d_Curve> curve = BRep_Tool::CurveOnSurface(
      TopoDS::Edge(edge->Value), TopoDS::Face(face->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no pcurve on the supplied face.");
    if (parameter < first || parameter > last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The pcurve parameter is outside the edge-on-face range.");
    gp_Pnt2d point;
    gp_Vec2d derivative;
    curve->D1(parameter, point, derivative);
    if (derivative.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The pcurve has no defined tangent at the requested parameter.");
    derivative.Normalize();
    *out_result = { parameter, { point.X(), point.Y() }, { derivative.X(), derivative.Y() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_length(
  const OcctSharp_ShapeHandle* edge, double* out_length)
{
  if (out_length == nullptr) { SetLastError("The edge length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_length = 0.0;
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve length requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    *out_length = GCPnts_AbscissaPoint::Length(curve, curve.FirstParameter(), curve.LastParameter());
    if (!std::isfinite(*out_length) || *out_length < 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned an invalid edge length.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_project_point(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point, OcctSharp_CurveProjection* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve projection output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateFinite(point.x, "Projection point X must be finite.");
    ValidateFinite(point.y, "Projection point Y must be finite.");
    ValidateFinite(point.z, "Projection point Z must be finite.");
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve projection requires an edge shape.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no 3D curve to project onto.");
    GeomAPI_ProjectPointOnCurve projection(gp_Pnt(point.x, point.y, point.z), curve, first, last);
    const int count = projection.NbPoints();
    if (count <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT found no point projection on the edge curve.");
    const gp_Pnt nearest = projection.NearestPoint();
    *out_result = { projection.LowerDistanceParameter(),
      { nearest.X(), nearest.Y(), nearest.Z() }, projection.LowerDistance(), count };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate(
  const OcctSharp_ShapeHandle* face, const double u_parameter, const double v_parameter,
  OcctSharp_SurfaceEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(u_parameter) || !std::isfinite(v_parameter))
  { SetLastError("Surface parameters must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface evaluation requires a face shape.");
    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), true);
    if (u_parameter < surface.FirstUParameter() || u_parameter > surface.LastUParameter()
        || v_parameter < surface.FirstVParameter() || v_parameter > surface.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UV parameters are outside the face range.");
    gp_Pnt point;
    gp_Vec derivative_u;
    gp_Vec derivative_v;
    surface.D1(u_parameter, v_parameter, point, derivative_u, derivative_v);
    gp_Vec normal = derivative_u.Crossed(derivative_v);
    if (normal.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no defined normal at the requested parameters.");
    normal.Normalize();
    if (face->Value.Orientation() == TopAbs_REVERSED) normal.Reverse();
    *out_result = { u_parameter, v_parameter,
      { point.X(), point.Y(), point.Z() }, { normal.X(), normal.Y(), normal.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate_derivatives(
  const OcctSharp_ShapeHandle* face, const double u_parameter, const double v_parameter,
  OcctSharp_SurfaceDerivativeEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface derivative output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(u_parameter) || !std::isfinite(v_parameter))
  { SetLastError("Surface parameters must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface derivative evaluation requires a face shape.");
    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), true);
    if (u_parameter < surface.FirstUParameter() || u_parameter > surface.LastUParameter()
        || v_parameter < surface.FirstVParameter() || v_parameter > surface.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UV parameters are outside the face range.");
    gp_Pnt point;
    gp_Vec derivative_u;
    gp_Vec derivative_v;
    surface.D1(u_parameter, v_parameter, point, derivative_u, derivative_v);
    gp_Vec normal = derivative_u.Crossed(derivative_v);
    if (normal.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no defined normal at the requested parameters.");
    normal.Normalize();
    if (face->Value.Orientation() == TopAbs_REVERSED) normal.Reverse();
    *out_result = { u_parameter, v_parameter,
      { point.X(), point.Y(), point.Z() },
      { derivative_u.X(), derivative_u.Y(), derivative_u.Z() },
      { derivative_v.X(), derivative_v.Y(), derivative_v.Z() },
      { normal.X(), normal.Y(), normal.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_project_point(
  const OcctSharp_ShapeHandle* face, const OcctSharp_Xyz point, const double tolerance,
  OcctSharp_SurfaceProjection* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface projection output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Surface projection tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(point.x, "Projection point X must be finite.");
    ValidateFinite(point.y, "Projection point Y must be finite.");
    ValidateFinite(point.z, "Projection point Z must be finite.");
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface projection requires a face shape.");
    const TopoDS_Face topology_face = TopoDS::Face(face->Value);
    const BRepAdaptor_Surface bounds(topology_face, true);
    const opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(topology_face);
    if (surface.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no surface to project onto.");
    GeomAPI_ProjectPointOnSurf projection(
      gp_Pnt(point.x, point.y, point.z), surface,
      bounds.FirstUParameter(), bounds.LastUParameter(),
      bounds.FirstVParameter(), bounds.LastVParameter(), tolerance);
    const int count = projection.NbPoints();
    if (!projection.IsDone() || count <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT found no point projection on the face surface.");
    double u = 0.0;
    double v = 0.0;
    projection.LowerDistanceParameters(u, v);
    const gp_Pnt nearest = projection.NearestPoint();
    *out_result = { u, v, { nearest.X(), nearest.Y(), nearest.Z() }, projection.LowerDistance(), count };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_trim(
  const OcctSharp_ShapeHandle* edge, const double first_parameter, const double last_parameter,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The trimmed edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(first_parameter) || !std::isfinite(last_parameter) || first_parameter >= last_parameter)
  { SetLastError("Trimmed edge parameters must be finite and strictly increasing."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge trimming requires an edge shape.");
    double source_first = 0.0;
    double source_last = 0.0;
    const opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), source_first, source_last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no 3D curve to trim.");
    if (first_parameter < source_first || last_parameter > source_last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested trim interval is outside the edge range.");
    const opencascade::handle<Geom_TrimmedCurve> trimmed = new Geom_TrimmedCurve(curve, first_parameter, last_parameter);
    BRepBuilderAPI_MakeEdge builder(trimmed);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the trimmed edge.");
    TopoDS_Edge result = builder.Edge();
    if (edge->Value.Orientation() == TopAbs_REVERSED) result.Reverse();
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_trim(
  const OcctSharp_ShapeHandle* face,
  const double first_u_parameter, const double last_u_parameter,
  const double first_v_parameter, const double last_v_parameter,
  const double tolerance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The trimmed face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(first_u_parameter) || !std::isfinite(last_u_parameter)
      || !std::isfinite(first_v_parameter) || !std::isfinite(last_v_parameter)
      || first_u_parameter >= last_u_parameter || first_v_parameter >= last_v_parameter
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Trimmed face bounds must be finite and increasing, and tolerance must be positive."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face trimming requires a face shape.");
    const TopoDS_Face topology_face = TopoDS::Face(face->Value);
    const BRepAdaptor_Surface bounds(topology_face, true);
    if (first_u_parameter < bounds.FirstUParameter() || last_u_parameter > bounds.LastUParameter()
        || first_v_parameter < bounds.FirstVParameter() || last_v_parameter > bounds.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested UV trim rectangle is outside the face range.");
    const opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(topology_face);
    if (surface.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no surface to trim.");
    const opencascade::handle<Geom_RectangularTrimmedSurface> trimmed =
      new Geom_RectangularTrimmedSurface(
        surface, first_u_parameter, last_u_parameter, first_v_parameter, last_v_parameter, true, true);
    BRepBuilderAPI_MakeFace builder(trimmed, tolerance);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the trimmed face.");
    TopoDS_Face result = builder.Face();
    if (face->Value.Orientation() == TopAbs_REVERSED) result.Reverse();
    *out_shape = AllocateShape(std::move(result));
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wedge(
  const double size_x, const double size_y, const double size_z, const double top_x_length,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wedge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(size_x) || !std::isfinite(size_y) || !std::isfinite(size_z)
      || !std::isfinite(top_x_length) || size_x <= 0.0 || size_y <= 0.0 || size_z <= 0.0
      || top_x_length < 0.0)
  {
    SetLastError("Wedge dimensions must be finite and greater than zero, and the top X length must be finite and non-negative.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeWedge builder(size_x, size_y, size_z, top_x_length);
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT wedge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_circle_edge(
  const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const double radius,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The circle edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0)
  { SetLastError("The circle radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(center.x, "Circle center X must be finite."); ValidateFinite(center.y, "Circle center Y must be finite."); ValidateFinite(center.z, "Circle center Z must be finite.");
    ValidateFinite(normal.x, "Circle normal X must be finite."); ValidateFinite(normal.y, "Circle normal Y must be finite."); ValidateFinite(normal.z, "Circle normal Z must be finite.");
    BRepBuilderAPI_MakeEdge builder(gp_Circ(gp_Ax2(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z)), radius));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT circle edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_arc_edge(
  const OcctSharp_Xyz start, const OcctSharp_Xyz middle, const OcctSharp_Xyz end,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The arc edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFinite(start.x, "Arc start X must be finite."); ValidateFinite(start.y, "Arc start Y must be finite."); ValidateFinite(start.z, "Arc start Z must be finite.");
    ValidateFinite(middle.x, "Arc middle X must be finite."); ValidateFinite(middle.y, "Arc middle Y must be finite."); ValidateFinite(middle.z, "Arc middle Z must be finite.");
    ValidateFinite(end.x, "Arc end X must be finite."); ValidateFinite(end.y, "Arc end Y must be finite."); ValidateFinite(end.z, "Arc end Z must be finite.");
    GC_MakeArcOfCircle arc(gp_Pnt(start.x, start.y, start.z), gp_Pnt(middle.x, middle.y, middle.z), gp_Pnt(end.x, end.y, end.z));
    if (!arc.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT circular arc construction did not complete.");
    BRepBuilderAPI_MakeEdge builder(arc.Value());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT arc edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_ellipse_edge(
  const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction,
  const double major_radius, const double minor_radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The ellipse edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(major_radius) || !std::isfinite(minor_radius)
      || major_radius <= 0.0 || minor_radius <= 0.0 || major_radius < minor_radius)
  { SetLastError("Ellipse radii must be finite and positive, with major radius greater than or equal to minor radius."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(center.x, "Ellipse center X must be finite."); ValidateFinite(center.y, "Ellipse center Y must be finite."); ValidateFinite(center.z, "Ellipse center Z must be finite.");
    ValidateFinite(normal.x, "Ellipse normal X must be finite."); ValidateFinite(normal.y, "Ellipse normal Y must be finite."); ValidateFinite(normal.z, "Ellipse normal Z must be finite.");
    ValidateFinite(x_direction.x, "Ellipse X direction X must be finite."); ValidateFinite(x_direction.y, "Ellipse X direction Y must be finite."); ValidateFinite(x_direction.z, "Ellipse X direction Z must be finite.");
    const gp_Ax2 axis(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z), gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    BRepBuilderAPI_MakeEdge builder(gp_Elips(axis, major_radius, minor_radius));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT ellipse edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_bezier_edge(
  const OcctSharp_Xyz* poles, const int32_t count, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The Bezier edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (poles == nullptr || count < 2) { SetLastError("A Bezier edge requires at least two poles."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_poles(1, count);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(poles[index].x, "Bezier pole X must be finite."); ValidateFinite(poles[index].y, "Bezier pole Y must be finite."); ValidateFinite(poles[index].z, "Bezier pole Z must be finite.");
      native_poles.SetValue(index + 1, gp_Pnt(poles[index].x, poles[index].y, poles[index].z));
    }
    const opencascade::handle<Geom_BezierCurve> curve = new Geom_BezierCurve(native_poles);
    BRepBuilderAPI_MakeEdge builder(curve);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT Bezier edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_interpolated_edge(
  const OcctSharp_Xyz* points, const int32_t count, const int32_t periodic, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The interpolated edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (points == nullptr || count < 2 || (periodic != 0 && count < 3))
  { SetLastError("Interpolation requires at least two points, or three for a periodic curve."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if ((periodic != 0 && periodic != 1) || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("The periodic flag must be zero or one and tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points =
      new NCollection_HArray1<gp_Pnt>(1, count);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(points[index].x, "Interpolation point X must be finite."); ValidateFinite(points[index].y, "Interpolation point Y must be finite."); ValidateFinite(points[index].z, "Interpolation point Z must be finite.");
      native_points->SetValue(index + 1, gp_Pnt(points[index].x, points[index].y, points[index].z));
    }
    GeomAPI_Interpolate interpolation(native_points, periodic != 0, tolerance);
    interpolation.Perform();
    if (!interpolation.IsDone() || interpolation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve interpolation did not complete.");
    BRepBuilderAPI_MakeEdge builder(interpolation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT interpolated edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_loft(
  const OcctSharp_ShapeHandle* const* sections, const int32_t count,
  const int32_t make_solid, const int32_t ruled, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The loft output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (sections == nullptr || count < 2)
  { SetLastError("A loft requires at least two wire or endpoint-vertex sections."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if ((make_solid != 0 && make_solid != 1) || (ruled != 0 && ruled != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Loft flags must be zero or one and tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepOffsetAPI_ThruSections builder(make_solid != 0, ruled != 0, tolerance);
    builder.CheckCompatibility(true);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(sections[index]);
      const TopAbs_ShapeEnum kind = sections[index]->Value.ShapeType();
      if (kind == TopAbs_WIRE) builder.AddWire(TopoDS::Wire(sections[index]->Value));
      else if (kind == TopAbs_VERTEX && (index == 0 || index == count - 1))
        builder.AddVertex(TopoDS::Vertex(sections[index]->Value));
      else
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Loft sections must be wires; only the first or last section may be a vertex.");
    }
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT loft construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_pipe(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* profile,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The pipe output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(spine);
    ValidateUsableShape(profile);
    if (spine->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pipe construction requires a wire spine.");
    BRepOffsetAPI_MakePipe builder(TopoDS::Wire(spine->Value), profile->Value);
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT pipe construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_sew(
  const OcctSharp_ShapeHandle* const* shapes, const int32_t count, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The sewing output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (shapes == nullptr || count < 1)
  { SetLastError("Sewing requires at least one topology shape."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (!std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Sewing tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_Sewing builder(tolerance, true, true, true, false);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(shapes[index]);
      builder.Add(shapes[index]->Value);
    }
    builder.Perform();
    TopoDS_Shape result = builder.SewedShape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT sewing produced a null result.");
    *out_shape = AllocateShape(std::move(result));
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wire(
  const OcctSharp_ShapeHandle* const* edges, const int32_t count,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wire output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (count <= 0 || edges == nullptr)
  { SetLastError("Wire construction requires at least one edge handle."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_MakeWire builder;
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(edges[index]);
      if (edges[index]->Value.ShapeType() != TopAbs_EDGE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Wire construction accepts edge shapes only.");
      builder.Add(TopoDS::Edge(edges[index]->Value));
    }
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not connect the supplied edges into a wire.");
    *out_shape = AllocateShape(builder.Wire());
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_create(
  const int32_t kind, const OcctSharp_Xyz* poles, const double* weights, const int32_t pole_count,
  const double* knots, const int32_t* multiplicities, const int32_t knot_count,
  const int32_t degree, const int32_t periodic, OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The curve output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (poles == nullptr || pole_count < 2 || (periodic != 0 && periodic != 1))
  { SetLastError("A freeform curve requires at least two poles and a Boolean periodic flag."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_poles(1, pole_count);
    NCollection_Array1<double> native_weights(1, pole_count);
    for (int32_t index = 0; index < pole_count; ++index)
    {
      native_poles.SetValue(index + 1, ToPoint(poles[index], "Curve poles must be finite."));
      const double weight = weights == nullptr ? 1.0 : weights[index];
      if (!std::isfinite(weight) || weight <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve weights must be finite and greater than zero.");
      native_weights.SetValue(index + 1, weight);
    }

    opencascade::handle<Geom_Curve> curve;
    if (kind == 1)
    {
      if (pole_count > Geom_BezierCurve::MaxDegree() + 1)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The Bezier pole count exceeds the OCCT maximum degree.");
      curve = weights == nullptr
        ? opencascade::handle<Geom_Curve>(new Geom_BezierCurve(native_poles))
        : opencascade::handle<Geom_Curve>(new Geom_BezierCurve(native_poles, native_weights));
    }
    else if (kind == 2)
    {
      if (knots == nullptr || multiplicities == nullptr || knot_count < 2 || degree < 1 || degree > Geom_BSplineCurve::MaxDegree())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A B-spline curve requires degree and matching knot/multiplicity arrays.");
      NCollection_Array1<double> native_knots(1, knot_count);
      NCollection_Array1<int> native_multiplicities(1, knot_count);
      for (int32_t index = 0; index < knot_count; ++index)
      {
        if (!std::isfinite(knots[index]) || (index > 0 && knots[index] <= knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "B-spline knots must be finite and strictly increasing.");
        native_knots.SetValue(index + 1, knots[index]);
        native_multiplicities.SetValue(index + 1, multiplicities[index]);
      }
      curve = weights == nullptr
        ? opencascade::handle<Geom_Curve>(new Geom_BSplineCurve(native_poles, native_knots, native_multiplicities, degree, periodic != 0))
        : opencascade::handle<Geom_Curve>(new Geom_BSplineCurve(native_poles, native_weights, native_knots, native_multiplicities, degree, periodic != 0));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve kind must be Bezier or B-spline.");

    BRepBuilderAPI_MakeEdge builder(curve);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform edge construction did not complete.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_interpolate(
  const OcctSharp_Xyz* points, const int32_t point_count, const OcctSharp_Xyz* endpoint_tangents,
  const int32_t tangent_count, const int32_t periodic, const double tolerance,
  OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The interpolation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (points == nullptr || point_count < (periodic == 0 ? 2 : 3) || (periodic != 0 && periodic != 1)
      || (tangent_count != 0 && tangent_count != 2) || (tangent_count == 2 && endpoint_tangents == nullptr)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Interpolation points, endpoint tangents, periodic flag, or tolerance are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points = new NCollection_HArray1<gp_Pnt>(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      native_points->SetValue(index + 1, ToPoint(points[index], "Interpolation points must be finite."));
    GeomAPI_Interpolate interpolation(native_points, periodic != 0, tolerance);
    if (tangent_count == 2)
      interpolation.Load(ToVector(endpoint_tangents[0], "The initial tangent must be finite and non-zero."),
                         ToVector(endpoint_tangents[1], "The final tangent must be finite and non-zero."), true);
    interpolation.Perform();
    if (!interpolation.IsDone() || interpolation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve interpolation did not complete.");
    BRepBuilderAPI_MakeEdge builder(interpolation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the interpolated edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_approximate(
  const OcctSharp_Xyz* points, const int32_t point_count, const int32_t minimum_degree,
  const int32_t maximum_degree, const int32_t continuity, const double tolerance,
  OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The approximation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (points == nullptr || point_count < 2 || minimum_degree < 1 || maximum_degree < minimum_degree
      || maximum_degree > Geom_BSplineCurve::MaxDegree() || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Curve approximation arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_points(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      native_points.SetValue(index + 1, ToPoint(points[index], "Approximation points must be finite."));
    GeomAPI_PointsToBSpline approximation(native_points, minimum_degree, maximum_degree, ToContinuity(continuity), tolerance);
    if (!approximation.IsDone() || approximation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve approximation did not complete.");
    BRepBuilderAPI_MakeEdge builder(approximation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the approximated edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_info(
  const OcctSharp_ShapeHandle* edge, OcctSharp_FreeformCurveInfo* out_info)
{
  if (out_info == nullptr) { SetLastError("The curve-info output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(curve);
    if (!trimmed.IsNull()) curve = trimmed->BasisCurve();
    double first = 0.0, last = 0.0;
    BRep_Tool::Range(TopoDS::Edge(edge->Value), first, last);
    const opencascade::handle<Geom_BezierCurve> bezier = opencascade::handle<Geom_BezierCurve>::DownCast(curve);
    const opencascade::handle<Geom_BSplineCurve> bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(curve);
    if (!bezier.IsNull()) *out_info = {1, bezier->Degree(), 0, bezier->IsRational() ? 1 : 0, bezier->NbPoles(), 0, first, last};
    else if (!bspline.IsNull()) *out_info = {2, bspline->Degree(), bspline->IsPeriodic() ? 1 : 0,
      bspline->IsRational() ? 1 : 0, bspline->NbPoles(), bspline->NbKnots(), first, last};
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The edge is not backed by a Bezier or B-spline curve.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_copy_definition(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz* poles, const int32_t pole_capacity,
  double* weights, const int32_t weight_capacity, double* knots, const int32_t knot_capacity,
  int32_t* multiplicities, const int32_t multiplicity_capacity)
{
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    const opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(curve);
    if (!trimmed.IsNull()) curve = trimmed->BasisCurve();
    const opencascade::handle<Geom_BezierCurve> bezier = opencascade::handle<Geom_BezierCurve>::DownCast(curve);
    const opencascade::handle<Geom_BSplineCurve> bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(curve);
    const int32_t pole_count = !bezier.IsNull() ? bezier->NbPoles() : !bspline.IsNull() ? bspline->NbPoles() : 0;
    const int32_t knot_count = !bspline.IsNull() ? bspline->NbKnots() : 0;
    if (pole_count == 0) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The edge is not backed by a Bezier or B-spline curve.");
    ValidateOutputCapacity(pole_capacity, pole_count, poles, "The curve pole buffer is too small.");
    ValidateOutputCapacity(weight_capacity, pole_count, weights, "The curve weight buffer is too small.");
    ValidateOutputCapacity(knot_capacity, knot_count, knots, "The curve knot buffer is too small.");
    ValidateOutputCapacity(multiplicity_capacity, knot_count, multiplicities, "The curve multiplicity buffer is too small.");
    for (int32_t index = 1; index <= pole_count; ++index)
    {
      poles[index - 1] = FromPoint(!bezier.IsNull() ? bezier->Pole(index) : bspline->Pole(index));
      weights[index - 1] = !bezier.IsNull() ? bezier->Weight(index) : bspline->Weight(index);
    }
    for (int32_t index = 1; index <= knot_count; ++index)
    {
      knots[index - 1] = bspline->Knot(index);
      multiplicities[index - 1] = bspline->Multiplicity(index);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_edit(
  const OcctSharp_ShapeHandle* edge, const int32_t operation, const int32_t degree,
  const double first_parameter, const double last_parameter, OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The edited-curve output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> source = GetEdgeCurve(edge);
    const opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(source);
    if (!trimmed.IsNull()) source = trimmed->BasisCurve();
    opencascade::handle<Geom_BezierCurve> bezier;
    opencascade::handle<Geom_BSplineCurve> bspline;
    const auto source_bezier = opencascade::handle<Geom_BezierCurve>::DownCast(source);
    const auto source_bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(source);
    if (!source_bezier.IsNull()) bezier = new Geom_BezierCurve(*source_bezier);
    else if (!source_bspline.IsNull()) bspline = new Geom_BSplineCurve(*source_bspline);
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve editing requires a Bezier or B-spline edge.");
    if (operation == 1)
    {
      if (degree < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The elevated degree must be positive.");
      if (!bezier.IsNull()) bezier->Increase(degree); else bspline->IncreaseDegree(degree);
    }
    else if (operation == 2) { if (!bezier.IsNull()) bezier->Reverse(); else bspline->Reverse(); }
    else if (operation == 3)
    {
      if (!std::isfinite(first_parameter) || !std::isfinite(last_parameter) || first_parameter >= last_parameter)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve segment bounds must be finite and increasing.");
      if (!bezier.IsNull()) bezier->Segment(first_parameter, last_parameter);
      else bspline->Segment(first_parameter, last_parameter);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve edit operation is unknown.");
    BRepBuilderAPI_MakeEdge builder(!bezier.IsNull() ? opencascade::handle<Geom_Curve>(bezier) : opencascade::handle<Geom_Curve>(bspline));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the edited curve edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_split(
  const OcctSharp_ShapeHandle* edge, const double* parameters, const int32_t parameter_count,
  OcctSharp_ShapeHandle** out_edges, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The split written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    double first = 0.0, last = 0.0;
    BRep_Tool::Range(TopoDS::Edge(edge->Value), first, last);
    if (parameter_count < 0 || (parameter_count > 0 && parameters == nullptr))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve split parameters are invalid.");
    ValidateOutputCapacity(capacity, parameter_count + 1, out_edges, "The curve split output buffer is too small.");
    double start = first;
    for (int32_t index = 0; index <= parameter_count; ++index)
    {
      const double end = index == parameter_count ? last : parameters[index];
      if (!std::isfinite(end) || end <= start || end >= last && index < parameter_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve split parameters must be strictly increasing inside the edge range.");
      const opencascade::handle<Geom_TrimmedCurve> segment = new Geom_TrimmedCurve(curve, start, end);
      BRepBuilderAPI_MakeEdge builder(segment);
      if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build a curve split segment.");
      out_edges[index] = AllocateShape(builder.Edge());
      ++*out_written;
      start = end;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_count(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The projection count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    GeomAPI_ProjectPointOnCurve projection(ToPoint(point, "The projection point must be finite."), GetEdgeCurve(edge));
    *out_count = projection.NbPoints();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_copy(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The projection written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_ProjectPointOnCurve projection(ToPoint(point, "The projection point must be finite."), GetEdgeCurve(edge));
    ValidateOutputCapacity(capacity, projection.NbPoints(), solutions, "The projection output buffer is too small.");
    for (int32_t index = 1; index <= projection.NbPoints(); ++index)
    {
      solutions[index - 1] = {FromPoint(projection.Point(index)), point, projection.Parameter(index), 0.0, 0.0, projection.Distance(index)};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The extrema count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { GeomAPI_ExtremaCurveCurve extrema(GetEdgeCurve(first), GetEdgeCurve(second)); *out_count = extrema.NbExtrema(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_copy(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The extrema written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_ExtremaCurveCurve extrema(GetEdgeCurve(first), GetEdgeCurve(second));
    ValidateOutputCapacity(capacity, extrema.NbExtrema(), solutions, "The extrema output buffer is too small.");
    for (int32_t index = 1; index <= extrema.NbExtrema(); ++index)
    {
      gp_Pnt first_point, second_point; double first_parameter = 0.0, second_parameter = 0.0;
      extrema.Points(index, first_point, second_point); extrema.Parameters(index, first_parameter, second_parameter);
      solutions[index - 1] = {FromPoint(first_point), FromPoint(second_point), first_parameter, second_parameter, 0.0, extrema.Distance(index)};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_count(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The intersection count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    GeomAPI_IntCS intersection(GetEdgeCurve(edge), GetFaceSurface(face));
    if (!intersection.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve-surface intersection did not complete.");
    *out_count = intersection.NbPoints();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_copy(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The intersection written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_IntCS intersection(GetEdgeCurve(edge), GetFaceSurface(face));
    if (!intersection.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve-surface intersection did not complete.");
    ValidateOutputCapacity(capacity, intersection.NbPoints(), solutions, "The intersection output buffer is too small.");
    for (int32_t index = 1; index <= intersection.NbPoints(); ++index)
    {
      double curve_parameter = 0.0, u = 0.0, v = 0.0;
      intersection.Parameters(index, curve_parameter, u, v);
      const gp_Pnt& point = intersection.Point(index);
      solutions[index - 1] = {FromPoint(point), FromPoint(point), curve_parameter, u, v, 0.0};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_profile(
  const OcctSharp_Xyz* points, const int32_t point_count, const OcctSharp_Xyz origin,
  const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction, const int32_t interpolate,
  const double tolerance, OcctSharp_ShapeHandle** out_wire)
{
  if (out_wire == nullptr) { SetLastError("The profile output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_wire = nullptr;
  if (points == nullptr || point_count < 3 || (interpolate != 0 && interpolate != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Planar-profile arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const gp_Ax2 plane(ToPoint(origin, "The profile origin must be finite."),
      gp_Dir(ToVector(normal, "The profile normal must be finite and non-zero.")),
      gp_Dir(ToVector(x_direction, "The profile X direction must be finite and non-zero.")));
    auto located = [&](const OcctSharp_Xyz& value)
    {
      if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z) || std::abs(value.z) > tolerance)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Located profile points must have finite XY and zero local Z within tolerance.");
      return plane.Location().Translated(gp_Vec(plane.XDirection()) * value.x + gp_Vec(plane.YDirection()) * value.y);
    };
    if (interpolate != 0)
    {
      const opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points = new NCollection_HArray1<gp_Pnt>(1, point_count);
      for (int32_t index = 0; index < point_count; ++index) native_points->SetValue(index + 1, located(points[index]));
      GeomAPI_Interpolate interpolation(native_points, true, tolerance); interpolation.Perform();
      if (!interpolation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar-profile interpolation did not complete.");
      BRepBuilderAPI_MakeEdge edge(interpolation.Curve()); BRepBuilderAPI_MakeWire wire(edge.Edge());
      if (!wire.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the interpolated profile wire.");
      *out_wire = AllocateShape(wire.Wire());
    }
    else
    {
      BRepBuilderAPI_MakePolygon polygon;
      for (int32_t index = 0; index < point_count; ++index) polygon.Add(located(points[index]));
      polygon.Close();
      if (!polygon.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the polygon profile wire.");
      *out_wire = AllocateShape(polygon.Wire());
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_offset(
  const OcctSharp_ShapeHandle* wire, const double distance, const double altitude,
  const int32_t join_type, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The planar-offset output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || !std::isfinite(altitude) || join_type < 0 || join_type > 2)
  { SetLastError("Planar-offset distance, altitude, or join type is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(wire);
    if (wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar offset requires a wire.");
    BRepOffsetAPI_MakeOffset builder(TopoDS::Wire(wire->Value), static_cast<GeomAbs_JoinType>(join_type), false);
    builder.Perform(distance, altitude);
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar-wire offset did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_create(
  const int32_t kind, const OcctSharp_Xyz* poles, const double* weights,
  const int32_t u_pole_count, const int32_t v_pole_count,
  const double* u_knots, const int32_t* u_multiplicities, const int32_t u_knot_count,
  const double* v_knots, const int32_t* v_multiplicities, const int32_t v_knot_count,
  const int32_t u_degree, const int32_t v_degree, const int32_t u_periodic, const int32_t v_periodic,
  const double* bounds, const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (poles == nullptr || u_pole_count < 2 || v_pole_count < 2
      || (u_periodic != 0 && u_periodic != 1) || (v_periodic != 0 && v_periodic != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Freeform-surface pole grid, periodic flags, or tolerance are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array2<gp_Pnt> native_poles(1, u_pole_count, 1, v_pole_count);
    NCollection_Array2<double> native_weights(1, u_pole_count, 1, v_pole_count);
    for (int32_t u = 0; u < u_pole_count; ++u)
      for (int32_t v = 0; v < v_pole_count; ++v)
      {
        const int32_t index = u * v_pole_count + v;
        native_poles.SetValue(u + 1, v + 1, ToPoint(poles[index], "Surface poles must be finite."));
        const double weight = weights == nullptr ? 1.0 : weights[index];
        if (!std::isfinite(weight) || weight <= 0.0)
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface weights must be finite and greater than zero.");
        native_weights.SetValue(u + 1, v + 1, weight);
      }

    opencascade::handle<Geom_Surface> surface;
    if (kind == 1)
    {
      if (u_pole_count > Geom_BezierSurface::MaxDegree() + 1 || v_pole_count > Geom_BezierSurface::MaxDegree() + 1)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The Bezier surface grid exceeds the OCCT maximum degree.");
      surface = weights == nullptr
        ? opencascade::handle<Geom_Surface>(new Geom_BezierSurface(native_poles))
        : opencascade::handle<Geom_Surface>(new Geom_BezierSurface(native_poles, native_weights));
    }
    else if (kind == 2)
    {
      if (u_knots == nullptr || v_knots == nullptr || u_multiplicities == nullptr || v_multiplicities == nullptr
          || u_knot_count < 2 || v_knot_count < 2 || u_degree < 1 || v_degree < 1
          || u_degree > Geom_BSplineSurface::MaxDegree() || v_degree > Geom_BSplineSurface::MaxDegree())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A B-spline surface requires valid U/V degree, knot, and multiplicity arrays.");
      NCollection_Array1<double> native_u_knots(1, u_knot_count), native_v_knots(1, v_knot_count);
      NCollection_Array1<int> native_u_multiplicities(1, u_knot_count), native_v_multiplicities(1, v_knot_count);
      for (int32_t index = 0; index < u_knot_count; ++index)
      {
        if (!std::isfinite(u_knots[index]) || (index > 0 && u_knots[index] <= u_knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface U knots must be finite and strictly increasing.");
        native_u_knots.SetValue(index + 1, u_knots[index]); native_u_multiplicities.SetValue(index + 1, u_multiplicities[index]);
      }
      for (int32_t index = 0; index < v_knot_count; ++index)
      {
        if (!std::isfinite(v_knots[index]) || (index > 0 && v_knots[index] <= v_knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface V knots must be finite and strictly increasing.");
        native_v_knots.SetValue(index + 1, v_knots[index]); native_v_multiplicities.SetValue(index + 1, v_multiplicities[index]);
      }
      surface = weights == nullptr
        ? opencascade::handle<Geom_Surface>(new Geom_BSplineSurface(native_poles, native_u_knots, native_v_knots,
            native_u_multiplicities, native_v_multiplicities, u_degree, v_degree, u_periodic != 0, v_periodic != 0))
        : opencascade::handle<Geom_Surface>(new Geom_BSplineSurface(native_poles, native_weights, native_u_knots, native_v_knots,
            native_u_multiplicities, native_v_multiplicities, u_degree, v_degree, u_periodic != 0, v_periodic != 0));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface kind must be Bezier or B-spline.");

    BRepBuilderAPI_MakeFace builder;
    if (bounds == nullptr) builder.Init(surface, true, tolerance);
    else
    {
      for (int index = 0; index < 4; ++index) ValidateFinite(bounds[index], "Surface trim bounds must be finite.");
      if (bounds[0] >= bounds[1] || bounds[2] >= bounds[3])
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface trim bounds must be increasing.");
      builder.Init(surface, bounds[0], bounds[1], bounds[2], bounds[3], tolerance);
    }
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform face construction did not complete.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_approximate(
  const OcctSharp_Xyz* points, const int32_t u_count, const int32_t v_count,
  const int32_t minimum_degree, const int32_t maximum_degree, const int32_t continuity,
  const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The approximated-surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (points == nullptr || u_count < 2 || v_count < 2 || minimum_degree < 1 || maximum_degree < minimum_degree
      || maximum_degree > Geom_BSplineSurface::MaxDegree() || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Surface approximation arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array2<gp_Pnt> native_points(1, u_count, 1, v_count);
    for (int32_t u = 0; u < u_count; ++u)
      for (int32_t v = 0; v < v_count; ++v)
        native_points.SetValue(u + 1, v + 1, ToPoint(points[u * v_count + v], "Surface approximation points must be finite."));
    GeomAPI_PointsToBSplineSurface approximation;
    if (continuity == -1) approximation.Interpolate(native_points);
    else approximation.Init(native_points, minimum_degree, maximum_degree, ToContinuity(continuity), tolerance);
    if (!approximation.IsDone() || approximation.Surface().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT surface approximation did not complete.");
    BRepBuilderAPI_MakeFace builder(approximation.Surface(), tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the approximated surface face.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_info(
  const OcctSharp_ShapeHandle* face, OcctSharp_FreeformSurfaceInfo* out_info)
{
  if (out_info == nullptr) { SetLastError("The surface-info output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> surface = GetFaceSurface(face);
    const opencascade::handle<Geom_RectangularTrimmedSurface> trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(surface);
    if (!trimmed.IsNull()) surface = trimmed->BasisSurface();
    double u1 = 0.0, u2 = 0.0, v1 = 0.0, v2 = 0.0; BRepTools::UVBounds(TopoDS::Face(face->Value), u1, u2, v1, v2);
    const auto bezier = opencascade::handle<Geom_BezierSurface>::DownCast(surface);
    const auto bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(surface);
    if (!bezier.IsNull()) *out_info = {1, bezier->UDegree(), bezier->VDegree(), 0, 0,
      (bezier->IsURational() || bezier->IsVRational()) ? 1 : 0, bezier->NbUPoles(), bezier->NbVPoles(), 0, 0, u1, u2, v1, v2};
    else if (!bspline.IsNull()) *out_info = {2, bspline->UDegree(), bspline->VDegree(), bspline->IsUPeriodic() ? 1 : 0,
      bspline->IsVPeriodic() ? 1 : 0, (bspline->IsURational() || bspline->IsVRational()) ? 1 : 0,
      bspline->NbUPoles(), bspline->NbVPoles(), bspline->NbUKnots(), bspline->NbVKnots(), u1, u2, v1, v2};
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The face is not backed by a Bezier or B-spline surface.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_copy_definition(
  const OcctSharp_ShapeHandle* face, OcctSharp_Xyz* poles, const int32_t pole_capacity,
  double* weights, const int32_t weight_capacity,
  double* u_knots, const int32_t u_knot_capacity, int32_t* u_multiplicities, const int32_t u_multiplicity_capacity,
  double* v_knots, const int32_t v_knot_capacity, int32_t* v_multiplicities, const int32_t v_multiplicity_capacity)
{
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> surface = GetFaceSurface(face);
    const auto trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(surface);
    if (!trimmed.IsNull()) surface = trimmed->BasisSurface();
    const auto bezier = opencascade::handle<Geom_BezierSurface>::DownCast(surface);
    const auto bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(surface);
    const int32_t u_count = !bezier.IsNull() ? bezier->NbUPoles() : !bspline.IsNull() ? bspline->NbUPoles() : 0;
    const int32_t v_count = !bezier.IsNull() ? bezier->NbVPoles() : !bspline.IsNull() ? bspline->NbVPoles() : 0;
    if (u_count == 0) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The face is not backed by a Bezier or B-spline surface.");
    const int32_t u_knot_count = !bspline.IsNull() ? bspline->NbUKnots() : 0;
    const int32_t v_knot_count = !bspline.IsNull() ? bspline->NbVKnots() : 0;
    ValidateOutputCapacity(pole_capacity, u_count * v_count, poles, "The surface pole buffer is too small.");
    ValidateOutputCapacity(weight_capacity, u_count * v_count, weights, "The surface weight buffer is too small.");
    ValidateOutputCapacity(u_knot_capacity, u_knot_count, u_knots, "The surface U-knot buffer is too small.");
    ValidateOutputCapacity(u_multiplicity_capacity, u_knot_count, u_multiplicities, "The surface U-multiplicity buffer is too small.");
    ValidateOutputCapacity(v_knot_capacity, v_knot_count, v_knots, "The surface V-knot buffer is too small.");
    ValidateOutputCapacity(v_multiplicity_capacity, v_knot_count, v_multiplicities, "The surface V-multiplicity buffer is too small.");
    for (int32_t u = 1; u <= u_count; ++u)
      for (int32_t v = 1; v <= v_count; ++v)
      {
        const int32_t index = (u - 1) * v_count + v - 1;
        poles[index] = FromPoint(!bezier.IsNull() ? bezier->Pole(u, v) : bspline->Pole(u, v));
        weights[index] = !bezier.IsNull() ? bezier->Weight(u, v) : bspline->Weight(u, v);
      }
    for (int32_t index = 1; index <= u_knot_count; ++index) { u_knots[index - 1] = bspline->UKnot(index); u_multiplicities[index - 1] = bspline->UMultiplicity(index); }
    for (int32_t index = 1; index <= v_knot_count; ++index) { v_knots[index - 1] = bspline->VKnot(index); v_multiplicities[index - 1] = bspline->VMultiplicity(index); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_edit(
  const OcctSharp_ShapeHandle* face, const int32_t operation, const int32_t u_degree, const int32_t v_degree,
  const double* bounds, const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The edited-surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (!std::isfinite(tolerance) || tolerance <= 0.0) { SetLastError("Surface edit tolerance is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> source = GetFaceSurface(face);
    const auto trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(source);
    if (!trimmed.IsNull()) source = trimmed->BasisSurface();
    const auto source_bezier = opencascade::handle<Geom_BezierSurface>::DownCast(source);
    const auto source_bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(source);
    opencascade::handle<Geom_BezierSurface> bezier;
    opencascade::handle<Geom_BSplineSurface> bspline;
    if (!source_bezier.IsNull()) bezier = new Geom_BezierSurface(*source_bezier);
    else if (!source_bspline.IsNull()) bspline = new Geom_BSplineSurface(*source_bspline);
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface editing requires a Bezier or B-spline face.");
    if (operation == 1) { if (!bezier.IsNull()) bezier->Increase(u_degree, v_degree); else bspline->IncreaseDegree(u_degree, v_degree); }
    else if (operation == 2) { if (!bezier.IsNull()) bezier->UReverse(); else bspline->UReverse(); }
    else if (operation == 3) { if (!bezier.IsNull()) bezier->VReverse(); else bspline->VReverse(); }
    else if (operation != 4) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The surface edit operation is unknown.");
    opencascade::handle<Geom_Surface> result = !bezier.IsNull() ? opencascade::handle<Geom_Surface>(bezier) : opencascade::handle<Geom_Surface>(bspline);
    BRepBuilderAPI_MakeFace builder;
    if (bounds != nullptr || operation == 4)
    {
      if (bounds == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface segmentation requires four bounds.");
      for (int index = 0; index < 4; ++index) ValidateFinite(bounds[index], "Surface segment bounds must be finite.");
      if (bounds[0] >= bounds[1] || bounds[2] >= bounds[3]) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface segment bounds must be increasing.");
      if (operation == 4) { if (!bezier.IsNull()) bezier->Segment(bounds[0], bounds[1], bounds[2], bounds[3]); else bspline->Segment(bounds[0], bounds[1], bounds[2], bounds[3]); }
      builder.Init(result, bounds[0], bounds[1], bounds[2], bounds[3], tolerance);
    }
    else builder.Init(result, true, tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the edited freeform face.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_ruled_face(
  const OcctSharp_ShapeHandle* first_edge, const OcctSharp_ShapeHandle* second_edge, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The ruled-face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(first_edge); ValidateUsableShape(second_edge);
    if (first_edge->Value.ShapeType() != TopAbs_EDGE || second_edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A ruled face requires two edges.");
    TopoDS_Face face = BRepFill::Face(TopoDS::Edge(first_edge->Value), TopoDS::Edge(second_edge->Value));
    if (face.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT ruled-face construction produced a null face.");
    *out_face = AllocateShape(face);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_fill(
  const OcctSharp_ShapeHandle* const* edges, const int32_t edge_count,
  const OcctSharp_Xyz* points, const int32_t point_count, const int32_t continuity,
  const double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr || out_diagnostics == nullptr) { SetLastError("The fill output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr; *out_diagnostics = {};
  if (edges == nullptr || edge_count < 2 || point_count < 0 || (point_count > 0 && points == nullptr)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Boundary-fill arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepFill_Filling fill(3, 15, 2, false, tolerance, tolerance, 0.01, 0.1, 8, 12);
    for (int32_t index = 0; index < edge_count; ++index)
    {
      ValidateUsableShape(edges[index]);
      if (edges[index]->Value.ShapeType() != TopAbs_EDGE) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Boundary fill accepts edge constraints only.");
      fill.Add(TopoDS::Edge(edges[index]->Value), ToContinuity(continuity), true);
    }
    for (int32_t index = 0; index < point_count; ++index) fill.Add(ToPoint(points[index], "Fill constraints must be finite."));
    fill.Build();
    if (!fill.IsDone() || fill.Face().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boundary filling did not complete.");
    const TopoDS_Face result = fill.Face();
    *out_diagnostics = {0, edge_count + point_count, 1, 0, 0, 0, BRepCheck_Analyzer(result).IsValid() ? 1 : 0,
      BRep_Tool::IsClosed(result) ? 1 : 0, fill.G0Error(), fill.G1Error(), fill.G2Error(), fill.G0Error()};
    *out_face = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_split(
  const OcctSharp_ShapeHandle* const* objects, const int32_t object_count,
  const OcctSharp_ShapeHandle* const* tools, const int32_t tool_count,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The split output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (objects == nullptr || object_count < 1 || tools == nullptr || tool_count < 1)
  { SetLastError("Topology splitting requires object and tool shapes."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_List<TopoDS_Shape> arguments, tool_shapes;
    for (int32_t index = 0; index < object_count; ++index) { ValidateUsableShape(objects[index]); arguments.Append(objects[index]->Value); }
    for (int32_t index = 0; index < tool_count; ++index) { ValidateUsableShape(tools[index]); tool_shapes.Append(tools[index]->Value); }
    BRepAlgoAPI_Splitter splitter; splitter.SetArguments(arguments); splitter.SetTools(tool_shapes); splitter.SetNonDestructive(true); splitter.Build();
    if (!splitter.IsDone() || splitter.Shape().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT topology splitting did not complete.");
    int32_t modified = 0, generated = 0, deleted = 0;
    for (int32_t index = 0; index < object_count; ++index)
    {
      modified += splitter.Modified(objects[index]->Value).Size(); generated += splitter.Generated(objects[index]->Value).Size();
      if (splitter.IsDeleted(objects[index]->Value)) ++deleted;
    }
    const TopoDS_Shape result = splitter.Shape();
    *out_diagnostics = {0, object_count + tool_count, CheckedTopologyCount(result, TopAbs_SOLID, false) + CheckedTopologyCount(result, TopAbs_FACE, false),
      modified, generated, deleted, BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_pipe_shell(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* const* profiles,
  const int32_t profile_count, const int32_t make_solid, const int32_t frenet, const int32_t transition_mode,
  const double tolerance, const int32_t maximum_degree, const int32_t maximum_segments,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The pipe-shell output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (profiles == nullptr || profile_count < 1 || (make_solid != 0 && make_solid != 1) || (frenet != 0 && frenet != 1)
      || transition_mode < 0 || transition_mode > 2 || !std::isfinite(tolerance) || tolerance <= 0.0
      || maximum_degree < 1 || maximum_segments < 1)
  { SetLastError("Pipe-shell options are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(spine);
    if (spine->Value.ShapeType() != TopAbs_WIRE) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pipe-shell construction requires a wire spine.");
    BRepOffsetAPI_MakePipeShell pipe(TopoDS::Wire(spine->Value)); pipe.SetMode(frenet != 0);
    pipe.SetTransitionMode(static_cast<BRepBuilderAPI_TransitionMode>(transition_mode));
    pipe.SetTolerance(tolerance, tolerance, 0.01); pipe.SetMaxDegree(maximum_degree); pipe.SetMaxSegments(maximum_segments); pipe.SetForceApproxC1(true);
    for (int32_t index = 0; index < profile_count; ++index) { ValidateUsableShape(profiles[index]); pipe.Add(profiles[index]->Value, false, true); }
    if (!pipe.IsReady()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The pipe-shell definition is not ready.");
    pipe.Build();
    if (!pipe.IsDone() || (make_solid != 0 && !pipe.MakeSolid()) || pipe.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT pipe-shell construction did not complete.");
    const TopoDS_Shape result = pipe.Shape();
    *out_diagnostics = {static_cast<int32_t>(pipe.GetStatus()), profile_count + 1, 1, 0, 0, 0,
      BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, pipe.ErrorOnSurface()};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_loft(
  const OcctSharp_ShapeHandle* const* sections, const int32_t section_count,
  const int32_t make_solid, const int32_t ruled, const int32_t smoothing, const int32_t continuity,
  const int32_t maximum_degree, const double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The loft output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (sections == nullptr || section_count < 2 || (make_solid != 0 && make_solid != 1) || (ruled != 0 && ruled != 1)
      || (smoothing != 0 && smoothing != 1) || maximum_degree < 1 || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Controlled-loft options are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepOffsetAPI_ThruSections loft(make_solid != 0, ruled != 0, tolerance); loft.CheckCompatibility(true);
    loft.SetSmoothing(smoothing != 0); loft.SetContinuity(ToContinuity(continuity)); loft.SetMaxDegree(maximum_degree); loft.SetMutableInput(false);
    for (int32_t index = 0; index < section_count; ++index)
    {
      ValidateUsableShape(sections[index]);
      if (sections[index]->Value.ShapeType() == TopAbs_WIRE) loft.AddWire(TopoDS::Wire(sections[index]->Value));
      else if (sections[index]->Value.ShapeType() == TopAbs_VERTEX && (index == 0 || index == section_count - 1)) loft.AddVertex(TopoDS::Vertex(sections[index]->Value));
      else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Loft sections must be wires, with optional endpoint vertices.");
    }
    loft.Build();
    if (!loft.IsDone() || loft.Shape().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT controlled loft did not complete.");
    const TopoDS_Shape result = loft.Shape();
    *out_diagnostics = {static_cast<int32_t>(loft.GetStatus()), section_count, 1, 0, 0, 0,
      BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_heal(
  const OcctSharp_ShapeHandle* shape, const double tolerance,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The heal output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (!std::isfinite(tolerance) || tolerance <= 0.0) { SetLastError("Healing tolerance is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); const bool before_valid = BRepCheck_Analyzer(shape->Value).IsValid(); TopoDS_Shape result;
    if (shape->Value.ShapeType() == TopAbs_FACE)
    {
      ShapeFix_Face fix(TopoDS::Face(shape->Value)); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Result();
    }
    else if (shape->Value.ShapeType() == TopAbs_SHELL)
    {
      ShapeFix_Shell fix(TopoDS::Shell(shape->Value)); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Shape();
    }
    else
    {
      ShapeFix_Shape fix(shape->Value); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Shape();
    }
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform healing produced a null result.");
    const bool after_valid = BRepCheck_Analyzer(result).IsValid();
    *out_diagnostics = {0, 1, 1, before_valid == after_valid ? 0 : 1, 0, 0, after_valid ? 1 : 0,
      BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_compute(
  const OcctSharp_ShapeHandle* const* shapes, const int32_t shape_count,
  const OcctSharp_DrawingProjection projection, const int32_t exact, const int32_t iso_count,
  const double deflection, OcctSharp_ShapeHandle** out_layers, const int32_t layer_capacity)
{
  if (shapes == nullptr || shape_count <= 0 || out_layers == nullptr || layer_capacity != 10)
  {
    SetLastError("Drawing computation requires at least one shape and exactly ten output layers.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  for (int32_t index = 0; index < layer_capacity; ++index) out_layers[index] = nullptr;
  if ((exact != 0 && exact != 1) || iso_count < 0 || iso_count > 100
      || !std::isfinite(deflection) || deflection <= 0.0)
  {
    SetLastError("Drawing exact flag, isoparameter count, or deflection is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    const HLRAlgo_Projector projector = MakeDrawingProjector(projection);
    std::vector<TopoDS_Shape> layers(10);
    if (exact != 0)
    {
      opencascade::handle<HLRBRep_Algo> algorithm = new HLRBRep_Algo();
      for (int32_t index = 0; index < shape_count; ++index)
      {
        ValidateUsableShape(shapes[index]);
        algorithm->Add(shapes[index]->Value, iso_count);
      }
      algorithm->Projector(projector);
      algorithm->Update();
      algorithm->Hide();
      HLRBRep_HLRToShape extraction(algorithm);
      layers[0] = extraction.VCompound();
      layers[1] = extraction.HCompound();
      layers[2] = extraction.Rg1LineVCompound();
      layers[3] = extraction.Rg1LineHCompound();
      layers[4] = extraction.RgNLineVCompound();
      layers[5] = extraction.RgNLineHCompound();
      layers[6] = extraction.OutLineVCompound();
      layers[7] = extraction.OutLineHCompound();
      layers[8] = extraction.IsoLineVCompound();
      layers[9] = extraction.IsoLineHCompound();
    }
    else
    {
      opencascade::handle<HLRBRep_PolyAlgo> algorithm = new HLRBRep_PolyAlgo();
      for (int32_t index = 0; index < shape_count; ++index)
      {
        ValidateUsableShape(shapes[index]);
        BRepMesh_IncrementalMesh mesh(shapes[index]->Value, deflection, false, 0.5, true);
        algorithm->Load(shapes[index]->Value);
      }
      algorithm->Projector(projector);
      algorithm->Update();
      HLRBRep_PolyHLRToShape extraction;
      extraction.Update(algorithm);
      layers[0] = extraction.VCompound();
      layers[1] = extraction.HCompound();
      layers[2] = extraction.Rg1LineVCompound();
      layers[3] = extraction.Rg1LineHCompound();
      layers[4] = extraction.RgNLineVCompound();
      layers[5] = extraction.RgNLineHCompound();
      layers[6] = extraction.OutLineVCompound();
      layers[7] = extraction.OutLineHCompound();
    }

    try
    {
      for (int32_t index = 0; index < layer_capacity; ++index)
        out_layers[index] = AllocateShape(NonNullDrawingLayer(std::move(layers[index])));
    }
    catch (...)
    {
      for (int32_t index = 0; index < layer_capacity; ++index)
      {
        if (out_layers[index] == nullptr) continue;
        UnregisterShape(out_layers[index]);
        delete out_layers[index];
        out_layers[index] = nullptr;
      }
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_section(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_Xyz plane_origin,
  const OcctSharp_Xyz plane_normal, const int32_t approximate, OcctSharp_ShapeHandle** out_section)
{
  if (out_section == nullptr)
  {
    SetLastError("The drawing section output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_section = nullptr;
  if (approximate != 0 && approximate != 1)
  {
    SetLastError("The drawing section approximation flag is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const gp_Pln plane(ToPoint(plane_origin, "Section plane origin must be finite."),
                       gp_Dir(ToVector(plane_normal, "Section plane normal must be finite and non-zero.")));
    BRepAlgoAPI_Section section(shape->Value, plane, false);
    section.Approximation(approximate != 0);
    section.Build();
    if (!section.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section computation did not complete.");
    *out_section = AllocateShape(NonNullDrawingLayer(section.Shape()));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_count(
  const OcctSharp_ShapeHandle* shape, const int32_t samples_per_curve,
  int32_t* out_polyline_count, int32_t* out_point_count)
{
  if (out_polyline_count == nullptr || out_point_count == nullptr)
  {
    SetLastError("Drawing polyline count output pointers are null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_polyline_count = 0;
  *out_point_count = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const DrawingPolylineData data = BuildDrawingPolylines(shape->Value, samples_per_curve);
    if (data.Polylines.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing polyline count exceeds the 32-bit ABI.");
    *out_polyline_count = static_cast<int32_t>(data.Polylines.size());
    *out_point_count = static_cast<int32_t>(data.Points.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_copy(
  const OcctSharp_ShapeHandle* shape, const int32_t samples_per_curve,
  OcctSharp_DrawingPolyline* polylines, const int32_t polyline_capacity,
  OcctSharp_Xyz* points, const int32_t point_capacity,
  int32_t* out_polylines_written, int32_t* out_points_written)
{
  if (out_polylines_written == nullptr || out_points_written == nullptr)
  {
    SetLastError("Drawing polyline copy output pointers are null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_polylines_written = 0;
  *out_points_written = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const DrawingPolylineData data = BuildDrawingPolylines(shape->Value, samples_per_curve);
    ValidateOutputCapacity(polyline_capacity, static_cast<int32_t>(data.Polylines.size()), polylines,
      "The drawing polyline output buffer is too small.");
    ValidateOutputCapacity(point_capacity, static_cast<int32_t>(data.Points.size()), points,
      "The drawing point output buffer is too small.");
    std::copy(data.Polylines.begin(), data.Polylines.end(), polylines);
    std::copy(data.Points.begin(), data.Points.end(), points);
    *out_polylines_written = static_cast<int32_t>(data.Polylines.size());
    *out_points_written = static_cast<int32_t>(data.Points.size());
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_count(
  const OcctSharp_ShapeHandle* shape, const int32_t item_kind, const int32_t ancestor_kind,
  int32_t* out_item_count, int32_t* out_ancestor_count, int32_t* out_relation_count)
{
  if (out_item_count == nullptr || out_ancestor_count == nullptr || out_relation_count == nullptr)
  { SetLastError("Topology adjacency count output pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_item_count = *out_ancestor_count = *out_relation_count = 0;
  if (item_kind < 0 || item_kind > 7 || ancestor_kind < 0 || ancestor_kind > 7)
  { SetLastError("Topology adjacency kinds must be TopAbs kinds from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (item_kind <= ancestor_kind)
  { SetLastError("The topology item kind must be lower-level than the ancestor kind."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> items;
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> ancestors;
    NCollection_IndexedDataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher> adjacency;
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind), items);
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(ancestor_kind), ancestors);
    TopExp::MapShapesAndUniqueAncestors(
      shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind),
      static_cast<TopAbs_ShapeEnum>(ancestor_kind), adjacency, false);
    int64_t relations = 0;
    for (int32_t index = 1; index <= items.Extent(); ++index)
      if (adjacency.Contains(items.FindKey(index))) relations += adjacency.FindFromKey(items.FindKey(index)).Extent();
    if (items.Extent() > std::numeric_limits<int32_t>::max()
        || ancestors.Extent() > std::numeric_limits<int32_t>::max()
        || relations > std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology adjacency snapshot exceeds 32-bit capacity.");
    *out_item_count = items.Extent();
    *out_ancestor_count = ancestors.Extent();
    *out_relation_count = static_cast<int32_t>(relations);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_snapshot(
  const OcctSharp_ShapeHandle* shape, const int32_t item_kind, const int32_t ancestor_kind,
  OcctSharp_ShapeHandle** out_items, const int32_t item_capacity,
  OcctSharp_ShapeHandle** out_ancestors, const int32_t ancestor_capacity,
  int32_t* out_offsets, const int32_t offset_capacity,
  int32_t* out_ancestor_indices, const int32_t relation_capacity,
  int32_t* out_items_written, int32_t* out_ancestors_written, int32_t* out_relations_written)
{
  if (out_items_written == nullptr || out_ancestors_written == nullptr || out_relations_written == nullptr)
  { SetLastError("Topology adjacency written-count pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_items_written = *out_ancestors_written = *out_relations_written = 0;
  if (item_kind < 0 || item_kind > 7 || ancestor_kind < 0 || ancestor_kind > 7 || item_kind <= ancestor_kind)
  { SetLastError("Topology adjacency kinds are invalid or not ordered from item to ancestor."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (item_capacity < 0 || ancestor_capacity < 0 || offset_capacity < 1 || relation_capacity < 0
      || (item_capacity > 0 && out_items == nullptr)
      || (ancestor_capacity > 0 && out_ancestors == nullptr)
      || out_offsets == nullptr
      || (relation_capacity > 0 && out_ancestor_indices == nullptr))
  { SetLastError("Topology adjacency buffer pointers or capacities are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> items;
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> ancestors;
    NCollection_IndexedDataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher> adjacency;
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind), items);
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(ancestor_kind), ancestors);
    TopExp::MapShapesAndUniqueAncestors(
      shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind),
      static_cast<TopAbs_ShapeEnum>(ancestor_kind), adjacency, false);
    int32_t relations = 0;
    for (int32_t index = 1; index <= items.Extent(); ++index)
      if (adjacency.Contains(items.FindKey(index))) relations += adjacency.FindFromKey(items.FindKey(index)).Extent();
    if (item_capacity < items.Extent() || ancestor_capacity < ancestors.Extent()
        || offset_capacity < items.Extent() + 1 || relation_capacity < relations)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A topology adjacency snapshot buffer is too small.");

    int32_t item_written = 0;
    int32_t ancestor_written = 0;
    try
    {
      for (int32_t index = 1; index <= items.Extent(); ++index)
        out_items[item_written++] = AllocateShape(items.FindKey(index));
      for (int32_t index = 1; index <= ancestors.Extent(); ++index)
        out_ancestors[ancestor_written++] = AllocateShape(ancestors.FindKey(index));

      int32_t relation_written = 0;
      out_offsets[0] = 0;
      for (int32_t index = 1; index <= items.Extent(); ++index)
      {
        const TopoDS_Shape& item = items.FindKey(index);
        if (adjacency.Contains(item))
        {
          const NCollection_List<TopoDS_Shape>& list = adjacency.FindFromKey(item);
          for (NCollection_List<TopoDS_Shape>::Iterator iterator(list); iterator.More(); iterator.Next())
          {
            const int ancestor_index = ancestors.FindIndex(iterator.Value());
            if (ancestor_index <= 0)
              throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned an ancestor outside the indexed topology map.");
            out_ancestor_indices[relation_written++] = ancestor_index - 1;
          }
        }
        out_offsets[index] = relation_written;
      }
      *out_items_written = item_written;
      *out_ancestors_written = ancestor_written;
      *out_relations_written = relation_written;
    }
    catch (...)
    {
      for (int32_t index = 0; index < item_written; ++index) occtsharp_shape_release(out_items[index]);
      for (int32_t index = 0; index < ancestor_written; ++index) occtsharp_shape_release(out_ancestors[index]);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_replace_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  const OcctSharp_ShapeHandle* replacement, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The reshaped output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidateUsableShape(target);
    ValidateUsableShape(replacement);
    bool contains = shape->Value.IsSame(target->Value);
    if (!contains)
    {
      for (TopExp_Explorer explorer(shape->Value, target->Value.ShapeType()); explorer.More(); explorer.Next())
      {
        if (explorer.Current().IsSame(target->Value)) { contains = true; break; }
      }
    }
    if (!contains)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The replacement target is not contained in the source topology.");
    BRepTools_ReShape reshaper;
    reshaper.Replace(target->Value, replacement->Value);
    TopoDS_Shape result = reshaper.Apply(shape->Value);
    if (result.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT produced a null replacement result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_remove_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The reshaped output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidateUsableShape(target);
    if (shape->Value.IsSame(target->Value))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The root shape cannot be removed from itself.");
    bool contains = false;
    for (TopExp_Explorer explorer(shape->Value, target->Value.ShapeType()); explorer.More(); explorer.Next())
    {
      if (explorer.Current().IsSame(target->Value)) { contains = true; break; }
    }
    if (!contains)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The removal target is not contained in the source topology.");
    BRepTools_ReShape reshaper;
    reshaper.Remove(target->Value);
    TopoDS_Shape result = reshaper.Apply(shape->Value);
    if (result.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT produced a null removal result.");
    *out_shape = AllocateShape(std::move(result));
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_make_thick_solid(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_ShapeHandle* const* closing_faces, const int32_t face_count,
  const double offset, const double tolerance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The thick-solid output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (closing_faces == nullptr || face_count < 1)
  { SetLastError("A thick solid requires at least one closing face to remove."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (!std::isfinite(offset) || offset == 0.0 || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The wall offset must be finite and non-zero, and tolerance must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    if (shape->Value.ShapeType() != TopAbs_SOLID)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Thick-solid construction requires a solid source shape.");
    NCollection_List<TopoDS_Shape> faces;
    for (int32_t index = 0; index < face_count; ++index)
    {
      ValidateUsableShape(closing_faces[index]);
      if (closing_faces[index]->Value.ShapeType() != TopAbs_FACE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Every thick-solid closing shape must be a face.");
      faces.Append(closing_faces[index]->Value);
    }
    BRepOffsetAPI_MakeThickSolid builder;
    builder.MakeThickSolidByJoin(shape->Value, faces, offset, tolerance);
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT thick-solid construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_summary(
  const OcctSharp_ShapeHandle* shape, OcctSharp_ShapeTopologySummary* out_summary)
{
  if (out_summary == nullptr)
  {
    SetLastError("The topology-summary output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_summary = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    out_summary->unique_counts = BuildTopologyCounts(shape->Value, true);
    out_summary->occurrence_counts = BuildTopologyCounts(shape->Value, false);
    out_summary->is_closed = IsTopologyClosed(shape->Value) ? 1 : 0;
    BRepCheck_Analyzer analyzer(shape->Value);
    out_summary->is_valid = analyzer.IsValid() ? 1 : 0;
    BuildToleranceRange(shape->Value, TopAbs_VERTEX,
      out_summary->min_vertex_tolerance, out_summary->max_vertex_tolerance);
    BuildToleranceRange(shape->Value, TopAbs_EDGE,
      out_summary->min_edge_tolerance, out_summary->max_edge_tolerance);
    BuildToleranceRange(shape->Value, TopAbs_FACE,
      out_summary->min_face_tolerance, out_summary->max_face_tolerance);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issue_count(
  const OcctSharp_ShapeHandle* shape,
  const int32_t geometry_checks,
  const int32_t exact,
  int32_t* out_is_valid,
  int32_t* out_issue_count)
{
  if (out_is_valid == nullptr || out_issue_count == nullptr)
  {
    SetLastError("A validation count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_is_valid = 0;
  *out_issue_count = 0;
  return Guard([&]
  {
    ValidationData data = BuildValidationData(shape, geometry_checks != 0, exact != 0);
    *out_is_valid = data.IsValid ? 1 : 0;
    *out_issue_count = static_cast<int32_t>(data.Issues.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issues(
  const OcctSharp_ShapeHandle* shape,
  const int32_t geometry_checks,
  const int32_t exact,
  OcctSharp_ValidationIssue* issues,
  const int32_t capacity,
  int32_t* out_is_valid,
  int32_t* out_issue_count)
{
  if (out_is_valid == nullptr || out_issue_count == nullptr || capacity < 0
      || (capacity > 0 && issues == nullptr))
  {
    SetLastError("The validation output buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_is_valid = 0;
  *out_issue_count = 0;
  return Guard([&]
  {
    ValidationData data = BuildValidationData(shape, geometry_checks != 0, exact != 0);
    *out_is_valid = data.IsValid ? 1 : 0;
    *out_issue_count = static_cast<int32_t>(data.Issues.size());
    if (capacity < *out_issue_count)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The validation issue buffer is too small.");
    if (*out_issue_count > 0)
      std::memcpy(issues, data.Issues.data(), data.Issues.size() * sizeof(OcctSharp_ValidationIssue));
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_with_history(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  const int32_t operation_kind, const int32_t tracked_kind,
  OcctSharp_ShapeHandle** out_shape, OcctSharp_BooleanHistorySummary* out_history)
{
  if (out_shape == nullptr || out_history == nullptr)
  { SetLastError("Boolean history output pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  *out_history = {};
  if (operation_kind < 0 || operation_kind > 2)
  { SetLastError("Boolean history operation must be Fuse, Cut, or Common."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (tracked_kind < 0 || tracked_kind > 7)
  { SetLastError("Boolean history tracked kind must be Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(left);
    ValidateUsableShape(right);
    std::unique_ptr<BRepAlgoAPI_BooleanOperation> operation;
    if (operation_kind == 0) operation = std::make_unique<BRepAlgoAPI_Fuse>(left->Value, right->Value);
    else if (operation_kind == 1) operation = std::make_unique<BRepAlgoAPI_Cut>(left->Value, right->Value);
    else operation = std::make_unique<BRepAlgoAPI_Common>(left->Value, right->Value);
    operation->Build();
    if (!operation->IsDone() || operation->Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean operation with history did not complete.");

    const TopAbs_ShapeEnum kind = static_cast<TopAbs_ShapeEnum>(tracked_kind);
    auto summarize = [&](const TopoDS_Shape& input,
                         int32_t& source_count,
                         int32_t& modified_source_count,
                         int32_t& generated_source_count,
                         int32_t& deleted_source_count,
                         int32_t& modified_result_count,
                         int32_t& generated_result_count)
    {
      NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> sources;
      TopExp::MapShapes(input, kind, sources);
      source_count = sources.Extent();
      for (int32_t index = 1; index <= sources.Extent(); ++index)
      {
        const TopoDS_Shape& source = sources.FindKey(index);
        const auto& modified = operation->Modified(source);
        const auto& generated = operation->Generated(source);
        if (!modified.IsEmpty()) ++modified_source_count;
        if (!generated.IsEmpty()) ++generated_source_count;
        if (operation->IsDeleted(source)) ++deleted_source_count;
        modified_result_count += modified.Extent();
        generated_result_count += generated.Extent();
      }
    };
    summarize(left->Value,
      out_history->left_source_count,
      out_history->left_modified_source_count,
      out_history->left_generated_source_count,
      out_history->left_deleted_source_count,
      out_history->left_modified_result_count,
      out_history->left_generated_result_count);
    summarize(right->Value,
      out_history->right_source_count,
      out_history->right_modified_source_count,
      out_history->right_generated_source_count,
      out_history->right_deleted_source_count,
      out_history->right_modified_result_count,
      out_history->right_generated_result_count);
    *out_shape = AllocateShape(operation->Shape());
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

namespace
{
BRepExtrema_DistShapeShape ComputeExactDistance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second)
{
  ValidateUsableShape(first);
  ValidateUsableShape(second);
  BRepExtrema_DistShapeShape operation(first->Value, second->Value);
  if (!operation.IsDone() || operation.NbSolution() <= 0)
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT exact distance did not produce a solution.");
  return operation;
}

OcctSharp_Xyz CopyPoint(const gp_Pnt& point)
{
  return { point.X(), point.Y(), point.Z() };
}

void CopyInertia(const gp_Mat& matrix, OcctSharp_InspectionProperties& properties)
{
  properties.i11 = matrix.Value(1, 1);
  properties.i12 = matrix.Value(1, 2);
  properties.i13 = matrix.Value(1, 3);
  properties.i21 = matrix.Value(2, 1);
  properties.i22 = matrix.Value(2, 2);
  properties.i23 = matrix.Value(2, 3);
  properties.i31 = matrix.Value(3, 1);
  properties.i32 = matrix.Value(3, 2);
  properties.i33 = matrix.Value(3, 3);
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, int32_t* count)
{
  if (count == nullptr) { SetLastError("The exact-distance count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape operation = ComputeExactDistance(first, second);
    *count = operation.NbSolution();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_solution(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  const int32_t index, OcctSharp_ExtremaSolution* solution)
{
  if (solution == nullptr) { SetLastError("The exact-distance solution pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *solution = {};
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape operation = ComputeExactDistance(first, second);
    if (index < 1 || index > operation.NbSolution())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The exact-distance solution index is outside the valid 1-based range.");

    const BRepExtrema_SupportType firstKind = operation.SupportTypeShape1(index);
    const BRepExtrema_SupportType secondKind = operation.SupportTypeShape2(index);
    OcctSharp_ShapeHandle* firstSupport = nullptr;
    OcctSharp_ShapeHandle* secondSupport = nullptr;
    try
    {
      firstSupport = AllocateShape(operation.SupportOnShape1(index));
      secondSupport = AllocateShape(operation.SupportOnShape2(index));
      solution->distance = operation.Value();
      solution->point_on_first = CopyPoint(operation.PointOnShape1(index));
      solution->point_on_second = CopyPoint(operation.PointOnShape2(index));
      solution->first_support_kind = static_cast<int32_t>(firstKind);
      solution->second_support_kind = static_cast<int32_t>(secondKind);
      solution->is_inner_solution = operation.InnerSolution() ? 1 : 0;
      if (firstKind == BRepExtrema_IsOnEdge)
      {
        operation.ParOnEdgeS1(index, solution->first_edge_parameter);
        solution->has_first_edge_parameter = 1;
      }
      else if (firstKind == BRepExtrema_IsInFace)
      {
        operation.ParOnFaceS1(index, solution->first_face_u, solution->first_face_v);
        solution->has_first_face_parameters = 1;
      }
      if (secondKind == BRepExtrema_IsOnEdge)
      {
        operation.ParOnEdgeS2(index, solution->second_edge_parameter);
        solution->has_second_edge_parameter = 1;
      }
      else if (secondKind == BRepExtrema_IsInFace)
      {
        operation.ParOnFaceS2(index, solution->second_face_u, solution->second_face_v);
        solution->has_second_face_parameters = 1;
      }
      solution->first_support = firstSupport;
      solution->second_support = secondSupport;
      firstSupport = nullptr;
      secondSupport = nullptr;
    }
    catch (...)
    {
      if (firstSupport != nullptr) occtsharp_shape_release(firstSupport);
      if (secondSupport != nullptr) occtsharp_shape_release(secondSupport);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_pair_classify(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  const double tolerance, int32_t* classification, double* distance,
  double* overlap_volume, OcctSharp_ShapeHandle** overlap_shape)
{
  if (classification == nullptr || distance == nullptr || overlap_volume == nullptr || overlap_shape == nullptr)
  { SetLastError("A shape-pair classification output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *classification = 0; *distance = 0.0; *overlap_volume = 0.0; *overlap_shape = nullptr;
  if (!std::isfinite(tolerance) || tolerance < 0.0)
  { SetLastError("Shape-pair tolerance must be finite and non-negative."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape extrema = ComputeExactDistance(first, second);
    *distance = extrema.Value();
    if (*distance > tolerance) { *classification = 0; return; }

    BRepAlgoAPI_Common common(first->Value, second->Value);
    common.Build();
    if (!common.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not compute the pair overlap.");
    const TopoDS_Shape overlap = common.Shape();
    if (!overlap.IsNull())
    {
      GProp_GProps overlapProps;
      BRepGProp::VolumeProperties(overlap, overlapProps, true);
      *overlap_volume = std::abs(overlapProps.Mass());
    }
    const double volumeTolerance = std::max(tolerance * tolerance * tolerance, 1.0e-18);
    if (*overlap_volume <= volumeTolerance) { *classification = 1; return; }

    GProp_GProps firstProps;
    GProp_GProps secondProps;
    BRepGProp::VolumeProperties(first->Value, firstProps, true);
    BRepGProp::VolumeProperties(second->Value, secondProps, true);
    const double smallerVolume = std::min(std::abs(firstProps.Mass()), std::abs(secondProps.Mass()));
    const double containmentTolerance = std::max(volumeTolerance, smallerVolume * 1.0e-9);
    *classification = smallerVolume > volumeTolerance
      && std::abs(*overlap_volume - smallerVolume) <= containmentTolerance ? 2 : 3;
    *overlap_shape = AllocateShape(overlap);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_inspection_properties(
  const OcctSharp_ShapeHandle* shape, const int32_t property_kind,
  OcctSharp_InspectionProperties* properties)
{
  if (properties == nullptr) { SetLastError("The inspection-properties output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *properties = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const TopAbs_ShapeEnum kind = shape->Value.ShapeType();
    GProp_GProps result;
    switch (property_kind)
    {
      case 0:
        if (kind != TopAbs_EDGE && kind != TopAbs_WIRE)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Length inspection requires an edge or wire.");
        BRepGProp::LinearProperties(shape->Value, result);
        break;
      case 1:
        if (kind != TopAbs_FACE && kind != TopAbs_SHELL)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Area inspection requires a face or shell.");
        BRepGProp::SurfaceProperties(shape->Value, result);
        break;
      case 2:
        if (kind != TopAbs_SOLID && kind != TopAbs_COMPSOLID && kind != TopAbs_COMPOUND)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Volume inspection requires a solid, compsolid, or compound.");
        BRepGProp::VolumeProperties(shape->Value, result, true);
        break;
      default:
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The inspection property kind is outside the supported range.");
    }
    properties->mass = result.Mass();
    properties->center = CopyPoint(result.CentreOfMass());
    CopyInertia(result.MatrixOfInertia(), *properties);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_angle(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, double* radians)
{
  if (radians == nullptr) { SetLastError("The shape-angle output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *radians = 0.0;
  return Guard([&]
  {
    ValidateUsableShape(first); ValidateUsableShape(second);
    if (first->Value.ShapeType() == TopAbs_EDGE && second->Value.ShapeType() == TopAbs_EDGE)
    {
      BRepAdaptor_Curve firstCurve(TopoDS::Edge(first->Value));
      BRepAdaptor_Curve secondCurve(TopoDS::Edge(second->Value));
      if (firstCurve.GetType() != GeomAbs_Line || secondCurve.GetType() != GeomAbs_Line)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge angle inspection requires two linear edges.");
      *radians = firstCurve.Line().Direction().Angle(secondCurve.Line().Direction());
      return;
    }
    if (first->Value.ShapeType() == TopAbs_FACE && second->Value.ShapeType() == TopAbs_FACE)
    {
      BRepAdaptor_Surface firstSurface(TopoDS::Face(first->Value), true);
      BRepAdaptor_Surface secondSurface(TopoDS::Face(second->Value), true);
      if (firstSurface.GetType() != GeomAbs_Plane || secondSurface.GetType() != GeomAbs_Plane)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face angle inspection requires two planar faces.");
      gp_Dir firstNormal = firstSurface.Plane().Axis().Direction();
      gp_Dir secondNormal = secondSurface.Plane().Axis().Direction();
      if (first->Value.Orientation() == TopAbs_REVERSED) firstNormal.Reverse();
      if (second->Value.Orientation() == TopAbs_REVERSED) secondNormal.Reverse();
      *radians = firstNormal.Angle(secondNormal);
      return;
    }
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Angle inspection requires two linear edges or two planar faces.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_radial_measurement(
  const OcctSharp_ShapeHandle* shape, OcctSharp_RadialMeasurement* measurement)
{
  if (measurement == nullptr) { SetLastError("The radial-measurement output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *measurement = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    if (shape->Value.ShapeType() == TopAbs_EDGE)
    {
      BRepAdaptor_Curve curve(TopoDS::Edge(shape->Value));
      if (curve.GetType() != GeomAbs_Circle)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge radial inspection requires circular geometry.");
      measurement->geometry_kind = 0;
      measurement->radius = curve.Circle().Radius();
    }
    else if (shape->Value.ShapeType() == TopAbs_FACE)
    {
      BRepAdaptor_Surface surface(TopoDS::Face(shape->Value), true);
      if (surface.GetType() == GeomAbs_Cylinder)
      {
        measurement->geometry_kind = 1;
        measurement->radius = surface.Cylinder().Radius();
      }
      else if (surface.GetType() == GeomAbs_Cone)
      {
        measurement->geometry_kind = 2;
        measurement->radius = surface.Cone().RefRadius();
        measurement->semi_angle = surface.Cone().SemiAngle();
      }
      else
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face radial inspection requires cylindrical or conical geometry.");
    }
    else
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Radial inspection requires an edge or face.");
    measurement->diameter = measurement->radius * 2.0;
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_import_step(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, int32_t* out_root_count)
{
  if (out_root_count == nullptr)
  {
    SetLastError("The imported STEP root-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_root_count = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    std::vector<TDF_Label> roots = ImportStepRootsIntoXdeDocument(file_path, document->Document);
    GetXdeShapeTool(document)->UpdateAssemblies();
    *out_root_count = static_cast<int32_t>(roots.size());
  });
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
  return occtsharp_xde_document_read_step_options(
    file_path, 1, 1, 1, 1, 1, 1, 1, out_document);
}

template <typename TProvider>
OcctSharp_Status ReadXdeMeshDocument(
  const char* file_path,
  OcctSharp_OcafDocumentHandle** out_document,
  TProvider& provider,
  const char* failure_message)
{
  if (out_document == nullptr)
  {
    SetLastError("The output mesh-scene XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      if (!provider.Read(TCollection_AsciiString(file_path), result->Document))
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, failure_message);
      GetXdeShapeTool(result)->UpdateAssemblies();
      *out_document = result;
    }
    catch (...)
    {
      occtsharp_ocaf_document_release(result);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_gltf(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return ReadXdeMeshDocument(file_path, out_document, provider, "OCCT could not transfer glTF/GLB into an XDE scene.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_obj(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return ReadXdeMeshDocument(file_path, out_document, provider, "OCCT could not transfer OBJ into an XDE scene.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step_options(
  const char* file_path,
  const int32_t read_names,
  const int32_t read_colors,
  const int32_t read_layers,
  const int32_t read_validation_properties,
  const int32_t read_materials,
  const int32_t read_gdt,
  const int32_t read_views,
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(read_names) || !is_flag(read_colors) || !is_flag(read_layers)
        || !is_flag(read_validation_properties) || !is_flag(read_materials)
        || !is_flag(read_gdt) || !is_flag(read_views))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE STEP read option is not Boolean.");
    ValidatePath(file_path);
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      STEPCAFControl_Reader reader;
      ConfigureXdeReader(
        reader,
        read_names != 0,
        read_colors != 0,
        read_layers != 0,
        read_validation_properties != 0,
        read_materials != 0,
        read_gdt != 0,
        read_views != 0);
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
  return occtsharp_xde_document_write_step_options(
    document, file_path, 0, 4, 1, 1, 1, 1, 1, 1);
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step_options(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  const int32_t model_type,
  const int32_t schema,
  const int32_t write_names,
  const int32_t write_colors,
  const int32_t write_layers,
  const int32_t write_validation_properties,
  const int32_t write_materials,
  const int32_t write_gdt)
{
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (model_type < static_cast<int32_t>(STEPControl_AsIs)
        || model_type > static_cast<int32_t>(STEPControl_Hybrid))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP model type is outside the supported range.");
    if (schema < static_cast<int32_t>(DESTEP_Parameters::WriteMode_StepSchema_AP214CD)
        || schema > static_cast<int32_t>(DESTEP_Parameters::WriteMode_StepSchema_AP242DIS))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP schema is outside the supported range.");
    if (!is_flag(write_names) || !is_flag(write_colors) || !is_flag(write_layers)
        || !is_flag(write_validation_properties) || !is_flag(write_materials)
        || !is_flag(write_gdt))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE STEP write option is not Boolean.");
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before STEP export.");
    }
    STEPCAFControl_Writer writer;
    ConfigureXdeWriter(
      writer,
      write_names != 0,
      write_colors != 0,
      write_layers != 0,
      write_validation_properties != 0,
      write_materials != 0,
      write_gdt != 0);
    DESTEP_Parameters parameters;
    parameters.InitFromStatic();
    parameters.WriteSchema = static_cast<DESTEP_Parameters::WriteMode_StepSchema>(schema);
    if (!writer.Transfer(document->Document, parameters, static_cast<STEPControl_StepModelType>(model_type)))
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer the XDE document to STEP.");
    }
    const IFSelect_ReturnStatus writeStatus = writer.Write(file_path);
    if (writeStatus != IFSelect_RetDone)
    {
      const std::string message =
        "OCCT wrote the XDE STEP document with non-success status "
        + std::to_string(static_cast<int>(writeStatus)) + ".";
      throw OperationFailure(
        OCCTSHARP_STATUS_FILE_IO_ERROR,
        message.c_str());
    }
  });
}

template <typename TProvider>
OcctSharp_Status WriteXdeMeshDocument(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  TProvider& provider,
  const char* failure_message)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before mesh-scene export.");
    NCollection_Sequence<TDF_Label> roots;
    const opencascade::handle<XCAFDoc_ShapeTool> shapeTool = GetXdeShapeTool(document);
    shapeTool->GetFreeShapes(roots);
    for (int32_t index = 1; index <= roots.Length(); ++index)
    {
      const TopoDS_Shape shape = shapeTool->GetShape(roots.Value(index));
      if (shape.IsNull()) continue;
      BRepMesh_IncrementalMesh mesher(shape, 0.1, false, 0.5, true);
      if (!mesher.IsDone())
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not triangulate an XDE scene root for mesh export.");
    }
    if (!provider.Write(TCollection_AsciiString(file_path), document->Document))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, failure_message);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_gltf(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT glTF/GLB scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_obj(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT OBJ scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_ply(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEPLY_ConfigurationNode> node = new DEPLY_ConfigurationNode();
  DEPLY_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT PLY scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_vrml(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEVRML_ConfigurationNode> node = new DEVRML_ConfigurationNode();
  DEVRML_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT VRML scene export failed.");
}

namespace
{
opencascade::handle<XCAFDoc_DimTolTool> GetDimTolTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::DimTolTool(document->Document->Main());
}

opencascade::handle<XCAFDoc_ViewTool> GetViewTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ViewTool(document->Document->Main());
}

opencascade::handle<XCAFDoc_ClippingPlaneTool> GetClippingPlaneTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ClippingPlaneTool(document->Document->Main());
}

opencascade::handle<TCollection_HAsciiString> MakePmiString(const char* value)
{
  return new TCollection_HAsciiString(value == nullptr ? "" : value);
}

std::string CopyPmiString(const opencascade::handle<TCollection_HAsciiString>& value)
{
  return value.IsNull() ? std::string() : std::string(value->ToCString());
}

void ValidateSavedView(const OcctSharp_SavedView& data)
{
  const auto finite_xyz = [](const OcctSharp_Xyz& value)
  {
    return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
  };
  const auto square_magnitude = [](const OcctSharp_Xyz& value)
  {
    return value.x * value.x + value.y * value.y + value.z * value.z;
  };
  if (data.projection_type < static_cast<int32_t>(XCAFView_ProjectionType_NoCamera)
      || data.projection_type > static_cast<int32_t>(XCAFView_ProjectionType_Central)
      || !finite_xyz(data.projection_point) || !finite_xyz(data.view_direction)
      || !finite_xyz(data.up_direction) || square_magnitude(data.view_direction) <= 1.0e-24
      || square_magnitude(data.up_direction) <= 1.0e-24
      || !std::isfinite(data.zoom_factor) || data.zoom_factor <= 0.0
      || !std::isfinite(data.window_horizontal_size) || data.window_horizontal_size <= 0.0
      || !std::isfinite(data.window_vertical_size) || data.window_vertical_size <= 0.0
      || !std::isfinite(data.front_clipping_distance)
      || !std::isfinite(data.back_clipping_distance))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view values are invalid or non-finite.");
  const gp_Dir view(data.view_direction.x, data.view_direction.y, data.view_direction.z);
  const gp_Dir up(data.up_direction.x, data.up_direction.y, data.up_direction.z);
  if (std::abs(view.Dot(up)) > 0.999999)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view up and view directions cannot be parallel.");
}

void SetSavedViewObject(
  const TDF_Label& label, const OcctSharp_SavedView& data,
  const char* name, const char* clippingExpression)
{
  ValidateSavedView(data);
  auto object = new XCAFView_Object();
  object->SetName(MakePmiString(name));
  object->SetType(static_cast<XCAFView_ProjectionType>(data.projection_type));
  object->SetProjectionPoint(gp_Pnt(data.projection_point.x, data.projection_point.y, data.projection_point.z));
  object->SetViewDirection(gp_Dir(data.view_direction.x, data.view_direction.y, data.view_direction.z));
  object->SetUpDirection(gp_Dir(data.up_direction.x, data.up_direction.y, data.up_direction.z));
  object->SetZoomFactor(data.zoom_factor);
  object->SetWindowHorizontalSize(data.window_horizontal_size);
  object->SetWindowVerticalSize(data.window_vertical_size);
  object->SetClippingExpression(MakePmiString(clippingExpression));
  if (data.has_front_clipping) object->SetFrontPlaneDistance(data.front_clipping_distance);
  else object->UnsetFrontPlaneClipping();
  if (data.has_back_clipping) object->SetBackPlaneDistance(data.back_clipping_distance);
  else object->UnsetBackPlaneClipping();
  object->SetViewVolumeSidesClipping(data.has_view_volume_sides_clipping != 0);
  XCAFDoc_View::Set(label)->SetObject(object);
}

NCollection_Sequence<TDF_Label> AddSavedViewPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_PlaneEquation* planes, const int32_t planeCount)
{
  if (planeCount < 0 || (planeCount > 0 && planes == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The saved-view clipping-plane array is invalid.");
  NCollection_Sequence<TDF_Label> labels;
  const auto tool = GetClippingPlaneTool(document);
  for (int32_t index = 0; index < planeCount; ++index)
  {
    const auto& plane = planes[index];
    if (!std::isfinite(plane.a) || !std::isfinite(plane.b)
        || !std::isfinite(plane.c) || !std::isfinite(plane.d))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view clipping-plane values must be finite.");
    labels.Append(tool->AddClippingPlane(
      gp_Pln(plane.a, plane.b, plane.c, plane.d),
      new TCollection_HAsciiString("OcctSharp saved view"), plane.capping != 0));
  }
  return labels;
}

void RemoveUnreferencedPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const NCollection_Sequence<TDF_Label>& labels)
{
  const auto tool = GetClippingPlaneTool(document);
  for (NCollection_Sequence<TDF_Label>::Iterator iterator(labels); iterator.More(); iterator.Next())
    tool->RemoveClippingPlane(iterator.Value());
}

gp_Pnt ToPoint(const OcctSharp_Xyz& value) { return gp_Pnt(value.x, value.y, value.z); }
gp_Dir ToDirection(const OcctSharp_Xyz& value) { return gp_Dir(value.x, value.y, value.z); }
gp_Ax2 ToAxis(const OcctSharp_Ax2& value)
{
  return gp_Ax2(ToPoint(value.origin), ToDirection(value.direction), ToDirection(value.x_direction));
}
gp_Pln ToPlane(const OcctSharp_Plane& value) { return gp_Pln(ToPoint(value.origin), ToDirection(value.normal)); }

OcctSharp_Ax2 CopyAxis(const gp_Ax2& value)
{
  return {
    CopyPoint(value.Location()),
    { value.XDirection().X(), value.XDirection().Y(), value.XDirection().Z() },
    { value.YDirection().X(), value.YDirection().Y(), value.YDirection().Z() },
    { value.Direction().X(), value.Direction().Y(), value.Direction().Z() }
  };
}

OcctSharp_Plane CopyPlane(const gp_Pln& value)
{
  return {
    CopyPoint(value.Location()),
    { value.Axis().Direction().X(), value.Axis().Direction().Y(), value.Axis().Direction().Z() }
  };
}

std::vector<std::string> SplitEntries(const char* entries)
{
  std::vector<std::string> result;
  if (entries == nullptr || entries[0] == '\0') return result;
  std::string value(entries);
  size_t start = 0;
  while (start <= value.size())
  {
    const size_t end = value.find('\n', start);
    const std::string item = value.substr(start, end == std::string::npos ? std::string::npos : end - start);
    if (!item.empty()) result.push_back(item);
    if (end == std::string::npos) break;
    start = end + 1;
  }
  return result;
}

NCollection_Sequence<TDF_Label> ResolveEntries(
  const OcctSharp_OcafDocumentHandle* document, const char* entries)
{
  NCollection_Sequence<TDF_Label> result;
  for (const std::string& entry : SplitEntries(entries)) result.Append(ResolveOcafLabel(document, entry.c_str()));
  return result;
}

std::vector<TDF_Label> PmiLabels(const OcctSharp_OcafDocumentHandle* document, const int32_t kind)
{
  NCollection_Sequence<TDF_Label> labels;
  if (kind == 0) GetDimTolTool(document)->GetDimensionLabels(labels);
  else if (kind == 1) GetDimTolTool(document)->GetGeomToleranceLabels(labels);
  else if (kind == 2) GetDimTolTool(document)->GetDatumLabels(labels);
  else if (kind == 3) GetViewTool(document)->GetViewLabels(labels);
  else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI item kind is outside the supported range.");
  std::vector<TDF_Label> result;
  result.reserve(static_cast<size_t>(labels.Size()));
  for (NCollection_Sequence<TDF_Label>::Iterator iterator(labels); iterator.More(); iterator.Next())
    result.push_back(iterator.Value());
  return result;
}

opencascade::handle<XCAFDimTolObjects_DimensionObject> GetDimensionObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_Dimension> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_Dimension::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a semantic dimension.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The semantic dimension has no value object.");
  return object;
}

opencascade::handle<XCAFDimTolObjects_GeomToleranceObject> GetToleranceObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_GeomTolerance> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_GeomTolerance::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a geometric tolerance.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The geometric tolerance has no value object.");
  return object;
}

opencascade::handle<XCAFDimTolObjects_DatumObject> GetDatumObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_Datum> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_Datum::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a datum.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The datum has no value object.");
  opencascade::handle<TDataStd_RealArray> point;
  if (label.FindChild(17, false).FindAttribute(TDataStd_RealArray::GetID(), point)
      && point->Length() == 3)
  {
    const int32_t lower = point->Lower();
    object->SetPoint(gp_Pnt(point->Value(lower), point->Value(lower + 1), point->Value(lower + 2)));
  }
  return object;
}

opencascade::handle<XCAFView_Object> GetSavedViewObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_View> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_View::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The saved view has no value object.");
  return object;
}

void ValidateArray(const void* values, const int32_t count, const char* message)
{
  if (count < 0 || (count > 0 && values == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}

void SetDimensionObject(
  const TDF_Label& label, const OcctSharp_PmiDimension& data,
  const double* values, const int32_t valueCount,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName)
{
  ValidateArray(values, valueCount, "The dimension value array is invalid.");
  ValidateArray(modifiers, modifierCount, "The dimension modifier array is invalid.");
  if (valueCount <= 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A semantic dimension requires at least one value.");
  auto object = new XCAFDimTolObjects_DimensionObject();
  object->SetType(static_cast<XCAFDimTolObjects_DimensionType>(data.type));
  auto valueArray = new NCollection_HArray1<double>(1, valueCount);
  for (int32_t index = 0; index < valueCount; ++index)
  {
    if (!std::isfinite(values[index])) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension values must be finite.");
    valueArray->SetValue(index + 1, values[index]);
  }
  object->SetValues(valueArray);
  if (data.has_qualifier) object->SetQualifier(static_cast<XCAFDimTolObjects_DimensionQualifier>(data.qualifier));
  if (data.has_angular_qualifier) object->SetAngularQualifier(static_cast<XCAFDimTolObjects_AngularQualifier>(data.angular_qualifier));
  if (data.has_class_of_tolerance)
    object->SetClassOfTolerance(data.is_hole != 0,
      static_cast<XCAFDimTolObjects_DimensionFormVariance>(data.form_variance),
      static_cast<XCAFDimTolObjects_DimensionGrade>(data.grade));
  object->SetNbOfDecimalPlaces(data.left_decimal_places, data.right_decimal_places);
  NCollection_Sequence<XCAFDimTolObjects_DimensionModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_DimensionModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_direction) object->SetDirection(ToDirection(data.direction));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_first_point) object->SetPoint(ToPoint(data.first_point));
  if (data.has_second_point) object->SetPoint2(ToPoint(data.second_point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  if ((description != nullptr && description[0] != '\0') || (descriptionName != nullptr && descriptionName[0] != '\0'))
    object->AddDescription(MakePmiString(description), MakePmiString(descriptionName));
  XCAFDoc_Dimension::Set(label)->SetObject(object);
}

void SetToleranceObject(
  const TDF_Label& label, const OcctSharp_PmiTolerance& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName)
{
  ValidateArray(modifiers, modifierCount, "The tolerance modifier array is invalid.");
  if (!std::isfinite(data.value) || !std::isfinite(data.zone_modifier_value) || !std::isfinite(data.maximum_value_modifier))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance values must be finite.");
  auto object = new XCAFDimTolObjects_GeomToleranceObject();
  object->SetType(static_cast<XCAFDimTolObjects_GeomToleranceType>(data.type));
  object->SetTypeOfValue(static_cast<XCAFDimTolObjects_GeomToleranceTypeValue>(data.type_of_value));
  object->SetValue(data.value);
  object->SetMaterialRequirementModifier(static_cast<XCAFDimTolObjects_GeomToleranceMatReqModif>(data.material_requirement));
  object->SetZoneModifier(static_cast<XCAFDimTolObjects_GeomToleranceZoneModif>(data.zone_modifier));
  object->SetValueOfZoneModifier(data.zone_modifier_value);
  object->SetMaxValueModifier(data.maximum_value_modifier);
  NCollection_Sequence<XCAFDimTolObjects_GeomToleranceModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_GeomToleranceModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_axis) object->SetAxis(ToAxis(data.axis));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_point) object->SetPoint(ToPoint(data.point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  if (data.affected_plane_type != 0)
    object->SetAffectedPlane(ToPlane(data.affected_plane),
      static_cast<XCAFDimTolObjects_ToleranceZoneAffectedPlane>(data.affected_plane_type));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  XCAFDoc_GeomTolerance::Set(label)->SetObject(object);
}

void SetDatumObject(
  const TDF_Label& label, const OcctSharp_PmiDatum& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName)
{
  ValidateArray(modifiers, modifierCount, "The datum modifier array is invalid.");
  auto object = new XCAFDimTolObjects_DatumObject();
  object->SetName(MakePmiString(name));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPosition(data.position);
  object->IsDatumTarget(data.is_datum_target != 0);
  object->SetDatumTargetType(static_cast<XCAFDimTolObjects_DatumTargetType>(data.target_type));
  object->SetDatumTargetLength(data.target_length);
  object->SetDatumTargetWidth(data.target_width);
  object->SetDatumTargetNumber(data.target_number);
  NCollection_Sequence<XCAFDimTolObjects_DatumSingleModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_DatumSingleModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_modifier_with_value)
    object->SetModifierWithValue(static_cast<XCAFDimTolObjects_DatumModifWithValue>(data.modifier_with_value), data.modifier_value);
  if (data.has_target_axis) object->SetDatumTargetAxis(ToAxis(data.target_axis));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_point) object->SetPoint(ToPoint(data.point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  auto attribute = XCAFDoc_Datum::Set(label, MakePmiString(name), MakePmiString(description), MakePmiString(identification));
  attribute->SetObject(object);
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_count(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, int32_t* count)
{
  if (count == nullptr) { SetLastError("The PMI count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&] { *count = static_cast<int32_t>(PmiLabels(document, kind).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_entry(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const int32_t index,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const std::vector<TDF_Label> labels = PmiLabels(document, kind);
    if (index < 1 || index > static_cast<int32_t>(labels.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI index is outside the valid 1-based range.");
    CopyLabelEntry(labels[static_cast<size_t>(index - 1)], buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDimension* data,
  const double* values, const int32_t valueCount, const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The dimension data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddDimension();
    SetDimensionObject(label, *data, values, valueCount, modifiers, modifierCount,
      semanticName, presentationName, description, descriptionName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDimension* data,
  const double* values, const int32_t valueCount, const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName)
{
  if (data == nullptr) { SetLastError("The dimension data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsDimension(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a semantic dimension.");
    const auto previous = GetDimensionObject(document, entry);
    SetDimensionObject(label, *data, values, valueCount, modifiers, modifierCount,
      semanticName, presentationName, description, descriptionName);
    const auto updated = GetDimensionObject(document, entry);
    updated->SetPath(previous->GetPath());
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_Dimension::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The tolerance data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddGeomTolerance();
    SetToleranceObject(label, *data, modifiers, modifierCount, semanticName, presentationName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName)
{
  if (data == nullptr) { SetLastError("The tolerance data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsGeomTolerance(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a geometric tolerance.");
    const auto previous = GetToleranceObject(document, entry);
    SetToleranceObject(label, *data, modifiers, modifierCount, semanticName, presentationName);
    const auto updated = GetToleranceObject(document, entry);
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_GeomTolerance::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The datum data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddDatum();
    SetDatumObject(label, *data, modifiers, modifierCount, name, description, identification, semanticName, presentationName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName)
{
  if (data == nullptr) { SetLastError("The datum data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsDatum(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a datum.");
    const auto previous = GetDatumObject(document, entry);
    SetDatumObject(label, *data, modifiers, modifierCount, name, description, identification, semanticName, presentationName);
    const auto updated = GetDatumObject(document, entry);
    updated->SetDatumTarget(previous->GetDatumTarget());
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_Datum::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiDimension* data, int32_t* valueCount, int32_t* modifierCount)
{
  if (data == nullptr || valueCount == nullptr || modifierCount == nullptr)
  { SetLastError("A dimension snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *valueCount = 0; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetDimensionObject(document, entry);
    data->type = static_cast<int32_t>(object->GetType());
    data->has_qualifier = object->HasQualifier() ? 1 : 0;
    if (data->has_qualifier) data->qualifier = static_cast<int32_t>(object->GetQualifier());
    data->has_angular_qualifier = object->HasAngularQualifier() ? 1 : 0;
    if (data->has_angular_qualifier) data->angular_qualifier = static_cast<int32_t>(object->GetAngularQualifier());
    bool isHole = false;
    XCAFDimTolObjects_DimensionFormVariance variance;
    XCAFDimTolObjects_DimensionGrade grade;
    data->has_class_of_tolerance = object->GetClassOfTolerance(isHole, variance, grade) ? 1 : 0;
    data->is_hole = isHole ? 1 : 0;
    data->form_variance = static_cast<int32_t>(variance);
    data->grade = static_cast<int32_t>(grade);
    object->GetNbOfDecimalPlaces(data->left_decimal_places, data->right_decimal_places);
    gp_Dir direction;
    data->has_direction = object->GetDirection(direction) ? 1 : 0;
    if (data->has_direction) data->direction = { direction.X(), direction.Y(), direction.Z() };
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_first_point = object->HasPoint() ? 1 : 0;
    if (data->has_first_point) data->first_point = CopyPoint(object->GetPoint());
    data->has_second_point = object->HasPoint2() ? 1 : 0;
    if (data->has_second_point) data->second_point = CopyPoint(object->GetPoint2());
    data->has_text_point = 1;
    data->text_point = CopyPoint(object->GetPointTextAttach());
    const auto values = object->GetValues();
    *valueCount = values.IsNull() ? 0 : values->Length();
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiTolerance* data, int32_t* modifierCount)
{
  if (data == nullptr || modifierCount == nullptr)
  { SetLastError("A tolerance snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetToleranceObject(document, entry);
    data->type = static_cast<int32_t>(object->GetType());
    data->type_of_value = static_cast<int32_t>(object->GetTypeOfValue());
    data->value = object->GetValue();
    data->material_requirement = static_cast<int32_t>(object->GetMaterialRequirementModifier());
    data->zone_modifier = static_cast<int32_t>(object->GetZoneModifier());
    data->zone_modifier_value = object->GetValueOfZoneModifier();
    data->maximum_value_modifier = object->GetMaxValueModifier();
    data->has_axis = object->HasAxis() ? 1 : 0;
    if (data->has_axis) data->axis = CopyAxis(object->GetAxis());
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_point = object->HasPoint() ? 1 : 0;
    if (data->has_point) data->point = CopyPoint(object->GetPoint());
    data->has_text_point = object->HasPointText() ? 1 : 0;
    if (data->has_text_point) data->text_point = CopyPoint(object->GetPointTextAttach());
    data->affected_plane_type = static_cast<int32_t>(object->GetAffectedPlaneType());
    if (object->HasAffectedPlane()) data->affected_plane = CopyPlane(object->GetAffectedPlane());
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiDatum* data, int32_t* modifierCount)
{
  if (data == nullptr || modifierCount == nullptr)
  { SetLastError("A datum snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetDatumObject(document, entry);
    data->position = object->GetPosition();
    data->is_datum_target = object->IsDatumTarget() ? 1 : 0;
    data->target_type = static_cast<int32_t>(object->GetDatumTargetType());
    data->target_length = object->GetDatumTargetLength();
    data->target_width = object->GetDatumTargetWidth();
    data->target_number = object->GetDatumTargetNumber();
    data->has_target_axis = object->HasDatumTargetParams() ? 1 : 0;
    if (data->has_target_axis) data->target_axis = CopyAxis(object->GetDatumTargetAxis());
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_point = object->HasPoint() ? 1 : 0;
    if (data->has_point) data->point = CopyPoint(object->GetPoint());
    data->has_text_point = object->HasPointText() ? 1 : 0;
    if (data->has_text_point) data->text_point = CopyPoint(object->GetPointTextAttach());
    XCAFDimTolObjects_DatumModifWithValue modifier;
    double modifierValue = 0.0;
    object->GetModifierWithValue(modifier, modifierValue);
    data->modifier_with_value = static_cast<int32_t>(modifier);
    data->modifier_value = modifierValue;
    data->has_modifier_with_value = std::abs(modifierValue) > 0.0 ? 1 : 0;
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_numeric_item(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, const int32_t index, double* realValue, int32_t* integerValue)
{
  if (realValue == nullptr || integerValue == nullptr)
  { SetLastError("A PMI numeric-item output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *realValue = 0.0; *integerValue = 0;
  return Guard([&]
  {
    if (index < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "PMI numeric item indices are 1-based.");
    if (kind == 0 && field == 0)
    {
      const auto values = GetDimensionObject(document, entry)->GetValues();
      if (values.IsNull() || index > values->Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension value index is out of range.");
      *realValue = values->Value(index); return;
    }
    if (kind == 0 && field == 1)
    {
      const auto values = GetDimensionObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    if (kind == 1 && field == 0)
    {
      const auto values = GetToleranceObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    if (kind == 2 && field == 0)
    {
      const auto values = GetDatumObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Datum modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI numeric field is unsupported.");
  });
}

namespace
{
std::string PmiText(const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry, const int32_t field)
{
  if (kind == 0)
  {
    const auto object = GetDimensionObject(document, entry);
    if (field == 0) return CopyPmiString(object->GetSemanticName());
    if (field == 1) return CopyPmiString(object->GetPresentationName());
    if (field == 2) return object->HasDescriptions() ? CopyPmiString(object->GetDescription(0)) : std::string();
    if (field == 3) return object->HasDescriptions() ? CopyPmiString(object->GetDescriptionName(0)) : std::string();
  }
  else if (kind == 1)
  {
    const auto object = GetToleranceObject(document, entry);
    if (field == 0) return CopyPmiString(object->GetSemanticName());
    if (field == 1) return CopyPmiString(object->GetPresentationName());
  }
  else if (kind == 2)
  {
    opencascade::handle<XCAFDoc_Datum> attribute;
    ResolveOcafLabel(document, entry).FindAttribute(XCAFDoc_Datum::GetID(), attribute);
    const auto object = GetDatumObject(document, entry);
    if (field == 0) return CopyPmiString(attribute->GetName());
    if (field == 1) return CopyPmiString(attribute->GetDescription());
    if (field == 2) return CopyPmiString(attribute->GetIdentification());
    if (field == 3) return CopyPmiString(object->GetSemanticName());
    if (field == 4) return CopyPmiString(object->GetPresentationName());
  }
  else if (kind == 3)
  {
    const auto object = GetSavedViewObject(document, entry);
    if (field == 0) return CopyPmiString(object->Name());
    if (field == 1) return CopyPmiString(object->ClippingExpression());
  }
  throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI text field is unsupported.");
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, int32_t* length)
{
  if (length == nullptr) { SetLastError("The PMI text-length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *length = 0;
  return Guard([&] { *length = static_cast<int32_t>(PmiText(document, kind, entry, field).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&] { CopyUtf8Result(PmiText(document, kind, entry, field), buffer, capacity, written); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_aux_shape(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t role, const OcctSharp_ShapeHandle* shape, const char* name)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document); ValidateUsableShape(shape);
    if (kind == 0)
    {
      const auto object = GetDimensionObject(document, entry);
      if (role == 0) object->SetPath(TopoDS::Edge(shape->Value));
      else if (role == 1) object->SetPresentation(shape->Value, MakePmiString(name));
      else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The dimension shape role is unsupported.");
      XCAFDoc_Dimension::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 1 && role == 1)
    {
      const auto object = GetToleranceObject(document, entry);
      object->SetPresentation(shape->Value, MakePmiString(name));
      XCAFDoc_GeomTolerance::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 2)
    {
      const auto object = GetDatumObject(document, entry);
      if (role == 0) object->SetDatumTarget(shape->Value);
      else if (role == 1) object->SetPresentation(shape->Value, MakePmiString(name));
      else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The datum shape role is unsupported.");
      XCAFDoc_Datum::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_clear_aux_shape(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry, const int32_t role)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    if (kind == 0)
    {
      const auto object = GetDimensionObject(document, entry);
      if (role == 0) object->SetPath(TopoDS_Edge());
      else object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_Dimension::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 1)
    {
      const auto object = GetToleranceObject(document, entry);
      object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_GeomTolerance::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 2)
    {
      const auto object = GetDatumObject(document, entry);
      if (role == 0) object->SetDatumTarget(TopoDS_Shape());
      else object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_Datum::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_get_aux_shape(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t role, int32_t* hasShape, OcctSharp_ShapeHandle** shape)
{
  if (hasShape == nullptr || shape == nullptr) { SetLastError("A PMI shape output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *hasShape = 0; *shape = nullptr;
  return Guard([&]
  {
    TopoDS_Shape value;
    if (kind == 0) value = role == 0 ? GetDimensionObject(document, entry)->GetPath() : GetDimensionObject(document, entry)->GetPresentation();
    else if (kind == 1) value = GetToleranceObject(document, entry)->GetPresentation();
    else if (kind == 2) value = role == 0 ? GetDatumObject(document, entry)->GetDatumTarget() : GetDatumObject(document, entry)->GetPresentation();
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
    if (!value.IsNull()) { *hasShape = 1; *shape = AllocateShape(value); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_references(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const char* firstEntries, const char* secondEntries)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label item = ResolveOcafLabel(document, entry);
    const NCollection_Sequence<TDF_Label> first = ResolveEntries(document, firstEntries);
    const NCollection_Sequence<TDF_Label> second = ResolveEntries(document, secondEntries);
    if (kind == 0) GetDimTolTool(document)->SetDimension(first, second, item);
    else if (kind == 1) GetDimTolTool(document)->SetGeomTolerance(first, item);
    else if (kind == 2) GetDimTolTool(document)->SetDatum(first, item);
    else if (kind == 3)
    {
      opencascade::handle<XCAFDoc_GraphNode> toleranceNode;
      if (item.FindAttribute(XCAFDoc::DatumTolRefGUID(), toleranceNode))
      {
        while (toleranceNode->NbChildren() > 0)
          toleranceNode->UnSetChild(1);
      }
      item.ForgetAttribute(XCAFDoc::DatumTolRefGUID());
      for (NCollection_Sequence<TDF_Label>::Iterator iterator(first); iterator.More(); iterator.Next())
        GetDimTolTool(document)->SetDatumToGeomTol(iterator.Value(), item);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference kind is unsupported.");
  });
}

namespace
{
NCollection_Sequence<TDF_Label> ReferenceLabels(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry)
{
  NCollection_Sequence<TDF_Label> first;
  NCollection_Sequence<TDF_Label> second;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (relation == 0 || relation == 1 || relation == 2 || relation == 4)
  {
    XCAFDoc_DimTolTool::GetRefShapeLabel(label, first, second);
    if (relation == 1) return second;
    return first;
  }
  if (relation == 3) XCAFDoc_DimTolTool::GetDatumOfTolerLabels(label, first);
  else if (relation == 5) GetDimTolTool(document)->GetTolerOfDatumLabels(label, first);
  else if (relation == 6)
  {
    const auto viewTool = GetViewTool(document);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefShapeLabel(label, first);
  }
  else if (relation == 7)
  {
    const auto viewTool = GetViewTool(document);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefGDTLabel(label, first);
  }
  else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference relation is unsupported.");
  return first;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_count(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry, int32_t* count)
{
  if (count == nullptr) { SetLastError("The PMI reference count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&] { *count = ReferenceLabels(document, relation, entry).Size(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_entry(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry,
  const int32_t index, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const NCollection_Sequence<TDF_Label> labels = ReferenceLabels(document, relation, entry);
    if (index < 1 || index > labels.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference index is out of range.");
    CopyLabelEntry(labels.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_remove(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    const bool valid = kind == 0 ? GetDimTolTool(document)->IsDimension(label)
      : kind == 1 ? GetDimTolTool(document)->IsGeomTolerance(label)
      : kind == 2 ? GetDimTolTool(document)->IsDatum(label) : false;
    if (!valid) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not the requested PMI kind.");
    label.ForgetAllAttributes(true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, const int32_t plane_count,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr)
  { SetLastError("The saved-view data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = viewTool->AddView();
    SetSavedViewObject(label, *data, name, clipping_expression);
    const NCollection_Sequence<TDF_Label> shapes = ResolveEntries(document, shape_entries);
    const NCollection_Sequence<TDF_Label> pmi = ResolveEntries(document, pmi_entries);
    const NCollection_Sequence<TDF_Label> clippingPlanes = AddSavedViewPlanes(document, planes, plane_count);
    viewTool->SetView(shapes, pmi, clippingPlanes, label);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, const int32_t plane_count)
{
  if (data == nullptr)
  { SetLastError("The saved-view data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    NCollection_Sequence<TDF_Label> previousPlanes;
    viewTool->GetRefClippingPlaneLabel(label, previousPlanes);
    SetSavedViewObject(label, *data, name, clipping_expression);
    const NCollection_Sequence<TDF_Label> shapes = ResolveEntries(document, shape_entries);
    const NCollection_Sequence<TDF_Label> pmi = ResolveEntries(document, pmi_entries);
    const NCollection_Sequence<TDF_Label> clippingPlanes = AddSavedViewPlanes(document, planes, plane_count);
    viewTool->SetView(shapes, pmi, clippingPlanes, label);
    RemoveUnreferencedPlanes(document, previousPlanes);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_SavedView* data, int32_t* plane_count)
{
  if (data == nullptr || plane_count == nullptr)
  { SetLastError("A saved-view snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *plane_count = 0;
  return Guard([&]
  {
    const auto object = GetSavedViewObject(document, entry);
    data->projection_type = static_cast<int32_t>(object->Type());
    data->projection_point = CopyPoint(object->ProjectionPoint());
    const gp_Dir viewDirection = object->ViewDirection();
    data->view_direction = { viewDirection.X(), viewDirection.Y(), viewDirection.Z() };
    const gp_Dir upDirection = object->UpDirection();
    data->up_direction = { upDirection.X(), upDirection.Y(), upDirection.Z() };
    data->zoom_factor = object->ZoomFactor();
    data->window_horizontal_size = object->WindowHorizontalSize();
    data->window_vertical_size = object->WindowVerticalSize();
    data->has_front_clipping = object->HasFrontPlaneClipping() ? 1 : 0;
    if (data->has_front_clipping) data->front_clipping_distance = object->FrontPlaneDistance();
    data->has_back_clipping = object->HasBackPlaneClipping() ? 1 : 0;
    if (data->has_back_clipping) data->back_clipping_distance = object->BackPlaneDistance();
    data->has_view_volume_sides_clipping = object->HasViewVolumeSidesClipping() ? 1 : 0;
    NCollection_Sequence<TDF_Label> clippingPlanes;
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefClippingPlaneLabel(label, clippingPlanes);
    *plane_count = clippingPlanes.Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_plane(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  const int32_t index, OcctSharp_PlaneEquation* plane)
{
  if (plane == nullptr)
  { SetLastError("The saved-view plane output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *plane = {};
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> clippingPlanes;
    if (!GetViewTool(document)->GetRefClippingPlaneLabel(ResolveOcafLabel(document, entry), clippingPlanes)
        || index < 1 || index > clippingPlanes.Size())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The saved-view clipping-plane index is outside the valid 1-based range.");
    gp_Pln value;
    opencascade::handle<TCollection_HAsciiString> nameValue;
    bool capping = false;
    if (!GetClippingPlaneTool(document)->GetClippingPlane(clippingPlanes.Value(index), value, nameValue, capping))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The saved-view clipping plane could not be read.");
    value.Coefficients(plane->a, plane->b, plane->c, plane->d);
    plane->capping = capping ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_remove(
  OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    NCollection_Sequence<TDF_Label> clippingPlanes;
    viewTool->GetRefClippingPlaneLabel(label, clippingPlanes);
    viewTool->RemoveView(label);
    RemoveUnreferencedPlanes(document, clippingPlanes);
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_visual_material(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const char* name, const int32_t name_length,
  const double red, const double green, const double blue, const double alpha,
  const double metallic, const double roughness,
  const double emissive_red, const double emissive_green, const double emissive_blue,
  const double refraction_index, const int32_t alpha_mode, const double alpha_cutoff)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateUtf8Input(name, name_length);
    const double values[] = {
      red, green, blue, alpha, metallic, roughness,
      emissive_red, emissive_green, emissive_blue, refraction_index, alpha_cutoff };
    for (const double value : values)
      if (!std::isfinite(value))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A visual-material value is not finite.");
    const auto in_unit = [](const double value) { return value >= 0.0 && value <= 1.0; };
    if (!in_unit(red) || !in_unit(green) || !in_unit(blue) || !in_unit(alpha)
        || !in_unit(metallic) || !in_unit(roughness)
        || !in_unit(emissive_red) || !in_unit(emissive_green) || !in_unit(emissive_blue)
        || !in_unit(alpha_cutoff))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Visual-material colors, factors, and cutoff must be in [0,1].");
    if (refraction_index < 1.0 || refraction_index > 3.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Visual-material refraction index must be in [1,3].");
    if (alpha_mode < static_cast<int32_t>(Graphic3d_AlphaMode_BlendAuto)
        || alpha_mode > static_cast<int32_t>(Graphic3d_AlphaMode_MaskBlend))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The visual-material alpha mode is invalid.");

    occ::handle<XCAFDoc_VisMaterial> material = new XCAFDoc_VisMaterial();
    XCAFDoc_VisMaterialPBR pbr;
    pbr.BaseColor = Quantity_ColorRGBA(
      static_cast<float>(red), static_cast<float>(green),
      static_cast<float>(blue), static_cast<float>(alpha));
    pbr.Metallic = static_cast<float>(metallic);
    pbr.Roughness = static_cast<float>(roughness);
    pbr.EmissiveFactor = NCollection_Vec3<float>(
      static_cast<float>(emissive_red), static_cast<float>(emissive_green),
      static_cast<float>(emissive_blue));
    pbr.RefractionIndex = static_cast<float>(refraction_index);
    material->SetPbrMaterial(pbr);
    material->SetAlphaMode(
      static_cast<Graphic3d_AlphaMode>(alpha_mode), static_cast<float>(alpha_cutoff));
    const TCollection_AsciiString material_name = MakeAsciiString(name, name_length);
    material->SetRawName(new TCollection_HAsciiString(material_name));
    const occ::handle<XCAFDoc_VisMaterialTool> tool =
      XCAFDoc_DocumentTool::VisMaterialTool(document->Document->Main());
    const TDF_Label material_label = tool->AddMaterial(material, material_name);
    tool->SetShapeMaterial(ResolveOcafLabel(document, entry), material_label);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material,
  double* red, double* green, double* blue, double* alpha,
  double* metallic, double* roughness,
  double* emissive_red, double* emissive_green, double* emissive_blue,
  double* refraction_index, int32_t* alpha_mode, double* alpha_cutoff)
{
  if (has_material == nullptr || red == nullptr || green == nullptr || blue == nullptr
      || alpha == nullptr || metallic == nullptr || roughness == nullptr
      || emissive_red == nullptr || emissive_green == nullptr || emissive_blue == nullptr
      || refraction_index == nullptr || alpha_mode == nullptr || alpha_cutoff == nullptr)
  {
    SetLastError("A visual-material output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *red = *green = *blue = *alpha = 0.0;
  *metallic = *roughness = 0.0;
  *emissive_red = *emissive_green = *emissive_blue = 0.0;
  *refraction_index = 0.0;
  *alpha_mode = 0;
  *alpha_cutoff = 0.0;
  return Guard([&]
  {
    const occ::handle<XCAFDoc_VisMaterial> material =
      XCAFDoc_VisMaterialTool::GetShapeMaterial(ResolveOcafLabel(document, entry));
    if (material.IsNull()) return;
    XCAFDoc_VisMaterialPBR pbr = material->HasPbrMaterial()
      ? material->PbrMaterial() : material->ConvertToPbrMaterial();
    if (!pbr.IsDefined) return;
    const Quantity_ColorRGBA base = pbr.BaseColor;
    *has_material = 1;
    *red = base.GetRGB().Red();
    *green = base.GetRGB().Green();
    *blue = base.GetRGB().Blue();
    *alpha = base.Alpha();
    *metallic = pbr.Metallic;
    *roughness = pbr.Roughness;
    *emissive_red = pbr.EmissiveFactor.r();
    *emissive_green = pbr.EmissiveFactor.g();
    *emissive_blue = pbr.EmissiveFactor.b();
    *refraction_index = pbr.RefractionIndex;
    *alpha_mode = static_cast<int32_t>(material->AlphaMode());
    *alpha_cutoff = material->AlphaCutOff();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material, int32_t* length)
{
  if (has_material == nullptr || length == nullptr)
  {
    SetLastError("A visual-material name output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *length = 0;
  return Guard([&]
  {
    TDF_Label material_label;
    if (!XCAFDoc_VisMaterialTool::GetShapeMaterial(
          ResolveOcafLabel(document, entry), material_label)) return;
    *has_material = 1;
    opencascade::handle<TDataStd_Name> name_attribute;
    if (material_label.FindAttribute(TDataStd_Name::GetID(), name_attribute))
      *length = name_attribute->Get().LengthOfCString();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    TDF_Label material_label;
    if (!XCAFDoc_VisMaterialTool::GetShapeMaterial(
          ResolveOcafLabel(document, entry), material_label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label has no visual material.");
    opencascade::handle<TDataStd_Name> name_attribute;
    const std::string name = material_label.FindAttribute(TDataStd_Name::GetID(), name_attribute)
      ? ExtendedToUtf8(name_attribute->Get()) : std::string();
    CopyUtf8Result(name, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_validation_properties(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_XdeValidationProperties* properties)
{
  if (properties == nullptr)
  {
    SetLastError("The XDE validation-properties output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *properties = {};
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    gp_Pnt centroid;
    properties->has_area = XCAFDoc_Area::Get(label, properties->area) ? 1 : 0;
    properties->has_volume = XCAFDoc_Volume::Get(label, properties->volume) ? 1 : 0;
    properties->has_centroid = XCAFDoc_Centroid::Get(label, centroid) ? 1 : 0;
    if (properties->has_centroid != 0)
      properties->centroid = {centroid.X(), centroid.Y(), centroid.Z()};
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_validation_properties(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_XdeValidationProperties* properties)
{
  if (properties == nullptr)
  {
    SetLastError("The XDE validation-properties input pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(properties->has_area) || !is_flag(properties->has_volume)
        || !is_flag(properties->has_centroid))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE validation-property presence flag is not Boolean.");
    if ((properties->has_area != 0 && (!std::isfinite(properties->area) || properties->area < 0.0))
        || (properties->has_volume != 0 && (!std::isfinite(properties->volume) || properties->volume < 0.0)))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE area and volume must be finite and non-negative.");
    if (properties->has_centroid != 0
        && (!std::isfinite(properties->centroid.x)
            || !std::isfinite(properties->centroid.y)
            || !std::isfinite(properties->centroid.z)))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE centroid must be finite.");

    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (properties->has_area != 0) XCAFDoc_Area::Set(label, properties->area);
    else label.ForgetAttribute(XCAFDoc_Area::GetID());
    if (properties->has_volume != 0) XCAFDoc_Volume::Set(label, properties->volume);
    else label.ForgetAttribute(XCAFDoc_Volume::GetID());
    if (properties->has_centroid != 0)
      XCAFDoc_Centroid::Set(
        label,
        gp_Pnt(properties->centroid.x, properties->centroid.y, properties->centroid.z));
    else label.ForgetAttribute(XCAFDoc_Centroid::GetID());
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

opencascade::handle<AIS_ColoredShape> FindPresentation(
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

int64_t FindPresentationId(
  const OcctSharp_ViewerHandle* viewer,
  const opencascade::handle<AIS_InteractiveObject>& presentation)
{
  for (const auto& candidate : viewer->Presentations)
  {
    if (candidate.second == presentation) return candidate.first;
  }
  throw OperationFailure(
    OCCTSHARP_STATUS_OCCT_FAILURE,
    "The detected AIS object is outside the managed presentation registry.");
}

opencascade::handle<Graphic3d_ClipPlane> FindClipPlane(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t clipPlaneId)
{
  const auto iterator = viewer->ClipPlanes.find(clipPlaneId);
  if (iterator == viewer->ClipPlanes.end())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer clip-plane ID does not exist.");
  }
  return iterator->second;
}

opencascade::handle<PrsDim_Dimension> FindDimension(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t dimensionId)
{
  const auto iterator = viewer->Dimensions.find(dimensionId);
  if (iterator == viewer->Dimensions.end())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer dimension ID does not exist.");
  return iterator->second;
}

void ConfigureDimension(
  const opencascade::handle<PrsDim_Dimension>& dimension,
  const char* modelUnits,
  const char* displayUnits,
  const int32_t hasCustomValue,
  const double customValue,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double lineWidth)
{
  if ((hasCustomValue != 0 && hasCustomValue != 1) || !std::isfinite(customValue)
      || !std::isfinite(flyout) || !std::isfinite(lineWidth) || lineWidth <= 0.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer dimension style values are invalid.");
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer dimension RGB values are invalid.");
  dimension->SetModelUnits(TCollection_AsciiString(modelUnits == nullptr ? "" : modelUnits));
  dimension->SetDisplayUnits(TCollection_AsciiString(displayUnits == nullptr ? "" : displayUnits));
  if (hasCustomValue != 0) dimension->SetCustomValue(customValue);
  else dimension->SetComputedValue();
  dimension->SetFlyout(flyout);
  dimension->SetColor(Quantity_Color(red, green, blue, Quantity_TOC_RGB));
  dimension->SetWidth(lineWidth);
  dimension->SetToUpdate();
}

bool IsFinite(const OcctSharp_Xyz& value)
{
  return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
}

void ValidateColor(const double red, const double green, const double blue)
{
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Viewer RGB components must be finite values in the inclusive range 0 to 1.");
  }
}

void ValidateSubshape(
  const opencascade::handle<AIS_ColoredShape>& presentation,
  const OcctSharp_ShapeHandle* subshape)
{
  ValidateUsableShape(subshape);
  const TopoDS_Shape root = presentation->Shape();
  bool contains = root.IsSame(subshape->Value);
  if (!contains)
  {
    for (TopExp_Explorer explorer(root, subshape->Value.ShapeType()); explorer.More(); explorer.Next())
    {
      if (explorer.Current().IsSame(subshape->Value))
      {
        contains = true;
        break;
      }
    }
  }
  if (!contains)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "The supplied topology is not a member of the presentation shape.");
  }
}

Aspect_TypeOfTriedronPosition ToTrihedronPosition(const int32_t position)
{
  switch (position)
  {
    case 0: return Aspect_TOTP_CENTER;
    case 1: return Aspect_TOTP_TOP;
    case 2: return Aspect_TOTP_BOTTOM;
    case 4: return Aspect_TOTP_LEFT;
    case 5: return Aspect_TOTP_LEFT_UPPER;
    case 6: return Aspect_TOTP_LEFT_LOWER;
    case 8: return Aspect_TOTP_RIGHT;
    case 9: return Aspect_TOTP_RIGHT_UPPER;
    case 10: return Aspect_TOTP_RIGHT_LOWER;
    default:
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The trihedron position is invalid.");
  }
}

V3d_TypeOfOrientation ToViewerProjection(const int32_t projection)
{
  switch (projection)
  {
    case 0: return V3d_TypeOfOrientation_Zup_Front;
    case 1: return V3d_TypeOfOrientation_Zup_Back;
    case 2: return V3d_TypeOfOrientation_Zup_Top;
    case 3: return V3d_TypeOfOrientation_Zup_Bottom;
    case 4: return V3d_TypeOfOrientation_Zup_Left;
    case 5: return V3d_TypeOfOrientation_Zup_Right;
    case 6: return V3d_TypeOfOrientation_Zup_AxoRight;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer projection is outside the supported range.");
  }
}

AIS_SelectionScheme ToSelectionScheme(const int32_t selectionMode)
{
  switch (selectionMode)
  {
    case 0: return AIS_SelectionScheme_Replace;
    case 1: return AIS_SelectionScheme_Add;
    case 2: return AIS_SelectionScheme_Remove;
    case 3: return AIS_SelectionScheme_XOR;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selection mode is outside the supported range.");
  }
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
    opencascade::handle<AIS_ColoredShape> presentation = new AIS_ColoredShape(shape->Value);
    const int64_t id = viewer->NextPresentationId++;
    viewer->Presentations.emplace(id, presentation);
    viewer->Context->Display(presentation, false);
    *presentation_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_create(
  OcctSharp_ViewerHandle* viewer,
  const int32_t kind,
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_Xyz* points,
  const int32_t point_count,
  const OcctSharp_PlaneEquation* plane,
  const char* model_units,
  const char* display_units,
  const int32_t has_custom_value,
  const double custom_value,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double line_width,
  int64_t* dimension_id)
{
  if (dimension_id == nullptr)
  { SetLastError("The viewer dimension ID output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *dimension_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    opencascade::handle<PrsDim_Dimension> dimension;
    if (kind == 0)
    {
      if (points == nullptr || point_count != 2 || plane == nullptr
          || !IsFinite(points[0]) || !IsFinite(points[1]))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A length dimension requires two finite points and one plane.");
      dimension = new PrsDim_LengthDimension(
        gp_Pnt(points[0].x, points[0].y, points[0].z),
        gp_Pnt(points[1].x, points[1].y, points[1].z),
        gp_Pln(plane->a, plane->b, plane->c, plane->d));
    }
    else if (kind == 1)
    {
      if (points == nullptr || point_count != 3
          || !IsFinite(points[0]) || !IsFinite(points[1]) || !IsFinite(points[2]))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An angle dimension requires three finite points.");
      dimension = new PrsDim_AngleDimension(
        gp_Pnt(points[0].x, points[0].y, points[0].z),
        gp_Pnt(points[1].x, points[1].y, points[1].z),
        gp_Pnt(points[2].x, points[2].y, points[2].z));
    }
    else if (kind == 2 || kind == 3)
    {
      ValidateUsableShape(shape);
      dimension = kind == 2
        ? opencascade::handle<PrsDim_Dimension>(new PrsDim_RadiusDimension(shape->Value))
        : opencascade::handle<PrsDim_Dimension>(new PrsDim_DiameterDimension(shape->Value));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer dimension kind is unsupported.");
    ConfigureDimension(dimension, model_units, display_units, has_custom_value, custom_value,
      flyout, red, green, blue, line_width);
    const int64_t id = viewer->NextDimensionId++;
    viewer->Dimensions.emplace(id, dimension);
    viewer->Context->Display(dimension, false);
    *dimension_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_update_style(
  OcctSharp_ViewerHandle* viewer,
  const int64_t dimension_id,
  const char* model_units,
  const char* display_units,
  const int32_t has_custom_value,
  const double custom_value,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double line_width)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    ConfigureDimension(dimension, model_units, display_units, has_custom_value, custom_value,
      flyout, red, green, blue, line_width);
    viewer->Context->Redisplay(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_visible(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id, const int32_t visible)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    if (visible != 0) viewer->Context->Display(dimension, false);
    else viewer->Context->Erase(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_selected(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id, const int32_t selected)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    const bool isSelected = viewer->Context->IsSelected(dimension);
    if (selected != 0 && !isSelected) viewer->Context->SetSelected(dimension, false);
    else if (selected == 0 && isSelected) viewer->Context->AddOrRemoveSelected(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_remove(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    viewer->Context->Remove(dimension, false);
    viewer->Dimensions.erase(dimension_id);
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
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    if (visible != 0) viewer->Context->Display(presentation, false);
    else viewer->Context->Erase(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_color(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const double red,
  const double green,
  const double blue)
{
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
  {
    SetLastError("Viewer RGB components must be finite values in the inclusive range 0 to 1.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetColor(presentation, Quantity_Color(red, green, blue, Quantity_TOC_RGB), false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_transparency(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const double transparency)
{
  if (!std::isfinite(transparency) || transparency < 0.0 || transparency > 1.0)
  {
    SetLastError("Viewer transparency must be a finite value in the inclusive range 0 to 1.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetTransparency(presentation, transparency, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_display_mode(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const int32_t display_mode)
{
  if (display_mode < 0 || display_mode > 1)
  {
    SetLastError("Viewer display mode must be wireframe or shaded.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetDisplayMode(presentation, display_mode, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_selection_kind(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id, const int32_t shape_kind)
{
  if (shape_kind < -1 || shape_kind > 7)
  { SetLastError("Viewer selection kind must be whole-object or a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->Deactivate(presentation);
    const int mode = shape_kind < 0
      ? 0
      : AIS_Shape::SelectionMode(static_cast<TopAbs_ShapeEnum>(shape_kind));
    viewer->Context->Activate(presentation, mode, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_presentation(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_projection(
  OcctSharp_ViewerHandle* viewer,
  const int32_t projection)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetProj(ToViewerProjection(projection));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_zoom(
  OcctSharp_ViewerHandle* viewer,
  const double factor)
{
  if (!std::isfinite(factor) || factor <= 0.0)
  {
    SetLastError("Viewer zoom factor must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetZoom(factor, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pan(
  OcctSharp_ViewerHandle* viewer,
  const int32_t delta_x,
  const int32_t delta_y)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Pan(delta_x, delta_y, 1.0, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_start_rotation(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  const double z_rotation_threshold)
{
  if (!std::isfinite(z_rotation_threshold) || z_rotation_threshold < 0.0)
  {
    SetLastError("Viewer Z rotation threshold must be finite and non-negative.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->StartRotation(x, y, z_rotation_threshold);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_rotate(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Rotation(x, y);
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at_mode(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  const int32_t selection_mode,
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
    viewer->Context->SelectDetected(ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_selection(
  OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->ClearSelected(false);
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer, int64_t* presentation_ids,
  OcctSharp_ShapeHandle** shapes, const int32_t capacity, int32_t* written)
{
  if (written == nullptr)
  { SetLastError("The viewer selected-topology count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  if (capacity < 0 || (capacity > 0 && (presentation_ids == nullptr || shapes == nullptr)))
  { SetLastError("The viewer selected-topology buffers or capacity are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const int32_t required = viewer->Context->NbSelected();
    if (capacity < required)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selected-topology buffers are too small.");
    int32_t index = 0;
    try
    {
      for (viewer->Context->InitSelected(); viewer->Context->MoreSelected(); viewer->Context->NextSelected())
      {
        const opencascade::handle<AIS_InteractiveObject> selected = viewer->Context->SelectedInteractive();
        int64_t presentation_id = 0;
        for (const auto& presentation : viewer->Presentations)
        {
          if (presentation.second == selected) { presentation_id = presentation.first; break; }
        }
        if (presentation_id == 0)
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "A selected AIS object is outside the managed presentation registry.");
        if (!viewer->Context->HasSelectedShape())
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The selected AIS owner does not expose topology.");
        TopoDS_Shape selected_shape = viewer->Context->SelectedShape();
        if (selected_shape.IsNull())
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null selected topology shape.");
        presentation_ids[index] = presentation_id;
        shapes[index] = AllocateShape(std::move(selected_shape));
        ++index;
      }
      *written = index;
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(shapes[cleanup]);
      *written = 0;
      throw;
    }
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_detected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer, int64_t* presentation_id, OcctSharp_ShapeHandle** shape)
{
  if (presentation_id == nullptr || shape == nullptr)
  { SetLastError("The viewer detected-topology output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *presentation_id = 0;
  *shape = nullptr;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (!viewer->Context->HasDetected() || !viewer->Context->HasDetectedShape()) return;
    const TopoDS_Shape detected = viewer->Context->DetectedShape();
    if (detected.IsNull()) return;
    *presentation_id = FindPresentationId(viewer, viewer->Context->DetectedInteractive());
    *shape = AllocateShape(detected);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_rectangle(
  OcctSharp_ViewerHandle* viewer, const int32_t min_x, const int32_t min_y,
  const int32_t max_x, const int32_t max_y, const int32_t selection_mode,
  int32_t* selected_count)
{
  if (selected_count == nullptr)
  { SetLastError("The viewer selected-count output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *selected_count = 0;
  if (min_x == max_x || min_y == max_y)
  { SetLastError("A viewer selection rectangle must have non-zero width and height."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->SelectRectangle(
      NCollection_Vec2<int>(std::min(min_x, max_x), std::min(min_y, max_y)),
      NCollection_Vec2<int>(std::max(min_x, max_x), std::max(min_y, max_y)),
      viewer->View, ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_polygon(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_Xy* points, const int32_t point_count,
  const int32_t selection_mode, int32_t* selected_count)
{
  if (selected_count == nullptr)
  { SetLastError("The viewer selected-count output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *selected_count = 0;
  if (points == nullptr || point_count < 3 || point_count > 4096)
  { SetLastError("A viewer selection polygon must contain between 3 and 4096 points."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  for (int32_t index = 0; index < point_count; ++index)
  {
    if (!std::isfinite(points[index].x) || !std::isfinite(points[index].y))
    { SetLastError("Viewer selection polygon coordinates must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    NCollection_Array1<gp_Pnt2d> polyline(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      polyline.SetValue(index + 1, gp_Pnt2d(points[index].x, points[index].y));
    viewer->Context->SelectPolygon(polyline, viewer->View, ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_pixel_tolerance(
  OcctSharp_ViewerHandle* viewer, const int32_t tolerance)
{
  if (tolerance < 0 || tolerance > 100)
  { SetLastError("Viewer pixel tolerance must be from 0 through 100."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->SetPixelTolerance(tolerance);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_shape_filter(
  OcctSharp_ViewerHandle* viewer, const int32_t shape_kind)
{
  if (shape_kind < 0 || shape_kind > 7)
  { SetLastError("Viewer shape filters support TopAbs kinds from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->RemoveFilters();
    viewer->ActiveFilter = new StdSelect_ShapeTypeFilter(static_cast<TopAbs_ShapeEnum>(shape_kind));
    viewer->Context->AddFilter(viewer->ActiveFilter);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_filters(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->RemoveFilters();
    viewer->ActiveFilter.Nullify();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selection_bounds(
  OcctSharp_ViewerHandle* viewer, int32_t* has_bounds, OcctSharp_BoundingBox* bounds)
{
  if (has_bounds == nullptr || bounds == nullptr)
  { SetLastError("The viewer selection-bounds output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_bounds = 0;
  *bounds = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const Bnd_Box box = viewer->Context->BoundingBoxOfSelection(viewer->View);
    if (box.IsVoid()) return;
    box.Get(bounds->min_x, bounds->min_y, bounds->min_z,
            bounds->max_x, bounds->max_y, bounds->max_z);
    *has_bounds = 1;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_selected(
  OcctSharp_ViewerHandle* viewer, const double margin, int32_t* fitted)
{
  if (fitted == nullptr)
  { SetLastError("The viewer fit-selected output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *fitted = 0;
  if (!std::isfinite(margin) || margin < 0.0 || margin >= 1.0)
  { SetLastError("Viewer fit-selected margin must be finite and in the range 0 to less than 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (viewer->Context->NbSelected() == 0) return;
    viewer->Context->FitSelected(viewer->View, margin, true);
    *fitted = 1;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_color(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double red, const double green, const double blue)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateSubshape(presentation, subshape);
    presentation->SetCustomColor(subshape->Value, Quantity_Color(red, green, blue, Quantity_TOC_RGB));
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_transparency(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double transparency)
{
  if (!std::isfinite(transparency) || transparency < 0.0 || transparency > 1.0)
  { SetLastError("Viewer subshape transparency must be from 0 through 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateSubshape(presentation, subshape);
    presentation->SetCustomTransparency(subshape->Value, transparency);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_width(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double width)
{
  if (!std::isfinite(width) || width <= 0.0 || width > 1000.0)
  { SetLastError("Viewer subshape width must be finite, positive, and no greater than 1000."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateSubshape(presentation, subshape);
    presentation->SetCustomWidth(subshape->Value, width);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_subshape_overrides(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateSubshape(presentation, subshape);
    presentation->UnsetCustomAspects(subshape->Value, true);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_all_subshape_overrides(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    presentation->ClearCustomAspects();
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_get_camera(
  OcctSharp_ViewerHandle* viewer, OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr)
  { SetLastError("The viewer camera output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *camera = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Eye(camera->eye.x, camera->eye.y, camera->eye.z);
    viewer->View->At(camera->target.x, camera->target.y, camera->target.z);
    viewer->View->Up(camera->up.x, camera->up.y, camera->up.z);
    viewer->View->Proj(camera->projection.x, camera->projection.y, camera->projection.z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_camera(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr || !IsFinite(camera->eye) || !IsFinite(camera->target)
      || !IsFinite(camera->up) || !IsFinite(camera->projection))
  { SetLastError("Viewer camera values must be non-null and finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const gp_Vec eye_to_target(
      gp_Pnt(camera->eye.x, camera->eye.y, camera->eye.z),
      gp_Pnt(camera->target.x, camera->target.y, camera->target.z));
    const gp_Vec projection(camera->projection.x, camera->projection.y, camera->projection.z);
    const gp_Vec up(camera->up.x, camera->up.y, camera->up.z);
    if (eye_to_target.SquareMagnitude() <= 1.0e-24 || projection.SquareMagnitude() <= 1.0e-24
        || up.SquareMagnitude() <= 1.0e-24)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera directions must be non-zero.");
    const gp_Dir direction(eye_to_target);
    const gp_Dir supplied_projection(projection);
    const gp_Dir supplied_up(up);
    if (std::abs(direction.Dot(supplied_projection)) < 0.999999)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera projection must agree with eye-to-target direction.");
    if (std::abs(direction.Dot(supplied_up)) > 0.999999)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera up direction cannot be parallel to its projection.");
    opencascade::handle<Graphic3d_Camera> updated = new Graphic3d_Camera();
    updated->Copy(viewer->View->Camera());
    updated->SetEyeAndCenter(
      gp_Pnt(camera->eye.x, camera->eye.y, camera->eye.z),
      gp_Pnt(camera->target.x, camera->target.y, camera->target.z));
    updated->SetUp(supplied_up);
    viewer->View->SetCamera(updated);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_screen_to_world(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, OcctSharp_Xyz* point)
{
  if (point == nullptr)
  { SetLastError("The screen-to-world output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *point = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Convert(x, y, point->x, point->y, point->z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_world_to_screen(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_Xyz* point, int32_t* x, int32_t* y)
{
  if (point == nullptr || x == nullptr || y == nullptr || !IsFinite(*point))
  { SetLastError("World-to-screen inputs and outputs must be non-null and finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *x = 0; *y = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Convert(point->x, point->y, point->z, *x, *y);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pick_ray(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, OcctSharp_ViewerPickRay* ray)
{
  if (ray == nullptr)
  { SetLastError("The viewer pick-ray output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *ray = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->ConvertWithProj(x, y, ray->origin.x, ray->origin.y, ray->origin.z,
                                  ray->direction.x, ray->direction.y, ray->direction.z);
    const gp_Dir direction(ray->direction.x, ray->direction.y, ray->direction.z);
    ray->direction = { direction.X(), direction.Y(), direction.Z() };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_window_fit(
  OcctSharp_ViewerHandle* viewer, const int32_t min_x, const int32_t min_y,
  const int32_t max_x, const int32_t max_y)
{
  if (min_x == max_x || min_y == max_y)
  { SetLastError("A viewer zoom rectangle must have non-zero width and height."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->WindowFit(std::min(min_x, max_x), std::min(min_y, max_y),
                            std::max(min_x, max_x), std::max(min_y, max_y));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_background_color(
  OcctSharp_ViewerHandle* viewer, const double red, const double green, const double blue)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    viewer->View->SetBackgroundColor(Quantity_TOC_RGB, red, green, blue);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create_clip_plane(
  OcctSharp_ViewerHandle* viewer, const double a, const double b, const double c, const double d,
  int64_t* clip_plane_id)
{
  if (clip_plane_id == nullptr)
  { SetLastError("The clip-plane ID output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *clip_plane_id = 0;
  if (!std::isfinite(a) || !std::isfinite(b) || !std::isfinite(c) || !std::isfinite(d)
      || a * a + b * b + c * c <= 1.0e-24)
  { SetLastError("A clip plane requires finite coefficients and a non-zero normal."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto plane = new Graphic3d_ClipPlane(gp_Pln(a, b, c, d));
    const int64_t id = viewer->NextClipPlaneId++;
    viewer->ClipPlanes.emplace(id, plane);
    viewer->View->AddClipPlane(plane);
    *clip_plane_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_update_clip_plane(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id,
  const double a, const double b, const double c, const double d)
{
  if (!std::isfinite(a) || !std::isfinite(b) || !std::isfinite(c) || !std::isfinite(d)
      || a * a + b * b + c * c <= 1.0e-24)
  { SetLastError("A clip plane requires finite coefficients and a non-zero normal."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    FindClipPlane(viewer, clip_plane_id)->SetEquation(gp_Pln(a, b, c, d));
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_clip_plane_enabled(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id, const int32_t enabled)
{
  if (enabled != 0 && enabled != 1)
  { SetLastError("Clip-plane enabled state must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    FindClipPlane(viewer, clip_plane_id)->SetOn(enabled != 0);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_clip_plane(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto plane = FindClipPlane(viewer, clip_plane_id);
    viewer->View->RemoveClipPlane(plane);
    viewer->ClipPlanes.erase(clip_plane_id);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_computed_mode(
  OcctSharp_ViewerHandle* viewer, const int32_t enabled)
{
  if (enabled != 0 && enabled != 1)
  { SetLastError("Viewer computed-mode state must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetComputedMode(enabled != 0);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_show_trihedron(
  OcctSharp_ViewerHandle* viewer, const int32_t position,
  const double red, const double green, const double blue, const double scale)
{
  if (!std::isfinite(scale) || scale <= 0.0 || scale > 1.0)
  { SetLastError("Viewer trihedron scale must be finite, positive, and no greater than 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    viewer->View->TriedronDisplay(ToTrihedronPosition(position),
      Quantity_Color(red, green, blue, Quantity_TOC_RGB), scale, V3d_WIREFRAME);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_hide_trihedron(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->TriedronErase();
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dump(
  OcctSharp_ViewerHandle* viewer, const char* file_path, const int32_t buffer_type)
{
  if (file_path == nullptr || file_path[0] == '\0' || buffer_type < 0 || buffer_type > 2)
  { SetLastError("Viewer screenshot path or buffer type is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (!viewer->View->Dump(file_path, static_cast<Graphic3d_BufferType>(buffer_type)))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT failed to write the viewer screenshot.");
  });
}

void OCCTSHARP_CALL occtsharp_viewer_release(OcctSharp_ViewerHandle* viewer)
{
  if (viewer != nullptr && UnregisterValue(viewer, LiveViewers))
  {
    if (!viewer->Context.IsNull()) viewer->Context->RemoveFilters();
    if (!viewer->View.IsNull())
      for (const auto& plane : viewer->ClipPlanes) viewer->View->RemoveClipPlane(plane.second);
    if (!viewer->Context.IsNull()) viewer->Context->RemoveAll(false);
    viewer->ActiveFilter.Nullify();
    viewer->ClipPlanes.clear();
    viewer->Presentations.clear();
    viewer->Dimensions.clear();
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
