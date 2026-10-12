using OcctSharp.Generated;
using OcctSharp.HatchingWorkspaces;
using OcctSharp.ModelingWorkspaces;
using OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchAQHatchingWorkflow
{
    internal static Geom2dLine Line(double x, double y, double dx = 1, double dy = 0) => new(new Point2d(x, y), new Direction2d(dx, dy));
    internal static HatchingBoundary Segment(List<IDisposable> owners, double x, double y, double nextX, double nextY)
    {
        double dx = nextX - x, dy = nextY - y, length = Math.Sqrt(dx * dx + dy * dy);
        var line = Line(x, y, dx / length, dy / length); owners.Add(line);
        return new(new HatchingCurve(line, 0, length));
    }
    internal static HatchingBoundary[] Rectangle(List<IDisposable> owners, double left = -4, double bottom = -3, double right = 4, double top = 3) =>
    [Segment(owners, left, bottom, right, bottom), Segment(owners, right, bottom, right, top),
     Segment(owners, right, top, left, top), Segment(owners, left, top, left, bottom)];

    static double Length(Shape shape)
    {
        if (shape.Kind is ShapeKind.Edge or ShapeKind.Wire) return shape.InspectProperties(InspectionPropertyKind.Length).Mass;
        var edges = shape.GetSubShapes(ShapeKind.Edge);
        try { return edges.Sum(e => e.InspectProperties(InspectionPropertyKind.Length).Mass); }
        finally { foreach (var edge in edges) edge.Dispose(); }
    }

    internal static void Run()
    {
        var owners = new List<IDisposable>();
        try
        {
            var rectangle = Rectangle(owners);
            using var hole = new Geom2dCircle(new Axis2d(new(0, 0), new(1, 0)), 1, true);
            HatchingBoundary[] boundaries = [.. rectangle, new(new HatchingCurve(hole, 0, 2 * Math.PI), TopAbsOrientation.TopAbs_REVERSED)];
            using var scan = Line(0, 0); using var high = Line(0, 2); using var outside = Line(0, 4);
            using var snapshot = GeometricHatching.Scan(boundaries, [new(scan, -10, 10), new(high, -10, 10), new(outside, -10, 10)]);
            if (typeof(GeometricHatching).Assembly.GetName().Name != "OcctSharp.Geometry"
                || typeof(GeometricHatching).Assembly.GetReferencedAssemblies().Any(a => a.Name == "OcctSharp" || a.Name == "OcctSharp.Modeling"))
                throw new InvalidOperationException("AQ geometry workspace introduced an invalid module dependency.");
            if (!snapshot.IsDone || snapshot.Curves.Any(c => !c.TrimDone || c.TrimFailed || !c.IsDone || c.Status != HatchGenErrorStatus.HatchGen_NoProblem))
                throw new InvalidOperationException("AQ valid contours failed native completion.");
            var domains = snapshot.Curves[0].Domains;
            if (domains.Count != 2 || Math.Abs(domains[0].First!.Value + 4) > 1e-7 || Math.Abs(domains[0].Last!.Value + 1) > 1e-7
                || Math.Abs(domains[1].First!.Value - 1) > 1e-7 || Math.Abs(domains[1].Last!.Value - 4) > 1e-7
                || snapshot.Curves[1].Domains.Count != 1 || snapshot.Curves[2].Domains.Count != 0)
                throw new InvalidOperationException("AQ rectangle/hole intervals disagree with analytic geometry.");
            using var trimmed2d = snapshot.TrimmedCurve(0, 0)!;
            using var lifted = snapshot.TrimmedCurve3d(0, 0, new Plane(new(new(0, 0, 0), new(0, 0, 1), new(1, 0, 0), new(0, 1, 0))))!;
            foreach (var owner in owners) owner.Dispose(); hole.Dispose(); scan.Dispose(); high.Dispose(); outside.Dispose(); snapshot.Dispose();
            Point2d midpoint = default; OcctSharp.ReferenceOperations.Geom2dCurveReferences.D0(trimmed2d, -2, ref midpoint);
            if (Math.Abs(midpoint.X + 2) > 1e-8 || Math.Abs(midpoint.Y) > 1e-8) throw new InvalidOperationException("AQ copied 2D trim lost geometry.");
            using var builder = ModelingBuilders.Edge(lifted, domains[0].ClippedFirst, domains[0].ClippedLast);
            using var edge = builder.Shape; lifted.Dispose(); builder.Dispose();
            if (!edge.IsValid || Math.Abs(edge.InspectProperties(InspectionPropertyKind.Length).Mass - 3) > 1e-7)
                throw new InvalidOperationException("AQ lifted curve/AP topology composition failed.");
            using var faceWire = ShapeFactory.CreatePolygonWire([new(-4, -3, 0), new(4, -3, 0), new(4, 3, 0), new(-4, 3, 0)], true);
            using var face = ShapeFactory.CreatePlanarFace(faceWire);
            using var common = edge.Common(face);
            if (Math.Abs(Length(common) - 3) > 1e-6) throw new InvalidOperationException("AQ trimmed topology falls outside face domain.");
            foreach (bool step in new[] { true, false })
            {
                string path = Path.Combine(Path.GetTempPath(), "OcctSharp.AQ." + Guid.NewGuid() + (step ? ".step" : ".iges"));
                try
                {
                    if (step) ShapeExchange.WriteStep(edge, path); else ShapeExchange.WriteIges(edge, path);
                    using var restored = step ? ShapeExchange.ReadStep(path) : ShapeExchange.ReadIges(path);
                    if (!restored.IsValid || Math.Abs(Length(restored) - 3) > 1e-5)
                        throw new InvalidOperationException("AQ trimmed edge independent readback failed.");
                }
                finally { File.Delete(path); }
            }
            using var coincident = Line(-2, 0, -1, 0);
            using var endpoint = Line(0, 0, -1, -1);
            using var crossing = Line(-1, 1, 1, -1);
            using var failureLine = Line(0, 0);
            using var failure = GeometricHatching.Scan([
                new(new HatchingCurve(coincident, 0, 3), TopAbsOrientation.TopAbs_REVERSED),
                new(new HatchingCurve(endpoint, 0, 3), TopAbsOrientation.TopAbs_REVERSED),
                new(new HatchingCurve(crossing, 0, 3))], [new(failureLine, -10, 10)], keepPoints: true);
            var diagnostic = failure.Curves[0];
            if (failure.IsDone || !diagnostic.IsDone || diagnostic.HasUsableDomains
                || diagnostic.Status != HatchGenErrorStatus.HatchGen_IncoherentParity
                || diagnostic.Domains.Count != 0 || diagnostic.Points.Count == 0)
                throw new InvalidOperationException("AQ native completion flag masked failed domain computation.");
            try { using var invalid = failure.TrimmedCurve(0, 0); }
            catch (InvalidOperationException) { return; }
            throw new InvalidOperationException("AQ failed domain query published a curve.");
        }
        finally { foreach (var owner in owners) owner.Dispose(); }
    }
}
