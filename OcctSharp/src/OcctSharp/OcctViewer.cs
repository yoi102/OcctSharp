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
        Input = new ViewerInputController(this);
    }

    internal ViewerHandle Handle { get; }

    /// <summary>Gets the parent-bound application input adapter for this viewer.</summary>
    public ViewerInputController Input { get; }

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

    /// <summary>Sets one of the standard Z-up camera projections.</summary>
    public void SetProjection(ViewerProjection projection)
    {
        if (!Enum.IsDefined(projection)) throw new ArgumentOutOfRangeException(nameof(projection));
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.SetViewerProjection(Handle, (int)projection), "viewer_set_projection");
    }

    /// <summary>Applies a positive zoom factor relative to the current interaction start.</summary>
    public void Zoom(double factor)
    {
        if (!double.IsFinite(factor) || factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.ZoomViewer(Handle, factor), "viewer_zoom");
    }

    /// <summary>Pans the view by client pixels.</summary>
    public void Pan(int deltaX, int deltaY)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.PanViewer(Handle, deltaX, deltaY), "viewer_pan");
    }

    /// <summary>Begins mouse-driven rotation at a client coordinate.</summary>
    public void StartRotation(int x, int y, double zRotationThreshold = 0.4)
    {
        if (!double.IsFinite(zRotationThreshold) || zRotationThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(zRotationThreshold));
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.StartViewerRotation(Handle, x, y, zRotationThreshold),
            "viewer_start_rotation");
    }

    /// <summary>Continues mouse-driven rotation from the last start/rotation coordinate.</summary>
    public void Rotate(int x, int y)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.RotateViewer(Handle, x, y), "viewer_rotate");
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

    /// <summary>Selects using replace, add, remove, or toggle semantics.</summary>
    public IReadOnlyList<ViewerPresentation> SelectAt(int x, int y, ViewerSelectionMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SelectViewerAtMode(Handle, x, y, (int)mode, out _),
            "viewer_select_at_mode");
        return GetSelection();
    }

    /// <summary>Clears the current selection without forcing a redraw.</summary>
    public void ClearSelection()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.ClearViewerSelection(Handle), "viewer_clear_selection");
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

    /// <summary>Returns selected presentations paired with independently owned topology copies.</summary>
    public unsafe IReadOnlyList<ViewerSelectionItem> GetSelectedItems()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.GetViewerSelectedCount(Handle, out int count), "viewer_selected_count");
        if (count == 0) return [];

        long[] ids = new long[count];
        nint[] nativeShapes = new nint[count];
        fixed (long* idPointer = ids)
        fixed (nint* shapePointer = nativeShapes)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.SnapshotViewerSelectedTopology(
                    Handle, idPointer, shapePointer, count, out int written),
                "viewer_selected_topology_snapshot");
            if (written != count)
            {
                for (int index = 0; index < written; ++index) NativeMethods.ReleaseShape(nativeShapes[index]);
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The viewer selected-topology count changed during enumeration.");
            }
        }

        List<ViewerSelectionItem> result = new(count);
        int created = 0;
        try
        {
            for (; created < count; ++created)
            {
                if (!_presentations.TryGetValue(ids[created], out ViewerPresentation? presentation)
                    || presentation.IsRemoved)
                    throw new OcctException(
                        NativeStatus.UnknownException.ToString(),
                        "A selected presentation is outside the managed viewer registry.");
                Shape shape = ShapeFactory.FromNativeHandle(nativeShapes[created], "viewer_selected_topology_snapshot");
                result.Add(new ViewerSelectionItem(presentation, shape));
            }
            return result;
        }
        catch
        {
            foreach (ViewerSelectionItem item in result) item.Dispose();
            for (int index = created; index < count; ++index) NativeMethods.ReleaseShape(nativeShapes[index]);
            throw;
        }
    }

    internal void SetVisible(ViewerPresentation presentation, bool visible)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationVisible(Handle, presentation.Id, visible ? 1 : 0),
            "viewer_set_presentation_visible");
    }

    internal void SetColor(ViewerPresentation presentation, ViewerColor color)
    {
        color.Validate();
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationColor(Handle, presentation.Id, color.Red, color.Green, color.Blue),
            "viewer_set_presentation_color");
    }

    internal void SetTransparency(ViewerPresentation presentation, double transparency)
    {
        if (!double.IsFinite(transparency) || transparency is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(transparency));
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationTransparency(Handle, presentation.Id, transparency),
            "viewer_set_presentation_transparency");
    }

    internal void SetDisplayMode(ViewerPresentation presentation, ViewerDisplayMode displayMode)
    {
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationDisplayMode(Handle, presentation.Id, (int)displayMode),
            "viewer_set_presentation_display_mode");
    }

    internal void SetSelectionKind(ViewerPresentation presentation, ShapeKind? kind)
    {
        if (kind is ShapeKind value && value is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                "Viewer subshape selection supports Compound through Vertex; use null for whole-object selection.");
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationSelectionKind(
                Handle, presentation.Id, kind is null ? -1 : (int)kind.Value),
            "viewer_set_presentation_selection_kind");
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
