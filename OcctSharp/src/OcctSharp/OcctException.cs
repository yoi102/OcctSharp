namespace OcctSharp;

/// <summary>Represents a failure reported by the native OcctSharp bridge or OCCT.</summary>
public sealed class OcctException : Exception
{
    internal OcctException(string nativeStatus, string message)
        : base(message)
    {
        NativeStatus = nativeStatus;
    }

    /// <summary>Gets the stable native status name associated with the failure.</summary>
    public string NativeStatus { get; }
}
