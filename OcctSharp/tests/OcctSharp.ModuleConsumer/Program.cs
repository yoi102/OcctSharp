using OcctSharp;

if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CE_CONSUMER_EVIDENCE") is {} ceEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_CE_EVIDENCE",Path.Combine(ceEvidence,"module-resources"));
OcctSharp.Tests.Shared.BatchCEExchangeWorkflow.Run();
if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CD_CONSUMER_EVIDENCE") is {} cdEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_CD_EVIDENCE_DIR",Path.Combine(cdEvidence,"module-fields"));
OcctSharp.Tests.Shared.BatchCDStepWorkflow.RunNative();

OcctSharp.Tests.Shared.BatchBRCompatibilityWorkflow.Run();
OcctSharp.Tests.Shared.BatchBTCoupledConstraintWorkflow.Run();
OcctSharp.Tests.Shared.BatchBUUnitWorkflow.Run();
OcctSharp.Tests.Shared.BatchBVPeriodicProjectionWorkflow.Run();
OcctSharp.Tests.Shared.BatchBWNonuniformFittingWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BX_CONSUMER_EVIDENCE") is {} bxEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_BX_EVIDENCE", Path.Combine(bxEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchBXTypedTopologyWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BY_CONSUMER_EVIDENCE") is {} byEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_BY_EVIDENCE", Path.Combine(byEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchBYBooleanRepairWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BZ_CONSUMER_EVIDENCE") is {} bzEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_BZ_EVIDENCE", Path.Combine(bzEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchBZLocalFeatureWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CA_CONSUMER_EVIDENCE") is {} caEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_CA_EVIDENCE", Path.Combine(caEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchCAMeshWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CB_CONSUMER_EVIDENCE") is {} cbEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_CB_EVIDENCE", Path.Combine(cbEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchCBDocumentWorkflow.Run();
if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CC_CONSUMER_EVIDENCE") is {} ccEvidence)
    Environment.SetEnvironmentVariable("OCCTSHARP_BATCH_CC_EVIDENCE", Path.Combine(ccEvidence, "module-visible"));
OcctSharp.Tests.Shared.BatchCCLocatedAssemblyWorkflow.Run();
OcctSharp.Tests.Shared.BatchCCLocatedAssemblyWorkflow.RunRejections();

OcctSharp.Tests.Shared.BatchXProjectionWorkflow.Run();
OcctSharp.Tests.Shared.BatchYGeometryWorkflow.Run();
OcctSharp.Tests.Shared.BatchZValueWorkflow.Run();
OcctSharp.Tests.Shared.BatchAAReferenceWorkflow.Run();
OcctSharp.Tests.Shared.BatchABOwnedWorkflow.Run();
OcctSharp.Tests.Shared.BatchACOwnedUtilityWorkflow.Run();
OcctSharp.Tests.Shared.BatchADOwnedDataWorkflow.Run();
OcctSharp.Tests.Shared.BatchAEExchangeScalarWorkflow.Run();
OcctSharp.Tests.Shared.BatchAFCopiedCollectionsWorkflow.Run();
OcctSharp.Tests.Shared.BatchAGSurfaceInterpolationWorkflow.Run();
OcctSharp.Tests.Shared.BatchAHBulkAnalysisWorkflow.Run();
OcctSharp.Tests.Shared.BatchAITopologyExchangeWorkflow.Run();
OcctSharp.Tests.Shared.BatchAJRepairWorkspaceWorkflow.Run();
OcctSharp.Tests.Shared.BatchAKTransferRecordsWorkflow.Run();
OcctSharp.Tests.Shared.BatchALNumericWorkflow.Run();
OcctSharp.Tests.Shared.BatchAMSplineWorkflow.Run();
OcctSharp.Tests.Shared.BatchANWriterWorkflow.Run();
OcctSharp.Tests.Shared.BatchAPModelingWorkflow.Run();
OcctSharp.Tests.Shared.BatchAQHatchingWorkflow.Run();
OcctSharp.Tests.Shared.BatchARMeshWorkflow.Run();
OcctSharp.Tests.Shared.BatchASDocumentWorkflow.Run();
OcctSharp.Tests.Shared.BatchAWGeometryWorkflow.Run();
OcctSharp.Tests.Shared.BatchAYBoundaryWorkflow.Run();
OcctSharp.Tests.Shared.BatchAXMetadataWorkflow.Run();
OcctSharp.Tests.Shared.BatchAVFoundationWorkflow.Run();
OcctSharp.Tests.Shared.BatchBAAssemblyWorkflow.Run();
OcctSharp.Tests.Shared.BatchBBSamplingWorkflow.Run();
OcctSharp.Tests.Shared.BatchBCAssociationWorkflow.Run();
OcctSharp.Tests.Shared.BatchBDExchangeWorkflow.Run();
OcctSharp.Tests.Shared.BatchBEMeshWorkflow.Run();
OcctSharp.Tests.Shared.BatchAZDiagnosticWorkflow.Run();
OcctSharp.Tests.Shared.BatchBFNumericWorkflow.Run();
OcctSharp.Tests.Shared.BatchBGModelingWorkflow.Run();
OcctSharp.Tests.Shared.BatchBHIntersectionWorkflow.Run();
OcctSharp.Tests.Shared.BatchBICollectionWorkflow.Run();
OcctSharp.Tests.Shared.BatchBJLocalStepWorkflow.Run();
OcctSharp.Tests.Shared.BatchBKConfiguredMeshWorkflow.Run();
OcctSharp.Tests.Shared.BatchBLConstructionWorkflow.Run();
OcctSharp.Tests.Shared.BatchBMFiniteExtremaWorkflow.Run();
OcctSharp.Tests.Shared.BatchBNBoundedTextWorkflow.Run();
OcctSharp.Tests.Shared.BatchBOMeshProximityWorkflow.Run();
OcctSharp.Tests.Shared.BatchBPLocalExportWorkflow.Run();
OcctSharp.Tests.Shared.BatchBQWholeRoundWorkflow.Run();
OcctSharp.Tests.Shared.BatchATExchangeWorkflow.Run();
OcctSharp.Tests.Shared.BatchAUIntegratedWorkflow.Run();

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
if (!box.GetSelfInterferenceReport().IsClear ||
    typeof(ShapeSelfInterferenceReport).Assembly.GetName().Name != "OcctSharp.Modeling")
    throw new InvalidOperationException("The direct Modeling self-interference report failed.");
using (var shift = GpTrsf.Create(1, 0, 0))
using (var shifted = box.Transformed(shift))
using (var overlap = ShapeFactory.CreateCompound([box, shifted]))
    if (overlap.GetSelfInterferenceReport().IsClear)
        throw new InvalidOperationException("The direct Modeling consumer missed compound interference.");
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
