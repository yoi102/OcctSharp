using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NativeStepAssemblyInput
{
    internal NativeStepAssemblyInput(nint filePath, ShapeTransform transform)
    {
        FilePath = filePath;
        TranslationX = transform.TranslationX;
        TranslationY = transform.TranslationY;
        TranslationZ = transform.TranslationZ;
        RotationAxisX = transform.RotationAxisX;
        RotationAxisY = transform.RotationAxisY;
        RotationAxisZ = transform.RotationAxisZ;
        RotationAngleRadians = transform.RotationAngleRadians;
    }

    internal readonly nint FilePath;
    internal readonly double TranslationX;
    internal readonly double TranslationY;
    internal readonly double TranslationZ;
    internal readonly double RotationAxisX;
    internal readonly double RotationAxisY;
    internal readonly double RotationAxisZ;
    internal readonly double RotationAngleRadians;
}
