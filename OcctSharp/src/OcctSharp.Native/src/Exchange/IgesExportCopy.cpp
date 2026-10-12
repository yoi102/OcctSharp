#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "Exchange/IgesExportCopy.hxx"
#include "Runtime/Error.hxx"
#include "Xde/Document.hxx"
#include <BRepAdaptor_Surface.hxx>
#include <Adaptor3d_Curve.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRepTools_Modifier.hxx>
#include <GProp_GProps.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <ShapeCustom_ConvertToBSpline.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <algorithm>
#include <cmath>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
namespace
{
// IGES can transfer bounded swept basis curves directly. Its unbounded line
// basis loses the finite side face; converting circular/periodic bases here
// can also damage their existing seam pcurves. Select by exact curve type.
class IgesLineExtrusionConversion : public ShapeCustom_ConvertToBSpline
{
public:
  bool NewSurface(const TopoDS_Face& face, occ::handle<Geom_Surface>& surface,
                  TopLoc_Location& location, double& tolerance,
                  bool& reverseWires, bool& reverseFace) override
  {
    BRepAdaptor_Surface adaptor(face, true);
    if (adaptor.GetType() != GeomAbs_SurfaceOfExtrusion
        || adaptor.BasisCurve()->GetType() != GeomAbs_Line) return false;
    return ShapeCustom_ConvertToBSpline::NewSurface(
      face, surface, location, tolerance, reverseWires, reverseFace);
  }
};
}
occ::handle<TDocStd_Document> CreateIgesExportCopy(
  const occ::handle<TDocStd_Document>& source)
{
  auto copy = CreateXdeDocument();
  InitializeXdeTools(copy);
  CloneXdeRootsIntoDocument(source, copy, "IGES export");
  const auto tool = XCAFDoc_DocumentTool::ShapeTool(copy->Main());
  NCollection_Sequence<TDF_Label> labels;
  tool->GetShapes(labels);
  for (const auto& label : labels)
  {
    if (!OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_IsSimpleShape(label) || OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_IsReference(label))
      continue;
    const auto original = OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(label);
    // IGESCAF silently omits linear-extrusion surfaces in surface transfer
    // mode. Convert their exact finite representations in this private copy.
    occ::handle<ShapeCustom_ConvertToBSpline> conversion = new IgesLineExtrusionConversion();
    conversion->SetExtrusionMode(true);
    conversion->SetRevolutionMode(false);
    conversion->SetOffsetMode(false);
    conversion->SetPlaneMode(false);
    BRepTools_Modifier modifier(false);
    modifier.Init(original);
    modifier.Perform(conversion);
    if (!modifier.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "IGES extrusion conversion failed.");
    const auto result = modifier.ModifiedShape(original);
    if (result.IsNull() || !BRepCheck_Analyzer(result).IsValid())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "IGES extrusion conversion produced invalid topology.");
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> beforeFaces, afterFaces, resultShapes, used;
    TopExp::MapShapes(original, TopAbs_FACE, beforeFaces);
    TopExp::MapShapes(result, TopAbs_FACE, afterFaces);
    TopExp::MapShapes(result, resultShapes);
    GProp_GProps beforeArea, afterArea;
    BRepGProp::SurfaceProperties(original, beforeArea);
    BRepGProp::SurfaceProperties(result, afterArea);
    const auto sourceArea = std::abs(beforeArea.Mass());
    if (beforeFaces.Size() != afterFaces.Size() || !std::isfinite(sourceArea)
        || !std::isfinite(afterArea.Mass())
        || std::abs(sourceArea - std::abs(afterArea.Mass())) > std::max(1e-5, sourceArea * 1e-6))
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "IGES extrusion conversion changed face count or surface area.");
    NCollection_Sequence<TDF_Label> subshapes;
    OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetSubShapes(label, subshapes);
    std::vector<std::pair<TDF_Label, TopoDS_Shape>> mapped;
    for (const auto& subshape : subshapes)
    {
      const auto before = OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(subshape);
      const auto after = modifier.ModifiedShape(before);
      if (after.IsNull() || before.ShapeType() != after.ShapeType()
          || !resultShapes.Contains(after) || used.Contains(after))
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "IGES conversion has no unique subshape metadata correspondence.");
      used.Add(after);
      mapped.emplace_back(subshape, after);
    }
    OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_SetShape_Bind(*(tool))(label, result);
    for (const auto& mapping : mapped) OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_SetShape_Bind(*(tool))(mapping.first, mapping.second);
  }
  tool->UpdateAssemblies();
  return copy;
}
}
