using V = OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchYGeometryWorkflow
{
    internal static readonly V.Axis2 Frame = new(new(3, -2, 5), new(0, 0, 1), new(1, 0, 0), new(0, 1, 0));
    internal static readonly V.Axis3 Indirect = new(Frame.Origin, Frame.Normal, Frame.XDirection, new(0, -1, 0));
    internal static readonly V.Axis22d Frame2d = new(new(3, -2), new(1, 0), new(0, -1));

    internal static void Run()
    {
        using GeomLine line = new(new V.Line3d(new(Frame.Origin, new(0, 0, 7))));
        V.Line3d saved = line.Lin();
        Equal(new V.Direction3d(0, 0, 1), saved.Position.Direction);
        line.SetLin(new(new(new(9, 8, 7), new(1, 0, 0))));
        line.Dispose();
        Equal(Frame.Origin, saved.Position.Origin);

        using Geom2dLine line2d = new(new V.Point2d(3, 4), new V.Direction2d(0, -8));
        Equal(new V.Line2d(new(new(3, 4), new(0, -1))), line2d.Lin2d());
        line2d.SetLin2d(new(new(new(7, 9), new(1, 0))));
        Equal(new V.Point2d(7, 9), line2d.Lin2d().Position.Origin);

        V.Circle3d circleValue = new(Frame, 4);
        using GeomCircle circle = new(circleValue);
        Equal(circleValue, circle.Circ());
        using GeomEllipse ellipse = new(new V.Ellipse3d(Frame, 6, 2));
        Equal(new V.Ellipse3d(Frame, 6, 2), ellipse.Elips());
        using GeomHyperbola hyperbola = new(new V.Hyperbola3d(Frame, 6, 2));
        Equal(new V.Hyperbola3d(Frame, 6, 2), hyperbola.Hypr());
        using GeomParabola parabola = new(new V.Parabola3d(Frame, 2));
        Equal(new V.Parabola3d(Frame, 2), parabola.Parab());

        using Geom2dCircle circle2d = new(Frame2d, 4);
        Equal(new V.Circle2d(Frame2d, 4), circle2d.Circ2d());
        using Geom2dEllipse ellipse2d = new(Frame2d, 6, 2);
        Equal(new V.Ellipse2d(Frame2d, 6, 2), ellipse2d.Elips2d());
        using Geom2dHyperbola hyperbola2d = new(new V.Hyperbola2d(Frame2d, 6, 2));
        Equal(new V.Hyperbola2d(Frame2d, 6, 2), hyperbola2d.Hypr2d());
        using Geom2dParabola parabola2d = new(new V.Parabola2d(Frame2d, 2));
        Equal(new V.Parabola2d(Frame2d, 2), parabola2d.Parab2d());

        using GeomPlane plane = new(new V.Plane(Indirect));
        Equal(new V.Plane(Indirect), plane.Pln());
        using GeomCylindricalSurface cylinder = new(new V.Cylinder(Indirect, 4));
        Equal(new V.Cylinder(Indirect, 4), cylinder.Cylinder());
        using GeomConicalSurface cone = new(new V.Cone(Indirect, 0.25, 4));
        Equal(new V.Cone(Indirect, 0.25, 4), cone.Cone());
        using GeomSphericalSurface sphere = new(new V.Sphere(Indirect, 4));
        Equal(new V.Sphere(Indirect, 4), sphere.Sphere());
        using GeomToroidalSurface torus = new(new V.Torus(Indirect, 6, 2));
        V.Torus copiedTorus = torus.Torus();
        torus.Dispose();
        Equal(new V.Torus(Indirect, 6, 2), copiedTorus);

        using GeomVectorWithMagnitude vector = new(new V.Vector3d(2, -3, 5));
        Equal(38d, vector.SquareMagnitude());
        using Geom2dVectorWithMagnitude vector2d = new(new V.Vector2d(-3, 5));
        Equal(34d, vector2d.SquareMagnitude());
    }

    private static void Equal<T>(T expected, T actual) where T : IEquatable<T>
    {
        if (!expected.Equals(actual))
            throw new InvalidOperationException($"Geometric copy mismatch for {typeof(T).Name}: expected {expected}, got {actual}.");
    }
}
