using System.Windows;

namespace OcctSharpViewer.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length != 2 || e.Args[0] != "--snapshot-smoke") { base.OnStartup(e); new MainWindow().Show(); return; }
        base.OnStartup(e);
        var window = new MainWindow { WindowStartupLocation = WindowStartupLocation.Manual, Left = -32000, Top = -32000 };
        MainWindow = window;
        window.ContentRendered += (_, _) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            try { window.RunSnapshotSmoke(System.IO.Path.GetFullPath(e.Args[1])); Shutdown(0); }
            catch (Exception error) { Console.Error.WriteLine(error); Shutdown(1); }
        }));
        window.Show();
    }
}
