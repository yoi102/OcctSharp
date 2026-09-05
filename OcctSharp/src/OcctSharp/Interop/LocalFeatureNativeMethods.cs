using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct LocalFeatureInfoRaw
{
    internal int Operation, Ready, Done, Valid, Partial, AlgorithmStatus, Composed, GroupSupport;
    internal int ContourCount, EdgeCount, SectionCount, FaultCount, HistoryCount, Reserved;
}
[StructLayout(LayoutKind.Sequential)]
internal struct ContourInfoRaw
{
    internal int Index, Program, Seed, FirstVertex, LastVertex, Closed, Tangent, Reserved;
    internal double Length;
    internal double LawProbeError;
    internal int LawSampleCount, LawApproximated;
}
[StructLayout(LayoutKind.Sequential)]
internal struct ContourEdgeRaw
{
    internal int Contour, Ordinal, SourceIndex, FirstVertex, LastVertex, Reserved;
    internal double FirstParameter, LastParameter;
}
[StructLayout(LayoutKind.Sequential)]
internal struct FilletSectionRaw
{
    internal int Contour, Patch, Ordinal, Reserved;
    internal XyzRaw Center, Normal, XDirection;
    internal double Radius, FirstParameter, LastParameter;
}
[StructLayout(LayoutKind.Sequential)]
internal struct LocalFeatureFaultRaw { internal int Kind, Contour, SourceIndex, Status; }
[StructLayout(LayoutKind.Sequential)]
internal struct LocalFeatureHistoryRaw { internal int ArgumentIndex, TopologyIndex, SourceKind, Kind, Group, ResultTopologyIndex; }
[StructLayout(LayoutKind.Sequential)]
internal struct RadiusSampleRaw { internal double Parameter, Radius; }
[StructLayout(LayoutKind.Sequential)]
internal struct VertexRadiusRaw { internal int Vertex, Reserved; internal double Radius; }
[StructLayout(LayoutKind.Sequential)]
internal struct FilletProgramRaw
{
    internal int Seed, Mode, SampleOffset, SampleCount, LawIndex, VertexOffset, VertexCount, Reserved;
    internal double Radius;
}
[StructLayout(LayoutKind.Sequential)]
internal struct FilletOptionsRaw
{
    internal int Action, Representation, Continuity, Reserved;
    internal double TangentTolerance, Tolerance3d, Tolerance2d, Approximation3d, Approximation2d, Deflection, AngularTolerance;
}
[StructLayout(LayoutKind.Sequential)]
internal struct ChamferProgramRaw { internal int Seed, Support, Method, Reserved; internal double First, Second; }
[StructLayout(LayoutKind.Sequential)]
internal struct FaceDraftProgramRaw
{
    internal int Face, Propagation, Reserved1, Reserved2;
    internal double Angle;
    internal XyzRaw Direction, PlaneOrigin, PlaneNormal;
}
[StructLayout(LayoutKind.Sequential)]
internal struct ShellDraftOptionsRaw
{
    internal int LimitKind, Keep, InternalDraft, Transition;
    internal double Angle, Length, AngleMinimum, AngleMaximum;
    internal XyzRaw Direction;
}
[StructLayout(LayoutKind.Sequential)]
internal struct SlidingPairRaw { internal int EdgeInput, FaceInput; }
[StructLayout(LayoutKind.Sequential)]
internal struct LimitedFeatureOptionsRaw
{
    internal int Operation, LimitMode, Fuse, Modify, SupportInput, FromInput, UntilInput, PathInput;
    internal double Extent, DraftAngle;
    internal XyzRaw Origin, Direction;
}
[StructLayout(LayoutKind.Sequential)]
internal struct RibSlotOptionsRaw
{
    internal int Revolution, Fuse, Sliding, AngularLimit;
    internal XyzRaw PlaneOrigin, PlaneNormal, Direction1, Direction2, AxisOrigin, AxisDirection;
    internal double Thickness1, Thickness2, AngleFirst, AngleLast;
}
[StructLayout(LayoutKind.Sequential)]
internal struct LocalHoleOptionsRaw
{
    internal int Mode, Reserved1, Reserved2, Reserved3;
    internal XyzRaw Origin, Direction;
    internal double Radius, First, Last;
}

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_local_feature_source_subshape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocalFeatureSourceSubshape(ShapeHandle source, int index, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_contour_fillet")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ContourFillet(ShapeHandle source, FilletProgramRaw* programs, int count,
        RadiusSampleRaw* samples, int sampleCount, VertexRadiusRaw* vertices, int vertexCount,
        LawInputRaw* laws, int lawCount, in FilletOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_contour_chamfer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ContourChamfer(ShapeHandle source, ChamferProgramRaw* programs, int count,
        int mode, int build, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_face_draft")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FaceDraft(ShapeHandle source, FaceDraftProgramRaw* programs, int count, int build, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shell_draft")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ShellDraft(nint* inputs, int count, in ShellDraftOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_limited_prism")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus LimitedPrism(nint* inputs, int count, SlidingPairRaw* sliding, int slidingCount,
        in LimitedFeatureOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_limited_sweep")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus LimitedSweep(nint* inputs, int count, SlidingPairRaw* sliding, int slidingCount,
        in LimitedFeatureOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_rib_slot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RibSlot(nint* inputs, int count, SlidingPairRaw* sliding, int slidingCount,
        in RibSlotOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_local_hole")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocalHole(ShapeHandle source, in LocalHoleOptionsRaw options, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_local_feature_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus LocalFeatureSnapshot(FeatureResultHandle result, out LocalFeatureInfoRaw info,
        ContourInfoRaw* contours, int contourCapacity, ContourEdgeRaw* edges, int edgeCapacity,
        FilletSectionRaw* sections, int sectionCapacity, LocalFeatureFaultRaw* faults, int faultCapacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_local_feature_history")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocalFeatureHistory(FeatureResultHandle result, int index, out LocalFeatureHistoryRaw info, out nint shape);
}
