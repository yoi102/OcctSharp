// Numerical inspection over exact knot spans; all partitioned topology stays call-local.
#include "Geometry/Conversions.hxx"
#include "Modeling/Inspection.hxx"
#include "Runtime/Error.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRepGProp_Domain.hxx>
#include <BRepGProp_Face.hxx>
#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <BRep_Tool.hxx>
#include <Geom2dAdaptor_Curve.hxx>
#include <Geom2dAPI_InterCurveCurve.hxx>
#include <Geom2d_Line.hxx>
#include <Geom2d_TrimmedCurve.hxx>
#include <Geom2d_OffsetCurve.hxx>
#include <Geom2d_BSplineCurve.hxx>
#include <GeomAdaptor_Curve.hxx>
#include <GeomAdaptor_Surface.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_OffsetCurve.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Geom_OffsetSurface.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_SurfaceOfLinearExtrusion.hxx>
#include <Geom_SurfaceOfRevolution.hxx>
#include <GProp_GProps.hxx>
#include <ShapeExtend.hxx>
#include <ShapeUpgrade_FaceDivide.hxx>
#include <ShapeUpgrade_ShapeDivide.hxx>
#include <ShapeUpgrade_SplitCurve2d.hxx>
#include <ShapeUpgrade_SplitCurve3d.hxx>
#include <ShapeUpgrade_SplitSurface.hxx>
#include <ShapeUpgrade_WireDivide.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <cmath>
#include <algorithm>
#include <array>
#include <functional>
#include <initializer_list>
#include <limits>
#include <sstream>
#include <string>
#include <vector>

