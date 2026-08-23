using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class SharedTransientHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal SharedTransientHandle()
        : base(true)
    {
    }

    internal SharedTransientHandle(nint handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseTransient(handle);
        return true;
    }
}
