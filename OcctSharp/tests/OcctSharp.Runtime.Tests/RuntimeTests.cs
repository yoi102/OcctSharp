using System.Runtime.InteropServices;
using OcctSharp.Generated;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class RuntimeTests
{
    [Fact]
    public void RuntimeInfoReportsExpectedVersions()
    {
        OcctRuntimeInfo info = OcctRuntime.Info;

        Assert.Equal(new Version(1, 45), info.AbiVersion);
        Assert.Equal("0.53.0", info.BridgeVersion);
        Assert.Equal("8.0.1", info.OcctVersion);
    }

    [Fact]
    public void GpPointUsesGeneratedValueCopyAndDistanceSemantics()
    {
        GpPoint origin = GpPoint.Origin;
        GpPoint point = GpPoint.Create(3, 4, 12);
        GpPoint copy = point.Copy();

        Assert.Equal(new GpPoint(0, 0, 0), origin);
        Assert.Equal(point, copy);
        Assert.Equal(13, point.DistanceTo(origin), 12);
        Assert.Throws<ArgumentOutOfRangeException>(() => GpPoint.Create(double.NaN, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GpPoint.Create(0, 0, double.PositiveInfinity));
    }

    [Fact]
    public void GpXyzPreservesOcctVectorAlgebraAndZeroNormalizationFailure()
    {
        GpXyz x = GpXyz.Create(1, 0, 0);
        GpXyz y = GpXyz.Create(0, 1, 0);
        Assert.Equal(new GpXyz(1, 1, 0), x.Added(y));
        Assert.Equal(new GpXyz(0, 0, 1), x.Crossed(y));
        Assert.Equal(0, x.Dot(y), 12);
        Assert.Equal(1, x.Crossed(y).Modulus, 12);
        Assert.Equal(new GpXyz(1, 0, 0), x.Normalized());
        Assert.Throws<OcctException>(() => GpXyz.Origin.Normalized());
        Assert.Throws<ArgumentOutOfRangeException>(() => GpXyz.Create(double.NaN, 0, 0));
    }

    [Fact]
    public void GpLinePreservesDirectionDistanceAngleAndConstructionFailure()
    {
        GpLine line = GpLine.Create(GpXyz.Origin, GpXyz.Create(1, 0, 0));
        Assert.Equal(1, line.DistanceTo(GpXyz.Create(0, 1, 0)), 12);
        Assert.Equal(Math.PI / 2, line.AngleTo(GpLine.Create(GpXyz.Origin, GpXyz.Create(0, 1, 0))), 12);
        Assert.Equal(new GpXyz(-1, 0, 0), line.Reversed().Direction);
        Assert.Equal(new GpXyz(0, 0, 1), GpLine.Default.Direction);
        Assert.Throws<OcctException>(() => GpLine.Create(GpXyz.Origin, GpXyz.Origin));
    }

    [Fact]
    public void GpCirclePreservesRadiusAreaLengthDistanceAndValidation()
    {
        GpCircle circle = GpCircle.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), 2);
        Assert.Equal(2, circle.Radius, 12);
        Assert.Equal(4 * Math.PI, circle.Area, 12);
        Assert.Equal(4 * Math.PI, circle.Length, 12);
        Assert.Equal(0, circle.DistanceTo(GpXyz.Create(2, 0, 0)), 12);
        Assert.Throws<OcctException>(() => GpCircle.Create(GpXyz.Origin, GpXyz.Origin, 2));
        Assert.Throws<OcctException>(() => GpCircle.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), -1));
    }

    [Fact]
    public void GpAx2AndPlanePreserveCoordinateAndDistanceSemantics()
    {
        GpAx2Value axis = GpAx2Value.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), GpXyz.Create(1, 0, 0));
        Assert.Equal(new GpXyz(0, 1, 0), axis.YDirection);
        Assert.Equal(0, axis.AngleTo(GpAx2Value.Default), 12);
        GpPlane plane = GpPlane.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1));
        Assert.Equal(2, plane.DistanceTo(GpXyz.Create(0, 0, 2)), 12);
        Assert.Equal(-2, plane.SignedDistanceTo(GpXyz.Create(0, 0, -2)), 12);
        Assert.Throws<OcctException>(() => GpAx2Value.Create(GpXyz.Origin, GpXyz.Create(0, 0, 1), GpXyz.Create(0, 0, 1)));
        Assert.Throws<OcctException>(() => GpPlane.Create(GpXyz.Origin, GpXyz.Origin));
    }

    [Fact]
    public void GpAx3PreservesRightHandedDirectionsAndConstructionFailure()
    {
        GpAx3Value axis = GpAx3Value.Create(
            GpXyz.Origin,
            GpXyz.Create(0, 0, 1),
            GpXyz.Create(1, 0, 0));

        Assert.Equal(new GpXyz(0, 1, 0), axis.YDirection);
        Assert.True(axis.IsDirect);
        Assert.True(GpAx3Value.Default.IsDirect);
        Assert.Throws<OcctException>(() => GpAx3Value.Create(
            GpXyz.Origin,
            GpXyz.Create(0, 0, 1),
            GpXyz.Create(0, 0, 1)));
    }

    [Fact]
    public void GPropPropertiesComputesVolumeCenterAndInertiaForBox()
    {
        using Shape box = ShapeFactory.CreateBox(10, 20, 30);
        using GPropProperties properties = GPropProperties.FromShape(box);

        Assert.Equal(6000, properties.Mass, 8);
        Assert.Equal(new GpPoint(5, 10, 15), properties.CenterOfMass);
        Assert.True(properties.InertiaValue(1, 1) > 0);
        Assert.Equal(properties.InertiaValue(1, 2), properties.InertiaValue(2, 1), 12);
        (double firstMoment, double secondMoment, double thirdMoment) = properties.PrincipalMoments;
        Assert.True(firstMoment >= 0 && secondMoment >= 0 && thirdMoment >= 0);
        _ = properties.Symmetry;

        using GPropProperties clone = properties.Clone();
        using GPropProperties combined = GPropProperties.Create();
        combined.Add(properties);
        combined.Add(clone, 0.5);
        Assert.Equal(9000, combined.Mass, 8);
        Assert.Throws<ArgumentException>(() => combined.Add(properties, 0));
        Assert.Throws<ArgumentException>(() => properties.InertiaValue(0, 1));
    }

    [Fact]
    public void ShapeFactoryCreatesSphereAndCylinderSolids()
    {
        using Shape sphere = ShapeFactory.CreateSphere(2);
        using Shape cylinder = ShapeFactory.CreateCylinder(2, 5);
        Assert.Equal(1, sphere.FaceCount);
        Assert.Equal(3, cylinder.FaceCount);
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateSphere(0));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateCylinder(2, double.NaN));
    }

    [Fact]
    public void ShapeFactoryCreatesEdgeWireAndPlanarFace()
    {
        using Shape edge = ShapeFactory.CreateEdge(GpPoint.Origin, GpPoint.Create(5, 0, 0));
        Assert.Equal(ShapeKind.Edge, edge.Kind);

        GpPoint[] corners =
        [
            GpPoint.Origin,
            GpPoint.Create(5, 0, 0),
            GpPoint.Create(5, 4, 0),
            GpPoint.Create(0, 4, 0),
        ];
        using Shape wire = ShapeFactory.CreatePolygonWire(corners, close: true);
        using Shape face = ShapeFactory.CreatePlanarFace(wire);
        Assert.Equal(ShapeKind.Wire, wire.Kind);
        Assert.Equal(ShapeKind.Face, face.Kind);
        Assert.Equal(1, face.FaceCount);
        wire.Dispose();
        Assert.Equal(1, face.FaceCount);

        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateEdge(GpPoint.Origin, GpPoint.Origin));
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreatePolygonWire([GpPoint.Origin]));
        Assert.Throws<InvalidCastException>(() => ShapeFactory.CreatePlanarFace(edge));
    }

    [Fact]
    public void BRepAdaptorSnapshotsCopyEdgeCurveAndFaceSurfaceValues()
    {
        using Shape edge = ShapeFactory.CreateEdge(GpPoint.Create(1, 2, 3), GpPoint.Create(5, 2, 3));
        EdgeCurveSnapshot curve = edge.GetEdgeCurveSnapshot();
        Assert.Equal(CurveGeometryType.Line, curve.CurveType);
        Assert.Equal(0, curve.FirstParameter, 12);
        Assert.Equal(4, curve.LastParameter, 12);
        Assert.Equal(new GpPoint(1, 2, 3), curve.StartPoint);
        Assert.Equal(new GpPoint(5, 2, 3), curve.EndPoint);

        using Shape wire = ShapeFactory.CreatePolygonWire(
        [
            GpPoint.Origin,
            GpPoint.Create(5, 0, 0),
            GpPoint.Create(5, 4, 0),
            GpPoint.Create(0, 4, 0),
        ], close: true);
        using Shape face = ShapeFactory.CreatePlanarFace(wire);
        FaceSurfaceSnapshot surface = face.GetFaceSurfaceSnapshot();
        Assert.Equal(SurfaceGeometryType.Plane, surface.SurfaceType);
        Assert.Equal(5, surface.LastUParameter - surface.FirstUParameter, 12);
        Assert.Equal(4, surface.LastVParameter - surface.FirstVParameter, 12);

        edge.Dispose();
        face.Dispose();
        Assert.Equal(new GpPoint(1, 2, 3), curve.StartPoint);
        Assert.Equal(SurfaceGeometryType.Plane, surface.SurfaceType);
        Assert.Throws<ObjectDisposedException>(() => edge.GetEdgeCurveSnapshot());
        Assert.Throws<ObjectDisposedException>(() => face.GetFaceSurfaceSnapshot());
    }

    [Fact]
    public void BRepAdaptorSnapshotsRejectWrongTopologyKinds()
    {
        using Shape box = ShapeFactory.CreateBox(1, 2, 3);
        using Shape edge = ShapeFactory.CreateEdge(GpPoint.Origin, GpPoint.Create(1, 0, 0));
        Assert.Throws<InvalidCastException>(() => box.GetEdgeCurveSnapshot());
        Assert.Throws<InvalidCastException>(() => edge.GetFaceSurfaceSnapshot());
    }

    [Fact]
    public void BRepAdaptorSnapshotAbiLayoutsAreStable()
    {
        Assert.Equal(72, Marshal.SizeOf<EdgeCurveSnapshotRaw>());
        Assert.Equal(0, Marshal.OffsetOf<EdgeCurveSnapshotRaw>(nameof(EdgeCurveSnapshotRaw.CurveType)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<EdgeCurveSnapshotRaw>(nameof(EdgeCurveSnapshotRaw.FirstParameter)).ToInt32());
        Assert.Equal(24, Marshal.OffsetOf<EdgeCurveSnapshotRaw>(nameof(EdgeCurveSnapshotRaw.StartPoint)).ToInt32());
        Assert.Equal(40, Marshal.SizeOf<FaceSurfaceSnapshotRaw>());
        Assert.Equal(0, Marshal.OffsetOf<FaceSurfaceSnapshotRaw>(nameof(FaceSurfaceSnapshotRaw.SurfaceType)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<FaceSurfaceSnapshotRaw>(nameof(FaceSurfaceSnapshotRaw.FirstUParameter)).ToInt32());
    }

    [Fact]
    public void FaceSnapshotCopiesTopologyAndSurvivesParentDisposal()
    {
        Shape[] faces;
        using (Shape box = ShapeFactory.CreateBox(2, 3, 4))
        {
            faces = box.GetFaces();
            Assert.Equal(6, faces.Length);
            Assert.All(faces, face => Assert.Equal(1, face.FaceCount));
        }

        try
        {
            Assert.All(faces, face => Assert.Equal(1, face.FaceCount));
        }
        finally
        {
            foreach (Shape face in faces) face.Dispose();
        }
    }

    [Fact]
    public void SubshapeSnapshotCoversEdgesWiresAndVertices()
    {
        using Shape box = ShapeFactory.CreateBox(2, 3, 4);
        Shape[] edges = box.GetSubShapes(ShapeKind.Edge);
        Shape[] wires = box.GetSubShapes(ShapeKind.Wire);
        Shape[] vertices = box.GetSubShapes(ShapeKind.Vertex);
        try
        {
            Assert.Equal(24, edges.Length);
            Assert.Equal(6, wires.Length);
            Assert.Equal(48, vertices.Length);
            Assert.Throws<ArgumentOutOfRangeException>(() => box.GetSubShapes(ShapeKind.Shape));
        }
        finally
        {
            foreach (Shape child in edges) child.Dispose();
            foreach (Shape child in wires) child.Dispose();
            foreach (Shape child in vertices) child.Dispose();
        }
    }

    [Fact]
    public void BooleanFuseAndCutReturnOwnedTopologyResults()
    {
        using Shape baseBox = ShapeFactory.CreateBox(10, 10, 10);
        using Shape tool = ShapeFactory.CreateBox(4, 4, 4).Transformed(ShapeTransform.CreateTranslationAndRotationZ(3, 3, 3, 0));
        using Shape fused = baseBox.Fuse(tool);
        using Shape cut = baseBox.Cut(tool);

        Assert.True(fused.FaceCount > 0);
        Assert.True(cut.FaceCount > 0);
        baseBox.Dispose();
        tool.Dispose();
        Assert.True(fused.FaceCount > 0);
        Assert.True(cut.FaceCount > 0);
    }

    [Fact]
    public void BooleanCommonAndMinimumDistanceReturnIndependentValues()
    {
        using Shape first = ShapeFactory.CreateBox(1, 1, 1);
        using Shape overlap = ShapeFactory.CreateBox(1, 1, 1)
            .Transformed(ShapeTransform.CreateTranslationAndRotationZ(0.5, 0, 0, 0));
        using Shape common = first.Common(overlap);
        Assert.True(common.FaceCount > 0);

        using Shape separated = ShapeFactory.CreateBox(1, 1, 1)
            .Transformed(ShapeTransform.CreateTranslationAndRotationZ(5, 0, 0, 0));
        ShapeDistanceResult distance = first.DistanceTo(separated);
        Assert.Equal(4, distance.Distance, 10);
        Assert.True(distance.SolutionCount > 0);
        Assert.Equal(1, distance.PointOnFirst.X, 10);
        Assert.Equal(5, distance.PointOnSecond.X, 10);

        first.Dispose();
        overlap.Dispose();
        separated.Dispose();
        Assert.True(common.FaceCount > 0);
        Assert.Equal(4, distance.Distance, 10);
    }

    [Fact]
    public void ModelingAlgorithmsRejectNullAndDisposedInputsAndDistanceLayoutIsStable()
    {
        using Shape valid = ShapeFactory.CreateBox(1, 1, 1);
        using Shape empty = ShapeFactory.CreateNull();
        Assert.Throws<ArgumentException>(() => valid.Common(empty));
        Assert.Throws<ArgumentException>(() => valid.DistanceTo(empty));
        valid.Dispose();
        Assert.Throws<ObjectDisposedException>(() => valid.Common(empty));
        Assert.Throws<ObjectDisposedException>(() => valid.DistanceTo(empty));

        Assert.Equal(64, Marshal.SizeOf<ShapeDistanceResultRaw>());
        Assert.Equal(0, Marshal.OffsetOf<ShapeDistanceResultRaw>(nameof(ShapeDistanceResultRaw.Distance)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<ShapeDistanceResultRaw>(nameof(ShapeDistanceResultRaw.PointOnFirst)).ToInt32());
        Assert.Equal(32, Marshal.OffsetOf<ShapeDistanceResultRaw>(nameof(ShapeDistanceResultRaw.PointOnSecond)).ToInt32());
        Assert.Equal(56, Marshal.OffsetOf<ShapeDistanceResultRaw>(nameof(ShapeDistanceResultRaw.SolutionCount)).ToInt32());
    }

    [Fact]
    public void MeshSnapshotReturnsBulkTrianglesAndNormals()
    {
        using Shape box = ShapeFactory.CreateBox(10, 20, 30);
        MeshSnapshot mesh = box.CreateMesh(0.1, 0.5);

        Assert.True(mesh.Vertices.Count > 0);
        Assert.True(mesh.Indices.Count > 0);
        Assert.Equal(0, mesh.Indices.Count % 3);
        Assert.Equal(mesh.Indices.Count / 3, mesh.TriangleCount);
        Assert.All(mesh.Indices, index => Assert.InRange(index, 0, mesh.Vertices.Count - 1));
        Assert.All(mesh.Vertices, vertex =>
        {
            Assert.True(double.IsFinite(vertex.X));
            Assert.True(double.IsFinite(vertex.NormalX));
            Assert.True(double.IsFinite(vertex.NormalY));
            Assert.True(double.IsFinite(vertex.NormalZ));
        });
    }

    [Fact]
    public void ShapeFixReturnsIndependentOwnedResult()
    {
        using Shape source = ShapeFactory.CreateBox(2, 3, 4);
        using Shape fixedShape = source.Fixed();

        Assert.False(fixedShape.IsNull);
        Assert.Equal(source.FaceCount, fixedShape.FaceCount);
        source.Dispose();
        Assert.Equal(6, fixedShape.FaceCount);
    }

    [Fact]
    public void ShapeUpgradeUnifySameDomainReturnsOwnedResult()
    {
        using Shape source = ShapeFactory.CreateBox(2, 3, 4);
        using Shape unified = source.UnifiedSameDomain();

        Assert.False(unified.IsNull);
        Assert.Equal(source.FaceCount, unified.FaceCount);
        source.Dispose();
        Assert.True(unified.FaceCount > 0);
    }

    [Fact]
    public void NullTopologyIsRejectedByBooleanAndHealingOperations()
    {
        using Shape valid = ShapeFactory.CreateBox(1, 2, 3);
        using Shape empty = ShapeFactory.CreateNull();

        Assert.True(empty.IsNull);
        ArgumentException fuseError = Assert.Throws<ArgumentException>(() => valid.Fuse(empty));
        Assert.Contains("null", fuseError.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<ArgumentException>(() => valid.Cut(empty));
        Assert.Throws<ArgumentException>(() => empty.Fixed());
        Assert.Throws<ArgumentException>(() => empty.UnifiedSameDomain());
    }

    [Fact]
    public void IgesGeometryRoundTripReadsAnOwnedShape()
    {
        string path = Path.Combine(Path.GetTempPath(), $"occtsharp-{Guid.NewGuid():N}.igs");
        try
        {
            using Shape source = ShapeFactory.CreateBox(2, 3, 4);
            ShapeExchange.WriteIges(source, path);
            using Shape restored = ShapeExchange.ReadIges(path);
            Assert.True(restored.FaceCount > 0);
            source.Dispose();
            Assert.True(restored.FaceCount > 0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void StlGeometryRoundTripReadsAnOwnedFacetedShape()
    {
        string path = Path.Combine(Path.GetTempPath(), $"occtsharp-{Guid.NewGuid():N}.stl");
        try
        {
            using Shape source = ShapeFactory.CreateBox(2, 3, 4);
            ShapeExchange.WriteStl(source, path);
            using Shape restored = ShapeExchange.ReadStl(path);
            Assert.True(restored.FaceCount > 0);
            source.Dispose();
            Assert.True(restored.FaceCount > 0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void MeshExchangeFormatsRoundTripSupportedProviders()
    {
        WithTemporaryDirectory(directory =>
        {
            using Shape source = ShapeFactory.CreateBox(2, 3, 4);
            string obj = ShapeExchange.WriteObj(source, Path.Combine(directory, "box.obj"));
            string ply = ShapeExchange.WritePly(source, Path.Combine(directory, "box.ply"));
            string glb = ShapeExchange.WriteGltf(source, Path.Combine(directory, "box.glb"));
            string vrml = ShapeExchange.WriteVrml(source, Path.Combine(directory, "box.wrl"));

            Assert.True(new FileInfo(obj).Length > 0);
            Assert.True(new FileInfo(ply).Length > 0);
            Assert.True(new FileInfo(glb).Length > 0);
            Assert.True(new FileInfo(vrml).Length > 0);

            using Shape objShape = ShapeExchange.ReadObj(obj);
            using Shape gltfShape = ShapeExchange.ReadGltf(glb);
            using Shape vrmlShape = ShapeExchange.ReadVrml(vrml);
            Assert.True(objShape.FaceCount > 0);
            Assert.True(gltfShape.FaceCount > 0);
            Assert.True(vrmlShape.FaceCount > 0);
            source.Dispose();
            Assert.True(objShape.FaceCount > 0);
        });
    }

    [Fact]
    public void OcafDocumentTransactionsPersistenceAndParentLifetimeAreDeterministic()
    {
        WithTemporaryDirectory(directory =>
        {
            string path = Path.Combine(directory, "document.cbf");
            string childEntry;
            OcafLabel disposedLabel;
            using (OcafDocument document = OcafDocument.Create())
            {
                Assert.False(document.HasOpenTransaction);
                Assert.Null(document.RootLabel.Name);
                Assert.Throws<ArgumentException>(() => document.RootLabel.AddChild());

                using (OcafTransaction transaction = document.BeginTransaction())
                {
                    document.RootLabel.Name = "根";
                    OcafLabel child = document.RootLabel.AddChild();
                    child.Name = "零件 A";
                    childEntry = child.Entry;
                    Assert.True(transaction.Commit());
                }

                Assert.False(document.HasOpenTransaction);
                Assert.Equal("根", document.RootLabel.Name);
                Assert.Equal(1, document.RootLabel.ChildCount);
                Assert.Equal("零件 A", document.GetLabel(childEntry).Name);

                using (document.BeginTransaction())
                {
                    document.RootLabel.Name = "aborted";
                    _ = document.RootLabel.AddChild();
                }

                Assert.Equal("根", document.RootLabel.Name);
                // OCAF rolls attributes back but retains the allocated empty label node.
                Assert.Equal(2, document.RootLabel.ChildCount);

                using (OcafTransaction open = document.BeginTransaction())
                {
                    Assert.Throws<ArgumentException>(() => document.Save(path));
                    open.Abort();
                }

                Assert.Equal(path, document.Save(path));
                Assert.True(new FileInfo(path).Length > 0);
                disposedLabel = document.GetLabel(childEntry);
            }

            Assert.Throws<ObjectDisposedException>(() => _ = disposedLabel.Name);

            using OcafDocument restored = OcafDocument.Open(path);
            Assert.Equal("根", restored.RootLabel.Name);
            Assert.Equal(1, restored.RootLabel.ChildCount);
            Assert.Equal("零件 A", restored.GetLabel(childEntry).Name);
        });
    }

    [Fact]
    public void XdeMetadataAssemblyBinXcafAndStepcafRoundTrip()
    {
        WithTemporaryDirectory(directory =>
        {
            string binaryPath = Path.Combine(directory, "assembly.xbf");
            string stepPath = Path.Combine(directory, "assembly.step");
            string partEntry;
            string assemblyEntry;
            string occurrenceEntry;
            XdeLabel disposedPart;

            using (Shape box = ShapeFactory.CreateBox(2, 3, 4))
            using (GpTrsf translation = GpTrsf.Create(10, 20, 30))
            using (TopLocLocation location = TopLocLocation.FromTransform(translation))
            using (XdeDocument document = XdeDocument.Create())
            {
                using (XdeTransaction transaction = document.BeginTransaction())
                {
                    XdeLabel part = document.AddShape(box, "Part A");
                    part.Color = new XdeColor(0.25, 0.5, 0.75, 1.0);
                    part.SetLayer("Mechanical");
                    part.AddLayer("Purchased");
                    part.Material = new XdeMaterial("Steel", "Structural steel", 7.85, "Density", "g/cm3");

                    XdeLabel assembly = document.AddAssembly("Assembly A");
                    XdeLabel occurrence = document.AddComponent(assembly, part, location);
                    partEntry = part.Entry;
                    assemblyEntry = assembly.Entry;
                    occurrenceEntry = occurrence.Entry;
                    Assert.True(transaction.Commit());
                }

                XdeLabel partLabel = document.GetLabel(partEntry);
                Assert.Equal("Part A", partLabel.Name);
                Assert.Equal(2, partLabel.Layers.Count);
                Assert.Equal("Mechanical", partLabel.Layers[0]);
                Assert.Equal("Purchased", partLabel.Layers[1]);
                Assert.Equal(new XdeMaterial("Steel", "Structural steel", 7.85, "Density", "g/cm3"), partLabel.Material);
                XdeColor color = Assert.IsType<XdeColor>(partLabel.Color);
                Assert.Equal(0.25, color.Red, 6);
                Assert.Equal(0.5, color.Green, 6);
                Assert.Equal(0.75, color.Blue, 6);

                XdeLabel assemblyLabel = document.GetLabel(assemblyEntry);
                Assert.True(assemblyLabel.IsAssembly);
                Assert.Equal(1, assemblyLabel.ComponentCount);
                Assert.Equal(occurrenceEntry, Assert.Single(assemblyLabel.GetComponents()).Entry);
                XdeLabel occurrenceLabel = document.GetLabel(occurrenceEntry);
                Assert.Equal(partEntry, occurrenceLabel.ReferredShape.Entry);
                using TopLocLocation copiedLocation = occurrenceLabel.Location;
                using GpTrsf copiedTransform = copiedLocation.ToTransform();
                Assert.Equal(10, copiedTransform.Value(1, 4), 10);
                Assert.Equal(20, copiedTransform.Value(2, 4), 10);
                Assert.Equal(30, copiedTransform.Value(3, 4), 10);
                using Shape copiedShape = partLabel.Shape;
                Assert.Equal(6, copiedShape.FaceCount);

                Assert.Single(document.GetFreeShapes());
                document.Save(binaryPath);
                document.WriteStep(stepPath);
                Assert.True(new FileInfo(binaryPath).Length > 0);
                Assert.True(new FileInfo(stepPath).Length > 0);
                disposedPart = partLabel;
            }

            Assert.Throws<ObjectDisposedException>(() => _ = disposedPart.Name);

            using (XdeDocument binary = XdeDocument.Open(binaryPath))
            {
                XdeLabel part = binary.GetLabel(partEntry);
                Assert.Equal("Part A", part.Name);
                Assert.Equal(2, part.Layers.Count);
                Assert.Equal("Mechanical", part.Layers[0]);
                Assert.Equal("Purchased", part.Layers[1]);
                Assert.Equal("Steel", part.Material?.Name);
                Assert.True(binary.GetLabel(assemblyEntry).IsAssembly);
                Assert.Equal(partEntry, binary.GetLabel(occurrenceEntry).ReferredShape.Entry);
            }

            using XdeDocument step = XdeDocument.ReadStep(stepPath);
            XdeLabel stepAssembly = Assert.Single(step.GetFreeShapes());
            Assert.True(stepAssembly.IsAssembly);
            XdeLabel stepOccurrence = Assert.Single(stepAssembly.GetComponents());
            XdeLabel stepPart = stepOccurrence.ReferredShape;
            Assert.Equal("Part A", stepPart.Name);
            Assert.Contains("Mechanical", stepPart.Layers);
            Assert.Contains("Purchased", stepPart.Layers);
            Assert.Equal("Steel", stepPart.Material?.Name);
            XdeColor stepColor = Assert.IsType<XdeColor>(stepPart.Color ?? stepOccurrence.Color);
            Assert.Equal(0.25, stepColor.Red, 6);
            Assert.Equal(0.5, stepColor.Green, 6);
            Assert.Equal(0.75, stepColor.Blue, 6);
            using Shape stepShape = stepPart.Shape;
            Assert.Equal(6, stepShape.FaceCount);
        });
    }

    [Fact]
    public void XdeValidationPropertiesOccurrencesAndStepOptionsRoundTrip()
    {
        WithTemporaryDirectory(directory =>
        {
            string allMetadataPath = Path.Combine(directory, "validation-all.step");
            string noPropertiesPath = Path.Combine(directory, "validation-no-properties.step");
            using Shape box = ShapeFactory.CreateBox(2, 3, 4);
            using XdeDocument document = XdeDocument.Create();
            XdeLabel rootAssembly;
            string partEntry;

            using (GpTrsf partTranslation = GpTrsf.Create(1, 2, 3))
            using (TopLocLocation partLocation = TopLocLocation.FromTransform(partTranslation))
            using (GpTrsf assemblyTranslation = GpTrsf.Create(10, 20, 30))
            using (TopLocLocation assemblyLocation = TopLocLocation.FromTransform(assemblyTranslation))
            using (XdeTransaction transaction = document.BeginTransaction())
            {
                XdeLabel part = document.AddPart(box, new XdePartMetadata(
                    "Property Part",
                    new XdeColor(0.2, 0.4, 0.6),
                    ["Validation"],
                    new XdeMaterial("Steel", "Validation material", 7.85, "Density", "g/cm3")));
                XdeValidationProperties computed = part.UpdateValidationPropertiesFromShape();
                partEntry = part.Entry;
                Assert.Equal(52, computed.Area!.Value, 10);
                Assert.Equal(24, computed.Volume!.Value, 10);
                Assert.Equal(new GpPoint(1, 1.5, 2), computed.Centroid!.Value);

                XdeLabel subassembly = document.AddAssembly("Property Subassembly");
                _ = document.AddComponent(subassembly, part, partLocation);
                rootAssembly = document.AddAssembly("Property Root");
                _ = document.AddComponent(rootAssembly, subassembly, assemblyLocation);
                Assert.True(transaction.Commit());
            }

            IReadOnlyList<XdeOccurrence> direct = rootAssembly.GetOccurrences(recursive: false);
            try
            {
                XdeOccurrence subassemblyOccurrence = Assert.Single(direct);
                Assert.True(subassemblyOccurrence.IsAssembly);
                Assert.Equal(1, subassemblyOccurrence.Depth);
            }
            finally { foreach (XdeOccurrence occurrence in direct) occurrence.Dispose(); }

            IReadOnlyList<XdeOccurrence> flattened = rootAssembly.GetOccurrences();
            try
            {
                Assert.Equal(2, flattened.Count);
                XdeOccurrence partOccurrence = flattened.Single(occurrence => !occurrence.IsAssembly);
                Assert.Equal(2, partOccurrence.Depth);
                using TopLocLocation worldLocation = partOccurrence.GetWorldLocation();
                using GpTrsf worldTransform = worldLocation.ToTransform();
                Assert.Equal(11, worldTransform.Value(1, 4), 10);
                Assert.Equal(22, worldTransform.Value(2, 4), 10);
                Assert.Equal(33, worldTransform.Value(3, 4), 10);
                using Shape located = partOccurrence.GetLocatedShape();
                BoundingBox3d bounds = located.GetBoundingBox();
                Assert.Equal(11, bounds.Minimum.X, 6);
                Assert.Equal(22, bounds.Minimum.Y, 6);
                Assert.Equal(33, bounds.Minimum.Z, 6);
            }
            finally { foreach (XdeOccurrence occurrence in flattened) occurrence.Dispose(); }

            Assert.Equal(
                allMetadataPath,
                document.WriteStep(allMetadataPath, new XdeStepWriteOptions(
                    ModelType: XdeStepModelType.AsIs,
                    WriteValidationProperties: true)));
            Assert.Equal(
                noPropertiesPath,
                document.WriteStep(noPropertiesPath, new XdeStepWriteOptions(
                    WriteNames: false,
                    WriteColors: false,
                    WriteLayers: false,
                    WriteValidationProperties: false,
                    WriteMaterials: false)));
            Assert.Throws<ArgumentOutOfRangeException>(() => document.WriteStep(
                Path.Combine(directory, "invalid.step"),
                new XdeStepWriteOptions((XdeStepModelType)999)));

            using (XdeDocument restored = XdeDocument.ReadStep(allMetadataPath))
            {
                XdeLabel restoredRoot = Assert.Single(restored.GetFreeShapes());
                IReadOnlyList<XdeOccurrence> occurrences = restoredRoot.GetOccurrences();
                try
                {
                    XdeLabel restoredPart = occurrences.Single(item => !item.IsAssembly).ReferredLabel;
                    XdeValidationProperties properties = restoredPart.ValidationProperties;
                    Assert.True(properties.IsComplete);
                    Assert.Equal(52, properties.Area!.Value, 8);
                    Assert.Equal(24, properties.Volume!.Value, 8);
                    Assert.Equal(new GpPoint(1, 1.5, 2), properties.Centroid!.Value);
                }
                finally { foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose(); }
            }

            using (XdeDocument filteredByWriter = XdeDocument.ReadStep(noPropertiesPath))
            {
                XdeLabel filteredRoot = Assert.Single(filteredByWriter.GetFreeShapes());
                IReadOnlyList<XdeOccurrence> occurrences = filteredRoot.GetOccurrences();
                try
                {
                    XdeLabel filteredPart = occurrences.Single(item => !item.IsAssembly).ReferredLabel;
                    Assert.False(filteredPart.ValidationProperties.HasAny);
                    Assert.Null(filteredPart.Color);
                    Assert.Empty(filteredPart.Layers);
                    Assert.Null(filteredPart.Material);
                    Assert.NotEqual("Property Part", filteredPart.Name);
                }
                finally { foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose(); }
            }

            using (XdeDocument filteredByReader = XdeDocument.ReadStep(
                allMetadataPath,
                new XdeStepReadOptions(
                    ReadNames: false,
                    ReadColors: false,
                    ReadLayers: false,
                    ReadValidationProperties: false,
                    ReadMaterials: false)))
            {
                XdeLabel filteredRoot = Assert.Single(filteredByReader.GetFreeShapes());
                IReadOnlyList<XdeOccurrence> occurrences = filteredRoot.GetOccurrences();
                try
                {
                    XdeLabel filteredPart = occurrences.Single(item => !item.IsAssembly).ReferredLabel;
                    Assert.False(filteredPart.ValidationProperties.HasAny);
                    Assert.Null(filteredPart.Color);
                    Assert.Empty(filteredPart.Layers);
                    Assert.Null(filteredPart.Material);
                    Assert.NotEqual("Property Part", filteredPart.Name);
                }
                finally { foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose(); }
            }

            XdeLabel originalPart = document.GetLabel(partEntry);
            Assert.Throws<ArgumentException>(() => originalPart.ValidationProperties = new(null, null, null));
            using (XdeTransaction transaction = document.BeginTransaction())
            {
                originalPart.ValidationProperties = new(null, null, null);
                Assert.True(transaction.Commit());
            }
            Assert.False(originalPart.ValidationProperties.HasAny);
        });
    }

    [Fact]
    public void CommonCadWaveRunsBrepInspectModifyMeshXdeAndStepEndToEnd()
    {
        WithTemporaryDirectory(directory =>
        {
            string brepPath = Path.Combine(directory, "common-wave.brep");
            string stepPath = Path.Combine(directory, "common-wave.step");
            using Shape source = ShapeFactory.CreateBox(20, 30, 40);
            ShapeTopologySummary sourceSummary = source.GetTopologySummary();
            Assert.True(sourceSummary.IsClosed);
            Assert.True(sourceSummary.IsValid);
            Assert.Equal(8, sourceSummary.UniqueCounts.VertexCount);
            Assert.Equal(12, sourceSummary.UniqueCounts.EdgeCount);
            Assert.Equal(6, sourceSummary.UniqueCounts.FaceCount);
            Assert.Equal(1, sourceSummary.UniqueCounts.SolidCount);
            Assert.True(sourceSummary.VertexTolerance.Maximum >= sourceSummary.VertexTolerance.Minimum);
            Assert.True(sourceSummary.EdgeTolerance.Maximum >= sourceSummary.EdgeTolerance.Minimum);
            Assert.True(sourceSummary.FaceTolerance.Maximum >= sourceSummary.FaceTolerance.Minimum);

            using Shape chamfered = source.Chamfer(1.5);
            using Shape moved = chamfered.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(5, 7, 11, 15));
            Assert.Equal(brepPath, ShapeExchange.WriteBrep(moved, brepPath));
            using Shape restored = ShapeExchange.ReadBrep(brepPath);
            ShapeTopologySummary restoredSummary = restored.GetTopologySummary();
            Assert.True(restoredSummary.IsClosed);
            Assert.True(restoredSummary.IsValid);
            Assert.Equal(restoredSummary.UniqueCounts.FaceCount, restored.FaceCount);
            Assert.True(restoredSummary.UniqueCounts.FaceCount > sourceSummary.UniqueCounts.FaceCount);

            DetailedMeshSnapshot mesh = restored.CreateDetailedMesh(0.25, 0.35);
            Assert.Equal(restored.FaceCount, mesh.FaceCount);
            Assert.NotEmpty(mesh.Vertices);
            Assert.NotEmpty(mesh.Triangles);
            Assert.True(mesh.HasUv);
            Assert.All(mesh.Vertices, vertex =>
            {
                double magnitude = Math.Sqrt(
                    vertex.NormalX * vertex.NormalX
                    + vertex.NormalY * vertex.NormalY
                    + vertex.NormalZ * vertex.NormalZ);
                Assert.Equal(1.0, magnitude, 8);
            });
            Assert.All(mesh.Triangles, triangle =>
            {
                Assert.InRange(triangle.VertexA, 0, mesh.Vertices.Count - 1);
                Assert.InRange(triangle.VertexB, 0, mesh.Vertices.Count - 1);
                Assert.InRange(triangle.VertexC, 0, mesh.Vertices.Count - 1);
                Assert.InRange(triangle.FaceIndex, 0, mesh.FaceCount - 1);
            });

            using (XdeDocument document = XdeDocument.Create())
            {
                using XdeTransaction transaction = document.BeginTransaction();
                XdeLabel partLabel = document.AddPart(chamfered, new XdePartMetadata(
                    "Common Wave Part",
                    new XdeColor(0.15, 0.35, 0.75, 1),
                    ["Mechanical", "Purchased"],
                    new XdeMaterial("Steel", "Structural steel", 7.85, "Density", "g/cm3")));
                Assert.Equal(["Mechanical", "Purchased"], partLabel.Layers);
                Assert.Equal("Steel", partLabel.Material?.Name);
                Assert.Equal(0.15, Assert.IsType<XdeColor>(partLabel.Color).Red, 6);
                XdeLabel assembly = document.AddAssembly("Common Wave Assembly");
                using TopLocLocation identity = TopLocLocation.Identity;
                _ = document.AddComponent(assembly, partLabel, identity);
                Assert.True(transaction.Commit());
                Assert.Equal(stepPath, document.WriteStep(stepPath));
            }

            using StepReadResult geometryRead = ShapeExchange.ReadStepWithReport(stepPath);
            Assert.True(geometryRead.Report.CandidateRootCount > 0);
            Assert.True(geometryRead.Report.TransferredRootCount > 0);
            Assert.True(geometryRead.Report.ShapeCount > 0);
            Assert.Equal(StepReadStatus.Done, geometryRead.Report.ReadStatus);
            Assert.True(double.IsFinite(geometryRead.Report.SystemLengthUnit));
            ShapeValidationReport validation = geometryRead.Shape.GetValidationReport();
            Assert.True(validation.IsValid);
            Assert.Empty(validation.Issues);
            using ShapeRepairResult repair = geometryRead.Shape.RepairWithReport();
            Assert.True(repair.Before.IsValid);
            Assert.True(repair.After.IsValid);

            using XdeDocument imported = XdeDocument.ReadStep(stepPath);
            XdeLabel importedAssembly = Assert.Single(imported.GetFreeShapes());
            XdeLabel importedOccurrence = Assert.Single(importedAssembly.GetComponents());
            XdeLabel part = importedOccurrence.ReferredShape;
            Assert.Equal("Common Wave Part", part.Name);
            Assert.Contains("Mechanical", part.Layers.Concat(importedOccurrence.Layers));
            Assert.Contains("Purchased", part.Layers.Concat(importedOccurrence.Layers));
            Assert.Equal("Steel", (part.Material ?? importedOccurrence.Material)?.Name);
            XdeColor color = Assert.IsType<XdeColor>(part.Color ?? importedOccurrence.Color);
            Assert.Equal(0.15, color.Red, 6);
            using Shape importedShape = part.Shape;
            Assert.True(importedShape.GetTopologySummary().IsValid);
            Assert.NotEmpty(importedShape.CreateDetailedMesh().Triangles);
        });
    }

    [Fact]
    public void CommonCadWaveAbiLayoutsAreStable()
    {
        Assert.Equal(32, Marshal.SizeOf<TopologyCountsRaw>());
        Assert.Equal(120, Marshal.SizeOf<ShapeTopologySummaryRaw>());
        Assert.Equal(72, Marshal.SizeOf<DetailedMeshVertexRaw>());
        Assert.Equal(20, Marshal.SizeOf<DetailedMeshTriangleRaw>());
        Assert.Equal(8, Marshal.SizeOf<ValidationIssueRaw>());
        Assert.Equal(24, Marshal.SizeOf<StepReadReportRaw>());
        Assert.Equal(56, Marshal.SizeOf<XdeValidationPropertiesRaw>());
    }

    [Fact]
    public void ViewerOwnsPresentationsEnforcesThreadAndProducesSelectionSnapshots()
    {
        nint window = NativeWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp viewer test", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        try
        {
            _ = NativeWindowMethods.ShowWindow(window, 4);
            _ = NativeWindowMethods.UpdateWindow(window);
            using OcctViewer viewer = OcctViewer.Create(window);
            Shape box = ShapeFactory.CreateBox(10, 20, 30);
            ViewerPresentation presentation = viewer.Display(box);
            box.Dispose();

            presentation.SetColor(new ViewerColor(0.2, 0.4, 0.8));
            presentation.SetTransparency(0.25);
            presentation.SetDisplayMode(ViewerDisplayMode.Wireframe);
            presentation.SetDisplayMode(ViewerDisplayMode.Shaded);
            Assert.Throws<ArgumentOutOfRangeException>(() => presentation.SetColor(new ViewerColor(-1, 0, 0)));
            presentation.Hide();
            presentation.Show();
            viewer.Resize();
            viewer.SetProjection(ViewerProjection.Front);
            viewer.SetProjection(ViewerProjection.Top);
            viewer.SetProjection(ViewerProjection.Axonometric);
            viewer.Zoom(1.1);
            viewer.Pan(8, -4);
            viewer.StartRotation(128, 128);
            viewer.Rotate(136, 132);
            Assert.Throws<ArgumentOutOfRangeException>(() => viewer.StartRotation(0, 0, -0.1));
            viewer.FitAll();
            viewer.Redraw();
            Exception? threadError = null;
            Thread worker = new(() =>
            {
                try { viewer.Redraw(); }
                catch (Exception error) { threadError = error; }
            });
            worker.Start();
            worker.Join();
            Assert.IsType<InvalidOperationException>(threadError);

            Assert.True(viewer.MoveTo(128, 128));
            Assert.Contains(presentation, viewer.SelectAt(128, 128));
            Assert.Contains(presentation, viewer.GetSelection());
            viewer.ClearSelection();
            Assert.Empty(viewer.GetSelection());
            Assert.Contains(presentation, viewer.SelectAt(128, 128, ViewerSelectionMode.Add));
            Assert.Empty(viewer.SelectAt(128, 128, ViewerSelectionMode.Remove));
            Assert.Contains(presentation, viewer.SelectAt(128, 128, ViewerSelectionMode.Toggle));
            Assert.Empty(viewer.SelectAt(128, 128, ViewerSelectionMode.Toggle));

            presentation.Dispose();
            Assert.Throws<ObjectDisposedException>(presentation.Hide);
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
        }
    }

    [Fact]
    public void MeshSnapshotRejectsInvalidDeflectionsAndDisposedShapes()
    {
        using Shape box = ShapeFactory.CreateBox(1, 2, 3);
        Assert.Throws<ArgumentException>(() => box.CreateMesh(0, 0.5));
        Assert.Throws<ArgumentException>(() => box.CreateMesh(0.1, double.NaN));
        box.Dispose();
        Assert.Throws<ObjectDisposedException>(() => box.CreateMesh());
    }

    [Fact]
    public void CreateBoxReturnsSixFaces()
    {
        using Shape shape = ShapeFactory.CreateBox(10, 20, 30);

        Assert.Equal(6, shape.FaceCount);
    }

    [Fact]
    public void InvalidBoxDimensionsAreRejected()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => ShapeFactory.CreateBox(0, 20, 30));

        Assert.Contains("greater than zero", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DisposeIsIdempotentAndAccessAfterDisposeFails()
    {
        Shape shape = ShapeFactory.CreateBox(1, 2, 3);

        shape.Dispose();
        shape.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = shape.FaceCount);
    }

    [Fact]
    public void NativeReleaseIsIdempotentAndStaleHandlesAreRejected()
    {
        NativeStatus createStatus = NativeMethods.CreateBox(1, 2, 3, out nint nativeShape);
        Assert.Equal(NativeStatus.Success, createStatus);
        Assert.NotEqual(nint.Zero, nativeShape);

        using ShapeHandle accessHandle = new(nativeShape);
        NativeStatus firstAccess = NativeMethods.GetFaceCount(accessHandle, out int faceCount);
        Assert.Equal(NativeStatus.Success, firstAccess);
        Assert.Equal(6, faceCount);
        accessHandle.SetHandleAsInvalid();

        NativeMethods.ReleaseShape(nativeShape);
        NativeMethods.ReleaseShape(nativeShape);

        using ShapeHandle staleHandle = new(nativeShape);
        NativeStatus staleAccess = NativeMethods.GetFaceCount(staleHandle, out _);
        staleHandle.SetHandleAsInvalid();
        Assert.Equal(NativeStatus.InvalidHandle, staleAccess);
        Assert.Contains("already released", Marshal.PtrToStringUTF8(NativeMethods.GetLastError()), StringComparison.OrdinalIgnoreCase);

        using ShapeHandle arbitraryHandle = new((nint)0x1234);
        NativeStatus arbitraryAccess = NativeMethods.GetFaceCount(arbitraryHandle, out _);
        arbitraryHandle.SetHandleAsInvalid();
        Assert.Equal(NativeStatus.InvalidHandle, arbitraryAccess);
    }

    [Fact]
    public void SharedTransientCloneRetainsReferenceUntilLastWrapper()
    {
        using SharedTransient first = SharedTransient.Create();
        Assert.False(first.IsNull);
        Assert.Equal(1, first.ReferenceCount);

        using SharedTransient second = first.Clone();
        Assert.Equal(2, first.ReferenceCount);
        Assert.Equal(2, second.ReferenceCount);

        first.Dispose();
        Assert.Equal(1, second.ReferenceCount);
        Assert.Throws<ObjectDisposedException>(() => _ = first.ReferenceCount);
    }

    [Fact]
    public void SharedTransientNullHandleCopiesAsNull()
    {
        using SharedTransient first = SharedTransient.CreateNull();
        using SharedTransient second = first.Clone();

        Assert.True(first.IsNull);
        Assert.True(second.IsNull);
        Assert.Equal(0, first.ReferenceCount);
        Assert.Equal(0, second.ReferenceCount);
    }

    [Fact]
    public void SharedTransientReportsRuntimeTypeAndBaseKind()
    {
        using SharedTransient derived = SharedTransient.CreateDerived();

        Assert.Equal("OcctSharp_TransientDerived", derived.TypeName);
        Assert.True(derived.IsKind("OcctSharp_TransientDerived"));
        Assert.True(derived.IsKind("Standard_Transient"));
        Assert.False(derived.IsKind("OcctSharp_UnknownTransient"));
    }

    [Fact]
    public void SharedTransientCheckedCastRetainsDerivedObject()
    {
        using SharedTransient source = SharedTransient.CreateDerived();
        Assert.True(source.TryCastDerived(out SharedTransientDerived? typed));
        Assert.NotNull(typed);
        using (typed)
        {
            Assert.Equal("OcctSharp_TransientDerived", typed.TypeName);
            Assert.True(typed.IsKind("Standard_Transient"));
            Assert.Equal(2, source.ReferenceCount);
        }

        Assert.Equal(1, source.ReferenceCount);
    }

    [Fact]
    public void SharedTransientCheckedCastRejectsWrongAndNullKinds()
    {
        using SharedTransient baseOnly = SharedTransient.Create();
        Assert.False(baseOnly.TryCastDerived(out SharedTransientDerived? wrong));
        Assert.Null(wrong);
        Assert.Throws<InvalidCastException>(() => baseOnly.CastDerived());

        using SharedTransient nullHandle = SharedTransient.CreateNull();
        Assert.False(nullHandle.TryCastDerived(out SharedTransientDerived? nullResult));
        Assert.Null(nullResult);
    }

    [Fact]
    public void GeneratedGeomCartesianPointPreservesSharedLifetimeAndValues()
    {
        using GeomCartesianPoint source = new(1, 2, 3);

        Assert.Equal(1, source.X());
        Assert.Equal(2, source.Y());
        Assert.Equal(3, source.Z());
        Assert.Equal(new Point3d(1, 2, 3), source.Pnt());
        Assert.Equal("Geom_CartesianPoint", source.TypeName);
        Assert.True(source.IsKind("Geom_Point"));
        Assert.True(source.IsKind("Standard_Transient"));
        Assert.Equal(1, source.ReferenceCount);

        using GeomCartesianPoint clone = source.Clone();
        Assert.Equal(2, source.ReferenceCount);
        clone.SetCoord(4, 5, 6);
        Assert.Equal(new Point3d(4, 5, 6), source.Pnt());

        source.Dispose();
        Assert.Equal(1, clone.ReferenceCount);
        clone.SetPnt(new Point3d(7, 8, 9));
        Assert.Equal(new Point3d(7, 8, 9), clone.Pnt());
        Assert.Throws<ObjectDisposedException>(() => source.X());
    }

    [Fact]
    public void GeneratedGeomCartesianPointSupportsPointValueConstructor()
    {
        using GeomCartesianPoint point = new(new Point3d(10, 20, 30));

        point.SetX(11);
        point.SetY(21);
        point.SetZ(31);

        Assert.Equal(new Point3d(11, 21, 31), point.Pnt());
    }

    [Fact]
    public void GeneratedTopologyPreservesTopoDsCopyAndOrientationSemantics()
    {
        using Shape source = ShapeFactory.CreateBox(10, 20, 30);

        Assert.False(source.IsNull);
        Assert.Equal(ShapeKind.Solid, source.Kind);
        Assert.Equal(ShapeOrientation.Forward, source.Orientation);

        using Shape clone = source.Clone();
        Assert.True(source.IsPartner(clone));
        Assert.True(source.IsSame(clone));
        Assert.True(source.IsEqual(clone));

        using Shape reversed = source.Reversed();
        Assert.True(source.IsPartner(reversed));
        Assert.True(source.IsSame(reversed));
        Assert.False(source.IsEqual(reversed));
        Assert.Equal(ShapeOrientation.Reversed, reversed.Orientation);
        Assert.Equal(6, reversed.FaceCount);

        source.Dispose();
        Assert.Equal(ShapeKind.Solid, clone.Kind);
        Assert.Equal(6, clone.FaceCount);
        Assert.Throws<ObjectDisposedException>(() => _ = source.Kind);
    }

    [Fact]
    public void GeneratedTopologyCheckedCastsPreserveTypedKindsAndLifetime()
    {
        using Shape source = ShapeFactory.CreateBox(10, 20, 30);
        using Solid solid = source.CastSolid();

        Assert.Equal(ShapeKind.Solid, solid.Kind);
        Assert.Equal(6, solid.FaceCount);
        Assert.True(source.IsPartner(solid));
        Assert.True(source.IsSame(solid));
        Assert.True(source.IsEqual(solid));

        Assert.True(source.TryCastSolid(out Solid? secondSolid));
        Assert.NotNull(secondSolid);
        using (secondSolid)
        {
            Assert.Equal(ShapeKind.Solid, secondSolid.Kind);
        }

        Assert.False(source.TryCastFace(out Face? wrongFace));
        Assert.Null(wrongFace);
        Assert.Throws<InvalidCastException>(() => source.CastFace());

        source.Dispose();
        Assert.Equal(6, solid.FaceCount);
        Assert.Equal(ShapeKind.Solid, solid.Kind);
    }

    [Fact]
    public void GeneratedTopologyCompoundCastIsChecked()
    {
        using Shape first = ShapeFactory.CreateBox(1, 2, 3);
        using Shape second = ShapeFactory.CreateBox(4, 5, 6);
        using Shape compoundSource = ShapeAssembly.Create(
        [
            new ShapePlacement(first, ShapeTransform.Identity),
            new ShapePlacement(second, ShapeTransform.CreateTranslationAndRotationZ(10, 0, 0, 15)),
        ]);

        using Compound compound = compoundSource.CastCompound();
        Assert.Equal(ShapeKind.Compound, compound.Kind);
        Assert.Equal(12, compound.FaceCount);
        Assert.False(compoundSource.TryCastSolid(out Solid? wrongSolid));
        Assert.Null(wrongSolid);
    }

    [Fact]
    public void BoxRoundTripsThroughStep()
    {
        WithTemporaryDirectory(directory =>
        {
            string path = Path.Combine(directory, "box.step");
            using Shape source = ShapeFactory.CreateBox(10, 20, 30);
            ShapeExchange.WriteStep(source, path);

            using Shape roundTripped = ShapeExchange.ReadStep(path);
            Assert.Equal(6, roundTripped.FaceCount);
            Assert.True(new FileInfo(path).Length > 0);
        });
    }

    [Fact]
    public void BoxWritesStlAndIges()
    {
        WithTemporaryDirectory(directory =>
        {
            using Shape box = ShapeFactory.CreateBox(10, 20, 30);
            string stl = ShapeExchange.WriteStl(box, Path.Combine(directory, "box.stl"));
            string iges = ShapeExchange.WriteIges(box, Path.Combine(directory, "box.iges"));

            Assert.True(new FileInfo(stl).Length > 84);
            Assert.True(new FileInfo(iges).Length > 0);
        });
    }

    [Fact]
    public void TransformedCompoundRoundTripsThroughStep()
    {
        WithTemporaryDirectory(directory =>
        {
            using Shape first = ShapeFactory.CreateBox(10, 20, 30);
            using Shape second = ShapeFactory.CreateBox(5, 6, 7);
            using Shape compound = ShapeAssembly.Create(
            [
                new ShapePlacement(first, ShapeTransform.Identity),
                new ShapePlacement(
                    second,
                    ShapeTransform.CreateTranslationAndRotationZ(50, 25, 5, 30)),
            ]);

            Assert.Equal(12, compound.FaceCount);
            string path = ShapeExchange.WriteStep(compound, Path.Combine(directory, "assembly.step"));
            using Shape roundTripped = ShapeExchange.ReadStep(path);
            Assert.Equal(12, roundTripped.FaceCount);
        });
    }

    [Fact]
    public void XdeAssemblyWritesOneAssemblyWithTransformedComponents()
    {
        WithTemporaryDirectory(directory =>
        {
            string firstPath = Path.Combine(directory, "first.step");
            string secondPath = Path.Combine(directory, "second.step");
            string assemblyPath = Path.Combine(directory, "assembly.step");
            using (Shape first = ShapeFactory.CreateBox(10, 20, 30))
            using (Shape second = ShapeFactory.CreateBox(5, 6, 7))
            {
                ShapeExchange.WriteStep(first, firstPath);
                ShapeExchange.WriteStep(second, secondPath);
            }

            using (XdeDocument document = XdeDocument.Create())
            {
                using XdeTransaction transaction = document.BeginTransaction();
                XdeLabel assembly = document.AddAssembly("Imported assembly");
                XdeLabel firstRoot = Assert.Single(document.ImportStep(firstPath));
                XdeLabel secondRoot = Assert.Single(document.ImportStep(secondPath));
                using TopLocLocation identity = TopLocLocation.Identity;
                using GpTrsf transform = ShapeTransform
                    .CreateTranslationAndRotationZ(50, 25, 5, 30)
                    .ToGpTrsf();
                using TopLocLocation placement = TopLocLocation.FromTransform(transform);
                _ = document.AddComponent(assembly, firstRoot, identity);
                _ = document.AddComponent(assembly, secondRoot, placement);
                Assert.True(transaction.Commit());
                document.WriteStep(assemblyPath);
            }

            string stepText = File.ReadAllText(assemblyPath);
            Assert.Contains("NEXT_ASSEMBLY_USAGE_OCCURRENCE", stepText, StringComparison.OrdinalIgnoreCase);
            using Shape roundTripped = ShapeExchange.ReadStep(assemblyPath);
            Assert.Equal(12, roundTripped.FaceCount);
            using XdeDocument restored = XdeDocument.ReadStep(assemblyPath);
            XdeLabel restoredAssembly = Assert.Single(restored.GetFreeShapes());
            Assert.True(restoredAssembly.IsAssembly);
            Assert.Equal(2, restoredAssembly.ComponentCount);
        });
    }

    [Fact]
    public void StepAssemblyInputAbiLayoutIsStable()
    {
        Assert.Equal(64, Marshal.SizeOf<NativeStepAssemblyInput>());
        Assert.Equal(0, Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.FilePath)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.TranslationX)).ToInt32());
        Assert.Equal(
            56,
            Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.RotationAngleRadians)).ToInt32());
    }

    [Fact]
    public void GeneratedPointConstructorCopiesCoordinatesAcrossAbi()
    {
        Point3dRaw point = GeneratedNativeMethods.CreatePoint3d(1.25, -2.5, 9.75);

        Assert.Equal(24, Marshal.SizeOf<Point3dRaw>());
        Assert.Equal(1.25, point.X);
        Assert.Equal(-2.5, point.Y);
        Assert.Equal(9.75, point.Z);
    }

    [Fact]
    public void GeneratedPointDefaultAndCopyConstructorsPreserveValueCopySemantics()
    {
        Point3dRaw defaultPoint = GeneratedNativeMethods.CreatePoint3dDefault();
        Point3dRaw copiedPoint = GeneratedNativeMethods.CreatePoint3dCopy(
            new Point3dRaw(4.5, -6.25, 8.75));

        Assert.Equal(0.0, defaultPoint.X);
        Assert.Equal(0.0, defaultPoint.Y);
        Assert.Equal(0.0, defaultPoint.Z);
        Assert.Equal(4.5, copiedPoint.X);
        Assert.Equal(-6.25, copiedPoint.Y);
        Assert.Equal(8.75, copiedPoint.Z);
    }

    [Fact]
    public void GeneratedPrecisionStaticsExecuteThroughTheValueCopyAbi()
    {
        double angular = GeneratedNativeMethods.PrecisionStaticAngular0();
        double confusion = GeneratedNativeMethods.PrecisionStaticConfusion0();
        double parameterizedApproximation = GeneratedNativeMethods.PrecisionStaticPApproximation1(100.0);

        Assert.True(double.IsFinite(angular) && angular > 0.0);
        Assert.True(double.IsFinite(confusion) && confusion > 0.0);
        Assert.True(double.IsFinite(parameterizedApproximation) && parameterizedApproximation > 0.0);
        Assert.Equal(1, GeneratedNativeMethods.PrecisionStaticIsInfinite0(double.PositiveInfinity));
        Assert.Equal(0, GeneratedNativeMethods.PrecisionStaticIsInfinite0(1.0));
    }

    [Fact]
    public void GeneratedTopAbsEnumStaticsExecuteThroughInt32Abi()
    {
        int composed = GeneratedNativeMethods.TopAbsStaticCompose0(0, 0);
        int reversed = GeneratedNativeMethods.TopAbsStaticReverse0(0);

        Assert.InRange(composed, 0, 3);
        Assert.InRange(reversed, 0, 3);
    }

    [Fact]
    public void GeneratedAdditionalScalarStaticsExecuteThroughValueCopyAbi()
    {
        double resolution = GeneratedNativeMethods.GpStaticResolution0();
        double scalePrecision = GeneratedNativeMethods.TopLocLocationStaticScalePrec0();
        int allocatorType = GeneratedNativeMethods.StandardStaticGetAllocatorType0();
        int stackTraceLength = GeneratedNativeMethods.StandardFailureStaticDefaultStackTraceLength0();
        int jsonKeyLength = GeneratedNativeMethods.StandardDumpStaticJsonKeyLength0(0);

        Assert.True(double.IsFinite(resolution) && resolution > 0.0);
        Assert.True(double.IsFinite(scalePrecision) && scalePrecision > 0.0);
        Assert.InRange(allocatorType, 0, 8);
        Assert.True(stackTraceLength >= 0);
        Assert.True(jsonKeyLength >= 0);
    }

    [Fact]
    public void GpTrsfIdentityAndCompositionPreserveMatrixValues()
    {
        using GpTrsf identity = GpTrsf.Identity;
        Assert.Equal(1.0, identity.Value(1, 1));
        Assert.Equal(1.0, identity.Value(2, 2));
        Assert.Equal(1.0, identity.Value(3, 3));
        Assert.Equal(0.0, identity.Value(1, 4));

        using GpTrsf translation = GpTrsf.Create(10, 20, 30);
        using GpTrsf composed = identity.Multiplied(translation);
        Assert.Equal(10.0, composed.Value(1, 4));
        Assert.Equal(20.0, composed.Value(2, 4));
        Assert.Equal(30.0, composed.Value(3, 4));
    }

    [Fact]
    public void GpTrsfCloneAndInverseAreIndependentValues()
    {
        using GpTrsf source = GpTrsf.Create(10, 0, 0, 0, 0, 1, Math.PI / 2);
        using GpTrsf clone = source.Clone();
        using GpTrsf inverse = source.Inverted();

        Assert.Equal(source.Value(1, 4), clone.Value(1, 4));
        using Shape shape = ShapeFactory.CreateBox(1, 1, 1);
        using Shape transformed = source.Apply(shape);
        Assert.Equal(6, transformed.FaceCount);
        using GpTrsf roundTrip = inverse.Multiplied(source);
        Assert.Equal(1.0, roundTrip.Value(1, 1), 8);
    }

    [Fact]
    public void GpTrsfRejectsNonFiniteValuesAndInvalidMatrixIndices()
    {
        Assert.Throws<ArgumentException>(() => GpTrsf.Create(double.NaN, 0, 0));
        using GpTrsf identity = GpTrsf.Identity;
        Assert.Throws<ArgumentException>(() => identity.Value(0, 1));
        Assert.Throws<ArgumentException>(() => identity.Value(1, 5));
    }

    [Fact]
    public void TopLocLocationPreservesIdentityCompositionAndTransformValues()
    {
        using TopLocLocation identity = TopLocLocation.Identity;
        Assert.True(identity.IsIdentity);

        using GpTrsf translation = GpTrsf.Create(10, 20, 30);
        using TopLocLocation location = TopLocLocation.FromTransform(translation);
        Assert.False(location.IsIdentity);

        using TopLocLocation clone = location.Clone();
        using TopLocLocation inverse = location.Inverted();
        using TopLocLocation roundTrip = inverse.Multiplied(location);
        using GpTrsf roundTripTransform = roundTrip.ToTransform();
        using GpTrsf cloneTransform = clone.ToTransform();
        Assert.True(roundTrip.IsIdentity);
        Assert.Equal(0.0, roundTripTransform.Value(1, 4), 12);
        Assert.Equal(10.0, cloneTransform.Value(1, 4), 12);
    }

    [Fact]
    public void TopLocLocationCanLocateAndMoveShapes()
    {
        using Shape source = ShapeFactory.CreateBox(1, 1, 1);
        using GpTrsf translation = GpTrsf.Create(5, 0, 0);
        using TopLocLocation location = TopLocLocation.FromTransform(translation);
        using Shape located = source.Located(location);
        using Shape moved = source.Moved(location);

        Assert.Equal(6, located.FaceCount);
        Assert.Equal(6, moved.FaceCount);
        Assert.False(located.IsNull);
        Assert.False(moved.IsNull);
    }

    [Fact]
    public void VectorDirectionAxisAndMatrixValuesRoundTrip()
    {
        using GpVec vector = GpVec.Create(3, 4, 0);
        Assert.Equal(5.0, vector.Magnitude, 12);
        Assert.Equal(25.0, vector.Dot(vector), 12);
        using GpVec zAxis = GpVec.Create(0, 0, 1);
        using GpVec cross = vector.Crossed(zAxis);
        Assert.Equal((4.0, -3.0, 0.0), cross.Components);
        using GpTrsf translation = vector.ToTranslation();
        Assert.Equal(3.0, translation.Value(1, 4), 12);

        using GpDir direction = GpDir.Create(0, 0, 1);
        using GpDir reversed = direction.Reversed();
        Assert.Equal((0.0, 0.0, -1.0), reversed.Components);

        using GpAx1 axis = GpAx1.Create(1, 2, 3, 0, 0, 1);
        using GpAx1 reverseAxis = axis.Reversed();
        Assert.Equal((1.0, 2.0, 3.0, 0.0, 0.0, -1.0), reverseAxis.Components);
        using GpTrsf rotation = axis.ToRotation(Math.PI / 2);
        Assert.Equal(0.0, rotation.Value(1, 1), 12);
        Assert.Equal(-1.0, rotation.Value(1, 2), 12);

        using GpMat identity = GpMat.Identity;
        Assert.Equal(1.0, identity.Determinant, 12);
        Assert.Equal(1.0, identity.Value(2, 2), 12);
        using GpMat matrix = GpMat.Create([1, 2, 3, 0, 1, 4, 5, 6, 0]);
        Assert.Equal(1.0, matrix.Determinant, 12);
    }

    [Fact]
    public void DirectionAndMatrixValidationRejectInvalidValues()
    {
        Assert.Throws<OcctException>(() => GpDir.Create(0, 0, 0));
        Assert.Throws<ArgumentException>(() => GpMat.Create([1, 2, 3]));
        using GpMat identity = GpMat.Identity;
        Assert.Throws<ArgumentException>(() => identity.Value(0, 1));
    }

    [Fact]
    public void Utf8AsciiAndExtendedStringsRoundTripWithIndependentOwnership()
    {
        using OcctAsciiString ascii = OcctAsciiString.Create("Hello 世界");
        Assert.Equal("Hello 世界", ascii.Value);
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount("Hello 世界"), ascii.Length);

        ascii.Append("!");
        using OcctAsciiString asciiClone = ascii.Clone();
        Assert.Equal("Hello 世界!", asciiClone.Value);

        using OcctExtendedString extended = ascii.ToExtended();
        Assert.Equal("Hello 世界!", extended.Value);
        Assert.Equal('H', extended[0]);
        using OcctAsciiString roundTrip = extended.ToAscii();
        Assert.Equal(ascii.Value, roundTrip.Value);
    }

    [Fact]
    public void RealSequenceSupportsIndexedMutationAndIndependentClone()
    {
        using OcctRealSequence sequence = OcctRealSequence.Create([1, 2, 3]);
        Assert.Equal(3, sequence.Count);
        Assert.Equal(2.0, sequence[1], 12);

        sequence.Set(1, 20);
        sequence.Add(4);
        using OcctRealSequence clone = sequence.Clone();
        sequence.RemoveAt(0);

        Assert.Equal(3, sequence.Count);
        Assert.Equal(20.0, sequence[0], 12);
        Assert.Equal(4, clone.Count);
        Assert.Equal(1.0, clone[0], 12);
        Assert.Throws<ArgumentException>(() => sequence[99]);
        Assert.Throws<ArgumentException>(() => sequence.Add(double.NaN));
    }

    [Fact]
    public void RealArrayPreservesNativeBoundsAndCloneOwnership()
    {
        using OcctRealArray array = OcctRealArray.Create([1, 2, 3]);
        Assert.Equal(3, array.Count);
        Assert.Equal(1, array.LowerBound);
        Assert.Equal(2.0, array[1], 12);
        array.Set(1, 20);
        using OcctRealArray clone = array.Clone();
        array.Set(0, 10);
        Assert.Equal(10.0, array[0], 12);
        Assert.Equal(20.0, clone[1], 12);
        Assert.Equal(1.0, clone[0], 12);
        Assert.Throws<ArgumentException>(() => array[99]);
        Assert.Throws<ArgumentException>(() => array.Set(0, double.PositiveInfinity));
    }

    [Fact]
    public void RealVectorSupportsAppendMutationAndEnumeration()
    {
        using OcctRealVector vector = OcctRealVector.Create([1, 2]);
        vector.Add(3);
        vector.Set(1, 20);
        using OcctRealVector clone = vector.Clone();
        vector.Set(0, 10);
        Assert.Equal([10.0, 20.0, 3.0], vector.ToArray());
        Assert.Equal([1.0, 20.0, 3.0], clone.ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => vector[-1]);
        Assert.Throws<ArgumentException>(() => vector.Set(99, 1));
        Assert.Throws<ArgumentException>(() => vector.Add(double.NaN));
    }

    [Fact]
    public void IntegerRealMapSupportsLookupMutationRemovalAndClone()
    {
        using OcctIntRealMap map = OcctIntRealMap.Create([new(7, 1.5), new(9, 2.5)]);
        Assert.Equal(2, map.Count);
        Assert.True(map.ContainsKey(7));
        Assert.Equal(1.5, map[7], 12);
        map[7] = 10;
        map[11] = 3;
        using OcctIntRealMap clone = map.Clone();
        Assert.Equal(10.0, clone[7], 12);
        Assert.True(map.Remove(9));
        Assert.False(map.Remove(99));
        Assert.Equal(3, clone.Count);
        Assert.Throws<ArgumentException>(() => map[99]);
        Assert.Throws<ArgumentException>(() => map[12] = double.NaN);
        Assert.Throws<ArgumentException>(() => OcctIntRealMap.Create([new(1, 1), new(1, 2)]));
    }

    [Fact]
    public void IntegerIndexedMapPreservesOrderAndCloneIndependence()
    {
        using OcctIntIndexedMap map = OcctIntIndexedMap.Create([4, 8]);
        Assert.Equal([4, 8], map.ToArray());
        Assert.Equal(1, map.FindIndex(8));
        Assert.Equal(-1, map.FindIndex(99));
        Assert.True(map.Add(12));
        Assert.False(map.Add(12));
        using OcctIntIndexedMap clone = map.Clone();
        Assert.Equal(12, map.RemoveLast());
        Assert.Equal([4, 8], map.ToArray());
        Assert.Equal([4, 8, 12], clone.ToArray());
        Assert.Throws<ArgumentException>(() => map[99]);
        Assert.Throws<ArgumentException>(() => OcctIntIndexedMap.Create([1, 1]));
    }

    [Fact]
    public void CollectionSnapshotsAreStableAcrossMutationAndDispose()
    {
        using OcctRealSequence sequence = OcctRealSequence.Create([1, 2]);
        using OcctRealArray array = OcctRealArray.Create([3, 4]);
        using OcctRealVector vector = OcctRealVector.Create([5, 6]);
        using OcctIntIndexedMap indexed = OcctIntIndexedMap.Create([7, 8]);
        using OcctIntRealMap map = OcctIntRealMap.Create([new(9, 10.0), new(11, 12.0)]);

        double[] sequenceSnapshot = sequence.Snapshot();
        double[] arraySnapshot = array.Snapshot();
        double[] vectorSnapshot = vector.Snapshot();
        int[] indexedSnapshot = indexed.Snapshot();
        KeyValuePair<int, double>[] mapSnapshot = map.Snapshot();

        sequence.Set(0, 100); array.Set(0, 200); vector.Set(0, 300); indexed.Add(13); map[9] = 900;
        Assert.Equal([1.0, 2.0], sequenceSnapshot);
        Assert.Equal([3.0, 4.0], arraySnapshot);
        Assert.Equal([5.0, 6.0], vectorSnapshot);
        Assert.Equal([7, 8], indexedSnapshot);
        Assert.Equal(10.0, mapSnapshot.Single(pair => pair.Key == 9).Value, 12);
    }

    [Fact]
    public void CollectionEnumerationFailsClosedAfterDisposeAndEmptySnapshotsAreSafe()
    {
        OcctRealSequence sequence = OcctRealSequence.Create([]);
        Assert.Empty(sequence.Snapshot());
        IEnumerator<double> enumerator = sequence.GetEnumerator();
        sequence.Dispose();
        Assert.Throws<ObjectDisposedException>(() => enumerator.MoveNext());

        using OcctIntRealMap map = OcctIntRealMap.Create([]);
        Assert.Empty(map.Snapshot());
        using OcctIntIndexedMap indexed = OcctIntIndexedMap.Create([]);
        Assert.Empty(indexed.Snapshot());
    }

    private static void WithTemporaryDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"occtsharp-runtime-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            action(directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(
            uint extendedStyle,
            string className,
            string windowName,
            uint style,
            int x,
            int y,
            int width,
            int height,
            nint parent,
            nint menu,
            nint instance,
            nint parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
