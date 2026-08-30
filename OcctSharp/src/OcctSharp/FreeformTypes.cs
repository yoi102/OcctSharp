namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Supported copied freeform definition families.</summary>
public enum FreeformGeometryKind { Bezier = 1, BSpline = 2 }

/// <summary>Requested geometric continuity.</summary>
public enum FreeformContinuity { C0 = 0, C1 = 1, C2 = 2, C3 = 3 }

/// <summary>Join treatment for a planar wire offset.</summary>
public enum PlanarOffsetJoin { Arc = 0, Tangent = 1, Intersection = 2 }

/// <summary>Transition treatment at a swept spine discontinuity.</summary>
public enum PipeTransition { Transformed = 0, RightCorner = 1, RoundCorner = 2 }

/// <summary>Finite parameter interval.</summary>
public readonly record struct ParameterRange(double First, double Last)
{
    internal void Validate(string name)
    {
        if (!double.IsFinite(First) || !double.IsFinite(Last) || First >= Last)
            throw new ArgumentOutOfRangeException(name, "Parameter bounds must be finite and increasing.");
    }
}

/// <summary>Finite rectangular surface parameter bounds.</summary>
public readonly record struct SurfaceParameterBounds(double FirstU, double LastU, double FirstV, double LastV)
{
    internal void Validate(string name)
    {
        if (!double.IsFinite(FirstU) || !double.IsFinite(LastU) || !double.IsFinite(FirstV) || !double.IsFinite(LastV)
            || FirstU >= LastU || FirstV >= LastV)
            throw new ArgumentOutOfRangeException(name, "Surface parameter bounds must be finite and increasing.");
    }
}

/// <summary>An immutable, fully copied Bezier or B-spline curve definition.</summary>
public sealed record FreeformCurveDefinition
{
    private readonly GpPoint[] poles;
    private readonly double[] weights;
    private readonly double[] knots;
    private readonly int[] multiplicities;

    private FreeformCurveDefinition(
        FreeformGeometryKind kind, IEnumerable<GpPoint> poles, IEnumerable<double>? weights,
        IEnumerable<double>? knots, IEnumerable<int>? multiplicities, int degree,
        bool periodic, ParameterRange? parameterRange)
    {
        Kind = kind;
        this.poles = poles?.ToArray() ?? throw new ArgumentNullException(nameof(poles));
        this.weights = weights?.ToArray() ?? [];
        this.knots = knots?.ToArray() ?? [];
        this.multiplicities = multiplicities?.ToArray() ?? [];
        Degree = degree;
        Periodic = periodic;
        ParameterRange = parameterRange;
        Validate();
    }

    public FreeformGeometryKind Kind { get; }
    public IReadOnlyList<GpPoint> Poles => Array.AsReadOnly((GpPoint[])poles.Clone());
    public IReadOnlyList<double> Weights => Array.AsReadOnly((double[])weights.Clone());
    public IReadOnlyList<double> Knots => Array.AsReadOnly((double[])knots.Clone());
    public IReadOnlyList<int> Multiplicities => Array.AsReadOnly((int[])multiplicities.Clone());
    public int Degree { get; }
    public bool Periodic { get; }
    public bool IsRational => weights.Length != 0 && weights.Any(value => Math.Abs(value - weights[0]) > 1e-14);
    public ParameterRange? ParameterRange { get; }

    public static FreeformCurveDefinition Bezier(
        IEnumerable<GpPoint> poles, IEnumerable<double>? weights = null,
        ParameterRange? parameterRange = null)
    {
        GpPoint[] copiedPoles = poles?.ToArray() ?? throw new ArgumentNullException(nameof(poles));
        return new(FreeformGeometryKind.Bezier, copiedPoles, weights, null, null, copiedPoles.Length - 1, false, parameterRange);
    }

    public static FreeformCurveDefinition BSpline(
        IEnumerable<GpPoint> poles, IEnumerable<double> knots, IEnumerable<int> multiplicities,
        int degree, bool periodic = false, IEnumerable<double>? weights = null,
        ParameterRange? parameterRange = null) =>
        new(FreeformGeometryKind.BSpline, poles, weights, knots, multiplicities, degree, periodic, parameterRange);

    internal GpPoint[] CopyPoles() => (GpPoint[])poles.Clone();
    internal double[] CopyWeights() => (double[])weights.Clone();
    internal double[] CopyKnots() => (double[])knots.Clone();
    internal int[] CopyMultiplicities() => (int[])multiplicities.Clone();

