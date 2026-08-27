namespace OcctSharp;

/// <summary>Represents a presentation ID parent-bound to an <see cref="OcctViewer"/>.</summary>
public sealed class ViewerPresentation : IDisposable
{
    internal ViewerPresentation(OcctViewer viewer, long id)
    {
        Viewer = viewer;
        Id = id;
    }

    internal OcctViewer Viewer { get; }
    internal long Id { get; }
    internal bool IsRemoved { get; private set; }

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

    /// <summary>Removes the presentation from its parent viewer.</summary>
    public void Dispose() => Viewer.Remove(this);

    internal void MarkRemoved() => IsRemoved = true;
}
