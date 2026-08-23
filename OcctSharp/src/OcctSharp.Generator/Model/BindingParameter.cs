namespace OcctSharp.Generator.Model;

public sealed record BindingParameter(
    int Position,
    string Name,
    BindingType Type,
    bool HasDefaultArgument);
