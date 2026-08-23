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

    /// <summary>Removes the presentation from its parent viewer.</summary>
    public void Dispose() => Viewer.Remove(this);

    internal void MarkRemoved() => IsRemoved = true;
}
