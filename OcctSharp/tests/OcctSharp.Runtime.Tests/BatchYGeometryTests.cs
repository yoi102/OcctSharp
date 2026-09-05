using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.Generated;
using OcctSharp.Interop;
using OcctSharp.Tests.Shared;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchYGeometryTests
{
    [Fact]
    public void PublicGeometricWorkflowPreservesValuesOrientationAndCopies() => BatchYGeometryWorkflow.Run();

    [Fact]
    public void DocumentsAndXdeRetainCopiedCoordinatesAndAnnotationPlanes()
    {
        using TObjTXYZ attribute = new();
        attribute.Set(new V.Coordinates3d(3,-5,7));
        V.Coordinates3d saved = attribute.Get();
        attribute.Set(new V.Coordinates3d(11,13,17));
        attribute.Dispose();
        Assert.Equal(new V.Coordinates3d(3,-5,7), saved);
        using XCAFNoteObjectsNoteObject note = new();
        note.SetPlane(BatchYGeometryWorkflow.Frame);
        V.Axis2 plane = note.GetPlane();
        note.Dispose();
        Assert.Equal(BatchYGeometryWorkflow.Frame, plane);
    }

    [Fact]
    public void IgesRecordsAndApproximationNodesUseCopiedCoordinates()
    {
        using IGESGeomLine line = new();
        line.Init(new V.Coordinates3d(1,2,3), new V.Coordinates3d(4,6,8));
        Assert.Equal(new Point3d(1,2,3), line.StartPoint());
        Assert.Equal(new Point3d(4,6,8), line.EndPoint());
        using IGESGeomDirection direction = new();
        direction.Init(new V.Coordinates3d(2,3,6));
        Assert.Equal(new V.Vector3d(2,3,6), direction.Value());
        using AdvApp2VarNode node = new(new V.Coordinates2d(3,5), 0, 0);
        V.Coordinates2d center = node.Coord();
        node.SetCoord(7,9);
        Assert.Equal(new V.Coordinates2d(7,9), node.Coord());
        node.Dispose();
        Assert.Equal(new V.Coordinates2d(3,5), center);
    }

    [Fact]
    public void AllThirtyManagedRecordsHaveSequentialBlittableLayouts()
    {
        (Type Type, int Size)[] expected =
        [
            (typeof(V.Coordinates2d),16), (typeof(V.Coordinates3d),24), (typeof(V.Point2d),16),
            (typeof(V.Vector2d),16), (typeof(V.Vector3d),24), (typeof(V.Direction2d),16), (typeof(V.Direction3d),24),
            (typeof(V.Axis1),48), (typeof(V.Axis2),96), (typeof(V.Axis3),96), (typeof(V.Axis2d),32), (typeof(V.Axis22d),48),
            (typeof(V.Matrix2x2),32), (typeof(V.Matrix3x3),72), (typeof(V.Quaternion),32),
            (typeof(V.Line2d),32), (typeof(V.Line3d),48), (typeof(V.Circle2d),56), (typeof(V.Circle3d),104),
            (typeof(V.Ellipse2d),64), (typeof(V.Ellipse3d),112), (typeof(V.Hyperbola2d),64), (typeof(V.Hyperbola3d),112),
            (typeof(V.Parabola2d),56), (typeof(V.Parabola3d),104), (typeof(V.Plane),96), (typeof(V.Cylinder),104),
            (typeof(V.Cone),112), (typeof(V.Sphere),104), (typeof(V.Torus),112),
        ];
        foreach ((Type type, int size) in expected)
        {
            Assert.Equal(LayoutKind.Sequential, type.StructLayoutAttribute!.Value);
            Assert.Equal(size, Marshal.SizeOf(type));
            int offset = 0;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Assert.Equal(offset, Marshal.OffsetOf(type, field.Name).ToInt32());
                Assert.True(field.FieldType == typeof(double) || expected.Any(e => e.Type == field.FieldType));
                offset += Marshal.SizeOf(field.FieldType);
            }
            Assert.Equal(size, offset);
        }
    }

    [Fact]
    public void StaticGeometryEvaluatesDerivativesAndCopiesConstantAxes()
    {
        Assert.Equal(new V.Vector3d(0, 4, 0), GeometryGeneratedNativeMethods.ElCLibStaticCircleDN0(0, BatchYGeometryWorkflow.Frame, 4, 1));
        Assert.Equal(new V.Vector2d(0, -4), GeometryGeneratedNativeMethods.ElCLibStaticCircleDN1(0, BatchYGeometryWorkflow.Frame2d, 4, 1));
        Assert.Equal(new V.Point2d(7, -2), GeometryGeneratedNativeMethods.ElCLibStaticCircleValue1(0, BatchYGeometryWorkflow.Frame2d, 4));
        Assert.Equal(new V.Axis1(new(0,0,0), new(1,0,0)), GeometryGeneratedNativeMethods.GpStaticOX0());
        Assert.Equal(new V.Axis2d(new(0,0), new(1,0)), GeometryGeneratedNativeMethods.GpStaticOX2d0());
        V.Quaternion q = GeometryGeneratedNativeMethods.GpQuaternionSLerpStaticInterpolate0(new(0,0,0,1), new(0,0,1,0), 0.5);
        Assert.Equal(Math.Sqrt(0.5), q.Z, 12);
        Assert.Equal(Math.Sqrt(0.5), q.W, 12);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFiniteNestedCoordinatesRejectWithoutPublishingAnOwner(double bad)
    {
        Assert.Throws<OcctException>(() => new GeomLine(new V.Line3d(new(new(bad,0,0), new(1,0,0)))));
        using GeomLine valid = new(new V.Line3d(new(new(1,2,3), new(1,0,0))));
        Assert.Equal(new V.Coordinates3d(1,2,3), valid.Lin().Position.Origin);
        Assert.Equal(string.Empty, Marshal.PtrToStringUTF8(RuntimeNativeMethods.GetLastError()));
    }

    [Fact]
    public void LargeFiniteDirectionsNormalizeWithoutOverflow()
    {
        using GeomLine line = new(new V.Line3d(new(new(0,0,0), new(double.MaxValue, double.MaxValue,0))));
        V.Direction3d d = line.Lin().Position.Direction;
        Assert.Equal(Math.Sqrt(0.5), d.X, 12);
        Assert.Equal(Math.Sqrt(0.5), d.Y, 12);
        using Geom2dLine line2d = new(new V.Point2d(0,0), new V.Direction2d(double.MaxValue, double.MaxValue));
        Assert.Equal(Math.Sqrt(0.5), line2d.Direction().X, 12);
    }

    [Fact]
    public void InvalidDirectionsFramesAndPrimitiveDomainsReject()
    {
        Assert.Throws<OcctException>(() => new GeomLine(new V.Line3d(new(new(0,0,0), new(0,0,0)))));
        Assert.Throws<OcctException>(() => new GeomCircle(new V.Circle3d(BatchYGeometryWorkflow.Frame with { YDirection = new(0,-1,0) }, 2)));
        Assert.Throws<OcctException>(() => new GeomCircle(new V.Circle3d(BatchYGeometryWorkflow.Frame with { XDirection = new(0,0,1) }, 2)));
        Assert.Throws<OcctException>(() => new Geom2dCircle(BatchYGeometryWorkflow.Frame2d with { YDirection = new(1,0) }, 2));
        Assert.Throws<OcctException>(() => new GeomCircle(new V.Circle3d(BatchYGeometryWorkflow.Frame, -1)));
        Assert.Throws<OcctException>(() => new GeomEllipse(new V.Ellipse3d(BatchYGeometryWorkflow.Frame, 1, 3)));
    }

    [Fact]
    public void CopiedReferenceIsIndependentOfReceiverMutationAndDisposal()
    {
        using Graphic3dCamera camera = new();
        camera.SetDirection(new V.Direction3d(0,0,-1));
        V.Direction3d saved = camera.Direction();
        camera.SetDirection(new V.Direction3d(1,0,0));
        camera.Dispose();
        Assert.Equal(new V.Direction3d(0,0,-1), saved);
        Assert.Throws<ObjectDisposedException>(() => camera.Direction());
    }

    [Fact]
    public void GeometricOutputRejectsNullAndZeroesOutputOnConversionFailure()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchYGeometryTests).Assembly);
        Assert.Equal(NativeStatus.InvalidArgument, GetAxis(nint.Zero));
        Assert.Equal(NativeStatus.OcctFailure, CircleDerivative(0, default, 4, 1, out V.Vector3d result));
        Assert.Equal(default, result);
    }

    [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_generated_gp_static_ox_0")]
    private static partial NativeStatus GetAxis(nint result);
    [DllImport("OcctSharp.Native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "occtsharp_generated_el_clib_circle_dn_static_circle_dn_0")]
    private static extern NativeStatus CircleDerivative(double u, V.Axis2 axis, double radius, int n, out V.Vector3d result);
}
