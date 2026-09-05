using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum AuthoringHistoryKind { Modified, Generated, FirstSection, LastSection, SimulatedSection, Unmapped, CompatibleSection, InputSnapshot, CurveSpan, SurfacePatch }
public readonly record struct AuthoringSourceReference(Guid PlanId, int ArgumentIndex, int TopologyIndex, ShapeKind Kind);
public sealed record AuthoringHistoryItem(AuthoringSourceReference? Source, AuthoringHistoryKind Kind, Shape? Shape) : IDisposable
{ public void Dispose() => Shape?.Dispose(); }
public sealed record AuthoringDiagnostics(bool Ready, bool AlgorithmDone, bool ShapeIsValid, bool IsSolid,
    int AlgorithmStatus, double? ApproximationError, int? ContinuityLimit, string Message);

/// <summary>Owns independent result/history shapes. Algorithm completion and geometry validity are separate.</summary>
public sealed class AuthoringResult : IDisposable
{
    internal AuthoringResult(Guid planId, Shape? shape, AuthoringDiagnostics diagnostics, AuthoringHistoryItem[] history)
    { PlanId = planId; Shape = shape; Diagnostics = diagnostics; History = Array.AsReadOnly(history); }
    public Guid PlanId { get; }
    public Shape? Shape { get; }
    public AuthoringDiagnostics Diagnostics { get; }
    public IReadOnlyList<AuthoringHistoryItem> History { get; }
    public IReadOnlyList<Shape> SimulatedSections => History.Where(h => h.Kind == AuthoringHistoryKind.SimulatedSection && h.Shape is not null).Select(h => h.Shape!).ToArray();
    public Shape? FirstSection => History.FirstOrDefault(h => h.Kind == AuthoringHistoryKind.FirstSection)?.Shape;
    public Shape? LastSection => History.FirstOrDefault(h => h.Kind == AuthoringHistoryKind.LastSection)?.Shape;
    public Shape RequireShape()
    {
        if (!Diagnostics.AlgorithmDone || !Diagnostics.ShapeIsValid || Shape is null)
            throw new InvalidOperationException($"Authoring result is not accepted: {Diagnostics.Message}");
        Shape.ThrowIfDisposed(); return Shape;
    }
    public void Dispose() { Shape?.Dispose(); foreach (var item in History) item.Dispose(); }
}

internal static class AuthoringBridge
{
    internal unsafe delegate T Call<T>(nint* shapes, int count);
    internal static unsafe T WithInputs<T>(IReadOnlyList<Shape> shapes, Call<T> call)
    {
        if (shapes.Count is < 1 or > 512) throw new ArgumentException("Authoring requires one to 512 shapes.");
        bool[] acquired = new bool[shapes.Count]; nint[] pointers = new nint[shapes.Count]; int index = 0;
        try
        {
            for (; index < shapes.Count; index++)
            {
                ArgumentNullException.ThrowIfNull(shapes[index]); shapes[index].ThrowIfDisposed();
                shapes[index].Handle.DangerousAddRef(ref acquired[index]); pointers[index] = shapes[index].Handle.DangerousGetHandle();
            }
            fixed (nint* values = pointers) return call(values, pointers.Length);
        }
        finally { for (int i = 0; i < acquired.Length; i++) if (acquired[i]) shapes[i].Handle.DangerousRelease(); }
    }
    internal static unsafe Shape[] CopyInputs(IReadOnlyList<Shape> shapes) => WithInputs(shapes, (p, count) =>
    {
        NativeError.ThrowIfFailed(NativeMethods.AuthoringCopyInputs(p, count, out nint raw), "authoring_copy_inputs");
        using FeatureResultHandle result = new(raw); List<Shape> copied = [];
        try
        {
            for (int i = 0; i < count; i++)
            {
                NativeError.ThrowIfFailed(NativeMethods.AuthoringHistory(result, i, out _, out nint item), "authoring_copy_input");
                copied.Add(ShapeFactory.FromNativeHandle(item, "authoring_copy_input"));
            }
            return copied.ToArray();
        }
        catch { foreach (Shape shape in copied) shape.Dispose(); throw; }
    });
    internal static unsafe AuthoringResult Read(Guid id, nint raw, AuthoringInfoRaw info)
    {
        using FeatureResultHandle result = new(raw); Shape? shape = null; List<AuthoringHistoryItem> history = [];
        try
        {
            if (info.HistoryCount is < 0 or > 1000000) throw new InvalidOperationException("Native history exceeds the bounded limit.");
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultShape(result, out nint root), "authoring_result_shape");
            if (root != 0) shape = ShapeFactory.FromNativeHandle(root, "authoring_result_shape");
            for (int i = 0; i < info.HistoryCount; i++)
            {
                NativeError.ThrowIfFailed(NativeMethods.AuthoringHistory(result, i, out AuthoringHistoryRaw h, out nint value), "authoring_history");
                Shape? owned = value != 0 ? ShapeFactory.FromNativeHandle(value, "authoring_history") : null;
                history.Add(new(h.SourceIndex < 0 || h.SubshapeIndex < 0 ? null : new(id, h.SourceIndex, h.SubshapeIndex, (ShapeKind)h.SourceKind), (AuthoringHistoryKind)h.Kind, owned));
            }
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, null, 0, out int required), "authoring_message_count");
            if (required is < 1 or > 1048576) throw new InvalidOperationException("Authoring diagnostic exceeds the bounded limit.");
            byte[] bytes = new byte[required]; fixed (byte* p = bytes)
                NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, p, bytes.Length, out _), "authoring_message");
            string message = Encoding.UTF8.GetString(bytes.AsSpan(0, bytes.Length - 1));
            return new(id, shape, new(info.Ready != 0, info.Done != 0, info.Valid != 0, info.Solid != 0,
                info.AlgorithmStatus, info.ErrorAvailable != 0 ? info.ApproximationError : null,
                info.ContinuityLimit >= 0 ? info.ContinuityLimit : null, message), history.ToArray());
        }
        catch { shape?.Dispose(); foreach (var item in history) item.Dispose(); throw; }
    }
    internal static void Positive(double value, string name)
    { if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(name); }
}
#pragma warning restore CS1591
