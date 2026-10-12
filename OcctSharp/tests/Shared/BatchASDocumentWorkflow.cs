using OcctSharp.DocumentProjections;
using OcctSharp.XdeAttributeProjections;

namespace OcctSharp.Tests.Shared;

internal static class BatchASDocumentWorkflow
{
    internal static NamedDataValues Values() => new(
        integers: new Dictionary<string, int> { ["zero"] = 0, ["n"] = 17 },
        reals: new Dictionary<string, double> { ["zero"] = 0, ["ratio"] = 2.25 },
        strings: new Dictionary<string, string> { ["empty"] = "", ["unicode"] = "图层🙂" },
        bytes: new Dictionary<string, byte> { ["zero"] = 0, ["max"] = 255 },
        integerArrays: new Dictionary<string, NamedDataArray<int>?> { ["bounded"] = new(-2, [4, 0, 7]), ["empty"] = new(9, []), ["null"] = null },
        realArrays: new Dictionary<string, NamedDataArray<double>?> { ["bounded"] = new(4, [.1, 0, 2.5]), ["empty"] = new(-5, []), ["null"] = null });

    internal static void Verify(NamedDataValues? data)
    {
        if (data is null || data.Integers["zero"] != 0 || data.Integers["n"] != 17 || data.Reals["ratio"] != 2.25
            || data.Strings["empty"] != "" || data.Strings["unicode"] != "图层🙂" || data.Bytes["max"] != 255
            || data.IntegerArrays["bounded"]!.LowerBound != -2 || !data.IntegerArrays["bounded"]!.Values.SequenceEqual([4, 0, 7])
            || data.IntegerArrays["empty"]!.LowerBound != 9 || data.IntegerArrays["empty"]!.Values.Count != 0 || data.IntegerArrays["null"] is not null
            || data.RealArrays["bounded"]!.LowerBound != 4 || data.RealArrays["bounded"]!.Values[0] != .1
            || data.RealArrays["empty"]!.LowerBound != -5 || data.RealArrays["empty"]!.Values.Count != 0 || data.RealArrays["null"] is not null)
            throw new InvalidOperationException("AS copied complete data/null/empty/bounds failed.");
    }

