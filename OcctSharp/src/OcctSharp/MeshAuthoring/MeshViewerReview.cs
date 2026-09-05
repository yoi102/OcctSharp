using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Viewer-thread-bound authored mesh review. A replacement invalidates every old revision token and presentation.</summary>
public sealed class MeshViewerReview : IDisposable
{
    private readonly OcctViewer viewer;
    private bool disposed;
    public MeshViewerReview(OcctViewer viewer, AuthoredMesh mesh, IReadOnlyDictionary<string, XdeVisualMaterial>? materials = null)
    {
        ArgumentNullException.ThrowIfNull(viewer); ArgumentNullException.ThrowIfNull(mesh);
        viewer.EnsureThread(); this.viewer = viewer; Presentation = CreatePresentation(mesh, materials); Revision = mesh.Revision;
    }
    public MeshRevision Revision { get; private set; }
    public ViewerPresentation Presentation { get; private set; }
    public void SelectAndFit(MeshRevision revision)
    {
        Validate();
        if (revision != Revision) throw new ArgumentException("Mesh review token belongs to a foreign or stale revision.", nameof(revision));
        NativeError.ThrowIfFailed(NativeMethods.RepairViewerSelect(viewer.Handle, Presentation.Id), "mesh_viewer_select");
        viewer.FitSelected(); viewer.Redraw();
    }
    public void Replace(AuthoredMesh mesh, IReadOnlyDictionary<string, XdeVisualMaterial>? materials = null)
    {
        Validate(); ArgumentNullException.ThrowIfNull(mesh);
        if (mesh.Revision == Revision) throw new ArgumentException("Replacement requires a new authored mesh revision.");
        ViewerPresentation next = CreatePresentation(mesh, materials);
        try { Presentation.Dispose(); }
        catch { next.Dispose(); throw; }
        Presentation = next; Revision = mesh.Revision;
    }
    private ViewerPresentation CreatePresentation(AuthoredMesh mesh, IReadOnlyDictionary<string, XdeVisualMaterial>? materials)
    {
        using XdeDocument document = XdeDocument.Create();
        MeshAssemblyProduct product = MeshAssembly.Create(document, mesh, "Mesh review", materials: materials);
        ViewerPresentation result = viewer.Display(product.Root);
        try { result.SetDisplayMode(ViewerDisplayMode.Shaded); return result; }
        catch { result.Dispose(); throw; }
    }
    private void Validate() { ObjectDisposedException.ThrowIf(disposed, this); viewer.EnsureThread(); }
    public void Dispose()
    {
        if (disposed) return;
        if (viewer.IsDisposed) { disposed = true; return; } // Parent already removed every native presentation.
        viewer.EnsureThread(); Presentation.Dispose(); disposed = true;
    }
}
