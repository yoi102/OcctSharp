using System.Collections;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Owns an OCCT <c>NCollection_Array1&lt;double&gt;</c> with native 1-based bounds.</summary>
public sealed class OcctRealArray : IReadOnlyList<double>, IDisposable
{
    private readonly RealArrayHandle handle;
    private OcctRealArray(RealArrayHandle handle) => this.handle = handle;
    internal static OcctRealArray FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native real array handle was null.") : new(new RealArrayHandle(value));
    public static OcctRealArray Create(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        double[] copy = values.ToArray();
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * copy.Length));
        try
        {
            if (copy.Length > 0) Marshal.Copy(copy, 0, memory, copy.Length);
            NativeError.ThrowIfFailed(NativeMethods.CreateRealArray(memory, copy.Length, out nint value), "real_array_create");
            return FromNative(value);
        }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public int Count { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetRealArrayLength(handle, out int length), "real_array_length"); return length; } }
    public int LowerBound { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetRealArrayLower(handle, out int lower), "real_array_lower"); return lower; } }
    public double this[int index]
    {
        get { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.GetRealArrayValue(handle, index + LowerBound, out double value), "real_array_value"); return value; }
    }
    public OcctRealArray Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneRealArray(handle, out nint value), "real_array_clone"); return FromNative(value); }
    public void Set(int index, double value) { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.SetRealArrayValue(handle, index + LowerBound, value), "real_array_set_value"); }
    /// <summary>Copies the current values in one native call; the returned array is independent of this collection.</summary>
    public double[] Snapshot()
    {
        ThrowIfDisposed(); int count = Count;
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * count));
        try { NativeError.ThrowIfFailed(NativeMethods.SnapshotRealArray(handle, memory, count, out int written), "real_array_snapshot"); double[] result = new double[written]; if (written > 0) Marshal.Copy(memory, result, 0, written); return result; }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public IEnumerator<double> GetEnumerator() { ThrowIfDisposed(); for (int index = 0; index < Count; index++) yield return this[index]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
