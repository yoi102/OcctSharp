namespace OcctSharp;

#pragma warning disable CS1591

public enum FeatureGlueMode { Off = 0, Shift = 1, Full = 2 }
public enum FeatureBooleanOperation { Fuse = 0, Cut = 1, Common = 2, Section = 3 }
public enum FeatureHistoryKind { Modified = 0, Generated = 1 }

public sealed record FeatureModelingOptions
{
    public double FuzzyTolerance { get; init; }
    public bool RunParallel { get; init; }
    public bool NonDestructive { get; init; } = true;
    public FeatureGlueMode Glue { get; init; }
    public bool RepairInputs { get; init; }
    public bool UnifyResult { get; init; }
}

public sealed record ChamferSelection(Shape Edge, Shape SupportFace);
public sealed record PlanarChamferSelection(Shape FirstEdge, Shape SecondEdge);

public sealed record FeatureOperationDiagnostics(
    bool Succeeded,
    bool Recovered,
    int ErrorCount,
    int WarningCount,
    int FaultyShapeCount,
    bool ResultIsValid,
    string StageMessage);

public sealed record FeatureHistoryItem(int SourceIndex, FeatureHistoryKind Kind, Shape Shape) : IDisposable
{
    public void Dispose() => Shape.Dispose();
}

public sealed class FeatureOperationResult : IDisposable
{
    internal FeatureOperationResult(
        Shape? shape, FeatureOperationDiagnostics diagnostics,
        IReadOnlyList<FeatureHistoryItem> history, IReadOnlyList<int> deletedSourceIndices)
    {
        Shape = shape;
        Diagnostics = diagnostics;
        History = history;
        DeletedSourceIndices = deletedSourceIndices;
    }

    public Shape? Shape { get; }
    public FeatureOperationDiagnostics Diagnostics { get; }
    public IReadOnlyList<FeatureHistoryItem> History { get; }
    public IReadOnlyList<int> DeletedSourceIndices { get; }
    public IReadOnlyList<FeatureHistoryItem> Modified => History.Where(item => item.Kind == FeatureHistoryKind.Modified).ToArray();
    public IReadOnlyList<FeatureHistoryItem> Generated => History.Where(item => item.Kind == FeatureHistoryKind.Generated).ToArray();

    public Shape RequireShape()
    {
        if (!Diagnostics.Succeeded || Shape is null)
            throw new InvalidOperationException($"Feature operation failed: {Diagnostics.StageMessage}");
        return Shape;
    }

    public void Dispose()
    {
        Shape?.Dispose();
        foreach (FeatureHistoryItem item in History) item.Dispose();
    }
}

#pragma warning restore CS1591
