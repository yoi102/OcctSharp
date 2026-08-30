using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_shape", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeDefinitionShape(OcafDocumentHandle document, string entry, ShapeHandle shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_location", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeOccurrenceLocation(
        OcafDocumentHandle document, string entry, LocationHandle location,
        nint resultEntryBuffer, int resultEntryCapacity, out int resultEntryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_remove_component", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveXdeComponent(OcafDocumentHandle document, string entry);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_remove_shape", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveXdeShape(OcafDocumentHandle document, string entry, int removeCompletely, out int removed);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_user_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeUserCount(OcafDocumentHandle document, string entry, int recursive, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_user_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeUserEntry(
        OcafDocumentHandle document, string entry, int recursive, int index,
        nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_clone_subtree", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneXdeSubtree(
        OcafDocumentHandle document, string entry,
        nint resultEntryBuffer, int resultEntryCapacity, out int resultEntryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_external_references", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeExternalReferences(
        OcafDocumentHandle document, string entry, nint referencePointers, int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_external_reference_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeExternalReferenceCount(OcafDocumentHandle document, string entry, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_external_reference_utf8_length", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeExternalReferenceLength(OcafDocumentHandle document, string entry, int index, out int length);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_external_reference_to_utf8", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeExternalReference(
        OcafDocumentHandle document, string entry, int index, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_set_assembly_item_reference", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeAssemblyItemReference(
        OcafDocumentHandle document, string holderEntry, string itemPath, int subshapeIndex);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_assembly_item_reference_info", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeAssemblyItemReferenceInfo(
        OcafDocumentHandle document, string holderEntry, out int hasReference,
        out int isOrphan, out int subshapeIndex, out int pathLength);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_label_assembly_item_reference_path", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeAssemblyItemReferencePath(
        OcafDocumentHandle document, string holderEntry, nint buffer, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_shuo_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateXdeShuo(
        OcafDocumentHandle document, nint occurrenceEntryPointers, int count,
        nint resultEntryBuffer, int resultEntryCapacity, out int resultEntryWritten);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_shuo_link_count", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeShuoLinkCount(OcafDocumentHandle document, string shuoEntry, int upper, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_xde_shuo_link_entry", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeShuoLinkEntry(
        OcafDocumentHandle document, string shuoEntry, int upper, int index,
        nint buffer, int capacity, out int written);
}
