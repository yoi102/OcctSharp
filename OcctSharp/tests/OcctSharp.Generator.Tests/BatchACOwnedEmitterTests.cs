using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class BatchACOwnedEmitterTests
{
    [Fact]
    public void NewConstReceiverSelectsConstNativeOverloadAndPreservesLegacyBodies()
    {
        BindingDeclaration matrixValue = Method("c:@S@math_Matrix@F@Value#I#I#1", "math_Matrix::Value",
            "math_Matrix::Value: const double &(const int, const int) const", OcctProductModule.Foundation,
            Type("double", reference: true, isConst: true), true, Type("int", isConst: true), Type("int", isConst: true));
        BindingDeclaration boxIsVoid = Method("c:@S@Bnd_Box@F@IsVoid#1", "Bnd_Box::IsVoid",
            "Bnd_Box::IsVoid: bool () const noexcept", OcctProductModule.Geometry, Type("bool"), true);
        Assert.True(OwnedValueOperationContract.Contains(matrixValue));
        Assert.True(OwnedValueOperationContract.Contains(boxIsVoid));
        var result = Emit([Record("math_Matrix", OcctProductModule.Foundation), matrixValue,
            Record("Bnd_Box", OcctProductModule.Geometry), boxIsVoid]);
        string foundation = Native(result, OcctProductModule.Foundation);
        string geometry = Native(result, OcctProductModule.Geometry);
        Assert.Contains("auto self = OcctSharpOwned::Lease<const math_Matrix>(receiver);", foundation, StringComparison.Ordinal);
        Assert.Contains("auto returned = self->Value(local0, local1);", foundation, StringComparison.Ordinal);
        Assert.DoesNotContain("Lease<math_Matrix>(receiver)", foundation, StringComparison.Ordinal);
        Assert.Contains("auto self = OcctSharpOwned::Lease<Bnd_Box>(receiver);", geometry, StringComparison.Ordinal);
        Assert.DoesNotContain("Lease<const Bnd_Box>", geometry, StringComparison.Ordinal);
        Assert.Equal(new[] { boxIsVoid.StableId, matrixValue.StableId }, result.SourceStableIds.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void InheritedParameterlessGetTypeExplicitlyHidesObjectMethod()
    {
        BindingDeclaration constructor = new("c:@S@ProjLib_Cone@F@ProjLib_Cone#&1$@S@gp_Cone#",
            "ProjLib_Cone::ProjLib_Cone", BindingDeclarationKind.Constructor, "ProjLib_Cone.hxx", 1, 1)
        {
            NativeSignature = "ProjLib_Cone::ProjLib_Cone: void (const gp_Cone &)",
            Access = BindingAccess.Public,
            ProductModule = OcctProductModule.Geometry,
            Parameters = [new(0, "cone", Type("gp_Cone", reference: true, isConst: true), false)],
        };
        BindingDeclaration getType = Method("c:@S@ProjLib_Projector@F@GetType#1", "ProjLib_Projector::GetType",
            "ProjLib_Projector::GetType: GeomAbs_CurveType () const", OcctProductModule.Geometry,
            Type("GeomAbs_CurveType"), true);
        var model = new BindingModel([Record("ProjLib_Cone", OcctProductModule.Geometry), constructor, getType,
            new("enum:GeomAbs_CurveType", "GeomAbs_CurveType", BindingDeclarationKind.Enum, "GeomAbs_CurveType.hxx", 1, 1)]);
        Assert.Contains(getType, OwnedValueOperationContract.SupportingOperations(model, "ProjLib_Cone"));
        var result = OwnedValueOperationEmitter.Emit("8.0.1", model, [], new HashSet<string>(), []);
        string managed = Managed(result, OcctProductModule.Geometry);
        Assert.Contains("public new GeomAbsCurveType GetType()", managed, StringComparison.Ordinal);
        Assert.DoesNotContain("public GeomAbsCurveType GetType()", managed, StringComparison.Ordinal);
        Assert.Equal(constructor.StableId, Assert.Single(result.SourceStableIds));
    }

    [Fact]
    public void TypedSelectSetterUsesOnlyOwnSelectedEntityGetterAndKeepsSyntheticIdsOutOfAccounting()
    {
        BindingDeclaration getter = SelectGetter();
        BindingDeclaration inheritedValue = Method("c:@S@StepData_SelectType@F@Value#1", "StepData_SelectType::Value",
            "StepData_SelectType::Value: const occ::handle<Standard_Transient> &() const", OcctProductModule.DataExchange,
            Shared("Standard_Transient", reference: true), true);
        BindingDeclaration inheritedType = Method("c:@S@StepData_SelectType@F@Type#1", "StepData_SelectType::Type",
            "StepData_SelectType::Type: occ::handle<Standard_Type> () const", OcctProductModule.DataExchange,
            Shared("Standard_Type"), true);
        BindingDeclaration otherOwnerGetter = Method("c:@S@StepAP203_ApprovedItem@F@Certification#1",
            "StepAP203_ApprovedItem::Certification",
            "StepAP203_ApprovedItem::Certification: occ::handle<StepBasic_Certification> () const",
            OcctProductModule.DataExchange, Shared("StepBasic_Certification"), true) with { SupportState = BindingSupportState.Manual };
        var result = EmitSelect(getter, SelectSetter(), additional: [inheritedValue, inheritedType, otherOwnerGetter]);
        string native = Native(result, OcctProductModule.DataExchange);
        string managed = Managed(result, OcctProductModule.DataExchange);
        const string synthetic = "support:owned-select-set:StepAP203_CertifiedItem:StepRepr_SuppliedPartRelationship";
        Assert.Contains("// Source: " + synthetic, native, StringComparison.Ordinal);
        Assert.Contains("public void SetValue(StepReprSuppliedPartRelationship arg0)", managed, StringComparison.Ordinal);
        Assert.Contains("ValidateStepReprSuppliedPartRelationship(arg0)->Value", native, StringComparison.Ordinal);
        Assert.Contains("self->SetValue(local0);", native, StringComparison.Ordinal);
        Assert.DoesNotContain("support:owned-select-set:StepAP203_CertifiedItem:Standard_Transient", native, StringComparison.Ordinal);
        Assert.DoesNotContain("support:owned-select-set:StepAP203_CertifiedItem:Standard_Type", native, StringComparison.Ordinal);
        Assert.DoesNotContain("support:owned-select-set:StepAP203_CertifiedItem:StepBasic_Certification", native, StringComparison.Ordinal);
        Assert.Equal(new[] { SelectConstructor().StableId, getter.StableId }.Order(StringComparer.Ordinal),
            result.SourceStableIds.Order(StringComparer.Ordinal));
        Assert.DoesNotContain(result.SourceStableIds, id => id.StartsWith("support:", StringComparison.Ordinal));
        Assert.DoesNotContain(SelectSetter().StableId, result.SourceStableIds);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("signature")]
    [InlineData("skipped")]
    [InlineData("unavailable")]
    public void TypedSelectSetterRequiresExactAvailableInheritedSetValue(string rejection)
    {
        BindingDeclaration? setter = rejection switch
        {
            "missing" => null,
            "signature" => SelectSetter() with { NativeSignature = "StepData_SelectType::SetValue: void (void *)" },
            "skipped" => SelectSetter() with { SupportState = BindingSupportState.Skipped },
            "unavailable" => SelectSetter() with { IsUnavailable = true },
            _ => throw new ArgumentOutOfRangeException(nameof(rejection)),
        };
        var result = EmitSelect(SelectGetter(), setter);
        Assert.DoesNotContain("support:owned-select-set:", Native(result, OcctProductModule.DataExchange), StringComparison.Ordinal);
        Assert.DoesNotContain("public void SetValue(StepReprSuppliedPartRelationship", Managed(result, OcctProductModule.DataExchange), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("manual")]
    [InlineData("skipped")]
    [InlineData("unavailable")]
    [InlineData("signature")]
    [InlineData("already-emitted")]
    public void GetterOutsideExactNewSelectionCannotCreateTypedSelectSetter(string rejection)
    {
        BindingDeclaration getter = SelectGetter();
        getter = rejection switch
        {
            "manual" => getter with { SupportState = BindingSupportState.Manual },
            "skipped" => getter with { SupportState = BindingSupportState.Skipped },
            "unavailable" => getter with { IsUnavailable = true },
            "signature" => getter with { NativeSignature = getter.NativeSignature + " changed" },
            "already-emitted" => getter,
            _ => throw new ArgumentOutOfRangeException(nameof(rejection)),
        };
        var result = EmitSelect(getter, SelectSetter(), alreadyEmitted: rejection == "already-emitted");
        Assert.DoesNotContain("support:owned-select-set:", Native(result, OcctProductModule.DataExchange), StringComparison.Ordinal);
        Assert.Equal(SelectConstructor().StableId, Assert.Single(result.SourceStableIds));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TypedSelectSetterRequiresInstanceParameterlessGetterMetadata(bool makeStatic)
    {
        // Retain exact catalogue identity while corrupting one structural property.
        // Setter synthesis must enforce its narrower shape even if a discovery model drifts.
        BindingDeclaration getter = makeStatic
            ? SelectGetter() with { IsStatic = true }
            : SelectGetter() with { Parameters = [new(0, "index", Type("int"), false)] };
        var result = EmitSelect(getter, SelectSetter());
        Assert.DoesNotContain("support:owned-select-set:", Native(result, OcctProductModule.DataExchange), StringComparison.Ordinal);
    }

    private static GeneratedBindingSet EmitSelect(BindingDeclaration getter, BindingDeclaration? setter,
        BindingDeclaration[]? additional = null, bool alreadyEmitted = false)
    {
        List<BindingDeclaration> declarations =
        [
            Record("StepAP203_CertifiedItem", OcctProductModule.DataExchange), SelectConstructor(), getter,
            Record("Standard_Transient", OcctProductModule.Foundation),
            Record("Standard_Type", OcctProductModule.Foundation),
            Record("StepRepr_SuppliedPartRelationship", OcctProductModule.DataExchange),
        ];
        if (setter is not null) declarations.Add(setter);
        declarations.AddRange(additional ?? []);
        return OwnedValueOperationEmitter.Emit("8.0.1", new BindingModel(declarations),
            [Scope("Standard_Transient", "StandardTransient"), Scope("Standard_Type", "StandardType"),
                Scope("StepRepr_SuppliedPartRelationship", "StepReprSuppliedPartRelationship")],
            alreadyEmitted ? new HashSet<string> { getter.StableId } : new HashSet<string>(), []);
    }

    private static BindingDeclaration SelectConstructor() => new("c:@S@StepAP203_CertifiedItem@F@StepAP203_CertifiedItem#",
        "StepAP203_CertifiedItem::StepAP203_CertifiedItem", BindingDeclarationKind.Constructor, "StepAP203_CertifiedItem.hxx", 1, 1)
    {
        NativeSignature = "StepAP203_CertifiedItem::StepAP203_CertifiedItem: void ()",
        Access = BindingAccess.Public,
        ProductModule = OcctProductModule.DataExchange,
    };

    private static BindingDeclaration SelectGetter() => Method("c:@S@StepAP203_CertifiedItem@F@SuppliedPartRelationship#1",
        "StepAP203_CertifiedItem::SuppliedPartRelationship",
        "StepAP203_CertifiedItem::SuppliedPartRelationship: occ::handle<StepRepr_SuppliedPartRelationship> () const",
        OcctProductModule.DataExchange, Shared("StepRepr_SuppliedPartRelationship"), true);

    private static BindingDeclaration SelectSetter() => Method(
        "c:@S@StepData_SelectType@F@SetValue#&1$@N@opencascade@S@handle>#$@S@Standard_Transient#",
        "StepData_SelectType::SetValue", "StepData_SelectType::SetValue: void (const occ::handle<Standard_Transient> &)",
        OcctProductModule.DataExchange, Type("void"), false, Shared("Standard_Transient", reference: true));

    private static GeneratedBindingSet Emit(BindingDeclaration[] declarations) =>
        OwnedValueOperationEmitter.Emit("8.0.1", new BindingModel(declarations), [], new HashSet<string>(), []);

    private static string Native(GeneratedBindingSet result, OcctProductModule module) => result.Files.Single(file =>
        file.RelativePath.EndsWith(module + ".OwnedValues.Generated.cpp", StringComparison.Ordinal)).Content;

    private static string Managed(GeneratedBindingSet result, OcctProductModule module) => result.Files.Single(file =>
        file.RelativePath.EndsWith(module + ".OwnedValues.Generated.cs", StringComparison.Ordinal)).Content;

    private static SharedHandleScopeConfiguration Scope(string native, string managed) => new()
    {
        NativeType = native, ManagedTypeName = managed, Header = native + ".hxx",
        ExportNamePrefix = native.ToLowerInvariant(), SuppressConstructors = true,
    };

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

    private static BindingType Shared(string native, bool reference = false) =>
        Type("opencascade::handle<" + native + ">", reference, reference) with { IsOcctHandle = true, HandleTargetType = native };

    private static BindingType Type(string native, bool reference = false, bool isConst = false) => new(
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native, (isConst ? "const " : "") + native,
        reference ? [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, isConst)]
            : [new(BindingTypeLayerKind.Value, isConst)], null, [], false, null);
}
