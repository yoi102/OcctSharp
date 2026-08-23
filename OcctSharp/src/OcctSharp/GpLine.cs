using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable value view of an OCCT <c>gp_Lin</c> line.</summary>
public readonly record struct GpLine(GpXyz Origin, GpXyz Direction)
{
    public static GpLine Default => FromRaw(NativeMethods.CreateLineDefault());
    public static GpLine Create(GpXyz origin, GpXyz direction) { NativeError.ThrowIfFailed(NativeMethods.CreateLine(ToRaw(origin), ToRaw(direction), out LineRaw result), "gp_lin_create"); return FromRaw(result); }
    public GpLine Reversed() => FromRaw(NativeMethods.ReverseLine(ToRaw()));
    public double DistanceTo(GpXyz point) => NativeMethods.GetLineDistance(ToRaw(), ToRaw(point));
    public double AngleTo(GpLine other) => NativeMethods.GetLineAngle(ToRaw(), other.ToRaw());
    private LineRaw ToRaw() => new(ToRaw(Origin), ToRaw(Direction));
    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static GpLine FromRaw(LineRaw value) => new(new GpXyz(value.Origin.X, value.Origin.Y, value.Origin.Z), new GpXyz(value.Direction.X, value.Direction.Y, value.Direction.Z));
}
#pragma warning restore CS1591
