using Microsoft.Win32.SafeHandles;
namespace OcctSharp.Interop;
internal sealed class VectorHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal VectorHandle(nint handle) : base(true) => SetHandle(handle);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseVector(handle); return true; }
}
