using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns a native OCCT topology shape.</summary>
public partial class Shape : IDisposable
{
    private readonly ShapeHandle handle;

    internal Shape(ShapeHandle handle)
    {
        this.handle = handle;
    }

    internal ShapeHandle Handle => handle;

    /// <summary>Gets the number of faces contained in this shape.</summary>
    public int FaceCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetFaceCount(handle, out int faceCount),
                "shape_get_face_count");
            return faceCount;
        }
    }

    /// <summary>Creates an owned shape with the supplied rigid transform applied.</summary>
    public Shape Transformed(ShapeTransform transform)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();
        NativeStatus status = NativeMethods.TransformShape(
            handle,
            transform.TranslationX,
            transform.TranslationY,
            transform.TranslationZ,
            transform.RotationAxisX,
            transform.RotationAxisY,
            transform.RotationAxisZ,
            transform.RotationAngleRadians,
            out nint transformedShape);
        NativeError.ThrowIfFailed(status, "shape_transform");
        return ShapeFactory.FromNativeHandle(transformedShape, "shape_transform");
    }

    /// <summary>Creates an owned shape by applying an OCCT <c>gp_Trsf</c> value.</summary>
    public Shape Transformed(GpTrsf transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return transform.Apply(this);
    }

    /// <summary>Returns an independently owned shape with an absolute location.</summary>
    public Shape Located(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Locate(this);
    }

    /// <summary>Returns an independently owned shape moved by a location.</summary>
    public Shape Moved(TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        return location.Move(this);
    }

    /// <summary>Releases the owned native shape.</summary>
    public void Dispose()
    {
        handle.Dispose();
        GC.SuppressFinalize(this);
    }
}
