namespace OcctSharp;

/// <summary>
/// Parent-bound state machine for forwarding application mouse, wheel, and semantic
/// keyboard input without exposing AIS or V3d pointers or installing reverse callbacks.
/// </summary>
public sealed class ViewerInputController
{
    private readonly OcctViewer viewer;
    private ViewerPointerButton? pressedButton;
    private int pressX;
    private int pressY;
    private int lastX;
    private int lastY;
    private bool dragged;

    internal ViewerInputController(OcctViewer viewer) => this.viewer = viewer;

    /// <summary>Begins a pointer gesture in client-pixel coordinates.</summary>
    public void PointerPressed(
        ViewerPointerButton button,
        int x,
        int y,
        ViewerModifierKeys modifiers = ViewerModifierKeys.None)
    {
        ValidateButton(button);
        ValidateModifiers(modifiers);
        pressedButton = button;
        pressX = lastX = x;
        pressY = lastY = y;
        dragged = false;
        if (button == ViewerPointerButton.Right) viewer.StartRotation(x, y);
        else viewer.MoveTo(x, y);
    }

    /// <summary>Continues detection, pan, or rotation using current application button state.</summary>
    public bool PointerMoved(
        int x,
        int y,
        ViewerPointerButtons buttons = ViewerPointerButtons.None,
        ViewerModifierKeys modifiers = ViewerModifierKeys.None)
    {
        ValidateButtons(buttons);
        ValidateModifiers(modifiers);
        if (Math.Abs(x - pressX) > 2 || Math.Abs(y - pressY) > 2) dragged = true;
        if ((buttons & ViewerPointerButtons.Right) != 0)
        {
            viewer.Rotate(x, y);
            lastX = x;
            lastY = y;
            return false;
        }
        if ((buttons & ViewerPointerButtons.Middle) != 0)
        {
            viewer.Pan(x - lastX, lastY - y);
            lastX = x;
            lastY = y;
            return false;
        }
        lastX = x;
        lastY = y;
        return viewer.MoveTo(x, y);
    }

    /// <summary>Ends a pointer gesture; an undragged left release performs selection.</summary>
    public IReadOnlyList<ViewerPresentation> PointerReleased(
        ViewerPointerButton button,
        int x,
        int y,
        ViewerModifierKeys modifiers = ViewerModifierKeys.None)
    {
        ValidateButton(button);
        ValidateModifiers(modifiers);
        bool shouldSelect = pressedButton == ViewerPointerButton.Left
            && button == ViewerPointerButton.Left
            && !dragged;
        pressedButton = null;
        lastX = x;
        lastY = y;
        if (!shouldSelect)
        {
            viewer.MoveTo(x, y);
            return viewer.GetSelection();
        }
        return viewer.SelectAt(x, y, SelectionMode(modifiers));
    }

    /// <summary>Applies a conventional 120-unit wheel notch zoom at the current view.</summary>
    public void MouseWheel(int delta, int x, int y, ViewerModifierKeys modifiers = ViewerModifierKeys.None)
    {
        ValidateModifiers(modifiers);
        if (delta == 0) return;
        viewer.MoveTo(x, y);
        viewer.Zoom(Math.Pow(1.1, delta / 120.0));
    }

    /// <summary>Forwards one semantic keyboard command and returns whether it was handled.</summary>
    public bool KeyDown(ViewerInputKey key, ViewerModifierKeys modifiers = ViewerModifierKeys.None)
    {
        if (!Enum.IsDefined(key)) throw new ArgumentOutOfRangeException(nameof(key));
        ValidateModifiers(modifiers);
        switch (key)
        {
            case ViewerInputKey.Escape: viewer.ClearSelection(); break;
            case ViewerInputKey.FitAll: viewer.FitAll(); break;
            case ViewerInputKey.Front: viewer.SetProjection(ViewerProjection.Front); break;
            case ViewerInputKey.Back: viewer.SetProjection(ViewerProjection.Back); break;
            case ViewerInputKey.Top: viewer.SetProjection(ViewerProjection.Top); break;
            case ViewerInputKey.Bottom: viewer.SetProjection(ViewerProjection.Bottom); break;
            case ViewerInputKey.Left: viewer.SetProjection(ViewerProjection.Left); break;
            case ViewerInputKey.Right: viewer.SetProjection(ViewerProjection.Right); break;
            case ViewerInputKey.Axonometric: viewer.SetProjection(ViewerProjection.Axonometric); break;
            default: return false;
        }
        return true;
    }

    private static ViewerSelectionMode SelectionMode(ViewerModifierKeys modifiers) =>
        (modifiers & ViewerModifierKeys.Control) != 0 ? ViewerSelectionMode.Toggle
        : (modifiers & ViewerModifierKeys.Alt) != 0 ? ViewerSelectionMode.Remove
        : (modifiers & ViewerModifierKeys.Shift) != 0 ? ViewerSelectionMode.Add
        : ViewerSelectionMode.Replace;

    private static void ValidateButton(ViewerPointerButton button)
    {
        if (!Enum.IsDefined(button)) throw new ArgumentOutOfRangeException(nameof(button));
    }

    private static void ValidateButtons(ViewerPointerButtons buttons)
    {
        if ((buttons & ~(ViewerPointerButtons.Left | ViewerPointerButtons.Middle | ViewerPointerButtons.Right)) != 0)
            throw new ArgumentOutOfRangeException(nameof(buttons));
    }

    private static void ValidateModifiers(ViewerModifierKeys modifiers)
    {
        if ((modifiers & ~(ViewerModifierKeys.Shift | ViewerModifierKeys.Control | ViewerModifierKeys.Alt)) != 0)
            throw new ArgumentOutOfRangeException(nameof(modifiers));
    }
}
