using OcctSharp.Interop;
using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchQCompletionTests
{
    [Fact]
    public void InvalidPortableRecordsAndContradictoryControlsAreRejected()
    {
        using Shape box = ShapeFactory.CreateBox(1, 2, 3); using RepairSnapshot source = RepairSnapshot.Create(box);
        RepairPlan plan = new(source, [new("solid", new SolidNormalizationRepair())]);
        string recipe = RepairSerialization.SerializeRecipe(plan);
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeRecipe(recipe.Replace("\"steps\": [", "\"steps\": [null,"), source));
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeAudit("{\"schema\":1}"));
        using RepairPreview preview = ShapeRepair.Preview(source, plan);
        string audit = RepairSerialization.SerializeAudit(preview);
        Assert.Throws<ArgumentException>(() => RepairSerialization.DeserializeAudit(audit.Replace("\"completed\": true", "\"completed\": false")));
        Assert.Throws<ArgumentException>(() => new RepairPlan(source, [new("none", new SameDomainUnificationRepair(false, false))]));
        Assert.Throws<ArgumentException>(() => new RepairPlan(source, [new("angle", new AngularDivisionRepair(Math.Tau + 1))]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepairPlan(source, [new("bad mode", new FaceNormalizationRepair((RepairControl)8))]));
        Assert.Throws<ArgumentException>(() => new RepairPlan(source, [new("dup", new SolidNormalizationRepair()), new("dup", new SolidNormalizationRepair())]));
        using RepairPreview off = ShapeRepair.Preview(source, new(source, [new("disabled", new SmallSolidRepair(100), control: RepairControl.Off)]));
        Assert.True(off.CanAccept, Messages(off)); Assert.Equal(RepairStageState.Skipped, off.Stages[0].State);
        Assert.Equal(6, off.Result!.Metrics.Volume!.Value, 6);
        using Shape accepted = off.Accept();
        Assert.Throws<InvalidOperationException>(() => off.Accept());
    }

    [Fact]
    public void NonmanifoldSewingReportsMultipleEdgesAndContinuitySplitsRealKinks()
    {
        using Shape left = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(0, 2, 0, 2));
        using Shape right = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(2, 4, 0, 2));
        using Shape wallWire = ShapeFactory.CreatePolygonWire([new(2, 0, 0), new(2, 2, 0), new(2, 2, 2), new(2, 0, 2)], true);
        using Shape wall = ShapeFactory.CreatePlanarFace(wallWire);
        using Shape junction = ShapeFactory.CreateCompound([left, right, wall]); using RepairSnapshot source = RepairSnapshot.Create(junction);
        using RepairPreview sewn = ShapeRepair.Preview(source, new(source, [new("nonmanifold", new SewingRepair(true))]));
        Assert.True(sewn.Completed, Messages(sewn));
        Assert.Contains(sewn.Stages[0].Findings, value => value.Kind == RepairFindingKind.SewingMultipleEdge && value.Source.HasValue);
        using Shape kinked = FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.BSpline(3, 2,
            [new(0, 0, 0), new(0, 2, 0), new(1, 0, 0), new(1, 2, 0), new(2, 0, 1), new(2, 2, 1)],
            [0, 1, 2], [2, 1, 2], [0, 1], [2, 2], 1, 1));
        using RepairSnapshot kink = RepairSnapshot.Create(kinked);
        using RepairPreview divided = ShapeRepair.Preview(kink, new(kink,
            [new("split C0 kink", new ContinuityDivisionRepair(ParametricRepairContinuity.C1))],
            budget: new(MaximumRelativeAreaChange: 1e-6)));
        Assert.True(divided.CanAccept, Messages(divided));
        Assert.Equal(2, divided.Result!.Topology.Count(value => value.Kind == ShapeKind.Face));
        Assert.Equal(2 + 2 * Math.Sqrt(2), divided.Result.Metrics.Area!.Value, 6);
    }

    [Fact]
    public unsafe void NativeRepairBuffersHandlesAndRepeatedOwnershipAreChecked()
    {
        Assert.Equal(32, Marshal.SizeOf<RepairInspectionRaw>());
        Assert.Equal(16, Marshal.SizeOf<RepairRelationRaw>());
        Assert.Equal(40, Marshal.SizeOf<RepairBoundaryRaw>());
        using Shape box = ShapeFactory.CreateBox(1, 2, 3);
        var stage = new RepairStageRaw(6, -1, 0, 0, 0, 10000, 1e-7, 1e-3, 0, 0);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RepairTopology(box.Handle, null, -1, out _));
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RepairSubshape(box.Handle, -1, out nint rejected));
        Assert.Equal(0, rejected);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RepairExecute(box.Handle, stage with { Operation = -1 }, null, 0, null, 0, null, 0, out rejected));
        Assert.Equal(0, rejected);
        using (var invalid = new RepairResultHandle(1))
            Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.RepairResultHistory(invalid, null, 0, out _));
        Assert.Equal(NativeStatus.Success, NativeMethods.RepairResultRelease(0));
        for (int iteration = 0; iteration < 48; ++iteration)
        {
            Assert.Equal(NativeStatus.Success, NativeMethods.RepairExecute(box.Handle, stage, null, 0, null, 0, null, 0, out nint pointer));
            using var result = new RepairResultHandle(pointer);
            Assert.Equal(NativeStatus.Success, NativeMethods.RepairResultHistory(result, null, 0, out int count));
            Assert.True(count > 0);
            RepairRelationRaw entry;
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RepairResultHistory(result, &entry, 1, out _));
            Assert.Equal(NativeStatus.Success, NativeMethods.RepairResultShape(result, out nint shape));
            using Shape owner = ShapeFactory.FromNativeHandle(shape, "test repair owner");
            result.Dispose(); result.Dispose();
            Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.RepairResultRelease(pointer));
            Assert.True(owner.IsValid); Assert.Equal(6, owner.FaceCount);
        }
        using RepairSnapshot snapshot = RepairSnapshot.Create(box);
        snapshot.Dispose();
        Assert.Throws<ObjectDisposedException>(() => snapshot.Select(0));
        Assert.Throws<ObjectDisposedException>(() => snapshot.ExtractFreeBoundaries());
        Assert.Throws<ObjectDisposedException>(() => new RepairPlan(snapshot, [new("solid", new SolidNormalizationRepair())]));
    }

    [Fact]
    public void MetadataConflictsForeignAndStaleSessionsNeverPublishPartialChanges()
    {
        using Shape face = PlaneWithHole(); using XdeDocument document = XdeDocument.Create();
        XdeLabel definition;
        using (XdeTransaction command = document.BeginTransaction("source"))
        { definition = document.AddPart(face, new("unchanged", new XdeColor(0.1, 0.3, 0.8))); command.Commit(); }
        using RepairDocumentSession session = new(document, definition);
        RepairSelection hole = session.Source.Topology.Last(value => value.Kind == ShapeKind.Wire).Selection;
        XdeLabel metadata;
        using (XdeTransaction command = document.BeginTransaction("hole metadata"))
        { metadata = session.GetOrCreateSubshapeLabel(hole); metadata.Name = "do not discard"; command.Commit(); }
        using RepairPreview preview = ShapeRepair.Preview(session.Source, new(session.Source, [new("remove hole", new InternalHoleRemovalRepair(5))]));
        Assert.True(preview.CanAccept, Messages(preview));
        RepairMetadataReview review = session.Review(preview);
        Assert.False(review.CanPublish); Assert.Contains(hole.Index, review.ConflictingSourceIndices);
        Assert.Throws<InvalidOperationException>(() => session.Publish(preview));
        Assert.False(preview.IsAccepted); Assert.False(document.HasOpenTransaction);
        Assert.Equal("do not discard", metadata.Name);
        using Shape unchanged = definition.Shape;
        Assert.Equal(96, unchanged.InspectProperties(InspectionPropertyKind.Area).Mass, 6);
        using XdeDocument foreign = XdeDocument.Create();
        Assert.Throws<ArgumentException>(() => new RepairDocumentSession(foreign, definition));
        using RepairSnapshot other = RepairSnapshot.Create(face);
        using RepairPreview wrong = ShapeRepair.Preview(other, new(other, [new("face", new FaceNormalizationRepair())]));
        Assert.Throws<ArgumentException>(() => session.Review(wrong));
        using (XdeTransaction command = document.BeginTransaction("replace current source"))
        {
            using Shape moved = face.Transformed(new ShapeTransform(1, 0, 0, 0, 0, 1, 0));
            document.UpdateDefinitionShape(definition, moved); command.Commit();
        }
        Assert.Throws<InvalidOperationException>(() => session.Review(preview));
    }

    [Fact]
    public void SewingContiguousEdgesAndProtectedUnificationHaveActualTopologyChanges()
    {
        using Shape left = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(0, 2, 0, 2));
        using Shape right = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(2, 4, 0, 2));
        using Shape compound = ShapeFactory.CreateCompound([left, right]); using RepairSnapshot source = RepairSnapshot.Create(compound);
        using RepairPreview sewn = ShapeRepair.Preview(source, new(source, [new("sew", new SewingRepair())]));
        Assert.True(sewn.CanAccept, Messages(sewn));
        RepairFinding common = Assert.Single(sewn.Stages[0].Findings, value => value.Kind == RepairFindingKind.SewingContiguousEdge);
        Assert.NotNull(common.Source);
        using RepairPreview unified = ShapeRepair.Preview(sewn.Result!, new(sewn.Result!, [new("unify", new SameDomainUnificationRepair())]));
        Assert.True(unified.CanAccept, Messages(unified));
        Assert.Single(unified.Result!.Topology, value => value.Kind == ShapeKind.Face);
        Assert.Equal(8, unified.Result.Metrics.Area!.Value, 6);
        using RepairPreview protectedJoin = ShapeRepair.Preview(sewn.Result!, new(sewn.Result!,
            [new("keep shared edge", new SameDomainUnificationRepair())], [common.Source!.Value]));
        Assert.True(protectedJoin.CanAccept, Messages(protectedJoin));
        Assert.Equal(2, protectedJoin.Result!.Topology.Count(value => value.Kind == ShapeKind.Face));
        Assert.Equal(8, protectedJoin.Result.Metrics.Area!.Value, 6);
    }

    [Fact]
    public void ReplacementHistoryAndConflictingSelectionsAreExplicit()
    {
        using Shape left = ShapeFactory.CreateEdge(new(0, 0, 0), new(1, 0, 0));
        using Shape right = ShapeFactory.CreateEdge(new(0, 2, 0), new(3, 2, 0));
        using Shape compound = ShapeFactory.CreateCompound([left, right]); using RepairSnapshot source = RepairSnapshot.Create(compound);
        RepairSelection[] edges = source.Topology.Where(value => value.Kind == ShapeKind.Edge).Select(value => value.Selection).ToArray();
        using RepairPreview replaced = ShapeRepair.Preview(source, new(source,
            [new("replace", new TopologyEditRepair([new(edges[0], edges[1])]))]));
        Assert.True(replaced.CanAccept, Messages(replaced));
        RepairHistoryRelation mapping = Assert.Single(replaced.History, value => value.Source == edges[0]);
        Assert.Equal(RepairRelationKind.Modified, mapping.Kind);
        using Shape changed = replaced.Result!.CopySubshape(mapping.Result!.Value);
        Assert.Equal(3, changed.GetBoundingBox().Maximum.X, 5);
        RepairSelection vertex = source.Topology.First(value => value.Kind == ShapeKind.Vertex).Selection;
        Assert.Throws<ArgumentException>(() => new RepairPlan(source,
            [new("wrong kind", new TopologyEditRepair([new(edges[0], vertex)]))]));
        using RepairPreview nested = ShapeRepair.Preview(source, new(source,
            [new("nested", new LocationNormalizationRepair(), [source.Select(0), edges[0]])]));
        Assert.False(nested.Completed); Assert.Null(nested.Result);
        Assert.Contains("conflict", Messages(nested), StringComparison.OrdinalIgnoreCase);
    }
}
