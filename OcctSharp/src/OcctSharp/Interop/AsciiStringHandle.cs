using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class AsciiStringHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal AsciiStringHandle() : base(true) { }
    internal AsciiStringHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseAscii(handle); return true; }
}
