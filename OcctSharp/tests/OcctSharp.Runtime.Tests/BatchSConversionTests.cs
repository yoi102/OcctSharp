namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchSConversionTests
{
    private static Shape Spline() => FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline(
        [new(0, 0, 0), new(1, 2, 0), new(2, 1, 0), new(3, 0, 0), new(4, 1, 0)], [2, 4, 7], [3, 2, 3], 2));
    private static Shape Surface() => FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.BSpline(3, 3,
        Enumerable.Range(0, 3).SelectMany(u => Enumerable.Range(0, 3).Select(v => new GpPoint(u, v, u * v * 0.1))),
        [2, 3, 6], [2, 1, 2], [4, 5, 9], [2, 1, 2], 1, 1));
    private static void Near(GpPoint a, GpPoint b, double tolerance = 1e-7)
    { Assert.InRange(Math.Abs(a.X - b.X), 0, tolerance); Assert.InRange(Math.Abs(a.Y - b.Y), 0, tolerance); Assert.InRange(Math.Abs(a.Z - b.Z), 0, tolerance); }

    [Theory]
    [InlineData(BoundaryPatchStyle.Stretch, false)]
    [InlineData(BoundaryPatchStyle.Coons, false)]
    [InlineData(BoundaryPatchStyle.Curved, false)]
    [InlineData(BoundaryPatchStyle.Stretch, true)]
    [InlineData(BoundaryPatchStyle.Coons, true)]
    [InlineData(BoundaryPatchStyle.Curved, true)]
    public void BoundaryStylesReturnCopiedIndependentPatches(BoundaryPatchStyle style, bool bezier)
    {
        using Shape wire = BatchSAuthoringTests.Square(); Shape[] edges = wire.GetSubShapes(ShapeKind.Edge);
        try
        {
            var definition = GuidedPatchConversion.CreateBoundaryPatch(edges, style, bezier);
            Assert.Equal(bezier ? FreeformGeometryKind.Bezier : FreeformGeometryKind.BSpline, definition.Kind);
            foreach (Shape edge in edges) edge.Dispose(); wire.Dispose();
            using Shape face = FreeformAuthoring.CreateSurfaceFace(definition);
            Assert.True(face.IsValid); Assert.Equal(4, face.InspectProperties(InspectionPropertyKind.Area).Mass, 5);
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Fact]
    public void CurveAssemblyAndCopiedBezierSpansKeepParameterProvenance()
    {
        using Shape first = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline([new(0, 0, 0), new(1, 0, 0)], [2, 5], [2, 2], 1));
        using Shape second = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline([new(1, 0, 0), new(2, 0, 0)], [10, 11], [2, 2], 1));
        var assembled = GuidedPatchConversion.AssembleCurves([first, second], useTangentSpeedRatio: false);
        Assert.Equal(2, assembled.Spans.Count); Assert.Equal(new ParameterRange(2, 5), assembled.Spans[0].SourceParameters);
        Assert.Equal(new ParameterRange(10, 11), assembled.Spans[1].SourceParameters);
        using Shape joined = FreeformAuthoring.CreateCurve(assembled.Definition);
        foreach (var span in assembled.Spans)
        {
            Shape source = span.SourceIndex == 0 ? first : second;
            for (int i = 0; i <= 4; i++)
                Near(source.EvaluateEdge(span.SourceParameters.First + (span.SourceParameters.Last - span.SourceParameters.First) * i / 4).Point,
                    joined.EvaluateEdge(span.ResultParameters.First + (span.ResultParameters.Last - span.ResultParameters.First) * i / 4).Point);
        }
        using Shape spline = Spline(); var pieces = GuidedPatchConversion.DecomposeCurve(spline);
        Assert.Equal(2, pieces.Count); Assert.Equal(new ParameterRange(2, 4), pieces[0].SourceParameters); Assert.Equal(new ParameterRange(4, 7), pieces[1].SourceParameters);
        foreach (var piece in pieces)
        {
            Assert.Equal(FreeformGeometryKind.Bezier, piece.Definition.Kind);
            using Shape copy = FreeformAuthoring.CreateCurve(piece.Definition);
            for (int i = 0; i <= 4; i++) Near(copy.EvaluateEdge(i / 4.0).Point,
                spline.EvaluateEdge(piece.SourceParameters.First + (piece.SourceParameters.Last - piece.SourceParameters.First) * i / 4).Point);
        }
        var extracted = GuidedPatchConversion.ExtractCurveSpan(spline, new(3, 6));
        using Shape range = FreeformAuthoring.CreateCurve(extracted);
        Assert.Equal(new ParameterRange(3, 6), extracted.ParameterRange);
        Near(spline.EvaluateEdge(4.5).Point, range.EvaluateEdge(4.5).Point);
        Assert.Throws<ArgumentException>(() => GuidedPatchConversion.ExtractCurveSpan(spline, new(1, 6)));
        using Shape disconnected = ShapeFactory.CreateEdge(new(5, 0, 0), new(6, 0, 0));
        Assert.Throws<ArgumentException>(() => GuidedPatchConversion.AssembleCurves([first, disconnected]));
    }

    [Fact]
    public void SurfaceGridAndExtractedPatchPreserveUvOrientationAndCopies()
    {
        using Shape source = Surface(); using Shape reversed = source.Reversed();
        var patches = GuidedPatchConversion.DecomposeSurface(reversed);
        Assert.Equal(4, patches.Count); Assert.All(patches, p => Assert.True(p.Reversed));
        Assert.Equal(4, patches.Select(p => (p.UIndex, p.VIndex)).Distinct().Count());
        foreach (var patch in patches)
        {
            using Shape copy = FreeformAuthoring.CreateSurfaceFace(patch.Definition);
            Assert.Equal(FreeformGeometryKind.Bezier, patch.Definition.Kind);
            var uv = patch.SourceParameters;
            for (int i = 0; i <= 4; i++) for (int j = 0; j <= 4; j++)
                Near(SurfaceModeling.Evaluate(copy, new(i / 4.0, j / 4.0)).Point,
                    SurfaceModeling.Evaluate(source, new(uv.FirstU + (uv.LastU - uv.FirstU) * i / 4, uv.FirstV + (uv.LastV - uv.FirstV) * j / 4)).Point);
        }
        var definition = GuidedPatchConversion.ExtractSurfacePatch(source, new(2.5, 5, 4.5, 8));
        using Shape extracted = FreeformAuthoring.CreateSurfaceFace(definition);
        Assert.Equal(new SurfaceParameterBounds(2.5, 5, 4.5, 8), definition.Bounds);
        Near(SurfaceModeling.Evaluate(source, new(3.5, 6)).Point, SurfaceModeling.Evaluate(extracted, new(3.5, 6)).Point);
        source.Dispose(); reversed.Dispose();
        using Shape later = FreeformAuthoring.CreateSurfaceFace(patches[0].Definition); Assert.True(later.IsValid);
    }

    [Fact]
    public void JoinsMeasurePositionAngleCurvatureAndDoNotInventSingularDerivatives()
    {
        using Shape first = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier([new(0, 0, 0), new(1, 0, 0)]));
        using Shape next = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier([new(1, 0, 0), new(2, 0, 0)]));
        var smooth = GuidedPatchConversion.CompareCurveJoin(first, 1, next, 0);
        Assert.Equal(new JoinResidual(0, 0, 0), smooth);
        var reverse = GuidedPatchConversion.CompareCurveJoin(first, 1, next, 0, true);
        Assert.Equal(Math.PI, reverse.AngleRadians!.Value, 9);
        using Shape singular = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.Bezier([new(1, 0, 0), new(1, 0, 0), new(2, 1, 0)]));
        var undefined = GuidedPatchConversion.CompareCurveJoin(first, 1, singular, 0);
        Assert.Equal(0, undefined.Position); Assert.Null(undefined.AngleRadians); Assert.Null(undefined.Curvature);
        using Shape wire = BatchSAuthoringTests.Square(); using Shape face = ShapeFactory.CreatePlanarFace(wire);
        Shape[] edges = face.GetSubShapes(ShapeKind.Edge);
        try
        {
            Assert.All(GuidedPatchConversion.CompareSurfaceBoundary(edges[0], face, face, 9), r =>
            { Assert.InRange(r.Position!.Value, 0, 1e-8); Assert.Equal(0, r.AngleRadians!.Value, 8); Assert.Equal(0, r.Curvature!.Value, 8); });
            using Shape shifted = face.Transformed(ShapeTransform.CreateTranslation(0, 0, 0.25));
            Assert.All(GuidedPatchConversion.CompareSurfaceBoundary(edges[0], face, shifted, 9), r => Assert.Equal(0.25, r.Position!.Value, 8));
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void TwoAndThreeBoundaryPatchesCoverEligibleNonQuadrilateralInputs(int count, bool bezier)
    {
        using Shape a = ShapeFactory.CreateEdge(new(0, 0, 0), new(2, 0, 0));
        using Shape b = count == 2 ? ShapeFactory.CreateEdge(new(0, 2, 0), new(2, 2, 0))
            : ShapeFactory.CreateEdge(new(2, 0, 0), new(0, 2, 0));
        using Shape c = ShapeFactory.CreateEdge(new(0, 2, 0), new(0, 0, 0));
        var definition = GuidedPatchConversion.CreateBoundaryPatch(count == 2 ? [a, b] : [a, b, c], BoundaryPatchStyle.Stretch, bezier);
        using Shape face = FreeformAuthoring.CreateSurfaceFace(definition); Assert.True(face.IsValid);
        Assert.Equal(count == 2 ? 4 : 2, face.InspectProperties(InspectionPropertyKind.Area).Mass, 5);
    }
}
#pragma warning restore CA1861
