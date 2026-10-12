using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class PresentationMetadataCopyTests
{
    private static readonly XdeMaterial Material=new("Source alloy","Physical density",2.7,"g/cm3","mass-density");
    private static readonly string[] ExpectedLayers=["Cutting","Inspection"];
    [Theory][InlineData(false)][InlineData(true)]
    public void IndependentChannelsLayersAndMaterialsCopyWithoutGeometryOrNameAndSurviveSourceDisposal(bool subshape)
    {
        using var destination=XdeDocument.Create();using var targetShape=ShapeFactory.CreateBox(5,5,5);
        XdeLabel target;XdePresentationColors expected;
        using(var source=XdeDocument.Create())using(var sourceShape=ShapeFactory.CreateBox(2,3,4))
        {
            XdeLabel origin;
            using(var command=source.BeginTransaction("Direct presentation channels"))
            {
                var root=source.AddShape(sourceShape,"Source name");origin=root;
                if(subshape)
                {
                    using var repair=new RepairDocumentSession(source,root);
                    origin=repair.GetOrCreateSubshapeLabel(repair.Source.Topology.First(t=>t.Kind==ShapeKind.Face).Selection);
                }
                origin.SetPresentationColor(XdeColorChannel.General,new(1,0,0,.25));
                origin.SetPresentationColor(XdeColorChannel.Surface,new(0,1,0,.5));
                origin.SetPresentationColor(XdeColorChannel.Curve,new(0,0,1,.75));
                origin.AddLayer("Cutting");origin.AddLayer("Inspection");origin.Material=Material;
                origin.VisualMaterial=new("Coating",new(.1,.2,.3,.8),.4,.6,new(0,0,0));
                command.Commit();
            }
            expected=origin.GetPresentationColors();
            Assert.Equal(1,expected.General!.Value.Red);Assert.Equal(1,expected.Surface!.Value.Green);Assert.Equal(1,expected.Curve!.Value.Blue);
            using(var command=destination.BeginTransaction("Copy presentation only"))
            {
                var root=destination.AddShape(targetShape,"Destination name");target=root;
                if(subshape)
                {
                    using var repair=new RepairDocumentSession(destination,root);
                    target=repair.GetOrCreateSubshapeLabel(repair.Source.Topology.First(t=>t.Kind==ShapeKind.Face).Selection);
                    target.Name="Destination subshape";
                }
                destination.CopyPresentationMetadata(origin,target);command.Commit();
            }
            Assert.Equal(expected,target.GetPresentationColors());
            Assert.Equal(Material,target.Material);Assert.Equal(origin.VisualMaterial,target.VisualMaterial);
            Assert.Equal(origin.Layers.Order(),target.Layers.Order());
            Assert.Equal(subshape?"Destination subshape":"Destination name",target.Name);
            using var actual=destination.GetFreeShapes().Single().Shape;Assert.Equal(125,actual.InspectProperties(InspectionPropertyKind.Volume).Mass,6);
        }
        Assert.Equal(expected,target.GetPresentationColors());Assert.Equal(Material,target.Material);
        Assert.Equal(ExpectedLayers,target.Layers.Order());
    }

    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void ClearingOneChannelDoesNotChangeTheOthersAndAbortingRestoresAssignments(int selected)
    {
        using var document=XdeDocument.Create();using var shape=ShapeFactory.CreateBox(2,3,4);XdeLabel label;
        using(var command=document.BeginTransaction("Colors"))
        {
            label=document.AddShape(shape,"Source");label.SetPresentationColor(XdeColorChannel.General,new(1,0,0));
            label.SetPresentationColor(XdeColorChannel.Surface,new(0,1,0));label.SetPresentationColor(XdeColorChannel.Curve,new(0,0,1));command.Commit();
        }
        var before=label.GetPresentationColors();
        using(var command=document.BeginTransaction("Aborted single-channel change"))
        {
            label.SetPresentationColor((XdeColorChannel)selected,null);var after=label.GetPresentationColors();
            var channels=new[]{after.General,after.Surface,after.Curve};Assert.Null(channels[selected]);
            for(int i=0;i<3;i++)if(i!=selected)Assert.Equal(new[]{before.General,before.Surface,before.Curve}[i],channels[i]);
        }
        Assert.Equal(before,label.GetPresentationColors());
        Assert.Throws<ArgumentOutOfRangeException>(()=>label.SetPresentationColor((XdeColorChannel)99,null));
        Assert.Throws<ArgumentOutOfRangeException>(()=>label.SetPresentationColor(XdeColorChannel.Surface,new(double.NaN,0,0)));
    }

    [Theory][InlineData("color")][InlineData("layer")][InlineData("material")][InlineData("visual")][InlineData("hidden")]
    public void AssignedDestinationIsRejectedBeforeMetadataChanges(string kind)
    {
        using var document=XdeDocument.Create();using var shape=ShapeFactory.CreateBox(2,3,4);XdeLabel source,target;
        using(var command=document.BeginTransaction("Prepare labels"))
        {
            source=document.AddShape(shape,"Source");using var other=ShapeFactory.CreateBox(3,4,5);target=document.AddShape(other,"Destination");
            source.Color=new(1,0,0);source.AddLayer("Source layer");source.Material=Material;
            switch(kind)
            {
                case "color":target.SetPresentationColor(XdeColorChannel.Curve,new(0,0,1));break;
                case "layer":target.AddLayer("Keep layer");break;
                case "material":target.Material=new("Keep material","",1,"kg/m3","density");break;
                case "hidden":target.IsVisible=false;break;
                default:target.VisualMaterial=new("Keep coating",new(0,1,0),.1,.3,new(0,0,0));break;
            }
            command.Commit();
        }
        var colors=target.GetPresentationColors();var layers=target.Layers.ToArray();var material=target.Material;var visual=target.VisualMaterial;
        using(var command=document.BeginTransaction("Rejected copy"))
            Assert.Throws<ArgumentException>(()=>document.CopyPresentationMetadata(source,target));
        Assert.Equal(colors,target.GetPresentationColors());Assert.Equal(layers,target.Layers);Assert.Equal(material,target.Material);Assert.Equal(visual,target.VisualMaterial);
        Assert.Throws<ArgumentException>(()=>document.CopyPresentationMetadata(source,target));
    }

    [Fact]
    public void HiddenMetadataSurvivesNativeReopenWithoutCopyingValidationOrArbitraryText()
    {
        var path=Path.Combine(Path.GetTempPath(),"presentation-"+Guid.NewGuid().ToString("N")+".xbf");
        string childEntry;
        try
        {
            using(var source=XdeDocument.Create())using(var destination=XdeDocument.Create())
            using(var sourceShape=ShapeFactory.CreateBox(2,3,4))using(var targetShape=ShapeFactory.CreateBox(5,5,5))
            {
                XdeLabel origin,target;XdeLabel hiddenFace;
                using(var edit=source.BeginTransaction("Hidden source and invalid-for-target data"))
                {
                    origin=source.AddShape(sourceShape,"Source");origin.IsVisible=false;
                    origin.SetPresentationColor(XdeColorChannel.Surface,new(.25,.5,.75,.5));
                    origin.AddLayer("Hidden layer");origin.Material=Material;
                    origin.UpdateValidationPropertiesFromShape();origin.Comment="Do not copy unrelated text";
                    using var repair=new RepairDocumentSession(source,origin);
                    hiddenFace=repair.GetOrCreateSubshapeLabel(repair.Source.Topology.First(t=>t.Kind==ShapeKind.Face).Selection);
                    hiddenFace.IsVisible=false;hiddenFace.SetPresentationColor(XdeColorChannel.Curve,new(1,0,0));
                    edit.Commit();
                }
                using(var edit=destination.BeginTransaction("Fresh hidden destination"))
                {
                    target=destination.AddShape(targetShape,"Preserve target name");target.Comment="Preserve target comment";
                    destination.CopyPresentationMetadata(origin,target);
                    using var repair=new RepairDocumentSession(destination,target);
                    var targetFace=repair.GetOrCreateSubshapeLabel(repair.Source.Topology.First(t=>t.Kind==ShapeKind.Face).Selection);
                    childEntry=targetFace.Entry;destination.CopyPresentationMetadata(hiddenFace,targetFace);
                    edit.Commit();
                }
                Assert.False(target.IsVisible);Assert.False(target.ValidationProperties.HasAny);
                Assert.Equal("Preserve target comment",target.Comment);
                var before=target.GetPresentationColors();
                using(var edit=destination.BeginTransaction("Aborted visibility change")){target.IsVisible=true;Assert.True(target.IsVisible);}
                Assert.Equal(before,target.GetPresentationColors());
                Assert.Throws<ArgumentException>(()=>target.IsVisible=true);
                destination.Save(path);
            }
            using var reopened=XdeDocument.Open(path);var root=reopened.GetFreeShapes().Single();
            Assert.False(root.IsVisible);Assert.Equal("Preserve target name",root.Name);
            Assert.Equal("Preserve target comment",root.Comment);Assert.False(root.ValidationProperties.HasAny);
            Assert.Equal(Material,root.Material);Assert.Equal("Hidden layer",Assert.Single(root.Layers));
            Assert.Equal(.5,root.GetPresentationColors().Surface!.Value.Alpha,6);
            var child=reopened.GetLabel(childEntry);Assert.False(child.IsVisible);
            Assert.Equal(1,child.GetPresentationColors().Curve!.Value.Red);
            using var shape=root.Shape;Assert.Equal(125,shape.InspectProperties(InspectionPropertyKind.Volume).Mass,6);
        }
        finally{if(File.Exists(path))File.Delete(path);}
    }

    [Fact]
    public void ForeignDestinationDisposedSourceAndSameLabelCannotPublish()
    {
        using var destination=XdeDocument.Create();using var foreign=XdeDocument.Create();using var shape=ShapeFactory.CreateBox(2,3,4);
        var sourceDocument=XdeDocument.Create();XdeLabel source,target,other;
        using(var edit=sourceDocument.BeginTransaction("Source")){source=sourceDocument.AddShape(shape,"Source");source.Color=new(1,0,0);edit.Commit();}
        using(var edit=destination.BeginTransaction("Destination")){target=destination.AddShape(shape,"Destination");edit.Commit();}
        using(var edit=foreign.BeginTransaction("Foreign")){other=foreign.AddShape(shape,"Foreign");edit.Commit();}
        Assert.Throws<ArgumentException>(()=>destination.CopyPresentationMetadata(source,other));
        using(var edit=destination.BeginTransaction("Same label"))Assert.Throws<ArgumentException>(()=>destination.CopyPresentationMetadata(target,target));
        sourceDocument.Dispose();using(var edit=destination.BeginTransaction("Disposed source"))
            Assert.Throws<ObjectDisposedException>(()=>destination.CopyPresentationMetadata(source,target));
        Assert.Null(target.GetPresentationColors().General);
    }
}
