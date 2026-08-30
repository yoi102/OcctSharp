using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Builds copied advanced meshes, diagnostics, and ordered LOD sets.</summary>
public static class AdvancedMesh
{
    /// <summary>Creates a fully copied grouped triangulation, statistics, and diagnostics for an owning shape.</summary>
    /// <param name="shape">The source shape. The returned snapshot does not retain it.</param>
    /// <param name="options">Optional meshing and diagnostic tolerances.</param>
    /// <returns>A document- and shape-independent mesh snapshot.</returns>
    public static unsafe AdvancedMeshSnapshot Create(Shape shape, AdvancedMeshOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
        AdvancedMeshOptions effective = options ?? new AdvancedMeshOptions();
        effective.Validate();
        OcctRuntime.EnsureCompatible();

        NativeError.ThrowIfFailed(
            AdvancedMeshNativeMethods.GetCount(
                shape.Handle, effective.LinearDeflection, effective.AngularDeflection,
                effective.MinimumSize, Flag(effective.Relative), Flag(effective.Parallel),
                Flag(effective.InternalVertices), Flag(effective.ControlSurfaceDeflection),
                out int vertexCount, out int triangleCount, out int faceCount),
            "shape_advanced_mesh_count");

        DetailedMeshVertexRaw[] rawVertices = new DetailedMeshVertexRaw[vertexCount];
        DetailedMeshTriangleRaw[] rawTriangles = new DetailedMeshTriangleRaw[triangleCount];
        fixed (DetailedMeshVertexRaw* vertexPointer = rawVertices)
        fixed (DetailedMeshTriangleRaw* trianglePointer = rawTriangles)
        {
            NativeError.ThrowIfFailed(
                AdvancedMeshNativeMethods.Copy(
                    shape.Handle, effective.LinearDeflection, effective.AngularDeflection,
                    effective.MinimumSize, Flag(effective.Relative), Flag(effective.Parallel),
                    Flag(effective.InternalVertices), Flag(effective.ControlSurfaceDeflection),
                    vertexPointer, rawVertices.Length, out int writtenVertices,
                    trianglePointer, rawTriangles.Length, out int writtenTriangles,
                    out int writtenFaces),
                "shape_advanced_mesh_snapshot");
            if (writtenVertices != vertexCount || writtenTriangles != triangleCount || writtenFaces != faceCount)
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The advanced-mesh count changed during bounded extraction.");
        }

        DetailedMeshVertex[] vertices = rawVertices.Select(static value => new DetailedMeshVertex(
            value.X, value.Y, value.Z,
            value.NormalX, value.NormalY, value.NormalZ,
            value.U, value.V, value.HasUv != 0)).ToArray();
        DetailedMeshTriangle[] triangles = rawTriangles.Select(static value => new DetailedMeshTriangle(
            value.VertexA, value.VertexB, value.VertexC,
            value.FaceIndex, value.IsReversed != 0)).ToArray();
        return Analyze(vertices, triangles, faceCount, effective.WeldTolerance);
    }

    /// <summary>Creates ordered fine-to-coarse independent mesh snapshots.</summary>
    /// <param name="shape">The source shape. The returned levels do not retain it.</param>
    /// <param name="linearDeflections">Strictly increasing positive deflections from fine to coarse.</param>
    /// <param name="options">Options shared by every level except linear deflection.</param>
    /// <returns>An ordered set of copied LOD meshes.</returns>
    public static AdvancedMeshLodSet CreateLods(
        Shape shape,
        IEnumerable<double> linearDeflections,
        AdvancedMeshOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(linearDeflections);
        double[] deflections = linearDeflections.ToArray();
        if (deflections.Length == 0) throw new ArgumentException("At least one LOD deflection is required.", nameof(linearDeflections));
        for (int index = 0; index < deflections.Length; ++index)
        {
            if (!double.IsFinite(deflections[index]) || deflections[index] <= 0)
                throw new ArgumentOutOfRangeException(nameof(linearDeflections));
            if (index > 0 && deflections[index] <= deflections[index - 1])
                throw new ArgumentException("LOD deflections must be strictly increasing from fine to coarse.", nameof(linearDeflections));
        }

        AdvancedMeshOptions baseline = options ?? new AdvancedMeshOptions();
        AdvancedMeshLod[] levels = new AdvancedMeshLod[deflections.Length];
        for (int index = 0; index < deflections.Length; ++index)
        {
            AdvancedMeshOptions levelOptions = baseline with { LinearDeflection = deflections[index] };
            levels[index] = new AdvancedMeshLod(index, deflections[index], Create(shape, levelOptions));
        }
        return new AdvancedMeshLodSet(levels);
    }

