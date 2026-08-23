using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class ShapeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal ShapeHandle()
        : base(true)
    {
    }

    internal ShapeHandle(nint handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseShape(handle);
        return true;
    }
}
