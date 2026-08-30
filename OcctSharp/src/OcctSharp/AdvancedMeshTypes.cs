namespace OcctSharp;

/// <summary>Controls copied advanced triangulation and managed diagnostic construction.</summary>
public sealed record AdvancedMeshOptions
{
    /// <summary>Gets the absolute or relative linear deflection used by OCCT meshing.</summary>
    public double LinearDeflection { get; init; } = 0.1;
    /// <summary>Gets the angular deflection in radians.</summary>
    public double AngularDeflection { get; init; } = 0.5;
    /// <summary>Gets the minimum permitted mesh element size.</summary>
    public double MinimumSize { get; init; }
    /// <summary>Gets whether linear deflection is relative to local shape size.</summary>
    public bool Relative { get; init; }
    /// <summary>Gets whether OCCT may mesh faces in parallel.</summary>
    public bool Parallel { get; init; } = true;
    /// <summary>Gets whether internal face vertices may be inserted.</summary>
    public bool InternalVertices { get; init; } = true;
    /// <summary>Gets whether surface deflection is checked during meshing.</summary>
    public bool ControlSurfaceDeflection { get; init; } = true;
    /// <summary>Gets the tolerance used to weld copied positions for topology diagnostics.</summary>
    public double WeldTolerance { get; init; } = 1e-7;

    internal void Validate()
    {
        PositiveFinite(LinearDeflection, nameof(LinearDeflection));
        PositiveFinite(AngularDeflection, nameof(AngularDeflection));
        if (!double.IsFinite(MinimumSize) || MinimumSize < 0)
            throw new ArgumentOutOfRangeException(nameof(MinimumSize), "Minimum size must be finite and non-negative.");
        PositiveFinite(WeldTolerance, nameof(WeldTolerance));
    }

    private static void PositiveFinite(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(name, "The value must be finite and greater than zero.");
    }
}

/// <summary>One immutable source-face primitive group.</summary>
public sealed record AdvancedMeshPrimitiveGroup(
    int FaceIndex,
    int FirstTriangle,
    int TriangleCount,
    XdeColor? Color,
    XdeVisualMaterial? VisualMaterial);

/// <summary>Copied aggregate size and geometry statistics for one mesh.</summary>
public sealed record AdvancedMeshStatistics(
    BoundingBox3d Bounds,
    int VertexCount,
    int UniqueVertexCount,
    int TriangleCount,
    int FaceGroupCount,
    double SurfaceArea,
    long EstimatedBytes);

/// <summary>Copied triangle-topology diagnostics after tolerance-based vertex welding.</summary>
public sealed record AdvancedMeshDiagnostics(
    int DegenerateTriangleCount,
    int BoundaryEdgeCount,
    int ManifoldEdgeCount,
    int NonManifoldEdgeCount,
    int ConnectedComponentCount)
{
    /// <summary>Gets whether the copied triangle topology is closed and two-manifold.</summary>
    public bool IsClosedManifold => DegenerateTriangleCount == 0
        && BoundaryEdgeCount == 0
        && NonManifoldEdgeCount == 0;
}

/// <summary>Owns a complete managed copy of grouped OCCT triangulations and diagnostics.</summary>
public sealed class AdvancedMeshSnapshot
{
    internal AdvancedMeshSnapshot(
        DetailedMeshVertex[] vertices,
        DetailedMeshTriangle[] triangles,
        AdvancedMeshPrimitiveGroup[] groups,
        AdvancedMeshStatistics statistics,
        AdvancedMeshDiagnostics diagnostics)
    {
        Vertices = Array.AsReadOnly(vertices);
        Triangles = Array.AsReadOnly(triangles);
        Groups = Array.AsReadOnly(groups);
        Statistics = statistics;
        Diagnostics = diagnostics;
    }

    /// <summary>Gets copied positions, normals, and optional UV coordinates.</summary>
    public IReadOnlyList<DetailedMeshVertex> Vertices { get; }
    /// <summary>Gets copied indexed triangles with source-face grouping information.</summary>
    public IReadOnlyList<DetailedMeshTriangle> Triangles { get; }
    /// <summary>Gets contiguous primitive groups corresponding to source faces.</summary>
    public IReadOnlyList<AdvancedMeshPrimitiveGroup> Groups { get; }
    /// <summary>Gets copied aggregate mesh statistics.</summary>
    public AdvancedMeshStatistics Statistics { get; }
    /// <summary>Gets topology diagnostics computed over tolerance-welded positions.</summary>
    public AdvancedMeshDiagnostics Diagnostics { get; }
    /// <summary>Gets whether at least one copied vertex has UV coordinates.</summary>
    public bool HasUv => Vertices.Any(static vertex => vertex.HasUv);

