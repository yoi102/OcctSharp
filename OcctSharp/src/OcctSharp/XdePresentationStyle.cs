namespace OcctSharp;

/// <summary>
/// Owns one independently copied, location-aware XDE presentation-style entry.
/// </summary>
public sealed class XdePresentationStyle : IDisposable
{
    internal XdePresentationStyle(
        Shape shape,
        bool isVisible,
        XdeColor? surfaceColor,
        XdeColor? curveColor,
        XdeColor? materialColor)
    {
        Shape = shape;
        IsVisible = isVisible;
        SurfaceColor = surfaceColor;
        CurveColor = curveColor;
        MaterialColor = materialColor;
    }

    /// <summary>Gets the independently owned and already located styled topology.</summary>
    public Shape Shape { get; }

    /// <summary>Gets whether XDE marks this topology as visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Gets the optional copied surface RGBA color.</summary>
    public XdeColor? SurfaceColor { get; }

    /// <summary>Gets the optional copied curve RGBA color.</summary>
    public XdeColor? CurveColor { get; }

    /// <summary>Gets the optional copied PBR base or common diffuse material color.</summary>
    public XdeColor? MaterialColor { get; }

    /// <summary>Gets a convenient shaded-view color using surface, material, then curve precedence.</summary>
    public XdeColor? EffectiveColor => SurfaceColor ?? MaterialColor ?? CurveColor;

    /// <summary>Releases the independently owned topology copy.</summary>
    public void Dispose()
    {
        Shape.Dispose();
        GC.SuppressFinalize(this);
    }
}
