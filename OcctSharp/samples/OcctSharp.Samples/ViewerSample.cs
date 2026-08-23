namespace OcctSharp.Samples;

internal static class ViewerSample
{
    public static int Run()
    {
        Console.WriteLine("即将打开 OCCT Viewer：移动鼠标可高亮实体，单击可选择，关闭窗口后返回菜单。");

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
