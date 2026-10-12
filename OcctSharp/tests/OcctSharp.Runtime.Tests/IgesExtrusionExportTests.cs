namespace OcctSharp.Runtime.Tests;

public sealed class IgesExtrusionExportTests
{
    private static Shape Prism(string profile)
    {
        if(profile=="circle")
        {
            using var edge=ShapeFactory.CreateCircleEdge(new(0,0,0),new(0,0,1),3);using var boundary=ShapeFactory.CreateWire([edge]);
            using var disk=ShapeFactory.CreatePlanarFace(boundary);using var normal=GpVec.Create(0,0,4);return disk.Extrude(normal);
        }
        GpPoint[] points=profile switch
        {
            "rectangle"=>[new(0,0,0),new(20,0,0),new(20,12,0),new(0,12,0)],
            "triangle"=>[new(0,0,0),new(20,0,0),new(0,12,0)],
            _=>[new(0,0,0),new(20,0,0),new(20,12,0),new(13,12,0),new(13,5,0),new(7,5,0),new(7,12,0),new(0,12,0)]
        };
        using var wire=ShapeFactory.CreatePolygonWire(points,true);using var face=ShapeFactory.CreatePlanarFace(wire);using var vector=GpVec.Create(0,0,4);
        return face.Extrude(vector);
    }
    private static double Area(Shape shape)
    {
        var faces=shape.GetFaces();try{return faces.Sum(f=>Math.Abs(f.InspectProperties(InspectionPropertyKind.Area).Mass));}
        finally{foreach(var face in faces)face.Dispose();}
    }
    [Theory]
    [InlineData("rectangle",false)][InlineData("rectangle",true)]
    [InlineData("triangle",false)][InlineData("triangle",true)]
    [InlineData("slot",false)][InlineData("slot",true)]
    [InlineData("circle",false)][InlineData("circle",true)]
    public void EveryFiniteExtrudedSideSurvivesIgesWithoutChangingTheSource(string profile,bool rotated)
    {
        string path=Path.Combine(Path.GetTempPath(),"OcctSharp.IgesExtrusion."+Guid.NewGuid()+".iges");
        using var shape=Prism(profile);using var move=GpTrsf.Create(30,-20,10,1,2,3,rotated?.6:0);using var moved=shape.Transformed(move);
        using var document=XdeDocument.Create();XdeLabel part;
        try
        {
            using(var transaction=document.BeginTransaction("Source extrusion")){part=document.AddShape(moved,profile);part.Color=new(1,0,0);transaction.Commit();}
            using var before=RepairSnapshot.Create(moved);int sourceFaces=moved.FaceCount;
            document.WriteIges(path);using var reread=ShapeExchange.ReadIges(path);
            Assert.True(reread.IsValid);Assert.Equal(sourceFaces,reread.FaceCount);Assert.Equal(Area(moved),Area(reread),4);
            using var original=part.Shape;using var after=RepairSnapshot.Create(original);Assert.Equal(before.Fingerprint,after.Fingerprint);
            Assert.Equal(new XdeColor(1,0,0),part.Color);Assert.Single(document.GetFreeShapes());
        }
        finally{if(File.Exists(path))File.Delete(path);}
    }
    [Theory][InlineData(false,"slot")][InlineData(true,"slot")][InlineData(false,"circle")][InlineData(true,"circle")]
    public void ExactSubshapeColorsAndSharedPlacementsSurviveThePrivateExportCopy(bool located,string profile)
    {
        string path=Path.Combine(Path.GetTempPath(),"OcctSharp.IgesColor."+Guid.NewGuid()+".iges");
        using var document=XdeDocument.Create();using var shape=Prism(profile);XdeLabel part;string fingerprint;
        try
        {
            using(var edit=document.BeginTransaction("Shared colored extrusion"))
            {
                part=document.AddShape(shape,"Slot");part.Color=new(1,0,0);part.SetLayer("Production");
                using var repair=new RepairDocumentSession(document,part);
                var selection=repair.Source.Topology.First(t=>
                {
                    if(t.Kind!=ShapeKind.Face)return false;using var face=repair.Source.CopySubshape(t.Selection);
                    return face.GetFaceSurfaceSnapshot().SurfaceType==SurfaceGeometryType.SurfaceOfExtrusion;
                });
                repair.GetOrCreateSubshapeLabel(selection.Selection).Color=new(0,1,0);
                var assembly=document.AddAssembly("Shared slots");
                foreach(int i in new[]{0,1})
                {
                    using var transform=GpTrsf.Create(30*i,located?-20:0,located?10:0,1,2,3,located?.6:0);
                    using var location=TopLocLocation.FromTransform(transform);document.AddComponent(assembly,part,location);
                }
                edit.Commit();
            }
            using(var original=part.Shape)fingerprint=RepairSnapshot.ComputeFingerprint(original);
            document.WriteIges(path);using var reopened=XdeDocument.ReadIges(path);using var output=ShapeExchange.ReadIges(path);
            Assert.Equal(2*shape.FaceCount,output.FaceCount);Assert.Equal(2*Area(shape),Area(output),4);
            var styles=reopened.GetFreeShapes().SelectMany(label=>label.GetPresentationStyles()).ToArray();
            try
            {
                Assert.Contains(styles,s=>s.EffectiveColor is {} c&&c.Green>.99&&c.Red<.01);
                Assert.Contains(styles,s=>s.EffectiveColor is {} c&&c.Red>.99&&c.Green<.01);
            }
            finally{foreach(var style in styles)style.Dispose();}
            using var source=part.Shape;Assert.Equal(fingerprint,RepairSnapshot.ComputeFingerprint(source));Assert.Equal("Slot",part.Name);
            Assert.Contains("Production",part.Layers);
        }
        finally{if(File.Exists(path))File.Delete(path);}
    }
}
