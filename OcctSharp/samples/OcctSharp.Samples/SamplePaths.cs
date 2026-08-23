namespace OcctSharp.Samples;

internal static class SamplePaths
{
    public static string GetDefaultOutputPath(string fileName) =>
        Path.GetFullPath(Path.Combine("artifacts", "samples", fileName));

    public static string[] FindDefaultStepInputs()
    {
        DirectoryInfo? directory = new(Environment.CurrentDirectory);
        while (directory is not null)
        {
            string dataDirectory = Path.Combine(directory.FullName, "data");
            string innerWorkspace = Path.Combine(directory.FullName, "OcctSharp");
            if (Directory.Exists(dataDirectory) && Directory.Exists(innerWorkspace))
            {
                return Directory.EnumerateFiles(dataDirectory)
                    .Where(static path =>
                        string.Equals(Path.GetExtension(path), ".step", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Path.GetExtension(path), ".stp", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            directory = directory.Parent;
        }

        return [];
    }
}
