using Xunit.Abstractions;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchWDriverTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CopiedCameraReconstructsOrthographicAndPerspectiveDepth(bool perspective)
    {
        using var window = new BatchWRenderingTests.Window(); var viewer = window.Viewer; var r = viewer.Rendering;
        using var shape = ShapeFactory.CreateBox(10, 10, 10); using var p = viewer.Display(shape); p.SetDisplayMode(ViewerDisplayMode.Shaded);
        var camera = r.SetCamera(new(new(5, 5, 40), new(5, 5, 5), new(0, 1, 0), 1, 20, 45, 1, 100, perspective, false));
        var depth = r.CaptureDepth(new(200, 160)); Assert.True(depth.TryReconstruct(100, 80, out var point)); Assert.InRange(point.Z, 9.999, 10.001);
        Assert.Equal(camera, r.GetCamera());
        var tiled = r.CaptureDepth(new(200, 160, 64)); Assert.True(tiled.TryReconstruct(100, 80, out var tiledPoint)); Assert.InRange(point.DistanceTo(tiledPoint), 0, .001);
        Assert.False(depth.TryReconstruct(0, 0, out _)); Assert.Throws<ArgumentOutOfRangeException>(() => depth.GetDepth(-1, 0));
        Assert.Throws<ArgumentException>(() => r.SetCamera(camera with { NearPlane = 101 })); Assert.Equal(camera, r.GetCamera());
        Assert.Throws<ArgumentException>(() => r.CaptureColor(new(100, 100, TileSize: 1))); Assert.Equal(camera, r.GetCamera());
        Assert.True(Energy(r.CaptureColor(new(100, 100))) > 1);
        var copied = depth.CopyDepths(); copied[80 * 200 + 100] = 1; Assert.True(depth.TryReconstruct(100, 80, out _));
        r.SetCamera(camera with { Eye = new(20, 20, 50) }); Assert.True(depth.TryReconstruct(100, 80, out var unchanged)); Assert.Equal(point, unchanged);
        r.SetCamera(camera); Assert.Equal(camera, r.GetCamera());
    }

    [Theory]
    [InlineData(ViewerShading.Unlit)]
    [InlineData(ViewerShading.Phong)]
    [InlineData(ViewerShading.Pbr)]
    public void ActualDriverRendersShadingAndQualityModes(ViewerShading shading)
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(r.GetCapabilities()));
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded);
        v.SetProjection(ViewerProjection.Axonometric); v.FitAll(); v.Zoom(.7); v.SetBackgroundColor(new(0, 0, 0));
        if (shading == ViewerShading.Pbr) Assert.True(r.GetCapabilities().SupportsPbr, "This success-path fixture requires a PBR-capable driver.");
        var profile = new ViewerRenderProfile { Shading = shading == ViewerShading.Pbr ? ViewerShading.Pbr : ViewerShading.Phong };
        r.SetProfile(profile);
        r.SetAppearance(p, new() { Shading = shading, Front = new(new(.8, .1, .05)) });
        var baseline = r.CaptureColor(new(160, 160)); Assert.True(Energy(baseline) > 5);
        var camera = r.GetCamera(); var caps = r.GetCapabilities();
        Assert.True(caps.MaximumMsaaSamples >= 2); r.SetProfile(profile with { MsaaSamples = 2 }); Assert.True(Energy(r.CaptureColor(new(160, 160))) > 5);
        r.SetProfile(profile with { ResolutionScale = 1.5 }); Assert.True(Energy(r.CaptureColor(new(160, 160))) > 5);
        Assert.Equal(camera, r.GetCamera()); Assert.True(caps.SupportsWeightedOit);
        r.SetProfile(profile with { Transparency = ViewerTransparencyMethod.WeightedOit, OitDepthFactor = .2 });
        r.SetAppearance(p, new() { Shading = shading, Front = new(new(.8, .1, .05)) { Alpha = .5 }, AlphaMode = ViewerAlphaMode.Blend });
        Assert.True(Energy(r.CaptureColor(new(160, 160))) > 1);
        Assert.Throws<ArgumentException>(() => r.SetProfile(new() { MsaaSamples = caps.MaximumMsaaSamples * 2 }));
        Assert.Throws<ArgumentException>(() => r.SetProfile(new() { MsaaSamples = 2, ResolutionScale = 2 }));
    }

    [Fact]
    public void TwoSidedMaterialCullingAndAlphaAreVisualOnly()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); var faces = box.GetFaces();
        try
        {
            using var p = v.Display(faces[5]); p.SetDisplayMode(ViewerDisplayMode.Shaded); v.SetProjection(ViewerProjection.Top); v.FitAll(); v.Zoom(.7); v.SetBackgroundColor(new(0, 0, 0));
            r.SetCamera(new(new(5, 5, 40), new(5, 5, 10), new(0, 1, 0), 1, 20, 45, 1, 100, false, false));
            r.ReplaceLightRig([new(ViewerLightKind.Ambient, new(1, 1, 1))]);
            var a = new ViewerAppearanceProfile
            {
                Shading = ViewerShading.Phong,
                DistinguishSides = true,
                Front = new(new(1, 0, 0)),
                Back = new(new(0, 0, 1))
            };
            r.SetAppearance(p, a); var front = Center(r.CaptureColor(new(100, 100))); Assert.True(front.R > 200 && front.B < 10, $"front={front}");
            var camera = r.GetCamera(); r.SetCamera(camera with { Eye = new(camera.Eye.X, camera.Eye.Y, 10 - (camera.Eye.Z - 10)) });
            var back = Center(r.CaptureColor(new(100, 100))); Assert.True(back.B > 200 && back.R < 10, $"back={back}");
            r.SetAppearance(p, a with { Culling = ViewerFaceCulling.Back }); Assert.True(Energy(r.CaptureColor(new(100, 100))) < 1, "back culling");
            r.SetCamera(camera); r.SetAppearance(p, a with { Culling = ViewerFaceCulling.Front }); Assert.True(Energy(r.CaptureColor(new(100, 100))) < 1, "front culling");
            r.SetAppearance(p, a with { AlphaMode = ViewerAlphaMode.Mask, Front = a.Front with { Alpha = .2 }, AlphaCutoff = .5 }); Assert.True(Energy(r.CaptureColor(new(100, 100))) < 1, "alpha mask");
            r.SetAppearance(p, a with { AlphaMode = ViewerAlphaMode.Blend, Front = a.Front with { Alpha = .5 } });
            var blend = Center(r.CaptureColor(new(100, 100))); Assert.InRange(blend.R, 100, 240);
            Assert.Equal(6, box.FaceCount);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }

    [Fact]
    public void HeadlightRotatesWithCameraWhileWorldLightDoesNot()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded);
        v.SetProjection(ViewerProjection.Top); v.FitAll(); v.Zoom(.7); v.SetBackgroundColor(new(0, 0, 0));
        r.SetAppearance(p, new() { Shading = ViewerShading.Phong, Front = new(new(1, 1, 1)) });
        var definition = new ViewerLightDefinition(ViewerLightKind.Directional, new(1, 1, 1)) { Direction = new(0, 0, -1) };
        r.ReplaceLightRig([definition]); var top = r.GetCamera(); var lit = Energy(r.CaptureColor(new(100, 100)));
        var bottom = top with { Eye = new(5, 5, 5 - (top.Eye.Z - 5)) }; r.SetCamera(bottom); var dark = Energy(r.CaptureColor(new(100, 100)));
        Assert.True(lit > dark + 20, $"world top={lit}, bottom={dark}");
        r.ReplaceLightRig([definition with { Headlight = true }]); var head = Energy(r.CaptureColor(new(100, 100))); Assert.True(head > dark + 20);
        r.SetCamera(top); Assert.InRange(Math.Abs(head - Energy(r.CaptureColor(new(100, 100)))), 0, 8);
    }

    [Theory]
    [InlineData(ViewerLightKind.Ambient)]
    [InlineData(ViewerLightKind.Positional)]
    [InlineData(ViewerLightKind.Spot)]
    public void LightActivationChangesRenderedPixels(ViewerLightKind kind)
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded);
        v.SetProjection(ViewerProjection.Top); v.FitAll(); v.Zoom(.7); v.SetBackgroundColor(new(0, 0, 0));
        r.SetProfile(new() { Shading = kind == ViewerLightKind.Ambient ? ViewerShading.Phong : ViewerShading.Pbr });
        r.SetAppearance(p, new() { Shading = kind == ViewerLightKind.Ambient ? ViewerShading.Phong : ViewerShading.Pbr, Front = new(new(1, 1, 1)) }); r.ReplaceLightRig([]);
        using var light = r.CreateLight(new(kind, new(1, 1, 1)) { Intensity = kind == ViewerLightKind.Ambient ? 1 : 50, Position = new(5, 5, 30), Direction = new(0, 0, -1), SpotAngle = 1.2, Range = 100 });
        var lit = Energy(r.CaptureColor(new(100, 100))); light.Update(light.Definition with { Active = false });
        var dark = Energy(r.CaptureColor(new(100, 100))); Assert.True(lit > dark + 5, $"kind={kind}, lit={lit}, dark={dark}");
        light.Update(light.Definition with { Active = true }); Assert.InRange(Math.Abs(lit - Energy(r.CaptureColor(new(100, 100)))), 0, 1);
    }

    [Fact]
    public void LayerScopesFollowDrawingOrderAndPreservePicking()
    {
        using var window = new BatchWRenderingTests.Window(); var v = window.Viewer; var r = v.Rendering;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box); p.SetDisplayMode(ViewerDisplayMode.Shaded);
        v.SetProjection(ViewerProjection.Top); v.FitAll(); v.Zoom(.7); v.SetBackgroundColor(new(0, 0, 0));
        r.SetAppearance(p, new() { Shading = ViewerShading.Unlit, Front = new(new(1, 0, 0)) });
        using var first = r.CreateLayer(); using var second = r.CreateLayer(); r.AssignLayer(p, second);
        var all = r.CaptureColor(new(100, 100)); Assert.True(Center(all).R > 200);
        Assert.Equal(new long[] { 0, first.Id, second.Id }, all.Scope.LayerIds); Assert.True(all.Scope.IncludesStandardOverlays);
        var scoped = r.CaptureColor(new(100, 100, Layer: first));
        Assert.Equal(new long[] { 0, first.Id }, scoped.Scope.LayerIds); Assert.False(scoped.Scope.IncludesStandardOverlays);
        Assert.True(Energy(r.CaptureColor(new(100, 100, Layer: first))) < 1);
        Assert.True(Energy(r.CaptureColor(new(100, 100, Layer: first, SingleLayer: true))) < 1);
        Assert.True(Center(r.CaptureColor(new(100, 100, Layer: second))).R > 200);
        Assert.True(Center(r.CaptureColor(new(100, 100, Layer: second, SingleLayer: true))).R > 200);
        Assert.Equal(all.CopyPixels(), r.CaptureColor(new(100, 100)).CopyPixels()); Assert.Contains(p, v.SelectAt(160, 160));
        v.ClearSelection(); second.Dispose(); Assert.Contains(r.CaptureDepth(new(100, 100)).CopyDepths(), x => x < 1);
    }

    [Fact]
    public void ReviewRecipeSerializesKeysAndReplaysIntoFreshResources()
    {
        using var first = new BatchWRenderingTests.Window(); using var second = new BatchWRenderingTests.Window();
        using var shape = ShapeFactory.CreateBox(10, 10, 10); using var a = first.Viewer.Display(shape); using var b = second.Viewer.Display(shape);
        a.SetDisplayMode(ViewerDisplayMode.Shaded); b.SetDisplayMode(ViewerDisplayMode.Shaded); first.Viewer.FitAll();
        var r = first.Viewer.Rendering; using var texture = r.CreateTexture(BatchWRenderingTests.Image(4, 4, 200, 50, 20));
        r.SetAppearance(a, new() { Shading = ViewerShading.Unlit }, texture);
        var recipe = r.CaptureRecipe("assembly-v1", new Dictionary<string, ViewerPresentation> { ["body"] = a }, _ => "paint");
        var json = recipe.ToJson(); Assert.DoesNotContain("C:\\", json, StringComparison.Ordinal); Assert.DoesNotContain("Native", json, StringComparison.Ordinal);
        var copied = ViewerReviewRecipe.FromJson(json); Assert.Equal(recipe.Camera, copied.Camera); Assert.Equal(recipe.Lights, copied.Lights);
        var target = second.Viewer.Rendering; using var other = target.CreateTexture(BatchWRenderingTests.Image(4, 4, 200, 50, 20));
        var assets = new Dictionary<string, ViewerPresentation> { ["body"] = b }; var textures = new Dictionary<string, ViewerTexture> { ["paint"] = other };
        var before = target.GetCamera(); Assert.Throws<ArgumentException>(() => target.ApplyRecipe(copied, "other-scope", assets, textures)); Assert.Equal(before, target.GetCamera());
        Assert.Throws<ArgumentException>(() => target.ApplyRecipe(copied, "assembly-v1", assets)); Assert.Equal(before, target.GetCamera());
        target.ApplyRecipe(copied, "assembly-v1", assets, textures);
        Assert.Equal(r.CaptureColor(new(100, 100)).CopyPixels(), target.CaptureColor(new(100, 100)).CopyPixels());
        first.Viewer.Dispose(); Assert.Equal(copied.Camera, target.GetCamera()); Assert.True(Energy(target.CaptureColor(new(100, 100))) > 5);
    }

    internal static double Energy(ViewerColorFrame frame) => frame.CopyPixels().Where((_, i) => i % 4 != 3).Average(x => (double)x);
    internal static (byte R, byte G, byte B) Center(ViewerColorFrame frame) { var bytes = frame.CopyPixels(); int i = (frame.Height / 2 * frame.Width + frame.Width / 2) * 4; return (bytes[i], bytes[i + 1], bytes[i + 2]); }
}
