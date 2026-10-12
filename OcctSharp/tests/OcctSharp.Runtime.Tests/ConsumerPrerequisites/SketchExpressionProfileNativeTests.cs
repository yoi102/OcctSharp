using System.Runtime.InteropServices;
using System.Security.Cryptography;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class SketchExpressionProfileNativeTests
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int ProfileRaw(nint outer,nint* holes,int count,SketchPlaneRaw* plane,double tolerance,nint* output);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int ContainsRaw(nint wire,SketchPoint2dRaw point,double tolerance,int* output);

    [Fact]
    public unsafe void NativeProfileRejectsAliasesAndWrongPlaneWithoutChangingCallerTopologyOrOutputs()
    {
        var loop=SketchExpressionChain2d.CreateOrdered([
            SketchCurveExpression2d.FromCurve(SketchCurve2d.Ellipse(default,8,3)).Offset(.2)],true);
        using var wire=loop.BuildWire(SketchPlane.XY);
        _=OcctRuntime.Info;
        nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        string directory=Path.Combine(Path.GetTempPath(),"b18-profile-native-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var create=Marshal.GetDelegateForFunctionPointer<ProfileRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_profile_face"));
            var contains=Marshal.GetDelegateForFunctionPointer<ContainsRaw>(NativeLibrary.GetExport(library,"occtsharp_sketch_expression_wire_contains"));
            nint handle=wire.Handle.DangerousGetHandle();
            var plane=SketchPlane.XY.ToRaw();var beforePlane=plane;
            string first=ShapeExchange.WriteBrep(wire,Path.Combine(directory,"before.brep"));
            byte[] before=SHA256.HashData(File.ReadAllBytes(first));
            Assert.Equal(1,create(handle,null,0,&plane,1e-7,(nint*)&plane));
            Assert.Equal(beforePlane,plane);
            Assert.Equal(1,create(handle,null,0,&plane,1e-7,(nint*)handle));
            Assert.Equal(1,contains(handle,new(0,0),1e-7,(int*)handle));
            nint output=123;
            var wrong=new SketchPlane(new(0,0,10),new(1,0,0),new(0,1,0)).ToRaw();
            Assert.Equal(1,create(handle,null,0,&wrong,1e-7,&output));
            Assert.Equal((nint)123,output);
            nint* aliasedHoles=stackalloc nint[]{handle};
            Assert.Equal(1,create(handle,aliasedHoles,1,&plane,1e-7,aliasedHoles));
            Assert.Equal(handle,aliasedHoles[0]);
            Assert.Equal(0,create(handle,null,0,&plane,1e-7,&output));
            using(var face=ShapeFactory.FromNativeHandle(output,"profile_native_test"))Assert.True(face.IsValid);
            int inside=73;
            Assert.Equal(0,contains(handle,new(0,0),1e-7,&inside));Assert.Equal(1,inside);
            Assert.Equal(0,contains(handle,new(20,0),1e-7,&inside));Assert.Equal(0,inside);
            Assert.Equal(1,contains(handle,new(double.NaN,0),1e-7,&inside));Assert.Equal(0,inside);
            string after=ShapeExchange.WriteBrep(wire,Path.Combine(directory,"after.brep"));
            Assert.Equal(before,SHA256.HashData(File.ReadAllBytes(after)));
            Assert.True(wire.IsValid);
        }
        finally{NativeLibrary.Free(library);Directory.Delete(directory,true);}
    }

    [Fact]
    public void RegionUsesItsOwnToleranceAndResourceBoundRatherThanAnEarlierLooseClosure()
    {
        var open=SketchExpressionChain2d.CreateOrdered([
            Line(new(0,0),new(4,0)),Line(new(4,0),new(4,4)),
            Line(new(4,4),new(0,4)),Line(new(0,4),new(0,.001))],true,.01);
        Assert.True(open.IsClosed);
        var error=Assert.Throws<SketchValidationException>(()=>SketchExpressionRegion2d.Classify([open],1e-7));
        Assert.Contains(error.Diagnostics,value=>value.Code==SketchDiagnosticCode.OpenChain);
        Assert.Throws<ArgumentException>(()=>SketchExpressionRegion2d.Classify(Enumerable.Repeat(open,257).ToArray()));
        Assert.Throws<ArgumentException>(()=>SketchExpressionProfile2d.Create(open,Enumerable.Repeat(open,256).ToArray()));
    }

    private static SketchCurveExpression2d Line(SketchPoint2d a,SketchPoint2d b)=>
        SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(a,b));
}
