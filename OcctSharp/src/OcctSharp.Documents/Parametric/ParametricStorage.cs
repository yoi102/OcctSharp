using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

// Internal storage over the existing retained document handle. The facade owns the
// document and all operations; this helper never disposes or duplicates that owner.
internal sealed partial class ParametricStorage(OcafDocumentHandle document)
{
    internal OcafDocumentHandle Handle => document;

    internal int Register(string entry, Guid driver)
    {
        NativeError.ThrowIfFailed(ParametricNative.Register(document, entry, driver.ToString(), out int id), "function_register");
        return id;
    }
    internal void Remove(string entry) => NativeError.ThrowIfFailed(ParametricNative.Remove(document, entry), "function_remove");
    internal unsafe void Rewire(string entry, IEnumerable<int> previous)
    {
        int[] values = previous.ToArray();
        fixed (int* p = values) NativeError.ThrowIfFailed(ParametricNative.Rewire(document, entry, p, values.Length), "function_rewire");
    }
    internal unsafe (int Id, int State, int[] Links) Links(string entry, bool next)
    {
        NativeError.ThrowIfFailed(ParametricNative.Links(document, entry, next ? 1 : 0, null, 0, out int count, out int id, out int state), "function_links_count");
        if (count is < 0 or > 4096) throw new InvalidOperationException("Native graph exceeds the limit.");
        int[] values = new int[count];
        fixed (int* p = values) NativeError.ThrowIfFailed(ParametricNative.Links(document, entry, next ? 1 : 0, p, count, out _, out _, out _), "function_links");
        return (id, state, values);
    }
    internal void State(string entry, ParametricExecutionState state)
    {
        int raw = EncodeState(state);
        NativeError.ThrowIfFailed(ParametricNative.State(document, entry, raw, state is ParametricExecutionState.Failed or ParametricExecutionState.Blocked ? 1 : 0), "function_state");
    }
    internal static int EncodeState(ParametricExecutionState state) => state switch { ParametricExecutionState.NotExecuted => 1, ParametricExecutionState.Executing => 2,
            ParametricExecutionState.Succeeded => 3, ParametricExecutionState.Failed => 4, ParametricExecutionState.Blocked => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(state)) };
    internal int Logbook(string entry, int operation)
    {
        NativeError.ThrowIfFailed(ParametricNative.Logbook(document, entry, operation, out int flags), "function_logbook");
        return flags;
    }
    internal void SetText(string entry, string key, string text)
    {
        using Utf8Buffer buffer = Utf8Buffer.FromString(text);
        NativeError.ThrowIfFailed(ParametricNative.SetText(document, entry, key, buffer.Pointer, buffer.Length), "parametric_text_set");
    }
    internal unsafe string? GetText(string entry, string key)
    {
        NativeError.ThrowIfFailed(ParametricNative.GetText(document, entry, key, out int found, null, 0, out int size), "parametric_text_count");
        if (found == 0) return null;
        if (size is < 1 or > 4_194_305) throw new InvalidOperationException("Stored text exceeds the limit.");
        byte[] buffer = new byte[size];
        fixed (byte* p = buffer) NativeError.ThrowIfFailed(ParametricNative.GetText(document, entry, key, out _, p, size, out _), "parametric_text_get");
        return Encoding.UTF8.GetString(buffer.AsSpan(0, size - 1));
    }
    internal unsafe void SetParameter(string entry, ParametricValue value)
    {
        int[] integers = value.Integers.ToArray();
        double[] reals = value.Reals.ToArray();
        ParameterInfoRaw info = new() { Kind = (int)value.Kind, Integer = value.Integral, Real = value.Real,
            Count = value.Kind == ParametricValueKind.IntegralArray ? integers.Length : reals.Length };
        using Utf8Buffer text = Utf8Buffer.FromString(value.Text ?? string.Empty);
        fixed (int* i = integers) fixed (double* r = reals)
            NativeError.ThrowIfFailed(ParametricNative.SetParameter(document, entry, in info, text.Pointer, text.Length, i, r), "parameter_set");
        SetText(entry, "unit", ((int)value.Unit).ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
    internal unsafe ParametricValue GetParameter(string entry)
    {
        NativeError.ThrowIfFailed(ParametricNative.GetParameter(document, entry, out var info, null, 0, out int size, null, null, 0), "parameter_count");
        if (info.Count is < 0 or > 1_000_000 || size is < 0 or > 4_194_305)
            throw new InvalidOperationException("Stored parameter exceeds bounded limits.");
        byte[] text = new byte[size];
        int[] integers = info.Kind == 4 ? new int[info.Count] : [];
        double[] reals = info.Kind == 5 ? new double[info.Count] : [];
        fixed (byte* p = text) fixed (int* i = integers) fixed (double* r = reals)
            NativeError.ThrowIfFailed(ParametricNative.GetParameter(document, entry, out info, p, size, out _, i, r, info.Count), "parameter_get");
        string? unit = GetText(entry, "unit");
        return new((ParametricValueKind)info.Kind, info.Integer, info.Real,
            size == 0 ? null : Encoding.UTF8.GetString(text.AsSpan(0, size - 1)), integers, reals,
            unit is null ? ParametricUnit.None : (ParametricUnit)int.Parse(unit, System.Globalization.CultureInfo.InvariantCulture));
    }
}
