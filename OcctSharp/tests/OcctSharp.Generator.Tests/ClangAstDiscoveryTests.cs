using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class ClangAstDiscoveryTests
{
    [Fact]
    public void DiscoversDeclarationsFromSemanticCppAst()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-generator-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "Sample.hxx"),
                """
                namespace opencascade { template<class T> class handle {}; }
                enum class SampleKind { First = -2, Second = 7 };
                class SampleBase {};
                class Sample : public SampleBase {
                public:
                    Sample(double scale = 1.0);
                    virtual const int* Value(const double& input, int* const output) const = 0;
                    static opencascade::handle<SampleBase> Make();
                };
                """);

            DiscoveryReport report = ClangAstDiscovery.Discover(
                new DiscoveryOptions(
                    testRoot,
                    ["Sample.hxx"],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["Sample"] = "TKSample",
                    }));

            Assert.Equal("test", report.OcctVersion);
            Assert.Equal(report.Model.Declarations.Count, report.Support.Total);
            Assert.True(report.Support.Pending > 0);
            Assert.True(report.Support.Skipped > 0);
            BindingDeclaration sample = Assert.Single(report.Model.Declarations, declaration =>
                declaration is { NativeName: "Sample", Kind: BindingDeclarationKind.Record });
            Assert.Equal("Sample", sample.SourcePackage);
            Assert.Equal("TKSample", sample.SourceToolkit);
            BindingBaseType baseType = Assert.Single(sample.BaseTypes);
            Assert.Equal("SampleBase", baseType.Type.NativeSpelling);
            Assert.Equal(BindingAccess.Public, baseType.Access);

            BindingDeclaration constructor = Assert.Single(report.Model.Declarations, declaration =>
                declaration is { NativeName: "Sample::Sample", Kind: BindingDeclarationKind.Constructor });
            Assert.True(Assert.Single(constructor.Parameters).HasDefaultArgument);

            BindingDeclaration value = Assert.Single(report.Model.Declarations, declaration =>
                declaration is { NativeName: "Sample::Value", Kind: BindingDeclarationKind.Method });
            Assert.True(value.IsConst);
            Assert.True(value.IsVirtual);
            Assert.True(value.IsPureVirtual);
            Assert.Contains("Sample::Value", value.NativeSignature, StringComparison.Ordinal);
            Assert.NotNull(value.ReturnType);
            Assert.Equal(
                [BindingTypeLayerKind.PointerIndirection, BindingTypeLayerKind.Value],
                value.ReturnType.Layers.Select(static layer => layer.Kind));
            Assert.False(value.ReturnType.Layers[0].IsConstQualified);
            Assert.True(value.ReturnType.Layers[1].IsConstQualified);

            Assert.Equal(2, value.Parameters.Count);
            Assert.Equal(
                [BindingTypeLayerKind.LValueReference, BindingTypeLayerKind.Value],
                value.Parameters[0].Type.Layers.Select(static layer => layer.Kind));
            Assert.True(value.Parameters[0].Type.Layers[1].IsConstQualified);
            Assert.True(value.Parameters[1].Type.Layers[0].IsConstQualified);

            BindingDeclaration make = Assert.Single(report.Model.Declarations, declaration =>
                declaration is { NativeName: "Sample::Make", Kind: BindingDeclarationKind.Method });
            Assert.True(make.IsStatic);
            Assert.NotNull(make.ReturnType);
            Assert.True(make.ReturnType.IsOcctHandle);
            Assert.Equal("SampleBase", make.ReturnType.HandleTargetType);

            BindingDeclaration sampleKind = Assert.Single(report.Model.Declarations, declaration =>
                declaration is { NativeName: "SampleKind", Kind: BindingDeclarationKind.Enum });
            Assert.Equal("int", sampleKind.EnumUnderlyingType);
            Assert.Equal(
                [new BindingEnumValue("First", "-2", false), new BindingEnumValue("Second", "7", false)],
                sampleKind.EnumValues);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}
