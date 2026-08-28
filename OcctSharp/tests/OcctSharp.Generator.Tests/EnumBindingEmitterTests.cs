using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class EnumBindingEmitterTests
{
    [Fact]
    public void EmitsReferencedEnumValuesWithManagedName()
    {
        BindingDeclaration enumDeclaration = new(
            "enum:Sample_Kind",
            "Sample_Kind",
            BindingDeclarationKind.Enum,
            "Sample_Kind.hxx",
            1,
            1)
        {
            ProductModule = OcctProductModule.Foundation,
            EnumUnderlyingType = "int",
            EnumValues =
            [
                new BindingEnumValue("Sample_First", "-1", false),
                new BindingEnumValue("Sample_Second", "4", false),
            ],
        };
        BindingDeclaration method = new(
            "method:Sample::Kind",
            "Sample::Kind",
            BindingDeclarationKind.Method,
            "Sample.hxx",
            2,
            1)
        {
            ReturnType = CreateEnumType("Sample_Kind"),
        };
        BindingModel model = new([enumDeclaration, method]);

        GeneratedBindingSet result = EnumBindingEmitter.Emit("test", model, [method.StableId]);

        Assert.Equal([enumDeclaration.StableId], result.SourceStableIds);
        GeneratedFile file = Assert.Single(result.Files);
        Assert.Equal("src/OcctSharp/Generated/Foundation/Foundation.Enums.Generated.cs", file.RelativePath);
        Assert.Contains("public enum SampleKind", file.Content, StringComparison.Ordinal);
        Assert.Contains("Sample_First = -1", file.Content, StringComparison.Ordinal);
        Assert.Contains("Sample_Second = 4", file.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsValuesOutsideVerifiedInt32Abi()
    {
        BindingDeclaration enumDeclaration = new(
            "enum:Huge",
            "Huge",
            BindingDeclarationKind.Enum,
            "Huge.hxx",
            1,
            1)
        {
            ProductModule = OcctProductModule.Foundation,
            EnumUnderlyingType = "unsigned long long",
            EnumValues = [new BindingEnumValue("TooLarge", "4294967295", true)],
        };
        BindingDeclaration method = new(
            "method:Sample::Huge",
            "Sample::Huge",
            BindingDeclarationKind.Method,
            "Sample.hxx",
            2,
            1)
        {
            ReturnType = CreateEnumType("Huge"),
        };

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            EnumBindingEmitter.Emit("test", new BindingModel([enumDeclaration, method]), [method.StableId]));

        Assert.Contains("outside the verified 32-bit ABI range", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitsAnExplicitlySelectedStandaloneEnum()
    {
        BindingDeclaration enumDeclaration = new(
            "enum:Standalone_Mode",
            "Standalone_Mode",
            BindingDeclarationKind.Enum,
            "Standalone_Mode.hxx",
            1,
            1)
        {
            ProductModule = OcctProductModule.Foundation,
            EnumUnderlyingType = "int",
            EnumValues = [new BindingEnumValue("Standalone_Off", "0", false)],
        };

        GeneratedBindingSet result = EnumBindingEmitter.Emit(
            "test",
            new BindingModel([enumDeclaration]),
            [enumDeclaration.StableId]);

        Assert.Equal([enumDeclaration.StableId], result.SourceStableIds);
        Assert.Contains("public enum StandaloneMode", Assert.Single(result.Files).Content, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsAnonymousEnumAsAStandaloneManagedType()
    {
        BindingDeclaration declaration = new(
            "enum:anonymous",
            "(unnamed enum at C:\\sdk\\Example.hxx:1:1)",
            BindingDeclarationKind.Enum,
            "Example.hxx",
            1,
            1)
        {
            EnumValues = [new BindingEnumValue("Example_Value", "0", false)],
        };

        Assert.False(EnumBindingEligibility.HasStableManagedTypeIdentity(declaration));
    }

    private static BindingType CreateEnumType(string name) => new(
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
