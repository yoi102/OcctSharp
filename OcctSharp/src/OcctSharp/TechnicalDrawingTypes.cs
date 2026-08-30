namespace OcctSharp;

#pragma warning disable CS1591

public enum DrawingAlgorithm { Exact = 0, Polygonal = 1 }
public enum DrawingVisibility { Visible = 0, Hidden = 1 }
public enum DrawingEdgeCategory { Sharp = 0, Smooth = 1, Sewn = 2, Outline = 3, Isoparameter = 4 }

public readonly record struct DrawingProjection(
    GpXyz Origin,
    GpXyz ViewDirection,
    GpXyz UpDirection,
    bool Perspective = false,
    double Focus = 1000.0)
{
    public static DrawingProjection Front => new(GpXyz.Origin, new(0, -1, 0), new(0, 0, 1));
    public static DrawingProjection Top => new(GpXyz.Origin, new(0, 0, 1), new(0, 1, 0));
    public static DrawingProjection Right => new(GpXyz.Origin, new(1, 0, 0), new(0, 0, 1));
    public static DrawingProjection Isometric => new(GpXyz.Origin, new(1, -1, 1), new(0, 0, 1));
}

public sealed record DrawingOptions
{
    public DrawingAlgorithm Algorithm { get; init; } = DrawingAlgorithm.Exact;
    public int IsoparameterCount { get; init; }
    public double Deflection { get; init; } = 0.1;
    public int SamplesPerCurve { get; init; } = 24;
}

public sealed record SvgDrawingOptions
{
    public double Width { get; init; } = 1000;
    public double Height { get; init; } = 800;
    public double Margin { get; init; } = 24;
    public double VisibleStrokeWidth { get; init; } = 1.5;
    public double HiddenStrokeWidth { get; init; } = 1.0;
    public string VisibleColor { get; init; } = "#111827";
    public string HiddenColor { get; init; } = "#6b7280";
    public string BackgroundColor { get; init; } = "#ffffff";
    public bool IncludeHidden { get; init; } = true;
    public bool IncludeIsoparameters { get; init; } = true;
}

public sealed record DrawingPolyline(IReadOnlyList<GpPoint> Points, bool Closed);

public sealed class DrawingLayer : IDisposable
{
    internal DrawingLayer(DrawingEdgeCategory category, DrawingVisibility visibility, Shape shape)
        => (Category, Visibility, Shape) = (category, visibility, shape);

    public DrawingEdgeCategory Category { get; }
    public DrawingVisibility Visibility { get; }
    public Shape Shape { get; }
    public void Dispose() => Shape.Dispose();
}

public sealed class DrawingView : IDisposable
{
    private readonly IReadOnlyList<DrawingLayer> layers;
    private bool disposed;

    internal DrawingView(DrawingProjection projection, DrawingOptions options, IReadOnlyList<DrawingLayer> layers)
        => (Projection, Options, this.layers) = (projection, options, layers);

    public DrawingProjection Projection { get; }
    public DrawingOptions Options { get; }
    public IReadOnlyList<DrawingLayer> Layers => layers;

    public DrawingLayer GetLayer(DrawingEdgeCategory category, DrawingVisibility visibility)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return layers.Single(layer => layer.Category == category && layer.Visibility == visibility);
    }

    public string ToSvg(SvgDrawingOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return TechnicalDrawing.ToSvg(this, options ?? new SvgDrawingOptions());
    }

    public void SaveSvg(string path, SvgDrawingOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, ToSvg(options), new System.Text.UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (DrawingLayer layer in layers) layer.Dispose();
    }
}

public sealed class StandardDrawingViews : IDisposable
{
    internal StandardDrawingViews(DrawingView front, DrawingView top, DrawingView right, DrawingView isometric)
        => (Front, Top, Right, Isometric) = (front, top, right, isometric);

    public DrawingView Front { get; }
    public DrawingView Top { get; }
    public DrawingView Right { get; }
    public DrawingView Isometric { get; }
    public IReadOnlyList<DrawingView> All => [Front, Top, Right, Isometric];
    public void Dispose() { Front.Dispose(); Top.Dispose(); Right.Dispose(); Isometric.Dispose(); }
}

#pragma warning restore CS1591
