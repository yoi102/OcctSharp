namespace OcctSharp;

/// <summary>Contains a copied XDE physical-material record.</summary>
public sealed record XdeMaterial(
    string Name,
    string Description,
    double Density,
    string DensityName,
    string DensityType);
