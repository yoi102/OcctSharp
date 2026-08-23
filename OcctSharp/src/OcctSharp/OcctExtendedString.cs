using System.Runtime.InteropServices;
using System.Text;
using OcctSharp.Interop;

#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an OCCT UTF-16 value and exposes explicit UTF-8 conversion.</summary>
public sealed class OcctExtendedString : IDisposable
{
    private readonly ExtendedStringHandle handle;
    private OcctExtendedString(ExtendedStringHandle handle) => this.handle = handle;
    internal static OcctExtendedString FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native extended string handle was null.") : new(new ExtendedStringHandle(value));
    public static OcctExtendedString Create(string value) { using Utf8Buffer buffer = Utf8Buffer.FromString(value); NativeError.ThrowIfFailed(NativeMethods.CreateExtended(buffer.Pointer, buffer.Length, out nint result), "extended_create_utf8"); return FromNative(result); }
    public int Length { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetExtendedLength(handle, out int length), "extended_length"); return length; } }
    public int Utf8Length { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetExtendedUtf8Length(handle, out int length), "extended_utf8_length"); return length; } }
    public bool IsEmpty => Length == 0;
    public string Value { get { ThrowIfDisposed(); int capacity = Utf8Length + 1; nint memory = Marshal.AllocHGlobal(capacity); try { NativeError.ThrowIfFailed(NativeMethods.CopyExtendedUtf8(handle, memory, capacity, out int written), "extended_to_utf8"); byte[] bytes = new byte[written]; if (written > 0) Marshal.Copy(memory, bytes, 0, written); return Encoding.UTF8.GetString(bytes); } finally { Marshal.FreeHGlobal(memory); } } }
    public char this[int index] { get { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.GetExtendedValue(handle, index + 1, out ushort value), "extended_value"); return (char)value; } }
    public OcctExtendedString Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneExtended(handle, out nint value), "extended_clone"); return FromNative(value); }
    public void Append(string value) { ThrowIfDisposed(); using Utf8Buffer buffer = Utf8Buffer.FromString(value); NativeError.ThrowIfFailed(NativeMethods.AppendExtendedUtf8(handle, buffer.Pointer, buffer.Length), "extended_append_utf8"); }
    public OcctAsciiString ToAscii() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.ConvertExtendedToAscii(handle, out nint value), "extended_to_ascii"); return OcctAsciiString.FromNative(value); }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
