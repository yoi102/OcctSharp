using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable right-handed coordinate-system value backed by OCCT <c>gp_Ax3</c>.</summary>
public readonly record struct GpAx3Value(GpXyz Origin, GpXyz XDirection, GpXyz YDirection, GpXyz Direction)
{
    public static GpAx3Value Default => FromRaw(NativeMethods.CreateAx3Default());

    public static GpAx3Value Create(GpXyz origin, GpXyz normal, GpXyz xDirection)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.CreateAx3(ToRaw(origin), ToRaw(normal), ToRaw(xDirection), out Ax3Raw result),
            "gp_ax3_create");
        return FromRaw(result);
    }

    public bool IsDirect => NativeMethods.IsAx3Direct(ToRaw()) != 0;

    private Ax3Raw ToRaw() => new(ToRaw(Origin), ToRaw(XDirection), ToRaw(YDirection), ToRaw(Direction));
    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static GpAx3Value FromRaw(Ax3Raw value) => new(
        new(value.Origin.X, value.Origin.Y, value.Origin.Z),
        new(value.XDirection.X, value.XDirection.Y, value.XDirection.Z),
        new(value.YDirection.X, value.YDirection.Y, value.YDirection.Z),
        new(value.Direction.X, value.Direction.Y, value.Direction.Z));
}
#pragma warning restore CS1591
