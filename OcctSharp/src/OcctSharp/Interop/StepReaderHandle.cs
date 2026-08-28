using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

internal sealed class StepReaderHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal StepReaderHandle(nint handle)
        : base(true) => SetHandle(handle);

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseStepReader(handle);
        return true;
    }
}
