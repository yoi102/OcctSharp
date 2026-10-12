using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Transformation;

/// <summary>AI exact signatures. Owning containers do not open arbitrary templates.</summary>
internal static class TopologyExchangeOperationContract
{
    internal static readonly IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations
        = CopiedCollectionOperationContract.Load("topology-exchange-operation-contracts.json").Operations;
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped && (op.Supporting || d.SupportState != BindingSupportState.Manual);
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        // Closed per-signature semantic projection: containers and noncopyable receivers stay private.
        string name=t.BaseCanonicalSpelling;
        if(t.CanonicalSpelling=="const char *const")
        { p=new("TM015Text", "const char*", "string", "string", "CallScopedUtf8", "Immediate UTF8 string input; embedded nulls rejected; no borrowed return."); return true; }
        if (name.Contains("TopoDS_Shape", StringComparison.Ordinal) && name.Contains("NCollection_", StringComparison.Ordinal))
        { p=new("TM015Shape", "void*", "TopologyCollectionHandle", "OwnedShapeSet", "ShapeValueCollectionCopy", "Copied values retain TShape, location and orientation."); return true; }
        if ((name.Contains("NCollection_HSequence<", StringComparison.Ordinal) && name.Contains("Standard_Transient", StringComparison.Ordinal)) || name=="Interface_EntityIterator" || name=="Interface_Graph")
        { p=new("TM015Entity", "void*", "ExchangeCollectionHandle", "RetainedEntitySet", "EntityCollectionCopy", "Private container/cursor with retained intrusive entity references."); return true; }
        if (name.Contains("Message_ProgressRange", StringComparison.Ordinal))
        { p=new("TM015Control", "void", "void", "void", "PrivateDefault", "Private default progress range; no callback escapes."); return true; }
        if (map.TryMap(ReferenceOperationContract.CopiedType(t), usage, out p)) return true;
        p=null; return false;
    }
}
