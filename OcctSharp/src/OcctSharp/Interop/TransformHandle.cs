using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class TransformHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal TransformHandle()
        : base(true)
    {
    }

    internal TransformHandle(nint handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseTransform(handle);
        return true;
    }
}
