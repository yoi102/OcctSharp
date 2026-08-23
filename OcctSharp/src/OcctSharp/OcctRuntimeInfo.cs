namespace OcctSharp;

/// <summary>Describes the loaded managed, native bridge, ABI, and OCCT versions.</summary>
public sealed record OcctRuntimeInfo(
    string ManagedVersion,
    string BridgeVersion,
    Version AbiVersion,
    string OcctVersion);
