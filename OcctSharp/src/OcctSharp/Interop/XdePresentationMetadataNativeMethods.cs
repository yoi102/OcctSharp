using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName,EntryPoint="occtsharp_xde_copy_presentation_metadata",StringMarshalling=StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs=[typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyXdePresentationMetadata(OcafDocumentHandle destination,string destinationEntry,OcafDocumentHandle source,string sourceEntry);
    [LibraryImport(LibraryName,EntryPoint="occtsharp_xde_label_get_color_channel",StringMarshalling=StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs=[typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetXdeColorChannel(OcafDocumentHandle document,string entry,int channel,out int assigned,out XdeColorRaw color,out int visible);
    [LibraryImport(LibraryName,EntryPoint="occtsharp_xde_label_set_color_channel",StringMarshalling=StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs=[typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeColorChannel(OcafDocumentHandle document,string entry,int channel,int assigned,XdeColorRaw color);
    [LibraryImport(LibraryName,EntryPoint="occtsharp_xde_label_set_visibility",StringMarshalling=StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs=[typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetXdeVisibility(OcafDocumentHandle document,string entry,int visible);
}
