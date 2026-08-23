using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Owns an opaque OCCT <c>GProp_GProps</c> global-properties accumulator.</summary>
public sealed class GPropProperties : IDisposable
{
    private readonly GPropsHandle handle;

    private GPropProperties(GPropsHandle handle) => this.handle = handle;

    /// <summary>Creates an empty accumulator using the absolute origin reference.</summary>
    public static GPropProperties Create()
    {
        NativeError.ThrowIfFailed(NativeMethods.CreateGProps(out nint properties), "gprops_create");
        return FromNativeHandle(properties);
    }

    /// <summary>Computes linear, surface, or volume properties for an owned shape.</summary>
    public static GPropProperties FromShape(Shape shape, GPropMode mode = GPropMode.Volume, bool onlyClosed = false)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
        NativeError.ThrowIfFailed(
            NativeMethods.CreateGPropsFromShape(shape.Handle, (int)mode, onlyClosed ? 1 : 0, out nint properties),
            "gprops_from_shape");
        return FromNativeHandle(properties);
    }

    public double Mass
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.GetGPropsMass(handle, out double mass), "gprops_mass");
            return mass;
        }
    }

    public GpPoint CenterOfMass
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.GetGPropsCenter(handle, out XyzRaw center), "gprops_center");
            return new GpPoint(center.X, center.Y, center.Z);
        }
    }

    public double InertiaValue(int row, int column)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetGPropsInertiaValue(handle, row, column, out double value),
            "gprops_inertia_value");
        return value;
    }

    public (double First, double Second, double Third) PrincipalMoments
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(
                NativeMethods.GetGPropsPrincipalMoments(handle, out double first, out double second, out double third),
                "gprops_principal_moments");
            return (first, second, third);
        }
    }

    public (bool HasAxis, bool HasPoint) Symmetry
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.GetGPropsSymmetry(handle, out int axis, out int point), "gprops_symmetry");
            return (axis != 0, point != 0);
        }
    }

    public GPropProperties Clone()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CloneGProps(handle, out nint properties), "gprops_clone");
        return FromNativeHandle(properties);
    }

    public void Add(GPropProperties item, double density = 1.0)
    {
        ArgumentNullException.ThrowIfNull(item);
        ThrowIfDisposed();
        item.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.AddGProps(handle, item.handle, density), "gprops_add");
    }

    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }

    private static GPropProperties FromNativeHandle(nint nativeHandle)
    {
        if (nativeHandle == 0)
            throw new OcctException(NativeStatus.UnknownException.ToString(), "The native bridge returned a null GProp_GProps handle.");
        return new GPropProperties(new GPropsHandle(nativeHandle));
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
}

public enum GPropMode
{
    Linear = 0,
    Surface = 1,
    Volume = 2,
}
#pragma warning restore CS1591
