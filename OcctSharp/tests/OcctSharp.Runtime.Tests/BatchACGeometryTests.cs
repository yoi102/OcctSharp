using OcctSharp.Interop;
using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchACGeometryTests
{
    static BatchACGeometryTests() => NativeLibraryResolver.EnsureRegistered(typeof(BatchACGeometryTests).Assembly);

    [Fact]
    public void ColorInputsAndReturnedColorsAreIndependentOwners()
    {
        using QuantityColor input = QuantityColor.Create(0.2, 0.4, 0.6, QuantityTypeOfColor.Quantity_TOC_RGB);
        using QuantityColorRGBA rgba = QuantityColorRGBA.Create(input, 0.75f);
        input.SetValues(1, 0, 0, QuantityTypeOfColor.Quantity_TOC_RGB);
        input.Dispose();

        using QuantityColor first = rgba.GetRGB();
        first.SetValues(0, 1, 0, QuantityTypeOfColor.Quantity_TOC_RGB);
        using QuantityColor second = rgba.GetRGB();
        Assert.Equal(0.75f, rgba.Alpha());
        rgba.Dispose();

        Assert.Equal(0.2, second.Red(), 6);
        Assert.Equal(0.4, second.Green(), 6);
        Assert.Equal(0.6, second.Blue(), 6);
        Assert.True(first.IsDifferent(second));
    }

    [Fact]
    public void ColorOwnerArgumentsRejectNullAndDisposedHandles()
    {
        using QuantityColor left = QuantityColor.Create();
        using QuantityColor right = QuantityColor.Create();
        right.Dispose();
        Assert.Throws<ObjectDisposedException>(() => left.Distance(right));
        Assert.Throws<ObjectDisposedException>(() => QuantityColorRGBA.Create(right, 1));
        Assert.Throws<ArgumentNullException>(() => left.Distance(null!));
        Assert.Throws<OcctException>(() => QuantityColor.Create(-1, 0, 0, QuantityTypeOfColor.Quantity_TOC_RGB));
        Assert.True(left.Red() >= 0);
    }

    [Fact]
    public void DateAndPeriodResultsSurviveAllInputDisposals()
    {
        using QuantityDate date = QuantityDate.Create(1, 31, 2024, 12, 0, 0, 0, 0);
        using QuantityPeriod day = QuantityPeriod.Create(86_400, 0);
        using QuantityDate next = date.Add(day);
        using QuantityPeriod difference = next.Difference(date);
        date.Dispose();
        day.Dispose();
        Assert.Equal((2024, 2, 1, 12), (next.Year(), next.Month(), next.Day(), next.Hour()));
        next.Dispose();
        int seconds = -1, micros = -1;
        difference.Values(ref seconds, ref micros);
        Assert.Equal((86_400, 0), (seconds, micros));
    }

    [Fact]
    public void InitializedMatricesComposeThroughConstOwnerInputsAndScalarAccess()
    {
        using MathMatrix diagonal = MathMatrix.Create(1, 2, 1, 2, 0);
        diagonal.SetDiag(3);
        using MathMatrix result = MathMatrix.Create(1, 2, 1, 2, 1);
        result.Multiply(diagonal);
        diagonal.Dispose();
        Assert.Equal(3, result.Value(1, 1));
        Assert.Equal(3, result.Value(1, 2));
        result.Init(0);
        result.SetDiag(4);
        Assert.Equal(16, result.Determinant(), 12);
        result.Invert();
        Assert.Equal(0.25, result.Value(1, 1), 12);
        Assert.Equal(0, result.Value(1, 2));
        result.Dispose();
        Assert.Throws<ObjectDisposedException>(() => result.Value(1, 1));
    }

    [Fact]
    public void MatrixDimensionFailureDoesNotPublishAnInvalidOperation()
    {
        using MathMatrix square = MathMatrix.Create(1, 2, 1, 2, 2);
        using MathMatrix incompatible = MathMatrix.Create(1, 3, 1, 3, 1);
        Assert.Throws<OcctException>(() => square.Add(incompatible));
        Assert.Equal(2, square.Value(1, 1));
        using MathMatrix singular = MathMatrix.Create(1, 2, 1, 2, 0);
        Assert.Throws<OcctException>(() => singular.Invert());
    }

    [Fact]
    public void AnalyticCircleConversionExposesOwnedSplineData()
    {
        V.Circle2d circle = Circle2dOperations.Create(Axis2dOperations.Create(), 2, true);
        using ConvertCircleToBSplineCurve conversion = ConvertCircleToBSplineCurve.Create(
            circle, ConvertParameterisationType.Convert_TgtThetaOver2);
        Assert.Equal(2, conversion.Degree());
        Assert.True(conversion.NbPoles() >= 5);
        Assert.True(conversion.IsPeriodic());
        V.Point2d first = conversion.Pole(1);
        Assert.Equal(4, first.X * first.X + first.Y * first.Y, 10);
        for (int i = 1; i <= conversion.NbPoles(); i++) Assert.True(conversion.Weight(i) > 0);
        Assert.True(conversion.Knot(conversion.NbKnots()) > conversion.Knot(1));
        conversion.Dispose();
        Assert.Equal(4, first.X * first.X + first.Y * first.Y, 10);
        using ConvertCircleToBSplineCurve half = ConvertCircleToBSplineCurve.Create(
            circle, 0, Math.PI, ConvertParameterisationType.Convert_TgtThetaOver2);
        Assert.False(half.IsPeriodic());
        Assert.Equal(2, half.Pole(1).X, 10);
        V.Point2d end = half.Pole(half.NbPoles());
        Assert.Equal(-2, end.X, 10);
        Assert.Equal(0, end.Y, 10);
    }

    [Fact]
    public void AnalyticProjectionPublishesCopiedResultsAndRejectsInvalidGeometry()
    {
        V.Plane plane = PlaneOperations.Create();
        V.Line3d line = Line3dOperations.Create(new Point3d(1, 2, 3), new V.Direction3d(1, 0, 0));
        using ProjLibPlane projection = ProjLibPlane.Create(plane, line);
        Assert.True(projection.IsDone());
        Assert.Equal(GeomAbsCurveType.GeomAbs_Line, projection.GetType());
        V.Line2d projected = projection.Line();
        Assert.Equal(1, projected.Position.Origin.X, 12);
        Assert.Equal(2, projected.Position.Origin.Y, 12);
        Assert.Throws<OcctException>(() => projection.Project(default(V.Line3d)));
        Assert.Equal(projected, projection.Line());
        projection.Project(Circle3dOperations.Create(Axis2Operations.Create(), 3));
        Assert.True(projection.IsDone());
        Assert.Equal(GeomAbsCurveType.GeomAbs_Circle, projection.GetType());
        Assert.Equal(3, projection.Circle().Radius, 12);
    }

    [Fact]
    public void FairCurveAndBezierConversionRetainResultsAcrossOwnerDisposal()
    {
        using FairCurveBatten batten = FairCurveBatten.Create(new V.Point2d(0, 0), new V.Point2d(10, 0), 1, 0);
        FairCurveAnalysisCode code = FairCurveAnalysisCode.FairCurve_NotConverged;
        Assert.True(batten.Compute(ref code, 100, 1e-6));
        Assert.Equal(FairCurveAnalysisCode.FairCurve_OK, code);
        Assert.Throws<OcctException>(() => batten.SetHeight(-1));
        Assert.Equal(1, batten.GetHeight());
        using Geom2dBSplineCurve curve = batten.Curve()!;
        Assert.NotNull(curve);
        batten.Dispose();
        using Geom2dConvertBSplineCurveToBezierCurve conversion = Geom2dConvertBSplineCurveToBezierCurve.Create(curve);
        curve.Dispose();
        Assert.True(conversion.NbArcs() > 0);
        using Geom2dBezierCurve arc = conversion.Arc(1)!;
        conversion.Dispose();
        V.Point2d start = arc.Pole(1);
        Assert.Equal(0, start.X, 8);
        Assert.Equal(0, start.Y, 8);
    }

    [Fact]
    public void TrigonometricFunctionPublishesBothScalarOutputs()
    {
        using MathTrigonometricEquationFunction function = MathTrigonometricEquationFunction.Create(0, 0, 1, 0, 0);
        double value = 17, derivative = 19;
        Assert.True(function.Values(Math.PI / 2, ref value, ref derivative));
        Assert.Equal(0, value, 12);
        Assert.Equal(-1, derivative, 12);
        Assert.True(function.Value(0, ref value));
        Assert.Equal(1, value, 12);
    }

    [Fact]
    public void HatchingTrimsAnOwnedLineAndPublishesBoundaryMetadata()
    {
        using HatchHatcher hatcher = HatchHatcher.Create(1e-9, true);
        hatcher.AddYLine(0);
        hatcher.Trim(new V.Point2d(-2, -1), new V.Point2d(2, -1), 1);
        hatcher.Trim(new V.Point2d(2, -1), new V.Point2d(2, 1), 2);
        hatcher.Trim(new V.Point2d(2, 1), new V.Point2d(-2, 1), 3);
        hatcher.Trim(new V.Point2d(-2, 1), new V.Point2d(-2, -1), 4);
        Assert.Equal(1, hatcher.NbLines());
        Assert.Equal(1, hatcher.NbIntervals(1));
        Assert.Equal(-2, hatcher.Start(1, 1), 12);
        Assert.Equal(2, hatcher.End(1, 1), 12);
        int index = -1;
        double parameter = -1;
        hatcher.StartIndex(1, 1, ref index, ref parameter);
        Assert.Equal(4, index);
        hatcher.EndIndex(1, 1, ref index, ref parameter);
        Assert.Equal(2, index);
    }
}
