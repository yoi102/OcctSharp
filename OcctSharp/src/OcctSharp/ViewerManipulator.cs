using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Controls one native-local AIS manipulator parent-bound to a viewer presentation.</summary>
public sealed class ViewerManipulator : IDisposable
{
    internal ViewerManipulator(OcctViewer viewer, ViewerPresentation presentation, long id)
    {
        Viewer = viewer;
        Presentation = presentation;
        Id = id;
    }

    internal OcctViewer Viewer { get; }
    internal ViewerPresentation Presentation { get; }
    internal long Id { get; }
    internal bool IsDetached { get; private set; }

    /// <summary>Gets a copied state snapshot.</summary>
    public ViewerManipulatorState State => Viewer.GetManipulatorState(this);

    /// <summary>Enables one interaction mode in the parent context.</summary>
    public void EnableMode(ViewerManipulatorMode mode) => Viewer.EnableManipulatorMode(this, mode);

    /// <summary>Shows or hides one axis/mode visual part.</summary>
    public void SetPart(ViewerManipulatorAxis axis, ViewerManipulatorMode mode, bool enabled) =>
        Viewer.SetManipulatorPart(this, axis, mode, enabled);

    /// <summary>Chooses whether detection activates a mode.</summary>
    public void SetActivationOnDetection(bool enabled) =>
        Viewer.SetManipulatorActivationOnDetection(this, enabled);

    /// <summary>Sets a copied finite manipulator position and orientation.</summary>
    public void SetPosition(GpAx2Value position) => Viewer.SetManipulatorPosition(this, position);

    /// <summary>Sets finite size, gap, and skin values.</summary>
    public void SetAppearance(double size, double gap, ViewerManipulatorSkin skin = ViewerManipulatorSkin.Shaded) =>
        Viewer.SetManipulatorAppearance(this, size, gap, skin);

    /// <summary>Enables or disables fixed-screen-size display.</summary>
    public void SetZoomPersistence(bool enabled) => Viewer.SetManipulatorZoomPersistence(this, enabled);

    /// <summary>Captures the transformation start state at client coordinates.</summary>
    public void Start(int x, int y) => Viewer.StartManipulator(this, x, y);

    /// <summary>Updates from client coordinates and returns an independent transformation.</summary>
    public GpTrsf Transform(int x, int y) => Viewer.TransformManipulator(this, x, y);

    /// <summary>Previews one caller-owned transformation after <see cref="Start"/>.</summary>
    public void Preview(GpTrsf transform) => Viewer.PreviewManipulator(this, transform);

    /// <summary>Applies or cancels the started transformation.</summary>
    public void Stop(bool apply = true) => Viewer.StopManipulator(this, apply);

    /// <summary>Detaches and removes this manipulator from its parent viewer.</summary>
    public void Dispose() => Viewer.RemoveManipulator(this);

    internal void MarkDetached() => IsDetached = true;
}
