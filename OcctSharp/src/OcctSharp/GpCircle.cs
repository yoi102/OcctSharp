using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable value view of an OCCT <c>gp_Circ</c> circle.</summary>
public readonly record struct GpCircle(GpXyz Center, GpXyz Normal, double Radius)
{
    public static GpCircle Default => FromRaw(NativeMethods.CreateCircleDefault());
    public static GpCircle Create(GpXyz center, GpXyz normal, double radius) { if (!double.IsFinite(radius)) throw new ArgumentOutOfRangeException(nameof(radius)); NativeError.ThrowIfFailed(NativeMethods.CreateCircle(ToRaw(center), ToRaw(normal), radius, out CircleRaw result), "gp_circ_create"); return FromRaw(result); }
    public double Area => NativeMethods.GetCircleArea(ToRaw());
    public double Length => NativeMethods.GetCircleLength(ToRaw());
    public double DistanceTo(GpXyz point) => NativeMethods.GetCircleDistance(ToRaw(), ToRaw(point));
    private CircleRaw ToRaw() => new(ToRaw(Center), ToRaw(Normal), Radius);
    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static GpCircle FromRaw(CircleRaw value) => new(new GpXyz(value.Center.X, value.Center.Y, value.Center.Z), new GpXyz(value.Normal.X, value.Normal.Y, value.Normal.Z), value.Radius);
}
#pragma warning restore CS1591
