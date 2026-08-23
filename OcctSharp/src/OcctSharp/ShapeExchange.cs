using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Provides the initial geometry-only STEP, STL, and IGES workflows.</summary>
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
