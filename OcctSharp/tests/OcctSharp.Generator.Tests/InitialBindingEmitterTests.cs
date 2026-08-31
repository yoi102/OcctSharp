using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class InitialBindingEmitterTests
{
    private const string TestStableId = "test:gp-pnt-three-coordinates";

    [Fact]
    public void EmitsDeterministicNativeAndManagedPointBinding()
    {
        DiscoveryReport report = CreateReport();

        GeneratedBindingSet first = InitialBindingEmitter.Emit(report);
        GeneratedBindingSet second = InitialBindingEmitter.Emit(report);

        Assert.Equal(first.OcctVersion, second.OcctVersion);
        Assert.Equal(first.SourceStableIds, second.SourceStableIds);
        Assert.Equal(first.Files, second.Files);
        Assert.Equal(4, first.Files.Count);
        Assert.Equal([TestStableId], first.SourceStableIds);
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Geometry.Values.Generated.cpp", StringComparison.Ordinal)
            && file.Content.Contains("gp_Pnt value", StringComparison.Ordinal));
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("Point3dRaw.Generated.cs", StringComparison.Ordinal)
            && file.Content.Contains("GeometryGeneratedNativeMethods", StringComparison.Ordinal));
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("Geometry.ModuleRuntime.Generated.cs", StringComparison.Ordinal)
            && file.Content.Contains("typeof(GeometryGeneratedNativeMethods).Assembly", StringComparison.Ordinal));
    }

    [Fact]
    public void EmitsSelectedPrecisionStaticsWithStableOverloadOrdinals()
    {
        DiscoveryReport baseReport = CreateReport();
        BindingType real = CreateValueType("double", isConst: false);
        BindingType constReal = CreateValueType("double", isConst: true);
        BindingType boolean = CreateValueType("bool", isConst: false);
        BindingModel model = new(
        [
            .. baseReport.Model.Declarations,
            CreatePrecisionMethod(
                "precision:p-approximation-no-arguments",
                "Precision::PApproximation",
                "Precision::PApproximation: double ()",
                real,
                []),
            CreatePrecisionMethod(
                "precision:p-approximation-with-value",
                "Precision::PApproximation",
                "Precision::PApproximation: double (const double)",
                real,
                [new BindingParameter(0, "value", constReal, false)]),
            CreatePrecisionMethod(
                "precision:is-infinite",
                "Precision::IsInfinite",
                "Precision::IsInfinite: bool (const double)",
                boolean,
                [new BindingParameter(0, "value", constReal, false)]),
        ]);

        GeneratedBindingSet result = InitialBindingEmitter.Emit(baseReport with { Model = model });

        Assert.Equal(4, result.SourceStableIds.Count);
        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Foundation.Values.Generated.cpp", StringComparison.Ordinal));
        GeneratedFile managed = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("ScalarRaw.Generated.cs", StringComparison.Ordinal));
        Assert.Contains("occtsharp_generated_precision_static_p_approximation_0(void)", native.Content, StringComparison.Ordinal);
        Assert.Contains("occtsharp_generated_precision_static_p_approximation_1(", native.Content, StringComparison.Ordinal);
        Assert.Contains("Precision::IsInfinite(value) ? 1 : 0", native.Content, StringComparison.Ordinal);
        Assert.Contains("internal static partial double PrecisionStaticPApproximation0()", managed.Content, StringComparison.Ordinal);
        Assert.Contains("internal static partial double PrecisionStaticPApproximation1(double value)", managed.Content, StringComparison.Ordinal);
        Assert.Contains("internal static partial int PrecisionStaticIsInfinite0(double value)", managed.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitsConfiguredTopAbsEnumScope()
    {
        DiscoveryReport baseReport = CreateReport();
        BindingType orientation = CreateValueType("TopAbs_Orientation", isConst: false);
        BindingModel model = new(
        [
            .. baseReport.Model.Declarations,
            new BindingDeclaration(
                "enum:top-abs-orientation",
                "TopAbs_Orientation",
                BindingDeclarationKind.Enum,
                "TopAbs.hxx",
                1,
                1)
            {
                SourcePackage = "TopAbs",
                SourceToolkit = "TKG3d",
                EnumUnderlyingType = "int",
                EnumValues =
                [
                    new BindingEnumValue("TopAbs_FORWARD", "0", false),
                    new BindingEnumValue("TopAbs_REVERSED", "1", false),
                ],
            },
            CreateScopeMethod(
                "topabs:compose",
                "TopAbs::Compose",
                "TopAbs::Compose: TopAbs_Orientation (const TopAbs_Orientation, const TopAbs_Orientation)",
                orientation,
                [
                    new BindingParameter(0, "theFirst", orientation, false),
                    new BindingParameter(1, "theSecond", orientation, false),
                ]),
            CreateScopeMethod(
                "topabs:reverse",
                "TopAbs::Reverse",
                "TopAbs::Reverse: TopAbs_Orientation (const TopAbs_Orientation)",
                orientation,
                [new BindingParameter(0, "theOrientation", orientation, false)]),
        ]);
        GenerationScopeConfiguration topAbsScope = new()
        {
            SourcePackage = "TopAbs",
            NativeNamePrefix = "TopAbs::",
            Header = "TopAbs.hxx",
            ExportNamePrefix = "top_abs",
            ManagedNamePrefix = "TopAbs",
        };

        GeneratedBindingSet result = InitialBindingEmitter.Emit(
            baseReport with { Model = model },
            [GenerationScopeConfiguration.Precision, topAbsScope]);

        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith(".cpp", StringComparison.Ordinal)
            && file.Content.Contains("occtsharp_generated_top_abs_static_compose_0", StringComparison.Ordinal));
        GeneratedFile managed = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("ScalarRaw.Generated.cs", StringComparison.Ordinal)
            && file.Content.Contains("TopAbsStaticCompose0", StringComparison.Ordinal));
        Assert.Contains("#include <TopAbs.hxx>", native.Content, StringComparison.Ordinal);
        Assert.Contains("occtsharp_generated_top_abs_static_compose_0", native.Content, StringComparison.Ordinal);
        Assert.Contains("static_cast<TopAbs_Orientation>(theFirst)", native.Content, StringComparison.Ordinal);
        Assert.Contains("TopAbsStaticCompose0", managed.Content, StringComparison.Ordinal);
        Assert.Contains("TopAbsStaticReverse0", managed.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void AllowsDistinctScopesWithinOneSourcePackage()
    {
        DiscoveryReport baseReport = CreateReport();
        BindingType real = CreateValueType("double", isConst: false);
        BindingModel model = new(
        [
            .. baseReport.Model.Declarations,
            CreateScopeMethod(
                "standard:allocator",
                "Standard::GetAllocatorType",
                "Standard::GetAllocatorType: double ()",
                real,
                [],
                "Standard",
                "Standard.hxx"),
            CreateScopeMethod(
                "standard:json",
                "Standard_Dump::JsonKeyLength",
                "Standard_Dump::JsonKeyLength: double ()",
                real,
                [],
                "Standard",
                "Standard_Dump.hxx"),
        ]);

        GeneratedBindingSet result = InitialBindingEmitter.Emit(
            baseReport with { Model = model },
            [
                new GenerationScopeConfiguration
                {
                    SourcePackage = "Standard",
                    NativeNamePrefix = "Standard::GetAllocatorType",
                    Header = "Standard.hxx",
                    ExportNamePrefix = "standard",
                    ManagedNamePrefix = "Standard",
                },
                new GenerationScopeConfiguration
                {
                    SourcePackage = "Standard",
                    NativeNamePrefix = "Standard_Dump::JsonKeyLength",
                    Header = "Standard_Dump.hxx",
                    ExportNamePrefix = "standard_dump",
                    ManagedNamePrefix = "StandardDump",
                },
            ]);

        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Foundation.Values.Generated.cpp", StringComparison.Ordinal));
        Assert.Contains("occtsharp_generated_standard_static_get_allocator_type_0", native.Content, StringComparison.Ordinal);
        Assert.Contains("occtsharp_generated_standard_dump_static_json_key_length_0", native.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitsStaticMethodsWithPointValueParameters()
    {
        DiscoveryReport baseReport = CreateReport();
        BindingType real = CreateValueType("double", isConst: false);
        BindingType point = CreateValueType("gp_Pnt", isConst: false);
        BindingModel model = new(
        [
            .. baseReport.Model.Declarations,
            CreateScopeMethod(
                "mesh:deflection",
                "BRepMesh_GeomTool::SquareDeflectionOfSegment",
                "BRepMesh_GeomTool::SquareDeflectionOfSegment: double (const gp_Pnt &, const gp_Pnt &, const gp_Pnt &)",
                real,
                [
                    new BindingParameter(0, "first", point, false),
                    new BindingParameter(1, "middle", point, false),
                    new BindingParameter(2, "last", point, false),
                ],
                "BRepMesh",
                "BRepMesh_GeomTool.hxx"),
        ]);

        GeneratedBindingSet result = InitialBindingEmitter.Emit(
            baseReport with { Model = model },
            [
                GenerationScopeConfiguration.Precision,
                new GenerationScopeConfiguration
                {
                    SourcePackage = "BRepMesh",
                    NativeNamePrefix = "BRepMesh_GeomTool::SquareDeflectionOfSegment",
                    Header = "BRepMesh_GeomTool.hxx",
                    ExportNamePrefix = "brep_mesh_geom_tool",
                    ManagedNamePrefix = "BRepMeshGeomTool",
                },
            ]);

        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Mesh.Values.Generated.cpp", StringComparison.Ordinal));
        GeneratedFile managed = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("Mesh.ScalarRaw.Generated.cs", StringComparison.Ordinal));
        Assert.Contains("gp_Pnt(first.x, first.y, first.z)", native.Content, StringComparison.Ordinal);
        Assert.Contains("Point3dRaw first", managed.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitsExactFreeFunctionScopeWithVoidReturn()
    {
        DiscoveryReport baseReport = CreateReport();
        BindingDeclaration function = new(
            "standard:assert-no-op",
            "Standard_ASSERT_DO_NOTHING",
            BindingDeclarationKind.Function,
            "Standard_Assert.hxx",
            1,
            1)
        {
            SourcePackage = "Standard",
            SourceToolkit = "TKernel",
            Access = BindingAccess.Public,
            ReturnType = CreateValueType("void", isConst: false),
        };
        BindingModel model = new([.. baseReport.Model.Declarations, function]);
        GenerationScopeConfiguration scope = new()
        {
            SourcePackage = "Standard",
            NativeNamePrefix = function.NativeName,
            Header = function.Header,
            ExportNamePrefix = "standard_assert",
            ManagedNamePrefix = "StandardAssert",
            ExactNativeName = true,
        };

        GeneratedBindingSet result = InitialBindingEmitter.Emit(
            baseReport with { Model = model },
            [GenerationScopeConfiguration.Precision, scope]);

        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Foundation.Values.Generated.cpp", StringComparison.Ordinal));
        GeneratedFile managed = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("Foundation.ScalarRaw.Generated.cs", StringComparison.Ordinal));
        Assert.Contains("return Standard_ASSERT_DO_NOTHING();", native.Content, StringComparison.Ordinal);
        Assert.Contains("internal static partial void StandardAssertStatic", managed.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReplacementRemovesOnlyManifestOwnedStaleFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), $"occtsharp-emitter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            GeneratedBindingSet initial = new(
                "8.0.1",
                ["first"],
                [
                    new GeneratedFile("src/OcctSharp/Generated/Current.Generated.cs", "current\n"),
                    new GeneratedFile("src/OcctSharp/Generated/Stale.Generated.cs", "stale\n"),
                ]);
            GeneratedOutputWriter.Replace(root, initial);
            string manualPath = Path.Combine(root, "src", "OcctSharp", "Generated", "Manual.cs");
            File.WriteAllText(manualPath, "manual");

            GeneratedBindingSet replacement = new(
                "8.0.1",
                ["first"],
                [new GeneratedFile("src/OcctSharp/Generated/Current.Generated.cs", "updated\n")]);
            GeneratedOutputWriter.Replace(root, replacement);

            Assert.False(File.Exists(Path.Combine(
                root, "src", "OcctSharp", "Generated", "Stale.Generated.cs")));
            Assert.True(File.Exists(manualPath));
            Assert.Equal(
                "updated\n",
                File.ReadAllText(Path.Combine(
                    root, "src", "OcctSharp", "Generated", "Current.Generated.cs")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ReplacementDoesNotRewriteByteIdenticalGeneratedFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), $"occtsharp-emitter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            GeneratedBindingSet bindingSet = new(
                "8.0.1",
                ["first"],
                [new GeneratedFile("src/OcctSharp/Generated/Current.Generated.cs", "current\n")]);
            GeneratedOutputWriter.Replace(root, bindingSet);
            string generatedPath = Path.Combine(
                root, "src", "OcctSharp", "Generated", "Current.Generated.cs");
            DateTime unchangedTimestamp = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(generatedPath, unchangedTimestamp);

            GeneratedOutputWriter.Replace(root, bindingSet);

            Assert.Equal(unchangedTimestamp, File.GetLastWriteTimeUtc(generatedPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static DiscoveryReport CreateReport()
    {
        BindingType coordinateType = CreateValueType("double", isConst: true);
        BindingModel model = new(
        [
            new BindingDeclaration(
                TestStableId,
                "gp_Pnt::gp_Pnt",
                BindingDeclarationKind.Constructor,
                "gp_Pnt.hxx",
                48,
                13)
            {
                Access = BindingAccess.Public,
                Parameters =
                [
                    new BindingParameter(0, "theXp", coordinateType, false),
                    new BindingParameter(1, "theYp", coordinateType, false),
                    new BindingParameter(2, "theZp", coordinateType, false),
                ],
            },
        ]);
        return new DiscoveryReport(
            "1.1",
            "8.0.1",
            "test",
            ["gp_Pnt.hxx"],
            [],
            model,
            BindingSupportSummary.Create(model));
    }

    private static BindingDeclaration CreatePrecisionMethod(
        string stableId,
        string nativeName,
        string signature,
        BindingType returnType,
        IReadOnlyList<BindingParameter> parameters)
    {
        return new BindingDeclaration(
            stableId,
            nativeName,
            BindingDeclarationKind.Method,
            "Precision.hxx",
            1,
            1)
        {
            NativeSignature = signature,
            SourcePackage = "Precision",
            Access = BindingAccess.Public,
            IsStatic = true,
            ReturnType = returnType,
            Parameters = parameters,
        };
    }

    private static BindingDeclaration CreateScopeMethod(
        string stableId,
        string nativeName,
        string signature,
        BindingType returnType,
        IReadOnlyList<BindingParameter> parameters,
        string sourcePackage = "TopAbs",
        string header = "TopAbs.hxx")
    {
        return new BindingDeclaration(
            stableId,
            nativeName,
            BindingDeclarationKind.Method,
            header,
            1,
            1)
        {
            NativeSignature = signature,
            SourcePackage = sourcePackage,
            Access = BindingAccess.Public,
            IsStatic = true,
            ReturnType = returnType,
            Parameters = parameters,
        };
    }

    private static BindingType CreateValueType(string nativeType, bool isConst)
    {
        string spelling = isConst ? $"const {nativeType}" : nativeType;
        return new BindingType(
            spelling,
            spelling,
            spelling,
            spelling,
            [new BindingTypeLayer(BindingTypeLayerKind.Value, isConst)],
            null,
            [],
            false,
            null);
    }
}
