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

    /// <summary>Creates a conical solid with distinct non-negative radii and a positive height.</summary>
    public static Shape CreateCone(double bottomRadius, double topRadius, double height)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateCone(bottomRadius, topRadius, height, out nint nativeShape),
            "shape_create_cone");
        return FromNativeHandle(nativeShape, "shape_create_cone");
    }

    /// <summary>Creates a toroidal solid whose major radius exceeds its positive minor radius.</summary>
    public static Shape CreateTorus(double majorRadius, double minorRadius)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateTorus(majorRadius, minorRadius, out nint nativeShape),
            "shape_create_torus");
        return FromNativeHandle(nativeShape, "shape_create_torus");
    }

    /// <summary>Creates a right-angular wedge with positive dimensions and a non-negative top X length.</summary>
    public static Shape CreateWedge(double sizeX, double sizeY, double sizeZ, double topXLength)
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateWedge(sizeX, sizeY, sizeZ, topXLength, out nint nativeShape),
            "shape_create_wedge");
        return FromNativeHandle(nativeShape, "shape_create_wedge");
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

    /// <summary>Creates a full circular edge from a center, plane normal, and radius.</summary>
    public static Shape CreateCircleEdge(GpPoint center, GpPoint normal, double radius)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.CreateCircleEdge(ToRaw(center), ToRaw(normal), radius, out nint nativeShape),
            "shape_create_circle_edge");
        return FromNativeHandle(nativeShape, "shape_create_circle_edge");
    }

    /// <summary>Creates a circular arc passing through three ordered points.</summary>
    public static Shape CreateArcEdge(GpPoint start, GpPoint middle, GpPoint end)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.CreateArcEdge(ToRaw(start), ToRaw(middle), ToRaw(end), out nint nativeShape),
            "shape_create_arc_edge");
        return FromNativeHandle(nativeShape, "shape_create_arc_edge");
    }

    /// <summary>Creates a full elliptical edge with an explicit in-plane major-axis direction.</summary>
    public static Shape CreateEllipseEdge(
        GpPoint center,
        GpPoint normal,
        GpPoint xDirection,
        double majorRadius,
        double minorRadius)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.CreateEllipseEdge(
                ToRaw(center), ToRaw(normal), ToRaw(xDirection), majorRadius, minorRadius, out nint nativeShape),
            "shape_create_ellipse_edge");
        return FromNativeHandle(nativeShape, "shape_create_ellipse_edge");
    }

    /// <summary>Creates a Bezier edge from copied control poles.</summary>
    public static unsafe Shape CreateBezierEdge(IReadOnlyList<GpPoint> poles)
    {
        XyzRaw[] rawPoles = CopyPoints(poles, 2, "A Bezier edge requires at least two poles.");
        fixed (XyzRaw* pointer = rawPoles)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.CreateBezierEdge(pointer, rawPoles.Length, out nint nativeShape),
                "shape_create_bezier_edge");
            return FromNativeHandle(nativeShape, "shape_create_bezier_edge");
        }
    }

    /// <summary>Creates a B-spline edge interpolating copied points.</summary>
    public static unsafe Shape CreateInterpolatedEdge(
        IReadOnlyList<GpPoint> points,
        bool periodic = false,
        double tolerance = 1e-7)
    {
        XyzRaw[] rawPoints = CopyPoints(
            points,
            periodic ? 3 : 2,
            periodic
                ? "A periodic interpolated edge requires at least three points."
                : "An interpolated edge requires at least two points.");
        fixed (XyzRaw* pointer = rawPoints)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.CreateInterpolatedEdge(
                    pointer, rawPoints.Length, periodic ? 1 : 0, tolerance, out nint nativeShape),
                "shape_create_interpolated_edge");
            return FromNativeHandle(nativeShape, "shape_create_interpolated_edge");
        }
    }

    /// <summary>Builds a shell or solid through copied wire sections.</summary>
    public static unsafe Shape CreateLoft(
        IReadOnlyList<Shape> sections,
        bool makeSolid = false,
        bool ruled = false,
        double tolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(sections);
        if (sections.Count < 2) throw new ArgumentException("A loft requires at least two sections.", nameof(sections));
        return WithBorrowedShapeHandles(sections, (pointers, count) =>
        {
            NativeError.ThrowIfFailed(
                NativeMethods.CreateLoft(
                    pointers, count, makeSolid ? 1 : 0, ruled ? 1 : 0, tolerance, out nint nativeShape),
                "shape_create_loft");
            return FromNativeHandle(nativeShape, "shape_create_loft");
        });
    }

    /// <summary>Sweeps a profile shape along a G1-continuous wire spine.</summary>
    public static Shape CreatePipe(Shape spine, Shape profile)
    {
        ArgumentNullException.ThrowIfNull(spine);
        ArgumentNullException.ThrowIfNull(profile);
        ObjectDisposedException.ThrowIf(spine.Handle.IsClosed, spine);
        ObjectDisposedException.ThrowIf(profile.Handle.IsClosed, profile);
        NativeError.ThrowIfFailed(
            NativeMethods.CreatePipe(spine.Handle, profile.Handle, out nint nativeShape),
            "shape_create_pipe");
        return FromNativeHandle(nativeShape, "shape_create_pipe");
    }

    /// <summary>Sews copied topology inputs using one connectivity tolerance.</summary>
    public static unsafe Shape Sew(IReadOnlyList<Shape> shapes, double tolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        if (shapes.Count == 0) throw new ArgumentException("Sewing requires at least one shape.", nameof(shapes));
        return WithBorrowedShapeHandles(shapes, (pointers, count) =>
        {
            NativeError.ThrowIfFailed(
                NativeMethods.SewShapes(pointers, count, tolerance, out nint nativeShape),
                "shape_sew");
            return FromNativeHandle(nativeShape, "shape_sew");
        });
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

    internal static XyzRaw ToRaw(GpPoint point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");
        return new XyzRaw(point.X, point.Y, point.Z);
    }

    private static XyzRaw[] CopyPoints(IReadOnlyList<GpPoint> points, int minimumCount, string message)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < minimumCount) throw new ArgumentException(message, nameof(points));
        XyzRaw[] result = new XyzRaw[points.Count];
        for (int index = 0; index < points.Count; ++index) result[index] = ToRaw(points[index]);
        return result;
    }

    internal unsafe delegate Shape ShapeArrayOperation(nint* handles, int count);

    internal static unsafe Shape WithBorrowedShapeHandles(
        IReadOnlyList<Shape> shapes,
        ShapeArrayOperation operation)
    {
        nint[] pointers = new nint[shapes.Count];
        bool[] references = new bool[shapes.Count];
        int acquired = 0;
        try
        {
            for (; acquired < shapes.Count; ++acquired)
            {
                Shape shape = shapes[acquired] ?? throw new ArgumentException("A shape collection contains null.", nameof(shapes));
                ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
                shape.Handle.DangerousAddRef(ref references[acquired]);
                pointers[acquired] = shape.Handle.DangerousGetHandle();
            }

            fixed (nint* pointer = pointers) return operation(pointer, pointers.Length);
        }
        finally
        {
            for (int index = acquired - 1; index >= 0; --index)
                if (references[index]) shapes[index].Handle.DangerousRelease();
        }
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
