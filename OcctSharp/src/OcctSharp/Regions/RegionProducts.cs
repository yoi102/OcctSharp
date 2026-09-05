using System.Text.Json;

namespace OcctSharp;

#pragma warning disable CS1591
public sealed record RegionProductDefinition(string Key, string OutputKey, XdeColor? Color = null);
public sealed record RegionProductMetadata(int SchemaVersion, Guid PartitionRevision, string PartKey, string OutputKey,
    IReadOnlyList<RegionAssignment> Cells, IReadOnlyList<RegionAssemblyInput> Sources);
public sealed record RegionProduct(XdeLabel Label, RegionProductMetadata Metadata);
public sealed record RegionProductSet(XdeLabel Root, IReadOnlyList<RegionProduct> Products);

public static class RegionProducts
{
    /// <summary>Creates separate products atomically. Region integers remain application semantics, not XDE visual material IDs.</summary>
    public static RegionProductSet Create(XdeDocument document, PartitionResult partition,
        IReadOnlyList<RegionProductDefinition> products, string name = "Regions", IReadOnlyList<RegionAssemblyInput>? sources = null)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(partition); ArgumentNullException.ThrowIfNull(products);
        if (products.Count is < 1 or > 128 || products.Any(p => p is null || string.IsNullOrWhiteSpace(p.Key)) ||
            products.Select(p => p.Key).Distinct(StringComparer.Ordinal).Count() != products.Count) throw new ArgumentException("Products require unique explicit part keys.");
        if (document.HasOpenTransaction) throw new InvalidOperationException("Region publication owns its complete transaction.");
        RegionAssemblyInput[] copiedSources = sources?.ToArray() ?? [];
        if (copiedSources.Select(s => s.Index).Distinct().Count() != copiedSources.Length || copiedSources.Any(s => s.Index < 0))
            throw new ArgumentException("Ambiguous source metadata indices.");
        List<Shape> shapes = []; List<RegionProduct> published = [];
        try
        {
            foreach (var product in products) shapes.Add(partition.CopyOutput(product.OutputKey));
            using var transaction = document.BeginTransaction("Publish partition region products");
            var root = document.AddAssembly(name); using var identity = TopLocLocation.Identity;
            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i]; var assignments = partition.GetAssignments(product.OutputKey);
                var inputIndices = assignments.SelectMany(a => partition.Cells[a.Cell.Index].InputMembership.Select((m, index) => (m, index))
                    .Where(m => m.m == RegionMembership.Inside).Select(m => m.index)).Distinct().ToHashSet();
                var metadata = new RegionProductMetadata(1, partition.Revision, product.Key, product.OutputKey, assignments,
                    Array.AsReadOnly(copiedSources.Where(s => inputIndices.Contains(s.Index)).ToArray()));
                // Represent multiple solid bodies explicitly in XDE's product tree.
                // A non-assembly compound with only a compound-level style has no
                // portable STEP product identity for each contained solid.
                XdeLabel definition;
                if (assignments.Count > 0 && assignments.All(a => a.Dimension == 3))
                {
                    Shape[] solids = shapes[i].GetSubShapes(ShapeKind.Solid);
                    try
                    {
                        if (solids.Length == 1) definition = document.AddShape(solids[0], product.Key);
                        else
                        {
                            definition = document.AddAssembly(product.Key);
                            for (int body = 0; body < solids.Length; body++)
                            {
                                string bodyName = $"{product.Key}/body-{body}";
                                var part = document.AddShape(solids[body], bodyName); if (product.Color is not null) part.Color = product.Color;
                                document.AddComponent(definition, part, identity).Name = bodyName;
                            }
                        }
                    }
                    finally { foreach (var solid in solids) solid.Dispose(); }
                }
                else definition = document.AddShape(shapes[i], product.Key);
                definition.Comment = JsonSerializer.Serialize(metadata); if (product.Color is not null) definition.Color = product.Color;
                document.AddComponent(root, definition, identity).Name = product.Key; published.Add(new(definition, metadata));
            }
            transaction.Commit(); return new(root, published.AsReadOnly());
        }
        finally { foreach (var shape in shapes) shape.Dispose(); }
    }
    /// <summary>Writes supported exact geometry/name/color. Semantic region data remains in OCAF comments and the copied metadata report.</summary>
    public static string Export(XdeDocument document, string path, XdeExchangeFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        return document.WriteExchange(path, format);
    }
    public const string ExchangeDisclosure = "STEP preserves explicit product names and colors. This IGES assembly path preserves geometry, colors and the assembly name, but not nested product names. Region rules, part keys, integer material keys, cell revisions and correspondence remain application/OCAF metadata, not portable executable CAD features.";
}
