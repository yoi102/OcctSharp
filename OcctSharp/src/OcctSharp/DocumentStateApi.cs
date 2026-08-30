using System.Runtime.InteropServices;
using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

internal static unsafe class DocumentStateApi
{
    private const int EntryCapacity = 4096;
    private const int GuidCapacity = 64;
    private const int TypeCapacity = 512;

    internal static string Save(
        OcafDocumentHandle document,
        string filePath,
        DocumentStorageFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        (int xde, int xml) = format switch
        {
            DocumentStorageFormat.BinOcaf => (0, 0),
            DocumentStorageFormat.XmlOcaf => (0, 1),
            DocumentStorageFormat.BinXcaf => (1, 0),
            DocumentStorageFormat.XmlXcaf => (1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SaveFormat(document, fullPath, xde, xml),
            "document_save_format");
        return fullPath;
    }

    internal static DocumentLabelSnapshot SnapshotLabel(OcafDocumentHandle document, string entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        string[] children = GetChildren(document, entry);
        unsafe
        {
            string? parent = null;
            int tag;
            int depth;
            int isRoot;
            int hasParent;
            int written;
            byte[] buffer = new byte[EntryCapacity];
            fixed (byte* pointer = buffer)
            {
                NativeError.ThrowIfFailed(
                    DocumentStateNativeMethods.GetLabelInfo(
                        document, entry, out tag, out depth, out isRoot, out hasParent,
                        pointer, buffer.Length, out written),
                    "document_label_info");
            }
            if (hasParent != 0) parent = Encoding.UTF8.GetString(buffer, 0, written);
            return new DocumentLabelSnapshot(
                entry, tag, depth, isRoot != 0, parent, children, GetAttributes(document, entry));
        }
    }

    internal static DocumentSnapshot Snapshot(OcafDocumentHandle document)
    {
        List<DocumentLabelSnapshot> labels = [];
        Stack<string> pending = new();
        pending.Push("0");
        try
        {
            while (pending.Count > 0)
            {
                DocumentLabelSnapshot label = SnapshotLabel(document, pending.Pop());
                labels.Add(label);
                for (int index = label.ChildEntries.Count - 1; index >= 0; --index)
                    pending.Push(label.ChildEntries[index]);
            }
            return new DocumentSnapshot(labels);
        }
        catch
        {
            foreach (Shape shape in labels.SelectMany(static label => label.Attributes)
                         .Select(static attribute => attribute.NamedShape)
                         .OfType<Shape>())
                shape.Dispose();
            throw;
        }
    }

    internal static DocumentDependencyGraph BuildGraph(
        DocumentSnapshot snapshot,
        IEnumerable<DocumentDependencyEdge>? additionalEdges = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        List<DocumentDependencyEdge> edges = [];
        foreach (DocumentLabelSnapshot label in snapshot.Labels)
        {
            foreach (DocumentAttributeSnapshot attribute in label.Attributes)
            {
                if (attribute.ReferenceEntry is string target)
                    edges.Add(new(label.Entry, target, DocumentDependencyEdgeKind.DirectReference));
                for (int index = 0; index < attribute.ReferenceEntries.Count; ++index)
                    edges.Add(new(label.Entry, attribute.ReferenceEntries[index],
                        DocumentDependencyEdgeKind.ReferenceArray, index));
                if (attribute.Tree is DocumentTreeSnapshot tree)
                    for (int index = 0; index < tree.ChildEntries.Count; ++index)
                        edges.Add(new(label.Entry, tree.ChildEntries[index],
                            DocumentDependencyEdgeKind.TreeNode, index));
            }
        }
        if (additionalEdges is not null) edges.AddRange(additionalEdges);
        return new DocumentDependencyGraph(snapshot.Labels.Select(static label => label.Entry), edges);
    }

    internal static string? GetText(
        OcafDocumentHandle document,
        string entry,
        DocumentAttributeKind kind)
    {
        ValidateTextKind(kind);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetTextInfo(document, entry, (int)kind, out int hasValue, out int length),
            "document_text_info");
        return hasValue == 0 ? null : ReadSized(length, (byte* buffer, int capacity, out int written) =>
            DocumentStateNativeMethods.CopyText(document, entry, (int)kind, buffer, capacity, out written),
            "document_text_copy");
    }

