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

/// <summary>A copied point plus first and second 3D derivatives evaluated on an edge.</summary>
public readonly record struct CurveDerivativeEvaluation(
    double Parameter,
    GpPoint Point,
    GpPoint FirstDerivative,
    GpPoint SecondDerivative);

/// <summary>A copied two-dimensional point or vector in a face UV parameter space.</summary>
public readonly record struct GpPoint2d(double X, double Y);

/// <summary>Copied bounds and endpoints for an edge pcurve on a specific face.</summary>
public readonly record struct PcurveSnapshot(
    double FirstParameter,
    double LastParameter,
    GpPoint2d StartPoint,
    GpPoint2d EndPoint);

/// <summary>A copied UV point and unit tangent evaluated on an edge pcurve.</summary>
public readonly record struct PcurveEvaluation(
    double Parameter,
    GpPoint2d Point,
    GpPoint2d Tangent);

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

/// <summary>A copied surface point, U/V derivatives, and oriented unit normal.</summary>
public readonly record struct SurfaceDerivativeEvaluation(
    double UParameter,
    double VParameter,
    GpPoint Point,
    GpPoint UDerivative,
    GpPoint VDerivative,
    GpPoint Normal);

/// <summary>The nearest copied point projection on a bounded face surface.</summary>
public readonly record struct SurfaceProjection(
    double UParameter,
    double VParameter,
    GpPoint Point,
    double Distance,
    int SolutionCount);
