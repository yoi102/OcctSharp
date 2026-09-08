using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchABOwnedWorkflow
{
    internal static void Run()
    {
        using BndBox box = BndBox.Create();
        box.Add(new Point3d(-1, -2, -3));
        box.Add(new Point3d(4, 5, 6));
        if (!box.Contains(new Point3d(0, 0, 0))) throw new InvalidOperationException("Owned box lost its bounds.");
        using BndBox copy = box.FinitePart();
        box.Dispose();
        if (copy.CornerMax() != new Point3d(4, 5, 6)) throw new InvalidOperationException("Owned result must survive source disposal.");
        using MathDirectPolynomialRoots roots = MathDirectPolynomialRoots.Create(1, -5, 6);
        if (!roots.IsDone() || roots.NbSolutions() != 2 || Math.Abs(roots.Value(1) * roots.Value(2) - 6) > 1e-9)
            throw new InvalidOperationException("Polynomial roots are incorrect.");
        using IntAna2dAnaIntersection intersection = IntAna2dAnaIntersection.Create(
            Line2dOperations.Create(new V.Axis2d(new V.Point2d(0, 0), new V.Direction2d(1, 0))),
            Line2dOperations.Create(new V.Axis2d(new V.Point2d(2, -1), new V.Direction2d(0, 1))));
        using IntAna2dIntPoint point = intersection.Point(1);
        intersection.Dispose();
        if (point.Value() != new V.Point2d(2, 0)) throw new InvalidOperationException("Copied intersection point is incorrect.");
        using GCMakeCircle maker = GCMakeCircle.Create(Axis2Operations.Create(), 3);
        if (!maker.IsDone()) throw new InvalidOperationException("Circle construction failed.");
        using GeomCircle circle = maker.Value()!;
        maker.Dispose();
        if (circle.Radius() != 3) throw new InvalidOperationException("Shared result did not retain its curve.");
    }
}
