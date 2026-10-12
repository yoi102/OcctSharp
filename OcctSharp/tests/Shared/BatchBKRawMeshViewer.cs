using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace OcctSharp.Tests.Shared;

internal static class BatchBKRawMeshViewer {
 static void Check(int status){if(status!=0)throw new InvalidOperationException("BK native Viewer status: "+status);}
 static void Require(bool condition,string detail){if(!condition)throw new InvalidOperationException("BK Viewer: "+detail);}
 static string Text(Func<byte[]?,int,(int Status,int Length)> query){var bytes=new byte[4096];var result=query(bytes,bytes.Length);Check(result.Status);Require(result.Length>0&&result.Length<bytes.Length,"bounded exact entry");return Encoding.UTF8.GetString(bytes,0,result.Length).TrimEnd('\0');}
 static byte[] Utf8(string value)=>Encoding.UTF8.GetBytes(value+"\0" );
 static string Free(nint doc,int index)=>Text((bytes,size)=>{int status=FreeEntry(doc,index,bytes,size,out int length);return(status,length);});
 static string Component(nint doc,string parent,int index)=>Text((bytes,size)=>{int status=ComponentEntry(doc,Utf8(parent),index,bytes,size,out int length);return(status,length);});
 static string Definition(nint doc,string occurrence)=>Text((bytes,size)=>{int status=Referred(doc,Utf8(occurrence),bytes,size,out int length);return(status,length);});
 internal static void Run(string gltf,string evidence){
  // Test/consumer C ABI calls must use the same packaged resolver as module calls.
  typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBKRawMeshViewer).Assembly]);
  using var window=new BatchAUVisibleWindow();nint imported=0,restored=0,viewer=0,volume=0,transform=0;string persisted=Path.ChangeExtension(gltf,".viewer.xbf");
  try{
   Check(Read(Utf8(gltf),out imported));Check(Save(imported,Utf8(persisted),1,0));Close(imported);imported=0;Check(Open(Utf8(persisted),out restored));Check(FreeCount(restored,out int roots));Require(roots==1,"one saved assembly root");string root=Free(restored,1);Check(ComponentCount(restored,Utf8(root),out int count));Require(count==2,"saved repeated occurrences");string occurrence=Component(restored,root,1),other=Component(restored,root,2),definition=Definition(restored,occurrence);Require(occurrence!=other&&definition==Definition(restored,other),"saved exact shared definition");
   Check(Create(window.Handle,out viewer));Check(Display(viewer,restored,Utf8(occurrence),out long id));Check(Mode(viewer,id,1));Check(Projection(viewer,2));Check(Fit(viewer));Check(Redraw(viewer));
   bool pressed=false;window.RawPaint=()=>Check(Redraw(viewer));window.RawInput=(message,x,y)=>{if(message==0x201)pressed=true;else if(message==0x200)Check(Move(viewer,x,y,out _));else if(message==0x202&&pressed){pressed=false;Check(Select(viewer,x,y,out int selected));Require(selected==1,"actual HWND selection count");}};
   Check(Move(viewer,0,0,out _));var before=window.Capture();window.Mouse(0x201,180,180,true);window.Mouse(0x202,180,180);window.Mouse(0x200,0,0);var after=window.Capture();int pixels=BatchAUVisibleWindow.Changed(before,after);Require(pixels>100,"actual DXGI selection repaint: "+pixels);
   var selectedIds=new long[1];Check(Selected(viewer,selectedIds,1,out int written));Require(written==1&&selectedIds[0]==id,"picked presentation registered for exact saved occurrence");Check(Pick(viewer,0,[180,180],2,0,out int hit,new double[3],out long picked));Require(hit==1&&picked==id,"saved occurrence presentation pick");
   Check(Capture(viewer,0,[180,180],2,2,1,out volume));Check(Visible(viewer,id,0));Check(Current(volume,viewer,out int current));Require(current==0,"visibility invalidates source volume");ReleaseVolume(volume,0);volume=0;Check(Visible(viewer,id,1));Check(Fit(viewer));
   Check(Capture(viewer,0,[180,180],2,2,1,out volume));Check(CreateTransform(1,0,0,0,0,1,0,out transform));Check(Transform(viewer,id,transform));Check(Current(volume,viewer,out current));Require(current==0,"transform invalidates source volume");
   Directory.CreateDirectory(evidence);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"selected.png"),after);File.WriteAllText(Path.Combine(evidence,"visible-result.json"),JsonSerializer.Serialize(new{state="PASS",pixels,root,occurrence,other,definition,presentation=id,sourceGltfSha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(gltf))),persisted=true,capture="DXGI composited client pixels",input="SendMessage HWND -> native Viewer move/select",identity="native queried saved occurrence -> registered presentation -> selected/picked id",visibilityStale=true,transformStale=true}));
  }finally{window.RawInput=null;window.RawPaint=null;if(volume!=0)ReleaseVolume(volume,0);if(transform!=0)ReleaseTransform(transform);if(viewer!=0)ReleaseViewer(viewer);if(restored!=0)Close(restored);if(imported!=0)Close(imported);File.Delete(persisted);}
 }
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_read_gltf",CallingConvention=CallingConvention.Cdecl)]static extern int Read(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open",CallingConvention=CallingConvention.Cdecl)]static extern int Open(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_save_format",CallingConvention=CallingConvention.Cdecl)]static extern int Save(nint doc,byte[] path,int xde,int xml);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_count")]static extern int FreeCount(nint doc,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_entry")]static extern int FreeEntry(nint doc,int index,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_component_count")]static extern int ComponentCount(nint doc,byte[] entry,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_component_entry")]static extern int ComponentEntry(nint doc,byte[] entry,int index,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_referred_entry")]static extern int Referred(nint doc,byte[] entry,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_create")]static extern int Create(nint hwnd,out nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_display_xde_label")]static extern int Display(nint viewer,nint doc,byte[] entry,out long id);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_display_mode")]static extern int Mode(nint viewer,long id,int mode);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_projection")]static extern int Projection(nint viewer,int mode);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_fit_all")]static extern int Fit(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_redraw")]static extern int Redraw(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_move_to")]static extern int Move(nint viewer,int x,int y,out int detected);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_select_at")]static extern int Select(nint viewer,int x,int y,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_selected_snapshot")]static extern int Selected(nint viewer,[Out]long[] ids,int capacity,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_scene_pick")]static extern int Pick(nint viewer,int kind,double[] input,int count,int stick,out int hit,[Out]double[] point,out long id);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_create")]static extern int Capture(nint viewer,int kind,double[] coords,int count,int tolerance,int overlap,out nint token);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_current")]static extern int Current(nint token,nint viewer,out int current);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_release")]static extern void ReleaseVolume(nint token,int kind);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_visible")]static extern int Visible(nint viewer,long id,int value);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_trsf_create_translation_rotation")]static extern int CreateTransform(double x,double y,double z,double ax,double ay,double az,double angle,out nint transform);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_presentation_set_transform")]static extern int Transform(nint viewer,long id,nint transform);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_trsf_release")]static extern void ReleaseTransform(nint transform);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_release")]static extern void ReleaseViewer(nint viewer);
}
