using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchWRenderingTests
{
    [Fact]
    public void DriverProfilesAndFourLightKindsHaveAtomicPortableState()
    {
        using var window = new Window(); var render = window.Viewer.Rendering;
        var caps = render.GetCapabilities(); Assert.True(caps.MaximumTextureSize >= 64); Assert.True(caps.MaximumLights >= 4);
        Assert.NotEmpty(render.SnapshotLightRig()); var original = render.GetProfile();
        var effective = render.SetProfile(new() { MsaaSamples = caps.MaximumMsaaSamples >= 2 ? 2 : 0 }); Assert.Equal(1, effective.ResolutionScale);
        Assert.Throws<ArgumentException>(() => render.SetProfile(effective with { MsaaSamples = 3 })); Assert.Equal(effective, render.GetProfile());
        Assert.Throws<ArgumentException>(() => render.SetProfile(effective with { Exposure = 1 })); Assert.Equal(effective, render.GetProfile());
        render.SetProfile(original);
        var definitions = new[] {
            new ViewerLightDefinition(ViewerLightKind.Ambient, new(.2,.2,.2)),
            new ViewerLightDefinition(ViewerLightKind.Directional, new(1,1,1)) { Direction = new(0,0,-4), Headlight = true },
            new ViewerLightDefinition(ViewerLightKind.Positional, new(1,0,0)) { Position = new(5,5,20), Range = 100 },
            new ViewerLightDefinition(ViewerLightKind.Spot, new(0,0,1)) { Position = new(5,5,20), Direction = new(0,0,-1), SpotAngle = .6 }
        };
        var lights = render.ReplaceLightRig(definitions); Assert.Equal(4, lights.Count); Assert.Equal(-1, lights[1].Definition.Direction.Z);
        var before = render.SnapshotLightRig().ToArray();
        Assert.Throws<ArgumentException>(() => render.ReplaceLightRig([definitions[0] with { Headlight = true }]));
        Assert.Equal(before, render.SnapshotLightRig());
        Assert.Throws<ArgumentException>(() => render.ReplaceLightRig([definitions[0], definitions[3] with { SpotAngle = 4 }])); Assert.Equal(before, render.SnapshotLightRig());
        lights[2].Update(lights[2].Definition with { Active = false }); Assert.False(lights[2].Definition.Active);
        var copied = render.SnapshotLightRig(); var restored = render.ReplaceLightRig(copied); Assert.Equal(copied, render.SnapshotLightRig());
        Assert.Throws<ObjectDisposedException>(() => lights[0].Update(definitions[0])); Assert.Equal(4, restored.Count);
        restored[0].Dispose(); Assert.Equal(3, render.SnapshotLightRig().Count);
    }

    [Fact]
    public void ColorDepthSizedTiledFramesAreIndependentAndRestoreView()
    {
        using var window = new Window(); var viewer = window.Viewer; using var box = ShapeFactory.CreateBox(10, 10, 10);
        using var presentation = viewer.Display(box); presentation.SetDisplayMode(ViewerDisplayMode.Shaded); viewer.SetProjection(ViewerProjection.Top); viewer.FitAll();
        var camera = viewer.GetCamera(); var render = viewer.Rendering;
        viewer.SaveScreenshot(Path.Combine(Path.GetTempPath(), "batch-w-legacy.png"), overwrite: true);
        var color = render.CaptureColor(new(192, 160)); Assert.Equal(192 * 160 * 4, color.CopyPixels().Length);
        var depth = render.CaptureDepth(new(192, 160)); Assert.Contains(depth.CopyDepths(), d => d >= 0 && d < 1);
        Assert.True(depth.TryReconstruct(96, 80, out var point)); Assert.InRange(point.X, 0, 10); Assert.InRange(point.Y, 0, 10); Assert.InRange(point.Z, 9.99, 10.01);
        Assert.Equal(camera, viewer.GetCamera());
        var tiled = render.CaptureColor(new(192, 160, 64)); Assert.Equal(color.CopyPixels().Length, tiled.CopyPixels().Length);
        var a = color.CopyOpaqueBgra(); var b = tiled.CopyOpaqueBgra(); double error = a.Zip(b, (x, y) => Math.Abs(x - y)).Average(); Assert.InRange(error, 0, 8);
        var pixels = color.CopyPixels(); byte original = pixels[0]; pixels[0] ^= 255; Assert.Equal(original, color.CopyPixels()[0]);
        viewer.Zoom(2); Assert.True(depth.TryReconstruct(96, 80, out var again)); Assert.Equal(point, again);
        viewer.Dispose(); Assert.True(depth.TryReconstruct(96, 80, out again)); Assert.Equal(point, again); Assert.NotEmpty(color.CopyPixels());
    }

    [Fact]
    public void OwnedTextureReplacementAppearanceResetAndEnvironmentLifetimeRender()
    {
        using var window = new Window(); var viewer = window.Viewer; var render = viewer.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = viewer.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded); viewer.FitAll();
        var capture = new ViewerCaptureOptions(128, 128);
        var baseline = render.CaptureColor(capture).CopyPixels();
        using var texture = render.CreateTexture(Image(16, 16, 255, 0, 0));
        var appearance = new ViewerAppearanceProfile { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true, ScaleS = .1, ScaleT = .1 } };
        render.SetAppearance(p, appearance, texture); var red = render.CaptureColor(capture).CopyPixels(); Assert.False(baseline.SequenceEqual(red));
        texture.Replace(Image(16, 16, 0, 0, 255)); var blue = render.CaptureColor(capture).CopyPixels(); Assert.False(red.SequenceEqual(blue));
        Assert.Same(texture, render.GetAppearance(p)!.Texture);
        Assert.Throws<ArgumentException>(() => render.SetAppearance(p, appearance with { AlphaCutoff = 2 }, texture)); Assert.Equal(appearance, render.GetAppearance(p)!.Profile);
        texture.Dispose(); Assert.Null(render.GetAppearance(p)!.Texture); render.ResetAppearance(p);
        var reset = render.CaptureColor(capture).CopyPixels(); Assert.Equal(baseline, reset);
        var faces = Enumerable.Range(0, 6).Select(i => render.CreateTexture(Image(8, 8, (byte)(20 + i * 30), 40, 100))).ToArray();
        using var environment = render.CreateEnvironment(faces); foreach (var face in faces) face.Dispose();
        render.SetProfile(new() { Shading = ViewerShading.Pbr }); render.SetEnvironment(environment, true, render.GetCapabilities().SupportsPbr);
        Assert.NotEmpty(render.CaptureColor(new(64, 64)).CopyPixels()); render.SetEnvironment(environment, false, false);
        render.SetEnvironment(null); environment.Dispose();
    }

    [Fact]
    public void ReviewLayersAndResourcesRejectForeignDisposedAndWrongThreadUse()
    {
        using var first = new Window(); using var second = new Window();
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = first.Viewer.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded); first.Viewer.FitAll();
        var render = first.Viewer.Rendering; using var texture = second.Viewer.Rendering.CreateTexture(Image(2, 2, 255, 255, 255));
        Assert.Throws<ArgumentException>(() => render.SetAppearance(p, new(), texture));
        using var layer = render.CreateLayer(); render.AssignLayer(p, layer);
        var color = render.CaptureColor(new(64, 64, Layer: layer, SingleLayer: true)); Assert.NotEmpty(color.CopyPixels());
        Assert.Throws<ArgumentException>(() => second.Viewer.Rendering.CaptureColor(new(64, 64, Layer: layer, SingleLayer: true)));
        Exception? caught = null; var thread = new Thread(() => { try { _ = render.GetCapabilities(); } catch (Exception e) { caught = e; } }); thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(caught);
        layer.Dispose(); Assert.Throws<ObjectDisposedException>(() => render.CaptureColor(new(64, 64, Layer: layer, SingleLayer: true)));
        first.Viewer.Redraw(); p.Dispose(); Assert.Throws<ObjectDisposedException>(() => render.ResetAppearance(p));
    }

    internal static ViewerPixelImage Image(int width, int height, byte r, byte g, byte b)
    {
        byte[] data = new byte[width * height * 4]; for (int i = 0; i < data.Length; i += 4) { data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = 255; }
        return new(width, height, data);
    }
    internal sealed class Window : IDisposable
    {
        private readonly nint handle;
        public Window() { handle = CreateWindowEx(0, "STATIC", "Batch W render validation", 0x80000000u, -32000, -32000, 320, 320, 0, 0, 0, 0); Assert.NotEqual(0, handle); _ = ShowWindow(handle, 4); _ = UpdateWindow(handle); Viewer = OcctViewer.Create(handle); }
        public OcctViewer Viewer { get; }
        public void Dispose() { Viewer.Dispose(); _ = DestroyWindow(handle); }
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)] private static extern nint CreateWindowEx(uint extended, string className, string name, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(nint window);
    }
}
