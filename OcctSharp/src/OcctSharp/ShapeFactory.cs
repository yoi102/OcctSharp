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

    /// <summary>Creates an explicit null topology value for diagnostic and validation tests.</summary>
    public static Shape CreateNull()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.CreateNullShape(out nint nativeShape), "shape_create_null");
        return FromNativeHandle(nativeShape, "shape_create_null");
    }

    /// <summary>Creates a spherical solid with a positive finite radius.</summary>
    public static Shape CreateSphere(double radius)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.CreateSphere(radius, out nint nativeShape), "shape_create_sphere");
        return FromNativeHandle(nativeShape, "shape_create_sphere");
    }

    /// <summary>Creates a cylindrical solid with positive finite radius and height.</summary>
    public static Shape CreateCylinder(double radius, double height)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.CreateCylinder(radius, height, out nint nativeShape), "shape_create_cylinder");
        return FromNativeHandle(nativeShape, "shape_create_cylinder");
    }

    /// <summary>Creates a straight owning edge between two distinct finite points.</summary>
    public static Shape CreateEdge(GpPoint start, GpPoint end)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateEdge(ToRaw(start), ToRaw(end), out nint nativeShape),
            "shape_create_edge");
        return FromNativeHandle(nativeShape, "shape_create_edge");
    }

    /// <summary>Creates an owning polygon wire from copied point values.</summary>
    public static unsafe Shape CreatePolygonWire(IReadOnlyList<GpPoint> points, bool close = false)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 2) throw new ArgumentException("A polygon wire requires at least two points.", nameof(points));
        XyzRaw[] rawPoints = new XyzRaw[points.Count];
        for (int index = 0; index < points.Count; ++index) rawPoints[index] = ToRaw(points[index]);
        fixed (XyzRaw* pointPointer = rawPoints)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.CreatePolygonWire(pointPointer, rawPoints.Length, close ? 1 : 0, out nint nativeShape),
                "shape_create_polygon_wire");
            return FromNativeHandle(nativeShape, "shape_create_polygon_wire");
        }
    }

    /// <summary>Creates an owning planar face from a closed planar wire.</summary>
    public static Shape CreatePlanarFace(Shape wire)
    {
        ArgumentNullException.ThrowIfNull(wire);
        wire.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.CreatePlanarFace(wire.Handle, out nint nativeShape), "shape_create_planar_face");
        return FromNativeHandle(nativeShape, "shape_create_planar_face");
    }

    private static XyzRaw ToRaw(GpPoint point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");
        return new XyzRaw(point.X, point.Y, point.Z);
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
