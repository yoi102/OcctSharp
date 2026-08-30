using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class DocumentStateNativeMethods
{
    private const string LibraryName = "OcctSharp.Native";

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_save_format", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SaveFormat(OcafDocumentHandle document, string path, int xde, int xml);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_label_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetLabelInfo(
        OcafDocumentHandle document, string entry, out int tag, out int depth,
        out int isRoot, out int hasParent, byte* parentBuffer, int parentCapacity, out int parentWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_child_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetChildEntry(
        OcafDocumentHandle document, string entry, int index, byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_attribute_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetAttributeCount(OcafDocumentHandle document, string entry, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_attribute_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetAttributeInfo(
        OcafDocumentHandle document, string entry, int index, out int kind,
        byte* idBuffer, int idCapacity, out int idWritten,
        byte* typeBuffer, int typeCapacity, out int typeWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_text_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTextInfo(
        OcafDocumentHandle document, string entry, int kind, out int hasValue, out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_text_copy", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CopyText(
        OcafDocumentHandle document, string entry, int kind, byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_text_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetText(
        OcafDocumentHandle document, string entry, int kind, nint utf8, int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_scalar_get", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetScalar(
        OcafDocumentHandle document, string entry, int kind,
        out int hasValue, out int integerValue, out double realValue);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_scalar_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetScalar(
        OcafDocumentHandle document, string entry, int kind, int integerValue, double realValue);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_attribute_remove", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveAttribute(OcafDocumentHandle document, string entry, int kind);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_array_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetArrayInfo(
        OcafDocumentHandle document, string entry, int kind, out int hasValue, out int lower, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_integer_array_copy", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CopyIntegerArray(
        OcafDocumentHandle document, string entry, int* values, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_real_array_copy", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CopyRealArray(
        OcafDocumentHandle document, string entry, double* values, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_integer_array_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SetIntegerArray(
        OcafDocumentHandle document, string entry, int lower, int* values, int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_real_array_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SetRealArray(
        OcafDocumentHandle document, string entry, int lower, double* values, int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_reference_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetReferenceInfo(
        OcafDocumentHandle document, string entry, int array, out int hasValue, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_reference_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetReferenceEntry(
        OcafDocumentHandle document, string entry, int array, int index,
        byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_reference_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetReference(OcafDocumentHandle document, string entry, string targetEntry);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_reference_array_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetReferenceArray(
        OcafDocumentHandle document, string entry, nint targetEntries, int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_tree_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTreeInfo(
        OcafDocumentHandle document, string entry, out int hasNode, out int hasParent, out int childCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_tree_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetTreeEntry(
        OcafDocumentHandle document, string entry, int parent, int index,
        byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_tree_reparent", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReparentTree(OcafDocumentHandle document, string entry, string parentEntry);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_tree_detach", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus DetachTree(OcafDocumentHandle document, string entry);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_named_shape_get", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetNamedShape(
        OcafDocumentHandle document, string entry, out int hasShape, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_named_shape_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetNamedShape(OcafDocumentHandle document, string entry, ShapeHandle shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_commit_named_command")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CommitNamedCommand(
        OcafDocumentHandle document, nint utf8, int length, out int changed);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_state")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetHistoryState(
        OcafDocumentHandle document, out int undoLimit, out int undoCount, out int redoCount, out int isChanged);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_set_limit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetHistoryLimit(OcafDocumentHandle document, int undoLimit);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_action")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ApplyHistoryAction(OcafDocumentHandle document, int action, out int changed);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_entry_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetHistoryEntryInfo(
        OcafDocumentHandle document, int redo, int index,
        out int beginTime, out int endTime, out int deltaCount, out int labelCount, out int nameLength);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_entry_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CopyHistoryEntryName(
        OcafDocumentHandle document, int redo, int index, byte* buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_document_history_entry_label")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus CopyHistoryEntryLabel(
        OcafDocumentHandle document, int redo, int index, int labelIndex,
        byte* buffer, int capacity, out int written);
}
