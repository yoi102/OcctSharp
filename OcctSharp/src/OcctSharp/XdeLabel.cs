using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Represents an XDE label by stable entry and remains parent-bound to its document.</summary>
public sealed class XdeLabel
{
    internal XdeLabel(XdeDocument document, string entry)
    {
        Document = document;
        Entry = entry;
    }

    internal XdeDocument Document { get; }

    /// <summary>Gets the stable TDF entry.</summary>
    public string Entry { get; }

    /// <summary>Creates a child metadata label inside the document's current transaction.</summary>
    public XdeLabel AddChild()
    {
        Document.ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.AddOcafChild(Document.Handle, Entry, out int tag), "ocaf_label_add_child");
        return Document.GetLabel($"{Entry}:{tag}");
    }

    /// <summary>Gets or sets the copied name attribute.</summary>
    public string? Name
    {
        get => Document.GetDocumentText(Entry, DocumentAttributeKind.Name);
        set
        {
            if (value is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.Name);
            else Document.SetDocumentText(Entry, DocumentAttributeKind.Name, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_Comment value.</summary>
    public string? Comment
    {
        get => Document.GetDocumentText(Entry, DocumentAttributeKind.Comment);
        set
        {
            if (value is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.Comment);
            else Document.SetDocumentText(Entry, DocumentAttributeKind.Comment, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_AsciiString value.</summary>
    public string? AsciiString
    {
        get => Document.GetDocumentText(Entry, DocumentAttributeKind.AsciiString);
        set
        {
            if (value is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.AsciiString);
            else Document.SetDocumentText(Entry, DocumentAttributeKind.AsciiString, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied integral value.</summary>
    public int? IntegerValue
    {
        get => Document.GetInteger(Entry);
        set
        {
            if (value.HasValue) Document.SetInteger(Entry, value.Value);
            else Document.RemoveAttribute(Entry, DocumentAttributeKind.IntegralValue);
        }
    }

    /// <summary>Gets, replaces, or removes the copied real value.</summary>
    public double? RealValue
    {
        get => Document.GetReal(Entry);
        set
        {
            if (value.HasValue) Document.SetReal(Entry, value.Value);
            else Document.RemoveAttribute(Entry, DocumentAttributeKind.Real);
        }
    }

    /// <summary>Gets an immutable copy of the bounded integer array.</summary>
    public DocumentIntegerArray? IntegerArray => Document.GetIntegerArray(Entry);

    /// <summary>Gets an immutable copy of the bounded real array.</summary>
    public DocumentRealArray? RealArray => Document.GetRealArray(Entry);

    /// <summary>Gets or replaces a direct same-document label reference.</summary>
    public XdeLabel? Reference
    {
        get
        {
            string? target = Document.GetReference(Entry);
            return target is null ? null : Document.GetLabel(target);
        }
        set
        {
            if (value is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.Reference);
            else
            {
                EnsureSameDocument(value);
                Document.SetReference(Entry, value.Entry);
            }
        }
    }

    /// <summary>Gets copied parent-bound labels from the ordered reference array.</summary>
    public IReadOnlyList<XdeLabel> References =>
        Document.GetReferenceArray(Entry).Select(Document.GetLabel).ToArray();

    /// <summary>Gets copied application-tree relationship metadata.</summary>
    public DocumentTreeSnapshot? Tree => Document.GetTree(Entry);

    /// <summary>Gets an independently owning copy of optional TNaming topology.</summary>
    public Shape? NamedShape => Document.GetNamedShape(Entry);

    /// <summary>Gets an owning copy of the shape stored by this label.</summary>
    public Shape Shape => Document.GetShape(Entry);

    /// <summary>Gets whether this label is an assembly.</summary>
    public bool IsAssembly => Document.IsAssembly(Entry);

    /// <summary>Gets the number of direct assembly components.</summary>
    public int ComponentCount => Document.GetComponentCount(Entry);

    /// <summary>Returns parent-bound component occurrence labels.</summary>
    public IReadOnlyList<XdeLabel> GetComponents() => Document.GetComponents(Entry);

    /// <summary>Gets the referred part label for a component occurrence.</summary>
    public XdeLabel ReferredShape => Document.GetReferredLabel(Entry);

    /// <summary>Gets an owning copy of this occurrence's location.</summary>
    public TopLocLocation Location => Document.GetLocation(Entry);

    /// <summary>Gets or replaces copied XDE area, volume, and centroid validation attributes.</summary>
    public XdeValidationProperties ValidationProperties
    {
        get => Document.GetValidationProperties(Entry);
        set => Document.SetValidationProperties(Entry, value);
    }

    /// <summary>Computes area, volume, and centroid from this label's shape and stores them.</summary>
    public XdeValidationProperties UpdateValidationPropertiesFromShape() =>
        Document.UpdateValidationPropertiesFromShape(Entry);

    /// <summary>Flattens direct or recursive component occurrences with composed world locations.</summary>
    public IReadOnlyList<XdeOccurrence> GetOccurrences(bool recursive = true) =>
        Document.GetOccurrences(Entry, recursive);

    /// <summary>Gets or sets the generic RGBA color assignment.</summary>
    public XdeColor? Color
    {
        get => Document.GetColor(Entry);
        set => Document.SetColor(Entry, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// Collects independently owned, location-aware XCAF presentation styles for this
    /// label, including inherited component and subshape styles.
    /// </summary>
    public IReadOnlyList<XdePresentationStyle> GetPresentationStyles() =>
        Document.GetPresentationStyles(Entry);

    /// <summary>Gets a copied snapshot of all assigned layer names.</summary>
    public IReadOnlyList<string> Layers => Document.GetLayers(Entry);

    /// <summary>Replaces existing layers with one layer.</summary>
    public void SetLayer(string layer) => Document.AddLayer(Entry, layer ?? throw new ArgumentNullException(nameof(layer)), true);

    /// <summary>Adds a layer while preserving existing layer assignments.</summary>
    public void AddLayer(string layer) => Document.AddLayer(Entry, layer ?? throw new ArgumentNullException(nameof(layer)), false);

    /// <summary>Gets or sets the copied physical-material assignment.</summary>
    public XdeMaterial? Material
    {
        get => Document.GetMaterial(Entry);
        set => Document.SetMaterial(Entry, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>Gets or sets the copied metallic-roughness visualization material.</summary>
    public XdeVisualMaterial? VisualMaterial
    {
        get => Document.GetVisualMaterial(Entry);
        set => Document.SetVisualMaterial(Entry, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>Copies this label's identity, children, and attributes.</summary>
    public DocumentLabelSnapshot CreateSnapshot() => Document.SnapshotLabel(Entry);

    /// <summary>Replaces or removes the bounded integer-array attribute.</summary>
    public void SetIntegerArray(int lowerBound, IReadOnlyList<int>? values)
    {
        if (values is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.IntegerArray);
        else Document.SetIntegerArray(Entry, lowerBound, values);
    }

    /// <summary>Replaces or removes the bounded real-array attribute.</summary>
    public void SetRealArray(int lowerBound, IReadOnlyList<double>? values)
    {
        if (values is null) Document.RemoveAttribute(Entry, DocumentAttributeKind.RealArray);
        else Document.SetRealArray(Entry, lowerBound, values);
    }

    /// <summary>Replaces the ordered same-document reference array; an empty list removes it.</summary>
    public void SetReferences(IReadOnlyList<XdeLabel> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        foreach (XdeLabel target in targets) EnsureSameDocument(target);
        Document.SetReferenceArray(Entry, targets.Select(static target => target.Entry).ToArray());
    }

    /// <summary>Moves this application-tree node below another same-document label.</summary>
    public void ReparentTree(XdeLabel parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        EnsureSameDocument(parent);
        Document.ReparentTree(Entry, parent.Entry);
    }

    /// <summary>Detaches this application-tree node from its current parent.</summary>
    public void DetachTree() => Document.DetachTree(Entry);

    /// <summary>Copies topology into this label's TNaming_NamedShape attribute.</summary>
    public void SetNamedShape(Shape shape) => Document.SetNamedShape(Entry, shape);

    /// <summary>Removes the named-topology attribute.</summary>
    public void ClearNamedShape() => Document.RemoveAttribute(Entry, DocumentAttributeKind.NamedShape);

    /// <summary>Removes one supported document attribute inside the current transaction.</summary>
    public void RemoveAttribute(DocumentAttributeKind kind) => Document.RemoveAttribute(Entry, kind);

    private void EnsureSameDocument(XdeLabel label)
    {
        if (!ReferenceEquals(label.Document, Document))
            throw new ArgumentException("The XDE label belongs to another document.", nameof(label));
    }
}
