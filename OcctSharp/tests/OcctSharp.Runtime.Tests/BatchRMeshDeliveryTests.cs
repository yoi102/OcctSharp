namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861

public sealed class BatchRMeshDeliveryTests
{
    [Theory]
    [InlineData(".obj")]
    [InlineData(".gltf")]
    [InlineData(".ply")]
    public void FormatOutputNeverFillsAbsentOrUndefinedOptionalChannels(string extension)
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchR.Channels.");
        try
        {
            foreach (bool undefined in new[] { false, true })
            {
                AuthoredMesh source = new([new(0, 0, 0), new(2, 0, 0), new(0, 2, 0)], [new(0, 1, 2)],
                    undefined ? [new(0, 0, 1), MeshNormal.Undefined, MeshNormal.Undefined] : null);
                AuthoredMeshExportResult output = AuthoredMeshExchange.Write(source, Path.Combine(directory.FullName, "mesh" + extension));
                if (undefined) Assert.Contains(output.Disclosures, d => d.Contains("Normal channel omitted", StringComparison.Ordinal));
                string text = File.ReadAllText(output.Path);
                if (extension == ".obj")
                {
                    Assert.DoesNotContain("\nvn ", text, StringComparison.Ordinal);
                    Assert.DoesNotContain("\nvt ", text, StringComparison.Ordinal);
                    Assert.Null(AuthoredMeshExchange.Read(output.Path).Mesh.Normals);
                }
                else if (extension == ".gltf")
                {
                    Assert.DoesNotContain("\"NORMAL\"", text, StringComparison.Ordinal);
                    Assert.DoesNotContain("\"TEXCOORD_0\"", text, StringComparison.Ordinal);
                }
                else
                {
                    Assert.DoesNotContain("property float nx", text, StringComparison.Ordinal);
                    Assert.DoesNotContain("property float s", text, StringComparison.Ordinal);
                }
            }
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void EditedMaterialsFormatsAssemblyAndRealViewerCompleteThePublicWorkflow() => Validation.BatchRMeshWorkflow.Run();

    [Fact]
    public void EditableObjChannelsSeamsMissingValuesAndMalformedLimitsAreExplicit()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchR.Import.");
        try
        {
            string path = Path.Combine(directory.FullName, "属性.obj");
            File.WriteAllText(path, "v 0 0 0\nv 2 0 0\nv 2 2 0\nv 0 2 0\nvt 0 0\nvt 1 0\nvt 1 1\nvt 2 0\nvt 3 1\nvt 2 1\nvn 0 0 1\nf 1/1/1 2/2/1 3/3/1\nf 1/4/1 3/5/1 4/6/1\n");
            AuthoredMeshImportResult imported = AuthoredMeshExchange.Read(path);
            Assert.Equal(6, imported.Mesh.Positions.Count); Assert.Equal(2, imported.Mesh.Triangles.Count);
            Assert.Equal(2, imported.Mesh.UVs!.Max(v => v.V) + 1);
            Assert.All(imported.Mesh.Normals!, n => Assert.Equal(new MeshNormal(0, 0, 1), n));
            Assert.Contains(imported.Disclosures, d => d.Contains("MTL", StringComparison.Ordinal));
            File.WriteAllText(path, "v 0 0 0\nv 1 0 0\nv 0 1 0\nvt 0.5 0.5\nvn 0 0 1\nf 1/1/1 2 3\n");
            AuthoredMeshImportResult partial = AuthoredMeshExchange.Read(path);
            Assert.Null(partial.Mesh.UVs); Assert.NotNull(partial.Mesh.Normals);
            Assert.Single(partial.Mesh.Normals!, n => n.IsDefined);
            Assert.Contains(partial.Disclosures, d => d.Contains("Partial OBJ UV", StringComparison.Ordinal));
            Assert.Contains(partial.Disclosures, d => d.Contains("undefined", StringComparison.Ordinal));
            File.WriteAllText(path, "v 0 0 0\nv 1 0 0\nv 0 1 0\nf 1 2 3\n");
            AuthoredMesh unadorned = AuthoredMeshExchange.Read(path).Mesh;
            Assert.Null(unadorned.Normals); Assert.Null(unadorned.UVs);
            Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Read(path, new(MaximumBytes: 1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => AuthoredMeshExchange.Read(path, new(MaximumBytes: long.MaxValue)));
            File.WriteAllText(path, "v 0 0 0\nf 1 2 999\n");
            Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Read(path));
            File.WriteAllText(path, ""); Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Read(path));
            string stl = Path.Combine(directory.FullName, "invalid.stl"); byte[] bad = new byte[84];
            BitConverter.GetBytes(uint.MaxValue).CopyTo(bad, 80); File.WriteAllBytes(stl, bad);
            Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Read(stl));
            Assert.Throws<NotSupportedException>(() => AuthoredMeshExchange.Read(Path.ChangeExtension(path, ".step")));
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void EditableObjRejectsInventedChannelsAndNormalMagnitudeDoesNotLoseDirection()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchR.ObjChannels.");
        try
        {
            string path = Path.Combine(directory.FullName, "channels.obj");
            const string positions = "v 0 0 0\nv 1 0 0\nv 0 1 0\n";
            foreach (string magnitude in new[] { "1e30", "1e-30" })
            {
                File.WriteAllText(path, positions + $"vn 0 {magnitude} {magnitude}\nf 1//1 2//1 3//1\n");
                AuthoredMesh mesh = AuthoredMeshExchange.Read(path).Mesh;
                Assert.NotNull(mesh.Normals);
                Assert.All(mesh.Normals, n =>
                {
                    Assert.True(n.IsDefined); Assert.Equal(1 / Math.Sqrt(2), n.Y, 6); Assert.Equal(n.Y, n.Z);
                });
            }
            foreach (string attributes in new[]
            {
                "vn 0 0 1\nf 1//99 2//1 3//1\n",
                "vt 0.5 0.5\nf 1/99 2/1 3/1\n",
                "f 1//1 2//1 3//1\n",
                "f 1/1 2/1 3/1\n"
            })
            {
                File.WriteAllText(path, positions + attributes);
                Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Read(path));
            }
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void DeliveryFailuresLeaveDocumentAndExistingOutputIntact()
    {
        AuthoredMesh mesh = Validation.BatchRMeshWorkflow.CreateMesh();
        using XdeDocument document = XdeDocument.Create();
        Assert.Throws<ArgumentException>(() => MeshAssembly.Create(document, mesh, "Missing materials"));
        Assert.Empty(document.GetFreeShapes()); Assert.False(document.HasOpenTransaction);
        using (XdeTransaction transaction = document.BeginTransaction())
            Assert.Throws<InvalidOperationException>(() => MeshAssembly.Create(document, mesh, "Nested", materials: Validation.BatchRMeshWorkflow.Materials()));
        Assert.Throws<ArgumentException>(() => MeshAssembly.Create(document, mesh, "Bad placement", [new("NaN", double.NaN)], Validation.BatchRMeshWorkflow.Materials()));
        Assert.Empty(document.GetFreeShapes());
        DirectoryInfo directory = Directory.CreateTempSubdirectory("OcctSharp.BatchR.Output.");
        try
        {
            string path = Path.Combine(directory.FullName, "original.obj"); File.WriteAllText(path, "Keep original");
            Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Write(mesh, path));
            Assert.Equal("Keep original", File.ReadAllText(path)); Assert.Single(directory.GetFiles());
            Assert.Throws<NotSupportedException>(() => AuthoredMeshExchange.Write(mesh, Path.ChangeExtension(path, ".step")));
            Assert.Throws<NotSupportedException>(() => AuthoredMeshExchange.Write(mesh, Path.ChangeExtension(path, ".iges")));
            Assert.Throws<ArgumentException>(() => AuthoredMeshExchange.Write(mesh, path, AuthoredMeshFormat.Stl));
        }
        finally { directory.Delete(true); }
    }
}
