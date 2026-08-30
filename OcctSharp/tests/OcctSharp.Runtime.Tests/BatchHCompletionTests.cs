using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchHCompletionTests
{
    [Fact]
    public void AdvancedMeshCopiesGroupsAttributesStatisticsDiagnosticsAndLods()
    {
        AdvancedMeshSnapshot mesh;
        using (Shape box = ShapeFactory.CreateBox(10, 8, 6))
        {
            mesh = AdvancedMesh.Create(box, new AdvancedMeshOptions
            {
                LinearDeflection = 0.08,
                AngularDeflection = 0.3,
                Relative = false,
                Parallel = true
            });
        }

        Assert.NotEmpty(mesh.Vertices);
        Assert.NotEmpty(mesh.Triangles);
        Assert.Equal(6, mesh.Groups.Count);
        Assert.True(mesh.HasUv);
        Assert.All(mesh.Vertices, vertex =>
        {
            Assert.True(double.IsFinite(vertex.X));
            Assert.InRange(
                Math.Sqrt(vertex.NormalX * vertex.NormalX + vertex.NormalY * vertex.NormalY + vertex.NormalZ * vertex.NormalZ),
                0.999999,
                1.000001);
        });
        Assert.Equal(mesh.Vertices.Count, mesh.Statistics.VertexCount);
        Assert.Equal(mesh.Triangles.Count, mesh.Statistics.TriangleCount);
        Assert.Equal(mesh.Groups.Count, mesh.Statistics.FaceGroupCount);
        Assert.Equal(new GpPoint(0, 0, 0), mesh.Statistics.Bounds.Minimum);
        Assert.Equal(new GpPoint(10, 8, 6), mesh.Statistics.Bounds.Maximum);
        Assert.InRange(mesh.Statistics.SurfaceArea, 375.999, 376.001);
        Assert.True(mesh.Statistics.EstimatedBytes > 0);
        Assert.Equal(0, mesh.Diagnostics.DegenerateTriangleCount);
        Assert.Equal(0, mesh.Diagnostics.BoundaryEdgeCount);
        Assert.Equal(0, mesh.Diagnostics.NonManifoldEdgeCount);
        Assert.Equal(1, mesh.Diagnostics.ConnectedComponentCount);
        Assert.True(mesh.Diagnostics.IsClosedManifold);

        using Shape sphere = ShapeFactory.CreateSphere(12);
        AdvancedMeshLodSet lods = AdvancedMesh.CreateLods(sphere, [0.05, 0.2, 0.8]);
        Assert.Equal(3, lods.Levels.Count);
        Assert.True(lods.Levels[0].Mesh.Statistics.TriangleCount >= lods.Levels[1].Mesh.Statistics.TriangleCount);
        Assert.True(lods.Levels[1].Mesh.Statistics.TriangleCount >= lods.Levels[2].Mesh.Statistics.TriangleCount);
        sphere.Dispose();
        Assert.NotEmpty(lods.Levels[0].Mesh.Triangles);
    }

    [Fact]
    public void XdeSceneCopiesPbrPhysicalMetadataHierarchySharedDefinitionsAndTransforms()
    {
        MeshScene scene;
        using (Shape partShape = ShapeFactory.CreateBox(4, 5, 6))
        using (XdeDocument document = XdeDocument.Create())
        {
            using (XdeTransaction transaction = document.BeginTransaction())
            {
                XdeLabel part = document.AddShape(partShape, "Shared Part");
                part.Color = new(0.15, 0.35, 0.75, 0.9);
                part.SetLayer("Mechanical");
                part.AddLayer("Visible");
                part.Material = new XdeMaterial("Aluminum", "6061-T6", 2.7, "Density", "g/cm3");
                part.VisualMaterial = new XdeVisualMaterial(
                    "Blue anodized",
                    new XdeColor(0.1, 0.25, 0.8, 0.9),
                    0.65,
                    0.22,
                    new GpXyz(0.02, 0.03, 0.08),
                    1.45,
                    XdeAlphaMode.Blend,
                    0.4);

                XdeLabel nested = document.AddAssembly("Nested Assembly");
                using (GpTrsf nestedPartTransform = GpTrsf.Create(2, 3, 4))
                using (TopLocLocation nestedPartLocation = TopLocLocation.FromTransform(nestedPartTransform))
                    _ = document.AddComponent(nested, part, nestedPartLocation);

                XdeLabel root = document.AddAssembly("Root Assembly");
                using (GpTrsf nestedTransform = GpTrsf.Create(10, 0, 0))
                using (TopLocLocation nestedLocation = TopLocLocation.FromTransform(nestedTransform))
                    _ = document.AddComponent(root, nested, nestedLocation);
                using (GpTrsf directTransform = GpTrsf.Create(20, 0, 0))
                using (TopLocLocation directLocation = TopLocLocation.FromTransform(directTransform))
                    _ = document.AddComponent(root, part, directLocation);

                Assert.True(transaction.Commit());
            }

            scene = MeshScene.FromXdeDocument(document);
        }

        MeshSceneDefinition definition = Assert.Single(scene.Definitions);
        Assert.NotEmpty(definition.Mesh.Triangles);
        Assert.Equal(4, scene.Nodes.Count);
        Assert.Equal(2, scene.InstanceCount);
        Assert.Single(scene.RootNodeIndices);
        MeshSceneNode[] instances = scene.Nodes.Where(node => node.MeshDefinitionIndex == definition.Index).ToArray();
        Assert.Equal(2, instances.Length);
        Assert.All(instances, node => Assert.Equal(definition.Key, node.DefinitionEntry));
        MeshSceneNode nestedInstance = Assert.Single(instances, node => node.Path.Count == 3);
        MeshSceneNode directInstance = Assert.Single(instances, node => node.Path.Count == 2);
        Assert.Equal(12, nestedInstance.WorldTransform.M14, 8);
        Assert.Equal(3, nestedInstance.WorldTransform.M24, 8);
        Assert.Equal(4, nestedInstance.WorldTransform.M34, 8);
        Assert.Equal(20, directInstance.WorldTransform.M14, 8);
        XdeColor copiedColor = Assert.IsType<XdeColor>(directInstance.Color);
        Assert.Equal(0.15, copiedColor.Red, 6);
        Assert.Equal(0.35, copiedColor.Green, 6);
        Assert.Equal(0.75, copiedColor.Blue, 6);
        Assert.Equal(0.9, copiedColor.Alpha, 6);
        Assert.Equal(["Mechanical", "Visible"], directInstance.Layers);
        Assert.Equal("Aluminum", directInstance.PhysicalMaterial?.Name);
        XdeVisualMaterial copiedMaterial = Assert.IsType<XdeVisualMaterial>(directInstance.VisualMaterial);
        Assert.Equal("Blue anodized", copiedMaterial.Name);
        Assert.Equal(0.65, copiedMaterial.Metallic, 6);
        Assert.Equal(0.22, copiedMaterial.Roughness, 6);
    }

    [Fact]
    public void DocumentAwareGltfGlbObjPlyAndVrmlInterchangeRoundTripsCopiedScenes()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            using Shape shape = ShapeFactory.CreateCylinder(5, 12);
            using XdeDocument document = XdeDocument.Create();
            using (XdeTransaction transaction = document.BeginTransaction())
            {
                XdeLabel part = document.AddShape(shape, "Mesh Interchange Part");
                part.Color = new(0.8, 0.3, 0.15, 1);
                part.VisualMaterial = new XdeVisualMaterial(
                    "Copper",
                    new XdeColor(0.8, 0.3, 0.15, 1),
                    0.8,
                    0.3,
                    GpXyz.Origin);
                Assert.True(transaction.Commit());
            }

            string gltf = document.WriteGltf(Path.Combine(directory, "scene.gltf"));
            string glb = document.WriteGltf(Path.Combine(directory, "scene.glb"));
            string obj = document.WriteObj(Path.Combine(directory, "scene.obj"));
            string ply = document.WritePly(Path.Combine(directory, "scene.ply"));
            string vrml = document.WriteVrml(Path.Combine(directory, "scene.wrl"));
            Assert.All(new[] { gltf, glb, obj, ply, vrml }, path => Assert.True(new FileInfo(path).Length > 0));

            MeshScene gltfScene = MeshScene.ReadGltf(gltf);
            MeshScene glbScene = MeshScene.ReadGltf(glb);
            MeshScene objScene = MeshScene.ReadObj(obj);
            Assert.NotEmpty(gltfScene.Definitions);
            Assert.NotEmpty(glbScene.Definitions);
            Assert.NotEmpty(objScene.Definitions);
            Assert.True(gltfScene.TotalTriangleCount > 0);
            Assert.True(glbScene.TotalTriangleCount > 0);
            Assert.True(objScene.TotalTriangleCount > 0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void StepXdeSceneLodInterchangeToRealHwndProducesScreenshot()
    {
        string directory = CreateTemporaryDirectory();
        nint window = CreateTestWindow();
        try
        {
            string step = Path.Combine(directory, "batch-h.step");
            using (Shape source = ShapeFactory.CreateSphere(10))
            using (XdeDocument document = XdeDocument.Create())
            {
                using XdeTransaction transaction = document.BeginTransaction();
                XdeLabel part = document.AddShape(source, "Batch H Scene Part");
                part.VisualMaterial = new XdeVisualMaterial(
                    "Batch H PBR", new XdeColor(0.2, 0.7, 0.45, 1), 0.35, 0.4, GpXyz.Origin);
                Assert.True(transaction.Commit());
                document.WriteStep(step);
            }

            using XdeDocument imported = XdeDocument.ReadStep(step);
            MeshScene scene = MeshScene.FromXdeDocument(imported, new AdvancedMeshOptions { LinearDeflection = 0.15 });
            Assert.NotEmpty(scene.Definitions);
            using Shape importedShape = Assert.Single(imported.GetFreeShapes()).Shape;
            AdvancedMeshLodSet lods = AdvancedMesh.CreateLods(importedShape, [0.08, 0.3, 1.0]);
            Assert.Equal(3, lods.Levels.Count);
            string glb = imported.WriteGltf(Path.Combine(directory, "batch-h.glb"));
            Assert.True(new FileInfo(glb).Length > 0);

            using OcctViewer viewer = OcctViewer.Create(window);
            using ViewerPresentation presentation = viewer.Display(importedShape);
            viewer.FitAll();
            viewer.Redraw();
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-h.png"));
            Assert.True(new FileInfo(screenshot).Length > 0);
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchH.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch H", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(
            uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height,
            nint parent, nint menu, nint instance, nint parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
