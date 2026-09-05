using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Public-only workflow also used by the clean facade package consumer.
internal static class BatchULocalFeatureWorkflow
{
    public static void Run()
    {
        var directory = Directory.CreateTempSubdirectory("OcctSharp.BatchU.");
        try
        {
            using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(10, 12, 15);
            XdeLabel definition, assembly, first;
            using (var command = document.BeginTransaction("Local feature fixture"))
            {
                definition = document.AddShape(box, "Batch U part"); definition.Color = new(.1, .4, .9);
                assembly = document.AddAssembly("Repeated local part");
                using var identity = TopLocLocation.Identity; first = document.AddComponent(assembly, definition, identity);
                using var translation = ShapeTransform.CreateTranslation(25, 0, 0).ToGpTrsf(); using var location = TopLocLocation.FromTransform(translation);
                document.AddComponent(assembly, definition, location); command.Commit();
            }
            using var session = new LocalFeatureDocumentSession(document, assembly, [first.Entry]);
            var seed = session.Source.Topology.First(t => t.Kind == ShapeKind.Edge).Selection;
            var protectedFace = SelectFace(session.Source, b => b.Minimum.X > 9.99);
            XdeLabel metadata;
            using (var command = document.BeginTransaction("Protected face metadata"))
            { metadata = session.GetOrCreateSubshapeLabel(protectedFace); metadata.Name = "preserved wall"; metadata.Color = new(.9, .1, .1); command.Commit(); }
            var recipe = ContourFilletRecipe.Create(session.Source, [FilletContourProgram.FromLaw(seed, ScalarLawDefinition.Linear(new(0, 1), .5, 1.5))]);
            using var result = recipe.Build(session.Source); using var simulation = recipe.Simulate(session.Source);
            Require(result.Diagnostics.AlgorithmDone && result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
            var review = session.Review(result, protectedTopology: [protectedFace]);
            Require(review.CanPublish && review.SharedOccurrences == 2 && review.MappedLabels == 1, "exact protected metadata mapping");
            session.Publish(result, protectedTopology: [protectedFace]);
            Require(metadata.Name == "preserved wall" && metadata.Color is { Red: > .8 }, "mapped label metadata");
            double volume = Mass(result.RequireShape()); VerifyOccurrences(assembly, volume);
            Require(document.Undo(), "shared definition undo"); VerifyOccurrences(assembly, 1800);
            Require(document.Redo(), "shared definition redo"); VerifyOccurrences(assembly, volume);
            VerifyExchange(result, directory.FullName, "law-fillet");
            using var floor = ShapeFactory.CreateBox(10, 10, 2); using var wall = ShapeFactory.CreateBox(2, 10, 10); using var basis = floor.Fuse(wall);
            using var wire = ShapeFactory.CreatePolygonWire([new(2, 5, 8), new(8, 5, 2)]);
            using var ribPlan = RibSlotPlan.Create(basis, wire, new() { PlaneOrigin = new(0, 5, 0), ThicknessDirection1 = new(0, 1, 0), ThicknessDirection2 = new(0, -1, 0) });
            using var rib = ribPlan.Build(); VerifyExchange(rib, directory.FullName, "rib");
            using var profileWire = ShapeFactory.CreatePolygonWire([new(2, 2, 15), new(4, 2, 15), new(4, 4, 15), new(2, 4, 15)], true);
            using var profile = ShapeFactory.CreatePlanarFace(profileWire);
            using var stopWire = ShapeFactory.CreatePolygonWire([new(-5, -5, 18), new(20, -5, 18), new(20, 20, 18), new(-5, 20, 18)], true);
            using var stop = ShapeFactory.CreatePlanarFace(stopWire);
            var faces = box.GetFaces();
            try
            {
                var support = faces.Single(f => f.GetBoundingBox().Minimum.Z > 14.99);
                using var plan = LimitedFeaturePlan.Create(box, profile, support, new() { Limit = LocalFeatureLimit.Until }, until: stop);
                using var limited = plan.Build(); VerifyExchange(limited, directory.FullName, "limited-prism");
            }
            finally { foreach (var face in faces) face.Dispose(); }
            using var failed = recipe.Replace(session.Source, 0, FilletContourProgram.Constant(seed, 100)).Build(session.Source);
            VerifyViewer(result, simulation, failed, stop, directory.FullName);
        }
        finally { directory.Delete(true); }
    }
    private static RepairSelection SelectFace(RepairSnapshot source, Func<BoundingBox3d, bool> predicate)
    {
        foreach (var item in source.Topology.Where(t => t.Kind == ShapeKind.Face))
        { using var shape = source.CopySubshape(item.Selection); if (predicate(shape.GetBoundingBox())) return item.Selection; }
        throw new InvalidOperationException("Missing fixture face.");
    }
    private static void VerifyOccurrences(XdeLabel assembly, double volume)
    {
        var occurrences = assembly.GetOccurrences();
        try
        {
            Require(occurrences.Count == 2, "shared occurrences"); List<double> starts = [];
            foreach (var occurrence in occurrences)
            { using var shape = occurrence.GetLocatedShape(); Require(Math.Abs(Mass(shape) - volume) < 1e-4, "shared edited volume"); starts.Add(shape.GetBoundingBox().Minimum.X); }
            starts.Sort(); Require(Math.Abs(starts[0]) < 1e-4 && Math.Abs(starts[1] - 25) < 1e-4, "unchanged repeated placements");
        }
        finally { foreach (var occurrence in occurrences) occurrence.Dispose(); }
    }
    private static void VerifyExchange(LocalFeatureResult result, string directory, string name)
    {
        using var original = GPropProperties.FromShape(result.RequireShape(), GPropMode.Surface);
        foreach (var (format, extension) in new[] { (XdeExchangeFormat.Step, "step"), (XdeExchangeFormat.Iges, "iges") })
        {
            var delivered = LocalFeatureDelivery.Export(result, Path.Combine(directory, name + "." + extension), format, "Batch U " + name, new(.1, .4, .9));
            using var reopened = XdeDocument.ReadExchange(delivered.Path); double area = 0;
            var roots = reopened.GetFreeShapes(); Require(roots.Length > 0, "exchange roots");
            foreach (var root in roots) { using var shape = root.Shape; Require(shape.IsValid, "exchange validity"); using var p = GPropProperties.FromShape(shape, GPropMode.Surface); area += p.Mass; }
            Require(Math.Abs(area - original.Mass) < .005, "STEP/IGES area fidelity: " + area + " / " + original.Mass);
            bool color = false; foreach (var style in roots.SelectMany(r => r.GetPresentationStyles())) using (style) color |= style.EffectiveColor is { Blue: > .8 };
            Require(color && delivered.Disclosure.Contains("No executable", StringComparison.Ordinal), "metadata/disclosure");
            using var metadata = reopened.CreateSnapshot(); Require(metadata.Labels.Any(l => reopened.GetLabel(l.Entry).Name?.Contains("Batch U", StringComparison.Ordinal) == true), "exchange name");
        }
    }
    private static void VerifyViewer(LocalFeatureResult result, LocalFeatureResult simulation, LocalFeatureResult failed, Shape stop, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch U contour review", 0x80000000u, -32000, -32000, 420, 420, 0, 0, 0, 0);
        Require(window != 0, "real HWND");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using var viewer = OcctViewer.Create(window); using var review = new LocalFeatureViewerReview(viewer);
            review.Show(result, simulation); Require(review.DisplayedSections > 0 && !review.ShowingFailure, "copied sections"); Capture(viewer, directory, "sections.png");
            var unchanged = review.Presentations.ToArray();
            bool foreign = false; try { review.Show(failed, simulation); } catch (ArgumentException) { foreign = true; }
            Require(foreign && unchanged.SequenceEqual(review.Presentations), "foreign simulation is atomic");
            using var invalidStop = ShapeFactory.CreateBox(1, 1, 1); invalidStop.Dispose();
            bool disposedStop = false; try { review.Show(result, limits: [invalidStop]); } catch (ObjectDisposedException) { disposedStop = true; }
            Require(disposedStop && unchanged.SequenceEqual(review.Presentations), "disposed limit is atomic");
            var old = review.Presentations[0]; review.Show(result, simulation, [stop]);
            bool stale = false; try { old.Show(); } catch (ObjectDisposedException) { stale = true; } Require(stale, "replaced IDs"); Capture(viewer, directory, "limit.png");
            review.Show(failed, context: result.RequireShape()); Require(review.ShowingFailure && review.Presentations.Count > 1, "fault chain and result context"); Capture(viewer, directory, "fault.png");
            bool wrongThread = false; var thread = new Thread(() => { try { review.Show(failed); } catch (InvalidOperationException) { wrongThread = true; } });
            thread.Start(); thread.Join(); Require(wrongThread, "thread affinity"); viewer.Dispose(); review.Dispose();
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static void Capture(OcctViewer viewer, string directory, string file)
    {
        string path = viewer.SaveScreenshot(Path.Combine(directory, file)); Require(new FileInfo(path).Length > 0, "capture");
        string? target = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_U_EVIDENCE");
        if (!string.IsNullOrWhiteSpace(target)) { Directory.CreateDirectory(target); File.Copy(path, Path.Combine(target, file), true); }
    }
    private static double Mass(Shape shape) { using var p = GPropProperties.FromShape(shape); return p.Mass; }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("Batch U: " + message); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
}
