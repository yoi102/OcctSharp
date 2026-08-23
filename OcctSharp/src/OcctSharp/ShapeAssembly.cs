using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Builds geometry-only compounds from independently owned shapes.</summary>
public static class ShapeAssembly
{
    /// <summary>Transforms every input and adds it to a new owned compound shape.</summary>
    public static Shape Create(IEnumerable<ShapePlacement> placements)
    {
        ArgumentNullException.ThrowIfNull(placements);
        OcctRuntime.EnsureCompatible();

        NativeStatus status = NativeMethods.CreateCompound(out nint nativeCompound);
        NativeError.ThrowIfFailed(status, "shape_create_compound");
        Shape compound = ShapeFactory.FromNativeHandle(nativeCompound, "shape_create_compound");

        try
        {
            int count = 0;
            foreach (ShapePlacement placement in placements)
            {
                ArgumentNullException.ThrowIfNull(placement);
                ArgumentNullException.ThrowIfNull(placement.Shape);
                using Shape transformed = placement.Shape.Transformed(placement.Transform);
                NativeError.ThrowIfFailed(
                    NativeMethods.AddToCompound(compound.Handle, transformed.Handle),
                    "compound_add");
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException("At least one shape placement is required.", nameof(placements));
            }

            return compound;
        }
        catch
        {
            compound.Dispose();
            throw;
        }
    }
}
