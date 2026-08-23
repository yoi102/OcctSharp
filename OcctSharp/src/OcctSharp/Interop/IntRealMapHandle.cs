using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class IntRealMapHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal IntRealMapHandle() : base(true) { }
    internal IntRealMapHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseIntRealMap(handle); return true; }
}
