using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

public static class SharedHandlePackageScopeExpander
{
    public static IReadOnlyList<SharedHandleScopeConfiguration> Expand(BindingModel model, IReadOnlyList<SharedHandleScopeConfiguration> explicitScopes, IReadOnlyList<SharedHandlePackageScopeConfiguration> packageScopes)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(explicitScopes); ArgumentNullException.ThrowIfNull(packageScopes);
        BindingModel eligibleModel = SharedHandleBindingEligibilityPass.Apply(SimpleBindingEligibilityPass.Apply(model));
        InitialTypeMap typeMap = InitialTypeMap.FromModel(eligibleModel);
        HashSet<string> transientTypes = FindTransientTypes(model);
        HashSet<string> abstractTypes = FindAbstractTypes(model);
        List<SharedHandleScopeConfiguration> result = [.. explicitScopes];
        HashSet<string> selectedTypes = explicitScopes.Select(static scope => scope.NativeType).ToHashSet(StringComparer.Ordinal);
        HashSet<string> packageTransientTypes = model.Declarations.Where(declaration => IsBindableRecordDeclaration(declaration) && transientTypes.Contains(declaration.NativeName) && MatchesPackageScope(declaration, packageScopes)).Select(static declaration => declaration.NativeName).ToHashSet(StringComparer.Ordinal);
        HashSet<string> candidateTypes = FindPackageCandidateTypes(eligibleModel, packageScopes, typeMap, transientTypes, abstractTypes);

        bool changed;
        do
        {
            changed = false;
            foreach (string candidate in candidateTypes.ToArray())
            {
                bool hasClosedConstructor = eligibleModel.Declarations.Any(declaration => declaration.Kind == BindingDeclarationKind.Constructor && declaration.SupportState == BindingSupportState.Supported && string.Equals(GetDeclaringType(declaration.NativeName), candidate, StringComparison.Ordinal) && declaration.Parameters.All(parameter => IsClosedConstructorParameter(parameter.Type, typeMap, candidateTypes, packageTransientTypes, selectedTypes)));
                if (!hasClosedConstructor) changed |= candidateTypes.Remove(candidate);
            }
        }
        while (changed);

