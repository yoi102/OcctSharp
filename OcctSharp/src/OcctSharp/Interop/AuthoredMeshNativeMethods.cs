using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_author_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshAuthorFace(AuthoredVertexRaw* vertices, int vertexCount,
        AuthoredTriangleRaw* triangles, int triangleCount, out nint output);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_existing_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshExistingSnapshot(ShapeHandle shape, AuthoredVertexRaw* vertices, int vertexCapacity,
        out int vertexCount, AuthoredTriangleRaw* triangles, int triangleCapacity, out int triangleCount, out int faceCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_replace_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshReplaceFace(ShapeHandle shape, int faceIndex, AuthoredVertexRaw* vertices,
        int vertexCount, AuthoredTriangleRaw* triangles, int triangleCount, out nint output);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_remesh_faces")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshRemeshFaces(ShapeHandle shape, int* faces, int faceCount,
        double linear, double angular, out nint output);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_is_exact")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MeshIsExact(ShapeHandle shape, out int exact);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_copy_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MeshCopyShape(ShapeHandle source, out nint output);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshTransformVertices(AuthoredVertexRaw* vertices, int count, double* matrix,
        int matrixCount, AuthoredVertexRaw* output, int capacity, out double determinant);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_weld_nodes")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshWeldNodes(AuthoredVertexRaw* vertices, int count, int* partitions,
        int partitionCount, double tolerance, int* representatives, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_coherent_patch")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshCoherentPatch(AuthoredVertexRaw* vertices, int vertexCount,
        AuthoredTriangleRaw* triangles, int triangleCount, int* replaced, AuthoredTriangleRaw* replacements, int replacementCount,
        AuthoredTriangleRaw* appended, int appendedCount, AuthoredTriangleRaw* output, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_remove_degenerate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshRemoveDegenerate(AuthoredVertexRaw* vertices, int vertexCount,
        AuthoredTriangleRaw* triangles, int triangleCount, double minimumArea, double minimumLength,
        int* removed, int capacity, out int removedCount);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_poly_connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshPolyConnect(AuthoredVertexRaw* vertices, int vertexCount,
        AuthoredTriangleRaw* triangles, int triangleCount, int* neighbors, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_polyline")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshPolyline(AuthoredVertexRaw* vertices, int vertexCount, int* indices,
        int indexCount, double* parameters, int parameterCount, out nint output);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_read_editable", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MeshReadEditable(string path, int format, long maximumBytes, out nint output, out int disclosures);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_write_stl", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshWriteStl(AuthoredVertexRaw* vertices, int vertexCount,
        AuthoredTriangleRaw* triangles, int triangleCount, string path, int binary);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_convert_coordinates")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus MeshConvertCoordinates(AuthoredVertexRaw* vertices, int count,
        double sourceUnit, int sourceUp, int sourceLeft, double targetUnit, int targetUp, int targetLeft,
        AuthoredVertexRaw* output, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mesh_write_document", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MeshWriteDocument(OcafDocumentHandle document, string path, int format, int binary, int channels);
}
