using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class RealArrayHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal RealArrayHandle() : base(true) { }
    internal RealArrayHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseRealArray(handle); return true; }
}
