namespace OcctSharp;

/// <summary>File formats supported by the XDE-centered exchange workflow.</summary>
public enum XdeExchangeFormat
{
    /// <summary>ISO 10303 STEP through STEPCAF.</summary>
    Step = 1,
    /// <summary>IGES through IGESCAF.</summary>
    Iges = 2,
}

/// <summary>IGESCAF metadata switches applied while reading an XDE document.</summary>
public sealed record XdeIgesReadOptions(
    bool ReadNames = true,
    bool ReadColors = true,
    bool ReadLayers = true);

/// <summary>IGESCAF metadata switches applied while exporting an XDE document.</summary>
public sealed record XdeIgesWriteOptions(
    bool WriteNames = true,
    bool WriteColors = true,
    bool WriteLayers = true);

/// <summary>Copied IGES source, root-transfer, and length-unit diagnostics.</summary>
public sealed record XdeIgesReadReport(
    int SourceEntityCount,
    int CandidateRootCount,
    int TransferredRootCount,
    double SourceLengthUnitMeters,
    double SystemLengthUnitMillimeters);
