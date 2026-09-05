using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591
public enum ParametricValueKind { Missing, Integral, Real, Text, IntegralArray, RealArray }
public enum ParametricUnit { None, Millimeter, Centimeter, Meter, Radian, Degree }

/// <summary>Physical dimensions in model millimetres and radians; exponents are bounded.</summary>
public readonly record struct ParametricDimension
{
    [JsonConstructor]
    public ParametricDimension(int length, int angle)
    {
        if (Math.Abs((long)length) > 8 || Math.Abs((long)angle) > 8)
            throw new ArgumentOutOfRangeException(nameof(length), "Dimension exponents must be between -8 and 8.");
        Length = length;
        Angle = angle;
    }

    public int Length { get; }
    public int Angle { get; }
    public static ParametricDimension Scalar => new(0, 0);
    public static ParametricDimension Distance => new(1, 0);
    public static ParametricDimension Rotation => new(0, 1);
    internal static ParametricDimension Of(ParametricUnit unit) => unit switch
    {
        ParametricUnit.None => Scalar,
        ParametricUnit.Millimeter or ParametricUnit.Centimeter or ParametricUnit.Meter => Distance,
        ParametricUnit.Radian or ParametricUnit.Degree => Rotation,
        _ => throw new ArgumentOutOfRangeException(nameof(unit))
    };
}

/// <summary>Immutable typed parameter. Missing and empty are separate values; inputs are copied.</summary>
public sealed class ParametricValue
{
    [JsonConstructor]
    public ParametricValue(ParametricValueKind kind, int integral, double real, string? text,
        IReadOnlyList<int>? integers, IReadOnlyList<double>? reals, ParametricUnit unit)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(unit)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (!double.IsFinite(real)) throw new ArgumentOutOfRangeException(nameof(real));
        if ((integers?.Count ?? 0) > 1_000_000 || (reals?.Count ?? 0) > 1_000_000)
            throw new ArgumentException("Parameter arrays are limited to one million values.");
        if ((text?.Length ?? 0) > 1_000_000) throw new ArgumentException("Parameter text is too large.");
        if (kind == ParametricValueKind.Text && text is null) throw new ArgumentNullException(nameof(text));
        if (kind == ParametricValueKind.IntegralArray && integers is null) throw new ArgumentNullException(nameof(integers));
        if (kind == ParametricValueKind.RealArray && reals is null) throw new ArgumentNullException(nameof(reals));
        if (kind is not (ParametricValueKind.Real or ParametricValueKind.RealArray) && unit != ParametricUnit.None)
            throw new ArgumentException("Only real values have declared units.");
        int[] copiedIntegers = integers?.ToArray() ?? [];
        double[] copiedReals = reals?.ToArray() ?? [];
        if (copiedReals.Any(x => !double.IsFinite(x))) throw new ArgumentException("Array values must be finite.");
        Kind = kind;
        Integral = integral;
        Real = real;
        Text = text;
        Integers = Array.AsReadOnly(copiedIntegers);
        Reals = Array.AsReadOnly(copiedReals);
        Unit = unit;
    }

    public ParametricValueKind Kind { get; }
    public int Integral { get; }
    public double Real { get; }
    public string? Text { get; }
    public IReadOnlyList<int> Integers { get; }
    public IReadOnlyList<double> Reals { get; }
    public ParametricUnit Unit { get; }
    [JsonIgnore] public bool HasValue => Kind != ParametricValueKind.Missing;
    public static ParametricValue Missing() => new(ParametricValueKind.Missing, 0, 0, null, null, null, ParametricUnit.None);
    public static ParametricValue FromInteger(int value) => new(ParametricValueKind.Integral, value, 0, null, null, null, ParametricUnit.None);
    public static ParametricValue FromReal(double value, ParametricUnit unit = ParametricUnit.None) => new(ParametricValueKind.Real, 0, value, null, null, null, unit);
    public static ParametricValue FromText(string value) => new(ParametricValueKind.Text, 0, 0, value, null, null, ParametricUnit.None);
    public static ParametricValue FromIntegers(IReadOnlyList<int> value) => new(ParametricValueKind.IntegralArray, 0, 0, null, value, null, ParametricUnit.None);
    public static ParametricValue FromReals(IReadOnlyList<double> value, ParametricUnit unit = ParametricUnit.None) => new(ParametricValueKind.RealArray, 0, 0, null, null, value, unit);

    internal ParametricQuantity Quantity()
    {
        if (Kind == ParametricValueKind.Integral) return new(Integral, ParametricDimension.Scalar);
        if (Kind != ParametricValueKind.Real) throw new InvalidOperationException("A present scalar parameter is required.");
        double factor = Unit switch { ParametricUnit.Centimeter => 10, ParametricUnit.Meter => 1000,
            ParametricUnit.Degree => Math.PI / 180, _ => 1 };
        return new(Real * factor, ParametricDimension.Of(Unit));
    }
}

public readonly record struct ParametricQuantity
{
    [JsonConstructor]
    public ParametricQuantity(double value, ParametricDimension dimension)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
        Value = value;
        Dimension = dimension;
    }
    public double Value { get; }
    public ParametricDimension Dimension { get; }
}

public readonly record struct ParametricParameterReference(Guid FeatureId, string Name);
#pragma warning restore CS1591
