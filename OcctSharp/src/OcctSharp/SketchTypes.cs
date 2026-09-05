using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public readonly record struct SketchPoint2d(double X, double Y)
{
    public static SketchPoint2d Origin => new(0.0, 0.0);
    public double DistanceTo(SketchPoint2d other) => Length(other.X - X, other.Y - Y);
    public SketchPoint2d Translated(SketchVector2d vector) => new(X + vector.X, Y + vector.Y);
    internal void Validate(string name)
    {
        if (!double.IsFinite(X) || !double.IsFinite(Y))
            throw new ArgumentOutOfRangeException(name, "Sketch point coordinates must be finite.");
    }
    private static double Length(double x, double y) => Math.Sqrt(x * x + y * y);
}

public readonly record struct SketchVector2d(double X, double Y)
{
    public double Magnitude => Math.Sqrt(X * X + Y * Y);
    public SketchDirection2d Normalized() => SketchDirection2d.Create(X, Y);
    internal void Validate(string name)
    {
        if (!double.IsFinite(X) || !double.IsFinite(Y))
            throw new ArgumentOutOfRangeException(name, "Sketch vector coordinates must be finite.");
    }
}

public readonly record struct SketchDirection2d
{
    private SketchDirection2d(double x, double y) => (X, Y) = (x, y);
    public double X { get; }
    public double Y { get; }
    public static SketchDirection2d XAxis => new(1.0, 0.0);
    public static SketchDirection2d YAxis => new(0.0, 1.0);
    public static SketchDirection2d Create(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(x), "Sketch direction coordinates must be finite.");
        double scale = Math.Max(Math.Abs(x), Math.Abs(y));
        if (scale <= 1e-15)
            throw new ArgumentException("A sketch direction must be non-zero.");
        double scaledX = x / scale, scaledY = y / scale;
        double magnitude = double.Hypot(scaledX, scaledY);
        return new(scaledX / magnitude, scaledY / magnitude);
    }
    public SketchDirection2d Reversed() => new(-X, -Y);
    internal void Validate() { if (X == 0 && Y == 0) throw new ArgumentException("A sketch direction must be non-zero."); }
}

/// <summary>An immutable orthonormal 3D placement for local 2D sketch coordinates.</summary>
public sealed class SketchPlane
{
    public SketchPlane(GpPoint origin, GpXyz xDirection, GpXyz yDirection)
    {
        ValidateFinite(origin.X, nameof(origin)); ValidateFinite(origin.Y, nameof(origin)); ValidateFinite(origin.Z, nameof(origin));
        ValidateFinite(xDirection.X, nameof(xDirection)); ValidateFinite(xDirection.Y, nameof(xDirection)); ValidateFinite(xDirection.Z, nameof(xDirection));
        ValidateFinite(yDirection.X, nameof(yDirection)); ValidateFinite(yDirection.Y, nameof(yDirection)); ValidateFinite(yDirection.Z, nameof(yDirection));
        GpXyz x = Normalize(xDirection, nameof(xDirection));
        double projection = Dot(yDirection, x);
        GpXyz orthogonalY = new(
            yDirection.X - projection * x.X,
            yDirection.Y - projection * x.Y,
            yDirection.Z - projection * x.Z);
        GpXyz y = Normalize(orthogonalY, nameof(yDirection));
        if (Math.Abs(Dot(x, y)) > 1e-12)
            throw new ArgumentException("Sketch plane axes must be orthogonal.");
        Origin = origin; XDirection = x; YDirection = y;
        Normal = Normalize(Cross(x, y), nameof(yDirection));
    }

    public static SketchPlane XY { get; } = new(new GpPoint(0, 0, 0), new GpXyz(1, 0, 0), new GpXyz(0, 1, 0));
    public static SketchPlane XZ { get; } = new(new GpPoint(0, 0, 0), new GpXyz(1, 0, 0), new GpXyz(0, 0, 1));
    public static SketchPlane YZ { get; } = new(new GpPoint(0, 0, 0), new GpXyz(0, 1, 0), new GpXyz(0, 0, 1));
    public GpPoint Origin { get; }
    public GpXyz XDirection { get; }
    public GpXyz YDirection { get; }
    public GpXyz Normal { get; }

