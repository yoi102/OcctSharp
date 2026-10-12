using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
using Collection = OcctSharp.Generator.Transformation.CopiedCollectionOperationContract.Collection;

namespace OcctSharp.Generator.Transformation;

/// <summary>Exact AH signatures; scalar references and curve containers never broaden the global map.</summary>
internal static class BulkCollectionOperationContract
{
    internal static readonly IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations
        = CopiedCollectionOperationContract.Load("bulk-collection-operation-contracts.json").Operations;
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind == BindingDeclarationKind.Method && d.SupportState is not (BindingSupportState.Skipped or BindingSupportState.Manual);
    internal static bool Conversion(BindingDeclaration d) => d.NativeName.StartsWith("GeomConvert::", StringComparison.Ordinal)
        || d.NativeName.StartsWith("Geom2dConvert::", StringComparison.Ordinal);
    internal static Collection? Find(BindingType t) => GridCollectionOperationContract.Find(t);
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        p = null;
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        if (Find(t) is { } c)
        {
            p = new("TM014", "void*", "CopiedHandle", c.Friendly, "CollectionCopy", "Exact sized AH owning copy.");
            return true;
        }
        if (Conversion(d) && (t.BaseCanonicalSpelling.Contains("NCollection_", StringComparison.Ordinal)))
        {
            // Every container occurrence in these ten exact signatures has an accepted typed copy contract.
            p = new("TM014", "void*", "CurveSetHandle", "OwnedCurveSet", "TypedCurveCollectionCopy", "Deep curve copies with intrusive ownership inside a leased registry owner.");
            return true;
        }
        if (ReferenceOperationContract.IsMutableReference(t))
            return map.TryMap(ReferenceOperationContract.CopiedType(t), usage, out p) && p?.Ownership == "ValueCopy";
        return map.TryMap(t, usage, out p);
    }
}
