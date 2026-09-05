namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Thread/parent-bound copied-topology review. A failed replacement leaves the previous presentations intact.</summary>
public sealed class RegionViewerReview : IDisposable
{
    private readonly OcctViewer viewer;
    private List<ViewerPresentation> presentations = [];
    private bool disposed;
    public RegionViewerReview(OcctViewer viewer) { ArgumentNullException.ThrowIfNull(viewer); viewer.EnsureThread(); this.viewer = viewer; }
    public IReadOnlyList<ViewerPresentation> Presentations => presentations.AsReadOnly();
    public Guid? Revision { get; private set; }
    public void Show(PartitionResult result, string outputKey, IReadOnlyList<RegionCellId>? cells = null,
        IReadOnlyList<RegionBoundaryId>? interfaces = null, Shape? envelope = null)
    {
        ArgumentNullException.ThrowIfNull(result); Check(); List<Shape> shapes = []; List<ViewerColor> colors = [];
        try
        {
            if (envelope is not null) { shapes.Add(RegionStorage.CopyShape(envelope)); colors.Add(new(.65, .65, .65)); }
            shapes.Add(result.CopyOutput(outputKey)); colors.Add(new(.25, .6, .85));
            foreach (var cell in cells ?? []) { shapes.Add(result.CopyCell(cell)); colors.Add(new(.9, .65, .15)); }
            foreach (var boundary in interfaces ?? []) { shapes.Add(result.CopyBoundary(boundary)); colors.Add(new(.9, .15, .25)); }
            Replace(shapes, colors); Revision = result.Revision;
        }
        finally { foreach (var shape in shapes) shape.Dispose(); }
    }
    public void ShowVolumes(VolumeConstructionResult result, IReadOnlyList<RegionCellId>? selected = null, Shape? envelope = null)
    {
        ArgumentNullException.ThrowIfNull(result); Check(); List<Shape> shapes = []; List<ViewerColor> colors = [];
        try
        {
            if (envelope is not null) { shapes.Add(RegionStorage.CopyShape(envelope)); colors.Add(new(.65, .65, .65)); }
            foreach (var volume in selected ?? result.Volumes.Select(v => v.Id).ToArray()) { shapes.Add(result.CopyVolume(volume)); colors.Add(new(.2, .8, .65)); }
            Replace(shapes, colors); Revision = result.Revision;
        }
        finally { foreach (var shape in shapes) shape.Dispose(); }
    }
    private void Replace(List<Shape> shapes, List<ViewerColor> colors)
    {
        if (shapes.Count > 2048) throw new ArgumentException("Review exceeds 2048 presentations.");
        List<ViewerPresentation> next = [];
        try
        {
            for (int i = 0; i < shapes.Count; i++) { var p = viewer.Display(shapes[i]); next.Add(p); p.SetColor(colors[i]); }
            viewer.Redraw();
        }
        catch { foreach (var p in next) p.Dispose(); throw; }
        var old = presentations; presentations = next; foreach (var p in old) p.Dispose();
        if (next.Count != 0) viewer.FitAll(); viewer.Redraw();
    }
    private void Check() { ObjectDisposedException.ThrowIf(disposed, this); viewer.EnsureThread(); }
    public void Dispose()
    {
        if (disposed) return;
        if (!viewer.IsDisposed) { viewer.EnsureThread(); foreach (var p in presentations) p.Dispose(); }
        presentations.Clear(); disposed = true;
    }
}
