using OcctSharp.ReferenceOperations;
using OcctSharp.ValueOperations;
using OcctSharp.Interop;
using System.Runtime.InteropServices;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchAAReferenceTests
{
    [Fact]
    public void SharedConsumerWorkflowRuns() => OcctSharp.Tests.Shared.BatchAAReferenceWorkflow.Run();

    [Fact]
    public void ColorConversionRoundTripsThreeScalarOutputs()
    {
        double h = 0, l = 0, s = 0;
        QuantityColorReferences.RgbHls(0.2, 0.4, 0.7, ref h, ref l, ref s);
        double r = 0, g = 0, b = 0;
        QuantityColorReferences.HlsRgb(h, l, s, ref r, ref g, ref b);
        // The pinned Quantity_Color implementation converts through Vec3<float>.
        Assert.Equal(0.2, r, 6); Assert.Equal(0.4, g, 6); Assert.Equal(0.7, b, 6);
    }

    [Fact]
    public void PlaneAndCylinderPublishCompleteCoefficientSets()
    {
        V.Plane plane = PlaneOperations.Create(Axis3Operations.Create());
        double a = 9, b = 9, c = 9, d = 9;
        GpPlnReferences.Coefficients(plane, ref a, ref b, ref c, ref d);
        Assert.Equal((0d, 0d, 1d, 0d), (a, b, c, d));
        using GeomCylindricalSurface cylinder = new(CylinderOperations.Create(Axis3Operations.Create(), 4));
        double e = 9, f = 9, g = 9, h = 9, i = 9, j = 9;
        GeomCylindricalSurfaceReferences.Coefficients(cylinder, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref i, ref j);
        Assert.Equal(new[] { 1d, 1d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, -16d }, new[] { a, b, c, d, e, f, g, h, i, j });
    }

    [Fact]
    public void XdeEnumQueryAcceptsAnInitializedEnumOutput()
    {
        using TCollectionHAsciiString empty = new();
        XCAFDimTolObjectsDatumTargetType type = default;
        Assert.False(STEPCAFControlGDTPropertyReferences.GetDatumTargetType(empty, ref type));
    }

    [Fact]
    public unsafe void RawSharedReceiverValidationPreservesNativeStatus()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchAAReferenceTests).Assembly);
        Point3d result = new(7, 11, 13);
        Assert.Equal(2, CurvePoint(0, 0, result, &result));
        Assert.Equal(default, result);
        Assert.Equal(8, CurvePoint(1, 0, result, &result));
        Assert.Equal(default, result);
    }

    [DllImport("OcctSharp.Native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "occtsharp_ref_geom_curve_d0_0")]
    private static unsafe extern int CurvePoint(nint receiver, double parameter, Point3d input, Point3d* output);

    [Fact]
    public unsafe void RawOutputsAreValidatedInitializedAndClearErrorsAfterSuccess()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchAAReferenceTests).Assembly);
        double x = 7, y = 11, z = 13;
        Assert.Equal(1, PointCoordinates(new(1, 2, 3), x, null, y, &y, z, &z));
        Assert.Equal((7d, 11d, 13d), (x, y, z));
        Assert.NotEqual(0, PointCoordinates(new(double.NaN, 2, 3), x, &x, y, &y, z, &z));
        Assert.Equal((0d, 0d, 0d), (x, y, z));
        Assert.Equal(0, PointCoordinates(new(1, 2, 3), x, &x, y, &y, z, &z));
        Assert.Equal((1d, 2d, 3d), (x, y, z));
        Assert.Equal(string.Empty, Marshal.PtrToStringUTF8(GetLastError()));
    }

    [DllImport("OcctSharp.Native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "occtsharp_ref_gp_pnt_coord_0")]
    private static unsafe extern int PointCoordinates(Point3d receiver, double x, double* updatedX, double y, double* updatedY, double z, double* updatedZ);

    [DllImport("OcctSharp.Native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "occtsharp_get_last_error")]
    private static extern nint GetLastError();

    [Fact]
    public void MissingDrawerValueReturnsFalseAndPreservesInitializedCopy()
    {
        using MeshVSMesh mesh = new(false);
        using MeshVSDrawer drawer = mesh.GetDrawer()!;
        bool value = true;
        Assert.False(MeshVSDrawerReferences.GetBoolean(drawer, int.MaxValue, ref value));
        Assert.True(value);
        drawer.SetBoolean(int.MaxValue, false);
        Assert.True(MeshVSDrawerReferences.GetBoolean(drawer, int.MaxValue, ref value));
        Assert.False(value);
    }

    [Fact]
    public void CircleThirdDerivativesMatchAnalyticValues()
    {
        using GeomCylindricalSurface cylinder = new(CylinderOperations.Create(Axis3Operations.Create(), 4));
        using GeomCurve circle = cylinder.VIso(0)!;
        Point3d point = new(10, 20, 30);
        V.Vector3d first = default, second = default, third = default;
        GeomCurveReferences.D3(circle, 0, ref point, ref first, ref second, ref third);
        Assert.Equal(new Point3d(4, 0, 0), point);
        Assert.Equal(new V.Vector3d(0, 4, 0), first);
        Assert.Equal(new V.Vector3d(-4, 0, 0), second);
        Assert.Equal(new V.Vector3d(0, -4, 0), third);
    }

    [Fact]
    public void ScalarReferencesAreInitializedInputsAndCopiedOutputs()
    {
        double x = 10, y = 20, z = 30;
        GpPntReferences.Coord(new(1, 2, 3), ref x, ref y, ref z);
        Assert.Equal((1d, 2d, 3d), (x, y, z));
        V.Transform3d transform = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref transform, new V.Vector3d(3, -2, 5));
        GpTrsfReferences.Transforms(transform, ref x, ref y, ref z);
        Assert.Equal((4d, 0d, 8d), (x, y, z));
    }

    [Fact]
    public void InvalidInputPreservesEveryReference()
    {
        double x = 7, y = 11, z = 13;
        Assert.Throws<OcctException>(() => GpPntReferences.Coord(new(double.NaN, 2, 3), ref x, ref y, ref z));
        Assert.Equal((7d, 11d, 13d), (x, y, z));
    }

    [Fact]
    public void DisposedSharedReceiverPreservesEveryReference()
    {
        using GeomCylindricalSurface cylinder = new(CylinderOperations.Create(Axis3Operations.Create(), 4));
        using GeomCurve circle = cylinder.VIso(0)!;
        circle.Dispose();
        Point3d point = new(10, 20, 30);
        V.Vector3d vector = new(7, 11, 13);
        Assert.Throws<ObjectDisposedException>(() => GeomCurveReferences.D1(circle, 0, ref point, ref vector));
        Assert.Equal(new Point3d(10, 20, 30), point);
        Assert.Equal(new V.Vector3d(7, 11, 13), vector);
    }
}
