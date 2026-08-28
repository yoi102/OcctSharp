using OcctSharp;

string nativeDirectory = Path.Combine(AppContext.BaseDirectory, "occt");
string nativeBridge = Path.Combine(nativeDirectory, "OcctSharp.Native.dll");
string misplacedNativeBridge = Path.Combine(AppContext.BaseDirectory, "OcctSharp.Native.dll");

if (!Directory.Exists(nativeDirectory) || !File.Exists(nativeBridge))
{
    throw new InvalidOperationException(
        $"The packaged native runtime was not copied to '{nativeDirectory}'.");
}

if (File.Exists(misplacedNativeBridge))
{
    throw new InvalidOperationException(
        $"The native bridge must not be copied beside the application at '{misplacedNativeBridge}'.");
}

string[] nativeFiles = Directory.GetFiles(nativeDirectory, "*.dll");
if (nativeFiles.Length < 2)
{
    throw new InvalidOperationException("The complete OCCT native dependency closure was not copied.");
}

OcctRuntimeInfo runtime = OcctRuntime.Info;
if (runtime.AbiVersion != new Version(1, 44)
    || runtime.BridgeVersion != "0.52.0"
    || runtime.OcctVersion != "8.0.1")
{
    throw new InvalidOperationException(
        $"Unexpected packaged runtime: ABI {runtime.AbiVersion}, OCCT {runtime.OcctVersion}.");
}

using Shape box = ShapeFactory.CreateBox(10, 20, 30);
ShapeTopologySummary packagedTopology = box.GetTopologySummary();
DetailedMeshSnapshot packagedDetailedMesh = box.CreateDetailedMesh();
if (box.FaceCount != 6 || !packagedTopology.IsClosed || !packagedTopology.IsValid
    || packagedTopology.UniqueCounts.VertexCount != 8
    || packagedDetailedMesh.TriangleCount == 0 || !packagedDetailedMesh.HasUv)
{
    throw new InvalidOperationException("The packaged common topology/detailed-mesh workflow failed.");
}

using Geom2dCartesianPoint generated2dPoint = new(2, 3);
using Geom2dDirection generated2dDirection = new(3, 4);
using Geom2dTransformation generated2dTransform = new();
using Geom2dVectorWithMagnitude generated2dVector = new(3, 4);
using GeomDirection generated3dDirection = new(2, 3, 6);
using GeomPlane generatedPlane = new(0, 0, 1, 0);
using GeomTransformation generated3dTransform = new();
using GeomVectorWithMagnitude generated3dVector = new(2, 3, 6);
using Geom2dCartesianPoint generated2dPointClone = generated2dPoint.Clone();
generated2dPointClone.SetCoord(5, 7);
generated3dTransform.SetTranslation(new Point3d(1, 2, 3), new Point3d(4, 6, 8));
if (generated2dPoint.ReferenceCount != 2
    || generated2dPoint.X() != 5 || generated2dPoint.Y() != 7
    || Math.Abs(generated2dDirection.Magnitude() - 1) > 1e-12
    || generated2dTransform.ScaleFactor() != 1
    || generated2dVector.Magnitude() != 5
    || Math.Abs(generated3dDirection.Magnitude() - 1) > 1e-12
    || Math.Abs(generatedPlane.EvalD0(2, 3).Z) > 1e-12
    || generated3dTransform.Value(1, 4) != 3
    || generated3dVector.Magnitude() != 7)
{
    throw new InvalidOperationException("The packaged generated Geom/Geom2d shared bindings failed.");
}

using BRepMeshIncrementalMesh generatedMesh = new();
using PolyTriangulationParameters generatedMeshParameters = new(0.25, 0.5, 0.01);
using ShapeAnalysisTransferParameters generatedAnalysis = new();
using ShapeFixRoot generatedFix = new();
using ShapeUpgradeTool generatedUpgrade = new();
generatedFix.SetPrecision(0.01);
generatedUpgrade.SetPrecision(0.02);
if (generatedMesh.TypeName != "BRepMesh_IncrementalMesh"
    || generatedMesh.GetStatusFlags() != 0
    || generatedMeshParameters.Deflection() != 0.25
    || generatedAnalysis.TypeName != "ShapeAnalysis_TransferParameters"
    || generatedFix.Precision() != 0.01
    || generatedUpgrade.Precision() != 0.02)
{
    throw new InvalidOperationException(
        "The packaged generated mesh, analysis, and healing bindings failed.");
}

