namespace OcctSharp;

/// <summary>Copied finite world-space oriented bounds with no native lifetime.</summary>
public readonly record struct OrientedBoundingBox3d(
    GpPoint Center,
    GpXyz XDirection,
    GpXyz YDirection,
    GpXyz ZDirection,
    double HalfSizeX,
    double HalfSizeY,
    double HalfSizeZ)
{
    /// <summary>Gets the full size along the local X axis.</summary>
    public double SizeX => HalfSizeX * 2;

    /// <summary>Gets the full size along the local Y axis.</summary>
    public double SizeY => HalfSizeY * 2;

    /// <summary>Gets the full size along the local Z axis.</summary>
    public double SizeZ => HalfSizeZ * 2;
}
