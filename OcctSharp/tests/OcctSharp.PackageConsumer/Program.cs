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
if (runtime.AbiVersion != new Version(1, 58)
    || runtime.BridgeVersion != "0.66.0"
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
    packagedPresentation.SetSelectionKind(ShapeKind.Face);
    if (!packagedViewer.Input.PointerMoved(128, 128))
    {
        throw new InvalidOperationException("The packaged viewer did not detect the displayed shape.");
    }
    packagedViewer.Input.PointerPressed(ViewerPointerButton.Left, 128, 128);
    if (!packagedViewer.Input.PointerReleased(ViewerPointerButton.Left, 128, 128).Contains(packagedPresentation))
        throw new InvalidOperationException("The packaged viewer input controller did not select the presentation.");
    IReadOnlyList<ViewerSelectionItem> packagedSelectedItems = packagedViewer.GetSelectedItems();
    try
    {
        if (packagedSelectedItems.Count != 1 || packagedSelectedItems[0].Shape.Kind != ShapeKind.Face)
            throw new InvalidOperationException("The packaged viewer did not return an owning face-selection snapshot.");
    }
    finally { foreach (ViewerSelectionItem item in packagedSelectedItems) item.Dispose(); }
    packagedViewer.Input.MouseWheel(120, 128, 128);
    if (!packagedViewer.Input.KeyDown(ViewerInputKey.Axonometric))
        throw new InvalidOperationException("The packaged semantic viewer keyboard command was not handled.");
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
    using Shape derivativeEdge = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(10, 0, 0));
    CurveDerivativeEvaluation derivative = derivativeEdge.EvaluateEdgeDerivatives(4);
    using Shape trimmedEdge = derivativeEdge.TrimEdge(2, 8);
    using Shape connectedEdge = ShapeFactory.CreateEdge(new GpPoint(10, 0, 0), new GpPoint(10, 5, 0));
    using Shape connectedWire = ShapeFactory.CreateWire([derivativeEdge, connectedEdge]);
    if (derivative.Point != new GpPoint(4, 0, 0)
        || derivative.FirstDerivative != new GpPoint(1, 0, 0)
        || derivative.SecondDerivative != GpPoint.Origin
        || Math.Abs(trimmedEdge.GetEdgeLength() - 6) > 1e-10
        || connectedWire.Kind != ShapeKind.Wire)
    {
        throw new InvalidOperationException("The packaged derivative/trim/wire workflow failed.");
    }

    Shape[] packageFaces = box.GetFaces();
    Shape[] packageFaceEdges = packageFaces[0].GetSubShapes(ShapeKind.Edge);
    try
    {
        FaceSurfaceSnapshot bounds = packageFaces[0].GetFaceSurfaceSnapshot();
        double u = (bounds.FirstUParameter + bounds.LastUParameter) / 2;
        double v = (bounds.FirstVParameter + bounds.LastVParameter) / 2;
        SurfaceDerivativeEvaluation surface = packageFaces[0].EvaluateFaceDerivatives(u, v);
        PcurveSnapshot pcurve = packageFaceEdges[0].GetPcurveSnapshot(packageFaces[0]);
        PcurveEvaluation pcurveValue = packageFaceEdges[0].EvaluatePcurve(
            packageFaces[0], (pcurve.FirstParameter + pcurve.LastParameter) / 2);
        using Shape trimmedFace = packageFaces[0].TrimFace(
            bounds.FirstUParameter + (bounds.LastUParameter - bounds.FirstUParameter) / 4,
            bounds.LastUParameter - (bounds.LastUParameter - bounds.FirstUParameter) / 4,
            bounds.FirstVParameter + (bounds.LastVParameter - bounds.FirstVParameter) / 4,
            bounds.LastVParameter - (bounds.LastVParameter - bounds.FirstVParameter) / 4);
        if (trimmedFace.Kind != ShapeKind.Face || !trimmedFace.IsValid
            || !double.IsFinite(surface.Normal.X) || !double.IsFinite(pcurveValue.Point.X))
            throw new InvalidOperationException("The packaged face derivative/pcurve/trim workflow failed.");
    }
    finally
    {
        foreach (Shape edge in packageFaceEdges) edge.Dispose();
        foreach (Shape face in packageFaces) face.Dispose();
    }

    using (TopologyAdjacencyMap adjacency = box.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face))
    {
        if (adjacency.GetItemIndices(0).Count != 4)
            throw new InvalidOperationException("The packaged reverse topology adjacency failed.");
    }

    using Shape secondBoxSource = ShapeFactory.CreateBox(10, 20, 30);
    using Shape secondBox = secondBoxSource.Transformed(
        ShapeTransform.CreateTranslationAndRotationZ(40, 0, 0, 0));
    using Shape editSource = ShapeFactory.CreateCompound([box, secondBox]);
    Shape[] editSolids = editSource.GetSubShapes(ShapeKind.Solid);
    using Shape replacementSolid = ShapeFactory.CreateBox(2, 3, 4);
    try
    {
        using Shape replaced = editSource.ReplaceSubshape(editSolids[0], replacementSolid);
        using Shape removed = editSource.RemoveSubshape(editSolids[1]);
        if (replaced.CountSubShapes(ShapeKind.Solid) != 2
            || removed.CountSubShapes(ShapeKind.Solid) != 1)
            throw new InvalidOperationException("The packaged replace/remove topology workflow failed.");
    }
    finally { foreach (Shape solid in editSolids) solid.Dispose(); }

    string selectiveInputPath = ShapeExchange.WriteStep(
        editSource, Path.Combine(exchangeDirectory, "selective-input.step"));
    Shape selectivelyImported;
    using (StepReadSession session = StepReadSession.Open(selectiveInputPath, 1.0))
    {
        if (session.Info.ReadStatus != StepReadStatus.Done
            || session.Info.CandidateRootCount <= 0
            || session.Info.FileUnits.Length.Count == 0)
            throw new InvalidOperationException("The packaged STEP session metadata failed.");
        selectivelyImported = session.TransferRoot(0);
    }
    using (selectivelyImported)
    {
        Shape[] importedSolids = selectivelyImported.GetSubShapes(ShapeKind.Solid);
        if (importedSolids.Length < 2)
            throw new InvalidOperationException("The packaged selective STEP transfer lost solid roots.");
        Shape editedImport;
        try { editedImport = selectivelyImported.RemoveSubshape(importedSolids[1]); }
        finally { foreach (Shape solid in importedSolids) solid.Dispose(); }
        using (editedImport)
        {
            string editedStepPath = ShapeExchange.WriteStep(
                editedImport, Path.Combine(exchangeDirectory, "selective-edited.step"));
            using Shape editedRoundTrip = ShapeExchange.ReadStep(editedStepPath);
            if (!editedRoundTrip.IsValid || editedRoundTrip.CountSubShapes(ShapeKind.Solid) != 1)
                throw new InvalidOperationException("The packaged selective STEP edit/export workflow failed.");
        }
    }

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

    nint reviewWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch D package review", 0x80000000u,
        -32000, -32000, 256, 256, 0, 0, 0, 0);
    if (reviewWindow == 0) throw new InvalidOperationException("The Batch D package review HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(reviewWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(reviewWindow);
        using OcctViewer reviewViewer = OcctViewer.Create(reviewWindow);
        IReadOnlyList<XdeOccurrence> reviewOccurrences = stepAssembly.GetOccurrences();
        try
        {
            XdeOccurrence occurrence = reviewOccurrences.Single();
            GpPoint occurrenceCenter;
            using (Shape locatedOccurrence = occurrence.GetLocatedShape())
            {
                BoundingBox3d locatedBounds = locatedOccurrence.GetBoundingBox();
                occurrenceCenter = new GpPoint(
                    (locatedBounds.Minimum.X + locatedBounds.Maximum.X) * 0.5,
                    (locatedBounds.Minimum.Y + locatedBounds.Maximum.Y) * 0.5,
                    (locatedBounds.Minimum.Z + locatedBounds.Maximum.Z) * 0.5);
            }
            ViewerPresentation reviewPresentation = reviewViewer.Display(occurrence);
            string occurrenceEntry = occurrence.OccurrenceLabel.Entry;
            if (reviewPresentation.SourceIdentity?.OccurrenceEntry != occurrenceEntry)
                throw new InvalidOperationException("The packaged viewer did not copy XDE occurrence identity.");
            occurrence.Dispose();

            reviewPresentation.SetSelectionKind(ShapeKind.Face);
            reviewViewer.SetPixelTolerance(4);
            reviewViewer.FitAll();
            reviewViewer.Redraw();
            ViewerPixelPoint centerPixel = reviewViewer.WorldToScreen(occurrenceCenter);
            if (!reviewViewer.MoveTo(centerPixel.X, centerPixel.Y))
                throw new InvalidOperationException("The packaged Batch D viewer did not detect the occurrence.");
            using ViewerDetectionItem detectedItem = reviewViewer.GetDetectedItem()
                ?? throw new InvalidOperationException("The packaged Batch D viewer did not return owning detected topology.");
            if (detectedItem.SourceIdentity?.OccurrenceEntry != occurrenceEntry)
                throw new InvalidOperationException("The packaged detected topology lost XDE identity.");
            reviewPresentation.SetSubshapeColor(detectedItem.Shape, new ViewerColor(0.8, 0.2, 0.1));
            reviewPresentation.SetSubshapeTransparency(detectedItem.Shape, 0.2);
            reviewPresentation.SetSubshapeWidth(detectedItem.Shape, 2);
            reviewPresentation.ClearSubshapeOverrides(detectedItem.Shape);

            reviewPresentation.SetSelectionKind(null);
            if (reviewViewer.SelectRectangle(0, 0, 255, 255).Count == 0
                || reviewViewer.SelectPolygon([
                    new GpPoint2d(0, 0), new GpPoint2d(255, 0),
                    new GpPoint2d(255, 255), new GpPoint2d(0, 255)]).Count == 0)
                throw new InvalidOperationException("The packaged Batch D area-selection workflow failed.");
            if (reviewViewer.GetSelectionBounds() is null || !reviewViewer.FitSelected())
                throw new InvalidOperationException("The packaged Batch D selection fit workflow failed.");
            reviewViewer.IsolateSelected();
            if (!reviewViewer.RestoreIsolation())
                throw new InvalidOperationException("The packaged Batch D isolate workflow failed.");

            ViewerCameraState reviewCamera = reviewViewer.GetCamera();
            reviewViewer.SetCamera(reviewCamera);
            GpPoint centerWorld = reviewViewer.ScreenToWorld(128, 128);
            ViewerPixelPoint projectedCenter = reviewViewer.WorldToScreen(centerWorld);
            ViewerPickRay reviewRay = reviewViewer.GetPickRay(128, 128);
            double rayMagnitude = Math.Sqrt(
                reviewRay.Direction.X * reviewRay.Direction.X
                + reviewRay.Direction.Y * reviewRay.Direction.Y
                + reviewRay.Direction.Z * reviewRay.Direction.Z);
            if (Math.Abs(projectedCenter.X - 128) > 1 || Math.Abs(projectedCenter.Y - 128) > 1
                || Math.Abs(rayMagnitude - 1) > 1e-6)
                throw new InvalidOperationException("The packaged Batch D camera conversion workflow failed.");

            reviewViewer.ZoomWindow(24, 24, 232, 232);
            reviewViewer.SetBackgroundColor(new ViewerColor(0.04, 0.06, 0.1));
            using (ViewerClipPlane clip = reviewViewer.CreateClipPlane(new ViewerPlaneEquation(1, 0, 0, -5)))
            {
                clip.Update(new ViewerPlaneEquation(0, 1, 0, -10));
                clip.SetEnabled(false);
                clip.SetEnabled(true);
            }
            reviewViewer.SetComputedHiddenLine(true);
            reviewViewer.SetComputedHiddenLine(false);
            reviewViewer.ShowTrihedron(ViewerTrihedronPosition.RightLower, scale: 0.08);
            reviewViewer.HideTrihedron();
            string reviewImage = reviewViewer.SaveScreenshot(
                Path.Combine(exchangeDirectory, "package-batch-d-review.png"), overwrite: true);
            if (!File.Exists(reviewImage) || new FileInfo(reviewImage).Length == 0)
                throw new InvalidOperationException("The packaged Batch D screenshot workflow failed.");
        }
        finally
        {
            foreach (XdeOccurrence occurrence in reviewOccurrences) occurrence.Dispose();
        }
    }
    finally { _ = PackageWindowMethods.DestroyWindow(reviewWindow); }

    using Shape batchETranslatedSource = ShapeFactory.CreateBox(10, 20, 30);
    using Shape batchETranslated = batchETranslatedSource.Transformed(
        ShapeTransform.CreateTranslationAndRotationZ(15, 0, 0, 0));
    using ExactDistanceResult batchEDistance = box.InspectDistanceTo(
        batchETranslated, new InspectionUnits("mm", "in", "rad", "deg", 4));
    using ShapePairInspection batchEPair = box.InspectPair(batchETranslated);
    if (Math.Abs(batchEDistance.Distance - 5) > 1e-8
        || batchEDistance.Solutions.Count == 0
        || batchEPair.Classification != ShapePairClassification.Separated
        || Math.Abs(box.InspectProperties(InspectionPropertyKind.Volume).Mass - 6000) > 1e-6)
        throw new InvalidOperationException("The packaged Batch E exact-inspection workflow failed.");

    string batchEXbf = Path.Combine(exchangeDirectory, "package-batch-e.xbf");
    string batchEStep = Path.Combine(exchangeDirectory, "package-batch-e-ap242.step");
    using XdeDocument batchEDocument = XdeDocument.Create();
    XdeSavedView batchESavedView;
    using (XdeTransaction transaction = batchEDocument.BeginTransaction())
    {
        XdeLabel part = batchEDocument.AddShape(box, "Package inspection part");
        XdeDatum datum = batchEDocument.CreateDatum(new XdeDatumDefinition
        {
            Name = "A",
            Identification = "A",
            SemanticName = "Primary datum",
            Position = 1
        }, [part]);
        XdeDimension dimension = batchEDocument.CreateDimension(new XdeDimensionDefinition(
            XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_LinearDistance,
            [10.0])
        {
            SemanticName = "Overall length",
            FirstPoint = GpPoint.Origin,
            SecondPoint = new GpPoint(10, 0, 0),
            TextPosition = new GpPoint(5, -3, 0)
        }, [part], [part]);
        XdeGeomTolerance tolerance = batchEDocument.CreateGeometricTolerance(
            new XdeGeomToleranceDefinition
            {
                Type = XCAFDimTolObjectsGeomToleranceType.XCAFDimTolObjects_GeomToleranceType_Flatness,
                Value = 0.1,
                SemanticName = "Flatness"
            }, [part], [datum]);
        batchESavedView = batchEDocument.CreateSavedView(new XdeSavedViewDefinition
        {
            Name = "Package inspection view",
            ProjectionType = XCAFViewProjectionType.XCAFView_ProjectionType_Parallel,
            ProjectionPoint = new GpPoint(30, 30, 30),
            ViewDirection = new GpXyz(-1, -1, -1),
            UpDirection = new GpXyz(0, 0, 1),
            ClippingPlanes = [new ViewerPlaneEquation(1, 0, 0, -8)]
        }, [part], [dimension, tolerance, datum]);
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch E transaction did not commit.");
    }
    batchEDocument.Save(batchEXbf);
    batchEDocument.WriteStep(batchEStep, new XdeStepWriteOptions(
        WriteGdt: true, Schema: XdeStepSchema.Ap242));
    using (XdeDocument reopenedBatchE = XdeDocument.Open(batchEXbf))
    {
        if (reopenedBatchE.GetDimensions().Length != 1
            || reopenedBatchE.GetGeometricTolerances().Length != 1
            || reopenedBatchE.GetDatums().Length != 1
            || reopenedBatchE.GetSavedViews().Length != 1)
            throw new InvalidOperationException("The packaged Batch E binary persistence workflow failed.");
    }
    using (XdeDocument importedBatchE = XdeDocument.ReadStep(batchEStep,
        new XdeStepReadOptions(ReadGdt: true, ReadSavedViews: true)))
    {
        if (importedBatchE.GetDimensions().Length != 1
            || importedBatchE.GetGeometricTolerances().Length != 1
            || importedBatchE.GetDatums().Length != 1)
            throw new InvalidOperationException("The packaged Batch E AP242 reimport workflow failed.");
    }

    nint batchEWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch E package inspection", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchEWindow == 0)
        throw new InvalidOperationException("The Batch E package inspection HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchEWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchEWindow);
        using OcctViewer batchEViewer = OcctViewer.Create(batchEWindow);
        using ViewerPresentation batchEPresentation = batchEViewer.Display(box);
        using Shape batchECircle = ShapeFactory.CreateCircleEdge(
            new GpPoint(5, 5, 12), new GpPoint(0, 0, 1), 3);
        ViewerDimensionStyle batchEStyle = new()
        {
            Units = new InspectionUnits("mm", "mm", "rad", "deg", 2),
            Color = new ViewerColor(1, 0.8, 0.1),
            Flyout = 6,
            LineWidth = 2
        };
        using ViewerDimension batchELength = batchEViewer.DisplayLengthDimension(
            GpPoint.Origin, new GpPoint(10, 0, 0), new ViewerPlaneEquation(0, 0, 1, 0), batchEStyle);
        using ViewerDimension batchEAngle = batchEViewer.DisplayAngleDimension(
            new GpPoint(10, 0, 0), GpPoint.Origin, new GpPoint(0, 10, 0), batchEStyle);
        using ViewerDimension batchERadius = batchEViewer.DisplayRadiusDimension(batchECircle, batchEStyle);
        using ViewerDimension batchEDiameter = batchEViewer.DisplayDiameterDimension(batchECircle, batchEStyle);
        batchELength.Hide();
        batchELength.Show();
        batchEAngle.UpdateStyle(batchEStyle with { CustomValue = 45 });
        batchEAngle.UpdateStyle(batchEStyle with { CustomValue = null });
        batchERadius.SetSelected();
        batchERadius.SetSelected(false);
        batchESavedView.ApplyTo(batchEViewer);
        batchEViewer.FitAll();
        batchEViewer.Redraw();
        string batchEImage = batchEViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-e-inspection.png"), overwrite: true);
        if (!File.Exists(batchEImage) || new FileInfo(batchEImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch E inspection screenshot workflow failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchEWindow); }

    FreeformCurveDefinition batchFBezierDefinition = FreeformCurveDefinition.Bezier(
        [new(0, 0, 0), new(3, 6, 0), new(7, -4, 0), new(10, 0, 0)],
        [1, 0.65, 0.8, 1]);
    FreeformCurveDefinition batchFSplineDefinition = FreeformCurveDefinition.BSpline(
        [new(0, 4, 2), new(3, 8, 2), new(7, 0, 2), new(10, 4, 2)],
        [0, 1], [4, 4], 3, weights: [1, 0.75, 0.75, 1]);
    using Shape batchFBezier = FreeformAuthoring.CreateCurve(batchFBezierDefinition);
    using Shape batchFSpline = FreeformAuthoring.CreateCurve(batchFSplineDefinition);
    using Shape batchFInterpolated = FreeformAuthoring.InterpolateCurve(
        [new(0, 0, 4), new(3, 2, 4), new(7, -2, 4), new(10, 0, 4)],
        new GpXyz(1, 0, 0), new GpXyz(1, 0, 0));
    using Shape batchFPeriodic = FreeformAuthoring.InterpolateCurve(
        [new(0, 0, 6), new(5, 0, 6), new(5, 5, 6), new(0, 5, 6)], periodic: true);
    using Shape batchFApproximated = FreeformAuthoring.ApproximateCurve(
        [new(0, 0, 8), new(2, 1, 8), new(4, -1, 8), new(6, 2, 8), new(8, 0, 8)]);
    FreeformCurveDefinition batchFBezierSnapshot = FreeformAuthoring.GetCurveDefinition(batchFBezier);
    FreeformCurveDefinition batchFSplineSnapshot = FreeformAuthoring.GetCurveDefinition(batchFSpline);
    using Shape batchFElevatedCurve = FreeformAuthoring.ElevateCurveDegree(batchFBezier, 5);
    using Shape batchFReversedCurve = FreeformAuthoring.ReverseCurve(batchFSpline);
    using Shape batchFSegmentedCurve = FreeformAuthoring.SegmentCurve(batchFBezier, new ParameterRange(0.2, 0.8));
    IReadOnlyList<Shape> batchFCurvePieces = FreeformAuthoring.SplitCurve(batchFBezier, [0.25, 0.6]);
    try
    {
        if (!batchFBezierSnapshot.IsRational || batchFSplineSnapshot.Degree != 3
            || !FreeformAuthoring.GetCurveDefinition(batchFPeriodic).Periodic
            || FreeformAuthoring.GetCurveDefinition(batchFInterpolated).Kind != FreeformGeometryKind.BSpline
            || FreeformAuthoring.GetCurveDefinition(batchFApproximated).Kind != FreeformGeometryKind.BSpline
            || FreeformAuthoring.GetCurveDefinition(batchFElevatedCurve).Degree != 5
            || FreeformAuthoring.GetCurveDefinition(batchFReversedCurve).Poles[0] != batchFSplineSnapshot.Poles[^1]
            || batchFCurvePieces.Count != 3
            || FreeformAuthoring.ProjectPoint(batchFBezier, new GpPoint(5, 3, 0)).Count == 0
            || FreeformAuthoring.CurveExtrema(batchFBezier, batchFSpline).Count == 0)
            throw new InvalidOperationException("The packaged Batch F curve definition/edit/solution closure failed.");
    }
    finally { foreach (Shape piece in batchFCurvePieces) piece.Dispose(); }

    GpPoint[] batchFBezierGrid =
    [
        new(0, 0, 0), new(0, 4, 0), new(0, 8, 0),
        new(4, 0, 0), new(4, 4, 3), new(4, 8, 0),
        new(8, 0, 0), new(8, 4, 0), new(8, 8, 0)
    ];
    using Shape batchFBezierFace = FreeformAuthoring.CreateSurfaceFace(
        FreeformSurfaceDefinition.Bezier(3, 3, batchFBezierGrid,
            [1, 0.8, 1, 0.9, 0.6, 0.9, 1, 0.8, 1]));
    GpPoint[] batchFSplineGrid =
    [
        new(0, 0, 12), new(0, 3, 13), new(0, 6, 12), new(0, 9, 11),
        new(3, 0, 13), new(3, 3, 15), new(3, 6, 13), new(3, 9, 12),
        new(6, 0, 12), new(6, 3, 14), new(6, 6, 12), new(6, 9, 11),
        new(9, 0, 11), new(9, 3, 12), new(9, 6, 11), new(9, 9, 10)
    ];
    using Shape batchFSplineFace = FreeformAuthoring.CreateSurfaceFace(
        FreeformSurfaceDefinition.BSpline(4, 4, batchFSplineGrid,
            [0, 1], [4, 4], [0, 1], [4, 4], 3, 3,
            weights: [1, 1, 1, 1, 1, 0.8, 0.8, 1, 1, 0.8, 0.8, 1, 1, 1, 1, 1]));
    IReadOnlyList<IReadOnlyList<GpPoint>> batchFFitGrid =
    [
        [new(0, 0, 0), new(0, 3, 1), new(0, 6, 0), new(0, 9, -1)],
        [new(3, 0, 1), new(3, 3, 3), new(3, 6, 1), new(3, 9, 0)],
        [new(6, 0, 0), new(6, 3, 2), new(6, 6, 0), new(6, 9, -1)],
        [new(9, 0, -1), new(9, 3, 0), new(9, 6, -1), new(9, 9, -2)]
    ];
    using Shape batchFInterpolatedSurface = FreeformAuthoring.InterpolateSurface(batchFFitGrid);
    using Shape batchFApproximatedSurface = FreeformAuthoring.ApproximateSurface(batchFFitGrid);
    using Shape batchFElevatedSurface = FreeformAuthoring.ElevateSurfaceDegree(batchFBezierFace, 4, 4);
    using Shape batchFReversedU = FreeformAuthoring.ReverseSurfaceU(batchFBezierFace);
    using Shape batchFReversedV = FreeformAuthoring.ReverseSurfaceV(batchFBezierFace);
    using Shape batchFTrimmedSurface = FreeformAuthoring.TrimSurface(
        batchFBezierFace, new SurfaceParameterBounds(0.15, 0.85, 0.2, 0.8));
    if (!FreeformAuthoring.GetSurfaceDefinition(batchFBezierFace).IsRational
        || FreeformAuthoring.GetSurfaceDefinition(batchFSplineFace).UDegree != 3
        || FreeformAuthoring.GetSurfaceDefinition(batchFInterpolatedSurface).Kind != FreeformGeometryKind.BSpline
        || FreeformAuthoring.GetSurfaceDefinition(batchFApproximatedSurface).Kind != FreeformGeometryKind.BSpline
        || FreeformAuthoring.GetSurfaceDefinition(batchFElevatedSurface).UDegree != 4
        || batchFReversedU.Kind != ShapeKind.Face || batchFReversedV.Kind != ShapeKind.Face
        || batchFTrimmedSurface.Kind != ShapeKind.Face)
        throw new InvalidOperationException("The packaged Batch F surface definition/edit/fitting closure failed.");

    using Shape batchFLowerEdge = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier(
        [new(0, 0, 0), new(4, 2, 0), new(8, 0, 0)]));
    using Shape batchFUpperEdge = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier(
        [new(0, 0, 4), new(4, -2, 5), new(8, 0, 4)]));
    using Shape batchFRuledFace = FreeformAuthoring.CreateRuledFace(batchFLowerEdge, batchFUpperEdge);
    using Shape batchFCrossing = ShapeFactory.CreateEdge(new GpPoint(-5, 0, 0), new GpPoint(5, 0, 0));
    using Shape batchFIntersectionPlane = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
        [new(0, -10, -10), new(0, -10, 10), new(0, 10, -10), new(0, 10, 10)]));
    if (FreeformAuthoring.IntersectCurveWithFace(batchFCrossing, batchFIntersectionPlane).Count != 1
        || batchFRuledFace.Kind != ShapeKind.Face)
        throw new InvalidOperationException("The packaged Batch F ruled/intersection closure failed.");

    Shape[] batchFBoundary =
    [
        ShapeFactory.CreateEdge(new(0, 0, 0), new(10, 0, 0)),
        ShapeFactory.CreateEdge(new(10, 0, 0), new(10, 10, 1)),
        ShapeFactory.CreateEdge(new(10, 10, 1), new(0, 10, 0)),
        ShapeFactory.CreateEdge(new(0, 10, 0), new(0, 0, 0))
    ];
    try
    {
        using FreeformShapeResult batchFFilled = FreeformAuthoring.FillBoundary(
            batchFBoundary, [new GpPoint(5, 5, 2)], FreeformContinuity.C0);
        using FreeformShapeResult batchFOffsetFace = FreeformAuthoring.OffsetFaceOrShell(batchFFilled.Shape, 0.5);
        if (batchFFilled.Shape.Kind != ShapeKind.Face || batchFFilled.Diagnostics.G0Error < 0
            || !batchFOffsetFace.Shape.IsValid)
            throw new InvalidOperationException("The packaged Batch F fill/freeform-offset closure failed.");
    }
    finally { foreach (Shape edge in batchFBoundary) edge.Dispose(); }

    GpPoint[] batchFProfilePoints = [new(-2, -2, 0), new(2, -2, 0), new(2, 2, 0), new(-2, 2, 0)];
    using Shape batchFLowerProfile = FreeformAuthoring.CreateLocatedPlanarProfile(
        batchFProfilePoints, new GpPoint(0, 0, 0), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0));
    using Shape batchFUpperProfile = FreeformAuthoring.CreateLocatedPlanarProfile(
        batchFProfilePoints, new GpPoint(0, 0, 8), new GpXyz(0, 0, 1), new GpXyz(1, 0, 0), interpolate: true);
    using Shape batchFPlanarOffset = FreeformAuthoring.OffsetPlanarWire(batchFLowerProfile, 1.0, join: PlanarOffsetJoin.Arc);
    using FreeformShapeResult batchFSmoothLoft = FreeformAuthoring.CreateLoft(
        [batchFLowerProfile, batchFUpperProfile], makeSolid: true, smoothing: true);
    using FreeformShapeResult batchFRuledLoft = FreeformAuthoring.CreateLoft(
        [batchFLowerProfile, batchFUpperProfile], makeSolid: true, ruled: true, smoothing: false);
    using Shape batchFSpineEdge = ShapeFactory.CreateEdge(new GpPoint(0, 0, 0), new GpPoint(0, 0, 12));
    using Shape batchFSpine = ShapeFactory.CreateWire([batchFSpineEdge]);
    using FreeformShapeResult batchFPipe = FreeformAuthoring.CreatePipeShell(
        batchFSpine, [batchFLowerProfile], makeSolid: true, transition: PipeTransition.Transformed,
        maximumDegree: 8, maximumSegments: 24);
    using Shape batchFSplitBox = ShapeFactory.CreateBox(10, 10, 10);
    using Shape batchFSplitTool = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
        [new(-1, -1, 5), new(-1, 11, 5), new(11, -1, 5), new(11, 11, 5)]));
    using FreeformShapeResult batchFSplit = FreeformAuthoring.SplitTopology([batchFSplitBox], [batchFSplitTool]);
    using FreeformShapeResult batchFHealed = FreeformAuthoring.Heal(batchFSplit.Shape);
    Shape[] batchFLoftFaces = batchFSmoothLoft.Shape.GetSubShapes(ShapeKind.Face);
    try
    {
        using FreeformShapeResult batchFSewn = FreeformAuthoring.SewHealValidate(batchFLoftFaces);
        if (batchFPlanarOffset.CountSubShapes(ShapeKind.Edge) < 4
            || !batchFSmoothLoft.Diagnostics.IsValid || !batchFRuledLoft.Diagnostics.IsValid
            || !batchFPipe.Diagnostics.IsValid || batchFSplit.Diagnostics.ResultCount < 2
            || batchFSplit.Diagnostics.ModifiedCount == 0 || !batchFHealed.Diagnostics.IsValid
            || !batchFSewn.Diagnostics.IsValid)
            throw new InvalidOperationException("The packaged Batch F profile/split/loft/pipe/heal closure failed.");
    }
    finally { foreach (Shape face in batchFLoftFaces) face.Dispose(); }

    string batchFStep = Path.Combine(exchangeDirectory, "package-batch-f-freeform.step");
    using (XdeDocument batchFDocument = XdeDocument.Create())
    {
        using XdeTransaction transaction = batchFDocument.BeginTransaction();
        XdeLabel label = batchFDocument.AddShape(batchFSmoothLoft.Shape, "Package Batch F Freeform Loft");
        label.Color = new XdeColor(0.18, 0.55, 0.86, 1.0);
        if (!transaction.Commit()) throw new InvalidOperationException("The packaged Batch F XDE transaction failed.");
        batchFDocument.WriteStep(batchFStep);
    }
    using XdeDocument batchFImportedDocument = XdeDocument.ReadStep(batchFStep);
    XdeLabel batchFImportedLabel = batchFImportedDocument.GetFreeShapes().Single();
    using Shape batchFImportedShape = batchFImportedLabel.Shape;
    DetailedMeshSnapshot batchFMesh = batchFImportedShape.CreateDetailedMesh(0.25, 0.5);
    ShapeInspectionProperties batchFMass = batchFImportedShape.InspectProperties(InspectionPropertyKind.Volume);
    if (!batchFImportedShape.IsValid || batchFImportedShape.CountSubShapes(ShapeKind.Face) < 3
        || batchFMesh.Vertices.Count == 0 || batchFMesh.Triangles.Count == 0 || batchFMass.Mass <= 0)
        throw new InvalidOperationException("The packaged Batch F STEP/XDE/mesh/measurement workflow failed.");

    nint batchFWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch F package freeform", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchFWindow == 0) throw new InvalidOperationException("The Batch F package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchFWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchFWindow);
        using OcctViewer batchFViewer = OcctViewer.Create(batchFWindow);
        using ViewerPresentation batchFPresentation = batchFViewer.Display(batchFImportedShape);
        batchFPresentation.SetSelectionKind(ShapeKind.Face);
        batchFViewer.FitAll();
        batchFViewer.Redraw();
        if (batchFViewer.SelectRectangle(0, 0, 319, 319).Count == 0)
            throw new InvalidOperationException("The packaged Batch F real-HWND selection workflow failed.");
        string batchFImage = batchFViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-f-freeform.png"), overwrite: true);
        if (!File.Exists(batchFImage) || new FileInfo(batchFImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch F screenshot workflow failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchFWindow); }

    using Shape batchGSection = TechnicalDrawing.CreateSection(
        batchFImportedShape, GpPlane.Create(new GpXyz(0, 0, 4), new GpXyz(0, 0, 1)));
    using DrawingView batchGExact = TechnicalDrawing.CreateView(
        batchFImportedShape, DrawingProjection.Isometric,
        new DrawingOptions { Algorithm = DrawingAlgorithm.Exact, IsoparameterCount = 1, SamplesPerCurve = 16 });
    using DrawingView batchGPolygonal = TechnicalDrawing.CreateView(
        batchFImportedShape,
        new DrawingProjection(new GpXyz(30, -50, 25), new GpXyz(-30, 50, -20), new GpXyz(0, 0, 1), true, 60),
        new DrawingOptions { Algorithm = DrawingAlgorithm.Polygonal, Deflection = 0.25, SamplesPerCurve = 12 });
    using StandardDrawingViews batchGViews = TechnicalDrawing.CreateStandardViews([batchFImportedShape]);
    IReadOnlyList<DrawingPolyline> batchGVisible = TechnicalDrawing.CopyPolylines(
        batchGExact.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape);
    string batchGSvg = Path.Combine(exchangeDirectory, "package-batch-g-drawing.svg");
    batchGExact.SaveSvg(batchGSvg, new SvgDrawingOptions { Width = 800, Height = 600 });
    if (batchGVisible.Count == 0 || batchGSection.CountSubShapes(ShapeKind.Edge) == 0
        || batchGPolygonal.Layers.Count != 10 || batchGViews.All.Count != 4
        || !File.Exists(batchGSvg) || new FileInfo(batchGSvg).Length <= 100)
        throw new InvalidOperationException("The packaged Batch G HLR/section/vector drawing workflow failed.");

    nint batchGWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch G package drawing", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchGWindow == 0) throw new InvalidOperationException("The Batch G package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchGWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchGWindow);
        using OcctViewer batchGViewer = OcctViewer.Create(batchGWindow);
        using ViewerPresentation batchGPresentation = batchGViewer.Display(
            batchGExact.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape);
        batchGViewer.FitAll(); batchGViewer.Redraw();
        string batchGImage = batchGViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-g-drawing.png"), overwrite: true);
        if (!File.Exists(batchGImage) || new FileInfo(batchGImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch G real-HWND drawing screenshot failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchGWindow); }

    AdvancedMeshSnapshot batchHMesh = AdvancedMesh.Create(
        batchFImportedShape,
        new AdvancedMeshOptions { LinearDeflection = 0.12, AngularDeflection = 0.35 });
    AdvancedMeshLodSet batchHLods = AdvancedMesh.CreateLods(batchFImportedShape, [0.08, 0.3, 1.0]);
    if (batchHMesh.Groups.Count == 0 || batchHMesh.Statistics.TriangleCount == 0
        || batchHMesh.Diagnostics.ConnectedComponentCount == 0 || batchHLods.Levels.Count != 3
        || batchHLods.Levels[0].Mesh.Statistics.TriangleCount < batchHLods.Levels[2].Mesh.Statistics.TriangleCount)
        throw new InvalidOperationException("The packaged Batch H grouped mesh/statistics/diagnostics/LOD workflow failed.");

    MeshScene batchHScene;
    string batchHGltf = Path.Combine(exchangeDirectory, "package-batch-h-scene.gltf");
    string batchHGlb = Path.Combine(exchangeDirectory, "package-batch-h-scene.glb");
    string batchHObj = Path.Combine(exchangeDirectory, "package-batch-h-scene.obj");
    string batchHPly = Path.Combine(exchangeDirectory, "package-batch-h-scene.ply");
    string batchHVrml = Path.Combine(exchangeDirectory, "package-batch-h-scene.wrl");
    using (XdeDocument batchHDocument = XdeDocument.Create())
    {
        using (XdeTransaction transaction = batchHDocument.BeginTransaction())
        {
            XdeLabel part = batchHDocument.AddShape(batchFImportedShape, "Package Batch H Shared Part");
            part.Color = new XdeColor(0.12, 0.55, 0.78, 1);
            part.SetLayer("Package Mesh Scene");
            part.Material = new XdeMaterial("Aluminum", "Package Batch H", 2.7, "Density", "g/cm3");
            part.VisualMaterial = new XdeVisualMaterial(
                "Package Batch H PBR", new XdeColor(0.12, 0.55, 0.78, 1),
                0.5, 0.25, GpXyz.Origin);
            XdeLabel assembly = batchHDocument.AddAssembly("Package Batch H Assembly");
            using (TopLocLocation identity = TopLocLocation.Identity)
                _ = batchHDocument.AddComponent(assembly, part, identity);
            using (GpTrsf transform = GpTrsf.Create(25, 0, 0))
            using (TopLocLocation batchHLocation = TopLocLocation.FromTransform(transform))
                _ = batchHDocument.AddComponent(assembly, part, batchHLocation);
            if (!transaction.Commit())
                throw new InvalidOperationException("The packaged Batch H XDE transaction failed.");
        }

        batchHScene = MeshScene.FromXdeDocument(batchHDocument);
        batchHDocument.WriteGltf(batchHGltf);
        batchHDocument.WriteGltf(batchHGlb);
        batchHDocument.WriteObj(batchHObj);
        batchHDocument.WritePly(batchHPly);
        batchHDocument.WriteVrml(batchHVrml);
    }
    if (batchHScene.Definitions.Count != 1 || batchHScene.InstanceCount != 2
        || batchHScene.Nodes.Count != 3 || batchHScene.TotalTriangleCount == 0
        || batchHScene.Nodes.Count(node => node.VisualMaterial is not null) != 2)
        throw new InvalidOperationException("The packaged Batch H copied hierarchy/material/shared-instance scene failed.");
    foreach (string path in new[] { batchHGltf, batchHGlb, batchHObj, batchHPly, batchHVrml })
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new InvalidOperationException($"The packaged Batch H interchange output is empty: '{path}'.");
    if (MeshScene.ReadGltf(batchHGltf).TotalTriangleCount == 0
        || MeshScene.ReadGltf(batchHGlb).TotalTriangleCount == 0
        || MeshScene.ReadObj(batchHObj).TotalTriangleCount == 0)
        throw new InvalidOperationException("The packaged Batch H glTF/GLB/OBJ read-back workflow failed.");

    nint batchHWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch H package scene", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchHWindow == 0) throw new InvalidOperationException("The Batch H package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchHWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchHWindow);
        using OcctViewer batchHViewer = OcctViewer.Create(batchHWindow);
        using ViewerPresentation batchHPresentation = batchHViewer.Display(batchFImportedShape);
        batchHViewer.FitAll(); batchHViewer.Redraw();
        string batchHImage = batchHViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-h-scene.png"), overwrite: true);
        if (!File.Exists(batchHImage) || new FileInfo(batchHImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch H real-HWND scene screenshot failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchHWindow); }

    string batchIGenericBin = Path.Combine(exchangeDirectory, "package-batch-i-generic.cbf");
    string batchIGenericXml = Path.Combine(exchangeDirectory, "package-batch-i-generic.xml");
    using (OcafDocument batchIGeneric = OcafDocument.Create())
    {
        batchIGeneric.UndoLimit = -1;
        using (OcafTransaction transaction = batchIGeneric.BeginTransaction("Package Batch I generic state"))
        {
            OcafLabel label = batchIGeneric.RootLabel.AddChild();
            label.Name = "Package 文档";
            label.Comment = "Copied state";
            label.IntegerValue = 801;
            label.SetRealArray(-1, [1.5, 3.0]);
            label.Reference = batchIGeneric.RootLabel;
            if (!transaction.Commit())
                throw new InvalidOperationException("The packaged Batch I generic command did not create history.");
        }
        if (batchIGeneric.UndoHistory.Single().Name != "Package Batch I generic state"
            || !batchIGeneric.CreateDependencyGraph().Edges.Any(static edge =>
                edge.Kind == DocumentDependencyEdgeKind.DirectReference))
            throw new InvalidOperationException("The packaged Batch I generic snapshot/graph/history workflow failed.");
        batchIGeneric.MarkSaved();
        if (!batchIGeneric.Undo() || !batchIGeneric.IsChanged || !batchIGeneric.Redo() || batchIGeneric.IsChanged)
            throw new InvalidOperationException("The packaged Batch I undo/redo/savepoint workflow failed.");
        batchIGeneric.Save(batchIGenericBin, DocumentStorageFormat.BinOcaf);
        batchIGeneric.Save(batchIGenericXml, DocumentStorageFormat.XmlOcaf);
    }
    using (OcafDocument batchIGenericBinary = OcafDocument.Open(batchIGenericBin))
    using (OcafDocument batchIGenericXmlReloaded = OcafDocument.Open(batchIGenericXml))
    using (DocumentSnapshot batchIGenericBinarySnapshot = batchIGenericBinary.CreateSnapshot())
    using (DocumentSnapshot batchIGenericXmlSnapshot = batchIGenericXmlReloaded.CreateSnapshot())
    {
        bool binaryHasState = batchIGenericBinarySnapshot.Labels.Any(static label => label.Attributes.Any(static attribute =>
            attribute.Kind == DocumentAttributeKind.IntegralValue && attribute.IntegerValue == 801));
        bool xmlHasState = batchIGenericXmlSnapshot.Labels.Any(static label => label.Attributes.Any(static attribute =>
            attribute.Kind == DocumentAttributeKind.IntegralValue && attribute.IntegerValue == 801));
        if (batchIGenericBinary.IsChanged || batchIGenericXmlReloaded.IsChanged || !binaryHasState || !xmlHasState)
            throw new InvalidOperationException("The packaged Batch I BinOcaf/XmlOcaf round trip failed.");
    }

    string batchIXdeBin = Path.Combine(exchangeDirectory, "package-batch-i-scene.xbf");
    string batchIXdeXml = Path.Combine(exchangeDirectory, "package-batch-i-scene.xml");
    string batchIOutputStep = Path.Combine(exchangeDirectory, "package-batch-i-output.step");
    Shape batchIOwningCopy;
    using (XdeDocument batchIDocument = XdeDocument.ReadStep(batchFStep))
    {
        XdeLabel imported = batchIDocument.GetFreeShapes().Single();
        using (XdeTransaction transaction = batchIDocument.BeginTransaction("Package Batch I STEP mutation"))
        {
            imported.Name = "Package Batch I Imported Root";
            imported.Comment = "Persistent history";
            imported.Reference = imported;
            if (!transaction.Commit())
                throw new InvalidOperationException("The packaged Batch I STEP mutation did not create history.");
        }
        DocumentDependencyGraph batchIGraph = batchIDocument.CreateDependencyGraph();
        if (batchIDocument.UndoHistory.Single().Name != "Package Batch I STEP mutation"
            || batchIGraph.IsAcyclic
            || !batchIGraph.GetOutgoing(imported.Entry).Any(static edge =>
                edge.Kind == DocumentDependencyEdgeKind.DirectReference))
            throw new InvalidOperationException("The packaged Batch I graph/history diagnostics failed.");
        batchIOwningCopy = imported.Shape;
        batchIDocument.Save(batchIXdeBin, DocumentStorageFormat.BinXcaf);
        batchIDocument.Save(batchIXdeXml, DocumentStorageFormat.XmlXcaf);
    }
    using (batchIOwningCopy)
    using (XdeDocument batchIBinaryReloaded = XdeDocument.Open(batchIXdeBin))
    using (XdeDocument batchIReloaded = XdeDocument.Open(batchIXdeXml))
    {
        XdeLabel binaryImported = batchIBinaryReloaded.GetFreeShapes().Single();
        XdeLabel imported = batchIReloaded.GetFreeShapes().Single();
        using Shape binaryShape = binaryImported.Shape;
        batchIReloaded.WriteStep(batchIOutputStep);
        if (batchIOwningCopy.Kind != ShapeKind.Solid
            || binaryShape.Kind != ShapeKind.Solid
            || binaryImported.Name != "Package Batch I Imported Root"
            || binaryImported.Comment != "Persistent history"
            || imported.Name != "Package Batch I Imported Root"
            || imported.Comment != "Persistent history"
            || !File.Exists(batchIOutputStep)
            || new FileInfo(batchIOutputStep).Length == 0)
            throw new InvalidOperationException("The packaged Batch I persistence/source-disposal/STEP export failed.");
    }

    FeatureModelingOptions batchJOptions = new()
    {
        FuzzyTolerance = 1e-7,
        RunParallel = true,
        NonDestructive = true,
        RepairInputs = true,
        UnifyResult = true
    };
    using Shape batchJSource = ShapeFactory.CreateBox(24, 18, 12);
    Shape[] batchJEdges = batchJSource.GetSubShapes(ShapeKind.Edge);
    FeatureOperationResult batchJFillet;
    try
    {
        batchJFillet = FeatureModeling.VariableFillet(
            batchJSource, [batchJEdges[0]], 0.5, 1.25, batchJOptions);
    }
    finally
    {
        foreach (Shape edge in batchJEdges) edge.Dispose();
    }
    using (batchJFillet)
    using (Shape batchJToolBase = ShapeFactory.CreateBox(8, 8, 16))
    using (Shape batchJTool = batchJToolBase.Transformed(ShapeTransform.CreateTranslation(10, 5, 0)))
    using (FeatureOperationResult batchJPreflight = FeatureModeling.Preflight(
        batchJFillet.RequireShape(), batchJTool, FeatureBooleanOperation.Fuse))
    using (FeatureOperationResult batchJFused = FeatureModeling.Boolean(
        FeatureBooleanOperation.Fuse, [batchJFillet.RequireShape()], [batchJTool], batchJOptions))
    using (FeatureOperationResult batchJHoled = FeatureModeling.CutHole(
        batchJFused.RequireShape(), new GpXyz(6, 9, 20), new GpXyz(0, 0, -1),
        2.0, 30, throughAll: true, batchJOptions))
    {
        if (!batchJFillet.Diagnostics.Succeeded || batchJFillet.History.Count == 0
            || !batchJPreflight.Diagnostics.Succeeded || !batchJFused.Diagnostics.Succeeded
            || batchJFused.History.Count == 0 || !batchJHoled.Diagnostics.Succeeded
            || !batchJHoled.RequireShape().IsValid)
            throw new InvalidOperationException("The packaged Batch J feature/options/preflight/history workflow failed.");

        batchJSource.Dispose();
        if (!batchJFillet.RequireShape().IsValid
            || batchJFillet.History.Any(static item => !item.Shape.IsValid))
            throw new InvalidOperationException("The packaged Batch J owning result/history lifetime failed.");

        string batchJStep = Path.Combine(exchangeDirectory, "package-batch-j-feature.step");
        using (XdeDocument batchJDocument = XdeDocument.Create())
        {
            using XdeTransaction transaction = batchJDocument.BeginTransaction("Package Batch J feature");
            XdeLabel label = batchJDocument.AddShape(batchJHoled.RequireShape(), "Package Batch J Feature");
            label.Color = new XdeColor(0.28, 0.62, 0.88, 1.0);
            if (!transaction.Commit())
                throw new InvalidOperationException("The packaged Batch J XDE transaction failed.");
            batchJDocument.WriteStep(batchJStep);
        }
        using XdeDocument batchJImported = XdeDocument.ReadStep(batchJStep);
        using Shape batchJImportedShape = batchJImported.GetFreeShapes().Single().Shape;
        if (!batchJImportedShape.IsValid)
            throw new InvalidOperationException("The packaged Batch J STEP/XDE round trip failed.");

        nint batchJWindow = PackageWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch J package feature", 0x80000000u,
            -32000, -32000, 320, 320, 0, 0, 0, 0);
        if (batchJWindow == 0)
            throw new InvalidOperationException("The Batch J package HWND could not be created.");
        try
        {
            _ = PackageWindowMethods.ShowWindow(batchJWindow, 4);
            _ = PackageWindowMethods.UpdateWindow(batchJWindow);
            using OcctViewer batchJViewer = OcctViewer.Create(batchJWindow);
            using ViewerPresentation batchJPresentation = batchJViewer.Display(batchJImportedShape);
            foreach (FeatureHistoryItem item in batchJFillet.Generated.Take(2))
                using (batchJViewer.Display(item.Shape)) { }
            batchJViewer.FitAll();
            batchJViewer.Redraw();
            string batchJImage = batchJViewer.SaveScreenshot(
                Path.Combine(exchangeDirectory, "package-batch-j-feature.png"), overwrite: true);
            if (!File.Exists(batchJImage) || new FileInfo(batchJImage).Length == 0)
                throw new InvalidOperationException("The packaged Batch J real-HWND screenshot failed.");
        }
        finally
        {
            _ = PackageWindowMethods.DestroyWindow(batchJWindow);
        }
    }

    string batchKStep = Path.Combine(exchangeDirectory, "package-batch-k-assembly.step");
    using Shape batchKPartShape = ShapeFactory.CreateBox(4, 5, 6);
    using Shape batchKAlternativeShape = ShapeFactory.CreateCylinder(2, 7);
    using Shape batchKReplacementShape = ShapeFactory.CreateBox(8, 9, 10);
    using XdeDocument batchKDocument = XdeDocument.Create();
    batchKDocument.UndoLimit = 16;
    XdeLabel batchKPart;
    XdeLabel batchKAlternative;
    XdeLabel batchKRoot;
    XdeLabel batchKNestedOccurrence;
    XdeLabel batchKSubassemblyOccurrence;
    XdeLabel batchKDirectOccurrence;
    using (XdeTransaction transaction = batchKDocument.BeginTransaction("Package Batch K assembly"))
    {
        batchKPart = batchKDocument.AddShape(batchKPartShape, "Package Shared Part");
        batchKPart.Color = new XdeColor(0.2, 0.55, 0.85, 1);
        batchKPart.SetLayer("Package Mechanical");
        batchKPart.Material = new XdeMaterial("Package Steel", "Batch K", 2, "Density", "u/mm3");
        batchKAlternative = batchKDocument.AddShape(batchKAlternativeShape, "Package Alternative");
        XdeLabel nested = batchKDocument.AddAssembly("Package Nested Assembly");
        using (GpTrsf nestedPartTransform = GpTrsf.Create(2, 3, 4))
        using (TopLocLocation nestedPartLocation = TopLocLocation.FromTransform(nestedPartTransform))
            batchKNestedOccurrence = batchKDocument.AddComponent(nested, batchKPart, nestedPartLocation);
        batchKRoot = batchKDocument.AddAssembly("Package Root Assembly");
        using (GpTrsf nestedTransform = GpTrsf.Create(10, 0, 0))
        using (TopLocLocation nestedLocation = TopLocLocation.FromTransform(nestedTransform))
            batchKSubassemblyOccurrence = batchKDocument.AddComponent(batchKRoot, nested, nestedLocation);
        using (GpTrsf directTransform = GpTrsf.Create(30, 0, 0))
        using (TopLocLocation directLocation = TopLocLocation.FromTransform(directTransform))
            batchKDirectOccurrence = batchKDocument.AddComponent(batchKRoot, batchKPart, directLocation);
        batchKDocument.SetOccurrenceMetadata(batchKDirectOccurrence, new AssemblyEffectiveMetadata(
            "Package Override", new XdeColor(0.8, 0.2, 0.1, 1), ["Package Override Layer"],
            new XdeMaterial("Package Dense Steel", "Occurrence", 3, "Density", "u/mm3"), null));
        batchKDocument.SetExternalReferences(batchKDirectOccurrence,
            ["parts/package-shared.step", "urn:occtsharp:package-batch-k"]);
        _ = batchKDocument.SetAssemblyItemReference(
            batchKRoot, [batchKSubassemblyOccurrence.Entry, batchKNestedOccurrence.Entry]);
        _ = batchKDocument.CreateShuo([batchKSubassemblyOccurrence, batchKNestedOccurrence]);
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch K initial transaction failed.");
    }

    using (AssemblyOccurrenceResolution resolution = batchKDocument.ResolveOccurrencePath(
               batchKRoot, [batchKSubassemblyOccurrence.Entry, batchKNestedOccurrence.Entry]))
    {
        AssemblyStructureSnapshot graph = batchKDocument.CreateAssemblyStructureSnapshot(batchKRoot);
        AssemblyBomReport structured = batchKDocument.CreateBom(batchKRoot);
        AssemblyBomReport flattened = batchKDocument.CreateBom(batchKRoot, flattened: true);
        AssemblyPropertyRollup rollup = batchKDocument.GetAssemblyPropertyRollup(batchKRoot);
        AssemblyEffectiveMetadata effective = batchKDocument.GetEffectiveMetadata(batchKDirectOccurrence);
        AssemblyItemReference? itemReference = batchKDocument.GetAssemblyItemReference(batchKRoot);
        if (!resolution.LocatedShape.IsValid || graph.Nodes.Count != 6 || graph.Links.Count != 6
            || graph.Diagnostics.Count != 1 || structured.Items.Count != 3 || flattened.Items.Count != 2
            || flattened.Items.Single(item => item.DefinitionEntry == batchKPart.Entry).Quantity != 2
            || batchKDocument.GetWhereUsed(batchKPart).Count != 2 || rollup.OccurrenceCount != 2
            || Math.Abs(rollup.Mass - 600) > 1e-8 || effective.Name != "Package Override"
            || effective.Material?.Name != "Package Dense Steel"
            || itemReference is null || itemReference.Path.Count != 2
            || batchKDocument.GetExternalReferences(batchKDirectOccurrence).Count != 2)
            throw new InvalidOperationException("The packaged Batch K graph/BOM/reference/metadata/rollup workflow failed.");
    }

    string batchKCloneEntry;
    string batchKMovedEntry;
    using (XdeTransaction transaction = batchKDocument.BeginTransaction("Package Batch K structural edit"))
    {
        XdeLabel batchKClone = batchKDocument.CloneSubtree(batchKRoot, "Package Independent Clone");
        batchKCloneEntry = batchKClone.Entry;
        using GpTrsf relocatedTransform = GpTrsf.Create(11, 12, 13);
        using TopLocLocation relocatedLocation = TopLocLocation.FromTransform(relocatedTransform);
        XdeLabel relocated = batchKDocument.RelocateOccurrence(batchKDirectOccurrence, relocatedLocation);
        XdeLabel relinked = batchKDocument.ReplaceOccurrence(relocated, batchKAlternative);
        XdeLabel moved = batchKDocument.ReparentOccurrence(relinked, batchKClone);
        batchKMovedEntry = moved.Entry;
        batchKDocument.UpdateDefinitionShape(batchKAlternative, batchKReplacementShape);
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch K structural transaction failed.");
    }
    if (batchKRoot.GetComponents().Count != 1
        || batchKDocument.GetLabel(batchKCloneEntry).GetComponents().Count != 3
        || batchKDocument.UndoHistory[0].Name != "Package Batch K structural edit"
        || !batchKDocument.Undo() || !batchKDocument.Redo())
        throw new InvalidOperationException("The packaged Batch K edit/undo/redo workflow failed.");
    using (XdeTransaction aborted = batchKDocument.BeginTransaction("Package Batch K abort"))
    {
        batchKDocument.RemoveOccurrence(batchKDocument.GetLabel(batchKMovedEntry));
        aborted.Abort();
    }
    if (batchKDocument.GetLabel(batchKCloneEntry).GetComponents().Count != 3)
        throw new InvalidOperationException("The packaged Batch K aborted edit was not rolled back.");

    batchKDocument.WriteStep(batchKStep);
    using XdeDocument batchKImported = XdeDocument.ReadStep(batchKStep);
    XdeLabel batchKImportedRoot = batchKImported.GetFreeShapes().Single(label => label.Name == "Package Root Assembly");
    nint batchKWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch K package assembly", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchKWindow == 0)
        throw new InvalidOperationException("The Batch K package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchKWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchKWindow);
        using OcctViewer batchKViewer = OcctViewer.Create(batchKWindow);
        IReadOnlyList<AssemblyViewerPresentation> batchKPresentations =
            batchKImported.DisplayAssembly(batchKImportedRoot, batchKViewer);
        try
        {
            batchKViewer.FitAll();
            batchKViewer.Redraw();
            string batchKImage = batchKViewer.SaveScreenshot(
                Path.Combine(exchangeDirectory, "package-batch-k-assembly.png"), overwrite: true);
            if (batchKPresentations.Count == 0 || !File.Exists(batchKImage)
                || new FileInfo(batchKImage).Length == 0)
                throw new InvalidOperationException("The packaged Batch K STEP/HWND occurrence review failed.");
        }
        finally
        {
            foreach (AssemblyViewerPresentation presentation in batchKPresentations) presentation.Dispose();
        }
    }
    finally
    {
        _ = PackageWindowMethods.DestroyWindow(batchKWindow);
    }

    string batchLStep = Path.Combine(exchangeDirectory, "package-batch-l-dmu.step");
    DigitalMockupReport batchLReport;
    using (Shape batchLPartShape = ShapeFactory.CreateBox(8, 4, 3))
    using (XdeDocument batchLDocument = XdeDocument.Create())
    {
        using XdeTransaction transaction = batchLDocument.BeginTransaction("Package Batch L DMU");
        XdeLabel part = batchLDocument.AddShape(batchLPartShape, "Package DMU Part");
        XdeLabel root = batchLDocument.AddAssembly("Package DMU Root");
        using GpTrsf firstTransform = GpTrsf.Create(0, 0, 0);
        using GpTrsf secondTransform = GpTrsf.Create(6, 0, 0);
        using TopLocLocation firstLocation = TopLocLocation.FromTransform(firstTransform);
        using TopLocLocation secondLocation = TopLocLocation.FromTransform(secondTransform);
        batchLDocument.AddComponent(root, part, firstLocation);
        batchLDocument.AddComponent(root, part, secondLocation);
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch L XDE transaction failed.");
        batchLReport = DigitalMockupAnalyzer.AnalyzeAssembly(root, new DigitalMockupPolicy
        {
            Clearance = 0.5,
            RunParallel = true,
            NonDestructive = true,
            ExactDistanceForAllPairs = true
        });
        batchLDocument.WriteStep(batchLStep);
    }
    using (batchLReport)
    {
        DigitalMockupPairResult pair = batchLReport.Pairs.Single();
        if (batchLReport.Items.Count != 2 || batchLReport.Summary.ExactPairCount != 1
            || pair.State != DigitalMockupPairState.Interfering || pair.OverlapVolume <= 0
            || pair.IssueTopology is null || !pair.IssueTopology.IsValid
            || batchLReport.Items.Any(item => item.OrientedBounds is null || item.OccurrencePath.Count == 0))
            throw new InvalidOperationException("The packaged Batch L bounds/pair-matrix/penetration/ownership workflow failed.");

        using XdeDocument batchLImported = XdeDocument.ReadStep(batchLStep);
        using DigitalMockupReport batchLReread = DigitalMockupAnalyzer.AnalyzeAssembly(
            batchLImported.GetFreeShapes().Single(),
            new DigitalMockupPolicy { ExactDistanceForAllPairs = true });
        if (batchLReread.Items.Count != 2 || batchLReread.Pairs.Count != 1)
            throw new InvalidOperationException("The packaged Batch L STEP/XDE traceability workflow failed.");

        nint batchLWindow = PackageWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch L package DMU", 0x80000000u,
            -32000, -32000, 320, 320, 0, 0, 0, 0);
        if (batchLWindow == 0)
            throw new InvalidOperationException("The Batch L package HWND could not be created.");
        try
        {
            _ = PackageWindowMethods.ShowWindow(batchLWindow, 4);
            _ = PackageWindowMethods.UpdateWindow(batchLWindow);
            using OcctViewer batchLViewer = OcctViewer.Create(batchLWindow);
            using DigitalMockupReviewSession review = DigitalMockupReviewSession.Display(batchLReport, batchLViewer);
            DigitalMockupPairId issue = review.IssueIds.Single();
            review.EnableSelection(issue, ShapeKind.Face);
            review.Isolate([issue]);
            string image = review.SaveKeyedScreenshot(
                Path.Combine(exchangeDirectory, "package-batch-l-dmu.png"), [issue], overwrite: true);
            if (!File.Exists(image) || new FileInfo(image).Length == 0)
                throw new InvalidOperationException("The packaged Batch L real-HWND issue review failed.");
        }
        finally { _ = PackageWindowMethods.DestroyWindow(batchLWindow); }
    }

    using Shape batchLIncrementalFirst = ShapeFactory.CreateBox(3, 3, 3);
    using Shape batchLIncrementalBase = ShapeFactory.CreateBox(3, 3, 3);
    using Shape batchLIncrementalOld = batchLIncrementalBase.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
    using Shape batchLIncrementalFarBase = ShapeFactory.CreateBox(1, 1, 1);
    using Shape batchLIncrementalFar = batchLIncrementalFarBase.Transformed(ShapeTransform.CreateTranslation(50, 0, 0));
    using DigitalMockupReport batchLBaseline = DigitalMockupAnalyzer.Analyze(
        [new("A", batchLIncrementalFirst), new("B", batchLIncrementalOld), new("C", batchLIncrementalFar)],
        new DigitalMockupPolicy { ExactDistanceForAllPairs = true });
    using Shape batchLIncrementalNew = batchLIncrementalBase.Transformed(ShapeTransform.CreateTranslation(2, 0, 0));
    using DigitalMockupReport batchLIncremental = DigitalMockupAnalyzer.AnalyzeIncremental(
        batchLBaseline,
        [new("A", batchLIncrementalFirst), new("B", batchLIncrementalNew), new("C", batchLIncrementalFar)],
        ["B"]);
    if (batchLIncremental.Summary.ReusedPairCount != 1
        || batchLIncremental.PairById[new DigitalMockupPairId("A", "B")].State != DigitalMockupPairState.Interfering)
        throw new InvalidOperationException("The packaged Batch L stable-ID incremental workflow failed.");

    string batchMStep = Path.Combine(exchangeDirectory, "package-batch-m-placement.step");
    using Shape batchMShape = ShapeFactory.CreateBox(10, 10, 10);
    using XdeDocument batchMDocument = XdeDocument.Create();
    batchMDocument.UndoLimit = 8;
    XdeLabel batchMRoot;
    XdeLabel batchMMoving;
    using (XdeTransaction transaction = batchMDocument.BeginTransaction("Package Batch M assembly"))
    {
        XdeLabel part = batchMDocument.AddShape(batchMShape, "Package Batch M Part");
        batchMRoot = batchMDocument.AddAssembly("Package Batch M Root");
        using GpTrsf firstTransform = GpTrsf.Create(0, 0, 0);
        using GpTrsf secondTransform = GpTrsf.Create(30, 0, 0);
        using TopLocLocation firstLocation = TopLocLocation.FromTransform(firstTransform);
        using TopLocLocation secondLocation = TopLocLocation.FromTransform(secondTransform);
        batchMMoving = batchMDocument.AddComponent(batchMRoot, part, firstLocation);
        _ = batchMDocument.AddComponent(batchMRoot, part, secondLocation);
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch M initial transaction failed.");
    }
    nint batchMWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch M package placement", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchMWindow == 0)
        throw new InvalidOperationException("The Batch M package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchMWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchMWindow);
        using OcctViewer batchMViewer = OcctViewer.Create(batchMWindow);
        IReadOnlyList<XdeOccurrence> batchMOccurrences = batchMRoot.GetOccurrences();
        using XdeOccurrence occurrence = batchMOccurrences
            .Single(item => item.OccurrenceLabel.Entry == batchMMoving.Entry);
        foreach (XdeOccurrence other in batchMOccurrences)
            if (!ReferenceEquals(other, occurrence)) other.Dispose();
        using ViewerPresentation presentation = batchMViewer.Display(occurrence);
        using ViewerManipulator manipulator = presentation.CreateManipulator(new()
        {
            EnabledModes = ViewerManipulatorModes.Rigid,
            ActivationOnDetection = true,
            Skin = ViewerManipulatorSkin.Flat,
            Size = 120,
            Gap = 12
        });
        manipulator.SetPart(ViewerManipulatorAxis.X, ViewerManipulatorMode.Translation, true);
        manipulator.EnableMode(ViewerManipulatorMode.Translation);
        manipulator.EnableMode(ViewerManipulatorMode.Rotation);
        manipulator.EnableMode(ViewerManipulatorMode.TranslationPlane);
        manipulator.Start(160, 160);
        using GpTrsf customPreview = GpTrsf.Create(5, 0, 0);
        manipulator.Preview(customPreview);
        manipulator.Stop(apply: false);
        using (GpTrsf cancelled = presentation.GetTransform())
            if (Math.Abs(cancelled.Value(1, 4)) > 1e-8)
                throw new InvalidOperationException("The packaged Batch M manipulator cancel failed.");

        using GpTrsf placement = GpTrsf.Create(25, 0, 0);
        using (XdePlacementEditSession edit = batchMDocument.BeginPlacementEdit(
                   batchMMoving, presentation, "Package Batch M placement"))
        {
            edit.Preview(placement);
            batchMMoving = edit.Commit();
        }
        if (batchMDocument.UndoHistory[0].Name != "Package Batch M placement"
            || presentation.SourceIdentity?.OccurrenceEntry != batchMMoving.Entry)
            throw new InvalidOperationException("The packaged Batch M replacement/history identity failed.");
        using (DigitalMockupReport report = DigitalMockupAnalyzer.AnalyzeAssembly(
                   batchMRoot, new DigitalMockupPolicy { ExactDistanceForAllPairs = true }))
            if (report.Pairs.Single().State != DigitalMockupPairState.Interfering)
                throw new InvalidOperationException("The packaged Batch M post-move DMU recheck failed.");
        if (!batchMDocument.Undo() || !batchMDocument.Redo())
            throw new InvalidOperationException("The packaged Batch M undo/redo workflow failed.");
        batchMDocument.WriteStep(batchMStep);
        using XdeDocument batchMImported = XdeDocument.ReadStep(batchMStep);
        IReadOnlyList<XdeOccurrence> batchMImportedOccurrences =
            batchMImported.GetFreeShapes().Single().GetOccurrences();
        try
        {
            if (batchMImportedOccurrences.Count != 2)
                throw new InvalidOperationException("The packaged Batch M STEP/XDE round trip failed.");
        }
        finally { foreach (XdeOccurrence item in batchMImportedOccurrences) item.Dispose(); }
        manipulator.Dispose();
        batchMViewer.FitAll();
        string batchMImage = batchMViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-m-placement.png"), overwrite: true);
        if (!File.Exists(batchMImage) || new FileInfo(batchMImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch M real-HWND screenshot failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchMWindow); }

    string batchNUnicodeDirectory = Path.Combine(exchangeDirectory, "BatchN-颜色");
    Directory.CreateDirectory(batchNUnicodeDirectory);
    string batchNIges = Path.Combine(batchNUnicodeDirectory, "package-batch-n-蓝色.iges");
    using Shape batchNShape = ShapeFactory.CreateCylinder(4, 14);
    using (XdeDocument batchNSource = XdeDocument.Create())
    {
        using XdeTransaction transaction = batchNSource.BeginTransaction("Package Batch N metadata");
        XdeLabel part = batchNSource.AddShape(batchNShape, "Package Batch N Part");
        part.Color = new XdeColor(0.12, 0.42, 0.88, 1);
        part.SetLayer("Package Batch N Layer");
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch N metadata transaction failed.");
        batchNSource.WriteExchange(batchNIges, XdeExchangeFormat.Iges);
    }
    using XdeDocument batchNImported = XdeDocument.ReadIges(
        batchNIges,
        new XdeIgesReadOptions(ReadNames: true, ReadColors: true, ReadLayers: true),
        out XdeIgesReadReport batchNReport);
    XdeLabel batchNRoot = batchNImported.GetFreeShapes().Single();
    if (batchNReport.SourceEntityCount <= 0 || batchNReport.TransferredRootCount != 1
        || batchNReport.SourceLengthUnitMeters <= 0 || batchNReport.SystemLengthUnitMillimeters <= 0
        || !(batchNRoot.Name?.Contains("Batch N", StringComparison.OrdinalIgnoreCase) ?? false))
        throw new InvalidOperationException("The packaged Batch N IGES diagnostics/name workflow failed.");
    IReadOnlyList<XdePresentationStyle> batchNStyles = batchNRoot.GetPresentationStyles();
    try
    {
        if (!batchNStyles.Any(style => style.EffectiveColor is not null))
            throw new InvalidOperationException("The packaged Batch N IGES color workflow failed.");
    }
    finally { foreach (XdePresentationStyle style in batchNStyles) style.Dispose(); }
    using XdeDocument batchNComposition = XdeDocument.Create();
    using (XdeTransaction transaction = batchNComposition.BeginTransaction("Package Batch N mixed import"))
    {
        if (batchNComposition.ImportExchange(batchNIges).Count != 1
            || batchNComposition.ImportExchange(batchMStep, XdeExchangeFormat.Step).Count != 1)
            throw new InvalidOperationException("The packaged Batch N mixed STEP/IGES import failed.");
        if (!transaction.Commit())
            throw new InvalidOperationException("The packaged Batch N mixed import transaction failed.");
    }
    string batchNRoundTrip = batchNComposition.WriteExchange(
        Path.Combine(batchNUnicodeDirectory, "package-batch-n-roundtrip.igs"));
    using XdeDocument batchNReread = XdeDocument.ReadExchange(batchNRoundTrip);
    if (batchNReread.GetFreeShapes().Length != 2)
        throw new InvalidOperationException("The packaged Batch N mixed IGES round trip failed.");

    SketchCurve2d batchOBottom = SketchCurve2d.Segment(new(0, 0), new(18, 0));
    SketchCurve2d batchORight = SketchCurve2d.Bezier([new(18, 0), new(18, 4), new(18, 9)]);
    SketchCurve2d batchOTop = SketchCurve2d.BSpline(
        [new(18, 9), new(9, 9), new(0, 9)], [0.0, 1.0, 2.0], [2, 1, 2], 1);
    SketchCurve2d batchOLeft = SketchCurve2d.Segment(new(0, 9), new(0, 0));
    SketchCurveChain2d batchOOuter = SketchCurveChain2d.Create(
        [batchOTop, batchOBottom, batchOLeft, batchORight], requireClosed: true);
    SketchCurveChain2d batchOHole = SketchCurveChain2d.Create(
        [SketchCurve2d.Circle(new(9, 4.5), 1.5)], requireClosed: true);
    SketchProfile2d batchOProfile = SketchProfile2d.Classify([batchOHole, batchOOuter]);
    SketchIntersection batchOCrossing = batchOBottom.Intersect(
        SketchCurve2d.Segment(new(6, -2), new(6, 2))).Single();
    if (batchOBottom.Evaluate(0.5).Point != new SketchPoint2d(9, 0)
        || batchOBottom.Project(new(9, 2)).Single().Distance < 1.999999
        || Math.Abs(batchOCrossing.Point.X - 6) > 1e-8
        || batchOOuter.Inspect().Count != 0)
        throw new InvalidOperationException("The packaged Batch O copied definition/inspection workflow failed.");

    using Shape batchOOffset = batchOOuter.Offset(SketchPlane.XY, 0.5);
    using Shape batchOFeature = batchOProfile.Extrude(SketchPlane.XY, 4);
    if (!batchOOffset.IsValid || !batchOFeature.IsValid
        || batchOFeature.InspectProperties(InspectionPropertyKind.Volume).Mass <= 0)
        throw new InvalidOperationException("The packaged Batch O topology/feature workflow failed.");

    XdePartMetadata batchOMetadata = new(
        "Package Batch O planar feature", new XdeColor(0.2, 0.7, 0.35), ["Sketch", "Feature"]);
    string batchOStep = SketchProfile2d.WriteStep(
        batchOFeature, Path.Combine(exchangeDirectory, "package-batch-o.step"), batchOMetadata);
    string batchOIges = SketchProfile2d.WriteIges(
        batchOFeature, Path.Combine(exchangeDirectory, "package-batch-o.iges"), batchOMetadata);
    using XdeDocument batchOStepDocument = XdeDocument.ReadStep(batchOStep);
    using XdeDocument batchOIgesDocument = XdeDocument.ReadIges(batchOIges);
    if (batchOStepDocument.GetFreeShapes().Length != 1
        || batchOIgesDocument.GetFreeShapes().Length != 1)
        throw new InvalidOperationException("The packaged Batch O STEP/IGES workflow failed.");

    nint batchOWindow = PackageWindowMethods.CreateWindowEx(
        0, "STATIC", "OcctSharp Batch O package sketch", 0x80000000u,
        -32000, -32000, 320, 320, 0, 0, 0, 0);
    if (batchOWindow == 0)
        throw new InvalidOperationException("The Batch O package HWND could not be created.");
    try
    {
        _ = PackageWindowMethods.ShowWindow(batchOWindow, 4);
        _ = PackageWindowMethods.UpdateWindow(batchOWindow);
        using OcctViewer batchOViewer = OcctViewer.Create(batchOWindow);
        using ViewerPresentation batchOPresentation = batchOViewer.Display(batchOFeature);
        batchOViewer.FitAll();
        batchOViewer.MoveTo(160, 160);
        batchOViewer.SelectAt(160, 160, ViewerSelectionMode.Replace);
        string batchOImage = batchOViewer.SaveScreenshot(
            Path.Combine(exchangeDirectory, "package-batch-o.png"), overwrite: true);
        if (!File.Exists(batchOImage) || new FileInfo(batchOImage).Length == 0)
            throw new InvalidOperationException("The packaged Batch O real-HWND workflow failed.");
    }
    finally { _ = PackageWindowMethods.DestroyWindow(batchOWindow); }
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
