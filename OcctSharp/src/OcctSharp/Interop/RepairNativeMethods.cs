using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairTopologyRaw(int Index, int Kind, int Orientation, int ParentIndex, double Tolerance);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairFindingRaw(int Kind, int SourceIndex, int RelatedIndex, int Status, double Value, double Limit);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairMetricsRaw(int Valid, int TopologyCount, int AreaAvailable, int VolumeAvailable,
    double MaximumTolerance, double Area, double Volume, double MaximumGap);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairInspectionRaw(double Tolerance, double SmallLength, double SmallArea, double ToleranceOutlier);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairStageRaw(int Operation, int Mode1, int Mode2, int Mode3, int Parts, int MaximumTopology,
    double Tolerance, double MaximumTolerance, double Threshold, double Angle);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairRelationRaw(int SourceIndex, int ResultIndex, int Kind, int Reserved);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RepairBoundaryRaw(int Closed, int AreaAvailable, int EdgeCount, int Reserved, double Length, double Area, double EndpointGap);

internal sealed class RepairResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal RepairResultHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() => NativeMethods.RepairResultRelease(handle) == NativeStatus.Success;
}

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_xde_subshape_label", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairXdeSubshapeLabel(OcafDocumentHandle document, string definitionEntry,
        int index, byte* entry, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_xde_apply", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairXdeApply(OcafDocumentHandle document, string definitionEntry, ShapeHandle candidate,
        RepairRelationRaw* history, int historyCount, int apply, int* conflicts, int capacity,
        out int conflictCount, out int mappedCount, out int occurrenceCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_viewer_select")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RepairViewerSelect(ViewerHandle viewer, long presentationId);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_serialized")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairSerialized(ShapeHandle shape, byte* output, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RepairCopy(ShapeHandle shape, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_topology")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairTopology(ShapeHandle shape, RepairTopologyRaw* output, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_subshape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RepairSubshape(ShapeHandle shape, int index, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_inspect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairInspect(ShapeHandle shape, in RepairInspectionRaw options,
        out RepairMetricsRaw metrics, RepairFindingRaw* findings, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_boundary")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairBoundary(ShapeHandle shape, double tolerance, int index,
        out RepairBoundaryRaw info, int* edges, int capacity, out int edgeCount, out int boundaryCount, out nint wire);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_execute")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairExecute(ShapeHandle source, in RepairStageRaw stage,
        int* selected, int selectedCount, int* protectedIndices, int protectedCount, nint* replacements, int replacementCount, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_result_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RepairResultShape(RepairResultHandle result, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_result_history")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairResultHistory(RepairResultHandle result, RepairRelationRaw* output, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_result_findings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RepairResultFindings(RepairResultHandle result, RepairFindingRaw* output, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_repair_result_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RepairResultRelease(nint result);
}
