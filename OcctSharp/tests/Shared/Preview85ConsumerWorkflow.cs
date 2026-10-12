using System.Runtime.InteropServices;

namespace OcctSharp.Tests.Shared;

/// <summary>New public contracts exercised by both independent package consumers.</summary>
internal static class Preview85ConsumerWorkflow
{
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("Preview85 consumer: " + message);
    }

    internal static void Run()
    {
        Require(OcctRuntime.Info.AbiVersion == new Version(1, 118), "actual native ABI");
        using var box = ShapeFactory.CreateBox(12, 12, 12);
        using (var copy = AuthoringInputCopy.Create([box, box]))
        {
            Require(copy.Inputs.Count == 2 && copy.Report.SchemaVersion == 1, "owned copy/report");
            Require(copy.Report.Rows.All(row => row.SourceArgumentMemberships == 2
                && row.TargetArgumentMemberships == 2 && !row.HasUniqueArgumentMapping), "shared-input ambiguity");
            Require(!copy.Report.ProvesCompleteOccurrencePaths && !copy.Report.ProvesFinalResultIdentity,
                "copy report overclaimed identity");
            Require(copy.Inputs.All(shape => RepairSnapshot.FindTopologyIndex(box, shape) == -1), "original copy identity reused");
        }

        using var spine = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(0, 0, 5)]);
        using var profile = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], true);
        using (var plan = GuidedSweepPlan.Create(spine, [new(profile)], new() { SolidPolicy = SweepSolidPolicy.RequireSolid }))
        using (var result = plan.Build())
        {
            Require(result.Diagnostics.ShapeIsValid && result.Diagnostics.IsSolid, result.Diagnostics.Message);
            Require(result.InputCopy is not null && result.HistoryDomains is not null, "actual copy/history reports missing");
            var domains = result.HistoryDomains!;
            Require(domains.InputDomainId == result.InputCopy!.TargetDomainId && domains.FinalDomainId != domains.InputDomainId,
                "history domains collapsed");
            Require(domains.Rows.Any(row => row.Kind == AuthoringHistoryKind.Generated
                && row.HasUniqueNativeCopySourceMembership && row.IsNativeMemberOfFinalResult), "actual generated membership missing");
            Require(Math.Abs(result.RequireShape().InspectProperties(InspectionPropertyKind.Volume).Mass - 20) <= 1e-7,
                "sweep volume");
        }

        using var hole = FeatureModeling.CutHole(box, new(6, 6, 20), new(0, 0, -1), 1, 30, true,
            new() { FuzzyTolerance = 1e-7, NonDestructive = true, RepairInputs = false, UnifyResult = true });
        using var snapshot = RepairSnapshot.Create(hole.RequireShape());
        var faces = snapshot.Topology.Where(item =>
        {
            if (item.Kind != ShapeKind.Face) return false;
            using var face = snapshot.CopySubshape(item.Selection);
            return face.GetFaceSurfaceSnapshot().SurfaceType == SurfaceGeometryType.Cylinder;
        }).Select(item => item.Selection).ToArray();
        Require(faces.Length == 1, "authored bore face");
        using (var result = DefeatureHistoryModeling.Build(snapshot, faces))
        {
            Require(result.Diagnostics.Operation == LocalFeatureOperation.Defeature
                && result.Diagnostics.ShapeIsValid && result.Diagnostics.AlgorithmDone, result.Diagnostics.Message);
            Require(Math.Abs(result.RequireShape().InspectAdaptiveProperties(InspectionPropertyKind.Volume, 1e-10).Properties.Mass - 1728) <= 1e-7,
                "defeature volume");
            Require(result.History.Any(row => row.Source?.TopologyIndex == faces[0].Index
                && row.Kind == LocalFeatureHistoryKind.Deleted && row.Shape is null), "native deletion history");
        }

        // Authored edge/face witnesses retain original domains and owned supports.
        string fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures", "b18-periodic-interior");
        using var first = ShapeExchange.ReadBrep(Path.Combine(fixtures, "circle-plane-0-a.brep"));
        using var second = ShapeExchange.ReadBrep(Path.Combine(fixtures, "circle-plane-0-b.brep"));
        using var distance = first.InspectDistanceTo(second);
        Require(distance.Solutions.Count > 0, "periodic distance missing");
        foreach (var solution in distance.Solutions)
        {
            Require(Math.Abs(solution.PointOnFirst.DistanceTo(solution.PointOnSecond) - solution.Distance) <= 1e-7,
                "distance witness mismatch");
            Require(solution.FirstSupport.IsValid && solution.SecondSupport.IsValid, "owned distance supports");
        }

        string output = Path.Combine(Path.GetTempPath(), "OcctSharp-preview85-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        try
        {
            using var exact = ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory, "Fixtures", "b18-exact-step",
                "spline-g1-normal-outward-Round.brep"));
            string step = ShapeExchange.WriteStep(exact, Path.Combine(output, "exact.step"));
            Require(File.ReadAllText(step).Contains("OFFSET_CURVE_3D", StringComparison.Ordinal), "exact STEP graph missing");
            using var readback = ShapeExchange.ReadStep(step);
            Require(readback.GetValidationReport(true, true).IsValid && readback.GetTopologySummary().UniqueCounts.SolidCount == 1,
                "empty/invalid STEP readback");
            double volume = readback.InspectAdaptiveProperties(InspectionPropertyKind.Volume, 1e-11).Properties.Mass;
            Require(Math.Abs(volume - 157.78143097450942) <= 1e-7, "strict exact STEP volume");
        }
        finally { Directory.Delete(output, true); }
#if !DIRECT_MODULE_CONSUMER
        InitialDisplay(box);
#endif
        Console.WriteLine("Preview85 owned copy/history/defeature/periodic-distance/exact-STEP package consumer PASS.");
    }

#if !DIRECT_MODULE_CONSUMER
    private static void InitialDisplay(Shape shape)
    {
        nint window = CreateWindowEx(0, "STATIC", "Preview85 package display", 0x80000000,
            -32000, -32000, 320, 320, 0, 0, 0, 0);
        Require(window != 0, "initial display HWND");
        try
        {
            using var viewer = OcctViewer.Create(window);
            var capture = new ViewerCaptureOptions(320, 320);
            ViewerCameraState camera;
            byte[] expected;
            using (var legacy = viewer.Display(shape))
            {
                legacy.SetDisplayMode(ViewerDisplayMode.Shaded); viewer.FitAll();
                camera = viewer.GetCamera(); expected = viewer.Rendering.CaptureColor(capture).CopyPixels();
            }
            using (var direct = viewer.DisplayInMode(shape, ViewerDisplayMode.Shaded))
            {
                viewer.SetCamera(camera);
                Require(expected.SequenceEqual(viewer.Rendering.CaptureColor(capture).CopyPixels()), "first display frame mismatch");
            }
            using var source = viewer.CreateSource(shape); using var identity = GpTrsf.Identity;
            using var instance = viewer.DisplayInMode(source, identity, ViewerDisplayMode.Shaded);
            viewer.SetCamera(camera);
            Require(expected.SequenceEqual(viewer.Rendering.CaptureColor(capture).CopyPixels()), "first shared display frame mismatch");
        }
        finally { _ = DestroyWindow(window); }
    }

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extended, string className, string name, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);
#endif
}
