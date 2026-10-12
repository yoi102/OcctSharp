using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;
using Collection = OcctSharp.Generator.Transformation.CopiedCollectionOperationContract.Collection;

namespace OcctSharp.Generator.Transformation;

/// <summary>Exact Array2 and retained HArray contracts; general template mapping stays closed.</summary>
internal static class GridCollectionOperationContract
{
    private static readonly (Dictionary<string, CopiedCollectionOperationContract.Operation> Operations, HashSet<string> Owners) Catalogue
        = CopiedCollectionOperationContract.Load("grid-collection-operation-contracts.json");
    internal static IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations => Catalogue.Operations;
    internal static IReadOnlySet<string> Owners => Catalogue.Owners;
    internal static readonly Collection[] Collections =
    [
        new("NCollection_Array2<gp_Pnt>", "gp_Pnt", "Point3dGrid", "OcctSharp_Value_Coordinates3d", "global::OcctSharp.Values.Coordinates3d",
            "gp_Pnt(OcctSharp_GeometryValues::ToNative(value))", "OcctSharp_GeometryValues::FromNative(value.XYZ())", Grid: true),
        new("NCollection_Array2<double>", "double", "RealGrid", "double", "double", "OcctSharp_GeometryValues::Check(value)", "value", Grid: true),
        new("NCollection_Array1<gp_Vec>", "gp_Vec", "Vector3dArray", "OcctSharp_Value_Vector3d", "global::OcctSharp.Values.Vector3d",
            "OcctSharp_GeometryValues::ToNative(value)", "OcctSharp_GeometryValues::FromNative(value)"),
        new("NCollection_Array1<gp_Vec2d>", "gp_Vec2d", "Vector2dArray", "OcctSharp_Value_Vector2d", "global::OcctSharp.Values.Vector2d",
            "OcctSharp_GeometryValues::ToNative(value)", "OcctSharp_GeometryValues::FromNative(value)"),
        new("NCollection_Array1<bool>", "bool", "BooleanArray", "uint8_t", "bool", "Boolean(value)", "static_cast<uint8_t>(value ? 1 : 0)"),
    ];
    internal static bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped && (op.Supporting || d.SupportState != BindingSupportState.Manual);
    internal static bool Retained(BindingType t) => t.IsOcctHandle && t.HandleTargetType?.StartsWith("NCollection_HArray1<", StringComparison.Ordinal) == true;
    internal static Collection? Find(BindingType t)
    {
        bool valueOrReference = t.Layers.Count == 1 && t.Layers[0].Kind == BindingTypeLayerKind.Value
            || t.Layers.Count == 2 && t.Layers[0].Kind == BindingTypeLayerKind.LValueReference && t.Layers[1].Kind == BindingTypeLayerKind.Value;
        if (!valueOrReference) return null;
        if (Retained(t))
        {
            if (t.Layers.Any(l => l.Kind == BindingTypeLayerKind.PointerIndirection)) return null;
            string array = t.HandleTargetType!.Replace("NCollection_HArray1<", "NCollection_Array1<", StringComparison.Ordinal);
            return Collections.Concat(CopiedCollectionOperationContract.Collections).FirstOrDefault(c => c.NativeType == array);
        }
        string native = t.BaseCanonicalSpelling.Replace("const ", "", StringComparison.Ordinal).Trim();
        return Collections.FirstOrDefault(c => c.NativeType == native) ?? CopiedCollectionOperationContract.Find(t);
    }
    internal static bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage, InitialTypeMap map, out BindingTypeProjection? p)
    {
        p = null;
        if (!Contains(d)) return map.TryMap(t, usage, out p);
        if (Find(t) is { } c)
        {
            p = new("TM013", "void*", "GridHandle", c.Friendly, Retained(t) ? "RetainedCollectionCopy" : "CollectionCopy",
                "Audited grid or Array1 copy; retained HArray uses independent intrusive ownership.");
            return true;
        }
        if (Owners.Contains(t.BaseCanonicalSpelling) && t.Layers.Count == 1)
        {
            p = new("TM012", "void*", "OwnerHandle", OwnedValueOperationContract.ManagedName(t.BaseCanonicalSpelling) + "Collections", "OwnedValue", "Audited algorithm owner.");
            return true;
        }
        return map.TryMap(t, usage, out p);
    }
}
