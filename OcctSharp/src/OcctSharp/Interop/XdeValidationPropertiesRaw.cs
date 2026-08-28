using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct XdeValidationPropertiesRaw(
    double Area,
    double Volume,
    XyzRaw Centroid,
    int HasArea,
    int HasVolume,
    int HasCentroid);
