using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class ViewerHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal ViewerHandle(nint handle)
        : base(true) => SetHandle(handle);

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseViewer(handle);
        return true;
    }
}
