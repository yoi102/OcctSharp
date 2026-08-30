using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct FeatureOptionsRaw
{
    internal double FuzzyTolerance;
    internal int RunParallel;
    internal int NonDestructive;
    internal int GlueMode;
    internal int RepairInputs;
    internal int UnifyResult;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FeatureResultInfoRaw
{
    internal int Operation;
    internal int Succeeded;
    internal int Recovered;
    internal int ErrorCount;
    internal int WarningCount;
    internal int FaultyShapeCount;
    internal int ModifiedCount;
    internal int GeneratedCount;
    internal int DeletedCount;
    internal int ResultIsValid;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FeatureHistoryInfoRaw
{
    internal int SourceIndex;
    internal int Kind;
}

internal sealed class FeatureResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal FeatureResultHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() { NativeMethods.ReleaseFeatureResult(handle); return true; }
}

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_execute")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ExecuteFeature(
        int operation, nint* shapes, int shapeCount, int primaryCount, int secondaryCount,
        double* parameters, int parameterCount, XyzRaw* vectors, int vectorCount,
        FeatureOptionsRaw options, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFeatureResultInfo(FeatureResultHandle result, out FeatureResultInfoRaw info);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFeatureResultShape(FeatureResultHandle result, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_history")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFeatureResultHistory(
        FeatureResultHandle result, int index, out FeatureHistoryInfoRaw info, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_deleted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFeatureResultDeleted(FeatureResultHandle result, int index, out int sourceIndex);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_message")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetFeatureResultMessage(
        FeatureResultHandle result, byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_feature_result_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseFeatureResult(nint result);
}
