using OcctSharp.Generated;

namespace OcctSharp;

/// <summary>Immutable value view of the OCCT <c>gp_Pnt</c> geometry primitive.</summary>
public readonly record struct GpPoint(double X, double Y, double Z)
{
    /// <summary>Creates a point through the generated <c>gp_Pnt</c> value-copy export.</summary>
    public static GpPoint Create(double x, double y, double z)
    {
        ValidateFinite(x, nameof(x)); ValidateFinite(y, nameof(y)); ValidateFinite(z, nameof(z));
        Point3dRaw raw = GeneratedNativeMethods.CreatePoint3d(x, y, z);
        return new GpPoint(raw.X, raw.Y, raw.Z);
    }

    /// <summary>Returns the generated OCCT default point value (the origin).</summary>
    public static GpPoint Origin
    {
        get { Point3dRaw raw = GeneratedNativeMethods.CreatePoint3dDefault(); return new GpPoint(raw.X, raw.Y, raw.Z); }
    }

    /// <summary>Copies this value through the generated <c>gp_Pnt</c> copy constructor.</summary>
    public GpPoint Copy()
    {
        Point3dRaw raw = GeneratedNativeMethods.CreatePoint3dCopy(new Point3dRaw(X, Y, Z));
        return new GpPoint(raw.X, raw.Y, raw.Z);
    }

    /// <summary>Computes Euclidean distance using the value-copy coordinates.</summary>
    public double DistanceTo(GpPoint other)
    {
        double dx = X - other.X, dy = Y - other.Y, dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static void ValidateFinite(double value, string name)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name, "The coordinate must be finite.");
    }
}
