using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class SupportClassificationPass
{
    public static BindingModel Apply(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new BindingModel(model.Declarations.Select(Classify));
    }

    private static BindingDeclaration Classify(BindingDeclaration declaration)
    {
        if (declaration.SupportState == BindingSupportState.Manual)
        {
            return declaration;
        }

        BindingSkipReason? reason = GetSkipReason(declaration);
        return declaration with
        {
            SupportState = reason is null ? BindingSupportState.Pending : BindingSupportState.Skipped,
            SkipReason = reason,
        };
    }

    private static BindingSkipReason? GetSkipReason(BindingDeclaration declaration)
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

        return null;
    }
}
