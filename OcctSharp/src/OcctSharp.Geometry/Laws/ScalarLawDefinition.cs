using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum ScalarLawKind { Constant, Linear, Interpolated, BSpline, Smooth, Composite }
public enum LawDomainPolicy { Reject, Clamp }
public readonly record struct LawDomain(double First, double Last)
{
    internal void Validate()
    {
        if (!double.IsFinite(First) || !double.IsFinite(Last) || First >= Last || !double.IsFinite(Last - First))
            throw new ArgumentOutOfRangeException(nameof(LawDomain), "The domain must be finite and increasing.");
    }
}
public sealed record ScalarLawSample(double Parameter, double Value, double? FirstDerivative, double? SecondDerivative);
public sealed record ScalarLawJoin(double Parameter, double ValueJump, double? FirstDerivativeJump, double? SecondDerivativeJump);
public sealed record ScalarLawSpanDefinition(ScalarLawKind Kind, LawDomain DefinitionDomain, LawDomain ActiveDomain,
    int Degree, double FirstValue, double LastValue, double? FirstDerivative, double? LastDerivative,
    IReadOnlyList<double> Values, IReadOnlyList<double> Parameters, IReadOnlyList<int> Multiplicities);
public sealed record ScalarLawSamplingReport(IReadOnlyList<ScalarLawSample> Samples, double SampledMinimum,
    double SampledMaximum, double ConservativeLowerBound)
{
    /// <summary>A positive control hull proves positivity; a non-positive bound does not prove a negative law.</summary>
    public bool HasGlobalPositivityProof => ConservativeLowerBound > 0;
    public bool SamplesArePositive => SampledMinimum > 0;
}

/// <summary>Immutable bounded scalar definition. Native Law_Function objects never escape evaluation/build calls.</summary>
public sealed class ScalarLawDefinition
{
    private sealed record Span(LawSpanRaw Data, double[] Values, double[] Parameters, int[] Multiplicities, LawDomain? ActiveDomain = null);
    private readonly Span[] spans;
    public ScalarLawKind Kind { get; }
    public LawDomain Domain { get; }
    public IReadOnlyList<ScalarLawSpanDefinition> Spans => Array.AsReadOnly(spans.Select(s => new ScalarLawSpanDefinition(
        (ScalarLawKind)s.Data.Kind, new(s.Data.First, s.Data.Last), Active(s), s.Data.Degree, s.Data.ValueFirst, s.Data.ValueLast,
        s.Data.Tangents != 0 || s.Data.Kind == 4 ? s.Data.DerivativeFirst : null,
        s.Data.Tangents != 0 || s.Data.Kind == 4 ? s.Data.DerivativeLast : null,
        Array.AsReadOnly((double[])s.Values.Clone()), Array.AsReadOnly((double[])s.Parameters.Clone()), Array.AsReadOnly((int[])s.Multiplicities.Clone()))).ToArray());
    private static LawDomain Active(Span s) => s.ActiveDomain ?? new(s.Data.First, s.Data.Last);
    private ScalarLawDefinition(ScalarLawKind kind, LawDomain domain, Span[] copied)
    { domain.Validate(); Kind = kind; Domain = domain; spans = copied; }

