namespace OcctSharp.Samples;

internal static class StepExportSample
{
    public static int Run()
    {
        using Shape box = SampleShape.CreateBox();
        string output = ShapeExchange.WriteStep(box, SampleConsole.ReadOutputPath("box.step"));
        Console.WriteLine($"Wrote STEP: {output}");
        return 0;
    }
}
