namespace OcctSharp;

/// <summary>Owns a complete managed snapshot of mesh definitions and scene instances.</summary>
public sealed class MeshScene
{
    private MeshScene(MeshSceneDefinition[] definitions, MeshSceneNode[] nodes)
    {
        Definitions = Array.AsReadOnly(definitions);
        Nodes = Array.AsReadOnly(nodes);
        RootNodeIndices = Array.AsReadOnly(nodes
            .Where(static node => node.ParentIndex < 0)
            .Select(static node => node.Index)
            .ToArray());
    }

    /// <summary>Gets deduplicated copied mesh definitions.</summary>
    public IReadOnlyList<MeshSceneDefinition> Definitions { get; }
    /// <summary>Gets copied assembly, part, and occurrence nodes in parent-before-child order.</summary>
    public IReadOnlyList<MeshSceneNode> Nodes { get; }
    /// <summary>Gets indices of all root nodes.</summary>
    public IReadOnlyList<int> RootNodeIndices { get; }
    /// <summary>Gets the number of nodes that instantiate a mesh definition.</summary>
    public int InstanceCount => Nodes.Count(static node => node.MeshDefinitionIndex >= 0);
    /// <summary>Gets the triangle count across deduplicated definitions.</summary>
    public int TotalTriangleCount => Definitions.Sum(static definition => definition.Mesh.Statistics.TriangleCount);

    /// <summary>Creates a one-node copied scene from an owning shape.</summary>
    public static MeshScene FromShape(
        Shape shape,
        string name = "Shape",
        AdvancedMeshOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        AdvancedMeshSnapshot mesh = AdvancedMesh.Create(shape, options);
        MeshSceneDefinition definition = new(0, "shape:0", mesh);
        MeshSceneNode node = new(
            0, -1, "shape:0", "shape:0", name, false, 0,
            MeshTransform.Identity, MeshTransform.Identity,
            Array.AsReadOnly(["shape:0"]), Array.Empty<string>(),
            null, null, null);
        return new MeshScene([definition], [node]);
    }

    /// <summary>Copies hierarchy, instances, metadata, transforms, and deduplicated meshes from XDE.</summary>
    public static MeshScene FromXdeDocument(
        XdeDocument document,
        AdvancedMeshOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.ThrowIfDisposed();
        AdvancedMeshOptions effective = options ?? new AdvancedMeshOptions();
        effective.Validate();

        List<MeshSceneDefinition> definitions = [];
        List<MeshSceneNode> nodes = [];
        Dictionary<string, int> definitionIndices = new(StringComparer.Ordinal);
        HashSet<string> activeAssemblies = new(StringComparer.Ordinal);

        foreach (XdeLabel root in document.GetFreeShapes())
        {
            using TopLocLocation identity = TopLocLocation.Identity;
            AddNode(root, root, -1, identity, identity, [root.Entry]);
        }
        return new MeshScene([.. definitions], [.. nodes]);

        void AddNode(
            XdeLabel occurrence,
            XdeLabel definition,
            int parentIndex,
            TopLocLocation localLocation,
            TopLocLocation worldLocation,
            IReadOnlyList<string> path)
        {
            bool isAssembly = definition.IsAssembly;
            XdeColor? color = occurrence.Color ?? definition.Color;
            XdeVisualMaterial? visualMaterial = occurrence.VisualMaterial ?? definition.VisualMaterial;
            XdeMaterial? physicalMaterial = occurrence.Material ?? definition.Material;
            IReadOnlyList<string> layers = occurrence.Layers.Count > 0
                ? Array.AsReadOnly([.. occurrence.Layers])
                : Array.AsReadOnly([.. definition.Layers]);
            int meshIndex = isAssembly ? -1 : GetDefinition(definition, color, visualMaterial);
            int nodeIndex = nodes.Count;
            nodes.Add(new MeshSceneNode(
                nodeIndex,
                parentIndex,
                occurrence.Entry,
                definition.Entry,
                occurrence.Name ?? definition.Name ?? definition.Entry,
                isAssembly,
                meshIndex,
                ToTransform(localLocation),
                ToTransform(worldLocation),
                Array.AsReadOnly([.. path]),
                layers,
                color,
                visualMaterial,
                physicalMaterial));

            if (!isAssembly) return;
            if (!activeAssemblies.Add(definition.Entry))
                throw new InvalidOperationException("The XDE scene contains an assembly cycle.");
            try
            {
                foreach (XdeLabel component in definition.GetComponents())
                {
                    XdeLabel referred = component.ReferredShape;
                    using TopLocLocation local = component.Location;
                    using TopLocLocation world = worldLocation.Multiplied(local);
                    AddNode(component, referred, nodeIndex, local, world, [.. path, component.Entry]);
                }
            }
            finally { activeAssemblies.Remove(definition.Entry); }
        }

        int GetDefinition(XdeLabel definition, XdeColor? color, XdeVisualMaterial? material)
        {
            if (definitionIndices.TryGetValue(definition.Entry, out int existing)) return existing;
            using Shape shape = definition.Shape;
            AdvancedMeshSnapshot mesh = AdvancedMesh.Create(shape, effective).WithStyle(color, material);
            int index = definitions.Count;
            definitions.Add(new MeshSceneDefinition(index, definition.Entry, mesh));
            definitionIndices.Add(definition.Entry, index);
            return index;
        }
    }

    /// <summary>Imports glTF or GLB and returns a document-independent scene snapshot.</summary>
    public static MeshScene ReadGltf(string filePath, AdvancedMeshOptions? options = null)
    {
        using XdeDocument document = XdeDocument.ReadGltf(filePath);
        return FromXdeDocument(document, options);
    }

    /// <summary>Imports OBJ and returns a document-independent scene snapshot.</summary>
    public static MeshScene ReadObj(string filePath, AdvancedMeshOptions? options = null)
    {
        using XdeDocument document = XdeDocument.ReadObj(filePath);
        return FromXdeDocument(document, options);
    }

    private static MeshTransform ToTransform(TopLocLocation location)
    {
        using GpTrsf transform = location.ToTransform();
        return new MeshTransform(
            transform.Value(1, 1), transform.Value(1, 2), transform.Value(1, 3), transform.Value(1, 4),
            transform.Value(2, 1), transform.Value(2, 2), transform.Value(2, 3), transform.Value(2, 4),
            transform.Value(3, 1), transform.Value(3, 2), transform.Value(3, 3), transform.Value(3, 4));
    }
}
