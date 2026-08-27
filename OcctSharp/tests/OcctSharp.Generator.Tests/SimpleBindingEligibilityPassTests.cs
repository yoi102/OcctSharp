using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class SimpleBindingEligibilityPassTests
{
    [Fact]
    public void PromotesOnlyValueCopyConstructorsAndStaticMethods()
    {
        BindingType real = CreateValueType("double");
        BindingType point = CreateValueType("gp_Pnt");
        BindingType pointer = new(
            "double *",
            "double *",
            "double *",
            "double *",
            [
                new BindingTypeLayer(BindingTypeLayerKind.PointerIndirection, false),
                new BindingTypeLayer(BindingTypeLayerKind.Value, false),
            ],
            null,
            [],
            false,
            null);

        BindingModel classified = SupportClassificationPass.Apply(new BindingModel(
        [
            Create("ctor", "gp_Pnt::gp_Pnt", BindingDeclarationKind.Constructor) with
            {
                Access = BindingAccess.Public,
                Parameters = [new BindingParameter(0, "x", real, false)],
            },
            Create("static", "gp_Pnt::Origin", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                IsStatic = true,
                ReturnType = point,
            },
            Create("instance", "gp_Pnt::X", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                ReturnType = real,
            },
            Create("pointer", "gp_Pnt::FromPointer", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                IsStatic = true,
                ReturnType = point,
                Parameters = [new BindingParameter(0, "value", pointer, false)],
            },
            Create("private", "gp_Pnt::Private", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Private,
                IsStatic = true,
                ReturnType = point,
            },
        ]));

        BindingModel result = SimpleBindingEligibilityPass.Apply(classified);

        Assert.Equal(BindingSupportState.Supported, Find(result, "ctor").SupportState);
        Assert.Equal(BindingSupportState.Supported, Find(result, "static").SupportState);
        Assert.Equal(BindingSupportState.Pending, Find(result, "instance").SupportState);
        Assert.Equal(BindingSupportState.Pending, Find(result, "pointer").SupportState);
        Assert.Equal(BindingSupportState.Skipped, Find(result, "private").SupportState);
        Assert.Equal("SK003", Find(result, "private").SkipReason?.Code);
    }

    [Fact]
    public void LeavesTransientConstructorsForSharedOwnershipAnalysis()
    {
        BindingModel classified = SupportClassificationPass.Apply(new BindingModel(
        [
            Create("transient", "Standard_Transient", BindingDeclarationKind.Record),
            Create("record", "Example_Transient", BindingDeclarationKind.Record) with
            {
                BaseTypes = [new BindingBaseType(CreateValueType("Standard_Transient"), BindingAccess.Public, false)],
            },
            Create("ctor", "Example_Transient::Example_Transient", BindingDeclarationKind.Constructor) with
            {
                Access = BindingAccess.Public,
            },
        ]));

        BindingModel result = SimpleBindingEligibilityPass.Apply(classified);

        Assert.Equal(BindingSupportState.Pending, Find(result, "ctor").SupportState);
    }

    [Fact]
    public void SkipsMembersOwnedByNestedOrTemplatedRecords()
    {
        BindingModel classified = SupportClassificationPass.Apply(new BindingModel(
        [
            Create("nested-record", "Owner::Nested", BindingDeclarationKind.Record),
            Create("nested-method", "Owner::Nested::Value", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                IsStatic = true,
                ReturnType = CreateValueType("double"),
            },
        ]));

        BindingDeclaration method = Find(classified, "nested-method");
        Assert.Equal(BindingSupportState.Skipped, method.SupportState);
        Assert.Equal("SK011", method.SkipReason?.Code);
    }

    [Fact]
    public void PromotesExportTrackedFreeFunctionsIncludingVoidReturns()
    {
        BindingDeclaration scalar = Create("scalar", "IntegerFirst", BindingDeclarationKind.Function) with
        {
            SourcePackage = "Standard",
            SourceToolkit = "TKernel",
            Access = BindingAccess.Public,
            ReturnType = CreateValueType("int"),
        };
        BindingDeclaration noValue = Create("void", "Standard_ASSERT_DO_NOTHING", BindingDeclarationKind.Function) with
        {
            SourcePackage = "Standard",
            SourceToolkit = "TKernel",
            Access = BindingAccess.Public,
            ReturnType = CreateValueType("void"),
        };

        BindingModel result = SimpleBindingEligibilityPass.Apply(
            SupportClassificationPass.Apply(new BindingModel([scalar, noValue])));

        Assert.Equal(BindingSupportState.Supported, Find(result, "scalar").SupportState);
        Assert.Equal(BindingSupportState.Supported, Find(result, "void").SupportState);
    }

    private static BindingDeclaration Create(
        string stableId,
        string nativeName,
        BindingDeclarationKind kind)
    {
        return new BindingDeclaration(stableId, nativeName, kind, "gp_Pnt.hxx", 1, 1);
    }

    private static BindingDeclaration Find(BindingModel model, string stableId)
    {
        return Assert.Single(model.Declarations, declaration => declaration.StableId == stableId);
    }

    private static BindingType CreateValueType(string nativeType)
    {
        return new BindingType(
            nativeType,
            nativeType,
            nativeType,
            nativeType,
            [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
            null,
            [],
            false,
            null);
    }
}
