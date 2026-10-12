using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchAGSurfaceInterpolationWorkflow
{
    internal static CopiedGrid<Coordinates3d> Grid(int rows = 3, int columns = 4, int rowLower = 1, int columnLower = 1)
    {
        var values = new Coordinates3d[rows * columns];
        for (int u = 0; u < rows; ++u)
        for (int v = 0; v < columns; ++v) values[u * columns + v] = new(u * 2, v * 3, u * v * 0.15);
        return new(values, rows, columns, rowLower, columnLower);
    }
    internal static Shape Face(GeomBSplineSurface surface)
    {
        var p = GeomBSplineSurfaceCollections.Poles(surface);
        return FreeformAuthoring.CreateSurfaceFace(FreeformSurfaceDefinition.BSpline(p.Rows, p.Columns,
            p.ToArray().Select(q => new GpPoint(q.X, q.Y, q.Z)),
            GeomBSplineSurfaceCollections.UKnots(surface).ToArray(), GeomBSplineSurfaceCollections.UMultiplicities(surface).ToArray(),
            GeomBSplineSurfaceCollections.VKnots(surface).ToArray(), GeomBSplineSurfaceCollections.VMultiplicities(surface).ToArray(),
            surface.UDegree(), surface.VDegree(), surface.IsUPeriodic(), surface.IsVPeriodic(),
            GeomBSplineSurfaceCollections.WeightsArray(surface).ToArray()));
    }
    internal static Shape Edge(GeomBSplineCurve curve)
    {
        return FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline(
            GeomBSplineCurveCollections.Poles(curve).ToArray().Select(p => new GpPoint(p.X, p.Y, p.Z)),
            GeomBSplineCurveCollections.Knots(curve).ToArray(), GeomBSplineCurveCollections.Multiplicities(curve).ToArray(),
            curve.Degree(), curve.IsPeriodic(), GeomBSplineCurveCollections.WeightsArray(curve).ToArray()));
    }
    internal static void Run()
    {
        if (typeof(FreeformAuthoring).Assembly.GetName().Name != "OcctSharp.Modeling"
            || typeof(FreeformCurveDefinition).Assembly.GetName().Name != "OcctSharp.Modeling"
            || typeof(ShapeExchange).Assembly.GetName().Name != "OcctSharp.DataExchange"
            || typeof(ShapeExchange).Assembly.GetReferencedAssemblies().Any(a => a.Name == "OcctSharp"))
            throw new InvalidOperationException("Surface/curve composition and exchange must work through their owning modules without a facade dependency.");
        using var defined = GeomBSplineSurfaceCollections.Create(Grid(3, 4, -2, 5), new([0, 1]), new([0, 1]), new([3, 3]), new([4, 4]), 2, 3, false, false);
        var poles = GeomBSplineSurfaceCollections.Poles(defined);
        if (poles.Rows != 3 || poles.Columns != 4 || poles[2, 3] != new Coordinates3d(2, 6, 0.3))
            throw new InvalidOperationException("Asymmetric surface grid was transposed or corrupted.");
        var evaluated = defined.EvalD0(0.4, 0.6);
        if (!double.IsFinite(evaluated.Z)) throw new InvalidOperationException("Surface evaluation failed.");
        using Shape face = Face(defined);
        defined.Dispose();
        if (!face.IsValid || face.FaceCount != 1 || poles[3, 4].X != 4)
            throw new InvalidOperationException("Surface face or snapshot depends on disposed geometry.");
        string path = Path.Combine(Path.GetTempPath(), "OcctSharp.AG." + Guid.NewGuid().ToString("N") + ".step");
        try
        {
            ShapeExchange.WriteStep(face, path);
            using Shape restored = ShapeExchange.ReadStep(path);
            if (!restored.IsValid || restored.FaceCount != 1) throw new InvalidOperationException("Surface STEP roundtrip failed.");
        }
        finally { File.Delete(path); }
        using var fit = GeomAPIPointsToBSplineSurfaceCollections.Create(Grid(4, 5), 3, 5, GeomAbsShape.GeomAbs_C2, 1e-5);
        if (!fit.IsDone()) throw new InvalidOperationException("Surface fitting did not complete.");
        using var fitted = fit.Surface(); fit.Dispose();
        using Shape fittedFace = Face(fitted);
        if (!fittedFace.IsValid) throw new InvalidOperationException("Fitted surface lost independent ownership.");
        Coordinates3d[] points = [new(0, 0, 0), new(1, 2, 0), new(3, 1, 1), new(4, 0, 0)];
        using var interpolation = GeomAPIInterpolateCollections.Create(new(points), new([0, 1, 2, 3]), false, 1e-7);
        points[0] = new(999, 999, 999);
        interpolation.Load(new CopiedArray<Vector3d>([new(1, 1, 0), new(0, 0, 0), new(0, 0, 0), new(1, -1, 0)]), new([true, false, false, true]), false);
        interpolation.Perform();
        if (!interpolation.IsDone()) throw new InvalidOperationException("3D interpolation did not complete.");
        using var curve = interpolation.Curve(); interpolation.Dispose();
        using Shape edge = Edge(curve); curve.Dispose();
        if (!edge.IsValid || edge.Kind != ShapeKind.Edge) throw new InvalidOperationException("Interpolated curve-to-edge composition failed.");
        using var interpolation2d = Geom2dAPIInterpolateCollections.Create(new([new Point2d(0, 0), new Point2d(1, 2), new Point2d(3, 0)]), false, 1e-7);
        interpolation2d.Load(new Vector2d(1, 1), new Vector2d(1, -1), false); interpolation2d.Perform();
        if (!interpolation2d.IsDone()) throw new InvalidOperationException("2D interpolation did not complete.");
        using var curve2d = interpolation2d.Curve(); interpolation2d.Dispose();
        using Shape edge2d = FreeformAuthoring.CreateCurve(FreeformCurveDefinition.BSpline(
            Geom2dBSplineCurveCollections.Poles(curve2d).ToArray().Select(p => new GpPoint(p.X, p.Y, 0)),
            Geom2dBSplineCurveCollections.Knots(curve2d).ToArray(), Geom2dBSplineCurveCollections.Multiplicities(curve2d).ToArray(), curve2d.Degree(), curve2d.IsPeriodic()));
        if (!edge2d.IsValid) throw new InvalidOperationException("2D curve-to-edge composition failed.");
    }
}