using StepBasicCoordinatedUniversalTimeOffset packagedOffset = new();
packagedOffset.Init(9, true, 30, StepBasicAheadOrBehind.StepBasic_aobAhead);
using StepBasicCoordinatedUniversalTimeOffset packagedOffsetClone = packagedOffset.Clone();
if (packagedOffset.ReferenceCount != 2
    || packagedOffsetClone.HourOffset() != 9
    || packagedOffsetClone.MinuteOffset() != 30
    || packagedOffsetClone.Sense() != StepBasicAheadOrBehind.StepBasic_aobAhead)
{
    throw new InvalidOperationException("The packaged generated StepBasic shared/enum binding did not preserve state.");
}

using StepBasicAction packagedAction = new();
using StepBasicActionMethod packagedMethod = new();
packagedAction.SetChosenMethod(packagedMethod);
using StepBasicActionMethod packagedReturnedMethod = packagedAction.ChosenMethod()
    ?? throw new InvalidOperationException("The packaged cross-generated shared handle returned null.");
packagedMethod.Dispose();
if (packagedReturnedMethod.TypeName != "StepBasic_ActionMethod")
{
    throw new InvalidOperationException("The packaged cross-generated shared handle was not retained independently.");
}
packagedAction.SetChosenMethod(null);
if (packagedAction.ChosenMethod() is not null)
{
    throw new InvalidOperationException("The packaged cross-generated shared handle did not round-trip null.");
}

Type[] packagedStepBasicTypes = typeof(StepBasicDate).Assembly.GetExportedTypes()
    .Where(static type => type.IsClass
        && type.Name.StartsWith("StepBasic", StringComparison.Ordinal)
        && typeof(IDisposable).IsAssignableFrom(type)
        && type.GetConstructor(Type.EmptyTypes) is not null)
    .OrderBy(static type => type.FullName, StringComparer.Ordinal)
    .ToArray();
if (packagedStepBasicTypes.Length != 129)
{
    throw new InvalidOperationException($"Expected 129 packaged generated StepBasic types, received {packagedStepBasicTypes.Length}.");
}
foreach (Type type in packagedStepBasicTypes)
{
    IDisposable instance = (IDisposable)(Activator.CreateInstance(type)
        ?? throw new InvalidOperationException($"Could not create packaged {type.FullName}."));
    IDisposable? stepClone = null;
    try
    {
        stepClone = (IDisposable)(type.GetMethod("Clone")!.Invoke(instance, null)
            ?? throw new InvalidOperationException($"Could not clone packaged {type.FullName}."));
        if ((int)type.GetProperty("ReferenceCount")!.GetValue(instance)! != 2)
        {
            throw new InvalidOperationException($"Packaged {type.FullName} did not retain its shared clone.");
        }
    }
    finally
    {
        instance.Dispose();
        stepClone?.Dispose();
    }
}

using StepGeomCartesianPoint packagedStepPoint = new();
packagedStepPoint.SetNbCoordinates(3);
using StepReprRepresentationItem packagedRepresentationItem = new();
using StepShapeBoxDomain packagedBoxDomain = new();
packagedBoxDomain.SetXlength(4);
packagedBoxDomain.SetYlength(5);
packagedBoxDomain.SetZlength(6);
using StepVisualColourRgb packagedColour = new();
packagedColour.SetRed(0.25);
packagedColour.SetGreen(0.5);
packagedColour.SetBlue(0.75);
if (packagedStepPoint.NbCoordinates() != 3
    || packagedRepresentationItem.TypeName != "StepRepr_RepresentationItem"
    || packagedBoxDomain.Xlength() != 4
    || packagedBoxDomain.Ylength() != 5
    || packagedBoxDomain.Zlength() != 6
    || packagedColour.Red() != 0.25
    || packagedColour.Green() != 0.5
    || packagedColour.Blue() != 0.75)
{
    throw new InvalidOperationException(
        "The packaged generated STEP geometry, representation, shape, or visual bindings failed.");
}

