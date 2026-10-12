using OcctSharp.Tests.Shared;
using OcctSharp.DocumentProjections;
using OcctSharp.XdeAttributeProjections;
using OcctSharp.Interop;
using System.Runtime.InteropServices;
using System.Text;
namespace OcctSharp.Runtime.Tests;
public sealed partial class DocumentProjectionTests
{
    [Fact] public void NativeCopiedMetadataAndActualUndoRedo() => BatchASDocumentWorkflow.RunNative();
    [Fact] public void ActualRepeatedAssemblyAndXcafReopen() => BatchASDocumentWorkflow.RunFacade();

    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf)]
    [InlineData(DocumentStorageFormat.XmlOcaf)]
    public void GenericActualPersistencePreservesAllCategories(DocumentStorageFormat format)
    {
        string path = Path.Combine(Path.GetTempPath(), "AS-" + Guid.NewGuid() + (format == DocumentStorageFormat.BinOcaf ? ".cbf" : ".xml"));
        try
        {
            using var document = OcafDocument.Create();
            using (var transaction = document.BeginTransaction("AS generic persistence"))
            {
                document.Projections.ReplaceNamedData(document.Projections.GetLabel(document.RootLabel.Entry), BatchASDocumentWorkflow.Values());
                Assert.True(transaction.Commit());
            }
            document.Save(path, format);
            using var reopened = OcafDocument.Open(path);
            BatchASDocumentWorkflow.Verify(reopened.Projections.Snapshot(reopened.Projections.GetLabel(document.RootLabel.Entry)).NamedData);
            BatchASDocumentWorkflow.Verify(document.Projections.Snapshot(document.Projections.GetLabel(document.RootLabel.Entry)).NamedData);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void DataCopiesInputsAndRejectsInvalidTextNumbersBoundsAndDuplicateKeys()
    {
        int[] values = [0, 7]; var array = new NamedDataArray<int>(-9, values);
        Dictionary<string, int> keys = new() { ["zero"] = 0 }; var data = new NamedDataValues(integers: keys);
        values[0] = 99; keys["zero"] = 99;
        Assert.Equal(0, array.Values[0]); Assert.Equal(0, data.Integers["zero"]); Assert.False(data.Integers.ContainsKey("missing"));
        Assert.Throws<NotSupportedException>(() => ((IList<int>)array.Values)[0] = 2);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)data.Integers).Add("x", 1));
        Assert.Throws<ArgumentException>(() => new NamedDataValues(integers: [new("x", 1), new("x", 2)]));
        Assert.Throws<ArgumentException>(() => new NamedDataValues(strings: [new("x", "embedded\0nul")]));
        Assert.Throws<EncoderFallbackException>(() => new NamedDataValues(strings: [new("x", "\uD800")]));
        Assert.Throws<ArgumentException>(() => new NamedDataValues(reals: [new("x", double.NaN)]));
        Assert.Throws<ArgumentException>(() => new NamedDataArray<double>(1, [double.PositiveInfinity]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NamedDataArray<int>(int.MinValue, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NamedDataArray<int>(int.MaxValue, [1]));
    }

    [Fact]
    public void ContextDomainsDisposedViewsAndParentsStayDistinct()
    {
        using var first = OcafDocument.Create(); using var second = OcafDocument.Create();
        var context = first.Projections; var label = context.GetLabel(first.RootLabel.Entry);
        Assert.Throws<ArgumentException>(() => second.Projections.Snapshot(label));
        using var sameParent = DocumentProjectionContext.BindLegacy(first.Handle);
        Assert.Throws<ArgumentException>(() => sameParent.Snapshot(label));
        Assert.ThrowsAny<Exception>(() => context.GetLabel("0:01"));
        Assert.ThrowsAny<Exception>(() => context.GetLabel("0:2147483648"));
        Assert.ThrowsAny<Exception>(() => context.GetLabel("0:1:999999"));
        Assert.ThrowsAny<Exception>(() => context.ReplaceNamedData(label, new()));
        context.Dispose(); Assert.Throws<ObjectDisposedException>(() => context.Snapshot(label));
        var recreated = first.Projections; Assert.NotSame(context, recreated);
        Assert.Throws<ArgumentException>(() => recreated.Snapshot(label));
        var fresh = recreated.GetLabel(first.RootLabel.Entry); first.Dispose();
        Assert.Throws<ObjectDisposedException>(() => recreated.Snapshot(fresh));
        Assert.Throws<ObjectDisposedException>(() => first.Projections);
    }

    [Fact]
    public unsafe void AtomicNativeMidBatchFailurePreservesOuterCommandEdits()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(DocumentProjectionTests).Assembly);
        using var document = OcafDocument.Create(); var context = document.Projections;
        using var command = document.BeginTransaction("AS preserve outer edits");
        document.RootLabel.Name = "outer edit";
        var live = context.GetLabel(document.RootLabel.Entry); context.ReplaceNamedData(live, BatchASDocumentWorkflow.Values());
        byte[] bytes = Payload(writer => { writer.Write(1); writer.Write(1); writer.Write(2); Text(writer, live.Entry); writer.Write(0); Text(writer, "0:1:999999"); writer.Write(0); });
        fixed (byte* p = bytes) Assert.NotEqual(0, Replace(document.Handle.DangerousGetHandle(), 1, p, bytes.Length));
        Assert.Equal("outer edit", document.RootLabel.Name); BatchASDocumentWorkflow.Verify(context.Snapshot(live).NamedData);
        Assert.True(command.Commit()); Assert.True(document.Undo()); Assert.Null(document.RootLabel.Name);
        Assert.True(document.Redo()); BatchASDocumentWorkflow.Verify(context.Snapshot(live).NamedData);
    }

    [Fact]
    public void LayerNoteRemovalOrphanStateAndAtomicCreateFailure()
    {
        using var document = OcafDocument.Create(); using var context = DocumentProjectionContext.BindLegacy(document.Handle);
        using var view = XdeAttributeProjectionContext.Bind(context); var main = context.GetLabel(document.RootLabel.Entry);
        Assert.ThrowsAny<Exception>(() => view.CreateLayer("no command"));
        using var command = document.BeginTransaction("AS lifecycle");
        var layer = view.CreateLayer("layer"); view.AssignLayer(main, layer); Assert.True(view.SetLayerVisibility(layer, false));
        Assert.False(view.SetLayerVisibility(layer, false)); Assert.True(view.UnassignLayer(main, layer));
        _ = view.UnassignLayer(main, layer); Assert.Empty(view.Snapshot(main).Layers.Single().AssociatedEntries);
        view.AssignLayer(main, layer); Assert.True(view.RemoveLayer(layer)); Assert.Empty(view.Snapshot(main).Layers);
        Assert.ThrowsAny<Exception>(() => view.AssignLayer(main, layer));
        var note = view.CreateNote(XdeNoteProjectionKind.Balloon, "", "", ""); Assert.True(view.Snapshot().Notes.Single().NativeIsOrphan);
        view.LinkNote(note, main); Assert.False(view.Snapshot().Notes.Single().NativeIsOrphan);
        Assert.True(view.UnlinkNote(note, main)); Assert.True(view.Snapshot().Notes.Single().NativeIsOrphan);
        view.LinkNote(note, main); Assert.True(view.UnlinkNote(note, main, true)); Assert.Empty(view.Snapshot().Notes);
        Assert.ThrowsAny<Exception>(() => view.LinkNote(note, main));
        var other = view.CreateNote(XdeNoteProjectionKind.Comment, "作者", "time", "text", [main]); Assert.True(view.DeleteNote(other));
        Assert.Empty(view.Snapshot(main).TargetNoteEntries); Assert.True(command.Commit());
    }

    [Theory]
    [InlineData(DocumentStorageFormat.BinXcaf)]
    [InlineData(DocumentStorageFormat.XmlXcaf)]
    public void SingletonZeroBoundArraysAndBalloonUnicodePersist(DocumentStorageFormat format)
    {
        string path = Path.Combine(Path.GetTempPath(), "AS-edge-" + Guid.NewGuid() + (format == DocumentStorageFormat.BinXcaf ? ".xbf" : ".xml"));
        try
        {
            using var document = XdeDocument.Create(); var context = document.Projections; var main = context.GetLabel("0:1");
            using (var command = document.BeginTransaction("AS edge persistence"))
            {
                context.ReplaceNamedData(main, new(reals: [new("precision", Math.PI)], strings: [new("key with space", "< & \" 中文🙂")], integerArrays: [new("zero", new(0, [7]))], realArrays: [new("zero", new(0, [Math.PI]))]));
                _ = document.AttributeProjections.CreateNote(XdeNoteProjectionKind.Balloon, "作者🙂", "时间<&", "气泡 & < \" 🙂", [main]);
                _ = document.AttributeProjections.CreateNote(XdeNoteProjectionKind.Comment, "", "", "", [main]);
                _ = document.AttributeProjections.CreateNote(XdeNoteProjectionKind.Comment, "001", "0123", "000", [main]);
                Assert.True(command.Commit());
            }
            document.Save(path, format);
            if (format == DocumentStorageFormat.XmlXcaf) _ = System.Xml.Linq.XDocument.Load(path);
            using var reopened = XdeDocument.Open(path);
            var data = reopened.Projections.Snapshot(reopened.Projections.GetLabel(main.Entry)).NamedData!;
            Assert.Equal(7, data.IntegerArrays["zero"]!.Values.Single()); Assert.Equal(0, data.IntegerArrays["zero"]!.LowerBound);
            Assert.Equal(Math.PI, data.Reals["precision"]); Assert.Equal(Math.PI, data.RealArrays["zero"]!.Values.Single());
            Assert.Equal("< & \" 中文🙂", data.Strings["key with space"]);
            var notes = reopened.AttributeProjections.Snapshot().Notes; Assert.Equal(3, notes.Count);
            var balloon = notes.Single(n => n.Kind == XdeNoteProjectionKind.Balloon);
            Assert.Equal("作者🙂", balloon.UserName); Assert.Equal("时间<&", balloon.TimeStamp); Assert.Equal("气泡 & < \" 🙂", balloon.Text);
            Assert.Contains(notes, n => n.Kind == XdeNoteProjectionKind.Comment && n.Text == "" && n.UserName == "" && n.TimeStamp == "");
            Assert.Contains(notes, n => n.Kind == XdeNoteProjectionKind.Comment && n.Text == "000" && n.UserName == "001" && n.TimeStamp == "0123");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static byte[] Payload(Action<BinaryWriter> write) { using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream, Encoding.UTF8, true); write(writer); writer.Flush(); return stream.ToArray(); }
    private static void Text(BinaryWriter writer, string value) { byte[] bytes = Encoding.UTF8.GetBytes(value); writer.Write(bytes.Length); writer.Write(bytes); }
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_document_projection_replace", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern int Replace(nint owner, int domain, byte* data, int length);

    [Fact]
    public unsafe void RawPayloadDecodeRejectsTruncationTypesUtf8CountsAndNonfiniteBeforeMutation()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(DocumentProjectionTests).Assembly);
        using var document = DocumentProjectionContext.CreateNativeDocument(); using var context = DocumentProjectionContext.Bind(document);
        var main = context.GetLabel("0:1"); document.NewCommand(); context.ReplaceNamedData(main, BatchASDocumentWorkflow.Values());
        byte[] valid = Payload(w => { w.Write(1); w.Write(1); w.Write(1); Text(w, main.Entry); w.Write(0); });
        for (int size = 0; size < valid.Length; size++) fixed (byte* p = valid) Assert.NotEqual(0, Replace(document.NativeHandle.DangerousGetHandle(), 0, p, size));
        var invalid = new List<byte[]>();
        foreach (int offset in new[] { 0, 4, 8 }) { var copy = (byte[])valid.Clone(); BitConverter.GetBytes(-1).CopyTo(copy, offset); invalid.Add(copy); }
        invalid.Add([.. valid, 7]);
        invalid.Add(Payload(w => { w.Write(1); w.Write(1); w.Write(1); w.Write(2); w.Write(new byte[] { 0xC0, 0xAF }); w.Write(0); }));
        invalid.Add(Payload(w => { w.Write(1); w.Write(1); w.Write(1); Text(w, "0:1\0"); w.Write(0); }));
        invalid.Add(Payload(w => { w.Write(1); w.Write(1); w.Write(1); Text(w, main.Entry); w.Write(2); }));
        invalid.Add(Payload(w => { w.Write(1); w.Write(1); w.Write(1); Text(w, main.Entry); w.Write(1); w.Write(0); w.Write(1); Text(w, "bad"); w.Write(double.NaN); }));
        invalid.Add(Payload(w => { w.Write(1); w.Write(1); w.Write(1); Text(w, main.Entry); w.Write(1); w.Write(4097); }));
        foreach (byte[] bytes in invalid) fixed (byte* p = bytes) Assert.NotEqual(0, Replace(document.NativeHandle.DangerousGetHandle(), 0, p, bytes.Length));
        fixed (byte* p = valid)
        {
            Assert.NotEqual(0, Replace(document.NativeHandle.DangerousGetHandle(), 1, p, valid.Length));
            Assert.NotEqual(0, Replace(document.NativeHandle.DangerousGetHandle(), 2, p, valid.Length));
            Assert.NotEqual(0, Replace(0, 0, p, valid.Length));
            Assert.NotEqual(0, Replace(document.NativeHandle.DangerousGetHandle(), 0, p, 16777217));
        }
        BatchASDocumentWorkflow.Verify(context.Snapshot(main).NamedData); Assert.True(document.CommitCommand());
    }

    [Fact]
    public unsafe void RawPayloadTypedCapacityStaleAndConcurrentReleaseGuards()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(DocumentProjectionTests).Assembly);
        using var document = DocumentProjectionContext.CreateNativeDocument();
        nint owner = document.NativeHandle.DangerousGetHandle();
        Assert.Equal(0, Snapshot(owner, 0, "0:1", out nint token));
        try
        {
            Assert.Equal(0, Size(token, 1, out int count)); Assert.True(count > 8);
            Assert.NotEqual(0, Size(token, 2, out _)); Assert.NotEqual(0, Size(token, 3, out _));
            byte[] bytes = Enumerable.Repeat((byte)0xA5, count).ToArray();
            fixed (byte* p = bytes) { Assert.NotEqual(0, Copy(token, 1, p, count - 1)); Assert.All(bytes, b => Assert.Equal(0xA5, b)); Assert.NotEqual(0, Copy(token, 2, p, count)); Assert.Equal(0, Copy(token, 1, p, count)); }
            Release(token, 2); Assert.Equal(0, Size(token, 1, out _));
            int failed = 0;
            Parallel.For(0, 128, i => { if (i == 0) Release(token, 1); else { int status = Size(token, 1, out int length); if (status == 0 && length != count) Interlocked.Increment(ref failed); } });
            Assert.Equal(0, failed); Assert.NotEqual(0, Size(token, 1, out _)); Release(token, 1);
            Assert.NotEqual(0, Size(owner, 1, out _)); Assert.NotEqual(0, Size(0, 1, out _));
            Assert.NotEqual(0, Snapshot(owner, 1, "0:1", out nint wrong)); Assert.Equal(0, wrong);
            document.Dispose(); Assert.NotEqual(0, Snapshot(owner, 0, "0:1", out nint stale)); Assert.Equal(0, stale);
        }
        finally { Release(token, 1); }
    }
    [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_document_projection_snapshot", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Snapshot(nint owner, int domain, string entry, out nint token);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_document_projection_payload_size", CallingConvention = CallingConvention.Cdecl)]
    private static extern int Size(nint token, int kind, out int count);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_document_projection_payload_copy", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern int Copy(nint token, int kind, byte* bytes, int capacity);
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_document_projection_payload_release", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Release(nint token, int kind);

    [Theory]
    [InlineData(DocumentStorageFormat.BinOcaf)]
    [InlineData(DocumentStorageFormat.XmlOcaf)]
    [InlineData(DocumentStorageFormat.BinXcaf)]
    [InlineData(DocumentStorageFormat.XmlXcaf)]
    public void OrdinarySdkSafeDataNeedsNoExtensionAndReopens(DocumentStorageFormat format)
    {
        string path = Path.Combine(Path.GetTempPath(), "AS-safe-" + Guid.NewGuid() + (format is DocumentStorageFormat.XmlOcaf or DocumentStorageFormat.XmlXcaf ? ".xml" : format == DocumentStorageFormat.BinXcaf ? ".xbf" : ".cbf"));
        try
        {
            if (format is DocumentStorageFormat.BinOcaf or DocumentStorageFormat.XmlOcaf)
            {
                using var doc = OcafDocument.Create();
                using (var command = doc.BeginTransaction()) { doc.Projections.ReplaceNamedData(doc.Projections.GetLabel("0:1"), new(integers: [new("zero", 0)], bytes: [new("b", 255)], integerArrays: [new("values", new(1, [3, 2, 1]))])); Assert.True(command.Commit()); }
                doc.Save(path, format); using var opened = OcafDocument.Open(path); VerifySafe(opened.Projections);
            }
            else
            {
                using var doc = XdeDocument.Create();
                using (var command = doc.BeginTransaction()) { doc.Projections.ReplaceNamedData(doc.Projections.GetLabel("0:1"), new(integers: [new("zero", 0)], bytes: [new("b", 255)], integerArrays: [new("values", new(1, [3, 2, 1]))])); Assert.True(command.Commit()); }
                doc.Save(path, format); using var opened = XdeDocument.Open(path); VerifySafe(opened.Projections);
            }
            byte[] file = File.ReadAllBytes(path); Assert.DoesNotContain("occtsharp_nameddata_v1", Encoding.UTF8.GetString(file), StringComparison.Ordinal);
            Assert.False(file.AsSpan().IndexOf(BitConverter.GetBytes(0x4F534E44)) >= 0);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
        static void VerifySafe(DocumentProjectionContext context) { var data = context.Snapshot(context.GetLabel("0:1")).NamedData!; Assert.Equal(0, data.Integers["zero"]); Assert.Equal(255, data.Bytes["b"]); Assert.Equal<int>([3, 2, 1], data.IntegerArrays["values"]!.Values); }
    }

    [Fact]
    public unsafe void FailedNoteAssociationLeavesNoCreatedToolOrNoteAndKeepsEarlierData()
    {
        NativeLibraryResolver.EnsureRegistered(typeof(DocumentProjectionTests).Assembly);
        using var document = DocumentProjectionContext.CreateNativeDocument(); using var context = DocumentProjectionContext.Bind(document); using var xde = XdeAttributeProjectionContext.Bind(context);
        document.NewCommand(); var main = context.GetLabel("0:1"); context.ReplaceNamedData(main, BatchASDocumentWorkflow.Values());
        int before = context.Snapshot(main).SubtreeAttributeCount;
        byte[] bytes = Payload(w => { w.Write(1); w.Write(2); w.Write(1); Text(w, "author"); Text(w, "time"); Text(w, "text"); w.Write(2); Text(w, main.Entry); Text(w, "0:1:999999"); });
        fixed (byte* p = bytes) { Assert.NotEqual(0, CreateNote(document.NativeHandle.DangerousGetHandle(), 0, p, bytes.Length, out nint token)); Assert.Equal(0, token); }
        Assert.Empty(xde.Snapshot().Notes); Assert.Empty(xde.Snapshot(main).TargetNoteEntries); Assert.Equal(before, context.Snapshot(main).SubtreeAttributeCount);
        BatchASDocumentWorkflow.Verify(context.Snapshot(main).NamedData); Assert.True(document.CommitCommand());
    }
    [DllImport("OcctSharp.Native", EntryPoint = "occtsharp_xde_attribute_projection_create_note", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern int CreateNote(nint owner, int domain, byte* data, int length, out nint token);

    [Fact]
    public void CorruptedXmlExtensionFailsRetrievalWithoutChangingLiveSourceOrFile()
    {
        string path = Path.Combine(Path.GetTempPath(), "AS-invalid-" + Guid.NewGuid() + ".xml");
        try
        {
            using var doc = OcafDocument.Create();
            using (var command = doc.BeginTransaction()) { doc.Projections.ReplaceNamedData(doc.Projections.GetLabel("0:1"), BatchASDocumentWorkflow.Values()); Assert.True(command.Commit()); }
            doc.Save(path, DocumentStorageFormat.XmlOcaf);
            string xml = File.ReadAllText(path); int marker = xml.IndexOf("occtsharp_nameddata_v1=\"", StringComparison.Ordinal);
            Assert.True(marker >= 0); int value = marker + "occtsharp_nameddata_v1=\"".Length;
            xml = string.Concat(xml.AsSpan(0, value), "GG", xml.AsSpan(value + 2)); File.WriteAllText(path, xml);
            Assert.ThrowsAny<Exception>(() => { using var ignored = OcafDocument.Open(path); });
            Assert.Equal(xml, File.ReadAllText(path)); BatchASDocumentWorkflow.Verify(doc.Projections.Snapshot(doc.Projections.GetLabel("0:1")).NamedData);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
