using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class GenerationScopeExpander
{
    public static IReadOnlyList<GenerationScopeConfiguration> Expand(
        BindingModel model,
        IReadOnlyList<GenerationScopeConfiguration> explicitScopes,
        bool includeAllSupportedStaticMethods)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(explicitScopes);
        if (!includeAllSupportedStaticMethods)
        {
            return explicitScopes;
        }

        BindingModel eligible = SharedHandleBindingEligibilityPass.Apply(
            SimpleBindingEligibilityPass.Apply(model));
        List<GenerationScopeConfiguration> result = [.. explicitScopes];
        HashSet<string> identities = explicitScopes
            .Select(static scope => scope.SourcePackage + "\u001f" + scope.NativeNamePrefix)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> exportNames = explicitScopes
            .Select(static scope => scope.ExportNamePrefix)
            .ToHashSet(StringComparer.Ordinal);

        foreach (IGrouping<string, BindingDeclaration> group in eligible.Declarations
                     .Where(static declaration =>
                         declaration.Kind == BindingDeclarationKind.Method
                         && declaration.IsStatic
                         && declaration.SupportState == BindingSupportState.Supported)
                     .Where(declaration => !explicitScopes.Any(scope =>
                         string.Equals(scope.SourcePackage, declaration.SourcePackage, StringComparison.Ordinal)
                         && declaration.NativeName.StartsWith(scope.NativeNamePrefix, StringComparison.Ordinal)))
                     .Where(static declaration => GetDeclaringType(declaration.NativeName).Length != 0)
                     .GroupBy(
                         static declaration => declaration.SourcePackage + "\u001f" + declaration.NativeName + "\u001f" + declaration.Header,
                         StringComparer.Ordinal)
                     .OrderBy(static group => group.Key, StringComparer.Ordinal))
        {
            BindingDeclaration first = group
                .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
                .First();
            string declaringType = GetDeclaringType(first.NativeName);
            string prefix = first.NativeName;
            string identity = first.SourcePackage + "\u001f" + prefix + "\u001f" + first.Header;
            if (!identities.Add(identity))
            {
                continue;
            }
            string exportNamePrefix = ToSnakeCase(first.NativeName);
            while (!exportNames.Add(exportNamePrefix))
            {
                exportNamePrefix += "_auto";
            }
            result.Add(new GenerationScopeConfiguration
            {
                SourcePackage = first.SourcePackage,
                NativeNamePrefix = prefix,
                Header = first.Header,
                ExportNamePrefix = exportNamePrefix,
                ManagedNamePrefix = ToManagedTypeName(declaringType),
                ExactNativeName = true,
            });
        }


        foreach (IGrouping<string, BindingDeclaration> group in eligible.Declarations
                     .Where(static declaration =>
                         declaration.Kind == BindingDeclarationKind.Function
                         && declaration.SupportState == BindingSupportState.Supported)
                     .GroupBy(
                         static declaration => declaration.SourcePackage + "\u001f" + declaration.NativeName + "\u001f" + declaration.Header,
                         StringComparer.Ordinal)
                     .OrderBy(static group => group.Key, StringComparer.Ordinal))
        {
            BindingDeclaration first = group
                .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
                .First();
            string headerStem = Path.GetFileNameWithoutExtension(first.Header);
            string identity = first.SourcePackage + "\u001f" + first.NativeName + "\u001f" + first.Header;
            if (!identities.Add(identity))
            {
                continue;
            }
            string exportNamePrefix = ToSnakeCase(first.SourcePackage + "_" + headerStem + "_" + first.NativeName);
            while (!exportNames.Add(exportNamePrefix))
            {
                exportNamePrefix += "_auto";
            }
            result.Add(new GenerationScopeConfiguration
            {
                SourcePackage = first.SourcePackage,
                NativeNamePrefix = first.NativeName,
                Header = first.Header,
                ExportNamePrefix = exportNamePrefix,
                ManagedNamePrefix = ToManagedTypeName(first.SourcePackage + "_" + headerStem),
                ExactNativeName = true,
            });
        }

        return result
            .OrderBy(static scope => scope.SourcePackage, StringComparer.Ordinal)
            .ThenBy(static scope => scope.NativeNamePrefix, StringComparer.Ordinal)
            .ThenBy(static scope => scope.Header, StringComparer.Ordinal)
            .ToArray();
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
            if (current is '_' or ':')
            {
                if (builder.Length > 0 && builder[^1] != '_') builder.Append('_');
                continue;
            }
            if (char.IsUpper(current) && index > 0 && value[index - 1] is not '_' and not ':' && !char.IsUpper(value[index - 1]))
                builder.Append('_');
            builder.Append(char.ToLowerInvariant(current));
        }
        return builder.ToString();
    }
}
