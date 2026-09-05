using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchSClosureTests
{
    [Fact]
    public void GuidedDeliveryRoundtripsRecipesExchangeAndRealViewer() => OcctSharp.Validation.BatchSGuidedWorkflow.Run();

    [Fact]
    public void TrimmedCompositesAndMultipleKnotsPreserveDerivativeBoundaries()
    {
        var first = ScalarLawDefinition.Linear(new(0, 4), 0, 4).Trim(new(1, 2));
        var second = ScalarLawDefinition.Linear(new(1, 5), 1, 5).Trim(new(2, 3));
        var composite = ScalarLawDefinition.Composite([first, second]);
        Assert.Equal(new LawDomain(1, 3), composite.Domain);
        Assert.Equal(new LawDomain(1, 2), composite.Spans[0].ActiveDomain);
        Assert.Equal(0, composite.InspectJoins()[0].ValueJump);
        Assert.Equal(2.5, composite.Evaluate(2.5).Value, 9);
        Assert.Equal(1, composite.Evaluate(2).FirstDerivative!.Value, 9);
        var mapped = composite.MapDomain(new(10, 20));
        Assert.Equal(2, mapped.Evaluate(15).Value, 9);
        Assert.Equal(0.2, mapped.Evaluate(15).FirstDerivative!.Value, 9);
        var corner = ScalarLawDefinition.BSpline([1, 2, 2, 1, 3], [0, 0.5, 1], [3, 2, 3], 2);
        Assert.Null(corner.Evaluate(0.5).FirstDerivative);
        Assert.Null(corner.Evaluate(0.5).SecondDerivative);
        Assert.NotNull(corner.Evaluate(0.25).SecondDerivative);
        var overshoot = ScalarLawDefinition.Interpolate([0, 1], [1, 1], -12, 12);
        Assert.False(overshoot.Sample(17).SamplesArePositive);
        using Shape spine = BatchSAuthoringTests.Spine(); using Shape section = BatchSAuthoringTests.Square();
        using var invalid = GuidedSweepPlan.Create(spine, [new(section)], scaleLaw: overshoot);
        Assert.Throws<ArgumentException>(() => invalid.Build());
    }

    [Theory]
    [InlineData(GuidedSweepContact.None)]
    [InlineData(GuidedSweepContact.KeepContact)]
    [InlineData(GuidedSweepContact.ContactOnBorder)]
    public void AuxiliaryContactsBuildAndPreserveGuideIsolation(GuidedSweepContact contact)
    {
        using Shape spine = BatchSAuthoringTests.Spine();
        using Shape sectionEdge = ShapeFactory.CreateCircleEdge(new(0, 0, 0), new(0, 0, 1), 2);
        using Shape profile = contact == GuidedSweepContact.ContactOnBorder
            ? ShapeFactory.CreatePolygonWire([new(0, -1, 0), new(2, -1, 0), new(2, 1, 0), new(0, 1, 0)], true)
            : ShapeFactory.CreateWire([sectionEdge]);
        using Shape guide = ShapeFactory.CreatePolygonWire([new(2, 0, 0), new(2, 0, 10)]);
        using var plan = GuidedSweepPlan.Create(spine, [new(profile)], new() { Frame = GuidedSweepFrame.AuxiliarySpine, Contact = contact }, guide);
        guide.Dispose(); spine.Dispose(); profile.Dispose();
        Assert.True(plan.Preflight().Ready);
        using var result = plan.Build();
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
        Assert.Equal(contact == GuidedSweepContact.None ? null : 0, result.Diagnostics.ContinuityLimit);
        Assert.InRange(result.RequireShape().GetBoundingBox().SizeZ, 9.999, 10.001);
        if (contact == GuidedSweepContact.ContactOnBorder)
        {
            using Shape inputSpine = BatchSAuthoringTests.Spine();
            using Shape inputGuide = ShapeFactory.CreatePolygonWire([new(2, 0, 0), new(2, 0, 10)]);
            using Shape line = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0)]);
            using var invalid = GuidedSweepPlan.Create(inputSpine, [new(line)], plan.Options, inputGuide);
            Assert.Throws<ArgumentException>(() => invalid.Preflight());
        }
    }

    [Fact]
    public void SupportSurfaceRetainsRealPcurveDependencyAfterSourceRelease()
    {
        using Shape perimeter = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 10), new(0, 5, 10), new(0, 5, 0)], true);
        using Shape support = ShapeFactory.CreatePlanarFace(perimeter);
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            Shape selected = edges.Single(e => e.GetBoundingBox().SizeZ > 9 && e.GetBoundingBox().Maximum.Y < 0.001);
            using Shape spine = ShapeFactory.CreateWire([selected]); using Shape section = BatchSAuthoringTests.Square();
            using var plan = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = GuidedSweepFrame.SupportSurface }, support);
            support.Dispose(); foreach (Shape edge in edges) edge.Dispose(); spine.Dispose(); section.Dispose();
            using var result = plan.Build();
            Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
            Assert.True(result.RequireShape().IsValid);
            Assert.InRange(result.RequireShape().GetBoundingBox().SizeZ, 9.999, 10.001);
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
        using Shape unsupported = BatchSAuthoringTests.Spine(); using Shape profile = BatchSAuthoringTests.Square();
        using Shape sphere = ShapeFactory.CreateSphere(5);
        using var rejected = GuidedSweepPlan.Create(unsupported, [new(profile)], new() { Frame = GuidedSweepFrame.SupportSurface }, sphere);
        Assert.Throws<ArgumentException>(() => rejected.Build());
    }

    [Fact]
    public void AttachedSectionsUseExactSpineVerticesAndRejectReverseOrdering()
    {
        using Shape spine = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 5), new(0, 0, 10)]);
        Shape[] vertices = spine.GetSubShapes(ShapeKind.Vertex);
        try
        {
            // GetSubShapes enumerates occurrences: the middle vertex appears twice.
            var ordered = vertices.DistinctBy(v => Math.Round(v.GetBoundingBox().Minimum.Z, 4)).OrderBy(v => v.GetBoundingBox().Minimum.Z).ToArray();
            using Shape a = BatchSAuthoringTests.Square(); using Shape b = BatchSAuthoringTests.Square(5, 3); using Shape c = BatchSAuthoringTests.Square(10);
            using var plan = GuidedSweepPlan.Create(spine, [new(a, ordered[0]), new(b, ordered[1], WithCorrection: true), new(c, ordered[2])],
                new() { SolidPolicy = SweepSolidPolicy.RequireSolid });
            using var result = plan.Build();
            Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.Diagnostics.IsSolid);
            Assert.Contains(result.History, h => h.Source is { ArgumentIndex: > 0, Kind: ShapeKind.Edge } && h.Kind == AuthoringHistoryKind.Generated);
            using var reverse = GuidedSweepPlan.Create(spine, [new(c, ordered[2]), new(a, ordered[0])]);
            Assert.Throws<ArgumentException>(() => reverse.Preflight());
        }
        finally { foreach (Shape vertex in vertices) vertex.Dispose(); }
    }

    [Fact]
    public void FixedFrameAndScaleSimulationHaveMeasuredDimensions()
    {
        using Shape spine = BatchSAuthoringTests.Spine(); using Shape section = BatchSAuthoringTests.Square();
        using var fixedPlan = GuidedSweepPlan.Create(spine, [new(section)], new() { Frame = GuidedSweepFrame.FixedFrame });
        using var simulation = fixedPlan.Simulate(5);
        for (int i = 0; i < 5; i++)
        {
            var bounds = simulation.SimulatedSections[i].GetBoundingBox();
            Assert.InRange(bounds.SizeX, 1.999, 2.001); Assert.InRange(bounds.SizeY, 1.999, 2.001);
            Assert.InRange(bounds.SizeZ, 0, 0.001); Assert.InRange(bounds.Minimum.Z, i * 2.5 - 0.001, i * 2.5 + 0.001);
        }
        using var scaled = GuidedSweepPlan.Create(spine, [new(section)], scaleLaw: ScalarLawDefinition.Linear(new(0, 1), 1, 2));
        using var result = scaled.Simulate(3);
        Assert.InRange(result.SimulatedSections[0].GetBoundingBox().SizeX, 1.999, 2.001);
        Assert.InRange(result.SimulatedSections[^1].GetBoundingBox().SizeX, 3.999, 4.001);
    }

    [Fact]
    public void SolidificationFailureOnlyKeepsShellUnderExplicitPolicy()
    {
        using Shape spine = BatchSAuthoringTests.Spine(); using Shape open = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0)]);
        using var required = GuidedSweepPlan.Create(spine, [new(open)], new() { SolidPolicy = SweepSolidPolicy.RequireSolid });
        using var failed = required.Build(); Assert.False(failed.Diagnostics.AlgorithmDone); Assert.Null(failed.Shape);
        using var optional = GuidedSweepPlan.Create(spine, [new(open)], new() { SolidPolicy = SweepSolidPolicy.AllowValidShellIfSolidificationFails });
        using var retained = optional.Build(); Assert.True(retained.Diagnostics.AlgorithmDone); Assert.False(retained.Diagnostics.IsSolid);
        Assert.True(retained.RequireShape().IsValid);
    }

    [Fact]
    public void LoftActuallyCorrectsUnequalEdgeCountsAndReturnsExactEdgeProvenance()
    {
        using Shape a = BatchSAuthoringTests.Square();
        using Shape b = ShapeFactory.CreatePolygonWire([new(0, 0, 5), new(0, 2, 5), new(2, 2, 5), new(2, 0, 5), new(1, 0, 5)], true);
        Assert.Equal(4, a.CountSubShapes(ShapeKind.Edge)); Assert.Equal(5, b.CountSubShapes(ShapeKind.Edge));
        string before = RepairSnapshot.ComputeFingerprint(b);
        using var result = GuidedLoft.Build([a, b], new() { CorrectCompatibility = true, Solid = true });
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message); Assert.True(result.RequireShape().IsValid);
        var corrected = result.History.Where(h => h.Kind == AuthoringHistoryKind.CompatibleSection).ToArray();
        Assert.Equal(2, corrected.Length);
        Assert.Equal(corrected[0].Shape!.CountSubShapes(ShapeKind.Edge), corrected[1].Shape!.CountSubShapes(ShapeKind.Edge));
        Assert.True(corrected[0].Shape!.CountSubShapes(ShapeKind.Edge) > 4);
        Assert.Contains(result.History, h => h.Source is { ArgumentIndex: 0, Kind: ShapeKind.Edge } && h.Kind == AuthoringHistoryKind.Generated);
        Assert.Equal(before, RepairSnapshot.ComputeFingerprint(b));
    }

    [Fact]
    public void MixedBoundaryInteriorAndSeedConstraintsAreIndividuallyVerified()
    {
        using Shape wire = BatchSAuthoringTests.Square(); using Shape support = ShapeFactory.CreatePlanarFace(wire);
        using Shape interior = ShapeFactory.CreateEdge(new(0.5, 1, 0), new(1.5, 1, 0));
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            List<SurfaceConstraint> constraints = edges.Select((e, i) => (SurfaceConstraint)new SurfaceEdgeConstraint($"e{i}", e,
                (SurfaceConstraintContinuity)(i % 3), SupportFace: support)).ToList();
            constraints.Insert(0, new SurfaceEdgeConstraint("interior", interior, Boundary: false));
            constraints.Add(new SurfacePointConstraint("free", new(0.5, 0.5, 0)));
            using var plan = ConstrainedFillPlan.Create(constraints, new() { Anisotropic = true, VerificationSamples = 17 }, support);
            using var result = plan.Build();
            Assert.True(result.Accepted, result.Result.Diagnostics.Message);
            Assert.Equal(6, result.Constraints.Count); Assert.All(result.Constraints, c => Assert.True(c.Accepted, c.Id));
            Assert.Equal(5, result.Constraints.Single(c => c.Id == "interior").KernelIndex);
            Assert.Contains(result.Result.History, h => h.Kind == AuthoringHistoryKind.Generated && h.Source.HasValue);
            Assert.Throws<ArgumentException>(() => ConstrainedFillPlan.Create(constraints, new() { Iterations = 9 }));
            Assert.Throws<ArgumentException>(() => ConstrainedFillPlan.Create(constraints.Append(constraints[0])));
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Fact]
    public void ConflictingRequiredConstraintsNeverBecomeAccepted()
    {
        using Shape wire = BatchSAuthoringTests.Square(); using Shape support = ShapeFactory.CreatePlanarFace(wire);
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            List<SurfaceConstraint> constraints = edges.Select((e, i) => (SurfaceConstraint)new SurfaceEdgeConstraint($"edge-{i}", e)).ToList();
            constraints.Add(new SurfacePointConstraint("conflict", new(0, 0, 10)));
            using var plan = ConstrainedFillPlan.Create(constraints, new() { MaximumSegments = 1, MaximumDegree = 2, Iterations = 1 });
            using var result = plan.Build();
            Assert.False(result.Accepted); Assert.Throws<InvalidOperationException>(() => result.RequireFace());
            Assert.Contains(result.Constraints, c => c.Required && !c.Accepted);
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Fact]
    public unsafe void LawAbiRejectsCapacityAndBadParametersWithoutPartialWrites()
    {
        Assert.Equal(72, Marshal.SizeOf<FillConstraintRaw>()); Assert.Equal(64, Marshal.SizeOf<FillOptionsRaw>());
        Assert.Equal(48, Marshal.SizeOf<ConstraintResidualRaw>()); Assert.Equal(80, Marshal.SizeOf<PatchOptionsRaw>());
        Assert.Equal(64, Marshal.SizeOf<PatchSpanRaw>());
        LawSpanRaw span = new() { First = 0, Last = 1, ActiveFirst = 0, ActiveLast = 1, ValueFirst = 2 };
        LawInputRaw law = new() { Spans = &span, SpanCount = 1, First = 0, Last = 1 };
        double* parameters = stackalloc double[] { 0, 1 };
        LawSampleRaw* output = stackalloc LawSampleRaw[2]; output[0].Value = 999; output[1].Value = 888;
        Assert.Equal(NativeStatus.InvalidArgument, ScalarLawInterop.Evaluate(in law, parameters, 2, output, 1, out _));
        Assert.Equal(999, output[0].Value); Assert.Equal(888, output[1].Value);
        parameters[1] = 2;
        Assert.Equal(NativeStatus.InvalidArgument, ScalarLawInterop.Evaluate(in law, parameters, 2, output, 2, out _));
        Assert.Equal(999, output[0].Value); Assert.Equal(888, output[1].Value);
    }

    [Fact]
    public void AuthoringOwnersRejectReleasedInputsAndSurviveRepeatedDisposal()
    {
        for (int loop = 0; loop < 16; loop++)
        {
            using Shape spine = BatchSAuthoringTests.Spine(); using Shape section = BatchSAuthoringTests.Square();
            string spineBefore = RepairSnapshot.ComputeFingerprint(spine), sectionBefore = RepairSnapshot.ComputeFingerprint(section);
            using var plan = GuidedSweepPlan.Create(spine, [new(section)]);
            Assert.Equal(spineBefore, RepairSnapshot.ComputeFingerprint(spine)); Assert.Equal(sectionBefore, RepairSnapshot.ComputeFingerprint(section));
            spine.Dispose(); section.Dispose();
            using var first = plan.Build(); using var second = plan.Build();
            first.Dispose(); plan.Dispose(); plan.Dispose();
            Assert.True(second.RequireShape().IsValid);
            Assert.All(second.History.Where(h => h.Shape is not null), h => Assert.False(h.Shape!.IsNull));
            second.Dispose(); Assert.Throws<ObjectDisposedException>(() => second.RequireShape());
            Assert.Throws<ObjectDisposedException>(() => plan.CopyInput(0));
            Assert.Throws<ObjectDisposedException>(() => GuidedSweepPlan.Create(spine, [new(section)]));
        }
    }

    [Fact]
    public unsafe void AuthoringRawHandlesFailClosedAndHistoryOwnsItsCopies()
    {
        using Shape spine = BatchSAuthoringTests.Spine(); nint input = spine.Handle.DangerousGetHandle();
        Assert.Equal(NativeStatus.Success, NativeMethods.AuthoringCopyInputs(&input, 1, out nint result));
        using FeatureResultHandle owner = new(result);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.AuthoringHistory(owner, -1, out _, out nint absent)); Assert.Equal(0, absent);
        Assert.Equal(NativeStatus.Success, NativeMethods.AuthoringHistory(owner, 0, out var info, out nint copied));
        using Shape copy = ShapeFactory.FromNativeHandle(copied, "test-owning-history");
        Assert.Equal((int)AuthoringHistoryKind.InputSnapshot, info.Kind); owner.Dispose(); spine.Dispose();
        Assert.Equal(1, copy.CountSubShapes(ShapeKind.Edge));
        Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.AuthoringCopyInputs(&input, 1, out nint stale)); Assert.Equal(0, stale);
        using Shape face = ShapeFactory.CreateBox(1, 1, 1);
        using var invalid = GuidedSweepPlan.Create(face, [new(copy)]);
        Assert.Throws<ArgumentException>(() => invalid.Preflight());
    }

    [Fact]
    public void AcceptedFillPublishesCopiedConstraintRecipe()
    {
        using Shape wire = BatchSAuthoringTests.Square(); using Shape support = ShapeFactory.CreatePlanarFace(wire);
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            using var plan = ConstrainedFillPlan.Create(edges.Select((edge, i) => new SurfaceEdgeConstraint($"e{i}", edge,
                SurfaceConstraintContinuity.G2, SupportFace: support)));
            var recipe = GuidedAuthoringDelivery.Capture(plan);
            using var result = plan.Build(); Assert.True(result.Accepted);
            plan.Dispose(); support.Dispose(); foreach (Shape edge in edges) edge.Dispose();
            using XdeDocument document = XdeDocument.Create();
            var published = GuidedAuthoringDelivery.Publish(document, result, recipe, "constrained G2 face");
            Assert.Contains("G2", published.Result.Name, StringComparison.Ordinal);
            Assert.Contains("constrained-fill", published.Recipe.AsciiString, StringComparison.Ordinal);
            using Shape owned = published.Result.Shape; result.Dispose(); Assert.True(owned.IsValid);
            Assert.Throws<ObjectDisposedException>(() => result.RequireFace());
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(7, 8)]
    public void RepeatedLowSamplingG2FillsHaveStableResidualsAndOwningHistory(int points, int iterations)
    {
        // OCCT 8.0.1's per-index G*Error getters allocate the initial curve sample
        // count but use refined samples. Final-surface verification must not call them.
        using Shape wire = BatchSAuthoringTests.Square();
        using Shape support = ShapeFactory.CreatePlanarFace(wire);
        Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
        try
        {
            var domain = SurfaceModeling.Describe(support).Bounds;
            List<SurfaceConstraint> constraints = edges.Select((edge, i) => (SurfaceConstraint)new SurfaceEdgeConstraint(
                $"edge-{i}", edge, SurfaceConstraintContinuity.G2, SupportFace: support)).ToList();
            constraints.Add(new SurfaceUvConstraint("center", support, (domain.FirstU + domain.LastU) / 2,
                (domain.FirstV + domain.LastV) / 2, SurfaceConstraintContinuity.G2));
            using var plan = ConstrainedFillPlan.Create(constraints,
                new() { PointsPerCurve = points, Iterations = iterations, VerificationSamples = 65 }, support);
            support.Dispose(); foreach (Shape edge in edges) edge.Dispose();
            for (int repeat = 0; repeat < 24; repeat++)
            {
                using var result = plan.Build();
                Assert.True(result.Accepted, $"Iteration {repeat}: {result.Result.Diagnostics.Message}");
                Assert.All(result.Constraints, constraint =>
                {
                    Assert.True(constraint.Accepted, constraint.Id);
                    Assert.InRange(constraint.PositionResidual!.Value, 0, plan.Options.Tolerance3d);
                    Assert.InRange(constraint.AngularResidual!.Value, 0, plan.Options.ToleranceAngular);
                    Assert.InRange(constraint.CurvatureResidual!.Value, 0, plan.Options.ToleranceCurvature);
                    Assert.Equal(constraint.Id == "center" ? 1 : 65, constraint.SampleCount);
                });
                Assert.All(result.Result.History.Where(h => h.Shape is not null), h => Assert.False(h.Shape!.IsNull));
                Assert.True(result.RequireFace().IsValid);
                result.Dispose(); result.Dispose();
            }
        }
        finally { foreach (Shape edge in edges) edge.Dispose(); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LoftPunctualEndpointsRemainIndependentAndRejectInteriorVertices(bool correctCompatibility)
    {
        using Shape axis = ShapeFactory.CreateEdge(new(1, 1, -2), new(1, 1, 2));
        Shape[] endpoints = axis.GetSubShapes(ShapeKind.Vertex);
        using Shape start = endpoints[0];
        using Shape middle = BatchSAuthoringTests.Square();
        using Shape end = endpoints[1];
        using var result = GuidedLoft.Build([start, middle, end], new() { Solid = true, Ruled = true, CorrectCompatibility = correctCompatibility });
        Assert.True(result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
        Assert.True(result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
        Assert.NotNull(result.FirstSection); Assert.NotNull(result.LastSection);
        Assert.InRange(result.RequireShape().GetBoundingBox().SizeZ, 3.999, 4.001);
        Assert.Throws<ArgumentException>(() => GuidedLoft.Build([middle, start, middle]));
        start.Dispose(); middle.Dispose(); end.Dispose();
        Assert.True(result.RequireShape().IsValid);
    }
}
#pragma warning restore CA1861
