using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns one binary-persistable OCAF document and its transaction state.</summary>
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
        return new OcafTransaction(this);
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

    internal bool CommitTransaction()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.CommitOcafCommand(Handle, out int changed),
            "ocaf_document_commit_command");
        return changed != 0;
    }

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
