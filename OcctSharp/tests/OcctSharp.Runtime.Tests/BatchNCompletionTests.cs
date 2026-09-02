using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchNCompletionTests
{
    [Fact]
    public void IgesXdeReadWritePreservesMetadataAndCopiesDiagnostics()
    {
        Assert.Equal(32, Marshal.SizeOf<OcctSharp.Interop.XdeIgesReadReportRaw>());
        string directory = CreateDirectory();
        try
        {
            string iges = Path.Combine(directory, "batch-n-metadata.iges");
            using Shape box = ShapeFactory.CreateBox(10, 20, 30);
            using (XdeDocument source = XdeDocument.Create())
            {
                using XdeTransaction transaction = source.BeginTransaction("Batch N metadata");
                XdeLabel part = source.AddShape(box, "Batch N colored part");
                part.Color = new XdeColor(0.15, 0.55, 0.85, 1);
                part.SetLayer("Batch N Layer");
                Assert.True(transaction.Commit());
                Assert.Equal(iges, source.WriteIges(iges));
            }

            using XdeDocument restored = XdeDocument.ReadIges(
                iges,
                new XdeIgesReadOptions(ReadNames: true, ReadColors: true, ReadLayers: true),
                out XdeIgesReadReport report);
            XdeLabel root = Assert.Single(restored.GetFreeShapes());
            Assert.True(report.SourceEntityCount > 0);
            Assert.True(report.CandidateRootCount > 0);
            Assert.Equal(1, report.TransferredRootCount);
            Assert.True(report.SourceLengthUnitMeters > 0);
            Assert.True(report.SystemLengthUnitMillimeters > 0);
            Assert.Contains("Batch N", root.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            IReadOnlyList<XdePresentationStyle> styles = root.GetPresentationStyles();
            try { Assert.Contains(styles, style => style.EffectiveColor is not null); }
            finally { foreach (XdePresentationStyle style in styles) style.Dispose(); }
            using (DocumentSnapshot snapshot = restored.CreateSnapshot())
            {
                Assert.Contains(
                    snapshot.Labels.SelectMany(label => restored.GetLabel(label.Entry).Layers),
                    layer => !string.IsNullOrWhiteSpace(layer));
            }
            using Shape restoredShape = root.Shape;
            Assert.True(restoredShape.IsValid);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void IgesMetadataOptionsAndGeometryRoundTripRemainIndependent()
    {
        string directory = CreateDirectory();
        try
        {
            string iges = Path.Combine(directory, "batch-n-options.igs");
            using Shape sourceShape = ShapeFactory.CreateCylinder(5, 12);
            using (XdeDocument source = XdeDocument.Create())
            {
                using XdeTransaction transaction = source.BeginTransaction();
                XdeLabel part = source.AddShape(sourceShape, "Filtered metadata");
                part.Color = new XdeColor(0.8, 0.25, 0.1, 1);
                part.SetLayer("Filtered layer");
                Assert.True(transaction.Commit());
                source.WriteIges(iges, new XdeIgesWriteOptions(
                    WriteNames: false,
                    WriteColors: false,
                    WriteLayers: false));
            }

            using XdeDocument restored = XdeDocument.ReadIges(
                iges,
                new XdeIgesReadOptions(ReadNames: false, ReadColors: false, ReadLayers: false),
                out XdeIgesReadReport report);
            XdeLabel root = Assert.Single(restored.GetFreeShapes());
            using Shape shape = root.Shape;
            Assert.True(shape.IsValid);
            Assert.True(report.SourceEntityCount > 0);
            using (DocumentSnapshot snapshot = restored.CreateSnapshot())
            {
                Assert.Empty(snapshot.Labels.SelectMany(label => restored.GetLabel(label.Entry).Layers));
            }
            IReadOnlyList<XdePresentationStyle> styles = root.GetPresentationStyles();
            try { Assert.DoesNotContain(styles, style => style.EffectiveColor is not null); }
            finally { foreach (XdePresentationStyle style in styles) style.Dispose(); }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void UnicodeIgesInputOutputAndFailureCleanTemporaryStages()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchN.颜色.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        HashSet<string> before = TemporaryStages();
        try
        {
            string unicodeIges = Path.Combine(directory, "零件-蓝色.iges");
            using Shape box = ShapeFactory.CreateBox(4, 5, 6);
            using (XdeDocument source = XdeDocument.Create())
            {
                using XdeTransaction transaction = source.BeginTransaction();
                _ = source.AddShape(box, "Unicode IGES");
                Assert.True(transaction.Commit());
                Assert.Equal(unicodeIges, source.WriteExchange(unicodeIges));
            }
            Assert.True(new FileInfo(unicodeIges).Length > 0);
            using XdeDocument restored = XdeDocument.ReadExchange(unicodeIges);
            Assert.Single(restored.GetFreeShapes());

            string invalid = Path.Combine(directory, "损坏.iges");
            File.WriteAllText(invalid, "not an IGES file");
            Assert.Throws<OcctException>(() => XdeDocument.ReadIges(invalid));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
            Assert.True(TemporaryStages().IsSubsetOf(before));
        }
    }

    [Fact]
    public void MixedExchangeImportLifetimeAndRealHwndViewerFormOneClosure()
    {
        string directory = CreateDirectory();
        nint window = CreateTestWindow();
        try
        {
            string step = Path.Combine(directory, "batch-n.step");
            string iges = Path.Combine(directory, "batch-n.iges");
            using Shape stepShape = ShapeFactory.CreateBox(8, 8, 8);
            using Shape igesShape = ShapeFactory.CreateCylinder(3, 10);
            ShapeExchange.WriteStep(stepShape, step);
            using (XdeDocument igesSource = XdeDocument.Create())
            {
                using XdeTransaction transaction = igesSource.BeginTransaction();
                XdeLabel part = igesSource.AddShape(igesShape, "IGES blue part");
                part.Color = new XdeColor(0.1, 0.35, 0.9, 1);
                Assert.True(transaction.Commit());
                igesSource.WriteExchange(iges, XdeExchangeFormat.Iges);
            }

            using XdeDocument assembly = XdeDocument.Create();
            XdeLabel root;
            using (XdeTransaction transaction = assembly.BeginTransaction("Batch N mixed import"))
            {
                XdeLabel stepPart = Assert.Single(assembly.ImportExchange(step, XdeExchangeFormat.Step));
                XdeLabel igesPart = Assert.Single(assembly.ImportExchange(iges));
                root = assembly.AddAssembly("Batch N mixed assembly");
                using TopLocLocation first = Location(0, 0, 0);
                using TopLocLocation second = Location(20, 0, 0);
                _ = assembly.AddComponent(root, stepPart, first);
                _ = assembly.AddComponent(root, igesPart, second);
                Assert.True(transaction.Commit());
            }

            using Shape assembledShape = root.Shape;
            Assert.True(assembledShape.IsValid);
            using OcctViewer viewer = OcctViewer.Create(window);
            using ViewerPresentation presentation = viewer.Display(root);
            viewer.FitAll();
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-n.png"));
            Assert.True(new FileInfo(screenshot).Length > 0);
            string roundTrip = assembly.WriteExchange(Path.Combine(directory, "batch-n-roundtrip.igs"));
            using XdeDocument restored = XdeDocument.ReadExchange(roundTrip, XdeExchangeFormat.Iges);
            Assert.NotEmpty(restored.GetFreeShapes());
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchN.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static HashSet<string> TemporaryStages() =>
        Directory.EnumerateFiles(Path.GetTempPath(), "occtsharp-exchange-*.tmp")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static TopLocLocation Location(double x, double y, double z)
    {
        using GpTrsf transform = GpTrsf.Create(x, y, z);
        return TopLocLocation.FromTransform(transform);
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch N", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
