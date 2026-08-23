namespace OcctSharp;

/// <summary>Pairs one STEP file with its placement in an XDE assembly.</summary>
public sealed record StepAssemblyInput(string FilePath, ShapeTransform Transform);