namespace OcctSharp::Native
{
namespace
{
void AppendKnot(double knot,double first,double last,bool periodic,double period,std::vector<double>& values)
{
  if(!periodic){if(knot>first&&knot<last)values.push_back(knot);return;}
  const double lower=std::ceil((first-knot)/period),upper=std::floor((last-knot)/period);
  if(!std::isfinite(lower)||!std::isfinite(upper)||upper-lower>1000000)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Moment periodic knot span exceeds its resource bound.");
  const int count=static_cast<int>(std::max(0.0,upper-lower+1));
  for(int index=0;index<count;++index){double value=knot+(lower+index)*period;if(value>first&&value<last)values.push_back(value);}
}
void AppendBasisKnots(const occ::handle<Geom_Curve>& curve,double first,double last,std::vector<double>& values,int depth=0)
{
  if(depth>64)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Moment curve basis exceeds64levels.");
  if(curve.IsNull())return;
  if(const auto offset=occ::handle<Geom_OffsetCurve>::DownCast(curve);!offset.IsNull())
    return AppendBasisKnots(offset->BasisCurve(),first,last,values,depth+1);
  if(const auto trim=occ::handle<Geom_TrimmedCurve>::DownCast(curve);!trim.IsNull())
    return AppendBasisKnots(trim->BasisCurve(),first,last,values,depth+1);
  if(const auto spline=occ::handle<Geom_BSplineCurve>::DownCast(curve);!spline.IsNull())
    for(int i=1;i<=spline->NbKnots();++i)AppendKnot(spline->Knot(i),first,last,spline->IsPeriodic(),spline->IsPeriodic()?spline->Period():0,values);
}
void AppendBasisKnots(const occ::handle<Geom2d_Curve>& curve,double first,double last,std::vector<double>& values,int depth=0)
{
  if(depth>64)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Moment pcurve basis exceeds64levels.");
  if(curve.IsNull())return;
  if(const auto offset=occ::handle<Geom2d_OffsetCurve>::DownCast(curve);!offset.IsNull())
    return AppendBasisKnots(offset->BasisCurve(),first,last,values,depth+1);
  if(const auto trim=occ::handle<Geom2d_TrimmedCurve>::DownCast(curve);!trim.IsNull())
    return AppendBasisKnots(trim->BasisCurve(),first,last,values,depth+1);
  if(const auto spline=occ::handle<Geom2d_BSplineCurve>::DownCast(curve);!spline.IsNull())
    for(int i=1;i<=spline->NbKnots();++i)AppendKnot(spline->Knot(i),first,last,spline->IsPeriodic(),spline->IsPeriodic()?spline->Period():0,values);
}
void AppendSupportKnots(const occ::handle<Geom_Surface>& surface,double umin,double umax,double vmin,double vmax,
  std::vector<double>& u,std::vector<double>& v,int depth=0)
{
  if(depth>64)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Moment support basis exceeds64levels.");
  if(surface.IsNull())return;
  if(const auto offset=occ::handle<Geom_OffsetSurface>::DownCast(surface);!offset.IsNull())
    return AppendSupportKnots(offset->BasisSurface(),umin,umax,vmin,vmax,u,v,depth+1);
  if(const auto trim=occ::handle<Geom_RectangularTrimmedSurface>::DownCast(surface);!trim.IsNull())
    return AppendSupportKnots(trim->BasisSurface(),umin,umax,vmin,vmax,u,v,depth+1);
  if(const auto extrusion=occ::handle<Geom_SurfaceOfLinearExtrusion>::DownCast(surface);!extrusion.IsNull())
    AppendBasisKnots(extrusion->BasisCurve(),umin,umax,u);
  if(const auto revolution=occ::handle<Geom_SurfaceOfRevolution>::DownCast(surface);!revolution.IsNull())
    AppendBasisKnots(revolution->BasisCurve(),vmin,vmax,v);
}
// Continuity dividers may remove knots within tolerance. In the pinned SDK the
// 2D divider then retains stale knot indices and can access freed knot storage.
// These call-local tools only add the exact CN interval boundaries; they never
// ask the continuity repair tools to simplify an authored or imported basis.
template<class Adaptor>
occ::handle<NCollection_HSequence<double>> CurveSpanValues(const Adaptor& curve)
{
  NCollection_Array1<double> values(1, curve.NbIntervals(GeomAbs_CN) + 1);
  curve.Intervals(values, GeomAbs_CN);
  occ::handle<NCollection_HSequence<double>> result = new NCollection_HSequence<double>;
  for (const double value : values) result->Append(value);
  return result;
}

class ExactCurve2dSpans final : public ShapeUpgrade_SplitCurve2d
{
  void Compute() override
  {
    const Geom2dAdaptor_Curve curve(myCurve, mySplitValues->First(), mySplitValues->Last());
    SetSplitValues(CurveSpanValues(curve));
    myNbCurves = mySplitValues->Length() - 1;
    myStatus = ShapeExtend::EncodeStatus(myNbCurves > 1 ? ShapeExtend_DONE1 : ShapeExtend_OK);
  }
};

class ExactCurve3dSpans final : public ShapeUpgrade_SplitCurve3d
{
  void Compute() override
  {
    const GeomAdaptor_Curve curve(myCurve, mySplitValues->First(), mySplitValues->Last());
    SetSplitValues(CurveSpanValues(curve));
    myNbCurves = mySplitValues->Length() - 1;
    myStatus = ShapeExtend::EncodeStatus(myNbCurves > 1 ? ShapeExtend_DONE1 : ShapeExtend_OK);
  }
};

class ExactSurfaceSpans final : public ShapeUpgrade_SplitSurface
{
  void Compute(const bool) override
  {
    const GeomAdaptor_Surface surface(mySurface, myUSplitValues->First(), myUSplitValues->Last(),
      myVSplitValues->First(), myVSplitValues->Last());
    NCollection_Array1<double> u(1, surface.NbUIntervals(GeomAbs_CN) + 1);
    NCollection_Array1<double> v(1, surface.NbVIntervals(GeomAbs_CN) + 1);
    surface.UIntervals(u, GeomAbs_CN);
    surface.VIntervals(v, GeomAbs_CN);
    occ::handle<NCollection_HSequence<double>> us = new NCollection_HSequence<double>;
    occ::handle<NCollection_HSequence<double>> vs = new NCollection_HSequence<double>;
    for (const double value : u) us->Append(value);
    for (const double value : v) vs->Append(value);
    SetUSplitValues(us);
    SetVSplitValues(vs);
    myNbResultingRow = myUSplitValues->Length() - 1;
    myNbResultingCol = myVSplitValues->Length() - 1;
    myStatus = ShapeExtend::EncodeStatus(myNbResultingRow > 1 || myNbResultingCol > 1
      ? ShapeExtend_DONE1 : ShapeExtend_OK);
  }
};

bool HasKnotBreaks(const TopoDS_Shape& shape)
{
  for (TopExp_Explorer faces(shape, TopAbs_FACE); faces.More(); faces.Next())
  {
    const TopoDS_Face face = TopoDS::Face(faces.Current());
    BRepAdaptor_Surface surface(face, true);
    if (surface.UContinuity() != GeomAbs_CN || surface.VContinuity() != GeomAbs_CN)
      return true;
    for (TopExp_Explorer edges(face, TopAbs_EDGE); edges.More(); edges.Next())
    {
      const TopoDS_Edge edge = TopoDS::Edge(edges.Current());
      if (!BRep_Tool::Degenerated(edge))
      {
        BRepAdaptor_Curve curve(edge);
        if (curve.Continuity() != GeomAbs_CN) return true;
      }
      double first = 0, last = 0;
      const auto pcurve = BRep_Tool::CurveOnSurface(edge, face, first, last);
      if (!pcurve.IsNull() && Geom2dAdaptor_Curve(pcurve, first, last).Continuity() != GeomAbs_CN)
        return true;
    }
  }
  return false;
}

bool RequiresSpanMoments(const TopoDS_Shape& shape)
{
  for (TopExp_Explorer edges(shape, TopAbs_EDGE); edges.More(); edges.Next())
  {
    const TopoDS_Edge edge = TopoDS::Edge(edges.Current());
    if (BRep_Tool::Degenerated(edge)) continue;
    BRepAdaptor_Curve curve(edge);
    const auto kind = curve.GetType();
    // An exact offset of a polynomial basis is generally non-polynomial too.
    // SDK adaptive volume may report tiny convergence error while retaining
    // biased offset-boundary moments; use the controlled exact evaluator.
    if (kind == GeomAbs_OffsetCurve || kind == GeomAbs_Circle || kind == GeomAbs_Ellipse ||
        kind == GeomAbs_Hyperbola || kind == GeomAbs_Parabola ||
        (kind == GeomAbs_BSplineCurve && curve.BSpline()->IsRational()) ||
        (kind == GeomAbs_BezierCurve && curve.Bezier()->IsRational())) return true;
  }
  for (TopExp_Explorer faces(shape, TopAbs_FACE); faces.More(); faces.Next())
  {
    BRepAdaptor_Surface surface(TopoDS::Face(faces.Current()), true);
    if (surface.GetType() == GeomAbs_BSplineSurface)
    {
      const auto spline = surface.BSpline();
      if (spline->IsURational() || spline->IsVRational()) return true;
    }
    if (surface.GetType() == GeomAbs_BezierSurface)
    {
      const auto bezier = surface.Bezier();
      if (bezier->IsURational() || bezier->IsVRational()) return true;
    }
  }
  return false;
}

TopoDS_Shape PartitionKnotSpans(const TopoDS_Shape& shape,
  const bool allowVolumeFallback, bool& needsMoments)
{
  if (!HasKnotBreaks(shape)) return shape;
  BRepBuilderAPI_Copy copy(shape, true, false);
  if (!copy.IsDone())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Inspection could not copy the exact shape.");
  occ::handle<ShapeUpgrade_WireDivide> wire = new ShapeUpgrade_WireDivide;
  wire->SetSplitCurve2dTool(new ExactCurve2dSpans);
  wire->SetSplitCurve3dTool(new ExactCurve3dSpans);
  occ::handle<ShapeUpgrade_FaceDivide> face = new ShapeUpgrade_FaceDivide;
  face->SetWireDivideTool(wire);
  face->SetSplitSurfaceTool(new ExactSurfaceSpans);
  ShapeUpgrade_ShapeDivide divide(copy.Shape());
  divide.SetSplitFaceTool(face);
  divide.Perform();
  if (divide.Status(ShapeExtend_FAIL) || divide.Result().IsNull())
  {
    // In this pinned SDK, Debug's face divider can fail on Boolean faces
    // supported by an extruded degree-one spline. Never integrate its invalid
    // partial result. Moment quadrature integrates the valid original copy with knot spans.
    if (allowVolumeFallback && BRepCheck_Analyzer(copy.Shape()).IsValid())
    {
      needsMoments = true;
      return copy.Shape();
    }
    std::string details;
    for (int status = ShapeExtend_FAIL1; status <= ShapeExtend_FAIL8; ++status)
      if (divide.Status(static_cast<ShapeExtend_Status>(status)))
        details += " " + std::to_string(status - ShapeExtend_FAIL1 + 1);
    if (!divide.Result().IsNull())
      details += std::string("; remaining breaks=") + (HasKnotBreaks(divide.Result()) ? "yes" : "no")
        + "; valid=" + (BRepCheck_Analyzer(divide.Result()).IsValid() ? "yes" : "no");
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE,
      ("Inspection could not partition exact knot spans; failure flags:" + details).c_str());
  }
  return divide.Result();
}

// The SDK's nested GK asks for relative convergence of each signed local
// moment. An exactly/near-zero component can therefore consume 1000 inner
// iterations at every outer sample. Integrate all ten divergence-theorem
// moments together, with a shape-scaled absolute error budget instead.
// Geometry is still evaluated on the exact surfaces and pcurves, never a mesh.
struct Moments
{
  std::array<double, 10> value{};
  std::array<double, 10> error{};
  void Add(const Moments& other, const double weight = 1)
  {
    for (size_t i = 0; i < value.size(); ++i)
    {
      value[i] += weight * other.value[i];
      error[i] += std::abs(weight) * other.error[i];
    }
  }
};

class MomentQuadrature
{
  size_t mySamples = 0;
  size_t myFace = 0, myBoundary = 0, myBoundaries = 0;
  size_t myUSpans = 0, myTSpans = 0;
  size_t myTSpan = 0;
  using Function = std::function<Moments(double)>;
  Moments Rule(const Function& function, const double first, const double last,
    const bool fine, const double tolerance, const int depth, const bool surfaceRule)
  {
    static constexpr double x8[] = { .18343464249564980494, .52553240991632898582,
      .79666647741362673959, .96028985649753623168 };
    static constexpr double w8[] = { .36268378337836198297, .31370664587788728734,
      .22238103445337447054, .10122853629037625915 };
    static constexpr double x16[] = { .09501250983763744019, .28160355077925891323,
      .45801677765722738634, .61787624440264374845, .75540440835500303390,
      .86563120238783174388, .94457502307323257608, .98940093499164993260 };
    static constexpr double w16[] = { .18945061045506849629, .18260341504492358887,
      .16915651939500253819, .14959598881657673208, .12462897125553387205,
      .09515851168249278481, .06225352393864789286, .02715245941175409485 };
    const double middle = first + (last - first) / 2, half = (last - first) / 2;
    const double* xs = fine ? x16 : x8;
    const double* ws = fine ? w16 : w8;
    Moments result;
    std::array<double, 10> absolute{};
    for (int j = 0; j < (fine ? 8 : 4); ++j)
    {
      for (const double sign : {-1., 1.})
      {
        if (++mySamples > 2000000)
        {
          std::ostringstream details;
          details << "Adaptive moment inspection exceeded its exact evaluation budget: face=" << myFace
            << "; boundary=" << myBoundary << '/' << myBoundaries
            << "; uSpans=" << myUSpans << "; tSpan=" << myTSpan << '/' << myTSpans
            << "; surfaceRule=" << surfaceRule << "; interval=" << first << ',' << last
            << "; budget=" << tolerance << "; depth=" << depth;
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE,
            details.str().c_str());
        }
        const Moments sample = function(middle + sign * half * xs[j]);
        result.Add(sample, half * ws[j]);
        for (size_t i = 0; i < absolute.size(); ++i)
          absolute[i] += std::abs(half * ws[j] * sample.value[i]);
      }
    }
    for (size_t i = 0; i < absolute.size(); ++i)
      result.error[i] += 16 * std::numeric_limits<double>::epsilon() * absolute[i];
    return result;
  }
public:
  void BeginFace(const size_t index) { mySamples = 0; myFace = index; }
  void BeginBoundary(const size_t index, const size_t count, const size_t uSpans, const size_t tSpans)
  { mySamples = 0; myBoundary = index; myBoundaries = count; myUSpans = uSpans; myTSpans = tSpans; }
  void BeginSpan(const size_t index) { mySamples = 0; myTSpan = index; }
  Moments Integrate(const Function& function, const double first, const double last,
    const double absoluteTolerance, const int depth = 0, const bool surfaceRule = false)
  {
    if (first == last) return {};
    Moments coarse = Rule(function, first, last, false, absoluteTolerance, depth, surfaceRule);
    Moments fine = Rule(function, first, last, true, absoluteTolerance, depth, surfaceRule);
    bool converged = true;
    for (size_t i = 0; i < fine.value.size(); ++i)
    {
      const double difference = 4 * std::abs(fine.value[i] - coarse.value[i]);
      const double numericalFloor = 4 * (fine.error[i] + coarse.error[i]);
      fine.error[i] += difference;
      if (!std::isfinite(fine.value[i]) || !std::isfinite(fine.error[i]))
        throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE,
          "Adaptive moment inspection produced a nonfinite integral.");
      // Do not subdivide away evaluation/roundoff uncertainty. Preserve that
      // uncertainty in the result and verify the combined global budget.
      converged &= fine.error[i] <= absoluteTolerance || difference <= numericalFloor;
    }
    if (converged) return fine;
    const double middle = first + (last - first) / 2;
    if (depth == 20 || middle == first || middle == last)
    {
      // Local budgets divide repeatedly and can fall below representable
      // parameter/evaluation resolution. Retain the unresolved error instead
      // of declaring a locally tiny contribution fatal. The combined mass and
      // all ten moment errors MUST still satisfy the requested global budget.
      return fine;
    }
    Moments result = Integrate(function, first, middle, absoluteTolerance / 2, depth + 1, surfaceRule);
    result.Add(Integrate(function, middle, last, absoluteTolerance / 2, depth + 1, surfaceRule));
    return result;
  }
};

