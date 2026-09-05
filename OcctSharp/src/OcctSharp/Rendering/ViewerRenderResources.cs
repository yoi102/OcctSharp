using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>A resource whose lifetime and thread belong to the original viewer.</summary>
public abstract class ViewerRenderResource : IDisposable
{
    internal ViewerRenderResource(ViewerRendering owner, long id) { Owner = owner; Id = id; }
    internal ViewerRendering Owner { get; }
    internal long Id { get; }
    internal bool Removed { get; set; }
    internal void Ensure(ViewerRendering expected)
    {
        if (!ReferenceEquals(Owner, expected)) throw new ArgumentException("Resource belongs to another viewer.");
        Owner.EnsureThread(); ObjectDisposedException.ThrowIf(Removed, this);
    }
    public void Dispose() { Owner.RemoveResource(this); GC.SuppressFinalize(this); }
}
public sealed class ViewerTexture : ViewerRenderResource
{
    internal ViewerTexture(ViewerRendering owner, long id, int width, int height) : base(owner, id) { Width = width; Height = height; }
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public void Replace(ViewerPixelImage image) => Owner.ReplaceTexture(this, image);
}
public sealed class ViewerEnvironment : ViewerRenderResource
{
    internal ViewerEnvironment(ViewerRendering owner, long id) : base(owner, id) { }
}
public sealed class ViewerReviewLayer : ViewerRenderResource
{
    internal ViewerReviewLayer(ViewerRendering owner, long id, ViewerLayerProfile profile) : base(owner, id) => Profile = profile;
    public ViewerLayerProfile Profile { get; internal set; }
    public void Update(ViewerLayerProfile profile) => Owner.UpdateLayer(this, profile);
}
public sealed record ViewerAppearanceSnapshot(ViewerAppearanceProfile Profile, ViewerTexture? Texture);

public sealed partial class ViewerRendering
{
    private readonly Dictionary<ViewerPresentation, ViewerAppearanceSnapshot> appearances = [];
    private readonly List<ViewerReviewLayer> layers = [];
    private readonly Dictionary<ViewerPresentation, ViewerReviewLayer> layerAssignments = [];
    private ViewerEnvironment? environment;
    private bool environmentBackground, environmentLighting;

