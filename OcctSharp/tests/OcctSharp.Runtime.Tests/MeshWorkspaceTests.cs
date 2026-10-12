using System.Reflection;
using System.Runtime.InteropServices;
using OcctSharp.Generated;
using OcctSharp.MeshingWorkspaces;
using OcctSharp.PolyWorkspaces;
using OcctSharp.Tests.Shared;
using OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class MeshWorkspaceTests
{
    [Fact] public void ExactMeshCopiesExistingEditingAndRealFileReadback() => BatchARMeshWorkflow.Run();

    [Fact] public void EmptyAndPointOnlyMeshesPreserveAbsentChannels()
    {
        using var empty = PolyDataWorkspaces.FromMesh(new([], []));
        Assert.Empty(empty.Mesh.Positions); Assert.Empty(empty.Mesh.Triangles); Assert.Null(empty.Bounds);
        Assert.Null(empty.Mesh.Normals); Assert.Null(empty.Mesh.UVs); Assert.Equal(PolyConnectivityDisposition.Empty, empty.ConnectivityDisposition);
        using var native = empty.CopyTriangulation(); using var copied = PolyDataWorkspaces.Copy(native);
        Assert.Empty(copied.Mesh.Positions); Assert.Null(copied.Neighbors);
        using var points = PolyDataWorkspaces.FromMesh(new([new(2, 3, 4)], [], uvs: [new(.1, .2)]), false);
        Assert.False(points.IsDoublePrecision); Assert.Single(points.Mesh.Positions); Assert.Empty(points.Mesh.Triangles);
        Assert.NotNull(points.Mesh.UVs); Assert.NotNull(points.Bounds); Assert.Equal(2, points.Bounds.Value.Minimum.X);
    }
    [Fact] public void FloatStorageQuantizesNodesAndUvsWithoutLosingLogicalOrigins()
    {
        const double value = .123456789123;
        AuthoredMesh source = new([new(value, 0, 0), new(1, 0, 0), new(0, 1, 0)], [new(0, 1, 2)],
            [MeshNormal.Undefined, new(0, 0, 1), new(0, 0, 1)], [new(value, .5), new(1, 0), new(0, 1)]);
        using var doubles = PolyDataWorkspaces.FromMesh(source);
        using var floats = PolyDataWorkspaces.FromMesh(source, false);
        Assert.Equal(value, doubles.Mesh.Positions[0].X); Assert.Equal((double)(float)value, floats.Mesh.Positions[0].X);
        Assert.Equal((double)(float)value, floats.Mesh.UVs![0].U); Assert.False(floats.Mesh.Normals![0].IsDefined);
        Assert.Equal(source.Revision, floats.SourceRevision); Assert.NotEqual(source.Revision, floats.Mesh.Revision);
        Assert.Equal(source.VertexOrigins, floats.Mesh.VertexOrigins);
        Assert.Equal(source.Triangles, floats.Mesh.Triangles);
    }
    [Fact] public void FloatOverflowIsRejectedBeforePublishingOwner()
    {
        AuthoredMesh source = new([new(double.MaxValue, 0, 0)], []);
        Assert.Throws<ArgumentException>(() => PolyDataWorkspaces.FromMesh(source, false));
        AuthoredMesh uv = new([new(0, 0, 0)], [], uvs: [new(double.MaxValue, 0)]);
        Assert.Throws<ArgumentException>(() => PolyDataWorkspaces.FromMesh(uv, false));
    }
    [Fact] public void NativeCopiesMetadataAndChannelsStayDetachedFromMutation()
    {
        using var snapshot = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        using var native = snapshot.CopyTriangulation(); native.Deflection(.125); native.SetMeshPurpose(2);
        using var initial = new PolyTriangulationParameters(.25, .75, .01); native.Parameters(initial);
        using var copied = PolyDataWorkspaces.Copy(native);
        Assert.Equal(.125, copied.Deflection); Assert.Equal(2U, copied.MeshPurpose);
        native.SetNode(1, new Point3d(999, 999, 999)); native.RemoveUVNodes(); native.RemoveNormals(); native.Deflection(.9);
        using var output = copied.CopyTriangulation(); copied.Dispose(); snapshot.Dispose(); native.Dispose(); initial.Dispose();
        Assert.Equal(0, output.Node(1).X); Assert.True(output.HasUVNodes()); Assert.True(output.HasNormals());
        Assert.Equal(.125, output.Deflection()); Assert.Equal(2U, output.MeshPurpose());
        using var metadata = output.Parameters(); Assert.NotNull(metadata); Assert.Equal(.25, metadata.Deflection());
    }
    [Fact] public void CachedBoundsAreRecomputedOnPrivateCopy()
    {
        using var snapshot = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square()); using var source = snapshot.CopyTriangulation();
        source.UpdateCachedMinMax(); Assert.True(source.HasCachedMinMax()); source.SetNode(2, new Point3d(20, 0, 0));
        using var copied = PolyDataWorkspaces.Copy(source);
        Assert.Equal(20, copied.Bounds!.Value.Maximum.X); Assert.True(source.HasCachedMinMax());
        source.SetNode(2, new Point3d(200, 0, 0)); Assert.Equal(20, copied.Bounds.Value.Maximum.X);
    }
    [Fact] public void MultipleOwningQueriesDoNotShareMutableData()
    {
        using var snapshot = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        using var first = snapshot.CopyTriangulation(); using var second = snapshot.CopyTriangulation();
        first.SetNode(1, new Point3d(7, 8, 9)); first.RemoveUVNodes();
        Assert.Equal(0, second.Node(1).X); Assert.True(second.HasUVNodes());
        snapshot.Dispose(); first.Dispose(); Assert.Equal(0, second.Node(1).X);
        Assert.Equal(0, snapshot.Mesh.Positions[0].X); Assert.Throws<ObjectDisposedException>(() => snapshot.CopyTriangulation());
    }
    [Fact] public void CoherentPolyNeighborsMatchCompleteIndexedIncidence()
    {
        using var snapshot = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        Assert.Equal(PolyConnectivityDisposition.Available, snapshot.ConnectivityDisposition);
        Assert.NotNull(snapshot.Neighbors); Assert.NotNull(snapshot.IncidentTriangles);
        var first = snapshot.Neighbors[0]; Assert.Equal(-1, first.FirstTriangle); Assert.Equal(-1, first.SecondTriangle); Assert.Equal(1, first.ThirdTriangle);
        Assert.Equal(3, first.ThirdOppositeNode);
        Assert.Equal(2, snapshot.IncidentTriangles[0].Count); Assert.Single(snapshot.IncidentTriangles[1]);
        MeshConnectivity full = MeshConnectivity.Create(snapshot.Mesh); Assert.Equal(full.IncidentTriangles[0].Order(), snapshot.IncidentTriangles[0].Order());
        snapshot.Dispose(); Assert.Equal(2, snapshot.IncidentTriangles[0].Count);
        Assert.Throws<NotSupportedException>(() => ((IList<PolyTriangleNeighbors>)snapshot.Neighbors)[0] = default);
    }
    [Fact] public void UnusedVertexHasEmptyNativeIncidentList()
    {
        AuthoredMesh mesh = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(9, 9, 9)], [new(0, 1, 2)]);
        using var snapshot = PolyDataWorkspaces.FromMesh(mesh);
        Assert.Equal(PolyConnectivityDisposition.Available, snapshot.ConnectivityDisposition); Assert.Empty(snapshot.IncidentTriangles![3]);
    }
    [Fact] public void DegenerateAndNonManifoldMeshesKeepDataAndRejectPolyProjection()
    {
        AuthoredMesh degenerate = new([new(0, 0, 0), new(1, 0, 0)], [new(0, 1, 0)]);
        using var d = PolyDataWorkspaces.FromMesh(degenerate); Assert.Equal(PolyConnectivityDisposition.Degenerate, d.ConnectivityDisposition);
        Assert.Single(d.Mesh.Triangles); Assert.Null(d.Neighbors);
        AuthoredMesh mesh = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, -1, 0), new(0, 0, 1)], [new(0, 1, 2), new(1, 0, 3), new(0, 1, 4)]);
        using var snapshot = PolyDataWorkspaces.FromMesh(mesh); Assert.Equal(PolyConnectivityDisposition.NonManifold, snapshot.ConnectivityDisposition);
        Assert.Null(snapshot.IncidentTriangles); Assert.Equal(3, snapshot.Mesh.Triangles.Count);
        Assert.Single(MeshConnectivity.Create(snapshot.Mesh).Edges, e => e.Uses.Count == 3);
    }
    [Fact] public void InconsistentOrientationAndDisconnectedVertexFansAreExplicit()
    {
        AuthoredMesh reversed = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, -1, 0)], [new(0, 1, 2), new(0, 1, 3)]);
        using var bad = PolyDataWorkspaces.FromMesh(reversed); Assert.Equal(PolyConnectivityDisposition.InconsistentOrientation, bad.ConnectivityDisposition);
        AuthoredMesh fan = new([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(-1, 0, 0), new(0, -1, 0)], [new(0, 1, 2), new(0, 3, 4)]);
        using var snapshot = PolyDataWorkspaces.FromMesh(fan); Assert.Equal(PolyConnectivityDisposition.IncompleteVertexFan, snapshot.ConnectivityDisposition);
        Assert.Null(snapshot.Neighbors); Assert.Equal(2, MeshConnectivity.Create(snapshot.Mesh).IncidentTriangles[0].Count);
    }
    [Fact] public void DisposedAndNullNativeSourcesAreRejected()
    {
        using var data = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        using var raw = data.CopyTriangulation(); raw.Dispose();
        Assert.Throws<ObjectDisposedException>(() => PolyDataWorkspaces.Copy(raw));
        Assert.Throws<ArgumentNullException>(() => PolyDataWorkspaces.Copy(null!));
    }
    [Fact] public void EffectiveParameterNormalizationIsSeparateFromRequestedValues()
    {
        using var box = ShapeFactory.CreateBox(1, 2, 3); var requested = new ControlledMeshParameters(Angle: .25, Deflection: .02);
        using var mesh = ControlledMeshing.Run(box, requested);
        Assert.Equal(requested, mesh.RequestedParameters); Assert.Equal(-1, requested.MinimumSize);
        Assert.Equal(.5, mesh.EffectiveParameters.InteriorAngle); Assert.Equal(.02, mesh.EffectiveParameters.InteriorDeflection);
        Assert.Equal(.002, mesh.EffectiveParameters.MinimumSize, 8); Assert.True(mesh.IsDone); Assert.True(mesh.HasCompleteTriangulation);
    }
    [Fact] public void ExplicitMeshingOptionsAndAlternateAlgorithmSurviveProjection()
    {
        using var box = ShapeFactory.CreateBox(1, 2, 3);
        var requested = new ControlledMeshParameters(Angle: .3, Deflection: .1, InteriorAngle: .4, InteriorDeflection: .2, MinimumSize: .01,
            Algorithm: IMeshToolsMeshAlgoType.IMeshTools_MeshAlgoType_Delabella, Parallel: true, InternalVertices: false, ControlAllSurfaces: true, AdjustMinimumSize: true);
        using var mesh = ControlledMeshing.Run(box, requested); Assert.Equal(requested, mesh.EffectiveParameters);
        Assert.True(mesh.HasCompleteTriangulation); Assert.Empty(mesh.MissingFaces);
    }
    [Fact] public void PointOrWireOnlyMeshingIsNotCompleteFaceTessellation()
    {
        using var edge = ShapeFactory.CreateEdge(new(0, 0, 0), new(1, 0, 0));
        using var mesh = ControlledMeshing.Run(edge); Assert.Equal(0, mesh.FaceCount); Assert.Empty(mesh.MissingFaces);
        Assert.False(mesh.HasCompleteTriangulation); Assert.Throws<InvalidOperationException>(() => mesh.CopyShape());
    }
    [Fact] public void SelfIntersectingFaceRetainsActualDoneErrorAndMissingFace()
    {
        using var wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 2, 0), new(0, 2, 0), new(2, 0, 0)], true);
        using var face = ShapeFactory.CreatePlanarFace(wire); using var mesh = ControlledMeshing.Run(face);
        Assert.True(mesh.IsDone); Assert.False(mesh.HasCompleteTriangulation); Assert.Equal(1, mesh.FaceCount);
        Assert.Equal(0, Assert.Single(mesh.MissingFaces));
        Assert.NotEqual(0, (int)mesh.StatusFlags & (int)IMeshDataStatus.IMeshData_SelfIntersectingWire);
        Assert.Throws<InvalidOperationException>(() => mesh.CopyShape());
    }
    [Fact] public void OpenWireErrorIsIncompleteEvenWhenTriangulationExists()
    {
        using var wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], false);
        using var face = ShapeFactory.CreatePlanarFace(wire); using var mesh = ControlledMeshing.Run(face);
        Assert.True(mesh.IsDone); Assert.Empty(mesh.MissingFaces); Assert.False(mesh.HasCompleteTriangulation);
        Assert.NotEqual(0, (int)mesh.StatusFlags & (int)IMeshDataStatus.IMeshData_OpenWire);
        Assert.Throws<InvalidOperationException>(() => mesh.CopyShape());
    }
    [Fact] public void UnmeshedSourceRemainsUnmeshedAfterControlledRun()
    {
        using var source = ShapeFactory.CreateBox(1, 2, 3);
        Assert.Throws<ArgumentException>(() => MeshTopology.SnapshotExisting(source));
        using var mesh = ControlledMeshing.Run(source); Assert.True(mesh.HasCompleteTriangulation);
        Assert.Throws<ArgumentException>(() => MeshTopology.SnapshotExisting(source));
        using var shape = mesh.CopyShape(); Assert.NotEmpty(MeshTopology.SnapshotExisting(shape).Mesh.Triangles);
    }
    [Fact] public void MeshingValidatesParametersAndRejectsDiscreteNullDisposedShapes()
    {
        using var box = ShapeFactory.CreateBox(1, 1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => ControlledMeshing.Run(box, new(Deflection: double.NaN)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ControlledMeshing.Run(box, new(InteriorAngle: 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ControlledMeshing.Run(box, new(Algorithm: (IMeshToolsMeshAlgoType)2)));
        Assert.Throws<OcctException>(() => ControlledMeshing.Run(box, new(Deflection: 1e-300)));
        using var nullShape = ShapeFactory.CreateNull(); Assert.Throws<ArgumentException>(() => ControlledMeshing.Run(nullShape));
        using var discrete = MeshTopology.CreateFace(BatchARMeshWorkflow.Square()); Assert.Throws<ArgumentException>(() => ControlledMeshing.Run(discrete));
        box.Dispose(); Assert.Throws<ObjectDisposedException>(() => ControlledMeshing.Run(box));
    }
    [Fact] public void MeshingShapeCopiesSurviveIndependentMutationAndSourceDisposal()
    {
        using var source = ShapeFactory.CreateBox(1, 2, 3); using var result = ControlledMeshing.Run(source);
        using var first = result.CopyShape(); using var second = result.CopyShape(); source.Dispose(); result.Dispose();
        AuthoredMesh before = MeshTopology.SnapshotExisting(second).Mesh;
        using var replacement = MeshTopology.ReplaceTriangulation(first, 0, BatchARMeshWorkflow.Square()); first.Dispose();
        Assert.Equal(before.Triangles.Count, MeshTopology.SnapshotExisting(second).Mesh.Triangles.Count);
        Assert.True(second.IsValid); Assert.True(result.HasCompleteTriangulation); Assert.Throws<ObjectDisposedException>(() => result.CopyShape());
    }
    [Fact] public void ConcurrentPolyAndMeshOwnerQueriesWithReleaseRemainSafe()
    {
        using var source = ShapeFactory.CreateBox(1, 1, 1); using var mesh = ControlledMeshing.Run(source);
        using var data = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        var errors = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
        Parallel.For(0, 80, i =>
        {
            try { if (i == 30) { mesh.Dispose(); data.Dispose(); } else { using var p = data.CopyTriangulation(); using var s = mesh.CopyShape(); Assert.Equal(4, p.NbNodes()); Assert.True(s.IsValid); } }
            catch (ObjectDisposedException) { }
            catch (Exception error) { errors.Enqueue(error); }
        });
        Assert.Empty(errors); Assert.Equal(4, data.Mesh.Positions.Count);
    }
    static nint Token(object value) => ((SafeHandle)value.GetType().GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(value)!).DangerousGetHandle();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawInfo(nint token, int* info, int capacity, double* values, int valueCapacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawIncident(nint token, int node, int* values, int capacity, out int count);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawCreate(double* points, int nodes, int* indices, int triangles, double* uv, double* normals, int useDouble, out nint token);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] unsafe delegate int RawMeshInfo(nint token, int* info, int count, double* parameters, int parameterCount, int* flags, int flagCount);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int RawMeshShape(nint token, out nint result);
    [Fact] public unsafe void RawMeshingBuffersAndFailedShapeQueriesPublishNothing()
    {
        using var box = ShapeFactory.CreateBox(1, 1, 1); using var mesh = ControlledMeshing.Run(box);
        using var wire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 2, 0), new(0, 2, 0), new(2, 0, 0)], true);
        using var face = ShapeFactory.CreatePlanarFace(wire); using var failed = ControlledMeshing.Run(face);
        using var wrong = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square());
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var info = Marshal.GetDelegateForFunctionPointer<RawMeshInfo>(NativeLibrary.GetExport(library, "occtsharp_meshing_info"));
            var shape = Marshal.GetDelegateForFunctionPointer<RawMeshShape>(NativeLibrary.GetExport(library, "occtsharp_meshing_shape"));
            int* values = stackalloc int[6]; int* flags = stackalloc int[10]; double* parameters = stackalloc double[5];
            values[0] = 123; flags[9] = 456; parameters[4] = 789;
            Assert.NotEqual(0, info(Token(mesh), values, 6, parameters, 4, flags, 10)); Assert.Equal(123, values[0]); Assert.Equal(456, flags[9]); Assert.Equal(789, parameters[4]);
            Assert.NotEqual(0, info(Token(wrong), values, 6, parameters, 5, flags, 10)); Assert.Equal(123, values[0]);
            Assert.NotEqual(0, shape(Token(failed), out nint result)); Assert.Equal(0, result);
            nint token = Token(mesh); mesh.Dispose(); Assert.NotEqual(0, info(token, values, 6, parameters, 5, flags, 10)); Assert.Equal(789, parameters[4]);
        }
        finally { NativeLibrary.Free(library); }
    }
    [Fact] public unsafe void RawPolyCapacityWrongTypedAndStaleTokensNeverWriteArrays()
    {
        using var data = PolyDataWorkspaces.FromMesh(BatchARMeshWorkflow.Square()); using var wrong = ShapeFactory.CreateBox(1, 1, 1);
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var info = Marshal.GetDelegateForFunctionPointer<RawInfo>(NativeLibrary.GetExport(library, "occtsharp_polydata_info"));
            var incident = Marshal.GetDelegateForFunctionPointer<RawIncident>(NativeLibrary.GetExport(library, "occtsharp_polydata_incident"));
            int* flags = stackalloc int[8]; double* values = stackalloc double[7]; flags[0] = 123; values[6] = 456;
            Assert.NotEqual(0, info(Token(data), flags, 7, values, 7)); Assert.Equal(123, flags[0]); Assert.Equal(456, values[6]);
            Assert.NotEqual(0, info(Token(wrong), flags, 8, values, 7)); Assert.Equal(123, flags[0]);
            Assert.NotEqual(0, incident(Token(data), -1, flags, 8, out int count)); Assert.Equal(0, count); Assert.Equal(123, flags[0]);
            Assert.NotEqual(0, incident(Token(data), 0, flags, 1, out count)); Assert.Equal(123, flags[0]);
            nint token = Token(data); data.Dispose(); Assert.NotEqual(0, info(token, flags, 8, values, 7)); Assert.Equal(456, values[6]);
        }
        finally { NativeLibrary.Free(library); }
    }
    [Fact] public unsafe void RawCreationBadCountsIndicesAndPrecisionPublishNoOwner()
    {
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            var create = Marshal.GetDelegateForFunctionPointer<RawCreate>(NativeLibrary.GetExport(library, "occtsharp_polydata_create"));
            double* points = stackalloc double[3] { 0, 0, 0 }; int* indices = stackalloc int[3] { 0, 0, 2 };
            Assert.NotEqual(0, create(points, -1, null, 0, null, null, 1, out nint owner)); Assert.Equal(0, owner);
            Assert.NotEqual(0, create(points, 1, indices, 1, null, null, 1, out owner)); Assert.Equal(0, owner);
            Assert.NotEqual(0, create(points, 1, null, 0, null, null, 2, out owner)); Assert.Equal(0, owner);
        }
        finally { NativeLibrary.Free(library); }
    }
}
