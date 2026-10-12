namespace OcctSharp.Runtime.Tests;

public sealed class ViewerInputCancellationTests
{
    [Fact]
    public void CancelPreservesSelectionAndSuppressesStaleMovesAndReleases()
    {
        using var window = new BatchWRenderingTests.Window(); var viewer = window.Viewer;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var presentation = viewer.Display(box);
        viewer.SetProjection(ViewerProjection.Top); viewer.FitAll();
        viewer.SelectAt(160, 160); Assert.Same(presentation, Assert.Single(viewer.GetSelection()));
        viewer.Input.PointerPressed(ViewerPointerButton.Right, 160, 160);
        var camera = viewer.GetCamera(); viewer.Input.CancelPointerGesture();
        viewer.Input.PointerMoved(200, 200, ViewerPointerButtons.Right); Assert.Equal(camera, viewer.GetCamera());
        viewer.Input.PointerReleased(ViewerPointerButton.Right, 200, 200);
        Assert.Same(presentation, Assert.Single(viewer.GetSelection()));
        viewer.ClearSelection(); viewer.Input.PointerPressed(ViewerPointerButton.Left, 160, 160);
        Exception? wrongThread = null; var thread = new Thread(() => { try { viewer.Input.CancelPointerGesture(); } catch (Exception error) { wrongThread = error; } });
        thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(wrongThread);
        Assert.Same(presentation, Assert.Single(viewer.Input.PointerReleased(ViewerPointerButton.Left, 160, 160)));
        viewer.ClearSelection(); viewer.Input.PointerPressed(ViewerPointerButton.Left, 160, 160);
        viewer.Input.CancelPointerGesture(); viewer.Input.CancelPointerGesture();
        Assert.Empty(viewer.Input.PointerReleased(ViewerPointerButton.Left, 160, 160));
        viewer.Input.PointerPressed(ViewerPointerButton.Middle, 160, 160); viewer.Input.CancelPointerGesture();
        camera = viewer.GetCamera(); viewer.Input.PointerMoved(190, 190, ViewerPointerButtons.Middle); Assert.Equal(camera, viewer.GetCamera());
        viewer.Dispose(); Assert.Throws<ObjectDisposedException>(viewer.Input.CancelPointerGesture);
    }
}
