using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Tests;

public sealed class GridCollectionContractTests
{
    private static BindingType Type(string name, bool reference = false) => new(name, name, name, name,
        reference ? [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, true)] : [new(BindingTypeLayerKind.Value, false)], null, [], false, null);
    private static BindingDeclaration Create()
    {
        var op = GridCollectionOperationContract.Operations.Values.Single(o => o.NativeName == "GeomAPI_Interpolate::GeomAPI_Interpolate" && !o.Signature.Contains("HArray1<double>", StringComparison.Ordinal));
        var points = Type("occ::handle<NCollection_HArray1<gp_Pnt>>", true) with { IsOcctHandle = true, HandleTargetType = "NCollection_HArray1<gp_Pnt>" };
        return new(op.StableId, op.NativeName, BindingDeclarationKind.Constructor, "GeomAPI_Interpolate.hxx", 0, 0) {
            NativeSignature = op.Signature, ProductModule = OcctProductModule.Geometry, Access = BindingAccess.Public,
            ReturnType = Type("void"), Parameters = [new(0, "Points", points, false), new(1, "PeriodicFlag", Type("bool"), false), new(2, "Tolerance", Type("double"), false)]
        };
    }
    [Fact]
    public void CatalogueAdmitsOnlyFrozenScopeAndKeepsNullablePointersExcluded()
    {
        Assert.Equal(63, GridCollectionOperationContract.Operations.Values.Count(o => !o.Supporting));
        Assert.Equal(18, GridCollectionOperationContract.Operations.Values.Count(o => o.Supporting));
        Assert.DoesNotContain(GridCollectionOperationContract.Operations.Values, o => o.Signature.Contains("*()", StringComparison.Ordinal));
        var d = Create(); Assert.True(GridCollectionOperationContract.Contains(d));
        Assert.False(GridCollectionOperationContract.Contains(d with { NativeSignature = d.NativeSignature + "drift" }));
        Assert.False(GridCollectionOperationContract.Contains(d with { Access = BindingAccess.Private }));
        Assert.False(GridCollectionOperationContract.Contains(d with { SupportState = BindingSupportState.Skipped }));
        Assert.Throws<InvalidDataException>(() => CopiedCollectionOperationEmitter.Emit("8.0.1", new([d with { NativeSignature = "drift" }]), [], new HashSet<string>(), [], grid: true));
    }
    [Fact]
    public void RetainedInputsUseIndependentIntrusiveCopiesAndCountedAlgorithmOwner()
    {
        var d = Create(); var map = new InitialTypeMap(); var points = d.Parameters[0].Type;
        Assert.True(GridCollectionOperationContract.TryMap(d, points, BindingTypeUsage.Parameter, map, out var p));
        Assert.Equal("RetainedCollectionCopy", p!.Ownership);
        Assert.False(GridCollectionOperationContract.Contains(d with { StableId = "unlisted" }));
        var grid = Type("NCollection_Array2<double>", true);
        Assert.False(GridCollectionOperationContract.TryMap(d with { StableId = "unlisted" }, grid, BindingTypeUsage.Parameter, map, out _));
        var files = CopiedCollectionOperationEmitter.Emit("8.0.1", new([d]), [], new HashSet<string>(), [], grid: true).Files;
        string cpp = files.Single(f => f.RelativePath.EndsWith("Geometry.GridOperations.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("std::make_shared<InterpolationOwner<GeomAPI_Interpolate>>", cpp, StringComparison.Ordinal);
        Assert.Contains("opencascade::handle<NCollection_HArray1<gp_Pnt>>(new NCollection_HArray1<gp_Pnt>(local0))", cpp, StringComparison.Ordinal);
        Assert.DoesNotContain("std::make_shared<NCollection_HArray1", cpp, StringComparison.Ordinal);
        Assert.Contains("Publication<InterpolationOwner<GeomAPI_Interpolate>>", cpp, StringComparison.Ordinal);
        Assert.Contains("(std::numeric_limits<int>::max() - 16) / 2", cpp, StringComparison.Ordinal);
    }
    [Theory]
    [InlineData("NCollection_Array2<float>")]
    [InlineData("NCollection_Array2<gp_Vec>")]
    [InlineData("NCollection_Array2<TopoDS_Shape>")]
    [InlineData("NCollection_Sequence<gp_Pnt>")]
    public void UnlistedTemplatesStayClosed(string name) => Assert.Null(GridCollectionOperationContract.Find(Type(name, true)));
    [Fact]
    public void PointersNeverUseGridOrRetainedCopyMapping()
    {
        var pointer = Type("NCollection_Array2<double>") with { Layers = [new(BindingTypeLayerKind.PointerIndirection, false), new(BindingTypeLayerKind.Value, true)] };
        Assert.Null(GridCollectionOperationContract.Find(pointer));
        Assert.Null(GridCollectionOperationContract.Find(pointer with { Layers = [new(BindingTypeLayerKind.PointerIndirection, false)] }));
        var harray = Create().Parameters[0].Type with { Layers = pointer.Layers };
        Assert.Null(GridCollectionOperationContract.Find(harray));
    }
    [Fact]
    public void SupportChecksOffsetsAndCopiesByRowsAndColumnsWithNormalizedBooleanAbi()
    {
        var files = GridCollectionSupportEmitter.Emit();
        string support = files.Single(f => f.RelativePath.EndsWith(".hxx", StringComparison.Ordinal)).Content;
        Assert.Contains("safe(count + col)", support, StringComparison.Ordinal);
        Assert.Contains("safe(begin + count)", support, StringComparison.Ordinal);
        string cpp = files.Single(f => f.RelativePath.EndsWith(".cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("row + i / cols, col + i % cols", cpp, StringComparison.Ordinal);
        Assert.Contains("std::vector<uint8_t>", cpp, StringComparison.Ordinal);
        Assert.Contains("Boolean(element)", cpp, StringComparison.Ordinal);
        Assert.True(cpp.IndexOf("GridBounds(row", StringComparison.Ordinal) < cpp.IndexOf("std::make_shared", StringComparison.Ordinal));
        string cs = files.Single(f => f.RelativePath.EndsWith(".cs", StringComparison.Ordinal)).Content;
        Assert.Contains("CopiedArray<bool>", cs, StringComparison.Ordinal);
        Assert.Contains("byte* data", cs, StringComparison.Ordinal);
        Assert.DoesNotContain("bool* data", cs, StringComparison.Ordinal);
    }
}
