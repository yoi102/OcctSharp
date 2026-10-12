using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Transformation;

/// <summary>Closed AM signatures; private spline workspaces and copied coupled results.</summary>
internal static class SplineWorkspacesOperationContract
{
    internal static readonly IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations
        = CopiedCollectionOperationContract.Load("spline-workspaces-operation-contracts.json").Operations;
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped && (op.Supporting || d.SupportState != BindingSupportState.Manual);
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        // Closed signature-specific inputs stay inside the native numeric adapter.
        // Even scalar-looking references are copied into immutable snapshot values.
        p = new("TM019", "void*", "SplineWorkspaceHandle", "CopiedGrid<double>", "PrivateSplineSnapshot",
            "Finite copied matrix values; bounded layouts and fresh private solvers.");
        return true;
    }
}
