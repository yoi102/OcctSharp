namespace OcctSharp.Generator.Model;

public sealed record BindingBaseType(
    BindingType Type,
    BindingAccess Access,
    bool IsVirtual);
