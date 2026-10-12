using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Tests;
public sealed class ClosedWorkflowContractTests {
 static readonly ClosedWorkflowOperationContract Contract=ClosedWorkflowOperationContract.ModelingBuilders;
 static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op)=>new(op.StableId,op.NativeName,BindingDeclarationKind.Method,op.NativeName.Split("::")[0]+".hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Modeling};
 [Fact]public void ExactAccessAndLifetimeCatalogueRequired(){var d=Declaration(Contract.Operations.Values.First());Assert.True(Contract.Contains(d));Assert.False(Contract.Contains(d with{NativeSignature=d.NativeSignature+" "}));Assert.False(Contract.Contains(d with{IsDeleted=true}));Assert.False(Contract.Contains(d with{Access=BindingAccess.Private}));Assert.False(Contract.Contains(d with{SupportState=BindingSupportState.Manual}));}
 [Fact]public void IncompleteSelectedScopeCannotEmit(){var d=Declaration(Contract.Operations.Values.First());Assert.Throws<InvalidDataException>(()=>ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel([d]),Contract));}
 [Fact]public void DeterminismAndPrivateModuleClosure(){var rows=Contract.Operations.Values.Select(Declaration).ToArray();var a=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows),Contract);var b=ClosedWorkflowOperationEmitter.Emit("8.0.1",new BindingModel(rows.Reverse()),Contract);Assert.Equal(66,a.SourceStableIds.Count);Assert.Equal(a.Files.Select(f=>f.Content),b.Files.Select(f=>f.Content));Assert.All(a.Files,f=>Assert.Equal(OcctProductModule.Modeling,f.ProductModule));}
 [Fact]public void GlobalMappingDoesNotAdmitBorrowedBuilders(){var t=new BindingType("BRepLib_MakeWire","BRepLib_MakeWire","BRepLib_MakeWire","BRepLib_MakeWire",[new(BindingTypeLayerKind.Value,false)],null,[],false,null);var map=InitialTypeMap.FromModel(new BindingModel([]));var d=Declaration(Contract.Operations.Values.First());Assert.False(map.TryMap(t,BindingTypeUsage.ReturnValue,out _));Assert.True(Contract.TryMap(d,t,BindingTypeUsage.ReturnValue,map,out var p));Assert.Equal("TM021",p!.RuleId);Assert.False(Contract.TryMap(d with{StableId="unapproved"},t,BindingTypeUsage.ReturnValue,map,out _));}
 [Fact]public void MutableImplicitCopiesRemainExcluded()=>Assert.DoesNotContain(Contract.Operations.Values,o=>o.Signature.Contains("&&")||o.Signature.Contains("const ShapeAnalysis_ShapeContents &")||o.NativeName.EndsWith("ModifyOffestSurfaceMode",StringComparison.Ordinal));
}