(string Prefix, int ExpectedCount)[] packagedStepFamilies =
[
    ("IGESAppli", 23),
    ("IGESBasic", 20),
    ("IGESDefs", 11),
    ("IGESDimen", 27),
    ("IGESDraw", 18),
    ("IGESGeom", 27),
    ("IGESGraph", 18),
    ("IGESSolid", 28),
    ("StepAP203", 11),
    ("StepAP214", 27),
    ("StepAP242", 4),
    ("StepDimTol", 50),
    ("StepElement", 21),
    ("StepFEA", 55),
    ("StepGeom", 85),
    ("StepKinematics", 81),
    ("StepRepr", 79),
    ("StepShape", 92),
    ("StepVisual", 110),
];
Type[] packagedTypes = typeof(StepBasicDate).Assembly.GetExportedTypes();
foreach ((string prefix, int expectedCount) in packagedStepFamilies)
{
    int actualCount = packagedTypes.Count(type => type.IsClass
        && type.Name.StartsWith(prefix, StringComparison.Ordinal)
        && typeof(IDisposable).IsAssignableFrom(type)
        && type.GetConstructor(Type.EmptyTypes) is not null);
    if (actualCount != expectedCount)
    {
        throw new InvalidOperationException(
            $"Expected {expectedCount} packaged generated {prefix} types, received {actualCount}.");
    }
}

using Shape packagedSphere = ShapeFactory.CreateSphere(2);
using Shape packagedCylinder = ShapeFactory.CreateCylinder(2, 5);
using Shape packagedCone = ShapeFactory.CreateCone(3, 1, 5);
using Shape packagedTorus = ShapeFactory.CreateTorus(4, 1);
if (packagedSphere.FaceCount != 1 || packagedCylinder.FaceCount != 3
    || !packagedCone.IsValid || !packagedTorus.IsValid
    || Math.Abs(packagedTorus.GetBoundingBox().SizeX - 10) > 1e-5)
{
    throw new InvalidOperationException("The packaged primitive builders did not create expected solids.");
}

using Shape packagedProfileWire = ShapeFactory.CreatePolygonWire(
    [new GpPoint(0, 0, 0), new GpPoint(2, 0, 0), new GpPoint(2, 2, 0), new GpPoint(0, 2, 0)],
    close: true);
using Shape packagedProfileFace = ShapeFactory.CreatePlanarFace(packagedProfileWire);
using GpVec packagedExtrusionVector = GpVec.Create(0, 0, 3);
using Shape packagedPrism = packagedProfileFace.Extrude(packagedExtrusionVector);
if (packagedPrism.Kind != ShapeKind.Solid || packagedPrism.CountSubShapes(ShapeKind.Face) != 6)
{
    throw new InvalidOperationException("The packaged extrusion bridge did not create the expected solid.");
}

Shape[] packagedFaces = box.GetFaces();
if (packagedFaces.Length != 6 || packagedFaces.Any(face => face.FaceCount != 1))
{
    throw new InvalidOperationException("The packaged topology face snapshot was not preserved.");
}

Shape[] packagedEdges = box.GetSubShapes(ShapeKind.Edge);
if (packagedEdges.Length != 24)
{
    throw new InvalidOperationException("The packaged edge snapshot did not preserve box topology.");
}
foreach (Shape edge in packagedEdges) edge.Dispose();

using Shape packagedEdge = ShapeFactory.CreateEdge(GpPoint.Origin, GpPoint.Create(4, 0, 0));
EdgeCurveSnapshot packagedCurve = packagedEdge.GetEdgeCurveSnapshot();
FaceSurfaceSnapshot packagedSurface = packagedFaces[0].GetFaceSurfaceSnapshot();
if (packagedCurve.CurveType != CurveGeometryType.Line
    || packagedCurve.StartPoint != GpPoint.Origin
    || packagedCurve.EndPoint != GpPoint.Create(4, 0, 0)
    || packagedSurface.SurfaceType != SurfaceGeometryType.Plane)
{
    throw new InvalidOperationException("The packaged BRepAdaptor snapshots did not preserve copied geometry values.");
}
foreach (Shape face in packagedFaces) face.Dispose();

