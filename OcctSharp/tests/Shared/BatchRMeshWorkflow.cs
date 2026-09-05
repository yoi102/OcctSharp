using System.Runtime.InteropServices;
using System.Text.Json;

namespace OcctSharp.Validation;

// Public-only delivery workflow also executed from the clean facade package consumer.
internal static class BatchRMeshWorkflow
{
    internal static AuthoredMesh CreateMesh() => new(
        [new(0, 0, 0), new(20, 0, 0), new(20, 20, 0), new(0, 20, 0)],
        [new(0, 1, 2, 3), new(0, 2, 3, 9)],
        Enumerable.Repeat(new MeshNormal(0, 0, 1), 4),
        [new(0, 0), new(1, 0), new(1, 1), new(0, 1)],
        [new(3, "Red triangle", "red", "part:3"), new(9, "Blue triangle", "blue", "part:9")]);

    internal static IReadOnlyDictionary<string, XdeVisualMaterial> Materials() =>
        new Dictionary<string, XdeVisualMaterial>(StringComparer.Ordinal)
        {
            ["red"] = new("Red", new(0.9, 0.05, 0.05), 0.1, 0.6, GpXyz.Origin),
            ["blue"] = new("Blue", new(0.05, 0.05, 0.9), 0.2, 0.5, GpXyz.Origin)
        };

