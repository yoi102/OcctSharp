using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;

namespace OcctSharp.Generator.Tests;

public sealed class TopologyBindingEmitterTests
{
    [Fact]
    public void EmitsConfiguredTopoDsShapeValueSemantics()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-topology-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "TopoDS_Shape.hxx"),
                """
                enum TopAbs_ShapeEnum { TopAbs_COMPOUND, TopAbs_SHAPE = 8 };
                enum TopAbs_Orientation { TopAbs_FORWARD, TopAbs_EXTERNAL = 3 };
                class TopoDS_Shape
                {
                public:
                    TopoDS_Shape();
                    TopoDS_Shape(const TopoDS_Shape& other);
                    bool IsNull() const;
                    TopAbs_ShapeEnum ShapeType() const;
                    TopAbs_Orientation Orientation() const;
                    TopoDS_Shape Reversed() const;
                    bool IsPartner(const TopoDS_Shape& other) const;
                    bool IsSame(const TopoDS_Shape& other) const;
                    bool IsEqual(const TopoDS_Shape& other) const;
                };
                """);

            DiscoveryReport report = ClangAstDiscovery.Discover(
                new DiscoveryOptions(testRoot, ["TopoDS_Shape.hxx"]));
            GeneratedBindingSet result = TopologyBindingEmitter.Emit(
                report.OcctVersion,
                report.Model,
                [CreateScope()]);

            Assert.Equal(8, result.SourceStableIds.Count);
            Assert.Equal(4, result.Files.Count);
            Assert.Contains(
                result.Files,
                file => file.RelativePath == "src/OcctSharp.Native/generated/Topology/OcctSharp.Topology.Generated.cpp"
                    && file.Content.Contains("IsPartner", StringComparison.Ordinal));
            Assert.Contains(
                result.Files,
                file => file.RelativePath == "src/OcctSharp/Generated/Topology/Topology.Generated.cs"
                    && file.Content.Contains("public Shape Reversed()", StringComparison.Ordinal)
                    && file.Content.Contains("public enum ShapeKind", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void RejectsUnsupportedTopologyScope()
    {
        GeneratedBindingSet empty = TopologyBindingEmitter.Emit(
            "test",
            new OcctSharp.Generator.Model.BindingModel([]),
            []);
        Assert.Empty(empty.Files);

        Assert.Throws<InvalidDataException>(() => TopologyBindingEmitter.Emit(
            "test",
            new OcctSharp.Generator.Model.BindingModel([]),
            [CreateScope() with { NativeType = "TopoDS_Face" }]));
    }

    [Fact]
    public void EmitsCheckedTypedTopologyCastsFromReviewedScope()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), $"occtsharp-typed-topology-test-{Guid.NewGuid():N}");
        string includeRoot = Path.Combine(testRoot, "inc");
        Directory.CreateDirectory(includeRoot);

        try
        {
            File.WriteAllText(
                Path.Combine(includeRoot, "Standard_Version.hxx"),
                "#define OCC_VERSION_COMPLETE \"test\"\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "TopoDS_Shape.hxx"),
                "enum TopAbs_ShapeEnum { TopAbs_COMPOUND, TopAbs_SHAPE = 8 }; enum TopAbs_Orientation { TopAbs_FORWARD, TopAbs_EXTERNAL = 3 }; class TopoDS_Shape { public: TopoDS_Shape(const TopoDS_Shape& other); bool IsNull() const; TopAbs_ShapeEnum ShapeType() const; TopAbs_Orientation Orientation() const; TopoDS_Shape Reversed() const; bool IsPartner(const TopoDS_Shape& other) const; bool IsSame(const TopoDS_Shape& other) const; bool IsEqual(const TopoDS_Shape& other) const; };\n");
            File.WriteAllText(
                Path.Combine(includeRoot, "TopoDS_Face.hxx"),
                "class TopoDS_Face {};\n");

            DiscoveryReport report = ClangAstDiscovery.Discover(
                new DiscoveryOptions(testRoot, ["TopoDS_Shape.hxx", "TopoDS_Face.hxx"]));
            GeneratedBindingSet result = TopologyBindingEmitter.Emit(
                report.OcctVersion,
                report.Model,
                [CreateScope() with
                {
                    TypedTypes =
                    [
                        new TopologyTypeConfiguration
                        {
                            NativeType = "TopoDS_Face",
                            Header = "TopoDS_Face.hxx",
                            ManagedTypeName = "Face",
                            ShapeKind = "Face",
                        },
                    ],
                }]);

            Assert.Equal(9, result.SourceStableIds.Count);
            string friendly = result.Files.Single(file => file.RelativePath.EndsWith("Topology.Generated.cs", StringComparison.Ordinal)).Content;
            string raw = result.Files.Single(file => file.RelativePath.EndsWith("TopologyRaw.Generated.cs", StringComparison.Ordinal)).Content;
            string native = result.Files.Single(file => file.RelativePath.EndsWith("Topology.Generated.cpp", StringComparison.Ordinal)).Content;
            Assert.Contains("public Face CastFace()", friendly, StringComparison.Ordinal);
            Assert.Contains("TryCastFace", friendly, StringComparison.Ordinal);
            Assert.Contains("CastFace", raw, StringComparison.Ordinal);
            Assert.Contains("TopoDS::Face(*value)", native, StringComparison.Ordinal);
            Assert.Contains("OCCTSHARP_STATUS_TYPE_MISMATCH", native, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void RejectsUnknownTypedTopologyKind()
    {
        Assert.Throws<InvalidDataException>(() => TopologyBindingEmitter.Emit(
            "test",
            new OcctSharp.Generator.Model.BindingModel([]),
            [CreateScope() with
            {
                TypedTypes =
                [
                    new TopologyTypeConfiguration
                    {
                        NativeType = "TopoDS_Face",
                        Header = "TopoDS_Face.hxx",
                        ManagedTypeName = "Face",
                        ShapeKind = "Solid",
                    },
                ],
            }]));
    }

    private static TopologyScopeConfiguration CreateScope() => new()
    {
        SourcePackage = "TopoDS",
        NativeType = "TopoDS_Shape",
        Header = "TopoDS_Shape.hxx",
        ExportNamePrefix = "topods_shape",
        ManagedTypeName = "Shape",
    };
}
