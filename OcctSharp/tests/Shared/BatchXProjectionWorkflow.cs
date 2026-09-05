namespace OcctSharp.Tests.Shared;

internal static class BatchXProjectionWorkflow
{
    internal static void Run()
    {
        using TDataStdByteArray bytes = new();
        bytes.Init(-2, 1);
        bytes.SetValue(-2, byte.MaxValue);
        bytes.SetValue(1, byte.MinValue);
        if (bytes.Value(-2) != 255 || bytes.Value(1) != 0 || bytes.Length() != 4)
            throw new InvalidOperationException("Generated unsigned-byte document mapping failed.");

        using ImagePixMap image = new();
        if (!image.InitZero(ImageFormat.Image_Format_RGBA, 17, 9, 0, 0x80)
            || image.Width() != 17 || image.Height() != 9 || image.SizeBytes() != 17UL * 9 * 4)
            throw new InvalidOperationException("Generated size_t image dimensions/stride failed.");

        using PolyTriangulation mesh = new();
        if (mesh.Parameters() is not null) throw new InvalidOperationException("Null referenced handle must stay null.");
        using PolyTriangulationParameters parameters = new(0.01, 0.2, 0.001);
        mesh.Parameters(parameters);
        using PolyTriangulationParameters retained = mesh.Parameters()
            ?? throw new InvalidOperationException("Retained parameter return is missing.");
        mesh.SetMeshPurpose(0x80000001U);
        if (mesh.MeshPurpose() != 0x80000001U) throw new InvalidOperationException("Unsigned 32-bit mask lost its high bit.");
        parameters.Dispose();
        mesh.Dispose();
        if (retained.Deflection() != 0.01 || retained.MinSize() != 0.001)
            throw new InvalidOperationException("Const-reference handle result did not survive both source owners.");

        using GeomCartesianPoint point = new(3, 4, 5);
        using StandardType identity = point.DynamicType()
            ?? throw new InvalidOperationException("Generated const-reference runtime type result is missing.");
        point.Dispose();
        if (!identity.IsKind("Standard_Type")) throw new InvalidOperationException("Retained dynamic type has invalid lifetime.");
    }
}
