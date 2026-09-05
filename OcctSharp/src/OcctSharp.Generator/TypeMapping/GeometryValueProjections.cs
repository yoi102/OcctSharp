using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.TypeMapping;

internal sealed record GeometryValueField(string Name, string Type, string Accessor);
internal sealed record GeometryValueProjection(string NativeType, string Name, GeometryValueField[] Fields, string Construct)
{
    internal string AbiType => "OcctSharp_Value_" + Name;
    internal string ManagedType => "global::OcctSharp.Values." + Name;
}

internal static class GeometryValueProjections
{
    private static GeometryValueField D(string name, string? accessor = null) => new(name, "double", accessor ?? name + "()");
    private static GeometryValueField V(string name, string type, string accessor) => new(name, type, accessor);
    private static GeometryValueProjection Coordinates(string native, string name, bool three) => new(
        native, name, three ? [D("X"), D("Y"), D("Z")] : [D("X"), D("Y")],
        "return " + native + "(Check(value.X), Check(value.Y)" + (three ? ", Check(value.Z)" : "") + ");");
    private static GeometryValueProjection Primitive(string native, string name, string axis, string accessor, params GeometryValueField[] scalars) => new(
        native, name, [V("Position", axis, accessor), .. scalars],
        "return " + native + "(ToNative(value.Position)" + string.Concat(scalars.Select(field => ", Check(value." + field.Name + ")")) + ");");

    // Dependency order is also the C record/overload declaration order.
    internal static readonly GeometryValueProjection[] All =
    [
        Coordinates("gp_XY", "Coordinates2d", false),
        Coordinates("gp_XYZ", "Coordinates3d", true),
        Coordinates("gp_Pnt2d", "Point2d", false),
        Coordinates("gp_Vec2d", "Vector2d", false),
        Coordinates("gp_Vec", "Vector3d", true),
        Direction("gp_Dir2d", "Direction2d", false),
        Direction("gp_Dir", "Direction3d", true),
        new("gp_Ax1", "Axis1", [V("Origin", "Coordinates3d", "Location().XYZ()"), V("Direction", "Direction3d", "Direction()")],
            "return gp_Ax1(gp_Pnt(ToNative(value.Origin)), ToNative(value.Direction));"),
        new("gp_Ax2", "Axis2", FrameFields(), FrameConstruction("gp_Ax2", false)),
        new("gp_Ax3", "Axis3", FrameFields(), FrameConstruction("gp_Ax3", true)),
        new("gp_Ax2d", "Axis2d", [V("Origin", "Point2d", "Location()"), V("Direction", "Direction2d", "Direction()")],
            "return gp_Ax2d(ToNative(value.Origin), ToNative(value.Direction));"),
        new("gp_Ax22d", "Axis22d", [V("Origin", "Point2d", "Location()"), V("XDirection", "Direction2d", "XDirection()"), V("YDirection", "Direction2d", "YDirection()")],
            "const auto x = ToNative(value.XDirection); const auto y = ToNative(value.YDirection);\n"
            + "if (std::abs(x.Dot(y)) > 1e-12) throw Standard_DomainError(\"Axis directions must be orthogonal.\");\n"
            + "return gp_Ax22d(ToNative(value.Origin), x, y);"),
        Matrix("gp_Mat2d", "Matrix2x2", 2),
        Matrix("gp_Mat", "Matrix3x3", 3),
        new("gp_Quaternion", "Quaternion", [D("X"), D("Y"), D("Z"), D("W")],
            "return gp_Quaternion(Check(value.X), Check(value.Y), Check(value.Z), Check(value.W));"),
        Primitive("gp_Lin2d", "Line2d", "Axis2d", "Position()"),
        Primitive("gp_Lin", "Line3d", "Axis1", "Position()"),
        Primitive("gp_Circ2d", "Circle2d", "Axis22d", "Axis()", D("Radius")),
        Primitive("gp_Circ", "Circle3d", "Axis2", "Position()", D("Radius")),
        Primitive("gp_Elips2d", "Ellipse2d", "Axis22d", "Axis()", D("MajorRadius"), D("MinorRadius")),
        Primitive("gp_Elips", "Ellipse3d", "Axis2", "Position()", D("MajorRadius"), D("MinorRadius")),
        Primitive("gp_Hypr2d", "Hyperbola2d", "Axis22d", "Axis()", D("MajorRadius"), D("MinorRadius")),
        Primitive("gp_Hypr", "Hyperbola3d", "Axis2", "Position()", D("MajorRadius"), D("MinorRadius")),
        Primitive("gp_Parab2d", "Parabola2d", "Axis22d", "Axis()", D("FocalLength", "Focal()")),
        Primitive("gp_Parab", "Parabola3d", "Axis2", "Position()", D("FocalLength", "Focal()")),
        Primitive("gp_Pln", "Plane", "Axis3", "Position()"),
        Primitive("gp_Cylinder", "Cylinder", "Axis3", "Position()", D("Radius")),
        Primitive("gp_Cone", "Cone", "Axis3", "Position()", D("SemiAngle"), D("ReferenceRadius", "RefRadius()")),
        Primitive("gp_Sphere", "Sphere", "Axis3", "Position()", D("Radius")),
        Primitive("gp_Torus", "Torus", "Axis3", "Position()", D("MajorRadius"), D("MinorRadius")),
    ];

