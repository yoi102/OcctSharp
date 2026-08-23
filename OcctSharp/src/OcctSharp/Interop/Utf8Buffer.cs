using System.Runtime.InteropServices;
using System.Text;

namespace OcctSharp.Interop;

internal sealed class Utf8Buffer : IDisposable
{
    private Utf8Buffer(nint pointer, int length)
    {
        Pointer = pointer;
        Length = length;
    }

    internal nint Pointer { get; }
    internal int Length { get; }

    internal static Utf8Buffer FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        nint pointer = Marshal.AllocHGlobal(Math.Max(1, bytes.Length));
        if (bytes.Length > 0)
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
        }

        return new Utf8Buffer(pointer, bytes.Length);
    }

    public void Dispose() => Marshal.FreeHGlobal(Pointer);
}
