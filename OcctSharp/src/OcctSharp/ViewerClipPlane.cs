namespace OcctSharp;

/// <summary>Represents a clip-plane ID parent-bound to one viewer.</summary>
public sealed class ViewerClipPlane : IDisposable
{
    internal ViewerClipPlane(OcctViewer viewer, long id, ViewerPlaneEquation equation)
    {
        Viewer = viewer;
        Id = id;
        Equation = equation;
    }

    internal OcctViewer Viewer { get; }
    internal long Id { get; }
    internal bool IsRemoved { get; private set; }

    /// <summary>Gets the last successfully applied copied equation.</summary>
    public ViewerPlaneEquation Equation { get; private set; }
    /// <summary>Gets whether this plane is enabled.</summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>Atomically replaces the equation.</summary>
    public void Update(ViewerPlaneEquation equation) => Viewer.UpdateClipPlane(this, equation);
    /// <summary>Enables or disables clipping without removing the plane.</summary>
    public void SetEnabled(bool enabled) => Viewer.SetClipPlaneEnabled(this, enabled);
    /// <summary>Removes this plane from its parent viewer.</summary>
    public void Dispose() => Viewer.RemoveClipPlane(this);

    internal void MarkUpdated(ViewerPlaneEquation equation) => Equation = equation;
    internal void MarkEnabled(bool enabled) => IsEnabled = enabled;
    internal void MarkRemoved() => IsRemoved = true;
}
