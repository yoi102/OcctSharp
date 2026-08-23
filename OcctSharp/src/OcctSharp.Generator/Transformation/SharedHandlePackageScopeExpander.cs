using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class SharedHandlePackageScopeExpander
{
    public static IReadOnlyList<SharedHandleScopeConfiguration> Expand(
        BindingModel model,
        IReadOnlyList<SharedHandleScopeConfiguration> explicitScopes,
        IReadOnlyList<SharedHandlePackageScopeConfiguration> packageScopes)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(explicitScopes);
        ArgumentNullException.ThrowIfNull(packageScopes);

        BindingModel eligibleModel = SharedHandleBindingEligibilityPass.Apply(
            SimpleBindingEligibilityPass.Apply(model));
        List<SharedHandleScopeConfiguration> result = [.. explicitScopes];
        HashSet<string> selectedTypes = explicitScopes
            .Select(static scope => scope.NativeType)
            .ToHashSet(StringComparer.Ordinal);

        foreach (SharedHandlePackageScopeConfiguration packageScope in packageScopes
            .OrderBy(static scope => scope.SourcePackage, StringComparer.Ordinal)
            .ThenBy(static scope => scope.NativeTypePrefix, StringComparer.Ordinal))
        {
            Validate(packageScope);
            HashSet<string> excluded = packageScope.ExcludedNativeTypes.ToHashSet(StringComparer.Ordinal);
            IEnumerable<IGrouping<string, BindingDeclaration>> constructorGroups = eligibleModel.Declarations
                .Where(declaration => declaration.Kind == BindingDeclarationKind.Constructor
                    && declaration.SupportState == BindingSupportState.Supported
                    && string.Equals(declaration.SourcePackage, packageScope.SourcePackage, StringComparison.Ordinal))
                .Select(declaration => (Declaration: declaration, DeclaringType: GetDeclaringType(declaration.NativeName)))
                .Where(item => item.DeclaringType.StartsWith(packageScope.NativeTypePrefix, StringComparison.Ordinal)
                    && !excluded.Contains(item.DeclaringType))
                .GroupBy(static item => item.DeclaringType, static item => item.Declaration, StringComparer.Ordinal)
                .OrderBy(static group => group.Key, StringComparer.Ordinal);

            foreach (IGrouping<string, BindingDeclaration> group in constructorGroups)
            {
                if (!selectedTypes.Add(group.Key))
                {
                    continue;
                }

                BindingDeclaration declaration = group
                    .OrderBy(static item => item.StableId, StringComparer.Ordinal)
                    .First();
                result.Add(new SharedHandleScopeConfiguration
                {
                    SourcePackage = packageScope.SourcePackage,
                    NativeType = group.Key,
                    Header = declaration.Header,
                    ExportNamePrefix = ToSnakeCase(group.Key),
                    ManagedTypeName = ToManagedTypeName(group.Key),
                });
            }
        }

        return result.OrderBy(static scope => scope.NativeType, StringComparer.Ordinal).ToArray();
    }

    private static void Validate(SharedHandlePackageScopeConfiguration scope)
    {
        if (string.IsNullOrWhiteSpace(scope.SourcePackage)
            || string.IsNullOrWhiteSpace(scope.NativeTypePrefix))
        {
            throw new InvalidDataException(
                "Every shared-handle package scope must define sourcePackage and nativeTypePrefix.");
        }
    }

    private static string GetDeclaringType(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        return separator <= 0 ? string.Empty : nativeName[..separator];
    }

    private static string ToManagedTypeName(string nativeType)
    {
        string[] parts = nativeType.Split(['_', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Concat(parts.Select(static part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static string ToSnakeCase(string value)
    {
        StringBuilder builder = new(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (current == '_')
            {
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }
                continue;
            }
            if (char.IsUpper(current)
                && index > 0
                && value[index - 1] != '_'
                && !char.IsUpper(value[index - 1]))
            {
                builder.Append('_');
            }
            builder.Append(char.ToLowerInvariant(current));
        }
        return builder.ToString();
    }
}