    private void Validate()
    {
        if (poles.Length < 2) throw new ArgumentException("A freeform curve requires at least two poles.", nameof(Poles));
        foreach (GpPoint pole in poles) ValidateFinite(pole, nameof(Poles));
        if (weights.Length != 0 && weights.Length != poles.Length)
            throw new ArgumentException("Curve weights must be omitted or match the pole count.", nameof(Weights));
        if (weights.Any(weight => !double.IsFinite(weight) || weight <= 0.0))
            throw new ArgumentOutOfRangeException(nameof(Weights), "Curve weights must be finite and greater than zero.");
        ParameterRange?.Validate(nameof(ParameterRange));
        if (Kind == FreeformGeometryKind.Bezier)
        {
            if (poles.Length > 26) throw new ArgumentException("OCCT supports Bezier degree at most 25.", nameof(Poles));
            return;
        }
        if (Degree is < 1 or > 25) throw new ArgumentOutOfRangeException(nameof(Degree), "B-spline degree must be between 1 and 25.");
        if (knots.Length < 2 || knots.Length != multiplicities.Length)
            throw new ArgumentException("B-spline knots and multiplicities must have equal length of at least two.");
        for (int index = 0; index < knots.Length; ++index)
        {
            if (!double.IsFinite(knots[index]) || index > 0 && knots[index] <= knots[index - 1])
                throw new ArgumentException("B-spline knots must be finite and strictly increasing.", nameof(Knots));
            int maximum = !Periodic && (index == 0 || index == knots.Length - 1) ? Degree + 1 : Degree;
            if (multiplicities[index] < 1 || multiplicities[index] > maximum)
                throw new ArgumentException("A B-spline multiplicity is outside the degree relationship.", nameof(Multiplicities));
        }
        int expectedPoles = Periodic ? multiplicities.Sum() - multiplicities[0] : multiplicities.Sum() - Degree - 1;
        if (expectedPoles != poles.Length)
            throw new ArgumentException($"The B-spline definition requires {expectedPoles} poles for its degree and multiplicities, not {poles.Length}.");
        if (Periodic && multiplicities[0] != multiplicities[^1])
            throw new ArgumentException("Periodic B-spline first and last multiplicities must match.", nameof(Multiplicities));
    }

    private static void ValidateFinite(GpPoint value, string name)
    {
        if (!double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Z))
            throw new ArgumentOutOfRangeException(name, "Point coordinates must be finite.");
    }
}

/// <summary>An immutable, rectangular, fully copied Bezier or B-spline surface definition.</summary>
public sealed record FreeformSurfaceDefinition
{
    private readonly GpPoint[] poles;
    private readonly double[] weights;
    private readonly double[] uKnots;
    private readonly double[] vKnots;
    private readonly int[] uMultiplicities;
    private readonly int[] vMultiplicities;

    private FreeformSurfaceDefinition(
        FreeformGeometryKind kind, int uPoleCount, int vPoleCount, IEnumerable<GpPoint> poles,
        IEnumerable<double>? weights, IEnumerable<double>? uKnots, IEnumerable<int>? uMultiplicities,
        IEnumerable<double>? vKnots, IEnumerable<int>? vMultiplicities, int uDegree, int vDegree,
        bool uPeriodic, bool vPeriodic, SurfaceParameterBounds? bounds)
    {
        Kind = kind; UPoleCount = uPoleCount; VPoleCount = vPoleCount;
        this.poles = poles?.ToArray() ?? throw new ArgumentNullException(nameof(poles));
        this.weights = weights?.ToArray() ?? [];
        this.uKnots = uKnots?.ToArray() ?? []; this.vKnots = vKnots?.ToArray() ?? [];
        this.uMultiplicities = uMultiplicities?.ToArray() ?? []; this.vMultiplicities = vMultiplicities?.ToArray() ?? [];
        UDegree = uDegree; VDegree = vDegree; UPeriodic = uPeriodic; VPeriodic = vPeriodic; Bounds = bounds;
        Validate();
    }

    public FreeformGeometryKind Kind { get; }
    public int UPoleCount { get; }
    public int VPoleCount { get; }
    public IReadOnlyList<GpPoint> Poles => Array.AsReadOnly((GpPoint[])poles.Clone());
    public IReadOnlyList<double> Weights => Array.AsReadOnly((double[])weights.Clone());
    public IReadOnlyList<double> UKnots => Array.AsReadOnly((double[])uKnots.Clone());
    public IReadOnlyList<double> VKnots => Array.AsReadOnly((double[])vKnots.Clone());
    public IReadOnlyList<int> UMultiplicities => Array.AsReadOnly((int[])uMultiplicities.Clone());
    public IReadOnlyList<int> VMultiplicities => Array.AsReadOnly((int[])vMultiplicities.Clone());
    public int UDegree { get; }
    public int VDegree { get; }
    public bool UPeriodic { get; }
    public bool VPeriodic { get; }
    public bool IsRational => weights.Length != 0 && weights.Any(value => Math.Abs(value - weights[0]) > 1e-14);
    public SurfaceParameterBounds? Bounds { get; }

    public static FreeformSurfaceDefinition Bezier(
        int uPoleCount, int vPoleCount, IEnumerable<GpPoint> poles,
        IEnumerable<double>? weights = null, SurfaceParameterBounds? bounds = null) =>
        new(FreeformGeometryKind.Bezier, uPoleCount, vPoleCount, poles, weights,
            null, null, null, null, uPoleCount - 1, vPoleCount - 1, false, false, bounds);