    public static void Run()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchR.");
        try
        {
            AuthoredMesh mesh = CreateMesh();
            VerifyPublication(mesh);
            VerifyFormats(mesh, directory.FullName);
            VerifyViewer(mesh, directory.FullName);
        }
        finally { directory.Delete(true); }
    }

    private static void VerifyPublication(AuthoredMesh mesh)
    {
        using XdeDocument document = XdeDocument.Create(); document.UndoLimit = 10;
        MeshAssemblyProduct publication = MeshAssembly.Create(document, mesh, "Two placed meshes",
            [new("Origin"), new("Placed", 30, 0, 5)], Materials());
        Require(publication.Root.GetComponents().Count == 2 && publication.Definition.GetComponents().Count == 2,
            "one grouped definition and two occurrences");
        Require(publication.Scene.Groups.Count == 2 && publication.Scene.Instances.Count == 2, "copied grouped scene");
        Require(publication.Scene.Groups[0].Group.SourceKey == "part:3" &&
            publication.Scene.Groups[1].Material?.BaseColor.Blue > 0.8, "material and source provenance");
        Require(publication.CoordinateMap.Source == mesh.Revision, "publication correspondence");
        List<double> starts = [];
        IReadOnlyList<XdeOccurrence> leaves = publication.Root.GetOccurrences();
        try
        {
            Require(leaves.Count(o => !o.IsAssembly) == 4, "repeated discrete leaf count");
            foreach (XdeOccurrence leaf in leaves.Where(o => !o.IsAssembly))
            {
                using Shape located = leaf.GetLocatedShape();
                Require(!MeshTopology.IsSurfaceBacked(located), "discrete capability boundary");
                AuthoredMesh read = MeshTopology.SnapshotExisting(located).Mesh;
                Require(read.Triangles.Count == 1, "no-remeshing occurrence snapshot");
                starts.Add(read.Positions.Min(p => p.X));
            }
            Require(starts.Min() == 0 && starts.Max() == 30, "rigid occurrence placements");
        }
        finally { foreach (XdeOccurrence leaf in leaves) leaf.Dispose(); }
        Require(document.Undo() && document.GetFreeShapes().Length == 0, "atomic publication undo");
        Require(document.Redo() && publication.Root.GetComponents().Count == 2, "publication redo");
        AuthoredMeshScene copied = publication.Scene;
        document.Dispose();
        Require(copied.Instances[1].Transform.M14 == 30 && copied.Mesh.Triangles.Count == 2, "scene survives document disposal");
        bool rejected = false;
        try { _ = publication.Root.Name; } catch (ObjectDisposedException) { rejected = true; }
        Require(rejected, "parent-bound labels reject disposed document");
    }

    private static void VerifyFormats(AuthoredMesh mesh, string directory)
    {
        foreach (string extension in new[] { ".stl", ".obj", ".gltf", ".glb", ".ply" })
        {
            string path = Path.Combine(directory, "編集モデル" + extension);
            AuthoredMeshExportResult exported = AuthoredMeshExchange.Write(mesh, path, materials: Materials());
            Require(exported.Assets.All(p => new FileInfo(p).Length > 0), "complete output asset set: " + extension);
            if (extension is ".stl" or ".obj")
            {
                AuthoredMesh read = AuthoredMeshExchange.Read(path, new(exported.Coordinates)).Mesh;
                Require(read.Triangles.Count == 2 && Math.Abs(Area(read) - 400) < 1e-4, "editable format area/indices: " + extension);
                Require(read.Positions.Max(p => p.X) == 20, "format units: " + extension);
                if (extension == ".obj")
                {
                    Require(read.UVs is not null && read.UVs.Any(uv => uv.U == 1 && uv.V == 1), "OBJ UV roundtrip");
                    Require(read.Normals is not null && read.Normals.All(n => n.IsDefined && n.Z > 0.999), "OBJ normal roundtrip");
                    Require(exported.Assets.Count == 2 && exported.Assets.Any(p => p.EndsWith(".mtl", StringComparison.Ordinal)), "OBJ sidecar publication");
                    string mtl = File.ReadAllText(exported.Assets.Single(p => p.EndsWith(".mtl", StringComparison.Ordinal)));
                    Require(mtl.Contains("newmtl", StringComparison.Ordinal) && mtl.Contains("Kd", StringComparison.Ordinal), "OBJ material definitions");
                }
            }
            else if (extension == ".gltf")
            {
                Require(exported.Coordinates == new MeshCoordinates(1, MeshUpAxis.Y), "glTF coordinate declaration");
                using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
                JsonElement root = json.RootElement;
                Require(root.GetProperty("materials").GetArrayLength() == 2, "glTF group material preservation");
                foreach (JsonElement buffer in root.GetProperty("buffers").EnumerateArray())
                    Require(File.Exists(Path.Combine(directory, buffer.GetProperty("uri").GetString()!)), "relative glTF binary sidecar");
                double maxX = root.GetProperty("accessors").EnumerateArray()
                    .Where(a => a.TryGetProperty("max", out JsonElement max) && max.GetArrayLength() == 3)
                    .Max(a => a.GetProperty("max")[0].GetDouble());
                Require(Math.Abs(maxX - 0.02) < 1e-6, "glTF metre conversion exactly once: " + maxX);
            }
            else if (extension == ".glb")
            {
                MeshScene read = MeshScene.ReadGltf(path);
                Require(read.TotalTriangleCount == 2 && read.Definitions.Any(d => d.Mesh.HasUv), "GLB actual reader preserves mesh channels");
            }
            else VerifyPly(path);
        }
        AuthoredMeshExportResult ascii = AuthoredMeshExchange.Write(mesh, Path.Combine(directory, "ascii.stl"), binaryStl: false);
        Require(Math.Abs(Area(AuthoredMeshExchange.Read(ascii.Path).Mesh) - 400) < 1e-4, "ASCII STL roundtrip");
    }

    // Read the writer's declared PLY format independently, including stored vertex values.
    private static void VerifyPly(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        int headerEnd = bytes.AsSpan().IndexOf("end_header\n"u8);
        Require(headerEnd >= 0, "PLY header terminator");
        int offset = headerEnd + "end_header\n".Length;
        string header = System.Text.Encoding.ASCII.GetString(bytes, 0, offset);
        Require(header.Contains("element face 2", StringComparison.Ordinal) && header.Contains("property float nx", StringComparison.Ordinal)
            && header.Contains("property float s", StringComparison.Ordinal) && header.Contains("property uchar red", StringComparison.Ordinal), "PLY channels: " + header);
        bool ascii = header.Contains("format ascii", StringComparison.Ordinal);
        Require(ascii || header.Contains("format binary_little_endian", StringComparison.Ordinal), "bounded PLY fixture format");
        string[] textValues = ascii ? System.Text.Encoding.ASCII.GetString(bytes, offset, bytes.Length - offset)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) : [];
        int textIndex = 0;
        using BinaryReader reader = new(new MemoryStream(bytes, offset, bytes.Length - offset));
        string[] lines = header.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int vertexLine = Array.FindIndex(lines, l => l.StartsWith("element vertex ", StringComparison.Ordinal));
        int count = int.Parse(lines[vertexLine].Split(' ')[2], System.Globalization.CultureInfo.InvariantCulture);
        List<string[]> properties = [];
        for (int i = vertexLine + 1; i < lines.Length && lines[i].StartsWith("property ", StringComparison.Ordinal); ++i)
            properties.Add(lines[i].Trim().Split(' '));
        double maxX = double.NegativeInfinity; HashSet<int> colors = [];
        for (int v = 0; v < count; ++v)
        {
            double red = 0, blue = 0;
            foreach (string[] property in properties)
            {
                double value = ascii ? double.Parse(textValues[textIndex++], System.Globalization.CultureInfo.InvariantCulture) : property[1] switch
                {
                    "double" => reader.ReadDouble(), "float" => reader.ReadSingle(), "uchar" => reader.ReadByte(),
                    "int" => reader.ReadInt32(), "uint" => reader.ReadUInt32(), _ => throw new InvalidOperationException("Unexpected PLY property: " + property[1])
                };
                if (property[2] == "x") maxX = Math.Max(maxX, value);
                if (property[2] == "red") red = value;
                if (property[2] == "blue") blue = value;
            }
            colors.Add(red > blue ? 1 : -1);
        }
        Require(count == 6 && maxX == 20 && colors.Count == 2, "PLY coordinates and group colors");
    }

    private static void VerifyViewer(AuthoredMesh mesh, string directory)
    {
        nint window = CreateWindowEx(0, "STATIC", "Batch R mesh review", 0x80000000u, -32000, -32000, 320, 320, 0, 0, 0, 0);
        Require(window != 0, "real HWND creation");
        try
        {
            _ = ShowWindow(window, 4); _ = UpdateWindow(window);
            using OcctViewer viewer = OcctViewer.Create(window);
            using MeshViewerReview review = new(viewer, mesh, Materials());
            review.SelectAndFit(mesh.Revision);
            Require(viewer.GetSelection().Contains(review.Presentation), "real native discrete selection");
            bool wrongThread = Task.Run(() =>
            {
                try { review.SelectAndFit(mesh.Revision); return false; }
                catch (InvalidOperationException) { return true; }
            }).GetAwaiter().GetResult();
            Require(wrongThread, "viewer thread affinity");
            viewer.ClearSelection(); viewer.Redraw();
            string groupedImage = viewer.SaveScreenshot(Path.Combine(directory, "mesh-groups.png"));
            ViewerPresentation old = review.Presentation;
            AuthoredMesh edited = MeshEditing.AssignGroup(mesh, mesh.SelectTriangles([0]), new(4, "Blue edit", "blue")).Mesh;
            bool failedReplacement = false;
            try { review.Replace(edited); } catch (ArgumentException) { failedReplacement = true; }
            Require(failedReplacement && review.Presentation == old && review.Revision == mesh.Revision, "failed replacement is atomic");
            review.Replace(edited, Materials());
            bool staleRejected = false, oldRejected = false;
            try { review.SelectAndFit(mesh.Revision); } catch (ArgumentException) { staleRejected = true; }
            try { old.SetDisplayMode(ViewerDisplayMode.Shaded); } catch (ObjectDisposedException) { oldRejected = true; }
            Require(staleRejected && oldRejected, "old revision and native presentation invalidation");
            review.SelectAndFit(edited.Revision);
            Require(viewer.GetSelection().Contains(review.Presentation), "replacement selection");
            viewer.ClearSelection(); viewer.Redraw();
            string image = viewer.SaveScreenshot(Path.Combine(directory, "mesh-blue.png"));
            Require(new FileInfo(image).Length > 0, "material review screenshot");
            string? evidence = Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_R_EVIDENCE");
            if (!string.IsNullOrWhiteSpace(evidence))
            {
                Directory.CreateDirectory(evidence); File.Copy(image, Path.Combine(evidence, "mesh-blue.png"), true);
                File.Copy(groupedImage, Path.Combine(evidence, "mesh-groups.png"), true);
            }
            viewer.Dispose();
            bool parentDisposed = false;
            try { review.SelectAndFit(edited.Revision); } catch (ObjectDisposedException) { parentDisposed = true; }
            Require(parentDisposed, "review rejects disposed viewer");
            review.Dispose(); review.Dispose();
        }
        finally { Require(DestroyWindow(window), "HWND cleanup"); }
    }
    private static double Area(AuthoredMesh mesh) => MeshEditing.Measure(mesh, mesh.SelectTriangles(Enumerable.Range(0, mesh.Triangles.Count))).SurfaceArea;
    private static void Require(bool condition, string operation)
    { if (!condition) throw new InvalidOperationException("Batch R workflow failed: " + operation); }
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);
}