    private static ScalarLawDefinition Endpoints(ScalarLawKind kind, LawDomain domain,
        double first, double last, double firstDerivative = 0, double lastDerivative = 0)
    {
        domain.Validate(); Finite([first, last, firstDerivative, lastDerivative]);
        return new(kind, domain, [new(new() { Kind = (int)kind, First = domain.First, Last = domain.Last,
            ValueFirst = first, ValueLast = last, DerivativeFirst = firstDerivative, DerivativeLast = lastDerivative }, [], [], [])]);
    }
    public static ScalarLawDefinition Constant(LawDomain domain, double value) => Endpoints(ScalarLawKind.Constant, domain, value, value);
    public static ScalarLawDefinition Linear(LawDomain domain, double first, double last) => Endpoints(ScalarLawKind.Linear, domain, first, last);
    public static ScalarLawDefinition Smooth(LawDomain domain, double first, double last,
        double firstDerivative = 0, double lastDerivative = 0) => Endpoints(ScalarLawKind.Smooth, domain, first, last, firstDerivative, lastDerivative);
    public static ScalarLawDefinition Interpolate(IEnumerable<double> parameters, IEnumerable<double> values,
        double? firstDerivative = null, double? lastDerivative = null)
    {
        double[] x = Copy(parameters), y = Copy(values);
        if (x.Length < 2 || x.Length != y.Length) throw new ArgumentException("Interpolation needs matching parameter/value arrays of length >= 2.");
        Increasing(x); Finite(y);
        if (firstDerivative.HasValue != lastDerivative.HasValue) throw new ArgumentException("Specify both endpoint derivatives or neither.");
        Finite([firstDerivative ?? 0, lastDerivative ?? 0]);
        var domain = new LawDomain(x[0], x[^1]); domain.Validate();
        return new(ScalarLawKind.Interpolated, domain, [new(new() { Kind = 2, First = domain.First, Last = domain.Last,
            Tangents = firstDerivative.HasValue ? 1 : 0, DerivativeFirst = firstDerivative ?? 0, DerivativeLast = lastDerivative ?? 0 }, y, x, [])]);
    }
    /// <summary>Non-periodic, non-rational scalar B-spline with copied poles and active knot domain.</summary>
    public static ScalarLawDefinition BSpline(IEnumerable<double> poles, IEnumerable<double> knots, IEnumerable<int> multiplicities, int degree)
    {
        double[] p = Copy(poles), k = Copy(knots); int[] m = Copy(multiplicities);
        if (degree is < 1 or > 25 || p.Length < degree + 1 || k.Length < 2 || k.Length != m.Length)
            throw new ArgumentException("Invalid B-spline dimensions or degree.");
        Finite(p); Increasing(k);
        for (int i = 0; i < m.Length; i++)
            if (m[i] < 1 || m[i] > degree + (i == 0 || i == m.Length - 1 ? 1 : 0)) throw new ArgumentException("Invalid knot multiplicity.");
        if (m.Sum(value => (long)value) != p.Length + degree + 1L) throw new ArgumentException("Invalid multiplicity sum.");
        int start = 0, end = m.Length - 1, sum = m[0];
        while (sum < degree + 1) sum += m[++start];
        sum = m[^1]; while (sum < degree + 1) sum += m[--end];
        var domain = new LawDomain(k[start], k[end]); domain.Validate();
        return new(ScalarLawKind.BSpline, domain, [new(new() { Kind = 3, Degree = degree, First = domain.First, Last = domain.Last }, p, k, m)]);
    }
    public static ScalarLawDefinition Composite(IEnumerable<ScalarLawDefinition> pieces)
    {
        ScalarLawDefinition[] copied = Copy(pieces, 256);
        if (copied.Length == 0) throw new ArgumentException("A composite requires at least one span.");
        List<Span> result = [];
        for (int i = 0; i < copied.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(copied[i]);
            if (i > 0 && copied[i - 1].Domain.Last != copied[i].Domain.First) throw new ArgumentException("Piece domains must be exactly consecutive.");
            foreach (Span span in copied[i].spans)
            {
                var active = Active(span); double first = Math.Max(active.First, copied[i].Domain.First), last = Math.Min(active.Last, copied[i].Domain.Last);
                if (first < last) result.Add(span with { ActiveDomain = new(first, last) });
            }
            if (result.Count > 256) throw new ArgumentException("At most 256 elementary spans are supported.");
        }
        return new(ScalarLawKind.Composite, new(copied[0].Domain.First, copied[^1].Domain.Last), result.ToArray());
    }
    public ScalarLawDefinition Trim(LawDomain domain)
    {
        domain.Validate(); if (domain.First < Domain.First || domain.Last > Domain.Last) throw new ArgumentOutOfRangeException(nameof(domain));
        return new(Kind, domain, (Span[])spans.Clone());
    }
    public ScalarLawDefinition MapDomain(LawDomain domain)
    {
        domain.Validate(); double ratio = (domain.Last - domain.First) / (Domain.Last - Domain.First);
        if (!double.IsFinite(ratio) || ratio <= 0) throw new ArgumentOutOfRangeException(nameof(domain));
        double Map(double p) => domain.First + (p - Domain.First) * ratio;
        Span[] mapped = spans.Select(span =>
        {
            LawSpanRaw data = span.Data;
            data.First = Map(data.First); data.Last = Map(data.Last);
            data.DerivativeFirst /= ratio; data.DerivativeLast /= ratio;
            double[] parameters = span.Parameters.Select(Map).ToArray();
            Finite([data.First, data.Last, data.DerivativeFirst, data.DerivativeLast]); Finite(parameters);
            var active = Active(span);
            return new Span(data, span.Values, parameters, span.Multiplicities, new(Map(active.First), Map(active.Last)));
        }).ToArray();
        return new(Kind, domain, mapped);
    }
    public ScalarLawSample Evaluate(double parameter, LawDomainPolicy policy = LawDomainPolicy.Reject) => EvaluateMany([parameter], policy).Samples[0];
    public unsafe ScalarLawSamplingReport EvaluateMany(IEnumerable<double> parameters, LawDomainPolicy policy = LawDomainPolicy.Reject)
    {
        if (!Enum.IsDefined(policy)) throw new ArgumentOutOfRangeException(nameof(policy));
        double[] input = Copy(parameters); if (input.Length == 0) throw new ArgumentException("At least one parameter is required.");
        Finite(input);
        for (int i = 0; i < input.Length; i++)
        {
            if (policy == LawDomainPolicy.Clamp) input[i] = Math.Clamp(input[i], Domain.First, Domain.Last);
            else if (input[i] < Domain.First || input[i] > Domain.Last) throw new ArgumentOutOfRangeException(nameof(parameters));
        }
        var buffers = Buffers(); LawSampleRaw[] output = new LawSampleRaw[input.Length]; double lower;
        fixed (LawSpanRaw* s = buffers.Spans) fixed (double* v = buffers.Values, p = input)
        fixed (int* m = buffers.Multiplicities) fixed (LawSampleRaw* o = output)
        {
            LawInputRaw raw = new() { Spans = s, Values = v, Multiplicities = m, SpanCount = buffers.Spans.Length,
                ValueCount = buffers.Values.Length, MultiplicityCount = buffers.Multiplicities.Length, First = Domain.First, Last = Domain.Last };
            NativeError.ThrowIfFailed(ScalarLawInterop.Evaluate(in raw, p, input.Length, o, output.Length, out lower), "law_evaluate");
        }
        ScalarLawSample[] samples = output.Select(raw => new ScalarLawSample(raw.Parameter, raw.Value,
            (raw.Defined & 1) != 0 ? raw.FirstDerivative : null, (raw.Defined & 2) != 0 ? raw.SecondDerivative : null)).ToArray();
        return new(Array.AsReadOnly(samples), samples.Min(s => s.Value), samples.Max(s => s.Value), lower);
    }
    public ScalarLawSamplingReport Sample(int count = 129)
    {
        if (count is < 2 or > 65536) throw new ArgumentOutOfRangeException(nameof(count));
        return EvaluateMany(Enumerable.Range(0, count).Select(i => i == count - 1 ? Domain.Last : Domain.First + (Domain.Last - Domain.First) * (i / (double)(count - 1))));
    }
    public IReadOnlyList<ScalarLawJoin> InspectJoins()
    {
        List<ScalarLawJoin> joins = [];
        for (int i = 1; i < spans.Length; i++)
        {
            double p = Active(spans[i]).First; if (p <= Domain.First || p >= Domain.Last) continue;
            ScalarLawSample left = new ScalarLawDefinition((ScalarLawKind)spans[i - 1].Data.Kind,
                new(Active(spans[i - 1]).First, p), [spans[i - 1]]).Evaluate(p);
            ScalarLawSample right = new ScalarLawDefinition((ScalarLawKind)spans[i].Data.Kind,
                new(p, Active(spans[i]).Last), [spans[i]]).Evaluate(p);
            joins.Add(new(p, right.Value - left.Value, right.FirstDerivative - left.FirstDerivative, right.SecondDerivative - left.SecondDerivative));
        }
        return joins.AsReadOnly();
    }
    internal (LawSpanRaw[] Spans, double[] Values, int[] Multiplicities) Buffers()
    {
        if (spans.Sum(s => (long)s.Values.Length + s.Parameters.Length) > 65536 || spans.Sum(s => (long)s.Multiplicities.Length) > 65536)
            throw new ArgumentException("The law exceeds the bounded numeric buffer limit.");
        List<double> values = []; List<int> multiplicities = []; List<LawSpanRaw> raw = [];
        foreach (Span span in spans)
        {
            LawSpanRaw data = span.Data; var active = Active(span); data.ActiveFirst = active.First; data.ActiveLast = active.Last;
            data.ValueOffset = values.Count; data.ValueCount = span.Values.Length; values.AddRange(span.Values);
            data.ParameterOffset = values.Count; data.ParameterCount = span.Parameters.Length; values.AddRange(span.Parameters);
            data.MultiplicityOffset = multiplicities.Count; multiplicities.AddRange(span.Multiplicities); raw.Add(data);
        }
        if (values.Count > 65536 || multiplicities.Count > 65536) throw new ArgumentException("The law exceeds the bounded numeric buffer limit.");
        return (raw.ToArray(), values.ToArray(), multiplicities.ToArray());
    }
    internal static T[] Copy<T>(IEnumerable<T> input, int maximum = 65536)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.TryGetNonEnumeratedCount(out int count) && count > maximum) throw new ArgumentException("Input exceeds the bounded limit.");
        List<T> result = [];
        foreach (T value in input) { if (result.Count == maximum) throw new ArgumentException("Input exceeds the bounded limit."); result.Add(value); }
        return result.ToArray();
    }
    private static void Finite(IEnumerable<double> values)
    { if (values.Any(value => !double.IsFinite(value))) throw new ArgumentOutOfRangeException(nameof(values), "Values must be finite."); }
    private static void Increasing(double[] values)
    { Finite(values); for (int i = 1; i < values.Length; i++) if (values[i] <= values[i - 1]) throw new ArgumentException("Parameters must strictly increase."); }
}
#pragma warning restore CS1591
