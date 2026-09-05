using OcctSharp.Interop;

namespace OcctSharp;

public sealed partial class ParametricDocument
{
    private Candidate Evaluate(StoredFeature feature, Dictionary<Guid, StoredFeature> features,
        Dictionary<Guid, Candidate> candidates, ParametricExecutionPlan plan)
    {
        var definition = feature.Definition;
        Dictionary<string, Shape> inputs = new(StringComparer.Ordinal);
        try
        {
            foreach (var input in definition.Inputs.Where(x => x.Kind != ParametricOutputKind.Scalar))
            {
                Shape shape = candidates.TryGetValue(input.FeatureId, out var candidate)
                    ? Share(candidate.Shape ?? throw new InvalidOperationException("A shape input is absent."))
                    : RequiredShape(features[input.FeatureId].ResultEntry);
                inputs.Add(input.Name, shape);
            }
            Shape Input(string name) => inputs.TryGetValue(name, out var value) ? value : throw new ArgumentException($"Missing shape input '{name}'.");
            double Value(string name, ParametricDimension dimension, double? fallback = null)
            {
                if (!plan.Quantities.TryGetValue(new(definition.Id, name), out var value))
                {
                    if (fallback is { } defaultValue && !definition.Parameters.ContainsKey(name)) return defaultValue;
                    throw new ArgumentException($"Missing scalar parameter '{name}'.");
                }
                if (value.Dimension != dimension) throw new ArgumentException($"Parameter '{name}' has incompatible units.");
                return value.Value;
            }
            double Length(string name, double? fallback = null) => Value(name, ParametricDimension.Distance, fallback);
            double Scalar(string name, double fallback) => Value(name, ParametricDimension.Scalar, fallback);
            double Angle(string name, double? fallback = null) => Value(name, ParametricDimension.Rotation, fallback);
            switch (definition.Kind)
            {
                case ParametricFeatureKind.SourceShape:
                    return new(RequiredShape(feature.SourceEntry));
                case ParametricFeatureKind.Box:
                    return new(ShapeFactory.CreateBox(Length("x"), Length("y"), Length("z")));
                case ParametricFeatureKind.Cylinder:
                    return new(ShapeFactory.CreateCylinder(Length("radius"), Length("height")));
                case ParametricFeatureKind.Placement:
                    return new(Input("source").Transformed(new ShapeTransform(Length("x", 0), Length("y", 0), Length("z", 0),
                        Scalar("axisX", 0), Scalar("axisY", 0), Scalar("axisZ", 1), Angle("angle", 0))));
                case ParametricFeatureKind.Extrusion:
                    using (var vector = GpVec.Create(Length("x", 0), Length("y", 0), Length("z")))
                        return new(Input("profile").Extrude(vector));
                case ParametricFeatureKind.Revolution:
                    using (var axis = GpAx1.Create(Length("x", 0), Length("y", 0), Length("z", 0),
                        Scalar("axisX", 0), Scalar("axisY", 0), Scalar("axisZ", 1)))
                        return new(Input("profile").Revolve(axis, Angle("angle")));
                case ParametricFeatureKind.Scalar:
                    return plan.Quantities.TryGetValue(new(definition.Id, "value"), out var quantity)
                        ? new(null, quantity) : throw new ArgumentException("A scalar feature requires a resolved 'value' parameter.");
                case ParametricFeatureKind.Boolean:
                    var boolean = Recipe<ParametricBooleanRecipe>(definition);
                    if (boolean.Options is { NonDestructive: false } or { RepairInputs: true } or { UnifyResult: true })
                        throw new NotSupportedException("Parametric Boolean execution requires non-destructive inputs and unaltered algorithm history. Use an explicit Q repair feature for healing/unification.");
                    Shape[] ordered = definition.Inputs.Select(x => Input(x.Name)).ToArray();
                    if (boolean.ArgumentCount < 1 || boolean.ArgumentCount > ordered.Length) throw new ArgumentException("Invalid Boolean argument partition.");
                    using (var result = FeatureModeling.Boolean(boolean.Operation, ordered[..boolean.ArgumentCount], ordered[boolean.ArgumentCount..], boolean.Options))
                    {
                        if (!result.Diagnostics.ResultIsValid) throw new InvalidOperationException(result.Diagnostics.StageMessage);
                        Candidate output = new(Share(result.RequireShape()));
                        try
                        {
                            output.Diagnostics.Add(result.Diagnostics.StageMessage);
                            // J reports an input-owner association, not the particular source
                            // subshape. Keep that distinction: do not fabricate TNaming.Modify(root, face).
                            foreach (var item in result.History)
                                output.AlgorithmHistory.Add(new(definition.Inputs[item.SourceIndex].FeatureId, item.Kind.ToString(), Share(item.Shape)));
                            foreach (int index in result.DeletedSourceIndices)
                                output.AlgorithmHistory.Add(new(definition.Inputs[index].FeatureId, "Deleted", null));
                            return output;
                        }
                        catch { output.Dispose(); throw; }
                    }
                case ParametricFeatureKind.Repair:
                    var repair = Recipe<ParametricRepairRecipe>(definition);
                    if (repair.Stages is null || repair.Stages.Any(x => x is null or TopologyEditRepair))
                        throw new ArgumentException("Persisted repair stages must be whole-source operations, not snapshot-bound topology edits.");
                    using (var source = RepairSnapshot.Create(Input("source")))
                    using (var preview = ShapeRepair.Preview(source, new(source,
                        repair.Stages.Select((x, i) => new RepairStep($"stage-{i}", x)).ToArray(), tolerance: repair.Tolerance, budget: repair.Budget)))
                    {
                        Candidate output = new(preview.Accept());
                        output.Diagnostics.AddRange(preview.Stages.Select(x => x.Message));
                        // Acceptance deep-copies topology: preserve copied Q history as diagnostics, not a fabricated selector identity map.
                        output.Diagnostics.Add(System.Text.Json.JsonSerializer.Serialize(preview.History));
                        return output;
                    }
                case ParametricFeatureKind.GuidedSweep:
                    var sweep = Recipe<ParametricSweepRecipe>(definition);
                    if (sweep.Sections is null) throw new ArgumentException("Sweep sections are absent.");
                    using (var sweepPlan = GuidedSweepPlan.Create(Input(sweep.Spine), sweep.Sections.Select(x =>
                        new GuidedSweepSection(Input(x.Profile), x.SpineVertex is null ? null : Input(x.SpineVertex), x.WithContact, x.WithCorrection)),
                        sweep.Options, sweep.GuideOrSupport is null ? null : Input(sweep.GuideOrSupport), sweep.ScaleLaw?.Build()))
                    using (var result = sweepPlan.Build())
                    {
                        var output = new Candidate(Share(result.RequireShape()));
                        try
                        {
                            var sources = new Dictionary<int, Guid> { [0] = definition.Inputs.Single(x => x.Name == sweep.Spine).FeatureId };
                            if (sweepPlan.GuideOrSupportInputIndex is { } guide)
                                sources[guide] = definition.Inputs.Single(x => x.Name == sweep.GuideOrSupport).FeatureId;
                            for (int i = 0; i < sweep.Sections.Count; i++)
                            {
                                var section = sweepPlan.Sections[i];
                                sources[section.ProfileInputIndex] = definition.Inputs.Single(x => x.Name == sweep.Sections[i].Profile).FeatureId;
                                if (section.SpineVertexInputIndex is { } vertex) sources[vertex] = definition.Inputs.Single(x => x.Name == sweep.Sections[i].SpineVertex).FeatureId;
                            }
                            foreach (var item in result.History)
                                output.AlgorithmHistory.Add(new(item.Source is { } origin ? sources[origin.ArgumentIndex] : Guid.Empty,
                                    item.Kind.ToString(), item.Shape is null ? null : Share(item.Shape)));
                            output.Diagnostics.Add(result.Diagnostics.Message);
                            output.Diagnostics.Add("Sweep history uses the plan's private input graph; result replacement does not promise arbitrary persistent subshape identity.");
                            return output;
                        }
                        catch { output.Dispose(); throw; }
                    }
                case ParametricFeatureKind.ConstrainedFill:
                    var fill = Recipe<ParametricFillRecipe>(definition);
                    if (fill.Constraints is null) throw new ArgumentException("Fill constraints are absent.");
                    SurfaceConstraint Constraint(ParametricFillConstraint c) => c.Kind switch
                    {
                        SurfaceConstraintKind.Edge => new SurfaceEdgeConstraint(c.Id, Input(c.Edge ?? throw new ArgumentException("Missing constraint edge.")),
                            c.Continuity, c.Boundary, c.Support is null ? null : Input(c.Support), c.Required),
                        SurfaceConstraintKind.SurfaceUvPoint => new SurfaceUvConstraint(c.Id, Input(c.Support ?? throw new ArgumentException("Missing constraint support.")), c.U, c.V, c.Continuity, c.Required),
                        SurfaceConstraintKind.Point => new SurfacePointConstraint(c.Id, c.Point, c.Required),
                        _ => throw new ArgumentException("Unknown constraint kind.")
                    };
                    using (var fillPlan = ConstrainedFillPlan.Create(fill.Constraints.Select(Constraint), fill.Options,
                        fill.InitialSurface is null ? null : Input(fill.InitialSurface)))
                    using (var result = fillPlan.Build())
                    {
                        var output = new Candidate(Share(result.RequireFace()));
                        output.Diagnostics.Add(System.Text.Json.JsonSerializer.Serialize(result.Constraints));
                        return output;
                    }
                case ParametricFeatureKind.Mesh:
                    var meshRecipe = Recipe<ParametricMeshRecipe>(definition);
                    // Meshing can mutate triangulations: use a private deep copy of accepted exact input.
                    using (var source = MeshTopology.CopyWithTriangulation(Input("source")))
                    {
                        AuthoredMesh mesh;
                        if (definition.Inputs.Single(x => x.Name == "source").Kind == ParametricOutputKind.ExactShape)
                        {
                            var snapshot = AdvancedMesh.Create(source, meshRecipe.Meshing);
                            mesh = new(snapshot.Vertices.Select(v => new GpPoint(v.X, v.Y, v.Z)),
                                snapshot.Triangles.Select(t => new MeshTriangle(t.VertexA, t.VertexB, t.VertexC, t.FaceIndex)),
                                snapshot.Vertices.Select(v => v.NormalX == 0 && v.NormalY == 0 && v.NormalZ == 0
                                    ? MeshNormal.Undefined : new(v.NormalX, v.NormalY, v.NormalZ)),
                                snapshot.Vertices.All(v => v.HasUv) ? snapshot.Vertices.Select(v => new MeshUv(v.U, v.V)) : null,
                                snapshot.Groups.Select(g => new MeshGroup(g.FaceIndex, $"Face {g.FaceIndex}")));
                        }
                        else mesh = MeshTopology.SnapshotExisting(source).Mesh;
                        if (meshRecipe.RemoveDuplicateTriangles) mesh = MeshEditing.RemoveDuplicates(mesh).Mesh;
                        if (meshRecipe.Compact) mesh = MeshEditing.Compact(mesh).Mesh;
                        if (meshRecipe.RecomputeNormals) mesh = MeshEditing.RebuildNormals(mesh).Mesh;
                        return new(MeshTopology.CreateFace(mesh));
                    }
                case ParametricFeatureKind.ContourFillet:
                case ParametricFeatureKind.ContourChamfer:
                case ParametricFeatureKind.FaceDraft:
                case ParametricFeatureKind.LimitedFeature:
                    return EvaluateLocalFeature(definition, Input, name => Length(name));
                default: throw new NotSupportedException("No built-in evaluator exists for this feature.");
            }
        }
        finally { foreach (var input in inputs.Values) input.Dispose(); }
    }

    private Shape RequiredShape(string entry) => DocumentStateApi.GetNamedShape(handle, entry)
        ?? throw new InvalidOperationException("A required stored result shape is absent.");
    private static Shape Share(Shape source)
    {
        using var identity = TopLocLocation.Identity;
        return source.Moved(identity);
    }
}
