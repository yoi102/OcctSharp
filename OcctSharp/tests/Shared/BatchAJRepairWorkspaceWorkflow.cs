using OcctSharp.CopiedCollections;
using OcctSharp.RepairWorkspaces;
namespace OcctSharp.Tests.Shared;

internal static class BatchAJRepairWorkspaceWorkflow
{
    internal static Shape DisconnectedFace()
    {
        static Shape Square(double x, double y, double size)
        {
            using var wire=ShapeFactory.CreatePolygonWire([new(x,y,0),new(x+size,y,0),new(x+size,y+size,0),new(x,y+size,0)],true);
            return ShapeFactory.CreatePlanarFace(wire);
        }
        using var outer=Square(0,0,10);using var firstHole=Square(1,1,2);using var secondHole=Square(5,5,2);
        using var firstCut=outer.Cut(firstHole);using var secondCut=firstCut.Cut(secondHole);using var holed=secondCut.GetFaces().Single();
        var wires=holed.GetSubShapes(ShapeKind.Wire);
        try
        {
            using var removed=holed.RemoveSubshape(wires.MaxBy(w=>w.InspectProperties(InspectionPropertyKind.Length).Mass)!);
            var bounds=removed.GetSubShapes(ShapeKind.Wire);
            Shape ordered=removed.GetFaces().Single();
            try
            {
                foreach(var bound in bounds)
                {
                    // Boolean output stores these edges out of traversal order. SplitFace
                    // requires chained bounds; use the public ordered-wire projection.
                    using var data=WireDataSnapshot.FromWire(bound,false);
                    using var wire=data.ToWire();
                    var next=ordered.ReplaceSubshape(bound,wire);ordered.Dispose();ordered=next;
                }
                return ordered;
            }
            catch { ordered.Dispose();throw; }
            finally { foreach(var bound in bounds)bound.Dispose(); }
        }
        finally { foreach(var wire in wires)wire.Dispose(); }
    }

    internal static void SplitDisconnectedFace()
    {
        using var source=DisconnectedFace();
        double sourceArea=source.InspectProperties(InspectionPropertyKind.Area).Mass;
        using var session=TopologyRepairSession.Create(source,RepairSessionKind.Face);
        using var orientation=session.FixFaceOrientation();using var outerWires=orientation.FirstMapKeys;
        using var split=session.SplitFace(orientation);using var outputs=split.Shapes;
        using var result=session.Snapshot();using var shape=result.Shape;
        var faces=shape.GetFaces();double area;
        try{area=faces.Sum(f=>f.InspectProperties(InspectionPropertyKind.Area).Mass);}finally{foreach(var face in faces)face.Dispose();}
        if(outerWires.Count!=2 || !split.Changed || shape.CountSubShapes(ShapeKind.Face)!=2 || !shape.IsValid ||
            Math.Abs(area-8)>1e-6 ||
            source.CountSubShapes(ShapeKind.Face)!=1 || Math.Abs(source.InspectProperties(InspectionPropertyKind.Area).Mass-sourceArea)>1e-10)
            throw new InvalidOperationException($"AJ disconnected outer bounds did not split into two independent valid faces: outerWires={outerWires.Count}, changed={split.Changed}, faces={shape.CountSubShapes(ShapeKind.Face)}, valid={shape.IsValid}, area={area:R}, sourceFaces={source.CountSubShapes(ShapeKind.Face)}, sourceAreaBefore={sourceArea:R}, sourceAreaAfter={source.InspectProperties(InspectionPropertyKind.Area).Mass:R}.");
        source.Dispose();session.Dispose();result.Dispose();orientation.Dispose();split.Dispose();
        if(!shape.IsValid || outputs.Count!=1 || outerWires.Count!=2)
            throw new InvalidOperationException("AJ split-face output lifetime failed.");
    }
    internal static Shape TinyEdgeFace()
    {
        using var wire = ShapeFactory.CreatePolygonWire(
            [new(0,0,0), new(0.0001,0,0), new(4,0,0), new(4,3,0), new(0,3,0)], true);
        return ShapeFactory.CreatePlanarFace(wire);
    }

