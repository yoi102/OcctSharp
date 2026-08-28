using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class GeometryTopologyTests
{
    [Fact]
    public void AnalyticAndSplineCurveBuildersProduceEvaluableOwnedEdges()
    {
        using Shape circle = ShapeFactory.CreateCircleEdge(
            new GpPoint(0, 0, 0), new GpPoint(0, 0, 1), 2);
        using Shape arc = ShapeFactory.CreateArcEdge(
            new GpPoint(1, 0, 0),
            new GpPoint(Math.Sqrt(0.5), Math.Sqrt(0.5), 0),
            new GpPoint(0, 1, 0));
        using Shape ellipse = ShapeFactory.CreateEllipseEdge(
            new GpPoint(0, 0, 0),
            new GpPoint(0, 0, 1),
            new GpPoint(1, 0, 0),
            3,
            1);
        using Shape bezier = ShapeFactory.CreateBezierEdge(
            [new GpPoint(0, 0, 0), new GpPoint(1, 2, 0), new GpPoint(3, 0, 0)]);
        using Shape interpolated = ShapeFactory.CreateInterpolatedEdge(
            [new GpPoint(0, 0, 0), new GpPoint(1, 2, 0), new GpPoint(3, 1, 0), new GpPoint(4, 0, 0)]);

        Assert.Equal(CurveGeometryType.Circle, circle.GetEdgeCurveSnapshot().CurveType);
        Assert.Equal(CurveGeometryType.Circle, arc.GetEdgeCurveSnapshot().CurveType);
        Assert.Equal(CurveGeometryType.Ellipse, ellipse.GetEdgeCurveSnapshot().CurveType);
        Assert.Equal(CurveGeometryType.BezierCurve, bezier.GetEdgeCurveSnapshot().CurveType);
        Assert.Equal(CurveGeometryType.BSplineCurve, interpolated.GetEdgeCurveSnapshot().CurveType);
        Assert.Equal(4 * Math.PI, circle.GetEdgeLength(), 8);
        Assert.Equal(Math.PI / 2, arc.GetEdgeLength(), 6);
        Assert.True(ellipse.GetEdgeLength() > 12);
        Assert.True(bezier.GetEdgeLength() > 3);
        Assert.True(interpolated.GetEdgeLength() > 4);

        EdgeCurveSnapshot snapshot = bezier.GetEdgeCurveSnapshot();
        CurveEvaluation evaluation = bezier.EvaluateEdge((snapshot.FirstParameter + snapshot.LastParameter) / 2);
        Assert.True(double.IsFinite(evaluation.Point.X));
        Assert.Equal(1, VectorMagnitude(evaluation.Tangent), 10);
    }

    [Fact]
    public void EdgeProjectionReturnsNearestBoundedCurvePoint()
    {
        using Shape edge = ShapeFactory.CreateEdge(new GpPoint(0, 0, 0), new GpPoint(10, 0, 0));

        CurveProjection projection = edge.ProjectPointOnEdge(new GpPoint(3, 4, 0));

        Assert.Equal(new GpPoint(3, 0, 0), projection.Point);
        Assert.Equal(3, projection.Parameter, 10);
        Assert.Equal(4, projection.Distance, 10);
        Assert.True(projection.SolutionCount >= 1);
    }

    [Fact]
    public void FaceEvaluationAndProjectionRespectBoundedFaceAndOrientation()
    {
        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        Shape[] faces = box.GetFaces();
        try
        {
            Shape face = faces[0];
            FaceSurfaceSnapshot bounds = face.GetFaceSurfaceSnapshot();
            double u = (bounds.FirstUParameter + bounds.LastUParameter) / 2;
            double v = (bounds.FirstVParameter + bounds.LastVParameter) / 2;
            SurfaceEvaluation evaluation = face.EvaluateFace(u, v);
            GpPoint query = new(
                evaluation.Point.X + evaluation.Normal.X * 2,
                evaluation.Point.Y + evaluation.Normal.Y * 2,
                evaluation.Point.Z + evaluation.Normal.Z * 2);

            SurfaceProjection projection = face.ProjectPointOnFace(query);

            Assert.Equal(1, VectorMagnitude(evaluation.Normal), 10);
            Assert.Equal(evaluation.Point.X, projection.Point.X, 7);
            Assert.Equal(evaluation.Point.Y, projection.Point.Y, 7);
            Assert.Equal(evaluation.Point.Z, projection.Point.Z, 7);
            Assert.Equal(2, projection.Distance, 7);
            Assert.True(projection.SolutionCount >= 1);
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }
    }

    [Fact]
    public void CurveSurfaceAndPcurveDerivativesAreCopiedValues()
    {
        using Shape line = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(10, 0, 0));
        CurveDerivativeEvaluation curve = line.EvaluateEdgeDerivatives(4);
        Assert.Equal(new GpPoint(4, 0, 0), curve.Point);
        Assert.Equal(new GpPoint(1, 0, 0), curve.FirstDerivative);
        Assert.Equal(GpPoint.Origin, curve.SecondDerivative);

        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        Shape[] faces = box.GetFaces();
        Shape[] edges = faces[0].GetSubShapes(ShapeKind.Edge);
        try
        {
            FaceSurfaceSnapshot bounds = faces[0].GetFaceSurfaceSnapshot();
            double u = (bounds.FirstUParameter + bounds.LastUParameter) / 2;
            double v = (bounds.FirstVParameter + bounds.LastVParameter) / 2;
            SurfaceDerivativeEvaluation surface = faces[0].EvaluateFaceDerivatives(u, v);
            Assert.Equal(1, VectorMagnitude(surface.Normal), 10);
            Assert.True(VectorMagnitude(surface.UDerivative) > 0);
            Assert.True(VectorMagnitude(surface.VDerivative) > 0);

            PcurveSnapshot pcurve = edges[0].GetPcurveSnapshot(faces[0]);
            double parameter = (pcurve.FirstParameter + pcurve.LastParameter) / 2;
            PcurveEvaluation evaluation = edges[0].EvaluatePcurve(faces[0], parameter);
            Assert.Equal(parameter, evaluation.Parameter, 10);
            Assert.Equal(1, Math.Sqrt(
                evaluation.Tangent.X * evaluation.Tangent.X
                + evaluation.Tangent.Y * evaluation.Tangent.Y), 10);

            faces[0].Dispose();
            edges[0].Dispose();
            Assert.True(double.IsFinite(surface.Point.X));
            Assert.True(double.IsFinite(evaluation.Point.X));
        }
        finally
        {
            foreach (Shape edge in edges) edge.Dispose();
            foreach (Shape face in faces) face.Dispose();
        }
    }

    [Fact]
    public void EdgeFaceTrimmingAndWireConstructionReturnIndependentOwners()
    {
        Shape first = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(10, 0, 0));
        Shape second = ShapeFactory.CreateEdge(new GpPoint(10, 0, 0), new GpPoint(10, 5, 0));
        using Shape trimmedEdge = first.TrimEdge(2, 8);
        using Shape wire = ShapeFactory.CreateWire([first, second]);
        Assert.Equal(6, trimmedEdge.GetEdgeLength(), 10);
        Assert.Equal(ShapeKind.Wire, wire.Kind);
        Assert.Equal(2, wire.CountSubShapes(ShapeKind.Edge));

        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        Shape[] faces = box.GetFaces();
        Shape trimmedFace;
        try
        {
            FaceSurfaceSnapshot bounds = faces[0].GetFaceSurfaceSnapshot();
            double uPadding = (bounds.LastUParameter - bounds.FirstUParameter) / 4;
            double vPadding = (bounds.LastVParameter - bounds.FirstVParameter) / 4;
            trimmedFace = faces[0].TrimFace(
                bounds.FirstUParameter + uPadding,
                bounds.LastUParameter - uPadding,
                bounds.FirstVParameter + vPadding,
                bounds.LastVParameter - vPadding);
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }

        using (trimmedFace)
        {
            first.Dispose();
            second.Dispose();
            box.Dispose();
            Assert.Equal(ShapeKind.Edge, trimmedEdge.Kind);
            Assert.Equal(ShapeKind.Wire, wire.Kind);
            Assert.Equal(ShapeKind.Face, trimmedFace.Kind);
            Assert.True(trimmedFace.IsValid);
        }
    }

    [Fact]
    public void TopologyAdjacencyCopiesUniqueEdgeToFaceRelations()
    {
        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        using TopologyAdjacencyMap map = box.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);

        Assert.Equal(ShapeKind.Edge, map.ItemKind);
        Assert.Equal(ShapeKind.Face, map.AncestorKind);
        Assert.Equal(12, map.Items.Count);
        Assert.Equal(6, map.Ancestors.Count);
        Assert.Equal(24, map.RelationCount);
        for (int index = 0; index < map.Items.Count; ++index)
        {
            Assert.Equal(ShapeKind.Edge, map.Items[index].Kind);
            ReadOnlySpan<int> ancestors = map.GetAncestorIndices(index).Span;
            Assert.Equal(2, ancestors.Length);
            Assert.All(ancestors.ToArray(), ancestor => Assert.InRange(ancestor, 0, map.Ancestors.Count - 1));
        }

        box.Dispose();
        Assert.All(map.Items, item => Assert.Equal(ShapeKind.Edge, item.Kind));
        Assert.All(map.Ancestors, ancestor => Assert.Equal(ShapeKind.Face, ancestor.Kind));
    }

    [Fact]
    public void ReverseAdjacencyAndTopologyReshapePreserveIndependentResults()
    {
        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        using TopologyAdjacencyMap map = box.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);
        for (int faceIndex = 0; faceIndex < map.Ancestors.Count; ++faceIndex)
        {
            IReadOnlyList<int> edgeIndices = map.GetItemIndices(faceIndex);
            Assert.Equal(4, edgeIndices.Count);
            Assert.All(edgeIndices, edgeIndex =>
                Assert.Contains(faceIndex, map.GetAncestorIndices(edgeIndex).ToArray()));
        }

        using Shape translatedBase = ShapeFactory.CreateBox(4, 5, 6);
        using Shape translated = translatedBase.Transformed(
            ShapeTransform.CreateTranslationAndRotationZ(8, 0, 0, 0));
        Shape compound = ShapeFactory.CreateCompound([box, translated]);
        Shape[] solids = compound.GetSubShapes(ShapeKind.Solid);
        using Shape replacement = ShapeFactory.CreateBox(2, 2, 2);
        Shape replaced = compound.ReplaceSubshape(solids[0], replacement);
        Shape removed = compound.RemoveSubshape(solids[1]);
        try
        {
            Assert.Equal(2, replaced.CountSubShapes(ShapeKind.Solid));
            Assert.Equal(1, removed.CountSubShapes(ShapeKind.Solid));

            compound.Dispose();
            foreach (Shape solid in solids) solid.Dispose();
            replacement.Dispose();
            Assert.Equal(2, replaced.CountSubShapes(ShapeKind.Solid));
            Assert.Equal(1, removed.CountSubShapes(ShapeKind.Solid));
        }
        finally
        {
            compound.Dispose();
            foreach (Shape solid in solids) solid.Dispose();
            replaced.Dispose();
            removed.Dispose();
        }
    }

    [Fact]
    public void LoftPipeAndSewCompleteCommonMultiShapeWorkflows()
    {
        using Shape lower = ShapeFactory.CreatePolygonWire(
            [new GpPoint(-1, -1, 0), new GpPoint(1, -1, 0), new GpPoint(1, 1, 0), new GpPoint(-1, 1, 0)],
            close: true);
        using Shape upper = ShapeFactory.CreatePolygonWire(
            [new GpPoint(-2, -2, 5), new GpPoint(2, -2, 5), new GpPoint(2, 2, 5), new GpPoint(-2, 2, 5)],
            close: true);
        using Shape loft = ShapeFactory.CreateLoft([lower, upper], makeSolid: true);
        Assert.Equal(ShapeKind.Solid, loft.Kind);
        Assert.True(loft.IsValid);
        Assert.Equal(5, loft.GetBoundingBox().SizeZ, 6);

        using Shape spine = ShapeFactory.CreatePolygonWire(
            [new GpPoint(0, 0, 0), new GpPoint(0, 0, 5)]);
        using Shape profile = ShapeFactory.CreatePolygonWire(
            [new GpPoint(-1, -1, 0), new GpPoint(1, -1, 0), new GpPoint(1, 1, 0), new GpPoint(-1, 1, 0)],
            close: true);
        using Shape pipe = ShapeFactory.CreatePipe(spine, profile);
        Assert.True(pipe.IsValid);
        Assert.True(pipe.CountSubShapes(ShapeKind.Face) >= 4);

        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        Shape[] faces = box.GetFaces();
        try
        {
            using Shape sewn = ShapeFactory.Sew(faces);
            Assert.True(sewn.IsValid);
            Assert.True(sewn.CountSubShapes(ShapeKind.Face) >= 6);
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }

        lower.Dispose();
        upper.Dispose();
        spine.Dispose();
        profile.Dispose();
        Assert.True(loft.IsValid);
        Assert.True(pipe.IsValid);
    }

    [Fact]
    public void WedgeAndThickSolidCompleteCommonSolidConstructionWorkflows()
    {
        using Shape wedge = ShapeFactory.CreateWedge(6, 5, 4, 2);
        Assert.Equal(ShapeKind.Solid, wedge.Kind);
        Assert.True(wedge.IsValid);
        Assert.Equal(6, wedge.GetBoundingBox().SizeX, 5);
        Assert.Equal(5, wedge.GetBoundingBox().SizeY, 5);
        Assert.Equal(4, wedge.GetBoundingBox().SizeZ, 5);

        using Shape box = ShapeFactory.CreateBox(6, 5, 4);
        Shape[] faces = box.GetFaces();
        try
        {
            using Shape hollow = box.MakeThickSolid([faces[0]], -0.5);
            Assert.Equal(ShapeKind.Solid, hollow.Kind);
            Assert.True(hollow.IsValid);
            Assert.True(hollow.CountSubShapes(ShapeKind.Face) > box.CountSubShapes(ShapeKind.Face));

            box.Dispose();
            faces[0].Dispose();
            Assert.True(hollow.IsValid);
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }
    }

    [Fact]
    public void BooleanHistoryCopiesFaceChangeSummariesAndOwnsResult()
    {
        using Shape left = ShapeFactory.CreateBox(4, 4, 4);
        using Shape rightBase = ShapeFactory.CreateBox(4, 4, 4);
        using Shape right = rightBase.Transformed(ShapeTransform.CreateTranslationAndRotationZ(2, 0, 0, 0));
        using BooleanOperationResult result = left.CutWithHistory(right, ShapeKind.Face);

        Assert.Equal(BooleanOperationKind.Cut, result.Operation);
        Assert.Equal(ShapeKind.Face, result.History.TrackedKind);
        Assert.Equal(6, result.History.Left.SourceCount);
        Assert.Equal(6, result.History.Right.SourceCount);
        Assert.True(
            result.History.Left.ModifiedSourceCount
            + result.History.Left.GeneratedSourceCount
            + result.History.Left.DeletedSourceCount > 0);
        Assert.True(result.Shape.IsValid);

        left.Dispose();
        right.Dispose();
        Assert.True(result.Shape.IsValid);
    }

    [Fact]
    public void GeometryOperationsRejectInvalidKindsParametersAndDisposedValues()
    {
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateCircleEdge(
            GpPoint.Origin, new GpPoint(0, 0, 1), 0));
        Assert.Throws<OcctException>(() => ShapeFactory.CreateArcEdge(
            GpPoint.Origin, new GpPoint(1, 0, 0), new GpPoint(2, 0, 0)));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateBezierEdge([GpPoint.Origin]));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateInterpolatedEdge(
            [GpPoint.Origin, new GpPoint(1, 0, 0)], periodic: true));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateWedge(1, 1, 1, -1));

        using Shape box = ShapeFactory.CreateBox(2, 2, 2);
        using Shape edge = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(1, 0, 0));
        Assert.Throws<InvalidCastException>(() => box.GetEdgeLength());
        Assert.Throws<InvalidCastException>(() => edge.EvaluateFace(0, 0));
        Assert.Throws<ArgumentException>(() => edge.EvaluateEdge(2));
        Assert.Throws<ArgumentException>(() => edge.EvaluateEdgeDerivatives(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => edge.TrimEdge(0.75, 0.25));
        Assert.Throws<InvalidCastException>(() => box.TrimEdge(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => edge.ProjectPointOnEdge(new GpPoint(double.NaN, 0, 0)));
        Assert.Throws<ArgumentException>(() => box.GetTopologyAdjacency(ShapeKind.Face, ShapeKind.Edge));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateWire([]));
        Assert.Throws<InvalidCastException>(() => ShapeFactory.CreateWire([box]));
        using Shape unrelated = ShapeFactory.CreateEdge(new GpPoint(4, 0, 0), new GpPoint(5, 0, 0));
        Assert.Throws<ArgumentException>(() => box.ReplaceSubshape(unrelated, edge));
        Assert.Throws<ArgumentException>(() => box.RemoveSubshape(unrelated));
        Assert.Throws<ArgumentException>(() => box.MakeThickSolid([], -0.1));
        Assert.Throws<InvalidCastException>(() => box.MakeThickSolid([edge], -0.1));

        edge.Dispose();
        Assert.Throws<ObjectDisposedException>(() => edge.GetEdgeLength());

        TopologyAdjacencyMap map = box.GetTopologyAdjacency(ShapeKind.Edge, ShapeKind.Face);
        Shape copiedEdge = map.Items[0];
        map.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _ = copiedEdge.Kind);
        Assert.Throws<ObjectDisposedException>(() => map.GetAncestorIndices(0));
    }

    [Fact]
    public void GeometryValueAbiLayoutsAreStable()
    {
        Assert.Equal(56, Marshal.SizeOf<CurveEvaluationRaw>());
        Assert.Equal(80, Marshal.SizeOf<CurveDerivativeEvaluationRaw>());
        Assert.Equal(48, Marshal.SizeOf<PcurveSnapshotRaw>());
        Assert.Equal(40, Marshal.SizeOf<PcurveEvaluationRaw>());
        Assert.Equal(48, Marshal.SizeOf<CurveProjectionRaw>());
        Assert.Equal(64, Marshal.SizeOf<SurfaceEvaluationRaw>());
        Assert.Equal(112, Marshal.SizeOf<SurfaceDerivativeEvaluationRaw>());
        Assert.Equal(56, Marshal.SizeOf<SurfaceProjectionRaw>());
        Assert.Equal(48, Marshal.SizeOf<BooleanHistorySummaryRaw>());
        Assert.Equal(8, Marshal.OffsetOf<CurveEvaluationRaw>(nameof(CurveEvaluationRaw.Point)).ToInt32());
        Assert.Equal(40, Marshal.OffsetOf<CurveProjectionRaw>(nameof(CurveProjectionRaw.SolutionCount)).ToInt32());
        Assert.Equal(16, Marshal.OffsetOf<SurfaceEvaluationRaw>(nameof(SurfaceEvaluationRaw.Point)).ToInt32());
        Assert.Equal(48, Marshal.OffsetOf<SurfaceProjectionRaw>(nameof(SurfaceProjectionRaw.SolutionCount)).ToInt32());
    }

    private static double VectorMagnitude(GpPoint value) =>
        Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
}