    public GpPoint ToWorld(SketchPoint2d point)
    {
        point.Validate(nameof(point));
        return new(
            Origin.X + point.X * XDirection.X + point.Y * YDirection.X,
            Origin.Y + point.X * XDirection.Y + point.Y * YDirection.Y,
            Origin.Z + point.X * XDirection.Z + point.Y * YDirection.Z);
    }

    public SketchPoint2d ToLocal(GpPoint point, double tolerance = 1e-9)
    {
        ValidateTolerance(tolerance, nameof(tolerance));
        ValidateFinite(point.X, nameof(point)); ValidateFinite(point.Y, nameof(point)); ValidateFinite(point.Z, nameof(point));
        GpXyz delta = new(point.X - Origin.X, point.Y - Origin.Y, point.Z - Origin.Z);
        double normalDistance = Dot(delta, Normal);
        if (Math.Abs(normalDistance) > tolerance)
            throw new ArgumentException("The world point does not lie in this sketch plane within tolerance.", nameof(point));
        return new(Dot(delta, XDirection), Dot(delta, YDirection));
    }

    internal SketchPlaneRaw ToRaw() => new(
        new(Origin.X, Origin.Y, Origin.Z),
        new(XDirection.X, XDirection.Y, XDirection.Z),
        new(YDirection.X, YDirection.Y, YDirection.Z));

    internal GpXyz ToWorldDirection(SketchDirection2d direction) => new(
        direction.X * XDirection.X + direction.Y * YDirection.X,
        direction.X * XDirection.Y + direction.Y * YDirection.Y,
        direction.X * XDirection.Z + direction.Y * YDirection.Z);

    private static GpXyz Normalize(GpXyz value, string name)
    {
        double length = Math.Sqrt(Dot(value, value));
        if (!double.IsFinite(length) || length <= 1e-15)
            throw new ArgumentException("A sketch plane axis must be finite and non-zero.", name);
        return new(value.X / length, value.Y / length, value.Z / length);
    }
    private static GpXyz Cross(GpXyz a, GpXyz b) => new(
        a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    private static double Dot(GpXyz a, GpXyz b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    private static void ValidateFinite(double value, string name) { if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name); }
    private static void ValidateTolerance(double value, string name) { if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(name); }
}

public enum SketchCurveKind { Segment = 1, Circle = 2, Ellipse = 3, Bezier = 4, BSpline = 5 }

public readonly record struct SketchEvaluation(
    SketchPoint2d Point, SketchVector2d FirstDerivative, double NativeParameter, double NormalizedParameter);

public readonly record struct SketchProjection(
    SketchPoint2d Point, double NativeParameter, double NormalizedParameter, double Distance);

public readonly record struct SketchIntersection(
    SketchPoint2d Point, double FirstNativeParameter, double SecondNativeParameter,
    double FirstNormalizedParameter, double SecondNormalizedParameter);

/// <summary>An immutable copied analytic or freeform 2D curve definition.</summary>
public sealed class SketchCurve2d
{
    private readonly SketchPoint2d[] poles;
    private readonly double[] weights;
    private readonly double[] knots;
    private readonly int[] multiplicities;

    private SketchCurve2d(
        SketchCurveKind kind, SketchPoint2d[] poles, double[]? weights, double[]? knots,
        int[]? multiplicities, int degree, bool periodic, bool reversed,
        double firstParameter, double lastParameter, double majorRadius = 0.0,
        double minorRadius = 0.0, double axisAngle = 0.0)
    {
        Kind = kind; this.poles = (SketchPoint2d[])poles.Clone();
        this.weights = weights is null ? [] : (double[])weights.Clone();
        this.knots = knots is null ? [] : (double[])knots.Clone();
        this.multiplicities = multiplicities is null ? [] : (int[])multiplicities.Clone();
        Degree = degree; Periodic = periodic; Reversed = reversed;
        FirstParameter = firstParameter; LastParameter = lastParameter;
        MajorRadius = majorRadius; MinorRadius = minorRadius; AxisAngle = axisAngle;
        Validate();
    }

    public SketchCurveKind Kind { get; }
    public IReadOnlyList<SketchPoint2d> Poles => Array.AsReadOnly((SketchPoint2d[])poles.Clone());
    public IReadOnlyList<double> Weights => Array.AsReadOnly((double[])weights.Clone());
    public IReadOnlyList<double> Knots => Array.AsReadOnly((double[])knots.Clone());
    public IReadOnlyList<int> Multiplicities => Array.AsReadOnly((int[])multiplicities.Clone());
    public int Degree { get; }
    public bool Periodic { get; }
    public bool Reversed { get; }
    public bool IsRational => weights.Length != 0;
    public double FirstParameter { get; }
    public double LastParameter { get; }
    public double MajorRadius { get; }
    public double MinorRadius { get; }
    public double AxisAngle { get; }

