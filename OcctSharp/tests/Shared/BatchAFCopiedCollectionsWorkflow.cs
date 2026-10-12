using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchAFCopiedCollectionsWorkflow
{
    internal static void Run()
    {
        Coordinates3d[] source = [new(0, 0, 0), new(1, 2, 0), new(3, 0, 0)];
        var poles = new CopiedArray<Coordinates3d>(source);
        source[0] = new(99, 99, 99);
        var knots = new CopiedArray<double>([0, 1]);
        var mults = new CopiedArray<int>([3, 3]);
        using var spline = GeomBSplineCurveCollections.Create(poles, knots, mults, 2, false);
        var saved = GeomBSplineCurveCollections.Poles(spline);
        if (saved[1] != new Coordinates3d(0, 0, 0) || saved.Count != 3 || spline.Degree() != 2)
            throw new InvalidOperationException("Copied curve definition did not round trip.");
        GeomBSplineCurveCollections.InsertKnots(spline, new([0.5]), new([1]), 1e-9, false);
        if (spline.NbKnots() != 3 || GeomBSplineCurveCollections.Knots(spline)[2] != 0.5)
            throw new InvalidOperationException("Bulk knot insertion failed.");
        var flat = GeomBSplineCurveCollections.KnotSequence(spline);
        if (flat.Count != 7) throw new InvalidOperationException("Copied flat knot sequence has an invalid count.");
        spline.Dispose();
        if (saved[3] != new Coordinates3d(3, 0, 0)) throw new InvalidOperationException("Snapshot depends on disposed receiver.");

        var points2d = new CopiedArray<Point2d>([new(0, 0), new(1, 2), new(3, 0)]);
        using var bezier = Geom2dBezierCurveCollections.Create(points2d, new([1, 2, 1]));
        if (Geom2dBezierCurveCollections.WeightsArray(bezier)[2] != 2)
            throw new InvalidOperationException("Rational 2D copied weights failed.");

        using var fitting = GeomAPIPointsToBSplineCollections.Create(poles, 2, 3, GeomAbsShape.GeomAbs_C1, 1e-6);
        if (!fitting.IsDone()) throw new InvalidOperationException("Point fitting did not complete.");
        using var fitted = fitting.Curve();
        fitting.Dispose();
        if (GeomBSplineCurveCollections.Poles(fitted).Count < 2)
            throw new InvalidOperationException("Fitted curve did not survive algorithm disposal.");

        CopiedArray<int> reversed = new([3, 1, 2], -2);
        BSplCLibCollections.Reverse(ref reversed);
        if (reversed.Lower != -2 || reversed[-2] != 2 || reversed[0] != 3)
            throw new InvalidOperationException("Copied reversal lost native bounds.");
        CopiedSequence<double> sequence = new([]);
        GeomLibCollections.FuseIntervals(new([0, 0.5, 1]), new([0, 0.25, 1]), ref sequence, 1e-9, false);
        if (sequence.Count != 4 || sequence[2] != 0.25)
            throw new InvalidOperationException("Array-to-sequence merge failed.");
    }
}
