using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns one binary/XML-persistable OCAF document and its command history.</summary>
public sealed class OcafDocument : IDisposable
{
    private OcafDocument(OcafDocumentHandle handle)
    {
        Handle = handle;
        RootLabel = new OcafLabel(this, ReadMainEntry());
    }

    internal OcafDocumentHandle Handle { get; }

    /// <summary>Gets the document's main label. The label is parent-bound to this document.</summary>
    public OcafLabel RootLabel { get; }

    /// <summary>Gets whether a transaction is currently open.</summary>
    public bool HasOpenTransaction
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(
                NativeMethods.HasOpenOcafCommand(Handle, out int open),
                "ocaf_document_has_open_command");
            return open != 0;
        }
    }

    /// <summary>Gets a copied view of the current undo, redo, and dirty state.</summary>
    public DocumentHistoryState HistoryState
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistoryState(Handle); }
    }

    /// <summary>Gets or sets the bounded undo depth; -1 means unlimited and zero disables history.</summary>
    public int UndoLimit
    {
        get => HistoryState.UndoLimit;
        set { ThrowIfDisposed(); DocumentStateApi.SetUndoLimit(Handle, value); }
    }

    /// <summary>Gets whether the document differs from its current savepoint.</summary>
    public bool IsChanged => HistoryState.IsChanged;

    /// <summary>Gets copied undo-history entries without exposing native deltas.</summary>
    public IReadOnlyList<DocumentHistoryEntry> UndoHistory
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistory(Handle, false); }
    }

    /// <summary>Gets copied redo-history entries without exposing native deltas.</summary>
    public IReadOnlyList<DocumentHistoryEntry> RedoHistory
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistory(Handle, true); }
    }

    /// <summary>Creates an empty BinOcaf document.</summary>
    public static OcafDocument Create()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateOcafDocument(out nint nativeDocument),
            "ocaf_document_create");
        return new OcafDocument(new OcafDocumentHandle(nativeDocument));
    }

    /// <summary>Opens a binary OCAF document.</summary>
    public static OcafDocument Open(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The binary OCAF document does not exist.", fullPath);
        }

        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.OpenOcafDocument(fullPath, out nint nativeDocument),
            "ocaf_document_open");
        return new OcafDocument(new OcafDocumentHandle(nativeDocument));
    }

    /// <summary>Begins a transaction. Disposing it without commit aborts its changes.</summary>
    public OcafTransaction BeginTransaction()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.BeginOcafCommand(Handle), "ocaf_document_begin_command");
        return new OcafTransaction(this, null);
    }

    /// <summary>Begins a named command. Disposing it without commit aborts its changes.</summary>
    public OcafTransaction BeginTransaction(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.BeginOcafCommand(Handle), "ocaf_document_begin_command");
        return new OcafTransaction(this, name);
    }

    /// <summary>Resolves an existing label by its stable TDF entry.</summary>
    public OcafLabel GetLabel(string entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        _ = GetChildCount(entry);
        return new OcafLabel(this, entry);
    }

    /// <summary>Saves this document in OCCT's binary OCAF format.</summary>
    public string Save(string filePath)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        NativeError.ThrowIfFailed(NativeMethods.SaveOcafDocument(Handle, fullPath), "ocaf_document_save");
        return fullPath;
    }

    /// <summary>Saves this generic document as BinOcaf or XmlOcaf.</summary>
    public string Save(string filePath, DocumentStorageFormat format)
    {
        ThrowIfDisposed();
        if (format is not (DocumentStorageFormat.BinOcaf or DocumentStorageFormat.XmlOcaf))
            throw new ArgumentOutOfRangeException(nameof(format), "A generic OCAF document requires BinOcaf or XmlOcaf.");
        return DocumentStateApi.Save(Handle, filePath, format);
    }

    /// <summary>Copies the complete label/attribute table. The snapshot survives this document.</summary>
    public DocumentSnapshot CreateSnapshot()
    {
        ThrowIfDisposed();
        return DocumentStateApi.Snapshot(Handle);
    }

    /// <summary>Builds a managed-owned dependency graph from one copied document snapshot.</summary>
    public DocumentDependencyGraph CreateDependencyGraph()
    {
        using DocumentSnapshot snapshot = CreateSnapshot();
        return DocumentStateApi.BuildGraph(snapshot);
    }

    /// <summary>Undoes one committed command and reports whether state changed.</summary>
    public bool Undo() { ThrowIfDisposed(); return DocumentStateApi.Undo(Handle); }

    /// <summary>Redoes one undone command and reports whether state changed.</summary>
    public bool Redo() { ThrowIfDisposed(); return DocumentStateApi.Redo(Handle); }

    /// <summary>Clears all undo entries.</summary>
    public void ClearUndoHistory() { ThrowIfDisposed(); DocumentStateApi.ClearUndos(Handle); }

    /// <summary>Clears all redo entries.</summary>
    public void ClearRedoHistory() { ThrowIfDisposed(); DocumentStateApi.ClearRedos(Handle); }

    /// <summary>Marks the current document time as the clean savepoint.</summary>
    public void MarkSaved() { ThrowIfDisposed(); DocumentStateApi.MarkSaved(Handle); }

    internal int AddChild(string parentEntry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.AddOcafChild(Handle, parentEntry, out int childTag),
            "ocaf_label_add_child");
        return childTag;
    }

    internal int GetChildCount(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetOcafChildCount(Handle, entry, out int count),
            "ocaf_label_child_count");
        return count;
    }

    internal void SetName(string entry, string value)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(value);
        using Utf8Buffer utf8 = Utf8Buffer.FromString(value);
        NativeError.ThrowIfFailed(
            NativeMethods.SetOcafLabelName(Handle, entry, utf8.Pointer, utf8.Length),
            "ocaf_label_set_name");
    }

    internal string? GetName(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetOcafLabelNameLength(Handle, entry, out int hasName, out int length),
            "ocaf_label_name_utf8_length");
        if (hasName == 0)
        {
            return null;
        }

        return ReadUtf8(length, (nint buffer, int capacity, out int written) =>
            NativeMethods.GetOcafLabelName(Handle, entry, buffer, capacity, out written));
    }

    internal bool CommitTransaction(string? name)
    {
        ThrowIfDisposed();
        if (name is not null) return DocumentStateApi.CommitNamedCommand(Handle, name);
        NativeError.ThrowIfFailed(
            NativeMethods.CommitOcafCommand(Handle, out int changed),
            "ocaf_document_commit_command");
        return changed != 0;
    }

    internal DocumentLabelSnapshot SnapshotLabel(string entry)
    {
        ThrowIfDisposed();
        return DocumentStateApi.SnapshotLabel(Handle, entry);
    }

    internal string? GetText(string entry, DocumentAttributeKind kind)
    {
        ThrowIfDisposed();
        return DocumentStateApi.GetText(Handle, entry, kind);
    }

    internal void SetText(string entry, DocumentAttributeKind kind, string value)
    {
        ThrowIfDisposed();
        DocumentStateApi.SetText(Handle, entry, kind, value);
    }

    internal int? GetInteger(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetInteger(Handle, entry); }
    internal void SetInteger(string entry, int value) { ThrowIfDisposed(); DocumentStateApi.SetInteger(Handle, entry, value); }
    internal double? GetReal(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReal(Handle, entry); }
    internal void SetReal(string entry, double value) { ThrowIfDisposed(); DocumentStateApi.SetReal(Handle, entry, value); }
    internal DocumentIntegerArray? GetIntegerArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetIntegerArray(Handle, entry); }
    internal void SetIntegerArray(string entry, int lowerBound, IReadOnlyList<int> values) { ThrowIfDisposed(); DocumentStateApi.SetIntegerArray(Handle, entry, lowerBound, values); }
    internal DocumentRealArray? GetRealArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetRealArray(Handle, entry); }
    internal void SetRealArray(string entry, int lowerBound, IReadOnlyList<double> values) { ThrowIfDisposed(); DocumentStateApi.SetRealArray(Handle, entry, lowerBound, values); }
    internal string? GetReference(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReference(Handle, entry); }
    internal void SetReference(string entry, string targetEntry) { ThrowIfDisposed(); DocumentStateApi.SetReference(Handle, entry, targetEntry); }
    internal IReadOnlyList<string> GetReferenceArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReferenceArray(Handle, entry); }
    internal void SetReferenceArray(string entry, IReadOnlyList<string> targets) { ThrowIfDisposed(); DocumentStateApi.SetReferenceArray(Handle, entry, targets); }
    internal DocumentTreeSnapshot? GetTree(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetTree(Handle, entry); }
    internal void ReparentTree(string entry, string parentEntry) { ThrowIfDisposed(); DocumentStateApi.ReparentTree(Handle, entry, parentEntry); }
    internal void DetachTree(string entry) { ThrowIfDisposed(); DocumentStateApi.DetachTree(Handle, entry); }
    internal Shape? GetNamedShape(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetNamedShape(Handle, entry); }
    internal void SetNamedShape(string entry, Shape shape) { ThrowIfDisposed(); DocumentStateApi.SetNamedShape(Handle, entry, shape); }
    internal void RemoveAttribute(string entry, DocumentAttributeKind kind) { ThrowIfDisposed(); DocumentStateApi.RemoveAttribute(Handle, entry, kind); }

    internal void AbortTransaction()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.AbortOcafCommand(Handle), "ocaf_document_abort_command");
    }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Handle.IsClosed || Handle.IsInvalid, this);

    /// <summary>Closes the native document and aborts any open transaction.</summary>
    public void Dispose() => Handle.Dispose();

    private string ReadMainEntry() => ReadUtf8(63, (nint buffer, int capacity, out int written) =>
        NativeMethods.GetOcafMainEntry(Handle, buffer, capacity, out written));

    private delegate NativeStatus Utf8Reader(nint buffer, int capacity, out int written);

    private static string ReadUtf8(int length, Utf8Reader reader)
    {
        nint buffer = Marshal.AllocHGlobal(checked(length + 1));
        try
        {
            NativeError.ThrowIfFailed(reader(buffer, length + 1, out int written), "ocaf_utf8_copy");
            return Marshal.PtrToStringUTF8(buffer, written) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
