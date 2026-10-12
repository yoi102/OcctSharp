using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed partial class A5ViewerSourceTests
{
    [Fact]
    public void SourceCallerReleaseRetainsInstancesAndCopiedTransforms()
    {
        using var window = new Window();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        using var source = window.Viewer.CreateSource(shape);
        using var transform = GpTrsf.Create(40, 0, 0, rotationAngleRadians: .4);
        using var instance = window.Viewer.Display(source, transform);
        using var copied = instance.GetTransform();
        for (int row = 1; row <= 3; ++row)
            for (int column = 1; column <= 4; ++column)
                Assert.Equal(transform.Value(row, column), copied.Value(row, column));
        source.Dispose(); shape.Dispose(); transform.Dispose();
        instance.SetDisplayMode(ViewerDisplayMode.Shaded);
        window.Viewer.FitAll();
        Assert.NotEmpty(window.Viewer.Rendering.CaptureColor(new(128, 128)).CopyPixels());
        instance.Hide(); instance.Show();
        using var manipulator = instance.CreateManipulator();
        instance.ResetTransform();
        using var reset = instance.GetTransform();
        Assert.Equal(0, reset.Value(1, 4));
        Assert.Throws<ObjectDisposedException>(() => window.Viewer.Display(source, copied));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(40, .4)]
    [InlineData(80, -.2)]
    public void SelectedAndDetectedTopologyReturnsExactOriginalSourceMembers(double offset, double angle)
    {
        using var window = new Window();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        using var source = window.Viewer.CreateSource(shape);
        using var transform = GpTrsf.Create(offset, 0, 0, rotationAngleRadians: angle);
        using var instance = window.Viewer.Display(source, transform);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded);
        window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        foreach (ShapeKind? kind in new ShapeKind?[] { null, ShapeKind.Face, ShapeKind.Edge })
        {
            instance.SetSelectionKind(kind);
            GpPoint local = kind == ShapeKind.Edge ? new(10, 0, 5) : new(10, 5, 5);
            var world = new GpPoint(
                transform.Value(1,1) * local.X + transform.Value(1,2) * local.Y + transform.Value(1,3) * local.Z + transform.Value(1,4),
                transform.Value(2,1) * local.X + transform.Value(2,2) * local.Y + transform.Value(2,3) * local.Z + transform.Value(2,4),
                transform.Value(3,1) * local.X + transform.Value(3,2) * local.Y + transform.Value(3,3) * local.Z + transform.Value(3,4));
            var pixel = window.Viewer.WorldToScreen(world);
            Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
            using var detected = window.Viewer.GetDetectedItem();
            Assert.NotNull(detected); Assert.Same(instance, detected.Presentation);
            Assert.True(RepairSnapshot.FindTopologyPartnerIndex(shape, detected.Shape) >= 0);
            if (offset == 0 && angle == 0) Assert.True(RepairSnapshot.FindTopologyIndex(shape, detected.Shape) >= 0);
            var items = window.Viewer.GetSelectedItems();
            try
            {
                var selected = Assert.Single(items); Assert.Same(instance, selected.Presentation);
                Assert.True(RepairSnapshot.FindTopologyPartnerIndex(shape, selected.Shape) >= 0);
                if (offset == 0 && angle == 0) Assert.True(RepairSnapshot.FindTopologyIndex(shape, selected.Shape) >= 0);
                instance.SetColor(new(.1, .2, .9));
                Assert.Same(instance, Assert.Single(window.Viewer.GetSelection()));
            }
            finally { foreach (var item in items) item.Dispose(); }
            window.Viewer.ClearSelection();
        }
    }

    [Fact]
    public void IndependentAppearanceRestoresExactFrameAndDoesNotChangeOtherInstance()
    {
        using var window = new Window();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        using var source = window.Viewer.CreateSource(shape);
        using var identity = GpTrsf.Identity; using var shift = GpTrsf.Create(40, 0, 0);
        using var first = window.Viewer.Display(source, identity);
        using var second = window.Viewer.Display(source, shift);
        first.SetDisplayMode(ViewerDisplayMode.Shaded); second.SetDisplayMode(ViewerDisplayMode.Shaded);
        window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        var capture = new ViewerCaptureOptions(320, 320);
        var baseline = window.Viewer.Rendering.CaptureColor(capture).CopyPixels();
        using var texture = window.Viewer.Rendering.CreateTexture(BatchWRenderingTests.Image(8, 8, 0, 0, 255));
        window.Viewer.Rendering.SetAppearance(first,
            new() { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true } }, texture);
        var changed = window.Viewer.Rendering.CaptureColor(capture).CopyPixels();
        Assert.False(baseline.SequenceEqual(changed));
        var unaffected = window.Viewer.WorldToScreen(new(50, 5, 5));
        int start = 4 * (unaffected.Y * capture.Width + unaffected.X);
        Assert.Equal(baseline.AsSpan(start, 4).ToArray(), changed.AsSpan(start, 4).ToArray());
        window.Viewer.Rendering.ResetAppearance(first);
        Assert.Equal(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        first.Dispose(); second.SetColor(new(.7, .2, .1));
        Assert.NotEmpty(window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
    }

    [Fact]
    public void XdeCallerDocumentReleaseRetainsIndependentColorChannels()
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        XdeLabel label;
        using (var transaction = document.BeginTransaction())
        {
            label = document.AddShape(shape, "A5 exact source");
            label.SetPresentationColor(XdeColorChannel.Surface, new(.1, .2, .8, .65));
            label.SetPresentationColor(XdeColorChannel.Curve, new(.4, .25, .05));
            transaction.Commit();
        }
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        document.Dispose(); source.Dispose(); shape.Dispose();
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); window.Viewer.FitAll();
        var shaded = window.Viewer.Rendering.CaptureColor(new(160, 160)).CopyPixels();
        instance.SetDisplayMode(ViewerDisplayMode.Wireframe);
        var wire = window.Viewer.Rendering.CaptureColor(new(160, 160)).CopyPixels();
        Assert.False(shaded.SequenceEqual(wire));
    }

    [Fact]
    public void SourcesRejectForeignDisposedAndWrongThreadUseAndClearPreservesInstances()
    {
        using var first = new Window(); using var second = new Window();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        using var source = first.Viewer.CreateSource(shape); using var identity = GpTrsf.Identity;
        using var instance = first.Viewer.Display(source, identity);
        Assert.Throws<ArgumentException>(() => second.Viewer.Display(source, identity));
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { _ = first.Viewer.Display(source, identity); }
            catch (Exception exception) { error = exception; }
        });
        thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(error);
        first.Viewer.ClearSources();
        Assert.Throws<ObjectDisposedException>(() => first.Viewer.Display(source, identity));
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); first.Viewer.FitAll();
        Assert.NotEmpty(first.Viewer.Rendering.CaptureColor(new(64, 64)).CopyPixels());
        using var next = first.Viewer.CreateSource(shape);
        first.Viewer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => first.Viewer.Display(next, identity));
        next.Dispose();
    }

    [Fact]
    public void LoadedNativePayloadBelongsToThisCandidate()
    {
        using var window = new Window();
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "occt"));
        var names = Directory.EnumerateFiles(directory, "*.dll")
            .Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        using var process = Process.GetCurrentProcess();
        var loaded = process.Modules.Cast<ProcessModule>()
            .Where(module => names.Contains(module.ModuleName)).Select(module => new
            {
                name = module.ModuleName,
                path = Path.GetFullPath(module.FileName),
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName)))
            }).OrderBy(row => row.name, StringComparer.Ordinal).ToArray();
        Assert.Contains(loaded, row => row.name.Equals("OcctSharp.Native.dll", StringComparison.OrdinalIgnoreCase));
        Assert.All(loaded, row => Assert.Equal(directory, Path.GetDirectoryName(row.path), ignoreCase: true));
        var output = Environment.GetEnvironmentVariable("OCCTSHARP_A5_EVIDENCE_DIR");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            var path = Path.Combine(output, "loaded-native.json");
            Assert.False(File.Exists(path));
            File.WriteAllText(path, JsonSerializer.Serialize(loaded));
        }
    }

    private sealed class Window : IDisposable
    {
        private readonly nint _handle;
        public Window()
        {
            _handle = CreateWindowEx(0, "STATIC", "Cadoryx A5 SDK validation", 0x80000000u,
                -32000, -32000, 320, 320, 0, 0, 0, 0);
            Assert.NotEqual(0, _handle);
            Viewer = OcctViewer.Create(_handle);
        }
        public OcctViewer Viewer { get; }
        public void Dispose() { Viewer.Dispose(); _ = DestroyWindow(_handle); }
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        private static extern nint CreateWindowEx(uint extended, string className, string name,
            uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(nint window);
    }
}
