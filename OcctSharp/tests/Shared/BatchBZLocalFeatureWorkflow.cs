using System.Text.Json;
namespace OcctSharp.Tests.Shared;
internal static class BatchBZLocalFeatureWorkflow
{
    static void Require(bool value, string message) { if (!value) throw new InvalidOperationException("BZ " + message); }
    static double Volume(Shape shape) => shape.InspectProperties(InspectionPropertyKind.Volume).Mass;
    static void Close(double actual, double expected) => Require(double.IsFinite(actual) && Math.Abs(actual - expected) < 1e-5, $"volume {actual:R}/{expected:R}");
    static string Fingerprint(Shape shape) { using var snapshot = RepairSnapshot.Create(shape); return snapshot.Fingerprint; }
    static RepairSelection Select(RepairSnapshot source, ShapeKind kind, Func<BoundingBox3d, bool> predicate)
    {
        foreach (var item in source.Topology.Where(t => t.Kind == kind))
        { using var shape = source.CopySubshape(item.Selection); if (predicate(shape.GetBoundingBox())) return item.Selection; }
        throw new InvalidOperationException("BZ requested source topology is absent.");
    }
    static void Rejected(Action action, string message)
    { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } Require(rejected, message); }
    internal static void Run(string operation)
    {
        Require(operation is "Fillet" or "Chamfer" or "Sweep", "unknown operation");
        using var box = ShapeFactory.CreateBox(10, 12, 15);
        using var source = RepairSnapshot.Create(box);
        string before = source.Fingerprint;
        var edge = Select(source, ShapeKind.Edge, b => Math.Abs(b.Maximum.X) < 1e-5 && Math.Abs(b.Maximum.Y) < 1e-5);
        var face = Select(source, ShapeKind.Face, b => Math.Abs(b.Maximum.X) < 1e-5);
        var stale = edge with { Source = edge.Source with { Revision = edge.Source.Revision + 1 } };
        using var foreign = RepairSnapshot.Create(box);
        var fillet = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(edge, 1)]);
        Rejected(() => fillet.Build(foreign), "foreign recipe source accepted");
        Rejected(() => ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(stale, 1)]), "stale selection accepted");
        Rejected(() => FilletContourProgram.Constant(edge, double.NaN), "invalid radius accepted");
        using var failed = ContourFilletRecipe.Create(source, [FilletContourProgram.Constant(edge, 100)]).Build(source);
        Require(!failed.Diagnostics.AlgorithmDone && failed.Shape is null && failed.Faults.Count > 0, "failed feature published a root or lost faults");
        bool rootRejected = false; try { failed.RequireShape(); } catch (InvalidOperationException) { rootRejected = true; }
        Require(rootRejected, "failed root accepted");
        Close(Volume(box), 1800); Require(Fingerprint(box) == before, "failed feature changed its source");
        Shape retained; double expected; Guid plan; int history; int simulated;
        if (operation == "Sweep")
        {
            using var spine = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 5)]);
            using var profile = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
            using var sweep = GuidedSweepPlan.Create(spine, [new(profile)], new() { SolidPolicy = SweepSolidPolicy.RequireSolid });
            Require(sweep.Preflight().Ready, "sweep preflight failed");
            using var preview = sweep.Simulate(3); Require(preview.Diagnostics.AlgorithmDone && preview.SimulatedSections.Count == 3, "sweep simulation failed");
            using var result = sweep.Build(); Require(result.Diagnostics.AlgorithmDone && result.Diagnostics.ShapeIsValid && result.Diagnostics.IsSolid, result.Diagnostics.Message);
            retained = result.RequireShape().Clone(); expected = 20; plan = sweep.Id; history = result.History.Count; simulated = preview.SimulatedSections.Count;
            spine.Dispose(); profile.Dispose(); sweep.Dispose(); result.Dispose(); preview.Dispose();
        }
        else
        {
            var chamfer = ContourChamferRecipe.Create(source, [new(edge, face, ChamferDimensions.Symmetric, 1)]);
            using var discovery = operation == "Fillet" ? fillet.Discover(source) : chamfer.Discover(source);
            Require(discovery.Contours.Count == 1 && discovery.ContourEdges.Count > 0, "source contour discovery failed");
            Require(discovery.Contours[0].Seed == edge && Math.Abs(discovery.Contours[0].Length - 15) < 1e-5, "contour selection/length differs");
            using var preview = fillet.Simulate(source); Require(preview.SimulatedSections.Count > 0, "fillet simulation missing");
            using var result = operation == "Fillet" ? fillet.Build(source) : chamfer.Build(source);
            Require(result.Diagnostics.AlgorithmDone && result.Diagnostics.ShapeIsValid, result.Diagnostics.Message);
            Require(result.History.Any(h => h.Kind == LocalFeatureHistoryKind.Generated && h.Shape?.Kind == ShapeKind.Face), "generated patch history missing");
            retained = result.RequireShape().Clone(); expected = operation == "Fillet" ? 1800 - 15 * (1 - Math.PI / 4) : 1792.5;
            plan = operation == "Fillet" ? fillet.Id : chamfer.Id; history = result.History.Count; simulated = preview.SimulatedSections.Count;
            result.Dispose();
        }
        using (retained)
        {
            using var current = source.CopyShape(); Require(Fingerprint(box) == before && Fingerprint(current) == before, "successful feature changed its source");
            source.Dispose(); box.Dispose(); foreign.Dispose(); failed.Dispose();
            Require(retained.IsValid && history > 0, "result depends on closed source/builder"); Close(Volume(retained), expected);
            string path = Path.Combine(Path.GetTempPath(), "OcctSharp.BZ." + Guid.NewGuid() + ".step");
            try
            {
                ShapeExchange.WriteStep(retained, path); using var reopened = ShapeExchange.ReadStep(path);
                Require(reopened.IsValid && reopened.CountSubShapes(ShapeKind.Solid) == 1, "STEP topology differs"); Close(Volume(reopened), expected);
                using var read = RepairSnapshot.Create(reopened); Rejected(() => fillet.Build(read), "STEP reopen rebound old selection");
                BatchBZRawTopologyViewer.Run(path, operation, Volume(reopened));
                if (Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BZ_EVIDENCE") is { } root)
                {
                    string output = Path.Combine(root, operation.ToLowerInvariant()); Directory.CreateDirectory(output);
                    File.WriteAllText(Path.Combine(output, "workflow-result.json"), JsonSerializer.Serialize(new { state = "PASS", operation, plan, history, simulated, sourcePreserved = true, foreignRejected = true, staleRejected = true, invalidParameterRejected = true, failureRollback = true, reopenedIdentityRejected = true, independentLifetime = true, stepVolume = Volume(reopened), stepSolids = 1 }));
                }
            }
            finally { File.Delete(path); }
        }
        Console.WriteLine($"BZ {operation} workflow passed: selected contour/private sweep, rollback, retained history/result, actual STEP geometry and HWND selection/invalidation.");
    }
    internal static void Run() { foreach (string operation in new[] { "Fillet", "Chamfer", "Sweep" }) Run(operation); }
}
