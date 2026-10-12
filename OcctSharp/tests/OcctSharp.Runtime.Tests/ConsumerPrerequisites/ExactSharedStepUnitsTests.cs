using System.Security.Cryptography;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ExactSharedStepUnitsTests
{
    private static readonly double[] LengthFactors = [1, 25.4, 1000];
    public static IEnumerable<object[]> SharedRoots()
    {
        foreach (bool circle in new[] { false, true })
        foreach (double system in new[] { 1d, 25.4, 1000d })
        foreach (bool reverse in new[] { false, true })
            yield return [circle, system, reverse];
    }

    [Theory]
    [MemberData(nameof(SharedRoots))]
    public void SharedActualRootsKeepUnitsOrderRepeatedTransferAndOwnedLifetime(bool circle, double system, bool reverse)
    {
        string file = Fixture(circle, reverse);
        byte[] before = SHA256.HashData(File.ReadAllBytes(file));
        var retained = new List<(Shape Shape, double Expected)>();
        try
        {
            using (var session = StepReadSession.Open(file, system))
            {
                var info = session.Info;
                Assert.Equal(3, info.CandidateRootCount);
                Assert.Equal(system, info.SystemLengthUnit);
                Assert.Equal(3, info.FileUnits.Length.Count);
                for (int repeat = 0; repeat < 2; repeat++)
                for (int root = 0; root < 3; root++)
                {
                    var shape = session.TransferRoot(root);
                    retained.Add((shape, Expected(circle, Factor(reverse, root) / system)));
                    Check(shape, retained[^1].Expected);
                    Assert.Same(info, session.Info);
                }
                Assert.Throws<ArgumentOutOfRangeException>(() => session.TransferRoot(3));
            }
            foreach (var item in retained) Check(item.Shape, item.Expected);
            Assert.Equal(before, SHA256.HashData(File.ReadAllBytes(file)));
        }
        finally { foreach (var item in retained) item.Shape.Dispose(); }
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void WholeFileAndReportedReadersKeepThreeStrictIndependentSolids(bool circle, bool reverse)
    {
        string file = Fixture(circle, reverse);
        byte[] before = SHA256.HashData(File.ReadAllBytes(file));
        using var shape = ShapeExchange.ReadStep(file);
        using var report = ShapeExchange.ReadStepWithReport(file);
        Assert.Equal(3, report.Report.CandidateRootCount);
        Assert.Equal(3, report.Report.TransferredRootCount);
        foreach (var owner in new[] { shape, report.Shape })
        {
            var solids = owner.GetSubShapes(ShapeKind.Solid);
            try
            {
                Assert.Equal(3, solids.Length);
                // Whole-file composition order is not a root-ordinal API contract.
                // Compare all three authored masses without inferring topology IDs.
                double[] expected = LengthFactors.Select(f => Expected(circle, f)).Order().ToArray();
                double[] actual = solids.Select(s => s.InspectAdaptiveProperties(InspectionPropertyKind.Volume, 1e-11).Properties.Mass).Order().ToArray();
                for (int i = 0; i < 3; i++)
                {
                    Assert.InRange(Math.Abs(actual[i] - expected[i]), 0, 1e-7);
                    Assert.True(solids[i].GetValidationReport(true, true).IsValid);
                }
            }
            finally { foreach (var solid in solids) solid.Dispose(); }
        }
        Assert.Equal(before, SHA256.HashData(File.ReadAllBytes(file)));
    }

    private static double Factor(bool reverse, int root) => (reverse ? 2 - root : root) switch { 0 => 1, 1 => 25.4, _ => 1000 };
    private static double Expected(bool circle, double scale) => (circle ? 2 * Math.PI : 2) * scale * scale * scale;
    private static string Fixture(bool circle, bool reverse) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "b18-shared-units", $"{(circle ? "circle" : "line")}-1.000000-{(reverse ? 1 : 0)}.step");
    private static void Check(Shape shape, double expected)
    {
        Assert.True(shape.GetValidationReport(true, true).IsValid);
        using var before = RepairSnapshot.Create(shape);
        var topology = shape.GetTopologySummary();
        var measured = shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume, 1e-11);
        Assert.InRange(Math.Abs(measured.Properties.Mass - expected), 0, 1e-7);
        Assert.InRange(measured.EstimatedRelativeError, 0, 1e-11);
        using var after = RepairSnapshot.Create(shape);
        Assert.Equal(before.Fingerprint, after.Fingerprint);
        Assert.Equal(topology, shape.GetTopologySummary());
    }
}
