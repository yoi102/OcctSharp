using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class ExtendedStringHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal ExtendedStringHandle() : base(true) { }
    internal ExtendedStringHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseExtended(handle); return true; }
}
