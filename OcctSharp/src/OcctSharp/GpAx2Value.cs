using OcctSharp.Interop;
namespace OcctSharp;
#pragma warning disable CS1591
/// <summary>Immutable right-handed coordinate system value backed by OCCT <c>gp_Ax2</c>.</summary>
public readonly record struct GpAx2Value(GpXyz Origin, GpXyz XDirection, GpXyz YDirection, GpXyz Direction)
{
    public static GpAx2Value Default => FromRaw(NativeMethods.CreateAx2Default());
    public static GpAx2Value Create(GpXyz origin, GpXyz normal, GpXyz xDirection) { NativeError.ThrowIfFailed(NativeMethods.CreateAx2(ToRaw(origin), ToRaw(normal), ToRaw(xDirection), out Ax2Raw result), "gp_ax2_create"); return FromRaw(result); }
    public double AngleTo(GpAx2Value other) => NativeMethods.GetAx2Angle(ToRaw(), other.ToRaw());
    private Ax2Raw ToRaw() => new(ToRaw(Origin), ToRaw(XDirection), ToRaw(YDirection), ToRaw(Direction));
    private static XyzRaw ToRaw(GpXyz v) => new(v.X, v.Y, v.Z);
    private static GpAx2Value FromRaw(Ax2Raw r) => new(new(r.Origin.X, r.Origin.Y, r.Origin.Z), new(r.XDirection.X, r.XDirection.Y, r.XDirection.Z), new(r.YDirection.X, r.YDirection.Y, r.YDirection.Z), new(r.Direction.X, r.Direction.Y, r.Direction.Z));
}
#pragma warning restore CS1591
