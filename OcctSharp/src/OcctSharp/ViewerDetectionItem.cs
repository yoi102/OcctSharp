namespace OcctSharp;

/// <summary>Pairs a detected presentation with an independently owned exact topology copy.</summary>
public sealed class ViewerDetectionItem : IDisposable
{
    internal ViewerDetectionItem(ViewerPresentation presentation, Shape shape)
    {
        Presentation = presentation;
        Shape = shape;
    }

    /// <summary>Gets the detected parent-bound presentation.</summary>
    public ViewerPresentation Presentation { get; }
    /// <summary>Gets copied XDE identity, when this presentation came from an occurrence.</summary>
    public ViewerSourceIdentity? SourceIdentity => Presentation.SourceIdentity;
    /// <summary>Gets the exact detected whole shape or subshape as an independent owner.</summary>
    public Shape Shape { get; }
    /// <summary>Releases only the copied topology.</summary>
    public void Dispose() => Shape.Dispose();
}
