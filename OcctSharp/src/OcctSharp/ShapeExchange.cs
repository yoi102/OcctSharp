using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Provides geometry-only BRep and mesh exchange workflows.</summary>
public static class ShapeExchange
{
    /// <summary>Reads all transferable roots from a STEP file into one owned shape.</summary>
    public static Shape ReadStep(string filePath)
    {
        string fullPath = ResolveInputPath(filePath);
        OcctRuntime.EnsureCompatible();
        NativeStatus status = NativeMethods.ReadStep(fullPath, out nint nativeShape);
        NativeError.ThrowIfFailed(status, "shape_read_step");
        return ShapeFactory.FromNativeHandle(nativeShape, "shape_read_step");
    }

    /// <summary>Reads all transferable roots from an IGES file into one owned shape.</summary>
    public static Shape ReadIges(string filePath)
    {
        string fullPath = ResolveInputPath(filePath);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.ReadIges(fullPath, out nint nativeShape), "shape_read_iges");
        return ShapeFactory.FromNativeHandle(nativeShape, "shape_read_iges");
    }

    /// <summary>Reads an STL file into a faceted owned shape.</summary>
    public static Shape ReadStl(string filePath)
    {
        string fullPath = ResolveInputPath(filePath);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.ReadStl(fullPath, out nint nativeShape), "shape_read_stl");
        return ShapeFactory.FromNativeHandle(nativeShape, "shape_read_stl");
    }

    /// <summary>Reads an OBJ mesh into one owned faceted shape.</summary>
    public static Shape ReadObj(string filePath) => ReadMesh(filePath, NativeMethods.ReadObj, "shape_read_obj");

    /// <summary>Reads a glTF or GLB scene into one owned faceted shape.</summary>
    public static Shape ReadGltf(string filePath) => ReadMesh(filePath, NativeMethods.ReadGltf, "shape_read_gltf");

    /// <summary>Reads a VRML scene into one owned faceted shape.</summary>
    public static Shape ReadVrml(string filePath) => ReadMesh(filePath, NativeMethods.ReadVrml, "shape_read_vrml");

    /// <summary>Writes an owned shape as ordinary geometry STEP.</summary>
    public static string WriteStep(Shape shape, string filePath)
    {
        ArgumentNullException.ThrowIfNull(shape);
        string fullPath = PrepareOutputPath(filePath);
        NativeError.ThrowIfFailed(NativeMethods.WriteStep(shape.Handle, fullPath), "shape_write_step");
        return fullPath;
    }

    /// <summary>Triangulates and writes a shape as STL.</summary>
    public static string WriteStl(Shape shape, string filePath, StlWriteOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(shape);
        string fullPath = PrepareOutputPath(filePath);
        StlWriteOptions effectiveOptions = options ?? StlWriteOptions.Default;
        NativeError.ThrowIfFailed(
            NativeMethods.WriteStl(
                shape.Handle,
                fullPath,
                effectiveOptions.LinearDeflection,
                effectiveOptions.AngularDeflection,
                effectiveOptions.Binary ? 1 : 0),
            "shape_write_stl");
        return fullPath;
    }

    /// <summary>Writes a shape as BRep-mode IGES geometry in millimeters.</summary>
    public static string WriteIges(Shape shape, string filePath)
    {
        ArgumentNullException.ThrowIfNull(shape);
        string fullPath = PrepareOutputPath(filePath);
        NativeError.ThrowIfFailed(NativeMethods.WriteIges(shape.Handle, fullPath), "shape_write_iges");
        return fullPath;
    }

    /// <summary>Triangulates and writes a shape as OBJ.</summary>
    public static string WriteObj(Shape shape, string filePath) => WriteMesh(shape, filePath, NativeMethods.WriteObj, "shape_write_obj");

    /// <summary>Triangulates and writes a shape as PLY. OCCT 8.0.1 does not provide PLY import.</summary>
    public static string WritePly(Shape shape, string filePath) => WriteMesh(shape, filePath, NativeMethods.WritePly, "shape_write_ply");

    /// <summary>Triangulates and writes a shape as glTF or GLB according to the extension.</summary>
    public static string WriteGltf(Shape shape, string filePath) => WriteMesh(shape, filePath, NativeMethods.WriteGltf, "shape_write_gltf");

    /// <summary>Triangulates and writes a shape as VRML.</summary>
    public static string WriteVrml(Shape shape, string filePath) => WriteMesh(shape, filePath, NativeMethods.WriteVrml, "shape_write_vrml");

    private delegate NativeStatus MeshRead(string filePath, out nint shape);
    private delegate NativeStatus MeshWrite(ShapeHandle shape, string filePath);

    private static Shape ReadMesh(string filePath, MeshRead reader, string operation)
    {
        string fullPath = ResolveInputPath(filePath);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(reader(fullPath, out nint nativeShape), operation);
        return ShapeFactory.FromNativeHandle(nativeShape, operation);
    }

    private static string WriteMesh(Shape shape, string filePath, MeshWrite writer, string operation)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
        string fullPath = PrepareOutputPath(filePath);
        NativeError.ThrowIfFailed(writer(shape.Handle, fullPath), operation);
        return fullPath;
    }

    private static string ResolveInputPath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The input file does not exist.", fullPath);
        }

        return fullPath;
    }

    private static string PrepareOutputPath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return fullPath;
    }
}
