using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace OcctSharp.Tests.Shared;
internal static class BatchBXRawTopologyViewer {
 static void Check(int status){if(status!=0)throw new InvalidOperationException("BX Viewer native status:"+status);}
 // Settle only the pre-input frame. Post-selection capture remains immediate:
 // no additional redraw or stability wait may manufacture selection repaint.
 static byte[] StableBefore(BatchAUVisibleWindow window){
  var timer=System.Diagnostics.Stopwatch.StartNew();byte[] previous=window.Capture();int stable=0;
  while(timer.ElapsedMilliseconds<10000){byte[] current=window.Capture();bool same=true;for(int y=16;y<344&&same;y++)for(int x=16;x<344&&same;x++){int i=(y*360+x)*4;for(int c=0;c<3;c++)if(current[i+c]!=previous[i+c]){same=false;break;}}
   stable=same?stable+1:0;if(stable>=3)return current;previous=current;
  }
  throw new InvalidOperationException("BX initial model frame did not stabilize before input.");
 }
#if DIRECT_MODULE_CONSUMER
 // Fixture-only use of the existing public surface ABI. The typed pcurve calls
 // still use the packaged generated managed API. No facade dependency is added.
 [StructLayout(LayoutKind.Sequential)]struct Point2{internal double X,Y;internal Point2(double x,double y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)]readonly record struct Curve(int Kind,int Degree,int Periodic,int Rational,int Reversed,int PoleCount,int KnotCount,double First,double Last,double Major,double Minor,double Angle,nint Poles,nint Weights,nint Knots,nint Multiplicities);
 internal static Shape TrimFace(Shape support){
  string seed=Path.Combine(Path.GetTempPath(),"OcctSharp.BX.module."+Guid.NewGuid()+".step"),output=seed+".trim.step";nint root=0,face=0,result=0;var wires=new List<nint>();
  try{ShapeExchange.WriteStep(support,seed);Check(Read(Encoding.UTF8.GetBytes(seed+"\0"),out root));var faces=new nint[1];Check(Faces(root,faces,1,out int written));face=faces[0];if(written!=1)throw new InvalidOperationException("BX module source must have exactly one face.");
   foreach(var box in new (double A,double B,double C,double D)[]{(0.0,0.0,1.0,1.0),(.25,.25,.75,.75)}){Point2[] p=[new(box.A,box.B),new(box.C,box.B),new(box.C,box.D),new(box.A,box.D)];var edges=new nint[4];
    if(wires.Count!=0)Array.Reverse(p);
    try{for(int i=0;i<4;i++){Point2[] points=[p[i],p[(i+1)%4]];var pinned=GCHandle.Alloc(points,GCHandleType.Pinned);try{double length=Math.Abs(points[1].X-points[0].X)+Math.Abs(points[1].Y-points[0].Y);Curve curve=new(1,0,0,0,0,2,0,0,length,0,0,0,pinned.AddrOfPinnedObject(),0,0,0);Check(Lift(face,in curve,1,1e-7,out edges[i]));}finally{pinned.Free();}}Check(Wire(face,edges,4,1e-7,out nint wire));wires.Add(wire);}finally{foreach(nint edge in edges)if(edge!=0)ReleaseShape(edge);}}
   Check(Face(face,wires.ToArray(),wires.Count,1e-7,out result));Check(Write(result,Encoding.UTF8.GetBytes(output+"\0")));return ShapeExchange.ReadStep(output);
  }finally{if(result!=0)ReleaseShape(result);foreach(nint wire in wires)ReleaseShape(wire);if(face!=0)ReleaseShape(face);if(root!=0)ReleaseShape(root);File.Delete(seed);File.Delete(output);}
 }
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_surface_lift_curve")]static extern int Lift(nint face,in Curve curve,int build,double tolerance,out nint edge);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_surface_make_wire")]static extern int Wire(nint face,nint[] edges,int count,double tolerance,out nint wire);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_surface_make_face")]static extern int Face(nint face,nint[] wires,int count,double tolerance,out nint result);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_write_step")]static extern int Write(nint shape,byte[] path);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_face_snapshot")]static extern int Faces(nint shape,[Out]nint[] faces,int capacity,out int written);
#endif
 internal static void Run(string step){
  typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBXRawTopologyViewer).Assembly]);
  using var window=new BatchAUVisibleWindow();nint viewer=0,shape=0,volume=0;
  try{Check(Read(Encoding.UTF8.GetBytes(step+"\0"),out shape));Check(Create(window.Handle,out viewer));Check(Display(viewer,shape,out long id));Check(Color(viewer,id,.15,.45,.75));Check(Mode(viewer,id,1));Check(Projection(viewer,2));Check(Fit(viewer));Check(Zoom(viewer,.65));Check(Redraw(viewer));
   bool pressed=false;window.RawPaint=()=>Check(Redraw(viewer));window.RawInput=(message,x,y)=>{if(message==0x201)pressed=true;else if(message==0x200)Check(Move(viewer,x,y,out _));else if(message==0x202&&pressed){pressed=false;Check(Select(viewer,x,y,out int count));if(count!=1)throw new InvalidOperationException("BX HWND selection count");}};
   Check(Pick(viewer,0,[90,180],2,0,out int hit,new double[3],out long picked));if(hit!=1||picked!=id)throw new InvalidOperationException("BX holed face exact ring pick identity");
   Check(Pick(viewer,0,[180,180],2,0,out int empty,new double[3],out _));if(empty!=0)throw new InvalidOperationException("BX hole centre unexpectedly picked geometry");
   Check(Move(viewer,0,0,out _));var before=StableBefore(window);window.Mouse(0x201,90,180,true);window.Mouse(0x202,90,180);window.Mouse(0x200,0,0);var after=window.Capture();int changed=BatchAUVisibleWindow.Changed(before,after);if(changed<=100)throw new InvalidOperationException("BX actual selection repaint missing");
   var selected=new long[1];Check(Selected(viewer,selected,1,out int written));if(written!=1||selected[0]!=id)throw new InvalidOperationException("BX selected presentation identity mismatch");
   Check(Capture(viewer,0,[90,180],2,2,1,out volume));Check(Visible(viewer,id,0));Check(Current(volume,viewer,out int current));if(current!=0)throw new InvalidOperationException("BX source visibility did not invalidate volume");
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_BX_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"selected.png"),after);File.Copy(step,Path.Combine(evidence,"holed.step"),true);File.WriteAllText(Path.Combine(evidence,"visible-result.json"),JsonSerializer.Serialize(new{state="PASS",pixels=changed,ringHit=true,holeCentreMiss=true,presentation=id,actualSelectedIdentity=selected[0],visibilityStale=true,capture="DXGI composited HWND client pixels",input="SendMessage HWND -> native move/select",stepArea=12}));}
  }finally{window.RawInput=null;window.RawPaint=null;if(volume!=0)ReleaseVolume(volume,0);if(viewer!=0)ReleaseViewer(viewer);if(shape!=0)ReleaseShape(shape);}
 }
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_read_step")]static extern int Read(byte[] path,out nint shape);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_release")]static extern void ReleaseShape(nint shape);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_create")]static extern int Create(nint hwnd,out nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_release")]static extern void ReleaseViewer(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_display_shape")]static extern int Display(nint viewer,nint shape,out long id);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_display_mode")]static extern int Mode(nint viewer,long id,int mode);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_color")]static extern int Color(nint viewer,long id,double r,double g,double b);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_projection")]static extern int Projection(nint viewer,int mode);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_fit_all")]static extern int Fit(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_zoom")]static extern int Zoom(nint viewer,double factor);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_redraw")]static extern int Redraw(nint viewer);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_move_to")]static extern int Move(nint viewer,int x,int y,out int detected);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_select_at")]static extern int Select(nint viewer,int x,int y,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_selected_snapshot")]static extern int Selected(nint viewer,[Out]long[] ids,int capacity,out int written);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_scene_pick")]static extern int Pick(nint viewer,int kind,double[] points,int count,int overlap,out int hit,[Out]double[] point,out long presentation);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_create")]static extern int Capture(nint viewer,int kind,double[] points,int count,int tolerance,int overlap,out nint token);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_current")]static extern int Current(nint token,nint viewer,out int current);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_presentation_visible")]static extern int Visible(nint viewer,long id,int visible);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_release")]static extern void ReleaseVolume(nint token,int role);
}
