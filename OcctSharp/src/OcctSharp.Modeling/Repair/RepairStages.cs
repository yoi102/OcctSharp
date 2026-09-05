using OcctSharp.Interop;
using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591

[JsonPolymorphic(TypeDiscriminatorPropertyName = "operation")]
[JsonDerivedType(typeof(ReorderWireRepair), "reorder-wire")]
[JsonDerivedType(typeof(ConnectWireRepair), "connect-wire")]
[JsonDerivedType(typeof(WireframeGapRepair), "wireframe-gaps")]
[JsonDerivedType(typeof(SmallEdgeRepair), "small-edges")]
[JsonDerivedType(typeof(FaceNormalizationRepair), "normalize-face")]
[JsonDerivedType(typeof(ShellNormalizationRepair), "normalize-shell")]
[JsonDerivedType(typeof(SolidNormalizationRepair), "normalize-solid")]
[JsonDerivedType(typeof(SmallFaceRepair), "small-faces")]
[JsonDerivedType(typeof(SmallSolidRepair), "small-solids")]
[JsonDerivedType(typeof(ToleranceNormalizationRepair), "normalize-tolerances")]
[JsonDerivedType(typeof(SewingRepair), "sew")]
[JsonDerivedType(typeof(InternalHoleRemovalRepair), "remove-holes")]
[JsonDerivedType(typeof(LocationNormalizationRepair), "normalize-locations")]
[JsonDerivedType(typeof(ContinuityDivisionRepair), "divide-continuity")]
[JsonDerivedType(typeof(AngularDivisionRepair), "divide-angle")]
[JsonDerivedType(typeof(AreaDivisionRepair), "divide-area")]
[JsonDerivedType(typeof(ClosedFaceDivisionRepair), "divide-closed-faces")]
[JsonDerivedType(typeof(ClosedEdgeDivisionRepair), "divide-closed-edges")]
[JsonDerivedType(typeof(SameDomainUnificationRepair), "unify")]
[JsonDerivedType(typeof(TopologyEditRepair), "edit-topology")]
public abstract record RepairStage
{
    private protected RepairStage() { }
    internal RepairStageRaw ToRaw(RepairTolerancePolicy tolerance, int maximumTopology)
    {
        RepairStageRaw value = this switch
        {
            ReorderWireRepair s => Raw(0, s.Closed ? 1 : 0, s.Reconnect ? 1 : 0),
            ConnectWireRepair s => Raw(1, s.Closed ? 1 : 0),
            WireframeGapRepair => Raw(2),
            SmallEdgeRepair s => Raw(3, s.AllowDrop ? 1 : 0, threshold: Positive(s.MaximumLength), angle: Positive(s.MaximumMergeAngle)),
            FaceNormalizationRepair s => Raw(4, Control(s.Orientation), Control(s.AddNaturalBound), Control(s.Wires)),
            ShellNormalizationRepair s => Raw(5, s.AllowNonManifold ? 1 : 0),
            SolidNormalizationRepair s => Raw(6, Control(s.ShellRepair)),
            SmallFaceRepair s => Raw(7, threshold: Positive(s.MaximumWidth)),
            SmallSolidRepair s => Raw(8, threshold: Positive(s.MaximumVolume)),
            ToleranceNormalizationRepair s when s.Kind is ShapeKind.Vertex or ShapeKind.Edge or ShapeKind.Face => Raw(9, (int)s.Kind),
            SewingRepair s => Raw(10, s.AllowNonManifold ? 1 : 0, s.UseLocalTolerances ? 1 : 0),
            InternalHoleRemovalRepair s => Raw(11, threshold: Positive(s.MaximumArea)),
            LocationNormalizationRepair s when (int)s.Level is >= 0 and <= 4 => Raw(12, (int)s.Level),
            ContinuityDivisionRepair s when s.Continuity is ParametricRepairContinuity.C0 or ParametricRepairContinuity.C1
                or ParametricRepairContinuity.C2 or ParametricRepairContinuity.C3 or ParametricRepairContinuity.CN =>
                Raw(13, (int)s.Continuity, threshold: Positive(s.Tolerance2d)),
            AngularDivisionRepair s when s.MaximumAngle <= Math.Tau => Raw(14, angle: Positive(s.MaximumAngle)),
            AreaDivisionRepair s => Raw(15, threshold: Positive(s.MaximumArea)),
            ClosedFaceDivisionRepair s when s.Parts is >= 2 and <= 64 => Raw(16, parts: s.Parts),
            ClosedEdgeDivisionRepair s when s.Parts is >= 2 and <= 64 => Raw(17, parts: s.Parts),
            SameDomainUnificationRepair s when s.Edges || s.Faces => Raw(18, s.Edges ? 1 : 0, s.Faces ? 1 : 0,
                s.AllowInternalEdges ? 1 : 0, angle: Positive(s.AngularTolerance)),
            TopologyEditRepair => Raw(19),
            _ => throw new ArgumentException("Unsupported or contradictory repair controls.")
        };
        return value;
        RepairStageRaw Raw(int operation, int mode1 = 0, int mode2 = 0, int mode3 = 0,
            int parts = 0, double threshold = 0, double angle = 0) =>
            new(operation, mode1, mode2, mode3, parts, maximumTopology, tolerance.Minimum, tolerance.Maximum, threshold, angle);
    }
    internal static double Positive(double value)
    {
        if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be finite and positive.");
        return value;
    }
    private static int Control(RepairControl value) => Enum.IsDefined(value) ? (int)value : throw new ArgumentOutOfRangeException(nameof(value));
}

