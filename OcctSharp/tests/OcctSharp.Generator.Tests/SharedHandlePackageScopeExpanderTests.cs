using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class SharedHandlePackageScopeExpanderTests
{
    [Fact]
    public void SuppressedPackageEmitsTransientScopesWithoutCreationExports()
    {
        BindingModel model = new(
        [
            Record("transient", "Standard_Transient"),
            Record("entity", "StepBasic_Entity", "Standard_Transient"),
            Constructor("entity-ctor", "StepBasic_Entity"),
        ]);
        SharedHandlePackageScopeConfiguration packageScope = new()
        {
            SourcePackage = "StepBasic",
            NativeTypePrefix = "StepBasic_",
            SuppressConstructors = true,
            ExcludedStableIds = ["missing-symbol"],
        };

        SharedHandleScopeConfiguration scope = Assert.Single(
            SharedHandlePackageScopeExpander.Expand(model, [], [packageScope]));

        Assert.Equal("StepBasic_Entity", scope.NativeType);
        Assert.True(scope.SuppressConstructors);
        Assert.Contains("missing-symbol", scope.ExcludedStableIds);
    }

    [Fact]
    public void ExpandsTransientTypesWithValueOrIntrusiveHandleConstructorsDeterministically()
    {
        BindingModel model = new(
        [
            Record("transient", "Standard_Transient"),
            Record("eligible-record", "StepBasic_Eligible", "Standard_Transient"),
            Record("abstract-record", "StepBasic_Abstract", "Standard_Transient", isAbstract: true),
            Record("excluded-record", "StepBasic_Excluded", "Standard_Transient"),
            Record("handle-only-record", "StepBasic_HandleOnly", "Standard_Transient"),
            Record("dependency-record", "StepBasic_Dependency", "Standard_Transient"),
            Record("value-record", "StepBasic_Value"),
            Constructor("eligible-ctor", "StepBasic_Eligible"),
            Constructor("abstract-ctor", "StepBasic_Abstract"),
            Constructor("excluded-ctor", "StepBasic_Excluded"),
            Constructor("handle-only-ctor", "StepBasic_HandleOnly", Handle("StepBasic_Dependency")),
            Constructor("dependency-ctor", "StepBasic_Dependency"),
            Constructor("value-ctor", "StepBasic_Value"),
        ]);
        SharedHandlePackageScopeConfiguration packageScope = new()
        {
            SourcePackage = "StepBasic",
            NativeTypePrefix = "StepBasic_",
            ExcludedNativeTypes = ["StepBasic_Excluded"],
            PlacementAllocatorNativeTypes = ["StepBasic_Eligible"],
        };

        IReadOnlyList<SharedHandleScopeConfiguration> result =
            SharedHandlePackageScopeExpander.Expand(model, [], [packageScope]);

        Assert.Equal(4, result.Count);
        Assert.Contains(result, static scope => scope.NativeType == "StepBasic_HandleOnly");
        Assert.Contains(result, static scope => scope.NativeType == "StepBasic_Abstract" && scope.SuppressConstructors);
        SharedHandleScopeConfiguration scope = Assert.Single(
            result, static item => item.NativeType == "StepBasic_Eligible");
        Assert.Equal("StepBasic_Eligible", scope.NativeType);
        Assert.Equal("StepBasic_Eligible.hxx", scope.Header);
        Assert.Equal("step_basic_eligible", scope.ExportNamePrefix);
        Assert.Equal("StepBasicEligible", scope.ManagedTypeName);
        Assert.True(scope.UsesPlacementAllocator);
    }

    [Fact]
    public void ExactConstructorExclusionKeepsTypeButSuppressesCreation()
    {
        BindingModel model = new(
        [
            Record("transient", "Standard_Transient"),
            Record("entity", "StepBasic_Entity", "Standard_Transient"),
            Constructor("missing-constructor", "StepBasic_Entity"),
        ]);
        SharedHandlePackageScopeConfiguration packageScope = new()
        {
            SourcePackage = "StepBasic",
            NativeTypePrefix = "StepBasic_",
            ExcludedStableIds = ["missing-constructor"],
        };

        SharedHandleScopeConfiguration scope = Assert.Single(
            SharedHandlePackageScopeExpander.Expand(model, [], [packageScope]));

        Assert.Equal("StepBasic_Entity", scope.NativeType);
        Assert.True(scope.SuppressConstructors);
        Assert.Contains("missing-constructor", scope.ExcludedStableIds);
    }

    private static BindingDeclaration Record(
        string id,
        string name,
        string? baseType = null,
        bool isAbstract = false) =>
        new(id, name, BindingDeclarationKind.Record, name + ".hxx", 1, 1)
        {
            Access = BindingAccess.Public,
            SourcePackage = "StepBasic",
            IsAbstract = isAbstract,
            BaseTypes = baseType is null
                ? []
                : [new BindingBaseType(Value(baseType), BindingAccess.Public, false)],
        };

    private static BindingDeclaration Constructor(
        string id,
        string nativeType,
        params BindingType[] parameterTypes) =>
        new(id, nativeType + "::" + nativeType, BindingDeclarationKind.Constructor, nativeType + ".hxx", 2, 1)
        {
            Access = BindingAccess.Public,
            SourcePackage = "StepBasic",
            Parameters = parameterTypes
                .Select((type, index) => new BindingParameter(index, $"arg{index}", type, false))
                .ToArray(),
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

    private static BindingType Handle(string targetType) => new(
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
        "opencascade::handle",
        [],
        true,
        targetType);
}
