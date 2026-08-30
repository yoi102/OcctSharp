using System.Globalization;
using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public static class TechnicalDrawing
{
    private const int LayerCount = 10;

    public static DrawingView CreateView(
        Shape shape,
        DrawingProjection projection,
        DrawingOptions? options = null) => CreateView([shape], projection, options);

    public static unsafe DrawingView CreateView(
        IReadOnlyList<Shape> shapes,
        DrawingProjection projection,
        DrawingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        if (shapes.Count == 0) throw new ArgumentException("A drawing view requires at least one shape.", nameof(shapes));
        DrawingOptions actual = Validate(options ?? new DrawingOptions());
        DrawingProjectionRaw rawProjection = ToRaw(projection);
        nint[] layerHandles = new nint[LayerCount];
        bool[] references = new bool[shapes.Count];
        nint[] shapeHandles = new nint[shapes.Count];
        int acquired = 0;
        try
        {
            for (; acquired < shapes.Count; ++acquired)
            {
                Shape shape = shapes[acquired] ?? throw new ArgumentException("A drawing shape collection contains null.", nameof(shapes));
                ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
                shape.Handle.DangerousAddRef(ref references[acquired]);
                shapeHandles[acquired] = shape.Handle.DangerousGetHandle();
            }
            fixed (nint* shapePointer = shapeHandles)
            fixed (nint* layerPointer = layerHandles)
            {
                NativeError.ThrowIfFailed(
                    NativeMethods.DrawingCompute(shapePointer, shapeHandles.Length, rawProjection,
                        actual.Algorithm == DrawingAlgorithm.Exact ? 1 : 0,
                        actual.IsoparameterCount, actual.Deflection, layerPointer, LayerCount),
                    "drawing_compute");
            }
            return OwnView(projection, actual, layerHandles);
        }
        catch
        {
            ReleaseUnownedHandles(layerHandles);
            throw;
        }
        finally
        {
            for (int index = acquired - 1; index >= 0; --index)
                if (references[index]) shapes[index].Handle.DangerousRelease();
        }
    }

    public static StandardDrawingViews CreateStandardViews(
        IReadOnlyList<Shape> shapes,
        DrawingOptions? options = null)
    {
        DrawingView? front = null, top = null, right = null, isometric = null;
        try
        {
            front = CreateView(shapes, DrawingProjection.Front, options);
            top = CreateView(shapes, DrawingProjection.Top, options);
            right = CreateView(shapes, DrawingProjection.Right, options);
            isometric = CreateView(shapes, DrawingProjection.Isometric, options);
            StandardDrawingViews result = new(front, top, right, isometric);
            front = top = right = isometric = null;
            return result;
        }
        finally
        {
            front?.Dispose(); top?.Dispose(); right?.Dispose(); isometric?.Dispose();
        }
    }

    public static Shape CreateSection(Shape shape, GpPlane plane, bool approximate = true)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
        NativeError.ThrowIfFailed(
            NativeMethods.DrawingSection(shape.Handle, ToRaw(plane.Origin), ToRaw(plane.Normal), approximate ? 1 : 0, out nint section),
            "drawing_section");
        return ShapeFactory.FromNativeHandle(section, "drawing_section");
    }

    public static unsafe IReadOnlyList<DrawingPolyline> CopyPolylines(Shape shape, int samplesPerCurve = 24)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
        NativeError.ThrowIfFailed(
            NativeMethods.DrawingPolylineCount(shape.Handle, samplesPerCurve, out int polylineCount, out int pointCount),
            "drawing_polyline_count");
        DrawingPolylineRaw[] rawPolylines = new DrawingPolylineRaw[polylineCount];
        XyzRaw[] rawPoints = new XyzRaw[pointCount];
        fixed (DrawingPolylineRaw* polylinePointer = rawPolylines)
        fixed (XyzRaw* pointPointer = rawPoints)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.DrawingPolylineCopy(shape.Handle, samplesPerCurve,
                    polylinePointer, rawPolylines.Length, pointPointer, rawPoints.Length,
                    out int polylinesWritten, out int pointsWritten),
                "drawing_polyline_copy");
            if (polylinesWritten != polylineCount || pointsWritten != pointCount)
                throw new OcctException(NativeStatus.UnknownException.ToString(), "Drawing polyline counts changed during extraction.");
        }
        DrawingPolyline[] result = new DrawingPolyline[rawPolylines.Length];
        for (int index = 0; index < result.Length; ++index)
        {
            DrawingPolylineRaw polyline = rawPolylines[index];
            GpPoint[] points = new GpPoint[polyline.PointCount];
            for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
            {
                XyzRaw point = rawPoints[polyline.PointOffset + pointIndex];
                points[pointIndex] = new(point.X, point.Y, point.Z);
            }
            result[index] = new(points, polyline.Closed != 0);
        }
        return result;
    }

    internal static string ToSvg(DrawingView view, SvgDrawingOptions options)
    {
        Validate(options);
        List<(DrawingLayer Layer, DrawingPolyline Polyline)> paths = [];
        foreach (DrawingLayer layer in view.Layers)
        {
            if (!options.IncludeHidden && layer.Visibility == DrawingVisibility.Hidden) continue;
            if (!options.IncludeIsoparameters && layer.Category == DrawingEdgeCategory.Isoparameter) continue;
            foreach (DrawingPolyline polyline in CopyPolylines(layer.Shape, view.Options.SamplesPerCurve))
                if (polyline.Points.Count >= 2) paths.Add((layer, polyline));
        }
        (double minX, double minY, double maxX, double maxY) = Bounds(paths);
        double contentWidth = Math.Max(maxX - minX, 1e-12);
        double contentHeight = Math.Max(maxY - minY, 1e-12);
        double scale = Math.Min((options.Width - 2 * options.Margin) / contentWidth,
                                (options.Height - 2 * options.Margin) / contentHeight);
        if (!double.IsFinite(scale) || scale <= 0) scale = 1;
        double offsetX = options.Margin + (options.Width - 2 * options.Margin - contentWidth * scale) / 2;
        double offsetY = options.Margin + (options.Height - 2 * options.Margin - contentHeight * scale) / 2;

        StringBuilder svg = new();
        svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"").Append(F(options.Width))
            .Append("\" height=\"").Append(F(options.Height)).Append("\" viewBox=\"0 0 ")
            .Append(F(options.Width)).Append(' ').Append(F(options.Height)).Append("\">\n")
            .Append("  <rect width=\"100%\" height=\"100%\" fill=\"").Append(Xml(options.BackgroundColor)).Append("\"/>\n");
        foreach ((DrawingLayer layer, DrawingPolyline polyline) in paths)
        {
            string color = layer.Visibility == DrawingVisibility.Hidden ? options.HiddenColor : options.VisibleColor;
            double width = layer.Visibility == DrawingVisibility.Hidden ? options.HiddenStrokeWidth : options.VisibleStrokeWidth;
            svg.Append("  <polyline data-category=\"").Append(layer.Category.ToString().ToLowerInvariant())
                .Append("\" data-visibility=\"").Append(layer.Visibility.ToString().ToLowerInvariant())
                .Append("\" fill=\"none\" stroke=\"").Append(Xml(color)).Append("\" stroke-width=\"").Append(F(width)).Append('"');
            if (layer.Visibility == DrawingVisibility.Hidden) svg.Append(" stroke-dasharray=\"6 4\"");
            svg.Append(" points=\"");
            foreach (GpPoint point in polyline.Points)
            {
                double x = offsetX + (point.X - minX) * scale;
                double y = options.Height - offsetY - (point.Y - minY) * scale;
                svg.Append(F(x)).Append(',').Append(F(y)).Append(' ');
            }
            svg.Append("\"/>\n");
        }
        return svg.Append("</svg>\n").ToString();
    }

    private static DrawingView OwnView(DrawingProjection projection, DrawingOptions options, nint[] handles)
    {
        List<DrawingLayer> layers = new(LayerCount);
        try
        {
            for (int index = 0; index < LayerCount; ++index)
            {
                DrawingEdgeCategory category = (DrawingEdgeCategory)(index / 2);
                DrawingVisibility visibility = (DrawingVisibility)(index % 2);
                layers.Add(new(category, visibility, ShapeFactory.FromNativeHandle(handles[index], "drawing_compute")));
                handles[index] = 0;
            }
            return new DrawingView(projection, options, layers);
        }
        catch
        {
            foreach (DrawingLayer layer in layers) layer.Dispose();
            throw;
        }
    }

    private static void ReleaseUnownedHandles(nint[] handles)
    {
        foreach (nint handle in handles)
        {
            if (handle == 0) continue;
            using Shape shape = ShapeFactory.FromNativeHandle(handle, "drawing_cleanup");
        }
    }

    private static DrawingOptions Validate(DrawingOptions options)
    {
        if (!Enum.IsDefined(options.Algorithm)) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.IsoparameterCount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(options));
        if (!double.IsFinite(options.Deflection) || options.Deflection <= 0) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.SamplesPerCurve is < 2 or > 4096) throw new ArgumentOutOfRangeException(nameof(options));
        return options;
    }

    private static void Validate(SvgDrawingOptions options)
    {
        if (!double.IsFinite(options.Width) || options.Width <= 0 || !double.IsFinite(options.Height) || options.Height <= 0
            || !double.IsFinite(options.Margin) || options.Margin < 0 || options.Margin * 2 >= Math.Min(options.Width, options.Height)
            || !double.IsFinite(options.VisibleStrokeWidth) || options.VisibleStrokeWidth <= 0
            || !double.IsFinite(options.HiddenStrokeWidth) || options.HiddenStrokeWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "SVG dimensions, margins, and stroke widths must be finite and usable.");
        ArgumentException.ThrowIfNullOrWhiteSpace(options.VisibleColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.HiddenColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BackgroundColor);
    }

    private static DrawingProjectionRaw ToRaw(DrawingProjection value)
    {
        ValidateVector(value.Origin, nameof(value.Origin), allowZero: true);
        ValidateVector(value.ViewDirection, nameof(value.ViewDirection), allowZero: false);
        ValidateVector(value.UpDirection, nameof(value.UpDirection), allowZero: false);
        double cx = value.UpDirection.Y * value.ViewDirection.Z - value.UpDirection.Z * value.ViewDirection.Y;
        double cy = value.UpDirection.Z * value.ViewDirection.X - value.UpDirection.X * value.ViewDirection.Z;
        double cz = value.UpDirection.X * value.ViewDirection.Y - value.UpDirection.Y * value.ViewDirection.X;
        if (cx * cx + cy * cy + cz * cz <= 1e-24)
            throw new ArgumentException("Drawing up and view directions must not be parallel.", nameof(value));
        if (!double.IsFinite(value.Focus) || value.Focus <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        return new(ToRaw(value.Origin), ToRaw(value.ViewDirection), ToRaw(value.UpDirection), value.Perspective ? 1 : 0, value.Focus);
    }

    private static void ValidateVector(GpXyz value, string name, bool allowZero)
    {
        if (!double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Z)
            || (!allowZero && value.X * value.X + value.Y * value.Y + value.Z * value.Z <= 1e-24))
            throw new ArgumentOutOfRangeException(name);
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) Bounds(
        List<(DrawingLayer Layer, DrawingPolyline Polyline)> paths)
    {
        if (paths.Count == 0) return (0, 0, 1, 1);
        double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;
        foreach ((_, DrawingPolyline polyline) in paths)
            foreach (GpPoint point in polyline.Points)
            {
                minX = Math.Min(minX, point.X); minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X); maxY = Math.Max(maxY, point.Y);
            }
        return (minX, minY, maxX, maxY);
    }

    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static string F(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);
    private static string Xml(string value) => System.Security.SecurityElement.Escape(value) ?? string.Empty;
}
#pragma warning restore CS1591
