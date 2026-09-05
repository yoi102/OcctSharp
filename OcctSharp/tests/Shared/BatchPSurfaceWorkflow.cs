using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Compiled by repository runtime tests and the isolated NuGet consumer. Only public
// APIs participate, so both routes prove the same cross-family dependency closure.
internal static class BatchPSurfaceWorkflow
{
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.BatchP." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using Shape support = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Cylinder,
                SketchPlane.XY, new(0, Math.Tau, 0, 8), 3);
            SketchCurve2d definition = SurfaceModeling.InterpolateUv([new(0.5, 1), new(1.5, 3), new(2.5, 6)]).Curve;
            using Shape incomplete = SurfaceModeling.LiftCurve(support, definition, build3d: false);
            using SurfaceRepairResult repaired = SurfaceModeling.Repair(incomplete, 1e-6);
            Require(repaired.Shape.IsValid && repaired.Diagnostics.MissingCurveCountAfter == 0, "pcurve repair");
            Require(SurfaceModeling.GetCurveDefinition(support, repaired.Shape).Curve.Degree > 1, "copied definition");
            SurfaceEvaluationPoint sample = SurfaceModeling.Evaluate(support, new(1, 4));
            Require(SurfaceModeling.ProjectPoint(support, sample.Point)[0].Distance < 1e-6, "point projection");
            using Shape trim = SurfaceModeling.CreateTrimmedFace(support,
                SketchProfile2d.Create(Rectangle(0.3, 1, 2.8, 7), [Rectangle(0.6, 2, 1.0, 3)]));
            Require(trim.IsValid && SurfaceModeling.GetBoundaryLoops(trim).Count == 2, "trimmed topology");
            using Shape section = SurfaceModeling.IntersectPlane(support,
                new(new(0, 0, 4), new(1, 0, 0), new(0, 1, 0)), new(-4, 4, -4, 4));
            Require(section.CountSubShapes(ShapeKind.Edge) > 0, "surface section");
            using Shape offSurface = ShapeFactory.CreateEdge(new(6, 0, 2), new(6, 0, 6));
            using Shape projected = SurfaceModeling.ProjectShape(support, offSurface);
            Require(projected.CountSubShapes(ShapeKind.Edge) > 0, "normal projection");
            XdePartMetadata metadata = new("Batch P surface trim", new XdeColor(0.2, 0.6, 0.8), ["Surface", "UV"]);
            string step = SketchProfile2d.WriteStep(trim, Path.Combine(directory, "surface.step"), metadata);
            string iges = SketchProfile2d.WriteIges(trim, Path.Combine(directory, "surface.iges"), metadata);
            support.Dispose(); incomplete.Dispose(); trim.Dispose();
            foreach (string path in new[] { step, iges })
            {
                using XdeDocument document = XdeDocument.ReadExchange(path);
                XdeLabel root = document.GetFreeShapes().Single();
                Require(root.Name?.Contains("Batch P", StringComparison.OrdinalIgnoreCase) == true, "exchange name");
                using Shape restored = root.Shape;
                Require(restored.IsValid && restored.FaceCount == 1, "exchange topology");
                IReadOnlyList<XdePresentationStyle> styles = root.GetPresentationStyles();
                try
                {
                    Require(styles.Any(style => style.EffectiveColor is { } color
                        && Math.Abs(color.Red - 0.2) < 0.02 && Math.Abs(color.Green - 0.6) < 0.02
                        && Math.Abs(color.Blue - 0.8) < 0.02), "exchange color");
                    using DocumentSnapshot snapshot = document.CreateSnapshot();
                    Require(snapshot.Labels.Any(label => document.GetLabel(label.Entry).Layers.Count > 0), "exchange layer");
                    nint window = CreateWindowEx(0, "STATIC", "Batch P surface review", 0x80000000u,
                        -32000, -32000, 320, 320, 0, 0, 0, 0);
                    Require(window != 0, "real HWND");
                    try
                    {
                        _ = ShowWindow(window, 4); _ = UpdateWindow(window);
                        using OcctViewer viewer = OcctViewer.Create(window);
                        using ViewerPresentation presentation = viewer.Display(root);
                        document.Dispose(); // Presentation owns the imported topology and styles.
                        viewer.FitAll(); viewer.Redraw();
                        bool selected = false;
                        for (int y = 40; y < 300 && !selected; y += 20)
                        for (int x = 40; x < 300 && !selected; x += 20)
                            selected = viewer.SelectAt(x, y, ViewerSelectionMode.Replace).Count > 0;
                        Require(selected, "real selection");
                        string screenshot = viewer.SaveScreenshot(Path.Combine(directory, Path.GetExtension(path) + ".png"));
                        Require(new FileInfo(screenshot).Length > 0, "real screenshot");
                    }
                    finally { Require(DestroyWindow(window), "HWND cleanup"); }
                }
                finally { foreach (XdePresentationStyle style in styles) style.Dispose(); }
                Require(restored.IsValid, "source document disposal");
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    private static SketchCurveChain2d Rectangle(double x0, double y0, double x1, double y1) => SketchCurveChain2d.Create([
        SketchCurve2d.Segment(new(x0, y0), new(x1, y0)), SketchCurve2d.Segment(new(x1, y0), new(x1, y1)),
        SketchCurve2d.Segment(new(x1, y1), new(x0, y1)), SketchCurve2d.Segment(new(x0, y1), new(x0, y0))], true);

    private static void Require(bool condition, string contract)
    {
        if (!condition) throw new InvalidOperationException("Batch P failed: " + contract);
    }

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string name, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
}
