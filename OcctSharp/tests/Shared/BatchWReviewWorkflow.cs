using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Public-only XDE/style/resource/capture path shared by runtime and clean package tests.
internal static class BatchWReviewWorkflow
{
    public static void Run()
    {
        var directory = Directory.CreateTempSubdirectory("OcctSharp.BatchW.");
        nint hwnd = 0;
        try
        {
            using var box = ShapeFactory.CreateBox(10, 10, 10); using var blueBox = ShapeFactory.CreateBox(10, 10, 10); using var doc = XdeDocument.Create();
            XdeLabel root, red, blue;
            using (var transaction = doc.BeginTransaction())
            {
                red = doc.AddShape(box, "red-body"); blue = doc.AddShape(blueBox, "blue-body");
                red.Color = new(.9, .02, .02); blue.Color = new(.02, .02, .9);
                red.VisualMaterial = new("review-source", new(.9, .02, .02), .5, .3, new(0, 0, 0), 1.5, XdeAlphaMode.Opaque, .5);
                root = doc.AddAssembly("review-assembly");
                using var identity = GpTrsf.Create(0, 0, 0); using var offset = GpTrsf.Create(14, 0, 0);
                using var a = TopLocLocation.FromTransform(identity); using var b = TopLocLocation.FromTransform(offset);
                doc.AddComponent(root, red, a); doc.AddComponent(root, blue, b); transaction.Commit();
            }
            var material = red.VisualMaterial; var color = red.Color; using var original = root.Shape;
            using var properties = GPropProperties.FromShape(original, GPropMode.Volume); double volume = properties.Mass;
            string step = Path.Combine(directory.FullName, "review.step"); doc.WriteStep(step);
            using var imported = XdeDocument.ReadStep(step); var input = imported.GetFreeShapes()[0];
            hwnd = CreateWindowEx(0, "STATIC", "Batch W copied review", 0x80000000u, -32000, -32000, 360, 300, 0, 0, 0, 0); Require(hwnd != 0, "HWND");
            ShowWindow(hwnd, 4); UpdateWindow(hwnd);
            using var viewer = OcctViewer.Create(hwnd); using var presentation = viewer.Display(input); presentation.SetDisplayMode(ViewerDisplayMode.Shaded);
            viewer.SetProjection(ViewerProjection.Top); viewer.FitAll(); viewer.Zoom(.8); var r = viewer.Rendering;
            r.ReplaceLightRig([new(ViewerLightKind.Ambient, new(1, 1, 1))]);
            var baseline = r.CaptureColor(new(240, 200)); var pixels = baseline.CopyPixels();
            string? debugEvidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_W_EVIDENCE");
            if (!string.IsNullOrWhiteSpace(debugEvidence)) { Directory.CreateDirectory(debugEvidence); viewer.SaveScreenshot(Path.Combine(debugEvidence, "source.png"), overwrite: true); }
            Require(CountDominant(pixels, 0) > 100 && CountDominant(pixels, 2) > 100, $"imported red and blue custom drawers: red={CountDominant(pixels, 0)}, blue={CountDominant(pixels, 2)}, roots={imported.GetFreeShapes().Length}");
            r.SetAppearance(presentation, new() { Shading = ViewerShading.Unlit, Front = new(new(0, 1, 0)) });
            var green = r.CaptureColor(new(240, 200)); Require(CountDominant(green.CopyPixels(), 1) > 500, "whole assembly review override");
            r.ResetAppearance(presentation); Require(baseline.CopyPixels().SequenceEqual(r.CaptureColor(new(240, 200)).CopyPixels()), "custom drawer reset");
            Require(red.Color == color && red.VisualMaterial == material, "source metadata unchanged");
            using var after = root.Shape; using var afterProperties = GPropProperties.FromShape(after, GPropMode.Volume); Require(Math.Abs(volume - afterProperties.Mass) < 1e-8, "source geometry unchanged");
            using var texture = r.CreateTexture(new(2, 2, new byte[] { 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255 }));
            r.SetAppearance(presentation, new() { Shading = ViewerShading.Unlit }, texture);
            var assets = new Dictionary<string, ViewerPresentation> { { "assembly", presentation } };
            var recipe = ViewerReviewRecipe.FromJson(r.CaptureRecipe("fixture-v1", assets, _ => "green").ToJson());
            r.ResetAppearance(presentation); r.ApplyRecipe(recipe, "fixture-v1", assets, new Dictionary<string, ViewerTexture> { { "green", texture } });
            Require(CountDominant(r.CaptureColor(new(240, 200)).CopyPixels(), 1) > 500, "portable review replay");
            var depth = r.CaptureDepth(new(240, 200)); Require(depth.CopyDepths().Any(x => x < 1), "copied model depth");
            Require(depth.Scope.LayerIds.SequenceEqual(new long[] { 0 }) && !depth.Scope.IncludesStandardOverlays, "explicit copied depth scope");
            string? evidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_W_EVIDENCE");
            if (!string.IsNullOrWhiteSpace(evidence)) { Directory.CreateDirectory(evidence); viewer.SaveScreenshot(Path.Combine(evidence, "review.png"), overwrite: true); }
            viewer.Dispose(); Require(depth.CopyDepths().Any(x => x < 1) && baseline.CopyPixels().Length == 240 * 200 * 4, "independent frame lifetime");
        }
        finally { if (hwnd != 0) DestroyWindow(hwnd); directory.Delete(true); }
    }
    private static int CountDominant(byte[] pixels, int channel) { int count = 0; for (int i = 0; i < pixels.Length; i += 4) if (pixels[i + channel] > 100 && pixels[i + channel] > pixels[i + (channel + 1) % 3] + 40 && pixels[i + channel] > pixels[i + (channel + 2) % 3] + 40) count++; return count; }
    private static void Require(bool condition, string name) { if (!condition) throw new InvalidOperationException("Batch W: " + name); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)] private static extern nint CreateWindowEx(uint extended, string className, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
}
