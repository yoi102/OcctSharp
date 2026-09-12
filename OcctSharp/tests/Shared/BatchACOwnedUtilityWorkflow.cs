using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchACOwnedUtilityWorkflow
{
    internal static void Run()
    {
        using QuantityColor color = QuantityColor.Create(0.2, 0.4, 0.6, QuantityTypeOfColor.Quantity_TOC_RGB);
        using QuantityColorRGBA rgba = QuantityColorRGBA.Create(color, 0.75f);
        color.Dispose();
        using QuantityColor copied = rgba.GetRGB();
        rgba.Dispose();
        if (Math.Abs(copied.Green() - 0.4) > 1e-6) throw new InvalidOperationException("Owned color copy lost its value.");

        V.Circle2d circle = Circle2dOperations.Create(Axis2dOperations.Create(), 2, true);
        using ConvertCircleToBSplineCurve conversion = ConvertCircleToBSplineCurve.Create(
            circle, ConvertParameterisationType.Convert_TgtThetaOver2);
        V.Point2d pole = conversion.Pole(1);
        if (conversion.Degree() != 2 || Math.Abs(pole.X * pole.X + pole.Y * pole.Y - 4) > 1e-9)
            throw new InvalidOperationException("Analytic spline conversion produced invalid poles.");

        using IntrvInterval whole = IntrvInterval.Create(0, 10);
        using IntrvInterval removed = IntrvInterval.Create(3, 7);
        using IntrvIntervals intervals = IntrvIntervals.Create(whole);
        intervals.Subtract(removed);
        if (intervals.NbIntervals() != 2) throw new InvalidOperationException("Interval subtraction lost a component.");
        using IntrvInterval right = intervals.Value(2);
        intervals.Dispose();
        whole.Dispose();
        removed.Dispose();
        if (right.Start() != 7 || right.End() != 10) throw new InvalidOperationException("Interval result did not survive its inputs.");
    }
}
