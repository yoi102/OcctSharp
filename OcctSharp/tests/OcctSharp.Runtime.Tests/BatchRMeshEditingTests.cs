using OcctSharp.Interop;
using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

// Expected index arrays are deliberately local to each immutable-correspondence assertion.
#pragma warning disable CA1861

public sealed class BatchRMeshEditingTests
{
    internal static AuthoredMesh Square(bool attributed = true) => new(
        [new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)],
        [new(0, 1, 2), new(0, 2, 3)],
        attributed ? Enumerable.Repeat(new MeshNormal(0, 0, 1), 4) : null,
        attributed ? [new(0, 0), new(1, 0), new(1, 1), new(0, 1)] : null);

    [Fact]
    public void AuthoredChannelsAreCopiedFiniteAndExplicitlyOptional()
    {
        GpPoint[] points = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)];
        MeshTriangle[] facets = [new(0, 1, 2)]; AuthoredMesh mesh = new(points, facets);
        points[0] = new(99, 99, 99); facets[0] = new(0, 0, 0);
        Assert.Equal(new GpPoint(0, 0, 0), mesh.Positions[0]); Assert.Equal(new MeshTriangle(0, 1, 2), mesh.Triangles[0]);
        Assert.Null(mesh.Normals); Assert.Null(mesh.UVs);
        Assert.Throws<ArgumentException>(() => new AuthoredMesh(points, facets, [new(0, 0, 1)]));
        Assert.Throws<ArgumentException>(() => new AuthoredMesh(points, facets, uvs: [new(0, 0)]));
        Assert.Throws<ArgumentException>(() => new AuthoredMesh([new(double.NaN, 0, 0)], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthoredMesh(points, [new(-1, 1, 2)]));
        Assert.Throws<ArgumentException>(() => new AuthoredMesh(points, facets, Enumerable.Repeat(new MeshNormal(0, 0, 0), 3)));
        AuthoredMesh undefined = new(points, facets, Enumerable.Repeat(MeshNormal.Undefined, 3));
        Assert.All(undefined.Normals!, n => Assert.False(n.IsDefined));
        Assert.Equal(72, Marshal.SizeOf<AuthoredVertexRaw>()); Assert.Equal(16, Marshal.SizeOf<AuthoredTriangleRaw>());
        Assert.Equal("OcctSharp.MeshData", typeof(AuthoredMesh).Assembly.GetName().Name);
        Assert.DoesNotContain(typeof(AuthoredMesh).Assembly.GetReferencedAssemblies(), a => a.Name == "OcctSharp.Modeling");
    }

    [Fact]
    public void PolylinesAndCornerSeamsRetainLogicalOrigins()
    {
        AuthoredMesh source = Square();
        MeshPolyline line = new([0, 1, 2, 3, 0], true, [0, 1, 2, 3, 4]);
        using Shape edge = MeshTopology.CreatePolyline(source, line);
        Assert.Equal(ShapeKind.Edge, edge.Kind);
        using Shape standalone = MeshTopology.CreatePolyline(new MeshPolyline3d([new(0, 0, 0), new(2, 0, 0)], parameters: [0, 2]));
        Assert.Equal(ShapeKind.Edge, standalone.Kind);
        Assert.Throws<ArgumentException>(() => new MeshPolyline([0, 1, 2], true));
        Assert.Throws<ArgumentException>(() => new MeshPolyline([0, 1], parameters: [2, 1]));
        Assert.Throws<ArgumentException>(() => new MeshPolyline3d([new(0, 0, 0), new(0, 0, 0)]));
        MeshUv[] corners = [new(0, 0), new(1, 0), new(1, 1), new(2, 0), new(3, 1), new(2, 1)];
        AuthoredMesh seams = AuthoredMesh.FromCorners(source.Positions, source.Triangles, cornerUvs: corners);
        Assert.Equal(6, seams.Positions.Count); Assert.Equal(seams.VertexOrigins[0], seams.VertexOrigins[3]);
        Assert.NotEqual(seams.UVs![0], seams.UVs[3]); corners[0] = new(55, 55); Assert.Equal(new MeshUv(0, 0), seams.UVs[0]);
        Assert.Throws<ArgumentException>(() => AuthoredMesh.FromCorners(source.Positions, source.Triangles, cornerUvs: [new(0, 0)]));
    }

    [Fact]
    public void PositionConnectivityDeletionExtractionCompactionAndMapCompositionAreImmutable()
    {
        AuthoredMesh source = Square(); MeshTriangleSelection old = source.SelectTriangles([1]);
        MeshEditResult moved = MeshEditing.SetPositions(source, source.SelectVertices([2]), [new(3, 2, 0)]);
        Assert.Equal(new GpPoint(2, 2, 0), source.Positions[2]); Assert.Null(moved.Mesh.Normals); Assert.Equal(source.UVs, moved.Mesh.UVs);
        Assert.Equal(2, moved.AffectedTriangles.Indices.Count);
        Assert.Throws<ArgumentException>(() => MeshEditing.DeleteTriangles(moved.Mesh, old));
        MeshEditResult deleted = MeshEditing.DeleteTriangles(moved.Mesh, moved.Map.MapSelection(old, moved.Mesh));
        Assert.Equal(new[] { 1 }, deleted.Map.Triangles.Deleted); Assert.Single(deleted.Mesh.Triangles);
        MeshEditResult compacted = MeshEditing.Compact(deleted.Mesh); Assert.Equal(3, compacted.Mesh.Positions.Count);
        MeshEditMap composed = moved.Map.Then(deleted.Map).Then(compacted.Map);
        Assert.Equal(new[] { 1 }, composed.Triangles.Deleted); Assert.Equal(new[] { 3 }, composed.Vertices.Deleted);
        Assert.Throws<ArgumentException>(() => composed.Then(moved.Map));
        MeshEditResult extracted = MeshEditing.Extract(source, old);
        Assert.Equal(3, extracted.Mesh.Positions.Count); Assert.Single(extracted.Mesh.Triangles); Assert.Equal(0, extracted.Mesh.Triangles[0].A);
        Assert.Equal(new[] { 0 }, extracted.Map.Triangles.Targets[1]);
        MeshEditResult connectivity = MeshEditing.SetTriangles(source, source.SelectTriangles([0]), [new(0, 1, 3)]);
        Assert.Equal(2, source.Triangles[0].C); Assert.Equal(3, connectivity.Mesh.Triangles[0].C); Assert.Null(connectivity.Mesh.Normals);
        Assert.Throws<ArgumentOutOfRangeException>(() => MeshEditing.SetTriangles(source, source.SelectTriangles([0]), [new(0, 1, 99)]));
        AuthoredMesh withLine = new([.. source.Positions, new(5, 0, 0), new(6, 0, 0)], source.Triangles,
            polylines: [new MeshPolyline([4, 5])]);
        Assert.Equal(6, MeshEditing.Compact(withLine).Mesh.Positions.Count);
    }

    [Fact]
    public void ConcatenationPreservesDistinctGroupsMaterialsAndSourceMaps()
    {
        AuthoredMesh a = Square(), b = Square(); MeshConcatenationResult joined = MeshEditing.Concatenate([a, b]);
        Assert.Equal(8, joined.Mesh.Positions.Count); Assert.Equal(4, joined.Mesh.Triangles.Count);
        Assert.Equal(2, joined.Mesh.Groups.Count); Assert.NotEqual(joined.Mesh.Triangles[0].Group, joined.Mesh.Triangles[2].Group);
        Assert.Equal(new[] { 4 }, joined.SourceMaps[1].Vertices.Targets[0]);
        Assert.Equal(new[] { 2 }, joined.SourceMaps[1].Triangles.Targets[0]);
        Assert.Equal(b.VertexOrigins[0], joined.Mesh.VertexOrigins[4]);
        Assert.Throws<ArgumentException>(() => MeshEditing.Concatenate([a, Square(false)]));
    }

    [Fact]
    public void FullAdjacencyBoundariesComponentsAndConstrainedExpansionAreObservable()
    {
        AuthoredMesh square = Square(); MeshConnectivity graph = MeshConnectivity.Create(square);
        Assert.Equal(new[] { 1 }, graph.Neighbors(0)); Assert.Equal(new[] { 0, 1 }, graph.IncidentTriangles[0]);
        Assert.Equal(4, graph.Edges.Count(e => e.IsBoundary));
        MeshBoundaryResult boundary = graph.Boundaries(); Assert.Empty(boundary.BranchVertices);
        Assert.True(Assert.Single(boundary.Chains).IsClosed); Assert.Equal(5, boundary.Chains[0].Vertices.Count);
        Assert.Equal(2, graph.Expand(square.SelectTriangles([0]), 1).Indices.Count);
        AuthoredMesh grouped = MeshEditing.AssignGroup(square, square.SelectTriangles([1]), new(8, "Blue", "blue")).Mesh;
        MeshConnectivity groupGraph = MeshConnectivity.Create(grouped);
        Assert.Single(groupGraph.Expand(grouped.SelectTriangles([0]), 1).Indices);
        Assert.Equal(2, groupGraph.Expand(grouped.SelectTriangles([0]), 1, false, false).Indices.Count);
        AuthoredMesh disconnected = MeshEditing.Concatenate([square, square]).Mesh;
        IReadOnlyList<MeshEditResult> components = MeshConnectivity.Create(disconnected).ExtractComponents();
        Assert.Equal(2, components.Count); Assert.All(components, c => Assert.Equal(2, c.Mesh.Triangles.Count));
        AuthoredMesh junction = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, -1, 0), new(0, 0, 1)],
            [new(0, 1, 2), new(1, 0, 3), new(0, 1, 4)]);
        MeshConnectivity junctionGraph = MeshConnectivity.Create(junction);
        Assert.Equal(3, Assert.Single(junctionGraph.Edges, e => e.IsNonManifold).Uses.Count);
        Assert.Equal(2, junctionGraph.Neighbors(0).Count); Assert.NotEmpty(junctionGraph.Boundaries().BranchVertices);
        Assert.False(MeshEditing.OrientComponents(junction).IsOrientable);
    }

    [Fact]
    public void CoherentPatchInsertionIsAtomicAndDegenerationAndDuplicatesHaveExactDeletionMaps()
    {
        AuthoredMesh source = new(Square().Positions, [new(0, 1, 2)]);
        MeshEditResult patch = MeshEditing.InsertPatch(source, [new(0, 2, 3)]);
        Assert.Equal(2, patch.Mesh.Triangles.Count); Assert.Single(source.Triangles);
        Assert.Equal(4, MeshConnectivity.Create(patch.Mesh).Edges.Count(e => e.IsBoundary));
        Assert.Throws<ArgumentException>(() => MeshEditing.InsertPatch(patch.Mesh, [new(0, 2, 1)]));
        Assert.Equal(2, patch.Mesh.Triangles.Count);
        MeshEditResult replaced = MeshEditing.InsertPatch(source, [], source.SelectTriangles([0]), [new(0, 1, 3)]);
        Assert.Equal(3, replaced.Mesh.Triangles[0].C);
        AuthoredMesh flawed = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(2, 0, 0)],
            [new(0, 1, 2), new(0, 1, 3), new(0, 0, 2), new(1, 2, 0), new(0, 2, 1)]);
        MeshEditResult clean = MeshEditing.RemoveDegenerate(flawed);
        Assert.Equal(new[] { 1, 2 }, clean.Map.Triangles.Deleted); Assert.Equal(3, clean.Mesh.Triangles.Count);
        MeshEditResult duplicates = MeshEditing.RemoveDuplicates(clean.Mesh);
        Assert.Equal(2, duplicates.Mesh.Triangles.Count); Assert.Equal(1, duplicates.Map.Triangles.Deleted.Single());
        Assert.Single(MeshEditing.RemoveDuplicates(clean.Mesh, true).Mesh.Triangles);
    }

    [Fact]
    public void WeldingUsesActualNativeIndicesAndPreservesExplicitAttributeSeams()
    {
        AuthoredMesh square = Square();
        AuthoredMesh corners = AuthoredMesh.FromCorners(square.Positions, square.Triangles,
            cornerUvs: [new(0, 0), new(1, 0), new(1, 1), new(2, 0), new(3, 1), new(2, 1)]);
        MeshEditResult preserved = MeshEditing.Weld(corners); Assert.Equal(6, preserved.Mesh.Positions.Count);
        MeshEditResult geometric = MeshEditing.Weld(corners, new(PreserveAttributes: false));
        Assert.Equal(4, geometric.Mesh.Positions.Count); Assert.Null(geometric.Mesh.UVs); Assert.NotEmpty(geometric.Diagnostics);
        Assert.Equal(geometric.Map.Vertices.Targets[0], geometric.Map.Vertices.Targets[3]);
        Assert.Equal(2, geometric.Mesh.Triangles.Count); Assert.Single(MeshConnectivity.Create(geometric.Mesh).Components());
        Assert.Throws<ArgumentOutOfRangeException>(() => MeshEditing.Weld(square, new(double.NaN)));
        AuthoredMesh near = new([new(0, 0, 0), new(1e-5, 0, 0), new(0, 1, 0)], [new(0, 1, 2)]);
        MeshEditResult collapsed = MeshEditing.Weld(near, new(1e-4)); Assert.Empty(collapsed.Mesh.Triangles);
        Assert.Equal(new[] { 0 }, collapsed.Map.Triangles.Deleted);
    }

    [Fact]
    public void CreaseSplittingOrientationNormalsAndUvEditingAreRevisionBound()
    {
        AuthoredMesh square = Square();
        AuthoredMesh bent = MeshEditing.SetPositions(square, square.SelectVertices([3]), [new(0, 2, 2)]).Mesh;
        MeshEditResult split = MeshEditing.SplitBoundaryVertices(bent, bent.SelectTriangles([0, 1]), 0.1);
        Assert.Equal(6, split.Mesh.Positions.Count); Assert.Equal(2, split.Map.Vertices.Targets[0].Count);
        MeshEditResult normals = MeshEditing.RebuildNormals(split.Mesh); Assert.All(normals.Mesh.Normals!, n => Assert.True(n.IsDefined));
        Assert.NotEqual(normals.Mesh.Normals![0], normals.Mesh.Normals[split.Map.Vertices.Targets[0][1]]);
        MeshEditResult uv = MeshEditing.TransformUvs(normals.Mesh, normals.Mesh.SelectTriangles([0]), new(2, 0, 1, 0, 2, 3));
        Assert.Equal(new MeshUv(1, 3), uv.Mesh.UVs![0]); Assert.Equal(normals.Mesh.Positions, uv.Mesh.Positions);
        Assert.Throws<ArgumentException>(() => MeshEditing.TransformUvs(square, square.SelectTriangles([0]), MeshUvTransform.Identity));
        MeshEditResult edited = MeshEditing.SetNormals(square, square.SelectVertices([0]), [new(0, 0, 12)]);
        Assert.Equal(new MeshNormal(0, 0, 1), edited.Mesh.Normals![0]);
        AuthoredMesh wrong = MeshEditing.SetTriangles(square, square.SelectTriangles([1]), [new(0, 3, 2)]).Mesh;
        MeshOrientationResult repaired = MeshEditing.OrientComponents(wrong); Assert.True(repaired.IsOrientable);
        Assert.All(repaired.Edit!.Mesh.Normals!, n => Assert.Equal(1, n.Z, 8));
        Assert.Throws<ArgumentException>(() => MeshEditing.ReverseComponents(square, square.SelectTriangles([0])));
        MeshEditResult reversed = MeshEditing.ReverseComponents(square, square.SelectTriangles([0, 1]));
        Assert.All(reversed.Mesh.Normals!, n => Assert.Equal(-1, n.Z, 8));
        AuthoredMesh isolated = new([new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)], [new(0, 1, 2)]);
        Assert.All(MeshEditing.RebuildNormals(isolated).Mesh.Normals!, n => Assert.False(n.IsDefined));
    }

    [Fact]
    public void AffineMirrorUnitsAndSelectedMeasurementsRetainGeometryAndProvenance()
    {
        AuthoredMesh square = Square();
        MeshEditResult rigid = MeshEditing.TransformRigid(square, AuthoredMeshTransform.Scale(3));
        Assert.Equal(36, MeshEditing.Measure(rigid.Mesh, rigid.Mesh.SelectTriangles([0, 1])).SurfaceArea, 7);
        AuthoredMeshTransform mirror = new(-2, 0.5, 0, 10, 0, 3, 0, 2, 0, 0, 4, 1);
        MeshEditResult affine = MeshEditing.Transform(square, mirror);
        Assert.Equal(2, affine.Mesh.Triangles[0].B); Assert.Equal(1, affine.Mesh.Triangles[0].C);
        Assert.Equal(new GpPoint(10, 2, 1), affine.Mesh.Positions[0]); Assert.Equal(24, MeshEditing.Measure(affine.Mesh, affine.Mesh.SelectTriangles([0, 1])).SurfaceArea, 8);
        Assert.All(affine.Mesh.Normals!, n => Assert.Equal(1, n.Z, 8));
        Assert.Throws<ArgumentException>(() => MeshEditing.Transform(square, AuthoredMeshTransform.Scale(0)));
        Assert.Throws<ArgumentException>(() => MeshEditing.TransformRigid(square, mirror));
        MeshCoordinates target = new(1, MeshUpAxis.Y, MeshHandedness.Left);
        MeshEditResult converted = MeshEditing.ConvertCoordinates(square, target);
        Assert.Equal(target, converted.Mesh.Coordinates); Assert.Equal(-0.002, converted.Mesh.Positions[2].X, 9);
        AuthoredMesh twice = MeshEditing.ConvertCoordinates(converted.Mesh, target).Mesh;
        Assert.Equal(converted.Mesh.Positions, twice.Positions);
        AuthoredMesh roundtrip = MeshEditing.ConvertCoordinates(converted.Mesh, square.Coordinates).Mesh;
        for (int i = 0; i < 4; ++i) Assert.True(square.Positions[i].DistanceTo(roundtrip.Positions[i]) < 1e-9);
        MeshRegionStatistics half = MeshEditing.Measure(square, square.SelectTriangles([0]));
        Assert.Equal(2, half.SurfaceArea, 9); Assert.Equal(3, half.VertexCount); Assert.Equal(square.Revision, half.Revision);
        Assert.Null(MeshEditing.Measure(square, square.SelectTriangles([])).Bounds);
        AuthoredMeshStatistics statistics = MeshEditing.Inspect(square);
        Assert.Equal(square.Revision, statistics.Revision); Assert.Equal(4, statistics.SurfaceArea, 9);
        Assert.Equal(4, statistics.BoundaryEdgeCount); Assert.Equal(0, statistics.DegenerateTriangleCount);
        Assert.Equal(1, statistics.ComponentCount); Assert.False(statistics.IsClosedManifold);
    }

    [Fact]
    public void DiscreteOwnersAndExactCacheCopiesHaveSeparateCapabilitiesAndLifetimes()
    {
        AuthoredMesh square = Square(); DiscreteMeshModel model = MeshTopology.Create(square);
        using Shape face = model.CopyShape(); Assert.False(MeshTopology.IsSurfaceBacked(face));
        Assert.Throws<NotSupportedException>(() => MeshTopology.RequireSurfaceBacked(face));
        Assert.Throws<ArgumentException>(() => MeshTopology.Remesh(face, [0]));
        using GpVec direction = GpVec.Create(0, 0, 1);
        Assert.Throws<ArgumentException>(() => face.Extrude(direction));
        Assert.Throws<ArgumentException>(() => face.InspectProperties(InspectionPropertyKind.Area));
        AuthoredMesh snapshot = model.Snapshot(); Assert.Equal(square.Triangles, snapshot.Triangles); Assert.Equal(square.Positions, snapshot.Positions);
        model.Dispose(); model.Dispose(); Assert.Throws<ObjectDisposedException>(() => model.Snapshot());
        Assert.Equal(2, MeshTopology.SnapshotExisting(face).Mesh.Triangles.Count);
        using Shape exact = ShapeFactory.CreateBox(2, 2, 2);
        Assert.True(MeshTopology.IsSurfaceBacked(exact));
        Assert.Throws<ArgumentException>(() => MeshTopology.SnapshotExisting(exact));
        using Shape meshed = MeshTopology.Remesh(exact, Enumerable.Range(0, 6));
        AuthoredMesh before = MeshTopology.SnapshotExisting(meshed).Mesh;
        using Shape replacement = MeshTopology.ReplaceTriangulation(meshed, 0, new(square.Positions, [new(0, 1, 2)]));
        AuthoredMesh replaced = MeshTopology.SnapshotExisting(replacement).Mesh;
        Assert.Equal(before.Triangles.Count - 1, replaced.Triangles.Count);
        Assert.Equal(before.Triangles, MeshTopology.SnapshotExisting(meshed).Mesh.Triangles);
        Assert.Equal(8, replacement.InspectProperties(InspectionPropertyKind.Volume).Mass, 6);
        Assert.Equal(8, meshed.InspectProperties(InspectionPropertyKind.Volume).Mass, 6);
        using Shape remeshed = MeshTopology.Remesh(replacement, [0]);
        Assert.Equal(before.Triangles.Count, MeshTopology.SnapshotExisting(remeshed).Mesh.Triangles.Count);
        Assert.Equal(before.Triangles.Count - 1, MeshTopology.SnapshotExisting(replacement).Mesh.Triangles.Count);
        exact.Dispose(); meshed.Dispose(); Assert.True(MeshTopology.IsSurfaceBacked(remeshed));
    }
}
