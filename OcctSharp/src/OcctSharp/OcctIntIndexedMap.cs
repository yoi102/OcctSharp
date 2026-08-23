using System.Collections;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Owns an OCCT <c>NCollection_IndexedMap&lt;int&gt;</c> with a 0-based managed view.</summary>
public sealed class OcctIntIndexedMap : IReadOnlyList<int>, IDisposable
{
    private readonly IntIndexedMapHandle handle;
    private OcctIntIndexedMap(IntIndexedMapHandle handle) => this.handle = handle;
    internal static OcctIntIndexedMap FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native indexed map handle was null.") : new(new IntIndexedMapHandle(value));
    public static OcctIntIndexedMap Create(IEnumerable<int> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        int[] copy = keys.ToArray();
        nint memory = Marshal.AllocHGlobal(Math.Max(1, sizeof(int) * copy.Length));
        try
        {
            if (copy.Length > 0) Marshal.Copy(copy, 0, memory, copy.Length);
            NativeError.ThrowIfFailed(NativeMethods.CreateIntIndexedMap(memory, copy.Length, out nint result), "int_indexed_map_create");
            return FromNative(result);
        }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public int Count { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetIntIndexedMapExtent(handle, out int extent), "int_indexed_map_extent"); return extent; } }
    public int this[int index] { get { ThrowIfDisposed(); ArgumentOutOfRangeException.ThrowIfNegative(index); NativeError.ThrowIfFailed(NativeMethods.GetIntIndexedMapKey(handle, index + 1, out int key), "int_indexed_map_key"); return key; } }
    public int FindIndex(int key) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.FindIntIndexedMapIndex(handle, key, out int index), "int_indexed_map_find_index"); return index == 0 ? -1 : index - 1; }
    public bool Add(int key) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.AddIntIndexedMap(handle, key, out _, out int added), "int_indexed_map_add"); return added != 0; }
    public int RemoveLast() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.RemoveLastIntIndexedMap(handle, out int key), "int_indexed_map_remove_last"); return key; }
    public OcctIntIndexedMap Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneIntIndexedMap(handle, out nint result), "int_indexed_map_clone"); return FromNative(result); }
    public IEnumerator<int> GetEnumerator() { ThrowIfDisposed(); for (int index = 0; index < Count; index++) yield return this[index]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
#pragma warning restore CS1591
