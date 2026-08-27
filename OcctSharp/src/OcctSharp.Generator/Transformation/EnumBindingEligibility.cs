using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class EnumBindingEligibility
{
    public static bool HasStableManagedTypeIdentity(BindingDeclaration declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        if (declaration.Kind != BindingDeclarationKind.Enum || declaration.EnumValues.Count == 0)
        {
            return false;
        }

        string[] components = declaration.NativeName.Split("::", StringSplitOptions.None);
        return components.Length != 0 && components.All(IsIdentifier);
    }

    private static bool IsIdentifier(string value) =>
        value.Length != 0
        && (value[0] == '_' || char.IsAsciiLetter(value[0]))
        && value.Skip(1).All(static character =>
            character == '_' || char.IsAsciiLetterOrDigit(character));
}
