namespace OcctSharp.Runtime.Tests;

public sealed partial class A5ViewerSourceTests
{
    [Theory]
    [InlineData(XdeColorChannel.Surface, ViewerDisplayMode.Wireframe)]
    [InlineData(XdeColorChannel.Curve, ViewerDisplayMode.Shaded)]
    public void AnAuthorColorChannelDoesNotOverrideTheOtherInheritedChannel(XdeColorChannel channel, ViewerDisplayMode mode)
    {
        using var window = new Window();
        using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        XdeLabel label;
        using (var transaction = document.BeginTransaction())
        {
            label = document.AddShape(shape, "Independent source color channel");
            label.SetPresentationColor(channel, new(.15, .25, .8));
            transaction.Commit();
        }
        byte[] expected;
        using (var ordinary = window.Viewer.Display(shape))
        {
            ordinary.SetDisplayMode(mode);
            window.Viewer.SetProjection(ViewerProjection.Axonometric);
            window.Viewer.FitAll();
            expected = CaptureRefresh(window);
        }
        using var source = window.Viewer.CreateSource(label);
        using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(mode);
        Assert.True(expected.SequenceEqual(CaptureRefresh(window)), "An author color channel replaced another inherited channel.");
        source.RefreshStyles(label);
        Assert.True(expected.SequenceEqual(CaptureRefresh(window)), "Refresh replaced an independent inherited color channel.");
    }

    [Theory]
    [InlineData(0, ViewerDisplayMode.Shaded)]
    [InlineData(1, ViewerDisplayMode.Shaded)]
    [InlineData(2, ViewerDisplayMode.Shaded)]
    [InlineData(0, ViewerDisplayMode.Wireframe)]
    [InlineData(1, ViewerDisplayMode.Wireframe)]
    public void SourceDefaultsPreserveOrdinaryFramesThroughRefreshAndCallerRelease(int style, ViewerDisplayMode mode)
    {
        using var window = new Window();
        using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        XdeLabel label;
        using (var transaction = document.BeginTransaction())
        {
            label = document.AddShape(shape, "Source default regression");
            if (style != 0)
                label.SetPresentationColor(XdeColorChannel.General,
                    new(.15, .25, .8, style == 2 ? .5 : 1));
            transaction.Commit();
        }
        byte[] expected;
        using (var ordinary = window.Viewer.Display(label))
        {
            ordinary.SetDisplayMode(mode);
            window.Viewer.SetProjection(ViewerProjection.Axonometric);
            window.Viewer.FitAll();
            expected = CaptureRefresh(window);
        }
        // Preserve the same camera and source identity. No approximate image
        // comparison or appearance substitution is allowed.
        using var source = window.Viewer.CreateSource(label);
        using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(mode);
        Assert.True(expected.SequenceEqual(CaptureRefresh(window)), "Source default frame differs from ordinary display.");
        source.RefreshStyles(label);
        Assert.True(expected.SequenceEqual(CaptureRefresh(window)), "Source refresh changed inherited default appearance.");
        document.Dispose(); shape.Dispose(); source.Dispose();
        Assert.True(expected.SequenceEqual(CaptureRefresh(window)), "Caller release changed inherited default appearance.");
    }
}
