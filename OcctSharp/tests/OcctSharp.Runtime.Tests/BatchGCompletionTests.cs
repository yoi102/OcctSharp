using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchGCompletionTests
{
    [Fact]
    public void ExactHiddenLineViewCopiesAllTenOwningLayers()
    {
        using Shape box = ShapeFactory.CreateBox(10, 8, 6);
        using DrawingView view = TechnicalDrawing.CreateView(
            box,
            DrawingProjection.Isometric,
            new DrawingOptions { Algorithm = DrawingAlgorithm.Exact, IsoparameterCount = 2 });

        Assert.Equal(10, view.Layers.Count);
        Assert.All(view.Layers, layer => Assert.Equal(ShapeKind.Compound, layer.Shape.Kind));
        Assert.NotEmpty(TechnicalDrawing.CopyPolylines(
            view.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape));
        Assert.NotEmpty(TechnicalDrawing.CopyPolylines(
            view.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Hidden).Shape));

        box.Dispose();
        Assert.NotEmpty(TechnicalDrawing.CopyPolylines(
            view.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape));
    }

    [Fact]
    public void PolygonalPerspectiveAndStandardViewsShareNoNativeAlgorithmLifetime()
    {
        using Shape box = ShapeFactory.CreateBox(10, 10, 10);
        using Shape cylinderSource = ShapeFactory.CreateCylinder(3, 14);
        using Shape cylinder = cylinderSource.Transformed(ShapeTransform.CreateTranslation(14, 0, 0));
        DrawingProjection perspective = new(new(30, -50, 25), new(-30, 50, -20), new(0, 0, 1), true, 60);
        DrawingOptions options = new() { Algorithm = DrawingAlgorithm.Polygonal, Deflection = 0.2 };
        using DrawingView view = TechnicalDrawing.CreateView([box, cylinder], perspective, options);
        Assert.NotEmpty(TechnicalDrawing.CopyPolylines(
            view.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape));

        using StandardDrawingViews standard = TechnicalDrawing.CreateStandardViews([box, cylinder]);
        Assert.Equal(4, standard.All.Count);
        Assert.All(standard.All, item => Assert.Equal(10, item.Layers.Count));

        Assert.Throws<ArgumentException>(() => TechnicalDrawing.CreateView(
            box, new DrawingProjection(GpXyz.Origin, new(0, 0, 1), new(0, 0, 2))));
    }

    [Fact]
    public void SectionPolylineAndLayeredSvgFormOneVectorOutputClosure()
    {
        using Shape box = ShapeFactory.CreateBox(20, 10, 8);
        using Shape section = TechnicalDrawing.CreateSection(box, GpPlane.Create(new(0, 0, 4), new(0, 0, 1)));
        Assert.Equal(4, section.CountSubShapes(ShapeKind.Edge));
        Assert.Equal(4, TechnicalDrawing.CopyPolylines(section, 8).Count);

        using DrawingView view = TechnicalDrawing.CreateView(box, DrawingProjection.Top,
            new DrawingOptions { IsoparameterCount = 1, SamplesPerCurve = 12 });
        string svg = view.ToSvg(new SvgDrawingOptions { Width = 640, Height = 480 });
        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("data-visibility=\"visible\"", svg, StringComparison.Ordinal);
        Assert.Contains("data-visibility=\"hidden\"", svg, StringComparison.Ordinal);
        Assert.Contains("stroke-dasharray", svg, StringComparison.Ordinal);

        string path = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchG.{Guid.NewGuid():N}.svg");
        try { view.SaveSvg(path); Assert.True(new FileInfo(path).Length > 100); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void StepXdeDrawingToRealHwndWorkflowProducesSvgAndScreenshot()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchG.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            using Shape authored = ShapeFactory.CreateBox(20, 12, 8);
            string step = Path.Combine(directory, "batch-g.step");
            using (XdeDocument document = XdeDocument.Create())
            {
                using XdeTransaction transaction = document.BeginTransaction();
                XdeLabel label = document.AddShape(authored, "Batch G Drawing Part");
                label.Color = new(0.2, 0.6, 0.85, 1);
                Assert.True(transaction.Commit());
                document.WriteStep(step);
            }
            using XdeDocument imported = XdeDocument.ReadStep(step);
            using Shape shape = Assert.Single(imported.GetFreeShapes()).Shape;
            using DrawingView drawing = TechnicalDrawing.CreateView(shape, DrawingProjection.Isometric,
                new DrawingOptions { IsoparameterCount = 1 });
            string svg = Path.Combine(directory, "batch-g.svg");
            drawing.SaveSvg(svg);
            Assert.True(new FileInfo(svg).Length > 100);

            using OcctViewer viewer = OcctViewer.Create(window);
            using ViewerPresentation visible = viewer.Display(
                drawing.GetLayer(DrawingEdgeCategory.Sharp, DrawingVisibility.Visible).Shape);
            viewer.FitAll(); viewer.Redraw();
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-g.png"));
            Assert.True(new FileInfo(screenshot).Length > 0);
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch G", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window); _ = NativeWindowMethods.ShowWindow(window, 4); _ = NativeWindowMethods.UpdateWindow(window); return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool DestroyWindow(nint window);
    }
}