double ComputeSpanMoments(const TopoDS_Shape& shape, const double relativeTolerance,
  OcctSharp_InspectionProperties& properties)
{
  std::vector<TopoDS_Face> faces;
  for (TopExp_Explorer shells(shape, TopAbs_SHELL); shells.More(); shells.Next())
  {
    if (!BRep_Tool::IsClosed(shells.Current())) continue;
    for (TopExp_Explorer items(shells.Current(), TopAbs_FACE); items.More(); items.Next())
    {
      const auto orientation = items.Current().Orientation();
      if (orientation == TopAbs_FORWARD || orientation == TopAbs_REVERSED)
        faces.push_back(TopoDS::Face(items.Current()));
    }
  }
  if (faces.empty()) { properties = {}; return 0; }
  Bnd_Box box;
  BRepBndLib::AddOptimal(shape, box, false, false);
  if (box.IsVoid() || box.IsOpen())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs finite exact bounds.");
  double xmin, ymin, zmin, xmax, ymax, zmax;
  box.Get(xmin, ymin, zmin, xmax, ymax, zmax);
  gp_Pnt origin(xmin + (xmax - xmin) / 2, ymin + (ymax - ymin) / 2,
    zmin + (zmax - zmin) / 2);
  gp_XYZ vertexSum(0, 0, 0);
  size_t vertexCount = 0;
  for (TopExp_Explorer vertices(shape, TopAbs_VERTEX); vertices.More(); vertices.Next())
  { vertexSum += BRep_Tool::Pnt(TopoDS::Vertex(vertices.Current())).XYZ(); ++vertexCount; }
  // Use the same reference point as SDK adaptive GProp. Tolerance-closed
  // imported boundaries can have a residual flux, making different reference
  // points produce different mass despite converged quadrature. The vertex
  // mean also keeps translated small parts numerically well conditioned.
  if (vertexCount != 0) origin = gp_Pnt(vertexSum / static_cast<double>(vertexCount));
  const double scale = std::max({xmax - xmin, ymax - ymin, zmax - zmin});
  if (!std::isfinite(scale) || scale <= 0)
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs a positive spatial scale.");
  const double volumeScale = scale * scale * scale, inertiaScale = volumeScale * scale * scale;
  GProp_GProps reference;
  BRepGProp::VolumeProperties(shape, reference, 1e-6, true);
  const double massScale = std::abs(reference.Mass() / volumeScale);
  // The rough mass only sizes the budget; all returned properties come from
  // the exact boundary integral. Check its actual mass error below as well.
  const double budget = relativeTolerance * std::max(massScale, 1e-20) / 8;
  MomentQuadrature quadrature;
  Moments total;
  size_t faceIndex = 0;
  for (const TopoDS_Face& face : faces)
  {
    // A face can have hundreds of boundaries, each with hundreds of exact
    // knot spans. Bound refinement within a smooth parameter span, not the
    // accumulated normal work of a detailed imported shape.
    quadrature.BeginFace(faceIndex++);
    BRepGProp_Face surface(true);
    surface.Load(face);
    // Evaluate the exact support in a call-local frame before adding a small
    // parameter-dependent displacement to a possibly million-unit center.
    // Subtracting the origin after world D1 loses those low bits at every
    // quadrature node and can make a smooth span look impossible to converge.
    // Only translate the support copy: its original u/v parametrization,
    // pcurves, knots and finite-domain restrictions remain unchanged. Apply
    // the original location's linear part to points and both derivatives,
    // preserving reflected/scaled locations and the original face orientation.
    TopLoc_Location supportLocation;
    const auto support = BRep_Tool::Surface(face, supportLocation);
    if (support.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs an exact support surface.");
    const gp_Trsf supportTransform = supportLocation.Transformation();
    const gp_Pnt supportOrigin = origin.Transformed(supportTransform.Inverted());
    gp_Trsf shift;
    shift.SetTranslation(gp_Vec(-supportOrigin.X(), -supportOrigin.Y(), -supportOrigin.Z()));
    const auto localSupport = occ::down_cast<Geom_Surface>(support->Transformed(shift));
    const GeomAdaptor_Surface localSurface(localSupport);
    gp_Trsf linearTransform = supportTransform;
    linearTransform.SetTranslationPart(gp_Vec(0, 0, 0));
    double umin, umax, vmin, vmax;
    surface.Bounds(umin, umax, vmin, vmax);
    const BRepAdaptor_Surface basis(face, true);
    NCollection_Array1<double> adaptorU(1, basis.NbUIntervals(GeomAbs_CN) + 1);
    basis.UIntervals(adaptorU, GeomAbs_CN);
    NCollection_Array1<double> adaptorV(1, basis.NbVIntervals(GeomAbs_CN) + 1);
    basis.VIntervals(adaptorV, GeomAbs_CN);
    std::vector<double> basisUKnots(adaptorU.begin(),adaptorU.end()),basisVKnots(adaptorV.begin(),adaptorV.end());
    // Offset adaptors can hide their spline-basis breaks on extrusion/revolution
    // supports. Retain those exact original parameters independently of the
    // SDK's continuity interval classification;never rebuild or fit the support.
    AppendSupportKnots(support,umin,umax,vmin,vmax,basisUKnots,basisVKnots);
    for(auto* values:{&basisUKnots,&basisVKnots}) {
      std::sort(values->begin(),values->end());values->erase(std::unique(values->begin(),values->end()),values->end());
    }
    BRepGProp_Domain domain;
    domain.Init(face);
    std::vector<TopoDS_Edge> edges;
    for (; domain.More(); domain.Next()) edges.push_back(domain.Value());
    const bool natural = face.NbChildren() == 0;
    if (!natural && edges.empty())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs exact face boundaries.");
    const size_t boundaryCount = natural ? 1 : edges.size();
    for (size_t edgeIndex = 0; edgeIndex < boundaryCount; ++edgeIndex)
    {
      if (natural) surface.Load(false, GeomAbs_IsoU);
      else if (!surface.Load(edges[edgeIndex]))
        throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs a usable exact pcurve.");
      const double first = surface.FirstParameter(), last = surface.LastParameter();
      const auto knots = surface.GetTKnots(first, last);
      std::vector<double> boundaryKnots(knots->begin(), knots->end());
      if (!natural)
      {
        double curveFirst = 0, curveLast = 0;
        auto pcurve = BRep_Tool::CurveOnSurface(edges[edgeIndex], face, curveFirst, curveLast);
        if (pcurve.IsNull())
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection needs the original pcurve spans.");
        // BRepGProp_Face::Load reverses both the curve and parameter domain for
        // a reversed trim edge. Match that domain when obtaining its spans.
        if (edges[edgeIndex].Orientation() == TopAbs_REVERSED) pcurve = pcurve->Reversed();
        const Geom2dAdaptor_Curve curve(pcurve, first, last);
        NCollection_Array1<double> exact(1, curve.NbIntervals(GeomAbs_CN) + 1);
        curve.Intervals(exact, GeomAbs_CN);
        // GetTKnots alone omits pcurve knots on analytic support surfaces in
        // this SDK. Add exact adaptor intervals without rebuilding any edges.
        boundaryKnots.insert(boundaryKnots.end(), exact.begin(), exact.end());
        AppendBasisKnots(pcurve,first,last,boundaryKnots);
        const auto crossingParameters = [&](const gp_Pnt2d& point, const gp_Dir2d& direction)
        {
          occ::handle<Geom2d_Curve> trim = new Geom2d_TrimmedCurve(pcurve, first, last, true, false);
          occ::handle<Geom2d_Curve> iso = new Geom2d_Line(point, direction);
          Geom2dAPI_InterCurveCurve crossings(trim, iso, 1e-12);
          if (!crossings.Intersector().IsDone())
            throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection could not locate original domain crossings.");
          for (int pointIndex = 1; pointIndex <= crossings.NbPoints(); ++pointIndex)
          {
            const double parameter = crossings.Intersector().Point(pointIndex).ParamOnFirst();
            if (parameter > first && parameter < last) boundaryKnots.push_back(parameter);
          }
        };
        // Finite-domain restriction is piecewise smooth at these crossings.
        // Integrate the pieces explicitly instead of discovering a tiny clamp
        // interval through recursive quadrature.
        crossingParameters(gp_Pnt2d(umin, 0), gp_Dir2d(0, 1));
        crossingParameters(gp_Pnt2d(umax, 0), gp_Dir2d(0, 1));
        crossingParameters(gp_Pnt2d(0, vmin), gp_Dir2d(1, 0));
        crossingParameters(gp_Pnt2d(0, vmax), gp_Dir2d(1, 0));
        // This SDK's GetTKnots does not actually add V-knot crossings. Locate
        // them on the original pcurve, so a C0 support derivative is never
        // hidden inside an otherwise smooth quadrature interval.
        for (size_t i = 1; i + 1 < basisVKnots.size(); ++i)
          crossingParameters(gp_Pnt2d(0, basisVKnots[i]), gp_Dir2d(1, 0));
      }
      std::sort(boundaryKnots.begin(), boundaryKnots.end());
      boundaryKnots.erase(std::unique(boundaryKnots.begin(), boundaryKnots.end()), boundaryKnots.end());
      quadrature.BeginBoundary(edgeIndex, boundaryCount, basisUKnots.size() - 1, boundaryKnots.size() - 1);
      const double edgeBudget = budget / (faces.size() * boundaryCount);
      const auto boundary = [&](const double t)
      {
        gp_Pnt2d uv;
        gp_Vec2d derivative;
        surface.D12d(t, uv, derivative);
        // Match the SDK's finite support-domain restriction. Imported pcurves
        // can overshoot the support bounds within BRep tolerances; extrapolated
        // support values are not part of its mass-property domain.
        uv.SetX(std::clamp(uv.X(), umin, umax));
        uv.SetY(std::clamp(uv.Y(), vmin, vmax));
        if (derivative.Y() == 0 || uv.X() == umin) return Moments{};
        const auto uknots = surface.GetUKnots(umin, uv.X());
        std::vector<double> innerKnots(uknots->begin(), uknots->end());
        for (const double value : basisUKnots)
          if (value > std::min(umin, uv.X()) && value < std::max(umin, uv.X()))
            innerKnots.push_back(value);
        std::sort(innerKnots.begin(), innerKnots.end());
        innerKnots.erase(std::unique(innerKnots.begin(), innerKnots.end()), innerKnots.end());
        const double innerBudget = edgeBudget * .1 /
          std::max(std::abs(derivative.Y() * (last - first)), 1e-30);
        Moments inner;
        const auto integrand = [&](const double u)
        {
          gp_Pnt point;
          gp_Vec du, dv;
          localSurface.D1(u, uv.Y(), point, du, dv);
          point.Transform(linearTransform);
          du.Transform(supportTransform);
          dv.Transform(supportTransform);
          gp_Vec normal = du.Crossed(dv);
          if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
          const double x = point.X() / scale;
          const double y = point.Y() / scale;
          const double z = point.Z() / scale;
          const double flux = (x * normal.X() + y * normal.Y() + z * normal.Z()) / (scale * scale);
          Moments sample;
          sample.value = {flux / 3, x * flux / 4, y * flux / 4, z * flux / 4,
            (y * y + z * z) * flux / 5, (x * x + z * z) * flux / 5,
            (x * x + y * y) * flux / 5, -x * y * flux / 5,
            -x * z * flux / 5, -y * z * flux / 5};
          return sample;
        };
        for (size_t i = 0; i + 1 < innerKnots.size(); ++i)
          inner.Add(quadrature.Integrate(integrand, innerKnots[i], innerKnots[i + 1],
            innerBudget / (innerKnots.size() - 1), 0, true), uv.X() < umin ? -1 : 1);
        Moments result;
        result.Add(inner, derivative.Y());
        return result;
      };
      for (size_t i = 0; i + 1 < boundaryKnots.size(); ++i)
      {
        quadrature.BeginSpan(i);
        total.Add(quadrature.Integrate(boundary, boundaryKnots[i], boundaryKnots[i + 1],
          edgeBudget / (boundaryKnots.size() - 1)));
      }
    }
  }
  const double mass = total.value[0];
  if (!std::isfinite(mass) || mass == 0)
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection did not resolve a nonzero closed volume.");
  const double estimatedError = total.error[0] / std::abs(mass);
  if (estimatedError > relativeTolerance)
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection did not meet the requested mass error.");
  for (const double error : total.error)
    if (error > relativeTolerance * std::abs(mass))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection did not meet the requested normalized moment error.");
  const double cx = total.value[1] / mass, cy = total.value[2] / mass, cz = total.value[3] / mass;
  properties.mass = mass * volumeScale;
  properties.center = {origin.X() + cx * scale, origin.Y() + cy * scale, origin.Z() + cz * scale};
  properties.i11 = (total.value[4] - mass * (cy * cy + cz * cz)) * inertiaScale;
  properties.i22 = (total.value[5] - mass * (cx * cx + cz * cz)) * inertiaScale;
  properties.i33 = (total.value[6] - mass * (cx * cx + cy * cy)) * inertiaScale;
  properties.i12 = properties.i21 = (total.value[7] + mass * cx * cy) * inertiaScale;
  properties.i13 = properties.i31 = (total.value[8] + mass * cx * cz) * inertiaScale;
  properties.i23 = properties.i32 = (total.value[9] + mass * cy * cz) * inertiaScale;
  for (const double value : {properties.mass, properties.center.x, properties.center.y,
       properties.center.z, properties.i11, properties.i22, properties.i33,
       properties.i12, properties.i13, properties.i23})
    if (!std::isfinite(value))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Moment inspection produced nonfinite properties.");
  return estimatedError;
}
}