public sealed record ReorderWireRepair(bool Closed = false, bool Reconnect = false) : RepairStage;
public sealed record ConnectWireRepair(bool Closed = false) : RepairStage;
public sealed record WireframeGapRepair : RepairStage;
public sealed record SmallEdgeRepair(double MaximumLength, bool AllowDrop = false, double MaximumMergeAngle = 0.05) : RepairStage;
public sealed record FaceNormalizationRepair(RepairControl Orientation = RepairControl.On,
    RepairControl AddNaturalBound = RepairControl.Off, RepairControl Wires = RepairControl.Auto) : RepairStage;
public sealed record ShellNormalizationRepair(bool AllowNonManifold = false) : RepairStage;
public sealed record SolidNormalizationRepair(RepairControl ShellRepair = RepairControl.Auto) : RepairStage;
public sealed record SmallFaceRepair(double MaximumWidth) : RepairStage;
public sealed record SmallSolidRepair(double MaximumVolume) : RepairStage;
public sealed record ToleranceNormalizationRepair(ShapeKind Kind) : RepairStage;
public sealed record SewingRepair(bool AllowNonManifold = false, bool UseLocalTolerances = false) : RepairStage;
public sealed record InternalHoleRemovalRepair(double MaximumArea) : RepairStage;
public sealed record LocationNormalizationRepair(ShapeKind Level = ShapeKind.Face) : RepairStage;
// Deliberately mirrors only supported OCCT parametric criteria, excluding G1/G2.
public enum ParametricRepairContinuity { C0 = 0, C1 = 2, C2 = 4, C3 = 5, CN = 6 }
public sealed record ContinuityDivisionRepair(ParametricRepairContinuity Continuity = ParametricRepairContinuity.C1, double Tolerance2d = 1e-7) : RepairStage;
public sealed record AngularDivisionRepair(double MaximumAngle) : RepairStage;
public sealed record AreaDivisionRepair(double MaximumArea) : RepairStage;
public sealed record ClosedFaceDivisionRepair(int Parts = 2) : RepairStage;
public sealed record ClosedEdgeDivisionRepair(int Parts = 2) : RepairStage;
public sealed record SameDomainUnificationRepair(bool Edges = true, bool Faces = true,
    bool AllowInternalEdges = false, double AngularTolerance = 1e-7) : RepairStage;
public readonly record struct RepairTopologyEdit(RepairSelection Target, RepairSelection? Replacement);
public sealed record TopologyEditRepair : RepairStage
{
    public IReadOnlyList<RepairTopologyEdit> Edits { get; }
    [JsonConstructor]
    public TopologyEditRepair(IReadOnlyList<RepairTopologyEdit> edits)
    {
        ArgumentNullException.ThrowIfNull(edits);
        Edits = Array.AsReadOnly(edits.ToArray());
        if (Edits.Count == 0) throw new ArgumentException("An edit stage requires edits.", nameof(edits));
    }
}

public sealed record RepairStep
{
    public string Name { get; }
    public RepairStage Stage { get; }
    /// <summary>Empty selects the complete source; otherwise selects disjoint source-bound closures.</summary>
    public IReadOnlyList<RepairSelection> Selection { get; }
    public RepairControl Control { get; }
    [JsonConstructor]
    public RepairStep(string name, RepairStage stage, IReadOnlyList<RepairSelection>? selection = null, RepairControl control = RepairControl.On)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name); ArgumentNullException.ThrowIfNull(stage);
        if (!Enum.IsDefined(control)) throw new ArgumentOutOfRangeException(nameof(control));
        (Name, Stage, Control) = (name, stage, control);
        Selection = Array.AsReadOnly(selection?.ToArray() ?? []);
    }
}
