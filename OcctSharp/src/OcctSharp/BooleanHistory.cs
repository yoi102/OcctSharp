namespace OcctSharp;

/// <summary>Identifies one supported Boolean operation.</summary>
public enum BooleanOperationKind
{
    /// <summary>Union.</summary>
    Fuse = 0,
    /// <summary>Subtraction.</summary>
    Cut = 1,
    /// <summary>Intersection.</summary>
    Common = 2,
}

/// <summary>Copied history counts for one Boolean input and one tracked topology kind.</summary>
public readonly record struct BooleanHistorySideSummary(
    int SourceCount,
    int ModifiedSourceCount,
    int GeneratedSourceCount,
    int DeletedSourceCount,
    int ModifiedResultCount,
    int GeneratedResultCount);

/// <summary>Copied Boolean history summary for both inputs.</summary>
public readonly record struct BooleanHistorySummary(
    ShapeKind TrackedKind,
    BooleanHistorySideSummary Left,
    BooleanHistorySideSummary Right);

/// <summary>Owns a Boolean result shape and its copied history summary.</summary>
public sealed class BooleanOperationResult : IDisposable
{
    internal BooleanOperationResult(BooleanOperationKind operation, Shape shape, BooleanHistorySummary history)
    {
        Operation = operation;
        Shape = shape;
        History = history;
    }

    /// <summary>Gets the operation that produced the result.</summary>
    public BooleanOperationKind Operation { get; }

    /// <summary>Gets the independently owned result topology.</summary>
    public Shape Shape { get; }

    /// <summary>Gets copied source/change/deletion counts.</summary>
    public BooleanHistorySummary History { get; }

    /// <summary>Releases the result topology.</summary>
    public void Dispose()
    {
        Shape.Dispose();
        GC.SuppressFinalize(this);
    }
}
