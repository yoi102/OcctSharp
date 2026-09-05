namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Rigid instance placement expressed in canonical millimetres and radians.</summary>
public sealed record MeshPlacement(string Name, double X = 0, double Y = 0, double Z = 0,
    double AxisX = 0, double AxisY = 0, double AxisZ = 1, double Angle = 0)
{
    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        double[] values = [X, Y, Z, AxisX, AxisY, AxisZ, Angle];
        if (values.Any(v => !double.IsFinite(v)) || (AxisX == 0 && AxisY == 0 && AxisZ == 0))
            throw new ArgumentException("Mesh occurrence placements require finite coordinates and a nonzero rotation axis.");
    }
}
public sealed record AuthoredMeshSceneGroup(MeshGroup Group, XdeVisualMaterial? Material, string DefinitionEntry);
public sealed record AuthoredMeshSceneInstance(string OccurrenceEntry, MeshPlacement Placement, MeshTransform Transform);
/// <summary>Copied publication-time scene with one immutable mesh definition and repeated rigid occurrences.</summary>
public sealed class AuthoredMeshScene
{
    internal AuthoredMeshScene(AuthoredMesh mesh, IEnumerable<AuthoredMeshSceneGroup> groups, IEnumerable<AuthoredMeshSceneInstance> instances) =>
        (Mesh, Groups, Instances) = (mesh, Array.AsReadOnly(groups.ToArray()), Array.AsReadOnly(instances.ToArray()));
    public AuthoredMesh Mesh { get; }
    public IReadOnlyList<AuthoredMeshSceneGroup> Groups { get; }
    public IReadOnlyList<AuthoredMeshSceneInstance> Instances { get; }
}
/// <summary>Document-bound publication labels plus a fully copied publication-time scene.</summary>
public sealed class MeshAssemblyProduct
{
    internal MeshAssemblyProduct(XdeLabel root, XdeLabel definition, XdeLabel[] occurrences, AuthoredMeshScene scene, MeshEditMap coordinateMap) =>
        (Root, Definition, Occurrences, Scene, CoordinateMap) = (root, definition, Array.AsReadOnly(occurrences), scene, coordinateMap);
    public XdeLabel Root { get; }
    public XdeLabel Definition { get; }
    public IReadOnlyList<XdeLabel> Occurrences { get; }
    public AuthoredMeshScene Scene { get; }
    public MeshEditMap CoordinateMap { get; }
}

/// <summary>Publishes discrete groups/materials and repeated instances atomically through the existing XDE owner.</summary>
public static class MeshAssembly
{
    public static MeshAssemblyProduct Create(XdeDocument document, AuthoredMesh source, string name,
        IEnumerable<MeshPlacement>? placements = null, IReadOnlyDictionary<string, XdeVisualMaterial>? materials = null)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(source); ArgumentException.ThrowIfNullOrWhiteSpace(name);
        document.ThrowIfDisposed();
        if (document.HasOpenTransaction) throw new InvalidOperationException("Mesh publication owns one transaction; close the current transaction first.");
        MeshPlacement[] instances = placements is null ? [new(name)] : MeshDataValidation.Copy(placements, nameof(placements), 100_000);
        if (instances.Length == 0 || instances.Any(p => p is null)) throw new ArgumentException("At least one non-null mesh occurrence is required.");
        foreach (MeshPlacement placement in instances) placement.Validate();
        Dictionary<string, XdeVisualMaterial> copiedMaterials = materials is null ? new(StringComparer.Ordinal) : new(materials, StringComparer.Ordinal);
        foreach (MeshGroup group in source.Groups)
            if (group.MaterialKey is not null)
            {
                if (!copiedMaterials.TryGetValue(group.MaterialKey, out XdeVisualMaterial? material) || material is null)
                    throw new ArgumentException($"Material '{group.MaterialKey}' is not resolved for mesh group {group.Key}.");
                material.Validate();
            }
        MeshEditResult converted = MeshEditing.ConvertCoordinates(source, new()); AuthoredMesh mesh = converted.Mesh;
        if (mesh.Triangles.Count == 0) throw new ArgumentException("A mesh assembly needs at least one triangle.");
        using XdeTransaction transaction = document.BeginTransaction("Publish authored mesh");
        XdeLabel root = document.AddAssembly(name), definition = document.AddAssembly($"{name} geometry");
        using TopLocLocation identity = TopLocLocation.Identity;
        List<AuthoredMeshSceneGroup> sceneGroups = [];
        foreach (MeshGroup group in mesh.Groups)
        {
            int[] selected = Enumerable.Range(0, mesh.Triangles.Count).Where(i => mesh.Triangles[i].Group == group.Key).ToArray();
            if (selected.Length == 0) continue;
            AuthoredMesh partMesh = MeshEditing.Extract(mesh, mesh.SelectTriangles(selected)).Mesh;
            using Shape face = MeshTopology.CreateFace(partMesh);
            XdeLabel part = document.AddShape(face, group.Name); part.AsciiString = $"mesh-group:{group.Key}";
            XdeVisualMaterial? material = group.MaterialKey is null ? null : copiedMaterials[group.MaterialKey];
            if (material is not null) { part.VisualMaterial = material; part.Color = material.BaseColor; }
            document.AddComponent(definition, part, identity);
            sceneGroups.Add(new(group, material, part.Entry));
        }
        List<XdeLabel> labels = []; List<AuthoredMeshSceneInstance> sceneInstances = [];
        foreach (MeshPlacement placement in instances)
        {
            using GpTrsf transform = GpTrsf.Create(placement.X, placement.Y, placement.Z, placement.AxisX, placement.AxisY, placement.AxisZ, placement.Angle);
            using TopLocLocation location = TopLocLocation.FromTransform(transform);
            XdeLabel occurrence = document.AddComponent(root, definition, location); occurrence.Name = placement.Name; labels.Add(occurrence);
            MeshTransform copy = new(transform.Value(1, 1), transform.Value(1, 2), transform.Value(1, 3), transform.Value(1, 4),
                transform.Value(2, 1), transform.Value(2, 2), transform.Value(2, 3), transform.Value(2, 4),
                transform.Value(3, 1), transform.Value(3, 2), transform.Value(3, 3), transform.Value(3, 4));
            sceneInstances.Add(new(occurrence.Entry, placement, copy));
        }
        transaction.Commit();
        return new(root, definition, labels.ToArray(), new(mesh, sceneGroups, sceneInstances), converted.Map);
    }
}
