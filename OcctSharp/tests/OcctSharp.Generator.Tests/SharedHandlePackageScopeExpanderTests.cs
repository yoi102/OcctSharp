using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class SharedHandlePackageScopeExpanderTests
{
    [Fact]
    public void ExpandsOnlyTransientTypesWithSafeConstructorsDeterministically()
    {
        BindingModel model = new(
        [
            Record("transient", "Standard_Transient"),
            Record("eligible-record", "StepBasic_Eligible", "Standard_Transient"),
            Record("excluded-record", "StepBasic_Excluded", "Standard_Transient"),
            Record("value-record", "StepBasic_Value"),
            Constructor("eligible-ctor", "StepBasic_Eligible"),
            Constructor("excluded-ctor", "StepBasic_Excluded"),
            Constructor("value-ctor", "StepBasic_Value"),
        ]);
        SharedHandlePackageScopeConfiguration packageScope = new()
        {
            SourcePackage = "StepBasic",
            NativeTypePrefix = "StepBasic_",
            ExcludedNativeTypes = ["StepBasic_Excluded"],
        };

        IReadOnlyList<SharedHandleScopeConfiguration> result =
            SharedHandlePackageScopeExpander.Expand(model, [], [packageScope]);

        SharedHandleScopeConfiguration scope = Assert.Single(result);
        Assert.Equal("StepBasic_Eligible", scope.NativeType);
        Assert.Equal("StepBasic_Eligible.hxx", scope.Header);
        Assert.Equal("step_basic_eligible", scope.ExportNamePrefix);
        Assert.Equal("StepBasicEligible", scope.ManagedTypeName);
    }

    private static BindingDeclaration Record(string id, string name, string? baseType = null) =>
        new(id, name, BindingDeclarationKind.Record, name + ".hxx", 1, 1)
        {
            Access = BindingAccess.Public,
            SourcePackage = "StepBasic",
            BaseTypes = baseType is null
                ? []
                : [new BindingBaseType(Value(baseType), BindingAccess.Public, false)],
        };

    private static BindingDeclaration Constructor(string id, string nativeType) =>
        new(id, nativeType + "::" + nativeType, BindingDeclarationKind.Constructor, nativeType + ".hxx", 2, 1)
        {
            Access = BindingAccess.Public,
            SourcePackage = "StepBasic",
        };

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
