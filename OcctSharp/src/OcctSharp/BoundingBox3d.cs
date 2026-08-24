namespace OcctSharp;

/// <summary>Represents copied finite axis-aligned bounds with no native lifetime.</summary>
public readonly record struct BoundingBox3d(GpPoint Minimum, GpPoint Maximum)
{
    /// <summary>Gets the size along the X axis.</summary>
    public double SizeX => Maximum.X - Minimum.X;

    /// <summary>Gets the size along the Y axis.</summary>
    public double SizeY => Maximum.Y - Minimum.Y;

    /// <summary>Gets the size along the Z axis.</summary>
    public double SizeZ => Maximum.Z - Minimum.Z;
}