    public static FreeformSurfaceDefinition BSpline(
        int uPoleCount, int vPoleCount, IEnumerable<GpPoint> poles,
        IEnumerable<double> uKnots, IEnumerable<int> uMultiplicities,
        IEnumerable<double> vKnots, IEnumerable<int> vMultiplicities,
        int uDegree, int vDegree, bool uPeriodic = false, bool vPeriodic = false,
        IEnumerable<double>? weights = null, SurfaceParameterBounds? bounds = null) =>
        new(FreeformGeometryKind.BSpline, uPoleCount, vPoleCount, poles, weights,
            uKnots, uMultiplicities, vKnots, vMultiplicities, uDegree, vDegree,
            uPeriodic, vPeriodic, bounds);

    internal GpPoint[] CopyPoles() => (GpPoint[])poles.Clone();
    internal double[] CopyWeights() => (double[])weights.Clone();
    internal double[] CopyUKnots() => (double[])uKnots.Clone();
    internal double[] CopyVKnots() => (double[])vKnots.Clone();
    internal int[] CopyUMultiplicities() => (int[])uMultiplicities.Clone();
    internal int[] CopyVMultiplicities() => (int[])vMultiplicities.Clone();

    private void Validate()
    {
        if (UPoleCount < 2 || VPoleCount < 2 || poles.Length != checked(UPoleCount * VPoleCount))
            throw new ArgumentException("Surface poles must form the declared rectangular grid.", nameof(Poles));
        foreach (GpPoint pole in poles)
            if (!double.IsFinite(pole.X) || !double.IsFinite(pole.Y) || !double.IsFinite(pole.Z))
                throw new ArgumentOutOfRangeException(nameof(Poles), "Surface pole coordinates must be finite.");
        if (weights.Length != 0 && weights.Length != poles.Length)
            throw new ArgumentException("Surface weights must be omitted or match the pole grid.", nameof(Weights));
        if (weights.Any(value => !double.IsFinite(value) || value <= 0.0))
            throw new ArgumentOutOfRangeException(nameof(Weights), "Surface weights must be finite and greater than zero.");
        Bounds?.Validate(nameof(Bounds));
        if (Kind == FreeformGeometryKind.Bezier)
        {
            if (UPoleCount > 26 || VPoleCount > 26) throw new ArgumentException("OCCT supports Bezier degree at most 25.");
            return;
        }
        ValidateDirection(UPoleCount, uKnots, uMultiplicities, UDegree, UPeriodic, "U");
        ValidateDirection(VPoleCount, vKnots, vMultiplicities, VDegree, VPeriodic, "V");
    }

    private static void ValidateDirection(int poleCount, double[] knots, int[] multiplicities, int degree, bool periodic, string axis)
    {
        if (degree is < 1 or > 25 || knots.Length < 2 || knots.Length != multiplicities.Length)
            throw new ArgumentException($"Surface {axis} degree, knots, and multiplicities are invalid.");
        for (int index = 0; index < knots.Length; ++index)
        {
            if (!double.IsFinite(knots[index]) || index > 0 && knots[index] <= knots[index - 1])
                throw new ArgumentException($"Surface {axis} knots must be finite and strictly increasing.");
            int maximum = !periodic && (index == 0 || index == knots.Length - 1) ? degree + 1 : degree;
            if (multiplicities[index] < 1 || multiplicities[index] > maximum)
                throw new ArgumentException($"A surface {axis} multiplicity is outside the degree relationship.");
        }
        int expected = periodic ? multiplicities.Sum() - multiplicities[0] : multiplicities.Sum() - degree - 1;
        if (expected != poleCount) throw new ArgumentException($"The surface {axis} definition requires {expected} poles, not {poleCount}.");
        if (periodic && multiplicities[0] != multiplicities[^1])
            throw new ArgumentException($"Periodic surface {axis} first and last multiplicities must match.");
    }
}

/// <summary>One copied projection, extrema, or curve/surface-intersection solution.</summary>
public readonly record struct FreeformSolution(
    GpPoint FirstPoint, GpPoint SecondPoint, double FirstParameter,
    double SecondParameter, double ThirdParameter, double Distance);

/// <summary>Copied algorithm status, history counts, validity, continuity error, and approximation error.</summary>
public readonly record struct FreeformDiagnostics(
    int Status, int InputCount, int ResultCount, int ModifiedCount, int GeneratedCount,
    int DeletedCount, bool IsValid, bool IsClosed, double G0Error, double G1Error,
    double G2Error, double ApproximationError);

/// <summary>An independently owned topology result and its copied diagnostics.</summary>
public sealed class FreeformShapeResult : IDisposable
{
    internal FreeformShapeResult(Shape shape, FreeformDiagnostics diagnostics) => (Shape, Diagnostics) = (shape, diagnostics);
    public Shape Shape { get; }
    public FreeformDiagnostics Diagnostics { get; }
    public void Dispose() => Shape.Dispose();
}
#pragma warning restore CS1591
