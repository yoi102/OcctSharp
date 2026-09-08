using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class OwnedValueOperationEmitterTests
{
    [Fact]
    public void MultipleOwnersShareOneRegistryAndHaveUniqueOutputFiles()
    {
        var model = new BindingModel(new[] { Record("Bnd_Box"), Record("Bnd_Range"), Constructor("Bnd_Box"), Constructor("Bnd_Range") });
        var result = OwnedValueOperationEmitter.Emit("8.0.1", model, [], new HashSet<string>(), []);
        Assert.Equal(2, result.SourceStableIds.Count);
        Assert.Equal(5, result.Files.Count);
        Assert.Equal(result.Files.Count, result.Files.Select(f => f.RelativePath).Distinct(StringComparer.Ordinal).Count());
        string native = result.Files.Single(f => f.RelativePath.EndsWith("Geometry.OwnedValues.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("std::make_shared<Bnd_Box>()", native, StringComparison.Ordinal);
        Assert.Contains("std::make_shared<Bnd_Range>()", native, StringComparison.Ordinal);
        Assert.Contains("catch (const Standard_Failure&", native, StringComparison.Ordinal);
        Assert.Contains("catch (...)", native, StringComparison.Ordinal);
        string managed = result.Files.Single(f => f.RelativePath.EndsWith(".cs", StringComparison.Ordinal)).Content;
        Assert.Contains("#nullable enable", managed, StringComparison.Ordinal);
        Assert.Contains("public void Dispose()", managed, StringComparison.Ordinal);
        Assert.DoesNotContain("DangerousGetHandle", managed, StringComparison.Ordinal);
    }

    [Fact]
    public void AuditedIdsRemainExcludedWhenAlreadyEmittedOrManual()
    {
        var box = Constructor("Bnd_Box");
        var range = Constructor("Bnd_Range") with { SupportState = BindingSupportState.Manual };
        var model = new BindingModel(new[] { Record("Bnd_Box"), Record("Bnd_Range"), box, range });
        var result = OwnedValueOperationEmitter.Emit("8.0.1", model, [], new HashSet<string> { box.StableId }, []);
        Assert.Empty(result.SourceStableIds);
        Assert.Empty(result.Files);
    }

    private static BindingDeclaration Record(string native) => new("record:" + native, native,
        BindingDeclarationKind.Record, native + ".hxx", 1, 1) { ProductModule = OcctProductModule.Geometry };
    private static BindingDeclaration Constructor(string native) => new("c:@S@" + native + "@F@" + native + "#",
        native + "::" + native, BindingDeclarationKind.Constructor, native + ".hxx", 1, 1)
    {
        NativeSignature = native + "::" + native + ": void ()" + (native == "Bnd_Box" ? " noexcept" : ""),
        Access = BindingAccess.Public, ProductModule = OcctProductModule.Geometry,
    };
}
