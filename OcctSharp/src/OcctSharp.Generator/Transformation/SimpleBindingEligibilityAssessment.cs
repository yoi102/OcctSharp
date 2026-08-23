namespace OcctSharp.Generator.Transformation;

public sealed record SimpleBindingEligibilityAssessment(
    string Code,
    string Category,
    string Detail,
    bool IsEligible);
