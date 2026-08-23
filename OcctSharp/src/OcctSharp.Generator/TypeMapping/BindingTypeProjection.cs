namespace OcctSharp.Generator.TypeMapping;

public sealed record BindingTypeProjection(
    string RuleId,
    string AbiType,
    string ManagedRawType,
    string ManagedFriendlyType,
    string Ownership,
    string Marshalling);
