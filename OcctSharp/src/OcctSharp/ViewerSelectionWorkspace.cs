using OcctSharp.SelectionWorkspaces;

namespace OcctSharp.SelectionWorkspaces;

/// <summary>Copied world hit and exact presentation/source identity. Overlays may have no presentation.</summary>
public sealed record ViewerWorldPick(bool Hit, GpPoint? Point, ViewerPresentation? Presentation, ViewerSourceIdentity? SourceIdentity);

/// <summary>Creating-thread viewer queries backed by generated Visualization bindings.</summary>
public sealed class ViewerSelectionWorkspace
{
    private readonly OcctViewer viewer;
    private readonly SelectionWorkspaceCore core;
    internal ViewerSelectionWorkspace(OcctViewer viewer)
    {
        this.viewer = viewer;
        core = new(viewer.Handle, viewer.EnsureThread);
    }
    /// <summary>Captures a copied point frustum for the current camera and scene revision.</summary>
    public ViewerSelectingVolume CreatePointVolume(double x, double y, int pixelTolerance = 2) => core.CreatePointVolume(x, y, pixelTolerance);
    /// <summary>Captures a copied box frustum with full inclusion or crossing semantics.</summary>
    public ViewerSelectingVolume CreateBoxVolume(double x1, double y1, double x2, double y2, bool allowOverlap = false) => core.CreateBoxVolume(x1, y1, x2, y2, allowOverlap);
    /// <summary>Picks an actual AIS scene point in client pixels.</summary>
    public ViewerWorldPick PickPoint(int x, int y, bool stickToRay = false) => Resolve(core.PickPoint(x, y, stickToRay));
    /// <summary>Picks an actual AIS scene point along a world axis.</summary>
    public ViewerWorldPick PickAxis(GpPoint origin, GpXyz direction) => Resolve(core.PickAxis(origin, direction));
    /// <summary>Copies the SDK scene gravity point.</summary>
    public GpPoint GravityPoint() => core.GravityPoint();
    /// <summary>Copies SDK view colors and driver diagnostics.</summary>
    public ViewerSelectionDiagnostics Diagnostics() => core.Diagnostics();
    private ViewerWorldPick Resolve(SelectionSceneHit hit)
    {
        var presentation = hit.PresentationId == 0 ? null : viewer.ResolveSelectionPresentation(hit.PresentationId);
        return new(hit.Hit, hit.Point, presentation, presentation?.SourceIdentity);
    }
}
