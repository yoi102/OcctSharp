using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Displays a copied defect with native selection. Replacing the active snapshot invalidates old review IDs.</summary>
public sealed class RepairViewerReview : IDisposable
{
    private readonly OcctViewer viewer;
    private RepairSnapshot snapshot;
    private ViewerPresentation? presentation;
    private bool disposed;
    public RepairViewerReview(OcctViewer viewer, RepairSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(viewer); ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.ThrowIfDisposed(); this.viewer = viewer; this.snapshot = snapshot;
    }
    public ViewerPresentation Focus(RepairSelection selection, ViewerColor? color = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this); snapshot.Validate(selection);
        using Shape shape = snapshot.CopySubshape(selection);
        ViewerPresentation next = viewer.Display(shape);
        try
        {
            next.SetColor(color ?? new ViewerColor(1, 0.15, 0.05));
            NativeError.ThrowIfFailed(NativeMethods.RepairViewerSelect(viewer.Handle, next.Id), "repair_viewer_select");
            viewer.FitSelected(); presentation?.Dispose(); presentation = next; return next;
        }
        catch { next.Dispose(); throw; }
    }
    public void ReplaceSnapshot(RepairSnapshot next)
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(next); next.ThrowIfDisposed();
        presentation?.Dispose(); presentation = null; snapshot = next;
    }
    public void Dispose() { if (disposed) return; disposed = true; presentation?.Dispose(); }
}
