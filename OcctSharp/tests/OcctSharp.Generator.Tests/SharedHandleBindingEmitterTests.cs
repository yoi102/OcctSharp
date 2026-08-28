using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class SharedHandleBindingEmitterTests
{
    [Fact]
    public void EmitsDeterministicTypedSharedHandleNativeAndManagedLayers()
    {
        BindingType real = Value("double");
        BindingType point = Value("gp_Pnt");
        BindingType voidType = Value("void");
        BindingType directionHandle = Handle("Geom_Direction");
        BindingModel model = new(
        [
            Declaration("ctor", "Geom_CartesianPoint::Geom_CartesianPoint", BindingDeclarationKind.Constructor) with
            {
                Parameters =
                [
                    new BindingParameter(0, "X", real, false),
                    new BindingParameter(1, "Y", real, false),
                    new BindingParameter(2, "Z", real, false),
                ],
            },
            Declaration("pnt", "Geom_CartesianPoint::Pnt", BindingDeclarationKind.Method) with
            {
                ReturnType = point,
                IsConst = true,
            },
            Declaration("set", "Geom_CartesianPoint::SetPnt", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
                Parameters = [new BindingParameter(0, "P", point, false)],
            },
            Declaration("direction", "Geom_CartesianPoint::Direction", BindingDeclarationKind.Method) with
            {
                ReturnType = directionHandle,
            },
            Declaration("set-direction", "Geom_CartesianPoint::SetDirection", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
                Parameters = [new BindingParameter(0, "Direction", directionHandle, false)],
            },
            Declaration("reserved-parameter", "Geom_CartesianPoint::SetOrientation", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
                Parameters = [new BindingParameter(0, "Or", real, false)],
            },
            Declaration("object-member", "Geom_CartesianPoint::GetType", BindingDeclarationKind.Method) with
            {
                ReturnType = real,
            },
            Declaration("value-return", "Geom_CartesianPoint::ValueAndResult", BindingDeclarationKind.Method) with
            {
                ReturnType = real,
                Parameters =
                [
                    new BindingParameter(0, "Value", real, false),
                    new BindingParameter(1, "ResultValue", real, false),
                ],
            },
            Declaration("macro-member", "Geom_CartesianPoint::MacroDefined", BindingDeclarationKind.Method) with
            {
                SourcePackage = "MacroInfrastructure",
                ReturnType = real,
            },
            Declaration("create-member", "Geom_CartesianPoint::Create", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
            },
            Declaration("clear-upper", "Geom_CartesianPoint::Clear", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
            },
            Declaration("clear-lower", "Geom_CartesianPoint::clear", BindingDeclarationKind.Method) with
            {
                ReturnType = voidType,
            },
            Declaration("macro-member-duplicate", "Geom_CartesianPoint::MacroDefined", BindingDeclarationKind.Method) with
            {
                SourcePackage = "MacroInfrastructure",
                ReturnType = real,
            },
            Declaration("direction-ctor", "Geom_Direction::Geom_Direction", BindingDeclarationKind.Constructor),
        ]);
        SharedHandleScopeConfiguration scope = new()
        {
            SourcePackage = "Geom",
            NativeType = "Geom_CartesianPoint",
            Header = "Geom_CartesianPoint.hxx",
            ExportNamePrefix = "geom_cartesian_point",
            ManagedTypeName = "GeomCartesianPoint",
        };
        SharedHandleScopeConfiguration directionScope = new()
        {
            SourcePackage = "Geom",
            NativeType = "Geom_Direction",
            Header = "Geom_Direction.hxx",
            ExportNamePrefix = "geom_direction",
            ManagedTypeName = "GeomDirection",
        };

        GeneratedBindingSet first = SharedHandleBindingEmitter.Emit(
            "8.0.1", model, [scope, directionScope], ["RWGltf_GltfPrimArrayData.hxx"]);
        GeneratedBindingSet second = SharedHandleBindingEmitter.Emit(
            "8.0.1", model, [scope, directionScope], ["RWGltf_GltfPrimArrayData.hxx"]);
        Assert.Throws<InvalidDataException>(() => SharedHandleBindingEmitter.Emit(
            "8.0.1", model, [scope, directionScope], ["duplicate.hxx", "duplicate.hxx"]));
        Assert.Throws<InvalidDataException>(() => SharedHandleBindingEmitter.Emit(
            "8.0.1", model, [scope, directionScope], ["unsafe\nheader.hxx"]));

        Assert.Equal(first.SourceStableIds, second.SourceStableIds);
        Assert.Equal(first.Files, second.Files);
        Assert.Equal(5, first.Files.Count);
        Assert.Equal(14, first.SourceStableIds.Count);
        Assert.Single(first.Files, static file =>
            file.Content.Contains("public readonly record struct Point3d", StringComparison.Ordinal));
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("SharedHandles.Generated.cpp", StringComparison.Ordinal)
            && file.Content.Contains("opencascade::handle<Geom_CartesianPoint>", StringComparison.Ordinal)
            && file.Content.IndexOf("#include <RWGltf_GltfPrimArrayData.hxx>", StringComparison.Ordinal)
                < file.Content.IndexOf("#include <Geom_CartesianPoint.hxx>", StringComparison.Ordinal)
            && file.Content.Contains("direction == nullptr ? opencascade::handle<Geom_Direction>() : ValidateGeomDirection(direction)->Value", StringComparison.Ordinal)
            && file.Content.Contains("AllocateGeomDirection(std::move(returnedHandle))", StringComparison.Ordinal)
            && file.Content.Contains("createdHandle = new Geom_CartesianPoint", StringComparison.Ordinal)
            && file.Content.Contains("occtsharp_generated_geom_cartesian_point_method_create_0", StringComparison.Ordinal)
            && file.Content.Contains("occtsharp_generated_geom_cartesian_point_method_clear_0", StringComparison.Ordinal)
            && file.Content.Contains("occtsharp_generated_geom_cartesian_point_method_clear_1", StringComparison.Ordinal)
            && file.Content.Contains("double or_value", StringComparison.Ordinal)
            && file.Content.Contains("GeneratedGuard", StringComparison.Ordinal));
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("SharedHandles.Generated.cs", StringComparison.Ordinal)
            && file.Content.Contains("public sealed class GeomCartesianPoint", StringComparison.Ordinal)
            && file.Content.Contains("public GeomDirection? Direction()", StringComparison.Ordinal)
            && file.Content.Contains("public void SetDirection(GeomDirection? direction)", StringComparison.Ordinal)
            && file.Content.Contains("public double OcctGetType()", StringComparison.Ordinal)
            && file.Content.Contains("public double ValueAndResult(double value, double resultValue)", StringComparison.Ordinal)
            && file.Content.Contains("out double resultValue2", StringComparison.Ordinal)
            && file.Content.Contains("return resultValue2;", StringComparison.Ordinal)
            && file.Content.Contains("public double MacroDefined()", StringComparison.Ordinal)
            && file.Content.Contains("public double MacroDefinedGenerated1()", StringComparison.Ordinal)
            && file.Content.Contains("public void Create()", StringComparison.Ordinal)
            && file.Content.Contains("public void Clear()", StringComparison.Ordinal)
            && file.Content.Contains("public void clear()", StringComparison.Ordinal)
            && file.Content.Contains("direction is null ? nint.Zero : direction.NativeHandle.DangerousGetHandle()", StringComparison.Ordinal)
            && file.Content.Contains("return global::OcctSharp.GeomDirection.FromNative", StringComparison.Ordinal)
            && file.Content.Contains("internal static GeomDirection? FromNative", StringComparison.Ordinal)
            && file.Content.Contains("public readonly record struct Point3d", StringComparison.Ordinal));
    }

    [Fact]
    public void RetainsPlacementAllocatorUntilGeneratedObjectRelease()
    {
        BindingType allocatorHandle = Handle("NCollection_IncAllocator");
        BindingModel model = new(
        [
            Declaration("allocator-ctor", "NCollection_IncAllocator::NCollection_IncAllocator", BindingDeclarationKind.Constructor) with
            {
                SourcePackage = "NCollection",
            },
            Declaration("curve-ctor", "BRepMeshData_Curve::BRepMeshData_Curve", BindingDeclarationKind.Constructor) with
            {
                SourcePackage = "BRepMesh",
                Parameters = [new BindingParameter(0, "theAllocator", allocatorHandle, false)],
            },
        ]);
        SharedHandleScopeConfiguration allocatorScope = new()
        {
            SourcePackage = "NCollection",
            NativeType = "NCollection_IncAllocator",
            Header = "NCollection_IncAllocator.hxx",
            ExportNamePrefix = "ncollection_inc_allocator",
            ManagedTypeName = "NCollectionIncAllocator",
        };
        SharedHandleScopeConfiguration curveScope = new()
        {
            SourcePackage = "BRepMesh",
            NativeType = "BRepMeshData_Curve",
            Header = "BRepMeshData_Curve.hxx",
            ExportNamePrefix = "brep_mesh_data_curve",
            ManagedTypeName = "BRepMeshDataCurve",
            UsesPlacementAllocator = true,
        };

        GeneratedBindingSet result = SharedHandleBindingEmitter.Emit(
            "8.0.1", model, [allocatorScope, curveScope]);

        GeneratedFile native = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Mesh.SharedHandles.Generated.cpp", StringComparison.Ordinal));
        GeneratedFile nativeHeader = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("OcctSharp.Mesh.SharedHandles.Generated.h", StringComparison.Ordinal));
        GeneratedFile managed = Assert.Single(result.Files, static file =>
            file.RelativePath.EndsWith("Mesh.SharedHandles.Generated.cs", StringComparison.Ordinal));
        Assert.Contains(
            "opencascade::handle<NCollection_IncAllocator> ConstructionAllocator;\n  opencascade::handle<BRepMeshData_Curve> Value;",
            nativeHeader.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "new (constructionAllocator) BRepMeshData_Curve(constructionAllocator)",
            native.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "AllocateBRepMeshDataCurve(std::move(createdHandle), std::move(constructionAllocator))",
            native.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "AllocateBRepMeshDataCurve(value->Value, value->ConstructionAllocator)",
            native.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "public BRepMeshDataCurve(NCollectionIncAllocator theAllocator)",
            managed.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "ArgumentNullException.ThrowIfNull(theAllocator);",
            managed.Content,
            StringComparison.Ordinal);
    }

    private static BindingDeclaration Declaration(
        string id,
        string name,
        BindingDeclarationKind kind) => new(
            id,
            name,
            kind,
            "Geom_CartesianPoint.hxx",
            1,
            1)
        {
            NativeSignature = name,
            SourcePackage = "Geom",
            Access = BindingAccess.Public,
            SupportState = BindingSupportState.Supported,
        };

    private static BindingType Value(string name) => new(
        name,
        name,
        name,
        name,
        [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
        null,
        [],
        false,
        null);

    private static BindingType Handle(string targetType) => new(
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        $"opencascade::handle<{targetType}>",
        [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
        "opencascade::handle",
        [],
        true,
        targetType);
}
