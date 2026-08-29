namespace OcctSharp;

/// <summary>Supported viewer-owned review dimension families.</summary>
public enum ViewerDimensionKind
{
    /// <summary>Linear distance between two points.</summary>
    Length = 0,
    /// <summary>Angle formed by three points.</summary>
    Angle = 1,
    /// <summary>Radius of circular, cylindrical, or conical topology.</summary>
    Radius = 2,
    /// <summary>Diameter of circular, cylindrical, or conical topology.</summary>
    Diameter = 3
}

/// <summary>Copied units and display style applied to one viewer-owned dimension.</summary>
public sealed record ViewerDimensionStyle
{
    /// <summary>Explicit model/display units used by the dimension.</summary>
    public InspectionUnits Units { get; init; } = new();
    /// <summary>Linear RGB annotation color.</summary>
    public ViewerColor Color { get; init; } = new(1, 0.85, 0.1);
    /// <summary>Signed dimension flyout distance in model units.</summary>
    public double Flyout { get; init; } = 10;
    /// <summary>Dimension line width.</summary>
    public double LineWidth { get; init; } = 1.5;
    /// <summary>Optional value overriding OCCT's computed measurement.</summary>
    public double? CustomValue { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Units);
        Color.Validate();
        if (!double.IsFinite(Flyout)) throw new ArgumentOutOfRangeException(nameof(Flyout));
        if (!double.IsFinite(LineWidth) || LineWidth <= 0) throw new ArgumentOutOfRangeException(nameof(LineWidth));
        if (CustomValue is double value && !double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(CustomValue));
    }
}

/// <summary>Parent-bound ID for a native PrsDim annotation owned by one viewer.</summary>
public sealed class ViewerDimension : IDisposable
{
    internal ViewerDimension(OcctViewer viewer, long id, ViewerDimensionKind kind, ViewerDimensionStyle style)
    {
        Viewer = viewer;
        Id = id;
        Kind = kind;
        Style = style;
    }

    internal OcctViewer Viewer { get; }
    internal long Id { get; }
    internal bool IsRemoved { get; private set; }
    internal bool IsVisible { get; private set; } = true;

    /// <summary>Gets the dimension family.</summary>
    public ViewerDimensionKind Kind { get; }
    /// <summary>Gets the last copied style submitted to the viewer.</summary>
    public ViewerDimensionStyle Style { get; private set; }

    /// <summary>Replaces copied units and visual style, then redisplays the annotation.</summary>
    public void UpdateStyle(ViewerDimensionStyle style) => Viewer.UpdateDimensionStyle(this, style);
    /// <summary>Shows the annotation without removing its parent-bound ID.</summary>
    public void Show() => Viewer.SetDimensionVisible(this, true);
    /// <summary>Hides the annotation without removing its parent-bound ID.</summary>
    public void Hide() => Viewer.SetDimensionVisible(this, false);
    /// <summary>Selects or deselects this annotation in its parent viewer.</summary>
    public void SetSelected(bool selected = true) => Viewer.SetDimensionSelected(this, selected);
    /// <summary>Removes the native annotation from its parent viewer.</summary>
    public void Dispose() => Viewer.RemoveDimension(this);

    internal void MarkRemoved() => IsRemoved = true;
    internal void MarkVisible(bool visible) => IsVisible = visible;
    internal void MarkStyle(ViewerDimensionStyle style) => Style = style;
}
