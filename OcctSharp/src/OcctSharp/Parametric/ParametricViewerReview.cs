namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Creating-thread-bound review. Refresh replaces IDs after recompute or undo; old presentation IDs become invalid.</summary>
public sealed class ParametricViewerReview : IDisposable
{
    private readonly OcctViewer viewer;
    private readonly ParametricDocument document;
    private List<ViewerPresentation> presentations = [];
    private bool disposed;
    public ParametricViewerReview(OcctViewer viewer, ParametricDocument document)
    {
        ArgumentNullException.ThrowIfNull(viewer); ArgumentNullException.ThrowIfNull(document);
        viewer.EnsureThread(); this.viewer = viewer; this.document = document;
    }
    public IReadOnlyList<ViewerPresentation> Presentations => presentations.AsReadOnly();
    public Guid? DisplayedRevision { get; private set; }
    public bool ShowingStaleInputs { get; private set; }
    public void Refresh(Guid feature)
    {
        Validate(); using var result = document.GetResult(feature);
        if (result.Shape is null) throw new NotSupportedException("Scalar results do not have a viewer presentation.");
        Replace([result.Shape], new(0.2, 0.65, 0.85)); DisplayedRevision = result.Revision; ShowingStaleInputs = false;
    }
    public void ShowFailureInputs(Guid feature)
    {
        Validate(); var snapshot = document.Features.SingleOrDefault(x => x.Definition.Id == feature)
            ?? throw new ArgumentException("The feature is absent.");
        if (snapshot.State is not (ParametricExecutionState.Failed or ParametricExecutionState.Blocked))
            throw new InvalidOperationException("Only a failed or blocked feature has failure-input review.");
        List<ParametricResult> inputs = [];
        try
        {
            foreach (var input in snapshot.Definition.Inputs.Where(x => x.Kind != ParametricOutputKind.Scalar))
                inputs.Add(document.GetResult(input.FeatureId, allowStale: true));
            if (inputs.Count == 0) inputs.Add(document.GetResult(feature, allowStale: true));
            Replace(inputs.Where(x => x.Shape is not null).Select(x => x.Shape!).ToArray(), new(0.95, 0.1, 0.1));
            ShowingStaleInputs = inputs.Any(x => x.IsStale); DisplayedRevision = null;
        }
        finally { foreach (var input in inputs) input.Dispose(); }
    }
    private void Replace(IReadOnlyList<Shape> shapes, ViewerColor color)
    {
        List<ViewerPresentation> next = [];
        try
        {
            foreach (var shape in shapes) { var presentation = viewer.Display(shape); next.Add(presentation); presentation.SetColor(color); }
            viewer.Redraw();
        }
        catch { foreach (var item in next) item.Dispose(); throw; }
        var previous = presentations; presentations = next;
        foreach (var item in previous) item.Dispose();
        if (next.Count != 0) viewer.FitAll(); viewer.Redraw();
    }
    private void Validate() { ObjectDisposedException.ThrowIf(disposed, this); viewer.EnsureThread(); }
    public void Dispose()
    {
        if (disposed) return;
        if (!viewer.IsDisposed) { viewer.EnsureThread(); foreach (var item in presentations) item.Dispose(); }
        presentations.Clear(); disposed = true;
    }
}
#pragma warning restore CS1591
