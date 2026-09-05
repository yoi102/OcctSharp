namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchQCompletionTests
{
    [Fact]
    public void ReversedWireEdgesAndNaturalFaceBoundsHaveDistinctRepairControls()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        using Shape reversedEdge = ReverseFirstChild(wire, "Wi");
        using RepairSnapshot source = RepairSnapshot.Create(reversedEdge);
        Assert.Contains(source.Findings, value => value.Kind == RepairFindingKind.WireOrdering && value.Status == 2);
        using RepairPreview ordered = ShapeRepair.Preview(source, new(source, [new("reorder", new ReorderWireRepair(true))]));
        Assert.True(ordered.CanAccept, Messages(ordered));
        Assert.Equal(4, ordered.Result!.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.InRange(ordered.Result.Metrics.MaximumEndpointGap, 0, 1e-7);
        Assert.Contains(ordered.History, value => value.Kind == RepairRelationKind.Modified);
        using Shape face = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.Bezier(2, 2,
            [new(0, 0, 0), new(0, 2, 0), new(2, 0, 0), new(2, 2, 0)]));
        Shape[] bounds = face.GetSubShapes(ShapeKind.Wire);
        using Shape unbounded = face.RemoveSubshape(bounds[0]);
        foreach (Shape bound in bounds) bound.Dispose();
        using RepairSnapshot faceSource = RepairSnapshot.Create(unbounded);
        Assert.DoesNotContain(faceSource.Topology, value => value.Kind == ShapeKind.Wire);
        using RepairPreview natural = ShapeRepair.Preview(faceSource, new(faceSource,
            [new("natural bounds", new FaceNormalizationRepair(RepairControl.Off, RepairControl.On, RepairControl.Off))]));
        Assert.True(natural.CanAccept, Messages(natural));
        Assert.Single(natural.Result!.Topology, value => value.Kind == ShapeKind.Wire);
        Assert.Equal(4, natural.Result.Metrics.Area!.Value, 6);
        using Shape holed = PlaneWithHole(); using RepairSnapshot holedSource = RepairSnapshot.Create(holed);
        using RepairPreview oriented = ShapeRepair.Preview(holedSource, new(holedSource,
            [new("orientation only", new FaceNormalizationRepair(RepairControl.On, RepairControl.Off))]));
        Assert.True(oriented.CanAccept, Messages(oriented));
        Assert.Equal(2, oriented.Result!.Topology.Count(value => value.Kind == ShapeKind.Wire));
        Assert.Equal(96, oriented.Result.Metrics.Area!.Value, 6);
    }

    private static Shape ReverseFirstChild(Shape source, string kind)
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.BatchQ.orientation." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = ShapeExchange.WriteBrep(source, Path.Combine(directory, "reversed.brep"));
            string contents = File.ReadAllText(path);
            var pattern = new System.Text.RegularExpressions.Regex(@"(?m)(^" + kind + @"\s*[01]+\s*)([+-])");
            Assert.Matches(pattern, contents);
            File.WriteAllText(path, pattern.Replace(contents, match => match.Groups[1].Value + (match.Groups[2].Value == "+" ? "-" : "+"), 1));
            return ShapeExchange.ReadBrep(path);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void IntersectionsOpenChainsAndNonplanarBoundariesRemainExplicit()
    {
        using Shape crossingWire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 2, 0), new(0, 2, 0), new(2, 0, 0)], true);
        using Shape crossingFace = ShapeFactory.CreatePlanarFace(crossingWire);
        using RepairSnapshot crossing = RepairSnapshot.Create(crossingFace);
        Assert.Contains(crossing.Findings, value => value.Kind == RepairFindingKind.WireIntersection
            && value.Status == 1 && value.Source.HasValue && value.Related.HasValue && value.Source != value.Related);
        using Shape open = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0)], false);
        using Shape nonplanar = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(1, 0, 0), new(1, 1, 1), new(0, 1, 0)], true);
        using Shape compound = ShapeFactory.CreateCompound([open, nonplanar]);
        using RepairSnapshot snapshot = RepairSnapshot.Create(compound);
        var boundaries = snapshot.ExtractFreeBoundaries();
        try
        {
            Assert.Equal(2, boundaries.Count);
            Assert.All(boundaries, value => Assert.Null(value.PlanarArea));
            var chain = Assert.Single(boundaries, value => !value.IsClosed);
            Assert.Equal(2, chain.Length, 8); Assert.Equal(Math.Sqrt(2), chain.EndpointGap!.Value, 8);
            Assert.Single(boundaries, value => value.IsClosed);
            using Shape copy = snapshot.CopyShape(); using RepairSnapshot unchanged = RepairSnapshot.Create(copy);
            Assert.Equal(snapshot.Fingerprint, unchanged.Fingerprint);
        }
        finally { foreach (var boundary in boundaries) boundary.Dispose(); }
    }

    [Fact]
    public void OverBudgetGapsAndProtectedSmallEdgesRejectAtomically()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        using Shape moved = edges[0].Transformed(new ShapeTransform(0, 0.1, 0, 0, 0, 1, 0));
        using Shape broken = wire.ReplaceSubshape(edges[0], moved);
        foreach (Shape edge in edges) edge.Dispose();
        using RepairSnapshot source = RepairSnapshot.Create(broken);
        using RepairPreview preview = ShapeRepair.Preview(source, new(source,
            [new("connect", new ConnectWireRepair(true)), new("unreached", new WireframeGapRepair())], tolerance: new(1e-7, 1e-4)));
        Assert.False(preview.CanAccept, Messages(preview));
        Assert.Equal(0.1, source.Metrics.MaximumEndpointGap, 5);
        using Shape small = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0.0001, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        using RepairSnapshot smallSource = RepairSnapshot.Create(small);
        RepairSelection protect = smallSource.Topology.First(value => value.Kind == ShapeKind.Edge).Selection;
        using RepairPreview protectedEdge = ShapeRepair.Preview(smallSource, new(smallSource,
            [new("drop", new SmallEdgeRepair(0.001, true))], [protect]));
        Assert.False(protectedEdge.Completed); Assert.Null(protectedEdge.Result);
        Assert.Contains("protect", Messages(protectedEdge), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BrokenShellAndSolidOrientationsCanBeNormalized()
    {
        using Shape box = ShapeFactory.CreateBox(2, 3, 4); using Shape broken = ReverseOneShellFace(box);
        using RepairSnapshot source = RepairSnapshot.Create(broken);
        Assert.Contains(source.Findings, value => value.Kind == RepairFindingKind.ShellOrientation);
        foreach (RepairStage operation in new RepairStage[] { new ShellNormalizationRepair(), new SolidNormalizationRepair() })
        {
            using RepairPreview repaired = ShapeRepair.Preview(source, new(source, [new("orientation", operation)]));
            Assert.True(repaired.CanAccept, operation.GetType().Name + ": " + Messages(repaired));
            Assert.DoesNotContain(repaired.Result!.Findings, value => value.Kind == RepairFindingKind.ShellOrientation);
            // A shell fixer makes face uses consistent; the solid fixer additionally
            // establishes which side encloses the bounded material.
            double volume = repaired.Result.Metrics.Volume!.Value;
            Assert.Equal(24, operation is SolidNormalizationRepair ? volume : Math.Abs(volume), 6);
        }
    }

    [Fact]
    public void SelectedSmallFaceRepairRemovesOnlyTheEligibleFace()
    {
        using Shape plane = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(0, 2, 0, 2));
        using Shape small = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(4, 4.0001, 0, 0.0001));
        using Shape compound = ShapeFactory.CreateCompound([plane, small]); using RepairSnapshot source = RepairSnapshot.Create(compound);
        RepairSelection selected = source.Topology.Last(value => value.Kind == ShapeKind.Face).Selection;
        using RepairPreview preview = ShapeRepair.Preview(source, new(source, [new("remove spot", new SmallFaceRepair(0.001), [selected])]));
        Assert.True(preview.CanAccept, Messages(preview));
        Assert.Single(preview.Result!.Topology, value => value.Kind == ShapeKind.Face);
        Assert.Equal(4, preview.Result.Metrics.Area!.Value, 7);
        Assert.Contains(preview.History, value => value.Source == selected && value.Kind == RepairRelationKind.Deleted);
        using RepairPreview protectedFace = ShapeRepair.Preview(source, new(source,
            [new("protected spot", new SmallFaceRepair(0.001), [selected])], [selected]));
        Assert.False(protectedFace.Completed); Assert.Null(protectedFace.Result);
    }

    [Fact]
    public void SelectedHoleRemovalAndLocationBakingPreserveTheirScopes()
    {
        using Shape face = PlaneWithHole(); using RepairSnapshot source = RepairSnapshot.Create(face);
        RepairSelection hole = source.Topology.Last(value => value.Kind == ShapeKind.Wire).Selection;
        using RepairPreview repaired = ShapeRepair.Preview(source, new(source,
            [new("selected hole", new InternalHoleRemovalRepair(5), [hole])]));
        Assert.True(repaired.CanAccept, Messages(repaired)); Assert.Equal(100, repaired.Result!.Metrics.Area!.Value, 6);
        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        using GpTrsf transform = GpTrsf.Create(20, 30, 40, 0, 0, 1, 0);
        using TopLocLocation location = TopLocLocation.FromTransform(transform);
        using Shape placed = box.Located(location); using RepairSnapshot positioned = RepairSnapshot.Create(placed);
        using RepairPreview normalized = ShapeRepair.Preview(positioned, new(positioned,
            [new("bake placements", new LocationNormalizationRepair(ShapeKind.Compound))], budget: new(MaximumRelativeVolumeChange: 1e-8)));
        Assert.True(normalized.CanAccept, Messages(normalized));
        using Shape output = normalized.Accept(); BoundingBox3d bounds = output.GetBoundingBox();
        Assert.Equal(20, bounds.Minimum.X, 5); Assert.Equal(22, bounds.Maximum.X, 5);
        Assert.Equal(30, bounds.Minimum.Y, 5); Assert.Equal(44, bounds.Maximum.Z, 5);
        Assert.DoesNotMatch(@"Locations\s+0\b", SerializedTopology(placed));
        Assert.Matches(@"Locations\s+0\b", SerializedTopology(output));
        Assert.Contains(normalized.History, value => value.Kind == RepairRelationKind.Modified);
    }

    private static unsafe string SerializedTopology(Shape shape)
    {
        Assert.Equal(OcctSharp.Interop.NativeStatus.Success,
            OcctSharp.Interop.NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count));
        byte[] bytes = new byte[count];
        fixed (byte* buffer = bytes)
            Assert.Equal(OcctSharp.Interop.NativeStatus.Success,
                OcctSharp.Interop.NativeMethods.RepairSerialized(shape.Handle, buffer, bytes.Length, out _));
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    [Fact]
    public void AllParametricContinuityModesAndClosedEdgesAreChecked()
    {
        using Shape cylinder = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Cylinder,
            SketchPlane.XY, new(0, Math.Tau, 0, 5), 2);
        using RepairSnapshot source = RepairSnapshot.Create(cylinder);
        foreach (ParametricRepairContinuity continuity in Enum.GetValues<ParametricRepairContinuity>())
        {
            using RepairPreview divided = ShapeRepair.Preview(source, new(source,
                [new("continuity", new ContinuityDivisionRepair(continuity))], budget: new(MaximumRelativeAreaChange: 1e-6)));
            Assert.True(divided.CanAccept, continuity + ": " + Messages(divided));
        }
        using RepairPreview closed = ShapeRepair.Preview(source, new(source, [new("closed edges", new ClosedEdgeDivisionRepair(3))]));
        Assert.True(closed.CanAccept, Messages(closed));
        Assert.True(closed.Result!.Topology.Count(value => value.Kind == ShapeKind.Edge) > source.Topology.Count(value => value.Kind == ShapeKind.Edge));
        Assert.Throws<ArgumentException>(() => new RepairPlan(source, [new("G2", new ContinuityDivisionRepair((ParametricRepairContinuity)3))]));
    }
}
