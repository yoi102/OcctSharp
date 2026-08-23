using System.Reflection;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static class NativeLibraryResolver
{
    private const string NativeLibraryName = "OcctSharp.Native";
    private const string NativeDirectoryName = "occt";
    private static readonly object RegistrationGate = new();
    private static bool _registered;

    internal static void EnsureRegistered()
    {
        lock (RegistrationGate)
        {
            if (_registered)
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(
                typeof(NativeLibraryResolver).Assembly,
                Resolve);
            _registered = true;
        }
    }

    private static nint Resolve(
        string libraryName,
        Assembly assembly,
        DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeLibraryName, StringComparison.Ordinal))
        {
            return nint.Zero;
        }

        if (!OperatingSystem.IsWindows()
            || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "OcctSharp 0.1 supports only a Windows x64 process.");
        }

        string nativeDirectory = Path.Combine(AppContext.BaseDirectory, NativeDirectoryName);
        string nativePath = Path.Combine(nativeDirectory, NativeLibraryName + ".dll");
        if (!File.Exists(nativePath))
        {
            throw new DllNotFoundException(
                $"OcctSharp native runtime was not found at '{nativePath}'. "
                + "Ensure the NuGet build assets copied the package's occt directory to the application output.");
        }

        try
        {
            return NativeLibrary.Load(nativePath);
        }
        catch (Exception error) when (error is DllNotFoundException or BadImageFormatException)
        {
            throw new DllNotFoundException(
                $"OcctSharp could not load '{nativePath}' or one of its dependencies. "
                + $"Keep the complete native dependency closure in '{nativeDirectory}'.",
                error);
        }
    }
}
