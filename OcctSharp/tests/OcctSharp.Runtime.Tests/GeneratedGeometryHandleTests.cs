namespace OcctSharp.Runtime.Tests;

public sealed class GeneratedGeometryHandleTests
{
    [Fact]
    public void GeneratedCartesianPointsAndDirectionsPreserveSharingAndValues()
    {
        using Geom2dCartesianPoint point = new(2, 3);
        using Geom2dCartesianPoint pointClone = point.Clone();
        Assert.Equal(2, point.ReferenceCount);
        Assert.Equal(2, pointClone.ReferenceCount);
        pointClone.SetCoord(5, 7);
        Assert.Equal(5, point.X());
        Assert.Equal(7, point.Y());
        Assert.Equal("Geom2d_CartesianPoint", point.TypeName);
        Assert.True(point.IsKind("Geom2d_Point"));

        using Geom2dDirection direction2d = new(3, 4);
        using GeomDirection direction3d = new(2, 3, 6);
        Assert.Equal(1, direction2d.Magnitude(), 12);
        Assert.Equal(1, direction2d.SquareMagnitude(), 12);
        Assert.Equal(1, direction3d.Magnitude(), 12);
        Assert.Equal(1, direction3d.SquareMagnitude(), 12);
    }

    [Fact]
    public void GeneratedVectorsAndTransformationsExposeValueCopyOperations()
    {
        using Geom2dVectorWithMagnitude vector2d = new(3, 4);
        using GeomVectorWithMagnitude vector3d = new(2, 3, 6);
        using GeomVectorWithMagnitude fromPoints = new(
            new Point3d(1, 2, 3),
            new Point3d(3, 6, 9));
        Assert.Equal(5, vector2d.Magnitude(), 12);
        Assert.Equal(7, vector3d.Magnitude(), 12);
        Assert.Equal(Math.Sqrt(56), fromPoints.Magnitude(), 12);

        using Geom2dVectorWithMagnitude vector2dClone = vector2d.Clone();
        vector2dClone.Normalize();
        Assert.Equal(1, vector2d.Magnitude(), 12);
        vector2d.Multiply(6);
        Assert.Equal(6, vector2dClone.Magnitude(), 12);

        using GeomTransformation transform3d = new();
        using Geom2dTransformation transform2d = new();
        Assert.Equal(1, transform3d.ScaleFactor(), 12);
        Assert.Equal(1, transform2d.ScaleFactor(), 12);
        transform3d.SetTranslation(new Point3d(1, 2, 3), new Point3d(4, 6, 8));
        Assert.Equal(3, transform3d.Value(1, 4), 12);
        Assert.Equal(4, transform3d.Value(2, 4), 12);
        Assert.Equal(5, transform3d.Value(3, 4), 12);
        transform3d.Invert();
        Assert.Equal(-3, transform3d.Value(1, 4), 12);
        transform2d.Power(0);
        Assert.Equal(1, transform2d.ScaleFactor(), 12);
    }

    [Fact]
    public void GeneratedPlaneEvaluatesAndDisposedHandlesFailClosed()
    {
        using GeomPlane plane = new(0, 0, 1, 0);
        Point3d point = plane.EvalD0(2, 3);
        Assert.True(double.IsFinite(point.X));
        Assert.True(double.IsFinite(point.Y));
        Assert.Equal(0, point.Z, 12);
        Assert.False(plane.IsUClosed());
        Assert.False(plane.IsVClosed());
        Assert.True(plane.IsKind("Geom_Surface"));

        GeomTransformation disposed = new();
        disposed.Dispose();
        disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => disposed.ScaleFactor());
        using GeomTransformation transform = new();
        Assert.Throws<OcctException>(() => transform.Value(0, 0));
    }
}
