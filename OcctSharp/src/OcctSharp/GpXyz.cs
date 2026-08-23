using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Immutable value view of OCCT <c>gp_XYZ</c> algebraic coordinates.</summary>
public readonly record struct GpXyz(double X, double Y, double Z)
{
    public static GpXyz Origin => FromRaw(NativeMethods.CreateXyzDefault());
    public static GpXyz Create(double x, double y, double z) { Validate(x, nameof(x)); Validate(y, nameof(y)); Validate(z, nameof(z)); return FromRaw(NativeMethods.CreateXyz(x, y, z)); }
    public GpXyz Copy() => FromRaw(NativeMethods.CopyXyz(ToRaw()));
    public GpXyz Added(GpXyz other) => FromRaw(NativeMethods.AddXyz(ToRaw(), other.ToRaw()));
    public GpXyz Crossed(GpXyz other) => FromRaw(NativeMethods.CrossXyz(ToRaw(), other.ToRaw()));
    public double Dot(GpXyz other) => NativeMethods.DotXyz(ToRaw(), other.ToRaw());
    public double Modulus => NativeMethods.GetXyzModulus(ToRaw());
    public GpXyz Normalized() { NativeError.ThrowIfFailed(NativeMethods.NormalizeXyz(ToRaw(), out XyzRaw result), "gp_xyz_normalized"); return FromRaw(result); }
    private XyzRaw ToRaw() => new(X, Y, Z);
    private static GpXyz FromRaw(XyzRaw raw) => new(raw.X, raw.Y, raw.Z);
    private static void Validate(double value, string name) { if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name, "The coordinate must be finite."); }
}
#pragma warning restore CS1591
