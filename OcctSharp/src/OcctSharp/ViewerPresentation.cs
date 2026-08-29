namespace OcctSharp;

/// <summary>Represents a presentation ID parent-bound to an <see cref="OcctViewer"/>.</summary>
public sealed class ViewerPresentation : IDisposable
{
    internal ViewerPresentation(OcctViewer viewer, long id, ViewerSourceIdentity? sourceIdentity = null)
    {
        Viewer = viewer;
        Id = id;
        SourceIdentity = sourceIdentity;
    }

    internal OcctViewer Viewer { get; }
    internal long Id { get; }
    internal bool IsRemoved { get; private set; }
    internal bool IsVisible { get; private set; } = true;

    /// <summary>Gets copied XDE occurrence identity, or null for an ordinary shape display.</summary>
    public ViewerSourceIdentity? SourceIdentity { get; }

    /// <summary>Shows this presentation without forcing a redraw.</summary>
    public void Show() => Viewer.SetVisible(this, true);

    /// <summary>Hides this presentation without forcing a redraw.</summary>
    public void Hide() => Viewer.SetVisible(this, false);

    /// <summary>Sets this presentation's linear RGB color without forcing a redraw.</summary>
    public void SetColor(ViewerColor color) => Viewer.SetColor(this, color);

    /// <summary>Sets transparency from 0 (opaque) to 1 (fully transparent).</summary>
    public void SetTransparency(double transparency) => Viewer.SetTransparency(this, transparency);

    /// <summary>Sets wireframe or shaded display for this presentation.</summary>
    public void SetDisplayMode(ViewerDisplayMode displayMode) => Viewer.SetDisplayMode(this, displayMode);

    /// <summary>
    /// Activates whole-object selection when null, or one topology-kind selection mode.
    /// </summary>
    public void SetSelectionKind(ShapeKind? kind) => Viewer.SetSelectionKind(this, kind);

    /// <summary>Sets one owned subshape's linear RGB review override.</summary>
    public void SetSubshapeColor(Shape subshape, ViewerColor color) => Viewer.SetSubshapeColor(this, subshape, color);

    /// <summary>Sets one owned subshape's transparency review override.</summary>
    public void SetSubshapeTransparency(Shape subshape, double transparency) =>
        Viewer.SetSubshapeTransparency(this, subshape, transparency);

    /// <summary>Sets one owned subshape's line-width review override.</summary>
    public void SetSubshapeWidth(Shape subshape, double width) => Viewer.SetSubshapeWidth(this, subshape, width);

    /// <summary>Clears all review overrides registered for one subshape.</summary>
    public void ClearSubshapeOverrides(Shape subshape) => Viewer.ClearSubshapeOverrides(this, subshape);

    /// <summary>Clears every subshape review override on this presentation.</summary>
    public void ClearAllSubshapeOverrides() => Viewer.ClearAllSubshapeOverrides(this);

    /// <summary>Removes the presentation from its parent viewer.</summary>
    public void Dispose() => Viewer.Remove(this);

    internal void MarkRemoved() => IsRemoved = true;
    internal void MarkVisible(bool visible) => IsVisible = visible;
}
