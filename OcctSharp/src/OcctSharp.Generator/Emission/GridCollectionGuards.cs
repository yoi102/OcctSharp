using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using Contract = OcctSharp.Generator.Transformation.GridCollectionOperationContract;

namespace OcctSharp.Generator.Emission;

/// <summary>Release-safe bounds and parallel-data preconditions from the pinned kernels.</summary>
internal static class GridCollectionGuards
{
    internal static IEnumerable<string> Emit(BindingDeclaration d)
    {
        string owner = OwnedValueOperationContract.GetOwnerName(d), member = d.NativeName[(owner.Length + 2)..];
        string Require(string expression, string message) => "Require(" + expression + ", \"" + message + "\");";
        var collections = d.Parameters.Where(p => Contract.Find(p.Type) != null).ToArray();
        string guard = Contract.Operations[d.StableId].Guard;
        switch (guard)
        {
            case "None": break;
            case "Done": yield return Require("self->IsDone()", "Algorithm has no result; check IsDone."); break;
            case "SurfaceCreate":
                yield return Require("local0.NbRows() >= 2 && local0.NbColumns() >= 2", "Surface requires at least two poles in each direction.");
                yield return "GridBounds(1, local0.NbRows(), 1, local0.NbColumns());";
                bool rational = collections.Any(p => p.Position == 1 && Contract.Find(p.Type) is { Grid: true });
                if (rational) { yield return "SameGrid(local0, local1); PositiveWeights(local1);"; }
                if (owner == "Geom_BezierSurface")
                    yield return Require("local0.NbRows() <= 26 && local0.NbColumns() <= 26", "Bezier surface degree exceeds maximum.");
                else
                {
                    int k = rational ? 2 : 1;
                    yield return "SplineDirection(local" + k + ", local" + (k+2) + ", local" + (k+4) + ", local" + (k+6) + ", local0.NbRows());";
                    yield return "SplineDirection(local" + (k+1) + ", local" + (k+3) + ", local" + (k+5) + ", local" + (k+7) + ", local0.NbColumns());";
                }
                break;
            case "SurfaceOutput":
                if (Contract.Find(d.Parameters[0].Type) is { Grid: true })
                    yield return Require("local0.NbRows() == self->NbUPoles() && local0.NbColumns() == self->NbVPoles()", "Output grid dimensions do not match surface.");
                else
                {
                    string dim = member.StartsWith('U') ? "U" : "V";
                    string length = member.Contains("Sequence", StringComparison.Ordinal) ? "self->" + dim + "KnotSequence().Length()" : "self->Nb" + dim + "Knots()";
                    yield return Require("local0.Length() == " + length, "Output length does not match surface.");
                }
                break;
            case "SurfaceEdit":
                if (member.Contains("Row", StringComparison.Ordinal) || member.Contains("Col", StringComparison.Ordinal))
                {
                    bool row = member.Contains("Row", StringComparison.Ordinal), insert = member.StartsWith("Insert", StringComparison.Ordinal);
                    string indexCount = row ? "self->NbUPoles()" : "self->NbVPoles()";
                    string dataCount = row ? "self->NbVPoles()" : "self->NbUPoles()";
                    bool before = member.EndsWith("Before", StringComparison.Ordinal);
                    yield return Require("local0 >= " + (before ? "2" : "1") + " && local0 <= " + indexCount + (before ? " + 1" : ""), "Surface row/column index is outside the pinned insertion bounds.");
                    if (insert)
                    {
                        yield return Require("local1.Lower() == 1 && local1.Length() == " + dataCount, "Inserted row/column must cover the whole opposite dimension.");
                        yield return Require((row ? "self->UDegree()" : "self->VDegree()") + " < 25", "Insertion exceeds Bezier maximum degree.");
                    }
                    else yield return Require("local1.Lower() >= 1 && local1.Upper() <= " + dataCount, "Edited array indices are outside surface bounds.");
                    if (collections.Length == 2)
                    {
                        if (owner == "Geom_BezierSurface" && !row && !insert)
                            yield return Require("local1.Lower() == 1 && local1.Length() == " + dataCount, "Bezier weighted pole-column edits require the complete column.");
                        yield return "SameBounds(local1, local2); PositiveWeights(local2);";
                    }
                    else if (member.StartsWith("SetWeight", StringComparison.Ordinal))
                    {
                        if (owner == "Geom_BezierSurface" && !row)
                            yield return Require("local1.Lower() == 1 && local1.Length() == " + dataCount, "Bezier weight-column edits require the complete column.");
                        yield return "PositiveWeights(local1);";
                    }
                }
                else if (member.StartsWith("Set", StringComparison.Ordinal))
                {
                    string dim = member.Contains('U') ? "U" : "V";
                    yield return Require("local0.Lower() == 1 && local0.Length() == self->Nb" + dim + "Knots()", "Knot count or lower bound does not match surface.");
                    yield return "Ascending(local0, true);";
                }
                else
                {
                    string dim = member.Contains('U') ? "U" : "V";
                    yield return Require("local0.Lower() == 1 && local1.Lower() == 1", "Knot insertion requires one-based arrays.");
                    yield return "SameBounds(local0, local1); Ascending(local0, true);";
                    yield return Require("local2 >= 0", "Knot tolerance must be nonnegative.");
                    yield return "for (const int m : local1) Require(m >= 0 && m <= self->" + dim + "Degree(), \"Invalid knot insertion multiplicity.\");";
                    yield return "int64_t growth = 0; for (const int m : local1) growth += m;";
                    yield return Require("growth + self->Nb" + dim + "Poles() < std::numeric_limits<int>::max()", "Knot insertion pole-count upper bound overflows.");
                    yield return "GridBounds(1, " + (dim == "U" ? "static_cast<int>(growth + self->NbUPoles())" : "self->NbUPoles()") + ", 1, "
                        + (dim == "V" ? "static_cast<int>(growth + self->NbVPoles())" : "self->NbVPoles()") + ");";
                }
                break;
            case "SurfaceFit":
                yield return Require("local0.LowerRow() == 1 && local0.LowerCol() == 1", "Surface fitting requires one-based grid indices.");
                yield return Require("local0.NbRows() >= 2 && local0.NbColumns() >= 2", "Surface fit requires at least two points in each direction.");
                foreach (var p in d.Parameters)
                {
                    string v = "local" + p.Position;
                    if (p.Name is "DegMin" or "DegMax") yield return Require(v + " >= 1 && " + v + " <= 25", "Invalid approximation degree.");
                    if (p.Name == "DegMax" && d.Parameters.FirstOrDefault(p => p.Name == "DegMin") is { } min)
                        yield return Require(v + " >= local" + min.Position, "Maximum degree must be at least minimum degree.");
                    if (p.Name.StartsWith("Weight", StringComparison.Ordinal)) yield return Require(v + " >= 0", "Smoothing weights must be nonnegative.");
                    if (p.Name is "Tol3D" or "Tol3d") yield return Require(v + " > 0", "Fitting tolerance must be positive.");
                    if (p.Name is "dX" or "dY" or "DX" or "DY") yield return Require(v + " != 0", "Height-field step must be nonzero.");
                    if (p.Name is "ParType" or "ParT") yield return Require("static_cast<int>(" + v + ") >= 0 && static_cast<int>(" + v + ") <= 2", "Invalid parameterization.");
                    if (p.Name is "Continuity" or "CC") yield return Require(v + " == GeomAbs_C0 || " + v + " == GeomAbs_C1 || " + v + " == GeomAbs_C2 || " + v + " == GeomAbs_C3 || " + v + " == GeomAbs_CN", "Invalid fitting continuity.");
                }
                break;
            case "InterpolateCreate":
                int periodic = d.Parameters.Count - 2, tol = d.Parameters.Count - 1;
                yield return Require("local0.Lower() == 1 && local0.Length() >= 2 && local0.Length() <= (std::numeric_limits<int>::max() - 16) / 2", "Interpolation requires one-based points and count space for tangent-expanded poles and knots.");
                yield return Require("local" + tol + " > 0", "Interpolation tolerance must be positive.");
                if (collections.Length == 2)
                {
                    yield return Require("local1.Lower() == 1 && local1.Length() == local0.Length() + (local" + periodic + " ? 1 : 0)", "Parameter count must match points plus periodic junction.");
                    yield return "Ascending(local1, true);";
                }
                yield return "for (int i = 2; i <= local0.Length(); ++i) Require(local0.Value(i).Distance(local0.Value(i-1)) > local" + tol + ", \"Consecutive interpolation points are too close.\");";
                yield return Require("!local" + periodic + " || local0.First().Distance(local0.Last()) > local" + tol, "Periodic points must omit the repeated junction.");
                break;
            case "TangentLoad":
                if (collections.Length > 0)
                {
                    yield return Require("local0.Lower() == 1 && local1.Lower() == 1 && local0.Length() == self->PointCount && local1.Length() == self->PointCount", "Tangents and flags must match the interpolation points.");
                    yield return "for (int i = 1; i <= self->PointCount; ++i) Require(!local1.Value(i) || local0.Value(i).Magnitude() > self->Tolerance, \"Active tangent is too small.\");";
                }
                else yield return Require("local0.Magnitude() > self->Tolerance && local1.Magnitude() > self->Tolerance", "Endpoint tangents are too small.");
                break;
            default: throw new InvalidDataException("Unknown grid guard " + guard);
        }
    }
}
