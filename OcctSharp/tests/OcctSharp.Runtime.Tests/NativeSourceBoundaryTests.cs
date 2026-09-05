using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class NativeSourceBoundaryTests
{
    [Fact]
    public void ManualAndGeneratedTopologyShareRegistrationAndRelease()
    {
        using Shape source = ShapeFactory.CreateBox(2, 3, 4);
        using Shape reversed = source.Reversed();
        source.Dispose();
        Assert.Equal(6, reversed.FaceCount);

        nint released = reversed.Handle.DangerousGetHandle();
        reversed.Dispose();
        NativeMethods.ReleaseShape(released);

        using ShapeHandle stale = new(released);
        NativeStatus status = NativeMethods.GetFaceCount(stale, out _);
        stale.SetHandleAsInvalid();
        Assert.Equal(NativeStatus.InvalidHandle, status);
        Assert.Contains("already released", LastError(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DifferentNativeDomainsUseOneDiagnosticBuffer()
    {
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.CreateBox(-1, 2, 3, out nint invalidBox));
        Assert.Equal(nint.Zero, invalidBox);
        Assert.Contains("Box dimensions", LastError(), StringComparison.Ordinal);

        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.CreateRealSequence(nint.Zero, -1, out nint invalidSequence));
        Assert.Equal(nint.Zero, invalidSequence);
        string collectionError = LastError();
        Assert.NotEmpty(collectionError);
        Assert.DoesNotContain("Box dimensions", collectionError, StringComparison.Ordinal);

        using Shape shape = ShapeFactory.CreateBox(1, 2, 3);
        Assert.Empty(LastError());
        Assert.Equal(6, shape.FaceCount);
        Assert.Empty(LastError());
    }

    [Fact]
    public void SharedDiagnosticStorageRemainsThreadLocal()
    {
        using Barrier barrier = new(2);
        Exception?[] failures = new Exception?[2];
        Thread[] workers = Enumerable.Range(0, 2).Select(index => new Thread(() =>
        {
            try
            {
                NativeStatus status = index == 0
                    ? NativeMethods.CreateBox(-1, 2, 3, out _)
                    : NativeMethods.CreateRealSequence(nint.Zero, -1, out _);
                Assert.Equal(NativeStatus.InvalidArgument, status);
                string ownError = LastError();
                Assert.NotEmpty(ownError);
                Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));
                Assert.Equal(ownError, LastError());

                // A successful Guard on one thread must not erase the other thread's error.
                if (index == 0)
                {
                    using Shape shape = ShapeFactory.CreateBox(1, 1, 1);
                    Assert.Empty(LastError());
                }
                Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));
                Assert.Equal(index == 0 ? string.Empty : ownError, LastError());
            }
            catch (Exception error)
            {
                failures[index] = error;
            }
        }) { IsBackground = true }).ToArray();

        foreach (Thread worker in workers) worker.Start();
        foreach (Thread worker in workers) Assert.True(worker.Join(TimeSpan.FromSeconds(30)));
        Assert.All(failures, failure => Assert.Null(failure));
    }

    private static string LastError() => Marshal.PtrToStringUTF8(NativeMethods.GetLastError()) ?? string.Empty;
}
