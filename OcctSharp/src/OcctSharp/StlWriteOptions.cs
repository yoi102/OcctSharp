namespace OcctSharp;

/// <summary>Controls triangulation and encoding for STL output.</summary>
public sealed record StlWriteOptions(
    double LinearDeflection,
    double AngularDeflection,
    bool Binary)
{
    /// <summary>Gets practical defaults for the samples.</summary>
    public static StlWriteOptions Default { get; } = new(0.1, 0.5, true);
}
