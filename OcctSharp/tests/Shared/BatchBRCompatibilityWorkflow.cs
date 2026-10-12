namespace OcctSharp.Tests.Shared;

internal static class BatchBRCompatibilityWorkflow
{
    internal static void Run()
    {
        double[] knots = BSplineKnotBuffers.FlatBezierKnots(3);
        ReadOnlySpan<double> expected = [0, 0, 0, 0, 1, 1, 1, 1];
        if (!knots.AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("The packaged compatibility adapter did not copy the complete cubic Bezier knot sequence.");
        knots[0] = 42;
        if (BSplineKnotBuffers.FlatBezierKnots(3)[0] != 0)
            throw new InvalidOperationException("A managed buffer mutation changed the SDK static sequence.");
        try
        {
            _ = BSplineKnotBuffers.FlatBezierKnots(0);
            throw new InvalidOperationException("The packaged compatibility adapter accepted degree zero.");
        }
        catch (ArgumentOutOfRangeException) { }
        Console.WriteLine("BR compatible generated knot adapter PASS: complete sequence, detached copy, invalid degree.");
    }
}
