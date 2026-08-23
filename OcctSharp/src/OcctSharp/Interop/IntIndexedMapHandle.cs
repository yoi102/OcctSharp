using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class IntIndexedMapHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal IntIndexedMapHandle() : base(true) { }
    internal IntIndexedMapHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseIntIndexedMap(handle); return true; }
}
