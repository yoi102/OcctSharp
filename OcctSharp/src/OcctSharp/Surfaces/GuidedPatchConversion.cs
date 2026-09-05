using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum BoundaryPatchStyle { Stretch, Coons, Curved }
public sealed record CurveSpanProvenance(int SourceIndex, ParameterRange SourceParameters, ParameterRange ResultParameters, bool Reversed);
public sealed record CopiedBezierSpan(int Index, ParameterRange SourceParameters, bool Reversed, FreeformCurveDefinition Definition);
public sealed record CopiedBezierPatch(int UIndex, int VIndex, SurfaceParameterBounds SourceParameters, bool Reversed, FreeformSurfaceDefinition Definition);
public sealed record CopiedCurveAssembly(FreeformCurveDefinition Definition, IReadOnlyList<CurveSpanProvenance> Spans);
public sealed record JoinResidual(double? Position, double? AngleRadians, double? Curvature);

/// <summary>Copied parameter-preserving B-spline pieces, boundary patches and explicit continuity witnesses.</summary>
public static class GuidedPatchConversion
{
    public static FreeformSurfaceDefinition CreateBoundaryPatch(IEnumerable<Shape> boundaries, BoundaryPatchStyle style = BoundaryPatchStyle.Coons,
        bool bezier = false, double tolerance = 1e-7)
    {
        if (!Enum.IsDefined(style)) throw new ArgumentOutOfRangeException(nameof(style));
        Shape[] inputs = ScalarLawDefinition.Copy(boundaries, 4); if (inputs.Length < 2) throw new ArgumentException("Two to four curves are required.");
        var converted = Execute(inputs, new() { Operation = 0, Style = (int)style, Bezier = bezier ? 1 : 0, Tolerance = tolerance });
        using var result = converted.Result;
        return FreeformAuthoring.GetSurfaceDefinition(result.RequireShape());
    }
    /// <summary>Result intervals preserve span provenance, not a promise of identical analytic parameterization.</summary>
    public static CopiedCurveAssembly AssembleCurves(IEnumerable<Shape> spans, double tolerance = 1e-7,
        bool useTangentSpeedRatio = true, int minimumMultiplicity = 0)
    {
        Shape[] inputs = ScalarLawDefinition.Copy(spans, 128);
        if (inputs.Length == 0 || minimumMultiplicity is < 0 or > 25) throw new ArgumentOutOfRangeException(nameof(spans));
        var converted = Execute(inputs, new() { Operation = 1, WithRatio = useTangentSpeedRatio ? 1 : 0, MinimumMultiplicity = minimumMultiplicity, Tolerance = tolerance });
        using var result = converted.Result;
        return new(FreeformAuthoring.GetCurveDefinition(result.RequireShape()), Array.AsReadOnly(converted.Spans.Select(s =>
            new CurveSpanProvenance(s.SourceIndex, new(s.First, s.Last), new(s.ResultFirst, s.ResultLast), s.Orientation == 1)).ToArray()));
    }
    public static IReadOnlyList<CopiedBezierSpan> DecomposeCurve(Shape bspline)
    {
        var converted = Execute([bspline], new() { Operation = 2, Tolerance = 1e-7 }); using var result = converted.Result;
        return Array.AsReadOnly(result.History.Select((h, i) => new CopiedBezierSpan(i,
            new(converted.Spans[i].First, converted.Spans[i].Last), converted.Spans[i].Orientation == 1,
            FreeformAuthoring.GetCurveDefinition(h.Shape!))).ToArray());
    }
    public static IReadOnlyList<CopiedBezierPatch> DecomposeSurface(Shape bspline)
    {
        var converted = Execute([bspline], new() { Operation = 3, Tolerance = 1e-7 }); using var result = converted.Result;
        return Array.AsReadOnly(result.History.Select((h, i) =>
        {
            var s = converted.Spans[i]; return new CopiedBezierPatch(s.UIndex, s.VIndex,
                new(s.First, s.Last, s.FirstV, s.LastV), s.Orientation == 1, FreeformAuthoring.GetSurfaceDefinition(h.Shape!));
        }).ToArray());
    }
    public static FreeformCurveDefinition ExtractCurveSpan(Shape bspline, ParameterRange parameters)
    {
        parameters.Validate(nameof(parameters)); var converted = Execute([bspline], new() { Operation = 4,
            First = parameters.First, Last = parameters.Last, Tolerance = 1e-7 }); using var result = converted.Result;
        return FreeformAuthoring.GetCurveDefinition(result.RequireShape());
    }
    public static FreeformSurfaceDefinition ExtractSurfacePatch(Shape bspline, SurfaceParameterBounds parameters)
    {
        parameters.Validate(nameof(parameters)); var converted = Execute([bspline], new() { Operation = 5, FirstU = parameters.FirstU, LastU = parameters.LastU,
            FirstV = parameters.FirstV, LastV = parameters.LastV, Tolerance = 1e-7 }); using var result = converted.Result;
        return FreeformAuthoring.GetSurfaceDefinition(result.RequireShape());
    }
    /// <summary>Samples the first face's boundary against the second surface; missing derivatives remain null.</summary>
    public static unsafe IReadOnlyList<JoinResidual> CompareSurfaceBoundary(Shape boundary, Shape first, Shape second, int count = 33, double tolerance = 1e-7)
    {
        ArgumentNullException.ThrowIfNull(boundary); ArgumentNullException.ThrowIfNull(first); ArgumentNullException.ThrowIfNull(second);
        boundary.ThrowIfDisposed(); first.ThrowIfDisposed(); second.ThrowIfDisposed();
        if (count is < 2 or > 4096) throw new ArgumentOutOfRangeException(nameof(count)); AuthoringBridge.Positive(tolerance, nameof(tolerance));
        ConstraintResidualRaw[] raw = new ConstraintResidualRaw[count]; fixed (ConstraintResidualRaw* p = raw)
            NativeError.ThrowIfFailed(NativeMethods.SurfaceJoin(boundary.Handle, first.Handle, second.Handle, count, tolerance, p, count), "authoring_surface_join");
        return Array.AsReadOnly(raw.Select(Convert).ToArray());
    }
    public static JoinResidual CompareCurveJoin(Shape first, double firstParameter, Shape second, double secondParameter, bool reverseSecondTangent = false)
    {
        ArgumentNullException.ThrowIfNull(first); ArgumentNullException.ThrowIfNull(second); first.ThrowIfDisposed(); second.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CurveJoin(first.Handle, second.Handle, firstParameter, secondParameter,
            reverseSecondTangent ? 1 : 0, out var residual), "authoring_curve_join"); return Convert(residual);
    }
    private static JoinResidual Convert(ConstraintResidualRaw raw) => new((raw.Defined & 1) != 0 ? raw.Position : null,
        (raw.Defined & 2) != 0 ? raw.Angle : null, (raw.Defined & 4) != 0 ? raw.Curvature : null);
    private static unsafe (AuthoringResult Result, PatchSpanRaw[] Spans) Execute(Shape[] inputs, PatchOptionsRaw options)
    {
        AuthoringBridge.Positive(options.Tolerance, nameof(options));
        // A bounded buffer avoids a second expensive native conversion and owns no native memory.
        return AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            PatchSpanRaw[] spans = new PatchSpanRaw[4096]; fixed (PatchSpanRaw* s = spans)
            {
                NativeError.ThrowIfFailed(NativeMethods.PatchConvert(p, count, in options, s, spans.Length,
                    out int written, out var info, out nint result), "patch_convert");
                return (AuthoringBridge.Read(Guid.NewGuid(), result, info), spans[..written]);
            }
        });
    }
}
#pragma warning restore CS1591
