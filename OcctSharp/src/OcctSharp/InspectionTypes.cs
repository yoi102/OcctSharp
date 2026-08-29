namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Topology role reported by an exact minimum-distance solution.</summary>
public enum InspectionSupportKind
{
    Vertex = 0,
    Edge = 1,
    Face = 2
}

/// <summary>Explicit model/display units carried by inspection results.</summary>
public sealed record InspectionUnits
{
    public InspectionUnits(
        string modelLengthUnit = "mm",
        string displayLengthUnit = "mm",
        string modelAngleUnit = "rad",
        string displayAngleUnit = "deg",
        int decimalPlaces = 3)
    {
        ModelLengthUnit = NormalizeLength(modelLengthUnit);
        DisplayLengthUnit = NormalizeLength(displayLengthUnit);
        ModelAngleUnit = NormalizeAngle(modelAngleUnit);
        DisplayAngleUnit = NormalizeAngle(displayAngleUnit);
        if (decimalPlaces is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
        DecimalPlaces = decimalPlaces;
    }

    public string ModelLengthUnit { get; }
    public string DisplayLengthUnit { get; }
    public string ModelAngleUnit { get; }
    public string DisplayAngleUnit { get; }
    public int DecimalPlaces { get; }

    public double ConvertLength(double value) =>
        value * LengthMetres(ModelLengthUnit) / LengthMetres(DisplayLengthUnit);

    public double ConvertArea(double value)
    {
        double scale = LengthMetres(ModelLengthUnit) / LengthMetres(DisplayLengthUnit);
        return value * scale * scale;
    }

    public double ConvertVolume(double value)
    {
        double scale = LengthMetres(ModelLengthUnit) / LengthMetres(DisplayLengthUnit);
        return value * scale * scale * scale;
    }

    public double ConvertAngle(double value)
    {
        double radians = ModelAngleUnit == "rad" ? value : value * Math.PI / 180.0;
        return DisplayAngleUnit == "rad" ? radians : radians * 180.0 / Math.PI;
    }

    public string FormatLength(double value) => $"{ConvertLength(value).ToString($"F{DecimalPlaces}", System.Globalization.CultureInfo.InvariantCulture)} {DisplayLengthUnit}";
    public string FormatAngle(double value) => $"{ConvertAngle(value).ToString($"F{DecimalPlaces}", System.Globalization.CultureInfo.InvariantCulture)} {DisplayAngleUnit}";

    private static string NormalizeLength(string unit) => unit.Trim().ToLowerInvariant() switch
    {
        "mm" or "millimeter" or "millimetre" => "mm",
        "cm" or "centimeter" or "centimetre" => "cm",
        "m" or "meter" or "metre" => "m",
        "in" or "inch" => "in",
        "ft" or "foot" => "ft",
        _ => throw new ArgumentException($"Unsupported length unit '{unit}'.", nameof(unit))
    };

    private static string NormalizeAngle(string unit) => unit.Trim().ToLowerInvariant() switch
    {
        "rad" or "radian" => "rad",
        "deg" or "degree" => "deg",
        _ => throw new ArgumentException($"Unsupported angle unit '{unit}'.", nameof(unit))
    };

    private static double LengthMetres(string unit) => unit switch
    {
        "mm" => 0.001,
        "cm" => 0.01,
        "m" => 1.0,
        "in" => 0.0254,
        "ft" => 0.3048,
        _ => throw new InvalidOperationException($"Unsupported normalized length unit '{unit}'.")
    };
}

public readonly record struct InspectionScalar(double ModelValue, double DisplayValue, string DisplayText, InspectionUnits Units);

/// <summary>One owning, copied exact-extrema solution.</summary>
public sealed class ShapeDistanceSolution : IDisposable
{
    internal ShapeDistanceSolution(
        double distance,
        GpPoint pointOnFirst,
        GpPoint pointOnSecond,
        InspectionSupportKind firstSupportKind,
        InspectionSupportKind secondSupportKind,
        Shape firstSupport,
        Shape secondSupport,
        double? firstEdgeParameter,
        double? secondEdgeParameter,
        (double U, double V)? firstFaceParameters,
        (double U, double V)? secondFaceParameters,
        bool isInnerSolution)
    {
        Distance = distance;
        PointOnFirst = pointOnFirst;
        PointOnSecond = pointOnSecond;
        FirstSupportKind = firstSupportKind;
        SecondSupportKind = secondSupportKind;
        FirstSupport = firstSupport;
        SecondSupport = secondSupport;
        FirstEdgeParameter = firstEdgeParameter;
        SecondEdgeParameter = secondEdgeParameter;
        FirstFaceParameters = firstFaceParameters;
        SecondFaceParameters = secondFaceParameters;
        IsInnerSolution = isInnerSolution;
    }

