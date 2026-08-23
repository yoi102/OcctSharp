using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns an OCCT AIS/V3d viewer bound to an existing Windows HWND.</summary>
public sealed class OcctViewer : IDisposable
{
    private readonly int _ownerThreadId;
    private readonly Dictionary<long, ViewerPresentation> _presentations = [];

    private OcctViewer(ViewerHandle handle)
    {
        Handle = handle;
        _ownerThreadId = Environment.CurrentManagedThreadId;
    }

    internal ViewerHandle Handle { get; }

    /// <summary>Creates a viewer for a non-zero HWND on the calling UI thread.</summary>
    public static OcctViewer Create(nint windowHandle)
    {
        if (windowHandle == 0) throw new ArgumentException("A non-zero HWND is required.", nameof(windowHandle));
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.CreateViewer(windowHandle, out nint viewer), "viewer_create");
        return new OcctViewer(new ViewerHandle(viewer));
    }

    /// <summary>Displays an independent AIS presentation of a shape.</summary>
    public ViewerPresentation Display(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.DisplayViewerShape(Handle, shape.Handle, out long id), "viewer_display_shape");
        ViewerPresentation presentation = new(this, id);
        _presentations.Add(id, presentation);
        return presentation;
    }

    /// <summary>Fits all displayed presentations and redraws the view.</summary>
    public void FitAll()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.FitAllViewer(Handle), "viewer_fit_all");
    }

    /// <summary>Redraws the bound window.</summary>
    public void Redraw()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.RedrawViewer(Handle), "viewer_redraw");
    }

    /// <summary>Notifies OCCT that the HWND client size changed.</summary>
    public void Resize()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.ResizeViewer(Handle), "viewer_resize");
    }

    /// <summary>Updates dynamic detection from client pixel coordinates.</summary>
    public bool MoveTo(int x, int y)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.MoveViewerTo(Handle, x, y, out int detected), "viewer_move_to");
        return detected != 0;
    }

    /// <summary>Selects at client pixel coordinates and returns a copied presentation snapshot.</summary>
    public IReadOnlyList<ViewerPresentation> SelectAt(int x, int y)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.SelectViewerAt(Handle, x, y, out _), "viewer_select_at");
        return GetSelection();
    }

    /// <summary>Returns a copied snapshot of selected managed presentations.</summary>
    public unsafe IReadOnlyList<ViewerPresentation> GetSelection()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.GetViewerSelectedCount(Handle, out int count), "viewer_selected_count");
        if (count == 0) return [];

        long[] ids = new long[count];
        fixed (long* pointer = ids)
        {
            NativeError.ThrowIfFailed(NativeMethods.SnapshotViewerSelection(Handle, pointer, ids.Length, out int written), "viewer_selected_snapshot");
            if (written != ids.Length) Array.Resize(ref ids, written);
        }

        List<ViewerPresentation> selected = new(ids.Length);
        foreach (long id in ids)
        {
            if (_presentations.TryGetValue(id, out ViewerPresentation? presentation) && !presentation.IsRemoved)
            {
                selected.Add(presentation);
            }
        }
        return selected;
    }

    internal void SetVisible(ViewerPresentation presentation, bool visible)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationVisible(Handle, presentation.Id, visible ? 1 : 0),
            "viewer_set_presentation_visible");
    }

    internal void Remove(ViewerPresentation presentation)
    {
        if (presentation.IsRemoved) return;
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(NativeMethods.RemoveViewerPresentation(Handle, presentation.Id), "viewer_remove_presentation");
        _presentations.Remove(presentation.Id);
        presentation.MarkRemoved();
    }

    internal bool IsDisposed => Handle.IsClosed || Handle.IsInvalid;

    /// <summary>Releases all presentations and native viewer resources on the owner thread.</summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        EnsureThread();
        foreach (ViewerPresentation presentation in _presentations.Values) presentation.MarkRemoved();
        _presentations.Clear();
        Handle.Dispose();
    }

    private void EnsurePresentation(ViewerPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        EnsureThread();
        if (!ReferenceEquals(presentation.Viewer, this))
        {
            throw new ArgumentException("The presentation belongs to another viewer.", nameof(presentation));
        }
        ObjectDisposedException.ThrowIf(presentation.IsRemoved, presentation);
    }

    private void EnsureThread()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (Environment.CurrentManagedThreadId != _ownerThreadId)
        {
            throw new InvalidOperationException("Viewer operations must run on the thread that created the viewer.");
        }
    }
}
