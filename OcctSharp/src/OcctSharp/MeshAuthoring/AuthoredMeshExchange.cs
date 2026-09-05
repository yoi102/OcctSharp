using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

public enum AuthoredMeshFormat { Stl, Obj, Gltf, Ply, Step, Iges }
public sealed record AuthoredMeshImportOptions(MeshCoordinates? SourceCoordinates = null, MeshCoordinates? TargetCoordinates = null,
    long MaximumBytes = 64 * 1024 * 1024);
public sealed class AuthoredMeshImportResult
{
    internal AuthoredMeshImportResult(AuthoredMesh mesh, IEnumerable<string> disclosures) =>
        (Mesh, Disclosures) = (mesh, Array.AsReadOnly(disclosures.ToArray()));
    public AuthoredMesh Mesh { get; }
    public IReadOnlyList<string> Disclosures { get; }
}
public sealed class AuthoredMeshExportResult
{
    internal AuthoredMeshExportResult(string path, MeshCoordinates coordinates, IEnumerable<string> assets, IEnumerable<string> disclosures) =>
        (Path, Coordinates, Assets, Disclosures) = (path, coordinates, Array.AsReadOnly(assets.ToArray()), Array.AsReadOnly(disclosures.ToArray()));
    public string Path { get; }
    public MeshCoordinates Coordinates { get; }
    public IReadOnlyList<string> Assets { get; }
    public IReadOnlyList<string> Disclosures { get; }
}

/// <summary>Bounded editable import and no-remeshing discrete delivery with explicit format/channel limitations.</summary>
public static class AuthoredMeshExchange
{
    public static AuthoredMeshImportResult Read(string path, AuthoredMeshImportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path); string fullPath = System.IO.Path.GetFullPath(path);
        string extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
        int format = extension switch { ".stl" => 0, ".obj" => 1, _ => throw new NotSupportedException("Direct editable import supports STL and OBJ.") };
        AuthoredMeshImportOptions policy = options ?? new();
        if (policy.MaximumBytes <= 0 || policy.MaximumBytes > 256 * 1024 * 1024) throw new ArgumentOutOfRangeException(nameof(options));
        MeshCoordinates coordinates = policy.SourceCoordinates ?? new(); coordinates.Validate(); policy.TargetCoordinates?.Validate();
        FileInfo file = new(fullPath);
        if (!file.Exists) throw new FileNotFoundException("Mesh input not found.", fullPath);
        if (file.Length == 0 || file.Length > policy.MaximumBytes) throw new ArgumentException("Mesh file is empty or exceeds the declared byte limit.");
        using ExchangePathStage stage = ExchangePathStage.ForInput(fullPath); OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.MeshReadEditable(stage.NativePath, format, policy.MaximumBytes, out nint native, out int flags), "mesh_read_editable");
        using Shape shape = ShapeFactory.FromNativeHandle(native, "mesh_read_editable");
        ExistingMeshSnapshot snapshot = MeshTopology.SnapshotExisting(shape, coordinates);
        List<string> disclosures = [.. snapshot.Disclosures, "Input coordinates/units are explicit caller assumptions; STL/OBJ do not carry a reliable unit convention."];
        if ((flags & 1) != 0) disclosures.Add("Partial OBJ UV data was omitted; missing UVs are not represented by fabricated zeros.");
        if ((flags & 2) != 0) disclosures.Add("Missing/invalid OBJ normals are explicitly undefined, not the reader's default +Z normal.");
        if ((flags & 4) != 0) disclosures.Add("STL retains positions and connectivity; UVs, authored vertex normals, groups and materials are unavailable.");
        if ((flags & 8) != 0) disclosures.Add("Direct OBJ import retains supported vertex/UV/normal seams; object/group names, MTL materials, lines and texture assets are not retained by this editable reader.");
        AuthoredMesh mesh = policy.TargetCoordinates is null ? snapshot.Mesh : MeshEditing.ConvertCoordinates(snapshot.Mesh, policy.TargetCoordinates).Mesh;
        return new(mesh, disclosures);
    }

    public static unsafe AuthoredMeshExportResult Write(AuthoredMesh mesh, string path, AuthoredMeshFormat? format = null,
        IReadOnlyDictionary<string, XdeVisualMaterial>? materials = null, bool binaryStl = true)
    {
        ArgumentNullException.ThrowIfNull(mesh); ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = System.IO.Path.GetFullPath(path), extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
        AuthoredMeshFormat selected = format ?? extension switch
        {
            ".stl" => AuthoredMeshFormat.Stl, ".obj" => AuthoredMeshFormat.Obj, ".gltf" or ".glb" => AuthoredMeshFormat.Gltf,
            ".ply" => AuthoredMeshFormat.Ply, ".step" or ".stp" => AuthoredMeshFormat.Step, ".iges" or ".igs" => AuthoredMeshFormat.Iges,
            _ => throw new NotSupportedException("Unrecognized authored mesh output format.")
        };
        if (!Enum.IsDefined(selected) || selected is AuthoredMeshFormat.Step or AuthoredMeshFormat.Iges)
            throw new NotSupportedException("Discrete triangulations cannot be delivered as exact STEP/IGES BRep by this API.");
        string expected = selected switch { AuthoredMeshFormat.Stl => ".stl", AuthoredMeshFormat.Obj => ".obj", AuthoredMeshFormat.Ply => ".ply", _ => extension == ".glb" ? ".glb" : ".gltf" };
        if (extension != expected) throw new ArgumentException("The output extension must agree with the selected mesh format.");
        if (mesh.Triangles.Count == 0) throw new ArgumentException("Mesh delivery requires triangles.");
        List<string> disclosures = [];
        if (mesh.Polylines.Count > 0) disclosures.Add("Associated polylines are not included in triangle-mesh format delivery.");
        using MeshDeliveryStage stage = new(fullPath); MeshCoordinates coordinates;
        if (selected == AuthoredMeshFormat.Stl)
        {
            AuthoredVertexRaw[] vertices = MeshBuffers.Vertices(mesh); AuthoredTriangleRaw[] triangles = MeshBuffers.Triangles(mesh.Triangles);
            OcctRuntime.EnsureCompatible();
            fixed (AuthoredVertexRaw* v = vertices)
            fixed (AuthoredTriangleRaw* t = triangles)
                NativeError.ThrowIfFailed(NativeMethods.MeshWriteStl(v, vertices.Length, t, triangles.Length, stage.NativePath, binaryStl ? 1 : 0), "mesh_write_stl");
            coordinates = mesh.Coordinates;
            disclosures.Add("STL drops groups/materials/UVs/authored normals and stores geometry with format precision; units remain an external assumption.");
        }
        else
        {
            // OCCT's format iterators substitute +Z for undefined normals. Omit the entire
            // optional channel explicitly instead of writing invented authored directions.
            AuthoredMesh delivered = mesh;
            if (mesh.Normals is not null && mesh.Normals.Any(n => !n.IsDefined))
            {
                delivered = new(mesh.Positions, mesh.Triangles, uvs: mesh.UVs, groups: mesh.Groups,
                    polylines: mesh.Polylines, coordinates: mesh.Coordinates);
                disclosures.Add("Normal channel omitted because it contains undefined values; format output does not fabricate +Z directions.");
            }
            using XdeDocument document = XdeDocument.Create();
            MeshAssembly.Create(document, delivered, "Authored mesh", materials: materials);
            int channels = (delivered.Normals is null ? 0 : 1) | (delivered.UVs is null ? 0 : 2);
            NativeError.ThrowIfFailed(NativeMethods.MeshWriteDocument(document.Handle, stage.NativePath, (int)selected, extension == ".glb" ? 1 : 0, channels), "mesh_write_document");
            coordinates = selected == AuthoredMeshFormat.Gltf ? new(1, MeshUpAxis.Y) : new();
            if (selected == AuthoredMeshFormat.Ply) disclosures.Add("PLY retains supported vertex channels/color and part IDs, not the complete PBR material model or assembly graph.");
            if (selected == AuthoredMeshFormat.Obj) disclosures.Add("OBJ/MTL retains supported mesh channels and basic material colors, not a complete PBR material model.");
            if (selected == AuthoredMeshFormat.Gltf) disclosures.Add("glTF uses metre/Y-up coordinates and float vertex precision; arbitrary texture assets are outside this delivery contract.");
        }
        return new(fullPath, coordinates, stage.Complete(), disclosures);
    }
}

