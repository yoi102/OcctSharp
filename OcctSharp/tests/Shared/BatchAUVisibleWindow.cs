using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace OcctSharp.Tests.Shared;

// An actual on-screen CS_OWNDC window. DXGI reads composited desktop pixels.
internal sealed class BatchAUVisibleWindow : IDisposable
{
    internal const int Size=360;
    const uint WindowStyle=0x06cf0000;
    readonly string className="OcctSharpAU"+Guid.NewGuid().ToString("N");
    readonly WndProc callback;
    readonly nint instance=GetModuleHandle(null);
    internal readonly nint Handle;
    readonly int screenX,screenY;
    readonly BatchAUDesktopCapture desktop;
    internal Exception? InputError { get; set; }
    internal Action<uint,int,int>? RawInput;
    internal Action? RawPaint;
#if !DIRECT_MODULE_CONSUMER
    internal OcctViewer? Viewer;
#endif
    internal BatchAUVisibleWindow()
    {
        // Visible-frame acceptance requires an awake display. Keep it awake only
        // for this test window and restore the thread's power request on disposal.
        _=SetThreadExecutionState(0x80000003);
        var monitor=new MonitorInfo{Size=(uint)Marshal.SizeOf<MonitorInfo>()};
        if(!GetMonitorInfo(MonitorFromWindow(GetForegroundWindow(),2),ref monitor))throw new InvalidOperationException("Cannot locate active monitor.");
        screenX=monitor.Work.Left+50;screenY=monitor.Work.Top+50;
        desktop=new(screenX,screenY);
        callback=Route;
        var wc=new WindowClass{Size=(uint)Marshal.SizeOf<WindowClass>(),Style=0x20,Procedure=Marshal.GetFunctionPointerForDelegate(callback),Instance=instance,ClassName=className};
        if(RegisterClassEx(ref wc)==0)throw new InvalidOperationException("AU window class registration failed.");
        var frame=new Rect{Right=Size,Bottom=Size};AdjustWindowRectEx(ref frame,WindowStyle,false,8);
        Handle=CreateWindowEx(8,className,"OcctSharp AU live selection validation",WindowStyle,screenX,screenY,frame.Right-frame.Left,frame.Bottom-frame.Top,0,0,instance,0);
        if(Handle==0)throw new InvalidOperationException("AU HWND creation failed.");
        ShowWindow(Handle,5);ShowWindow(Handle,5);SetWindowPos(Handle,-1,screenX,screenY,frame.Right-frame.Left,frame.Bottom-frame.Top,0x40);UpdateWindow(Handle);
        if(!IsWindowVisible(Handle))throw new InvalidOperationException("AU window is not visible.");
    }
    nint Route(nint hwnd,uint message,nuint wp,nint lp)
    {
        if(RawInput is {} input && message is 0x201 or 0x200 or 0x202){try{input(message,(short)((long)lp&0xffff),(short)(((long)lp>>16)&0xffff));}catch(Exception e){InputError=e;}return 0;}
        if(RawPaint is {} rawPaint && message==0xf){BeginPaint(hwnd,out var state);try{rawPaint();}catch(Exception e){InputError=e;}finally{EndPaint(hwnd,ref state);}return 0;}
#if !DIRECT_MODULE_CONSUMER
        if(message==0x14)return 1;
        if(message==0xf && Viewer is {} paintViewer)
        {
            BeginPaint(hwnd,out var paint);
            try{paintViewer.Redraw();}catch(Exception e){InputError=e;}
            finally{EndPaint(hwnd,ref paint);}
            return 0;
        }
        if(Viewer is {} v && message is 0x201 or 0x200 or 0x202)
        {
            int x=(short)((long)lp&0xffff),y=(short)(((long)lp>>16)&0xffff);
            try{if(message==0x201)v.Input.PointerPressed(ViewerPointerButton.Left,x,y);
                else if(message==0x202)v.Input.PointerReleased(ViewerPointerButton.Left,x,y);
                else v.Input.PointerMoved(x,y,(wp&1)!=0?ViewerPointerButtons.Left:ViewerPointerButtons.None);}
            catch(Exception e){InputError=e;}
            return 0;
        }
#endif
        return DefWindowProc(hwnd,message,wp,lp);
    }
    internal void Mouse(uint message,int x,int y,bool down=false)
    {SendMessage(Handle,message,down?1u:0u,(nint)((y<<16)|(x&0xffff)));if(InputError is {} e)throw new InvalidOperationException("Actual HWND input route failed.",e);}
    internal void Resize(int width,int height){var frame=new Rect{Right=width,Bottom=height};AdjustWindowRectEx(ref frame,WindowStyle,false,8);SetWindowPos(Handle,-1,screenX,screenY,frame.Right-frame.Left,frame.Bottom-frame.Top,0x40);}
    internal byte[] Capture()
    {
        // Let the window visibility/compositor messages run on its owning UI thread.
        // No extra Viewer redraw is issued: the selection call must supply that repaint.
        var time=System.Diagnostics.Stopwatch.StartNew();
        while(time.ElapsedMilliseconds<120){PumpOnce();Thread.Sleep(10);}
        _=DwmFlush();GetClientRect(Handle,out var rect);if(rect.Right!=Size||rect.Bottom!=Size)throw new InvalidOperationException("Unexpected visible client extent.");
        var point=new PixelPoint();if(!ClientToScreen(Handle,ref point))throw new InvalidOperationException("Cannot locate visible client.");
        _=DwmGetWindowAttribute(Handle,14,out int cloaked,4);GetWindowDisplayAffinity(Handle,out uint affinity);
        string? evidence=Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE");
        if(!string.IsNullOrWhiteSpace(evidence)){Directory.CreateDirectory(evidence);var input=OpenInputDesktop(0,false,1);try{File.WriteAllText(Path.Combine(evidence,"window-state.json"),System.Text.Json.JsonSerializer.Serialize(new{hwnd=(long)Handle,point.X,point.Y,cloaked,affinity,visible=IsWindowVisible(Handle),foreground=(long)GetForegroundWindow(),process=Environment.ProcessId,station=DesktopName(GetProcessWindowStation()),desktop=DesktopName(GetThreadDesktop(GetCurrentThreadId())),inputDesktop=DesktopName(input)}));}finally{if(input!=0)CloseDesktop(input);}}
        return desktop.Capture(point.X,point.Y,Size,Size);
    }
    internal static int Changed(byte[] a,byte[] b)=>Enumerable.Range(0,a.Length/4).Count(i=>a[i*4]!=b[i*4]||a[i*4+1]!=b[i*4+1]||a[i*4+2]!=b[i*4+2]);
    internal static void PumpOnce(){for(int i=0;i<32&&PeekMessage(out var message,0,0,0,1);i++){TranslateMessage(ref message);DispatchMessage(ref message);}}
    internal static void SavePng(string path,byte[] bgra)
    {
        using var file=File.Create(path);file.Write(new byte[]{137,80,78,71,13,10,26,10});
        byte[] header=new byte[13];System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header,Size);System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4),Size);header[8]=8;header[9]=2;
        Chunk(file,"IHDR",header);
        using var bytes=new MemoryStream();using(var zip=new ZLibStream(bytes,CompressionLevel.Optimal,true))for(int y=0;y<Size;y++){zip.WriteByte(0);for(int x=0;x<Size;x++){int i=(y*Size+x)*4;zip.WriteByte(bgra[i+2]);zip.WriteByte(bgra[i+1]);zip.WriteByte(bgra[i]);}}
        Chunk(file,"IDAT",bytes.ToArray());Chunk(file,"IEND",[]);
    }
    static void Chunk(Stream file,string type,byte[] data)
    {Span<byte> four=stackalloc byte[4];System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(four,data.Length);file.Write(four);var name=Encoding.ASCII.GetBytes(type);file.Write(name);file.Write(data);uint crc=uint.MaxValue;foreach(byte b in name.Concat(data)){crc^=b;for(int i=0;i<8;i++)crc=(crc&1)!=0?(crc>>1)^0xedb88320u:crc>>1;}System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(four,~crc);file.Write(four);}
    public void Dispose(){desktop.Dispose();DestroyWindow(Handle);UnregisterClass(className,instance);_=SetThreadExecutionState(0x80000000);GC.KeepAlive(callback);}
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]delegate nint WndProc(nint h,uint m,nuint w,nint l);
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct WindowClass{public uint Size,Style;public nint Procedure;public int ClassExtra,WindowExtra;public nint Instance,Icon,Cursor,Background;public string? Menu;public string ClassName;public nint SmallIcon;}
    [StructLayout(LayoutKind.Sequential)]struct PixelPoint{public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]struct Message{public nint Window;public uint Id;public nuint WParam;public nint LParam;public uint Time;public PixelPoint Point;public uint Private;}
    [StructLayout(LayoutKind.Sequential)]struct Rect{public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential)]struct MonitorInfo{public uint Size;public Rect Bounds,Work;public uint Flags;}
    [StructLayout(LayoutKind.Sequential)]struct Paint{public nint DC;public int Erase;public Rect Rectangle;public int Restore,Update;public long Reserved1,Reserved2,Reserved3,Reserved4;}
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern nint GetModuleHandle(string? name);
    [DllImport("kernel32.dll")]static extern uint SetThreadExecutionState(uint flags);
    [DllImport("user32.dll",EntryPoint="RegisterClassExW",CharSet=CharSet.Unicode)]static extern ushort RegisterClassEx(ref WindowClass wc);
    [DllImport("user32.dll",EntryPoint="UnregisterClassW",CharSet=CharSet.Unicode)]static extern bool UnregisterClass(string name,nint instance);
    [DllImport("user32.dll",EntryPoint="CreateWindowExW",CharSet=CharSet.Unicode)]static extern nint CreateWindowEx(uint ex,string cls,string title,uint style,int x,int y,int w,int h,nint parent,nint menu,nint instance,nint parameter);
    [DllImport("user32.dll",EntryPoint="DefWindowProcW")]static extern nint DefWindowProc(nint h,uint m,nuint w,nint l);
    [DllImport("user32.dll",EntryPoint="SendMessageW")]static extern nint SendMessage(nint h,uint m,nuint w,nint l);
    [DllImport("user32.dll")]static extern bool ShowWindow(nint h,int command);
    [DllImport("user32.dll")]static extern bool UpdateWindow(nint h);
    [DllImport("user32.dll")]static extern bool IsWindowVisible(nint h);
    [DllImport("user32.dll")]static extern bool SetWindowPos(nint h,nint after,int x,int y,int w,int height,uint flags);
    [DllImport("user32.dll")]static extern bool AdjustWindowRectEx(ref Rect rectangle,uint style,bool menu,uint extended);
    [DllImport("user32.dll")]static extern bool DestroyWindow(nint h);
    [DllImport("user32.dll")]static extern bool GetClientRect(nint h,out Rect rect);
    [DllImport("user32.dll")]static extern bool ClientToScreen(nint h,ref PixelPoint point);
    [DllImport("dwmapi.dll")]static extern int DwmFlush();
    [DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(nint window,uint attribute,out int value,uint size);
    [DllImport("user32.dll")]static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]static extern bool GetWindowDisplayAffinity(nint hwnd,out uint affinity);
    [DllImport("user32.dll")]static extern nint MonitorFromWindow(nint window,uint flags);
    [DllImport("user32.dll",EntryPoint="GetMonitorInfoW")]static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
    static string DesktopName(nint desktop){var value=new char[256];return GetUserObjectInformation(desktop,2,value,512,out _)?new string(value).TrimEnd('\0'):"unknown";}
    [DllImport("user32.dll",EntryPoint="GetUserObjectInformationW",CharSet=CharSet.Unicode)]static extern bool GetUserObjectInformation(nint handle,int index,[Out]char[] value,int length,out int needed);
    [DllImport("user32.dll")]static extern nint GetProcessWindowStation();
    [DllImport("user32.dll")]static extern nint GetThreadDesktop(uint thread);
    [DllImport("kernel32.dll")]static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]static extern nint OpenInputDesktop(uint flags,bool inherit,uint access);
    [DllImport("user32.dll")]static extern bool CloseDesktop(nint desktop);
    [DllImport("user32.dll")]static extern nint BeginPaint(nint hwnd,out Paint paint);
    [DllImport("user32.dll")]static extern bool EndPaint(nint hwnd,ref Paint paint);
    [DllImport("user32.dll",EntryPoint="PeekMessageW")]static extern bool PeekMessage(out Message message,nint window,uint first,uint last,uint flags);
    [DllImport("user32.dll")]static extern bool TranslateMessage(ref Message message);
    [DllImport("user32.dll",EntryPoint="DispatchMessageW")]static extern nint DispatchMessage(ref Message message);
}