    internal AdvancedMeshSnapshot WithStyle(XdeColor? color, XdeVisualMaterial? material)
    {
        AdvancedMeshPrimitiveGroup[] groups = Groups
            .Select(group => group with { Color = color, VisualMaterial = material })
            .ToArray();
        return new AdvancedMeshSnapshot(
            [.. Vertices], [.. Triangles], groups, Statistics, Diagnostics);
    }
}

/// <summary>One ordered copied level of detail.</summary>
public sealed record AdvancedMeshLod(int Level, double LinearDeflection, AdvancedMeshSnapshot Mesh);

/// <summary>Owns ordered fine-to-coarse mesh snapshots.</summary>
public sealed class AdvancedMeshLodSet
{
    internal AdvancedMeshLodSet(AdvancedMeshLod[] levels) => Levels = Array.AsReadOnly(levels);
    /// <summary>Gets the immutable fine-to-coarse LOD sequence.</summary>
    public IReadOnlyList<AdvancedMeshLod> Levels { get; }
}

/// <summary>Copied OCCT 3x4 affine transformation in row-major order.</summary>
public readonly record struct MeshTransform(
    double M11, double M12, double M13, double M14,
    double M21, double M22, double M23, double M24,
    double M31, double M32, double M33, double M34)
{
    /// <summary>Gets the identity transformation.</summary>
    public static MeshTransform Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0);
}

/// <summary>Defines the alpha behavior of an XDE visualization material.</summary>
public enum XdeAlphaMode
{
    /// <summary>Lets the provider infer blending from the base color alpha.</summary>
    BlendAuto = -1,
    /// <summary>Renders the material as fully opaque.</summary>
    Opaque = 0,
    /// <summary>Rejects fragments below the alpha cutoff.</summary>
    Mask = 1,
    /// <summary>Uses alpha blending.</summary>
    Blend = 2,
    /// <summary>Combines alpha masking and blending.</summary>
    MaskBlend = 3
}

/// <summary>Copied metallic-roughness XDE visualization material.</summary>
public sealed record XdeVisualMaterial(
    string Name,
    XdeColor BaseColor,
    double Metallic,
    double Roughness,
    GpXyz EmissiveFactor,
    double RefractionIndex = 1.5,
    XdeAlphaMode AlphaMode = XdeAlphaMode.BlendAuto,
    double AlphaCutoff = 0.5)
{
    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Name);
        BaseColor.Validate();
        Unit(Metallic, nameof(Metallic));
        Unit(Roughness, nameof(Roughness));
        Unit(EmissiveFactor.X, nameof(EmissiveFactor));
        Unit(EmissiveFactor.Y, nameof(EmissiveFactor));
        Unit(EmissiveFactor.Z, nameof(EmissiveFactor));
        Unit(AlphaCutoff, nameof(AlphaCutoff));
        if (!double.IsFinite(RefractionIndex) || RefractionIndex is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(RefractionIndex));
        if (!Enum.IsDefined(AlphaMode)) throw new ArgumentOutOfRangeException(nameof(AlphaMode));
    }

    private static void Unit(double value, string name)
    {
        if (!double.IsFinite(value) || value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(name, "The value must be in [0,1].");
    }
}

/// <summary>One deduplicated mesh definition in a copied scene.</summary>
public sealed record MeshSceneDefinition(int Index, string Key, AdvancedMeshSnapshot Mesh);

/// <summary>One copied root, assembly, part, or occurrence node.</summary>
public sealed record MeshSceneNode(
    int Index,
    int ParentIndex,
    string Entry,
    string DefinitionEntry,
    string Name,
    bool IsAssembly,
    int MeshDefinitionIndex,
    MeshTransform LocalTransform,
    MeshTransform WorldTransform,
    IReadOnlyList<string> Path,
    IReadOnlyList<string> Layers,
    XdeColor? Color,
    XdeVisualMaterial? VisualMaterial,
    XdeMaterial? PhysicalMaterial);
