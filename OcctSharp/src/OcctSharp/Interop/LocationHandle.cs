using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class LocationHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal LocationHandle()
        : base(true)
    {
    }

    internal LocationHandle(nint handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseLocation(handle);
        return true;
    }
}
