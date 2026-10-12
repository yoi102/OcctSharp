using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class HatchingWorkspaceContractTests
{
    static readonly ClosedWorkflowOperationContract Contract = ClosedWorkflowOperationContract.HatchingWorkspaces;
    static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op) => new(
        op.StableId, op.NativeName, BindingDeclarationKind.Method, op.NativeName.Split("::")[0] + ".hxx", 1, 1)
    { NativeSignature = op.Signature, Access = BindingAccess.Public, ProductModule = OcctProductModule.Geometry };

    [Fact]
    public void PrimaryAdmissionRequiresExactSignatureAndPublicAccess()
    {
        var d = Declaration(Contract.Operations.Values.First(o => !o.Supporting));
        Assert.True(Contract.Contains(d));
        Assert.False(Contract.Contains(d with { NativeSignature = d.NativeSignature + " " }));
        Assert.False(Contract.Contains(d with { Access = BindingAccess.Protected }));
        Assert.False(Contract.Contains(d with { SupportState = BindingSupportState.Manual }));
        Assert.False(Contract.Contains(d with { IsPureVirtual = true }));
    }

    [Fact]
    public void SupportingDeclarationsDoNotStartAnUnselectedWorkflow()
    {
        var rows = Contract.Operations.Values.Where(o => o.Supporting).Select(Declaration);
        var output = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(rows), Contract);
        Assert.Empty(output.Files);
        Assert.Empty(output.SourceStableIds);
    }

    [Fact]
    public void PrimaryWithoutFullPrerequisitesCannotEmit()
    {
        var d = Declaration(Contract.Operations.Values.First(o => !o.Supporting));
        Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel([d]), Contract));
    }

    [Fact]
    public void DeterministicCatalogueKeepsGeometryModuleAndExcludesBorrowedMutation()
    {
        var rows = Contract.Operations.Values.Select(Declaration).ToArray();
        var a = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(rows), Contract);
        var b = ClosedWorkflowOperationEmitter.Emit("8.0.1", new BindingModel(rows.Reverse()), Contract);
        Assert.Equal(37, a.SourceStableIds.Count);
        Assert.Equal(a.Files.Select(f => f.Content), b.Files.Select(f => f.Content));
        Assert.All(a.Files, f => Assert.Equal(OcctProductModule.Geometry, f.ProductModule));
        Assert.DoesNotContain(Contract.Operations.Values, o => o.NativeName.EndsWith("ChangeIntersector", StringComparison.Ordinal));
        Assert.DoesNotContain(Contract.Operations.Values, o => o.Signature.Contains("&&"));
        Assert.DoesNotContain(Contract.Operations.Values, o => o.Signature == "Geom2dHatch_Hatcher::IsDone: bool () const");
    }
}
