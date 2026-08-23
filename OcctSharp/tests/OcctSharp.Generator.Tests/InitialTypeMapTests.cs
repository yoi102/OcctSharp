using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Tests;

public sealed class InitialTypeMapTests
{
    [Theory]
    [InlineData("Standard_Integer", "int", "TM001", "int32_t", "int")]
    [InlineData("Standard_Real", "double", "TM002", "double", "double")]
    [InlineData("Standard_Boolean", "bool", "TM003", "int32_t", "bool")]
    [InlineData("gp_Pnt", "gp_Pnt", "TM005", "OcctSharp_Point3d", "Point3d")]
    [InlineData("TopoDS_Shape", "TopoDS_Shape", "TM007", "OcctSharp_ShapeHandle*", "Shape")]
    public void MapsInitialValueTypes(
        string nativeType,
        string canonicalType,
        string ruleId,
        string abiType,
        string friendlyType)
    {
        InitialTypeMap map = new();

        bool mapped = map.TryMap(
            CreateValueType(nativeType, canonicalType),
            BindingTypeUsage.ReturnValue,
            out BindingTypeProjection? projection);

        Assert.True(mapped);
        Assert.NotNull(projection);
        Assert.Equal(ruleId, projection.RuleId);
        Assert.Equal(abiType, projection.AbiType);
        Assert.Equal(friendlyType, projection.ManagedFriendlyType);
        Assert.Equal(ruleId == "TM007" ? "Owning" : "ValueCopy", projection.Ownership);
    }

    [Fact]
    public void MapsKnownEnumThroughExplicitUnderlyingAbiType()
    {
        InitialTypeMap map = new(["SampleKind"]);

        Assert.True(map.TryMap(
            CreateValueType("SampleKind", "SampleKind"),
            BindingTypeUsage.Parameter,
            out BindingTypeProjection? projection));
        Assert.Equal("TM004", projection?.RuleId);
        Assert.Equal("int32_t", projection?.AbiType);
        Assert.Equal("SampleKind", projection?.ManagedFriendlyType);
    }

    [Fact]
    public void AllowsConstReferenceInputButRejectsPointerAndReferenceReturn()
    {
        InitialTypeMap map = new();
        BindingType constReference = CreateType(
            "const Standard_Real &",
            "const double &",
            "Standard_Real",
            "double",
            new BindingTypeLayer(BindingTypeLayerKind.LValueReference, false),
            new BindingTypeLayer(BindingTypeLayerKind.Value, true));
        BindingType pointer = CreateType(
            "Standard_Real *",
            "double *",
            "Standard_Real",
            "double",
            new BindingTypeLayer(BindingTypeLayerKind.PointerIndirection, false),
            new BindingTypeLayer(BindingTypeLayerKind.Value, false));

        Assert.True(map.TryMap(constReference, BindingTypeUsage.Parameter, out _));
        Assert.False(map.TryMap(constReference, BindingTypeUsage.ReturnValue, out _));
        Assert.False(map.TryMap(pointer, BindingTypeUsage.Parameter, out _));
    }

    [Fact]
    public void MapsTopLevelConstValueFromClangDiscovery()
    {
        InitialTypeMap map = new();
        BindingType constValue = CreateType(
            "const double",
            "const double",
            "const double",
            "const double",
            new BindingTypeLayer(BindingTypeLayerKind.Value, true));

        Assert.True(map.TryMap(constValue, BindingTypeUsage.Parameter, out BindingTypeProjection? projection));
        Assert.Equal("TM002", projection?.RuleId);
    }

    [Fact]
    public void MapsOcctHandleAsRetainedOpaqueSharedWrapper()
    {
        InitialTypeMap map = new();
        BindingType handle = new(
            "occ::handle<Geom_CartesianPoint>",
            "opencascade::handle<Geom_CartesianPoint>",
            "occ::handle<Geom_CartesianPoint>",
            "opencascade::handle<Geom_CartesianPoint>",
            [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
            "opencascade::handle",
            [new BindingTemplateArgument("Type", "Geom_CartesianPoint")],
            true,
            "Geom_CartesianPoint");

        Assert.True(map.TryMap(handle, BindingTypeUsage.ReturnValue, out BindingTypeProjection? projection));
        Assert.Equal("TM006", projection?.RuleId);
        Assert.Equal("OcctSharp_TransientHandle*", projection?.AbiType);
        Assert.Equal("GeomCartesianPoint", projection?.ManagedFriendlyType);
        Assert.Equal("Shared", projection?.Ownership);
    }

    private static BindingType CreateValueType(string nativeType, string canonicalType)
    {
        return CreateType(
            nativeType,
            canonicalType,
            nativeType,
            canonicalType,
            new BindingTypeLayer(BindingTypeLayerKind.Value, false));
    }

    private static BindingType CreateType(
        string nativeType,
        string canonicalType,
        string baseNativeType,
        string baseCanonicalType,
        params BindingTypeLayer[] layers)
    {
        return new BindingType(
            nativeType,
            canonicalType,
            baseNativeType,
            baseCanonicalType,
            layers,
            null,
            [],
            false,
            null);
    }
}
