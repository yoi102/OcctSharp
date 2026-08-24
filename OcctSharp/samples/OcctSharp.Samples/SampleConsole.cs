namespace OcctSharp.Samples;

internal static class SampleConsole
{
    public static void WriteMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== OcctSharp Samples ===");
        Console.WriteLine("1. Create a solid");
        Console.WriteLine("2. Create a solid and export STEP");
        Console.WriteLine("3. Create a solid and export STL");
        Console.WriteLine("4. Create a solid and export IGES");
        Console.WriteLine("5. Read, transform, and merge STEP files into an XDE STEP assembly");
        Console.WriteLine("6. Open the interactive OCCT Viewer");
        Console.WriteLine("0. Exit");
        Console.Write("Choose an operation: ");
    }

    public static string ReadOutputPath(string defaultFileName)
    {
        Console.Write($"Output path (press Enter to use artifacts/samples/{defaultFileName}): ");
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input)
            ? SamplePaths.GetDefaultOutputPath(defaultFileName)
            : Path.GetFullPath(input);
    }

    public static string[] ReadStepInputs()
    {
        Console.Write("Number of STEP input files (press Enter to use every STEP file in data): ");
        string? countText = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(countText))
        {
            return SamplePaths.FindDefaultStepInputs();
        }

        if (!int.TryParse(countText, out int count) || count <= 0)
        {
            throw new ArgumentException("The STEP file count must be a positive integer.");
        }

        string[] paths = new string[count];
        for (int index = 0; index < count; index++)
        {
            Console.Write($"Path to STEP file {index + 1}: ");
            string? path = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A STEP file path cannot be empty.");
            }

            paths[index] = Path.GetFullPath(path);
        }

        return paths;
    }

    public static int InvalidChoice()
    {
        Console.WriteLine("Invalid choice. Enter a number shown in the menu.");
        return 2;
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press Enter to return to the main menu...");
        Console.ReadLine();
    }
}
