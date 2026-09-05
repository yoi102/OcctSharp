using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp
{
#pragma warning disable CS1591
    public enum ParametricEvolutionKind { Primitive, Generated, Modified, Deleted }
    public enum ParametricSelectionStatus { Resolved, Missing, Ambiguous, Unsupported, WrongType, Deleted }
    public sealed record ParametricEvolution(ParametricEvolutionKind Kind, Shape? Before, Shape? After) : IDisposable
    {
        public void Dispose() { Before?.Dispose(); After?.Dispose(); }
    }
#pragma warning restore CS1591

    internal sealed partial class ParametricStorage
    {
        internal unsafe void Record(string entry, ParametricEvolutionKind kind, IReadOnlyList<Shape> before, IReadOnlyList<Shape> after)
        {
            int count = kind == ParametricEvolutionKind.Deleted ? before.Count : after.Count;
            if (count < 1 || (kind != ParametricEvolutionKind.Primitive && before.Count != count)) throw new ArgumentException("Evolution arrays differ.");
            AuthoringBridge.WithInputs(before.Concat(after).ToArray(), (p, _) =>
            {
                NativeError.ThrowIfFailed(ParametricNative.Record(document, entry, (int)kind,
                    before.Count == 0 ? null : p, after.Count == 0 ? null : p + before.Count, count), "naming_record");
                return 0;
            });
        }
        internal IReadOnlyList<ParametricEvolution> History(string entry, int transaction = -1)
        {
            NativeError.ThrowIfFailed(ParametricNative.History(document, entry, transaction, -1, out int count, out _, out _, out _), "naming_history_count");
            if (count is < 0 or > 1_000_000) throw new InvalidOperationException("History is too large.");
            List<ParametricEvolution> result = [];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    NativeError.ThrowIfFailed(ParametricNative.History(document, entry, transaction, i, out _, out int kind, out nint before, out nint after), "naming_history");
                    Shape? first = null;
                    Shape? second = null;
                    try
                    {
                        if (before != 0) first = ShapeFactory.FromNativeHandle(before, "naming_history_before");
                        if (after != 0) second = ShapeFactory.FromNativeHandle(after, "naming_history_after");
                        // TNaming_Evolution: PRIMITIVE=0, GENERATED=1, MODIFY=2, DELETE=3.
                        result.Add(new((ParametricEvolutionKind)kind, first, second));
                    }
                    catch
                    {
                        if (first is null && before != 0) NativeMethods.ReleaseShape(before); else first?.Dispose();
                        if (second is null && after != 0) NativeMethods.ReleaseShape(after); else second?.Dispose();
                        throw;
                    }
                }
                return result.AsReadOnly();
            }
            catch { foreach (var item in result) item.Dispose(); throw; }
        }
        internal bool Select(string parent, string context, Shape selection, ShapeKind kind)
        {
            selection.ThrowIfDisposed();
            NativeError.ThrowIfFailed(ParametricNative.Select(document, parent, context, selection.Handle, (int)kind, out int selected), "naming_select");
            return selected != 0;
        }
        internal (ParametricSelectionStatus Status, Shape? Shape) Resolve(string parent, ShapeKind kind)
        {
            NativeError.ThrowIfFailed(ParametricNative.Resolve(document, parent, (int)kind, out int status, out nint shape), "naming_resolve");
            return ((ParametricSelectionStatus)status, shape == 0 ? null : ShapeFactory.FromNativeHandle(shape, "naming_resolve"));
        }
        internal unsafe void Relocate(IReadOnlyList<string> sources, IReadOnlyList<string> targets, bool retainExternal)
        {
            if (sources.Count != targets.Count) throw new ArgumentException("Relocation root counts differ.");
            List<Utf8Buffer> leases = [];
            try
            {
                // This ABI takes C strings, unlike the shared length-delimited UTF-8 buffer APIs.
                foreach (string entry in sources.Concat(targets)) leases.Add(Utf8Buffer.FromString(entry + '\0'));
                nint[] pointers = leases.Select(x => x.Pointer).ToArray();
                fixed (nint* p = pointers)
                    NativeError.ThrowIfFailed(ParametricNative.Relocate(document, p, p + sources.Count, sources.Count, retainExternal ? 1 : 0), "parametric_relocate");
            }
            finally { foreach (var lease in leases) lease.Dispose(); }
        }
    }
}

namespace OcctSharp.Interop
{
    internal static unsafe partial class ParametricNative
    {
        [LibraryImport(LibraryName, EntryPoint = "occtsharp_naming_record", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus Record(OcafDocumentHandle document, string entry, int kind, nint* before, nint* after, int count);
        [LibraryImport(LibraryName, EntryPoint = "occtsharp_naming_history", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus History(OcafDocumentHandle document, string entry, int transaction, int index,
            out int count, out int kind, out nint before, out nint after);
        [LibraryImport(LibraryName, EntryPoint = "occtsharp_naming_select", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus Select(OcafDocumentHandle document, string parent, string context, ShapeHandle shape, int kind, out int selected);
        [LibraryImport(LibraryName, EntryPoint = "occtsharp_naming_resolve", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus Resolve(OcafDocumentHandle document, string parent, int kind, out int status, out nint shape);
        [LibraryImport(LibraryName, EntryPoint = "occtsharp_parametric_relocate")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus Relocate(OcafDocumentHandle document, nint* sources, nint* targets, int count, int retainExternal);
    }
}
