using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class SharedHandleBindingEligibilityPassTests
{
    [Fact]
    public void PromotesVerifiedTransientConstructorsAndValueMethodsOnly()
    {
        BindingType real = Value("double");
        BindingType voidType = Value("void");
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
            Record("transient", "Standard_Transient"),
            Record("base", "Geom_Point", "Standard_Transient"),
            Record("derived", "Geom_CartesianPoint", "Geom_Point"),
            Declaration("ctor", "Geom_CartesianPoint::Geom_CartesianPoint", BindingDeclarationKind.Constructor) with
            {
                Access = BindingAccess.Public,
                Parameters = [new BindingParameter(0, "x", real, false)],
            },
            Declaration("getter", "Geom_CartesianPoint::X", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                IsConst = true,
                ReturnType = real,
            },
            Declaration("setter", "Geom_CartesianPoint::SetX", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                ReturnType = voidType,
                Parameters = [new BindingParameter(0, "x", real, false)],
            },
            Declaration("pointer", "Geom_CartesianPoint::Unsafe", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                ReturnType = pointer,
            },
            Declaration("destructor", "Geom_CartesianPoint::~Geom_CartesianPoint", BindingDeclarationKind.Method) with
            {
                Access = BindingAccess.Public,
                ReturnType = voidType,
            },
        ]));

        BindingModel result = SharedHandleBindingEligibilityPass.Apply(classified);

        Assert.Equal(BindingSupportState.Supported, Find(result, "ctor").SupportState);
        Assert.Equal(BindingSupportState.Supported, Find(result, "getter").SupportState);
        Assert.Equal(BindingSupportState.Supported, Find(result, "setter").SupportState);
        Assert.Equal(BindingSupportState.Pending, Find(result, "pointer").SupportState);
        Assert.Equal(BindingSupportState.Pending, Find(result, "destructor").SupportState);
    }

    private static BindingDeclaration Record(string id, string name, string? baseType = null) =>
        Declaration(id, name, BindingDeclarationKind.Record) with
        {
            Access = BindingAccess.Public,
            BaseTypes = baseType is null
                ? []
                : [new BindingBaseType(Value(baseType), BindingAccess.Public, false)],
        };

    private static BindingDeclaration Declaration(
        string id,
        string name,
        BindingDeclarationKind kind) =>
        new(id, name, kind, name.Split("::")[0] + ".hxx", 1, 1);

    private static BindingDeclaration Find(BindingModel model, string id) =>
        Assert.Single(model.Declarations, declaration => declaration.StableId == id);

    private static BindingType Value(string name) => new(
        name,
        name,
        name,
        name,
        [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
        null,
        [],
        false,
        null);
}
