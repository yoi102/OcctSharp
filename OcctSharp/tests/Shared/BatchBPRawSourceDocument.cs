using System.Runtime.InteropServices;
using System.Text;
using OcctSharp.ConstructionWorkspaces;
namespace OcctSharp.Tests.Shared;
// Source fixture uses the existing public native document ABI for both consumers.
// The new multi-file export always runs through the generated public BP API.
internal static class BatchBPRawSourceDocument {
 internal static void Register()=>typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBPRawSourceDocument).Assembly]);
 internal static string ModelName=>BatchBNBoundedTextWorkflow.Names()[1]+" 🚀../同名";
 internal static Shape Solid(double width=4,double height=2){using var built=ParameterizedConstruction.AnalyticFace(AnalyticConstructionSurface.Plane,ConstructionFrame.World,0,width,0,width);using var face=built.CopyShape();using var vector=GpVec.Create(0,0,height);return face.Extrude(vector);}
 internal static string Create(string directory,double height=2){Register();string a=Path.Combine(directory,"bl-one.step"),b=Path.Combine(directory,"bl-two.step"),seed=Path.Combine(directory,"seed.step");using(var shape=Solid(height:height))ShapeExchange.WriteStep(shape,a);using(var shape=Solid(2,height))ShapeExchange.WriteStep(shape,b);nint doc=0,identity=0,translated=0,trsf=0;try{Check(Read(Utf8(a),out doc));Check(Begin(doc));Check(Import(doc,Utf8(b),out int roots));if(roots!=1)throw new InvalidOperationException("BP second BL source import failed.");string first=Free(doc,1),second=Free(doc,2);Name(doc,first,ModelName);Name(doc,second,ModelName);Check(Color(doc,Utf8(first),new(.2,.3,.4,1)));Check(Color(doc,Utf8(second),new(.7,.6,.5,1)));byte[] layer=Encoding.UTF8.GetBytes("本轮图层");Check(Layer(doc,Utf8(first),layer,layer.Length,1));Check(Layer(doc,Utf8(second),layer,layer.Length,1));Check(Identity(out identity));Check(Trsf(10,0,0,0,0,1,0,out trsf));Check(Location(trsf,out translated));byte[] assemblyName=Encoding.UTF8.GetBytes(BatchBNBoundedTextWorkflow.Names()[0]);string assembly=Output((data,count)=>{int status=Assembly(doc,assemblyName,assemblyName.Length,data,count,out int written);return(status,written);});_=Output((data,count)=>{int status=Component(doc,Utf8(assembly),Utf8(first),identity,data,count,out int written);return(status,written);});_=Output((data,count)=>{int status=Component(doc,Utf8(assembly),Utf8(first),translated,data,count,out int written);return(status,written);});_=Output((data,count)=>{int status=Component(doc,Utf8(assembly),Utf8(second),identity,data,count,out int written);return(status,written);});byte[] otherName=Encoding.UTF8.GetBytes("共享定义第二根");string other=Output((data,count)=>{int status=Assembly(doc,otherName,otherName.Length,data,count,out int written);return(status,written);});_=Output((data,count)=>{int status=Component(doc,Utf8(other),Utf8(first),identity,data,count,out int written);return(status,written);});Check(Commit(doc,out int changed));if(changed!=1)throw new InvalidOperationException("BP source assembly did not commit.");Check(Write(doc,Utf8(seed)));return seed;}
 finally{if(translated!=0)ReleaseLocation(translated);if(identity!=0)ReleaseLocation(identity);if(trsf!=0)ReleaseTrsf(trsf);if(doc!=0)Close(doc);}
 }
 static string Free(nint doc,int index)=>Output((data,count)=>{int status=FreeEntry(doc,index,data,count,out int written);return(status,written);});
 static string Output(Func<byte[],int,(int Status,int Written)> call){byte[] data=new byte[1024];var result=call(data,data.Length);Check(result.Status);if(result.Written<1||result.Written>=data.Length)throw new InvalidDataException("BP source label buffer.");return Encoding.UTF8.GetString(data,0,result.Written);}
 static void Name(nint doc,string entry,string name){byte[] text=Encoding.UTF8.GetBytes(name);Check(SetName(doc,Utf8(entry),text,text.Length));}
 internal static byte[] Utf8(string text)=>Encoding.UTF8.GetBytes(text+"\0");
 internal static void Check(int status,[System.Runtime.CompilerServices.CallerArgumentExpression(nameof(status))]string operation=""){if(status!=0)throw new InvalidOperationException("BP source native document ABI failed: "+operation+" => "+status+": "+Marshal.PtrToStringUTF8(LastError()));}
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_get_last_error")]static extern nint LastError();
 [StructLayout(LayoutKind.Sequential)]readonly record struct RawColor(double R,double G,double B,double A);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_read_step")]static extern int Read(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_import_step")]static extern int Import(nint doc,byte[] path,out int roots);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_write_step")]static extern int Write(nint doc,byte[] path);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_begin_command")]static extern int Begin(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_commit_command")]static extern int Commit(nint doc,out int changed);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_entry")]static extern int FreeEntry(nint doc,int index,[Out]byte[] data,int count,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_label_set_name")]static extern int SetName(nint doc,byte[] entry,byte[] name,int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_add_assembly")]static extern int Assembly(nint doc,byte[] name,int length,[Out]byte[] data,int count,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_add_component")]static extern int Component(nint doc,byte[] assembly,byte[] part,nint location,[Out]byte[] data,int count,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_location_create_identity")]static extern int Identity(out nint location);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_location_create_from_trsf")]static extern int Location(nint trsf,out nint location);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_location_release")]static extern void ReleaseLocation(nint location);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_trsf_create_translation_rotation")]static extern int Trsf(double x,double y,double z,double ax,double ay,double az,double angle,out nint trsf);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_trsf_release")]static extern void ReleaseTrsf(nint trsf);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_set_color")]static extern int Color(nint doc,byte[] entry,RawColor color);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_set_layer")]static extern int Layer(nint doc,byte[] entry,byte[] layer,int length,int replace);
}
