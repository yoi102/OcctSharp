using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class RealSequenceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal RealSequenceHandle() : base(true) { }
    internal RealSequenceHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseRealSequence(handle); return true; }
}
