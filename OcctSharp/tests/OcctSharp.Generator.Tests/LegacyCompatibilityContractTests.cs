using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class LegacyCompatibilityContractTests
{
    private static BindingDeclaration KnotDeclaration()
    {
        var operation = ClosedWorkflowOperationContract.LegacyKnotBuffers.Operations.Values.Single();
        return new(operation.StableId, operation.NativeName, BindingDeclarationKind.Method, "BSplCLib.hxx", 1, 1)
        {
            NativeSignature = operation.Signature,
            Access = BindingAccess.Public,
            ProductModule = OcctProductModule.Geometry
        };
    }

    [Fact]
    public void StaticSequenceCannotDriftIntoScalarOrManualEmission()
    {
        var contract = ClosedWorkflowOperationContract.LegacyKnotBuffers;
        var declaration = KnotDeclaration();
        Assert.True(contract.Contains(declaration));
        Assert.False(contract.Contains(declaration with { NativeSignature = "BSplCLib::FlatBezierKnots: double (int)" }));
        Assert.False(contract.Contains(declaration with { Access = BindingAccess.Protected }));
        Assert.False(contract.Contains(declaration with { SupportState = BindingSupportState.Manual }));
        Assert.False(contract.Contains(declaration with { IsUnavailable = true }));
        Assert.False(SimpleBindingEligibilityPass.Assess(declaration, new()).IsEligible);
    }

    [Fact]
    public void CompatibilityEmissionKeepsExistingNamesAndDegreeAndCapacityGuards()
    {
        var result = ClosedWorkflowOperationEmitter.Emit("8.0.1", new([KnotDeclaration()]), ClosedWorkflowOperationContract.LegacyKnotBuffers);
        Assert.Single(result.SourceStableIds);
        Assert.Equal(3, result.Files.Count);
        var native = result.Files.Single(f => f.RelativePath.EndsWith(".cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("occtsharp_bsplclib_flat_bezier_knots", native);
        Assert.Contains("degree > BSplCLib::MaxDegree()", native);
        Assert.Contains("2 * (degree + 1)", native);
        Assert.Contains("ValidateOutputCapacity", native);
        Assert.Contains("*written = 0", native);
        Assert.Contains("public static class BSplineKnotBuffers", result.Files.Single(f => f.RelativePath.EndsWith(".cs", StringComparison.Ordinal)).Content);
        Assert.All(LegacyFoundationSupportEmitter.Emit(), f => Assert.Contains("no additional SDK declaration coverage", f.Content));
    }

    [Fact]
    public void ProvenanceIncludesBothContractsAndTemplatesInStableOrder()
    {
        var first = GeneratorResourceProvenance.Capture();
        Assert.Equal(first, GeneratorResourceProvenance.Capture());
        Assert.Equal(first.Select(x => x.ResourceName).Order(StringComparer.Ordinal), first.Select(x => x.ResourceName));
        Assert.Equal(first.Count, first.Select(x => x.ResourceName).Distinct().Count());
        Assert.Contains(first, x => x.ResourceName.EndsWith("legacy-knot-buffers-operation-contracts.json", StringComparison.Ordinal));
        Assert.Contains(first, x => x.ResourceName.EndsWith("legacy-foundation-collections-native.txt", StringComparison.Ordinal));
        Assert.All(first, x => Assert.Matches("^[0-9A-F]{64}$", x.Sha256));
    }
}