    public static SketchCurve2d Segment(SketchPoint2d start, SketchPoint2d end)
    {
        start.Validate(nameof(start)); end.Validate(nameof(end));
        double length = start.DistanceTo(end);
        if (length <= 1e-12) throw new ArgumentException("A segment must have non-zero length.");
        return new(SketchCurveKind.Segment, [start, end], null, null, null, 1, false, false, 0.0, length);
    }

    public static SketchCurve2d Circle(SketchPoint2d center, double radius, double axisAngle = 0.0) =>
        CircularArc(center, radius, 0.0, Math.Tau, axisAngle);

    public static SketchCurve2d CircularArc(
        SketchPoint2d center, double radius, double startAngle, double sweepAngle,
        double axisAngle = 0.0)
    {
        center.Validate(nameof(center)); ValidatePositive(radius, nameof(radius));
        ValidateFinite(startAngle, nameof(startAngle)); ValidateFinite(sweepAngle, nameof(sweepAngle)); ValidateFinite(axisAngle, nameof(axisAngle));
        if (Math.Abs(sweepAngle) <= 1e-12 || Math.Abs(sweepAngle) > Math.Tau + 1e-12)
            throw new ArgumentOutOfRangeException(nameof(sweepAngle), "A circular sweep must be non-zero and no greater than one revolution.");
        bool reversed = sweepAngle < 0.0;
        double first = reversed ? startAngle + sweepAngle : startAngle;
        double last = reversed ? startAngle : startAngle + sweepAngle;
        return new(SketchCurveKind.Circle, [center], null, null, null, 2, false, reversed, first, last, radius, radius, axisAngle);
    }

    public static SketchCurve2d Ellipse(
        SketchPoint2d center, double majorRadius, double minorRadius, double axisAngle = 0.0) =>
        EllipticArc(center, majorRadius, minorRadius, 0.0, Math.Tau, axisAngle);

    public static SketchCurve2d EllipticArc(
        SketchPoint2d center, double majorRadius, double minorRadius,
        double startAngle, double sweepAngle, double axisAngle = 0.0)
    {
        center.Validate(nameof(center)); ValidatePositive(majorRadius, nameof(majorRadius)); ValidatePositive(minorRadius, nameof(minorRadius));
        if (minorRadius > majorRadius) throw new ArgumentException("Ellipse major radius must not be smaller than its minor radius.");
        ValidateFinite(startAngle, nameof(startAngle)); ValidateFinite(sweepAngle, nameof(sweepAngle)); ValidateFinite(axisAngle, nameof(axisAngle));
        if (Math.Abs(sweepAngle) <= 1e-12 || Math.Abs(sweepAngle) > Math.Tau + 1e-12)
            throw new ArgumentOutOfRangeException(nameof(sweepAngle), "An elliptic sweep must be non-zero and no greater than one revolution.");
        bool reversed = sweepAngle < 0.0;
        double first = reversed ? startAngle + sweepAngle : startAngle;
        double last = reversed ? startAngle : startAngle + sweepAngle;
        return new(SketchCurveKind.Ellipse, [center], null, null, null, 2, false, reversed,
            first, last, majorRadius, minorRadius, axisAngle);
    }

    public static SketchCurve2d Bezier(IReadOnlyList<SketchPoint2d> poles, IReadOnlyList<double>? weights = null)
    {
        SketchPoint2d[] copied = CopyPoints(poles, 2);
        double[] copiedWeights = CopyWeights(weights, copied.Length);
        return new(SketchCurveKind.Bezier, copied, copiedWeights, null, null, copied.Length - 1,
            false, false, 0.0, 1.0);
    }

