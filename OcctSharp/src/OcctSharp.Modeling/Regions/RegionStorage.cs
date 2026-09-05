using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

internal enum RegionItemKind { Cell, Membership, Boundary, BoundaryUse, Assignment, Output, RuleEffect,
    InputMeasure, History, Fault, SourceFace, UnusedFace, FreeBoundary, InternalTopology, HelperCheck, ShellCandidate, VolumeShell }

/// <summary>Private owning topology values preserve shared identities; public access always deep-copies.</summary>
internal sealed class RegionStorage : IDisposable
{
    private bool disposed;
    private readonly Shape?[] shapes;
    private RegionStorage(RegionInfoRaw info, RegionItemRaw[] items, Shape?[] shapes, string message)
    { Info = info; Items = items; this.shapes = shapes; Message = message; }
    internal RegionInfoRaw Info { get; }
    internal RegionItemRaw[] Items { get; }
    internal string Message { get; }
    internal IEnumerable<(RegionItemRaw Item, int Index)> Find(RegionItemKind kind) =>
        Items.Select((item, index) => (item, index)).Where(x => x.item.Kind == (int)kind);
    internal Shape Get(int index)
    {
        ThrowIfDisposed();
        if ((uint)index >= shapes.Length || shapes[index] is not { } shape) throw new ArgumentException("Region item has no topology.");
        return shape;
    }
    internal Shape Copy(int index) => CopyShape(Get(index));
    internal static Shape CopyShape(Shape shape)
    {
        shape.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.RepairCopy(shape.Handle, out nint value), "region_copy");
        return ShapeFactory.FromNativeHandle(value, "region_copy");
    }
    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
    public void Dispose() { if (disposed) return; disposed = true; foreach (var shape in shapes) shape?.Dispose(); }
    internal static unsafe RegionStorage Read(nint raw)
    {
        using FeatureResultHandle result = new(raw);
        NativeError.ThrowIfFailed(NativeMethods.RegionSnapshot(result, out var info, null, 0), "region_counts");
        if (info.ItemCount is < 0 or > 1000000 || info.CellCount is < 0 or > 100000 || info.OutputCount is < 0 or > 128)
            throw new InvalidOperationException("Native region snapshot exceeds its bounds.");
        RegionItemRaw[] items = new RegionItemRaw[info.ItemCount]; Shape?[] shapes = new Shape?[items.Length];
        try
        {
            fixed (RegionItemRaw* p = items) NativeError.ThrowIfFailed(NativeMethods.RegionSnapshot(result, out info, p, items.Length), "region_snapshot");
            for (int i = 0; i < items.Length; i++)
            {
                NativeError.ThrowIfFailed(NativeMethods.RegionItemShape(result, i, out nint value), "region_item_shape");
                if (value != 0) shapes[i] = ShapeFactory.FromNativeHandle(value, "region_item_shape");
            }
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, null, 0, out int count), "region_message_count");
            if (count is < 1 or > 1048576) throw new InvalidOperationException("Region diagnostic exceeds its bounds.");
            byte[] bytes = new byte[count];
            fixed (byte* p = bytes) NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, p, count, out _), "region_message");
            return new(info, items, shapes, Encoding.UTF8.GetString(bytes.AsSpan(0, count - 1)));
        }
        catch { foreach (var shape in shapes) shape?.Dispose(); throw; }
    }
}
