using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static class NativeError
{
    internal static void ThrowIfFailed(NativeStatus status, string operation)
    {
        if (status is NativeStatus.Success)
        {
            return;
        }

        string? nativeMessage = Marshal.PtrToStringUTF8(RuntimeNativeMethods.GetLastError());
        string message = string.IsNullOrWhiteSpace(nativeMessage)
            ? $"Native operation '{operation}' failed with status {status}."
            : nativeMessage;

        if (status is NativeStatus.InvalidArgument)
        {
            throw new ArgumentException(message);
        }

        if (status is NativeStatus.InvalidHandle or NativeStatus.NullHandle)
        {
            throw new ObjectDisposedException(operation, message);
        }

        if (status is NativeStatus.TypeMismatch)
        {
            throw new InvalidCastException(message);
        }

        throw new OcctException(status.ToString(), message);
    }
}

internal static partial class RuntimeNativeMethods
{
    private const string LibraryName = "OcctSharp.Native";

    static RuntimeNativeMethods() =>
        NativeLibraryResolver.EnsureRegistered(typeof(RuntimeNativeMethods).Assembly);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_abi_version")]
    internal static partial uint GetAbiVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_bridge_version")]
    internal static partial nint GetBridgeVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_occt_version")]
    internal static partial nint GetOcctVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_last_error")]
    internal static partial nint GetLastError();
}