    public static SketchCurve2d BSpline(
        IReadOnlyList<SketchPoint2d> poles, IReadOnlyList<double> knots,
        IReadOnlyList<int> multiplicities, int degree, bool periodic = false,
        IReadOnlyList<double>? weights = null)
    {
        SketchPoint2d[] copiedPoles = CopyPoints(poles, 2);
        ArgumentNullException.ThrowIfNull(knots); ArgumentNullException.ThrowIfNull(multiplicities);
        double[] copiedKnots = knots.ToArray(); int[] copiedMultiplicities = multiplicities.ToArray();
        if (copiedKnots.Length < 2) throw new ArgumentException("A B-spline requires at least two knots.", nameof(knots));
        SketchCurve2d definition = new(SketchCurveKind.BSpline, copiedPoles, CopyWeights(weights, copiedPoles.Length),
            copiedKnots, copiedMultiplicities, degree, periodic, false, copiedKnots[0], copiedKnots[^1]);
        if (periodic) return definition;
        int first = 0, last = copiedKnots.Length - 1;
        int firstMultiplicity = copiedMultiplicities[first], lastMultiplicity = copiedMultiplicities[last];
        while (firstMultiplicity <= degree) firstMultiplicity += copiedMultiplicities[++first];
        while (lastMultiplicity <= degree) lastMultiplicity += copiedMultiplicities[--last];
        return definition.Copy(firstParameter: copiedKnots[first], lastParameter: copiedKnots[last]);
    }

    /// <summary>Interpolates the input points with a degree-one (piecewise linear) B-spline.</summary>
    public static SketchCurve2d Interpolate(IReadOnlyList<SketchPoint2d> points)
    {
        SketchPoint2d[] copied = CopyPoints(points, 2);
        double[] knots = Enumerable.Range(0, copied.Length).Select(value => (double)value).ToArray();
        int[] multiplicities = Enumerable.Repeat(1, copied.Length).ToArray();
        multiplicities[0] = multiplicities[^1] = 2;
        return BSpline(copied, knots, multiplicities, 1);
    }

    public SketchCurve2d Trim(double firstNormalizedParameter, double lastNormalizedParameter)
    {
        ValidateNormalized(firstNormalizedParameter, nameof(firstNormalizedParameter));
        ValidateNormalized(lastNormalizedParameter, nameof(lastNormalizedParameter));
        if (firstNormalizedParameter >= lastNormalizedParameter)
            throw new ArgumentException("Trim parameters must be increasing.");
        return Reversed
            ? Copy(firstParameter: ToNative(1 - lastNormalizedParameter), lastParameter: ToNative(1 - firstNormalizedParameter))
            : Copy(firstParameter: ToNative(firstNormalizedParameter), lastParameter: ToNative(lastNormalizedParameter));
    }

    public IReadOnlyList<SketchCurve2d> Split(IReadOnlyList<double> normalizedParameters)
    {
        ArgumentNullException.ThrowIfNull(normalizedParameters);
        double[] values = normalizedParameters.ToArray();
        for (int index = 0; index < values.Length; ++index)
        {
            ValidateNormalized(values[index], nameof(normalizedParameters));
            if (values[index] <= 0.0 || values[index] >= 1.0 || index > 0 && values[index] <= values[index - 1])
                throw new ArgumentException("Split parameters must be strictly increasing inside (0,1).", nameof(normalizedParameters));
        }
        List<SketchCurve2d> result = new(values.Length + 1); double first = 0.0;
        foreach (double value in values) { result.Add(Trim(first, value)); first = value; }
        result.Add(Trim(first, 1.0)); return result;
    }

    public SketchCurve2d Reverse() => Copy(reversed: !Reversed);

    public SketchCurve2d Transform(SketchTransform2d transform)
    {
        SketchPoint2d[] transformed = poles.Select(transform.Apply).ToArray();
        double scale = transform.UniformScale;
        SketchVector2d transformedAxis = transform.Apply(new SketchVector2d(Math.Cos(AxisAngle), Math.Sin(AxisAngle)));
        double axis = Math.Atan2(transformedAxis.Y, transformedAxis.X);
        bool analytic = Kind is SketchCurveKind.Circle or SketchCurveKind.Ellipse;
        bool mirror = analytic && transform.ReversesOrientation;
        bool reverse = Reversed ^ mirror;
        double first = mirror ? -LastParameter : FirstParameter;
        double last = mirror ? -FirstParameter : LastParameter;
        if (Kind == SketchCurveKind.Segment) { first *= scale; last *= scale; }
        return new(Kind, transformed, weights, knots, multiplicities, Degree, Periodic, reverse,
            first, last, Math.Abs(MajorRadius * scale), Math.Abs(MinorRadius * scale), axis);
    }

