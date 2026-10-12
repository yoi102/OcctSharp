using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class DocumentProjectionContractTests
{
    static readonly ClosedWorkflowOperationContract[] Contracts =
        [ClosedWorkflowOperationContract.DocumentProjections, ClosedWorkflowOperationContract.XdeAttributeProjections];
    static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op) => new(
        op.StableId, op.NativeName, BindingDeclarationKind.Method, op.NativeName.Split("::")[0] + ".hxx", 1, 1)
    { NativeSignature = op.Signature, Access = BindingAccess.Public, ProductModule = OcctProductModule.Documents };

    [Fact] public void DocumentBoundaryRequiresExactPublicSignature()
    {
        foreach (var contract in Contracts)
        {
            var d = Declaration(contract.Operations.Values.First(o => !o.Supporting));
            Assert.True(contract.Contains(d));
            Assert.False(contract.Contains(d with { NativeSignature = d.NativeSignature + " " }));
            Assert.False(contract.Contains(d with { Access = BindingAccess.Private }));
            Assert.False(contract.Contains(d with { SupportState = BindingSupportState.Manual }));
            Assert.False(contract.Contains(d with { IsPureVirtual = true }));
        }
    }
    [Fact] public void ExistingSupportCannotPublishAnUnadmittedDocumentWorkflow()
    {
        foreach (var contract in Contracts)
        {
            var output = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(contract.Operations.Values.Where(o => o.Supporting).Select(Declaration)), contract);
            Assert.Empty(output.Files); Assert.Empty(output.SourceStableIds);
        }
    }
    [Fact] public void PartialDocumentClosureIsRejected()
    {
        foreach (var contract in Contracts)
            Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1",
                new BindingModel([Declaration(contract.Operations.Values.First(o => !o.Supporting))]), contract));
    }
    [Fact] public void DocumentRulesAreDeterministicAndDoNotAdmitGlobalMutationOrStreams()
    {
        foreach (var contract in Contracts)
        {
            var rows = contract.Operations.Values.Select(Declaration).ToArray();
            var a = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(rows), contract);
            var b = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(rows.Reverse()), contract);
            Assert.Equal(contract.Operations.Count, a.SourceStableIds.Count);
            Assert.Equal(a.Files.Select(f => f.Content), b.Files.Select(f => f.Content));
            Assert.All(a.Files, f => Assert.Equal(contract.Module, f.ProductModule));
            Assert.DoesNotContain(contract.Operations.Values, o => o.Signature.Contains("Standard_OStream") || o.Signature.Contains("OSD_File") || o.Signature.Contains("&&"));
            Assert.DoesNotContain(contract.Operations.Values, o => o.NativeName.EndsWith("::RelocateLabel", StringComparison.Ordinal) || o.NativeName.EndsWith("::SetObject", StringComparison.Ordinal));
        }
        var document = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(Contracts[0].Operations.Values.Select(Declaration)), Contracts[0]);
        Assert.Single(document.Files, f => f.RelativePath.EndsWith("ProjectionSupport.Generated.hxx", StringComparison.Ordinal));
    }
}
