using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Transformation;

/// <summary>Closed AJ signatures; private maps, modes and contexts never broaden the global map.</summary>
internal static class RepairWorkspaceOperationContract
{
    internal static readonly IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations
        = CopiedCollectionOperationContract.Load("repair-workspace-operation-contracts.json").Operations;
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped && (op.Supporting || d.SupportState != BindingSupportState.Manual);
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        bool privateType = t.BaseCanonicalSpelling.Contains("NCollection_", StringComparison.Ordinal)
            || t.HandleTargetType is "ShapeExtend_WireData" or "ShapeBuild_ReShape" or "BRepTools_History"
            || t.BaseCanonicalSpelling is "ShapeAnalysis_WireOrder" or "Message_ProgressRange";
        if (!privateType && map.TryMap(ReferenceOperationContract.CopiedType(t), usage, out p)) return true;
        // Each non-value parameter belongs to a signature-specific private native workflow.
        // No opaque native parameter escapes through the public managed API.
        p = new("TM016", "void*", "RepairWorkspaceHandle", "RepairWorkspace", "PrivateCoupledWorkspace",
            "Exact audited private receiver/context, detached outputs and checked epoch; no borrowed references.");
        return true;
    }
}
