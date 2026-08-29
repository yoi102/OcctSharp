namespace OcctSharp;

/// <summary>Common STEPCAF metadata switches applied before importing an XDE document.</summary>
public sealed record XdeStepReadOptions(
    bool ReadNames = true,
    bool ReadColors = true,
    bool ReadLayers = true,
    bool ReadValidationProperties = true,
    bool ReadMaterials = true,
    bool ReadGdt = true,
    bool ReadSavedViews = true);

/// <summary>Common STEPCAF metadata switches and shape representation applied during export.</summary>
public sealed record XdeStepWriteOptions(
    XdeStepModelType ModelType = XdeStepModelType.AsIs,
    bool WriteNames = true,
    bool WriteColors = true,
    bool WriteLayers = true,
    bool WriteValidationProperties = true,
    bool WriteMaterials = true,
    bool WriteGdt = true,
    XdeStepSchema Schema = XdeStepSchema.Ap242);

/// <summary>STEP application protocol/schema selected for XDE export.</summary>
public enum XdeStepSchema
{
    /// <summary>AP214 committee draft.</summary>
    Ap214Cd = 1,
    /// <summary>AP214 draft international standard.</summary>
    Ap214Dis = 2,
    /// <summary>AP203 configuration-controlled design.</summary>
    Ap203 = 3,
    /// <summary>AP214 international standard.</summary>
    Ap214Is = 4,
    /// <summary>AP242 managed model-based 3D engineering.</summary>
    Ap242 = 5
}

/// <summary>OCCT STEP shape representation selected for XDE export.</summary>
public enum XdeStepModelType
{
    /// <summary>Uses OCCT's highest suitable STEP representation.</summary>
    AsIs = 0,
    /// <summary>Writes manifold solid BRep.</summary>
    ManifoldSolidBrep = 1,
    /// <summary>Writes BRep with voids.</summary>
    BrepWithVoids = 2,
    /// <summary>Writes faceted BRep.</summary>
    FacetedBrep = 3,
    /// <summary>Writes faceted BRep including voids.</summary>
    FacetedBrepAndBrepWithVoids = 4,
    /// <summary>Writes a shell-based surface model.</summary>
    ShellBasedSurfaceModel = 5,
    /// <summary>Writes a geometric curve set.</summary>
    GeometricCurveSet = 6,
    /// <summary>Uses OCCT's hybrid representation.</summary>
    Hybrid = 7
}
