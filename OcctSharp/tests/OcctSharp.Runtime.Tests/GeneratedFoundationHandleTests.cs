namespace OcctSharp.Runtime.Tests;

public sealed class GeneratedFoundationHandleTests
{
    [Fact]
    public void GeneratedAsciiAndExtendedStringsRetainAndCopyNativeState()
    {
        using TCollectionHAsciiString ascii = new(42);
        Assert.Equal(42, ascii.IntegerValue());
        Assert.Equal(2, ascii.Length());
        Assert.True(ascii.IsIntegerValue());

        using TCollectionHAsciiString asciiClone = ascii.Clone();
        Assert.Equal(2, ascii.ReferenceCount);
        Assert.Equal(2, asciiClone.ReferenceCount);

        using TCollectionHExtendedString extended = new(ascii);
        Assert.Equal(2, extended.Length());
        Assert.True(extended.IsAscii());

        ascii.Dispose();
        Assert.Equal(42, asciiClone.IntegerValue());
        Assert.Throws<ObjectDisposedException>(() => ascii.IntegerValue());
    }

    [Fact]
    public void ReturnOnlyTransientScopesExposeLifetimeWithoutFakeConstructors()
    {
        Assert.Empty(typeof(StandardTransient).GetConstructors());
        Assert.Contains(typeof(StandardTransient).GetMethods(), method => method.Name == nameof(StandardTransient.Clone));
    }
}
