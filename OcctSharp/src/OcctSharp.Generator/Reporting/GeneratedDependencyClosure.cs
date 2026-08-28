using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Reporting;

public sealed record GeneratedDependencyClosureReport(
    string SchemaVersion,
    string OcctVersion,
    string BindingModelSchemaVersion,
    int EmittedDeclarationCount,
    int GeneratedFileCount,
    bool IsComplete,
    bool ManagedProjectSplitReady,
    bool NativeDllSplitReady,
    string RecommendedDecision,
    IReadOnlyList<GeneratedModuleDependency> DirectDependencies,
    IReadOnlyList<GeneratedModuleClosure> TransitiveClosures,
    IReadOnlyList<GeneratedModuleCycle> CyclicGroups,
    IReadOnlyList<GeneratedDependencyIssue> Issues);

public sealed record GeneratedModuleDependency(
    string SourceModule,
    string TargetModule,
    bool AllowedByTargetGraph,
    int ReferenceCount,
    IReadOnlyList<string> SourceStableIds);

public sealed record GeneratedModuleClosure(
    string SourceModule,
    IReadOnlyList<string> ReferencedModules);

public sealed record GeneratedModuleCycle(IReadOnlyList<string> Modules);

public sealed record GeneratedDependencyIssue(
    string Code,
    string SourceStableId,
    string SourceModule,
    string ReferenceKind,
    string NativeType,
    string Detail);

public static class GeneratedDependencyClosureAnalyzer
{
    private const string KeepSingleDecision = "KeepSingleManagedProjectAndNativeDll";

    public static GeneratedDependencyClosureReport Create(
        string occtVersion,
        string bindingModelSchemaVersion,
        BindingModel model,
        GeneratedBindingSet bindingSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occtVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(bindingModelSchemaVersion);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(bindingSet);

        HashSet<string> emittedIds = bindingSet.SourceStableIds.ToHashSet(StringComparer.Ordinal);
        BindingDeclaration[] emittedDeclarations = model.Declarations
            .Where(declaration => emittedIds.Contains(declaration.StableId))
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .ToArray();
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        NativeTypeModuleIndex moduleIndex = NativeTypeModuleIndex.Create(model);
        List<ResolvedReference> references = [];
        List<GeneratedDependencyIssue> issues = [];

        foreach (BindingDeclaration declaration in emittedDeclarations)
        {
            if (declaration.ProductModule == OcctProductModule.Unassigned)
            {
                issues.Add(new GeneratedDependencyIssue(
                    "SD001",
                    declaration.StableId,
                    declaration.ProductModule.ToString(),
                    "Declaration",
                    declaration.NativeName,
                    "An emitted declaration has no product-module assignment."));
                continue;
            }

            if (declaration.ReturnType is not null)
            {
                ResolveType(
                    declaration,
                    declaration.ReturnType,
                    BindingTypeUsage.ReturnValue,
                    "Return",
                    typeMap,
                    moduleIndex,
                    references,
                    issues);
            }

            foreach (BindingParameter parameter in declaration.Parameters.OrderBy(static parameter => parameter.Position))
            {
                ResolveType(
                    declaration,
                    parameter.Type,
                    BindingTypeUsage.Parameter,
                    $"Parameter[{parameter.Position}]",
                    typeMap,
                    moduleIndex,
                    references,
                    issues);
            }

            foreach ((BindingBaseType baseType, int index) in declaration.BaseTypes
                .Select(static (item, index) => (item, index)))
            {
                ResolveType(
                    declaration,
                    baseType.Type,
                    BindingTypeUsage.Field,
                    $"Base[{index}]",
                    typeMap,
                    moduleIndex,
                    references,
                    issues);
            }
        }

        GeneratedModuleDependency[] directDependencies = references
            .Where(static reference => reference.Source != reference.Target)
            .GroupBy(static reference => (reference.Source, reference.Target))
            .OrderBy(static group => group.Key.Source)
            .ThenBy(static group => group.Key.Target)
            .Select(static group => new GeneratedModuleDependency(
                group.Key.Source.ToString(),
                group.Key.Target.ToString(),
                OcctProductModuleGraph.CanReference(group.Key.Source, group.Key.Target),
                group.Count(),
                group.Select(static reference => reference.SourceStableId)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()))
            .ToArray();

        foreach (GeneratedModuleDependency dependency in directDependencies
            .Where(static dependency => !dependency.AllowedByTargetGraph))
        {
            issues.Add(new GeneratedDependencyIssue(
                "SD002",
                dependency.SourceStableIds[0],
                dependency.SourceModule,
                "ModuleEdge",
                dependency.TargetModule,
                $"The observed {dependency.SourceModule} -> {dependency.TargetModule} edge is outside the ADR-0061 target graph."));
        }

        OcctProductModule[] presentModules = emittedDeclarations
            .Select(static declaration => declaration.ProductModule)
            .Where(static module => module != OcctProductModule.Unassigned)
            .Concat(bindingSet.Files.Select(static file => file.ProductModule))
            .Distinct()
            .Order()
            .ToArray();
        Dictionary<OcctProductModule, OcctProductModule[]> adjacency = presentModules.ToDictionary(
            static module => module,
            module => directDependencies
                .Where(dependency => string.Equals(dependency.SourceModule, module.ToString(), StringComparison.Ordinal))
                .Select(static dependency => Enum.Parse<OcctProductModule>(dependency.TargetModule))
                .Distinct()
                .Order()
                .ToArray());
        GeneratedModuleClosure[] closures = presentModules
            .Select(module => new GeneratedModuleClosure(
                module.ToString(),
                GetObservedClosure(module, adjacency).Select(static target => target.ToString()).ToArray()))
            .ToArray();
        GeneratedModuleCycle[] cycles = FindCyclicGroups(presentModules, adjacency)
            .Select(static group => new GeneratedModuleCycle(group.Select(static module => module.ToString()).ToArray()))
            .ToArray();

        GeneratedDependencyIssue[] orderedIssues = issues
            .OrderBy(static issue => issue.Code, StringComparer.Ordinal)
            .ThenBy(static issue => issue.SourceModule, StringComparer.Ordinal)
            .ThenBy(static issue => issue.SourceStableId, StringComparer.Ordinal)
            .ThenBy(static issue => issue.ReferenceKind, StringComparer.Ordinal)
            .ThenBy(static issue => issue.NativeType, StringComparer.Ordinal)
            .ToArray();
        bool isComplete = orderedIssues.All(static issue => issue.Code == "SD002");
        bool managedProjectSplitReady = isComplete
            && directDependencies.All(static dependency => dependency.AllowedByTargetGraph)
            && cycles.Length == 0;

        return new GeneratedDependencyClosureReport(
            "1.0",
            occtVersion,
            bindingModelSchemaVersion,
            emittedDeclarations.Length,
            bindingSet.Files.Count,
            isComplete,
            managedProjectSplitReady,
            NativeDllSplitReady: false,
            managedProjectSplitReady ? "ManagedSplitEligibleNativeSplitDeferred" : KeepSingleDecision,
            directDependencies,
            closures,
            cycles,
            orderedIssues);
    }

