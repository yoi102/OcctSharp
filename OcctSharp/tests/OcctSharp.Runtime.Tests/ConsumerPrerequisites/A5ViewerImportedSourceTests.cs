using System.IO.Compression;
using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

public sealed partial class A5ViewerSourceTests
{
    [Theory]
    [InlineData(false, ViewerDisplayMode.Shaded)]
    [InlineData(false, ViewerDisplayMode.Wireframe)]
    [InlineData(true, ViewerDisplayMode.Shaded)]
    [InlineData(true, ViewerDisplayMode.Wireframe)]
    public void ImportedStepHierarchyAndIgesStylesRetainExactSelectionsAfterCallerRelease(bool iges, ViewerDisplayMode mode)
    {
        void Trace(string stage) => TraceImportSource($"iges={iges},mode={mode}: {stage}");
        var path = Path.Combine(Path.GetTempPath(), "Cadoryx-A5-" + Guid.NewGuid().ToString("N") + (iges ? ".iges" : ".step"));
        try
        {
            Trace("author/export");
            using (var authored = XdeDocument.Create())
            using (var box = ShapeFactory.CreateBox(20, 10, 5))
            using (var identity = GpTrsf.Identity)
            using (var shift = GpTrsf.Create(40, 0, 0, rotationAngleRadians: .25))
            using (var zeroLocation = TopLocLocation.FromTransform(identity))
            using (var shiftedLocation = TopLocLocation.FromTransform(shift))
            {
                using (var edit = authored.BeginTransaction())
                {
                    var part = authored.AddShape(box, "Imported styled exact part");
                    part.SetPresentationColor(XdeColorChannel.Surface, new(.15, .25, .8, .65));
                    part.SetPresentationColor(XdeColorChannel.Curve, new(.45, .25, .05));
                    using var repair = new RepairDocumentSession(authored, part);
                    var face = repair.GetOrCreateSubshapeLabel(repair.Source.Topology.First(item => item.Kind == ShapeKind.Face).Selection);
                    face.SetPresentationColor(XdeColorChannel.Surface, new(.9, .15, .05));
                    if (!iges)
                    {
                        var nested = authored.AddAssembly("Nested reusable part");
                        authored.AddComponent(nested, part, zeroLocation);
                        var second = authored.AddComponent(nested, part, shiftedLocation);
                        second.SetPresentationColor(XdeColorChannel.Surface, new(.1, .8, .2));
                        var root = authored.AddAssembly("Two-level shared source");
                        authored.AddComponent(root, nested, zeroLocation);
                    }
                    edit.Commit();
                }
                if (iges) authored.WriteIges(path); else authored.WriteStep(path);
            }
            using var imported = iges ? XdeDocument.ReadIges(path) : XdeDocument.ReadStep(path);
            Trace("imported");
            var label = Assert.Single(imported.GetFreeShapes());
            if (!iges)
            {
                Assert.True(label.IsAssembly);
                var nested = Assert.Single(label.GetComponents()).ReferredShape;
                Assert.True(nested.IsAssembly); Assert.Equal(2, nested.ComponentCount);
            }
            using var original = label.Shape;
            var fingerprint = RepairSnapshot.ComputeFingerprint(original);
            using var window = new Window(); using var identityInstance = GpTrsf.Identity;
            using var source = window.Viewer.CreateSource(label);
            Trace("source-created");
            using var instance = window.Viewer.Display(source, identityInstance);
            Trace("instance-displayed");
            instance.SetDisplayMode(mode); window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
            var before = CaptureRefresh(window);
            AssertMatchesFreshSource(window, label, instance, identityInstance, mode);
            Trace("initial-reference-compared");
            // A style-only revision of the imported root retains exact topology.
            using (var edit = imported.BeginTransaction())
            { label.SetPresentationColor(XdeColorChannel.Curve, new(.2, .7, .4)); edit.Commit(); }
            source.RefreshStyles(label);
            Trace("refreshed");
            AssertMatchesFreshSource(window, label, instance, identityInstance, mode);
            Trace("refreshed-reference-compared");
            var refreshed = CaptureRefresh(window);
            imported.Dispose(); File.Delete(path); source.Dispose();
            Trace("callers-released");
            Assert.Equal(refreshed, CaptureRefresh(window));
            Assert.Equal(fingerprint, RepairSnapshot.ComputeFingerprint(original));
            instance.SetDisplayMode(ViewerDisplayMode.Shaded);
            foreach (ShapeKind? kind in new ShapeKind?[] { null, ShapeKind.Face, ShapeKind.Edge })
            {
                Trace($"selection-kind={kind}");
                instance.SetSelectionKind(kind);
                Trace("selection-mode-activated");
                var pixel = window.Viewer.WorldToScreen(kind == ShapeKind.Edge ? new(10, 0, 5) : new(10, 5, 5));
                try
                {
                    Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
                    Trace("selected");
                    using var diagnosed = window.Viewer.GetDetectedItem();
                    Trace($"detected-shape={diagnosed?.Shape.Kind}");
                }
                catch (Exception error) { Trace(error.ToString()); throw; }
                using var detected = window.Viewer.GetDetectedItem(); Assert.NotNull(detected);
                Assert.Same(instance, detected.Presentation);
                Assert.True(RepairSnapshot.FindTopologyIndex(original, detected.Shape) >= 0);
                using var selected = window.Viewer.GetSelectedItems().Single();
                Assert.True(RepairSnapshot.FindTopologyIndex(original, selected.Shape) >= 0);
                window.Viewer.ClearSelection();
                Trace($"selection-kind={kind} complete");
            }
            Trace("restore-display-mode");
            instance.SetDisplayMode(mode); Assert.Equal(refreshed, CaptureRefresh(window));
            Assert.NotEmpty(before);
            Trace("remove-instance"); instance.Dispose(); Trace("instance-removed");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static void TraceImportSource(string stage)
    {
        var directory = Environment.GetEnvironmentVariable("OCCTSHARP_A5_EVIDENCE_DIR");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        File.AppendAllText(Path.Combine(directory, "import-source.trace"), stage + Environment.NewLine);
    }

    [Fact]
    public void ExternalGltfTextureIsSnapshottedBeforeFirstDisplayAndCallerRelease()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Cadoryx-A5-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var imagePath = Path.Combine(directory, "red.png");
            WriteSourceTexture(imagePath);
            var path = Path.Combine(directory, "textured.gltf"); WriteTexturedGltf(path);
            using var document = XdeDocument.ReadGltf(path);
            var label = Assert.Single(document.GetFreeShapes());
            using var sourceWindow = new Window();
            using var source = sourceWindow.Viewer.CreateSource(label);
            byte[] expected;
            using (var referenceWindow = new Window())
            using (var referenceSource = referenceWindow.Viewer.CreateSource(label))
            using (var referenceTransform = GpTrsf.Identity)
            using (var reference = referenceWindow.Viewer.Display(referenceSource, referenceTransform))
            {
                reference.SetDisplayMode(ViewerDisplayMode.Shaded);
                referenceWindow.Viewer.FitAll(); expected = CaptureRefresh(referenceWindow);
            }
            Assert.True(Enumerable.Range(0, expected.Length / 4).Count(index =>
                expected[index * 4] > 120 && expected[index * 4 + 1] < 60 && expected[index * 4 + 2] < 60) > 100,
                "The reference must visibly render the red source texture.");
            // No GPU presentation was created from source before deleting the
            // caller's file. A cached ordinary texture cannot mask a borrowed path.
            File.Delete(imagePath); document.Dispose();
            using var identity = GpTrsf.Identity;
            using var instance = sourceWindow.Viewer.Display(source, identity);
            source.Dispose(); instance.SetDisplayMode(ViewerDisplayMode.Shaded);
            sourceWindow.Viewer.FitAll();
            Assert.Equal(expected, CaptureRefresh(sourceWindow));
        }
        finally
        {
            Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), Path.GetFullPath(directory), StringComparison.OrdinalIgnoreCase);
            Directory.Delete(directory, true);
        }
    }

    private static void WriteTexturedGltf(string path)
    {
        using var data = new MemoryStream(); using (var writer = new BinaryWriter(data, System.Text.Encoding.UTF8, true))
        {
            foreach (float value in new float[] { 0,0,0, 20,0,0, 20,10,0, 0,10,0 }) writer.Write(value);
            for (int index = 0; index < 4; index++) { writer.Write(0f); writer.Write(0f); writer.Write(1f); }
            foreach (float value in new float[] { 0,0, 1,0, 1,1, 0,1 }) writer.Write(value);
            foreach (ushort value in new ushort[] { 0,1,2, 0,2,3 }) writer.Write(value);
        }
        var model = new
        {
            asset = new { version = "2.0" }, scene = 0,
            scenes = new[] { new { nodes = new[] { 0 } } }, nodes = new[] { new { mesh = 0 } },
            buffers = new[] { new { byteLength = (int)data.Length, uri = "data:application/octet-stream;base64," + Convert.ToBase64String(data.ToArray()) } },
            bufferViews = new[] {
                new { buffer = 0, byteOffset = 0, byteLength = 48 }, new { buffer = 0, byteOffset = 48, byteLength = 48 },
                new { buffer = 0, byteOffset = 96, byteLength = 32 }, new { buffer = 0, byteOffset = 128, byteLength = 12 } },
            accessors = new object[] {
                new { bufferView = 0, componentType = 5126, count = 4, type = "VEC3", min = new[] { 0,0,0 }, max = new[] { 20,10,0 } },
                new { bufferView = 1, componentType = 5126, count = 4, type = "VEC3" },
                new { bufferView = 2, componentType = 5126, count = 4, type = "VEC2" },
                new { bufferView = 3, componentType = 5123, count = 6, type = "SCALAR" } },
            meshes = new[] { new { primitives = new[] { new { attributes = new { POSITION = 0, NORMAL = 1, TEXCOORD_0 = 2 }, indices = 3, material = 0 } } } },
            images = new[] { new { uri = "red.png" } }, textures = new[] { new { source = 0 } },
            materials = new[] { new { doubleSided = true, pbrMetallicRoughness = new { baseColorTexture = new { index = 0 }, metallicFactor = 0, roughnessFactor = 1 } } }
        };
        File.WriteAllText(path, JsonSerializer.Serialize(model));
    }

    private static void WriteSourceTexture(string path)
    {
        using var image = new MemoryStream(); image.Write(new byte[] { 137,80,78,71,13,10,26,10 });
        WriteChunk(image, "IHDR", [0,0,0,2, 0,0,0,2, 8,2,0,0,0]);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true))
            zlib.Write(new byte[] { 0,255,0,0,255,0,0, 0,255,0,0,255,0,0 });
        WriteChunk(image, "IDAT", compressed.ToArray()); WriteChunk(image, "IEND", []);
        File.WriteAllBytes(path, image.ToArray());
    }
    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
        uint crc = uint.MaxValue;
        foreach (byte value in bytes)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u);
        }
        WriteBigEndian(stream, (uint)data.Length); stream.Write(bytes); WriteBigEndian(stream, ~crc);
    }
    private static void WriteBigEndian(Stream stream, uint value) => stream.Write(new[]
        { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
}
