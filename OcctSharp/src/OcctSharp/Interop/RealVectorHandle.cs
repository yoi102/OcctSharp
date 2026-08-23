using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class RealVectorHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal RealVectorHandle() : base(true) { }
    internal RealVectorHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseRealVector(handle); return true; }
}
