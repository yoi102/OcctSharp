using OcctSharp;

namespace OcctSharp.Samples;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 1 && string.Equals(args[0], "--smoke", StringComparison.OrdinalIgnoreCase))
        {
            return RunSmoke();
        }

        while (true)
        {
            SampleConsole.WriteMenu();
            string? choice = Console.ReadLine()?.Trim();
            if (choice is null || choice == "0")
            {
                Console.WriteLine("Program ended.");
                return 0;
            }

            try
            {
                _ = choice switch
                {
                    "1" => CreateShapeSample.Run(),
                    "2" => StepExportSample.Run(),
                    "3" => StlExportSample.Run(),
                    "4" => IgesExportSample.Run(),
                    "5" => XdeStepAssemblySample.Run(),
                    "6" => ViewerSample.Run(),
                    "7" => CommonApiWaveSample.Run(),
                    _ => SampleConsole.InvalidChoice(),
                };

                SampleConsole.Pause();
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine($"Operation failed: {error.Message}");
                SampleConsole.Pause();
            }
        }
    }

    private static int RunSmoke()
    {
        string nativeDirectory = Path.Combine(AppContext.BaseDirectory, "occt");
        string[] nativeFiles = Directory.Exists(nativeDirectory)
            ? Directory.GetFiles(nativeDirectory, "*.dll")
            : [];
        if (nativeFiles.Length != 62)
        {
            throw new InvalidOperationException(
                $"Expected 62 bundled native DLLs below '{nativeDirectory}', found {nativeFiles.Length}.");
        }

        OcctRuntimeInfo runtime = OcctRuntime.Info;
        if (runtime.AbiVersion != new Version(1, 45)
            || runtime.BridgeVersion != "0.53.0"
            || runtime.OcctVersion != "8.0.1")
        {
            throw new InvalidOperationException(
                $"Unexpected runtime identity: ABI {runtime.AbiVersion}, bridge {runtime.BridgeVersion}, OCCT {runtime.OcctVersion}.");
        }

        using Shape box = ShapeFactory.CreateBox(10, 20, 30);
        ShapeTopologySummary topology = box.GetTopologySummary();
        DetailedMeshSnapshot mesh = box.CreateDetailedMesh();
        if (box.FaceCount != 6 || !topology.IsClosed || !topology.IsValid
            || topology.UniqueCounts.VertexCount != 8 || mesh.TriangleCount == 0)
        {
            throw new InvalidOperationException("The common CAD smoke workflow returned unexpected topology or mesh data.");
        }

        Console.WriteLine(
            $"OcctSharp smoke passed: ABI {runtime.AbiVersion}, bridge {runtime.BridgeVersion}, OCCT {runtime.OcctVersion}, {nativeFiles.Length} DLLs, box faces {box.FaceCount}, mesh triangles {mesh.TriangleCount}.");
        return 0;
    }
}
