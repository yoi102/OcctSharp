namespace OcctSharp;

/// <summary>Defines a rigid rotation about the origin followed by a translation.</summary>
public readonly record struct ShapeTransform(
    double TranslationX,
    double TranslationY,
    double TranslationZ,
    double RotationAxisX,
    double RotationAxisY,
    double RotationAxisZ,
    double RotationAngleRadians)
{
    /// <summary>Gets the identity transform.</summary>
    public static ShapeTransform Identity { get; } = new(0, 0, 0, 0, 0, 1, 0);

    /// <summary>Creates a translation without rotation.</summary>
    public static ShapeTransform CreateTranslation(double x, double y, double z) =>
        new(x, y, z, 0, 0, 1, 0);

    /// <summary>Creates a Z-axis rotation in degrees followed by a translation.</summary>
    public static ShapeTransform CreateTranslationAndRotationZ(
        double x,
        double y,
        double z,
        double angleDegrees) =>
        new(x, y, z, 0, 0, 1, angleDegrees * Math.PI / 180.0);

    /// <summary>Converts this compatibility record to an owned OCCT transformation value.</summary>
    public GpTrsf ToGpTrsf() => GpTrsf.Create(
        TranslationX,
        TranslationY,
        TranslationZ,
        RotationAxisX,
        RotationAxisY,
        RotationAxisZ,
        RotationAngleRadians);
}
