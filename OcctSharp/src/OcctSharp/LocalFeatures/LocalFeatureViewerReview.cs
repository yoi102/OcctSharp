namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Creating-thread-bound result/fault/limit review with sampled traces of copied simulated circles.</summary>
public sealed class LocalFeatureViewerReview : IDisposable
{
    private readonly OcctViewer viewer;
    private List<ViewerPresentation> presentations = [];
    private bool disposed;
    public LocalFeatureViewerReview(OcctViewer viewer)
    { ArgumentNullException.ThrowIfNull(viewer); viewer.EnsureThread(); this.viewer = viewer; }
    public IReadOnlyList<ViewerPresentation> Presentations => presentations.AsReadOnly();
    public Guid? PlanId { get; private set; }
    public int DisplayedSections { get; private set; }
    public bool ShowingFailure { get; private set; }
    public void Show(LocalFeatureResult result, LocalFeatureResult? simulation = null, IEnumerable<Shape>? limits = null, int sectionStride = 1, Shape? context = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this); viewer.EnsureThread(); ArgumentNullException.ThrowIfNull(result); result.ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfLessThan(sectionStride, 1);
        if (simulation is not null)
        {
            simulation.ThrowIfDisposed();
            if (simulation.PlanId != result.PlanId || simulation.Source != result.Source)
                throw new ArgumentException("Section simulation belongs to a different recipe or source.");
        }
        var stops = ScalarLawDefinition.Copy(limits ?? [], 256);
        context?.ThrowIfDisposed();
        foreach (var stop in stops) { ArgumentNullException.ThrowIfNull(stop); stop.ThrowIfDisposed(); }
        var sections = (simulation ?? result).SimulatedSections.Where((_, i) => i % sectionStride == 0).ToArray();
        if (sections.Length > 2048) throw new ArgumentException("Increase sectionStride to display at most 2048 section traces.");
        List<ViewerPresentation> next = [];
        void Display(Shape shape, ViewerColor color)
        { var p = viewer.Display(shape); next.Add(p); p.SetColor(color); }
        try
        {
            if (context is not null) Display(context, new(.5, .55, .6));
            if (result.Shape is { } root) Display(root, new(.2, .65, .85));
            foreach (var item in result.GetGroup(LocalFeatureHistoryKind.ProblemShape)) Display(item, new(.95, .1, .1));
            foreach (var item in result.GetGroup(LocalFeatureHistoryKind.Partial)) Display(item, new(.95, .55, .1));
            foreach (var item in result.GetGroup(LocalFeatureHistoryKind.Limit).Concat(stops)) Display(item, new(.45, .45, .7));
            foreach (var section in sections)
            {
                var n = section.Normal; var x = section.XDirection;
                GpXyz y = new(n.Y * x.Z - n.Z * x.Y, n.Z * x.X - n.X * x.Z, n.X * x.Y - n.Y * x.X);
                GpPoint[] points = Enumerable.Range(0, 33).Select(i =>
                {
                    double angle = section.FirstParameter + (section.LastParameter - section.FirstParameter) * i / 32;
                    double a = section.Radius * Math.Cos(angle), b = section.Radius * Math.Sin(angle);
                    return new GpPoint(section.Center.X + a * x.X + b * y.X, section.Center.Y + a * x.Y + b * y.Y, section.Center.Z + a * x.Z + b * y.Z);
                }).ToArray();
                using var trace = ShapeFactory.CreatePolygonWire(points); Display(trace, new(.1, .9, .2));
            }
            viewer.Redraw();
        }
        catch { foreach (var p in next) p.Dispose(); throw; }
        var previous = presentations; presentations = next; foreach (var p in previous) p.Dispose();
        PlanId = result.PlanId; DisplayedSections = sections.Length; ShowingFailure = !result.Diagnostics.AlgorithmDone;
        if (next.Count != 0) viewer.FitAll(); viewer.Redraw();
    }
    public void Dispose()
    {
        if (disposed) return;
        if (!viewer.IsDisposed) { viewer.EnsureThread(); foreach (var p in presentations) p.Dispose(); }
        presentations.Clear(); disposed = true;
    }
}
#pragma warning restore CS1591
