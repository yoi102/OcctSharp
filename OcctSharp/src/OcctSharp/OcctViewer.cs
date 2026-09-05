using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns an OCCT AIS/V3d viewer bound to an existing Windows HWND.</summary>
public sealed class OcctViewer : IDisposable
{
    private readonly int _ownerThreadId;
    private readonly Dictionary<long, ViewerPresentation> _presentations = [];
    private readonly Dictionary<long, ViewerClipPlane> _clipPlanes = [];
    private readonly Dictionary<long, ViewerDimension> _dimensions = [];
    private readonly Dictionary<long, ViewerManipulator> _manipulators = [];
    private readonly List<ViewerClipPlane> _savedViewClipPlanes = [];
    private Dictionary<long, bool>? _isolationVisibility;

    private OcctViewer(ViewerHandle handle)
    {
        Handle = handle;
        _ownerThreadId = Environment.CurrentManagedThreadId;
        Input = new ViewerInputController(this);
        Rendering = new ViewerRendering(this);
    }

    internal ViewerHandle Handle { get; }

    /// <summary>Gets the parent-bound application input adapter for this viewer.</summary>
    public ViewerInputController Input { get; }

    /// <summary>Gets the parent/thread-bound light, appearance, render-resource and copied-frame controller.</summary>
    public ViewerRendering Rendering { get; }

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
        return DisplayCore(shape, null);
    }

    /// <summary>
    /// Displays an XDE label through OCCT's inherited component and subshape styles,
    /// including surface, curve, material-base, alpha, and visibility overrides.
    /// The presentation remains valid after the source document is disposed.
    /// </summary>
    public ViewerPresentation Display(XdeLabel label)
    {
        ArgumentNullException.ThrowIfNull(label);
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.DisplayViewerXdeLabel(
                Handle, label.Document.Handle, label.Entry, out long id),
            "viewer_display_xde_label");
        ViewerPresentation presentation = new(this, id, null);
        _presentations.Add(id, presentation);
        if (_isolationVisibility is not null)
        {
            _isolationVisibility[id] = true;
            SetVisible(presentation, false);
        }
        return presentation;
    }

    /// <summary>Displays a located XDE occurrence with copied identity independent of the document.</summary>
    public ViewerPresentation Display(XdeOccurrence occurrence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ViewerSourceIdentity identity = new(
            occurrence.Path,
            occurrence.OccurrenceLabel.Entry,
            occurrence.ReferredLabel.Entry);
        using Shape locatedShape = occurrence.GetLocatedShape();
        return DisplayCore(locatedShape, identity);
    }

    private ViewerPresentation DisplayCore(Shape shape, ViewerSourceIdentity? identity)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.DisplayViewerShape(Handle, shape.Handle, out long id), "viewer_display_shape");
        ViewerPresentation presentation = new(this, id, identity);
        _presentations.Add(id, presentation);
        if (_isolationVisibility is not null)
        {
            _isolationVisibility[id] = true;
            SetVisible(presentation, false);
        }
        return presentation;
    }

    /// <summary>Displays a viewer-owned linear dimension between two copied points.</summary>
    public ViewerDimension DisplayLengthDimension(
        GpPoint first, GpPoint second, ViewerPlaneEquation plane,
        ViewerDimensionStyle? style = null) =>
        CreateDimension(ViewerDimensionKind.Length, null, [first, second], plane, style ?? new());

    /// <summary>Displays a viewer-owned angular dimension formed by first, center, and third points.</summary>
    public ViewerDimension DisplayAngleDimension(
        GpPoint first, GpPoint center, GpPoint third,
        ViewerDimensionStyle? style = null) =>
        CreateDimension(ViewerDimensionKind.Angle, null, [first, center, third], default, style ?? new());

    /// <summary>Displays a viewer-owned radial dimension for circular or revolved topology.</summary>
    public ViewerDimension DisplayRadiusDimension(Shape shape, ViewerDimensionStyle? style = null) =>
        CreateDimension(ViewerDimensionKind.Radius, shape, [], default, style ?? new());

    /// <summary>Displays a viewer-owned diameter dimension for circular or revolved topology.</summary>
    public ViewerDimension DisplayDiameterDimension(Shape shape, ViewerDimensionStyle? style = null) =>
        CreateDimension(ViewerDimensionKind.Diameter, shape, [], default, style ?? new());

    private unsafe ViewerDimension CreateDimension(
        ViewerDimensionKind kind,
        Shape? shape,
        GpPoint[] points,
        ViewerPlaneEquation plane,
        ViewerDimensionStyle style)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (kind is ViewerDimensionKind.Radius or ViewerDimensionKind.Diameter)
            ArgumentNullException.ThrowIfNull(shape);
        else foreach (GpPoint point in points) ValidateFinite(point, nameof(points));
        if (kind == ViewerDimensionKind.Length) plane.Validate();
        style.Validate();
        XyzRaw[] rawPoints = points.Select(ToRaw).ToArray();
        PlaneEquationRaw rawPlane = new(plane.A, plane.B, plane.C, plane.D, 0);
        string modelUnits = kind == ViewerDimensionKind.Angle ? style.Units.ModelAngleUnit : style.Units.ModelLengthUnit;
        string displayUnits = kind == ViewerDimensionKind.Angle ? style.Units.DisplayAngleUnit : style.Units.DisplayLengthUnit;
        EnsureThread();
        bool shapeAddedRef = false;
        try
        {
            shape?.Handle.DangerousAddRef(ref shapeAddedRef);
            fixed (XyzRaw* pointPointer = rawPoints)
            {
                NativeError.ThrowIfFailed(NativeMethods.CreateViewerDimension(
                    Handle, (int)kind, shape?.Handle.DangerousGetHandle() ?? 0,
                    pointPointer, rawPoints.Length, in rawPlane, modelUnits, displayUnits,
                    style.CustomValue.HasValue ? 1 : 0, style.CustomValue.GetValueOrDefault(), style.Flyout,
                    style.Color.Red, style.Color.Green, style.Color.Blue, style.LineWidth, out long id),
                    "viewer_dimension_create");
                ViewerDimension dimension = new(this, id, kind, style);
                _dimensions.Add(id, dimension);
                return dimension;
            }
        }
        finally { if (shapeAddedRef) shape!.Handle.DangerousRelease(); }
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

    /// <summary>Copies the current exact detected whole shape or subshape into an owning result.</summary>
    public ViewerDetectionItem? GetDetectedItem()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SnapshotViewerDetectedTopology(Handle, out long id, out nint nativeShape),
            "viewer_detected_topology_snapshot");
        if (nativeShape == 0) return null;
        try
        {
            if (!_presentations.TryGetValue(id, out ViewerPresentation? presentation) || presentation.IsRemoved)
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The detected presentation is outside the managed viewer registry.");
            Shape shape = ShapeFactory.FromNativeHandle(nativeShape, "viewer_detected_topology_snapshot");
            nativeShape = 0;
            return new ViewerDetectionItem(presentation, shape);
        }
        finally
        {
            if (nativeShape != 0) NativeMethods.ReleaseShape(nativeShape);
        }
    }

    /// <summary>Selects within a client-pixel rectangle using the requested selection scheme.</summary>
    public IReadOnlyList<ViewerPresentation> SelectRectangle(
        int x1, int y1, int x2, int y2, ViewerSelectionMode mode = ViewerSelectionMode.Replace)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (x1 == x2 || y1 == y2) throw new ArgumentException("Selection rectangle must have non-zero area.");
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SelectViewerRectangle(Handle, x1, y1, x2, y2, (int)mode, out _),
            "viewer_select_rectangle");
        return GetSelection();
    }

    /// <summary>Selects within a client-pixel polygon using the requested selection scheme.</summary>
    public unsafe IReadOnlyList<ViewerPresentation> SelectPolygon(
        IReadOnlyList<GpPoint2d> points, ViewerSelectionMode mode = ViewerSelectionMode.Replace)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (points.Count is < 3 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(points), "Selection polygon requires 3 through 4096 points.");
        XyRaw[] raw = new XyRaw[points.Count];
        for (int index = 0; index < raw.Length; ++index)
        {
            GpPoint2d point = points[index];
            if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
                throw new ArgumentOutOfRangeException(nameof(points), "Selection polygon coordinates must be finite.");
            raw[index] = new XyRaw(point.X, point.Y);
        }
        EnsureThread();
        fixed (XyRaw* pointer = raw)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.SelectViewerPolygon(Handle, pointer, raw.Length, (int)mode, out _),
                "viewer_select_polygon");
        }
        return GetSelection();
    }

    /// <summary>Sets the context-wide pixel detection tolerance from 0 through 100.</summary>
    public void SetPixelTolerance(int tolerance)
    {
        if (tolerance is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(tolerance));
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPixelTolerance(Handle, tolerance), "viewer_set_pixel_tolerance");
    }

    /// <summary>Replaces the current built-in filter with one topology-kind filter.</summary>
    public void SetShapeFilter(ShapeKind kind)
    {
        if (kind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(kind));
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerShapeFilter(Handle, (int)kind), "viewer_set_shape_filter");
    }

    /// <summary>Clears every viewer-owned built-in selection filter.</summary>
    public void ClearFilters()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.ClearViewerFilters(Handle), "viewer_clear_filters");
    }

    /// <summary>Returns copied selection bounds, or null when selection is empty.</summary>
    public BoundingBox3d? GetSelectionBounds()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.GetViewerSelectionBounds(Handle, out int hasBounds, out BoundingBoxRaw bounds),
            "viewer_selection_bounds");
        return hasBounds == 0
            ? null
            : new BoundingBox3d(
                new GpPoint(bounds.MinX, bounds.MinY, bounds.MinZ),
                new GpPoint(bounds.MaxX, bounds.MaxY, bounds.MaxZ));
    }

    /// <summary>Fits selected geometry and returns false when selection is empty.</summary>
    public bool FitSelected(double margin = 0.01)
    {
        if (!double.IsFinite(margin) || margin is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(margin));
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.FitSelectedViewer(Handle, margin, out int fitted), "viewer_fit_selected");
        return fitted != 0;
    }

    /// <summary>Shows only selected presentations while retaining a reversible visibility snapshot.</summary>
    public void IsolateSelected()
    {
        EnsureThread();
        IReadOnlyList<ViewerPresentation> selected = GetSelection();
        if (selected.Count == 0) throw new InvalidOperationException("Cannot isolate an empty selection.");
        if (_isolationVisibility is not null) RestoreIsolation();
        HashSet<long> selectedIds = [.. selected.Select(item => item.Id)];
        _isolationVisibility = _presentations.ToDictionary(pair => pair.Key, pair => pair.Value.IsVisible);
        foreach (ViewerPresentation presentation in _presentations.Values)
            SetVisible(presentation, selectedIds.Contains(presentation.Id));
    }

    /// <summary>Restores the visibility state captured by <see cref="IsolateSelected"/>.</summary>
    public bool RestoreIsolation()
    {
        EnsureThread();
        if (_isolationVisibility is null) return false;
        Dictionary<long, bool> snapshot = _isolationVisibility;
        _isolationVisibility = null;
        foreach ((long id, bool visible) in snapshot)
            if (_presentations.TryGetValue(id, out ViewerPresentation? presentation) && !presentation.IsRemoved)
                SetVisible(presentation, visible);
        return true;
    }

    /// <summary>Returns one finite copied camera snapshot.</summary>
    public ViewerCameraState GetCamera()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.GetViewerCamera(Handle, out ViewerCameraRaw raw), "viewer_get_camera");
        return new ViewerCameraState(
            ToPoint(raw.Eye), ToPoint(raw.Target), ToXyz(raw.Up), ToXyz(raw.Projection));
    }

    /// <summary>Validates and atomically restores one camera snapshot.</summary>
    public void SetCamera(ViewerCameraState camera)
    {
        ValidateFinite(camera.Eye, nameof(camera));
        ValidateFinite(camera.Target, nameof(camera));
        ValidateFinite(camera.Up, nameof(camera));
        ValidateFinite(camera.Projection, nameof(camera));
        ViewerCameraRaw raw = new(
            ToRaw(camera.Eye), ToRaw(camera.Target), ToRaw(camera.Up), ToRaw(camera.Projection));
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.SetViewerCamera(Handle, in raw), "viewer_set_camera");
    }

    /// <summary>Applies copied camera, visibility, and clipping values from one saved model view.</summary>
    public void ApplySavedView(XdeSavedViewSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        XdeSavedViewDefinition definition = snapshot.Definition;
        GpPoint eye = definition.ProjectionPoint;
        GpPoint target = new(
            eye.X + definition.ViewDirection.X,
            eye.Y + definition.ViewDirection.Y,
            eye.Z + definition.ViewDirection.Z);
        SetCamera(new ViewerCameraState(eye, target, definition.UpDirection, definition.ViewDirection));

        HashSet<string> visible = snapshot.VisibleShapeEntries.ToHashSet(StringComparer.Ordinal);
        foreach (ViewerPresentation presentation in _presentations.Values)
        {
            if (presentation.IsRemoved || presentation.SourceIdentity is not ViewerSourceIdentity identity) continue;
            SetVisible(presentation,
                visible.Contains(identity.OccurrenceEntry) || visible.Contains(identity.ReferredEntry));
        }

        foreach (ViewerClipPlane plane in _savedViewClipPlanes.ToArray()) plane.Dispose();
        _savedViewClipPlanes.Clear();
        foreach (ViewerPlaneEquation equation in definition.ClippingPlanes)
            _savedViewClipPlanes.Add(CreateClipPlane(equation));
        Redraw();
    }

    /// <summary>Converts one client pixel to a world point on the current view plane.</summary>
    public GpPoint ScreenToWorld(int x, int y)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.ViewerScreenToWorld(Handle, x, y, out XyzRaw raw), "viewer_screen_to_world");
        return ToPoint(raw);
    }

    /// <summary>Projects one finite world point into client pixels.</summary>
    public ViewerPixelPoint WorldToScreen(GpPoint point)
    {
        ValidateFinite(point, nameof(point));
        XyzRaw raw = ToRaw(point);
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.ViewerWorldToScreen(Handle, in raw, out int x, out int y), "viewer_world_to_screen");
        return new ViewerPixelPoint(x, y);
    }

    /// <summary>Produces a normalized world-space pick ray for one client pixel.</summary>
    public ViewerPickRay GetPickRay(int x, int y)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.GetViewerPickRay(Handle, x, y, out ViewerPickRayRaw raw), "viewer_pick_ray");
        return new ViewerPickRay(ToPoint(raw.Origin), ToXyz(raw.Direction));
    }

    /// <summary>Zooms to a normalized non-degenerate client rectangle.</summary>
    public void ZoomWindow(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2 || y1 == y2) throw new ArgumentException("Zoom rectangle must have non-zero area.");
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.WindowFitViewer(Handle, x1, y1, x2, y2), "viewer_window_fit");
    }

    /// <summary>Sets the view's linear RGB background and redraws.</summary>
    public void SetBackgroundColor(ViewerColor color)
    {
        color.Validate();
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerBackgroundColor(Handle, color.Red, color.Green, color.Blue),
            "viewer_set_background_color");
    }

    /// <summary>Creates an enabled parent-bound clipping plane.</summary>
    public ViewerClipPlane CreateClipPlane(ViewerPlaneEquation equation)
    {
        equation.Validate();
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateViewerClipPlane(
                Handle, equation.A, equation.B, equation.C, equation.D, out long id),
            "viewer_create_clip_plane");
        ViewerClipPlane plane = new(this, id, equation);
        _clipPlanes.Add(id, plane);
        return plane;
    }

    /// <summary>Enables or disables computed hidden-line review mode.</summary>
    public void SetComputedHiddenLine(bool enabled)
    {
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerComputedMode(Handle, enabled ? 1 : 0), "viewer_set_computed_mode");
    }

    /// <summary>Shows and configures the standard orientation trihedron.</summary>
    public void ShowTrihedron(
        ViewerTrihedronPosition position = ViewerTrihedronPosition.LeftLower,
        ViewerColor? color = null,
        double scale = 0.08)
    {
        if (!Enum.IsDefined(position)) throw new ArgumentOutOfRangeException(nameof(position));
        if (!double.IsFinite(scale) || scale is <= 0 or > 1) throw new ArgumentOutOfRangeException(nameof(scale));
        ViewerColor actualColor = color ?? new ViewerColor(1, 1, 1);
        actualColor.Validate();
        EnsureThread();
        NativeError.ThrowIfFailed(
            NativeMethods.ShowViewerTrihedron(
                Handle, (int)position, actualColor.Red, actualColor.Green, actualColor.Blue, scale),
            "viewer_show_trihedron");
    }

    /// <summary>Hides the standard orientation trihedron.</summary>
    public void HideTrihedron()
    {
        EnsureThread();
        NativeError.ThrowIfFailed(NativeMethods.HideViewerTrihedron(Handle), "viewer_hide_trihedron");
    }

    /// <summary>Writes a selected viewer buffer to a durable path and returns its full path.</summary>
    public string SaveScreenshot(
        string filePath,
        ViewerScreenshotBuffer buffer = ViewerScreenshotBuffer.Rgb,
        bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!Enum.IsDefined(buffer)) throw new ArgumentOutOfRangeException(nameof(buffer));
        string fullPath = Path.GetFullPath(filePath);
        string extension = Path.GetExtension(fullPath).ToLowerInvariant();
        if (extension is not (".png" or ".bmp" or ".jpg" or ".jpeg"))
            throw new ArgumentException("Screenshot path must use PNG, BMP, JPG, or JPEG.", nameof(filePath));
        string? directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory)) throw new ArgumentException("Screenshot path has no directory.", nameof(filePath));
        Directory.CreateDirectory(directory);
        if (!overwrite && File.Exists(fullPath)) throw new IOException($"Screenshot already exists: '{fullPath}'.");

        EnsureThread();
        string stagingPath = CreateScreenshotStagingPath(extension);
        try
        {
            NativeError.ThrowIfFailed(NativeMethods.DumpViewer(Handle, stagingPath, (int)buffer), "viewer_dump");
            if (!File.Exists(stagingPath) || new FileInfo(stagingPath).Length == 0)
                throw new IOException("OCCT did not produce a non-empty screenshot.");
            File.Move(stagingPath, fullPath, overwrite);
            return fullPath;
        }
        finally
        {
            if (File.Exists(stagingPath)) File.Delete(stagingPath);
        }
    }

    internal void SetVisible(ViewerPresentation presentation, bool visible)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationVisible(Handle, presentation.Id, visible ? 1 : 0),
            "viewer_set_presentation_visible");
        presentation.MarkVisible(visible);
    }

    internal void UpdateDimensionStyle(ViewerDimension dimension, ViewerDimensionStyle style)
    {
        EnsureDimension(dimension);
        style.Validate();
        string modelUnits = dimension.Kind == ViewerDimensionKind.Angle
            ? style.Units.ModelAngleUnit : style.Units.ModelLengthUnit;
        string displayUnits = dimension.Kind == ViewerDimensionKind.Angle
            ? style.Units.DisplayAngleUnit : style.Units.DisplayLengthUnit;
        NativeError.ThrowIfFailed(NativeMethods.UpdateViewerDimensionStyle(
            Handle, dimension.Id, modelUnits, displayUnits,
            style.CustomValue.HasValue ? 1 : 0, style.CustomValue.GetValueOrDefault(), style.Flyout,
            style.Color.Red, style.Color.Green, style.Color.Blue, style.LineWidth),
            "viewer_dimension_update_style");
        dimension.MarkStyle(style);
    }

    internal void SetDimensionVisible(ViewerDimension dimension, bool visible)
    {
        EnsureDimension(dimension);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerDimensionVisible(Handle, dimension.Id, visible ? 1 : 0),
            "viewer_dimension_set_visible");
        dimension.MarkVisible(visible);
    }

    internal void SetDimensionSelected(ViewerDimension dimension, bool selected)
    {
        EnsureDimension(dimension);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerDimensionSelected(Handle, dimension.Id, selected ? 1 : 0),
            "viewer_dimension_set_selected");
    }

    internal void RemoveDimension(ViewerDimension dimension)
    {
        ArgumentNullException.ThrowIfNull(dimension);
        if (dimension.IsRemoved) return;
        EnsureDimension(dimension);
        NativeError.ThrowIfFailed(
            NativeMethods.RemoveViewerDimension(Handle, dimension.Id), "viewer_dimension_remove");
        _dimensions.Remove(dimension.Id);
        dimension.MarkRemoved();
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

    internal void SetSubshapeColor(ViewerPresentation presentation, Shape subshape, ViewerColor color)
    {
        ArgumentNullException.ThrowIfNull(subshape);
        color.Validate();
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerSubshapeColor(
                Handle, presentation.Id, subshape.Handle, color.Red, color.Green, color.Blue),
            "viewer_set_subshape_color");
    }

    internal void SetSubshapeTransparency(
        ViewerPresentation presentation, Shape subshape, double transparency)
    {
        ArgumentNullException.ThrowIfNull(subshape);
        if (!double.IsFinite(transparency) || transparency is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(transparency));
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerSubshapeTransparency(
                Handle, presentation.Id, subshape.Handle, transparency),
            "viewer_set_subshape_transparency");
    }

    internal void SetSubshapeWidth(ViewerPresentation presentation, Shape subshape, double width)
    {
        ArgumentNullException.ThrowIfNull(subshape);
        if (!double.IsFinite(width) || width is <= 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(width));
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerSubshapeWidth(Handle, presentation.Id, subshape.Handle, width),
            "viewer_set_subshape_width");
    }

    internal void ClearSubshapeOverrides(ViewerPresentation presentation, Shape subshape)
    {
        ArgumentNullException.ThrowIfNull(subshape);
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.ClearViewerSubshapeOverrides(Handle, presentation.Id, subshape.Handle),
            "viewer_clear_subshape_overrides");
    }

    internal void ClearAllSubshapeOverrides(ViewerPresentation presentation)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.ClearAllViewerSubshapeOverrides(Handle, presentation.Id),
            "viewer_clear_all_subshape_overrides");
    }

    internal void UpdateClipPlane(ViewerClipPlane plane, ViewerPlaneEquation equation)
    {
        equation.Validate();
        EnsureClipPlane(plane);
        NativeError.ThrowIfFailed(
            NativeMethods.UpdateViewerClipPlane(
                Handle, plane.Id, equation.A, equation.B, equation.C, equation.D),
            "viewer_update_clip_plane");
        plane.MarkUpdated(equation);
    }

    internal void SetClipPlaneEnabled(ViewerClipPlane plane, bool enabled)
    {
        EnsureClipPlane(plane);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerClipPlaneEnabled(Handle, plane.Id, enabled ? 1 : 0),
            "viewer_set_clip_plane_enabled");
        plane.MarkEnabled(enabled);
    }

    internal void RemoveClipPlane(ViewerClipPlane plane)
    {
        if (plane.IsRemoved) return;
        EnsureClipPlane(plane);
        NativeError.ThrowIfFailed(
            NativeMethods.RemoveViewerClipPlane(Handle, plane.Id), "viewer_remove_clip_plane");
        _clipPlanes.Remove(plane.Id);
        plane.MarkRemoved();
    }

    internal void Remove(ViewerPresentation presentation)
    {
        if (presentation.IsRemoved) return;
        EnsurePresentation(presentation);
        foreach (ViewerManipulator manipulator in _manipulators.Values
                     .Where(item => ReferenceEquals(item.Presentation, presentation)).ToArray())
            RemoveManipulator(manipulator);
        NativeError.ThrowIfFailed(NativeMethods.RemoveViewerPresentation(Handle, presentation.Id), "viewer_remove_presentation");
        _presentations.Remove(presentation.Id);
        Rendering.ForgetPresentation(presentation);
        _isolationVisibility?.Remove(presentation.Id);
        presentation.MarkRemoved();
    }

    internal ViewerManipulator CreateManipulator(ViewerPresentation presentation, ViewerManipulatorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.AttachViewerManipulator(
                Handle, presentation.Id, options.AdjustPosition ? 1 : 0,
                options.AdjustSize ? 1 : 0, 0, out long id),
            "viewer_manipulator_attach");
        ViewerManipulator manipulator = new(this, presentation, id);
        _manipulators.Add(id, manipulator);
        try
        {
            SetManipulatorAppearance(manipulator, options.Size, options.Gap, options.Skin);
            SetManipulatorActivationOnDetection(manipulator, options.ActivationOnDetection);
            SetManipulatorZoomPersistence(manipulator, options.ZoomPersistence);
            if (options.Position is GpAx2Value position) SetManipulatorPosition(manipulator, position);
            if (options.EnableModesOnAttach)
                foreach ((ViewerManipulatorModes flag, ViewerManipulatorMode mode) in ManipulatorModes)
                    if ((options.EnabledModes & flag) != 0) EnableManipulatorMode(manipulator, mode);
            return manipulator;
        }
        catch
        {
            RemoveManipulator(manipulator);
            throw;
        }
    }

    internal GpTrsf GetTransform(ViewerPresentation presentation)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.GetViewerPresentationTransform(Handle, presentation.Id, out nint transform),
            "viewer_presentation_get_transform");
        return GpTrsf.FromNativeHandle(transform);
    }

    internal void SetTransform(ViewerPresentation presentation, GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        transform.ThrowIfDisposedForLocation();
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerPresentationTransform(Handle, presentation.Id, transform.Handle),
            "viewer_presentation_set_transform");
    }

    internal void ResetTransform(ViewerPresentation presentation)
    {
        EnsurePresentation(presentation);
        NativeError.ThrowIfFailed(
            NativeMethods.ResetViewerPresentationTransform(Handle, presentation.Id),
            "viewer_presentation_reset_transform");
    }

    internal void SetManipulatorPart(
        ViewerManipulator manipulator, ViewerManipulatorAxis axis, ViewerManipulatorMode mode, bool enabled)
    {
        if (!Enum.IsDefined(axis)) throw new ArgumentOutOfRangeException(nameof(axis));
        ValidateManipulatorMode(mode);
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerManipulatorPart(Handle, manipulator.Id, (int)axis, (int)mode, enabled ? 1 : 0),
            "viewer_manipulator_set_part");
    }

    internal void EnableManipulatorMode(ViewerManipulator manipulator, ViewerManipulatorMode mode)
    {
        ValidateManipulatorMode(mode);
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.EnableViewerManipulatorMode(Handle, manipulator.Id, (int)mode),
            "viewer_manipulator_enable_mode");
    }

    internal void SetManipulatorActivationOnDetection(ViewerManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerManipulatorActivationOnDetection(Handle, manipulator.Id, enabled ? 1 : 0),
            "viewer_manipulator_set_activation_on_detection");
    }

    internal void SetManipulatorPosition(ViewerManipulator manipulator, GpAx2Value position)
    {
        EnsureManipulator(manipulator);
        Ax2Raw raw = ToRaw(position);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerManipulatorPosition(Handle, manipulator.Id, in raw),
            "viewer_manipulator_set_position");
    }

    internal void SetManipulatorAppearance(
        ViewerManipulator manipulator, double size, double gap, ViewerManipulatorSkin skin)
    {
        if (!Enum.IsDefined(skin)) throw new ArgumentOutOfRangeException(nameof(skin));
        if (!double.IsFinite(size) || size is <= 0 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(size));
        if (!double.IsFinite(gap) || gap < 0 || gap > size)
            throw new ArgumentOutOfRangeException(nameof(gap));
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerManipulatorAppearance(Handle, manipulator.Id, size, gap, (int)skin),
            "viewer_manipulator_set_appearance");
    }

    internal void SetManipulatorZoomPersistence(ViewerManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.SetViewerManipulatorZoomPersistence(Handle, manipulator.Id, enabled ? 1 : 0),
            "viewer_manipulator_set_zoom_persistence");
    }

    internal void StartManipulator(ViewerManipulator manipulator, int x, int y)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.StartViewerManipulator(Handle, manipulator.Id, x, y), "viewer_manipulator_start");
    }

    internal GpTrsf TransformManipulator(ViewerManipulator manipulator, int x, int y)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.TransformViewerManipulatorMouse(Handle, manipulator.Id, x, y, out nint transform),
            "viewer_manipulator_transform_mouse");
        return GpTrsf.FromNativeHandle(transform);
    }

    internal void PreviewManipulator(ViewerManipulator manipulator, GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        transform.ThrowIfDisposedForLocation();
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.TransformViewerManipulatorCustom(Handle, manipulator.Id, transform.Handle),
            "viewer_manipulator_transform_custom");
    }

    internal void StopManipulator(ViewerManipulator manipulator, bool apply)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.StopViewerManipulator(Handle, manipulator.Id, apply ? 1 : 0),
            "viewer_manipulator_stop");
    }

    internal ViewerManipulatorState GetManipulatorState(ViewerManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.GetViewerManipulatorState(Handle, manipulator.Id, out ViewerManipulatorStateRaw state),
            "viewer_manipulator_get_state");
        return new ViewerManipulatorState(
            state.Attached != 0, (ViewerManipulatorMode)state.ActiveMode, state.ActiveAxis,
            state.HasActiveTransformation != 0, state.ActivationOnDetection != 0,
            state.ZoomPersistence != 0, (ViewerManipulatorSkin)state.Skin,
            state.Size, state.Gap, FromRaw(state.Position));
    }

    internal void RemoveManipulator(ViewerManipulator manipulator)
    {
        ArgumentNullException.ThrowIfNull(manipulator);
        if (manipulator.IsDetached) return;
        EnsureManipulator(manipulator);
        NativeError.ThrowIfFailed(
            NativeMethods.DetachViewerManipulator(Handle, manipulator.Id), "viewer_manipulator_detach");
        _manipulators.Remove(manipulator.Id);
        manipulator.MarkDetached();
    }

    internal bool IsDisposed => Handle.IsClosed || Handle.IsInvalid;

    /// <summary>Releases all presentations and native viewer resources on the owner thread.</summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        EnsureThread();
        foreach (ViewerPresentation presentation in _presentations.Values) presentation.MarkRemoved();
        foreach (ViewerClipPlane plane in _clipPlanes.Values) plane.MarkRemoved();
        foreach (ViewerDimension dimension in _dimensions.Values) dimension.MarkRemoved();
        foreach (ViewerManipulator manipulator in _manipulators.Values) manipulator.MarkDetached();
        _presentations.Clear();
        _clipPlanes.Clear();
        _dimensions.Clear();
        _manipulators.Clear();
        _savedViewClipPlanes.Clear();
        _isolationVisibility = null;
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

    private void EnsureClipPlane(ViewerClipPlane plane)
    {
        ArgumentNullException.ThrowIfNull(plane);
        EnsureThread();
        if (!ReferenceEquals(plane.Viewer, this))
            throw new ArgumentException("The clip plane belongs to another viewer.", nameof(plane));
        ObjectDisposedException.ThrowIf(plane.IsRemoved, plane);
    }

    private void EnsureDimension(ViewerDimension dimension)
    {
        ArgumentNullException.ThrowIfNull(dimension);
        EnsureThread();
        if (!ReferenceEquals(dimension.Viewer, this))
            throw new ArgumentException("The viewer dimension belongs to another viewer.", nameof(dimension));
        ObjectDisposedException.ThrowIf(dimension.IsRemoved || !_dimensions.ContainsKey(dimension.Id), dimension);
    }

    private void EnsureManipulator(ViewerManipulator manipulator)
    {
        ArgumentNullException.ThrowIfNull(manipulator);
        EnsureThread();
        if (!ReferenceEquals(manipulator.Viewer, this))
            throw new ArgumentException("The manipulator belongs to another viewer.", nameof(manipulator));
        ObjectDisposedException.ThrowIf(
            manipulator.IsDetached || !_manipulators.ContainsKey(manipulator.Id), manipulator);
    }

    private static readonly (ViewerManipulatorModes Flag, ViewerManipulatorMode Mode)[] ManipulatorModes =
    [
        (ViewerManipulatorModes.Translation, ViewerManipulatorMode.Translation),
        (ViewerManipulatorModes.Rotation, ViewerManipulatorMode.Rotation),
        (ViewerManipulatorModes.Scaling, ViewerManipulatorMode.Scaling),
        (ViewerManipulatorModes.TranslationPlane, ViewerManipulatorMode.TranslationPlane)
    ];

    private static void ValidateManipulatorMode(ViewerManipulatorMode mode)
    {
        if (mode is < ViewerManipulatorMode.Translation or > ViewerManipulatorMode.TranslationPlane)
            throw new ArgumentOutOfRangeException(nameof(mode));
    }

    private static Ax2Raw ToRaw(GpAx2Value value) => new(
        ToRaw(value.Origin), ToRaw(value.XDirection), ToRaw(value.YDirection), ToRaw(value.Direction));

    private static GpAx2Value FromRaw(Ax2Raw value) => new(
        ToXyz(value.Origin), ToXyz(value.XDirection), ToXyz(value.YDirection), ToXyz(value.Direction));

    private static GpPoint ToPoint(XyzRaw value) => new(value.X, value.Y, value.Z);
    private static GpXyz ToXyz(XyzRaw value) => new(value.X, value.Y, value.Z);
    private static XyzRaw ToRaw(GpPoint value) => new(value.X, value.Y, value.Z);
    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);

    private static void ValidateFinite(GpPoint value, string parameterName)
    {
        if (!double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Z))
            throw new ArgumentOutOfRangeException(parameterName, "Point coordinates must be finite.");
    }

    private static void ValidateFinite(GpXyz value, string parameterName)
    {
        if (!double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Z))
            throw new ArgumentOutOfRangeException(parameterName, "Vector coordinates must be finite.");
    }

    private static string CreateScreenshotStagingPath(string extension)
    {
        string systemTemp = Path.Combine(
            Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "Windows", "Temp");
        foreach (string candidate in new[] { Path.GetTempPath(), systemTemp, AppContext.BaseDirectory })
        {
            if (!Directory.Exists(candidate) || candidate.Any(character => character > 127)) continue;
            return Path.Combine(candidate, $"occtsharp-{Guid.NewGuid():N}{extension}");
        }
        throw new IOException("No writable ASCII staging directory is available for the OCCT screenshot bridge.");
    }

    internal void EnsureThread()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (Environment.CurrentManagedThreadId != _ownerThreadId)
        {
            throw new InvalidOperationException("Viewer operations must run on the thread that created the viewer.");
        }
    }
}
