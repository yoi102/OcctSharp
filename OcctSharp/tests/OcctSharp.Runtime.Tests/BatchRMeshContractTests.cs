using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861

public sealed class BatchRMeshContractTests
{
    [Fact]
    public void NonOrientableManifoldCycleIsReportedWithoutGuessing()
    {
        const int segments = 5;
        List<GpPoint> points = []; List<MeshTriangle> triangles = [];
        for (int i = 0; i < segments; ++i)
        {
            double angle = Math.Tau * i / segments;
            foreach (double width in new[] { -0.4, 0.4 })
                points.Add(new((2 + width * Math.Cos(angle / 2)) * Math.Cos(angle),
                    (2 + width * Math.Cos(angle / 2)) * Math.Sin(angle), width * Math.Sin(angle / 2)));
            int a = 2 * i, b = a + 1;
            int c = i == segments - 1 ? 1 : a + 2, d = i == segments - 1 ? 0 : a + 3;
            triangles.Add(new(a, b, d)); triangles.Add(new(a, d, c));
        }
        AuthoredMesh mobius = new(points, triangles);
        MeshConnectivity graph = MeshConnectivity.Create(mobius);
        Assert.DoesNotContain(graph.Edges, e => e.IsNonManifold); Assert.Single(graph.Components());
        MeshOrientationResult result = MeshEditing.OrientComponents(mobius);
        Assert.False(result.IsOrientable); Assert.Null(result.Edit);
        Assert.Contains(result.Issues, i => i.Reason.Contains("Non-orientable", StringComparison.Ordinal));
        Assert.Equal(triangles, mobius.Triangles);
    }

    [Fact]
    public void AreaWeightingAndAffineNormalsAgreeWithIndependentCrossProducts()
    {
        AuthoredMesh wedge = new([new(0, 0, 0), new(2, 0, 0), new(0, 2, 0), new(0, 0, 1)], [new(0, 1, 2), new(1, 0, 3)]);
        AuthoredMesh rebuilt = MeshEditing.RebuildNormals(wedge).Mesh;
        Assert.Equal(2 / Math.Sqrt(20), rebuilt.Normals![0].Y, 10);
        Assert.Equal(4 / Math.Sqrt(20), rebuilt.Normals[0].Z, 10);
        AuthoredMesh plane = MeshEditing.RebuildNormals(new AuthoredMesh([new(0, 0, 0), new(2, 1, 1), new(1, 3, 2)], [new(0, 1, 2)])).Mesh;
        AuthoredMesh transformed = MeshEditing.Transform(plane, new(-2, 0.5, 1, 10, 0, 3, 0.2, 2, 0, 0, 4, 1)).Mesh;
        MeshTriangle t = transformed.Triangles[0];
        GpPoint a = transformed.Positions[t.A], b = transformed.Positions[t.B], c = transformed.Positions[t.C];
        MeshNormal expected = new MeshNormal((b.Y - a.Y) * (c.Z - a.Z) - (b.Z - a.Z) * (c.Y - a.Y),
            (b.Z - a.Z) * (c.X - a.X) - (b.X - a.X) * (c.Z - a.Z),
            (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)).Normalized();
        Assert.All(transformed.Normals!, n =>
        {
            Assert.Equal(expected.X, n.X, 9); Assert.Equal(expected.Y, n.Y, 9); Assert.Equal(expected.Z, n.Z, 9);
        });
        AuthoredMesh largeNormals = new(wedge.Positions, wedge.Triangles, Enumerable.Repeat(new MeshNormal(0, 1e300, 1e300), 4));
        AuthoredMesh normalized = MeshEditing.Transform(largeNormals, AuthoredMeshTransform.Identity).Mesh;
        Assert.Equal(1 / Math.Sqrt(2), normalized.Normals![0].Y, 10);
        Assert.Equal(1 / Math.Sqrt(2), MeshEditing.ConvertCoordinates(largeNormals, new()).Mesh.Normals![0].Y, 6);
    }

