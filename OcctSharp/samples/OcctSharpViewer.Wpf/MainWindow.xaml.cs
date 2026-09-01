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
}
