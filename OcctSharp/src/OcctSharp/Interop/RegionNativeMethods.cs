using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct RegionInfoRaw { internal int Done, Valid, ItemCount, CellCount, OutputCount, Warnings, Reserved1, Reserved2; }
[StructLayout(LayoutKind.Sequential)]
internal struct RegionItemRaw { internal int Kind, A, B, C, D, Flags; internal double Measure; }
[StructLayout(LayoutKind.Sequential)]
internal struct PartitionOptionsRaw { internal double Fuzzy; internal int Parallel, CheckInputs, MaxCells, Reserved; }
[StructLayout(LayoutKind.Sequential)]
internal struct RegionRuleRaw { internal int Output, Action, Material, Offset, Count, Dimension; internal double MaximumMeasure; }
[StructLayout(LayoutKind.Sequential)]
internal struct RegionOutputRaw { internal int RemoveBoundaries, Containers, Reserved1, Reserved2; }
[StructLayout(LayoutKind.Sequential)]
internal struct VolumeOptionsRaw { internal double Fuzzy; internal int Intersect, AvoidInternal, Parallel, MaxSolids; }

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_region_classify_solid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RegionClassifySolid(ShapeHandle shape, XyzRaw point, double tolerance, out int state);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_partition_build")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus PartitionBuild(nint* inputs, int count, in PartitionOptionsRaw options,
        RegionRuleRaw* rules, int ruleCount, int* expressions, int expressionCount, RegionOutputRaw* outputs, int outputCount, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_volume_build")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus VolumeBuild(nint* inputs, int count, in VolumeOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_region_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RegionSnapshot(FeatureResultHandle result, out RegionInfoRaw info, RegionItemRaw* items, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_region_item_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RegionItemShape(FeatureResultHandle result, int index, out nint shape);
}
