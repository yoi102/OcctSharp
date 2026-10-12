using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace OcctSharp.Tests.Shared;
internal static class BatchBYRawTopologyViewer {
 static void Check(int status){if(status!=0)throw new InvalidOperationException("BY Viewer native status:"+status);}
 static byte[] StableBefore(BatchAUVisibleWindow window){var timer=System.Diagnostics.Stopwatch.StartNew();byte[] previous=window.Capture();int stable=0;while(timer.ElapsedMilliseconds<10000){byte[] current=window.Capture();bool same=true;for(int y=16;y<344&&same;y++)for(int x=16;x<344&&same;x++){int i=(y*360+x)*4;for(int c=0;c<3;c++)if(current[i+c]!=previous[i+c]){same=false;break;}}stable=same?stable+1:0;if(stable>=3)return current;previous=current;}throw new InvalidOperationException("BY initial model frame did not stabilize before input.");}
 internal static void Run(string step,TopologyBooleanOperation operation,double actualVolume){
  typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBYRawTopologyViewer).Assembly]);
  using var window=new BatchAUVisibleWindow();nint viewer=0,shape=0,volume=0;
  try{Check(Read(Encoding.UTF8.GetBytes(step+"\0"),out shape));Check(Create(window.Handle,out viewer));Check(Display(viewer,shape,out long id));Check(Color(viewer,id,.15,.45,.75));Check(Mode(viewer,id,1));Check(Projection(viewer,2));Check(Fit(viewer));Check(Zoom(viewer,.65));Check(Redraw(viewer));
   bool pressed=false;window.RawPaint=()=>Check(Redraw(viewer));window.RawInput=(message,x,y)=>{if(message==0x201)pressed=true;else if(message==0x200)Check(Move(viewer,x,y,out _));else if(message==0x202&&pressed){pressed=false;Check(Select(viewer,x,y,out int count));if(count!=1)throw new InvalidOperationException("BY HWND selection count");}};
   int px=operation==TopologyBooleanOperation.Common?180:100,py=operation==TopologyBooleanOperation.Common?120:180;
   Check(Pick(viewer,0,[px,py],2,0,out int hit,new double[3],out long picked));if(hit!=1||picked!=id)throw new InvalidOperationException("BY exact edited-body pick identity");
   bool splitGapMiss=false;if(operation==TopologyBooleanOperation.Cut){Check(Pick(viewer,0,[180,180],2,0,out int empty,new double[3],out _));if(empty!=0)throw new InvalidOperationException("BY removed cutter gap unexpectedly picked geometry");splitGapMiss=true;}
   Check(Move(viewer,0,0,out _));var before=StableBefore(window);window.Mouse(0x201,px,py,true);window.Mouse(0x202,px,py);window.Mouse(0x200,0,0);var after=window.Capture();int changed=BatchAUVisibleWindow.Changed(before,after);if(changed<=100)throw new InvalidOperationException("BY actual selection repaint missing");
   var selected=new long[1];Check(Selected(viewer,selected,1,out int written));if(written!=1||selected[0]!=id)throw new InvalidOperationException("BY selected presentation identity mismatch");
   Check(Capture(viewer,0,[px,py],2,2,1,out volume));Check(Visible(viewer,id,0));Check(Current(volume,viewer,out int current));if(current!=0)throw new InvalidOperationException("BY visibility did not invalidate volume");
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BY_EVIDENCE") is {} root){string evidence=Path.Combine(root,operation.ToString().ToLowerInvariant());Directory.CreateDirectory(evidence);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"selected.png"),after);File.Copy(step,Path.Combine(evidence,"edited.step"),true);File.WriteAllText(Path.Combine(evidence,"visible-result.json"),JsonSerializer.Serialize(new{state="PASS",operation=operation.ToString(),pixels=changed,bodyHit=true,splitGapMiss,presentation=id,actualSelectedIdentity=selected[0],visibilityStale=true,capture="DXGI composited HWND client pixels",input="SendMessage HWND -> native move/select",stepVolume=actualVolume}));}
  }finally{window.RawInput=null;window.RawPaint=null;if(volume!=0)ReleaseVolume(volume,0);if(viewer!=0)ReleaseViewer(viewer);if(shape!=0)ReleaseShape(shape);}
 }
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_read_step")]static extern int Read(byte[] path,out nint shape);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_release")]static extern void ReleaseShape(nint shape);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_create")]static extern int Create(nint hwnd,out nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_release")]static extern void ReleaseViewer(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_display_shape")]static extern int Display(nint viewer,nint shape,out long id);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_display_mode")]static extern int Mode(nint viewer,long id,int mode);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_color")]static extern int Color(nint viewer,long id,double r,double g,double b);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_projection")]static extern int Projection(nint viewer,int projection);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_fit_all")]static extern int Fit(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_zoom")]static extern int Zoom(nint viewer,double factor);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_redraw")]static extern int Redraw(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_move_to")]static extern int Move(nint viewer,int x,int y,out int detected);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_select_at")]static extern int Select(nint viewer,int x,int y,out int selected);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_selected_snapshot")]static extern int Selected(nint viewer,[Out]long[] values,int capacity,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_visible")]static extern int Visible(nint viewer,long id,int visible);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_scene_pick")]static extern int Pick(nint viewer,int kind,double[] points,int count,int overlap,out int hit,[Out]double[] point,out long presentation);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_create")]static extern int Capture(nint viewer,int kind,double[] points,int count,int tolerance,int overlap,out nint token);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_current")]static extern int Current(nint token,nint viewer,out int current);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_release")]static extern void ReleaseVolume(nint token,int kind);
}
