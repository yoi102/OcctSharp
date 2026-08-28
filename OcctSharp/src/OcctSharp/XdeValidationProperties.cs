namespace OcctSharp;

/// <summary>Copied XDE validation-property values; each field may be absent in external documents.</summary>
public readonly record struct XdeValidationProperties
{
    /// <summary>Creates a copied snapshot; null fields represent absent XCAF attributes.</summary>
    public XdeValidationProperties(double? area, double? volume, GpPoint? centroid)
    {
        if (area is double areaValue && (!double.IsFinite(areaValue) || areaValue < 0))
            throw new ArgumentOutOfRangeException(nameof(area), "Area must be finite and non-negative.");
        if (volume is double volumeValue && (!double.IsFinite(volumeValue) || volumeValue < 0))
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be finite and non-negative.");
        if (centroid is GpPoint point
            && (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z)))
            throw new ArgumentOutOfRangeException(nameof(centroid), "Centroid coordinates must be finite.");
        Area = area;
        Volume = volume;
        Centroid = centroid;
    }

    /// <summary>Gets the stored surface area, when present.</summary>
    public double? Area { get; }
    /// <summary>Gets the stored volume, when present.</summary>
    public double? Volume { get; }
    /// <summary>Gets the stored centroid, when present.</summary>
    public GpPoint? Centroid { get; }
    /// <summary>Gets whether at least one validation property is present.</summary>
    public bool HasAny => Area.HasValue || Volume.HasValue || Centroid.HasValue;
    /// <summary>Gets whether area, volume, and centroid are all present.</summary>
    public bool IsComplete => Area.HasValue && Volume.HasValue && Centroid.HasValue;
}
