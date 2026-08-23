using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class GPropsHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal GPropsHandle() : base(true) { }
    internal GPropsHandle(nint handle) : base(true) => SetHandle(handle);
    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseGProps(handle);
        return true;
    }
}