        foreach (SharedHandlePackageScopeConfiguration packageScope in packageScopes.OrderBy(static scope => scope.SourcePackage, StringComparer.Ordinal).ThenBy(static scope => scope.NativeTypePrefix, StringComparer.Ordinal))
        {
            Validate(packageScope);
            HashSet<string> excluded = packageScope.ExcludedNativeTypes.ToHashSet(StringComparer.Ordinal);
            foreach (IGrouping<string, BindingDeclaration> group in eligibleModel.Declarations.Where(declaration => !packageScope.SuppressConstructors && declaration.Kind == BindingDeclarationKind.Constructor && declaration.SupportState == BindingSupportState.Supported && !packageScope.ExcludedStableIds.Contains(declaration.StableId, StringComparer.Ordinal) && string.Equals(declaration.SourcePackage, packageScope.SourcePackage, StringComparison.Ordinal) && declaration.Parameters.All(parameter => IsSupportedConstructorParameter(parameter.Type, typeMap))).Select(declaration => (Declaration: declaration, Type: GetDeclaringType(declaration.NativeName))).Where(item => candidateTypes.Contains(item.Type) && item.Type.StartsWith(packageScope.NativeTypePrefix, StringComparison.Ordinal) && !excluded.Contains(item.Type)).GroupBy(static item => item.Type, static item => item.Declaration, StringComparer.Ordinal).OrderBy(static group => group.Key, StringComparer.Ordinal))
            {
                if (!selectedTypes.Add(group.Key)) continue;
                BindingDeclaration declaration = group.OrderBy(static item => item.StableId, StringComparer.Ordinal).First();
                result.Add(CreateScope(packageScope, group.Key, declaration.Header, false));
            }
            foreach (BindingDeclaration declaration in model.Declarations.Where(declaration => IsBindableRecordDeclaration(declaration) && transientTypes.Contains(declaration.NativeName) && string.Equals(declaration.SourcePackage, packageScope.SourcePackage, StringComparison.Ordinal) && declaration.NativeName.StartsWith(packageScope.NativeTypePrefix, StringComparison.Ordinal) && !excluded.Contains(declaration.NativeName)).OrderBy(static declaration => declaration.NativeName, StringComparer.Ordinal))
            {
                if (selectedTypes.Add(declaration.NativeName)) result.Add(CreateScope(packageScope, declaration.NativeName, declaration.Header, packageScope.SuppressConstructors || abstractTypes.Contains(declaration.NativeName) || !candidateTypes.Contains(declaration.NativeName)));
            }
        }
        return result.OrderBy(static scope => scope.NativeType, StringComparer.Ordinal).ToArray();
    }

    private static SharedHandleScopeConfiguration CreateScope(SharedHandlePackageScopeConfiguration packageScope, string type, string header, bool suppressConstructors) => new() { SourcePackage = packageScope.SourcePackage, NativeType = type, Header = header, ExportNamePrefix = ToSnakeCase(type), ManagedTypeName = ToManagedTypeName(type), SuppressConstructors = suppressConstructors, ExcludedStableIds = packageScope.ExcludedStableIds, UsesPlacementAllocator = packageScope.PlacementAllocatorNativeTypes.Contains(type, StringComparer.Ordinal) };
    private static bool IsSupportedConstructorParameter(BindingType type, InitialTypeMap map) => (map.TryMap(type, BindingTypeUsage.Parameter, out BindingTypeProjection? projection) && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal)) || (type.IsOcctHandle && !string.IsNullOrWhiteSpace(type.HandleTargetType));
    private static bool IsClosedConstructorParameter(BindingType type, InitialTypeMap map, HashSet<string> candidates, HashSet<string> abstractTypes, HashSet<string> explicitTypes) => (map.TryMap(type, BindingTypeUsage.Parameter, out BindingTypeProjection? projection) && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal)) || (type.IsOcctHandle && !string.IsNullOrWhiteSpace(type.HandleTargetType) && (candidates.Contains(type.HandleTargetType.Trim()) || abstractTypes.Contains(type.HandleTargetType.Trim()) || explicitTypes.Contains(type.HandleTargetType.Trim())));
    private static HashSet<string> FindPackageCandidateTypes(BindingModel model, IReadOnlyList<SharedHandlePackageScopeConfiguration> scopes, InitialTypeMap map, HashSet<string> transientTypes, HashSet<string> abstractTypes) => model.Declarations.Where(declaration => declaration.Kind == BindingDeclarationKind.Constructor && declaration.SupportState == BindingSupportState.Supported && declaration.Parameters.All(parameter => IsSupportedConstructorParameter(parameter.Type, map))).Select(declaration => (Declaration: declaration, Type: GetDeclaringType(declaration.NativeName))).Where(item => transientTypes.Contains(item.Type) && !abstractTypes.Contains(item.Type) && scopes.Any(scope => !scope.SuppressConstructors && string.Equals(item.Declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal) && item.Declaration.NativeName.StartsWith(scope.NativeTypePrefix, StringComparison.Ordinal) && !scope.ExcludedNativeTypes.Contains(item.Declaration.NativeName, StringComparer.Ordinal) && !scope.ExcludedStableIds.Contains(item.Declaration.StableId, StringComparer.Ordinal))).Select(static item => item.Type).ToHashSet(StringComparer.Ordinal);
    private static bool MatchesPackageScope(BindingDeclaration declaration, IReadOnlyList<SharedHandlePackageScopeConfiguration> scopes) => scopes.Any(scope => string.Equals(declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal) && declaration.NativeName.StartsWith(scope.NativeTypePrefix, StringComparison.Ordinal) && !scope.ExcludedNativeTypes.Contains(declaration.NativeName, StringComparer.Ordinal));
    private static bool IsBindableRecordDeclaration(BindingDeclaration declaration) => declaration.Kind == BindingDeclarationKind.Record && declaration.Access is not (BindingAccess.Private or BindingAccess.Protected) && !declaration.IsTemplated && IsSimpleIdentifier(declaration.NativeName);
    private static bool IsSimpleIdentifier(string value) => value.Length != 0 && (value[0] == '_' || char.IsAsciiLetter(value[0])) && value.Skip(1).All(static character => character == '_' || char.IsAsciiLetterOrDigit(character));
    private static HashSet<string> FindTransientTypes(BindingModel model) => FindInheritanceClosure(model, ["Standard_Transient"]);
    private static HashSet<string> FindAbstractTypes(BindingModel model)
    {
        HashSet<string> direct = model.Declarations.Where(static declaration => declaration.Kind == BindingDeclarationKind.Record && declaration.IsAbstract).Select(static declaration => declaration.NativeName).ToHashSet(StringComparer.Ordinal);
        foreach (BindingDeclaration method in model.Declarations.Where(static declaration => declaration.Kind == BindingDeclarationKind.Method && declaration.IsPureVirtual)) direct.Add(GetDeclaringType(method.NativeName));
        // Abstractness itself is not an inheritance closure: a derived OCCT type may
        // implement every inherited pure virtual member. Clang's record fact is the
        // authoritative result, while the method check only covers a direct parser gap.
        return direct;
    }
    private static HashSet<string> FindInheritanceClosure(BindingModel model, IEnumerable<string> roots)
    {
        Dictionary<string, string[]> bases = model.Declarations.Where(static declaration => declaration.Kind == BindingDeclarationKind.Record).GroupBy(static declaration => declaration.NativeName, StringComparer.Ordinal).ToDictionary(static group => group.Key, static group => group.SelectMany(static declaration => declaration.BaseTypes).Select(static baseType => NormalizeTypeName(baseType.Type.BaseCanonicalSpelling)).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
        HashSet<string> result = new(roots, StringComparer.Ordinal); bool changed;
        do { changed = false; foreach ((string type, string[] baseTypes) in bases) if (!result.Contains(type) && baseTypes.Any(result.Contains)) changed |= result.Add(type); } while (changed);
        return result;
    }
    private static void Validate(SharedHandlePackageScopeConfiguration scope) { if (string.IsNullOrWhiteSpace(scope.SourcePackage) || string.IsNullOrWhiteSpace(scope.NativeTypePrefix)) throw new InvalidDataException("Every shared-handle package scope must define sourcePackage and nativeTypePrefix."); if (scope.PlacementAllocatorNativeTypes.Any(string.IsNullOrWhiteSpace) || scope.PlacementAllocatorNativeTypes.Distinct(StringComparer.Ordinal).Count() != scope.PlacementAllocatorNativeTypes.Count) throw new InvalidDataException($"Shared-handle package scope '{scope.SourcePackage}' has invalid or duplicate placementAllocatorNativeTypes."); }
    private static string GetDeclaringType(string nativeName) { int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal); return separator <= 0 ? string.Empty : nativeName[..separator]; }
    private static string NormalizeTypeName(string value) { string normalized = value.Trim(); foreach (string prefix in new[] { "class ", "struct ", "const " }) if (normalized.StartsWith(prefix, StringComparison.Ordinal)) normalized = normalized[prefix.Length..].TrimStart(); return normalized.EndsWith(" const", StringComparison.Ordinal) ? normalized[..^6].TrimEnd() : normalized; }
    private static string ToManagedTypeName(string nativeType) { string[] parts = nativeType.Split(['_', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); return string.Concat(parts.Select(static part => char.ToUpperInvariant(part[0]) + part[1..])); }
    private static string ToSnakeCase(string value) { StringBuilder builder = new(value.Length + 8); for (int index = 0; index < value.Length; index++) { char current = value[index]; if (current == '_') { if (builder.Length > 0 && builder[^1] != '_') builder.Append('_'); continue; } if (char.IsUpper(current) && index > 0 && value[index - 1] != '_' && !char.IsUpper(value[index - 1])) builder.Append('_'); builder.Append(char.ToLowerInvariant(current)); } return builder.ToString(); }
}
