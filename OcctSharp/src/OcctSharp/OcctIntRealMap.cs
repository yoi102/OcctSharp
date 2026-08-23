using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Owns an OCCT <c>NCollection_DataMap&lt;int,double&gt;</c>.</summary>
public sealed class OcctIntRealMap : IDisposable
{
    private readonly IntRealMapHandle handle;
    private OcctIntRealMap(IntRealMapHandle handle) => this.handle = handle;
    internal static OcctIntRealMap FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native integer-real map handle was null.") : new(new IntRealMapHandle(value));
    public static OcctIntRealMap Create(IEnumerable<KeyValuePair<int, double>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        KeyValuePair<int, double>[] copy = entries.ToArray();
        nint keys = Marshal.AllocHGlobal(Math.Max(1, sizeof(int) * copy.Length));
        nint values = Marshal.AllocHGlobal(Math.Max(1, sizeof(double) * copy.Length));
        try
        {
            int[] keyCopy = copy.Select(static pair => pair.Key).ToArray();
            double[] valueCopy = copy.Select(static pair => pair.Value).ToArray();
            if (copy.Length > 0) { Marshal.Copy(keyCopy, 0, keys, copy.Length); Marshal.Copy(valueCopy, 0, values, copy.Length); }
            NativeError.ThrowIfFailed(NativeMethods.CreateIntRealMap(keys, values, copy.Length, out nint result), "int_real_map_create");
            return FromNative(result);
        }
        finally { Marshal.FreeHGlobal(keys); Marshal.FreeHGlobal(values); }
    }
    public int Count { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetIntRealMapExtent(handle, out int extent), "int_real_map_extent"); return extent; } }
    public bool ContainsKey(int key) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.IsIntRealMapBound(handle, key, out int bound), "int_real_map_is_bound"); return bound != 0; }
    public double this[int key] { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.FindIntRealMap(handle, key, out double value), "int_real_map_find"); return value; } set { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.BindIntRealMap(handle, key, value), "int_real_map_bind"); } }
    public bool Remove(int key) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.UnbindIntRealMap(handle, key, out int removed), "int_real_map_unbind"); return removed != 0; }
    public OcctIntRealMap Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneIntRealMap(handle, out nint result), "int_real_map_clone"); return FromNative(result); }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