    internal static void RunNative()
    {
        if (typeof(NamedDataValues).Assembly.GetName().Name != "OcctSharp.Documents"
            || typeof(XdeAttributeProjectionContext).Assembly.GetName().Name != "OcctSharp.Xde")
            throw new InvalidOperationException("AS projection assembly identity changed.");
        using var native = DocumentProjectionContext.CreateNativeDocument();
        using var context = DocumentProjectionContext.Bind(native);
        using var xde = XdeAttributeProjectionContext.Bind(context);
        var main = context.GetLabel("0:1"); var root = context.GetLabel("0:");
        var before = context.Snapshot(main); var empty = xde.Snapshot();
        if (before.NamedData is not null || empty.Layers.Count != 0 || empty.Notes.Count != 0
            || context.Snapshot(main).SubtreeAttributeCount != before.SubtreeAttributeCount)
            throw new InvalidOperationException("AS reads created missing tools/data.");
        native.NewCommand();
        context.ReplaceNamedData(main, Values());
        var layer = xde.CreateLayer("图层层", false); xde.AssignLayer(main, layer);
        var comment = xde.CreateNote(XdeNoteProjectionKind.Comment, "作者🙂", "2026-10-07", "真实评论", [main]);
        _ = xde.CreateNote(XdeNoteProjectionKind.Balloon, "作者", "stamp", "气泡", [root]);
        if (!native.CommitCommand()) throw new InvalidOperationException("AS existing command did not commit.");
        var detached = context.Snapshot(main); Verify(detached.NamedData);
        var snapshot = xde.Snapshot(main);
        if (snapshot.Layers.Count != 1 || snapshot.Layers[0].IsVisible || !snapshot.Layers[0].AssociatedEntries.Contains(main.Entry)
            || snapshot.Notes.Count != 2 || snapshot.TargetNoteEntries.Count != 1 || snapshot.TargetNoteEntries[0] != comment.Entry
            || snapshot.Notes.Single(n => n.Entry == comment.Entry).Text != "真实评论" || !snapshot.TargetIsAnnotated
            || !snapshot.Annotations.Any(a => a.ItemPath == main.Entry && a.NoteEntries.Contains(comment.Entry)))
            throw new InvalidOperationException("AS native layer/note/association workflow failed.");
        if (!native.Undo() || context.Snapshot(main).NamedData is not null || xde.Snapshot().Notes.Count != 0)
            throw new InvalidOperationException("AS undo retained new data/notes.");
        if (!native.Redo()) throw new InvalidOperationException("AS redo failed.");
        Verify(context.Snapshot(main).NamedData);
        native.NewCommand(); context.ReplaceNamedData(main, new());
        if (!native.CommitCommand() || context.Snapshot(main).NamedData!.Integers.Count != 0)
            throw new InvalidOperationException("AS empty-category replacement failed.");
        if (!native.Undo()) throw new InvalidOperationException("AS replacement undo failed."); Verify(context.Snapshot(main).NamedData);
        if (!native.Redo() || context.Snapshot(main).NamedData!.Integers.Count != 0)
            throw new InvalidOperationException("AS replacement redo retained old keys.");
        native.Dispose(); context.Dispose(); xde.Dispose(); Verify(detached.NamedData);
        if (snapshot.Notes.Count != 2) throw new InvalidOperationException("AS copied snapshots depended on native lifetime.");
    }

#if !DIRECT_MODULE_CONSUMER
    internal static void RunFacade()
    {
        string directory = Path.Combine(Path.GetTempPath(), "OcctSharp.AS." + Guid.NewGuid()); Directory.CreateDirectory(directory);
        try
        {
            using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(2, 3, 4);
            XdeLabel part, first, second; string noteEntry;
            using (var transaction = document.BeginTransaction("AS repeated instance metadata"))
            {
                part = document.AddShape(box, "共享部件"); var assembly = document.AddAssembly("AS assembly");
                using var identity = TopLocLocation.Identity;
                first = document.AddComponent(assembly, part, identity); second = document.AddComponent(assembly, part, identity);
                var data = document.Projections; var xde = document.AttributeProjections;
                var definition = data.GetLabel(part.Entry); var a = data.GetLabel(first.Entry); var b = data.GetLabel(second.Entry);
                data.ReplaceNamedData([new(definition, Values()), new(a, new(integers: new Dictionary<string, int> { ["instance"] = 1 })), new(b, new(integers: new Dictionary<string, int> { ["instance"] = 2 }))]);
                var layer = xde.CreateLayer("实例图层", false); xde.AssignLayer(a, layer);
                noteEntry = xde.CreateNote(XdeNoteProjectionKind.Comment, "作者", "原始时间", "第一实例注释", [a]).Entry;
                if (!transaction.Commit()) throw new InvalidOperationException("AS named facade command failed.");
            }
            var firstSnapshot = document.AttributeProjections.Snapshot(document.Projections.GetLabel(first.Entry));
            if (firstSnapshot.TargetNoteEntries.Single() != noteEntry
                || document.AttributeProjections.Snapshot(document.Projections.GetLabel(second.Entry)).TargetNoteEntries.Count != 0
                || document.AttributeProjections.Snapshot(document.Projections.GetLabel(part.Entry)).TargetNoteEntries.Count != 0)
                throw new InvalidOperationException("AS definition/occurrence annotation domains were mixed.");
            if (!document.Undo() || !document.Redo()) throw new InvalidOperationException("AS named facade history failed.");
            foreach (var format in new[] { DocumentStorageFormat.BinXcaf, DocumentStorageFormat.XmlXcaf })
            {
                string path = Path.Combine(directory, format == DocumentStorageFormat.BinXcaf ? "metadata.xbf" : "metadata.xml");
                document.Save(path, format);
                if (format == DocumentStorageFormat.XmlXcaf) _ = System.Xml.Linq.XDocument.Load(path);
                using var reopened = XdeDocument.Open(path);
                try { Verify(reopened.Projections.Snapshot(reopened.Projections.GetLabel(part.Entry)).NamedData); }
                catch (Exception error) { throw new InvalidOperationException($"AS {format} complete NamedData readback failed.", error); }
                var xde = reopened.AttributeProjections.Snapshot(reopened.Projections.GetLabel(first.Entry));
                if (xde.TargetNoteEntries.Single() != noteEntry || xde.Notes.Single(n => n.Entry == noteEntry).Text != "第一实例注释"
                    || xde.Layers.Single().IsVisible || !xde.Layers.Single().AssociatedEntries.Contains(first.Entry)
                    || reopened.GetLabel(first.Entry).ReferredShape.Entry != part.Entry || reopened.GetLabel(second.Entry).ReferredShape.Entry != part.Entry)
                    throw new InvalidOperationException($"AS {format} actual XCAF reopen lost metadata/instance identity: {System.Text.Json.JsonSerializer.Serialize(xde)}; referred={reopened.GetLabel(first.Entry).ReferredShape.Entry},{reopened.GetLabel(second.Entry).ReferredShape.Entry}; expected={part.Entry}");
            }
        }
        finally { Directory.Delete(directory, true); }
    }
#endif
    internal static void Run()
    {
        RunNative();
#if !DIRECT_MODULE_CONSUMER
        RunFacade();
#endif
    }
}
