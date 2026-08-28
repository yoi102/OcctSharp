using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class OcctProductModuleTests
{
    [Theory]
    [InlineData("Standard", "TKernel", OcctProductModule.Foundation)]
    [InlineData("gp", "TKMath", OcctProductModule.Geometry)]
    [InlineData("GeomAPI", "TKGeomAlgo", OcctProductModule.Geometry)]
    [InlineData("TopoDS", "TKBRep", OcctProductModule.Modeling)]
    [InlineData("ShapeFix", "TKShHealing", OcctProductModule.Modeling)]
    [InlineData("BRepMeshData", "TKMesh", OcctProductModule.Mesh)]
    [InlineData("TDocStd", "TKCDF", OcctProductModule.Documents)]
    [InlineData("AppStdL", null, OcctProductModule.Documents)]
    [InlineData("BinObjMgt", null, OcctProductModule.Documents)]
    [InlineData("StepBasic", "TKDESTEP", OcctProductModule.DataExchange)]
    [InlineData("IFGraph", null, OcctProductModule.DataExchange)]
    [InlineData("StlAPI", null, OcctProductModule.DataExchange)]
    [InlineData("XCAFDoc", "TKXCAF", OcctProductModule.Xde)]
    [InlineData("AIS", "TKV3d", OcctProductModule.Visualization)]
    [InlineData("IVtkTools", "TKIVtk", OcctProductModule.IVtk)]
    [InlineData("OpenGles", "TKOpenGles", OcctProductModule.OpenGles)]
    [InlineData("ViewerTest", "TKViewerTest", OcctProductModule.Draw)]
    [InlineData("MeshTest", null, OcctProductModule.Draw)]
    [InlineData("AdvApprox", null, OcctProductModule.Geometry)]
    [InlineData("Sweep", null, OcctProductModule.Modeling)]
    [InlineData("TColStd", null, OcctProductModule.Foundation)]
    public void ClassifiesRepresentativeOcctPackages(
        string package,
        string? toolkit,
        OcctProductModule expected)
    {
        Assert.Equal(expected, OcctProductModuleClassifier.ClassifyOrThrow(package, toolkit));
    }

    [Fact]
    public void LocksAnAcyclicManagedDependencyDirection()
    {
        Assert.True(OcctProductModuleGraph.CanReference(OcctProductModule.Xde, OcctProductModule.Documents));
        Assert.True(OcctProductModuleGraph.CanReference(OcctProductModule.Xde, OcctProductModule.Geometry));
        Assert.True(OcctProductModuleGraph.CanReference(OcctProductModule.Visualization, OcctProductModule.Mesh));
        Assert.False(OcctProductModuleGraph.CanReference(OcctProductModule.Modeling, OcctProductModule.Visualization));
        Assert.False(OcctProductModuleGraph.CanReference(OcctProductModule.Documents, OcctProductModule.DataExchange));

        foreach (OcctProductModule module in Enum.GetValues<OcctProductModule>()
            .Where(static module => module != OcctProductModule.Unassigned))
        {
            Assert.DoesNotContain(module, OcctProductModuleGraph.GetDependencyClosure(module));
        }
    }
}
