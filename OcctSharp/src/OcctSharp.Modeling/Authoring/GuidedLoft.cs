using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum AuthoringContinuity { C0, C1, C2 }
public enum LoftParameterization { ChordLength, Centripetal, Uniform }
public sealed record GuidedLoftOptions
{
    public bool Solid { get; init; }
    public bool Ruled { get; init; }
    public bool CorrectCompatibility { get; init; } = true;
    public bool Smoothing { get; init; }
    public int MaximumDegree { get; init; } = 8;
    public AuthoringContinuity Continuity { get; init; } = AuthoringContinuity.C2;
    public LoftParameterization Parameterization { get; init; }
    public double Tolerance { get; init; } = 1e-6;
    public double LengthWeight { get; init; } = 1;
    public double CurvatureWeight { get; init; } = 1;
    public double TorsionWeight { get; init; } = 1;
}
public static class GuidedLoft
{
    public static unsafe AuthoringResult Build(IEnumerable<Shape> sections, GuidedLoftOptions? options = null)
    {
        Shape[] source = ScalarLawDefinition.Copy(sections, 128); options ??= new();
        if (source.Length < 2 || options.MaximumDegree is < 2 or > 25 || !Enum.IsDefined(options.Continuity) || !Enum.IsDefined(options.Parameterization))
            throw new ArgumentOutOfRangeException(nameof(sections));
        AuthoringBridge.Positive(options.Tolerance, nameof(options.Tolerance));
        AuthoringBridge.Positive(options.LengthWeight, nameof(options.LengthWeight));
        AuthoringBridge.Positive(options.CurvatureWeight, nameof(options.CurvatureWeight));
        AuthoringBridge.Positive(options.TorsionWeight, nameof(options.TorsionWeight));
        LoftOptionsRaw raw = new() { Solid = options.Solid ? 1 : 0, Ruled = options.Ruled ? 1 : 0,
            Compatibility = options.CorrectCompatibility ? 1 : 0, Smoothing = options.Smoothing ? 1 : 0,
            MaximumDegree = options.MaximumDegree, Continuity = (int)options.Continuity, Parameterization = (int)options.Parameterization,
            Tolerance = options.Tolerance, Weight1 = options.LengthWeight, Weight2 = options.CurvatureWeight, Weight3 = options.TorsionWeight };
        return AuthoringBridge.WithInputs(source, (p, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.GuidedLoft(p, count, in raw, out AuthoringInfoRaw info, out nint result), "guided_loft");
            return AuthoringBridge.Read(Guid.NewGuid(), result, info);
        });
    }
}
#pragma warning restore CS1591
