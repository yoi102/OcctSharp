namespace OcctSharp.Generator.Emission;

public sealed record GeneratedManifest(
    string SchemaVersion,
    string OcctVersion,
    string BindingModelSchemaVersion,
    IReadOnlyList<string> SourceStableIds,
    IReadOnlyList<GeneratedManifestFile> Files);

public sealed record GeneratedManifestFile(
    string RelativePath,
    string Sha256,
    string ProductModule,
    string ApiLayer,
    string OutputShard);
