using System.Runtime.InteropServices;
using OcctSharp.Interop;
#pragma warning disable CS1591
namespace OcctSharp;

/// <summary>Owns an opaque OCCT 3x3 matrix value.</summary>
public sealed class GpMat : IDisposable
{
    private readonly MatrixHandle handle;
    private GpMat(MatrixHandle handle) => this.handle = handle;
    public static GpMat Identity { get { NativeError.ThrowIfFailed(NativeMethods.CreateIdentityMatrix(out nint m),"mat_identity"); return new GpMat(new MatrixHandle(m)); } }
    public static GpMat Create(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values); if (values.Count != 9) throw new ArgumentException("A gp_Mat requires exactly nine values.",nameof(values));
        double[] copy = values.ToArray(); nint memory = Marshal.AllocHGlobal(sizeof(double) * 9);
        try { Marshal.Copy(copy,0,memory,9); NativeError.ThrowIfFailed(NativeMethods.CreateMatrix(memory,out nint m),"mat_create"); return new GpMat(new MatrixHandle(m)); }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public GpMat Clone() { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.CloneMatrix(handle,out nint m),"mat_clone"); return new GpMat(new MatrixHandle(m)); }
    public double Value(int row,int column) { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetMatrixValue(handle,row,column,out double v),"mat_value"); return v; }
    public double Determinant { get { ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.GetMatrixDeterminant(handle,out double d),"mat_determinant"); return d; } }
    public void Dispose() { handle.Dispose(); GC.SuppressFinalize(this); }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed,this);
}
#pragma warning restore CS1591