    public SketchEvaluation Evaluate(double parameter, bool normalized = true) => SketchModeling.Evaluate(this, parameter, normalized);
    public IReadOnlyList<SketchProjection> Project(SketchPoint2d point) => SketchModeling.Project(this, point);
    public IReadOnlyList<SketchIntersection> Intersect(SketchCurve2d other, double tolerance = 1e-7) =>
        SketchModeling.Intersect(this, other, tolerance);
    public Shape ToEdge(SketchPlane plane) => SketchModeling.CreateEdge(this, plane);

    internal double ToNative(double normalized) => normalized switch
    {
        0 => FirstParameter,
        1 => LastParameter,
        _ => FirstParameter + normalized * (LastParameter - FirstParameter)
    };
    internal double ToNormalized(double native) => (native - FirstParameter) / (LastParameter - FirstParameter);

    internal unsafe TResult WithRaw<TResult>(RawAction<TResult> action)
    {
        SketchPoint2dRaw[] rawPoles = poles.Select(value => new SketchPoint2dRaw(value.X, value.Y)).ToArray();
        fixed (SketchPoint2dRaw* polePointer = rawPoles)
        fixed (double* weightPointer = weights)
        fixed (double* knotPointer = knots)
        fixed (int* multiplicityPointer = multiplicities)
        {
            SketchCurveRaw raw = new()
            {
                Kind = (int)Kind, Degree = Degree, Periodic = Periodic ? 1 : 0,
                Rational = weights.Length == 0 ? 0 : 1, Reversed = Reversed ? 1 : 0,
                PoleCount = rawPoles.Length, KnotCount = knots.Length,
                FirstParameter = FirstParameter, LastParameter = LastParameter,
                MajorRadius = MajorRadius, MinorRadius = MinorRadius, AxisAngle = AxisAngle,
                Poles = polePointer, Weights = weights.Length == 0 ? null : weightPointer,
                Knots = knots.Length == 0 ? null : knotPointer,
                Multiplicities = multiplicities.Length == 0 ? null : multiplicityPointer
            };
            return action(&raw);
        }
    }

    internal unsafe delegate TResult RawAction<TResult>(SketchCurveRaw* raw);

    private SketchCurve2d Copy(bool? reversed = null, double? firstParameter = null, double? lastParameter = null) =>
        new(Kind, poles, weights, knots, multiplicities, Degree, Periodic, reversed ?? Reversed,
            firstParameter ?? FirstParameter, lastParameter ?? LastParameter, MajorRadius, MinorRadius, AxisAngle);

    private void Validate()
    {
        if (!Enum.IsDefined(Kind)) throw new ArgumentOutOfRangeException(nameof(Kind));
        foreach (SketchPoint2d pole in poles) pole.Validate(nameof(Poles));
        if (!double.IsFinite(FirstParameter) || !double.IsFinite(LastParameter) || FirstParameter >= LastParameter)
            throw new ArgumentException("Curve parameter range must be finite and increasing.");
        if (Kind == SketchCurveKind.Bezier && (poles.Length < 2 || poles.Length > 26))
            throw new ArgumentException("A Bezier curve requires 2 through 26 poles.");
        if (Kind != SketchCurveKind.BSpline) return;
        if (Degree is < 1 or > 25 || knots.Length < 2 || knots.Length != multiplicities.Length)
            throw new ArgumentException("B-spline degree, knots, or multiplicities are invalid.");
        if (Periodic && multiplicities[0] != multiplicities[^1])
            throw new ArgumentException("Periodic B-spline end multiplicities must match.");
        for (int index = 0; index < knots.Length; ++index)
        {
            if (!double.IsFinite(knots[index]) || index > 0 && knots[index] <= knots[index - 1])
                throw new ArgumentException("B-spline knots must be finite and strictly increasing.");
            int maximum = !Periodic && (index == 0 || index == knots.Length - 1) ? Degree + 1 : Degree;
            if (multiplicities[index] < 1 || multiplicities[index] > maximum)
                throw new ArgumentException("A B-spline multiplicity is outside its degree relationship.");
        }
        int expected = Periodic ? multiplicities.Sum() - multiplicities[0] : multiplicities.Sum() - Degree - 1;
        if (expected != poles.Length) throw new ArgumentException($"This B-spline requires {expected} poles, not {poles.Length}.");
    }

