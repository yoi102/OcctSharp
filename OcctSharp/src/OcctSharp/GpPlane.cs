using OcctSharp.Interop;
namespace OcctSharp;
#pragma warning disable CS1591
/// <summary>Immutable plane value backed by OCCT <c>gp_Pln</c>.</summary>
public readonly record struct GpPlane(GpXyz Origin, GpXyz Normal)
{
    public static GpPlane Default => FromRaw(NativeMethods.CreatePlaneDefault());
    public static GpPlane Create(GpXyz origin, GpXyz normal) { NativeError.ThrowIfFailed(NativeMethods.CreatePlane(ToRaw(origin), ToRaw(normal), out PlaneRaw result), "gp_pln_create"); return FromRaw(result); }
    public double DistanceTo(GpXyz point) => NativeMethods.GetPlaneDistance(ToRaw(), ToRaw(point));
    public double SignedDistanceTo(GpXyz point) => NativeMethods.GetPlaneSignedDistance(ToRaw(), ToRaw(point));
    private PlaneRaw ToRaw() => new(ToRaw(Origin), ToRaw(Normal));
    private static XyzRaw ToRaw(GpXyz v) => new(v.X, v.Y, v.Z);
    private static GpPlane FromRaw(PlaneRaw r) => new(new(r.Origin.X, r.Origin.Y, r.Origin.Z), new(r.Normal.X, r.Normal.Y, r.Normal.Z));
}
#pragma warning restore CS1591
