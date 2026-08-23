using OcctSharp.Interop;
#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an opaque OCCT <c>gp_Vec</c> value.</summary>
public sealed class GpVec : IDisposable
{
    private readonly VectorHandle handle;
    private GpVec(VectorHandle handle) => this.handle = handle;
    internal VectorHandle Handle => handle;
    internal static GpVec FromNative(nint value) => value == 0 ? throw new OcctException("UnknownException", "Native vector handle was null.") : new(new VectorHandle(value));
    public static GpVec Create(double x, double y, double z) { NativeError.ThrowIfFailed(NativeMethods.CreateVector(x,y,z,out nint v), "vec_create"); return FromNative(v); }
    public GpVec Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneVector(handle,out nint v),"vec_clone"); return FromNative(v); }
    public (double X, double Y, double Z) Components { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetVectorComponents(handle,out double x,out double y,out double z),"vec_components"); return (x,y,z); } }
    public double Magnitude { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetVectorMagnitude(handle,out double m),"vec_magnitude"); return m; } }
    public double Dot(GpVec other) { ArgumentNullException.ThrowIfNull(other); ThrowIfDisposed(); other.ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetVectorDot(handle,other.handle,out double d),"vec_dot"); return d; }
    public GpVec Crossed(GpVec other) { ArgumentNullException.ThrowIfNull(other); ThrowIfDisposed(); other.ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CrossVectors(handle,other.handle,out nint v),"vec_crossed"); return FromNative(v); }
    public GpTrsf ToTranslation() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CreateTranslationTransform(handle,out nint t),"trsf_create_translation_vec"); return GpTrsf.FromNativeHandle(t); }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed,this);
}
#pragma warning restore CS1591
