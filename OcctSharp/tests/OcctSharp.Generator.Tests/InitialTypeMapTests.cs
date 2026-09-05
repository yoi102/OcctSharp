using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Tests;

public sealed class InitialTypeMapTests
{
    [Fact]
    public void ExactReverseModuleEdgeCannotBePromotedByEitherEligibilityPass()
    {
        BindingDeclaration method = new("c:@S@Adaptor3d_TopolTool@F@Classify#&1$@S@gp_Pnt2d#d#b#",
            "Adaptor3d_TopolTool::Classify", BindingDeclarationKind.Method, "Adaptor3d_TopolTool.hxx", 1, 1)
        { Access = BindingAccess.Public, ReturnType = CreateValueType("double", "double"), SupportState = BindingSupportState.Pending };
        var assessment = Transformation.SimpleBindingEligibilityPass.Assess(method, new InitialTypeMap());
        Assert.False(assessment.IsEligible);
        Assert.Equal("EL009", assessment.Code);
        BindingDeclaration owner = new("y:owner", "Adaptor3d_TopolTool", BindingDeclarationKind.Record, "Adaptor3d_TopolTool.hxx", 1, 1)
        { BaseTypes = [new(CreateValueType("Standard_Transient", "Standard_Transient"), BindingAccess.Public, false)] };
        BindingDeclaration unrelated = method with { StableId = "y:unrelated-overload" };
        BindingModel result = Transformation.SharedHandleBindingEligibilityPass.Apply(new BindingModel([owner, method, unrelated]));
        Assert.Equal(BindingSupportState.Pending, result.Declarations.Single(d => d.StableId == method.StableId).SupportState);
        Assert.Equal(BindingSupportState.Supported, result.Declarations.Single(d => d.StableId == unrelated.StableId).SupportState);
    }

