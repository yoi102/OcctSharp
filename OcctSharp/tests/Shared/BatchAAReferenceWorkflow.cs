using OcctSharp.ReferenceOperations;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchAAReferenceWorkflow
{
    internal static void Run()
    {
        using GeomCylindricalSurface cylinder = new(CylinderOperations.Create(Axis3Operations.Create(), 4));
        using GeomCurve circle = cylinder.VIso(0)!;
        Point3d point = new(10, 20, 30);
        V.Vector3d first = default, second = default, third = default;
        GeomCurveReferences.D3(circle, 0, ref point, ref first, ref second, ref third);
        Equal(new Point3d(4, 0, 0), point);
        Equal(new V.Vector3d(0, 4, 0), first);
        Equal(new V.Vector3d(-4, 0, 0), second);
        Equal(new V.Vector3d(0, -4, 0), third);
        double x = 7, y = 11, z = 13;
        GpPntReferences.Coord(point, ref x, ref y, ref z);
        Equal((4d, 0d, 0d), (x, y, z));
        V.Transform3d transform = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref transform, new V.Vector3d(3, -2, 5));
        GpTrsfReferences.Transforms(transform, ref x, ref y, ref z);
        Equal((7d, -2d, 5d), (x, y, z));
        using MeshVSMesh mesh = new(false);
        using MeshVSDrawer drawer = mesh.GetDrawer()!;
        bool value = true;
        if (MeshVSDrawerReferences.GetBoolean(drawer, int.MaxValue, ref value) || !value)
            throw new InvalidOperationException("Missing drawer key must return false and preserve its initialized value.");
    }

    private static void Equal<T>(T expected, T actual) where T : IEquatable<T>
    {
        if (!expected.Equals(actual)) throw new InvalidOperationException($"Batch AA expected {expected}, got {actual}.");
    }
}
