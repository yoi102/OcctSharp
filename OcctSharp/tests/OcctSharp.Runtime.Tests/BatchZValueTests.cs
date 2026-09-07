using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.Tests.Shared;
using OcctSharp.Interop;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchZValueTests
{
    [Fact]
    public void PublicValueWorkflowRunsThroughCopiedAndSharedGeometry() => BatchZValueWorkflow.Run();

    [Theory]
    [InlineData(typeof(V.Transform2d), 48)]
    [InlineData(typeof(V.Transform3d), 96)]
    [InlineData(typeof(V.AffineTransform2d), 48)]
    [InlineData(typeof(V.AffineTransform3d), 96)]
    public void TransformLayoutsAreExplicitRowMajorDoubles(Type type, int size)
    {
        Assert.Equal(LayoutKind.Sequential, type.StructLayoutAttribute!.Value);
        Assert.Equal(size, Marshal.SizeOf(type));
        int offset = 0;
        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            Assert.Equal(typeof(double), field.FieldType);
            Assert.Equal(offset, Marshal.OffsetOf(type, field.Name).ToInt32());
            offset += 8;
        }
        Assert.Equal(size, offset);
    }

    [Fact]
    public void FailedMutationsPreserveTheReceiver()
    {
        V.Matrix3x3 matrix = new(1,2,3, 2,4,6, 3,6,9);
        V.Matrix3x3 savedMatrix = matrix;
        Assert.Throws<OcctException>(() => Matrix3x3Operations.Invert(ref matrix));
        Assert.Equal(savedMatrix, matrix);
        V.Circle3d circle = Circle3dOperations.Create(Axis2Operations.Create(), 4);
        V.Circle3d savedCircle = circle;
        Assert.Throws<OcctException>(() => Circle3dOperations.SetRadius(ref circle, -1));
        Assert.Equal(savedCircle, circle);
        Point3d point = new(1,2,3);
        Assert.Throws<OcctException>(() => Point3dOperations.SetCoord(ref point, 0, 99));
        Assert.Equal(new Point3d(1,2,3), point);
        Assert.Throws<OcctException>(() => Point3dOperations.SetX(ref point, double.NaN));
        Assert.Equal(new Point3d(1,2,3), point);
    }

    [Fact]
    public void SingularAffineValuesRemainRepresentableButNotInvertible()
    {
        V.AffineTransform2d a2 = new(1,2,3, 2,4,5);
        V.AffineTransform3d a3 = new(1,2,3,4, 2,4,6,7, 3,6,9,10);
        Assert.True(AffineTransform2dOperations.IsSingular(a2));
        Assert.True(AffineTransform3dOperations.IsSingular(a3));
        V.AffineTransform2d saved2 = a2;
        V.AffineTransform3d saved3 = a3;
        Assert.Throws<OcctException>(() => AffineTransform2dOperations.Invert(ref a2));
        Assert.Throws<OcctException>(() => AffineTransform3dOperations.Invert(ref a3));
        Assert.Equal(saved2, a2); Assert.Equal(saved3, a3);
    }

    [Fact]
    public void SimilarityInputsRejectShearAndNonfiniteCoefficients()
    {
        Assert.Throws<OcctException>(() => Transform2dOperations.Value(new(1,2,0, 0,1,0), 1, 1));
        Assert.Throws<OcctException>(() => Transform3dOperations.Value(new(1,2,0,0, 0,1,0,0, 0,0,1,0), 1, 1));
        Assert.Throws<OcctException>(() => Transform2dOperations.Value(new(1,0,double.PositiveInfinity, 0,1,0), 1, 1));
        Assert.Throws<OcctException>(() => Transform3dOperations.Value(default, 1, 1));
    }

    [Theory]
    [InlineData(-3)]
    [InlineData(0.25)]
    [InlineData(2)]
    public void SimilarityScaleReflectionCompositionAndInversePreservePoints(double factor)
    {
        V.Transform3d scale = Transform3dOperations.Create();
        Transform3dOperations.SetScale(ref scale, new Point3d(0,0,0), factor);
        V.Transform3d shift = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref shift, new V.Vector3d(3,5,7));
        V.Transform3d composed = Transform3dOperations.Multiplied(shift, scale);
        Point3d point = new(2,4,6);
        Point3dOperations.Transform(ref point, composed);
        Assert.Equal(2*factor+3, point.X, 10);
        Assert.Equal(4*factor+5, point.Y, 10);
        Assert.Equal(6*factor+7, point.Z, 10);
        Transform3dOperations.Invert(ref composed);
        Point3dOperations.Transform(ref point, composed);
        Assert.Equal(2, point.X, 10); Assert.Equal(4, point.Y, 10); Assert.Equal(6, point.Z, 10);
        Assert.Equal(factor < 0, Transform3dOperations.IsNegative(scale));
    }

    [Fact]
    public void CoordinatesVectorsDirectionsAndMatricesUseIndependentCopies()
    {
        V.Vector3d vector = Vector3dOperations.Create(2,3,6);
        V.Vector3d unit = Vector3dOperations.Normalized(vector);
        Assert.Equal(new V.Vector3d(2,3,6), vector);
        Assert.Equal(2.0/7, unit.X, 12);
        V.Direction3d direction = Direction3dOperations.Create(new V.Coordinates3d(vector.X, vector.Y, vector.Z));
        Assert.Equal(6.0/7, direction.Z, 12);
        Assert.Equal(new V.Coordinates3d(23,53,88), Coordinates3dOperations.Multiplied(new(2,3,5), new V.Matrix3x3(1,2,3, 4,5,6, 7,8,10)));
        Assert.Equal(new V.Coordinates2d(29,76), Coordinates2dOperations.Multiplied(new(7,11), new V.Matrix2x2(1,2,3,5)));
    }

    [Theory]
    [InlineData("Circle3d")]
    [InlineData("Ellipse3d")]
    [InlineData("Hyperbola3d")]
    [InlineData("Parabola3d")]
    [InlineData("Plane")]
    [InlineData("Cylinder")]
    [InlineData("Cone")]
    [InlineData("Sphere")]
    [InlineData("Torus")]
    public void ConicsAndSurfacesTranslateWithIndependentValueReceivers(string family)
    {
        V.Axis2 frame = Axis2Operations.Create();
        V.Axis3 surfaceFrame = Axis3Operations.Create();
        object original = family switch
        {
            "Circle3d" => Circle3dOperations.Create(frame, 4),
            "Ellipse3d" => Ellipse3dOperations.Create(frame, 6, 2),
            "Hyperbola3d" => Hyperbola3dOperations.Create(frame, 6, 2),
            "Parabola3d" => Parabola3dOperations.Create(frame, 2),
            "Plane" => PlaneOperations.Create(surfaceFrame),
            "Cylinder" => CylinderOperations.Create(surfaceFrame, 4),
            "Cone" => ConeOperations.Create(surfaceFrame, 0.25, 4),
            "Sphere" => SphereOperations.Create(surfaceFrame, 4),
            "Torus" => TorusOperations.Create(surfaceFrame, 6, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(family)),
        };
        Type operations = typeof(Point3dOperations).Assembly.GetType("OcctSharp.ValueOperations." + family + "Operations")!;
        MethodInfo translate = operations.GetMethod("Translate", [original.GetType().MakeByRefType(), typeof(V.Vector3d)])!;
        MethodInfo location = operations.GetMethod("Location", [original.GetType()])!;
        object?[] args = [original, new V.Vector3d(3,5,7)];
        translate.Invoke(null, args);
        Assert.Equal(new Point3d(3,5,7), location.Invoke(null, [args[0]]));
        Assert.Equal(new Point3d(0,0,0), location.Invoke(null, [original]));
    }

    [Fact]
    public void AffineCompositionInverseAndSimilarityConversionKeepCoefficients()
    {
        V.AffineTransform3d shear = AffineTransform3dOperations.Create(new V.Matrix3x3(1,2,0, 0,1,0, 0,0,1), new V.Coordinates3d(3,5,7));
        Assert.Equal(2, AffineTransform3dOperations.Value(shear, 1, 2));
        V.AffineTransform3d identity = AffineTransform3dOperations.Multiplied(shear, AffineTransform3dOperations.Inverted(shear));
        Assert.Equal(1, identity.M11, 12); Assert.Equal(0, identity.M12, 12); Assert.Equal(0, identity.M14, 12);
        V.Transform3d similarity = AffineTransform3dOperations.Trsf(identity);
        Assert.Equal(1, similarity.M33, 12);
        Assert.Throws<OcctException>(() => AffineTransform3dOperations.Trsf(shear));
        V.Transform2d identity2 = AffineTransform2dOperations.Trsf2d(AffineTransform2dOperations.Create());
        Assert.Equal(1, identity2.M11);
    }

    [Fact]
    public void QuaternionRotationAndAxisOrientationSurviveCopies()
    {
        V.Quaternion quaternion = new(0,0,Math.Sqrt(0.5),Math.Sqrt(0.5));
        V.Vector3d rotated = QuaternionOperations.Multiply(quaternion, new V.Vector3d(2,0,0));
        Assert.Equal(0, rotated.X, 12); Assert.Equal(2, rotated.Y, 12);
        V.Axis3 frame = Axis3Operations.Create();
        Axis3Operations.YReverse(ref frame);
        Assert.False(Axis3Operations.Direct(frame));
        V.Transform3d translation = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref translation, new V.Vector3d(3,5,7));
        Axis3Operations.Transform(ref frame, translation);
        Assert.False(Axis3Operations.Direct(frame));
        Assert.Equal(new Point3d(3,5,7), Axis3Operations.Location(frame));
    }

    [Fact]
    public unsafe void RawNullOutputAndFailuresSetErrorAndSuccessClearsIt()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchZValueTests).Assembly);
        Assert.Equal(1, SetPointX(new(1,2,3), 4, null));
        Point3d updated;
        Assert.NotEqual(0, SetPointX(new(1,2,3), double.NaN, &updated));
        Assert.Equal(default, updated);
        Assert.Equal(0, SetPointX(new(1,2,3), 4, &updated));
        Assert.Equal(new Point3d(4,2,3), updated);
        Assert.Equal(string.Empty, Marshal.PtrToStringUTF8(GetLastError()));
    }

    [DllImport("OcctSharp.Native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "occtsharp_value_gp_pnt_setx_0")]
    private static unsafe extern int SetPointX(Point3d receiver, double value, Point3d* updated);

    [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_get_last_error")]
    private static partial nint GetLastError();
}
