namespace OcctSharp;

#pragma warning disable CS1591
public sealed record RepairVolumeSource(int VolumeInput, RepairIdentity RepairedSnapshot, IReadOnlyList<RepairHistoryRelation> RepairHistory);
public sealed class RepairVolumeResult : IDisposable
{
    internal RepairVolumeResult(VolumeConstructionResult result, RepairVolumeSource[] sources) { Result = result; Sources = Array.AsReadOnly(sources); }
    public VolumeConstructionResult Result { get; }
    /// <summary>Exact input-level chain. Subshape mapping is available only where the separate copied repair and volume histories supply it.</summary>
    public IReadOnlyList<RepairVolumeSource> Sources { get; }
    public void Dispose() => Result.Dispose();
}
public static class RepairToVolume
{
    /// <summary>Consumes all Q acceptances only after a valid volume result is built; failure leaves previews unconsumed.</summary>
    public static RepairVolumeResult Build(IReadOnlyList<RepairPreview> previews, VolumeConstructionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(previews);
        if (previews.Count is < 1 or > 512 || previews.Distinct().Count() != previews.Count) throw new ArgumentException("Require distinct repair previews.");
        List<Shape> inputs = []; VolumeConstructionResult? result = null;
        try
        {
            foreach (var preview in previews) { ArgumentNullException.ThrowIfNull(preview); preview.EnsureAcceptable(); inputs.Add(preview.Result!.CopyShape()); }
            using var plan = VolumeConstructionPlan.Create(inputs, options); result = plan.Build();
            using var accepted = result.CopyResult();
            if (result.Volumes.Count == 0) throw new InvalidOperationException("The repaired inputs did not produce any bounded volume.");
            var sources = previews.Select((p, i) => new RepairVolumeSource(i, p.Result!.Identity, Array.AsReadOnly(p.History.ToArray()))).ToArray();
            foreach (var preview in previews) preview.MarkAccepted();
            var output = new RepairVolumeResult(result, sources); result = null; return output;
        }
        finally { result?.Dispose(); foreach (var shape in inputs) shape.Dispose(); }
    }
}
