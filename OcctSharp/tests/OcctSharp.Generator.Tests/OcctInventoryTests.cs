using OcctSharp.Generator.Inventory;

namespace OcctSharp.Generator.Tests;

public sealed class OcctInventoryTests
{
    [Fact]
    public void CataloguesOnlyPublicEntryHeaderExtensionsDeterministically()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-inventory-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(Path.Combine(includeRoot, "Alpha.hxx"), string.Empty);
            File.WriteAllText(Path.Combine(includeRoot, "Beta_Item.hxx"), string.Empty);
            File.WriteAllText(Path.Combine(includeRoot, "CHeader.h"), string.Empty);
            File.WriteAllText(Path.Combine(includeRoot, "Ignored.lxx"), string.Empty);

            OcctInventoryReport report = OcctInventory.CreateCatalog(testRoot);

            Assert.Equal("test", report.OcctVersion);
            Assert.False(report.SemanticScan);
            Assert.False(report.IsComplete);
            Assert.Equal(4, report.Headers.Total);
            Assert.Equal(1, report.Headers.H);
            Assert.Equal(3, report.Headers.Hxx);
            Assert.Equal(0, report.Headers.Scanned);
            Assert.Null(report.Declarations);
            Assert.Equal(
                ["Alpha", "Beta", "CHeader", "Standard"],
                report.Headers.Packages.Select(static package => package.SourcePackage));
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void SemanticInventoryIsolatesInvalidHeadersAndKeepsSuccessfulDeclarations()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-inventory-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "Alpha.hxx"),
                "class Alpha { public: Alpha(double value); };\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "Broken.hxx"),
                "#error intentionally invalid inventory fixture\n");

            OcctInventoryReport report = OcctInventory.Discover(testRoot, batchSize: 3);

            Assert.True(report.SemanticScan);
            Assert.False(report.IsComplete);
            Assert.Equal(3, report.Headers.Total);
            Assert.Equal(2, report.Headers.Scanned);
            OcctInventoryFailure failure = Assert.Single(report.Failures);
            Assert.Equal("Broken.hxx", failure.Header);
            Assert.NotNull(report.Declarations);
            Assert.True(report.Declarations.Total >= 2);
            Assert.Contains(report.Declarations.Packages, package => package.SourcePackage == "Alpha");
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void SemanticInventoryAppliesConfiguredPreambleHeaders()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-inventory-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(Path.Combine(includeRoot, "Preamble.hxx"), "#pragma once\nclass RequiredType {};\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "Consumer.hxx"),
                "class Consumer { public: void Use(RequiredType value); };\n");

            OcctInventoryReport report = OcctInventory.Discover(
                testRoot,
                batchSize: 1,
                preambleHeaders: ["Preamble.hxx"]);

            Assert.True(report.IsComplete);
            Assert.Empty(report.Failures);
            Assert.Contains(
                report.Declarations!.Packages,
                package => package.SourcePackage == "Consumer");
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}
