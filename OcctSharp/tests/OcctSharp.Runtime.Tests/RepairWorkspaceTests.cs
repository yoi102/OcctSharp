using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.RepairWorkspaces;
using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;

public sealed class RepairWorkspaceTests
{
    static Shape Face()
    {
        using var wire=ShapeFactory.CreatePolygonWire([new(0,0,0),new(4,0,0),new(4,3,0),new(0,3,0)],true);
        return ShapeFactory.CreatePlanarFace(wire);
    }
    static void DisposeAll(IEnumerable<Shape> shapes){foreach(var s in shapes)s.Dispose();}
    static Shape Vertex(GpPoint point)
    {
        using var edge=ShapeFactory.CreateEdge(point,new(point.X+1,point.Y+1,point.Z+1));
        var vertices=edge.GetSubShapes(ShapeKind.Vertex);try{return RepairDiagnostics.WithOrientation(vertices[0],0);}finally{DisposeAll(vertices);}
    }
    [Fact] public void SharedPublicRepairWorkflowChangesTopologyPreservesSourceAndRoundtripsBothFormats()=>BatchAJRepairWorkspaceWorkflow.Run();

    [Fact] public void CoupledFaceMapsActuallySplitDisconnectedOuterBounds()=>BatchAJRepairWorkspaceWorkflow.SplitDisconnectedFace();

    [Fact] public void LoopRepairReturnsEveryWireAtAnExactSharedBranchVertex()
    {
        using var polygon=ShapeFactory.CreatePolygonWire([new(0,0,0),new(2,0,0),new(2,2,0),new(0,0,0),new(-2,0,0),new(-2,-2,0)],true);
        var vertices=polygon.GetSubShapes(ShapeKind.Vertex);
        try
        {
            List<Shape> repeated=[];foreach(var v in vertices){var b=v.GetBoundingBox();if(Math.Abs(b.Minimum.X+b.Maximum.X)<1e-6 && Math.Abs(b.Minimum.Y+b.Maximum.Y)<1e-6 && !repeated.Any(s=>s.IsSame(v)))repeated.Add(v);}
            Assert.Single(repeated);using var face=ShapeFactory.CreatePlanarFace(polygon);using var session=TopologyRepairSession.Create(face,RepairSessionKind.Face);
            using var loops=session.FixFaceLoops();using var outputs=loops.Shapes;Assert.True(loops.Changed);Assert.Equal(2,outputs.Count);
            double length=0;for(int i=0;i<outputs.Count;++i){using var loop=outputs.GetShape(i);Assert.True(loop.IsValid);length+=loop.InspectProperties(InspectionPropertyKind.Length).Mass;}
            Assert.Equal(polygon.InspectProperties(InspectionPropertyKind.Length).Mass,length,7);session.Dispose();Assert.Equal(2,outputs.Count);
        }
        finally{DisposeAll(vertices);}
    }

