namespace OcctSharp.Samples;

internal static class SampleConsole
{
    public static void WriteMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== OcctSharp 示例 ===");
        Console.WriteLine("1. 创建实体");
        Console.WriteLine("2. 创建实体并输出 STEP");
        Console.WriteLine("3. 创建实体并输出 STL");
        Console.WriteLine("4. 创建实体并输出 IGES");
        Console.WriteLine("5. 读取多个 STEP、变换并合并为 XDE STEP");
        Console.WriteLine("0. 退出");
        Console.Write("请选择操作：");
    }

    public static string ReadOutputPath(string defaultFileName)
    {
        Console.Write($"输出文件路径（直接回车使用 artifacts/samples/{defaultFileName}）：");
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input)
            ? SamplePaths.GetDefaultOutputPath(defaultFileName)
            : Path.GetFullPath(input);
    }

    public static string[] ReadStepInputs()
    {
        Console.Write("STEP 输入文件数量（直接回车使用 data 文件夹中的全部 STEP）：");
        string? countText = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(countText))
        {
            return SamplePaths.FindDefaultStepInputs();
        }

        if (!int.TryParse(countText, out int count) || count <= 0)
        {
            throw new ArgumentException("STEP 文件数量必须是大于零的整数。");
        }

        string[] paths = new string[count];
        for (int index = 0; index < count; index++)
        {
            Console.Write($"第 {index + 1} 个 STEP 文件路径：");
            string? path = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("STEP 文件路径不能为空。");
            }

            paths[index] = Path.GetFullPath(path);
        }

        return paths;
    }

    public static int InvalidChoice()
    {
        Console.WriteLine("无效选择，请输入菜单中的数字。");
        return 2;
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("按回车返回主菜单...");
        Console.ReadLine();
    }
}
