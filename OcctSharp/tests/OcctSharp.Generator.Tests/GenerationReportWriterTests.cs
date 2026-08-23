using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Reporting;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class GenerationReportWriterTests
{
    [Fact]
    public void ReportsAllStatesWithDeterministicPackageCoverageAndDiagnostics()
    {
        BindingType real = CreateValueType("double");
        BindingType point = CreateValueType("gp_Pnt");
        BindingModel classified = SupportClassificationPass.Apply(new BindingModel(
        [
            Create("pending", "Sample", BindingDeclarationKind.Record, "PackageB", "TKB"),
            Create("manual", "Manual", BindingDeclarationKind.Function, "PackageA", "TKA") with
            {
                SupportState = BindingSupportState.Manual,
            },
            Create("skipped", "Hidden", BindingDeclarationKind.Method, "PackageA", "TKA") with
            {
                Access = BindingAccess.Private,
            },
            Create("emitted", "gp_Pnt::gp_Pnt", BindingDeclarationKind.Constructor, "PackageA", "TKA") with
            {
                Access = BindingAccess.Public,
                Parameters = [new BindingParameter(0, "x", real, false)],
            },
            Create("eligible", "gp::Resolution", BindingDeclarationKind.Method, "PackageB", "TKB") with
            {
                Access = BindingAccess.Public,
                IsStatic = true,
                ReturnType = real,
            },
        ]));
        BindingModel model = SimpleBindingEligibilityPass.Apply(classified);
        DiscoveryReport discovery = new(
            "1.1",
            "test",
            "test",
            ["Sample.hxx"],
            [],
            model,
            BindingSupportSummary.Create(model));
        GeneratedBindingSet bindingSet = new("test", ["emitted"], []);

        GenerationReportSet first = GenerationReportWriter.Create(discovery, bindingSet);
        GenerationReportSet second = GenerationReportWriter.Create(discovery, bindingSet);

        Assert.Equal(first.Coverage.Totals, second.Coverage.Totals);
        Assert.Equal(
            first.Coverage.Packages.Select(static package => package.SourcePackage),
            second.Coverage.Packages.Select(static package => package.SourcePackage));
        Assert.Equal(
            first.Diagnostics.Declarations.Select(static item => item.StableId),
            second.Diagnostics.Declarations.Select(static item => item.StableId));
        Assert.Equal(5, first.Coverage.Totals.Total);
        Assert.Equal(1, first.Coverage.Totals.Pending);
        Assert.Equal(1, first.Coverage.Totals.Skipped);
        Assert.Equal(2, first.Coverage.Totals.Supported);
        Assert.Equal(1, first.Coverage.Totals.Manual);
        Assert.Equal(1, first.Coverage.Totals.Emitted);
        Assert.Equal(["PackageA", "PackageB"], first.Coverage.Packages.Select(static package => package.SourcePackage));
        Assert.Equal(["eligible", "emitted", "manual", "pending", "skipped"], first.Diagnostics.Declarations.Select(static item => item.StableId));
        Assert.Equal("EM001", Find(first, "emitted").Code);
        Assert.Equal("EL000", Find(first, "eligible").Code);
        Assert.Equal("MN001", Find(first, "manual").Code);
        Assert.Equal("EL001", Find(first, "pending").Code);
        Assert.Equal("SK003", Find(first, "skipped").Code);
    }

    [Fact]
    public void WritesByteStableReportsThroughStaging()
    {
        string root = Path.Combine(Path.GetTempPath(), $"occtsharp-report-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            DiscoveryReport discovery = new(
                "1.1",
                "test",
                "test",
                ["Sample.hxx"],
                [],
                new BindingModel([Create("pending", "Sample", BindingDeclarationKind.Record, "Package", "TK")]),
                new BindingSupportSummary(1, 1, 0, 0, 0, new Dictionary<string, int>()));
            GenerationReportSet reports = GenerationReportWriter.Create(
                discovery,
                new GeneratedBindingSet("test", [], []));

            GenerationReportWriter.Write(root, reports);
            string coveragePath = Path.Combine(root, "artifacts", "generator-reports", "coverage.json");
            string diagnosticsPath = Path.Combine(root, "artifacts", "generator-reports", "diagnostics.json");
            string firstCoverage = File.ReadAllText(coveragePath);
            string firstDiagnostics = File.ReadAllText(diagnosticsPath);

            GenerationReportWriter.Write(root, reports);

            Assert.Equal(firstCoverage, File.ReadAllText(coveragePath));
            Assert.Equal(firstDiagnostics, File.ReadAllText(diagnosticsPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static BindingDeclaration Create(
        string stableId,
        string nativeName,
        BindingDeclarationKind kind,
        string package,
        string toolkit)
    {
        return new BindingDeclaration(stableId, nativeName, kind, "Sample.hxx", 1, 1)
        {
            SourcePackage = package,
            SourceToolkit = toolkit,
        };
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

    private static GenerationDeclarationDiagnostic Find(GenerationReportSet reports, string stableId)
    {
        return Assert.Single(reports.Diagnostics.Declarations, item => item.StableId == stableId);
    }
}
