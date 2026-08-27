namespace OcctSharp;

/// <summary>Provides metadata-preserving STEPCAF/XDE assembly workflows.</summary>
[Obsolete("Use XdeDocument.ImportStep, AddAssembly, AddComponent, and WriteStep for composable XDE workflows.")]
public static class StepAssembly
{
    /// <summary>
    /// Reads STEP files through STEPCAF, preserves their XDE label trees and metadata,
    /// places them below one assembly root, and writes a new STEP file.
    /// </summary>
    public static string WriteXde(IEnumerable<StepAssemblyInput> inputs, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        StepAssemblyInput[] items = inputs.ToArray();
        if (items.Length == 0)
        {
            throw new ArgumentException("At least one STEP input is required.", nameof(inputs));
        }

        using XdeDocument document = XdeDocument.Create();
        using (XdeTransaction transaction = document.BeginTransaction())
        {
            XdeLabel assembly = document.AddAssembly("OcctSharp Assembly");
            foreach (StepAssemblyInput item in items)
            {
                if (item is null)
                {
                    throw new ArgumentException("STEP inputs cannot contain null entries.", nameof(inputs));
                }
                using GpTrsf transform = item.Transform.ToGpTrsf();
                using TopLocLocation location = TopLocLocation.FromTransform(transform);
                foreach (XdeLabel importedRoot in document.ImportStep(item.FilePath))
                    _ = document.AddComponent(assembly, importedRoot, location);
            }
            _ = transaction.Commit();
        }
        return document.WriteStep(outputPath);
    }
}
