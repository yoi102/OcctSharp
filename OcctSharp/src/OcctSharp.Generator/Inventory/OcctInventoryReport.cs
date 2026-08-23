using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Inventory;

public sealed record OcctInventoryReport(
    string SchemaVersion,
    string OcctVersion,
    string Parser,
    bool SemanticScan,
    bool IsComplete,
    int BatchSize,
    OcctHeaderInventory Headers,
    OcctDeclarationInventory? Declarations,
    IReadOnlyList<OcctInventoryBatch> Batches,
    IReadOnlyList<OcctInventoryFailure> Failures);

public sealed record OcctHeaderInventory(
    int Total,
    int H,
    int Hxx,
    int Scanned,
    int Failed,
    IReadOnlyList<OcctPackageHeaderInventory> Packages);

public sealed record OcctPackageHeaderInventory(
    string SourcePackage,
    int Total,
    int H,
    int Hxx);

public sealed record OcctDeclarationInventory(
    int Total,
    int Pending,
    int Skipped,
    int Supported,
    int Manual,
    IReadOnlyList<OcctPackageDeclarationInventory> Packages);

public sealed record OcctPackageDeclarationInventory(
    string SourcePackage,
    string? SourceToolkit,
    int Total,
    int Pending,
    int Skipped,
    int Supported,
    int Manual);

public sealed record OcctInventoryBatch(
    int Sequence,
    IReadOnlyList<string> Headers,
    int DeclarationCount,
    int DiagnosticCount);

public sealed record OcctInventoryFailure(
    string Header,
    string Error);
