using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchUDiagnosticTests
{
    [Fact]
    public void SharedDefinitionExchangeAndRealHwndReview() => Validation.BatchULocalFeatureWorkflow.Run();

    [Fact]
    public void RealCornerPlateFailureReturnsOnlyAnOwningDiagnosticPartial()
    {
        // A deliberately over-coarse 2D approximation causes the real OCCT plate
        // solver to fail, after its stripes have produced a diagnostic BadShape.
        GpPoint[] polygon = [new(10, 0, 0), new(0, 10, 0), new(-10, 0, 0), new(0, -10, 0)];
        List<Shape> faces = [];
        try
        {
            using var bottom = ShapeFactory.CreatePolygonWire(polygon.Reverse().ToArray(), true); faces.Add(ShapeFactory.CreatePlanarFace(bottom));
            for (int i = 0; i < polygon.Length; i++)
            { using var wire = ShapeFactory.CreatePolygonWire([polygon[i], polygon[(i + 1) % polygon.Length], new(1, 2, 10)], true); faces.Add(ShapeFactory.CreatePlanarFace(wire)); }
            using var shell = ShapeFactory.Sew(faces); Assert.True(shell.IsValid);
            using var source = RepairSnapshot.Create(shell); string fingerprint = source.Fingerprint;
            var edges = source.Topology.Where(t => t.Kind == ShapeKind.Edge).Where(t =>
            { using var edge = source.CopySubshape(t.Selection); return edge.GetBoundingBox().Maximum.Z > 9.99; }).Select(t => t.Selection).ToArray();
            Assert.Equal(4, edges.Length);
            var programs = edges.Select((e, i) => FilletContourProgram.Constant(e, .2 * (1 + .1 * i))).ToArray();
            using var good = ContourFilletRecipe.Create(source, programs, new() { Continuity = FilletContinuity.C2, Approximation2d = .001, Tolerance2d = .001 }).Build(source);
            Assert.True(good.Diagnostics.AlgorithmDone, good.Diagnostics.Message); Assert.True(good.Diagnostics.ShapeIsValid);
            using var result = ContourFilletRecipe.Create(source, programs, new() { Continuity = FilletContinuity.C2, Approximation2d = .1, Tolerance2d = .1 }).Build(source);
            Assert.False(result.Diagnostics.AlgorithmDone); Assert.False(result.Diagnostics.ShapeIsValid);
            Assert.True(result.Diagnostics.HasPartialResult); Assert.NotEmpty(result.Faults); Assert.Null(result.Shape);
            Assert.Throws<InvalidOperationException>(() => result.RequireShape());
            Assert.Throws<InvalidOperationException>(() => LocalFeatureAcceptance.Inspect(source, result));
            var partial = Assert.Single(result.GetGroup(LocalFeatureHistoryKind.Partial));
            Assert.DoesNotContain(result.History.Where(h => h.Kind == LocalFeatureHistoryKind.Partial), h => h.ResultTopologyIndex.HasValue);
            Assert.Equal(fingerprint, RepairSnapshot.ComputeFingerprint(source.Shape));
            source.Dispose(); shell.Dispose();
            Assert.True(partial.GetTopologySummary().UniqueCounts.FaceCount > 0);
            using var properties = GPropProperties.FromShape(partial, GPropMode.Surface); Assert.True(properties.Mass > 0);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }
}
