namespace OcctSharp.Runtime.Tests;

public sealed class TransformedDefinitionCopyTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void ExactReflectedFaceColorsAndLayersSurviveSourceDisposalAndStepRoundtrip(bool sameDocument)
    {
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        using var destination=XdeDocument.Create();XdeLabel copy;
        try
        {
            using(var source=sameDocument?null:XdeDocument.Create())
            {
                var owner=source??destination;XdeLabel part;
                using(var command=owner.BeginTransaction("Colored source"))
                using(var box=ShapeFactory.CreateBox(4,3,2))
                {
                    part=owner.AddShape(box,"Source");part.Color=new(.2,.4,.8);part.SetLayer("Manufacturing");
                    part.Material=new("Steel","Copied physical material",7.85e-6,"g/mm3","mass density");
                    part.VisualMaterial=new("Finish",new(.2,.4,.8),.4,.6,new(0,0,0));
                    part.ValidationProperties=new(52,24,new(2,1.5,1));
                    part.Comment="Authoring data is not transformed";
                    using var repair=new RepairDocumentSession(owner,part);
                    foreach(var face in repair.Source.Topology.Where(t=>t.Kind==ShapeKind.Face))
                    {
                        using var shape=repair.Source.CopySubshape(face.Selection);var bounds=shape.GetBoundingBox();
                        if(Math.Abs(bounds.Maximum.Z-bounds.Minimum.Z)<1e-6&&Math.Abs(bounds.Maximum.Z-2)<1e-6)
                            repair.GetOrCreateSubshapeLabel(face.Selection).Color=new(1,0,0,.65);
                    }
                    command.Commit();
                }
                using var mirror=GpTrsf.CreatePlaneMirror(0,0,0,0,0,1);
                using(var command=destination.BeginTransaction("Exact reflected definition"))
                {copy=destination.CopyTransformedDefinition(part,mirror,"Reflected");command.Commit();}
                using var original=part.Shape;Assert.True(original.GetBoundingBox().Maximum.Z>1.99);
                Assert.Contains("Manufacturing",copy.Layers);
                Assert.Equal(part.Material,copy.Material);
                Assert.Equal(part.VisualMaterial,copy.VisualMaterial);
                Assert.False(copy.ValidationProperties.HasAny);Assert.Null(copy.Comment);
                if(sameDocument)
                {
                    using var command=destination.BeginTransaction("Remove independent source");destination.RemoveDefinition(part);command.Commit();
                }
            }
            Check(copy,true);
            destination.WriteStep(path);
            using var reopened=XdeDocument.ReadStep(path);Check(reopened.GetFreeShapes().Single(),false);
        }
        finally{if(File.Exists(path))File.Delete(path);}
    }

    [Fact] public void LocatedSourceFaceMetadataIsNormalizedBeforeExactReflection()
    {
        using var source=XdeDocument.Create();using var destination=XdeDocument.Create();XdeLabel part,copy;
        using(var edit=source.BeginTransaction("Located source"))using(var box=ShapeFactory.CreateBox(4,3,2))
        using(var placement=GpTrsf.Create(7,-3,9,0,0,1,0))using(var location=TopLocLocation.FromTransform(placement))
        using(var located=box.Located(location))
        {
            part=source.AddShape(box,"Source");source.UpdateDefinitionShape(part,located);part.Color=new(.2,.4,.8);
            using var repair=new RepairDocumentSession(source,part);
            var red=Assert.Single(repair.Source.Topology,t=>
            {
                if(t.Kind!=ShapeKind.Face)return false;
                using var face=repair.Source.CopySubshape(t.Selection);var b=face.GetBoundingBox();
                return Math.Abs(b.Maximum.Z-b.Minimum.Z)<1e-6&&Math.Abs(b.Maximum.Z-11)<1e-6;
            });
            repair.GetOrCreateSubshapeLabel(red.Selection).Color=new(1,0,0,.65);edit.Commit();
        }
        using var stored=part.Location;using var transform=stored.ToTransform();using var inverse=transform.Inverted();
        using var mirror=GpTrsf.CreatePlaneMirror(0,0,0,0,0,1);using var localMirror=mirror.Multiplied(inverse);
        using(var edit=destination.BeginTransaction("Normalize and reflect")){copy=destination.CopyTransformedDefinition(part,localMirror);edit.Commit();}
        Check(copy,true);
        using var original=part.Shape;Assert.Equal(9,original.GetBoundingBox().Minimum.Z,6);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public void IdentityAndRigidCopiesHaveIndependentTopologyAndLeaveSourceMetadataUnchanged(bool translated)
    {
        using var document=XdeDocument.Create();XdeLabel source,copy;
        using(var edit=document.BeginTransaction("Source"))using(var box=ShapeFactory.CreateBox(4,3,2))
        {source=document.AddShape(box,"Original");source.Color=new(.1,.3,.7);source.SetLayer("Source layer");edit.Commit();}
        var originalColor=source.Color;
        using var transform=GpTrsf.Create(translated?7:0,translated?-3:0,translated?9:0,0,0,1,0);
        using(var edit=document.BeginTransaction("Independent copy"))
        {copy=document.CopyTransformedDefinition(source,transform,"Copy");copy.Color=new(.9,.2,.1);edit.Commit();}
        Assert.NotEqual(source.Entry,copy.Entry);Assert.Equal("Original",source.Name);Assert.Equal(originalColor,source.Color);
        using var original=source.Shape;using var copied=copy.Shape;
        Assert.Equal(0,original.GetBoundingBox().Minimum.Z,6);Assert.Equal(translated?9:0,copied.GetBoundingBox().Minimum.Z,6);
        using(var edit=document.BeginTransaction("Remove original")){document.RemoveDefinition(source);edit.Commit();}
        Assert.Equal(24,copied.InspectProperties(InspectionPropertyKind.Volume).Mass,8);Assert.Contains("Source layer",copy.Layers);
    }

    private static void Check(XdeLabel copy,bool alpha)
    {
        using var shape=copy.Shape;Assert.True(shape.IsValid);Assert.Equal(1,shape.GetTopologySummary().UniqueCounts.SolidCount);
        Assert.Equal(24,shape.InspectProperties(InspectionPropertyKind.Volume).Mass,8);
        var styles=copy.GetPresentationStyles();
        try
        {
            var red=Assert.Single(styles,s=>s.SurfaceColor is {} color&&color.Red>.99&&color.Green<.01);
            Assert.Equal(1,red.Shape.FaceCount);Assert.Equal(-2,red.Shape.InspectProperties(InspectionPropertyKind.Area).CenterOfMass.Z,7);
            if(alpha)Assert.Equal(.65,red.SurfaceColor!.Value.Alpha,6);
            Assert.Contains(styles,s=>s.EffectiveColor is {} color&&Math.Abs(color.Blue-.8)<1e-6);
        }
        finally{foreach(var style in styles)style.Dispose();}
    }

    [Fact] public void RejectedAssemblyAndMissingTransactionDoNotPublishAResult()
    {
        using var source=XdeDocument.Create();XdeLabel assembly;
        using(var command=source.BeginTransaction("Assembly"))
        using(var box=ShapeFactory.CreateBox(1,2,3))
        using(var location=TopLocLocation.Identity)
        {assembly=source.AddAssembly("Assembly");source.AddComponent(assembly,source.AddShape(box,"Part"),location);command.Commit();}
        using var destination=XdeDocument.Create();using var mirror=GpTrsf.CreatePlaneMirror(0,0,0,0,0,1);
        Assert.Throws<ArgumentException>(()=>destination.CopyTransformedDefinition(assembly,mirror));
        using(var command=destination.BeginTransaction("Rejected input"))Assert.Throws<ArgumentException>(()=>destination.CopyTransformedDefinition(assembly,mirror));
        Assert.Empty(destination.GetFreeShapes());
        mirror.Dispose();Assert.Throws<ObjectDisposedException>(()=>destination.CopyTransformedDefinition(assembly,mirror));
    }
}
