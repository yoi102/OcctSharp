using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

internal static class GeometricOperationEligibility
{
    internal static string Owner(BindingDeclaration declaration) => declaration.NativeName.Contains("::", StringComparison.Ordinal)
        ? declaration.NativeName[..declaration.NativeName.LastIndexOf("::", StringComparison.Ordinal)] : string.Empty;

    internal static bool IsEligible(BindingDeclaration declaration, InitialTypeMap map)
    {
        string owner = Owner(declaration);
        if (owner != "gp_Pnt" && !GeometryValueProjections.All.Any(value => value.NativeType == owner)) return false;
        if (declaration.Access != BindingAccess.Public || declaration.IsStatic || declaration.IsOverloadedOperator
            || declaration.IsDeleted || declaration.IsUnavailable || declaration.IsTemplated || declaration.IsVariadic
            || declaration.SupportState is BindingSupportState.Skipped or BindingSupportState.Manual
            || declaration.Kind is not (BindingDeclarationKind.Method or BindingDeclarationKind.Constructor)
            || declaration.NativeName.Contains("::~", StringComparison.Ordinal)) return false;
        // Native cache/form flags are not part of the copied coefficient contract.
        if (declaration.NativeName is "gp_Trsf::SetForm" or "gp_GTrsf::SetForm") return false;
        if (declaration.Parameters.Any(p => !IsValue(p.Type, BindingTypeUsage.Parameter, map))) return false;
        return declaration.Kind == BindingDeclarationKind.Constructor
            || (declaration.ReturnType is { } result && map.TryMap(result, BindingTypeUsage.ReturnValue, out BindingTypeProjection? output)
                && output is { } && (output.RuleId == "TM000" || output.Ownership == "ValueCopy"));
    }

    internal static bool IsValue(BindingType type, BindingTypeUsage usage, InitialTypeMap map) =>
        map.TryMap(type, usage, out BindingTypeProjection? projection) && projection?.Ownership == "ValueCopy";
}
