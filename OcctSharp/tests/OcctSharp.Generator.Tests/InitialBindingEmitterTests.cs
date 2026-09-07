using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class InitialBindingEmitterTests
{
    private const string TestStableId = "test:gp-pnt-three-coordinates";

    [Fact]
    public void AuditedReferencesPublishAllCopiesAfterSuccessAndRejectSignatureDrift()
    {
        DiscoveryReport basis = CreateReport();
        BindingType reference = new("double &", "double &", "double", "double",
            [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, false)], null, [], false, null);
        BindingDeclaration method = new("c:@S@gp_Pnt@F@Coord#&d#S0_#S0_#1", "gp_Pnt::Coord", BindingDeclarationKind.Method, "gp_Pnt.hxx", 1, 1)
        {
            NativeSignature = "gp_Pnt::Coord: void (double &, double &, double &) const noexcept",
            Access = BindingAccess.Public, IsConst = true, ProductModule = OcctProductModule.Geometry,
            ReturnType = CreateValueType("void", false),
            Parameters = [new(0, "x", reference, false), new(1, "y", reference, false), new(2, "z", reference, false)],
        };
        GeneratedBindingSet Emit(BindingDeclaration declaration) => InitialBindingEmitter.Emit(basis with
            { Model = new([..basis.Model.Declarations, declaration]) });
        var result = Emit(method);
        Assert.Contains(method.StableId, result.SourceStableIds);
        string native = Assert.Single(result.Files, f => f.RelativePath.EndsWith("ReferenceOperations.Generated.cpp", StringComparison.Ordinal)).Content;
        string managed = Assert.Single(result.Files, f => f.RelativePath.EndsWith("ReferenceOperations.Generated.cs", StringComparison.Ordinal)).Content;
        Assert.True(native.IndexOf("const auto copied2", StringComparison.Ordinal) < native.IndexOf("*updatedArg0 = copied0", StringComparison.Ordinal));
        Assert.True(managed.IndexOf("NativeError.ThrowIfFailed", StringComparison.Ordinal) < managed.IndexOf("arg0 = updatedArg0", StringComparison.Ordinal));
        Assert.Contains("ref double arg0, ref double arg1, ref double arg2", managed, StringComparison.Ordinal);
        Assert.Contains("return error.Status", native, StringComparison.Ordinal);
        Assert.DoesNotContain(method.StableId, Emit(method with { NativeSignature = method.NativeSignature + " changed" }).SourceStableIds);
        Assert.DoesNotContain(method.StableId, Emit(method with { IsConst = false }).SourceStableIds);
        Assert.DoesNotContain(method.StableId, Emit(method with { SupportState = BindingSupportState.Manual }).SourceStableIds);
        Assert.DoesNotContain("unlisted:reference", Emit(method with { StableId = "unlisted:reference" }).SourceStableIds);
    }

    [Theory]
    [InlineData("gp_Mat")]
    [InlineData("gp_Mat2d")]
    [InlineData("gp_GTrsf")]
    [InlineData("gp_GTrsf2d")]
    public void SingularInverseGuardsPrecedeKernelCalls(string owner)
    {
        DiscoveryReport basis = CreateReport();
        BindingDeclaration invert = new("z:inverse:" + owner, owner + "::Invert", BindingDeclarationKind.Method, owner + ".hxx", 1, 1)
        { Access = BindingAccess.Public, ReturnType = CreateValueType("void", false) };
        BindingDeclaration power = invert with { StableId = "z:power:" + owner, NativeName = owner + "::Power",
            Parameters = [new(0, "n", CreateValueType("int", false), false)] };
        GeneratedBindingSet result = InitialBindingEmitter.Emit(basis with { Model = new([..basis.Model.Declarations, invert, power]) });
        string native = Assert.Single(result.Files, f => f.RelativePath.EndsWith("ValueOperations.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("if (arg0 < 0 && value.IsSingular())", native, StringComparison.Ordinal);
        Assert.True(native.IndexOf("if (value.IsSingular())", StringComparison.Ordinal) < native.IndexOf("value.Invert();", StringComparison.Ordinal));
    }

    [Fact]
    public void ValueOperationsCopyReceiversAndPublishMutationsOnlyAfterSuccess()
    {
        DiscoveryReport basis = CreateReport();
        BindingDeclaration query = new("z:pnt-x", "gp_Pnt::X", BindingDeclarationKind.Method, "gp_Pnt.hxx", 1, 1)
        { Access = BindingAccess.Public, IsConst = true, ReturnType = CreateValueType("double", false) };
        BindingDeclaration mutate = query with { StableId = "z:pnt-set", NativeName = "gp_Pnt::SetX", IsConst = false,
            ReturnType = CreateValueType("void", false), Parameters = [new(0, "x", CreateValueType("double", false), false)] };
        BindingDeclaration manual = mutate with { StableId = "z:manual", NativeName = "gp_Pnt::SetY", SupportState = BindingSupportState.Manual };
        BindingType pointer = new("double*", "double*", "double", "double", [new(BindingTypeLayerKind.PointerIndirection, false),new(BindingTypeLayerKind.Value,false)],null,[],false,null);
        BindingDeclaration unsafeCall = mutate with { StableId = "z:pointer", Parameters = [new(0,"p",pointer,false)] };
        GeneratedBindingSet result = InitialBindingEmitter.Emit(basis with { Model = new([..basis.Model.Declarations, query, mutate, manual, unsafeCall]) });
        Assert.Contains(query.StableId, result.SourceStableIds);
        Assert.Contains(mutate.StableId, result.SourceStableIds);
        Assert.DoesNotContain(manual.StableId, result.SourceStableIds);
        Assert.DoesNotContain(unsafeCall.StableId, result.SourceStableIds);
        string native = Assert.Single(result.Files, f => f.RelativePath.EndsWith("ValueOperations.Generated.cpp", StringComparison.Ordinal)).Content;
        string managed = Assert.Single(result.Files, f => f.RelativePath.EndsWith("ValueOperations.Generated.cs", StringComparison.Ordinal)).Content;
        Assert.Contains("const auto value = gp_Pnt(", native, StringComparison.Ordinal);
        Assert.Contains("*updated = {};", native, StringComparison.Ordinal);
        Assert.Contains("const auto copiedReceiver =", native, StringComparison.Ordinal);
        Assert.Contains("public static void SetX(ref Point3d receiver, double arg0)", managed, StringComparison.Ordinal);
        Assert.True(managed.IndexOf("NativeError.ThrowIfFailed", StringComparison.Ordinal) < managed.IndexOf("receiver = new Point3d", StringComparison.Ordinal));
    }

    [Fact]
    public void CheckedStaticParametersEscapeManagedKeywordsOnly()
    {
        DiscoveryReport basis = CreateReport();
        BindingDeclaration method = CreatePrecisionMethod("y:keyword", "Precision::Reference", "keyword parameter",
            CreateValueType("double", false), [new(0, "ref", CreateValueType("double", false), false)]);
        GeneratedBindingSet emitted = InitialBindingEmitter.Emit(basis with { Model = new BindingModel([.. basis.Model.Declarations, method]) });
        string managed = Assert.Single(emitted.Files, f => f.RelativePath.EndsWith("Foundation.ScalarRaw.Generated.cs", StringComparison.Ordinal)).Content;
        string native = Assert.Single(emitted.Files, f => f.RelativePath.EndsWith("Foundation.Values.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("double @ref", managed, StringComparison.Ordinal);
        Assert.Contains("Checked(@ref, out double", managed, StringComparison.Ordinal);
        Assert.Contains("double ref", native, StringComparison.Ordinal);
        Assert.DoesNotContain("@ref", native, StringComparison.Ordinal);
    }

    [Fact]
    public void NewScalarOverloadAppendsWithoutChangingExistingAbiAndContainsExceptions()
    {
        DiscoveryReport basis = CreateReport();
        BindingDeclaration oldMethod = CreatePrecisionMethod(
            "c:@S@Precision@F@PApproximation#d#S", "Precision::PApproximation", "Z existing double",
            CreateValueType("double", false), [new(0, "value", CreateValueType("double", false), false)]);
        BindingDeclaration newMethod = CreatePrecisionMethod(
            "x:new-float-overload", "Precision::PApproximation", "A new float sorts before old double",
            CreateValueType("double", false), [new(0, "value", CreateValueType("float", false), false)]);
        GeneratedBindingSet emitted = InitialBindingEmitter.Emit(basis with
        {
            Model = new BindingModel([.. basis.Model.Declarations, newMethod, oldMethod]),
        });
        string native = Assert.Single(emitted.Files, file => file.RelativePath.EndsWith("Foundation.Values.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("double OCCTSHARP_CALL occtsharp_generated_precision_static_p_approximation_0(", native, StringComparison.Ordinal);
        Assert.Contains("OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_precision_static_p_approximation_1(float value, double* generatedResult)", native, StringComparison.Ordinal);
        Assert.Contains("static_cast<float>(value)", native, StringComparison.Ordinal);
        Assert.Contains("catch (const Standard_Failure& error)", native, StringComparison.Ordinal);
        Assert.Contains("if (generatedResult == nullptr)", native, StringComparison.Ordinal);
        string managed = Assert.Single(emitted.Files, file => file.RelativePath.EndsWith("Foundation.ScalarRaw.Generated.cs", StringComparison.Ordinal)).Content;
        Assert.Contains("internal static partial double PrecisionStaticPApproximation0(double value)", managed, StringComparison.Ordinal);
        Assert.Contains("NativeError.ThrowIfFailed(PrecisionStaticPApproximation1Checked", managed, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitsDeterministicNativeAndManagedPointBinding()
    {
        DiscoveryReport report = CreateReport();

        GeneratedBindingSet first = InitialBindingEmitter.Emit(report);
        GeneratedBindingSet second = InitialBindingEmitter.Emit(report);

        Assert.Equal(first.OcctVersion, second.OcctVersion);
        Assert.Equal(first.SourceStableIds, second.SourceStableIds);
        Assert.Equal(first.Files, second.Files);
        Assert.Equal(7, first.Files.Count);
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
    public void GeometricRecordsHaveExplicitLayoutsAndPreserveFrameHandedness()
    {
        GeneratedBindingSet emitted = InitialBindingEmitter.Emit(CreateReport());
        string header = Assert.Single(emitted.Files, file => file.RelativePath.EndsWith("GeometryValues.Generated.h", StringComparison.Ordinal)).Content;
        string managed = Assert.Single(emitted.Files, file => file.RelativePath.EndsWith("GeometryValues.Generated.cs", StringComparison.Ordinal)).Content;
        Assert.Contains("static_assert(sizeof(OcctSharp_Value_Matrix3x3) == 72)", header, StringComparison.Ordinal);
        Assert.Contains("offsetof(OcctSharp_Value_Matrix3x3, M23) == 40", header, StringComparison.Ordinal);
        Assert.Contains("result.YReverse()", header, StringComparison.Ordinal);
        Assert.Contains("Inconsistent axis handedness", header, StringComparison.Ordinal);
        Assert.Contains("std::isfinite", header, StringComparison.Ordinal);
        Assert.Contains("gp_Mat2d(gp_XY(Check(value.M11), Check(value.M21)), gp_XY(Check(value.M12), Check(value.M22)))", header, StringComparison.Ordinal);
        Assert.Contains("[StructLayout(LayoutKind.Sequential)]", managed, StringComparison.Ordinal);
        Assert.Contains("public readonly record struct Torus(Axis3 Position, double MajorRadius, double MinorRadius)", managed, StringComparison.Ordinal);
        Assert.Single(emitted.SourceStableIds); // Support DTOs are not fake emitted native declarations.
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
                "c:@S@Precision@F@PApproximation#S",
                "Precision::PApproximation",
                "Precision::PApproximation: double ()",
                real,
                []),
            CreatePrecisionMethod(
                "c:@S@Precision@F@PApproximation#d#S",
                "Precision::PApproximation",
                "Precision::PApproximation: double (const double)",
                real,
                [new BindingParameter(0, "value", constReal, false)]),
            CreatePrecisionMethod(
                "c:@S@Precision@F@IsInfinite#d#S",
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
            "c:@F@Standard_ASSERT_DO_NOTHING#",
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
