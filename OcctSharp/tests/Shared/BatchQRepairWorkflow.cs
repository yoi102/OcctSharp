using System.Runtime.InteropServices;

namespace OcctSharp.Validation;

// Public-only workflow shared with the isolated package consumer.
internal static class BatchQRepairWorkflow
{
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.BatchQ.Workflow." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using Shape plane = SurfaceModeling.CreateAnalyticFace(AnalyticSurfaceKind.Plane, SketchPlane.XY, new(0, 10, 0, 10));
            using Shape face = SurfaceModeling.CreateTrimmedFace(plane, SketchProfile2d.Create(Rectangle(0, 0, 10, 10), [Rectangle(4, 4, 6, 6)]));
            using XdeDocument document = XdeDocument.Create(); document.UndoLimit = 10;
            XdeLabel definition, assembly;
            using (XdeTransaction transaction = document.BeginTransaction("author shared repair source"))
            {
                definition = document.AddPart(face, new("Batch Q repaired plate", new XdeColor(0.2, 0.6, 0.8), ["Repair"]));
                assembly = document.AddAssembly("Batch Q repeated plates");
                using TopLocLocation origin = TopLocLocation.Identity;
                using GpTrsf translation = GpTrsf.Create(20, 0, 0, 0, 0, 1, 0);
                using TopLocLocation location = TopLocLocation.FromTransform(translation);
                document.AddComponent(assembly, definition, origin);
                document.AddComponent(assembly, definition, location);
                transaction.Commit();
            }
            using RepairDocumentSession session = new(document, definition);
            RepairSelection boundaryEdge = session.Source.Topology.First(value => value.Kind == ShapeKind.Edge).Selection;
            XdeLabel edgeLabel;
            using (XdeTransaction transaction = document.BeginTransaction("name protected boundary"))
            {
                edgeLabel = session.GetOrCreateSubshapeLabel(boundaryEdge);
                edgeLabel.Name = "keep boundary"; edgeLabel.Color = new XdeColor(0.8, 0.1, 0.2);
                transaction.Commit();
            }
            RepairPlan plan = new(session.Source, [new("remove small hole", new InternalHoleRemovalRepair(5))],
                [boundaryEdge], budget: new(MaximumRelativeAreaChange: 0.05));
            using RepairPreview preview = ShapeRepair.Preview(session.Source, plan);
            Require(preview.CanAccept, "repair acceptance: " + string.Join(";", preview.Stages.Select(value => value.Message)));
            RepairMetadataReview review = session.Review(preview);
            Require(review.CanPublish && review.SharedOccurrences == 2 && review.MappedLabels >= 1, "metadata review");
            session.Publish(preview);
            Require(preview.IsAccepted && !document.HasOpenTransaction, "atomic commit");
            Require(edgeLabel.Name == "keep boundary" && edgeLabel.Color is { Red: > 0.7 }, "subshape name and color");
            Require(definition.Name == "Batch Q repaired plate" && definition.Color is { Blue: > 0.7 }, "definition metadata");
            CheckAssembly(assembly, 100);
            Require(document.Undo(), "repair undo"); CheckAssembly(assembly, 96);
            Require(document.Redo(), "repair redo"); CheckAssembly(assembly, 100);
            using (Shape assemblyShape = assembly.Shape)
                Require(assemblyShape.FaceCount == 2, "cached assembly topology after redo: " + assemblyShape.FaceCount + " roots=" + string.Join(",", document.GetFreeShapes().Select(value => value.Entry + ":" + value.Name)));
            string recipe = RepairSerialization.SerializeRecipe(plan);
            Require(RepairSerialization.DeserializeRecipe(recipe, session.Source).Source == session.Source.Identity, "recipe reload");
            Require(RepairSerialization.DeserializeAudit(RepairSerialization.SerializeAudit(preview)).Accepted, "portable audit");

