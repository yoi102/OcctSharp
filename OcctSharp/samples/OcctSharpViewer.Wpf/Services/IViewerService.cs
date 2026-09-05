using OcctSharp;

namespace OcctSharpViewer.Wpf.Services;

public interface IViewerService
{
    void LoadModel(string filePath);
    void FitAll();
    void SetProjection(ViewerProjection projection);
    void SetDisplayMode(ViewerDisplayMode displayMode);
    void ClearSelection();
    ViewerColorFrame CaptureSnapshot();
}
