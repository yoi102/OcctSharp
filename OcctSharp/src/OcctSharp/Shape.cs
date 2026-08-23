using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns a native OCCT topology shape.</summary>
public partial class Shape : IDisposable
{
    private readonly ShapeHandle handle;

    internal Shape(ShapeHandle handle)
    {
        this.handle = handle;
    }

    internal ShapeHandle Handle => handle;

    /// <summary>Gets the number of faces contained in this shape.</summary>
    public int FaceCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetFaceCount(handle, out int faceCount),
                "shape_get_face_count");
            return faceCount;
        }
    }

    /// <summary>Returns independent owned face copies in deterministic explorer order.</summary>
    public unsafe Shape[] GetFaces()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.GetFaceCount(handle, out int count), "shape_get_face_count");
        if (count == 0)
        {
            return [];
        }

        nint[] nativeFaces = new nint[count];
        fixed (nint* facePointer = nativeFaces)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetFaceSnapshot(handle, facePointer, nativeFaces.Length, out int written),
                "shape_face_snapshot");
            if (written != nativeFaces.Length)
            {
                for (int index = 0; index < written; ++index)
                {
                    NativeMethods.ReleaseShape(nativeFaces[index]);
                }

                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native face snapshot count changed during enumeration.");
            }
        }

        Shape[] result = new Shape[nativeFaces.Length];
        int created = 0;
        try
        {
            for (; created < nativeFaces.Length; ++created)
            {
                result[created] = ShapeFactory.FromNativeHandle(nativeFaces[created], "shape_face_snapshot");
            }

            return result;
        }
        catch
        {
            for (int index = created; index < nativeFaces.Length; ++index)
            {
                NativeMethods.ReleaseShape(nativeFaces[index]);
            }

            throw;
        }
    }

    /// <summary>Returns independent owned copies of a supported topological subshape kind.</summary>
    public unsafe Shape[] GetSubShapes(ShapeKind kind)
    {
        if (kind is < ShapeKind.Compound or > ShapeKind.Vertex)
            throw new ArgumentOutOfRangeException(nameof(kind), "Only Compound through Vertex can be explored.");
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.GetSubshapeCount(handle, (int)kind, out int expected), "shape_subshape_count");
        if (expected == 0) return [];
        nint[] nativeShapes = new nint[expected];
        fixed (nint* shapePointer = nativeShapes)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetSubshapeSnapshot(handle, (int)kind, shapePointer, nativeShapes.Length, out int written),
                "shape_subshape_snapshot");
            if (written != nativeShapes.Length)
                throw new OcctException(NativeStatus.UnknownException.ToString(), "The native subshape snapshot count changed during enumeration.");
        }
        Shape[] result = new Shape[nativeShapes.Length];
        int created = 0;
        try
        {
            for (; created < nativeShapes.Length; ++created)
                result[created] = ShapeFactory.FromNativeHandle(nativeShapes[created], "shape_subshape_snapshot");
            return result;
        }
        catch
        {
            for (int index = created; index < nativeShapes.Length; ++index) NativeMethods.ReleaseShape(nativeShapes[index]);
            throw;
        }
    }

    /// <summary>Copies the adapted curve type, parameter range, and endpoint values for an edge.</summary>
    public EdgeCurveSnapshot GetEdgeCurveSnapshot()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetEdgeCurveSnapshot(handle, out EdgeCurveSnapshotRaw snapshot),
            "shape_edge_curve_snapshot");
        return new EdgeCurveSnapshot(
            (CurveGeometryType)snapshot.CurveType,
            snapshot.FirstParameter,
            snapshot.LastParameter,
            new GpPoint(snapshot.StartPoint.X, snapshot.StartPoint.Y, snapshot.StartPoint.Z),
            new GpPoint(snapshot.EndPoint.X, snapshot.EndPoint.Y, snapshot.EndPoint.Z));
    }

    /// <summary>Copies the adapted surface type and UV bounds for a face.</summary>
    public FaceSurfaceSnapshot GetFaceSurfaceSnapshot(bool restrictToFace = true)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetFaceSurfaceSnapshot(handle, restrictToFace ? 1 : 0, out FaceSurfaceSnapshotRaw snapshot),
            "shape_face_surface_snapshot");
        return new FaceSurfaceSnapshot(
            (SurfaceGeometryType)snapshot.SurfaceType,
            snapshot.FirstUParameter,
            snapshot.LastUParameter,
            snapshot.FirstVParameter,
            snapshot.LastVParameter);
    }

    /// <summary>Creates an owned shape with the supplied rigid transform applied.</summary>
    public Shape Transformed(ShapeTransform transform)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();
        NativeStatus status = NativeMethods.TransformShape(
            handle,
            transform.TranslationX,
            transform.TranslationY,
            transform.TranslationZ,
            transform.RotationAxisX,
            transform.RotationAxisY,
            transform.RotationAxisZ,
            transform.RotationAngleRadians,
            out nint transformedShape);
        NativeError.ThrowIfFailed(status, "shape_transform");
        return ShapeFactory.FromNativeHandle(transformedShape, "shape_transform");
    }

    /// <summary>Creates an owned shape by applying an OCCT <c>gp_Trsf</c> value.</summary>
    public Shape Transformed(GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return transform.Apply(this);
    }

    /// <summary>Returns an independently owned shape with an absolute location.</summary>
    public Shape Located(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Locate(this);
    }

    /// <summary>Returns an independently owned shape moved by a location.</summary>
    public Shape Moved(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Move(this);
    }

    /// <summary>Computes an OCCT boolean union and returns an independently owned result.</summary>
    public Shape Fuse(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(NativeMethods.BooleanFuse(handle, other.handle, out nint result), "shape_boolean_fuse");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_fuse");
    }

    /// <summary>Subtracts <paramref name="tool"/> from this shape through OCCT BRepAlgoAPI.</summary>
    public Shape Cut(Shape tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(tool.handle.IsClosed, tool);
        NativeError.ThrowIfFailed(NativeMethods.BooleanCut(handle, tool.handle, out nint result), "shape_boolean_cut");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_cut");
    }

    /// <summary>Computes the OCCT Boolean intersection and returns an independently owned result.</summary>
    public Shape Common(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.BooleanCommon(handle, other.handle, out nint result),
            "shape_boolean_common");
        return ShapeFactory.FromNativeHandle(result, "shape_boolean_common");
    }

    /// <summary>Copies the minimum distance and one corresponding point pair from OCCT.</summary>
    public ShapeDistanceResult DistanceTo(Shape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        ObjectDisposedException.ThrowIf(other.handle.IsClosed, other);
        NativeError.ThrowIfFailed(
            NativeMethods.GetShapeDistance(handle, other.handle, out ShapeDistanceResultRaw result),
            "shape_distance");
        return new ShapeDistanceResult(
            result.Distance,
            new GpPoint(result.PointOnFirst.X, result.PointOnFirst.Y, result.PointOnFirst.Z),
            new GpPoint(result.PointOnSecond.X, result.PointOnSecond.Y, result.PointOnSecond.Z),
            result.SolutionCount);
    }

    /// <summary>Runs OCCT's safe ShapeFix_Shape pass and returns an owned result.</summary>
    public Shape Fixed()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(NativeMethods.FixShape(handle, out nint result), "shape_fix");
        return ShapeFactory.FromNativeHandle(result, "shape_fix");
    }

    /// <summary>Unifies compatible neighbouring faces and edges into an owned result.</summary>
    public Shape UnifiedSameDomain()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.UnifySameDomain(handle, out nint result),
            "shape_unify_same_domain");
        return ShapeFactory.FromNativeHandle(result, "shape_unify_same_domain");
    }

    /// <summary>Builds an owned managed copy of the shape's triangle mesh.</summary>
    public unsafe MeshSnapshot CreateMesh(double linearDeflection = 0.1, double angularDeflection = 0.5)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.GetMeshCount(handle, linearDeflection, angularDeflection, out int vertexCount, out int indexCount),
            "shape_mesh_count");
        MeshVertexRaw[] nativeVertices = new MeshVertexRaw[vertexCount];
        int[] indices = new int[indexCount];
        fixed (MeshVertexRaw* vertexPointer = nativeVertices)
        fixed (int* indexPointer = indices)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetMeshSnapshot(
                    handle,
                    linearDeflection,
                    angularDeflection,
                    vertexPointer,
                    nativeVertices.Length,
                    out int writtenVertices,
                    indexPointer,
                    indices.Length,
                    out int writtenIndices),
                "shape_mesh_snapshot");
            if (writtenVertices != nativeVertices.Length || writtenIndices != indices.Length)
            {
                throw new OcctException(
                    NativeStatus.UnknownException.ToString(),
                    "The native mesh snapshot count changed during extraction.");
            }
        }

        MeshVertex[] vertices = new MeshVertex[nativeVertices.Length];
        for (int index = 0; index < vertices.Length; ++index)
        {
            MeshVertexRaw value = nativeVertices[index];
            vertices[index] = new MeshVertex(value.X, value.Y, value.Z, value.NormalX, value.NormalY, value.NormalZ);
        }

        return new MeshSnapshot(vertices, indices);
    }

    /// <summary>Releases the owned native shape.</summary>
    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }
}
