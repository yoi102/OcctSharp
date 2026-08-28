using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ValidationIssueRaw
{
    internal readonly int ShapeKind;
    internal readonly int Status;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct StepReadReportRaw
{
    internal readonly int CandidateRootCount;
    internal readonly int TransferredRootCount;
    internal readonly int ShapeCount;
    internal readonly int ReadStatus;
    internal readonly double SystemLengthUnit;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct StepReaderInfoRaw
{
    internal readonly int CandidateRootCount;
    internal readonly int ReadStatus;
    internal readonly double SystemLengthUnit;
    internal readonly int LengthUnitCount;
    internal readonly int AngleUnitCount;
    internal readonly int SolidAngleUnitCount;
}
