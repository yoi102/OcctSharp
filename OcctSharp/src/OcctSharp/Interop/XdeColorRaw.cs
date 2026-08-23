using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct XdeColorRaw(double Red, double Green, double Blue, double Alpha);
