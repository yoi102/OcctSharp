using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public sealed record MeshWeldOptions(double Tolerance = 1e-7, bool PreserveAttributes = true, bool PreserveMaterials = true,
    bool RemoveCollapsedTriangles = true);

public static partial class MeshEditing
{
    public static unsafe MeshEditResult Transform(AuthoredMesh source, AuthoredMeshTransform transform)
    {
        ArgumentNullException.ThrowIfNull(source); double[] matrix = transform.Values;
        if (matrix.Any(v => !double.IsFinite(v)) || !double.IsFinite(transform.Determinant) || transform.Determinant == 0)
            throw new ArgumentException("Mesh transform must be finite and invertible.", nameof(transform));
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(source), output = new AuthoredVertexRaw[vertices.Length];
        double determinant;
        OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices, o = output)
        fixed (double* m = matrix)
            NativeError.ThrowIfFailed(NativeMethods.MeshTransformVertices(v, vertices.Length, m, matrix.Length, o, output.Length, out determinant), "mesh_transform");
        AuthoredMesh result = Create(source, MeshBuffers.Positions(output),
            source.Triangles.Select(t => determinant < 0 ? t.Reversed() : t), source.Normals is null ? null : MeshBuffers.Normals(output), source.UVs);
        return IdentityResult(source, result);
    }

    public static MeshEditResult TransformRigid(AuthoredMesh source, AuthoredMeshTransform transform)
    {
        double[] a = transform.Values;
        double[] squared = [a[0] * a[0] + a[4] * a[4] + a[8] * a[8], a[1] * a[1] + a[5] * a[5] + a[9] * a[9], a[2] * a[2] + a[6] * a[6] + a[10] * a[10]];
        double tolerance = Math.Max(squared.Max(), 1e-300) * 1e-12;
        if (squared.Any(s => !double.IsFinite(s)) || squared[0] == 0 ||
            Math.Abs(squared[0] - squared[1]) > tolerance || Math.Abs(squared[0] - squared[2]) > tolerance ||
            Math.Abs(a[0] * a[1] + a[4] * a[5] + a[8] * a[9]) > tolerance ||
            Math.Abs(a[0] * a[2] + a[4] * a[6] + a[8] * a[10]) > tolerance ||
            Math.Abs(a[1] * a[2] + a[5] * a[6] + a[9] * a[10]) > tolerance)
            throw new ArgumentException("The rigid/uniform-scale path does not accept shear or nonuniform scaling.");
        return Transform(source, transform);
    }

    public static unsafe MeshEditResult ConvertCoordinates(AuthoredMesh source, MeshCoordinates target)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(target); target.Validate();
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(source), output = new AuthoredVertexRaw[vertices.Length];
        OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices, o = output)
            NativeError.ThrowIfFailed(NativeMethods.MeshConvertCoordinates(v, vertices.Length,
                source.Coordinates.MetresPerUnit, (int)source.Coordinates.UpAxis, (int)source.Coordinates.Handedness,
                target.MetresPerUnit, (int)target.UpAxis, (int)target.Handedness, o, output.Length), "mesh_convert_coordinates");
        bool mirror = source.Coordinates.Handedness != target.Handedness;
        AuthoredMesh result = Create(source, MeshBuffers.Positions(output), source.Triangles.Select(t => mirror ? t.Reversed() : t),
            source.Normals is null ? null : MeshBuffers.Normals(output), source.UVs, coordinates: target);
        return IdentityResult(source, result);
    }

    /// <summary>Welds using OCCT's actual merge indices; geometric-only welding explicitly drops authored UV/normal channels.</summary>
    public static unsafe MeshEditResult Weld(AuthoredMesh source, MeshWeldOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source); MeshWeldOptions policy = options ?? new();
        if (!double.IsFinite(policy.Tolerance) || policy.Tolerance < 0) throw new ArgumentOutOfRangeException(nameof(options));
        int[] partitions = new int[source.Positions.Count];
        Dictionary<int, MeshGroup> groups = source.Groups.ToDictionary(g => g.Key);
        MeshConnectivity graph = MeshConnectivity.Create(source);
        Dictionary<(MeshNormal? Normal, MeshUv? Uv, string Material), int> keys = [];
        for (int i = 0; i < partitions.Length; ++i)
        {
            string materials = policy.PreserveMaterials ? string.Join(";", graph.IncidentTriangles[i]
                .Select(t => groups[source.Triangles[t].Group].MaterialKey).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(key => key is null ? "-1:" : $"{key.Length}:{key}")) : "";
            var key = (policy.PreserveAttributes ? source.Normals?[i] : null, policy.PreserveAttributes ? source.UVs?[i] : null, materials);
            if (!keys.TryGetValue(key, out int partition)) keys.Add(key, partition = keys.Count);
            partitions[i] = partition;
        }
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(source); int[] representatives = new int[vertices.Length];
        OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (int* p = partitions, r = representatives)
            NativeError.ThrowIfFailed(NativeMethods.MeshWeldNodes(v, vertices.Length, p, partitions.Length, policy.Tolerance, r, representatives.Length), "mesh_weld_nodes");
        int[] retained = representatives.Distinct().Order().ToArray();
        Dictionary<int, int> targets = retained.Select((index, target) => (index, target)).ToDictionary(x => x.index, x => x.target);
        int[] oldToNew = representatives.Select(i => targets[i]).ToArray();
        List<MeshTriangle> triangles = []; List<int> triangleSources = [];
        for (int i = 0; i < source.Triangles.Count; ++i)
        {
            MeshTriangle t = source.Triangles[i]; t = new(oldToNew[t.A], oldToNew[t.B], oldToNew[t.C], t.Group);
            if (policy.RemoveCollapsedTriangles && (t.A == t.B || t.B == t.C || t.A == t.C)) continue;
            triangles.Add(t); triangleSources.Add(i);
        }
        List<MeshPolyline> lines = []; List<string> diagnostics = [];
        foreach ((MeshPolyline line, int index) in source.Polylines.Select((line, index) => (line, index)))
        {
            int[] indices = line.Indices.Select(i => oldToNew[i]).ToArray();
            if (indices.Zip(indices.Skip(1)).Any(pair => pair.First == pair.Second) ||
                (line.IsClosed && indices.Distinct().Count() < 3) || (!line.IsClosed && indices[0] == indices[^1]))
                diagnostics.Add($"Polyline {index} omitted because welding collapsed its connectivity.");
            else lines.Add(new(indices, line.IsClosed, line.Parameters));
        }
        if (!policy.PreserveAttributes) diagnostics.Add("Geometric-only welding drops authored normal and UV channels; rebuild them explicitly.");
        AuthoredMesh result = Create(source, retained.Select(i => source.Positions[i]), triangles,
            policy.PreserveAttributes && source.Normals is not null ? retained.Select(i => source.Normals[i]) : null,
            policy.PreserveAttributes && source.UVs is not null ? retained.Select(i => source.UVs[i]) : null,
            polylines: lines, origins: retained.Select(i => source.VertexOrigins[i]));
        MeshIndexMap vertexMap = new(oldToNew.Select(i => new[] { i }), retained.Length);
        return new(result, new(source.Revision, result.Revision, vertexMap,
            MeshIndexMap.FromResultSources(source.Triangles.Count, triangleSources)), Enumerable.Range(0, triangles.Count), diagnostics);
    }

    public static unsafe MeshEditResult RemoveDegenerate(AuthoredMesh source, double minimumArea = 0, double minimumLength = 0)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!double.IsFinite(minimumArea) || minimumArea < 0 || !double.IsFinite(minimumLength) || minimumLength < 0)
            throw new ArgumentException("Degeneration thresholds must be finite and nonnegative.");
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(source); AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(source.Triangles);
        int[] removed = new int[triangles.Length]; int count; OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
        fixed (int* r = removed)
            NativeError.ThrowIfFailed(NativeMethods.MeshRemoveDegenerate(v, vertices.Length, t, triangles.Length,
                minimumArea, minimumLength, r, removed.Length, out count), "mesh_remove_degenerate");
        if ((uint)count > (uint)removed.Length) throw new OcctException("InvalidMeshResult", "Native removed-index count exceeds capacity.");
        return DeleteTriangles(source, source.SelectTriangles(removed.Take(count)));
    }

    /// <summary>Adds a connected patch using existing vertex indices; positions/channels can be authored before insertion.</summary>
    public static unsafe MeshEditResult InsertPatch(AuthoredMesh source, IEnumerable<MeshTriangle> appended,
        MeshTriangleSelection? replacementSelection = null, IEnumerable<MeshTriangle>? replacements = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        MeshTriangleSelection selection = replacementSelection ?? source.SelectTriangles([]); selection.Validate(source);
        MeshTriangle[] added = MeshDataValidation.Copy(appended, nameof(appended));
        MeshTriangle[] changed = replacements is null ? [] : MeshDataValidation.Copy(replacements, nameof(replacements));
        if (changed.Length != selection.Indices.Count) throw new ArgumentException("Patch replacement count differs from selection.");
        if (source.Triangles.Count + (long)added.Length > MeshDataValidation.MaximumElements) throw new ArgumentException("Patch exceeds element limit.");
        MeshTriangle[] final = [.. source.Triangles, .. added];
        for (int i = 0; i < changed.Length; ++i) final[selection.Indices[i]] = changed[i];
        AuthoredMesh proposed = Create(source, source.Positions, final, null, source.UVs);
        MeshConnectivity graph = MeshConnectivity.Create(proposed);
        foreach (MeshTriangleSelection component in graph.Components())
            if (component.Indices.Any(i => i >= source.Triangles.Count) && source.Triangles.Count > 0 && !component.Indices.Any(i => i < source.Triangles.Count))
                throw new ArgumentException("Inserted patches must be edge-connected to the existing mesh.");
        if (source.Triangles.Count == 0 && graph.Components().Count > 1) throw new ArgumentException("The initial patch must be connected.");
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(source); AuthoredTriangleRaw[] baseline = MeshBuffers.Triangles(source.Triangles);
        AuthoredTriangleRaw[] patch = MeshBuffers.Triangles(added), replacement = MeshBuffers.Triangles(changed), output = new AuthoredTriangleRaw[final.Length];
        int[] indices = [.. selection.Indices]; OcctRuntime.EnsureCompatible();
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* b = baseline, p = patch, r = replacement, o = output)
        fixed (int* i = indices)
            NativeError.ThrowIfFailed(NativeMethods.MeshCoherentPatch(v, vertices.Length, b, baseline.Length, i, r, indices.Length,
                p, patch.Length, o, output.Length), "mesh_coherent_patch");
        AuthoredMesh result = Create(source, source.Positions, MeshBuffers.Triangles(output), null, source.UVs);
        return new(result, new(source.Revision, result.Revision, MeshIndexMap.Identity(source.Positions.Count),
            new MeshIndexMap(Enumerable.Range(0, source.Triangles.Count).Select(i => new[] { i }), result.Triangles.Count)),
            selection.Indices.Concat(Enumerable.Range(source.Triangles.Count, added.Length)), ["Derived normals invalidated after coherent patch editing."]);
    }
}
