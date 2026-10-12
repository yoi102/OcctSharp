namespace OcctSharp.Runtime.Tests;

public sealed partial class A5ViewerSourceTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(.3)]
    public void ExplicitRootTransparencyIncludingZeroSurvivesSourceAlphaChange(double transparency)
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); instance.SetTransparency(transparency);
        window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        var before = CaptureRefresh(window);
        SetRefreshColor(document, label, new(.1, .2, .8, .25)); source.RefreshStyles(label);
        Assert.Equal(before, CaptureRefresh(window));
        AssertMatchesFreshSource(window, label, instance, identity,
            configure: reference => reference.SetTransparency(transparency));
    }

    [Fact]
    public void RefreshStylesRetainsPresentationSelectionAndExplicitRootOverrides()
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5);
        var label = AddRefreshLabel(document, shape);
        using var source = window.Viewer.CreateSource(label);
        using var identity = GpTrsf.Identity; using var shift = GpTrsf.Create(40, 0, 0);
        using var first = window.Viewer.Display(source, identity);
        using var second = window.Viewer.Display(source, shift);
        first.SetDisplayMode(ViewerDisplayMode.Shaded); second.SetDisplayMode(ViewerDisplayMode.Shaded);
        // An explicit color equal to the old default remains an override.
        second.SetColor(new(.1, .2, .8)); second.SetTransparency(.3);
        window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        var before = CaptureRefresh(window);
        first.SetSelectionKind(ShapeKind.Face);
        var point = window.Viewer.WorldToScreen(new(10, 5, 5));
        Assert.Same(first, Assert.Single(window.Viewer.SelectAt(point.X, point.Y)));
        using var selected = window.Viewer.GetSelectedItems().Single();
        SetRefreshColor(document, label, new(.8, .1, .05, .65));
        source.RefreshStyles(label);
        Assert.Same(first, Assert.Single(window.Viewer.GetSelection()));
        using var afterSelection = window.Viewer.GetSelectedItems().Single();
        Assert.Equal(RepairSnapshot.FindTopologyIndex(shape, selected.Shape),
            RepairSnapshot.FindTopologyIndex(shape, afterSelection.Shape));
        window.Viewer.ClearSelection();
        var after = CaptureRefresh(window);
        Assert.False(before.SequenceEqual(after));
        var unaffected = window.Viewer.WorldToScreen(new(50, 5, 5));
        var index = 4 * (unaffected.Y * 320 + unaffected.X);
        Assert.Equal(before.AsSpan(index, 4).ToArray(), after.AsSpan(index, 4).ToArray());
        AssertMatchesFreshSource(window, label, first, identity);
        using var addedTransform = GpTrsf.Create(80, 0, 0);
        using var added = window.Viewer.Display(source, addedTransform);
        added.SetDisplayMode(ViewerDisplayMode.Shaded);
    }

    [Fact]
    public void OriginalSubshapeOverridesSurviveRefreshAndClearRestoresUpdatedDefaults()
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        using var top = SelectOriginalTop(window, instance);
        instance.SetSubshapeColor(top, new(.05, .9, .15));
        instance.SetSubshapeTransparency(top, .2); instance.SetSubshapeWidth(top, 3);
        SetRefreshColor(document, label, new(.9, .1, .05, .65)); source.RefreshStyles(label);
        // A transparent overridden top still shows the refreshed underlying
        // faces. Compare against the new source with exactly the same overrides.
        AssertMatchesFreshSource(window, label, instance, identity, configure: reference =>
        {
            reference.SetSubshapeColor(top, new(.05, .9, .15));
            reference.SetSubshapeTransparency(top, .2); reference.SetSubshapeWidth(top, 3);
        });
        instance.ClearSubshapeOverrides(top);
        AssertMatchesFreshSource(window, label, instance, identity);
        instance.SetSubshapeColor(top, new(.2, .05, .9)); instance.ClearAllSubshapeOverrides();
        AssertMatchesFreshSource(window, label, instance, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Wireframe);
        instance.SetSubshapeColor(top, new(.05, .9, .15)); instance.SetSubshapeWidth(top, 3);
        using (var edit = document.BeginTransaction())
        { label.SetPresentationColor(XdeColorChannel.Curve, new(.8, .1, .05)); edit.Commit(); }
        source.RefreshStyles(label);
        AssertMatchesFreshSource(window, label, instance, identity, ViewerDisplayMode.Wireframe,
            reference => { reference.SetSubshapeColor(top, new(.05, .9, .15)); reference.SetSubshapeWidth(top, 3); });
        instance.ClearAllSubshapeOverrides();
        AssertMatchesFreshSource(window, label, instance, identity, ViewerDisplayMode.Wireframe);
    }

    [Fact]
    public void AppearanceAndTextureSurviveRefreshAndResetToUpdatedSourceStyles()
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        using var texture = window.Viewer.Rendering.CreateTexture(BatchWRenderingTests.Image(8, 8, 0, 0, 255));
        window.Viewer.Rendering.SetAppearance(instance,
            new() { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true } }, texture);
        var before = CaptureRefresh(window);
        SetRefreshColor(document, label, new(.9, .1, .05, .65)); source.RefreshStyles(label);
        Assert.Equal(before, CaptureRefresh(window));
        window.Viewer.Rendering.ResetAppearance(instance);
        AssertMatchesFreshSource(window, label, instance, identity);
        // Updating and releasing the texture uses the newly rebound drawers.
        window.Viewer.Rendering.SetAppearance(instance,
            new() { Shading = ViewerShading.Unlit, Mapping = new() { Planar = true } }, texture);
        source.RefreshStyles(label); texture.Dispose();
        window.Viewer.Rendering.ResetAppearance(instance);
        AssertMatchesFreshSource(window, label, instance, identity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefreshVisibilityRemovesHiddenSelectionsAndSupportsShowingAgain(bool hideWhole)
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        using var top = SelectOriginalTop(window, instance);
        using var original = label.Shape;
        var topIndex = RepairSnapshot.FindTopologyIndex(original, top); Assert.True(topIndex >= 0);
        XdeLabel hidden;
        using (var edit = document.BeginTransaction())
        {
            using var repair = new RepairDocumentSession(document, label);
            hidden = hideWhole ? label : repair.GetOrCreateSubshapeLabel(
                repair.Source.Topology.Single(item => item.Selection.Index == topIndex).Selection);
            hidden.IsVisible = false; edit.Commit();
        }
        var pixel = window.Viewer.WorldToScreen(new(10, 5, 5));
        Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
        source.RefreshStyles(label);
        Assert.Empty(window.Viewer.GetSelection());
        _ = window.Viewer.MoveTo(pixel.X, pixel.Y);
        using var detected = window.Viewer.GetDetectedItem();
        if (hideWhole) Assert.Null(detected);
        else if (detected is not null)
            Assert.NotEqual(topIndex, RepairSnapshot.FindTopologyIndex(original, detected.Shape));
        using (var edit = document.BeginTransaction()) { hidden.IsVisible = true; edit.Commit(); }
        source.RefreshStyles(label);
        Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
        using var restored = window.Viewer.GetSelectedItems().Single();
        Assert.Equal(topIndex, RepairSnapshot.FindTopologyIndex(original, restored.Shape));
    }

    [Fact]
    public void FailedStyleRefreshKeepsFrameAndSelectionAndRejectsRemovedOrWrongThreadSources()
    {
        using var window = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var otherShape = ShapeFactory.CreateBox(20, 10, 5); var other = AddRefreshLabel(document, otherShape);
        using var source = window.Viewer.CreateSource(label); using var identity = GpTrsf.Identity;
        using var instance = window.Viewer.Display(source, identity);
        instance.SetDisplayMode(ViewerDisplayMode.Shaded); window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        using var top = SelectOriginalTop(window, instance);
        var pixel = window.Viewer.WorldToScreen(new(10, 5, 5));
        _ = window.Viewer.SelectAt(pixel.X, pixel.Y); var before = CaptureRefresh(window);
        Assert.Throws<ArgumentException>(() => source.RefreshStyles(other));
        Assert.Equal(before, CaptureRefresh(window)); Assert.Same(instance, Assert.Single(window.Viewer.GetSelection()));
        Exception? error = null;
        var thread = new Thread(() => { try { source.RefreshStyles(label); } catch (Exception exception) { error = exception; } });
        thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(error);
        using var disposedDocument = XdeDocument.Create(); var disposedLabel = AddRefreshLabel(disposedDocument, shape);
        disposedDocument.Dispose(); Assert.Throws<ObjectDisposedException>(() => source.RefreshStyles(disposedLabel));
        Assert.Equal(before, CaptureRefresh(window));
        source.Dispose(); Assert.Throws<ObjectDisposedException>(() => source.RefreshStyles(label));
        Assert.Equal(before, CaptureRefresh(window));
    }

    [Fact]
    public void RefreshIsViewerLocalAndRetainedInstancesOutliveAllCallers()
    {
        using var first = new Window(); using var second = new Window(); using var document = XdeDocument.Create();
        using var shape = ShapeFactory.CreateBox(20, 10, 5); var label = AddRefreshLabel(document, shape);
        using var firstSource = first.Viewer.CreateSource(label); using var secondSource = second.Viewer.CreateSource(label);
        using var identity = GpTrsf.Identity; using var firstInstance = first.Viewer.Display(firstSource, identity);
        using var secondInstance = second.Viewer.Display(secondSource, identity);
        foreach (var window in new[] { first, second }) { window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll(); }
        firstInstance.SetDisplayMode(ViewerDisplayMode.Shaded); secondInstance.SetDisplayMode(ViewerDisplayMode.Shaded);
        var beforeFirst = CaptureRefresh(first); var beforeSecond = CaptureRefresh(second);
        SetRefreshColor(document, label, new(.9, .1, .05, .65)); firstSource.RefreshStyles(label);
        Assert.False(beforeFirst.SequenceEqual(CaptureRefresh(first))); Assert.Equal(beforeSecond, CaptureRefresh(second));
        first.Viewer.ClearSources(); second.Viewer.ClearSources(); document.Dispose(); shape.Dispose();
        firstInstance.Hide(); firstInstance.Show(); secondInstance.Hide(); secondInstance.Show();
        Assert.NotEmpty(CaptureRefresh(first)); Assert.Equal(beforeSecond, CaptureRefresh(second));
        first.Viewer.Dispose(); Assert.Equal(beforeSecond, CaptureRefresh(second));
    }

    private static XdeLabel AddRefreshLabel(XdeDocument document, Shape shape)
    {
        using var transaction = document.BeginTransaction();
        var label = document.AddShape(shape, "A5 style revision");
        label.SetPresentationColor(XdeColorChannel.Surface, new(.1, .2, .8, .65));
        label.SetPresentationColor(XdeColorChannel.Curve, new(.4, .25, .05));
        transaction.Commit(); return label;
    }
    private static void SetRefreshColor(XdeDocument document, XdeLabel label, XdeColor color)
    {
        using var transaction = document.BeginTransaction();
        label.SetPresentationColor(XdeColorChannel.Surface, color); transaction.Commit();
    }
    private static byte[] CaptureRefresh(Window window) => window.Viewer.Rendering.CaptureColor(new(320, 320)).CopyPixels();
    private static Shape SelectOriginalTop(Window window, ViewerPresentation instance)
    {
        instance.SetSelectionKind(ShapeKind.Face);
        var pixel = window.Viewer.WorldToScreen(new(10, 5, 5));
        Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
        using var item = window.Viewer.GetSelectedItems().Single();
        var shape = item.Shape.Clone(); window.Viewer.ClearSelection(); return shape;
    }
    private static void AssertMatchesFreshSource(Window window, XdeLabel label, ViewerPresentation instance,
        GpTrsf transform, ViewerDisplayMode mode = ViewerDisplayMode.Shaded,
        Action<ViewerPresentation>? configure = null)
    {
        var actual = CaptureRefresh(window);
        instance.Hide(); using var freshSource = window.Viewer.CreateSource(label);
        using var reference = window.Viewer.Display(freshSource, transform);
        reference.SetDisplayMode(mode); configure?.Invoke(reference);
        Assert.Equal(actual, CaptureRefresh(window));
        reference.Dispose(); instance.Show();
    }
}
