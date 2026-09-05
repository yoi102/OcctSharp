using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchWAssetsTests
{
    [Fact]
    public void XdeStyleResetMetadataAndPublicConsumerWorkflow() => Validation.BatchWReviewWorkflow.Run();
    [Fact]
    public void MissingMeshUvsRejectUntilPlanarMappingIsExplicit()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        var mesh = new AuthoredMesh([new(0, 0, 0), new(10, 0, 0), new(0, 10, 0)], [new(0, 1, 2)]);
        using var model = MeshTopology.Create(mesh); using var shape = model.CopyShape(); using var p = v.Display(shape); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.FitAll();
        using var texture = r.CreateTexture(BatchWRenderingTests.Image(2, 2, 255, 0, 0));
        Assert.Throws<ArgumentException>(() => r.SetAppearance(p, new(), texture)); Assert.Null(r.GetAppearance(p));
        r.SetAppearance(p, new() { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true } }, texture);
        Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(64, 64))) > 1); Assert.Null(MeshTopology.SnapshotExisting(shape).Mesh.UVs);
    }

    [Fact]
    public void SixFaceAndPackedCubeOrderingAgreeForEveryCameraDirection()
    {
        using var window = new BatchWRenderingTests.Window(); var r = window.Viewer.Rendering;
        (byte R, byte G, byte B)[] colors = [(255, 0, 0), (0, 255, 0), (0, 0, 255), (255, 255, 0), (255, 0, 255), (0, 255, 255)];
        var faces = colors.Select(c => r.CreateTexture(BatchWRenderingTests.Image(8, 8, c.R, c.G, c.B))).ToArray();
        try
        {
            using var separate = r.CreateEnvironment(faces); byte[] pixels = new byte[48 * 8 * 4];
            for (int y = 0; y < 8; y++) for (int x = 0; x < 48; x++) { int i = (y * 48 + x) * 4; var c = colors[x / 8]; pixels[i] = c.R; pixels[i + 1] = c.G; pixels[i + 2] = c.B; pixels[i + 3] = 255; }
            using var image = r.CreateTexture(new(48, 8, pixels)); using var packed = r.CreatePackedEnvironment(image, [0, 1, 2, 3, 4, 5]);
            GpPoint[] directions = [new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0), new(0, 0, 1), new(0, 0, -1)];
            var observed = new HashSet<(byte, byte, byte)>();
            for (int i = 0; i < 6; i++)
            {
                r.SetCamera(new(new(0, 0, 0), directions[i], i < 4 ? new(0, 0, 1) : new(0, 1, 0), 1, 1, 45, .1, 100, true, false));
                r.SetEnvironment(separate, true, false); var a = BatchWDriverTests.Center(r.CaptureColor(new(32, 32)));
                r.SetEnvironment(packed, true, false); var b = BatchWDriverTests.Center(r.CaptureColor(new(32, 32))); Assert.Equal(a, b); observed.Add(a);
            }
            Assert.Equal(6, observed.Count); Assert.All(colors, c => Assert.Contains(c, observed));
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }
    [Fact]
    public void CubemapPackingBackgroundAndPbrIlluminationAreIndependent()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        Assert.True(r.GetCapabilities().SupportsPbr); r.SetProfile(new() { Shading = ViewerShading.Pbr });
        v.SetBackgroundColor(new(0, 0, 0));
        using var white = r.CreateTexture(BatchWRenderingTests.Image(8, 8, 255, 255, 255));
        using var black = r.CreateTexture(BatchWRenderingTests.Image(8, 8, 0, 0, 0));
        using var bright = r.CreateEnvironment([white, white, white, white, white, white]);
        using var dark = r.CreateEnvironment([black, black, black, black, black, black]);
        using var packedTexture = r.CreateTexture(BatchWRenderingTests.Image(48, 8, 255, 255, 255));
        using var packed = r.CreatePackedEnvironment(packedTexture, [0, 1, 2, 3, 4, 5]);
        Assert.Throws<ArgumentException>(() => r.CreatePackedEnvironment(packedTexture, [0, 0, 2, 3, 4, 5]));
        Assert.Throws<ArgumentException>(() => r.CreateEnvironment([white, white, white, white, white, packedTexture]));
        r.SetEnvironment(bright, true, false); var separate = r.CaptureColor(new(96, 96)); Assert.True(BatchWDriverTests.Energy(separate) > 240);
        r.SetEnvironment(packed, true, false); Assert.Equal(separate.CopyPixels(), r.CaptureColor(new(96, 96)).CopyPixels());
        packedTexture.Dispose(); white.Dispose(); r.SetEnvironment(bright, false, true); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))) < 1);
        using var sphere = ShapeFactory.CreateSphere(5); using var p = v.Display(sphere); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.FitAll(); v.Zoom(.7);
        r.ReplaceLightRig([new(ViewerLightKind.Ambient, new(1, 1, 1))]); r.SetAppearance(p, new() { Shading = ViewerShading.Pbr, Front = new(new(.8, .8, .8)) { Metallic = .8, Roughness = .2 } });
        var lit = BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))); r.SetEnvironment(dark, false, true);
        var unlit = BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))); Assert.True(lit > unlit + 10, $"IBL bright={lit}, dark={unlit}");
        r.SetEnvironment(null); var defaults = r.CaptureColor(new(96, 96));
        r.SetEnvironment(bright, false, true); var before = r.CaptureColor(new(96, 96));
        var recipe = r.CaptureRecipe("model", new Dictionary<string, ViewerPresentation> { { "sphere", p } }, environmentKey: _ => "studio");
        Assert.Equal(new ViewerRecipeEnvironment("studio", false, true), recipe.Environment);
        bright.Dispose(); Assert.Equal(defaults.CopyPixels(), r.CaptureColor(new(96, 96)).CopyPixels());
        r.SetEnvironment(packed, true, false); p.Hide(); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))) > 240);
        r.SetEnvironment(null); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))) < 1); Assert.NotEmpty(before.CopyPixels());
    }

    [Fact]
    public void PathTracingExposureAndFilmicSettingsHaveActualPixelEffects()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        Assert.True(r.GetCapabilities().SupportsPathTracing, "This success-path fixture requires ray tracing support.");
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.FitAll(); v.Zoom(.7);
        var profile = new ViewerRenderProfile { Mode = ViewerRenderMode.PathTracing, Exposure = -2 };
        r.SetProfile(profile); var dark = BatchWDriverTests.Energy(r.CaptureColor(new(96, 96)));
        r.SetProfile(profile with { Exposure = 2 }); var bright = BatchWDriverTests.Energy(r.CaptureColor(new(96, 96)));
        Assert.True(bright > dark + 5, $"path exposure dark={dark}, bright={bright}");
        var effective = r.SetProfile(profile with { Exposure = 1, ToneMapping = ViewerToneMapping.Filmic, WhitePoint = 4 });
        Assert.Equal(4, effective.WhitePoint); var filmic = r.CaptureColor(new(96, 96)); Assert.True(BatchWDriverTests.Energy(filmic) > 1);
        Assert.Throws<ArgumentException>(() => r.SetProfile(profile with { MsaaSamples = 2 }));
        r.SetProfile(new()); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(96, 96))) > 1);
    }

    [Fact]
    public void TextureInputOriginSamplingTransformsAndLocalDecodeAreOwned()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.SetProjection(ViewerProjection.Top); v.FitAll(); v.Zoom(.7);
        byte[] pixels = new byte[8 * 8 * 4]; for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) { int i = (y * 8 + x) * 4; pixels[i] = (byte)(x < 4 ? 255 : 0); pixels[i + 2] = (byte)(x < 4 ? 0 : 255); pixels[i + 3] = 255; }
        var image = new ViewerPixelImage(8, 8, pixels); using var texture = r.CreateTexture(image); Array.Fill(pixels, (byte)0);
        var appearance = new ViewerAppearanceProfile { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true, ScaleS = .1, ScaleT = .1, Repeat = true, Filter = ViewerTextureFilter.Nearest } };
        r.SetAppearance(p, appearance, texture); var baseline = r.CaptureColor(new(120, 120));
        r.SetAppearance(p, appearance with { Mapping = appearance.Mapping with { RotationDegrees = 90, TranslationS = .25 } }, texture);
        Assert.False(baseline.CopyPixels().SequenceEqual(r.CaptureColor(new(120, 120)).CopyPixels()));
        foreach (var filter in Enum.GetValues<ViewerTextureFilter>()) foreach (var aniso in Enum.GetValues<ViewerTextureAnisotropy>())
        {
            r.SetAppearance(p, appearance with { Mapping = appearance.Mapping with { Filter = filter, Anisotropy = aniso } }, texture); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(64, 64))) > 1);
        }
        r.SetAppearance(p, appearance with { Mapping = new() { Filter = ViewerTextureFilter.Nearest } }, texture); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(120, 120))) > 1);
        r.SetAppearance(p, appearance, texture); var original = image.CopyPixels(); var flipped = new byte[original.Length];
        for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) { int source = (y * 8 + x) * 4, target = ((7 - y) * 8 + x) * 4; flipped[target] = original[source + 2]; flipped[target + 1] = original[source + 1]; flipped[target + 2] = original[source]; flipped[target + 3] = 255; }
        texture.Replace(new(8, 8, flipped, format: ViewerPixelFormat.Bgra8, bottomUp: true)); Assert.Equal(baseline.CopyPixels(), r.CaptureColor(new(120, 120)).CopyPixels());
        var path = Path.Combine(Path.GetTempPath(), "occtsharp-w-image-" + Guid.NewGuid().ToString("N") + ".png");
        try { v.SaveScreenshot(path); using var decoded = r.CreateTextureFromFile(path); File.Delete(path); r.SetAppearance(p, appearance, decoded); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(64, 64))) > 1); }
        finally { if (File.Exists(path)) File.Delete(path); }
        Assert.Throws<ArgumentException>(() => r.CreateTextureFromFile("https://example.com/image.png"));
        Assert.Throws<ArgumentException>(() => new ViewerPixelImage(8, 8, new byte[7]));
    }

    [Fact]
    public unsafe void RawBuffersFlagsStaleIdsAndRepeatedLifetimesAreChecked()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        var request = new FrameRequestRaw { Width = 16, Height = 16, Layer = -1, AdjustAspect = 1 };
        byte* bytes = stackalloc byte[1024]; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.FrameCapture(v.Handle, in request, bytes, 3, out _));
        request.Depth = 2; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.FrameCapture(v.Handle, in request, bytes, 1024, out _));
        var pixel = new PixelInputRaw { Width = 1, Height = 1, Stride = 4, Reserved = 1 }; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.TexturePixels(v.Handle, 0, in pixel, bytes, 4, out _));
        var profile = new RenderProfileRaw { Reserved = 1 }; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.RenderProfile(v.Handle, &profile, out _));
        using var shape = ShapeFactory.CreateBox(1, 1, 1); using var p = v.Display(shape); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.FitAll();
        for (int i = 0; i < 24; i++)
        {
            using var texture = r.CreateTexture(BatchWRenderingTests.Image(2, 2, 200, 30, 50));
            using var light = r.CreateLight(new(ViewerLightKind.Ambient, new(.2, .2, .2)));
            using var layer = r.CreateLayer(); r.AssignLayer(p, layer); r.SetAppearance(p, new(), texture);
            texture.Replace(BatchWRenderingTests.Image(2, 2, 20, 50, 200)); Assert.True(BatchWDriverTests.Energy(r.CaptureColor(new(32, 32))) > 1);
            long stale = texture.Id; texture.Dispose(); Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.TextureRemove(v.Handle, stale)); r.ResetAppearance(p);
        }
        var resource = r.CreateTexture(BatchWRenderingTests.Image(2, 2, 1, 2, 3)); v.Dispose(); resource.Dispose(); Assert.Throws<ObjectDisposedException>(() => resource.Replace(BatchWRenderingTests.Image(2, 2, 4, 5, 6)));
    }
}
