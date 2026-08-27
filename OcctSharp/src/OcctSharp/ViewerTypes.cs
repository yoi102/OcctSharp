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