using Shape booleanTool = ShapeFactory.CreateBox(2, 2, 2).Transformed(ShapeTransform.CreateTranslationAndRotationZ(1, 1, 1, 0));
using Shape sectionTool = ShapeFactory.CreateBox(2, 2, 2).Transformed(ShapeTransform.CreateTranslationAndRotationZ(9, 1, 1, 0));
using Shape fused = box.Fuse(booleanTool);
using Shape cut = box.Cut(booleanTool);
using Shape section = box.Section(sectionTool);
using Shape filleted = box.Fillet(1);
using Shape chamfered = box.Chamfer(1);
using Shape offsetShape = box.Offset(0.25);
using Shape wedge = ShapeFactory.CreateWedge(6, 5, 4, 2);
if (fused.FaceCount <= 0 || cut.FaceCount <= 0
    || section.CountSubShapes(ShapeKind.Edge) <= 0
    || !filleted.IsValid || !chamfered.IsValid || !offsetShape.IsValid || !wedge.IsValid)
{
    throw new InvalidOperationException("The packaged boolean bridge did not return valid topology.");
}

using Shape circleEdge = ShapeFactory.CreateCircleEdge(GpPoint.Origin, new GpPoint(0, 0, 1), 2);
using Shape bezierEdge = ShapeFactory.CreateBezierEdge(
    [GpPoint.Origin, new GpPoint(1, 2, 0), new GpPoint(3, 0, 0)]);
CurveEvaluation curveEvaluation = bezierEdge.EvaluateEdge(0.5);
CurveProjection curveProjection = circleEdge.ProjectPointOnEdge(new GpPoint(3, 0, 0));
if (circleEdge.GetEdgeLength() <= 0 || !double.IsFinite(curveEvaluation.Point.X)
    || Math.Abs(curveProjection.Distance - 1) > 1e-8)
{
    throw new InvalidOperationException("The packaged curve construction/evaluation profile failed.");
}

Shape[] topologyFaces = box.GetFaces();
try
{
    FaceSurfaceSnapshot surfaceBounds = topologyFaces[0].GetFaceSurfaceSnapshot();
    SurfaceEvaluation surfaceEvaluation = topologyFaces[0].EvaluateFace(
        (surfaceBounds.FirstUParameter + surfaceBounds.LastUParameter) / 2,
        (surfaceBounds.FirstVParameter + surfaceBounds.LastVParameter) / 2);
    SurfaceProjection surfaceProjection = topologyFaces[0].ProjectPointOnFace(surfaceEvaluation.Point);
    using TopologyAdjacencyMap adjacency = box.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);
    using Shape thickSolid = box.MakeThickSolid([topologyFaces[0]], -0.5);
    using Shape sewn = ShapeFactory.Sew(topologyFaces);
    if (surfaceProjection.Distance > 1e-7 || adjacency.Items.Count != 12
        || adjacency.RelationCount != 24 || !thickSolid.IsValid || !sewn.IsValid)
    {
        throw new InvalidOperationException("The packaged surface/topology/thick-solid profile failed.");
    }
}
finally
{
    foreach (Shape face in topologyFaces) face.Dispose();
}

using Shape loftLower = ShapeFactory.CreatePolygonWire(
    [new GpPoint(-1, -1, 0), new GpPoint(1, -1, 0), new GpPoint(1, 1, 0), new GpPoint(-1, 1, 0)], true);
using Shape loftUpper = ShapeFactory.CreatePolygonWire(
    [new GpPoint(-2, -2, 4), new GpPoint(2, -2, 4), new GpPoint(2, 2, 4), new GpPoint(-2, 2, 4)], true);
using Shape loft = ShapeFactory.CreateLoft([loftLower, loftUpper], makeSolid: true);
using Shape pipeSpine = ShapeFactory.CreatePolygonWire([GpPoint.Origin, new GpPoint(0, 0, 4)]);
using Shape pipe = ShapeFactory.CreatePipe(pipeSpine, loftLower);
using BooleanOperationResult history = box.CutWithHistory(booleanTool, ShapeKind.Face);
if (!loft.IsValid || !pipe.IsValid || history.History.Left.SourceCount != 6 || !history.Shape.IsValid)
{
    throw new InvalidOperationException("The packaged loft/pipe/Boolean-history profile failed.");
}

using Shape fixedShape = box.Fixed();
using Shape unifiedShape = box.UnifiedSameDomain();
using Shape nullShape = ShapeFactory.CreateNull();
if (fixedShape.FaceCount <= 0 || unifiedShape.FaceCount <= 0 || !nullShape.IsNull)
{
    throw new InvalidOperationException("The packaged healing result profile did not preserve topology/null semantics.");
}
try
{
    _ = box.Cut(nullShape);
    throw new InvalidOperationException("The packaged null-topology contract accepted an invalid Boolean input.");
}
catch (ArgumentException)
{
}
try
{
    _ = nullShape.Fixed();
    throw new InvalidOperationException("The packaged null-topology contract accepted an invalid healing input.");
}
catch (ArgumentException)
{
}

