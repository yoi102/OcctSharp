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

    /// <summary>Gets the number of direct child labels.</summary>
    public int ChildCount => _document.GetChildCount(Entry);

    /// <summary>Gets or sets the optional TDataStd_Name attribute.</summary>
    public string? Name
    {
        get => _document.GetName(Entry);
        set => _document.SetName(Entry, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>Adds a child label inside the current document transaction.</summary>
    public OcafLabel AddChild()
    {
        int childTag = _document.AddChild(Entry);
        return new OcafLabel(_document, $"{Entry}:{childTag}");
    }
}
