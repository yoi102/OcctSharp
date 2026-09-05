using OcctSharp.Interop;
using System.Security.Cryptography;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>An immutable, owning diagnostic snapshot. Indices are valid only for its identity and revision.</summary>
public sealed class RepairSnapshot : IDisposable
{
    private readonly Shape shape;
    private bool disposed;
    internal Shape Shape { get { ThrowIfDisposed(); return shape; } }
    public RepairIdentity Identity { get; }
    public string Fingerprint { get; }
    /// <summary>Caller-declared linear unit. All thresholds must use this same unit; areas/volumes use its powers.</summary>
    public string Unit { get; }
    public RepairInspectionOptions Options { get; }
    public IReadOnlyList<RepairTopologyItem> Topology { get; }
    public IReadOnlyList<RepairFinding> Findings { get; }
    public IReadOnlyList<RepairToleranceDistribution> Tolerances { get; }
    public RepairMetrics Metrics { get; }

    private unsafe RepairSnapshot(Shape owned, string unit, RepairIdentity identity, RepairInspectionOptions options)
    {
        shape = owned; Unit = unit; Identity = identity; Options = options;
        Fingerprint = ComputeFingerprint(owned);
        NativeError.ThrowIfFailed(NativeMethods.RepairTopology(owned.Handle, null, 0, out int count), "repair_topology_count");
        RepairTopologyRaw[] raw = new RepairTopologyRaw[count];
        fixed (RepairTopologyRaw* values = raw)
            NativeError.ThrowIfFailed(NativeMethods.RepairTopology(owned.Handle, values, count, out count), "repair_topology");
        Topology = Array.AsReadOnly(raw.Select(value => new RepairTopologyItem(new(identity, value.Index), (ShapeKind)value.Kind,
            value.Orientation, value.ParentIndex < 0 ? null : value.ParentIndex,
            value.Kind is 4 or 6 or 7 ? value.Tolerance : null)).ToArray());
        RepairInspectionRaw controls = new(options.Tolerance, options.SmallLength, options.SmallArea, options.ToleranceOutlier);
        NativeError.ThrowIfFailed(NativeMethods.RepairInspect(owned.Handle, in controls, out RepairMetricsRaw metrics, null, 0, out count), "repair_inspect_count");
        RepairFindingRaw[] findings = new RepairFindingRaw[count];
        fixed (RepairFindingRaw* values = findings)
            NativeError.ThrowIfFailed(NativeMethods.RepairInspect(owned.Handle, in controls, out metrics, values, count, out count), "repair_inspect");
        Metrics = Convert(metrics);
        Findings = Array.AsReadOnly(findings.Select(value => Convert(value, identity)).ToArray());
        Tolerances = Array.AsReadOnly(Topology.Where(value => value.Tolerance.HasValue).GroupBy(value => value.Kind)
            .Select(group => new RepairToleranceDistribution(group.Key, group.Count(), group.Min(value => value.Tolerance!.Value),
                group.Max(value => value.Tolerance!.Value), group.Average(value => value.Tolerance!.Value),
                Array.AsReadOnly(group.Where(value => value.Tolerance > options.ToleranceOutlier).Select(value => value.Selection).ToArray()))).ToArray());
    }

    public static RepairSnapshot Create(Shape source, string unit = "mm", long revision = 0, RepairInspectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        ArgumentOutOfRangeException.ThrowIfNegative(revision);
        NativeError.ThrowIfFailed(NativeMethods.RepairCopy(source.Handle, out nint copy), "repair_copy");
        return Own(ShapeFactory.FromNativeHandle(copy, "repair_copy"), unit, new(Guid.NewGuid(), revision), options ?? new());
    }
    internal static RepairSnapshot Own(Shape owned, string unit, RepairIdentity identity, RepairInspectionOptions options)
    {
        try { return new(owned, unit, identity, options); }
        catch { owned.Dispose(); throw; }
    }
    public RepairSelection Select(int index)
    {
        ThrowIfDisposed(); if ((uint)index >= Topology.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return new(Identity, index);
    }
    public Shape CopyShape()
    {
        ThrowIfDisposed(); NativeError.ThrowIfFailed(NativeMethods.RepairCopy(shape.Handle, out nint copy), "repair_copy");
        return ShapeFactory.FromNativeHandle(copy, "repair_copy");
    }
    public Shape CopySubshape(RepairSelection selection)
    {
        Validate(selection); NativeError.ThrowIfFailed(NativeMethods.RepairSubshape(shape.Handle, selection.Index, out nint copy), "repair_subshape");
        return ShapeFactory.FromNativeHandle(copy, "repair_subshape");
    }
    public unsafe IReadOnlyList<RepairFreeBoundary> ExtractFreeBoundaries()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.RepairBoundary(shape.Handle, Options.Tolerance, -1, out _, null, 0, out _, out int count, out _), "repair_boundary_count");
        List<RepairFreeBoundary> result = [];
        try
        {
            for (int index = 0; index < count; ++index)
            {
                NativeError.ThrowIfFailed(NativeMethods.RepairBoundary(shape.Handle, Options.Tolerance, index,
                    out _, null, 0, out int edgeCount, out _, out _), "repair_boundary_edges");
                int[] indices = new int[Math.Max(1, edgeCount)];
                fixed (int* edges = indices)
                {
                    NativeError.ThrowIfFailed(NativeMethods.RepairBoundary(shape.Handle, Options.Tolerance, index,
                        out RepairBoundaryRaw info, edges, indices.Length, out edgeCount, out _, out nint wire), "repair_boundary");
                    Shape owned = ShapeFactory.FromNativeHandle(wire, "repair_boundary");
                    try
                    {
                        result.Add(new(owned, info.Closed != 0, info.Length, info.AreaAvailable != 0 ? info.Area : null,
                            info.EndpointGap >= 0 ? info.EndpointGap : null,
                            Array.AsReadOnly(indices.Take(edgeCount).Where(value => value >= 0).Select(Select).ToArray())));
                    }
                    catch { owned.Dispose(); throw; }
                }
            }
            return result.AsReadOnly();
        }
        catch { foreach (RepairFreeBoundary boundary in result) boundary.Dispose(); throw; }
    }
    internal void Validate(RepairSelection selection)
    {
        ThrowIfDisposed();
        if (selection.Source != Identity) throw new ArgumentException("Foreign or stale repair selection.", nameof(selection));
        if ((uint)selection.Index >= Topology.Count) throw new ArgumentOutOfRangeException(nameof(selection));
    }
    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
    internal static unsafe string ComputeFingerprint(Shape shape)
    {
        shape.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count), "repair_identity_count");
        byte[] data = new byte[count];
        fixed (byte* buffer = data)
            NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, buffer, data.Length, out count), "repair_identity");
        return System.Convert.ToHexString(SHA256.HashData(data));
    }
    internal static RepairMetrics Convert(RepairMetricsRaw value) => new(value.Valid != 0, value.TopologyCount,
        value.MaximumTolerance, value.AreaAvailable != 0 ? value.Area : null, value.VolumeAvailable != 0 ? value.Volume : null, value.MaximumGap);
    internal static RepairFinding Convert(RepairFindingRaw value, RepairIdentity identity) => new((RepairFindingKind)value.Kind,
        value.SourceIndex >= 0 ? new(identity, value.SourceIndex) : null, value.RelatedIndex >= 0 ? new(identity, value.RelatedIndex) : null,
        value.Status, value.Value, value.Limit);
    public void Dispose() { if (disposed) return; disposed = true; shape.Dispose(); }
}
