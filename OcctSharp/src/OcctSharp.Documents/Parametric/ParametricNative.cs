using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct ParameterInfoRaw
{
    internal int Kind, Integer, Count, Reserved;
    internal double Real;
}

internal static unsafe partial class ParametricNative
{
    private const string LibraryName = "OcctSharp.Native";
    static ParametricNative() => NativeLibraryResolver.EnsureRegistered(typeof(ParametricNative).Assembly);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_register", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus Register(OcafDocumentHandle document, string entry, string driver, out int id);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_remove", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus Remove(OcafDocumentHandle document, string entry);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_rewire", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus Rewire(OcafDocumentHandle document, string entry, int* previous, int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_links", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus Links(OcafDocumentHandle document, string entry, int next,
        int* values, int capacity, out int count, out int id, out int state);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_state", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus State(OcafDocumentHandle document, string entry, int state, int failure);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_function_logbook", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus Logbook(OcafDocumentHandle document, string entry, int operation, out int flags);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_parametric_text_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetText(OcafDocumentHandle document, string entry, string key, nint text, int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_parametric_text_get", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetText(OcafDocumentHandle document, string entry, string key, out int found,
        byte* buffer, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_parameter_set", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetParameter(OcafDocumentHandle document, string entry, in ParameterInfoRaw info,
        nint text, int textLength, int* integers, double* reals);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_parameter_get", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetParameter(OcafDocumentHandle document, string entry, out ParameterInfoRaw info,
        byte* text, int textCapacity, out int written, int* integers, double* reals, int capacity);
}
