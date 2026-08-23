using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Creates common OCCT topology shapes.</summary>
public static class ShapeFactory
{
    /// <summary>Creates an axis-aligned box with positive finite dimensions.</summary>
    public static Shape CreateBox(double sizeX, double sizeY, double sizeZ)
    {
        OcctRuntime.EnsureCompatible();

        NativeStatus status = NativeMethods.CreateBox(sizeX, sizeY, sizeZ, out nint nativeShape);
        NativeError.ThrowIfFailed(status, "shape_create_box");

        return FromNativeHandle(nativeShape, "shape_create_box");
    }

    internal static Shape FromNativeHandle(nint nativeShape, string operation)
    {
        if (nativeShape == 0)
        {
            throw new OcctException(
                NativeStatus.UnknownException.ToString(),
                $"The native bridge reported success for '{operation}' but returned a null shape handle.");
        }

        return new Shape(new ShapeHandle(nativeShape));
    }
}
