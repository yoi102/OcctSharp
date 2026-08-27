namespace OcctSharp;

/// <summary>Pairs one STEP file with its placement in an XDE assembly.</summary>
[Obsolete("Use XdeDocument.ImportStep and AddComponent with TopLocLocation.")]
public sealed record StepAssemblyInput(string FilePath, ShapeTransform Transform);