using Shape common = box.Common(booleanTool);
using Shape distant = ShapeFactory.CreateBox(1, 1, 1).Transformed(ShapeTransform.CreateTranslationAndRotationZ(20, 0, 0, 0));
ShapeDistanceResult packageDistance = box.DistanceTo(distant);
if (common.FaceCount <= 0 || packageDistance.Distance <= 0 || packageDistance.SolutionCount <= 0)
{
    throw new InvalidOperationException("The packaged modeling algorithm profile did not return valid common/distance results.");
}

GpPoint packagePoint = GpPoint.Create(3, 4, 12);
if (packagePoint.DistanceTo(GpPoint.Origin) != 13)
{
    throw new InvalidOperationException("The packaged GpPoint facade did not round-trip its generated value contract.");
}

if (GpXyz.Create(1, 0, 0).Crossed(GpXyz.Create(0, 1, 0)) != new GpXyz(0, 0, 1))
{
    throw new InvalidOperationException("The packaged GpXyz facade did not preserve cross-product semantics.");
}

if (GpLine.Create(GpXyz.Origin, GpXyz.Create(1, 0, 0)).DistanceTo(GpXyz.Create(0, 1, 0)) != 1)
{
    throw new InvalidOperationException("The packaged GpLine facade did not preserve distance semantics.");
}

if (GpCircle.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), 2).Area != 4 * Math.PI)
{
    throw new InvalidOperationException("The packaged GpCircle facade did not preserve area semantics.");
}

if (GpPlane.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1)).DistanceTo(GpXyz.Create(0, 0, 2)) != 2)
{
    throw new InvalidOperationException("The packaged GpPlane facade did not preserve distance semantics.");
}

if (!GpAx3Value.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), GpXyz.Create(1, 0, 0)).IsDirect)
{
    throw new InvalidOperationException("The packaged GpAx3 facade did not preserve right-handed semantics.");
}

using GPropProperties packagedProperties = GPropProperties.FromShape(box);
if (Math.Abs(packagedProperties.Mass - 6000) > 1e-8
    || packagedProperties.CenterOfMass != new GpPoint(5, 10, 15))
{
    throw new InvalidOperationException("The packaged GProp_GProps bridge did not preserve volume properties.");
}

nint viewerWindow = PackageWindowMethods.CreateWindowEx(
    0, "STATIC", "OcctSharp package viewer", 0x80000000u,
    -32000, -32000, 256, 256, 0, 0, 0, 0);
if (viewerWindow == 0)
{
    throw new InvalidOperationException("The package consumer could not create a viewer HWND.");
}
try
{
    _ = PackageWindowMethods.ShowWindow(viewerWindow, 4);
    _ = PackageWindowMethods.UpdateWindow(viewerWindow);
    using OcctViewer packagedViewer = OcctViewer.Create(viewerWindow);
    using ViewerPresentation packagedPresentation = packagedViewer.Display(box);
    packagedPresentation.SetColor(new ViewerColor(0.2, 0.4, 0.8));
    packagedPresentation.SetTransparency(0.2);
    packagedPresentation.SetDisplayMode(ViewerDisplayMode.Shaded);
    packagedViewer.Resize();
    packagedViewer.SetProjection(ViewerProjection.Front);
    packagedViewer.SetProjection(ViewerProjection.Axonometric);
    packagedViewer.Zoom(1.05);
    packagedViewer.Pan(4, -2);
    packagedViewer.StartRotation(128, 128);
    packagedViewer.Rotate(132, 130);
    packagedViewer.FitAll();
    packagedViewer.Redraw();
    if (!packagedViewer.MoveTo(128, 128)
        || !packagedViewer.SelectAt(128, 128).Contains(packagedPresentation))
    {
        throw new InvalidOperationException("The packaged viewer did not produce the expected selection snapshot.");
    }
    packagedViewer.ClearSelection();
    if (packagedViewer.GetSelection().Count != 0
        || !packagedViewer.SelectAt(128, 128, ViewerSelectionMode.Add).Contains(packagedPresentation)
        || packagedViewer.SelectAt(128, 128, ViewerSelectionMode.Remove).Count != 0)
    {
        throw new InvalidOperationException("The packaged viewer selection modes failed.");
    }
}
finally
{
    _ = PackageWindowMethods.DestroyWindow(viewerWindow);
}

