using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Public-only real-file and real-HWND acceptance, also compiled into the clean facade consumer.
internal static class BatchTParametricWorkflow
{
    public static void Run()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchT.");
        try
        {
            using var graph = ParametricDocument.CreateXde();
            var box = new ParametricFeatureDefinition(Guid.NewGuid(), "Parametric part", ParametricFeatureKind.Box,
                new Dictionary<string, ParametricParameter> { ["x"] = Length(2), ["y"] = Length(3), ["z"] = Length(4) }, []);
            graph.Add(box); Succeeded(graph.Recompute());
            XdeDocument document = graph.Xde; XdeLabel definition; XdeLabel first, second;
            using (var transaction = document.BeginTransaction("Repeated parametric occurrences"))
            using (var result = graph.GetResult(box.Id))
            using (var identity = TopLocLocation.Identity)
            using (var shift = ShapeTransform.CreateTranslation(8, 0, 0).ToGpTrsf())
            using (var moved = TopLocLocation.FromTransform(shift))
            {
                definition = document.AddShape(result.Shape!, "Shared parametric definition"); definition.Color = new(0.2, 0.65, 0.85);
                var assembly = document.AddAssembly("Parametric assembly");
                first = document.AddComponent(assembly, definition, identity); second = document.AddComponent(assembly, definition, moved);
                transaction.Commit();
            }
            Succeeded(graph.EditAndRecompute(box.WithParameter("x", Length(5))));
            var publication = graph.PublishDefinition(box.Id, document, definition);
            Require(publication.CanPublish && publication.SharedOccurrences == 2, "replace one definition used twice");
            using (var a = first.Shape) using (var b = second.Shape)
            {
                Require(Math.Abs(a.GetBoundingBox().SizeX - 5) < 1e-5 && Math.Abs(b.GetBoundingBox().SizeX - 5) < 1e-5, "both occurrences updated");
                Require(Math.Abs(b.GetBoundingBox().Minimum.X - 8) < 1e-5, "placement retained");
            }
            Require(definition.Color is { Blue: > 0.8 } && definition.Name == "Shared parametric definition", "definition metadata retained");
            foreach (var format in new[] { XdeExchangeFormat.Step, XdeExchangeFormat.Iges })
            {
                string extension = format == XdeExchangeFormat.Step ? "step" : "iges";
                var export = graph.Export(box.Id, Path.Combine(directory.FullName, "参数模型." + extension), format, "Batch T product", new(0.2, 0.65, 0.85));
                using var reopened = XdeDocument.ReadExchange(export.Path);
                var roots = reopened.GetFreeShapes(); Require(roots.Length > 0, "exact exchange roots");
                double area = 0;
                foreach (var root in roots)
                {
                    using var shape = root.Shape; Require(shape.IsValid, "exact exchange validity");
                    using var properties = GPropProperties.FromShape(shape, GPropMode.Surface); area += properties.Mass;
                }
                Require(Math.Abs(area - 94) < 1e-3, "exact STEP/IGES area");
                bool color = false;
                foreach (var style in roots.SelectMany(x => x.GetPresentationStyles()))
                    using (style) color |= style.EffectiveColor is { Blue: > 0.8, Red: < 0.3 };
                Require(color && export.Disclosure.Contains("No executable", StringComparison.Ordinal), "color and graph exclusion disclosure");
            }
            // Restore a clean current definition before the viewer's edit/undo sequence.
            Succeeded(graph.EditAndRecompute(box));
            VerifyViewer(graph, box, directory.FullName);
        }
        finally { directory.Delete(true); }
    }

    private static void VerifyViewer(ParametricDocument graph, ParametricFeatureDefinition box, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch T parametric review", 0x80000000u, -32000, -32000, 360, 360, 0, 0, 0, 0);
        Require(window != 0, "real HWND creation");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using var viewer = OcctViewer.Create(window); using var review = new ParametricViewerReview(viewer, graph);
            review.Refresh(box.Id); var old = review.Presentations.Single(); Guid? previous = review.DisplayedRevision;
            Succeeded(graph.EditAndRecompute(box.WithParameter("x", Length(5)))); review.Refresh(box.Id);
            Require(review.DisplayedRevision != previous, "new result generation displayed"); Capture(viewer, directory, "recomputed.png");
            bool stale = false; try { old.Show(); } catch (ObjectDisposedException) { stale = true; }
            Require(stale, "replaced viewer IDs rejected");
            Require(graph.Undo(), "undo recompute"); review.Refresh(box.Id);
            Require(review.DisplayedRevision == previous, "undo refresh returns original generation"); Capture(viewer, directory, "undo.png");
            graph.Update(box.WithParameter("x", Length(-1))); Require(!graph.Recompute().Succeeded, "failed feature not accepted");
            review.ShowFailureInputs(box.Id); Require(review.ShowingStaleInputs, "last-good review explicitly stale"); Capture(viewer, directory, "failed.png");
            bool wrongThread = false; Thread thread = new(() => { try { review.ShowFailureInputs(box.Id); } catch (InvalidOperationException) { wrongThread = true; } });
            thread.Start(); thread.Join(); Require(wrongThread, "creating-thread affinity");
            viewer.Dispose(); review.Dispose(); review.Dispose();
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static ParametricParameter Length(double value) => ParametricParameter.FromValue(ParametricValue.FromReal(value, ParametricUnit.Millimeter));
    private static void Succeeded(ParametricRecomputeReport report) => Require(report.Succeeded, string.Join("; ", report.Issues.Select(x => x.Message)));
    private static void Capture(OcctViewer viewer, string directory, string file)
    {
        string path = viewer.SaveScreenshot(Path.Combine(directory, file)); Require(new FileInfo(path).Length > 0, "screenshot");
        string? evidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_T_EVIDENCE");
        if (!string.IsNullOrWhiteSpace(evidence)) { Directory.CreateDirectory(evidence); File.Copy(path, Path.Combine(evidence, file), true); }
    }
    private static void Require(bool condition, string operation)
    { if (!condition) throw new InvalidOperationException("Batch T workflow failed: " + operation); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
}
