using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class SerializedOccurrenceTests
{
    private static readonly bool[] Switches = [false, true];
    public static IEnumerable<object[]> Scenes() => from scene in Enumerable.Range(0, 8) from located in Switches from reverse in Switches
        select new object[] { scene, located, reverse };
    [Fact]
    public void NewLayoutsAndIndependentFormatPreserveOriginalLayoutsAndBudgets()
    {
        Assert.Equal(40, Marshal.SizeOf<SerializedOccurrenceInfoRaw>()); Assert.Equal(40, Marshal.SizeOf<SerializedOccurrenceRowRaw>());
        Assert.Equal(32, Marshal.SizeOf<SerializedReferenceInfoRaw>()); Assert.Equal(32, Marshal.SizeOf<SerializedReferenceRowRaw>());
        Assert.Equal(16, Marshal.SizeOf<AuthoringHistoryRaw>());

        Assert.Equal(512, BrepOccurrenceSnapshot.MaximumOccurrences); Assert.Equal(64, BrepOccurrenceSnapshot.MaximumDepth);
        Assert.Equal(BrepReferenceSnapshot.MaximumBytes, BrepOccurrenceSnapshot.MaximumBytes);
    }
    [Theory, MemberData(nameof(Scenes))]
    public void ActualCompletePathsSurviveOriginalOwnerReleaseAndKeepDirectionLocationAndMultiPathAmbiguity(int scene, bool located, bool reverse)
    {
        using var raw = Scene(scene); using var transform = GpTrsf.Create(located ? 11 : 0, located ? -7 : 0, located ? 3 : 0, 0, 0, 1, located ? .7 : 0);
        using var location = TopLocLocation.FromTransform(transform); using var placed = raw.Located(location); using var identity = TopLocLocation.Identity;
        using var root = reverse ? ReversedRoot(placed) : placed.Moved(identity);
        byte[] before = Bytes(root); var bounds = root.GetBoundingBox();
        var measureKind = scene is 3 or 4 ? InspectionPropertyKind.Length : InspectionPropertyKind.Volume;
        double mass = root.InspectProperties(measureKind).Mass;
        if (scene == 0) Assert.InRange(Math.Abs(Math.Abs(mass) - 6), 0, 1e-7);
        if (scene == 1) Assert.InRange(Math.Abs(Math.Abs(mass) - 32 * Math.PI / 3), 0, 1e-7);
        if (scene == 5) Assert.InRange(Math.Abs(Math.Abs(mass) - 40), 0, 1e-7);
        var snapshot = BrepOccurrenceSnapshot.Capture(root); Assert.Equal(before, Bytes(root));
        Assert.True(snapshot.Report.NativeSourceCaptureProven); Assert.True(snapshot.Report.ProvesCompleteDecodedOccurrencePaths);
        Assert.False(snapshot.Report.ProvesComposedReferenceIdentity);
        if (scene is 0 or 2 or 5 or 6 or 7) Assert.True(snapshot.Report.HasAmbiguousShapeIdentities);
        raw.Dispose(); placed.Dispose(); root.Dispose();
        using var restored = snapshot.RestoreFromProvenNativeSource(); Verify(restored);
        Assert.True(restored.Report.NativeSourceCaptureProven); Assert.Equal(snapshot.Report.SourceDomainId, restored.Report.SourceDomainId);
        Assert.NotEqual(snapshot.Report.DecodedDomainId, restored.Report.DecodedDomainId); Assert.True(restored.Root.IsValid);
        AssertBounds(bounds, restored.Root.GetBoundingBox()); Assert.InRange(Math.Abs(restored.Root.InspectProperties(measureKind).Mass - mass), 0, 1e-7);
        using var second = snapshot.Restore(); Verify(second);
        Assert.Empty(second.FindSameIdentityOccurrences(restored.Root));
        Assert.All(restored.Members, member => Assert.Empty(second.FindSameIdentityOccurrences(member)));
        var imported = BrepOccurrenceSnapshot.Open(snapshot.CopyBrepBytes(), snapshot.CopyOccurrenceTable());
        Assert.False(imported.Report.NativeSourceCaptureProven); Assert.Null(imported.Report.SourceDomainId);
        Assert.True(imported.Report.ProvesCompleteDecodedOccurrencePaths); Assert.False(imported.Report.ProvesComposedReferenceIdentity);
        Assert.Throws<InvalidOperationException>(() => imported.RestoreFromProvenNativeSource());
        using var reopened = imported.Restore(); Verify(reopened); Assert.False(reopened.Report.NativeSourceCaptureProven);
        string path = Temp();
        try
        {
            File.WriteAllBytes(path, snapshot.CopyBrepBytes()); using var ordinary = ShapeExchange.ReadBrep(path);
            AssertBounds(bounds, ordinary.GetBoundingBox()); Assert.True(ordinary.IsValid);
            Assert.InRange(Math.Abs(ordinary.InspectProperties(measureKind).Mass - mass), 0, 1e-7);
            Assert.Empty(restored.FindSameIdentityOccurrences(ordinary));
        }
        finally { File.Delete(path); }
        var paths = restored.Report.Rows.Select(row => string.Join('/', row.Path)).ToArray(); restored.Dispose();
        Assert.Equal(paths, restored.Report.Rows.Select(row => string.Join('/', row.Path)));
        Assert.Throws<ObjectDisposedException>(() => restored.FindSameIdentityOccurrences(second.Root));
    }
    [Fact]
    public void SameNativeBoxThroughDifferentParentsCannotResolveShapeOnlyButExplicitPathsWork()
    {
        using var root = Scene(7); var snapshot = BrepOccurrenceSnapshot.Capture(root); using var graph = snapshot.Restore();
        Assert.Equal(175, graph.Report.Rows.Count); Assert.Equal(172, graph.Report.Rows.Count(row => row.IsShapeIdentityAmbiguous));
        var boxes = graph.Report.Rows.Where(row => row.Kind == ShapeKind.Solid).ToArray(); Assert.Equal(2, boxes.Length);
        Assert.NotEqual(string.Join('/', boxes[0].Path), string.Join('/', boxes[1].Path));
        using var first = graph.Resolve(boxes[0].Path); using var second = graph.Resolve(boxes[1].Path);
        Assert.Equal(0, RepairSnapshot.FindTopologyIndex(first, second));
        Assert.Equal(2, graph.FindSameIdentityOccurrences(first).Count); Assert.Throws<InvalidOperationException>(() => graph.ResolveUnique(first));
        using var equalGeometry = ShapeFactory.CreateBox(1, 2, 3); Assert.Empty(graph.FindSameIdentityOccurrences(equalGeometry));
        Assert.Throws<InvalidOperationException>(() => graph.ResolveUnique(equalGeometry));
    }
    [Fact]
    public void CompleteRowAndDepthBudgetsRejectRatherThanPublishingPartialProofOrChangingGeometry()
    {
        var boxes = Enumerable.Range(0, 7).Select(_ => ShapeFactory.CreateBox(1, 2, 3)).ToArray();
        try
        {
            using var compound = ShapeFactory.CreateCompound(boxes); byte[] before = Bytes(compound);
            Assert.ThrowsAny<Exception>(() => BrepOccurrenceSnapshot.Capture(compound)); Assert.Equal(before, Bytes(compound));
            // The pre-existing geometry/reference contract still accepts this model.
            var original = BrepReferenceSnapshot.Capture(compound, []); using var restored = original.Restore();
            Assert.InRange(Math.Abs(restored.Root.InspectProperties(InspectionPropertyKind.Volume).Mass - 42), 0, 1e-7);
        }
        finally { foreach (var box in boxes) box.Dispose(); }
        using var boxRoot = ShapeFactory.CreateBox(1, 2, 3); int originalDepth = BrepOccurrenceSnapshot.Capture(boxRoot).Report.Rows.Max(row => row.Path.Count);
        using var atLimit = Nested(64 - originalDepth); var valid = BrepOccurrenceSnapshot.Capture(atLimit);
        Assert.Equal(64, valid.Report.Rows.Max(row => row.Path.Count)); using var graph = valid.Restore(); Assert.True(graph.Root.IsValid);
        using var beyond = Nested(65 - originalDepth); byte[] beforeDepth = Bytes(beyond);
        Assert.ThrowsAny<Exception>(() => BrepOccurrenceSnapshot.Capture(beyond)); Assert.Equal(beforeDepth, Bytes(beyond));
    }
    [Fact]
    public void SnapshotBuffersAndNestedReportPathsAreIndependentOwnedValues()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepOccurrenceSnapshot.Capture(root);
        byte[] b = snapshot.CopyBrepBytes(), t = snapshot.CopyOccurrenceTable(); b[0] ^= 1; t[0] ^= 1;
        var child = snapshot.Report.Rows.First(row => row.Path.Count > 0); var path = Assert.IsAssignableFrom<IList<int>>(child.Path);
        Assert.Throws<NotSupportedException>(() => path[0] = 999); root.Dispose(); using var restored = snapshot.RestoreFromProvenNativeSource(); Verify(restored);
        Assert.True(snapshot.CopyOccurrenceTable().AsSpan().StartsWith("OCROCC26"u8));
        var rows = Assert.IsAssignableFrom<IList<BrepOccurrenceRow>>(snapshot.Report.Rows);
        Assert.Throws<NotSupportedException>(() => rows.RemoveAt(0));
    }
    [Fact]
    public void SeparateOwnedResolutionSurvivesClosingResultAndRejectsAnIndependentRestore()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepOccurrenceSnapshot.Capture(root);
        using var result = snapshot.Restore(); using var member = result.Resolve(result.Report.Rows.First(row => row.Kind == ShapeKind.Face).Path);
        result.Dispose(); Assert.True(member.IsValid); using var another = snapshot.Restore(); Assert.Empty(another.FindSameIdentityOccurrences(member));
        Assert.Throws<ObjectDisposedException>(() => result.Resolve([]));
    }
    [Fact]
    public void PreCancelledCaptureOpenAndRestoreDoNotPublishPartialOwnersOrChangeSource()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepOccurrenceSnapshot.Capture(root); byte[] before = Bytes(root);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel(); var token = cancellation.Token;
        Assert.Throws<OperationCanceledException>(() => BrepOccurrenceSnapshot.Capture(root, token));
        Assert.Throws<OperationCanceledException>(() => BrepOccurrenceSnapshot.Open(snapshot.CopyBrepBytes(), snapshot.CopyOccurrenceTable(), token));
        Assert.Throws<OperationCanceledException>(() => snapshot.Restore(token)); Assert.Equal(before, Bytes(root));
        using var valid = snapshot.RestoreFromProvenNativeSource(); Verify(valid);
    }
    [Fact]
    public void RecomputedChecksumDoesNotPromoteDeclaredSourceAddressToOriginalNativeProof()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepOccurrenceSnapshot.Capture(root);
        byte[] brep = snapshot.CopyBrepBytes(), table = snapshot.CopyOccurrenceTable(); int first = RowOffsets(table)[0];
        BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(first, 4), 1); Rehash(brep, table);
        var imported = BrepOccurrenceSnapshot.Open(brep, table); Assert.False(imported.Report.NativeSourceCaptureProven);
        Assert.Equal(1, imported.Report.Rows[0].DeclaredSourceTopologyIndex); Assert.Equal(0, imported.Report.Rows[0].DecodedTopologyIndex);
        Assert.Throws<InvalidOperationException>(() => imported.RestoreFromProvenNativeSource());
        using var restored = imported.Restore(); Verify(restored); Assert.False(restored.Report.NativeSourceCaptureProven);
    }
    [Fact]
    public void IndependentOccurrenceFormatCannotBeMisreadByOriginalReferenceReaderOrViceVersa()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var old = BrepReferenceSnapshot.Capture(root); var next = BrepOccurrenceSnapshot.Capture(root);
        Assert.ThrowsAny<Exception>(() => BrepOccurrenceSnapshot.Open(old.CopyBrepBytes(), old.CopyReferenceTable()));
        Assert.ThrowsAny<Exception>(() => BrepReferenceSnapshot.Open(next.CopyBrepBytes(), next.CopyOccurrenceTable()));
        using var preserved = old.RestoreFromProvenNativeSource(); Assert.False(preserved.Report.ProvesCompleteOccurrencePaths);
        using var current = next.RestoreFromProvenNativeSource(); Assert.True(current.Report.ProvesCompleteDecodedOccurrencePaths);
    }
    [Fact]
    public void ManagedDisposedNullInvalidAndMissingPathsRejectWithoutGuessingIdentity()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); var snapshot = BrepOccurrenceSnapshot.Capture(root); using var graph = snapshot.Restore();
        using var disposed = ShapeFactory.CreateBox(1, 2, 3); disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => BrepOccurrenceSnapshot.Capture(disposed));
        Assert.Throws<ObjectDisposedException>(() => graph.FindSameIdentityOccurrences(disposed));
        Assert.Throws<ArgumentOutOfRangeException>(() => BrepOccurrenceSnapshot.Open([], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => graph.Resolve([-1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => graph.Resolve(Enumerable.Repeat(0, 65).ToArray()));
        Assert.Throws<ArgumentException>(() => graph.Resolve([999]));
        using var nullShape = ShapeFactory.CreateNull(); Assert.ThrowsAny<Exception>(() => BrepOccurrenceSnapshot.Capture(nullShape));
    }
    [Fact]
    public unsafe void NativeNullInvalidRowsAndShortBuffersKeepExistingShapesAndAllOldApisIndependent()
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); byte[] before = Bytes(root);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceRestore(null, 1, null, 1, out nint bad)); Assert.Equal(0, bad);
        NativeError.ThrowIfFailed(NativeMethods.SerializedOccurrenceCapture(root.Handle, out nint raw), "test_occurrence_capture"); using FeatureResultHandle result = new(raw);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceRow(result, -1, out _));
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceMember(result, 512, out nint absent)); Assert.Equal(0, absent);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceBuffer(result, 2, null, 0, out int count)); Assert.Equal(0, count);
        byte value = 0; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceBuffer(result, 0, &value, 1, out count)); Assert.True(count > 1);
        int child = 0; Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrencePath(result, 2, &child, 1, out count)); Assert.True(count > 1);
        NativeError.ThrowIfFailed(NativeMethods.SerializedOccurrenceInfo(result, out var info), "test_occurrence_info");
        int shared = -1;
        for (int i = 0; i < info.RowCount; i++)
        {
            NativeError.ThrowIfFailed(NativeMethods.SerializedOccurrenceRow(result, i, out var row), "test_occurrence_row");
            if (row.SameIdentityPaths > 1) { shared = i; break; }
        }
        Assert.True(shared >= 0);
        NativeError.ThrowIfFailed(NativeMethods.SerializedOccurrenceMember(result, shared, out nint memberRaw), "test_occurrence_member");
        using var member = ShapeFactory.FromNativeHandle(memberRaw, "test_occurrence_member");
        NativeError.ThrowIfFailed(NativeMethods.SerializedOccurrenceMatches(result, member.Handle, 0, null, 0, out count), "test_occurrence_matches"); Assert.True(count > 1);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceMatches(result, member.Handle, 0, &child, 1, out count)); Assert.True(count > 1);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedOccurrenceMatches(result, member.Handle, 2, null, 0, out count)); Assert.Equal(0, count);
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.SerializedReferenceInfo(result, out _)); Assert.Equal(before, Bytes(root));
    }
    [Theory]
    [InlineData("magic")]
    [InlineData("schema")]
    [InlineData("row-limit")]
    [InlineData("path-count")]
    [InlineData("depth")]
    [InlineData("root-kind")]
    [InlineData("root-orientation")]
    [InlineData("topology-limit")]
    [InlineData("wrong-kind")]
    [InlineData("row-orientation")]
    [InlineData("token-orientation")]
    [InlineData("wrong-path")]
    [InlineData("duplicate-path")]
    [InlineData("multiplicity")]
    [InlineData("checksum")]
    [InlineData("truncated")]
    [InlineData("other-graph")]
    public void NativePathTokenCoverageRejectsTamperingEvenAfterRecomputedIntegrity(string mutation)
    {
        using var root = ShapeFactory.CreateBox(1, 2, 3); using var other = ShapeFactory.CreateSphere(2);
        var snapshot = BrepOccurrenceSnapshot.Capture(root); byte[] sourceBefore = Bytes(root), brep = snapshot.CopyBrepBytes(), table = snapshot.CopyOccurrenceTable();
        var rows = RowOffsets(table); int first = rows[0];
        switch (mutation)
        {
            case "magic": table[0] ^= 1; break;
            case "schema": Write(8, 2); break;
            case "row-limit": Write(16, 513); break;
            case "path-count": Write(20, BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(20, 4)) + 1); break;
            case "depth": Write(first + 12, 65); break;
            case "root-kind": Write(24, (int)ShapeKind.Face); break;
            case "root-orientation": Write(28, 1); break;
            case "topology-limit": Write(12, 1000001); break;
            case "wrong-kind": Write(first + 4, (int)ShapeKind.Face); break;
            case "row-orientation": Write(first + 8, 1); break;
            case "token-orientation":
                int token = first + 28; table[token] = table[token] == (byte)'+' ? (byte)'-' : (byte)'+'; Rehash(brep, table); break;
            case "wrong-path": Write(rows[1] + 24, 999); break;
            case "duplicate-path":
                var faces = snapshot.Report.Rows.Where(row => row.Kind == ShapeKind.Face).Take(2).ToArray(); Assert.Equal(2, faces.Length);
                int destination = rows[faces[1].ReferenceIndex] + 24;
                for (int i = 0; i < faces[0].Path.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(destination + i * 4, 4), faces[0].Path[i]);
                Rehash(brep, table); break;
            case "multiplicity": Write(first + 16, 2); break;
            case "checksum": table[^1] ^= 1; break;
            case "truncated": table = table[..^1]; break;
            case "other-graph": brep = BrepOccurrenceSnapshot.Capture(other).CopyBrepBytes(); Rehash(brep, table); break;
            default: throw new ArgumentException("Unknown mutation", nameof(mutation));
        }
        Assert.ThrowsAny<Exception>(() => BrepOccurrenceSnapshot.Open(brep, table)); Assert.Equal(sourceBefore, Bytes(root));
        using var valid = snapshot.RestoreFromProvenNativeSource(); Verify(valid); Assert.InRange(Math.Abs(valid.Root.InspectProperties(InspectionPropertyKind.Volume).Mass - 6), 0, 1e-7);
        void Write(int offset, int value) { BinaryPrimitives.WriteInt32LittleEndian(table.AsSpan(offset, 4), value); Rehash(brep, table); }
    }
    private static void Verify(BrepOccurrenceRestoreResult result)
    {
        Assert.True(result.Report.ProvesCompleteDecodedOccurrencePaths); Assert.False(result.Report.ProvesComposedReferenceIdentity);
        Assert.Equal(result.Report.Rows.Count, result.Members.Count);
        Assert.Equal(result.Report.Rows.Count, result.Report.Rows.Select(row => string.Join('/', row.Path)).Distinct(StringComparer.Ordinal).Count());
        Assert.Single(result.Report.Rows, row => row.Path.Count == 0);
        foreach (var row in result.Report.Rows)
        {
            using var actual = result.Resolve(row.Path); var member = result.Members[row.ReferenceIndex];
            Assert.Equal(0, RepairSnapshot.FindTopologyIndex(actual, member));
            Assert.Equal(row.DecodedTopologyIndex, RepairSnapshot.FindTopologyIndex(result.Root, member));
            var matches = result.FindSameIdentityOccurrences(member); Assert.Equal(row.SameIdentityPathCount, matches.Count);
            Assert.Contains(row.ReferenceIndex, matches);
            var exact = result.FindExactIdentityOccurrences(member); Assert.Equal(row.ExactIdentityPathCount, exact.Count);
            Assert.Contains(row.ReferenceIndex, exact);
            Assert.All(exact, index => Assert.Equal(row.Orientation, result.Report.Rows[index].Orientation));
            if (row.IsShapeIdentityAmbiguous) Assert.Throws<InvalidOperationException>(() => result.ResolveUnique(member));
            else { using var unique = result.ResolveUnique(member); Assert.Equal(0, RepairSnapshot.FindTopologyIndex(actual, unique)); }
        }
    }
    private static Shape Scene(int scene)
    {
        if (scene == 0) return ShapeFactory.CreateBox(1, 2, 3);
        if (scene == 1) return ShapeFactory.CreateSphere(2);
        if (scene == 2)
        {
            using var box = ShapeFactory.CreateBox(1, 2, 3); using var t = GpTrsf.Create(3, 2, 1, 0, 0, 1, .4); using var loc = TopLocLocation.FromTransform(t);
            using var moved = box.Located(loc); using var nested = ShapeFactory.CreateCompound([box, moved]); using var outer = nested.Located(loc);
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
        if (scene == 6)
        {
            using var a = BatchSAuthoringTests.Square(); using var b = BatchSAuthoringTests.Square(5, 3); using var c = BatchSAuthoringTests.Square(10);
            using var loft = GuidedLoft.Build([a, b, c], new() { Solid = true, CorrectCompatibility = true }); using var identity = GpTrsf.Identity;
            return loft.RequireShape().Transformed(identity);
        }
        using var source = ShapeFactory.CreateBox(1, 2, 3); using var parentA = ShapeFactory.CreateCompound([source]); using var parentB = ShapeFactory.CreateCompound([source]);
        return ShapeFactory.CreateCompound([parentA, parentB]);
    }
    private static Shape Nested(int levels)
    {
        Shape current = ShapeFactory.CreateBox(1, 2, 3);
        try { for (int i = 0; i < levels; i++) { var parent = ShapeFactory.CreateCompound([current]); current.Dispose(); current = parent; } return current; }
        catch { current.Dispose(); throw; }
    }
    private static Shape ReversedRoot(Shape source)
    {
        var old = BrepReferenceSnapshot.Capture(source, []); byte[] bytes = old.CopyBrepBytes(), table = old.CopyReferenceTable();
        int size = BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(40, 4)); var token = table.AsSpan(44, size);
        Assert.True(bytes.AsSpan().EndsWith(token)); Assert.True(token[0] is (byte)'+' or (byte)'-');
        bytes[bytes.Length - size] = token[0] == (byte)'+' ? (byte)'-' : (byte)'+';
        string path = Temp(); try { File.WriteAllBytes(path, bytes); return ShapeExchange.ReadBrep(path); } finally { File.Delete(path); }
    }
    private static List<int> RowOffsets(byte[] table)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(16, 4)); int offset = 48 + BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(44, 4)); List<int> rows = [];
        for (int i = 0; i < count; i++) { rows.Add(offset); int depth = BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(offset + 12, 4)); offset += 24 + 4 * depth; offset += 4 + BinaryPrimitives.ReadInt32LittleEndian(table.AsSpan(offset, 4)); }
        Assert.Equal(table.Length - 32, offset); return rows;
    }
    private static void Rehash(byte[] brep, byte[] table) { byte[] content = [.. brep, .. table.AsSpan(0, table.Length - 32)]; SHA256.HashData(content).CopyTo(table, table.Length - 32); }
    private static string Temp() => Path.Combine(Path.GetTempPath(), "occtsharp-occurrence-" + Guid.NewGuid().ToString("N") + ".brep");
    private static void AssertBounds(BoundingBox3d source, BoundingBox3d target)
    {
        double[] a = [source.Minimum.X, source.Minimum.Y, source.Minimum.Z, source.Maximum.X, source.Maximum.Y, source.Maximum.Z];
        double[] b = [target.Minimum.X, target.Minimum.Y, target.Minimum.Z, target.Maximum.X, target.Maximum.Y, target.Maximum.Z];
        for (int i = 0; i < a.Length; i++) Assert.InRange(Math.Abs(a[i] - b[i]), 0, 1e-7);
    }
    private static unsafe byte[] Bytes(Shape shape)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, null, 0, out int count), "test_occurrence_source_count");
        byte[] bytes = new byte[count]; fixed (byte* values = bytes) NativeError.ThrowIfFailed(NativeMethods.RepairSerialized(shape.Handle, values, count, out _), "test_occurrence_source_bytes"); return bytes;
    }
}
#pragma warning restore CA1861
