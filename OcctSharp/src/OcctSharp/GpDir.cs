using OcctSharp.Interop;
#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an opaque OCCT unit direction.</summary>
public sealed class GpDir : IDisposable
{
    private readonly DirectionHandle handle;
    private GpDir(DirectionHandle handle) => this.handle = handle;
    internal DirectionHandle Handle => handle;
    internal static GpDir FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native direction handle was null.") : new(new DirectionHandle(value));
    public static GpDir Create(double x,double y,double z) { NativeError.ThrowIfFailed(NativeMethods.CreateDirection(x,y,z,out nint d),"dir_create"); return FromNative(d); }
    public GpDir Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneDirection(handle,out nint d),"dir_clone"); return FromNative(d); }
    public (double X,double Y,double Z) Components { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetDirectionComponents(handle,out double x,out double y,out double z),"dir_components"); return (x,y,z); } }
    public double Dot(GpDir other) { ArgumentNullException.ThrowIfNull(other); ThrowIfDisposed(); other.ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetDirectionDot(handle,other.handle,out double d),"dir_dot"); return d; }
    public GpDir Reversed() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.ReverseDirection(handle,out nint d),"dir_reversed"); return FromNative(d); }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed,this);
}
#pragma warning restore CS1591
