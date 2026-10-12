using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace OcctSharp.Tests.Shared;

internal static class BatchBQRawAssemblyViewer {
 static void Check(int status){if(status!=0)throw new InvalidOperationException("BQ native Viewer status: "+status);}
 static void Require(bool condition,string detail){if(!condition)throw new InvalidOperationException("BQ Viewer: "+detail);}
 static string Text(Func<byte[]?,int,(int Status,int Length)> query){var bytes=new byte[4096];var result=query(bytes,bytes.Length);Check(result.Status);Require(result.Length>0&&result.Length<bytes.Length,"bounded exact entry");return Encoding.UTF8.GetString(bytes,0,result.Length).TrimEnd('\0');}
 static byte[] Utf8(string value)=>Encoding.UTF8.GetBytes(value+"\0" );
 static string Free(nint doc,int index)=>Text((bytes,size)=>{int status=FreeEntry(doc,index,bytes,size,out int length);return(status,length);});
 static string Component(nint doc,string parent,int index)=>Text((bytes,size)=>{int status=ComponentEntry(doc,Utf8(parent),index,bytes,size,out int length);return(status,length);});
 static string Definition(nint doc,string occurrence)=>Text((bytes,size)=>{int status=Referred(doc,Utf8(occurrence),bytes,size,out int length);return(status,length);});
 internal static void Run(string step,string evidence,IReadOnlyList<string> values){
  // Test/consumer C ABI calls must use the same packaged resolver as module calls.
  typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBQRawAssemblyViewer).Assembly]);
  using var window=new BatchAUVisibleWindow();nint imported=0,restored=0,viewer=0,volume=0,transform=0;string persisted=Path.ChangeExtension(step,".viewer.xbf");
  try{
   Check(Read(Utf8(step),out imported));PersistValues(imported,values);Check(Save(imported,Utf8(persisted),1,0));Close(imported);imported=0;Check(Open(Utf8(persisted),out restored));Check(FreeCount(restored,out int roots));Require(roots==1,"one saved assembly root");string root=Free(restored,1);VerifyValues(restored,root,values);Check(ComponentCount(restored,Utf8(root),out int count));Require(count==3,"saved repeated occurrences");string occurrence=Component(restored,root,1),other=Component(restored,root,2),definition=Definition(restored,occurrence);Require(occurrence!=other&&definition==Definition(restored,other),"saved exact shared definition");
   Check(Create(window.Handle,out viewer));Check(Background(viewer,.1,.15,.2));Check(Display(viewer,restored,Utf8(occurrence),out long id));Check(Mode(viewer,id,1));Check(Projection(viewer,6));Check(Fit(viewer));Check(Zoom(viewer,.65));Check(Redraw(viewer));
   bool pressed=false;window.RawPaint=()=>Check(Redraw(viewer));window.RawInput=(message,x,y)=>{if(message==0x201)pressed=true;else if(message==0x200)Check(Move(viewer,x,y,out _));else if(message==0x202&&pressed){pressed=false;Check(Select(viewer,x,y,out int selected));Require(selected==1,"actual HWND selection count");}};
   Check(Move(viewer,0,0,out _));var before=window.Capture();window.Mouse(0x201,180,180,true);window.Mouse(0x202,180,180);window.Mouse(0x200,0,0);var after=window.Capture();int pixels=BatchAUVisibleWindow.Changed(before,after),interiorSelectionPixels=InteriorChanged(before,after),visibleModelPixelsBefore=Foreground(before),visibleModelPixelsAfter=Foreground(after);Require(visibleModelPixelsBefore>1000&&visibleModelPixelsAfter>1000&&interiorSelectionPixels>100,"visible interior model and actual interior selection repaint");Require(pixels>100,"actual DXGI selection repaint: "+pixels);
   var selectedIds=new long[1];Check(Selected(viewer,selectedIds,1,out int written));Require(written==1&&selectedIds[0]==id,"picked presentation registered for exact saved occurrence");Check(Pick(viewer,0,[180,180],2,0,out int hit,new double[3],out long picked));Require(hit==1&&picked==id,"saved occurrence presentation pick");
   Check(Capture(viewer,0,[180,180],2,2,1,out volume));Check(Visible(viewer,id,0));Check(Current(volume,viewer,out int current));Require(current==0,"visibility invalidates source volume");ReleaseVolume(volume,0);volume=0;Check(Visible(viewer,id,1));Check(Fit(viewer));
   Check(Capture(viewer,0,[180,180],2,2,1,out volume));Check(CreateTransform(1,0,0,0,0,1,0,out transform));Check(Transform(viewer,id,transform));Check(Current(volume,viewer,out current));Require(current==0,"transform invalidates source volume");
   Directory.CreateDirectory(evidence);File.Copy(persisted,Path.Combine(evidence,"roundtrip.xbf"),true);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"selected.png"),after);File.WriteAllText(Path.Combine(evidence,"visible-result.json"),JsonSerializer.Serialize(new{state="PASS",pixels,interiorSelectionPixels,visibleModelPixelsBefore,visibleModelPixelsAfter,framing="axonometric / 0.65 zoom after FitAll",root,occurrence,other,definition,presentation=id,sourceStepSha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(step))),persisted=true,persistedValues=values,persistedXbfSha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(persisted))),capture="DXGI composited client pixels",input="SendMessage HWND -> native Viewer move/select",identity="native queried saved occurrence -> registered presentation -> selected/picked id",visibilityStale=true,transformStale=true}));
  }finally{window.RawInput=null;window.RawPaint=null;if(volume!=0)ReleaseVolume(volume,0);if(transform!=0)ReleaseTransform(transform);if(viewer!=0)ReleaseViewer(viewer);if(restored!=0)Close(restored);if(imported!=0)Close(imported);File.Delete(persisted);}
 }
 static int InteriorChanged(byte[] a,byte[] b){int count=0;for(int y=16;y<344;++y)for(int x=16;x<344;++x){int i=(y*360+x)*4;if(a[i]!=b[i]||a[i+1]!=b[i+1]||a[i+2]!=b[i+2])++count;}return count;}
 static int Foreground(byte[] pixels){int count=0,corner=(16*360+16)*4;for(int y=16;y<344;++y)for(int x=16;x<344;++x){int i=(y*360+x)*4;if(Enumerable.Range(0,3).Any(c=>Math.Abs(pixels[i+c]-pixels[corner+c])>8))++count;}return count;}
 static void PersistValues(nint doc,IReadOnlyList<string> values){string entry=Free(doc,1);Check(Begin(doc));using var stream=new MemoryStream();using(var writer=new BinaryWriter(stream,Encoding.UTF8,true)){writer.Write(1);writer.Write(1);writer.Write(61);writer.Write(1);WriteText(writer,entry);writer.Write(8);WriteText(writer,"");writer.Write(1);writer.Write(0);writer.Write(values.Count);foreach(string value in values)WriteText(writer,value);}var bytes=stream.ToArray();Check(ReplaceValues(doc,1,bytes,bytes.Length));Check(Commit(doc,out int changed));Require(changed==1,"new round metadata committed");}
 static void WriteText(BinaryWriter writer,string value){var bytes=Encoding.UTF8.GetBytes(value);writer.Write(bytes.Length);writer.Write(bytes);}
 static string ReadText(BinaryReader reader){int count=reader.ReadInt32();Require(count>=0&&count<=1048576&&count<=reader.BaseStream.Length-reader.BaseStream.Position,"bounded persisted text");return new UTF8Encoding(false,true).GetString(reader.ReadBytes(count));}
 static void VerifyValues(nint doc,string root,IReadOnlyList<string> expected){nint token=0;try{Check(SnapshotValues(doc,1,Utf8(root),8,Utf8(""),out token));Check(PayloadSize(token,1,out int length));Require(length>=20&&length<=1048576,"bounded new round persistent values");var bytes=new byte[length];Check(PayloadCopy(token,1,bytes,bytes.Length));using var reader=new BinaryReader(new MemoryStream(bytes),Encoding.UTF8);Require(reader.ReadInt32()==1&&reader.ReadInt32()==1&&reader.ReadInt32()==60&&reader.ReadInt32()==8&&reader.ReadInt32()==1,"actual persisted direct string list");_=ReadText(reader);Require(reader.ReadInt32()==0&&reader.ReadInt32()==expected.Count,"actual persistent list bounds");foreach(string value in expected)Require(ReadText(reader)==value,"exact new round XBF values");Require(reader.BaseStream.Position==reader.BaseStream.Length,"no extra persistent data");}finally{if(token!=0)PayloadRelease(token,1);}}
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_begin_command")]static extern int Begin(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_commit_command")]static extern int Commit(nint doc,out int changed);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_replace")]static extern int ReplaceValues(nint doc,int domain,byte[] bytes,int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_snapshot")]static extern int SnapshotValues(nint doc,int domain,byte[] entry,int kind,byte[] id,out nint token);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_size")]static extern int PayloadSize(nint token,int kind,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_copy")]static extern int PayloadCopy(nint token,int kind,[Out]byte[] bytes,int capacity);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_release")]static extern void PayloadRelease(nint token,int kind);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_read_step",CallingConvention=CallingConvention.Cdecl)]static extern int Read(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open",CallingConvention=CallingConvention.Cdecl)]static extern int Open(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_save_format",CallingConvention=CallingConvention.Cdecl)]static extern int Save(nint doc,byte[] path,int xde,int xml);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_count")]static extern int FreeCount(nint doc,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_entry")]static extern int FreeEntry(nint doc,int index,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_component_count")]static extern int ComponentCount(nint doc,byte[] entry,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_component_entry")]static extern int ComponentEntry(nint doc,byte[] entry,int index,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_referred_entry")]static extern int Referred(nint doc,byte[] entry,[Out]byte[]? bytes,int capacity,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_background_color")]static extern int Background(nint viewer,double r,double g,double b);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_zoom")]static extern int Zoom(nint viewer,double factor);
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
