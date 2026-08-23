using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Provides identity and compatibility checks for the loaded native runtime.</summary>
public static class OcctRuntime
{
    private const int ExpectedAbiMajor = 1;
    private static readonly Lazy<OcctRuntimeInfo> RuntimeInfo = new(LoadAndValidate);

    /// <summary>Gets information about the loaded OcctSharp and OCCT runtime.</summary>
    public static OcctRuntimeInfo Info => RuntimeInfo.Value;

    internal static void EnsureCompatible() => _ = RuntimeInfo.Value;

    private static OcctRuntimeInfo LoadAndValidate()
    {
        uint encodedAbiVersion = NativeMethods.GetAbiVersion();
        int abiMajor = (int)(encodedAbiVersion >> 16);
        int abiMinor = (int)(encodedAbiVersion & 0xFFFFU);

        if (abiMajor != ExpectedAbiMajor)
        {
            throw new BadImageFormatException(
                $"OcctSharp native ABI {abiMajor}.{abiMinor} is incompatible with expected major version {ExpectedAbiMajor}.");
        }

        string bridgeVersion = ReadUtf8(NativeMethods.GetBridgeVersion(), "native bridge version");
        string occtVersion = ReadUtf8(NativeMethods.GetOcctVersion(), "OCCT version");
        string managedVersion = typeof(OcctRuntime).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? typeof(OcctRuntime).Assembly.GetName().Version?.ToString()
            ?? "unknown";

        return new OcctRuntimeInfo(
            managedVersion,
            bridgeVersion,
            new Version(abiMajor, abiMinor),
            occtVersion);
    }

    private static string ReadUtf8(nint value, string field)
    {
        return Marshal.PtrToStringUTF8(value)
            ?? throw new BadImageFormatException($"The {field} returned by the native bridge is null.");
    }
}
