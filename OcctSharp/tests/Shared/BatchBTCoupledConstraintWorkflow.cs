using OcctSharp.CopiedCollections;
using OcctSharp.Geometry.Constraints;
using OcctSharp.Geometry.Workspaces;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;
internal static class BatchBTCoupledConstraintWorkflow {
    private static void Near(double expected, double actual) {
        if (!double.IsFinite(actual) || Math.Abs(expected - actual) > 2e-8)
            throw new InvalidOperationException("BT polynomial/derivative mismatch.");
    }
    internal static void Run() {
        Coordinates3d[] positions = [new(0, 0, 0), new(1, 1, 1)];
        Coordinates3d[] tangents = [new(1, 0, 0), new(1, 2, 3)];
        Coordinates3d[] curvatures = [new(0, 2, 0), new(0, 2, 6)];
        Point2d[] p2 = [new(4, 7), new(8, 11), new(12, 15)];
        Point2d[] t2 = [new(1, 2), new(3, 4), new(5, 6)];
        Point2d[] c2 = [new(10, 20), new(30, 40), new(50, 60)];
        var inputs = new List<ConstraintRowInput> { new() };
        for (int mode = 0; mode <= 2; ++mode) {
            var a = new ConstraintInput3d(positions, mode >= 1 ? tangents : null, mode == 2 ? curvatures : null, -7);
            var b = new ConstraintInput2d(p2, mode >= 1 ? t2 : null, mode == 2 ? c2 : null, 14);
            inputs.Add(new(a)); inputs.Add(new(null, b)); inputs.Add(new(a, b));
        }
        var snapshots = CoupledConstraints.Copy(inputs, 30);
        if (snapshots.Count != 10 || snapshots[0].HasTangency || snapshots[0].Positions3d is not null)
            throw new InvalidOperationException("BT empty/default constraint failed.");
        for (int r = 1; r < snapshots.Count; ++r) {
            var row = snapshots[r]; int mode = (r - 1) / 3;
            if (row.NativeOrdinal != r + 1 || row.HasTangency != (mode >= 1) || row.HasCurvature != (mode == 2))
                throw new InvalidOperationException("BT channel mode/ordinal differs.");
            if (row.Positions3d is not null && (row.Positions3d.Lower != -7 || !row.Positions3d.ToArray().SequenceEqual(positions)))
                throw new InvalidOperationException("BT3D positions/bounds differ.");
            if (row.Positions2d is not null && (row.Positions2d.Lower != 14 || !row.Positions2d.ToArray().SequenceEqual(p2)))
                throw new InvalidOperationException("BT2D global-index mapping differs.");
            if (mode >= 1 && (row.Tangents3d is not null && !row.Tangents3d.ToArray().SequenceEqual(tangents)
                || row.Tangents2d is not null && !row.Tangents2d.ToArray().SequenceEqual(t2)))
                throw new InvalidOperationException("BT tangents differ.");
            if (mode == 2 && (row.Curvatures3d is not null && !row.Curvatures3d.ToArray().SequenceEqual(curvatures)
                || row.Curvatures2d is not null && !row.Curvatures2d.ToArray().SequenceEqual(c2)))
                throw new InvalidOperationException("BT curvatures differ.");
        }
        positions[0] = new(99, 99, 99); tangents[0] = new(99, 99, 99); curvatures[0] = new(99, 99, 99);
        p2[0] = new(99, 99); t2[0] = new(99, 99); c2[0] = new(99, 99);
        var saved = snapshots[9];
        // Both native objects and original enumerations have ended. All channels feed actual Hermite interpolation.
        var sampleRows = new List<double>();
        // The SDK band factorization has no pivoting: right endpoint derivatives
        // precede its position to preserve nonzero diagonal pivots.
        foreach (var v in new[] { saved.Positions3d![-7], saved.Tangents3d![-7], saved.Curvatures3d![-7],
            saved.Curvatures3d![-6], saved.Tangents3d![-6], saved.Positions3d![-6] })
            sampleRows.AddRange([v.X, v.Y, v.Z]);
        var solved = SplineWorkspaceInterpolation.Curve([0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1], 5,
            [0, 0, 0, 1, 1, 1], [0, 1, 2, 2, 1, 0], new(sampleRows.ToArray(), 6, 3));
        var poles = new CopiedArray<Coordinates3d>(Enumerable.Range(1, 6)
            .Select(i => new Coordinates3d(solved[i, 1], solved[i, 2], solved[i, 3])).ToArray());
        using var curve = GeomBSplineCurveCollections.Create(poles, new([0.0, 1]), new([6, 6]), 5, false);
        using var cache = SplineWorkspace.Cache(curve, .5);
        for (int i = 0; i <= 20; ++i) {
            double t = i / 20.0; var p = curve.EvalD0(t); var d = cache.EvaluateCurve(t, 2);
            Near(t, p.X); Near(t * t, p.Y); Near(t * t * t, p.Z);
            Near(1, d[1, 1]); Near(2 * t, d[1, 2]); Near(3 * t * t, d[1, 3]);
            Near(0, d[2, 1]); Near(2, d[2, 2]); Near(6 * t, d[2, 3]);
        }
        using var edge = BatchAGSurfaceInterpolationWorkflow.Edge(curve);
        curve.Dispose(); cache.Dispose();
        if (!edge.IsValid || edge.CountSubShapes(ShapeKind.Edge) != 1 || saved.Positions3d![-7] != new Coordinates3d(0, 0, 0)
            || saved.Positions2d![14] != new Point2d(4, 7) || saved.Curvatures2d![14] != new Point2d(10, 20))
            throw new InvalidOperationException("BT detached result/edge failed.");
        string path = Path.Combine(Path.GetTempPath(), "OcctSharp.BT." + Guid.NewGuid() + ".step");
        try {
            ShapeExchange.WriteStep(edge, path);
            using var read = ShapeExchange.ReadStep(path);
            if (!read.IsValid || read.CountSubShapes(ShapeKind.Edge) != 1) throw new InvalidOperationException("BT STEP readback failed.");
            using var readEdge = read.GetSubShapes(ShapeKind.Edge).Single();
            foreach (double t in new[] { 0.0, .5, 1.0 }) {
                var point = readEdge.EvaluateEdge(t).Point;
                Near(t, point.X); Near(t * t, point.Y); Near(t * t * t, point.Z);
            }
        } finally { File.Delete(path); }
        Console.WriteLine($"BT coupled-copy workflow passed: {snapshots.Count} initialized SDK profiles, independent Hermite derivatives and STEP geometry readback.");
    }
}
