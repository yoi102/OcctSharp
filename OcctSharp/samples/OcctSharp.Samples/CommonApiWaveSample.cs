namespace OcctSharp.Samples;

internal static class CommonApiWaveSample
{
    public static int Run()
    {
        string brepPath = SamplePaths.GetDefaultOutputPath("common-api-part.brep");
        string stepPath = SamplePaths.GetDefaultOutputPath("common-api-assembly.step");
        using Shape box = ShapeFactory.CreateBox(80, 60, 40);
        using Shape chamfered = box.Chamfer(4);
        ShapeTopologySummary topology = chamfered.GetTopologySummary();
        DetailedMeshSnapshot mesh = chamfered.CreateDetailedMesh(0.5, 0.35);

        ShapeExchange.WriteBrep(chamfered, brepPath);
        using Shape restored = ShapeExchange.ReadBrep(brepPath);
        using XdeDocument document = XdeDocument.Create();
        using (XdeTransaction transaction = document.BeginTransaction())
        {
            XdeLabel part = document.AddPart(restored, new XdePartMetadata(
                "Common API Part",
                new XdeColor(0.15, 0.45, 0.85),
                ["Mechanical", "Demo"],
                new XdeMaterial("Steel", "Sample material", 7.85, "Density", "g/cm3")));
            XdeLabel assembly = document.AddAssembly("Common API Assembly");
            using TopLocLocation identity = TopLocLocation.Identity;
            _ = document.AddComponent(assembly, part, identity);
            transaction.Commit();
        }
        document.WriteStep(stepPath);

        Console.WriteLine($"Closed/valid: {topology.IsClosed}/{topology.IsValid}");
        Console.WriteLine($"Unique V/E/F: {topology.UniqueCounts.VertexCount}/{topology.UniqueCounts.EdgeCount}/{topology.UniqueCounts.FaceCount}");
        Console.WriteLine($"Detailed mesh: {mesh.Vertices.Count} nodes, {mesh.TriangleCount} triangles, {mesh.FaceCount} faces, UV={mesh.HasUv}");
        Console.WriteLine($"BREP: {brepPath}");
        Console.WriteLine($"XDE STEP: {stepPath}");
        return 0;
    }
}
