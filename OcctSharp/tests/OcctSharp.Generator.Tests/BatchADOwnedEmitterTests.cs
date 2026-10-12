using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class BatchADOwnedEmitterTests
{
    [Theory]
    [InlineData("HatchGen_PointOnHatching", "Point")]
    [InlineData("HatchGen_PointOnHatching", "RemPoint")]
    [InlineData("IGESConvGeom_GeomBuilder", "Point")]
    public void IndexedPointOperationsGuardRangeBeforeNativeAccess(string owner, string member)
    {
        bool remove = member == "RemPoint";
        bool iges = owner.StartsWith("IGES", StringComparison.Ordinal);
        string resultType = remove ? "void" : iges ? "gp_XYZ" : "HatchGen_PointOnElement";
        string signatureType = !remove && !iges ? "const HatchGen_PointOnElement &" : resultType;
        string signature = owner + "::" + member + ": " + signatureType
            + (signatureType.EndsWith('&') ? "" : " ") + "(const int)" + (remove ? "" : " const");
        OcctProductModule module = iges ? OcctProductModule.DataExchange : OcctProductModule.Geometry;
        BindingDeclaration method = Method("c:@S@" + owner + "@F@" + member + "#I#" + (remove ? "" : "1"),
            owner + "::" + member, signature,
            module, Type(resultType, !remove && !iges, !remove && !iges), !remove, Type("int", isConst: true));
        Assert.True(OwnedValueOperationContract.Contains(method));
        var result = Emit([Record(owner, module), Record("HatchGen_PointOnElement", OcctProductModule.Geometry), method]);
        string native = Native(result, module);
        const string guard = "if (local0 < 1 || local0 > self->NbPoints()) throw Standard_Failure";
        Assert.Contains(guard, native, StringComparison.Ordinal);
        Assert.True(native.IndexOf(guard, StringComparison.Ordinal) < native.IndexOf("self->" + member + "(local0)", StringComparison.Ordinal));
    }

    [Fact]
    public void HatchEndpointConstReferencePublishesAnIndependentOwner()
    {
        BindingDeclaration getter = Method("c:@S@HatchGen_Domain@F@FirstPoint#1", "HatchGen_Domain::FirstPoint",
            "HatchGen_Domain::FirstPoint: const HatchGen_PointOnHatching &() const", OcctProductModule.Geometry,
            Type("HatchGen_PointOnHatching", true, true), true);
        var result = Emit([Record("HatchGen_Domain", OcctProductModule.Geometry),
            Record("HatchGen_PointOnHatching", OcctProductModule.Geometry), getter]);
        string native = Native(result, OcctProductModule.Geometry);
        Assert.Contains("Lease<const HatchGen_Domain>(receiver)", native, StringComparison.Ordinal);
        Assert.Contains("std::make_shared<HatchGen_PointOnHatching>(self->FirstPoint())", native, StringComparison.Ordinal);
        Assert.Contains("Publish<OcctSharp_Owned_HatchGen_PointOnHatchingHandle>", native, StringComparison.Ordinal);
        Assert.Equal(getter.StableId, Assert.Single(result.SourceStableIds));
        Assert.False(OwnedValueOperationContract.Contains(getter with { NativeSignature = getter.NativeSignature + " changed" }));
    }

    [Fact]
    public void NearestPickCopiesAStaticReferenceResultWhileBothInputsAreLeased()
    {
        BindingDeclaration minimum = Method("c:@S@SelectBasics_PickResult@F@Min#&1$@S@SelectBasics_PickResult#S0_#S",
            "SelectBasics_PickResult::Min",
            "SelectBasics_PickResult::Min: const SelectBasics_PickResult &(const SelectBasics_PickResult &, const SelectBasics_PickResult &)",
            OcctProductModule.Visualization, Type("SelectBasics_PickResult", true, true), false,
            Type("SelectBasics_PickResult", true, true), Type("SelectBasics_PickResult", true, true)) with { IsStatic = true };
        var result = Emit([Record("SelectBasics_PickResult", OcctProductModule.Visualization), minimum]);
        string native = Native(result, OcctProductModule.Visualization);
        Assert.Contains("Lease<SelectBasics_PickResult>(arg0)", native, StringComparison.Ordinal);
        Assert.Contains("Lease<SelectBasics_PickResult>(arg1)", native, StringComparison.Ordinal);
        Assert.Contains("std::make_shared<SelectBasics_PickResult>(SelectBasics_PickResult::Min(*local0, *local1))", native, StringComparison.Ordinal);
        Assert.Equal(minimum.StableId, Assert.Single(result.SourceStableIds));
    }

    [Theory]
    [InlineData("c:@S@IGESData_DirChecker@F@IsSet#1")]
    [InlineData("c:@S@IntTools_Curve@F@D0#&1d#&$@S@gp_Pnt#1")]
    [InlineData("c:@S@IntTools_Curve@F@Type#1")]
    [InlineData("c:@S@Message_ExecStatus@F@IsSet#$@E@Message_Status#1")]
    [InlineData("c:@S@Message_ExecStatus@F@Set#$@E@Message_Status#")]
    [InlineData("c:@S@Message_ExecStatus@F@StatusByIndex#I#S")]
    public void UnauditedInitializationAndBitIndexPathsStayOutsideTheCatalogue(string stableId) =>
        Assert.DoesNotContain(stableId, OwnedValueOperationContract.StableIds);

    private static GeneratedBindingSet Emit(BindingDeclaration[] declarations) =>
        OwnedValueOperationEmitter.Emit("8.0.1", new BindingModel(declarations), [], new HashSet<string>(), []);

    private static string Native(GeneratedBindingSet result, OcctProductModule module) => result.Files.Single(file =>
        file.RelativePath.EndsWith(module + ".OwnedValues.Generated.cpp", StringComparison.Ordinal)).Content;

    private static BindingDeclaration Record(string native, OcctProductModule module) => new("record:" + native,
        native, BindingDeclarationKind.Record, native + ".hxx", 1, 1) { ProductModule = module };

    private static BindingDeclaration Method(string id, string native, string signature, OcctProductModule module,
        BindingType result, bool isConst, params BindingType[] parameters) => new(id, native,
        BindingDeclarationKind.Method, native.Split("::")[0] + ".hxx", 1, 1)
    {
        NativeSignature = signature, Access = BindingAccess.Public, ProductModule = module,
        ReturnType = result, IsConst = isConst,
        Parameters = parameters.Select((type, index) => new BindingParameter(index, "arg" + index, type, false)).ToArray(),
    };

    private static BindingType Type(string native, bool reference = false, bool isConst = false) => new(
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native, (isConst ? "const " : "") + native,
        reference ? [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, isConst)]
            : [new(BindingTypeLayerKind.Value, isConst)], null, [], false, null);
}