/// <summary>Stages the complete native output asset set; sidecars have unique names and are never silently overwritten.</summary>
internal sealed class MeshDeliveryStage : IDisposable
{
    private readonly DirectoryInfo directory;
    private readonly string destination;
    internal MeshDeliveryStage(string destination)
    {
        this.destination = destination; directory = Directory.CreateTempSubdirectory("occtsharp-mesh-");
        if (directory.FullName.Any(c => c > 127)) { directory.Delete(); throw new IOException("Native mesh export requires an ASCII temporary directory."); }
        NativePath = System.IO.Path.Combine(directory.FullName, $"occtsharp-{Guid.NewGuid():N}{System.IO.Path.GetExtension(destination)}");
    }
    internal string NativePath { get; }
    internal IReadOnlyList<string> Complete()
    {
        if (!File.Exists(NativePath) || new FileInfo(NativePath).Length == 0) throw new IOException("Mesh provider did not create the primary output.");
        string targetDirectory = System.IO.Path.GetDirectoryName(destination)!; Directory.CreateDirectory(targetDirectory);
        List<string> created = [];
        try
        {
            foreach (FileInfo file in directory.GetFiles().OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                if (file.FullName == NativePath) continue;
                string target = System.IO.Path.Combine(targetDirectory, file.Name); file.CopyTo(target, false); created.Add(target);
            }
            File.Move(NativePath, destination, true); created.Insert(0, destination); return created.AsReadOnly();
        }
        catch
        {
            foreach (string path in created) File.Delete(path);
            throw;
        }
    }
    public void Dispose() => directory.Delete(true);
}
