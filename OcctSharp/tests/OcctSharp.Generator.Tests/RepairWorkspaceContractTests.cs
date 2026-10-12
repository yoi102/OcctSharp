using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;
namespace OcctSharp.Generator.Tests;

public sealed class RepairWorkspaceContractTests
{
    static BindingDeclaration Declaration(CopiedCollectionOperationContract.Operation op) => new(
        op.StableId, op.NativeName, BindingDeclarationKind.Method,
        op.NativeName.Split("::")[0] + ".hxx", 1, 1)
    { NativeSignature = op.Signature, Access = BindingAccess.Public, ProductModule = OcctProductModule.Modeling };

    [Fact]
    public void DriftDeletedPrivateAndUnapprovedManualCandidatesAreRejected()
    {
        var d = Declaration(RepairWorkspaceOperationContract.Operations.Values.First(o => !o.Supporting));
        Assert.True(RepairWorkspaceOperationContract.Contains(d));
        Assert.False(RepairWorkspaceOperationContract.Contains(d with { NativeSignature = d.NativeSignature + " " }));
        Assert.False(RepairWorkspaceOperationContract.Contains(d with { Access = BindingAccess.Private }));
        Assert.False(RepairWorkspaceOperationContract.Contains(d with { IsDeleted = true }));
        Assert.False(RepairWorkspaceOperationContract.Contains(d with { SupportState = BindingSupportState.Manual }));
    }

    [Fact]
    public void MutableAliasesMissingWireConstructorAndCompositeSurfacesStayExcluded()
    {
        Assert.DoesNotContain(RepairWorkspaceOperationContract.Operations.Values, o => o.NativeName.StartsWith("ShapeFix_ComposeShell::", StringComparison.Ordinal));
        Assert.DoesNotContain(RepairWorkspaceOperationContract.Operations.Values, o => o.Signature.Contains("const ShapeFix_WireSegment &", StringComparison.Ordinal));
        Assert.DoesNotContain(RepairWorkspaceOperationContract.Operations.Values, o => o.Signature.Contains("const ShapeExtend_WireData &", StringComparison.Ordinal));
        Assert.DoesNotContain(RepairWorkspaceOperationContract.Operations.Values, o => o.NativeName == "ShapeFix_WireSegment::ShapeFix_WireSegment" && o.Signature.Contains("const TopoDS_Wire &", StringComparison.Ordinal));
    }

    [Fact]
    public void PrivateMapProjectionDoesNotOpenGlobalOrUnapprovedSignatures()
    {
        var t = new BindingType("NCollection_DataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher>",
            "NCollection_DataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher>",
            "NCollection_DataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher>",
            "NCollection_DataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher>",
            [new(BindingTypeLayerKind.Value, false)], null, [], false, null);
        var map = InitialTypeMap.FromModel(new BindingModel([]));
        var d = Declaration(RepairWorkspaceOperationContract.Operations.Values.First(o => o.NativeName == "ShapeFix_Wireframe::CheckSmallEdges"));
        Assert.False(map.TryMap(t, BindingTypeUsage.Parameter, out _));
        Assert.True(RepairWorkspaceOperationContract.TryMap(d, t, BindingTypeUsage.Parameter, map, out var projection));
        Assert.Equal("TM016", projection!.RuleId);
        Assert.False(RepairWorkspaceOperationContract.TryMap(d with { StableId = "unapproved" }, t, BindingTypeUsage.Parameter, map, out _));
    }

    [Fact]
    public void PartialCatalogueCannotPublishAnInflatedMigrationManifest()
    {
        var d = Declaration(RepairWorkspaceOperationContract.Operations.Values.First());
        Assert.Throws<InvalidDataException>(() => RepairWorkspaceOperationEmitter.Emit("8.0.1", new BindingModel([d])));
    }

    [Fact]
    public void FullCatalogueEmitsDeterministicallyWithCoupledPublicationAndNoBorrowedPublicMaps()
    {
        var model = new BindingModel(RepairWorkspaceOperationContract.Operations.Values.Select(Declaration));
        var a = RepairWorkspaceOperationEmitter.Emit("8.0.1", model);
        var b = RepairWorkspaceOperationEmitter.Emit("8.0.1", model);
        Assert.Equal(10, a.Files.Count);
        Assert.Equal(a.Files.Select(f => f.Content), b.Files.Select(f => f.Content));
        Assert.Equal(RepairWorkspaceOperationContract.Operations.Count, a.SourceStableIds.Count);
        Assert.All(a.Files, f => Assert.Equal(OcctProductModule.Modeling, f.ProductModule));
        string sessions = a.Files.Single(f => f.RelativePath.EndsWith("Repairsessions.Generated.cpp", StringComparison.Ordinal)).Content;
        Assert.Contains("workspace->parent.get()==session.get() && workspace->epoch==session->epoch", sessions);
        Assert.Contains("DetachWorkspace(*workspace)", sessions);
    }
}
