using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Emission;

public sealed record GeneratedFile(
    string RelativePath,
    string Content,
    OcctProductModule ProductModule = OcctProductModule.Runtime,
    GeneratedApiLayer ApiLayer = GeneratedApiLayer.Raw,
    string OutputShard = "Legacy");
