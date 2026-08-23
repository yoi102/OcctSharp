namespace OcctSharp.Samples;

internal static class StlExportSample
{
    public static int Run()
    {
        using Shape box = SampleShape.CreateBox();
        string output = ShapeExchange.WriteStl(box, SampleConsole.ReadOutputPath("box.stl"));
        Console.WriteLine($"Wrote binary STL: {output}");
        return 0;
    }
}
