using Microsoft.Win32.SafeHandles;
namespace OcctSharp.Interop;
internal sealed class MatrixHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal MatrixHandle(nint handle) : base(true) => SetHandle(handle);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseMatrix(handle); return true; }
}