    private static SketchPoint2d[] CopyPoints(IReadOnlyList<SketchPoint2d> values, int minimum)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count < minimum) throw new ArgumentException($"At least {minimum} points are required.", nameof(values));
        SketchPoint2d[] result = values.ToArray(); foreach (SketchPoint2d point in result) point.Validate(nameof(values)); return result;
    }
    private static double[] CopyWeights(IReadOnlyList<double>? values, int poleCount)
    {
        if (values is null) return [];
        double[] result = values.ToArray();
        if (result.Length != poleCount || result.Any(value => !double.IsFinite(value) || value <= 0.0))
            throw new ArgumentException("Weights must match the pole count and be finite and positive.", nameof(values));
        return result;
    }
    private static void ValidateFinite(double value, string name) { if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name); }
    private static void ValidatePositive(double value, string name) { if (!double.IsFinite(value) || value <= 0.0) throw new ArgumentOutOfRangeException(name); }
    private static void ValidateNormalized(double value, string name) { if (!double.IsFinite(value) || value < 0.0 || value > 1.0) throw new ArgumentOutOfRangeException(name); }
}

/// <summary>An immutable uniform affine transform for copied sketch definitions.</summary>
public readonly record struct SketchTransform2d(
    double M11, double M12, double M21, double M22, double TranslationX, double TranslationY)
{
    public static SketchTransform2d Identity => new(1, 0, 0, 1, 0, 0);
    public static SketchTransform2d Translation(double x, double y) => new(1, 0, 0, 1, x, y);
    public static SketchTransform2d Rotation(double radians, SketchPoint2d center = default)
    {
        if (!double.IsFinite(radians)) throw new ArgumentOutOfRangeException(nameof(radians)); center.Validate(nameof(center));
        double c = Math.Cos(radians), s = Math.Sin(radians);
        return new(c, -s, s, c, center.X - c * center.X + s * center.Y, center.Y - s * center.X - c * center.Y);
    }
    public static SketchTransform2d Scale(double factor, SketchPoint2d center = default)
    {
        if (!double.IsFinite(factor) || Math.Abs(factor) <= 1e-15) throw new ArgumentOutOfRangeException(nameof(factor)); center.Validate(nameof(center));
        return new(factor, 0, 0, factor, center.X * (1 - factor), center.Y * (1 - factor));
    }
    public static SketchTransform2d Mirror(SketchPoint2d origin, SketchDirection2d axis)
    {
        axis.Validate();
        origin.Validate(nameof(origin)); double x = axis.X, y = axis.Y;
        double m11 = 2 * x * x - 1, m12 = 2 * x * y, m22 = 2 * y * y - 1;
        return new(m11, m12, m12, m22,
            origin.X - m11 * origin.X - m12 * origin.Y,
            origin.Y - m12 * origin.X - m22 * origin.Y);
    }
    public SketchPoint2d Apply(SketchPoint2d point) => new(
        M11 * point.X + M12 * point.Y + TranslationX,
        M21 * point.X + M22 * point.Y + TranslationY);
    public SketchVector2d Apply(SketchVector2d vector) => new(M11 * vector.X + M12 * vector.Y, M21 * vector.X + M22 * vector.Y);
    public double UniformScale
    {
        get
        {
            Validate(); double sx = Math.Sqrt(M11 * M11 + M21 * M21), sy = Math.Sqrt(M12 * M12 + M22 * M22);
            if (!double.IsFinite(sx) || !double.IsFinite(sy) || sx <= 1e-15 || sy <= 1e-15)
                throw new InvalidOperationException("Sketch transforms must have finite non-zero scale.");
            if (Math.Abs(sx - sy) > 1e-10 * Math.Max(1.0, sx)) throw new InvalidOperationException("Sketch curves support only uniform affine scale.");
            return sx;
        }
    }
    public double RotationAngle => Math.Atan2(M21, M11);
    public bool ReversesOrientation => M11 * M22 - M12 * M21 < 0.0;
    private void Validate()
    {
        if (!double.IsFinite(M11) || !double.IsFinite(M12) || !double.IsFinite(M21) || !double.IsFinite(M22)
            || !double.IsFinite(TranslationX) || !double.IsFinite(TranslationY))
            throw new InvalidOperationException("Sketch transform coefficients must be finite.");
        double dot = M11 * M12 + M21 * M22;
        if (Math.Abs(dot) > 1e-10) throw new InvalidOperationException("Sketch transforms cannot contain shear.");
    }
}

#pragma warning restore CS1591
