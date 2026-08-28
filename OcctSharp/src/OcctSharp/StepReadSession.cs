using System.Text;
using System.Collections.ObjectModel;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Copied STEP file unit names grouped by physical quantity.</summary>
public sealed record StepFileUnits(
    IReadOnlyList<string> Length,
    IReadOnlyList<string> Angle,
    IReadOnlyList<string> SolidAngle);

/// <summary>Immutable metadata captured when a STEP reader session opens.</summary>
public sealed record StepReadSessionInfo(
    int CandidateRootCount,
    StepReadStatus ReadStatus,
    double SystemLengthUnit,
    StepFileUnits FileUnits);

/// <summary>
/// Owns one parsed STEP reader so callers can inspect units and selectively transfer roots.
/// Returned shapes are independent owners and survive session disposal.
/// </summary>
public sealed class StepReadSession : IDisposable
{
    private readonly object sync = new();
    private readonly StepReaderHandle handle;

    private StepReadSession(StepReaderHandle handle, StepReadSessionInfo info)
    {
        this.handle = handle;
        Info = info;
    }

    /// <summary>Gets immutable file and transfer metadata.</summary>
    public StepReadSessionInfo Info { get; }

    /// <summary>
    /// Parses a STEP file once. A null target unit keeps OCCT's default system length unit;
    /// otherwise the positive finite OCCT length factor is applied before any transfer.
    /// </summary>
    public static StepReadSession Open(string filePath, double? targetSystemLengthUnit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The STEP input file does not exist.", fullPath);
        if (targetSystemLengthUnit is double unit && (!double.IsFinite(unit) || unit <= 0))
            throw new ArgumentOutOfRangeException(nameof(targetSystemLengthUnit));

        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.OpenStepReader(
                fullPath,
                targetSystemLengthUnit ?? 0.0,
                out nint nativeReader,
                out StepReaderInfoRaw raw),
            "step_reader_open");
        StepReaderHandle handle = new(nativeReader);
        try
        {
            StepFileUnits units = new(
                ReadUnits(handle, 0, raw.LengthUnitCount),
                ReadUnits(handle, 1, raw.AngleUnitCount),
                ReadUnits(handle, 2, raw.SolidAngleUnitCount));
            return new StepReadSession(
                handle,
                new StepReadSessionInfo(
                    raw.CandidateRootCount,
                    (StepReadStatus)raw.ReadStatus,
                    raw.SystemLengthUnit,
                    units));
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    /// <summary>Transfers one zero-based candidate root to an independent owning shape.</summary>
    public Shape TransferRoot(int rootIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rootIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rootIndex, Info.CandidateRootCount);
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.TransferStepReaderRoot(handle, rootIndex, out nint nativeShape),
                "step_reader_transfer_root");
            return ShapeFactory.FromNativeHandle(nativeShape, "step_reader_transfer_root");
        }
    }

    /// <summary>
    /// Transfers selected zero-based roots, or every candidate root when indices are omitted.
    /// Each returned shape has independent ownership.
    /// </summary>
    public Shape[] TransferRoots(IEnumerable<int>? rootIndices = null)
    {
        int[] indices = rootIndices?.ToArray() ?? Enumerable.Range(0, Info.CandidateRootCount).ToArray();
        if (indices.Length == 0) throw new ArgumentException("At least one STEP root must be selected.", nameof(rootIndices));
        if (indices.Distinct().Count() != indices.Length)
            throw new ArgumentException("STEP root indices must be unique.", nameof(rootIndices));
        Shape[] result = new Shape[indices.Length];
        int transferred = 0;
        try
        {
            for (; transferred < indices.Length; ++transferred) result[transferred] = TransferRoot(indices[transferred]);
            return result;
        }
        catch
        {
            for (int index = 0; index < transferred; ++index) result[index].Dispose();
            throw;
        }
    }

    /// <summary>Releases the parsed STEP model. Already transferred shapes remain valid.</summary>
    public void Dispose()
    {
        lock (sync) handle.Dispose();
    }

    private static unsafe ReadOnlyCollection<string> ReadUnits(StepReaderHandle handle, int kind, int count)
    {
        string[] result = new string[count];
        for (int index = 0; index < count; ++index)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetStepReaderUnitUtf8Length(handle, kind, index, out int length),
                "step_reader_unit_utf8_length");
            byte[] bytes = new byte[length];
            fixed (byte* pointer = bytes)
            {
                NativeError.ThrowIfFailed(
                    NativeMethods.CopyStepReaderUnitUtf8(handle, kind, index, pointer, bytes.Length, out int written),
                    "step_reader_unit_to_utf8");
                if (written != bytes.Length) Array.Resize(ref bytes, written);
            }
            result[index] = Encoding.UTF8.GetString(bytes);
        }
        return Array.AsReadOnly(result);
    }
}
