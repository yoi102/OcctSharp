using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchZValueWorkflow
{
    internal static void Run()
    {
        V.Transform3d translation = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref translation, new V.Vector3d(3, -2, 5));
        Point3d original = Point3dOperations.Create(new V.Coordinates3d(1, 2, 3));
        Point3d moved = original;
        Point3dOperations.Transform(ref moved, translation);
        Equal(new Point3d(4, 0, 8), moved);
        Equal(new Point3d(1, 2, 3), original);
        Transform3dOperations.Invert(ref translation);
        Point3dOperations.Transform(ref moved, translation);
        Near(original.X, moved.X); Near(original.Y, moved.Y); Near(original.Z, moved.Z);

        V.Transform2d rotation = Transform2dOperations.Create();
        Transform2dOperations.SetRotation(ref rotation, new V.Point2d(0, 0), Math.PI / 2);
        V.Point2d rotated = Point2dOperations.Transformed(new(2, 0), rotation);
        Near(0, rotated.X); Near(2, rotated.Y);

        V.AffineTransform2d shear = AffineTransform2dOperations.Create(new V.Matrix2x2(1, 2, 0, 1), new V.Coordinates2d(3, 5));
        V.Coordinates2d transformed = AffineTransform2dOperations.Transformed(shear, new(7, 11));
        Equal(new V.Coordinates2d(32, 16), transformed);
        Equal(new V.Coordinates2d(7, 11), AffineTransform2dOperations.Transformed(AffineTransform2dOperations.Inverted(shear), transformed));

        V.Axis2 frame = Axis2Operations.Create();
        V.Circle3d circleValue = Circle3dOperations.Create(frame, 4);
        V.Transform3d scale = Transform3dOperations.Create();
        Transform3dOperations.SetScale(ref scale, new Point3d(0, 0, 0), 2);
        Circle3dOperations.Transform(ref circleValue, scale);
        using GeomCircle circle = new(circleValue);
        Near(8, circle.Radius());
        circle.Transform(translation);
        V.Circle3d copied = circle.Circ();
        circle.Dispose();
        Equal(new Point3d(-3, 2, -5), Circle3dOperations.Location(copied));
        Near(8, Circle3dOperations.Radius(copied));
    }

    private static void Equal<T>(T expected, T actual) where T : IEquatable<T>
    {
        if (!expected.Equals(actual)) throw new InvalidOperationException($"Batch Z expected {expected}, got {actual}.");
    }

    private static void Near(double expected, double actual)
    {
        if (!double.IsFinite(actual) || Math.Abs(expected - actual) > 1e-10)
            throw new InvalidOperationException($"Batch Z expected {expected}, got {actual}.");
    }
}
