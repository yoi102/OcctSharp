namespace OcctSharp;

/// <summary>Represents normalized linear RGBA channel values used by XDE metadata.</summary>
public readonly record struct XdeColor(double Red, double Green, double Blue, double Alpha = 1.0)
{
    internal void Validate()
    {
        if (!IsChannel(Red) || !IsChannel(Green) || !IsChannel(Blue) || !IsChannel(Alpha))
        {
            throw new ArgumentOutOfRangeException(nameof(XdeColor), "XDE color channels must be finite values from zero through one.");
        }
    }

    private static bool IsChannel(double value) => double.IsFinite(value) && value is >= 0 and <= 1;
}
