using Microsoft.Win32.SafeHandles;
namespace OcctSharp.Interop;
internal sealed class AxisHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal AxisHandle(nint handle) : base(true) => SetHandle(handle);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseAxis(handle); return true; }
}