    private static AdvancedMeshSnapshot Analyze(
        DetailedMeshVertex[] vertices,
        DetailedMeshTriangle[] triangles,
        int faceCount,
        double weldTolerance)
    {
        Dictionary<VertexKey, int> welded = [];
        int[] weldedIndices = new int[vertices.Length];
        for (int index = 0; index < vertices.Length; ++index)
        {
            DetailedMeshVertex vertex = vertices[index];
            VertexKey key = new(Quantize(vertex.X), Quantize(vertex.Y), Quantize(vertex.Z));
            if (!welded.TryGetValue(key, out int weldedIndex))
            {
                weldedIndex = welded.Count;
                welded.Add(key, weldedIndex);
            }
            weldedIndices[index] = weldedIndex;
        }

        double minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0;
        if (vertices.Length > 0)
        {
            minX = maxX = vertices[0].X;
            minY = maxY = vertices[0].Y;
            minZ = maxZ = vertices[0].Z;
            foreach (DetailedMeshVertex vertex in vertices)
            {
                minX = Math.Min(minX, vertex.X); minY = Math.Min(minY, vertex.Y); minZ = Math.Min(minZ, vertex.Z);
                maxX = Math.Max(maxX, vertex.X); maxY = Math.Max(maxY, vertex.Y); maxZ = Math.Max(maxZ, vertex.Z);
            }
        }

        Dictionary<EdgeKey, EdgeUse> edges = [];
        DisjointSet components = new(triangles.Length);
        double area = 0;
        int degenerate = 0;
        double areaTolerance = weldTolerance * weldTolerance;
        for (int triangleIndex = 0; triangleIndex < triangles.Length; ++triangleIndex)
        {
            DetailedMeshTriangle triangle = triangles[triangleIndex];
            DetailedMeshVertex a = vertices[triangle.VertexA];
            DetailedMeshVertex b = vertices[triangle.VertexB];
            DetailedMeshVertex c = vertices[triangle.VertexC];
            double abX = b.X - a.X, abY = b.Y - a.Y, abZ = b.Z - a.Z;
            double acX = c.X - a.X, acY = c.Y - a.Y, acZ = c.Z - a.Z;
            double crossX = abY * acZ - abZ * acY;
            double crossY = abZ * acX - abX * acZ;
            double crossZ = abX * acY - abY * acX;
            double triangleArea = 0.5 * Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);
            area += triangleArea;
            if (triangleArea <= areaTolerance) degenerate++;

            AddEdge(weldedIndices[triangle.VertexA], weldedIndices[triangle.VertexB], triangleIndex);
            AddEdge(weldedIndices[triangle.VertexB], weldedIndices[triangle.VertexC], triangleIndex);
            AddEdge(weldedIndices[triangle.VertexC], weldedIndices[triangle.VertexA], triangleIndex);
        }

        int boundary = 0, manifold = 0, nonManifold = 0;
        foreach (EdgeUse use in edges.Values)
        {
            if (use.Count == 1) boundary++;
            else if (use.Count == 2) manifold++;
            else nonManifold++;
        }
        int componentCount = triangles.Length == 0
            ? 0
            : Enumerable.Range(0, triangles.Length).Select(components.Find).Distinct().Count();

        AdvancedMeshPrimitiveGroup[] groups = new AdvancedMeshPrimitiveGroup[faceCount];
        int cursor = 0;
        for (int face = 0; face < faceCount; ++face)
        {
            int first = cursor;
            while (cursor < triangles.Length && triangles[cursor].FaceIndex == face) cursor++;
            groups[face] = new AdvancedMeshPrimitiveGroup(face, first, cursor - first, null, null);
        }

        BoundingBox3d bounds = new(new GpPoint(minX, minY, minZ), new GpPoint(maxX, maxY, maxZ));
        long estimatedBytes = (long)vertices.Length * (sizeof(double) * 8 + sizeof(byte))
            + (long)triangles.Length * sizeof(int) * 5;
        AdvancedMeshStatistics statistics = new(
            bounds, vertices.Length, welded.Count, triangles.Length, groups.Length, area, estimatedBytes);
        AdvancedMeshDiagnostics diagnostics = new(degenerate, boundary, manifold, nonManifold, componentCount);
        return new AdvancedMeshSnapshot(vertices, triangles, groups, statistics, diagnostics);

        long Quantize(double value)
        {
            double scaled = Math.Round(value / weldTolerance, MidpointRounding.AwayFromZero);
            return scaled >= long.MaxValue ? long.MaxValue
                : scaled <= long.MinValue ? long.MinValue
                : (long)scaled;
        }

        void AddEdge(int first, int second, int triangleIndex)
        {
            EdgeKey key = first <= second ? new(first, second) : new(second, first);
            if (edges.TryGetValue(key, out EdgeUse use))
            {
                components.Union(use.FirstTriangle, triangleIndex);
                edges[key] = use with { Count = use.Count + 1 };
            }
            else edges.Add(key, new EdgeUse(triangleIndex, 1));
        }
    }

    private static int Flag(bool value) => value ? 1 : 0;
    private readonly record struct VertexKey(long X, long Y, long Z);
    private readonly record struct EdgeKey(int A, int B);
    private readonly record struct EdgeUse(int FirstTriangle, int Count);

    private sealed class DisjointSet
    {
        private readonly int[] parent;
        private readonly byte[] rank;
        internal DisjointSet(int count)
        {
            parent = Enumerable.Range(0, count).ToArray();
            rank = new byte[count];
        }
        internal int Find(int value)
        {
            while (parent[value] != value)
            {
                parent[value] = parent[parent[value]];
                value = parent[value];
            }
            return value;
        }
        internal void Union(int left, int right)
        {
            int leftRoot = Find(left), rightRoot = Find(right);
            if (leftRoot == rightRoot) return;
            if (rank[leftRoot] < rank[rightRoot]) parent[leftRoot] = rightRoot;
            else if (rank[leftRoot] > rank[rightRoot]) parent[rightRoot] = leftRoot;
            else { parent[rightRoot] = leftRoot; rank[leftRoot]++; }
        }
    }
}
