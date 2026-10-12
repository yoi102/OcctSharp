using System.Security.Cryptography;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ExactOffsetXdeRoundTripTests
{
    [Theory]
    [InlineData("arc-g1",false)][InlineData("arc-g1",true)]
    [InlineData("spline-g1",false)][InlineData("spline-g1",true)]
    [InlineData("spline-g2",false)][InlineData("spline-g2",true)]
    public void LocatedRootOrSharedDefinitionPreservesPmiSourceStateAndStrictGeometry(string kind,bool assembly)
    {
        string output=Path.Combine(Path.GetTempPath(),"OcctSharpExactXde",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(output);
        using var source=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","b18-periodic-distance",kind+"-normal-outward-Round.brep"));
        double volume=Mass(source);using var document=XdeDocument.Create();XdeLabel part,root;XdeDimension dimension;
        using(var transaction=document.BeginTransaction("author"))
        {
            part=document.AddPart(source,new XdePartMetadata("Exact definition",new XdeColor(.2,.4,.7),["Manufacturing"],new XdeMaterial("Steel","Authored material",7.85,"density","g/cm3")));
            root=part;part.IntegerValue=17;
            if(assembly)
            {
                root=document.AddAssembly("Shared exact assembly");using var identity=TopLocLocation.Identity;using var transform=GpTrsf.Create(50,10,0);using var moved=TopLocLocation.FromTransform(transform);
                document.AddComponent(root,part,identity);document.AddComponent(root,part,moved);
            }
            dimension=document.CreateDimension(new XdeDimensionDefinition(XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Size_CurveLength,[12.3]){SemanticName="Authored exact length"},[part]);transaction.Commit();
        }
        using(var transaction=document.BeginTransaction("metadata edit")){part.IntegerValue=23;transaction.Commit();}
        document.MarkSaved();var beforeState=document.HistoryState;using var beforeShape=root.Shape;byte[] before=Bytes(beforeShape,"before");var color=part.Color;var material=part.Material;var layers=part.Layers.ToArray();
        string step=document.WriteStep(Path.Combine(output,"exact.step"),new XdeStepWriteOptions());
        Assert.Equal(beforeState,document.HistoryState);Assert.Equal("Exact definition",part.Name);Assert.Equal(23,part.IntegerValue);Assert.Equal(color,part.Color);Assert.Equal(material,part.Material);Assert.Equal(layers,part.Layers);
        Assert.Equal(12.3,dimension.GetSnapshot().Definition.Values.Single());using var afterShape=root.Shape;Assert.Equal(before,Bytes(afterShape,"after"));
        foreach(bool readColors in new[]{true,false})
        {
            using var imported=XdeDocument.ReadStep(step,new XdeStepReadOptions(ReadColors:readColors));var rootLabel=Assert.Single(imported.GetFreeShapes());using var shape=rootLabel.Shape;
            Assert.True(shape.GetValidationReport(true,true).IsValid);Assert.Equal(assembly?2:1,shape.GetTopologySummary().UniqueCounts.SolidCount);
            Assert.InRange(Math.Abs(Mass(shape)-volume*(assembly?2:1)),0,1e-7);
            Assert.Equal(12.3,Assert.Single(imported.GetDimensions()).GetSnapshot().Definition.Values.Single());
        }
        Assert.True(document.Undo());Assert.Equal(17,part.IntegerValue);Assert.True(document.Redo());Assert.Equal(23,part.IntegerValue);
        Assert.Equal(before,Bytes(afterShape,"after-history"));
        byte[] Bytes(Shape shape,string name)=>SHA256.HashData(File.ReadAllBytes(ShapeExchange.WriteBrep(shape,Path.Combine(output,name+".brep"))));
    }
    private static double Mass(Shape shape)=>Math.Abs(shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-11).Properties.Mass);
}