    internal static void Run()
    {
        SplitDisconnectedFace();
        using var source = TinyEdgeFace();
        double originalArea = source.InspectProperties(InspectionPropertyKind.Area).Mass;
        int originalEdges = source.CountSubShapes(ShapeKind.Edge);
        using var repair = TopologyRepairSession.Create(source, RepairSessionKind.Wireframe, 0.001);
        using var maps = repair.CheckSmallEdges();
        using var small = maps.PrimaryKeys;
        using var edgeFaces = maps.FirstMapKeys;
        using var faceSmall = maps.SecondMapKeys;
        using var multiple = maps.SecondaryKeys;
        if (small.Count == 0 || edgeFaces.Count == 0 || faceSmall.Count == 0)
            throw new InvalidOperationException("AJ did not detect and retain the coupled small-edge workspace.");
        using var merged = repair.MergeSmallEdges(maps);
        using var updatedSmall = merged.PrimaryKeys;
        using var changedCollections = merged.Shapes;
        using var result = repair.Snapshot();
        using var fixedShape = result.Shape;
        if (!merged.Changed || !fixedShape.IsValid || fixedShape.CountSubShapes(ShapeKind.Edge) >= originalEdges)
            throw new InvalidOperationException("AJ small-edge repair did not actually change valid topology.");
        if (source.CountSubShapes(ShapeKind.Edge) != originalEdges ||
            Math.Abs(source.InspectProperties(InspectionPropertyKind.Area).Mass-originalArea) > 1e-10 ||
            Math.Abs(fixedShape.InspectProperties(InspectionPropertyKind.Area).Mass-originalArea) > 1e-6)
            throw new InvalidOperationException("AJ repair changed the original or exceeded its area tolerance.");
        using var faceRepair = TopologyRepairSession.Create(fixedShape, RepairSessionKind.Face);
        using var orientation = faceRepair.FixFaceOrientation();
        using var splitFace = faceRepair.SplitFace(orientation);
        using var final = faceRepair.Snapshot();
        using var finalShape = final.Shape;
        repair.Dispose();faceRepair.Dispose();source.Dispose();maps.Dispose();merged.Dispose();result.Dispose();
        if (!fixedShape.IsValid || !finalShape.IsValid || changedCollections.Count == 0)
            throw new InvalidOperationException("AJ independent repair result lifetime failed.");
        using var inspection = RepairSnapshot.Create(finalShape);
        if (!inspection.Metrics.IsValid) throw new InvalidOperationException("AJ final validation failed.");
        foreach (var format in new[] { ExchangeCollectionFormat.Step, ExchangeCollectionFormat.Iges })
        {
            string path = Path.Combine(Path.GetTempPath(), "OcctSharp.AJ."+Guid.NewGuid().ToString("N")+
                (format==ExchangeCollectionFormat.Step?".step":".iges"));
            try
            {
                if (format==ExchangeCollectionFormat.Step) ShapeExchange.WriteStep(finalShape,path);
                else ShapeExchange.WriteIges(finalShape,path);
                using var session = ExchangeCollectionSession.Read(path,format);
                using var roots = session.GiveList();
                using var entities = session.Entities;
                if (roots.Count == 0 || entities.Count == 0 || session.Transfer(roots,ExchangeCollectionTransfer.Record) <= 0)
                    throw new InvalidOperationException("AJ real-file transfer failed.");
                var stats = session.GetStatsTransfer(roots);
                using var shapes = session.ShapeResults(true);
                using var restored = shapes.GetShape(0);
                if (stats.Failures != 0 || stats.Results == 0 || !restored.IsValid ||
                    Math.Abs(restored.InspectProperties(InspectionPropertyKind.Area).Mass-originalArea)>1e-5)
                    throw new InvalidOperationException("AJ repaired geometry roundtrip failed.");
            }
            finally { File.Delete(path); }
        }
    }
}
