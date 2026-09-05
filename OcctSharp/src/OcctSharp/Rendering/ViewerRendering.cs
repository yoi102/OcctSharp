using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Parent/thread-bound controller for one viewer's render state and owned resources.</summary>
public sealed partial class ViewerRendering
{
    private readonly OcctViewer viewer;
    private readonly Dictionary<long, ViewerLight> lights = [];
    internal ViewerRendering(OcctViewer viewer) => this.viewer = viewer;
    internal bool IsDisposed => viewer.IsDisposed;
    internal void EnsureThread() => viewer.EnsureThread();
    private void EnsurePresentation(ViewerPresentation value)
    {
        ArgumentNullException.ThrowIfNull(value); EnsureThread();
        if (!ReferenceEquals(value.Viewer, viewer)) throw new ArgumentException("Presentation belongs to another viewer.");
        ObjectDisposedException.ThrowIf(value.IsRemoved, value);
    }
    /// <summary>Queries the current context's driver limits without exposing its handle.</summary>
    public ViewerRenderCapabilities GetCapabilities()
    {
        EnsureThread(); NativeError.ThrowIfFailed(NativeMethods.RenderCaps(viewer.Handle, out var c), "viewer_render_caps");
        return new(c.MaxLights, c.MaxTexture, c.MaxDumpX, c.MaxDumpY, c.MaxTextureUnits, c.MaxMsaa,
            c.Pbr != 0, c.Raytracing != 0, c.Srgb != 0, c.Oit != 0, c.OitMsaa != 0, c.MaxAnisotropy);
    }
    /// <summary>Returns copied effective rendering values.</summary>
    public unsafe ViewerRenderProfile GetProfile()
    {
        EnsureThread(); NativeError.ThrowIfFailed(NativeMethods.RenderProfile(viewer.Handle, null, out var p), "viewer_render_profile"); return FromRaw(p);
    }
    /// <summary>Applies a complete validated quality profile and returns effective float-precision values; no silent capability downgrade.</summary>
    public unsafe ViewerRenderProfile SetProfile(ViewerRenderProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile); EnsureThread(); var p = ToRaw(profile);
        NativeError.ThrowIfFailed(NativeMethods.RenderProfile(viewer.Handle, &p, out var effective), "viewer_render_profile"); return FromRaw(effective);
    }
    private static RenderProfileRaw ToRaw(ViewerRenderProfile p) => new() {
        Mode = (int)p.Mode, Shading = (int)p.Shading, Msaa = p.MsaaSamples, Transparency = (int)p.Transparency, ToneMapping = (int)p.ToneMapping,
        ResolutionScale = p.ResolutionScale, OitDepthFactor = p.OitDepthFactor, Exposure = p.Exposure, WhitePoint = p.WhitePoint,
        EnvironmentPower = p.EnvironmentPower, EnvironmentLevels = p.EnvironmentLevels, DiffuseSamples = p.DiffuseSamples,
        SpecularSamples = p.SpecularSamples, BakeProbability = p.BakeProbability };
    private static ViewerRenderProfile FromRaw(RenderProfileRaw p) => new() {
        Mode = (ViewerRenderMode)p.Mode, Shading = (ViewerShading)p.Shading, MsaaSamples = p.Msaa, Transparency = (ViewerTransparencyMethod)p.Transparency, ToneMapping = (ViewerToneMapping)p.ToneMapping,
        ResolutionScale = p.ResolutionScale, OitDepthFactor = p.OitDepthFactor, Exposure = p.Exposure, WhitePoint = p.WhitePoint,
        EnvironmentPower = p.EnvironmentPower, EnvironmentLevels = p.EnvironmentLevels, DiffuseSamples = p.DiffuseSamples,
        SpecularSamples = p.SpecularSamples, BakeProbability = p.BakeProbability };

    /// <summary>Replaces the whole rig with fresh parent-bound identities; validation/allocation failure preserves the old rig.</summary>
    public IReadOnlyList<ViewerLight> ReplaceLightRig(IReadOnlyList<ViewerLightDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions); EnsureThread();
        if (definitions.Count > 128) throw new ArgumentException("Light rig exceeds 128 owned entries.");
        var raw = definitions.Select(d => ToRaw(d, 0)).ToArray();
        var result = ApplyLights(raw); return Array.AsReadOnly(result);
    }
    /// <summary>Creates one light while retaining the other registered lights, including inactive entries.</summary>
    public ViewerLight CreateLight(ViewerLightDefinition definition)
    {
        var current = ReadLights(); if (current.Length >= 128) throw new ArgumentException("Light budget exceeded.");
        var updated = current.Append(ToRaw(definition, 0)).ToArray(); return ApplyLights(updated)[^1];
    }
    /// <summary>Snapshots portable definitions without native or parent-bound identifiers.</summary>
    public IReadOnlyList<ViewerLightDefinition> SnapshotLightRig() => Array.AsReadOnly(ReadLights().Select(FromRaw).ToArray());
    private unsafe LightRaw[] ReadLights()
    {
        EnsureThread(); NativeError.ThrowIfFailed(NativeMethods.LightsSnapshot(viewer.Handle, null, 0, out int count), "viewer_lights_snapshot");
        var result = new LightRaw[count]; fixed (LightRaw* p = result)
            NativeError.ThrowIfFailed(NativeMethods.LightsSnapshot(viewer.Handle, p, result.Length, out _), "viewer_lights_snapshot");
        foreach (var item in result) if (!lights.ContainsKey(item.Id)) lights.Add(item.Id, new ViewerLight(this) { Id = item.Id });
        return result;
    }
    private unsafe ViewerLight[] ApplyLights(LightRaw[] input)
    {
        EnsureThread(); var ids = new long[input.Length];
        var result = input.Select(x => x.Id != 0 ? lights[x.Id] : new ViewerLight(this)).ToArray();
        var next = new Dictionary<long, ViewerLight>(input.Length);
        lights.EnsureCapacity(input.Length);
        fixed (LightRaw* p = input) fixed (long* o = ids)
            NativeError.ThrowIfFailed(NativeMethods.LightsReplace(viewer.Handle, p, input.Length, o, ids.Length), "viewer_lights_replace");
        for (int i = 0; i < result.Length; ++i) { result[i].Id = ids[i]; next.Add(ids[i], result[i]); }
        foreach (var item in lights) if (!next.ContainsKey(item.Key)) item.Value.Removed = true;
        lights.Clear(); foreach (var item in next) lights.Add(item.Key, item.Value); return result;
    }
    internal ViewerLightDefinition GetLight(ViewerLight light) { light.Ensure(this); return FromRaw(ReadLights().Single(x => x.Id == light.Id)); }
    internal void UpdateLight(ViewerLight light, ViewerLightDefinition definition)
    { light.Ensure(this); var current = ReadLights(); current[Array.FindIndex(current, x => x.Id == light.Id)] = ToRaw(definition, light.Id); _ = ApplyLights(current); }
    internal void RemoveLight(ViewerLight light)
    {
        if (light.Removed || IsDisposed) { light.Removed = true; return; }
        light.Ensure(this); _ = ApplyLights(ReadLights().Where(x => x.Id != light.Id).ToArray());
    }
    private static LightRaw ToRaw(ViewerLightDefinition d, long id)
    {
        ArgumentNullException.ThrowIfNull(d);
        return new() { Id = id, Kind = (int)d.Kind, Active = d.Active ? 1 : 0, Headlight = d.Headlight ? 1 : 0,
            Red = d.Color.Red, Green = d.Color.Green, Blue = d.Color.Blue, Intensity = d.Intensity,
            X = d.Position.X, Y = d.Position.Y, Z = d.Position.Z, Dx = d.Direction.X, Dy = d.Direction.Y, Dz = d.Direction.Z,
            ConstantAttenuation = d.ConstantAttenuation, LinearAttenuation = d.LinearAttenuation, Range = d.Range, Angle = d.SpotAngle, Concentration = d.Concentration };
    }
    private static ViewerLightDefinition FromRaw(LightRaw d) => new((ViewerLightKind)d.Kind, new(d.Red, d.Green, d.Blue)) {
        Active = d.Active != 0, Headlight = d.Headlight != 0, Intensity = d.Intensity, Position = new(d.X, d.Y, d.Z),
        Direction = new(d.Dx, d.Dy, d.Dz), ConstantAttenuation = d.ConstantAttenuation, LinearAttenuation = d.LinearAttenuation,
        Range = d.Range, SpotAngle = d.Angle, Concentration = d.Concentration };
}

/// <summary>A light entry owned by one viewer; inactive lights remain owned until removed.</summary>
public sealed class ViewerLight : IDisposable
{
    private readonly ViewerRendering owner;
    internal ViewerLight(ViewerRendering owner) => this.owner = owner;
    internal long Id { get; set; }
    internal bool Removed { get; set; }
    internal void Ensure(ViewerRendering expected) {
        if (!ReferenceEquals(owner, expected)) throw new ArgumentException("Light belongs to another viewer.");
        owner.EnsureThread(); ObjectDisposedException.ThrowIf(Removed, this);
    }
    /// <summary>Gets the current copied definition.</summary>
    public ViewerLightDefinition Definition => owner.GetLight(this);
    /// <summary>Atomically updates this light within the full rig.</summary>
    public void Update(ViewerLightDefinition definition) => owner.UpdateLight(this, definition);
    /// <summary>Removes this entry without affecting other registered lights.</summary>
    public void Dispose() => owner.RemoveLight(this);
}
