using OcctSharp.Interop;
using OcctSharp.Generated;

namespace OcctSharp.Runtime.Tests;

public sealed class BSplineKnotBuffersTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void FlatBezierKnotsCopiesWholeStaticSequence(int degree)
    {
        double[] first = BSplineKnotBuffers.FlatBezierKnots(degree);
        Assert.Equal(2 * (degree + 1), first.Length);
        Assert.All(first.Take(degree + 1), value => Assert.Equal(0, value));
        Assert.All(first.Skip(degree + 1), value => Assert.Equal(1, value));

        first[0] = 42;
        Assert.Equal(0, BSplineKnotBuffers.FlatBezierKnots(degree)[0]);
    }

    [Fact]
    public unsafe void InvalidDegreeAndBuffersNeverPublishPartialOutput()
    {
        int maxDegree = GeometryGeneratedNativeMethods.BSplCLibStaticMaxDegree0();
        Assert.Equal(2 * (maxDegree + 1), BSplineKnotBuffers.FlatBezierKnots(maxDegree).Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => BSplineKnotBuffers.FlatBezierKnots(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => BSplineKnotBuffers.FlatBezierKnots(maxDegree + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => BSplineKnotBuffers.FlatBezierKnots(int.MaxValue));

        double[] values = Enumerable.Repeat(-7.0, 8).ToArray();
        fixed (double* buffer = values)
        {
            Assert.Equal(NativeStatus.InvalidArgument, BSplineKnotInterop.FlatBezierKnots(2, buffer, 5, out int shortWritten));
            Assert.Equal(0, shortWritten);
            Assert.All(values, value => Assert.Equal(-7, value));

            Assert.Equal(NativeStatus.InvalidArgument, BSplineKnotInterop.FlatBezierKnots(0, buffer, values.Length, out int invalidWritten));
            Assert.Equal(0, invalidWritten);
            Assert.Equal(NativeStatus.InvalidArgument, BSplineKnotInterop.FlatBezierKnots(maxDegree + 1, buffer, values.Length, out int highWritten));
            Assert.Equal(0, highWritten);
            Assert.Equal(NativeStatus.InvalidArgument, BSplineKnotInterop.FlatBezierKnots(2, null, values.Length, out int nullWritten));
            Assert.Equal(0, nullWritten);
        }
    }
}
