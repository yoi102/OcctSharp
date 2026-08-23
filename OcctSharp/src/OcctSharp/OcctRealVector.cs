using System.Collections;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Owns an OCCT 8 dynamic-array-backed <c>NCollection_Vector&lt;double&gt;</c>.</summary>
public sealed class OcctRealVector : IReadOnlyList<double>, IDisposable
{
    private readonly RealVectorHandle handle;
    private OcctRealVector(RealVectorHandle handle) => this.handle = handle;
    internal static OcctRealVector FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native real vector handle was null.") : new(new RealVectorHandle(value));
    public static OcctRealVector Create(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        double[] copy = values.ToArray();
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * copy.Length));
        try
        {
            if (copy.Length > 0) Marshal.Copy(copy, 0, memory, copy.Length);
            NativeError.ThrowIfFailed(NativeMethods.CreateRealVector(memory, copy.Length, out nint value), "real_vector_create");
            return FromNative(value);
        }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public int Count { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetRealVectorLength(handle, out int length), "real_vector_length"); return length; } }
    public double this[int index] { get { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.GetRealVectorValue(handle, index, out double value), "real_vector_value"); return value; } }
    public OcctRealVector Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneRealVector(handle, out nint value), "real_vector_clone"); return FromNative(value); }
    public void Add(double value) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.AppendRealVector(handle, value), "real_vector_append"); }
    public void Set(int index, double value) { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.SetRealVectorValue(handle, index, value), "real_vector_set_value"); }
    /// <summary>Copies the current values in one native call; the returned array is independent of this collection.</summary>
    public double[] Snapshot()
    {
        ThrowIfDisposed(); int count = Count;
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * count));
        try { NativeError.ThrowIfFailed(NativeMethods.SnapshotRealVector(handle, memory, count, out int written), "real_vector_snapshot"); double[] result = new double[written]; if (written > 0) Marshal.Copy(memory, result, 0, written); return result; }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public IEnumerator<double> GetEnumerator() { ThrowIfDisposed(); for (int index = 0; index < Count; index++) yield return this[index]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