    internal static void SetText(
        OcafDocumentHandle document,
        string entry,
        DocumentAttributeKind kind,
        string value)
    {
        ValidateTextKind(kind);
        ArgumentNullException.ThrowIfNull(value);
        using Utf8Buffer utf8 = Utf8Buffer.FromString(value);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetText(document, entry, (int)kind, utf8.Pointer, utf8.Length),
            "document_text_set");
    }

    internal static int? GetInteger(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetScalar(document, entry, (int)DocumentAttributeKind.IntegralValue,
                out int hasValue, out int value, out _),
            "document_integer_get");
        return hasValue == 0 ? null : value;
    }

    internal static void SetInteger(OcafDocumentHandle document, string entry, int value) =>
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetScalar(document, entry, (int)DocumentAttributeKind.IntegralValue, value, 0),
            "document_integer_set");

    internal static double? GetReal(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetScalar(document, entry, (int)DocumentAttributeKind.Real,
                out int hasValue, out _, out double value),
            "document_real_get");
        return hasValue == 0 ? null : value;
    }

    internal static void SetReal(OcafDocumentHandle document, string entry, double value)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetScalar(document, entry, (int)DocumentAttributeKind.Real, 0, value),
            "document_real_set");
    }

    internal static DocumentIntegerArray? GetIntegerArray(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetArrayInfo(document, entry, (int)DocumentAttributeKind.IntegerArray,
                out int hasValue, out int lower, out int count),
            "document_integer_array_info");
        if (hasValue == 0) return null;
        int[] values = new int[count];
        unsafe
        {
            fixed (int* pointer = values)
                NativeError.ThrowIfFailed(
                    DocumentStateNativeMethods.CopyIntegerArray(document, entry, pointer, values.Length, out int written),
                    "document_integer_array_copy");
        }
        return new DocumentIntegerArray(lower, values);
    }

    internal static void SetIntegerArray(
        OcafDocumentHandle document,
        string entry,
        int lowerBound,
        IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int[] copied = [.. values];
        _ = checked(lowerBound + copied.Length - (copied.Length == 0 ? 0 : 1));
        unsafe
        {
            fixed (int* pointer = copied)
                NativeError.ThrowIfFailed(
                    DocumentStateNativeMethods.SetIntegerArray(document, entry, lowerBound, pointer, copied.Length),
                    "document_integer_array_set");
        }
    }

    internal static DocumentRealArray? GetRealArray(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetArrayInfo(document, entry, (int)DocumentAttributeKind.RealArray,
                out int hasValue, out int lower, out int count),
            "document_real_array_info");
        if (hasValue == 0) return null;
        double[] values = new double[count];
        unsafe
        {
            fixed (double* pointer = values)
                NativeError.ThrowIfFailed(
                    DocumentStateNativeMethods.CopyRealArray(document, entry, pointer, values.Length, out int written),
                    "document_real_array_copy");
        }
        return new DocumentRealArray(lower, values);
    }

    internal static void SetRealArray(
        OcafDocumentHandle document,
        string entry,
        int lowerBound,
        IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        double[] copied = [.. values];
        if (copied.Any(static value => !double.IsFinite(value)))
            throw new ArgumentOutOfRangeException(nameof(values), "Real-array values must be finite.");
        _ = checked(lowerBound + copied.Length - (copied.Length == 0 ? 0 : 1));
        unsafe
        {
            fixed (double* pointer = copied)
                NativeError.ThrowIfFailed(
                    DocumentStateNativeMethods.SetRealArray(document, entry, lowerBound, pointer, copied.Length),
                    "document_real_array_set");
        }
    }

    internal static string? GetReference(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetReferenceInfo(document, entry, 0, out int hasValue, out _),
            "document_reference_info");
        return hasValue == 0 ? null : ReadReference(document, entry, false, 1);
    }

    internal static void SetReference(OcafDocumentHandle document, string entry, string targetEntry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetEntry);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetReference(document, entry, targetEntry),
            "document_reference_set");
    }

    internal static IReadOnlyList<string> GetReferenceArray(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetReferenceInfo(document, entry, 1, out int hasValue, out int count),
            "document_reference_array_info");
        if (hasValue == 0) return [];
        string[] entries = new string[count];
        for (int index = 0; index < count; ++index)
            entries[index] = ReadReference(document, entry, true, index + 1);
        return entries;
    }

    internal static void SetReferenceArray(
        OcafDocumentHandle document,
        string entry,
        IReadOnlyList<string> targetEntries)
    {
        ArgumentNullException.ThrowIfNull(targetEntries);
        nint[] pointers = new nint[targetEntries.Count];
        try
        {
            for (int index = 0; index < targetEntries.Count; ++index)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(targetEntries[index]);
                pointers[index] = Marshal.StringToCoTaskMemUTF8(targetEntries[index]);
            }
            unsafe
            {
                fixed (nint* pointer = pointers)
                    NativeError.ThrowIfFailed(
                        DocumentStateNativeMethods.SetReferenceArray(
                            document, entry, (nint)pointer, pointers.Length),
                        "document_reference_array_set");
            }
        }
        finally
        {
            foreach (nint pointer in pointers)
                if (pointer != 0) Marshal.FreeCoTaskMem(pointer);
        }
    }

    internal static DocumentTreeSnapshot? GetTree(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetTreeInfo(document, entry,
                out int hasNode, out int hasParent, out int childCount),
            "document_tree_info");
        if (hasNode == 0) return null;
        string? parent = hasParent == 0 ? null : ReadTreeEntry(document, entry, true, 1);
        string[] children = new string[childCount];
        for (int index = 0; index < childCount; ++index)
            children[index] = ReadTreeEntry(document, entry, false, index + 1);
        return new DocumentTreeSnapshot(parent, children);
    }

    internal static void ReparentTree(OcafDocumentHandle document, string entry, string parentEntry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentEntry);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.ReparentTree(document, entry, parentEntry),
            "document_tree_reparent");
    }

    internal static void DetachTree(OcafDocumentHandle document, string entry) =>
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.DetachTree(document, entry),
            "document_tree_detach");

    internal static Shape? GetNamedShape(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetNamedShape(document, entry, out int hasShape, out nint shape),
            "document_named_shape_get");
        return hasShape == 0 ? null : ShapeFactory.FromNativeHandle(shape, "document_named_shape_get");
    }

    internal static void SetNamedShape(OcafDocumentHandle document, string entry, Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetNamedShape(document, entry, shape.Handle),
            "document_named_shape_set");
    }

    internal static void RemoveAttribute(
        OcafDocumentHandle document,
        string entry,
        DocumentAttributeKind kind)
    {
        if (kind is DocumentAttributeKind.Unknown) throw new ArgumentOutOfRangeException(nameof(kind));
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.RemoveAttribute(document, entry, (int)kind),
            "document_attribute_remove");
    }

    internal static bool CommitNamedCommand(OcafDocumentHandle document, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        using Utf8Buffer utf8 = Utf8Buffer.FromString(name);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.CommitNamedCommand(
                document, utf8.Pointer, utf8.Length, out int changed),
            "document_commit_named_command");
        return changed != 0;
    }

    internal static DocumentHistoryState GetHistoryState(OcafDocumentHandle document)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetHistoryState(document,
                out int undoLimit, out int undoCount, out int redoCount, out int isChanged),
            "document_history_state");
        return new DocumentHistoryState(undoLimit, undoCount, redoCount, isChanged != 0);
    }

    internal static void SetUndoLimit(OcafDocumentHandle document, int undoLimit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(undoLimit, -1);
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.SetHistoryLimit(document, undoLimit),
            "document_history_set_limit");
    }

    internal static bool Undo(OcafDocumentHandle document) => ApplyHistoryAction(document, 0, "document_undo");
    internal static bool Redo(OcafDocumentHandle document) => ApplyHistoryAction(document, 1, "document_redo");
    internal static void ClearUndos(OcafDocumentHandle document) => ApplyHistoryAction(document, 2, "document_clear_undos");
    internal static void ClearRedos(OcafDocumentHandle document) => ApplyHistoryAction(document, 3, "document_clear_redos");
    internal static void MarkSaved(OcafDocumentHandle document) => ApplyHistoryAction(document, 4, "document_mark_saved");

    internal static IReadOnlyList<DocumentHistoryEntry> GetHistory(OcafDocumentHandle document, bool redo)
    {
        DocumentHistoryState state = GetHistoryState(document);
        int count = redo ? state.AvailableRedos : state.AvailableUndos;
        DocumentHistoryEntry[] entries = new DocumentHistoryEntry[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            NativeError.ThrowIfFailed(
                DocumentStateNativeMethods.GetHistoryEntryInfo(
                    document, redo ? 1 : 0, nativeIndex,
                    out int beginTime, out int endTime, out int deltaCount,
                    out int labelCount, out int nameLength),
                "document_history_entry_info");
            string name = ReadSized(nameLength, (byte* buffer, int capacity, out int written) =>
                DocumentStateNativeMethods.CopyHistoryEntryName(
                    document, redo ? 1 : 0, nativeIndex, buffer, capacity, out written),
                "document_history_entry_name");
            string[] labels = new string[labelCount];
            for (int labelIndex = 0; labelIndex < labelCount; ++labelIndex)
                labels[labelIndex] = ReadFixed((byte* buffer, int capacity, out int written) =>
                    DocumentStateNativeMethods.CopyHistoryEntryLabel(
                        document, redo ? 1 : 0, nativeIndex, labelIndex + 1,
                        buffer, capacity, out written),
                    "document_history_entry_label");
            entries[index] = new DocumentHistoryEntry(name, beginTime, endTime, deltaCount, labels);
        }
        return entries;
    }

    private static List<DocumentAttributeSnapshot> GetAttributes(
        OcafDocumentHandle document,
        string entry)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.GetAttributeCount(document, entry, out int count),
            "document_attribute_count");
        List<DocumentAttributeSnapshot> attributes = new(count);
        try
        {
            for (int index = 0; index < count; ++index)
            {
                (DocumentAttributeKind kind, string id, string nativeType) =
                    GetAttributeInfo(document, entry, index + 1);
                string? text = kind is DocumentAttributeKind.Name or DocumentAttributeKind.Comment or DocumentAttributeKind.AsciiString
                    ? GetText(document, entry, kind) : null;
                int? integer = kind == DocumentAttributeKind.IntegralValue ? GetInteger(document, entry) : null;
                double? real = kind == DocumentAttributeKind.Real ? GetReal(document, entry) : null;
                DocumentIntegerArray? integers = kind == DocumentAttributeKind.IntegerArray
                    ? GetIntegerArray(document, entry) : null;
                DocumentRealArray? reals = kind == DocumentAttributeKind.RealArray
                    ? GetRealArray(document, entry) : null;
                string? reference = kind == DocumentAttributeKind.Reference ? GetReference(document, entry) : null;
                IReadOnlyList<string> references = kind == DocumentAttributeKind.ReferenceArray
                    ? GetReferenceArray(document, entry) : [];
                DocumentTreeSnapshot? tree = kind == DocumentAttributeKind.TreeNode
                    ? GetTree(document, entry) : null;
                Shape? shape = kind == DocumentAttributeKind.NamedShape ? GetNamedShape(document, entry) : null;
                attributes.Add(new DocumentAttributeSnapshot(
                    kind, id, nativeType, text, integer, real, integers, reals,
                    reference, references, tree, shape));
            }
            return attributes;
        }
        catch
        {
            foreach (Shape shape in attributes.Select(static attribute => attribute.NamedShape).OfType<Shape>())
                shape.Dispose();
            throw;
        }
    }

    private static unsafe (DocumentAttributeKind Kind, string Id, string NativeType) GetAttributeInfo(
        OcafDocumentHandle document,
        string entry,
        int index)
    {
        byte[] id = new byte[GuidCapacity];
        byte[] type = new byte[TypeCapacity];
        int kind;
        int idWritten;
        int typeWritten;
        fixed (byte* idPointer = id)
        fixed (byte* typePointer = type)
            NativeError.ThrowIfFailed(
                DocumentStateNativeMethods.GetAttributeInfo(
                    document, entry, index, out kind,
                    idPointer, id.Length, out idWritten,
                    typePointer, type.Length, out typeWritten),
                "document_attribute_info");
        return ((DocumentAttributeKind)kind,
            Encoding.UTF8.GetString(id, 0, idWritten),
            Encoding.UTF8.GetString(type, 0, typeWritten));
    }

    private static string[] GetChildren(OcafDocumentHandle document, string entry)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.GetOcafChildCount(document, entry, out int count),
            "document_child_count");
        string[] children = new string[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            children[index] = ReadFixed((byte* buffer, int capacity, out int written) =>
                DocumentStateNativeMethods.GetChildEntry(
                    document, entry, nativeIndex, buffer, capacity, out written),
                "document_child_entry");
        }
        return children;
    }

    private static string ReadReference(
        OcafDocumentHandle document,
        string entry,
        bool array,
        int index) => ReadFixed((byte* buffer, int capacity, out int written) =>
            DocumentStateNativeMethods.GetReferenceEntry(
                document, entry, array ? 1 : 0, index, buffer, capacity, out written),
            "document_reference_entry");

    private static string ReadTreeEntry(
        OcafDocumentHandle document,
        string entry,
        bool parent,
        int index) => ReadFixed((byte* buffer, int capacity, out int written) =>
            DocumentStateNativeMethods.GetTreeEntry(
                document, entry, parent ? 1 : 0, index, buffer, capacity, out written),
            "document_tree_entry");

    private static bool ApplyHistoryAction(OcafDocumentHandle document, int action, string operation)
    {
        NativeError.ThrowIfFailed(
            DocumentStateNativeMethods.ApplyHistoryAction(document, action, out int changed),
            operation);
        return changed != 0;
    }

    private static void ValidateTextKind(DocumentAttributeKind kind)
    {
        if (kind is not (DocumentAttributeKind.Name or DocumentAttributeKind.Comment or DocumentAttributeKind.AsciiString))
            throw new ArgumentOutOfRangeException(nameof(kind));
    }

    private static unsafe string ReadFixed(Utf8Reader reader, string operation) =>
        ReadBuffer(EntryCapacity, reader, operation);

    private static unsafe string ReadSized(int length, Utf8Reader reader, string operation) =>
        ReadBuffer(checked(length + 1), reader, operation);

    private static unsafe string ReadBuffer(int capacity, Utf8Reader reader, string operation)
    {
        byte[] buffer = new byte[Math.Max(1, capacity)];
        fixed (byte* pointer = buffer)
        {
            NativeError.ThrowIfFailed(reader(pointer, buffer.Length, out int written), operation);
            return Encoding.UTF8.GetString(buffer, 0, written);
        }
    }

    private unsafe delegate NativeStatus Utf8Reader(byte* buffer, int capacity, out int written);
}
