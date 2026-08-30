using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class AdvancedMeshNativeMethods
{
    private const string LibraryName = "OcctSharp.Native";

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_advanced_mesh_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetCount(
        ShapeHandle shape,
        double linearDeflection,
        double angularDeflection,
        double minimumSize,
        int relative,
        int parallel,
        int internalVertices,
        int controlSurfaceDeflection,
        out int vertexCount,
        out int triangleCount,
        out int faceCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_advanced_mesh_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus Copy(
        ShapeHandle shape,
        double linearDeflection,
        double angularDeflection,
        double minimumSize,
        int relative,
        int parallel,
        int internalVertices,
        int controlSurfaceDeflection,
        DetailedMeshVertexRaw* vertices,
        int vertexCapacity,
        out int vertexCount,
        DetailedMeshTriangleRaw* triangles,
        int triangleCapacity,
        out int triangleCount,
        out int faceCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_read_gltf", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadGltf(string filePath, out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_read_obj", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadObj(string filePath, out nint document);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_write_gltf", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteGltf(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_write_obj", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteObj(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_write_ply", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WritePly(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_document_write_vrml", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteVrml(OcafDocumentHandle document, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_visual_material", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetVisualMaterial(
        OcafDocumentHandle document,
        string entry,
        nint name,
        int nameLength,
        double red,
        double green,
        double blue,
        double alpha,
        double metallic,
        double roughness,
        double emissiveRed,
        double emissiveGreen,
        double emissiveBlue,
        double refractionIndex,
        int alphaMode,
        double alphaCutoff);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_visual_material_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVisualMaterial(
        OcafDocumentHandle document,
        string entry,
        out int hasMaterial,
        out double red,
        out double green,
        out double blue,
        out double alpha,
        out double metallic,
        out double roughness,
        out double emissiveRed,
        out double emissiveGreen,
        out double emissiveBlue,
        out double refractionIndex,
        out int alphaMode,
        out double alphaCutoff);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_visual_material_name_utf8_length", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVisualMaterialNameLength(
        OcafDocumentHandle document,
        string entry,
        out int hasMaterial,
        out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_visual_material_name_to_utf8", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyVisualMaterialName(
        OcafDocumentHandle document,
        string entry,
        nint buffer,
        int capacity,
        out int written);
}
