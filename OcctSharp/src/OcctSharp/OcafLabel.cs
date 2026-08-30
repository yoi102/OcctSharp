namespace OcctSharp;

/// <summary>Represents an OCAF label by stable entry and remains parent-bound to its document.</summary>
public sealed class OcafLabel
{
    private readonly OcafDocument _document;

    internal OcafLabel(OcafDocument document, string entry)
    {
        _document = document;
        Entry = entry;
    }

    /// <summary>Gets the stable colon-separated TDF label entry.</summary>
    public string Entry { get; }

    internal OcafDocument Document => _document;

    /// <summary>Gets the number of direct child labels.</summary>
    public int ChildCount => _document.GetChildCount(Entry);

    /// <summary>Gets or sets the optional TDataStd_Name attribute.</summary>
    public string? Name
    {
        get => _document.GetText(Entry, DocumentAttributeKind.Name);
        set
        {
            if (value is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.Name);
            else _document.SetText(Entry, DocumentAttributeKind.Name, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_Comment value.</summary>
    public string? Comment
    {
        get => _document.GetText(Entry, DocumentAttributeKind.Comment);
        set
        {
            if (value is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.Comment);
            else _document.SetText(Entry, DocumentAttributeKind.Comment, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_AsciiString value.</summary>
    public string? AsciiString
    {
        get => _document.GetText(Entry, DocumentAttributeKind.AsciiString);
        set
        {
            if (value is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.AsciiString);
            else _document.SetText(Entry, DocumentAttributeKind.AsciiString, value);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_Integer value.</summary>
    public int? IntegerValue
    {
        get => _document.GetInteger(Entry);
        set
        {
            if (value.HasValue) _document.SetInteger(Entry, value.Value);
            else _document.RemoveAttribute(Entry, DocumentAttributeKind.IntegralValue);
        }
    }

    /// <summary>Gets, replaces, or removes the copied TDataStd_Real value.</summary>
    public double? RealValue
    {
        get => _document.GetReal(Entry);
        set
        {
            if (value.HasValue) _document.SetReal(Entry, value.Value);
            else _document.RemoveAttribute(Entry, DocumentAttributeKind.Real);
        }
    }

    /// <summary>Gets an immutable copy of the bounded integer array, or null when absent.</summary>
    public DocumentIntegerArray? IntegerArray => _document.GetIntegerArray(Entry);

    /// <summary>Gets an immutable copy of the bounded real array, or null when absent.</summary>
    public DocumentRealArray? RealArray => _document.GetRealArray(Entry);

    /// <summary>Gets or replaces the direct same-document reference.</summary>
    public OcafLabel? Reference
    {
        get
        {
            string? target = _document.GetReference(Entry);
            return target is null ? null : _document.GetLabel(target);
        }
        set
        {
            if (value is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.Reference);
            else
            {
                EnsureSameDocument(value);
                _document.SetReference(Entry, value.Entry);
            }
        }
    }

    /// <summary>Gets copied parent-bound labels from the ordered reference array.</summary>
    public IReadOnlyList<OcafLabel> References =>
        _document.GetReferenceArray(Entry).Select(_document.GetLabel).ToArray();

    /// <summary>Gets copied application-tree relationship metadata.</summary>
    public DocumentTreeSnapshot? Tree => _document.GetTree(Entry);

    /// <summary>Gets an independently owning copy of optional named topology.</summary>
    public Shape? NamedShape => _document.GetNamedShape(Entry);

    /// <summary>Adds a child label inside the current document transaction.</summary>
    public OcafLabel AddChild()
    {
        int childTag = _document.AddChild(Entry);
        return new OcafLabel(_document, $"{Entry}:{childTag}");
    }

    /// <summary>Copies this label's identity, children, and attributes.</summary>
    public DocumentLabelSnapshot CreateSnapshot() => _document.SnapshotLabel(Entry);

    /// <summary>Returns direct child labels in deterministic tag order.</summary>
    public IReadOnlyList<OcafLabel> GetChildren() =>
        CreateSnapshot().ChildEntries.Select(_document.GetLabel).ToArray();

    /// <summary>Returns recursive descendants in deterministic depth-first order.</summary>
    public IReadOnlyList<OcafLabel> GetDescendants()
    {
        List<OcafLabel> labels = [];
        Stack<OcafLabel> pending = new(GetChildren().Reverse());
        while (pending.Count > 0)
        {
            OcafLabel label = pending.Pop();
            labels.Add(label);
            foreach (OcafLabel child in label.GetChildren().Reverse()) pending.Push(child);
        }
        return labels;
    }

    /// <summary>Replaces or removes the bounded integer-array attribute.</summary>
    public void SetIntegerArray(int lowerBound, IReadOnlyList<int>? values)
    {
        if (values is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.IntegerArray);
        else _document.SetIntegerArray(Entry, lowerBound, values);
    }

    /// <summary>Replaces or removes the bounded real-array attribute.</summary>
    public void SetRealArray(int lowerBound, IReadOnlyList<double>? values)
    {
        if (values is null) _document.RemoveAttribute(Entry, DocumentAttributeKind.RealArray);
        else _document.SetRealArray(Entry, lowerBound, values);
    }

    /// <summary>Replaces the ordered same-document reference array; an empty list removes it.</summary>
    public void SetReferences(IReadOnlyList<OcafLabel> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        foreach (OcafLabel target in targets) EnsureSameDocument(target);
        _document.SetReferenceArray(Entry, targets.Select(static target => target.Entry).ToArray());
    }

    /// <summary>Moves this application-tree node below another same-document label.</summary>
    public void ReparentTree(OcafLabel parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        EnsureSameDocument(parent);
        _document.ReparentTree(Entry, parent.Entry);
    }

    /// <summary>Detaches this application-tree node from its current parent.</summary>
    public void DetachTree() => _document.DetachTree(Entry);

    /// <summary>Copies the supplied topology into this label's TNaming_NamedShape attribute.</summary>
    public void SetNamedShape(Shape shape) => _document.SetNamedShape(Entry, shape);

    /// <summary>Removes the named-topology attribute.</summary>
    public void ClearNamedShape() => _document.RemoveAttribute(Entry, DocumentAttributeKind.NamedShape);

    /// <summary>Removes one supported attribute inside the current transaction.</summary>
    public void RemoveAttribute(DocumentAttributeKind kind) => _document.RemoveAttribute(Entry, kind);

    private void EnsureSameDocument(OcafLabel label)
    {
        if (!ReferenceEquals(label._document, _document))
            throw new ArgumentException("The OCAF label belongs to another document.", nameof(label));
    }
}
