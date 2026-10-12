using OcctSharp.MeshingWorkspaces;
using OcctSharp.PolyWorkspaces;

namespace OcctSharp.Tests.Shared;

internal static class BatchARMeshWorkflow
{
    internal static AuthoredMesh Square() => new(
        [new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)],
        [new(0, 1, 2), new(0, 2, 3)], Enumerable.Repeat(new MeshNormal(0, 0, 1), 4),
        [new(0, 0), new(1, 0), new(1, 1), new(0, 1)]);

    internal static void Run()
    {
        if (typeof(PolyDataWorkspaces).Assembly.GetName().Name != "OcctSharp.MeshData"
            || typeof(PolyDataWorkspaces).Assembly.GetReferencedAssemblies().Any(a => a.Name is "OcctSharp.Modeling" or "OcctSharp.Mesh" or "OcctSharp"))
            throw new InvalidOperationException("AR Poly copy introduced an invalid MeshData dependency.");
        using var source = ShapeFactory.CreateBox(2, 3, 4);
        _ = source.CreateDetailedMesh(.2, .5);
        int before = MeshTopology.SnapshotExisting(source).Mesh.Triangles.Count;
        using var meshing = ControlledMeshing.Run(source, new(Deflection: .05));
        if (!meshing.IsDone || !meshing.HasCompleteTriangulation || meshing.FaceCount != 6
            || meshing.MissingFaces.Count != 0 || meshing.EffectiveParameters.MinimumSize <= 0
            || meshing.EffectiveParameters.InteriorDeflection != .05 || meshing.EffectiveParameters.InteriorAngle != 1)
            throw new InvalidOperationException("AR native mesh completion/effective parameter normalization failed.");
        if (MeshTopology.SnapshotExisting(source).Mesh.Triangles.Count != before)
            throw new InvalidOperationException("AR mesher changed source triangulations.");
        using var shape = meshing.CopyShape(); meshing.Dispose(); source.Dispose();
        AuthoredMesh existing = MeshTopology.SnapshotExisting(shape).Mesh;
        if (existing.Triangles.Count == 0 || existing.UVs is null) throw new InvalidOperationException("AR exact mesh channels were lost.");
        using var snapshot = PolyDataWorkspaces.FromMesh(existing);
        using var native = snapshot.CopyTriangulation(); snapshot.Dispose();
        using var copied = PolyDataWorkspaces.Copy(native); native.Dispose();
        if (copied.Mesh.Triangles.Count != existing.Triangles.Count || copied.Mesh.UVs is null || copied.Bounds is null)
            throw new InvalidOperationException("AR native copied data failed detachment.");
        var edited = MeshEditing.Extract(copied.Mesh, copied.Mesh.SelectTriangles([0]));
        copied.Dispose();
        using var discrete = MeshTopology.Create(edited.Mesh);
        using var discreteShape = discrete.CopyShape(); discrete.Dispose();
        if (MeshTopology.IsSurfaceBacked(discreteShape) || MeshTopology.SnapshotExisting(discreteShape).Mesh.Triangles.Count != 1)
            throw new InvalidOperationException("AR existing discrete topology composition failed.");
        using var square = PolyDataWorkspaces.FromMesh(Square(), doublePrecision: false);
        if (square.IsDoublePrecision || square.ConnectivityDisposition != PolyConnectivityDisposition.Available
            || square.Neighbors is null || square.IncidentTriangles is null || square.IncidentTriangles[0].Count != 2)
            throw new InvalidOperationException("AR coherent Poly adjacency failed.");
        MeshConnectivity connectivity = MeshConnectivity.Create(square.Mesh);
        if (connectivity.Edges.Count(e => e.IsBoundary) != 4) throw new InvalidOperationException("AR copied adjacency disagrees with full incidence.");
        using var openWire = ShapeFactory.CreatePolygonWire([new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0)], false);
        using var openFace = ShapeFactory.CreatePlanarFace(openWire);
        using var incomplete = ControlledMeshing.Run(openFace);
        if (!incomplete.IsDone || incomplete.HasCompleteTriangulation || incomplete.MissingFaces.Count != 0
            || ((int)incomplete.StatusFlags & (int)IMeshDataStatus.IMeshData_OpenWire) == 0)
            throw new InvalidOperationException("AR done-flag/existing triangles masked an open-wire error.");
        bool rejected = false;
        try { using var invalid = incomplete.CopyShape(); }
        catch (InvalidOperationException) { rejected = true; }
        if (!rejected) throw new InvalidOperationException("AR incomplete mesh published topology.");
#if AR_DIRECT_MODULE
        // These geometry exchange APIs belong to DataExchange. STL readback is
        // faceted geometry and therefore requires explicit triangulation.
        foreach (string extension in new[] { ".brep", ".stl" })
        {
            string path = Path.Combine(Path.GetTempPath(), "OcctSharp.AR." + Guid.NewGuid() + extension);
            try
            {
                if (extension == ".brep") ShapeExchange.WriteBrep(discreteShape, path);
                else ShapeExchange.WriteStl(discreteShape, path);
                using var restored = extension == ".brep" ? ShapeExchange.ReadBrep(path)
                    : ShapeExchange.ReadStl(path);
                if (extension == ".stl") _ = restored.CreateDetailedMesh(.05, .5);
                AuthoredMesh readback = MeshTopology.SnapshotExisting(restored).Mesh;
                if (readback.Triangles.Count != 1 || readback.Positions.Count != 3
                    || edited.Mesh.Positions.Any(p => !readback.Positions.Any(q =>
                        Math.Abs(p.X - q.X) < 1e-5 && Math.Abs(p.Y - q.Y) < 1e-5 && Math.Abs(p.Z - q.Z) < 1e-5)))
                    throw new InvalidOperationException("AR direct-module mesh format coordinates/readback failed.");
            }
            finally { File.Delete(path); File.Delete(Path.ChangeExtension(path, ".mtl")); }
        }
#else
        foreach (string extension in new[] { ".obj", ".stl" })
        {
            string path = Path.Combine(Path.GetTempPath(), "OcctSharp.AR." + Guid.NewGuid() + extension);
            AuthoredMeshExportResult? exported = null;
            try
            {
                exported = AuthoredMeshExchange.Write(edited.Mesh, path);
                AuthoredMesh restored = AuthoredMeshExchange.Read(path, new(exported.Coordinates)).Mesh;
                if (restored.Triangles.Count != 1) throw new InvalidOperationException("AR real mesh format readback failed.");
            }
            finally { if (exported is not null) foreach (string asset in exported.Assets) File.Delete(asset); else File.Delete(path); }
        }
#endif
    }
}
