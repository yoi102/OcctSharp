using System.Windows;
using OcctSharpViewer.Wpf.Services;
using OcctSharpViewer.Wpf.ViewModels;

namespace OcctSharpViewer.Wpf;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new MainWindowViewModel(new FileDialogService());
        DataContext = viewModel;
        Closed += (_, _) => viewModel.DetachViewer();
    }

    private void OnViewerReady(object? sender, EventArgs e) => viewModel.AttachViewer(Viewport);

    private void OnSelectionChanged(object? sender, int count) => viewModel.SelectionCount = count;

    private void OnViewerError(object? sender, string message) => viewModel.ReportViewerError(message);

    internal void RunSnapshotSmoke(string output)
    {
        Viewport.DisplaySnapshotSmokeShape();
        viewModel.AttachViewer(Viewport);
        viewModel.CaptureSnapshotCommand.Execute(null);
        var bitmap = viewModel.ReviewSnapshot ?? throw new InvalidOperationException(viewModel.StatusText);
        if (!bitmap.IsFrozen || bitmap.PixelWidth != 360 || bitmap.PixelHeight != 240)
            throw new InvalidOperationException("Copied WPF snapshot dimensions/lifetime are invalid.");
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using var stream = System.IO.File.Create(output); encoder.Save(stream);
    }
}
