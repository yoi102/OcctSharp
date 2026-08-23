namespace OcctSharp.Samples;

internal static class XdeStepAssemblySample
{
    public static int Run()
    {
        string output = SampleConsole.ReadOutputPath("assembled.step");
        string[] inputs = SampleConsole.ReadStepInputs();

        if (inputs.Length == 0)
        {
            throw new InvalidOperationException(
                "No STEP inputs were supplied and no STEP files were found in the repository data directory.");
        }

        StepAssemblyInput[] placements = inputs
            .Select(static (path, index) => new StepAssemblyInput(
                path,
                ShapeTransform.CreateTranslationAndRotationZ(
                    (index % 3) * 60.0,
                    (index / 3) * 60.0,
                    index * 3.0,
                    index * 15.0)))
            .ToArray();

        StepMetadataSummary inputMetadata = StepMetadataSummary.Sum(inputs);
        string writtenPath = StepAssembly.WriteXde(placements, output);
        StepMetadataSummary outputMetadata = StepMetadataSummary.FromFile(writtenPath);
        Console.WriteLine(
            $"Wrote STEPCAF/XDE assembly with {placements.Length} transformed inputs: {writtenPath}");
        Console.WriteLine($"Input metadata:  {inputMetadata}");
        Console.WriteLine($"Output metadata: {outputMetadata}");
        return 0;
    }
}




