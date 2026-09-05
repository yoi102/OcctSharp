using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

[StructLayout(LayoutKind.Sequential)]
internal struct LawSpanRaw
{
    internal int Kind, Degree, Tangents, ValueOffset, ValueCount, ParameterOffset, ParameterCount, MultiplicityOffset;
    internal double First, Last, ValueFirst, ValueLast, DerivativeFirst, DerivativeLast;
    internal double ActiveFirst, ActiveLast;
}
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct LawInputRaw
{
    internal LawSpanRaw* Spans;
    internal double* Values;
    internal int* Multiplicities;
    internal int SpanCount, ValueCount, MultiplicityCount, Reserved;
    internal double First, Last;
}
[StructLayout(LayoutKind.Sequential)]
internal struct LawSampleRaw
{
    internal double Parameter, Value, FirstDerivative, SecondDerivative;
    internal int Defined, Reserved;
}
internal static partial class ScalarLawInterop
{
    static ScalarLawInterop() => NativeLibraryResolver.EnsureRegistered(typeof(ScalarLawInterop).Assembly);
    [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_law_evaluate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus Evaluate(in LawInputRaw input, double* parameters, int count,
        LawSampleRaw* samples, int capacity, out double conservativeLowerBound);
}
