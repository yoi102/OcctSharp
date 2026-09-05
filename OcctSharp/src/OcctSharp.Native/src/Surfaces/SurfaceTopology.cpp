#include "SurfaceCommon.hxx"
#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepOffsetAPI_NormalProjection.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepFeat_SplitShape.hxx>
#include <BRepLib.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepTools_WireExplorer.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <Geom_Plane.hxx>
#include <Geom_CylindricalSurface.hxx>
#include <Geom_SphericalSurface.hxx>
#include <Geom_ConicalSurface.hxx>
#include <Geom_ToroidalSurface.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_List.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <ShapeBuild_Edge.hxx>
#include <Geom_Curve.hxx>

using namespace OcctSharp::SurfaceBridge;
namespace {
// The generic copier loses pcurves whose supporting face is not in the copied shape.
// Copy those representations explicitly. Surface supports are immutable; topology and
// mutable curve geometry are private to the result, including standalone edge inputs.
TopoDS_Shape CopyTopology(const TopoDS_Shape& source) {
  BRepBuilderAPI_Copy copier(source, false, false);
  BRep_Builder builder; ShapeBuild_Edge edgeBuilder;
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> edges;
  TopExp::MapShapes(source, TopAbs_EDGE, edges);
  for (const auto& item : edges) {
    const auto from = TopoDS::Edge(item);
    const auto to = TopoDS::Edge(copier.ModifiedShape(item));
    edgeBuilder.CopyPCurves(to, from);
    TopLoc_Location location; double first, last;
    auto curve = BRep_Tool::Curve(from, location, first, last);
    if (!curve.IsNull()) builder.UpdateEdge(to,
      opencascade::handle<Geom_Curve>::DownCast(curve->Copy()), location, BRep_Tool::Tolerance(from));
  }
  return copier.Shape();
}
struct State {
  int valid = 0, edges = 0, missing = 0, inconsistent = 0;
  double tolerance = 0;
  explicit State(const TopoDS_Shape& shape) {
    valid = BRepCheck_Analyzer(shape).IsValid();
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> map;
    TopExp::MapShapes(shape, TopAbs_EDGE, map); edges = map.Extent();
    for (const auto& item : map) {
      auto edge = TopoDS::Edge(item); double first, last;
      if (!BRep_Tool::Degenerated(edge) && BRep_Tool::Curve(edge, first, last).IsNull()) ++missing;
      if (!BRep_Tool::SameParameter(edge) || !BRep_Tool::SameRange(edge) || !BRepLib::CheckSameRange(edge)) ++inconsistent;
      tolerance = std::max(tolerance, BRep_Tool::Tolerance(edge));
    }
    for (TopExp_Explorer vertex(shape, TopAbs_VERTEX); vertex.More(); vertex.Next())
      tolerance = std::max(tolerance, BRep_Tool::Tolerance(TopoDS::Vertex(vertex.Current())));
  }
};
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_lift_curve(
  const OcctSharp_ShapeHandle* handle, const OcctSharp_SketchCurve* input,
  int32_t build_3d, double tolerance, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(handle);
    Require(input && (build_3d == 0 || build_3d == 1), "Invalid UV lifting input.");
    auto curve = SketchCurve(*input);
    BRepBuilderAPI_MakeEdge builder(curve, data.surface, curve->FirstParameter(), curve->LastParameter());
    Require(builder.IsDone(), "The UV curve could not be lifted to an edge.");
    auto edge = builder.Edge();
    if (build_3d) {
      Require(BRepLib::BuildCurve3d(edge, tolerance), "The lifted 3D curve could not be built.");
      BRepLib::SameParameter(edge, tolerance);
    }
    if (input->reversed) edge.Reverse();
    Result(edge.Moved(data.location), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_project_shape(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* source_handle,
  const OcctSharp_SurfaceProjectionOptions* options, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    FaceData data(face_handle); auto source = Shape(source_handle);
    Require(options, "The projection options are null.");
    Tolerance(options->tolerance_3d); Tolerance(options->tolerance_2d);
    Require(source.ShapeType() == TopAbs_EDGE || source.ShapeType() == TopAbs_WIRE, "Normal projection requires an edge or wire.");
    Require(std::isfinite(options->maximum_distance) && options->maximum_degree >= 2 && options->maximum_degree <= 25
      && options->maximum_segments > 0 && options->maximum_segments <= 10000
      && (options->limit_to_face == 0 || options->limit_to_face == 1)
      && options->continuity >= 0 && options->continuity <= 2, "Invalid normal projection controls.");
    auto source_copy = CopyTopology(source), face_copy = CopyTopology(data.face);
    BRepOffsetAPI_NormalProjection projector(face_copy);
    projector.Add(source_copy);
    projector.SetParams(options->tolerance_3d, options->tolerance_2d,
      options->continuity == 0 ? GeomAbs_C0 : options->continuity == 1 ? GeomAbs_C1 : GeomAbs_C2,
      options->maximum_degree, options->maximum_segments);
    projector.SetMaxDistance(options->maximum_distance); projector.SetLimit(options->limit_to_face != 0);
    projector.Compute3d(true); projector.Build();
    Require(projector.IsDone(), "Normal projection failed.");
    Result(projector.Projection(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_make_wire(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* const* edges,
  int32_t count, double tolerance, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(face_handle);
    Require(edges && count > 0 && count <= 100000, "A surface wire requires edges.");
    NCollection_List<TopoDS_Shape> copies; BRep_Builder topology;
    for (int index = 0; index < count; ++index) {
      auto edge = TopoDS::Edge(TypedShape(edges[index], TopAbs_EDGE));
      double first, last; auto pcurve = BRep_Tool::CurveOnSurface(edge, data.face, first, last);
      Require(!pcurve.IsNull(), "An input edge has no pcurve on the supplied face.");
      auto copied = CopyTopology(edge);
      for (TopExp_Explorer vertex(copied, TopAbs_VERTEX); vertex.More(); vertex.Next())
        topology.UpdateVertex(TopoDS::Vertex(vertex.Current()), tolerance);
      copies.Append(copied);
    }
    BRepBuilderAPI_MakeWire builder; builder.Add(copies);
    Require(builder.IsDone(), "The surface edges do not form a connected wire.");
    State built(builder.Wire());
    Require(built.edges == count, "Some surface edges were disconnected or duplicated.");
    Result(builder.Wire(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_make_face(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* const* wires,
  int32_t count, double tolerance, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(face_handle);
    Require(wires && count > 0 && count <= 10000, "A trimmed face requires an outer wire and optional holes.");
    std::vector<TopoDS_Wire> local;
    for (int index = 0; index < count; ++index) {
      auto wire = TypedShape(wires[index], TopAbs_WIRE);
      local.push_back(TopoDS::Wire(CopyTopology(wire).Moved(data.location.Inverted())));
    }
    BRepBuilderAPI_MakeFace builder(data.surface, local[0], true);
    for (int index = 1; index < count; ++index) builder.Add(local[index]);
    Require(builder.IsDone(), "Surface trimming failed.");
    auto face = builder.Face().Moved(data.location);
    if (data.face.Orientation() == TopAbs_REVERSED) face.Reverse();
    Require(BRepCheck_Analyzer(face).IsValid(), "The UV loops produced an invalid trimmed face.");
    Result(face, output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_repair(
  const OcctSharp_ShapeHandle* handle, int32_t perform, double tolerance, double maximum_tolerance,
  OcctSharp_SurfaceRepairInfo* info, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); Tolerance(maximum_tolerance);
    Require(info && output && (perform == 0 || perform == 1) && maximum_tolerance >= tolerance, "Invalid surface repair controls.");
    auto source = Shape(handle); State before(source);
    auto result = CopyTopology(source);
    if (perform) {
      Require(BRepLib::BuildCurves3d(result, tolerance), "Missing 3D curve reconstruction failed.");
      BRepLib::SameParameter(result, tolerance, true);
    }
    State after(result);
    Require(after.tolerance <= std::max(maximum_tolerance, before.tolerance), "Surface repair exceeds the allowed tolerance growth.");
    *info = {before.valid, after.valid, before.edges, after.edges, before.missing, after.missing,
      before.inconsistent, after.inconsistent, before.tolerance, after.tolerance};
    Result(result, output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_boundary(
  const OcctSharp_ShapeHandle* handle, OcctSharp_SurfaceBoundaryInfo* info,
  OcctSharp_ShapeHandle** edges, int32_t capacity, int32_t* count)
{
  if (count) *count = 0;
  return Invoke([&] {
    FaceData data(handle); Require(count && capacity >= 0, "Invalid boundary count buffer.");
    std::vector<OcctSharp_SurfaceBoundaryInfo> records; std::vector<TopoDS_Edge> values;
    auto outer = BRepTools::OuterWire(data.face); int loop = 0;
    for (TopExp_Explorer item(data.face, TopAbs_WIRE); item.More(); item.Next(), ++loop) {
      auto wire = TopoDS::Wire(item.Current());
      for (BRepTools_WireExplorer edge(wire, data.face); edge.More(); edge.Next()) {
        auto current = edge.Current(); GProp_GProps properties; BRepGProp::LinearProperties(current, properties);
        records.push_back({loop, wire.IsSame(outer), static_cast<int>(current.Orientation()),
          BRep_Tool::IsClosed(current, data.face), BRep_Tool::Degenerated(current), 0, properties.Mass()});
        values.push_back(current);
      }
    }
    Require(values.size() <= INT32_MAX, "The boundary count exceeds the ABI."); *count = static_cast<int>(values.size());
    if (!info && !edges && capacity == 0) return;
    Require(info && edges && capacity >= *count, "The boundary buffers are too small.");
    std::vector<ShapeOwner> owners; owners.reserve(values.size());
    for (const auto& value : values) owners.emplace_back(OcctSharp_Internal_AllocateShape(value), &occtsharp_shape_release);
    std::copy(records.begin(), records.end(), info);
    for (size_t index = 0; index < owners.size(); ++index) edges[index] = owners[index].release();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_split(
  const OcctSharp_ShapeHandle* handle, const OcctSharp_ShapeHandle* const* tools,
  int32_t count, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    FaceData data(handle); Require(tools && count > 0 && count <= 10000, "Face splitting requires edge or wire tools.");
    auto face = TopoDS::Face(CopyTopology(data.face));
    BRepFeat_SplitShape splitter(face);
    for (int index = 0; index < count; ++index) {
      auto tool = CopyTopology(Shape(tools[index]));
      for (TopExp_Explorer item(tool, TopAbs_EDGE); item.More(); item.Next()) {
        double first, last;
        Require(!BRep_Tool::CurveOnSurface(TopoDS::Edge(item.Current()), face, first, last).IsNull(),
          "Every split edge must have a pcurve on the supporting face.");
      }
      if (tool.ShapeType() == TopAbs_EDGE) splitter.Add(TopoDS::Edge(tool), face);
      else if (tool.ShapeType() == TopAbs_WIRE) splitter.Add(TopoDS::Wire(tool), face);
      else throw Failure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A split tool must be an edge or wire.");
    }
    splitter.Build(); Require(splitter.IsDone(), "Face splitting failed.");
    Require(BRepCheck_Analyzer(splitter.Shape()).IsValid(), "Face splitting produced invalid topology.");
    Result(splitter.Shape(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_create_analytic(
  int32_t kind, const OcctSharp_SketchPlane* plane, double radius, double secondary,
  const double* bounds, double tolerance, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); Require(plane && bounds && kind >= 0 && kind <= 4, "Invalid analytic surface inputs.");
    for (int index = 0; index < 4; ++index) Require(std::isfinite(bounds[index]), "UV bounds must be finite.");
    Require(bounds[0] < bounds[1] && bounds[2] < bounds[3], "UV bounds must increase.");
    const double pi = std::acos(-1.0);
    Require(kind == 0 || bounds[1] - bounds[0] <= 2 * pi + tolerance, "Periodic U bounds cannot exceed one period.");
    Require(kind != 2 || (bounds[2] >= -pi / 2 && bounds[3] <= pi / 2), "Sphere V bounds must stay between the poles.");
    Require(kind != 4 || bounds[3] - bounds[2] <= 2 * pi + tolerance, "Torus V bounds cannot exceed one period.");
    gp_Vec x(Point(plane->x_direction).XYZ()), y(Point(plane->y_direction).XYZ());
    Require(x.Magnitude() > tolerance && y.Magnitude() > tolerance, "Surface frame axes must be nonzero.");
    x.Normalize(); y.Normalize(); Require(std::abs(x.Dot(y)) <= tolerance, "Surface frame axes must be orthogonal.");
    gp_Ax3 frame(Point(plane->origin), gp_Dir(x.Crossed(y)), gp_Dir(x));
    opencascade::handle<Geom_Surface> surface;
    if (kind == 0) surface = new Geom_Plane(frame);
    else {
      Require(std::isfinite(radius) && radius > 0, "The analytic surface radius must be positive and finite.");
      if (kind == 1) surface = new Geom_CylindricalSurface(frame, radius);
      else if (kind == 2) surface = new Geom_SphericalSurface(frame, radius);
      else if (kind == 3) {
        Require(std::isfinite(secondary) && std::abs(secondary) > 1e-9 && std::abs(secondary) < std::acos(-1.0) / 2, "The cone semi-angle is invalid.");
        surface = new Geom_ConicalSurface(frame, secondary, radius);
      } else {
        Require(std::isfinite(secondary) && secondary > 0 && secondary < radius, "The torus minor radius must be smaller than its major radius.");
        surface = new Geom_ToroidalSurface(frame, radius, secondary);
      }
    }
    BRepBuilderAPI_MakeFace builder(surface, bounds[0], bounds[1], bounds[2], bounds[3], tolerance);
    Require(builder.IsDone(), "Analytic face creation failed."); Result(builder.Face(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_section(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  double tolerance, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    Tolerance(tolerance); FaceData a(first), b(second);
    BRepAlgoAPI_Section section(a.face, b.face, false);
    section.SetNonDestructive(true); section.SetFuzzyValue(tolerance);
    section.Approximation(true); section.ComputePCurveOn1(true); section.ComputePCurveOn2(true);
    section.Build(); Require(section.IsDone() && !section.HasErrors(), "Surface section failed."); Result(section.Shape(), output);
  });
}
