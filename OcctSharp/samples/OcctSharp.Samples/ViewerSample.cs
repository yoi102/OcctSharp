namespace OcctSharp.Samples;

internal static class ViewerSample
{
    public static int Run()
    {
        Console.WriteLine("Opening the OCCT Viewer. Move the pointer to highlight the solid, click to select it, and close the window to return to the menu.");

        using NativeViewerWindow window = NativeViewerWindow.Create("OcctSharp Viewer Sample", 960, 640);
        using OcctViewer viewer = OcctViewer.Create(window.Handle);
        window.Attach(viewer);

        using Shape box = ShapeFactory.CreateBox(80, 60, 40);
        using ViewerPresentation presentation = viewer.Display(box);
        presentation.SetColor(new ViewerColor(0.15, 0.45, 0.85));
        presentation.SetTransparency(0.1);
        presentation.SetDisplayMode(ViewerDisplayMode.Shaded);
        viewer.SetProjection(ViewerProjection.Axonometric);
        viewer.FitAll();
        viewer.Redraw();

        window.RunMessageLoop();
        return 0;
    }
}
