using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Tests;

public sealed class BindingModelTests
{
    [Fact]
    public void DeclarationsAreSortedByStableId()
    {
        BindingModel model = new(
        [
            new("record:z", "z", BindingDeclarationKind.Record, "z.hxx", 1, 1),
            new("record:a", "a", BindingDeclarationKind.Record, "a.hxx", 1, 1),
        ]);

        Assert.Equal(["record:a", "record:z"], model.Declarations.Select(static item => item.StableId));
    }

    [Fact]
    public void DuplicateStableIdsAreCollapsedDeterministically()
    {
        BindingModel model = new(
        [
            new("record:a", "a", BindingDeclarationKind.Record, "z.hxx", 20, 1),
            new("record:a", "a", BindingDeclarationKind.Record, "a.hxx", 10, 1),
        ]);

        BindingDeclaration declaration = Assert.Single(model.Declarations);
        Assert.Equal("a.hxx", declaration.Header);
    }
}
