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

        StepMetadataSummary inputMetadata = StepMetadataSummary.Sum(inputs);
        using XdeDocument document = XdeDocument.Create();
        using (XdeTransaction transaction = document.BeginTransaction())
        {
            XdeLabel assembly = document.AddAssembly("Sample assembly");
            for (int index = 0; index < inputs.Length; ++index)
            {
                ShapeTransform placement = ShapeTransform.CreateTranslationAndRotationZ(
                    (index % 3) * 60.0,
                    (index / 3) * 60.0,
                    index * 3.0,
                    index * 15.0);
                using GpTrsf transform = placement.ToGpTrsf();
                using TopLocLocation location = TopLocLocation.FromTransform(transform);
                foreach (XdeLabel importedRoot in document.ImportStep(inputs[index]))
                    _ = document.AddComponent(assembly, importedRoot, location);
            }
            _ = transaction.Commit();
        }
        string writtenPath = document.WriteStep(output);
        StepMetadataSummary outputMetadata = StepMetadataSummary.FromFile(writtenPath);
        Console.WriteLine(
            $"Wrote STEPCAF/XDE assembly with {inputs.Length} transformed inputs: {writtenPath}");
        Console.WriteLine($"Input metadata:  {inputMetadata}");
        Console.WriteLine($"Output metadata: {outputMetadata}");
        return 0;
    }
}



