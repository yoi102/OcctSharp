using OcctSharp.Generated;
using OcctSharp.Interop;
using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed partial class BatchXProjectionTests
{
    [Fact]
    public void NewStaticCallRejectsNullOutputAndClearsSharedErrorOnSuccess()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(BatchXProjectionTests).Assembly);
        Assert.Equal(NativeStatus.InvalidArgument, ConvertHalfWithOutputPointer(1, nint.Zero));
        Assert.Contains("output is null", Marshal.PtrToStringUTF8(RuntimeNativeMethods.GetLastError()), StringComparison.Ordinal);
        Assert.Equal((ushort)0x3c00, VisualizationGeneratedNativeMethods.ImagePixMapStaticConvertToHalfFloat0(1));
        Assert.Equal(string.Empty, Marshal.PtrToStringUTF8(RuntimeNativeMethods.GetLastError()));
    }

    [Fact]
    public void AppendedFloatOverloadKeepsExistingDoubleOverloadCallable()
    {
        const double input = 0.125;
        double oldValue = FoundationGeneratedNativeMethods.QuantityColorStaticConvert_LinearRGB_To_sRGB0(input);
        float newValue = FoundationGeneratedNativeMethods.QuantityColorStaticConvert_LinearRGB_To_sRGB1((float)input);
        Assert.InRange(Math.Abs(oldValue - newValue), 0, 0.000001);
        Assert.InRange(oldValue, 0.3885, 0.3887);
    }

    [Theory]
    [InlineData(-5_000_000_000L, -5000d)]
    [InlineData(5_000_000_000L, 5000d)]
    public void Signed64BitTimeValuesPreserveSignAndExceed32BitRange(long value, double seconds) =>
        Assert.Equal(seconds, VisualizationGeneratedNativeMethods.MediaFormatContextStaticFormatUnitsToSeconds0(value));

    [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_generated_image_pix_map_convert_to_half_float_static_convert_to_half_float_0")]
    private static partial NativeStatus ConvertHalfWithOutputPointer(float value, nint result);

    [Fact]
    public void GeneratedProjectionWorkflowSurvivesSourceDisposal() =>
        OcctSharp.Tests.Shared.BatchXProjectionWorkflow.Run();

    [Theory]
    [InlineData(0f, 0)]
    [InlineData(1f, 0x3c00)]
    [InlineData(-2f, 0xc000)]
    [InlineData(65504f, 0x7bff)]
    public void FloatAndUnsignedShortHalfConversionPreservesKnownBitPatterns(float value, int bits)
    {
        ushort half = VisualizationGeneratedNativeMethods.ImagePixMapStaticConvertToHalfFloat0(value);
        Assert.Equal((ushort)bits, half);
        Assert.Equal(value, VisualizationGeneratedNativeMethods.ImagePixMapStaticConvertFromHalfFloat0(half));
    }

    [Fact]
    public void StaticPointReferenceReturnsCoordinates()
    {
        Point3dRaw origin = GeometryGeneratedNativeMethods.GpStaticOrigin0();
        Assert.Equal(0, origin.X);
        Assert.Equal(0, origin.Y);
        Assert.Equal(0, origin.Z);
    }

    [Fact]
    public void CopiedPointReferenceDoesNotFollowLaterMutation()
    {
        using AISTextLabel label = new();
        label.SetPosition(new Point3d(1, 2, 3));
        Point3d copied = label.Position();
        label.SetPosition(new Point3d(9, 8, 7));
        Assert.Equal(new Point3d(1, 2, 3), copied);
        Assert.Equal(new Point3d(9, 8, 7), label.Position());
        label.Dispose();
        Assert.Equal(new Point3d(1, 2, 3), copied);
        Assert.Throws<ObjectDisposedException>(() => label.Position());
    }

    [Fact]
    public void UnsignedCameraCountersAdvanceWithoutLosingProjectionState()
    {
        using Graphic3dCamera camera = new();
        ulong before = camera.ProjectionState();
        camera.SetScale(2);
        Assert.NotEqual(before, camera.ProjectionState());
        Assert.Equal(2, camera.Scale());
        camera.Dispose();
        Assert.Throws<ObjectDisposedException>(() => camera.WorldViewState());
    }
}