    [Fact]
    public void LocalSplittingPreservesUnselectedVertexFansAndEmptySelectionChannels()
    {
        AuthoredMesh source = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(-1, 0, 0), new(0, -1, 0),
            new(10, 0, 0), new(11, 0, 0), new(10, 1, 0)], [new(0, 1, 2), new(0, 3, 4), new(5, 6, 7)],
            Enumerable.Repeat(new MeshNormal(0, 0, 1), 8));
        MeshEditResult empty = MeshEditing.SplitBoundaryVertices(source, source.SelectTriangles([]));
        Assert.Equal(source.Triangles, empty.Mesh.Triangles); Assert.Equal(source.Normals, empty.Mesh.Normals);
        Assert.Empty(empty.AffectedTriangles.Indices);
        MeshEditResult isolated = MeshEditing.SplitBoundaryVertices(source, source.SelectTriangles([2]));
        Assert.Equal(8, isolated.Mesh.Positions.Count); Assert.Equal(source.Triangles, isolated.Mesh.Triangles);
        Assert.All(isolated.Map.Vertices.Targets, row => Assert.Single(row));
    }

    [Fact]
    public void MaterialPartitionsAndFloatHashPrecisionCannotSilentlyCollapseGeometry()
    {
        AuthoredMesh joined = MeshEditing.Concatenate([BatchRMeshEditingTests.Square(false), BatchRMeshEditingTests.Square(false)]).Mesh;
        AuthoredMesh materials = MeshEditing.AssignGroup(joined, joined.SelectTriangles([2, 3]), new(8, "Other", "other")).Mesh;
        Assert.Equal(8, MeshEditing.Weld(materials, new(PreserveAttributes: false)).Mesh.Positions.Count);
        Assert.Equal(4, MeshEditing.Weld(materials, new(PreserveAttributes: false, PreserveMaterials: false)).Mesh.Positions.Count);
        AuthoredMesh translated = new([new(100_000_000, 0, 0), new(100_000_001, 0, 0), new(100_000_000, 10, 0)], [new(0, 1, 2)]);
        Assert.Throws<ArgumentException>(() => MeshEditing.Weld(translated, new(0)));
        Assert.Equal(3, translated.Positions.Count);
        AuthoredMesh withLine = new([new(0, 0, 0), new(1e-5, 0, 0), new(0, 1, 0)], [new(0, 1, 2)],
            polylines: [new MeshPolyline([0, 1])]);
        MeshEditResult collapsed = MeshEditing.Weld(withLine, new(1e-4));
        Assert.Empty(collapsed.Mesh.Polylines); Assert.Contains(collapsed.Diagnostics, d => d.Contains("Polyline 0", StringComparison.Ordinal));
    }

    [Fact]
    public void BoundedInputIsRejectedBeforeEnumeratingOrAllocatingReportedExcess()
    {
        Assert.Throws<ArgumentException>(() => new AuthoredMesh(new OversizedPoints(), []));
        MeshPolyline repeated = new([0, 1]);
        Assert.Throws<ArgumentException>(() => new AuthoredMesh([new(0, 0, 0), new(1, 0, 0)], [],
            polylines: Enumerable.Repeat(repeated, MeshDataValidation.MaximumElements / 2 + 1)));
        Assert.Throws<ArgumentException>(() => new AuthoredMesh([], [], coordinates: new(double.PositiveInfinity)));
        AuthoredMesh source = BatchRMeshEditingTests.Square();
        Assert.Throws<ArgumentException>(() => MeshEditing.SetNormals(source, source.SelectVertices([0]), [new(double.NaN, 0, 1)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeshConnectivity.Create(source).Expand(source.SelectTriangles([0]), int.MaxValue));
    }

    [Fact]
    public unsafe void NativeCapacityInvalidFlagsAndShapeFailureOutputsAreAtomic()
    {
        OcctRuntime.EnsureCompatible();
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.MeshAuthorFace(null, 1, null, 1, out nint failed)); Assert.Equal(0, failed);
        AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(BatchRMeshEditingTests.Square());
        AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(BatchRMeshEditingTests.Square().Triangles);
        fixed (AuthoredVertexRaw* v = vertices)
        fixed (AuthoredTriangleRaw* t = triangles)
        {
            v[0] = v[0] with { Reserved = 1 };
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.MeshAuthorFace(v, vertices.Length, t, triangles.Length, out failed)); Assert.Equal(0, failed);
            v[0] = v[0] with { Reserved = 0 };
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.MeshPolyConnect(v, vertices.Length, t, triangles.Length, null, 0));
            AuthoredTriangleRaw sentinel = new() { A = 77, B = 88, C = 99 };
            Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.MeshCoherentPatch(v, vertices.Length, t, triangles.Length,
                null, null, 0, null, 0, &sentinel, 1));
            Assert.Equal(77, sentinel.A); Assert.Equal(88, sentinel.B); Assert.Equal(99, sentinel.C);
        }
        using Shape discrete = MeshTopology.CreateFace(BatchRMeshEditingTests.Square());
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.MeshExistingSnapshot(discrete.Handle, null, 1, out _, null, 1, out _, out _));
        using ShapeHandle invalid = new((nint)0x12345);
        Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.MeshCopyShape(invalid, out failed)); Assert.Equal(0, failed);
        Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.MeshIsExact(invalid, out int exact)); Assert.Equal(0, exact);
    }

    [Fact]
    public void CacheCopiesRetainUnselectedMeshesAndIndependentGeometryAcrossRepeatedDisposal()
    {
        for (int run = 0; run < 12; ++run)
        {
            using Shape box = ShapeFactory.CreateBox(2, 3, 4);
            using Shape mesh = MeshTopology.Remesh(box, Enumerable.Range(0, 6));
            using Shape copy = MeshTopology.Remesh(mesh, [0], 0.01, 0.2);
            Shape[] sourceFaces = mesh.GetFaces(), resultFaces = copy.GetFaces();
            try
            {
                for (int i = 1; i < 6; ++i)
                {
                    AuthoredMesh before = MeshTopology.SnapshotExisting(sourceFaces[i]).Mesh;
                    AuthoredMesh after = MeshTopology.SnapshotExisting(resultFaces[i]).Mesh;
                    Assert.Equal(before.Positions, after.Positions); Assert.Equal(before.Triangles, after.Triangles); Assert.Equal(before.UVs, after.UVs);
                }
                using Shape detached = MeshTopology.CopyWithTriangulation(resultFaces[0]);
                mesh.Dispose(); box.Dispose(); copy.Dispose();
                Assert.Equal(2, MeshTopology.SnapshotExisting(detached).Mesh.Triangles.Count);
                Assert.True(MeshTopology.IsSurfaceBacked(detached));
            }
            finally
            {
                foreach (Shape face in sourceFaces) face.Dispose();
                foreach (Shape face in resultFaces) face.Dispose();
            }
        }
    }

    private sealed class OversizedPoints : System.Collections.Generic.ICollection<GpPoint>
    {
        public int Count => int.MaxValue;
        public bool IsReadOnly => true;
        public IEnumerator<GpPoint> GetEnumerator() => throw new InvalidOperationException("Must not enumerate an excessive count.");
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(GpPoint item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(GpPoint item) => throw new NotSupportedException();
        public void CopyTo(GpPoint[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(GpPoint item) => throw new NotSupportedException();
    }
}