    private static void ResolveType(
        BindingDeclaration declaration,
        BindingType type,
        BindingTypeUsage usage,
        string referenceKind,
        InitialTypeMap typeMap,
        NativeTypeModuleIndex moduleIndex,
        List<ResolvedReference> references,
        List<GeneratedDependencyIssue> issues)
    {
        if (!typeMap.TryMap(type, usage, out BindingTypeProjection? projection) || projection is null)
        {
            issues.Add(Unresolved(declaration, referenceKind, type, "No accepted TypeMap projection exists for an emitted signature type."));
            return;
        }

        OcctProductModule? target = projection.RuleId switch
        {
            "TM004" => moduleIndex.Resolve(type.BaseCanonicalSpelling, type.BaseNativeSpelling),
            "TM005" => OcctProductModule.Geometry,
            "TM006" when !string.IsNullOrWhiteSpace(type.HandleTargetType) =>
                moduleIndex.Resolve(type.HandleTargetType),
            "TM007" => OcctProductModule.Modeling,
            _ => null,
        };

        if (projection.RuleId is "TM004" or "TM006" && target is null)
        {
            issues.Add(Unresolved(
                declaration,
                referenceKind,
                type,
                $"{projection.RuleId} resolved the ABI projection but its OCCT target has no product-module identity."));
            return;
        }

        if (target is not null)
        {
            references.Add(new ResolvedReference(
                declaration.ProductModule,
                target.Value,
                declaration.StableId));
        }
    }

    private static GeneratedDependencyIssue Unresolved(
        BindingDeclaration declaration,
        string referenceKind,
        BindingType type,
        string detail) => new(
            "SD001",
            declaration.StableId,
            declaration.ProductModule.ToString(),
            referenceKind,
            type.IsOcctHandle && !string.IsNullOrWhiteSpace(type.HandleTargetType)
                ? type.HandleTargetType
                : type.CanonicalSpelling,
            detail);

    private static OcctProductModule[] GetObservedClosure(
        OcctProductModule source,
        IReadOnlyDictionary<OcctProductModule, OcctProductModule[]> adjacency)
    {
        HashSet<OcctProductModule> result = [];
        Stack<OcctProductModule> pending = new(adjacency.GetValueOrDefault(source, []));
        while (pending.Count != 0)
        {
            OcctProductModule target = pending.Pop();
            if (!result.Add(target))
            {
                continue;
            }
            foreach (OcctProductModule next in adjacency.GetValueOrDefault(target, []))
            {
                pending.Push(next);
            }
        }
        result.Remove(source);
        return result.Order().ToArray();
    }

