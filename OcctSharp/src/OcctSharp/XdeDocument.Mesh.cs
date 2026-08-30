using OcctSharp.Interop;

namespace OcctSharp;

public sealed partial class XdeDocument
{
    /// <summary>Imports a glTF or GLB file as an XDE scene document.</summary>
    public static XdeDocument ReadGltf(string filePath) =>
        OpenCore(filePath, AdvancedMeshNativeMethods.ReadGltf, "xde_document_read_gltf");

    /// <summary>Imports an OBJ file as an XDE scene document.</summary>
    public static XdeDocument ReadObj(string filePath) =>
        OpenCore(filePath, AdvancedMeshNativeMethods.ReadObj, "xde_document_read_obj");

    /// <summary>Writes this XDE scene as glTF or binary GLB according to the extension.</summary>
    public string WriteGltf(string filePath) =>
        WriteFile(filePath, AdvancedMeshNativeMethods.WriteGltf, "xde_document_write_gltf");

    /// <summary>Writes this XDE scene as OBJ with document-aware names and styles.</summary>
    public string WriteObj(string filePath) =>
        WriteFile(filePath, AdvancedMeshNativeMethods.WriteObj, "xde_document_write_obj");

    /// <summary>Writes this XDE scene as PLY.</summary>
    public string WritePly(string filePath) =>
        WriteFile(filePath, AdvancedMeshNativeMethods.WritePly, "xde_document_write_ply");

    /// <summary>Writes this XDE scene as VRML.</summary>
    public string WriteVrml(string filePath) =>
        WriteFile(filePath, AdvancedMeshNativeMethods.WriteVrml, "xde_document_write_vrml");

    internal void SetVisualMaterial(string entry, XdeVisualMaterial material)
    {
        ArgumentNullException.ThrowIfNull(material);
        material.Validate();
        ThrowIfDisposed();
        using Utf8Buffer name = Utf8Buffer.FromString(material.Name);
        NativeError.ThrowIfFailed(
            AdvancedMeshNativeMethods.SetVisualMaterial(
                Handle, entry, name.Pointer, name.Length,
                material.BaseColor.Red, material.BaseColor.Green,
                material.BaseColor.Blue, material.BaseColor.Alpha,
                material.Metallic, material.Roughness,
                material.EmissiveFactor.X, material.EmissiveFactor.Y,
                material.EmissiveFactor.Z, material.RefractionIndex,
                (int)material.AlphaMode, material.AlphaCutoff),
            "xde_label_set_visual_material");
    }

    internal XdeVisualMaterial? GetVisualMaterial(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            AdvancedMeshNativeMethods.GetVisualMaterial(
                Handle, entry, out int hasMaterial,
                out double red, out double green, out double blue, out double alpha,
                out double metallic, out double roughness,
                out double emissiveRed, out double emissiveGreen, out double emissiveBlue,
                out double refractionIndex, out int alphaMode, out double alphaCutoff),
            "xde_label_visual_material_info");
        if (hasMaterial == 0) return null;
        NativeError.ThrowIfFailed(
            AdvancedMeshNativeMethods.GetVisualMaterialNameLength(
                Handle, entry, out int hasNamedMaterial, out int length),
            "xde_label_visual_material_name_length");
        string name = hasNamedMaterial == 0 ? string.Empty : ReadSized(
            length,
            (nint buffer, int capacity, out int written) =>
                AdvancedMeshNativeMethods.CopyVisualMaterialName(
                    Handle, entry, buffer, capacity, out written),
            "xde_label_visual_material_name");
        return new XdeVisualMaterial(
            name,
            new XdeColor(red, green, blue, alpha),
            metallic,
            roughness,
            new GpXyz(emissiveRed, emissiveGreen, emissiveBlue),
            refractionIndex,
            (XdeAlphaMode)alphaMode,
            alphaCutoff);
    }
}
