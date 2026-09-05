namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Creating-thread-bound section/constraint/result review using the existing viewer owner.</summary>
public sealed class GuidedAuthoringReview : IDisposable
{
    private readonly OcctViewer viewer;
    private List<ViewerPresentation> presentations = [];
    private bool disposed;
    public IReadOnlyList<ViewerPresentation> Presentations => presentations.AsReadOnly();
    public GuidedAuthoringReview(OcctViewer viewer) { ArgumentNullException.ThrowIfNull(viewer); viewer.EnsureThread(); this.viewer = viewer; }
    public void ShowSimulation(AuthoringResult simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        if (!simulation.Diagnostics.AlgorithmDone || simulation.SimulatedSections.Count == 0) throw new ArgumentException("A completed section simulation is required.");
        Replace(simulation.SimulatedSections, new(0.1, 0.7, 0.9), ViewerDisplayMode.Wireframe);
    }
    public void ShowResult(AuthoringResult result) { ArgumentNullException.ThrowIfNull(result); Replace([result.RequireShape()], new(0.2, 0.7, 0.4), ViewerDisplayMode.Shaded); }
    public void ShowResult(ConstrainedFillResult result) { ArgumentNullException.ThrowIfNull(result); Replace([result.RequireFace()], new(0.2, 0.7, 0.4), ViewerDisplayMode.Shaded); }
    public void ShowUnsatisfiedConstraints(ConstrainedFillPlan plan, ConstrainedFillResult result)
    {
        Validate(); ArgumentNullException.ThrowIfNull(plan); ArgumentNullException.ThrowIfNull(result);
        if (result.Result.PlanId != plan.Id) throw new ArgumentException("Constraint review belongs to a foreign plan.");
        var failed = result.Constraints.Where(c => !c.Accepted).Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        List<Shape> shapes = [];
        try
        {
            foreach (var constraint in plan.Constraints.Where(c => failed.Contains(c.Id)))
            {
                if (constraint.ShapeInputIndex is int edge) shapes.Add(plan.CopyInput(edge));
                else
                {
                    GpPoint point = constraint.Point;
                    if (constraint.SupportInputIndex is int index)
                    {
                        using Shape support = plan.CopyInput(index);
                        point = SurfaceModeling.Evaluate(support, new(constraint.U, constraint.V)).Point;
                    }
                    using Shape marker = ShapeFactory.CreateSphere(0.05);
                    shapes.Add(marker.Transformed(ShapeTransform.CreateTranslation(point.X, point.Y, point.Z)));
                }
            }
            Replace(shapes, new(0.95, 0.1, 0.1), ViewerDisplayMode.Wireframe);
        }
        finally { foreach (Shape shape in shapes) shape.Dispose(); }
    }
    private void Replace(IReadOnlyList<Shape> shapes, ViewerColor color, ViewerDisplayMode mode)
    {
        Validate(); List<ViewerPresentation> next = [];
        try
        {
            foreach (Shape shape in shapes) { ViewerPresentation p = viewer.Display(shape); next.Add(p); p.SetColor(color); p.SetDisplayMode(mode); }
            viewer.Redraw();
        }
        catch { foreach (var item in next) item.Dispose(); throw; }
        // Installation failures leave the previous presentations alive. Once installed,
        // a camera/redraw error must not dispose the new list behind the caller's back.
        List<ViewerPresentation> previous = presentations;
        presentations = next;
        foreach (var old in previous) old.Dispose();
        if (next.Count != 0) viewer.FitAll();
        viewer.Redraw();
    }
    private void Validate() { ObjectDisposedException.ThrowIf(disposed, this); viewer.EnsureThread(); }
    public void Dispose()
    {
        if (disposed) return; if (viewer.IsDisposed) { presentations.Clear(); disposed = true; return; }
        viewer.EnsureThread(); foreach (var item in presentations) item.Dispose(); presentations.Clear(); disposed = true;
    }
}
#pragma warning restore CS1591