            foreach (string path in new[] { document.WriteStep(Path.Combine(directory, "repaired.step")), document.WriteIges(Path.Combine(directory, "repaired.iges")) })
            {
                using XdeDocument reopened = XdeDocument.ReadExchange(path);
                XdeLabel[] roots = reopened.GetFreeShapes();
                if (Path.GetExtension(path) == ".step")
                {
                    using Shape geometryOnly = ShapeExchange.ReadStep(path);
                    Require(geometryOnly.FaceCount == 2, "STEP geometry-only face count: " + geometryOnly.FaceCount);
                    using XdeDocument withoutColors = XdeDocument.ReadStep(path, new XdeStepReadOptions(ReadColors: false));
                    CheckAssembly(withoutColors.GetFreeShapes().Single(), 100);
                    CheckAssembly(roots.Single(), 100);
                }
                double area = 0; List<Shape> importedShapes = [];
                try
                {
                    foreach (XdeLabel root in roots)
                    {
                        Shape imported = root.Shape; importedShapes.Add(imported);
                        Require(imported.IsValid, "reopened validity");
                        using RepairSnapshot copied = RepairSnapshot.Create(imported);
                        if (imported.FaceCount > 0)
                            area += copied.Metrics.Area ?? throw new InvalidOperationException("Reopened area is unavailable: " + path + " / " + copied.Metrics);
                    }
                    Require(Math.Abs(area - 200) < 1e-4, "reopened area and millimetre scale: " + Path.GetExtension(path) + " area=" + area + " faces=" + string.Join(",", importedShapes.Select(value => value.FaceCount))
                        + " roots=" + string.Join(",", roots.Select(value => value.Entry + ":" + value.Name))
                        + " face entities=" + System.Text.RegularExpressions.Regex.Count(File.ReadAllText(path), "ADVANCED_FACE"));
                    using Shape whole = ShapeFactory.CreateCompound(importedShapes);
                    BoundingBox3d bounds = whole.GetBoundingBox();
                    Require(Math.Abs(bounds.Minimum.X) < 1e-4 && Math.Abs(bounds.Maximum.X - 30) < 1e-4, "reopened occurrence placement");
                    using DocumentSnapshot metadata = reopened.CreateSnapshot();
                    Require(metadata.Labels.Any(label => reopened.GetLabel(label.Entry).Name?.Contains("Batch Q", StringComparison.Ordinal) == true), "reopened names");
                    Require(roots.SelectMany(root => root.GetPresentationStyles()).Aggregate(false, (found, style) =>
                    {
                        using (style) return found || style.EffectiveColor is { Blue: > 0.7 };
                    }), "reopened supported color");
                }
                finally { foreach (Shape imported in importedShapes) imported.Dispose(); }
            }
            ReviewInViewer(session.Source, preview.Result!, directory);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void CheckAssembly(XdeLabel assembly, double expectedArea)
    {
        IReadOnlyList<XdeOccurrence> occurrences = assembly.GetOccurrences();
        try
        {
            Require(occurrences.Count == 2, "shared occurrence count"); List<double> starts = [];
            foreach (XdeOccurrence occurrence in occurrences)
            {
                using Shape shape = occurrence.GetLocatedShape();
                using RepairSnapshot snapshot = RepairSnapshot.Create(shape);
                Require(snapshot.Metrics.Area is double area && Math.Abs(area - expectedArea) < 1e-5,
                    "one shared definition update: " + snapshot.Metrics);
                starts.Add(shape.GetBoundingBox().Minimum.X);
            }
            Require(Math.Abs(starts.Min()) < 1e-5 && Math.Abs(starts.Max() - 20) < 1e-5, "preserved placements");
        }
        finally { foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose(); }
    }
    private static void ReviewInViewer(RepairSnapshot source, RepairSnapshot result, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch Q diagnostic review", 0x80000000u, -32000, -32000, 320, 320, 0, 0, 0, 0);
        Require(window != 0, "real HWND creation");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using OcctViewer viewer = OcctViewer.Create(window);
            using RepairViewerReview review = new(viewer, source);
            RepairSelection sourceFace = source.Topology.First(value => value.Kind == ShapeKind.Face).Selection;
            ViewerPresentation presentation = review.Focus(sourceFace);
            Require(viewer.GetSelection().Contains(presentation), "source defect native selection");
            review.ReplaceSnapshot(result);
            bool staleRejected = false;
            try { review.Focus(sourceFace); } catch (ArgumentException) { staleRejected = true; }
            Require(staleRejected, "stale review identity rejection");
            presentation = review.Focus(result.Topology.First(value => value.Kind == ShapeKind.Face).Selection);
            Require(viewer.GetSelection().Contains(presentation), "result defect native selection");
            viewer.Redraw();
            Require(new FileInfo(viewer.SaveScreenshot(Path.Combine(directory, "repair.png"))).Length > 0, "review screenshot");
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static SketchCurveChain2d Rectangle(double x1, double y1, double x2, double y2) => SketchCurveChain2d.Create([
        SketchCurve2d.Segment(new(x1, y1), new(x2, y1)), SketchCurve2d.Segment(new(x2, y1), new(x2, y2)),
        SketchCurve2d.Segment(new(x2, y2), new(x1, y2)), SketchCurve2d.Segment(new(x1, y2), new(x1, y1))], true);
    private static void Require(bool condition, string operation)
    { if (!condition) throw new InvalidOperationException("Batch Q workflow failed: " + operation); }
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
