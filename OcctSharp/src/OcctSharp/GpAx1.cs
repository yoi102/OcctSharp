using OcctSharp.Interop;
#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an opaque OCCT axis defined by an origin and unit direction.</summary>
public sealed class GpAx1 : IDisposable
{
    private readonly AxisHandle handle;
    private GpAx1(AxisHandle handle) => this.handle = handle;
    internal AxisHandle Handle => handle;
    internal static GpAx1 FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native axis handle was null.") : new(new AxisHandle(value));
    public static GpAx1 Create(double originX,double originY,double originZ,double directionX,double directionY,double directionZ) { NativeError.ThrowIfFailed(NativeMethods.CreateAxis(originX,originY,originZ,directionX,directionY,directionZ,out nint a),"ax1_create"); return FromNative(a); }
    public GpAx1 Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneAxis(handle,out nint a),"ax1_clone"); return FromNative(a); }
    public (double OriginX,double OriginY,double OriginZ,double DirectionX,double DirectionY,double DirectionZ) Components { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetAxisComponents(handle,out double ox,out double oy,out double oz,out double dx,out double dy,out double dz),"ax1_components"); return (ox,oy,oz,dx,dy,dz); } }
    public GpAx1 Reversed() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.ReverseAxis(handle,out nint a),"ax1_reversed"); return FromNative(a); }
    public GpTrsf ToRotation(double angleRadians) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CreateRotationTransform(handle,angleRadians,out nint t),"trsf_create_rotation_axis"); return GpTrsf.FromNativeHandle(t); }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed,this);
}
#pragma warning restore CS1591