bool RequiresInspectionSpanPartition(const TopoDS_Shape& shape)
{
  return HasKnotBreaks(shape);
}

double ComputeAdaptiveProperties(const TopoDS_Shape& shape, const int32_t propertyKind,
  const double relativeTolerance, OcctSharp_InspectionProperties& properties)
{
  // Rational/conic boundaries require controlled moment quadrature. Polynomial
  // spans retain the SDK's adaptive Gauss integration.
  bool needsMoments = propertyKind == 2 && RequiresSpanMoments(shape);
  // BRepGProp_Face exposes exact U surface spans and the combined pcurve/V
  // spans. Vector integration can use the ORIGINAL trimmed boundary directly.
  // A topological face divider may rebuild pcurves within its geometric
  // tolerance and is not an accuracy-preserving prerequisite for moments.
  if (propertyKind == 2 && (needsMoments || HasKnotBreaks(shape)))
    return ComputeSpanMoments(shape, relativeTolerance, properties);
  const TopoDS_Shape integrationShape = PartitionKnotSpans(shape, propertyKind == 2, needsMoments);
  GProp_GProps result;
  if (needsMoments) return ComputeSpanMoments(integrationShape, relativeTolerance, properties);
  const double estimatedError = propertyKind == 1
    ? BRepGProp::SurfaceProperties(integrationShape, result, relativeTolerance)
    : BRepGProp::VolumeProperties(integrationShape, result, relativeTolerance, true);
  if (!std::isfinite(estimatedError) || estimatedError < 0 || !std::isfinite(result.Mass()))
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Adaptive inspection did not produce finite properties.");
  properties.mass = result.Mass();
  properties.center = CopyPoint(result.CentreOfMass());
  CopyInertia(result.MatrixOfInertia(), properties);
  for (const double value : {properties.center.x, properties.center.y, properties.center.z,
       properties.i11, properties.i12, properties.i13, properties.i21, properties.i22,
       properties.i23, properties.i31, properties.i32, properties.i33})
    if (!std::isfinite(value))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "Adaptive inspection produced nonfinite centroid or inertia.");
  return estimatedError;
}
}
