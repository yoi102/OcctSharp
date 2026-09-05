using OcctSharp;

byte[] frameInput = [10,20,30,255];
var frame = ViewerColorFrame.FromRgba(1,1,frameInput);
frameInput[0] = 99;
if (frame.CopyOpaqueBgra()[2] != 10 || typeof(ViewerColorFrame).Assembly.GetName().Name != "OcctSharp.Visualization" ||
    typeof(ViewerColorFrame).Assembly.GetReferencedAssemblies().Any(x => x.Name is "PresentationCore" or "PresentationFramework" or "OcctSharp"))
    throw new InvalidOperationException("Copied Visualization frames must remain independent of the facade and WPF.");

OcctRuntimeInfo runtime = OcctRuntime.Info;
if (runtime.OcctVersion != "8.0.1")
{
    throw new InvalidOperationException($"Expected OCCT 8.0.1, got '{runtime.OcctVersion}'.");
}

using Shape box = ShapeFactory.CreateBox(10, 20, 30);
if (box.IsNull || box.Kind != ShapeKind.Solid || box.FaceCount != 6)
{
    throw new InvalidOperationException("The direct Modeling package did not create a valid six-face box.");
}

if (File.Exists(Path.Combine(AppContext.BaseDirectory, "OcctSharp.dll")))
{
    throw new InvalidOperationException("The direct module consumer unexpectedly received the OcctSharp facade assembly.");
}

using RepairSnapshot repairSource = RepairSnapshot.Create(box);
using RepairPreview repair = ShapeRepair.Preview(repairSource,
    new RepairPlan(repairSource, [new("normalize solid", new SolidNormalizationRepair())],
        budget: new(MaximumRelativeVolumeChange: 1e-8)));
if (!repair.CanAccept || repair.Result?.Metrics.Volume is not double volume || Math.Abs(volume - 6000) > 1e-5)
{
    throw new InvalidOperationException("The direct Modeling repair preview failed its geometry budget.");
}
using Shape acceptedRepair = repair.Accept();
repair.Dispose(); repairSource.Dispose();
if (!acceptedRepair.IsValid || acceptedRepair.FaceCount != 6)
{
    throw new InvalidOperationException("The direct Modeling repair result did not survive preview disposal.");
}

AuthoredMesh authored = new([new(0, 0, 0), new(2, 0, 0), new(0, 2, 0)], [new(0, 1, 2)]);
using DiscreteMeshModel discrete = MeshTopology.Create(authored);
using Shape discreteCopy = discrete.CopyShape();
discrete.Dispose();
AuthoredMesh roundtripMesh = MeshTopology.SnapshotExisting(discreteCopy).Mesh;
if (MeshTopology.IsSurfaceBacked(discreteCopy) || roundtripMesh.Triangles.Count != 1 || roundtripMesh.Positions[1].X != 2)
{
    throw new InvalidOperationException("The facade-free Modeling consumer failed the authored mesh lifetime roundtrip.");
}

using Shape guidedSpine = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 10)]);
using Shape guidedProfile = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
using GuidedSweepPlan guidedPlan = GuidedSweepPlan.Create(guidedSpine, [new(guidedProfile)],
    new() { SolidPolicy = SweepSolidPolicy.RequireSolid }, scaleLaw: ScalarLawDefinition.Constant(new(0, 1), 1));
using AuthoringResult guidedResult = guidedPlan.Build();
guidedPlan.Dispose(); guidedSpine.Dispose(); guidedProfile.Dispose();
if (!guidedResult.Diagnostics.IsSolid || !guidedResult.RequireShape().IsValid || guidedResult.RequireShape().FaceCount != 6)
    throw new InvalidOperationException("The facade-free Modeling consumer failed the owning guided sweep.");

using RepairSnapshot contourSource = RepairSnapshot.Create(box);
var seed = contourSource.Topology.First(t => t.Kind == ShapeKind.Edge).Selection;
var contourRecipe = ContourFilletRecipe.Create(contourSource, [FilletContourProgram.FromLaw(seed, ScalarLawDefinition.Linear(new(0, 1), .5, 1))]);
using LocalFeatureResult contourResult = contourRecipe.Build(contourSource);
using LocalFeatureResult contourSections = contourRecipe.Simulate(contourSource);
contourSource.Dispose();
if (!contourResult.RequireShape().IsValid || contourSections.SimulatedSections.Count == 0)
    throw new InvalidOperationException("The facade-free Modeling consumer failed the owning law-driven fillet.");

Console.WriteLine(
    "Checking direct Modeling partition and volume workflows.");
using Shape regionOther = box.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
using PartitionPlan regionPlan = PartitionPlan.Create([box, regionOther]);
using PartitionResult regionResult = regionPlan.Build([new("all", [new(RegionExpression.All, 1)])]);
using Shape regionCopy = regionResult.CopyOutput("all");
regionPlan.Dispose(); regionResult.Dispose();
if (!regionCopy.IsValid) throw new InvalidOperationException("Direct Modeling partition copy lost ownership.");
using VolumeConstructionPlan volumePlan = VolumeConstructionPlan.Create([box]);
using VolumeConstructionResult volumeResult = volumePlan.Build();
if (volumeResult.Volumes.Count != 1 || !volumeResult.HelperBoxExcluded)
    throw new InvalidOperationException("Direct Modeling bounded-volume construction failed.");

Console.WriteLine(
    $"Direct Modeling consumer passed: {box.Kind}, {box.FaceCount} faces, owning guided sweep, OCCT {runtime.OcctVersion}.");
