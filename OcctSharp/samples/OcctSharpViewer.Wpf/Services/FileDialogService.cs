using Microsoft.Win32;

namespace OcctSharpViewer.Wpf.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? SelectModelFile()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Open a STEP or IGES model",
            Filter = "CAD models (*.step;*.stp;*.iges;*.igs)|*.step;*.stp;*.iges;*.igs|STEP files (*.step;*.stp)|*.step;*.stp|IGES files (*.iges;*.igs)|*.iges;*.igs|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