    public double Distance { get; }
    public GpPoint PointOnFirst { get; }
    public GpPoint PointOnSecond { get; }
    public InspectionSupportKind FirstSupportKind { get; }
    public InspectionSupportKind SecondSupportKind { get; }
    public Shape FirstSupport { get; }
    public Shape SecondSupport { get; }
    public double? FirstEdgeParameter { get; }
    public double? SecondEdgeParameter { get; }
    public (double U, double V)? FirstFaceParameters { get; }
    public (double U, double V)? SecondFaceParameters { get; }
    public bool IsInnerSolution { get; }

    public void Dispose()
    {
        FirstSupport.Dispose();
        SecondSupport.Dispose();
    }
}

public sealed class ExactDistanceResult : IDisposable
{
    internal ExactDistanceResult(IReadOnlyList<ShapeDistanceSolution> solutions, InspectionUnits units)
    {
        Solutions = solutions;
        Units = units;
    }

    public IReadOnlyList<ShapeDistanceSolution> Solutions { get; }
    public InspectionUnits Units { get; }
    public double Distance => Solutions.Count == 0 ? double.NaN : Solutions[0].Distance;
    public InspectionScalar DisplayDistance => new(Distance, Units.ConvertLength(Distance), Units.FormatLength(Distance), Units);
    public void Dispose() { foreach (ShapeDistanceSolution solution in Solutions) solution.Dispose(); }
}

public enum ShapePairClassification
{
    Separated = 0,
    Touching = 1,
    Contained = 2,
    Interfering = 3
}

public sealed class ShapePairInspection : IDisposable
{
    internal ShapePairInspection(ShapePairClassification classification, double distance, double overlapVolume, Shape? overlap, InspectionUnits units)
    {
        Classification = classification;
        Distance = distance;
        OverlapVolume = overlapVolume;
        Overlap = overlap;
        Units = units;
    }

    public ShapePairClassification Classification { get; }
    public double Distance { get; }
    public double OverlapVolume { get; }
    public Shape? Overlap { get; }
    public InspectionUnits Units { get; }
    public void Dispose() => Overlap?.Dispose();
}

public enum InspectionPropertyKind { Length = 0, Area = 1, Volume = 2 }

public readonly record struct InertiaTensor(
    double I11, double I12, double I13,
    double I21, double I22, double I23,
    double I31, double I32, double I33);

public readonly record struct ShapeInspectionProperties(
    InspectionPropertyKind Kind,
    double Mass,
    GpPoint CenterOfMass,
    InertiaTensor Inertia,
    InspectionUnits Units)
{
    public double DisplayValue => Kind switch
    {
        InspectionPropertyKind.Length => Units.ConvertLength(Mass),
        InspectionPropertyKind.Area => Units.ConvertArea(Mass),
        InspectionPropertyKind.Volume => Units.ConvertVolume(Mass),
        _ => throw new InvalidOperationException()
    };
}

public enum RadialGeometryKind { Circle = 0, Cylinder = 1, Cone = 2 }

public readonly record struct ShapeRadialMeasurement(
    RadialGeometryKind GeometryKind,
    double Radius,
    double Diameter,
    double SemiAngleRadians,
    InspectionUnits Units);

public readonly record struct ShapeAngleMeasurement(double Radians, InspectionUnits Units)
{
    public double DisplayValue => Units.ConvertAngle(Radians);
    public string DisplayText => Units.FormatAngle(Radians);
}

#pragma warning restore CS1591