    private static OcctProductModule[][] FindCyclicGroups(
        IReadOnlyList<OcctProductModule> modules,
        IReadOnlyDictionary<OcctProductModule, OcctProductModule[]> adjacency)
    {
        Dictionary<OcctProductModule, int> indices = [];
        Dictionary<OcctProductModule, int> lowLinks = [];
        HashSet<OcctProductModule> onStack = [];
        Stack<OcctProductModule> stack = [];
        List<OcctProductModule[]> groups = [];
        int index = 0;

        void Visit(OcctProductModule module)
        {
            indices[module] = index;
            lowLinks[module] = index;
            index++;
            stack.Push(module);
            onStack.Add(module);

            foreach (OcctProductModule target in adjacency.GetValueOrDefault(module, []))
            {
                if (!indices.TryGetValue(target, out int targetIndex))
                {
                    Visit(target);
                    lowLinks[module] = Math.Min(lowLinks[module], lowLinks[target]);
                }
                else if (onStack.Contains(target))
                {
                    lowLinks[module] = Math.Min(lowLinks[module], targetIndex);
                }
            }

            if (lowLinks[module] != indices[module])
            {
                return;
            }

            List<OcctProductModule> group = [];
            OcctProductModule item;
            do
            {
                item = stack.Pop();
                onStack.Remove(item);
                group.Add(item);
            }
            while (item != module);

            if (group.Count > 1)
            {
                groups.Add(group.Order().ToArray());
            }
        }

        foreach (OcctProductModule module in modules)
        {
            if (!indices.ContainsKey(module))
            {
                Visit(module);
            }
        }

        return groups
            .OrderBy(static group => group[0])
            .ToArray();
    }

    private sealed record ResolvedReference(
        OcctProductModule Source,
        OcctProductModule Target,
        string SourceStableId);

    private sealed class NativeTypeModuleIndex
    {
        private readonly IReadOnlyDictionary<string, OcctProductModule> _exact;
        private readonly IReadOnlyDictionary<string, OcctProductModule> _unqualified;

        private NativeTypeModuleIndex(
            IReadOnlyDictionary<string, OcctProductModule> exact,
            IReadOnlyDictionary<string, OcctProductModule> unqualified)
        {
            _exact = exact;
            _unqualified = unqualified;
        }

        public static NativeTypeModuleIndex Create(BindingModel model)
        {
            BindingDeclaration[] types = model.Declarations
                .Where(static declaration => declaration.Kind is BindingDeclarationKind.Record or BindingDeclarationKind.Enum)
                .Where(static declaration => declaration.ProductModule != OcctProductModule.Unassigned)
                .ToArray();
            Dictionary<string, OcctProductModule> exact = new(StringComparer.Ordinal);
            foreach (IGrouping<string, BindingDeclaration> group in types.GroupBy(
                static declaration => NormalizeTypeName(declaration.NativeName),
                StringComparer.Ordinal))
            {
                OcctProductModule[] modules = group.Select(static declaration => declaration.ProductModule).Distinct().ToArray();
                if (modules.Length == 1)
                {
                    exact[group.Key] = modules[0];
                }
            }

            Dictionary<string, OcctProductModule> unqualified = new(StringComparer.Ordinal);
            foreach (IGrouping<string, KeyValuePair<string, OcctProductModule>> group in exact.GroupBy(
                static item => GetUnqualifiedName(item.Key),
                StringComparer.Ordinal))
            {
                OcctProductModule[] modules = group.Select(static item => item.Value).Distinct().ToArray();
                if (group.Count() == 1 || modules.Length == 1)
                {
                    unqualified[group.Key] = modules[0];
                }
            }
            return new NativeTypeModuleIndex(exact, unqualified);
        }

        public OcctProductModule? Resolve(params string[] spellings)
        {
            foreach (string spelling in spellings.Where(static spelling => !string.IsNullOrWhiteSpace(spelling)))
            {
                string normalized = NormalizeTypeName(spelling);
                if (_exact.TryGetValue(normalized, out OcctProductModule exact)
                    || _unqualified.TryGetValue(GetUnqualifiedName(normalized), out exact))
                {
                    return exact;
                }
            }
            return null;
        }

        private static string NormalizeTypeName(string value)
        {
            string normalized = value.Trim();
            if (normalized.StartsWith("const ", StringComparison.Ordinal))
            {
                normalized = normalized[6..].TrimStart();
            }
            if (normalized.EndsWith(" const", StringComparison.Ordinal))
            {
                normalized = normalized[..^6].TrimEnd();
            }
            return normalized.TrimStart(':');
        }

        private static string GetUnqualifiedName(string value)
        {
            int separator = value.LastIndexOf("::", StringComparison.Ordinal);
            return separator < 0 ? value : value[(separator + 2)..];
        }
    }
}
