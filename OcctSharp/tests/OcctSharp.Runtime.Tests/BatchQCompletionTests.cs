using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchQCompletionTests
{
    [Fact]
    public void SharedDefinitionRepairReopensStepIgesAndSelectsRealViewerDefects() => OcctSharp.Validation.BatchQRepairWorkflow.Run();
    [Fact]
    public void SnapshotOwnsTopologyAndCopiedToleranceProvenance()
    {
        Assert.Equal(24, Marshal.SizeOf<RepairTopologyRaw>());
        Assert.Equal(32, Marshal.SizeOf<RepairFindingRaw>());
        Assert.Equal(48, Marshal.SizeOf<RepairMetricsRaw>());
        Assert.Equal(56, Marshal.SizeOf<RepairStageRaw>());
        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        using RepairSnapshot snapshot = RepairSnapshot.Create(box, "mm", 12);
        box.Dispose();
        Assert.True(snapshot.Metrics.IsValid);
        Assert.Equal(52, snapshot.Metrics.Area!.Value, 6);
        Assert.Equal(24, snapshot.Metrics.Volume!.Value, 6);
        Assert.Equal(6, snapshot.Topology.Count(value => value.Kind == ShapeKind.Face));
        Assert.Equal(12, snapshot.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.Equal(8, snapshot.Topology.Count(value => value.Kind == ShapeKind.Vertex));
        Assert.All(snapshot.Topology, value => Assert.Equal(snapshot.Identity, value.Selection.Source));
        Assert.Equal(3, snapshot.Tolerances.Count);
        Assert.All(snapshot.Tolerances, value => Assert.InRange(value.Minimum, 1e-9, 1e-6));
        using Shape face = snapshot.CopySubshape(snapshot.Topology.First(value => value.Kind == ShapeKind.Face).Selection);
        snapshot.Dispose();
        Assert.True(face.IsValid);
        Assert.Throws<ObjectDisposedException>(() => snapshot.CopyShape());
    }

    [Fact]
    public void AtomicPreviewComposesHistoryAndRejectsForeignAndUnverifiedInputs()
    {
        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        using RepairSnapshot source = RepairSnapshot.Create(box);
        using RepairSnapshot foreign = RepairSnapshot.Create(box);
        RepairStep[] steps = [new("shells", new ShellNormalizationRepair()), new("solids", new SolidNormalizationRepair()),
            new("unify", new SameDomainUnificationRepair())];
        RepairPlan plan = new(source, steps, budget: new(MaximumRelativeAreaChange: 1e-9, MaximumRelativeVolumeChange: 1e-9));
        steps[0] = new("caller changed array", new SmallSolidRepair(1000));
        using RepairPreview preview = ShapeRepair.Preview(source, plan);
        Assert.True(preview.Completed, Messages(preview)); Assert.True(preview.CanAccept, Messages(preview));
        Assert.Equal(3, preview.Stages.Count); Assert.Equal("shells", preview.Stages[0].Name);
        Assert.All(preview.History, value => Assert.Equal(source.Identity, value.Source.Source));
        Assert.Contains(preview.History, value => value.Result.HasValue);
        using Shape accepted = preview.Accept(); preview.Dispose(); source.Dispose();
        Assert.Equal(24, accepted.InspectProperties(InspectionPropertyKind.Volume).Mass, 6);
        Assert.Throws<ObjectDisposedException>(() => preview.Accept());
        Assert.Throws<ArgumentException>(() => ShapeRepair.Preview(foreign, plan));
        Assert.Throws<ArgumentException>(() => new RepairPlan(foreign, [new("stale", new ReorderWireRepair(), [plan.Source == foreign.Identity ? foreign.Select(0) : new(plan.Source, 0)])]));
        Assert.Throws<ArgumentException>(() => new RepairPlan(foreign, [new("G1", new ContinuityDivisionRepair((ParametricRepairContinuity)1))]));
    }

    [Fact]
    public void FreeBoundariesMeasurePlanarLoopsAndSurviveSourceDisposal()
    {
        using Shape face = PlaneWithHole();
        using RepairSnapshot snapshot = RepairSnapshot.Create(face);
        IReadOnlyList<RepairFreeBoundary> boundaries = snapshot.ExtractFreeBoundaries();
        try
        {
            Assert.Equal(2, boundaries.Count);
            Assert.All(boundaries, boundary => { Assert.True(boundary.IsClosed); Assert.NotNull(boundary.PlanarArea); Assert.Equal(4, boundary.SourceEdges.Count); });
            Assert.Equal(48, boundaries.Sum(value => value.Length), 5);
            Assert.Collection(boundaries.Select(value => value.PlanarArea!.Value).Order(), value => Assert.Equal(4, value, 5), value => Assert.Equal(100, value, 5));
            snapshot.Dispose(); face.Dispose();
            Assert.All(boundaries, boundary => Assert.Equal(4, boundary.Wire.CountSubShapes(ShapeKind.Edge)));
        }
        finally { foreach (RepairFreeBoundary boundary in boundaries) boundary.Dispose(); }
    }

    [Fact]
    public void HoleRemovalBudgetsAndProtectedWireAreAtomic()
    {
        using Shape face = PlaneWithHole(); using RepairSnapshot source = RepairSnapshot.Create(face);
        RepairStep[] steps = [new("holes", new InternalHoleRemovalRepair(5))];
        using RepairPreview preview = ShapeRepair.Preview(source, new(source, steps));
        Assert.True(preview.CanAccept, Messages(preview));
        Assert.True(Math.Abs(preview.Result!.Metrics.Area!.Value - 100) < 1e-5, Messages(preview));
        Assert.Equal(96, source.Metrics.Area!.Value, 5);
        using RepairPreview bounded = ShapeRepair.Preview(source, new(source, steps, budget: new(MaximumRelativeAreaChange: 0.001)));
        Assert.True(bounded.Completed); Assert.False(bounded.CanAccept);
        Assert.Contains(bounded.BudgetChecks, check => check.Name == "relative-area-change" && check.State == RepairCheckState.Failed);
        Assert.Throws<InvalidOperationException>(() => bounded.Accept());
        RepairSelection[] wires = source.Topology.Where(value => value.Kind == ShapeKind.Wire).Select(value => value.Selection).ToArray();
        using RepairPreview protectedPreview = ShapeRepair.Preview(source, new(source, steps, wires));
        Assert.False(protectedPreview.Completed); Assert.Null(protectedPreview.Result);
        Assert.Contains("protected", Messages(protectedPreview), StringComparison.OrdinalIgnoreCase);
        using RepairPreview unavailable = ShapeRepair.Preview(source, new(source, [new("face", new FaceNormalizationRepair())],
            budget: new(MaximumRelativeVolumeChange: 0.1)));
        Assert.True(unavailable.Completed, Messages(unavailable)); Assert.False(unavailable.CanAccept);
        Assert.Contains(unavailable.BudgetChecks, check => check.State == RepairCheckState.Unavailable);
    }

    [Fact]
    public void DivisionPreservesAreaAndBoundsGrowth()
    {
        using Shape cylinder = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Cylinder,
            new(new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)), new(0, Math.Tau, 0, 5), 2);
        using RepairSnapshot source = RepairSnapshot.Create(cylinder);
        foreach (RepairStage stage in new RepairStage[] { new AngularDivisionRepair(Math.PI / 2), new AreaDivisionRepair(20),
            new ClosedFaceDivisionRepair(2), new ClosedEdgeDivisionRepair(2), new ContinuityDivisionRepair() })
        {
            using RepairPreview preview = ShapeRepair.Preview(source, new(source, [new("divide", stage)],
                budget: new(MaximumRelativeAreaChange: 1e-6)));
            Assert.True(preview.CanAccept, stage.GetType().Name + ": " + Messages(preview));
            Assert.Equal(20 * Math.PI, preview.Result!.Metrics.Area!.Value, 4);
            if (stage is AngularDivisionRepair or AreaDivisionRepair or ClosedFaceDivisionRepair)
                Assert.True(preview.Result.Topology.Count(value => value.Kind == ShapeKind.Face) >= 2);
        }
        using RepairPreview failed = ShapeRepair.Preview(source, new(source,
            [new("too many", new AngularDivisionRepair(Math.PI / 6)), new("must skip", new FaceNormalizationRepair())], maximumTopology: 5));
        Assert.False(failed.Completed); Assert.Null(failed.Result);
        Assert.Equal(RepairStageState.Failed, failed.Stages[0].State);
        Assert.Equal(RepairStageState.Skipped, failed.Stages[1].State);
    }

    [Fact]
    public void SelectedSolidFilteringDoesNotTouchSiblingAndReportsRemoval()
    {
        using Shape large = ShapeFactory.CreateBox(5, 5, 5); using Shape little = ShapeFactory.CreateBox(0.1, 0.1, 0.1);
        using Shape translated = little.Transformed(new ShapeTransform(10, 0, 0, 0, 0, 1, 0));
        using Shape compound = ShapeFactory.CreateCompound([large, translated]); using RepairSnapshot source = RepairSnapshot.Create(compound);
        RepairSelection small = source.Topology.Last(value => value.Kind == ShapeKind.Solid).Selection;
        using RepairPreview preview = ShapeRepair.Preview(source, new(source, [new("small", new SmallSolidRepair(0.01), [small])]));
        Assert.True(preview.CanAccept, Messages(preview)); Assert.Equal(125, preview.Result!.Metrics.Volume!.Value, 6);
        Assert.Contains(preview.History, value => value.Source == small && value.Kind == RepairRelationKind.Deleted);
        Assert.Equal(125.001, source.Metrics.Volume!.Value, 6);
    }

    private static string Messages(RepairPreview preview) => string.Join("; ", preview.Stages.Select(value => value.Name + ": " + value.Message)
        .Concat(preview.BudgetChecks.Select(value => value.Name + ": " + value.State))
        .Concat(preview.Stages.SelectMany(value => value.Findings).Concat(preview.Result?.Findings ?? []).Select(value => value.ToString())));

    [Fact]
    public void BrokenWireShellDegeneracyAndThinFacesHaveScopedFindings()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        using Shape displaced = edges[0].Transformed(new ShapeTransform(0, 0.0001, 0, 0, 0, 1, 0));
        using Shape broken = wire.ReplaceSubshape(edges[0], displaced);
        foreach (Shape edge in edges) edge.Dispose();
        using Shape box = ShapeFactory.CreateBox(2, 3, 4); Shape[] faces = box.GetFaces();
        using Shape farFace = faces[0].Transformed(new ShapeTransform(30, 0, 0, 0, 0, 1, 0));
        using Shape disconnected = box.ReplaceSubshape(faces[0], farFace);
        using Shape badOrientation = ReverseOneShellFace(box);
        foreach (Shape face in faces) face.Dispose();
        using Shape sphere = ShapeFactory.CreateSphere(1);
        using Shape thinWire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(5, 0, 0), new(5, 0.0001, 0), new(0, 0.0001, 0)], true);
        using Shape thinFace = ShapeFactory.CreatePlanarFace(thinWire);
        using Shape compound = ShapeFactory.CreateCompound([broken, disconnected, badOrientation, sphere, thinFace]);
        using RepairSnapshot snapshot = RepairSnapshot.Create(compound, options: new(SmallLength: 0.001, SmallArea: 0.01, ToleranceOutlier: 1e-9));
        Assert.False(snapshot.Metrics.IsValid);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.EndpointGap && value.Value >= 0.00009);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.DisconnectedShell);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.ShellOrientation);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.DegenerateEdge);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.SmallAreaFace);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.StripFace);
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.ToleranceOutlier);
        Assert.All(snapshot.Findings.Where(value => value.Source.HasValue), value => Assert.Equal(snapshot.Identity, value.Source!.Value.Source));
        Assert.Contains(snapshot.Findings, value => value.Kind == RepairFindingKind.WireIntersection && value.Status == 3);
    }

    [Fact]
    public void WireRepairsConnectWithinBudgetWithoutMutatingTheDamagedSource()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        using Shape moved = edges[0].Transformed(new ShapeTransform(0, 0.0001, 0, 0, 0, 1, 0));
        using Shape broken = wire.ReplaceSubshape(edges[0], moved);
        foreach (Shape edge in edges) edge.Dispose();
        using RepairSnapshot source = RepairSnapshot.Create(broken);
        string initial = source.Fingerprint;
        using RepairPreview preview = ShapeRepair.Preview(source, new(source,
            [new("order", new ReorderWireRepair(true)), new("connect", new ConnectWireRepair(true)), new("gaps", new WireframeGapRepair())],
            tolerance: new(1e-7, 0.001)));
        Assert.True(preview.CanAccept, Messages(preview));
        Assert.Equal(4, preview.Result!.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.InRange(preview.Result.Metrics.MaximumEndpointGap, 0, 0.001);
        Assert.Equal(4, preview.Result.Topology.Count(value => value.Kind == ShapeKind.Vertex));
        var vertexMappings = preview.History.Where(value => value.Result.HasValue
            && source.Topology[value.Source.Index].Kind == ShapeKind.Vertex).ToArray();
        Assert.Contains(vertexMappings.GroupBy(value => value.Result!.Value), group => group.Count() > 1);
        Assert.Contains(preview.Stages[2].Findings, value => value.Kind == RepairFindingKind.WireGap3d);
        using Shape sourceCopy = source.CopyShape();
        using RepairSnapshot unchanged = RepairSnapshot.Create(sourceCopy);
        Assert.Equal(initial, unchanged.Fingerprint);
        Assert.True(source.Metrics.MaximumEndpointGap >= 0.00009);
        Assert.Contains(preview.Stages[2].Findings, value => value.Kind == RepairFindingKind.WireGap2d && value.Status == 3);
        using Shape damagedFace = ShapeFactory.CreatePlanarFace(broken);
        using RepairSnapshot supported = RepairSnapshot.Create(damagedFace);
        Assert.Contains(supported.Findings, value => value.Kind == RepairFindingKind.WireGap2d
            && value.Status == 1 && value.Value >= 0.00009);
        using RepairPreview facePreview = ShapeRepair.Preview(supported, new(supported,
            [new("order", new ReorderWireRepair(true)), new("connect", new ConnectWireRepair(true)), new("gaps", new WireframeGapRepair())],
            tolerance: new(1e-7, 0.001)));
        Assert.True(facePreview.CanAccept, Messages(facePreview));
        var residuals = facePreview.Stages[2].Findings.Where(value => value.Kind == RepairFindingKind.WireGap2d).ToArray();
        Assert.NotEmpty(residuals);
        Assert.All(residuals, value => {
            Assert.Equal(0, value.Status); Assert.InRange(value.Value, 0, 1e-7);
            Assert.Equal(facePreview.Result!.Identity, value.Source!.Value.Source);
            Assert.Equal(facePreview.Result.Identity, value.Related!.Value.Source);
        });
    }

    [Fact]
    public void SmallEdgesAndScopedTolerancesHaveVerifiedResults()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0.0001, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        using RepairSnapshot source = RepairSnapshot.Create(wire);
        RepairSelection corner = source.Topology.Last(value => value.Kind == ShapeKind.Vertex).Selection;
        using RepairPreview small = ShapeRepair.Preview(source, new(source,
            [new("small edges", new SmallEdgeRepair(0.001, true))], [corner]));
        Assert.True(small.CanAccept, Messages(small));
        Assert.True(small.Result!.Topology.Count(value => value.Kind == ShapeKind.Edge) < source.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.Contains(small.History, value => value.Source == corner && value.Kind == RepairRelationKind.Unchanged && value.Result.HasValue);
        Assert.Contains(small.History, value => source.Topology[value.Source.Index].Kind == ShapeKind.Edge
            && value.Kind is RepairRelationKind.Deleted or RepairRelationKind.Modified);
        using Shape box = ShapeFactory.CreateBox(1, 2, 3); using RepairSnapshot solid = RepairSnapshot.Create(box);
        RepairSelection vertex = solid.Topology.First(value => value.Kind == ShapeKind.Vertex).Selection;
        using RepairPreview tolerance = ShapeRepair.Preview(solid, new(solid,
            [new("vertex tolerance", new ToleranceNormalizationRepair(ShapeKind.Vertex), [vertex])], tolerance: new(1e-6, 1e-5)));
        Assert.True(tolerance.CanAccept, Messages(tolerance));
        int changedIndex = tolerance.History.Single(value => value.Source == vertex).Result!.Value.Index;
        Assert.Equal(1e-6, tolerance.Result!.Topology[changedIndex].Tolerance!.Value, 10);
        Assert.Equal(1, tolerance.Result.Topology.Count(value => value.Kind == ShapeKind.Vertex && value.Tolerance >= 1e-6));
        using RepairPreview whole = ShapeRepair.Preview(solid, new(solid,
            [new("vertex tolerances", new ToleranceNormalizationRepair(ShapeKind.Vertex))], tolerance: new(1e-6, 1e-5)));
        Assert.True(whole.CanAccept, Messages(whole));
        Assert.All(whole.Result!.Topology.Where(value => value.Kind == ShapeKind.Vertex), value => Assert.InRange(value.Tolerance!.Value, 1e-6, 1e-5));
        Assert.All(solid.Topology.Where(value => value.Kind == ShapeKind.Vertex), value => Assert.True(value.Tolerance < 1e-6));
    }

    [Fact]
    public void SewingReturnsBoundaryReviewAndExplicitEditsComposeDeletions()
    {
        using Shape box = ShapeFactory.CreateBox(2, 3, 4); Shape[] faces = box.GetFaces();
        try
        {
            using Shape open = ShapeFactory.CreateCompound(faces.Take(5).ToArray()); using RepairSnapshot source = RepairSnapshot.Create(open);
            using RepairPreview preview = ShapeRepair.Preview(source, new(source, [new("sew", new SewingRepair())]));
            Assert.True(preview.CanAccept, Messages(preview));
            Assert.Contains(preview.Stages[0].Findings, value => value.Kind == RepairFindingKind.SewingFreeEdge && value.Source.HasValue);
            Assert.Equal(5, preview.Result!.Topology.Count(value => value.Kind == ShapeKind.Face));
            foreach (RepairFinding finding in preview.Stages[0].Findings.Where(value => value.Source.HasValue))
            {
                using Shape affected = preview.Result.CopySubshape(finding.Source!.Value); Assert.Equal(0, affected.FaceCount);
            }
        }
        finally { foreach (Shape face in faces) face.Dispose(); }
        using Shape a = ShapeFactory.CreateEdge(new(0, 0, 0), new(1, 0, 0));
        using Shape b = ShapeFactory.CreateEdge(new(0, 1, 0), new(1, 1, 0));
        using Shape compound = ShapeFactory.CreateCompound([a, b]); using RepairSnapshot edits = RepairSnapshot.Create(compound);
        RepairSelection[] items = edits.Topology.Where(value => value.Kind == ShapeKind.Edge).Select(value => value.Selection).ToArray();
        using RepairPreview removed = ShapeRepair.Preview(edits, new(edits,
            [new("remove", new TopologyEditRepair([new(items[0], null)])), new("normalize", new LocationNormalizationRepair())]));
        Assert.True(removed.CanAccept, Messages(removed));
        Assert.Contains(removed.History, value => value.Source == items[0] && value.Kind == RepairRelationKind.Deleted);
        Assert.Equal(1, removed.Result!.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.Throws<ArgumentException>(() => new RepairPlan(edits,
            [new("cycle", new TopologyEditRepair([new(items[0], items[1]), new(items[1], items[0])]))]));
    }

    [Fact]
    public void PortableRecipesRebindOnlyMatchingGeometryUnitsAndRevision()
    {
        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        using RepairSnapshot source = RepairSnapshot.Create(box, revision: 7);
        RepairPlan plan = new(source, [new("faces", new FaceNormalizationRepair()), new("shells", new ShellNormalizationRepair())]);
        string json = RepairSerialization.SerializeRecipe(plan);
        Assert.DoesNotContain("handle", json, StringComparison.OrdinalIgnoreCase);
        using RepairSnapshot same = RepairSnapshot.Create(box, revision: 7);
        RepairPlan reloaded = RepairSerialization.DeserializeRecipe(json, same);
        Assert.Equal(same.Identity, reloaded.Source);
        using RepairPreview preview = ShapeRepair.Preview(same, reloaded);
        Assert.True(preview.CanAccept, Messages(preview));
        using Shape accepted = preview.Accept();
        RepairAuditRecord audit = RepairSerialization.DeserializeAudit(RepairSerialization.SerializeAudit(preview));
        Assert.True(audit.Accepted); Assert.Equal(2, audit.Stages.Count); Assert.Equal(24, audit.After!.Volume!.Value, 6);
        using RepairSnapshot wrongRevision = RepairSnapshot.Create(box, revision: 8);
        using RepairSnapshot wrongUnit = RepairSnapshot.Create(box, "m", 7);
        using Shape different = ShapeFactory.CreateBox(3, 3, 4); using RepairSnapshot wrongShape = RepairSnapshot.Create(different, revision: 7);
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeRecipe(json, wrongRevision));
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeRecipe(json, wrongUnit));
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeRecipe(json, wrongShape));
    }
    private static Shape PlaneWithHole()
    {
        using Shape plane = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane,
            new(new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)), new(0, 10, 0, 10));
        SketchCurveChain2d outer = SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(0, 0), new(10, 0)), SketchCurve2d.Segment(new(10, 0), new(10, 10)),
            SketchCurve2d.Segment(new(10, 10), new(0, 10)), SketchCurve2d.Segment(new(0, 10), new(0, 0))], true);
        SketchCurveChain2d hole = SketchCurveChain2d.Create([
            SketchCurve2d.Segment(new(4, 4), new(6, 4)), SketchCurve2d.Segment(new(6, 4), new(6, 6)),
            SketchCurve2d.Segment(new(6, 6), new(4, 6)), SketchCurve2d.Segment(new(4, 6), new(4, 4))], true);
        return SurfaceModeling.CreateTrimmedFace(plane, SketchProfile2d.Create(outer, [hole]));
    }

    private static Shape ReverseOneShellFace(Shape source)
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.BatchQ.fixture." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = ShapeExchange.WriteBrep(source, Path.Combine(directory, "reversed-face.brep"));
            string text = File.ReadAllText(path);
            var shell = new System.Text.RegularExpressions.Regex(@"(?m)(^Sh\s*[01]+\s*)([+-])");
            Assert.True(shell.IsMatch(text), text[Math.Max(0, text.IndexOf("\nSh", StringComparison.Ordinal))..]);
            File.WriteAllText(path, shell.Replace(text, match => match.Groups[1].Value + (match.Groups[2].Value == "+" ? "-" : "+"), 1));
            return ShapeExchange.ReadBrep(path);
        }
        finally { Directory.Delete(directory, true); }
    }
}
