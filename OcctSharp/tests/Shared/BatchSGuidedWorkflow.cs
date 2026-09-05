using System.Runtime.InteropServices;
using System.Text.Json;

namespace OcctSharp.Validation;

// Public-only acceptance workflow shared with the isolated facade package consumer.
internal static class BatchSGuidedWorkflow
{
    public static void Run()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchS.");
        try
        {
            using Shape spine = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 10)]);
            using Shape wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
            using var plan = GuidedSweepPlan.Create(spine, [new(wire)], new() { SolidPolicy = SweepSolidPolicy.RequireSolid });
            GuidedAuthoringRecipe recipe = GuidedAuthoringDelivery.Capture(plan);
            using AuthoringResult simulation = plan.Simulate(5);
            using AuthoringResult result = plan.Build();
            Require(result.Diagnostics.IsSolid, "accepted solid sweep");
            using Shape support = ShapeFactory.CreatePlanarFace(wire);
            Shape[] edges = support.GetSubShapes(ShapeKind.Edge);
            ConstrainedFillPlan bad;
            try
            {
                List<SurfaceConstraint> constraints = edges.Select((edge, i) => (SurfaceConstraint)new SurfaceEdgeConstraint("edge-" + i, edge)).ToList();
                constraints.Add(new SurfacePointConstraint("conflicting-point", new(0, 0, 10)));
                bad = ConstrainedFillPlan.Create(constraints, new() { MaximumSegments = 1, MaximumDegree = 2, Iterations = 1 });
            }
            finally { foreach (Shape edge in edges) edge.Dispose(); }
            using (bad)
            using (ConstrainedFillResult unsatisfied = bad.Build())
            {
                Require(!unsatisfied.Accepted, "unsatisfied constraints are not accepted geometry");
                spine.Dispose(); wire.Dispose(); support.Dispose(); plan.Dispose();
                VerifyDocument(result, recipe, directory.FullName);
                VerifyViewer(simulation, result, bad, unsatisfied, directory.FullName);
            }
        }
        finally { directory.Delete(true); }
    }

    private static void VerifyDocument(AuthoringResult result, GuidedAuthoringRecipe recipe, string directory)
    {
        using XdeDocument document = XdeDocument.Create(); document.UndoLimit = 10;
        XdeLabel source;
        using (var transaction = document.BeginTransaction("source reference"))
        {
            source = document.GetLabel("0:1").AddChild(); source.Name = "copied source definition"; transaction.Commit();
        }
        var product = GuidedAuthoringDelivery.Publish(document, result, recipe, "Batch S guided solid", new(0.2, 0.7, 0.4), [source]);
        Require(!document.HasOpenTransaction && product.Recipe.Reference?.Entry == product.Result.Entry, "atomic recipe/result reference");
        Require(product.Recipe.References.Single().Entry == source.Entry, "same-document source references");
        Require(JsonSerializer.Deserialize<GuidedAuthoringRecipe>(product.Recipe.AsciiString!)?.PlanId == recipe.PlanId, "copied recipe JSON");
        Require(recipe.SourceFingerprints.Count == 2 && recipe.SourceFingerprints.All(v => v.Length == 64), "source fingerprint snapshots");
        Require(document.Undo() && document.GetFreeShapes().Length == 0, "publication undo");
        Require(document.Redo() && document.GetFreeShapes().Length == 1, "publication redo");
        Require(product.Result.Color is { Green: > 0.6 } && product.Result.Name == "Batch S guided solid", "supported color and name");
        using (XdeDocument stored = XdeDocument.Open(document.Save(Path.Combine(directory, "配方.xbf"))))
        {
            Require(stored.GetLabel(product.Recipe.Entry).AsciiString == product.Recipe.AsciiString, "BinXCAF recipe persistence");
            Require(stored.GetLabel(product.Recipe.Entry).References.Single().Entry == source.Entry, "BinXCAF source references");
        }
        bool wrongPlan = false;
        try { GuidedAuthoringDelivery.Publish(document, result, recipe with { PlanId = Guid.NewGuid() }, "foreign"); }
        catch (ArgumentException) { wrongPlan = true; }
        Require(wrongPlan && document.GetFreeShapes().Length == 1, "foreign recipe fails before mutation");
        using XdeDocument foreign = XdeDocument.Create(); bool wrongDocument = false;
        try { GuidedAuthoringDelivery.Publish(document, result, recipe, "foreign", sourceReferences: [foreign.GetLabel("0:1")]); }
        catch (ArgumentException) { wrongDocument = true; }
        Require(wrongDocument && document.GetFreeShapes().Length == 1, "foreign label fails before mutation");
        foreach (string file in new[] { document.WriteStep(Path.Combine(directory, "扫掠.step")), document.WriteIges(Path.Combine(directory, "扫掠.iges")) })
        {
            using XdeDocument reopened = XdeDocument.ReadExchange(file);
            var roots = reopened.GetFreeShapes(); Require(roots.Length > 0, "exchange roots");
            double area = 0;
            foreach (var root in roots)
            {
                using Shape shape = root.Shape; Require(shape.IsValid, "exchange topology validity");
                Shape[] faces = shape.GetFaces();
                try { area += faces.Sum(face => face.InspectProperties(InspectionPropertyKind.Area).Mass); }
                finally { foreach (Shape face in faces) face.Dispose(); }
                Require(Math.Abs(shape.GetBoundingBox().SizeZ - 10) < 1e-3, "millimetre scale");
            }
            Require(Math.Abs(area - 88) < 1e-3, "STEP/IGES area including caps: " + area);
            using DocumentSnapshot snapshot = reopened.CreateSnapshot();
            Require(snapshot.Labels.Any(label => reopened.GetLabel(label.Entry).Name?.Contains("Batch S", StringComparison.Ordinal) == true), "exchange names");
            bool colorFound = false;
            foreach (var style in roots.SelectMany(root => root.GetPresentationStyles()))
            {
                using (style) colorFound |= style.EffectiveColor is { Green: > 0.6, Red: < 0.3 };
            }
            Require(colorFound, "exchange supported color");
        }
        document.Dispose(); Require(result.RequireShape().IsValid, "result survives document disposal");
        bool stale = false; try { _ = product.Recipe.AsciiString; } catch (ObjectDisposedException) { stale = true; }
        Require(stale, "parent-bound recipe label");
    }

    private static void VerifyViewer(AuthoringResult simulation, AuthoringResult result, ConstrainedFillPlan bad,
        ConstrainedFillResult unsatisfied, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch S guided authoring", 0x80000000u, -32000, -32000, 360, 360, 0, 0, 0, 0);
        Require(window != 0, "real HWND creation");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using OcctViewer viewer = OcctViewer.Create(window); using GuidedAuthoringReview review = new(viewer);
            review.ShowSimulation(simulation); Require(review.Presentations.Count == 5, "simulated sections displayed");
            Capture(viewer, directory, "simulation.png"); ViewerPresentation old = review.Presentations[0];
            review.ShowUnsatisfiedConstraints(bad, unsatisfied); Require(review.Presentations.Count > 0, "red failing edges/point markers displayed");
            Capture(viewer, directory, "unsatisfied.png");
            using (Shape boundary = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)], true))
            {
                Shape[] edges = boundary.GetSubShapes(ShapeKind.Edge);
                try
                {
                    using var foreign = ConstrainedFillPlan.Create(edges.Select((edge, i) => new SurfaceEdgeConstraint("foreign-" + i, edge)));
                    ViewerPresentation retained = review.Presentations[0];
                    bool rejected = false;
                    try { review.ShowUnsatisfiedConstraints(foreign, unsatisfied); } catch (ArgumentException) { rejected = true; }
                    Require(rejected && ReferenceEquals(retained, review.Presentations[0]), "foreign review fails without replacing presentations");
                    retained.Show();
                }
                finally { foreach (Shape edge in edges) edge.Dispose(); }
            }
            bool stale = false; try { old.Show(); } catch (ObjectDisposedException) { stale = true; }
            Require(stale, "replaced section presentation invalidated");
            review.ShowResult(result); Require(review.Presentations.Count == 1, "accepted owning result displayed");
            Capture(viewer, directory, "result.png");
            bool wrongThread = false;
            Thread otherThread = new(() =>
            {
                try { review.ShowResult(result); } catch (InvalidOperationException) { wrongThread = true; }
            });
            otherThread.Start(); otherThread.Join();
            Require(wrongThread, "creating-thread affinity");
            viewer.Dispose(); bool parent = false;
            try { review.ShowResult(result); } catch (ObjectDisposedException) { parent = true; }
            Require(parent, "disposed viewer rejects review"); review.Dispose(); review.Dispose();
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static void Capture(OcctViewer viewer, string directory, string file)
    {
        string path = viewer.SaveScreenshot(Path.Combine(directory, file)); Require(new FileInfo(path).Length > 0, "screenshot " + file);
        string? evidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_S_EVIDENCE");
        if (!string.IsNullOrWhiteSpace(evidence)) { Directory.CreateDirectory(evidence); File.Copy(path, Path.Combine(evidence, file), true); }
    }
    private static void Require(bool condition, string operation)
    { if (!condition) throw new InvalidOperationException("Batch S workflow failed: " + operation); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);
}
