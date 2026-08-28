namespace OcctSharp.Generator.Model;

public static class OcctProductModuleGraph
{
    private static readonly Dictionary<OcctProductModule, OcctProductModule[]> DirectDependencies =
        new Dictionary<OcctProductModule, OcctProductModule[]>
        {
            [OcctProductModule.Runtime] = [],
            [OcctProductModule.Foundation] = [OcctProductModule.Runtime],
            [OcctProductModule.Geometry] = [OcctProductModule.Foundation],
            [OcctProductModule.MeshData] = [OcctProductModule.Geometry],
            [OcctProductModule.Modeling] = [OcctProductModule.Geometry, OcctProductModule.MeshData],
            [OcctProductModule.Mesh] = [OcctProductModule.Modeling, OcctProductModule.MeshData],
            [OcctProductModule.Documents] = [OcctProductModule.Modeling, OcctProductModule.MeshData],
            [OcctProductModule.Visualization] =
                [OcctProductModule.Modeling, OcctProductModule.Mesh, OcctProductModule.Documents],
            [OcctProductModule.DataExchange] =
                [OcctProductModule.Modeling, OcctProductModule.MeshData, OcctProductModule.Mesh,
                    OcctProductModule.Documents, OcctProductModule.Visualization],
            [OcctProductModule.Xde] =
                [OcctProductModule.Documents, OcctProductModule.DataExchange, OcctProductModule.Visualization],
            [OcctProductModule.IVtk] = [OcctProductModule.Visualization],
            [OcctProductModule.OpenGles] = [OcctProductModule.Visualization],
            [OcctProductModule.Draw] = [OcctProductModule.Xde, OcctProductModule.Visualization],
        };

    public static IReadOnlyList<OcctProductModule> GetDirectDependencies(OcctProductModule module) =>
        DirectDependencies.TryGetValue(module, out OcctProductModule[]? dependencies)
            ? dependencies
            : throw new ArgumentOutOfRangeException(nameof(module), module, "The module has no dependency contract.");

    public static bool CanReference(OcctProductModule source, OcctProductModule target) =>
        source == target || GetDependencyClosure(source).Contains(target);

    public static IReadOnlySet<OcctProductModule> GetDependencyClosure(OcctProductModule module)
    {
        HashSet<OcctProductModule> result = [];
        AddDependencies(module, result, []);
        return result;
    }

    private static void AddDependencies(
        OcctProductModule module,
        HashSet<OcctProductModule> result,
        HashSet<OcctProductModule> active)
    {
        if (!active.Add(module))
        {
            throw new InvalidDataException($"The product-module dependency graph contains a cycle at '{module}'.");
        }
        foreach (OcctProductModule dependency in GetDirectDependencies(module))
        {
            if (result.Add(dependency))
            {
                AddDependencies(dependency, result, active);
            }
        }
        active.Remove(module);
    }
}
