using System.Runtime.InteropServices;
using OcctSharp;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class PlaneMirrorTests
{
    [Fact]
    public void StyledMirroredCompoundStepRoundtripDoesNotDuplicateRecoveryShapes()
    {
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        try
        {
            using var box=ShapeFactory.CreateBox(2,3,4);using var mirror=GpTrsf.CreatePlaneMirror(5,0,0,1,0,0);
            using var reflected=box.Transformed(mirror);using var compound=ShapeFactory.CreateCompound([box,reflected]);
            using(var document=XdeDocument.Create())
            {
                using(var edit=document.BeginTransaction("Styled compound"))
                {
                    var root=document.AddAssembly("Assembly");var body=document.AddShape(compound,"Compound");body.Color=new XdeColor(.2,.4,.8);
                    using var identity=TopLocLocation.Identity;document.AddComponent(root,body,identity);edit.Commit();
                }
                document.WriteStep(path);
            }
            using(var exact=ShapeExchange.ReadStep(path))Assert.Equal(48,exact.InspectProperties(InspectionPropertyKind.Volume).Mass,7);
            double Volume(XdeLabel label)
            {if(label.IsAssembly)return label.GetComponents().Sum(c=>Volume(c.ReferredShape));using var shape=label.Shape;return shape.InspectProperties(InspectionPropertyKind.Volume).Mass;}
            foreach(bool readColors in new[]{false,true})
            {
                using var loaded=XdeDocument.ReadStep(path,new XdeStepReadOptions(ReadColors:readColors));
                Assert.Single(loaded.GetFreeShapes());Assert.Equal(48,loaded.GetFreeShapes().Sum(Volume),7);
                var root=loaded.GetFreeShapes().Single();Assert.Single(root.GetComponents());
                if(readColors)
                {
                    var styles=root.GetComponents().Single().ReferredShape.GetPresentationStyles();
                    try
                    {
                        Assert.NotEmpty(styles);
                        Assert.All(styles,style=>{Assert.NotNull(style.EffectiveColor);Assert.Equal(.2,style.EffectiveColor!.Value.Red,6);Assert.Equal(.4,style.EffectiveColor.Value.Green,6);Assert.Equal(.8,style.EffectiveColor.Value.Blue,6);});
                        Assert.Equal(48,styles.Sum(s=>s.Shape.InspectProperties(InspectionPropertyKind.Volume).Mass),7);
                    }
                    finally{foreach(var style in styles)style.Dispose();}
                }
            }
        }
        finally{File.Delete(path);}
    }
    [Theory][InlineData(0,0,0,1,0,0)][InlineData(7,2,3,1,2,3)]
    public void ExactPlaneReflectionPreservesSolidAndInverseWhileOwningIndependentResults(double x,double y,double z,double nx,double ny,double nz)
    {
        using var primitive=ShapeFactory.CreateBox(4,3,2);using var source=primitive.Transformed(ShapeTransform.CreateTranslation(1,2,3));
        var mirror=GpTrsf.CreatePlaneMirror(x,y,z,nx,ny,nz);using var clone=mirror.Clone();using var reflected=source.Transformed(mirror);mirror.Dispose();
        Assert.True(reflected.GetTopologySummary().IsValid);Assert.Equal(1,reflected.GetTopologySummary().UniqueCounts.SolidCount);
        Assert.Equal(24,reflected.InspectProperties(InspectionPropertyKind.Volume).Mass,8);
        var matrix=new double[3,3];for(int row=0;row<3;row++)for(int column=0;column<3;column++)matrix[row,column]=clone.Value(row+1,column+1);
        double determinant=matrix[0,0]*(matrix[1,1]*matrix[2,2]-matrix[1,2]*matrix[2,1])-matrix[0,1]*(matrix[1,0]*matrix[2,2]-matrix[1,2]*matrix[2,0])+matrix[0,2]*(matrix[1,0]*matrix[2,1]-matrix[1,1]*matrix[2,0]);Assert.Equal(-1,determinant,10);
        var props=reflected.InspectProperties(InspectionPropertyKind.Volume);var normal=new[]{nx,ny,nz};double length=Math.Sqrt(nx*nx+ny*ny+nz*nz);normal=normal.Select(v=>v/length).ToArray();
        var original=new[]{3d,3.5,4};var origin=new[]{x,y,z};double distance=Enumerable.Range(0,3).Sum(i=>(original[i]-origin[i])*normal[i]);
        Assert.Equal(original[0]-2*distance*normal[0],props.CenterOfMass.X,8);Assert.Equal(original[1]-2*distance*normal[1],props.CenterOfMass.Y,8);Assert.Equal(original[2]-2*distance*normal[2],props.CenterOfMass.Z,8);
        using var restored=reflected.Transformed(clone);var expected=source.GetBoundingBox();var actual=restored.GetBoundingBox();
        Assert.Equal(expected.Minimum.X,actual.Minimum.X,7);Assert.Equal(expected.Minimum.Y,actual.Minimum.Y,7);Assert.Equal(expected.Minimum.Z,actual.Minimum.Z,7);
        Assert.Equal(expected.Maximum.X,actual.Maximum.X,7);Assert.Equal(expected.Maximum.Y,actual.Maximum.Y,7);Assert.Equal(expected.Maximum.Z,actual.Maximum.Z,7);
        Assert.Throws<ObjectDisposedException>(()=>source.Transformed(mirror));
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".brep");
        try{ShapeExchange.WriteBrep(reflected,path);using var loaded=ShapeExchange.ReadBrep(path);Assert.Equal(24,loaded.InspectProperties(InspectionPropertyKind.Volume).Mass,8);Assert.True(loaded.GetTopologySummary().IsValid);}
        finally{File.Delete(path);}
    }
    [Fact] public void InvalidPlaneInputsAndRepeatedDisposalRemainSafe()
    {
        Assert.Throws<ArgumentException>(()=>GpTrsf.CreatePlaneMirror(0,0,0,0,0,0));
        Assert.Throws<ArgumentException>(()=>GpTrsf.CreatePlaneMirror(double.NaN,0,0,1,0,0));
        Assert.Throws<ArgumentException>(()=>GpTrsf.CreatePlaneMirror(0,0,0,double.PositiveInfinity,0,0));
        for(int i=0;i<64;i++){var mirror=GpTrsf.CreatePlaneMirror(i,0,0,2,0,0);mirror.Dispose();mirror.Dispose();}
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private unsafe delegate int MirrorExport(double x,double y,double z,double nx,double ny,double nz,nint* output);
    [Fact] public unsafe void NativeAbiRejectsNullOutputAndClearsFailureOutput()
    {
        _=OcctRuntime.Info;var library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        try
        {
            var create=Marshal.GetDelegateForFunctionPointer<MirrorExport>(NativeLibrary.GetExport(library,"occtsharp_trsf_create_plane_mirror"));
            Assert.Equal(1,create(0,0,0,1,0,0,null));nint output=123;Assert.Equal(1,create(0,0,0,0,0,0,&output));Assert.Equal(0,output);
            output=123;Assert.Equal(1,create(double.NaN,0,0,1,0,0,&output));Assert.Equal(0,output);
        }
        finally{NativeLibrary.Free(library);}
    }
}
