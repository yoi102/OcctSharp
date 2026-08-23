using System.Runtime.InteropServices;
using System.Text;
using OcctSharp.Interop;

#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an OCCT UTF-8 string value with byte-based indexing semantics.</summary>
public sealed class OcctAsciiString : IDisposable
{
    private readonly AsciiStringHandle handle;

    private OcctAsciiString(AsciiStringHandle handle) => this.handle = handle;

    internal static OcctAsciiString FromNative(nint value) => value == 0
        ? throw new OcctException("UnknownException", "Native ASCII string handle was null.")
        : new(new AsciiStringHandle(value));

    public static OcctAsciiString Create(string value)
    {
        using Utf8Buffer buffer = Utf8Buffer.FromString(value);
        NativeError.ThrowIfFailed(NativeMethods.CreateAscii(buffer.Pointer, buffer.Length, out nint result), "ascii_create");
        return FromNative(result);
    }

    public int Length
    {
        get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetAsciiLength(handle, out int length), "ascii_length"); return length; }
    }

    public bool IsEmpty => Length == 0;

    public string Value
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.GetAsciiLength(handle, out int length), "ascii_length");
            nint memory = Marshal.AllocHGlobal(length + 1);
            try
            {
                NativeError.ThrowIfFailed(NativeMethods.CopyAsciiUtf8(handle, memory, length + 1, out int written), "ascii_to_utf8");
                byte[] bytes = new byte[written];
                if (written > 0) Marshal.Copy(memory, bytes, 0, written);
                return Encoding.UTF8.GetString(bytes);
            }
            finally { Marshal.FreeHGlobal(memory); }
        }
    }

    public OcctAsciiString Clone()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CloneAscii(handle, out nint value), "ascii_clone");
        return FromNative(value);
    }

    public void Append(string value)
    {
        ThrowIfDisposed();
        using Utf8Buffer buffer = Utf8Buffer.FromString(value);
        NativeError.ThrowIfFailed(NativeMethods.AppendAscii(handle, buffer.Pointer, buffer.Length), "ascii_append");
    }

    public OcctExtendedString ToExtended()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.ConvertAsciiToExtended(handle, out nint value), "ascii_to_extended");
        return OcctExtendedString.FromNative(value);
    }

    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
