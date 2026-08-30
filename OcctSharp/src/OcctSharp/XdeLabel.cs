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

    /// <summary>Gets or sets the copied name attribute.</summary>
    public string? Name
    {
        get => Document.GetName(Entry);
        set => Document.SetName(Entry, value ?? throw new ArgumentNullException(nameof(value)));
    }

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
}
