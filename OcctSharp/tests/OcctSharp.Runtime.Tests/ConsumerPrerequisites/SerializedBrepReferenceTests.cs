using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class SerializedBrepReferenceTests
{
    private static readonly bool[] Switches = [false, true];
    public static IEnumerable<object[]> Scenes() => from scene in Enumerable.Range(0, 7) from located in Switches from reverse in Switches
        select new object[] { scene, located, reverse };
    [Fact]
    public void IndependentLayoutsKeepOriginalHistoryCopyAndDomainLayouts()
    {
        Assert.Equal(32, Marshal.SizeOf<SerializedReferenceInfoRaw>());
        Assert.Equal(32, Marshal.SizeOf<SerializedReferenceRowRaw>());
        Assert.Equal(16, Marshal.SizeOf<AuthoringHistoryRaw>());


        Assert.Equal(512, BrepReferenceSnapshot.MaximumReferences);
        Assert.Equal(64 * 1024 * 1024, BrepReferenceSnapshot.MaximumBytes);
    }
    [Theory, MemberData(nameof(Scenes))]
    public void ActualSevenGraphsKeepSelectedNativeMembersLocationDirectionAndIndependentDecodeAfterAllSourcesReleased(
        int scene, bool located, bool reverse)
    {
        using var raw = Scene(scene); using var transform = GpTrsf.Create(located ? 11 : 0, located ? -7 : 0, located ? 3 : 0, 0, 0, 1, located ? .7 : 0);
        using var location = TopLocLocation.FromTransform(transform); using var placed = raw.Located(location);
        using var identityLocation = TopLocLocation.Identity;
        using var root = reverse ? ReversedRoot(placed) : placed.Moved(identityLocation);
        var placedBounds = placed.GetBoundingBox();
        AssertBounds(placedBounds, root.GetBoundingBox());
        if (!reverse) Assert.Equal(0, RepairSnapshot.FindTopologyIndex(placed, root));
        List<Shape> selected = [root]; selected.AddRange(root.GetSubShapes(ShapeKind.Face)); selected.AddRange(root.GetSubShapes(ShapeKind.Edge)); selected.AddRange(root.GetSubShapes(ShapeKind.Vertex));
        try
        {
            var measureKind = scene is 3 or 4 ? InspectionPropertyKind.Length : InspectionPropertyKind.Volume;
            byte[] before = Bytes(root); double originalVolume = root.InspectProperties(measureKind).Mass;
            if (scene is 3 or 4)
            {
                Assert.True(originalVolume > 0);
                Assert.Throws<InvalidCastException>(() => root.InspectProperties(InspectionPropertyKind.Volume));
            }
            if (scene == 0) Assert.InRange(Math.Abs(Math.Abs(originalVolume) - 6), 0, 1e-7);
            if (scene == 1) Assert.InRange(Math.Abs(Math.Abs(originalVolume) - 32 * Math.PI / 3), 0, 1e-7);
            if (scene == 5) Assert.InRange(Math.Abs(Math.Abs(originalVolume) - 40), 0, 1e-7);
            int[] sourceAddresses = selected.Select(member => RepairSnapshot.FindTopologyIndex(root, member)).ToArray();
            Assert.All(sourceAddresses, index => Assert.True(index >= 0));
            var snapshot = BrepReferenceSnapshot.Capture(root, selected);
            Assert.Equal(before, Bytes(root)); Assert.True(snapshot.Report.NativeSourceCaptureProven);
            Assert.Equal(sourceAddresses, snapshot.Report.Rows.Select(row => row.DeclaredSourceTopologyIndex));
            Assert.False(snapshot.Report.ProvesCompleteOccurrencePaths); Assert.False(snapshot.Report.ProvesComposedReferenceIdentity);
            byte[] brep = snapshot.CopyBrepBytes(), table = snapshot.CopyReferenceTable();
            Assert.True(brep.AsSpan().StartsWith("DBRep_DrawableShape\n"u8));
            raw.Dispose(); placed.Dispose(); root.Dispose(); foreach (var member in selected.Skip(1)) member.Dispose();
            using var restored = snapshot.RestoreFromProvenNativeSource();
            Verify(restored); Assert.True(restored.Root.IsValid);
            AssertBounds(placedBounds, restored.Root.GetBoundingBox());
            Assert.InRange(Math.Abs(restored.Root.InspectProperties(measureKind).Mass - originalVolume), 0, 1e-7);
            Assert.True(restored.Report.NativeSourceCaptureProven);
            Assert.Equal(snapshot.Report.SourceDomainId, restored.Report.SourceDomainId);
            Assert.NotEqual(snapshot.Report.DecodedDomainId, restored.Report.DecodedDomainId);
            Assert.Equal(snapshot.Report.RootOrientation, restored.Report.RootOrientation);
            using var second = snapshot.Restore(); Verify(second);
            Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(second.Root, restored.Root));
            Assert.All(restored.Members, member => Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(second.Root, member)));
            string path = Temp();
            try
            {
                File.WriteAllBytes(path, brep); using var ordinary = ShapeExchange.ReadBrep(path);
                AssertBounds(placedBounds, ordinary.GetBoundingBox());
                Assert.True(ordinary.IsValid); Assert.InRange(Math.Abs(ordinary.InspectProperties(measureKind).Mass - originalVolume), 0, 1e-7);
                Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(ordinary, restored.Root));
                Assert.All(restored.Members, member => Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(ordinary, member)));
            }
            finally { File.Delete(path); }
            var imported = BrepReferenceSnapshot.Open(brep, table);
            Assert.False(imported.Report.NativeSourceCaptureProven); Assert.Null(imported.Report.SourceDomainId);
            Assert.Throws<InvalidOperationException>(imported.RestoreFromProvenNativeSource);
            using var importedGraph = imported.Restore(); Verify(importedGraph);
            Assert.False(importedGraph.Report.NativeSourceCaptureProven); Assert.True(importedGraph.Report.NativeDecodedMembershipProven);
            var values = restored.Report.Rows.ToArray(); restored.Dispose(); Assert.Equal(values, restored.Report.Rows.ToArray());
        }
        finally { foreach (var member in selected.Skip(1)) member.Dispose(); }
    }
    [Fact]
    public void SameCoordinatesAndIndependentCopiesCannotManufactureNativeCaptureMembership()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); using var same = ShapeFactory.CreateBox(1, 2, 3);
        using var transform = GpTrsf.Identity; using var copied = root.Transformed(transform);
        byte[] before = Bytes(root);
        Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(root, same)); Assert.Equal(-1, RepairSnapshot.FindTopologyIndex(root, copied));
        Assert.ThrowsAny<Exception>(() => BrepReferenceSnapshot.Capture(root, [same]));
        Assert.ThrowsAny<Exception>(() => BrepReferenceSnapshot.Capture(root, [copied])); Assert.Equal(before, Bytes(root));
    }
    [Fact]
    public void EmptyReferenceCaptureAndExportCopiesKeepOwnedValuesAndUnknownStoredOrigin()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3);
        var snapshot = BrepReferenceSnapshot.Capture(root, []); Assert.Empty(snapshot.Report.Rows);
        byte[] brep = snapshot.CopyBrepBytes(), table = snapshot.CopyReferenceTable(); brep[0] ^= 1; table[0] ^= 1;
        root.Dispose(); using var restored = snapshot.RestoreFromProvenNativeSource(); Verify(restored); Assert.Empty(restored.Members);
        var saved = BrepReferenceSnapshot.Open(snapshot.CopyBrepBytes(), snapshot.CopyReferenceTable());
        Assert.False(saved.Report.NativeSourceCaptureProven); Assert.Throws<InvalidOperationException>(saved.RestoreFromProvenNativeSource);
        using var decoded = saved.Restore(); Assert.Equal(6, decoded.Root.InspectProperties(InspectionPropertyKind.Volume).Mass, 10);
    }
    [Fact]
    public void RepeatedSelectedSlotsRetainExactNativeIdentityWithoutClaimingOccurrencePaths()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); Shape[] faces = root.GetSubShapes(ShapeKind.Face);
        try
        {
            var snapshot = BrepReferenceSnapshot.Capture(root, [faces[0], faces[0], root]);
            using var restored = snapshot.Restore(); Verify(restored);
            Assert.Equal(restored.Report.Rows[0].DecodedTopologyIndex, restored.Report.Rows[1].DecodedTopologyIndex);
            Assert.Equal(0, RepairSnapshot.FindTopologyIndex(restored.Members[0], restored.Members[1]));
            Assert.False(restored.Report.ProvesCompleteOccurrencePaths);
        }
        finally { foreach (var face in faces) face.Dispose(); }
    }
    [Fact]
    public void RecomputedIntegrityCannotPromoteDeclaredStoredSourceAddressToNativeSourceProof()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepReferenceSnapshot.Capture(root);
        byte[] brep = snapshot.CopyBrepBytes(), table = snapshot.CopyReferenceTable(); int first = FirstRow(table);
        BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first, 4), 1); Rehash(brep, table);
        var imported = BrepReferenceSnapshot.Open(brep, table); Assert.False(imported.Report.NativeSourceCaptureProven);
        Assert.Equal(1, Assert.Single(imported.Report.Rows).DeclaredSourceTopologyIndex);
        Assert.Equal(0, Assert.Single(imported.Report.Rows).DecodedTopologyIndex);
        Assert.Throws<InvalidOperationException>(imported.RestoreFromProvenNativeSource);
        using var result = imported.Restore(); Verify(result); Assert.False(result.Report.NativeSourceCaptureProven);
    }
    [Theory]
    [InlineData("magic")]
    [InlineData("schema")]
    [InlineData("row-limit")]
    [InlineData("source-address")]
    [InlineData("kind")]
    [InlineData("orientation")]
    [InlineData("token-index")]
    [InlineData("token-length")]
    [InlineData("checksum")]
    [InlineData("other-graph")]
    [InlineData("shape-count")]
    [InlineData("truncated")]
    public void MalformedBoundBytesAndTokensAreRejectedInsteadOfRebinding(string mutation)
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); using var other = ShapeFactory.CreateBox(4, 5, 6);
        var snapshot = BrepReferenceSnapshot.Capture(root); byte[] brep = snapshot.CopyBrepBytes(), table = snapshot.CopyReferenceTable(); int first = FirstRow(table);
        switch (mutation)
        {
            case "magic": table[0] ^= 1; break;
            case "schema": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(8, 4), 2); Rehash(brep, table); break;
            case "row-limit": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(24, 4), 513); Rehash(brep, table); break;
            case "source-address": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first, 4), snapshot.Report.DeclaredSourceTopologyCount); Rehash(brep, table); break;
            case "kind": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first + 4, 4), (int)ShapeKind.Edge); Rehash(brep, table); break;
            case "orientation": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first + 8, 4), 1); Rehash(brep, table); break;
            case "token-index": table[first + 17] = (byte)'0'; Rehash(brep, table); break;
            case "token-length": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first + 12, 4), 65); Rehash(brep, table); break;
            case "checksum": table[^1] ^= 1; break;
            case "other-graph": brep = BrepReferenceSnapshot.Capture(other).CopyBrepBytes(); break;
            case "shape-count": BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(28, 4), 1000001); Rehash(brep, table); break;
            case "truncated": table = table[..^1]; break;
            default: throw new ArgumentException("Unknown original mutation", nameof(mutation));
        }
        Assert.ThrowsAny<Exception>(() => BrepReferenceSnapshot.Open(brep, table));
        using var valid = snapshot.RestoreFromProvenNativeSource(); Verify(valid);
        Assert.Equal(6, valid.Root.InspectProperties(InspectionPropertyKind.Volume).Mass, 10);
    }
    [Fact]
    public unsafe void NativeNullOutOfRangeAndShortBuffersFailWithoutAllocatingShapesOrChangingInputs()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); byte[] before = Bytes(root);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceCapture(root.Handle, null, 513, out nint bad)); Assert.Equal(0, bad);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceRestore(null, 1, null, 1, out bad)); Assert.Equal(0, bad);
        NativeError.ThrowIfFailed(NativeMethods.SerializedReferenceCapture(root.Handle, null, 0, out nint raw), "test_capture_empty");
        using FeatureResultHandle result = new(raw);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceRow(result, -1, out _));
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceMember(result, 0, out nint absent)); Assert.Equal(0, absent);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceBuffer(result, 2, null, 0, out int count)); Assert.Equal(0, count);
        byte value = 0; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceBuffer(result, 0, &value, 1, out count)); Assert.True(count > 1);
        Assert.Equal(before, Bytes(root));
    }
    [Fact]
    public void DisposedAndTooManyManagedSourcesAndEmptyImportKeepOriginalLimits()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); using var disposed = ShapeFactory.CreateBox(1, 2, 3); disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => BrepReferenceSnapshot.Capture(disposed));
        Assert.Throws<ObjectDisposedException>(() => BrepReferenceSnapshot.Capture(root, [disposed]));
        Assert.Throws<ArgumentException>(() => BrepReferenceSnapshot.Capture(root, Enumerable.Repeat(root, 513)));
        Assert.Throws<ArgumentOutOfRangeException>(() => BrepReferenceSnapshot.Open([], []));
        using var nullShape = ShapeFactory.CreateNull(); Assert.ThrowsAny<Exception>(() => BrepReferenceSnapshot.Capture(nullShape));
    }
    private static void Verify(BrepReferenceRestoreResult result)
    {
        Assert.True(result.Report.NativeDecodedMembershipProven); Assert.False(result.Report.ProvesCompleteOccurrencePaths); Assert.False(result.Report.ProvesComposedReferenceIdentity);
        Assert.Equal(result.Members.Count, result.Report.Rows.Count);
        for (int i = 0; i < result.Members.Count; i++)
        { var row = result.Report.Rows[i]; Assert.Equal(i, row.ReferenceIndex); Assert.Equal(RepairSnapshot.FindTopologyIndex(result.Root, result.Members[i]), row.DecodedTopologyIndex); Assert.True(row.DecodedTopologyIndex >= 0); }
    }
    private static void AssertBounds(BoundingBox3d expected, BoundingBox3d actual)
    {
        double[] source = [expected.Minimum.X, expected.Minimum.Y, expected.Minimum.Z, expected.Maximum.X, expected.Maximum.Y, expected.Maximum.Z];
        double[] target = [actual.Minimum.X, actual.Minimum.Y, actual.Minimum.Z, actual.Maximum.X, actual.Maximum.Y, actual.Maximum.Z];
        for (int i = 0; i < source.Length; i++) Assert.InRange(Math.Abs(target[i] - source[i]), 0, 1e-7);
    }
    private static Shape Scene(int scene)
    {
        if (scene == 0) return ShapeFactory.CreateBox(1, 2, 3);
        if (scene == 1) return ShapeFactory.CreateSphere(2);
        if (scene == 2)
        {
            using var box = ShapeFactory.CreateBox(1, 2, 3); using var t = GpTrsf.Create(3, 2, 1, 0, 0, 1, .4);
            using var loc = TopLocLocation.FromTransform(t); using var moved = box.Located(loc);
            using var nested = ShapeFactory.CreateCompound([box, moved]); using var outer = nested.Located(loc);
            return ShapeFactory.CreateCompound([box, moved, outer]);
        }
        if (scene == 3) return ShapeFactory.CreateBezierEdge([new(-1, 0, 0), new(-.5, 1, 0), new(.5, 1, 0), new(1, 0, 0)]);
        if (scene == 4)
        {
            var expression = SketchCurveExpression2d.FromCurve(SketchCurve2d.Bezier([new(-.5, 0), new(0, 1), new(.5, 0)])).Offset(.03).Trim(.1, .9);
            using var wire = SketchExpressionChain2d.CreateOrdered([expression]).BuildWire(SketchPlane.XY);
            using var result = GuidedSweepPlan.PrepareOffsetCurves(wire); using var identity = GpTrsf.Identity;
            Assert.InRange(result.Diagnostics.ApproximationError!.Value, 0, 1e-10); return result.RequireShape().Transformed(identity);
        }
        if (scene == 5)
        {
            using var spine = BatchSAuthoringTests.Spine(); using var square = BatchSAuthoringTests.Square();
            using var plan = GuidedSweepPlan.Create(spine, [new(square)], new() { SolidPolicy = SweepSolidPolicy.RequireSolid });
            using var result = plan.Build(); using var identity = GpTrsf.Identity; return result.RequireShape().Transformed(identity);
        }
        using var a = BatchSAuthoringTests.Square(); using var b = BatchSAuthoringTests.Square(5, 3); using var d = BatchSAuthoringTests.Square(10);
        using var loft = GuidedLoft.Build([a, b, d], new() { Solid = true, CorrectCompatibility = true }); using var transform = GpTrsf.Identity;
        return loft.RequireShape().Transformed(transform);
    }
    private static Shape ReversedRoot(Shape source)
    {
        var snapshot = BrepReferenceSnapshot.Capture(source, []); byte[] bytes = snapshot.CopyBrepBytes(), table = snapshot.CopyReferenceTable();
        int tokenSize = BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(40, 4)); ReadOnlySpan<byte> token = table.AsSpan(44, tokenSize);
        Assert.True(bytes.AsSpan().EndsWith(token)); Assert.True(token[0] is (byte)'+' or (byte)'-');
        bytes[bytes.Length - tokenSize] = token[0] == (byte)'+' ? (byte)'-' : (byte)'+';
        string path = Temp(); try { File.WriteAllBytes(path, bytes); return ShapeExchange.ReadBrep(path); } finally { File.Delete(path); }
    }
    private static int FirstRow(byte[] table) => 44 + BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(40, 4));
    private static void Rehash(byte[] brep, byte[] table)
    { byte[] content = [.. brep, .. table.AsSpan(0, table.Length - 32)]; SHA256.HashData(content).CopyTo(table, table.Length - 32); }
    private static string Temp() => Path.Combine(Path.GetTempPath(), "occtsharp-brep-reference-" + Guid.NewGuid().ToString("N") + ".brep");
    private static unsafe byte[] Bytes(Shape shape)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count), "test_source_bytes_count");
        byte[] bytes = new byte[count]; fixed (byte* data = bytes) NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, data, bytes.Length, out _), "test_source_bytes"); return bytes;
    }
}