if (box.IsNull || box.Kind != ShapeKind.Solid || box.Orientation != ShapeOrientation.Forward)
{
    throw new InvalidOperationException("The packaged generated topology binding returned unexpected box semantics.");
}

using Shape clone = box.Clone();
using Shape reversed = box.Reversed();
if (!box.IsPartner(clone)
    || !box.IsSame(clone)
    || !box.IsEqual(clone)
    || !box.IsPartner(reversed)
    || !box.IsSame(reversed)
    || box.IsEqual(reversed)
    || reversed.Orientation != ShapeOrientation.Reversed)
{
    throw new InvalidOperationException("The packaged TopoDS_Shape copy/orientation semantics are invalid.");
}

using GeomCartesianPoint point = new(1, 2, 3);
point.SetPnt(new Point3d(4, 5, 6));
if (point.X() != 4 || point.Y() != 5 || point.Z() != 6
    || point.TypeName != "Geom_CartesianPoint")
{
    throw new InvalidOperationException(
        "The packaged generated shared-handle binding returned unexpected point data.");
}

using GpTrsf translation = GpTrsf.Create(10, 20, 30);
using GpTrsf inverse = translation.Inverted();
if (translation.Value(1, 4) != 10 || translation.Value(2, 4) != 20
    || translation.Value(3, 4) != 30
    || Math.Abs(inverse.Value(1, 4) + 10) > 1e-12)
{
    throw new InvalidOperationException("The packaged GpTrsf value bridge returned unexpected matrix data.");
}

using TopLocLocation location = TopLocLocation.FromTransform(translation);
using TopLocLocation inverseLocation = location.Inverted();
using TopLocLocation identityLocation = inverseLocation.Multiplied(location);
if (location.IsIdentity || !identityLocation.IsIdentity)
{
    throw new InvalidOperationException("The packaged TopLoc_Location bridge returned unexpected identity semantics.");
}

using GpVec vector = GpVec.Create(3, 4, 0);
using GpDir direction = GpDir.Create(0, 0, 1);
using GpAx1 axis = GpAx1.Create(0, 0, 0, 0, 0, 1);
using GpMat matrix = GpMat.Identity;
using GpTrsf vectorTranslation = vector.ToTranslation();
using GpTrsf axisRotation = axis.ToRotation(Math.PI / 2);
if (vector.Magnitude != 5 || direction.Dot(direction) != 1 || axis.Components.DirectionZ != 1 || matrix.Determinant != 1
    || Math.Abs(vectorTranslation.Value(1, 4) - 3) > 1e-12 || Math.Abs(axisRotation.Value(1, 1)) > 1e-12)
{
    throw new InvalidOperationException("The packaged gp value bridge returned unexpected vector/axis/matrix data.");
}

using OcctAsciiString ascii = OcctAsciiString.Create("包");
ascii.Append(" ok");
using OcctExtendedString extended = ascii.ToExtended();
if (extended.Value != "包 ok" || extended[0] != '包')
{
    throw new InvalidOperationException("The packaged UTF-8 string bridge returned unexpected text.");
}

using OcctRealSequence sequence = OcctRealSequence.Create([1, 2, 3]);
sequence.Set(1, 20);
sequence.Add(4);
if (sequence.Count != 4 || sequence[1] != 20)
{
    throw new InvalidOperationException("The packaged real sequence bridge returned unexpected values.");
}

using OcctRealArray array = OcctRealArray.Create([1, 2, 3]);
array.Set(1, 20);
using OcctRealVector vectorValues = OcctRealVector.Create([4, 5]);
vectorValues.Add(6);
if (array.LowerBound != 1 || array[1] != 20 || vectorValues.Count != 3 || vectorValues[2] != 6)
{
    throw new InvalidOperationException("The packaged array/vector bridge returned unexpected values.");
}

using OcctIntRealMap map = OcctIntRealMap.Create([new(1, 2.0)]);
map[2] = 3.0;
using OcctIntIndexedMap indexedMap = OcctIntIndexedMap.Create([5, 6]);
indexedMap.Add(7);
if (map[2] != 3.0 || indexedMap[2] != 7 || indexedMap.FindIndex(5) != 0)
{
    throw new InvalidOperationException("The packaged map bridge returned unexpected values.");
}

