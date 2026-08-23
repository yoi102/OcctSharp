namespace OcctSharp.Generator.Model;

public sealed record BindingType(
    string NativeSpelling,
    string CanonicalSpelling,
    string BaseNativeSpelling,
    string BaseCanonicalSpelling,
    IReadOnlyList<BindingTypeLayer> Layers,
    string? TemplateName,
    IReadOnlyList<BindingTemplateArgument> TemplateArguments,
    bool IsOcctHandle,
    string? HandleTargetType);
