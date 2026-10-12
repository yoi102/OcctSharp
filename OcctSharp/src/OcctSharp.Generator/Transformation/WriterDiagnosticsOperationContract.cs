using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Transformation;

/// <summary>Closed AN signatures; no borrowed mutable process or binder escapes.</summary>
internal static class WriterDiagnosticsOperationContract
{
    internal static readonly IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations
        = CopiedCollectionOperationContract.Load("writer-diagnostics-operation-contracts.json").Operations;
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped && (op.Supporting || d.SupportState != BindingSupportState.Manual);
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        // Closed signature-specific inputs stay inside the native transfer adapter.
        // Even scalar-looking references are copied into immutable snapshot values.
        p = new("TM020", "void*", "WriterDiagnosticHandle", "WriterTransferSnapshot", "PrivateWriterSnapshot",
            "Private single epoch; bounded traversal, exact index domains and copied diagnostics.");
        return true;
    }
}
