using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns an opaque OCCT <c>gp_Trsf</c> value and exposes copy-safe composition.</summary>
public sealed class GpTrsf : IDisposable
{
    private readonly TransformHandle handle;

    private GpTrsf(TransformHandle handle)
    {
        this.handle = handle;
    }

    internal TransformHandle Handle => handle;

    internal static GpTrsf FromNativeHandle(nint nativeHandle)
    {
        if (nativeHandle == 0)
        {
            throw new OcctException(NativeStatus.UnknownException.ToString(), "The native bridge returned a null transform handle.");
        }

        return new GpTrsf(new TransformHandle(nativeHandle));
    }

    internal void ThrowIfDisposedForLocation() => ThrowIfDisposed();

    /// <summary>Creates an identity transformation.</summary>
    public static GpTrsf Identity
    {
        get
        {
            NativeError.ThrowIfFailed(NativeMethods.CreateTransformIdentity(out nint transform), "trsf_create_identity");
            return FromNativeHandle(transform);
        }
    }

    /// <summary>Creates a translation and optional rotation around the origin.</summary>
    public static GpTrsf Create(
        double translationX,
        double translationY,
        double translationZ,
        double rotationAxisX = 0,
        double rotationAxisY = 0,
        double rotationAxisZ = 1,
        double rotationAngleRadians = 0)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.CreateTransform(
                translationX,
                translationY,
                translationZ,
                rotationAxisX,
                rotationAxisY,
                rotationAxisZ,
                rotationAngleRadians,
                out nint transform),
            "trsf_create");
        return FromNativeHandle(transform);
    }

    /// <summary>Returns an independent native value copy.</summary>
    public GpTrsf Clone()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CloneTransform(handle, out nint transform), "trsf_clone");
        return FromNativeHandle(transform);
    }

    /// <summary>Returns the inverse transformation.</summary>
    public GpTrsf Inverted()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.InvertTransform(handle, out nint transform), "trsf_inverted");
        return FromNativeHandle(transform);
    }

    /// <summary>Returns this transformation composed with <paramref name="other"/>.</summary>
    public GpTrsf Multiplied(GpTrsf other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.MultiplyTransforms(handle, other.handle, out nint transform),
            "trsf_multiplied");
        return FromNativeHandle(transform);
    }

    /// <summary>Reads one OCCT 1-based 3x4 matrix value.</summary>
    public double Value(int row, int column)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetTransformValue(handle, row, column, out double value),
            "trsf_value");
        return value;
    }

    /// <summary>Applies this transformation and returns an independently owned shape.</summary>
    public Shape Apply(Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.TransformShapeWithTransform(shape.Handle, handle, out nint transformedShape),
            "shape_transform_trsf");
        return ShapeFactory.FromNativeHandle(transformedShape, "shape_transform_trsf");
    }

    /// <summary>Releases the native transformation value.</summary>
    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}
