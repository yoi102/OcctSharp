namespace OcctSharp;

/// <summary>Outcome reported by the OCCT STEP file reader before root transfer.</summary>
public enum StepReadStatus
{
    /// <summary>No read operation or result is available.</summary>
    Void = 0,
    /// <summary>The STEP file was read successfully.</summary>
    Done = 1,
    /// <summary>The reader encountered a recoverable input error.</summary>
    Error = 2,
    /// <summary>The reader failed to load the STEP file.</summary>
    Failed = 3,
    /// <summary>The read operation was stopped.</summary>
    Stopped = 4
}

/// <summary>Copied STEPControl transfer counts and system length unit.</summary>
public readonly record struct StepReadReport(
    int CandidateRootCount,
    int TransferredRootCount,
    int ShapeCount,
    StepReadStatus ReadStatus,
    double SystemLengthUnit);

/// <summary>Owns a STEP shape together with its copied read/transfer report.</summary>
public sealed class StepReadResult : IDisposable
{
    internal StepReadResult(Shape shape, StepReadReport report) => (Shape, Report) = (shape, report);
    /// <summary>Gets the independently owned transferred shape.</summary>
    public Shape Shape { get; }
    /// <summary>Gets the copied STEP read and transfer report.</summary>
    public StepReadReport Report { get; }
    /// <summary>Releases the transferred shape.</summary>
    public void Dispose() => Shape.Dispose();
}
