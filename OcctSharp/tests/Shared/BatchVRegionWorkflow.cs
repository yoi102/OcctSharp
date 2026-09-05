using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Public-only scenario used by both the runtime suite and clean facade consumer.
internal static class BatchVRegionWorkflow
{
    public static void Run()
    {
        var directory = Directory.CreateTempSubdirectory("OcctSharp.BatchV.");
        try
        {
            using var a = ShapeFactory.CreateBox(10, 10, 10); using var b = a.Transformed(ShapeTransform.CreateTranslation(5, 0, 0));
            using var plan = PartitionPlan.Create([a, b]);
            using var partition = plan.Build([new("left", [new(RegionExpression.Input(0).Except(RegionExpression.Input(1)), 4)]),
                new("right", [new(RegionExpression.Input(1), 8)]),
                new("materials", [new(RegionExpression.Input(0).Except(RegionExpression.Input(1)), 4), new(RegionExpression.Input(1), 8)], true)]);
            Require(partition.EvaluatePrecision().Accepted && partition.Cells.Count == 3, "partition acceptance");
            using var document = XdeDocument.Create();
            var products = RegionProducts.Create(document, partition, [new("left product", "left", new(.9, .15, .2)), new("right product", "right", new(.1, .25, .9))]);
            Require(products.Products.Count == 2 && products.Products.All(p => p.Label.Comment is not null), "region metadata");
            double area = 0;
            foreach (var product in products.Products) { using var shape = product.Label.Shape; using var props = GPropProperties.FromShape(shape, GPropMode.Surface); area += props.Mass; }
            foreach (var (format, extension) in new[] { (XdeExchangeFormat.Step, "step"), (XdeExchangeFormat.Iges, "iges") })
            {
                string file = RegionProducts.Export(document, Path.Combine(directory.FullName, "regions." + extension), format);
                using var reopened = XdeDocument.ReadExchange(file); var roots = reopened.GetFreeShapes(); Require(roots.Length > 0, "exchange roots");
                double actual = 0; bool red = false, blue = false;
                foreach (var root in roots)
                {
                    using var shape = root.Shape; Require(shape.IsValid, "reopened exact topology");
                    using var props = GPropProperties.FromShape(shape, GPropMode.Surface); actual += props.Mass;
                    foreach (var style in root.GetPresentationStyles()) using (style)
                    { red |= style.EffectiveColor is { Red: > .8 }; blue |= style.EffectiveColor is { Blue: > .8 }; }
                }
                Require(Math.Abs(actual - area) < .01, $"{format} separate region surface measures: {actual} / {area}"); Require(red && blue, "both product colors");
                using var metadata = reopened.CreateSnapshot();
                string expectedName = format == XdeExchangeFormat.Step ? "left product" : "Regions";
                Require(metadata.Labels.Any(l => reopened.GetLabel(l.Entry).Name?.Contains(expectedName, StringComparison.Ordinal) == true),
                    $"{format} product name: " + string.Join(" | ", metadata.Labels.Select(l => reopened.GetLabel(l.Entry).Name)));
            }
            Require(RegionProducts.ExchangeDisclosure.Contains("application/OCAF", StringComparison.Ordinal), "format boundary disclosure");
            using var occupied = ShapeFactory.CreateBox(5, 10, 10); using var voidPlan = BoundedVoidPlan.Create(a, [occupied]); using var voids = voidPlan.Build();
            using var voidShape = voids.CopyOutput("voids"); using var volumePlan = VolumeConstructionPlan.Create([voidShape]); using var volumes = volumePlan.Build();
            Require(volumes.Volumes.Count == 1 && Math.Abs(volumes.Volumes[0].Volume - 500) < 1e-6 && volumes.HelperBoxExcluded, "bounded void construction");
            VerifyViewer(partition, volumes, a, directory.FullName);
        }
        finally { directory.Delete(true); }
    }
    private static void VerifyViewer(PartitionResult partition, VolumeConstructionResult voids, Shape envelope, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch V cell/interface/void review", 0x80000000u, -32000, -32000, 420, 420, 0, 0, 0, 0);
        Require(window != 0, "real HWND");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using var viewer = OcctViewer.Create(window); using var review = new RegionViewerReview(viewer);
            review.Show(partition, "materials", [partition.Cells[0].Id], partition.GetMaterialInterfaces("materials").Select(b => b.Id).ToArray());
            Require(review.Revision == partition.Revision && review.Presentations.Count >= 3, "cell and interface review");
            viewer.SelectRectangle(0, 0, 420, 420); Require(viewer.GetSelection().Count > 0, "real region selection"); Capture(viewer, directory, "interfaces.png");
            var previous = review.Presentations.ToArray(); bool rejected = false;
            try { review.Show(partition, "materials", [new(Guid.NewGuid(), 0)]); } catch (ArgumentException) { rejected = true; }
            Require(rejected && previous.SequenceEqual(review.Presentations), "foreign cell leaves prior review intact");
            var stale = review.Presentations[0]; review.ShowVolumes(voids, envelope: envelope); review.Presentations[0].SetTransparency(.8);
            Require(review.Revision == voids.Revision && review.Presentations.Count == 2, "void and explicit envelope");
            viewer.Redraw(); Capture(viewer, directory, "void.png");
            bool invalidated = false; try { stale.Show(); } catch (ObjectDisposedException) { invalidated = true; } Require(invalidated, "prior presentation invalidated");
            bool wrongThread = false; var thread = new Thread(() => { try { review.ShowVolumes(voids); } catch (InvalidOperationException) { wrongThread = true; } });
            thread.Start(); thread.Join(); Require(wrongThread, "viewer thread affinity"); viewer.Dispose(); review.Dispose();
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static void Capture(OcctViewer viewer, string directory, string name)
    {
        string file = viewer.SaveScreenshot(Path.Combine(directory, name)); Require(new FileInfo(file).Length > 0, "capture");
        string? evidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_V_EVIDENCE");
        if (!string.IsNullOrWhiteSpace(evidence)) { Directory.CreateDirectory(evidence); File.Copy(file, Path.Combine(evidence, name), true); }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("Batch V: " + message); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
}
