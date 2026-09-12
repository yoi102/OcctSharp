using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchACModelingTests
{
    [Fact]
    public void ConicIntersectionCopiesRemainIndependentOfAlgorithmAndDomains()
    {
        using IntRes2dDomain horizontalDomain = IntRes2dDomain.Create();
        using IntRes2dDomain verticalDomain = IntRes2dDomain.Create();
        V.Line2d horizontal = Line2dOperations.Create(
            new V.Axis2d(new V.Point2d(0, 0), new V.Direction2d(1, 0)));
        V.Line2d vertical = Line2dOperations.Create(
            new V.Axis2d(new V.Point2d(2, -1), new V.Direction2d(0, 1)));
        using IntCurveIntConicConic intersection = IntCurveIntConicConic.Create(
            horizontal, horizontalDomain, vertical, verticalDomain, 1e-9, 1e-9);
        Assert.True(intersection.IsDone());
        Assert.False(intersection.IsEmpty());
        Assert.Equal(1, intersection.NbPoints());
        Assert.Equal(0, intersection.NbSegments());
        using IntRes2dIntersectionPoint point = intersection.Point(1);
        using IntRes2dTransition transition = point.TransitionOfFirst();
        intersection.Dispose();
        horizontalDomain.Dispose();
        verticalDomain.Dispose();
        Assert.Equal(new V.Point2d(2, 0), point.Value());
        Assert.Equal(2, point.ParamOnFirst(), 10);
        Assert.Equal(1, point.ParamOnSecond(), 10);
        point.Dispose();
        Assert.Equal(IntRes2dPosition.IntRes2d_Middle, transition.PositionOnCurve());
        Assert.False(transition.IsTangent());
    }

    [Fact]
    public void SegmentOwnsCopiedEndpointsAndRejectsAbsentEndpointQueries()
    {
        using IntRes2dTransition firstTransition = IntRes2dTransition.Create(
            false, IntRes2dPosition.IntRes2d_Middle, IntRes2dTypeTrans.IntRes2d_In);
        using IntRes2dTransition secondTransition = IntRes2dTransition.Create(
            false, IntRes2dPosition.IntRes2d_Middle, IntRes2dTypeTrans.IntRes2d_Out);
        using IntRes2dIntersectionPoint first = IntRes2dIntersectionPoint.Create(
            new V.Point2d(1, 2), 3, 4, firstTransition, secondTransition, false);
        using IntRes2dIntersectionPoint last = IntRes2dIntersectionPoint.Create(
            new V.Point2d(5, 6), 7, 8, firstTransition, secondTransition, false);
        using IntRes2dIntersectionSegment segment = IntRes2dIntersectionSegment.Create(first, last, false, false);
        using IntRes2dIntersectionPoint result = segment.LastPoint();
        segment.Dispose();
        first.Dispose();
        last.Dispose();
        firstTransition.Dispose();
        secondTransition.Dispose();
        Assert.Equal(new V.Point2d(5, 6), result.Value());
        Assert.Equal(7, result.ParamOnFirst());

        using IntRes2dIntersectionSegment infinite = IntRes2dIntersectionSegment.Create(false);
        Assert.False(infinite.HasFirstPoint());
        Assert.False(infinite.HasLastPoint());
        Assert.Throws<OcctException>(() => infinite.FirstPoint());
        Assert.Throws<OcctException>(() => infinite.LastPoint());
    }

    [Fact]
    public void UncomputedIntersectionAndInvalidOwnerArgumentsFailBeforeUse()
    {
        using IntCurveIntConicConic intersection = IntCurveIntConicConic.Create();
        Assert.False(intersection.IsDone());
        Assert.Throws<OcctException>(() => intersection.NbPoints());
        Assert.Throws<OcctException>(() => intersection.Point(1));
        using IntRes2dDomain domain = IntRes2dDomain.Create();
        Assert.Throws<OcctException>(() => domain.FirstPoint());
        V.Line2d line = Line2dOperations.Create(
            new V.Axis2d(new V.Point2d(0, 0), new V.Direction2d(1, 0)));
        domain.Dispose();
        Assert.Throws<ObjectDisposedException>(() => intersection.Perform(line, domain, line, domain, 1e-9, 1e-9));
        Assert.Throws<ArgumentNullException>(() => intersection.Perform(line, null!, line, null!, 1e-9, 1e-9));
        intersection.Dispose();
        intersection.Dispose();
        Assert.Throws<ObjectDisposedException>(() => intersection.IsDone());
    }

    [Fact]
    public void IntervalSubtractionPublishesIndependentResultIntervals()
    {
        using IntrvInterval entire = IntrvInterval.Create(0, 10);
        using IntrvInterval middle = IntrvInterval.Create(3, 7);
        using IntrvIntervals intervals = IntrvIntervals.Create(entire);
        intervals.Subtract(middle);
        Assert.Equal(2, intervals.NbIntervals());
        using IntrvInterval left = intervals.Value(1);
        using IntrvInterval right = intervals.Value(2);
        intervals.Dispose();
        entire.Dispose();
        middle.Dispose();
        Assert.Equal((0d, 3d), (left.Start(), left.End()));
        Assert.Equal((7d, 10d), (right.Start(), right.End()));
        left.SetStart(-2, 0);
        Assert.Equal(7, right.Start());
    }

    [Fact]
    public void SubdivisionSamplesAndMarkedRangesKeepCopiedBounds()
    {
        using IntToolsCurveRangeSample sample = IntToolsCurveRangeSample.Create(1);
        sample.SetDepth(1);
        Assert.Equal(1, sample.GetDepth());
        Assert.Equal(4, sample.GetRangeIndexDeeper(4));
        using IntToolsRange sampled = sample.GetRange(0, 8, 4);
        sample.Dispose();
        Assert.Equal((2d, 4d), (sampled.First(), sampled.Last()));
        using IntToolsMarkedRangeSet ranges = IntToolsMarkedRangeSet.Create(0, 10, 4);
        Assert.True(ranges.InsertRange(2, 5, 9));
        Assert.Equal(3, ranges.Length());
        Assert.Equal(9, ranges.Flag(2));
        using IntToolsRange marked = ranges.Range(2);
        ranges.Dispose();
        double first = -1, last = -1;
        marked.Range(ref first, ref last);
        Assert.Equal((2d, 5d), (first, last));
    }

    [Fact]
    public void ProjectorPublishesCopiedCoordinatesAndPreservesRefsOnInvalidInput()
    {
        using HLRAlgoProjector projector = HLRAlgoProjector.Create(Transform3dOperations.Create(), false, 0);
        Assert.False(projector.Perspective());
        V.Point2d projected = new(9, 8);
        projector.Project(new Point3d(2, 3, 4), ref projected);
        Assert.Equal(new V.Point2d(2, 3), projected);
        V.Vector2d derivative = new(8, 9);
        projector.Project(new Point3d(2, 3, 4), new V.Vector3d(1, 2, 0), ref projected, ref derivative);
        Assert.Equal(new V.Vector2d(1, 2), derivative);
        projected = new V.Point2d(9, 8);
        Assert.Throws<OcctException>(() => projector.Project(new Point3d(double.NaN, 0, 0), ref projected));
        Assert.Equal(new V.Point2d(9, 8), projected);
        projector.Dispose();
        Assert.Throws<ObjectDisposedException>(() => projector.Project(new Point3d(0, 0, 0), ref projected));
        Assert.Equal(new V.Point2d(9, 8), projected);
    }

    [Fact]
    public void HiddenEdgeIntervalsExcludeTheOccludedMiddle()
    {
        using HLRAlgoEdgeStatus edge = HLRAlgoEdgeStatus.Create(0, 0, 10, 0);
        Assert.True(edge.AllVisible());
        edge.Hide(3, 0, 7, 0, false, false);
        Assert.Equal(2, edge.NbVisiblePart());
        double first = -1, last = -1;
        float firstTolerance = 0, lastTolerance = 0;
        edge.VisiblePart(1, ref first, ref firstTolerance, ref last, ref lastTolerance);
        Assert.Equal((0d, 3d), (first, last));
        edge.VisiblePart(2, ref first, ref firstTolerance, ref last, ref lastTolerance);
        Assert.Equal((7d, 10d), (first, last));
        edge.HideAll();
        Assert.True(edge.AllHidden());
        edge.ShowAll();
        Assert.True(edge.AllVisible());
    }

    [Fact]
    public void WireOrderReconstructsAClosedLoopFromShuffledCopiedCoordinates()
    {
        using ShapeAnalysisWireOrder order = ShapeAnalysisWireOrder.Create();
        order.SetMode(true, 1e-7, false);
        order.Add(new V.Coordinates3d(1, 0, 0), new V.Coordinates3d(1, 1, 0));
        order.Add(new V.Coordinates3d(0, 0, 0), new V.Coordinates3d(1, 0, 0));
        order.Add(new V.Coordinates3d(0, 1, 0), new V.Coordinates3d(0, 0, 0));
        order.Add(new V.Coordinates3d(1, 1, 0), new V.Coordinates3d(0, 1, 0));
        order.Perform(true);
        Assert.True(order.IsDone());
        Assert.Equal(4, order.NbEdges());
        var reordered = new List<(V.Coordinates3d Start, V.Coordinates3d End)>();
        var indices = new HashSet<int>();
        for (int i = 1; i <= 4; i++)
        {
            int original = order.Ordered(i);
            Assert.True(indices.Add(Math.Abs(original)));
            V.Coordinates3d start = default, end = default;
            order.XYZ(Math.Abs(original), ref start, ref end);
            reordered.Add(original < 0 ? (end, start) : (start, end));
        }
        for (int i = 0; i < reordered.Count; i++)
            Assert.Equal(reordered[i].End, reordered[(i + 1) % reordered.Count].Start);
        order.Clear();
        Assert.Equal(0, order.NbEdges());
    }
}
