using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class SupportClassificationPass
{
    public static BindingModel Apply(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        HashSet<string> bindableRecords = model.Declarations
            .Where(static declaration =>
                declaration.Kind == BindingDeclarationKind.Record
                && declaration.Access is not (BindingAccess.Private or BindingAccess.Protected)
                && !declaration.IsTemplated
                && IsSimpleIdentifier(declaration.NativeName))
            .Select(static declaration => declaration.NativeName)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> allRecords = model.Declarations
            .Where(static declaration => declaration.Kind == BindingDeclarationKind.Record)
            .Select(static declaration => declaration.NativeName)
            .ToHashSet(StringComparer.Ordinal);
        return new BindingModel(model.Declarations.Select(declaration =>
            Classify(declaration, bindableRecords, allRecords)));
    }

    private static BindingDeclaration Classify(
        BindingDeclaration declaration,
        IReadOnlySet<string> bindableRecords,
        IReadOnlySet<string> allRecords)
    {
        if (declaration.SupportState == BindingSupportState.Manual)
        {
            return declaration;
        }

        BindingSkipReason? reason = GetSkipReason(declaration, bindableRecords, allRecords);
        return declaration with
        {
            SupportState = reason is null ? BindingSupportState.Pending : BindingSupportState.Skipped,
            SkipReason = reason,
        };
    }

    private static BindingSkipReason? GetSkipReason(
        BindingDeclaration declaration,
        IReadOnlySet<string> bindableRecords,
        IReadOnlySet<string> allRecords)
    {
        if (declaration.IsUnavailable)
        {
            return new BindingSkipReason("SK001", "Unavailable", "Clang marks the declaration unavailable.");
        }

        if (declaration.IsDeleted)
        {
            return new BindingSkipReason("SK002", "Deleted", "The C++ declaration is deleted.");
        }

        if (declaration.Access is BindingAccess.Private or BindingAccess.Protected)
        {
            return new BindingSkipReason(
                "SK003",
                "NonPublic",
                $"The declaration has {declaration.Access.ToString().ToLowerInvariant()} C++ access.");
        }

        if (declaration.IsVariadic)
        {
            return new BindingSkipReason("SK004", "Variadic", "C-style variadic calls do not have a safe general ABI projection.");
        }

        if (declaration.IsTemplated)
        {
            return new BindingSkipReason("SK005", "Template", "Template declarations require an explicit specialization rule.");
        }

        if (declaration.IsOverloadedOperator)
        {
            return new BindingSkipReason("SK006", "Operator", "Operator overload projection is not implemented.");
        }

        if (declaration.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method)
        {
            string? declaringType = GetDeclaringType(declaration.NativeName);
            if (declaringType is not null
                && (!IsSimpleIdentifier(declaringType)
                    || (allRecords.Contains(declaringType)
                        && !bindableRecords.Contains(declaringType))))
            {
                return new BindingSkipReason(
                    "SK011",
                    "NonBindableDeclaringType",
                    "The member belongs to a nested, templated, or non-public C++ record that has no stable top-level C ABI identity.");
            }
        }

        return null;
    }

    private static string? GetDeclaringType(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        return separator <= 0 ? null : nativeName[..separator];
    }

    private static bool IsSimpleIdentifier(string value)
    {
        if (value.Length == 0 || !(value[0] == '_' || char.IsAsciiLetter(value[0])))
        {
            return false;
        }
        return value.Skip(1).All(static character =>
            character == '_' || char.IsAsciiLetterOrDigit(character));
    }
}
