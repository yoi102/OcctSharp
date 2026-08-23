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
        ]);
        SharedHandleScopeConfiguration scope = new()
        {
            SourcePackage = "Geom",
            NativeType = "Geom_CartesianPoint",
            Header = "Geom_CartesianPoint.hxx",
            ExportNamePrefix = "geom_cartesian_point",
            ManagedTypeName = "GeomCartesianPoint",
        };

        GeneratedBindingSet first = SharedHandleBindingEmitter.Emit("8.0.1", model, [scope]);
        GeneratedBindingSet second = SharedHandleBindingEmitter.Emit("8.0.1", model, [scope]);

        Assert.Equal(first.SourceStableIds, second.SourceStableIds);
        Assert.Equal(first.Files, second.Files);
        Assert.Equal(4, first.Files.Count);
        Assert.Equal(3, first.SourceStableIds.Count);
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("SharedHandles.Generated.cpp", StringComparison.Ordinal)
            && file.Content.Contains("opencascade::handle<Geom_CartesianPoint>", StringComparison.Ordinal)
            && file.Content.Contains("GeneratedGuard", StringComparison.Ordinal));
        Assert.Contains(first.Files, static file =>
            file.RelativePath.EndsWith("SharedHandles.Generated.cs", StringComparison.Ordinal)
            && file.Content.Contains("public sealed class GeomCartesianPoint", StringComparison.Ordinal)
            && file.Content.Contains("public readonly record struct Point3d", StringComparison.Ordinal));
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
}
