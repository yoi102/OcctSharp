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

        string? nativeMessage = Marshal.PtrToStringUTF8(NativeMethods.GetLastError());
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
