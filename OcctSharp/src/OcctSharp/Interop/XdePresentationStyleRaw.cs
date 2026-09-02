using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct XdePresentationStyleRaw(
    int IsVisible,
    int HasSurfaceColor,
    int HasCurveColor,
    int HasMaterialColor,
    XdeColorRaw SurfaceColor,
    XdeColorRaw CurveColor,
    XdeColorRaw MaterialColor);
