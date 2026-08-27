namespace OcctSharp;

/// <summary>Identifies the OCCT geometry family behind an edge curve adaptor.</summary>
public enum CurveGeometryType
{
    /// <summary>A straight line.</summary>
    Line = 0,
    /// <summary>A circle.</summary>
    Circle = 1,
    /// <summary>An ellipse.</summary>
    Ellipse = 2,
    /// <summary>A hyperbola.</summary>
    Hyperbola = 3,
    /// <summary>A parabola.</summary>
    Parabola = 4,
    /// <summary>A Bezier curve.</summary>
    BezierCurve = 5,
    /// <summary>A B-spline curve.</summary>
    BSplineCurve = 6,
    /// <summary>An offset curve.</summary>
    OffsetCurve = 7,
    /// <summary>A curve outside the recognized analytic and spline families.</summary>
    OtherCurve = 8,
}

/// <summary>Identifies the OCCT geometry family behind a face surface adaptor.</summary>
public enum SurfaceGeometryType
{
    /// <summary>A plane.</summary>
    Plane = 0,
    /// <summary>A cylindrical surface.</summary>
    Cylinder = 1,
    /// <summary>A conical surface.</summary>
    Cone = 2,
    /// <summary>A spherical surface.</summary>
    Sphere = 3,
    /// <summary>A toroidal surface.</summary>
    Torus = 4,
    /// <summary>A Bezier surface.</summary>
    BezierSurface = 5,
    /// <summary>A B-spline surface.</summary>
    BSplineSurface = 6,
    /// <summary>A surface of revolution.</summary>
    SurfaceOfRevolution = 7,
    /// <summary>A surface of extrusion.</summary>
    SurfaceOfExtrusion = 8,
    /// <summary>An offset surface.</summary>
    OffsetSurface = 9,
    /// <summary>A surface outside the recognized analytic and spline families.</summary>
    OtherSurface = 10,
}

/// <summary>A caller-owned value snapshot of an edge's adapted 3D curve.</summary>
public readonly record struct EdgeCurveSnapshot(
    CurveGeometryType CurveType,
    double FirstParameter,
    double LastParameter,
    GpPoint StartPoint,
    GpPoint EndPoint);

/// <summary>A caller-owned value snapshot of a face's adapted surface and UV bounds.</summary>
public readonly record struct FaceSurfaceSnapshot(
    SurfaceGeometryType SurfaceType,
    double FirstUParameter,
    double LastUParameter,
    double FirstVParameter,
    double LastVParameter);

/// <summary>A copied point and unit tangent evaluated on an edge curve.</summary>
public readonly record struct CurveEvaluation(double Parameter, GpPoint Point, GpPoint Tangent);

/// <summary>The nearest copied point projection on an edge curve.</summary>
public readonly record struct CurveProjection(
    double Parameter,
    GpPoint Point,
    double Distance,
    int SolutionCount);

/// <summary>A copied point and oriented unit normal evaluated on a face surface.</summary>
public readonly record struct SurfaceEvaluation(
    double UParameter,
    double VParameter,
    GpPoint Point,
    GpPoint Normal);

/// <summary>The nearest copied point projection on a bounded face surface.</summary>
public readonly record struct SurfaceProjection(
    double UParameter,
    double VParameter,
    GpPoint Point,
    double Distance,
    int SolutionCount);
