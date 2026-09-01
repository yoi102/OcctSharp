using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using OcctSharp;
using OcctSharpViewer.Wpf.Services;

namespace OcctSharpViewer.Wpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IFileDialogService fileDialog;
    private IViewerService? viewer;

    [ObservableProperty]
    private string statusText = "Initializing OCCT viewer...";

    [ObservableProperty]
    private string currentFileName = "No model loaded";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionText))]
    private int selectionCount;

    [ObservableProperty]
    private bool isViewerReady;

    public MainWindowViewModel(IFileDialogService fileDialog) => this.fileDialog = fileDialog;

    public string WindowTitle => CurrentFileName == "No model loaded"
        ? "OcctSharpViewer.Wpf"
        : $"OcctSharpViewer.Wpf — {CurrentFileName}";

    public string SelectionText => $"Selected: {SelectionCount}";

    public void AttachViewer(IViewerService viewerService)
    {
        viewer = viewerService;
        IsViewerReady = true;
        StatusText = "Ready. Open a STEP or IGES file.";
        RefreshCommands();
    }

    public void ReportViewerError(string message) => StatusText = $"Viewer input failed: {message}";

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void OpenModel()
    {
        string? filePath = fileDialog.SelectModelFile();
        if (filePath is null) return;

        try
        {
            StatusText = $"Loading {Path.GetFileName(filePath)}...";
            viewer!.LoadModel(filePath);
            CurrentFileName = Path.GetFileName(filePath);
            OnPropertyChanged(nameof(WindowTitle));
            SelectionCount = 0;
            StatusText = $"Loaded {filePath}";
        }
        catch (Exception error)
        {
            StatusText = $"Load failed: {error.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void FitAll() => RunViewerAction(() => viewer!.FitAll(), "Fit all");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Axonometric() => SetProjection(ViewerProjection.Axonometric, "Axonometric view");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Front() => SetProjection(ViewerProjection.Front, "Front view");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Top() => SetProjection(ViewerProjection.Top, "Top view");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Left() => SetProjection(ViewerProjection.Left, "Left view");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Right() => SetProjection(ViewerProjection.Right, "Right view");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Shaded() => SetDisplayMode(ViewerDisplayMode.Shaded, "Shaded display");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void Wireframe() => SetDisplayMode(ViewerDisplayMode.Wireframe, "Wireframe display");

    [RelayCommand(CanExecute = nameof(CanUseViewer))]
    private void ClearSelection()
    {
        RunViewerAction(() => viewer!.ClearSelection(), "Selection cleared");
        SelectionCount = 0;
    }

    public void DetachViewer()
    {
        viewer = null;
        IsViewerReady = false;
        RefreshCommands();
    }

    private bool CanUseViewer() => IsViewerReady && viewer is not null;

    private void SetProjection(ViewerProjection projection, string status) =>
        RunViewerAction(() => viewer!.SetProjection(projection), status);

    private void SetDisplayMode(ViewerDisplayMode mode, string status) =>
        RunViewerAction(() => viewer!.SetDisplayMode(mode), status);

    private void RunViewerAction(Action action, string successStatus)
    {
        try
        {
            action();
            StatusText = successStatus;
        }
        catch (Exception error)
        {
            StatusText = $"Viewer operation failed: {error.Message}";
        }
    }

    private void RefreshCommands()
    {
        OpenModelCommand.NotifyCanExecuteChanged();
        FitAllCommand.NotifyCanExecuteChanged();
        AxonometricCommand.NotifyCanExecuteChanged();
        FrontCommand.NotifyCanExecuteChanged();
        TopCommand.NotifyCanExecuteChanged();
        LeftCommand.NotifyCanExecuteChanged();
        RightCommand.NotifyCanExecuteChanged();
        ShadedCommand.NotifyCanExecuteChanged();
        WireframeCommand.NotifyCanExecuteChanged();
        ClearSelectionCommand.NotifyCanExecuteChanged();
    }
}
