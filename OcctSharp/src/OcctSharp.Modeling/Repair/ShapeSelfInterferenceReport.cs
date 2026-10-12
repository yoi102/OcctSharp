namespace OcctSharp;

/// <summary>Copied OCCT fault record counts; these are not counts of intersecting pairs or a topology correspondence.</summary>
/// <param name="SelfInterferenceFaultCount">Records classified by OCCT as self-interference.</param>
/// <param name="OtherFaultCount">Other or unknown faults encountered by the requested check.</param>
public sealed record ShapeSelfInterferenceReport(int SelfInterferenceFaultCount,int OtherFaultCount)
{
    /// <summary>True only when the completed check reports no self-interference and no other faults. Local BRep validity is a separate check.</summary>
    public bool IsClear=>SelfInterferenceFaultCount==0&&OtherFaultCount==0;
}
