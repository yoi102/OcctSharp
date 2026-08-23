using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class OcafDocumentHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal OcafDocumentHandle()
        : base(true)
    {
    }

    internal OcafDocumentHandle(nint handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseOcafDocument(handle);
        return true;
    }
}
