namespace OcctSharp.Runtime.Tests;

public sealed class GeneratedCommonModelingHandleTests
{
    [Fact]
    public void GeneratedMeshAndPolyTypesPreserveValuesAndSharing()
    {
        using BRepMeshIncrementalMesh mesh = new();
        using BRepMeshIncrementalMesh meshClone = mesh.Clone();
        Assert.Equal("BRepMesh_IncrementalMesh", mesh.TypeName);
        Assert.True(mesh.IsKind("BRepMesh_DiscretRoot"));
        Assert.Equal(2, mesh.ReferenceCount);
        Assert.Equal(2, meshClone.ReferenceCount);
        Assert.Equal(0, mesh.GetStatusFlags());
        Assert.False(mesh.IsModified());

        using PolyTriangulationParameters parameters = new(0.25, 0.5, 0.01);
        Assert.Equal("Poly_TriangulationParameters", parameters.TypeName);
        Assert.Equal(0.25, parameters.Deflection(), 12);
        Assert.Equal(0.5, parameters.Angle(), 12);
        Assert.Equal(0.01, parameters.MinSize(), 12);
        Assert.True(parameters.HasDeflection());
        Assert.True(parameters.HasAngle());
        Assert.True(parameters.HasMinSize());
    }

    [Fact]
    public void GeneratedAnalysisAndHealingToolsExposeSafeScalarState()
    {
        using ShapeAnalysisTransferParameters analysis = new();
        Assert.Equal("ShapeAnalysis_TransferParameters", analysis.TypeName);
        Assert.True(analysis.IsKind("Standard_Transient"));
        analysis.SetMaxTolerance(0.5);
        Assert.True(double.IsFinite(analysis.Perform(0.25, true)));

        using ShapeFixRoot fix = new();
        fix.SetPrecision(0.01);
        fix.SetMinTolerance(0.001);
        fix.SetMaxTolerance(0.1);
        Assert.Equal(0.01, fix.Precision(), 12);
        Assert.Equal(0.001, fix.MinTolerance(), 12);
        Assert.Equal(0.1, fix.MaxTolerance(), 12);
        Assert.Equal(0.1, fix.LimitTolerance(1), 12);

        using ShapeUpgradeTool upgrade = new();
        upgrade.SetPrecision(0.02);
        upgrade.SetMinTolerance(0.002);
        upgrade.SetMaxTolerance(0.2);
        Assert.Equal(0.02, upgrade.Precision(), 12);
        Assert.Equal(0.002, upgrade.MinTolerance(), 12);
        Assert.Equal(0.2, upgrade.MaxTolerance(), 12);
        Assert.Equal(0.0001, upgrade.LimitTolerance(0.0001), 12);
    }

    [Fact]
    public void GeneratedCommonModelingHandlesFailClosedAfterDispose()
    {
        ShapeFixRoot fix = new();
        fix.Dispose();
        fix.Dispose();
        Assert.Throws<ObjectDisposedException>(() => fix.Precision());

        ShapeUpgradeTool upgrade = new();
        upgrade.Dispose();
        Assert.Throws<ObjectDisposedException>(() => upgrade.LimitTolerance(1));
    }
}
