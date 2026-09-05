using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Copied monographic camera. FOV is degrees; clipping planes are eye-space distances.
/// Perspective scale is derived from eye/target distance and FOV; orthographic scale is explicit.</summary>
public sealed record ViewerReviewCamera(GpPoint Eye, GpPoint Target, GpXyz Up, double Aspect,
    double Scale, double FieldOfViewY, double NearPlane, double FarPlane, bool Perspective, bool AutoFitDepth);

public sealed partial class ViewerRendering
{
    public unsafe ViewerReviewCamera GetCamera()
    {
        EnsureThread(); NativeError.ThrowIfFailed(NativeMethods.ReviewCamera(viewer.Handle, null, out var raw), "viewer_review_camera");
        return Camera(raw);
    }
    public unsafe ViewerReviewCamera SetCamera(ViewerReviewCamera camera)
    {
        ArgumentNullException.ThrowIfNull(camera); EnsureThread();
        var raw = new ReviewCameraRaw { Aspect = camera.Aspect, Scale = camera.Scale, FovY = camera.FieldOfViewY,
            Near = camera.NearPlane, Far = camera.FarPlane, Perspective = camera.Perspective ? 1 : 0, AutoDepth = camera.AutoFitDepth ? 1 : 0 };
        raw.Eye[0] = camera.Eye.X; raw.Eye[1] = camera.Eye.Y; raw.Eye[2] = camera.Eye.Z;
        raw.Target[0] = camera.Target.X; raw.Target[1] = camera.Target.Y; raw.Target[2] = camera.Target.Z;
        raw.Up[0] = camera.Up.X; raw.Up[1] = camera.Up.Y; raw.Up[2] = camera.Up.Z;
        NativeError.ThrowIfFailed(NativeMethods.ReviewCamera(viewer.Handle, &raw, out var effective), "viewer_review_camera");
        return Camera(effective);
    }
    private static unsafe ViewerReviewCamera Camera(ReviewCameraRaw r) => new(new(r.Eye[0],r.Eye[1],r.Eye[2]),
        new(r.Target[0],r.Target[1],r.Target[2]), new(r.Up[0],r.Up[1],r.Up[2]), r.Aspect,r.Scale,r.FovY,r.Near,r.Far,r.Perspective != 0,r.AutoDepth != 0);
}