    internal static bool TryMap(string nativeType, out BindingTypeProjection? projection)
    {
        GeometryValueProjection? value = Array.Find(All, item => item.NativeType == nativeType);
        projection = value is null ? null : new BindingTypeProjection(
            "TM009", value.AbiType, value.ManagedType, value.ManagedType, "ValueCopy",
            "Explicit accessor/constructor geometric value copy; no C++ layout or borrowed storage crosses the ABI.");
        return value is not null;
    }

    private static GeometryValueField[] FrameFields() =>
    [
        V("Origin", "Coordinates3d", "Location().XYZ()"), V("Normal", "Direction3d", "Direction()"),
        V("XDirection", "Direction3d", "XDirection()"), V("YDirection", "Direction3d", "YDirection()"),
    ];

    private static GeometryValueProjection Direction(string native, string name, bool three) => new(
        native, name, three ? [D("X"), D("Y"), D("Z")] : [D("X"), D("Y")],
        "const double x = Check(value.X), y = Check(value.Y)" + (three ? ", z = Check(value.Z)" : "") + ";\n"
        + "if (!(std::hypot(x, y" + (three ? ", z" : "") + ") > gp::Resolution())) throw Standard_DomainError(\"Direction magnitude is too small.\");\n"
        + "const double scale = " + (three ? "std::fmax(std::fmax(std::abs(x), std::abs(y)), std::abs(z))" : "std::fmax(std::abs(x), std::abs(y))") + ";\n"
        + "return " + native + "(x / scale, y / scale" + (three ? ", z / scale" : "") + ");");

    private static string FrameConstruction(string native, bool indirect) =>
        "const auto z = ToNative(value.Normal); const auto x = ToNative(value.XDirection); const auto y = ToNative(value.YDirection);\n"
        + "if (std::abs(z.Dot(x)) > 1e-12) throw Standard_DomainError(\"Axis directions must be orthogonal.\");\n"
        + native + " result(gp_Pnt(ToNative(value.Origin)), z, x);\n"
        + (indirect ? "if (result.YDirection().Dot(y) < 0) result.YReverse();\n" : "")
        + "if (!result.YDirection().IsEqual(y, 1e-12)) throw Standard_DomainError(\"Inconsistent axis handedness or Y direction.\");\n"
        + "return result;";

    private static GeometryValueProjection Matrix(string native, string name, int dimension)
    {
        GeometryValueField[] fields = Enumerable.Range(1, dimension).SelectMany(row =>
            Enumerable.Range(1, dimension).Select(column => D($"M{row}{column}", $"Value({row}, {column})"))).ToArray();
        string construct = dimension == 2
            ? "return gp_Mat2d(gp_XY(Check(value.M11), Check(value.M21)), gp_XY(Check(value.M12), Check(value.M22)));"
            : "return " + native + "(" + string.Join(", ", fields.Select(field => "Check(value." + field.Name + ")")) + ");";
        return new(native, name, fields, construct);
    }
}
