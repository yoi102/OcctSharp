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
    IReadOnlyList<OcctInventoryFailure> Failures,
    OcctFinalClassification? FinalClassification);

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

public sealed record OcctFinalClassification(
    bool IsComplete,
    int DeclarationTotal,
    int DeclarationClassified,
    int DeclarationPending,
    IReadOnlyList<OcctDispositionCount> DeclarationStates,
    IReadOnlyList<OcctReasonCount> DeclarationReasons,
    int HeaderTotal,
    int HeaderClassified,
    int HeaderPending,
    IReadOnlyList<OcctDispositionCount> HeaderStates,
    IReadOnlyList<OcctReasonCount> HeaderReasons,
    IReadOnlyList<OcctDeclarationDisposition> Declarations,
    IReadOnlyList<OcctHeaderDisposition> Headers);

public sealed record OcctDispositionCount(string State, int Count);

public sealed record OcctReasonCount(string Code, string Category, int Count);

public sealed record OcctDeclarationDisposition(
    string StableId,
    string NativeName,
    string Kind,
    string Header,
    string SourcePackage,
    string? SourceToolkit,
    string State,
    string Code,
    string Category);

public sealed record OcctHeaderDisposition(
    string Header,
    string State,
    string Code,
    string Category);
