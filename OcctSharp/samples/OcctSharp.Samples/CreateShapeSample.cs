namespace OcctSharp.Samples;

internal static class CreateShapeSample
{
    public static int Run()
    {
        using Shape box = SampleShape.CreateBox();
        Console.WriteLine($"Created a 40 x 30 x 20 box with {box.FaceCount} faces.");
        return 0;
    }
}