    [Theory]
    [InlineData("gp_XY")]
    [InlineData("gp_XYZ")]
    [InlineData("gp_Pnt2d")]
    [InlineData("gp_Vec2d")]
    [InlineData("gp_Vec")]
    [InlineData("gp_Dir2d")]
    [InlineData("gp_Dir")]
    [InlineData("gp_Ax1")]
    [InlineData("gp_Ax2")]
    [InlineData("gp_Ax3")]
    [InlineData("gp_Ax2d")]
    [InlineData("gp_Ax22d")]
    [InlineData("gp_Mat2d")]
    [InlineData("gp_Mat")]
    [InlineData("gp_Quaternion")]
    [InlineData("gp_Lin2d")]
    [InlineData("gp_Lin")]
    [InlineData("gp_Circ2d")]
    [InlineData("gp_Circ")]
    [InlineData("gp_Elips2d")]
    [InlineData("gp_Elips")]
    [InlineData("gp_Hypr2d")]
    [InlineData("gp_Hypr")]
    [InlineData("gp_Parab2d")]
    [InlineData("gp_Parab")]
    [InlineData("gp_Pln")]
    [InlineData("gp_Cylinder")]
    [InlineData("gp_Cone")]
    [InlineData("gp_Sphere")]
    [InlineData("gp_Torus")]
    public void GeometryCopiesAcceptValuesAndConstReferencesOnly(string native)
    {
        InitialTypeMap map = new();
        foreach (BindingTypeUsage usage in new[] { BindingTypeUsage.Parameter, BindingTypeUsage.ReturnValue })
        {
            Assert.True(map.TryMap(CreateValueType(native, native), usage, out BindingTypeProjection? projection));
            Assert.Equal("TM009", projection?.RuleId);
            Assert.Equal("ValueCopy", projection?.Ownership);
            Assert.True(map.TryMap(CreateType("const " + native + " &", "const " + native + " &", native, native,
                new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, true)), usage, out _));
            Assert.False(map.TryMap(CreateType(native + " &", native + " &", native, native,
                new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, false)), usage, out _));
            Assert.False(map.TryMap(CreateType(native + " *", native + " *", native, native,
                new(BindingTypeLayerKind.PointerIndirection, false), new(BindingTypeLayerKind.Value, false)), usage, out _));
        }
    }

    [Fact]
    public void MappingGeometryInputsDoesNotClaimUnemittedValueConstructors()
    {
        BindingDeclaration constructor = new("y:vector-ctor", "gp_Vec::gp_Vec", BindingDeclarationKind.Constructor, "gp_Vec.hxx", 1, 1);
        Assert.False(Transformation.SimpleBindingEligibilityPass.Assess(constructor, new InitialTypeMap()).IsEligible);
        Assert.False(new InitialTypeMap().TryMap(CreateValueType("gp_Trsf", "gp_Trsf"), BindingTypeUsage.ReturnValue, out _));
        BindingDeclaration pointFromXyz = constructor with { NativeName = "gp_Pnt::gp_Pnt",
            Parameters = [new(0, "xyz", CreateValueType("gp_XYZ", "gp_XYZ"), false)] };
        Assert.Equal("EL010", Transformation.SimpleBindingEligibilityPass.Assess(pointFromXyz, new InitialTypeMap()).Code);
    }

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
    public void CopiesConstReferenceScalarsButRejectsPointers()
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
        Assert.True(map.TryMap(constReference, BindingTypeUsage.ReturnValue, out BindingTypeProjection? copied));
        Assert.Equal("ValueCopy", copied?.Ownership);
        Assert.False(map.TryMap(pointer, BindingTypeUsage.Parameter, out _));
    }

    [Theory]
    [InlineData("float", "float", "float")]
    [InlineData("signed char", "int8_t", "sbyte")]
    [InlineData("unsigned char", "uint8_t", "byte")]
    [InlineData("short", "int16_t", "short")]
    [InlineData("unsigned short", "uint16_t", "ushort")]
    [InlineData("unsigned int", "uint32_t", "uint")]
    [InlineData("long", "int32_t", "int")]
    [InlineData("unsigned long", "uint32_t", "uint")]
    [InlineData("long long", "int64_t", "long")]
    [InlineData("unsigned long long", "uint64_t", "ulong")]
    public void MapsNumericValuesAndConstReferenceCopies(string native, string abi, string managed)
    {
        InitialTypeMap map = new();
        foreach (BindingTypeUsage usage in new[] { BindingTypeUsage.Parameter, BindingTypeUsage.ReturnValue })
        {
            foreach (BindingType type in new[]
            {
                CreateValueType("ScalarAlias", native),
                CreateType("const ScalarAlias &", $"const {native} &", "ScalarAlias", native,
                    new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, true)),
            })
            {
                Assert.True(map.TryMap(type, usage, out BindingTypeProjection? projection));
                Assert.Equal("TM008", projection?.RuleId);
                Assert.Equal(abi, projection?.AbiType);
                Assert.Equal(managed, projection?.ManagedRawType);
            }
        }
    }

    [Theory]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("unsigned long long")]
    [InlineData("gp_Pnt")]
    public void RejectsMutableAndTransferReferences(string native)
    {
        InitialTypeMap map = new();
        foreach (BindingTypeLayerKind kind in new[] { BindingTypeLayerKind.LValueReference, BindingTypeLayerKind.RValueReference, BindingTypeLayerKind.PointerIndirection })
        {
            BindingType type = CreateType(native + " &", native + " &", native, native,
                new(kind, false), new(BindingTypeLayerKind.Value, false));
            Assert.False(map.TryMap(type, BindingTypeUsage.Parameter, out _));
            Assert.False(map.TryMap(type, BindingTypeUsage.ReturnValue, out _));
        }
        Assert.False(map.TryMap(CreateValueType("long double", "long double"), BindingTypeUsage.ReturnValue, out _));
        Assert.False(map.TryMap(CreateValueType("char", "char"), BindingTypeUsage.ReturnValue, out _));
    }

    [Fact]
    public void KeepsBorrowedTopologyReferencesUnmapped()
    {
        BindingType type = CreateType("const TopoDS_Shape &", "const TopoDS_Shape &", "TopoDS_Shape", "TopoDS_Shape",
            new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, true));
        Assert.False(new InitialTypeMap().TryMap(type, BindingTypeUsage.ReturnValue, out _));
    }

    [Fact]
    public void ArrayStartReferenceIsNotEligibleAsAScalarCopy()
    {
        BindingType type = CreateType("const double &", "const double &", "double", "double",
            new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, true));
        BindingDeclaration method = new("c:@S@BSplCLib@F@FlatBezierKnots#I#S", "BSplCLib::FlatBezierKnots",
            BindingDeclarationKind.Method, "BSplCLib.hxx", 2041, 1)
        { IsStatic = true, Access = BindingAccess.Public, ReturnType = type };
        Assert.True(new InitialTypeMap().TryMap(type, BindingTypeUsage.ReturnValue, out _));
        var assessment = Transformation.SimpleBindingEligibilityPass.Assess(method, new InitialTypeMap());
        Assert.False(assessment.IsEligible);
        Assert.Equal("EL008", assessment.Code);
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
