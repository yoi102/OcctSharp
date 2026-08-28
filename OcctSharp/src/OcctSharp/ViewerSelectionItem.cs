namespace OcctSharp;

/// <summary>Pairs a parent-bound presentation with an independently owned selected topology copy.</summary>
public sealed class ViewerSelectionItem : IDisposable
{
    internal ViewerSelectionItem(ViewerPresentation presentation, Shape shape)
    {
        Presentation = presentation;
        Shape = shape;
    }

    /// <summary>Gets the presentation that owns the native selection mode.</summary>
    public ViewerPresentation Presentation { get; }

    /// <summary>Gets the independently owned selected whole shape or subshape.</summary>
    public Shape Shape { get; }

    /// <summary>Releases the copied selected topology. The presentation is not removed.</summary>
    public void Dispose() => Shape.Dispose();
}
