namespace OcctSharp.Samples;

internal static class Program
{
    public static int Main()
    {
        while (true)
        {
            SampleConsole.WriteMenu();
            string? choice = Console.ReadLine()?.Trim();
            if (choice is null || choice == "0")
            {
                Console.WriteLine("程序结束。");
                return 0;
            }

            try
            {
                _ = choice switch
                {
                    "1" => CreateShapeSample.Run(),
                    "2" => StepExportSample.Run(),
                    "3" => StlExportSample.Run(),
                    "4" => IgesExportSample.Run(),
                    "5" => XdeStepAssemblySample.Run(),
                    "6" => ViewerSample.Run(),
                    _ => SampleConsole.InvalidChoice(),
                };

                SampleConsole.Pause();
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine($"操作失败：{error.Message}");
                SampleConsole.Pause();
            }
        }
    }
}
