using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class CoupledConstraintContractTests {
    private static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op) => new(op.StableId, op.NativeName, BindingDeclarationKind.Method, "Test.hxx", 1, 1) { NativeSignature = op.Signature, Access = BindingAccess.Public, ProductModule = OcctProductModule.Geometry };
    [Fact] public void ExactConstraintAdmissionRejectsUnsafeDrift() {
        var contract = ClosedWorkflowOperationContract.CoupledConstraints;
        foreach (var op in contract.Operations.Values) {
            var d = Declaration(op);
            Assert.True(contract.Contains(d));
            Assert.False(contract.Contains(d with { NativeSignature = d.NativeSignature + " " }));
            Assert.False(contract.Contains(d with { Access = BindingAccess.Protected }));
            Assert.False(contract.Contains(d with { IsDeleted = true }));
            Assert.False(contract.Contains(d with { SupportState = BindingSupportState.Manual }));
        }
    }
    [Fact] public void CoupledCopyRequiresCompleteClosureAndDeterministicEmission() {
        var contract = ClosedWorkflowOperationContract.CoupledConstraints;
        var rows = contract.Operations.Values.Select(Declaration).ToArray();
        var first = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows), contract);
        var reversed = ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Reverse()), contract);
        Assert.Equal(20, first.SourceStableIds.Count);
        Assert.Equal(3, first.Files.Count);
        Assert.Equal(first.Files.Select(x => x.Content), reversed.Files.Select(x => x.Content));
        Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new(rows.Skip(1)), contract));
        Assert.DoesNotContain(contract.Operations.Values, x => x.NativeName.Contains("SetTang") || x.Signature.Contains("&&"));
    }
}
