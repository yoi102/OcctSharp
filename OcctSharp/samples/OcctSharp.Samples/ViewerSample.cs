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
        viewer.FitAll();
        viewer.Redraw();

        window.RunMessageLoop();
        return 0;
    }
}