    [Fact] public void OrderedWireStoragePreservesDuplicatesOrientationAndFunctionalEdits()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var other=ShapeFactory.CreateEdge(new(2,0,0),new(3,0,0));
        using var set=OwnedShapeSet.Create(edge,edge);using var data=WireDataSnapshot.Create(set);using var reversed=data.AddOrientedEdge(other,1);
        using var a=data.GetEdge(1);using var b=data.GetEdge(2);using var r=reversed.GetEdge(3);Assert.True(a.IsEqual(b));Assert.True(r.IsSame(other));Assert.False(r.IsEqual(other));
        using var prepended=data.AddOrientedShape(other,2);using var first=prepended.GetEdge(1);Assert.True(first.IsEqual(other));
        using var changed=data.SetEdge(1,other);using var old=data.GetEdge(1);using var updated=changed.GetEdge(1);Assert.True(old.IsEqual(edge));Assert.True(updated.IsEqual(other));Assert.Equal(2,data.Count);Assert.Equal(3,reversed.Count);
        Assert.Equal(1,data.IndexOf(edge));Assert.Equal(0,data.IndexOf(other));data.Dispose();set.Dispose();edge.Dispose();Assert.Equal(2,changed.Count);Assert.True(a.IsValid);
    }
    [Fact] public void WireInsertOverloadsAndNativeBuildersKeepDetachedSequenceStorage()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var set=OwnedShapeSet.Create(edge);using var data=WireDataSnapshot.Create(set);using var wire=data.ToWire();
        using var viaEdge=data.InsertEdge(edge);using var viaShape=data.InsertShape(wire);using var viaWire=data.InsertWire(wire);
        using var reversedWire=data.AddOrientedWire(wire,1);using var reversedShape=data.AddOrientedShape(wire,1);
        Assert.Equal(1,data.Count);Assert.Equal(2,viaEdge.Count);Assert.Equal(2,viaShape.Count);Assert.Equal(2,viaWire.Count);Assert.Equal(2,reversedWire.Count);Assert.Equal(2,reversedShape.Count);
        using var loaded=WireDataSnapshot.FromWire(wire);using var built=loaded.ToWire(true);Assert.True(loaded.WasChained);Assert.True(built.IsValid);
    }
    [Fact] public void NonmanifoldPolicyRetainsSeparateEdgesAndSegmentRejectsSilentLoss()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var internalEdge=RepairDiagnostics.WithOrientation(edge,2);using var set=OwnedShapeSet.Create(internalEdge);
        using var manifold=WireDataSnapshot.Create(set,true);using var all=WireDataSnapshot.Create(set,false);
        Assert.Equal(0,manifold.Count);Assert.Equal(1,manifold.NonManifoldCount);using var retained=manifold.NonManifoldEdges;using var value=manifold.GetNonManifoldEdge(1);Assert.Equal(1,retained.Count);Assert.True(value.IsEqual(internalEdge));
        Assert.Equal(1,all.Count);Assert.Equal(0,all.NonManifoldCount);Assert.Throws<ArgumentException>(()=>RepairWireSegment.Create(manifold));using var segment=RepairWireSegment.Create(all);Assert.Equal(1,segment.Count);
    }
    [Fact] public void EmptyNullWrongTypesIndicesAndOrientedModesAreRejectedBeforeNativeMutation()
    {
        using var emptySet=OwnedShapeSet.Create();using var empty=WireDataSnapshot.Create(emptySet);Assert.Equal(0,empty.Count);Assert.Throws<ArgumentException>(()=>empty.GetEdge(1));Assert.Throws<ArgumentException>(()=>empty.ToWire());
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var box=ShapeFactory.CreateBox(1,1,1);using var wrong=OwnedShapeSet.Create(box);
        Assert.Throws<ArgumentException>(()=>WireDataSnapshot.Create(wrong));Assert.Throws<ArgumentException>(()=>empty.InsertEdge(edge,-1));Assert.Throws<ArgumentException>(()=>empty.AddOrientedEdge(edge,4));Assert.Throws<ArgumentException>(()=>empty.SetEdge(int.MaxValue,edge));
        Assert.Throws<ArgumentNullException>(()=>WireDataSnapshot.Create(null!));Assert.Throws<ArgumentException>(()=>RepairDiagnostics.WithOrientation(edge,4));using var nullShape=ShapeFactory.CreateNull();Assert.Throws<ArgumentException>(()=>TopologyRepairSession.Create(nullShape,RepairSessionKind.Face));
        edge.Dispose();Assert.Throws<ObjectDisposedException>(()=>empty.InsertEdge(edge));
    }
    [Fact] public void WireOrderDiagnosticsKeepSignedIndicesAndSurviveInputDisposal()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{using var set=OwnedShapeSet.Create(edges[0],edges[2],edges[1],edges[3]);using var data=WireDataSnapshot.Create(set);using var report=RepairDiagnostics.AnalyzeWire(data,face);
            Assert.True(report.IsReady);Assert.True(report.NeedsReorder);Assert.Equal(4,report.Count);Assert.Equal(Enumerable.Range(1,4),Enumerable.Range(1,4).Select(i=>Math.Abs(report.Ordered(i))).Order());
            Assert.Throws<ArgumentException>(()=>report.Ordered(0));Assert.Throws<ArgumentOutOfRangeException>(()=>report.Status((WireDiagnosticCheck)8,ShapeExtendStatus.ShapeExtend_OK));
            using var loops=RepairDiagnostics.WireLoops(data,face);using var vertices=loops.Shapes;using var keys=loops.FirstMapKeys;_ = report.Status(WireDiagnosticCheck.Order,ShapeExtendStatus.ShapeExtend_DONE);
            data.Dispose();set.Dispose();face.Dispose();using var retained=report.Face;Assert.True(retained.IsValid);
        }finally{DisposeAll(edges);}
    }
    [Fact] public void PrivateWireReorderActuallyChangesOrderingWithoutChangingOriginalEdges()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{using var set=OwnedShapeSet.Create(edges[0],edges[2],edges[1],edges[3]);using var data=WireDataSnapshot.Create(set);using var wire=data.ToWire();
            using var session=TopologyRepairSession.Create(wire,RepairSessionKind.Wire,supportFace:face);session.SetWireMode(WireRepairMode.Reorder,1);Assert.True(session.ReorderWire());using var fixedWire=session.Shape;
            using var privateFace=session.WireSupportFace;using var ordered=WireDataSnapshot.FromWire(fixedWire);using var report=RepairDiagnostics.AnalyzeWire(ordered,privateFace);Assert.False(report.NeedsReorder);
            foreach(var mode in Enum.GetValues<WireRepairMode>())session.SetWireMode(mode,0);Assert.Throws<ArgumentException>(()=>session.SetWireMode(WireRepairMode.PreferencePCurve,-1));
            Assert.Throws<ArgumentException>(()=>session.SetWireMode((WireRepairMode)int.MaxValue,0));_ = session.Perform();using var built=session.WireAPIMake;Assert.True(built.IsValid);Assert.Equal(4,data.Count);
        }finally{DisposeAll(edges);}
    }
    [Fact] public void ConnectionDiagnosticsAreCopiedAndEmptyWireIsRejected()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{using var set=OwnedShapeSet.Create(edges[0]);using var data=WireDataSnapshot.Create(set);var distances=RepairDiagnostics.CheckConnection(data,face,edges[1]);Assert.True(double.IsFinite(distances.TailHead));
            Assert.Throws<ArgumentException>(()=>RepairDiagnostics.CheckConnection(data,face,edges[1],double.NaN));using var empty=OwnedShapeSet.Create();using var noWire=WireDataSnapshot.Create(empty);Assert.Throws<ArgumentException>(()=>RepairDiagnostics.AnalyzeWire(noWire,face));
        }finally{DisposeAll(edges);}
    }
    [Fact] public void SegmentPatchIndicesAndEditsStayCoupledAndDetached()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var other=ShapeFactory.CreateEdge(new(2,0,0),new(3,0,0));using var set=OwnedShapeSet.Create(edge);using var data=WireDataSnapshot.Create(set);using var segment=RepairWireSegment.Create(data);
        segment.SetPatch(1,new(0,1,2,3));Assert.True(segment.GetPatch(1).IsComplete);segment.TightenPatch(1,0,1);segment.TightenPatch(1,1,1);segment.TightenPatch(1,2,3);segment.TightenPatch(1,3,3);Assert.Equal(new RepairPatchBounds(1,1,3,3,true),segment.GetPatch(1));
        segment.AddEdge(other);segment.AddEdge(edge,1,new(1,2,3,4));Assert.Equal(3,segment.Count);segment.SetEdge(1,other);using var changed=segment.GetEdge(1);Assert.True(changed.IsSame(other));segment.Orientation=1;Assert.Equal(1,segment.Orientation);
        using var first=segment.FirstVertex;using var last=segment.LastVertex;segment.SetVertex(first);Assert.True(segment.IsVertex);using var stored=segment.Vertex;Assert.True(stored.IsSame(first));using var snapshot=segment.WireData;
        segment.Clear();Assert.Equal(0,segment.Count);Assert.False(segment.IsClosed);Assert.False(segment.IsVertex);Assert.Throws<ArgumentException>(()=>segment.FirstVertex);Assert.Equal(3,snapshot.Count);segment.Load(data);Assert.Equal(1,segment.Count);Assert.Equal(1,data.Count);
        data.Dispose();segment.Dispose();Assert.True(last.IsValid);Assert.Equal(3,snapshot.Count);
    }
    [Fact] public void InvalidSegmentRangesDoNotCorruptPreviousPatchOrEpoch()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var set=OwnedShapeSet.Create(edge);using var data=WireDataSnapshot.Create(set);using var segment=RepairWireSegment.Create(data);segment.SetPatch(1,new(0,1,0,1));
        Assert.Throws<ArgumentException>(()=>segment.SetPatch(0,new(0,1,0,1)));Assert.Throws<ArgumentException>(()=>segment.SetPatch(1,new(int.MinValue,int.MaxValue,0,1)));Assert.Throws<ArgumentException>(()=>segment.TightenPatch(1,0,2));
        Assert.Throws<ArgumentException>(()=>segment.AddEdge(edge,int.MaxValue));Assert.Throws<ArgumentException>(()=>segment.Orientation=4);Assert.Equal(new RepairPatchBounds(0,1,0,1,true),segment.GetPatch(1));Assert.Equal(1,segment.Count);
    }
    [Fact] public void CoupledSmallEdgeMapsPublishEveryCollectionAndRejectForeignAndStaleWorkspaces()
    {
        using var source=BatchAJRepairWorkspaceWorkflow.TinyEdgeFace();using var session=TopologyRepairSession.Create(source,RepairSessionKind.Wireframe,.001);using var foreign=TopologyRepairSession.Create(source,RepairSessionKind.Wireframe,.001);using var maps=session.CheckSmallEdges();
        Assert.True(maps.Changed);using var small=maps.PrimaryKeys;using var keys=maps.FirstMapKeys;using var faceKeys=maps.SecondMapKeys;using var multiple=maps.SecondaryKeys;Assert.NotEmpty(Enumerable.Range(0,small.Count));Assert.True(keys.Count>0);Assert.Equal(1,faceKeys.Count);
        for(int i=0;i<keys.Count;++i){using var values=maps.FirstMapValues(i);Assert.True(values.Count>0);}using var faceValues=maps.SecondMapValues(0);Assert.True(faceValues.Count>0);
        Assert.Throws<ArgumentException>(()=>maps.FirstMapValues(keys.Count));Assert.Throws<ArgumentException>(()=>foreign.MergeSmallEdges(maps));Assert.Throws<ArgumentException>(()=>session.MergeSmallEdges(maps,limitAngle:double.NaN));
        using var changed=session.MergeSmallEdges(maps);Assert.True(changed.Changed);using var newKeys=changed.FirstMapKeys;using var shapes=changed.Shapes;Assert.Equal(1,shapes.Count);Assert.Throws<ArgumentException>(()=>session.MergeSmallEdges(maps));
        session.Dispose();maps.Dispose();using var result=shapes.GetShape(0);Assert.True(result.IsValid);Assert.True(small.Count>0);
    }
    [Fact] public void ConfigurationChangesInvalidateMapsButRejectedConfigurationDoesNot()
    {
        using var source=Face();using var session=TopologyRepairSession.Create(source,RepairSessionKind.Wireframe);using var maps=session.CheckSmallEdges();Assert.Throws<InvalidOperationException>(()=>session.SetFaceMode(FaceRepairMode.SplitFace,1));
        using var noop=session.MergeSmallEdges(maps);Assert.False(noop.Changed);using var old=session.CheckSmallEdges();session.SetDropSmallEdges(false);Assert.Throws<ArgumentException>(()=>session.MergeSmallEdges(old));using var fresh=session.CheckSmallEdges();using var next=session.MergeSmallEdges(fresh);Assert.False(next.Changed);
    }
    [Fact] public void WireframeReportsCurveRepairsAndRetainsUnresolvedTopologicalGapEvidence()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try
        {
            using var moved=edges[0].Transformed(ShapeTransform.CreateTranslation(0,.0001,0));using var damaged=face.ReplaceSubshape(edges[0],moved);
            using var source=RepairSnapshot.Create(damaged);Assert.True(source.Metrics.MaximumEndpointGap>0);
            using var session=TopologyRepairSession.Create(damaged,RepairSessionKind.Wireframe,.001);Assert.True(session.Perform());
            // FixWireGaps fixes 3D/2D curve gaps; it does not promise to reconnect
            // arbitrary distinct vertices. Native DONE is not topology acceptance.
            using var output=session.Shape;using var fixedSnapshot=RepairSnapshot.Create(output);Assert.True(session.Status(ShapeExtendStatus.ShapeExtend_DONE));
            Assert.Equal(source.Metrics.MaximumEndpointGap,fixedSnapshot.Metrics.MaximumEndpointGap,10);
            using var after=RepairSnapshot.Create(damaged);Assert.Equal(source.Metrics.MaximumEndpointGap,after.Metrics.MaximumEndpointGap,10);
        }
        finally{DisposeAll(edges);}
    }
    [Fact] public void FaceOrientationMapsAndLoopOutputsStayInOneReceiverEpoch()
    {
        using var source=Face();using var session=TopologyRepairSession.Create(source,RepairSessionKind.Face);foreach(var mode in Enum.GetValues<FaceRepairMode>())session.SetFaceMode(mode,0);using var maps=session.FixFaceOrientation();using var keys=maps.FirstMapKeys;
        Assert.Equal(0,keys.Count);Assert.False(maps.Changed);using var split=session.SplitFace(maps);Assert.False(split.Changed);using var loops=session.FixFaceLoops();using var wires=loops.Shapes;Assert.Equal(0,wires.Count);using var result=session.Face;Assert.True(result.IsValid);Assert.Throws<ArgumentException>(()=>session.SplitFace(maps));Assert.Throws<ArgumentException>(()=>session.SetFaceMode((FaceRepairMode)100,0));
        _ = session.Perform();_ = session.Status(ShapeExtendStatus.ShapeExtend_OK);session.Dispose();Assert.True(result.IsValid);Assert.Equal(0,keys.Count);
    }
    [Fact] public void ShellAndSolidRepairPreserveOriginalVolumeAndIndependentOutputs()
    {
        using var source=ShapeFactory.CreateBox(2,3,4);var shells=source.GetSubShapes(ShapeKind.Shell);
        try{using var analysis=RepairDiagnostics.AnalyzeShell(shells[0]);Assert.True(analysis.IsSourceLoaded);Assert.Equal(1,analysis.Count);Assert.False(analysis.HasFreeEdges);Assert.True(analysis.HasConnectedEdges);using var loaded=analysis.LoadedShells;using var bad=analysis.BadEdges;using var free=analysis.FreeEdges;Assert.Equal(0,bad.Count);Assert.Equal(0,free.Count);
            using var shell=TopologyRepairSession.Create(shells[0],RepairSessionKind.Shell);shell.SetShellOrientationMode(1);_ = shell.Perform();using var fixedShell=shell.Shell;using var errors=shell.ErrorFaces;Assert.Equal(0,errors.CountSubShapes(ShapeKind.Face));using var newSolid=RepairDiagnostics.SolidFromShell(fixedShell);Assert.True(newSolid.IsValid);
            using var solid=TopologyRepairSession.Create(source,RepairSessionKind.Solid);_ = solid.Perform();using var output=solid.Shape;Assert.True(output.IsValid);Assert.Equal(24,output.InspectProperties(InspectionPropertyKind.Volume).Mass,7);Assert.Equal(24,source.InspectProperties(InspectionPropertyKind.Volume).Mass,7);
            shell.Dispose();solid.Dispose();source.Dispose();Assert.True(output.IsValid);Assert.Equal(1,loaded.Count);
        }finally{DisposeAll(shells);}
    }
    [Fact] public void OpenShellDiagnosticsReportEveryFreeEdge()
    {
        using var box=ShapeFactory.CreateBox(2,3,4);var faces=box.GetFaces();
        try{using var open=ShapeFactory.Sew(faces.Take(5).ToArray());using var report=RepairDiagnostics.AnalyzeShell(open,true);using var free=report.FreeEdges;Assert.True(report.HasFreeEdges);Assert.True(free.Count>0);}
        finally{DisposeAll(faces);}
    }
    [Fact] public void IncorrectShellOrientationReportsBadEdgesAndActuallyRepairsThem()
    {
        using var box=ShapeFactory.CreateBox(2,3,4);var shells=box.GetSubShapes(ShapeKind.Shell);
        string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AJ."+Guid.NewGuid().ToString("N")+".brep");
        try
        {
            ShapeExchange.WriteBrep(shells[0],path);string text=File.ReadAllText(path);
            var pattern=new System.Text.RegularExpressions.Regex(@"(?m)(^Sh\s*[01]+\s*)([+-])");Assert.Matches(pattern,text);
            File.WriteAllText(path,pattern.Replace(text,m=>m.Groups[1].Value+(m.Groups[2].Value=="+"?"-":"+"),1));
            using var damaged=ShapeExchange.ReadBrep(path);using var diagnostic=RepairDiagnostics.AnalyzeShell(damaged);using var bad=diagnostic.BadEdges;Assert.True(bad.Count>0);
            using var session=TopologyRepairSession.Create(damaged,RepairSessionKind.Shell);Assert.True(session.Perform());using var repaired=session.Shape;
            using var clean=RepairDiagnostics.AnalyzeShell(repaired);using var cleanBad=clean.BadEdges;Assert.Equal(0,cleanBad.Count);
            using var after=RepairDiagnostics.AnalyzeShell(damaged);using var originalBad=after.BadEdges;Assert.Equal(bad.Count,originalBad.Count);
        }
        finally{File.Delete(path);DisposeAll(shells);}
    }
    [Fact] public void FreeBoundConnectionDispatchAndSplitResultsSurviveInputs()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var other=ShapeFactory.CreateEdge(new(1,0,0),new(2,0,0));using var edges=OwnedShapeSet.Create(edge,other);using var connected=RepairDiagnostics.ConnectEdges(edges);using var wires=connected.Shapes;Assert.Equal(1,wires.Count);
        using var joined=RepairDiagnostics.ConnectWires(wires);using var joinedWires=joined.Shapes;using var vertexKeys=joined.FirstMapKeys;using var dispatched=RepairDiagnostics.DispatchWires(joinedWires);using var compounds=dispatched.Shapes;Assert.Equal(2,compounds.Count);
        using var split=RepairDiagnostics.SplitWires(joinedWires);using var closed=split.PrimaryKeys;using var open=split.SecondaryKeys;Assert.Equal(0,closed.Count);Assert.Equal(1,open.Count);
        using var face=Face();using var bounds=RepairDiagnostics.FreeBounds(face);using var freeBounds=bounds.Shapes;Assert.Equal(2,freeBounds.Count);edges.Dispose();connected.Dispose();joined.Dispose();Assert.Equal(1,wires.Count);
    }
    [Fact] public void FreeBoundEmptyDuplicateAndWrongTypeInputsAreExplicit()
    {
        using var empty=OwnedShapeSet.Create();using var connected=RepairDiagnostics.ConnectEdges(empty);using var shapes=connected.Shapes;Assert.Equal(0,shapes.Count);using var box=ShapeFactory.CreateBox(1,1,1);using var wrong=OwnedShapeSet.Create(box);Assert.Throws<ArgumentException>(()=>RepairDiagnostics.ConnectEdges(wrong));
        Assert.Throws<ArgumentException>(()=>RepairDiagnostics.ConnectEdges(empty,0));Assert.Throws<ArgumentException>(()=>RepairDiagnostics.ConnectWires(empty,double.NaN));using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var duplicates=OwnedShapeSet.Create(edge,edge);using var dupeResult=RepairDiagnostics.ConnectEdges(duplicates);using var duplicated=dupeResult.Shapes;Assert.True(duplicated.Count>0);
    }
    [Fact] public void ExactSplitHistoryRetainsEveryPathInTheFrozenResultIdentityDomain()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var splitWire=ShapeFactory.CreatePolygonWire([new(0,0,0),new(1,0,0),new(2,0,0)],false);var children=splitWire.GetSubShapes(ShapeKind.Edge);using var first=children[0];using var second=children[1];using var parts=OwnedShapeSet.Create(first,second);using var session=TopologyRepairSession.Create(edge,RepairSessionKind.Replacement);session.Replace(edge,parts);using var result=session.Snapshot();using var paths=result.Modified(edge);using var leaf=result.Leaf(edge);using var shape=result.Shape;
        Assert.Equal(2,paths.Count);Assert.Equal(1,leaf.Count);Assert.Equal(RepairRelationEvidence.Modified,result.Relation(edge).Evidence);Assert.Equal(2,shape.CountSubShapes(ShapeKind.Edge));
        var outputs=shape.GetSubShapes(ShapeKind.Edge);try{for(int i=0;i<paths.Count;++i){using var p=paths.GetShape(i);Assert.Contains(outputs,s=>s.IsSame(p));}}finally{DisposeAll(outputs);}
        using var lookalike=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));Assert.Throws<ArgumentException>(()=>result.Modified(lookalike));Assert.Throws<ArgumentException>(()=>result.Relation(lookalike));session.Dispose();result.Dispose();edge.Dispose();Assert.Equal(2,paths.Count);Assert.True(shape.IsValid);
    }
    [Fact] public void ExactMergeHistoryPreservesCommonOutputForAllInputs()
    {
        using var first=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var second=ShapeFactory.CreateEdge(new(1,0,0),new(2,0,0));using var source=ShapeFactory.CreateCompound([first,second]);using var joined=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var inputs=OwnedShapeSet.Create(first,second);using var session=TopologyRepairSession.Create(source,RepairSessionKind.Replacement);session.Merge(inputs,joined);using var result=session.Snapshot();using var a=result.Modified(first);using var b=result.Modified(second);Assert.Equal(1,a.Count);Assert.Equal(1,b.Count);using var one=a.GetShape(0);using var two=b.GetShape(0);Assert.True(one.IsSame(two));using var output=result.Shape;Assert.Equal(1,output.CountSubShapes(ShapeKind.Edge));Assert.Equal(2,source.CountSubShapes(ShapeKind.Edge));
    }
    [Fact] public void RemovalNoopAndUnavailableEvidenceRemainDistinct()
    {
        using var first=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var second=ShapeFactory.CreateEdge(new(1,0,0),new(2,0,0));using var source=ShapeFactory.CreateCompound([first,second]);using var session=TopologyRepairSession.Create(source,RepairSessionKind.Replacement);using var before=session.Snapshot();Assert.Equal(RepairRelationEvidence.Unchanged,before.Relation(first).Evidence);Assert.Equal(RepairRelationEvidence.Unavailable,before.Relation(source).Evidence);
        session.Remove(first);using var after=session.Snapshot();Assert.Equal(RepairRelationEvidence.Removed,after.Relation(first).Evidence);using var paths=after.Modified(first);using var leaf=after.Leaf(first);using var generated=after.Generated(second);Assert.Equal(0,paths.Count);Assert.Equal(0,leaf.Count);Assert.Equal(0,generated.Count);using var applied=session.ApplyUntil();Assert.Equal(1,applied.CountSubShapes(ShapeKind.Edge));Assert.Throws<ArgumentException>(()=>session.ApplyUntil(9));
    }
    [Fact] public void ReplacementMembershipDuplicateTypeAndLocationValidationPreserveTheReceiver()
    {
        using var first=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var source=ShapeFactory.CreateCompound([first]);using var same=RepairDiagnostics.WithOrientation(first,1);using var foreign=ShapeFactory.CreateEdge(new(0,0,0),new(1,0,0));using var shifted=first.Transformed(ShapeTransform.CreateTranslation(1,0,0));using var session=TopologyRepairSession.Create(source,RepairSessionKind.Replacement);using var output=OwnedShapeSet.Create(first);
        Assert.Throws<ArgumentException>(()=>session.Replace(foreign,output));Assert.Throws<ArgumentException>(()=>session.Remove(shifted));using var duplicates=OwnedShapeSet.Create(first,first);Assert.Throws<ArgumentException>(()=>session.Merge(duplicates,foreign));using var wrong=OwnedShapeSet.Create(source);Assert.Throws<ArgumentException>(()=>session.Replace(first,wrong));using var empty=OwnedShapeSet.Create();Assert.Throws<ArgumentException>(()=>session.Replace(first,empty));
        using var result=session.Snapshot();Assert.Equal(RepairRelationEvidence.Unchanged,result.Relation(same).Evidence);Assert.Equal(RepairRelationEvidence.Unchanged,result.Relation(first).Evidence);
    }
    [Fact] public void PrivateVertexCopiesPreserveOriginalPositionAndTolerances()
    {
        using var source=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));var vertices=source.GetSubShapes(ShapeKind.Vertex);
        try{using var session=TopologyRepairSession.Create(source,RepairSessionKind.Replacement);using var tolerance=session.CopyVertex(vertices[0],1e-4);using var moved=session.CopyVertex(vertices[1],1e-4,new(2,1,0));Assert.True(tolerance.IsValid);Assert.True(moved.IsValid);using var result=session.Snapshot();Assert.Equal(RepairRelationEvidence.Modified,result.Relation(vertices[1]).Evidence);Assert.Equal(2,source.GetEdgeLength(),10);Assert.Throws<ArgumentException>(()=>session.CopyVertex(vertices[0],double.NaN));}
        finally{DisposeAll(vertices);}
    }
    [Fact] public void MultiVertexReplacementIsRejectedBeforeMutationWhenNativeHistoryCannotRepresentIt()
    {
        using var source=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));var vertices=source.GetSubShapes(ShapeKind.Vertex);
        try{using var outputs=OwnedShapeSet.Create(vertices);using var session=TopologyRepairSession.Create(source,RepairSessionKind.Replacement);Assert.Throws<ArgumentException>(()=>session.Replace(vertices[0],outputs));using var snapshot=session.Snapshot();Assert.Equal(RepairRelationEvidence.Unchanged,snapshot.Relation(vertices[0]).Evidence);}
        finally{DisposeAll(vertices);}
    }
    [Theory][InlineData(RepairEdgeOperation.AddCurve3d)][InlineData(RepairEdgeOperation.AddPCurve)][InlineData(RepairEdgeOperation.RemoveCurve3d)][InlineData(RepairEdgeOperation.RemovePCurve)][InlineData(RepairEdgeOperation.SameParameter)][InlineData(RepairEdgeOperation.SameParameterOnFace)][InlineData(RepairEdgeOperation.VertexTolerance)][InlineData(RepairEdgeOperation.VertexToleranceOnFace)][InlineData(RepairEdgeOperation.Reversed2d)]
    public void EdgeFixesHaveIndependentGeometryAndPreserveOriginal(RepairEdgeOperation operation)
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{double length=edges[0].GetEdgeLength();using var fixedEdge=RepairDiagnostics.FixEdge(edges[0],operation,face);using var result=fixedEdge.Shapes;using var edge=result.GetShape(0);Assert.Equal(length,edges[0].GetEdgeLength(),10);face.Dispose();fixedEdge.Dispose();Assert.True(edge.GetEdgeLength()>0);}
        finally{DisposeAll(edges);}
    }
    [Fact] public void SplitToolPublishesBothEdgesAndRejectsForeignFaceMembers()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{var curve=edges[0].GetEdgeCurveSnapshot();double middle=(curve.FirstParameter+curve.LastParameter)/2;using var vertex=Vertex(edges[0].EvaluateEdge(middle).Point);using var split=RepairDiagnostics.SplitEdge(edges[0],face,vertex,middle);using var shapes=split.Shapes;Assert.True(split.Changed);Assert.Equal(2,shapes.Count);double length=0;for(int i=0;i<2;++i){using var e=shapes.GetShape(i);length+=e.GetEdgeLength();}Assert.Equal(edges[0].GetEdgeLength(),length,6);
            using var nochange=RepairDiagnostics.SplitEdge(edges[0],face,vertex,curve.FirstParameter);Assert.False(nochange.Changed);using var empty=nochange.Shapes;Assert.Equal(0,empty.Count);
            using var lookalike=ShapeFactory.CreateEdge(edges[0].EvaluateEdge(curve.FirstParameter).Point,edges[0].EvaluateEdge(curve.LastParameter).Point);Assert.Throws<ArgumentException>(()=>RepairDiagnostics.SplitEdge(lookalike,face,vertex,middle));Assert.Throws<ArgumentException>(()=>RepairDiagnostics.SplitEdge(edges[0],face,vertex,double.PositiveInfinity));
        }finally{DisposeAll(edges);}
    }
    [Fact] public void SplitCutSpanAndCutPublishCoupledSequencesAndSelectedIndices()
    {
        using var face=Face();var edges=face.GetSubShapes(ShapeKind.Edge);
        try{var c=edges[0].GetEdgeCurveSnapshot();double a=c.FirstParameter+(c.LastParameter-c.FirstParameter)*.25,b=c.FirstParameter+(c.LastParameter-c.FirstParameter)*.75;using var va=Vertex(edges[0].EvaluateEdge(a).Point);using var vb=Vertex(edges[0].EvaluateEdge(b).Point);using var vm=Vertex(edges[0].EvaluateEdge((a+b)/2).Point);
            using var cut=RepairDiagnostics.CutEdge(edges[0],face,c.FirstParameter,a);using var cutShapes=cut.Shapes;Assert.Equal(1,cutShapes.Count);
            using var splitCut=RepairDiagnostics.SplitCutEdge(edges[0],face,vm,a,b);using var splitShapes=splitCut.Shapes;Assert.True(splitCut.Changed);Assert.Equal(2,splitShapes.Count);
            using var span=RepairDiagnostics.SplitSpan(edges[0],face,va,vb,a,b);using var spanShapes=span.Shapes;Assert.True(span.Changed);Assert.True(spanShapes.Count>=2);Assert.InRange(span.SelectedNativeIndex,1,spanShapes.Count);Assert.Equal(c.LastParameter-c.FirstParameter,edges[0].GetEdgeLength(),7);
        }finally{DisposeAll(edges);}
    }
    [Fact] public void SafeHandleReleaseRacesKeepIndependentWireAndSegmentOutputs()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var set=OwnedShapeSet.Create(edge);
        for(int attempt=0;attempt<40;++attempt){using var data=WireDataSnapshot.Create(set);using var segment=RepairWireSegment.Create(data);var errors=new System.Collections.Concurrent.ConcurrentQueue<Exception>();
            Parallel.For(0,12,i=>{try{if(i==0){data.Dispose();segment.Dispose();}else{using var snapshot=segment.WireData;using var value=snapshot.GetEdge(1);Assert.True(value.IsValid);}}catch(ObjectDisposedException){}catch(Exception e){errors.Enqueue(e);}});Assert.Empty(errors);}
    }
    [Fact] public void SessionWorkspaceAndHistoryReleaseRacesFinishWithIndependentValues()
    {
        using var source=Face();
        for(int attempt=0;attempt<30;++attempt){using var session=TopologyRepairSession.Create(source,RepairSessionKind.Wireframe);using var maps=session.CheckSmallEdges();using var result=session.Snapshot();var errors=new System.Collections.Concurrent.ConcurrentQueue<Exception>();
            Parallel.For(0,12,i=>{try{if(i==0){session.Dispose();maps.Dispose();result.Dispose();}else if(i%2==0){using var shape=result.Shape;Assert.True(shape.IsValid);}else{using var keys=maps.FirstMapKeys;Assert.Equal(4,keys.Count);}}catch(ObjectDisposedException){}catch(Exception e){errors.Enqueue(e);}});Assert.Empty(errors);}
    }
    [Fact] public void FailedNullResultPublicationRetiresThePrivateReceiverAndPreservesTheSource()
    {
        using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var session=TopologyRepairSession.Create(edge,RepairSessionKind.Replacement);session.Remove(edge);
        Assert.Throws<ArgumentException>(()=>session.Snapshot());Assert.Throws<ArgumentException>(()=>session.Shape);Assert.Throws<ArgumentException>(()=>session.Remove(edge));Assert.Equal(2,edge.GetEdgeLength(),10);
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int InfoRaw(nint token,nint values,int capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SetRaw(nint token,int selector,int index,out nint result);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void ReleaseRaw(nint token,int kind);
    static nint Token(object owner)=>(owner.GetType().GetField("handle",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(owner) as SafeHandle)!.DangerousGetHandle();
    [Fact] public unsafe void RawWrongStaleTokensBadCapacitiesAndPartialOutputsStayContained()
    {
        _ = OcctRuntime.Info;nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        try{T Export<T>(string name)where T:Delegate=>Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library,name));
            var info=Export<InfoRaw>("occtsharp_repair_wiredata_info");var get=Export<SetRaw>("occtsharp_repair_workspace_set");var release=Export<ReleaseRaw>("occtsharp_repair_release");
            using var edge=ShapeFactory.CreateEdge(new(0,0,0),new(2,0,0));using var set=OwnedShapeSet.Create(edge);using var data=WireDataSnapshot.Create(set);nint token=Token(data);int* values=stackalloc int[4];for(int i=0;i<4;++i)values[i]=123;
            Assert.NotEqual(0,info(token,(nint)values,3));Assert.Equal(123,values[0]);Assert.NotEqual(0,info(Token(set),(nint)values,4));Assert.Equal(123,values[0]);Assert.NotEqual(0,get(token,0,0,out var result));Assert.Equal(0,result);
            release(token,2);Assert.Equal(0,info(token,(nint)values,4));Assert.Equal(1,values[0]);data.Dispose();for(int i=0;i<4;++i)values[i]=321;Assert.NotEqual(0,info(token,(nint)values,4));Assert.Equal(321,values[0]);Assert.NotEqual(0,get(0,5,int.MaxValue,out result));Assert.Equal(0,result);
        }finally{NativeLibrary.Free(library);}
    }
}
