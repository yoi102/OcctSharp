namespace OcctSharp;

/// <summary>Explicit bounded-envelope subtraction; never models an infinite-space complement.</summary>
public sealed class BoundedVoidPlan : IDisposable
{
    private readonly PartitionPlan plan;
    private readonly RegionExpression expression;
    private BoundedVoidPlan(PartitionPlan plan, RegionExpression expression) { this.plan = plan; this.expression = expression; }
    /// <summary>Captures one valid solid envelope and one or more occupied regions.</summary>
    public static BoundedVoidPlan Create(Shape envelope, IReadOnlyList<Shape> occupied, PartitionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(envelope); ArgumentNullException.ThrowIfNull(occupied);
        if (envelope.Kind != ShapeKind.Solid || !envelope.IsValid || occupied.Count is < 1 or > 127)
            throw new ArgumentException("Voids require one valid solid envelope and one to 127 occupied inputs.");
        var taken = RegionExpression.Input(1);
        for (int i = 2; i <= occupied.Count; i++) taken = taken.Union(RegionExpression.Input(i));
        return new(PartitionPlan.Create(new[] { envelope }.Concat(occupied).ToArray(), options), RegionExpression.Input(0).Except(taken));
    }
    /// <summary>Builds the 'voids' output; occupied portions outside the envelope are excluded by the expression.</summary>
    public PartitionResult Build() => plan.Build([new("voids", [new(expression, 1)], removeInternalBoundaries: true, makeContainers: true)]);
    /// <summary>Releases the private input plan.</summary>
    public void Dispose() => plan.Dispose();
}
