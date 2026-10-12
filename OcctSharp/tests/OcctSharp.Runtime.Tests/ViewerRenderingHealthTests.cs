namespace OcctSharp.Runtime.Tests;

public sealed class ViewerRenderingHealthTests
{
    [Fact]
    public void HealthQueryIsReadOnlyAndExplicitInvalidationPreservesIdentityAndOtherViewer()
    {
        using var first = new BatchWRenderingTests.Window(); using var second = new BatchWRenderingTests.Window();
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = first.Viewer.Display(box);
        using var other = second.Viewer.Display(box);
        p.SetDisplayMode(ViewerDisplayMode.Shaded); other.SetDisplayMode(ViewerDisplayMode.Shaded);
        first.Viewer.FitAll(); second.Viewer.FitAll(); first.Viewer.Redraw(); second.Viewer.Redraw();
        var camera = first.Viewer.Rendering.GetCamera(); var otherCamera = second.Viewer.Rendering.GetCamera();
        var before = first.Viewer.Rendering.CaptureColor(new(128, 128)).CopyPixels();
        var otherBefore = second.Viewer.Rendering.CaptureColor(new(128, 128)).CopyPixels();
        Assert.Equal(ViewerRenderingHealth.Healthy, first.Viewer.GetRenderingHealth());
        Assert.Equal(ViewerRenderingHealth.Healthy, first.Viewer.GetRenderingHealth());
        Assert.Equal(camera, first.Viewer.Rendering.GetCamera());
        first.Viewer.InvalidateGpuResources();
        Assert.Equal(ViewerRenderingHealth.PresentationDataLost, first.Viewer.GetRenderingHealth());
        Assert.Equal(ViewerRenderingHealth.PresentationDataLost, first.Viewer.GetRenderingHealth());
        Assert.Equal(ViewerRenderingHealth.Healthy, second.Viewer.GetRenderingHealth());
        Assert.Equal(otherCamera, second.Viewer.Rendering.GetCamera());
        Assert.Equal(otherBefore, second.Viewer.Rendering.CaptureColor(new(128, 128)).CopyPixels());
        first.Viewer.Redraw(); Assert.Equal(ViewerRenderingHealth.Healthy, first.Viewer.GetRenderingHealth());
        Assert.Equal(before, first.Viewer.Rendering.CaptureColor(new(128, 128)).CopyPixels());
        first.Viewer.SetProjection(ViewerProjection.Top); first.Viewer.FitAll(); first.Viewer.SelectAt(160, 160);
        Assert.Same(p, Assert.Single(first.Viewer.GetSelection()));
    }

    [Fact]
    public void ThreadAndDisposedGuardsPrecedeHealthAccessAndInvalidation()
    {
        using var window = new BatchWRenderingTests.Window(); var viewer = window.Viewer;
        Exception? read = null, write = null;
        var thread = new Thread(() => { try { _ = viewer.GetRenderingHealth(); } catch (Exception e) { read = e; }
            try { viewer.InvalidateGpuResources(); } catch (Exception e) { write = e; } });
        thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(read); Assert.IsType<InvalidOperationException>(write);
        viewer.Dispose(); Assert.Throws<ObjectDisposedException>(() => viewer.GetRenderingHealth());
        Assert.Throws<ObjectDisposedException>(viewer.InvalidateGpuResources);
    }
}