string exchangeDirectory = Path.Combine(Path.GetTempPath(), $"OcctSharp.PackageConsumer.{Guid.NewGuid():N}");
Directory.CreateDirectory(exchangeDirectory);
try
{
    string stepPath = ShapeExchange.WriteStep(box, Path.Combine(exchangeDirectory, "box.step"));
    using StepReadResult stepResult = ShapeExchange.ReadStepWithReport(stepPath);
    using ShapeRepairResult repairResult = stepResult.Shape.RepairWithReport();
    if (stepResult.Report.ReadStatus != StepReadStatus.Done
        || stepResult.Report.TransferredRootCount <= 0
        || !repairResult.Before.IsValid || !repairResult.After.IsValid)
        throw new InvalidOperationException("The packaged STEP diagnostic/repair workflow failed.");
    string brepPath = ShapeExchange.WriteBrep(box, Path.Combine(exchangeDirectory, "box.brep"));
    using Shape brepShape = ShapeExchange.ReadBrep(brepPath);
    if (!brepShape.GetTopologySummary().IsValid || brepShape.CreateDetailedMesh().TriangleCount == 0)
    {
        throw new InvalidOperationException("The packaged BREP/topology/detailed-mesh round-trip failed.");
    }
    string objPath = ShapeExchange.WriteObj(box, Path.Combine(exchangeDirectory, "box.obj"));
    string plyPath = ShapeExchange.WritePly(box, Path.Combine(exchangeDirectory, "box.ply"));
    string glbPath = ShapeExchange.WriteGltf(box, Path.Combine(exchangeDirectory, "box.glb"));
    string vrmlPath = ShapeExchange.WriteVrml(box, Path.Combine(exchangeDirectory, "box.wrl"));

    if (new[] { objPath, plyPath, glbPath, vrmlPath }.Any(path => new FileInfo(path).Length == 0))
    {
        throw new InvalidOperationException("A packaged mesh-format writer produced an empty file.");
    }

    using Shape objShape = ShapeExchange.ReadObj(objPath);
    using Shape gltfShape = ShapeExchange.ReadGltf(glbPath);
    using Shape vrmlShape = ShapeExchange.ReadVrml(vrmlPath);
    if (objShape.IsNull || gltfShape.IsNull || vrmlShape.IsNull
        || objShape.FaceCount == 0 || gltfShape.FaceCount == 0 || vrmlShape.FaceCount == 0)
    {
        throw new InvalidOperationException("A packaged mesh-format reader returned empty topology.");
    }

    string ocafPath = Path.Combine(exchangeDirectory, "package-document.cbf");
    string childEntry;
    using (OcafDocument document = OcafDocument.Create())
    {
        using OcafTransaction transaction = document.BeginTransaction();
        document.RootLabel.Name = "Package root";
        OcafLabel child = document.RootLabel.AddChild();
        child.Name = "包零件";
        childEntry = child.Entry;
        if (!transaction.Commit())
        {
            throw new InvalidOperationException("The packaged OCAF transaction did not create a delta.");
        }
        document.Save(ocafPath);
    }

    using OcafDocument restoredDocument = OcafDocument.Open(ocafPath);
    if (restoredDocument.RootLabel.Name != "Package root"
        || restoredDocument.RootLabel.ChildCount != 1
        || restoredDocument.GetLabel(childEntry).Name != "包零件")
    {
        throw new InvalidOperationException("The packaged binary OCAF document did not preserve labels and names.");
    }

    string xdePath = Path.Combine(exchangeDirectory, "package-assembly.xbf");
    string xdeStepPath = Path.Combine(exchangeDirectory, "package-assembly.step");
    string xdePartEntry;
    string xdeAssemblyEntry;
    using (XdeDocument xde = XdeDocument.Create())
    {
        using XdeTransaction transaction = xde.BeginTransaction();
        XdeLabel part = xde.AddPart(box, new XdePartMetadata(
            "Package part",
            new XdeColor(0.2, 0.4, 0.6),
            ["Package layer"],
            new XdeMaterial("Steel", "Package material", 7.85, "Density", "g/cm3")));
        XdeValidationProperties validationProperties = part.UpdateValidationPropertiesFromShape();
        if (!validationProperties.IsComplete || Math.Abs(validationProperties.Volume!.Value - 6000) > 1e-8)
            throw new InvalidOperationException("The packaged XDE validation-property computation failed.");
        XdeLabel assembly = xde.AddAssembly("Package assembly");
        _ = xde.AddComponent(assembly, part, location);
        xdePartEntry = part.Entry;
        xdeAssemblyEntry = assembly.Entry;
        transaction.Commit();
        IReadOnlyList<XdeOccurrence> occurrences = assembly.GetOccurrences();
        try
        {
            XdeOccurrence occurrence = occurrences.Single();
            using TopLocLocation worldLocation = occurrence.GetWorldLocation();
            using GpTrsf worldTransform = worldLocation.ToTransform();
            using Shape located = occurrence.GetLocatedShape();
            if (Math.Abs(worldTransform.Value(1, 4) - 10) > 1e-8
                || Math.Abs(located.GetBoundingBox().Minimum.X - 10) > 1e-6)
                throw new InvalidOperationException("The packaged XDE occurrence traversal failed.");
        }
        finally { foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose(); }
        xde.Save(xdePath);
        xde.WriteStep(xdeStepPath, new XdeStepWriteOptions(
            ModelType: XdeStepModelType.AsIs,
            WriteValidationProperties: true));
    }

    using (XdeDocument binaryXde = XdeDocument.Open(xdePath))
    {
        XdeLabel part = binaryXde.GetLabel(xdePartEntry);
        if (part.Name != "Package part" || part.Color is null
            || !part.Layers.Contains("Package layer") || part.Material?.Name != "Steel"
            || !part.ValidationProperties.IsComplete
            || !binaryXde.GetLabel(xdeAssemblyEntry).IsAssembly)
        {
            throw new InvalidOperationException("The packaged BinXCAF metadata round-trip failed.");
        }
    }

    string composedStepPath = Path.Combine(exchangeDirectory, "package-composed-assembly.step");
    using (XdeDocument composedXde = XdeDocument.Create())
    {
        using XdeTransaction transaction = composedXde.BeginTransaction();
        XdeLabel importedRoot = composedXde.ImportStep(xdeStepPath).Single();
        XdeLabel assembly = composedXde.AddAssembly("Composed package assembly");
        using TopLocLocation identity = TopLocLocation.Identity;
        _ = composedXde.AddComponent(assembly, importedRoot, identity);
        transaction.Commit();
        composedXde.WriteStep(composedStepPath);
    }
    using (XdeDocument composedRoundTrip = XdeDocument.ReadStep(composedStepPath))
    {
        XdeLabel composedAssembly = composedRoundTrip.GetFreeShapes().Single();
        if (!composedAssembly.IsAssembly || composedAssembly.ComponentCount != 1)
        {
            throw new InvalidOperationException("The packaged composable XDE STEP import workflow failed.");
        }
    }

    using XdeDocument stepXde = XdeDocument.ReadStep(xdeStepPath, new XdeStepReadOptions(
        ReadValidationProperties: true));
    XdeLabel stepAssembly = stepXde.GetFreeShapes().Single();
    XdeLabel stepPart = stepAssembly.GetComponents().Single().ReferredShape;
    if (!stepAssembly.IsAssembly || stepPart.Name != "Package part" || stepPart.Color is null
        || !stepPart.Layers.Contains("Package layer") || stepPart.Material?.Name != "Steel"
        || !stepPart.ValidationProperties.IsComplete)
    {
        throw new InvalidOperationException("The packaged STEPCAF metadata round-trip failed.");
    }
}
finally
{
    Directory.Delete(exchangeDirectory, recursive: true);
}

Console.WriteLine(
    $"Package consumer passed with {nativeFiles.Length} DLLs in 'occt', "
    + $"ABI {runtime.AbiVersion}, bridge {runtime.BridgeVersion}, OCCT {runtime.OcctVersion}.");

internal static class PackageWindowMethods
{
    [System.Runtime.InteropServices.DllImport(
        "user32.dll",
        EntryPoint = "CreateWindowExW",
        CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    internal static extern nint CreateWindowEx(
        uint extendedStyle,
        string className,
        string windowName,
        uint style,
        int x,
        int y,
        int width,
        int height,
        nint parent,
        nint menu,
        nint instance,
        nint parameter);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static extern bool ShowWindow(nint window, int command);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static extern bool UpdateWindow(nint window);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static extern bool DestroyWindow(nint window);
}
