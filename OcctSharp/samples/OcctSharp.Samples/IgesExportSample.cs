namespace OcctSharp.Samples;

internal static class IgesExportSample
{
    public static int Run()
    {
        using Shape box = SampleShape.CreateBox();
        string output = ShapeExchange.WriteIges(box, SampleConsole.ReadOutputPath("box.iges"));
        Console.WriteLine($"Wrote IGES: {output}");
        return 0;
    }
}
