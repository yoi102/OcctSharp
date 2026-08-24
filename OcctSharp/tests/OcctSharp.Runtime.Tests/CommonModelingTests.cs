using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class CommonModelingTests
{
    [Fact]
    public void ConeAndTorusAreValidOwnedSolidsWithFiniteBounds()
    {
        using Shape cone = ShapeFactory.CreateCone(4, 2, 8);
        using Shape torus = ShapeFactory.CreateTorus(5, 1);

        Assert.Equal(ShapeKind.Solid, cone.Kind);
        Assert.Equal(ShapeKind.Solid, torus.Kind);
        Assert.True(cone.IsValid);
        Assert.True(torus.IsValid);
        Assert.True(cone.CountSubShapes(ShapeKind.Face) >= 3);
        Assert.True(torus.CountSubShapes(ShapeKind.Face) >= 1);

        BoundingBox3d bounds = torus.GetBoundingBox();
        Assert.Equal(-6, bounds.Minimum.X, 5);
        Assert.Equal(6, bounds.Maximum.X, 5);
        Assert.Equal(12, bounds.SizeX, 5);
        Assert.Equal(2, bounds.SizeZ, 5);
    }

    [Fact]
    public void ExtrudeAndRevolveReturnSourceIndependentTopology()
    {
        using Shape wire = ShapeFactory.CreatePolygonWire(
            [new GpPoint(0, 0, 0), new GpPoint(2, 0, 0), new GpPoint(2, 3, 0), new GpPoint(0, 3, 0)],
            close: true);
        using Shape face = ShapeFactory.CreatePlanarFace(wire);
        using GpVec direction = GpVec.Create(0, 0, 5);
        using Shape prism = face.Extrude(direction);

        Assert.Equal(ShapeKind.Solid, prism.Kind);
        Assert.True(prism.IsValid);
        Assert.Equal(6, prism.CountSubShapes(ShapeKind.Face));
        Assert.Equal(5, prism.GetBoundingBox().SizeZ, 5);

        using Shape profile = ShapeFactory.CreateEdge(new GpPoint(2, 0, 0), new GpPoint(2, 0, 3));
        using GpAx1 axis = GpAx1.Create(0, 0, 0, 0, 0, 1);
        using Shape revolved = profile.Revolve(axis, Math.PI);
        Assert.True(revolved.CountSubShapes(ShapeKind.Face) > 0);

        face.Dispose();
        direction.Dispose();
        profile.Dispose();
        axis.Dispose();
        Assert.True(prism.IsValid);
        Assert.True(revolved.IsValid);
    }

    [Fact]
    public void AllAndSingleEdgeFilletAndChamferProduceOwnedResults()
    {
        using Shape box = ShapeFactory.CreateBox(10, 10, 10);
        using Shape allFillets = box.Fillet(1);
        using Shape allChamfers = box.Chamfer(1);
        Shape[] edges = box.GetSubShapes(ShapeKind.Edge);
        try
        {
            using Shape oneFillet = box.Fillet(edges[0], 1);
            using Shape oneChamfer = box.Chamfer(edges[0], 1);
            Assert.True(allFillets.IsValid);
            Assert.True(allChamfers.IsValid);
            Assert.True(oneFillet.IsValid);
            Assert.True(oneChamfer.IsValid);

            box.Dispose();
            Assert.True(allFillets.FaceCount > 0);
            Assert.True(allChamfers.FaceCount > 0);
            Assert.True(oneFillet.FaceCount > 0);
            Assert.True(oneChamfer.FaceCount > 0);
        }
        finally
        {
            foreach (Shape edge in edges) edge.Dispose();
        }
    }

    [Fact]
    public void OffsetSectionBoundsValidityAndCountAreValueOrOwnedResults()
    {
        using Shape box = ShapeFactory.CreateBox(4, 5, 6);
        using Shape offset = box.Offset(0.25);
        using Shape cutter = ShapeFactory.CreateBox(4, 5, 6)
            .Transformed(ShapeTransform.CreateTranslationAndRotationZ(2, 0, 0, 0));
        using Shape section = box.Section(cutter);

        Assert.True(offset.IsValid);
        Assert.True(section.CountSubShapes(ShapeKind.Edge) > 0);
        Shape[] faces = box.GetSubShapes(ShapeKind.Face);
        try
        {
            Assert.Equal(faces.Length, box.CountSubShapes(ShapeKind.Face));
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }

        BoundingBox3d bounds = box.GetBoundingBox();
        box.Dispose();
        cutter.Dispose();
        Assert.True(offset.FaceCount > 0);
        Assert.True(section.CountSubShapes(ShapeKind.Edge) > 0);
        Assert.Equal(4, bounds.SizeX, 5);
        Assert.Equal(5, bounds.SizeY, 5);
        Assert.Equal(6, bounds.SizeZ, 5);
    }

    [Fact]
    public void CommonModelingRejectsInvalidNullDisposedAndWrongEdgeInputs()
    {
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateCone(2, 2, 3));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateTorus(1, 1));

        using Shape box = ShapeFactory.CreateBox(4, 4, 4);
        using Shape nullShape = ShapeFactory.CreateNull();
        using Shape foreignEdge = ShapeFactory.CreateEdge(new GpPoint(0, 0, 0), new GpPoint(1, 0, 0));
        using GpVec zero = GpVec.Create(0, 0, 0);
        using GpAx1 axis = GpAx1.Create(0, 0, 0, 0, 0, 1);
        Assert.Throws<ArgumentException>(() => box.Extrude(zero));
        Assert.Throws<ArgumentException>(() => box.Revolve(axis, double.NaN));
        Assert.Throws<ArgumentException>(() => box.Fillet(0));
        Assert.Throws<ArgumentException>(() => box.Chamfer(-1));
        Assert.Throws<ArgumentException>(() => box.Offset(0));
        Assert.Throws<ArgumentException>(() => box.Offset(1, 0));
        Assert.Throws<InvalidCastException>(() => box.Fillet(box, 1));
        Assert.Throws<InvalidCastException>(() => box.Chamfer(box, 1));
        Assert.Throws<OcctException>(() => box.Fillet(foreignEdge, 1));
        Assert.Throws<OcctException>(() => box.Chamfer(foreignEdge, 1));
        Assert.Throws<ArgumentException>(() => nullShape.GetBoundingBox());
        Assert.Throws<ArgumentException>(() => _ = nullShape.IsValid);

        box.Dispose();
        Assert.Throws<ObjectDisposedException>(() => box.GetBoundingBox());
        Assert.Throws<ObjectDisposedException>(() => box.CountSubShapes(ShapeKind.Edge));
    }

    [Fact]
    public void BoundingBoxAbiLayoutIsStable()
    {
        Assert.Equal(48, Marshal.SizeOf<BoundingBoxRaw>());
        Assert.Equal(0, Marshal.OffsetOf<BoundingBoxRaw>(nameof(BoundingBoxRaw.MinX)).ToInt32());
        Assert.Equal(24, Marshal.OffsetOf<BoundingBoxRaw>(nameof(BoundingBoxRaw.MaxX)).ToInt32());
        Assert.Equal(40, Marshal.OffsetOf<BoundingBoxRaw>(nameof(BoundingBoxRaw.MaxZ)).ToInt32());
    }
}
