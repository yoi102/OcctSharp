using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OcctSharp;

namespace OcctSharpViewer.Wpf.Services;

/// <summary>Sample-only CPU copy adapter. No native, D3D or live OpenGL resource enters WPF.</summary>
public static class ViewerSnapshotBitmap
{
    public static WriteableBitmap Create(ViewerColorFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);
        byte[] pixels = frame.CopyOpaqueBgra();
        bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Stride, 0);
        bitmap.Freeze(); return bitmap;
    }
}
