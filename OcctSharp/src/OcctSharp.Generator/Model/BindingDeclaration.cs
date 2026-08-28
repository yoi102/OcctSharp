namespace OcctSharp.Generator.Model;

public sealed record BindingDeclaration(
    string StableId,
    string NativeName,
    BindingDeclarationKind Kind,
    string Header,
    int Line,
    int Column)
{
    public string NativeSignature { get; init; } = NativeName;

    public string SourcePackage { get; init; } = string.Empty;

    public string? SourceToolkit { get; init; }

    public OcctProductModule ProductModule { get; init; }

    public BindingAccess Access { get; init; }

    public BindingType? ReturnType { get; init; }

    public IReadOnlyList<BindingParameter> Parameters { get; init; } = [];

    public IReadOnlyList<BindingBaseType> BaseTypes { get; init; } = [];

    public IReadOnlyList<BindingEnumValue> EnumValues { get; init; } = [];

    public string? EnumUnderlyingType { get; init; }

    public bool IsConst { get; init; }

    public bool IsStatic { get; init; }

    public bool IsVariadic { get; init; }

    public bool IsVirtual { get; init; }

    public bool IsPureVirtual { get; init; }

    public bool IsAbstract { get; init; }

    public bool IsTemplated { get; init; }

    public int TemplateParameterListCount { get; init; }

    public string TemplateSpecializationKind { get; init; } = "Undeclared";

    public bool IsDeleted { get; init; }

    public bool IsUnavailable { get; init; }

    public bool IsDeprecated { get; init; }

    public bool IsOverloadedOperator { get; init; }

    public BindingSupportState SupportState { get; init; } = BindingSupportState.Pending;

    public BindingSkipReason? SkipReason { get; init; }
}
