using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Tests;

public sealed class CopiedCollectionContractTests
{
    private static BindingType Type(string name, bool reference = false, bool isConst = false) => new(
        name, name, name, name, reference ? [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, isConst)]
            : [new(BindingTypeLayerKind.Value, isConst)], null, [], false, null);
    private static BindingDeclaration Reverse()
    {
        var entry = CopiedCollectionOperationContract.Operations.Values.Single(o => o.NativeName == "BSplCLib::Reverse"
            && o.Signature == "BSplCLib::Reverse: void (NCollection_Array1<int> &)");
        return new(entry.StableId, entry.NativeName, BindingDeclarationKind.Method, "BSplCLib.hxx", 0, 0)
        {
            NativeSignature = entry.Signature, Access = BindingAccess.Public, ProductModule = OcctProductModule.Geometry,
            IsStatic = true, ReturnType = Type("void"), Parameters = [new(0, "Mults", Type("NCollection_Array1<int>", true), false)],
        };
    }

    [Fact]
    public void GeneralMapStillRejectsCollectionsWhileAuditedMapAcceptsExactEntry()
    {
        var d = Reverse(); var map = new InitialTypeMap(); var type = d.Parameters[0].Type;
        Assert.False(map.TryMap(type, BindingTypeUsage.Parameter, out _));
        Assert.True(CopiedCollectionOperationContract.TryMap(d, type, BindingTypeUsage.Parameter, map, out var projection));
        Assert.Equal("TM011", projection!.RuleId);
        Assert.False(CopiedCollectionOperationContract.TryMap(d with { StableId = "unlisted" }, type, BindingTypeUsage.Parameter, map, out _));
    }

    [Theory]
    [InlineData("NCollection_Array1<float>")]
    [InlineData("NCollection_Array1<TopoDS_Shape>")]
    [InlineData("NCollection_Array2<double>")]
    [InlineData("NCollection_Sequence<gp_Pnt>")]
    public void UnlistedContainersAndElementsStayBlocked(string name) => Assert.Null(CopiedCollectionOperationContract.Find(Type(name, true)));

    [Fact]
    public void PointerContainersNeverUseTheCopyProfile()
    {
        var type = Type("NCollection_Array1<double>") with { Layers = [new(BindingTypeLayerKind.PointerIndirection, false), new(BindingTypeLayerKind.Value, true)] };
        Assert.Null(CopiedCollectionOperationContract.Find(type));
    }

    [Fact]
    public void SelectionPreservesManualAndSkippedAndRejectsDrift()
    {
        var d = Reverse();
        Assert.True(CopiedCollectionOperationContract.Contains(d));
        Assert.False(CopiedCollectionOperationContract.Contains(d with { SupportState = BindingSupportState.Manual }));
        Assert.False(CopiedCollectionOperationContract.Contains(d with { SupportState = BindingSupportState.Skipped }));
        Assert.False(CopiedCollectionOperationContract.Contains(d with { Access = BindingAccess.Private }));
        Assert.Throws<InvalidDataException>(() => CopiedCollectionOperationEmitter.Emit("8.0.1", new([d with { NativeSignature = d.NativeSignature + " drift" }]), [], new HashSet<string>(), []));
    }

    [Fact]
    public void NativeCopiesAndTransactionalPublicationPrecedeManagedRefAssignment()
    {
        var result = CopiedCollectionOperationEmitter.Emit("8.0.1", new([Reverse()]), [], new HashSet<string>(), []);
        var cpp = result.Files.Single(f => f.RelativePath.EndsWith("Geometry.CollectionOperations.Generated.cpp", StringComparison.Ordinal)).Content;
        var cs = result.Files.Single(f => f.RelativePath.EndsWith("Geometry.CollectionOperations.Generated.cs", StringComparison.Ordinal)).Content;
        Assert.Contains("auto local0 = *OcctSharpOwned::Lease<const NCollection_Array1<int>>", cpp, StringComparison.Ordinal);
        Assert.Contains("Publication<NCollection_Array1<int>>", cpp, StringComparison.Ordinal);
        Assert.True(cs.IndexOf("ReadIntegerArray", StringComparison.Ordinal) < cs.IndexOf("arg0 = copied_", StringComparison.Ordinal));
        Assert.Contains("finally", cs, StringComparison.Ordinal);
        Assert.Equal(Reverse().StableId, Assert.Single(result.SourceStableIds));
    }
}