    public unsafe ViewerTexture CreateTexture(ViewerPixelImage image)
    {
        ArgumentNullException.ThrowIfNull(image); EnsureThread(); var raw = PixelDescription(image);
        fixed (byte* bytes = image.Pixels) {
            NativeError.ThrowIfFailed(NativeMethods.TexturePixels(viewer.Handle, 0, in raw, bytes, image.Pixels.Length, out long id), "viewer_texture_pixels");
            return new(this, id, image.Width, image.Height);
        }
    }
    /// <summary>Decodes an explicitly supplied local file. URLs and network shares are rejected; the file is not retained as a resource reference.</summary>
    public ViewerTexture CreateTextureFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path); EnsureThread();
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && !uri.IsFile) throw new ArgumentException("Only local image paths are supported.");
        path = Path.GetFullPath(path); if (path.StartsWith(@"\\", StringComparison.Ordinal) || !File.Exists(path)) throw new ArgumentException("An existing local-drive image is required.");
        NativeError.ThrowIfFailed(NativeMethods.TextureFile(viewer.Handle, path, out long id, out var info), "viewer_texture_file"); return new(this, id, info.Width, info.Height);
    }
    internal unsafe void ReplaceTexture(ViewerTexture texture, ViewerPixelImage image)
    {
        ArgumentNullException.ThrowIfNull(image); texture.Ensure(this); var raw = PixelDescription(image);
        fixed (byte* bytes = image.Pixels) NativeError.ThrowIfFailed(NativeMethods.TexturePixels(viewer.Handle, texture.Id, in raw, bytes, image.Pixels.Length, out _), "viewer_texture_pixels");
        texture.Width = image.Width; texture.Height = image.Height;
    }
    private static PixelInputRaw PixelDescription(ViewerPixelImage image) => new() { Width = image.Width, Height = image.Height, Stride = image.Stride, Format = (int)image.Format, BottomUp = image.BottomUp ? 1 : 0 };

    /// <summary>Replaces the complete shading profile, including document-derived subshape drawers; never edits the source document.</summary>
    public unsafe void SetAppearance(ViewerPresentation presentation, ViewerAppearanceProfile profile, ViewerTexture? texture = null)
    {
        EnsurePresentation(presentation); ArgumentNullException.ThrowIfNull(profile); texture?.Ensure(this);
        ArgumentNullException.ThrowIfNull(profile.Front); ArgumentNullException.ThrowIfNull(profile.Back); ArgumentNullException.ThrowIfNull(profile.Mapping);
        var snapshot = new ViewerAppearanceSnapshot(profile, texture); appearances.EnsureCapacity(appearances.Count + 1);
        var raw = Appearance(profile, texture?.Id ?? 0);
        NativeError.ThrowIfFailed(NativeMethods.ReviewAppearance(viewer.Handle, presentation.Id, &raw), "viewer_appearance"); appearances[presentation] = snapshot;
    }
    public ViewerAppearanceSnapshot? GetAppearance(ViewerPresentation presentation) { EnsurePresentation(presentation); return appearances.GetValueOrDefault(presentation); }
    public unsafe void ResetAppearance(ViewerPresentation presentation)
    {
        EnsurePresentation(presentation); NativeError.ThrowIfFailed(NativeMethods.ReviewAppearance(viewer.Handle, presentation.Id, null), "viewer_appearance_reset"); appearances.Remove(presentation);
    }
    private static unsafe AppearanceRaw Appearance(ViewerAppearanceProfile p, long texture)
    {
        var m = p.Mapping; var result = new AppearanceRaw { Front = Material(p.Front), Back = Material(p.Back), Shading = (int)p.Shading,
            Distinguish = p.DistinguishSides ? 1 : 0, Culling = (int)p.Culling, AlphaMode = (int)p.AlphaMode, AlphaCutoff = p.AlphaCutoff,
            Texture = texture, Planar = m.Planar ? 1 : 0, Repeat = m.Repeat ? 1 : 0, Filter = (int)m.Filter, Anisotropy = (int)m.Anisotropy,
            ScaleS = m.ScaleS, ScaleT = m.ScaleT, TranslateS = m.TranslationS, TranslateT = m.TranslationT, Rotation = m.RotationDegrees };
        result.PlaneS[0] = m.PlaneS.A; result.PlaneS[1] = m.PlaneS.B; result.PlaneS[2] = m.PlaneS.C; result.PlaneS[3] = m.PlaneS.D;
        result.PlaneT[0] = m.PlaneT.A; result.PlaneT[1] = m.PlaneT.B; result.PlaneT[2] = m.PlaneT.C; result.PlaneT[3] = m.PlaneT.D; return result;
    }
    private static ReviewMaterialRaw Material(ViewerReviewMaterial m) => new() { Red = m.Color.Red, Green = m.Color.Green, Blue = m.Color.Blue,
        Alpha = m.Alpha, Metallic = m.Metallic, Roughness = m.Roughness, Ior = m.IndexOfRefraction, Emission = m.Emission };

    /// <summary>Creates an immutable environment from +X,-X,+Y,-Y,+Z,-Z image copies; source texture replacement/removal does not change it.</summary>
    public ViewerEnvironment CreateEnvironment(IReadOnlyList<ViewerTexture> faces)
    {
        ArgumentNullException.ThrowIfNull(faces); if (faces.Count != 6) throw new ArgumentException("Exactly six faces are required.");
        return CreateEnvironmentCore(faces, null);
    }
    /// <summary>Creates a packed environment with an explicit +X,-X,+Y,-Y,+Z,-Z to row-major tile permutation.</summary>
    public ViewerEnvironment CreatePackedEnvironment(ViewerTexture image, IReadOnlyList<int> sideToTile)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(sideToTile);
        if (sideToTile.Count != 6) throw new ArgumentException("Six tile indices are required."); return CreateEnvironmentCore([image], sideToTile.ToArray());
    }
    private unsafe ViewerEnvironment CreateEnvironmentCore(IReadOnlyList<ViewerTexture> images, int[]? order)
    {
        EnsureThread(); var ids = images.Select(x => { ArgumentNullException.ThrowIfNull(x); x.Ensure(this); return x.Id; }).ToArray();
        fixed (long* p = ids) fixed (int* o = order) {
            NativeError.ThrowIfFailed(NativeMethods.EnvironmentCreate(viewer.Handle, p, ids.Length, o, out long id), "viewer_environment_create"); return new(this, id);
        }
    }
    /// <summary>Controls visible background independently from PBR illumination. Null clears both and restores the prior background type.</summary>
    public void SetEnvironment(ViewerEnvironment? value, bool showBackground = true, bool illuminate = true)
    {
        EnsureThread(); value?.Ensure(this); if (value is null) { showBackground = false; illuminate = false; }
        NativeError.ThrowIfFailed(NativeMethods.EnvironmentSet(viewer.Handle, value?.Id ?? 0, showBackground ? 1 : 0, illuminate ? 1 : 0), "viewer_environment_set");
        environment = value; environmentBackground = showBackground; environmentLighting = illuminate;
    }

    public ViewerReviewLayer CreateLayer(ViewerLayerProfile? profile = null)
    {
        EnsureThread(); profile ??= new(); var raw = Layer(profile); layers.EnsureCapacity(layers.Count + 1);
        NativeError.ThrowIfFailed(NativeMethods.ReviewLayerSet(viewer.Handle, 0, in raw, out long id), "viewer_layer_set");
        var result = new ViewerReviewLayer(this, id, profile); layers.Add(result); return result;
    }
    internal void UpdateLayer(ViewerReviewLayer layer, ViewerLayerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile); layer.Ensure(this); var raw = Layer(profile);
        NativeError.ThrowIfFailed(NativeMethods.ReviewLayerSet(viewer.Handle, layer.Id, in raw, out _), "viewer_layer_set"); layer.Profile = profile;
    }
    public void AssignLayer(ViewerPresentation presentation, ViewerReviewLayer? layer)
    {
        EnsurePresentation(presentation); layer?.Ensure(this); layerAssignments.EnsureCapacity(layerAssignments.Count + 1);
        NativeError.ThrowIfFailed(NativeMethods.ReviewLayerAssign(viewer.Handle, presentation.Id, layer?.Id ?? 0), "viewer_layer_assign");
        if (layer is null) layerAssignments.Remove(presentation); else layerAssignments[presentation] = layer;
    }
    private static ReviewLayerRaw Layer(ViewerLayerProfile p) => new() { DepthTest = p.DepthTest ? 1 : 0, DepthWrite = p.DepthWrite ? 1 : 0, ClearDepth = p.ClearDepth ? 1 : 0, Immediate = p.Immediate ? 1 : 0 };
    internal void ForgetPresentation(ViewerPresentation p) { appearances.Remove(p); layerAssignments.Remove(p); }
    internal void RemoveResource(ViewerRenderResource resource)
    {
        if (resource.Removed || IsDisposed) { resource.Removed = true; return; }
        resource.Ensure(this);
        switch (resource)
        {
            case ViewerTexture texture:
                NativeError.ThrowIfFailed(NativeMethods.TextureRemove(viewer.Handle, resource.Id), "viewer_texture_remove");
                foreach (var item in appearances.ToArray()) if (ReferenceEquals(item.Value.Texture, texture)) appearances[item.Key] = item.Value with { Texture = null };
                break;
            case ViewerEnvironment:
                NativeError.ThrowIfFailed(NativeMethods.EnvironmentRemove(viewer.Handle, resource.Id), "viewer_environment_remove");
                if (ReferenceEquals(environment, resource)) { environment = null; environmentBackground = environmentLighting = false; }
                break;
            case ViewerReviewLayer layer:
                NativeError.ThrowIfFailed(NativeMethods.ReviewLayerRemove(viewer.Handle, resource.Id), "viewer_layer_remove"); layers.Remove(layer);
                foreach (var item in layerAssignments.Where(x => ReferenceEquals(x.Value, layer)).ToArray()) layerAssignments.Remove(item.Key);
                break;
            default: throw new ArgumentException("Unknown viewer resource.");
        }
        resource.Removed = true;
    }
}
