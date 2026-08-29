namespace OcctSharp;

/// <summary>Linear RGB presentation color with components in the inclusive range 0 to 1.</summary>
public readonly record struct ViewerColor(double Red, double Green, double Blue)
{
    internal void Validate()
    {
        if (!double.IsFinite(Red) || !double.IsFinite(Green) || !double.IsFinite(Blue)
            || Red is < 0 or > 1 || Green is < 0 or > 1 || Blue is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(ViewerColor), "RGB components must be finite values from 0 to 1.");
    }
}

/// <summary>Copied XDE identity attached to a viewer presentation.</summary>
public sealed record ViewerSourceIdentity
{
    /// <summary>Creates an identity independent from its source XDE document.</summary>
    public ViewerSourceIdentity(
        IEnumerable<string> occurrencePath,
        string occurrenceEntry,
        string referredEntry)
    {
        ArgumentNullException.ThrowIfNull(occurrencePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(occurrenceEntry);
        ArgumentException.ThrowIfNullOrWhiteSpace(referredEntry);
        OccurrencePath = Array.AsReadOnly([.. occurrencePath]);
        OccurrenceEntry = occurrenceEntry;
        ReferredEntry = referredEntry;
    }

    /// <summary>Gets the copied root-to-leaf occurrence-entry path.</summary>
    public IReadOnlyList<string> OccurrencePath { get; }
    /// <summary>Gets the copied occurrence label entry.</summary>
    public string OccurrenceEntry { get; }
    /// <summary>Gets the copied referred-definition label entry.</summary>
    public string ReferredEntry { get; }
}

/// <summary>One immutable copied camera state.</summary>
public readonly record struct ViewerCameraState(
    GpPoint Eye,
    GpPoint Target,
    GpXyz Up,
    GpXyz Projection);

/// <summary>A world-space ray produced from one client pixel.</summary>
public readonly record struct ViewerPickRay(GpPoint Origin, GpXyz Direction);

/// <summary>Integer client-pixel coordinates.</summary>
public readonly record struct ViewerPixelPoint(int X, int Y);

/// <summary>Cartesian clip-plane equation A*X + B*Y + C*Z + D = 0.</summary>
public readonly record struct ViewerPlaneEquation(double A, double B, double C, double D)
{
    internal void Validate()
    {
        if (!double.IsFinite(A) || !double.IsFinite(B) || !double.IsFinite(C) || !double.IsFinite(D)
            || A * A + B * B + C * C <= 1e-24)
            throw new ArgumentOutOfRangeException(nameof(ViewerPlaneEquation),
                "Plane coefficients must be finite and the normal must be non-zero.");
    }
}

/// <summary>Viewer buffer written by a screenshot operation.</summary>
public enum ViewerScreenshotBuffer
{
    /// <summary>RGB color without alpha.</summary>
    Rgb = 0,
    /// <summary>RGBA color.</summary>
    Rgba = 1,
    /// <summary>Depth buffer.</summary>
    Depth = 2
}

/// <summary>Standard orientation-trihedron placement.</summary>
public enum ViewerTrihedronPosition
{
    /// <summary>View center.</summary>
    Center = 0,
    /// <summary>Top center.</summary>
    Top = 1,
    /// <summary>Bottom center.</summary>
    Bottom = 2,
    /// <summary>Left center.</summary>
    Left = 4,
    /// <summary>Upper-left corner.</summary>
    LeftUpper = 5,
    /// <summary>Lower-left corner.</summary>
    LeftLower = 6,
    /// <summary>Right center.</summary>
    Right = 8,
    /// <summary>Upper-right corner.</summary>
    RightUpper = 9,
    /// <summary>Lower-right corner.</summary>
    RightLower = 10
}

/// <summary>Common AIS shape presentation modes.</summary>
public enum ViewerDisplayMode
{
    /// <summary>Draws edges without shaded faces.</summary>
    Wireframe = 0,
    /// <summary>Draws shaded faces.</summary>
    Shaded = 1
}

/// <summary>Standard Z-up camera projections.</summary>
public enum ViewerProjection
{
    /// <summary>Z-up front view.</summary>
    Front = 0,
    /// <summary>Z-up back view.</summary>
    Back = 1,
    /// <summary>Z-up top view.</summary>
    Top = 2,
    /// <summary>Z-up bottom view.</summary>
    Bottom = 3,
    /// <summary>Z-up left view.</summary>
    Left = 4,
    /// <summary>Z-up right view.</summary>
    Right = 5,
    /// <summary>Z-up right axonometric view.</summary>
    Axonometric = 6
}

/// <summary>How detected objects change the current selection.</summary>
public enum ViewerSelectionMode
{
    /// <summary>Replaces the current selection.</summary>
    Replace = 0,
    /// <summary>Adds detected objects.</summary>
    Add = 1,
    /// <summary>Removes detected objects.</summary>
    Remove = 2,
    /// <summary>Toggles detected objects.</summary>
    Toggle = 3
}

/// <summary>Application pointer buttons forwarded to the viewer input controller.</summary>
public enum ViewerPointerButton
{
    /// <summary>The primary pointer button.</summary>
    Left = 0,
    /// <summary>The middle pointer button, commonly the wheel button.</summary>
    Middle = 1,
    /// <summary>The secondary pointer button.</summary>
    Right = 2
}

/// <summary>Application pointer-button state used during pointer movement.</summary>
[Flags]
public enum ViewerPointerButtons
{
    /// <summary>No pointer button is pressed.</summary>
    None = 0,
    /// <summary>The primary pointer button is pressed.</summary>
    Left = 1,
    /// <summary>The middle pointer button is pressed.</summary>
    Middle = 2,
    /// <summary>The secondary pointer button is pressed.</summary>
    Right = 4
}

/// <summary>Application modifier keys used to select replace/add/remove/toggle behavior.</summary>
[Flags]
public enum ViewerModifierKeys
{
    /// <summary>No modifier key is pressed.</summary>
    None = 0,
    /// <summary>The Shift key is pressed.</summary>
    Shift = 1,
    /// <summary>The Control key is pressed.</summary>
    Control = 2,
    /// <summary>The Alt key is pressed.</summary>
    Alt = 4
}

/// <summary>Semantic keyboard commands accepted by the viewer input controller.</summary>
public enum ViewerInputKey
{
    /// <summary>Clears the current interactive selection.</summary>
    Escape = 0,
    /// <summary>Fits all displayed presentations in the view.</summary>
    FitAll = 1,
    /// <summary>Switches to the front projection.</summary>
    Front = 2,
    /// <summary>Switches to the back projection.</summary>
    Back = 3,
    /// <summary>Switches to the top projection.</summary>
    Top = 4,
    /// <summary>Switches to the bottom projection.</summary>
    Bottom = 5,
    /// <summary>Switches to the left projection.</summary>
    Left = 6,
    /// <summary>Switches to the right projection.</summary>
    Right = 7,
    /// <summary>Switches to the standard axonometric projection.</summary>
    Axonometric = 8
}
