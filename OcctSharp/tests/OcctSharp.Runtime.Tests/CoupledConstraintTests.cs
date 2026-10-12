using System.Collections;
using System.Runtime.InteropServices;
using OcctSharp.Geometry.Constraints;
using OcctSharp.Tests.Shared;
using OcctSharp.Values;
namespace OcctSharp.Runtime.Tests;
public sealed class CoupledConstraintTests {
    [Fact] public void EveryInitializedConstructorFeedsDetachedHermiteCadWorkflow() => BatchBTCoupledConstraintWorkflow.Run();
    [Fact] public void EmptyBatchAndEmptySidesAreExplicit() {
        Assert.Empty(CoupledConstraints.Copy([], 0));
        var row = CoupledConstraints.Copy([new(new([]), new([]))], 0).Single();
        Assert.Null(row.Positions3d); Assert.Null(row.Positions2d);
        Assert.False(row.HasTangency); Assert.False(row.HasCurvature);
    }
    [Fact] public void IllegalCoupledCountsModesBoundsAndFiniteValuesRejectWholeCall() {
        Coordinates3d[] p = [new(1, 2, 3), new(4, 5, 6)];
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([new(new(p, [new(1, 0, 0)]))]));
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([new(new(p, null, p))]));
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([new(new(p, p), new([new(1, 2)]))]));
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([new(new([], []))]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([new(new(p, Lower: int.MaxValue))]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([new(new(p))], 1));
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([new(new(p)), new(new([new(double.NaN, 0, 0)]))]));
        Assert.Throws<ArgumentNullException>(() => CoupledConstraints.Copy(null!));
        Assert.Throws<ArgumentNullException>(() => CoupledConstraints.Copy([new(new ConstraintInput3d(null!))]));
        Assert.Throws<ArgumentNullException>(() => CoupledConstraints.Copy([new(null, new ConstraintInput2d(null!))]));
        Assert.Throws<ArgumentException>(() => CoupledConstraints.Copy([null!]));
    }
    private sealed class Tracked<T>(Func<IEnumerable<T>> values) : IEnumerable<T> {
        internal int ReadCount, DisposeCount;
        public IEnumerator<T> GetEnumerator() { try { foreach (var value in values()) { ++ReadCount; yield return value; } } finally { ++DisposeCount; } }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private static IEnumerable<Coordinates3d> Endless() { while (true) yield return new(1, 2, 3); }
    private static IEnumerable<Coordinates3d> Failing() { yield return new(1, 2, 3); throw new InvalidOperationException("enumeration failure"); }
    [Fact] public void LazyInputsAreSinglePassBoundedAndAlwaysDisposed() {
        var p = new Tracked<Coordinates3d>(() => [new(1, 2, 3)]);
        var rows = new Tracked<ConstraintRowInput>(() => [new(new(p))]);
        Assert.Single(CoupledConstraints.Copy(rows)); Assert.Equal(1, p.ReadCount); Assert.Equal(1, p.DisposeCount); Assert.Equal(1, rows.DisposeCount);
        var endless = new Tracked<Coordinates3d>(Endless);
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([new(new(endless))], 3));
        Assert.Equal(4, endless.ReadCount); Assert.Equal(1, endless.DisposeCount);
        var failure = new Tracked<Coordinates3d>(Failing);
        Assert.Throws<InvalidOperationException>(() => CoupledConstraints.Copy([new(new(failure))])); Assert.Equal(1, failure.DisposeCount);
    }
    [Fact] public void RowAndTotalPointBudgetsCoverNestedGroups() {
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy(Enumerable.Repeat(new ConstraintRowInput(), CoupledConstraints.MaximumRows + 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([new(), new(new([new(1, 2, 3)])), new(null, new([new(1, 2)]))], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([], -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoupledConstraints.Copy([], CoupledConstraints.MaximumPoints + 1));
        Assert.Equal(CoupledConstraints.MaximumRows, CoupledConstraints.Copy(Enumerable.Repeat(new ConstraintRowInput(), CoupledConstraints.MaximumRows), 0).Count);
        var maximum = CoupledConstraints.Copy([new(new(Enumerable.Repeat(new Coordinates3d(1, 2, 3), CoupledConstraints.MaximumPoints), Lower: -40000))]).Single();
        Assert.Equal(CoupledConstraints.MaximumPoints, maximum.Positions3d!.Count);
        Assert.Equal(new Coordinates3d(1, 2, 3), maximum.Positions3d[maximum.Positions3d.Upper]);
        foreach (int lower in new[] { int.MinValue + 1, int.MaxValue - 1 }) {
            var bound = CoupledConstraints.Copy([new(new([new(7, 8, 9)], Lower: lower))]).Single();
            Assert.Equal(new Coordinates3d(7, 8, 9), bound.Positions3d![lower]);
        }
    }
    [Fact] public unsafe void NativeFailureDoesNotWriteAnyOutputChannel() {
        // Establish the resolver in the same module as the raw export.
        CoupledConstraints.Copy([]);
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try {
        var CopyNative = Marshal.GetDelegateForFunctionPointer<CopyRaw>(NativeLibrary.GetExport(library, "occtsharp_coupled_constraints_copy"));
        int[] rows = [1, 1, 1, 1, 2, -2, 1, 9, 0, 0];
        double[] input3 = new double[18], input2 = new double[6];
        double[] output3 = Enumerable.Repeat(77.0, 18).ToArray(), output2 = Enumerable.Repeat(88.0, 6).ToArray();
        int[] flags = [99, 99];
        fixed (int* r = rows, f = flags) fixed (double* i3 = input3, i2 = input2, o3 = output3, o2 = output2) {
            Assert.NotEqual(0, CopyNative(r, 2, 10, i3, 18, i2, 6, 3, o3, 17, o2, 6, f, 2));
            input3[17] = double.PositiveInfinity;
            Assert.NotEqual(0, CopyNative(r, 2, 10, i3, 18, i2, 6, 3, o3, 18, o2, 6, f, 2));
            input3[17] = 0; rows[9] = 7;
            Assert.NotEqual(0, CopyNative(r, 2, 10, i3, 18, i2, 6, 3, o3, 18, o2, 6, f, 2));
        }
        Assert.All(output3, x => Assert.Equal(77, x)); Assert.All(output2, x => Assert.Equal(88, x)); Assert.All(flags, x => Assert.Equal(99, x));
        } finally { NativeLibrary.Free(library); }
    }
    [Fact] public void ConcurrentIndependentCallsRetainNoSharedSdkArrays() {
        Parallel.For(0, 80, i => {
            var row = CoupledConstraints.Copy([new(new([new(i, 2, 3)]), new([new(i, 4)]))]).Single();
            Assert.Equal(i, row.Positions3d![1].X); Assert.Equal(i, row.Positions2d![1].X);
            var copy = row.Positions3d.ToArray(); copy[0] = new(-1, -1, -1); Assert.Equal(i, row.Positions3d[1].X);
        });
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int CopyRaw(int* rows, int rowCount, int rowCapacity, double* input3, int count3, double* input2, int count2, int pointBudget, double* output3, int capacity3, double* output2, int capacity2, int* flags, int flagCapacity);
}
