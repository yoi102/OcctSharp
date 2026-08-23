using System.Text.RegularExpressions;

namespace OcctSharp.Samples;

internal readonly record struct StepMetadataSummary(
    int Colours,
    int StyledItems,
    int PresentationStyles,
    int Materials,
    int ProductDefinitions,
    int AssemblyUsages)
{
    public static StepMetadataSummary FromFile(string path)
    {
        string text = File.ReadAllText(path);
        return new StepMetadataSummary(
            Count(text, "COLOUR_RGB"),
            Count(text, "STYLED_ITEM"),
            Count(text, "PRESENTATION_STYLE_ASSIGNMENT"),
            Count(text, "MATERIAL"),
            Count(text, "PRODUCT_DEFINITION"),
            Count(text, "NEXT_ASSEMBLY_USAGE_OCCURRENCE"));
    }

    public static StepMetadataSummary Sum(IEnumerable<string> paths) =>
        paths.Select(FromFile).Aggregate(default(StepMetadataSummary), static (sum, value) => sum + value);

    public static StepMetadataSummary operator +(StepMetadataSummary left, StepMetadataSummary right) =>
        new(
            left.Colours + right.Colours,
            left.StyledItems + right.StyledItems,
            left.PresentationStyles + right.PresentationStyles,
            left.Materials + right.Materials,
            left.ProductDefinitions + right.ProductDefinitions,
            left.AssemblyUsages + right.AssemblyUsages);

    public override string ToString() =>
        $"COLOUR_RGB={Colours}, STYLED_ITEM={StyledItems}, "
        + $"PRESENTATION_STYLE_ASSIGNMENT={PresentationStyles}, MATERIAL={Materials}, "
        + $"PRODUCT_DEFINITION={ProductDefinitions}, "
        + $"NEXT_ASSEMBLY_USAGE_OCCURRENCE={AssemblyUsages}";

    private static int Count(string text, string entity) =>
        Regex.Count(text, $@"\b{Regex.Escape(entity)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
