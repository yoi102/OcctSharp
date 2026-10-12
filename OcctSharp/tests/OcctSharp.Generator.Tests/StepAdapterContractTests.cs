using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;

public sealed class StepAdapterContractTests
{
    [Fact] public void ExactAdmissionRejectsSignatureDriftAndIncompleteClosure()
    {
        var contract = ClosedWorkflowOperationContract.StepAdapters;
        Assert.Equal(11, contract.Operations.Count);
        foreach (var op in contract.Operations.Values)
        {
            var row = new BindingDeclaration(op.StableId, op.NativeName, BindingDeclarationKind.Method, "STEPControl_Reader.hxx", 1, 1)
            { NativeSignature = op.Signature, Access = BindingAccess.Public, SupportState = BindingSupportState.Pending };
            Assert.True(contract.Contains(row));
            Assert.False(contract.Contains(row with { NativeSignature = op.Signature + " const" }));
            Assert.False(contract.Contains(row with { IsDeleted = true }));
            Assert.False(contract.Contains(row with { SupportState = BindingSupportState.Manual }));
            Assert.Throws<InvalidDataException>(() => ClosedWorkflowOperationEmitter.Emit("8.0.1", new([row]), contract));
        }
    }

    [Fact] public void StepReceiverIsBoundBeforeArgumentsWithoutCopyingSession()
    {
        var type = new BindingType("void", "void", "void", "void", [new(BindingTypeLayerKind.Value, false)], null, [], false, null);
        var row = new BindingDeclaration("test", "XSControl_Reader::ClearShapes", BindingDeclarationKind.Method, "XSControl_Reader.hxx", 1, 1)
        { NativeSignature = "XSControl_Reader::ClearShapes: void ()", ReturnType = type };
        var source = Assert.Single(PrivateDocumentOperationEmitter.Emit("8.0.1", [row], ClosedWorkflowOperationContract.StepAdapters, "").Files).Content;
        Assert.Contains("inline auto XSControl_Reader_ClearShapes_Bind(XSControl_Reader& receiver)", source);
        Assert.Contains("[address=&receiver]", source);
        Assert.DoesNotContain("OCCTSHARP_CALL", source);
    }
}
