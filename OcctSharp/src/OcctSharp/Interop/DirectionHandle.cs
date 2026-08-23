using Microsoft.Win32.SafeHandles;
namespace OcctSharp.Interop;
internal sealed class DirectionHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal DirectionHandle(nint handle) : base(true) => SetHandle(handle);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseDirection(handle); return true; }
}
