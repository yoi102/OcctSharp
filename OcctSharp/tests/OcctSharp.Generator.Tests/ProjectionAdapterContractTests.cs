using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Tests;
public sealed class ProjectionAdapterContractTests {
    static ClosedWorkflowOperationContract Contract=>ClosedWorkflowOperationContract.ProjectionAdapters;
    static BindingDeclaration Row(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,
        op.NativeName.EndsWith("::GeomAPI_ProjectPointOnCurve",StringComparison.Ordinal)||op.NativeName.EndsWith("::GeomAPI_ProjectPointOnSurf",StringComparison.Ordinal)?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,
        op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public};
    [Fact] public void ExactCallPrivateClosureRejectsDriftUnsafeAndRemainingManual() {
        foreach(var op in Contract.Operations.Values){var d=Row(op);Assert.True(Contract.Contains(d));
            Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));
            Assert.False(Contract.Contains(d with{SupportState=BindingSupportState.Manual}));
            Assert.False(Contract.Contains(d with{Access=BindingAccess.Private}));
            Assert.False(Contract.Contains(d with{IsVariadic=true}));}
        Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel([Row(Contract.Operations.Values.First())]),Contract));
    }
    [Fact] public void CompleteClosureEmitsOnlyNativePrivateHeaderDeterministically() {
        var rows=Contract.Operations.Values.Select(Row).ToArray();
        var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows),Contract);
        var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()),Contract);
        Assert.Equal(17,a.SourceStableIds.Count);Assert.Single(a.Files);Assert.Equal(a.Files[0].Content,b.Files[0].Content);
        Assert.EndsWith(".hxx",a.Files[0].RelativePath);Assert.DoesNotContain("OCCTSHARP_CALL",a.Files[0].Content);
    }
}
