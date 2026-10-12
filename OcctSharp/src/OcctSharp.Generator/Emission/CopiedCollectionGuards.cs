using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using Contract = OcctSharp.Generator.Transformation.CopiedCollectionOperationContract;

namespace OcctSharp.Generator.Emission;

/// <summary>Pinned-kernel preconditions, including checks compiled out of its Release libraries.</summary>
internal static class CopiedCollectionGuards
{
    internal static IEnumerable<string> Emit(BindingDeclaration d)
    {
        string guard = Contract.Operations[d.StableId].Guard;
        string owner = OwnedValueOperationContract.GetOwnerName(d);
        string member = d.NativeName[(owner.Length + 2)..];
        var arrays = d.Parameters.Where(p => Contract.Find(p.Type) is { Sequence: false }).ToArray();
        string Require(string expression, string message) => "Require(" + expression + ", \"" + message + "\");";
        if (guard is "BezierCreate" or "SplineCreate" or "Fit" or "FuseIntervals" or "SetKnots" or "InsertKnots" or "CurveOutputLength")
            foreach (var p in arrays) yield return Require("local" + p.Position + ".Lower() == 1", "This algorithm requires one-based input arrays.");
        switch (guard)
        {
            case "None": break;
            case "Done": yield return Require("self->IsDone()", "Fitting algorithm has no result; check IsDone."); break;
            case "BezierCreate":
                yield return Require("local0.Length() >= 2 && local0.Length() <= " + owner + "::MaxDegree() + 1", "Invalid Bezier pole count.");
                if (arrays.Length == 2)
                {
                    yield return "SameBounds(local0, local1);";
                    yield return "for (int i = 0; i < local1.Length(); ++i) Require(local1.Value(i+1) > gp::Resolution(), \"Weights must be positive.\");";
                }
                break;
            case "SplineCreate":
                bool rational = arrays.Length == 4;
                int knot = rational ? 2 : 1, mult = knot + 1, degree = mult + 1, periodic = degree + 1;
                yield return "Minimum(local0, 2); Minimum(local" + knot + ", 2); SameBounds(local" + knot + ", local" + mult + "); Ascending(local" + knot + ", true);";
                yield return Require("local" + degree + " >= 1 && local" + degree + " <= " + owner + "::MaxDegree()", "Invalid spline degree.");
                yield return "int64_t sum = 0; for (int i = 0; i < local" + mult + ".Length(); ++i) { const int m = local" + mult + ".Value(i+1); Require(m > 0 && m <= local" + degree + " + 1, \"Invalid knot multiplicity.\"); sum += m; }";
                yield return Require("sum <= std::numeric_limits<int>::max() - local" + degree + " - 1", "Knot multiplicity sum overflows.");
                yield return Require("BSplCLib::NbPoles(local" + degree + ", local" + periodic + ", local" + mult + ") == local0.Length()", "Spline pole count does not match multiplicities.");
                if (rational)
                {
                    yield return "SameBounds(local0, local1);";
                    yield return "for (int i = 0; i < local1.Length(); ++i) Require(local1.Value(i+1) > gp::Resolution(), \"Weights must be positive.\");";
                }
                break;
            case "CurveOutputLength":
                string length = member switch { "Poles" => "self->NbPoles()", "Multiplicities" or "Knots" => "self->NbKnots()", "KnotSequence" => "self->KnotSequence().Length()", _ => throw new InvalidDataException("Unknown copied curve output.") };
                yield return Require("local0.Length() == " + length, "Curve output array length does not match."); break;
            case "SetKnots":
                yield return Require("local0.Length() == self->NbKnots()", "Knot count does not match curve."); yield return "Ascending(local0, true);"; break;
            case "InsertKnots":
                yield return "SameBounds(local0, local1); Ascending(local0, true);";
                yield return Require("local2 >= 0", "Knot tolerance must be nonnegative.");
                yield return "for (int i = 0; i < local1.Length(); ++i) Require(local1.Value(i+1) >= 0 && local1.Value(i+1) <= self->Degree(), \"Invalid insertion multiplicity.\");"; break;
            case "Fit":
                yield return "Minimum(local0, 2);";
                if (arrays.Length == 2) { yield return "SameBounds(local0, local1); Ascending(local1, true);"; }
                foreach (var p in d.Parameters)
                {
                    string local = "local" + p.Position;
                    if (p.Name is "DegMin" or "DegMax") yield return Require(local + " >= 1 && " + local + " <= 25", "Invalid approximation degree.");
                    if (p.Name == "DegMax" && d.Parameters.FirstOrDefault(p => p.Name == "DegMin") is { } min)
                        yield return Require(local + " >= local" + min.Position, "DegMax must be at least DegMin.");
                    if (p.Name.StartsWith("Weight", StringComparison.Ordinal)) yield return Require(local + " >= 0", "Smoothing weights must be nonnegative.");
                    if (p.Name is "Tol3D" or "Tol2D") yield return Require(local + " > 0", "Fitting tolerance must be positive.");
                    if (p.Name == "DX") yield return Require(local + " != 0", "Sampling step must be nonzero.");
                    if (p.Name == "ParType") yield return Require("static_cast<int>(" + local + ") >= 0 && static_cast<int>(" + local + ") <= 2", "Invalid parameterization.");
                    if (p.Name == "Continuity") yield return Require(local + " == GeomAbs_C0 || " + local + " == GeomAbs_C1 || " + local + " == GeomAbs_C2 || " + local + " == GeomAbs_C3 || " + local + " == GeomAbs_CN", "Invalid fitting continuity.");
                }
                break;
            case "Reparametrize":
                yield return "Minimum(local2, 2); Ascending(local2, true);";
                yield return Require("local0 < local1", "New parameter interval must increase."); break;
            case "Reverse":
                if (d.Parameters.Count == 2)
                    yield return Require("local1 >= local0.Lower() && local1 <= local0.Upper()", "Reverse pivot is outside array bounds.");
                else if (Contract.Find(d.Parameters[0].Type)?.Element == "double") yield return "Ascending(local0, false);";
                break;
            case "MaxKnotMult":
                yield return Require("local1 >= local0.Lower() && local2 <= local0.Upper() && local1 <= local2", "Multiplicity range is outside array bounds."); break;
            case "FuseIntervals":
                yield return "Minimum(local0, 2); Minimum(local1, 2); Ascending(local0, true); Ascending(local1, true);";
                yield return Require("local3 >= 0", "Interval confusion must be nonnegative."); break;
            default: throw new InvalidDataException("Unknown copied collection guard " + guard);
        }
    }
}
