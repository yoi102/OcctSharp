using System.Collections;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns a registry-backed OCCT <c>NCollection_Sequence&lt;double&gt;</c>.</summary>
public sealed class OcctRealSequence : IReadOnlyList<double>, IDisposable
{
    private readonly RealSequenceHandle handle;
    private OcctRealSequence(RealSequenceHandle handle) => this.handle = handle;
    internal static OcctRealSequence FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native real sequence handle was null.") : new(new RealSequenceHandle(value));
    public static OcctRealSequence Create(IEnumerable<double> values) { ArgumentNullException.ThrowIfNull(values); double[] copy = values.ToArray(); nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * copy.Length)); try { if (copy.Length > 0) Marshal.Copy(copy, 0, memory, copy.Length); NativeError.ThrowIfFailed(NativeMethods.CreateRealSequence(memory, copy.Length, out nint value), "real_sequence_create"); return FromNative(value); } finally { Marshal.FreeHGlobal(memory); } }
    public int Count { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetRealSequenceLength(handle, out int length), "real_sequence_length"); return length; } }
    public double this[int index] { get { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.GetRealSequenceValue(handle, index + 1, out double value), "real_sequence_value"); return value; } }
    public OcctRealSequence Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneRealSequence(handle, out nint value), "real_sequence_clone"); return FromNative(value); }
    public void Add(double value) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.AppendRealSequence(handle, value), "real_sequence_append"); }
    public void Set(int index, double value) { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.SetRealSequenceValue(handle, index + 1, value), "real_sequence_set_value"); }
    public void RemoveAt(int index) { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.RemoveRealSequence(handle, index + 1), "real_sequence_remove"); }
    /// <summary>Copies the current values in one native call; the returned array is independent of this collection.</summary>
    public double[] Snapshot()
    {
        ThrowIfDisposed(); int count = Count;
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * count));
        try { NativeError.ThrowIfFailed(NativeMethods.SnapshotRealSequence(handle, memory, count, out int written), "real_sequence_snapshot"); double[] result = new double[written]; if (written > 0) Marshal.Copy(memory, result, 0, written); return result; }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public IEnumerator<double> GetEnumerator() { ThrowIfDisposed(); for (int index = 0; index < Count; index++) yield return this[index]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
