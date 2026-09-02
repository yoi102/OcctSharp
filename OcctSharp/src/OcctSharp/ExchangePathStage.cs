namespace OcctSharp;

/// <summary>Hides narrow OCCT file APIs behind cleanup-safe ASCII staging.</summary>
internal sealed class ExchangePathStage : IDisposable
{
    private readonly bool output;
    private bool completed;

    private ExchangePathStage(string originalPath, string nativePath, bool output)
    {
        OriginalPath = originalPath;
        NativePath = nativePath;
        this.output = output;
    }

    internal string OriginalPath { get; }
    internal string NativePath { get; }

    internal static ExchangePathStage ForInput(string fullPath)
    {
        if (IsAscii(fullPath)) return new(fullPath, fullPath, false);
        string nativePath = CreateTemporaryPath();
        try
        {
            File.Copy(fullPath, nativePath, true);
            return new(fullPath, nativePath, false);
        }
        catch
        {
            TryDelete(nativePath);
            throw;
        }
    }

    internal static ExchangePathStage ForOutput(string fullPath) =>
        IsAscii(fullPath) ? new(fullPath, fullPath, true) : new(fullPath, CreateTemporaryPath(), true);

    internal void Complete()
    {
        if (!output || completed) return;
        if (!string.Equals(OriginalPath, NativePath, StringComparison.Ordinal))
        {
            string? originalRoot = Path.GetPathRoot(OriginalPath);
            string? nativeRoot = Path.GetPathRoot(NativePath);
            if (string.Equals(originalRoot, nativeRoot, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(NativePath, OriginalPath, true);
            }
            else
            {
                File.Copy(NativePath, OriginalPath, true);
                File.Delete(NativePath);
            }
        }
        completed = true;
    }

    public void Dispose()
    {
        if (!string.Equals(OriginalPath, NativePath, StringComparison.Ordinal))
            TryDelete(NativePath);
    }

    private static string CreateTemporaryPath()
    {
        string directory = Path.GetTempPath();
        if (!IsAscii(directory))
            throw new IOException("The system temporary directory is not representable by the native OCCT narrow-path API.");
        return Path.Combine(directory, $"occtsharp-exchange-{Guid.NewGuid():N}.tmp");
    }

    private static bool IsAscii(string value)
    {
        foreach (char character in value)
        {
            if (character > 0x7f) return false;
        }
        return true;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
