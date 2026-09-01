namespace OcctSharp;

/// <summary>Identifies one OCCT manipulator interaction mode.</summary>
public enum ViewerManipulatorMode
{
    /// <summary>No active manipulation mode.</summary>
    None = 0,
    /// <summary>Translate along one axis.</summary>
    Translation = 1,
    /// <summary>Rotate around one axis.</summary>
    Rotation = 2,
    /// <summary>Scale around the manipulator origin.</summary>
    Scaling = 3,
    /// <summary>Translate in one axis-aligned plane.</summary>
    TranslationPlane = 4
}

/// <summary>Identifies one zero-based manipulator axis.</summary>
public enum ViewerManipulatorAxis
{
    /// <summary>The X axis.</summary>
    X = 0,
    /// <summary>The Y axis.</summary>
    Y = 1,
    /// <summary>The Z axis.</summary>
    Z = 2
}

/// <summary>Selects the shaded or flat OCCT manipulator presentation.</summary>
public enum ViewerManipulatorSkin
{
    /// <summary>Shaded three-dimensional parts.</summary>
    Shaded = 0,
    /// <summary>Flat camera-oriented parts.</summary>
    Flat = 1
}

/// <summary>Flags the selection modes enabled when a manipulator is created.</summary>
[Flags]
public enum ViewerManipulatorModes
{
    /// <summary>No enabled mode.</summary>
    None = 0,
    /// <summary>Translation mode.</summary>
    Translation = 1,
    /// <summary>Rotation mode.</summary>
    Rotation = 2,
    /// <summary>Scaling mode.</summary>
    Scaling = 4,
    /// <summary>Planar translation mode.</summary>
    TranslationPlane = 8,
    /// <summary>All rigid placement modes, excluding scale.</summary>
    Rigid = Translation | Rotation | TranslationPlane,
    /// <summary>Every supported mode.</summary>
    All = Translation | Rotation | Scaling | TranslationPlane
}

/// <summary>Configures one viewer-parent-bound manipulator.</summary>
public sealed record ViewerManipulatorOptions
{
    /// <summary>Gets whether attach centers the manipulator on the presentation.</summary>
    public bool AdjustPosition { get; init; } = true;
    /// <summary>Gets whether attach derives manipulator size from presentation bounds.</summary>
    public bool AdjustSize { get; init; }
    /// <summary>Gets whether attach initially enables selection modes.</summary>
    public bool EnableModesOnAttach { get; init; } = true;
    /// <summary>Gets whether detection activates a mode before selection.</summary>
    public bool ActivationOnDetection { get; init; }
    /// <summary>Gets whether the manipulator retains a fixed screen size.</summary>
    public bool ZoomPersistence { get; init; }
    /// <summary>Gets the manipulator visual skin.</summary>
    public ViewerManipulatorSkin Skin { get; init; } = ViewerManipulatorSkin.Shaded;
    /// <summary>Gets the modes enabled after attachment.</summary>
    public ViewerManipulatorModes EnabledModes { get; init; } = ViewerManipulatorModes.All;
    /// <summary>Gets the finite positive side length.</summary>
    public double Size { get; init; } = 150.0;
    /// <summary>Gets the finite non-negative spacing between visual parts.</summary>
    public double Gap { get; init; } = 20.0;
    /// <summary>Gets an optional copied position overriding automatic placement.</summary>
    public GpAx2Value? Position { get; init; }

    internal void Validate()
    {
        if (!Enum.IsDefined(Skin)) throw new ArgumentOutOfRangeException(nameof(Skin));
        if ((EnabledModes & ~ViewerManipulatorModes.All) != 0)
            throw new ArgumentOutOfRangeException(nameof(EnabledModes));
        if (!double.IsFinite(Size) || Size is <= 0 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(Size));
        if (!double.IsFinite(Gap) || Gap < 0 || Gap > Size)
            throw new ArgumentOutOfRangeException(nameof(Gap));
    }
}

/// <summary>Copies the observable state of one native-local manipulator.</summary>
public sealed record ViewerManipulatorState(
    bool IsAttached,
    ViewerManipulatorMode ActiveMode,
    int ActiveAxis,
    bool HasActiveTransformation,
    bool ActivationOnDetection,
    bool ZoomPersistence,
    ViewerManipulatorSkin Skin,
    double Size,
    double Gap,
    GpAx2Value Position);
