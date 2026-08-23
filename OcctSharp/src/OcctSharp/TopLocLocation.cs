using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns an opaque OCCT <c>TopLoc_Location</c> value.</summary>
public sealed class TopLocLocation : IDisposable
{
    private readonly LocationHandle handle;

    internal TopLocLocation(LocationHandle handle)
    {
        this.handle = handle;
    }

    internal LocationHandle Handle => handle;

    /// <summary>Creates an identity location.</summary>
    public static TopLocLocation Identity
    {
        get
        {
            NativeError.ThrowIfFailed(
                NativeMethods.CreateLocationIdentity(out nint location),
                "location_create_identity");
            return new TopLocLocation(new LocationHandle(location));
        }
    }

    /// <summary>Creates a location from an independent transformation value.</summary>
    public static TopLocLocation FromTransform(GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        transform.ThrowIfDisposedForLocation();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateLocation(transform.Handle, out nint location),
            "location_create_from_trsf");
        return new TopLocLocation(new LocationHandle(location));
    }

    /// <summary>Returns an independent value copy.</summary>
    public TopLocLocation Clone()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CloneLocation(handle, out nint location), "location_clone");
        return new TopLocLocation(new LocationHandle(location));
    }

    /// <summary>Returns the inverse location.</summary>
    public TopLocLocation Inverted()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.InvertLocation(handle, out nint location), "location_inverted");
        return new TopLocLocation(new LocationHandle(location));
    }

    /// <summary>Returns this location composed with <paramref name="other"/>.</summary>
    public TopLocLocation Multiplied(TopLocLocation other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.MultiplyLocations(handle, other.handle, out nint location),
            "location_multiplied");
        return new TopLocLocation(new LocationHandle(location));
    }

    /// <summary>Gets whether this location is OCCT identity.</summary>
    public bool IsIdentity
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.IsLocationIdentity(handle, out int isIdentity), "location_is_identity");
            return isIdentity != 0;
        }
    }

    /// <summary>Returns the associated transformation as an independent value.</summary>
    public GpTrsf ToTransform()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.LocationToTransform(handle, out nint transform), "location_to_trsf");
        return GpTrsf.FromNativeHandle(transform);
    }

    /// <summary>Applies this location as a new absolute location.</summary>
    public Shape Locate(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        return LocateCore(shape, moved: false);
    }

    /// <summary>Moves this shape by composing the location with its current location.</summary>
    public Shape Move(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        return LocateCore(shape, moved: true);
    }

    /// <summary>Releases the native location value.</summary>
    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }

    private Shape LocateCore(Shape shape, bool moved)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.LocateShape(shape.Handle, handle, moved ? 1 : 0, out nint locatedShape),
            moved ? "shape_moved" : "shape_located");
        return ShapeFactory.FromNativeHandle(locatedShape, moved ? "shape_moved" : "shape_located");
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
