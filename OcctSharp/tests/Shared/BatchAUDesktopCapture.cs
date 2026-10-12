using System.Runtime.InteropServices;

namespace OcctSharp.Tests.Shared;

// DXGI reads the composited desktop, including accelerated OpenGL windows that
// this driver's GDI BitBlt path omits. This is acceptance tooling, not a binding.
internal sealed class BatchAUDesktopCapture : IDisposable
{
    nint device,context,duplicate;
    OutputDescription outputDesc;
    internal BatchAUDesktopCapture(int x,int y)
    {
        Initialize(x,y);
    }
    void Initialize(int x,int y)
    {
        nint dxgi=0,adapter=0,output=0,output1=0;
        try
        {
            Check(D3D11CreateDevice(0,1,0,0,0,0,7,out device,out _,out context));
            dxgi=Query(device,new("54ec77fa-1377-44e6-8c32-88fd5f44c84c"));Check(Call<GetObject>(dxgi,7)(dxgi,out adapter));
            for(uint index=0;;index++){Check(Call<EnumOutput>(adapter,7)(adapter,index,out output));Check(Call<GetOutputDesc>(output,7)(output,ref outputDesc));if(outputDesc.Attached!=0&&x>=outputDesc.Bounds.Left&&y>=outputDesc.Bounds.Top&&x<outputDesc.Bounds.Right&&y<outputDesc.Bounds.Bottom)break;Marshal.Release(output);output=0;}
            Require(outputDesc.Rotation is 0 or 1,"Rotated desktop is outside this acceptance helper.");
            if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE") is {} evidence)
            {
                Directory.CreateDirectory(evidence);
                File.WriteAllText(Path.Combine(evidence,"dxgi-output.json"),System.Text.Json.JsonSerializer.Serialize(new{outputDesc.Device,left=outputDesc.Bounds.Left,top=outputDesc.Bounds.Top,right=outputDesc.Bounds.Right,bottom=outputDesc.Bounds.Bottom,outputDesc.Attached,outputDesc.Rotation,monitor=(long)outputDesc.Monitor}));
            }
            output1=Query(output,new("00cddea8-939b-4b83-a340-a685226666cc"));Check(Call<Duplicate>(output1,22)(output1,device,out duplicate));
        }
        catch{Dispose();throw;}
        finally{foreach(var obj in new[]{output1,output,adapter,dxgi})if(obj!=0)Marshal.Release(obj);}
    }
    internal byte[] Capture(int x,int y,int width,int height)
    {
        for(int attempt=0;;attempt++)
        {
            try{return CaptureFrame(x,y,width,height);}
            catch(COMException error) when(attempt<2 && error.HResult is unchecked((int)0x887A0026) or unchecked((int)0x887A0005) or unchecked((int)0x887A0007))
            {
                // Desktop duplication is invalidated by compositor/device changes.
                // Recreate the capture session; issue no Viewer redraw or input event.
                if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE") is {} evidence)
                {
                    Directory.CreateDirectory(evidence);
                    File.AppendAllText(Path.Combine(evidence,"dxgi-recovery.jsonl"),System.Text.Json.JsonSerializer.Serialize(new{attempt=attempt+1,hresult=error.HResult,time=DateTimeOffset.UtcNow})+Environment.NewLine);
                }
                Dispose();Thread.Sleep(50);Initialize(x,y);
            }
        }
    }
    byte[] CaptureFrame(int x,int y,int width,int height)
    {
        nint resource=0,texture=0,staging=0;var frame=Marshal.AllocHGlobal(96);bool acquired=false,mapped=false;
        try
        {
            Check(Call<Acquire>(duplicate,8)(duplicate,1000,frame,out resource));acquired=true;
            texture=Query(resource,new("6f15aaf2-d208-4e89-9ab4-489535d34f9c"));var td=new TextureDescription();Call<GetTextureDesc>(texture,10)(texture,ref td);
            x-=outputDesc.Bounds.Left;y-=outputDesc.Bounds.Top;
            Require(td.Format==87&&x>=0&&y>=0&&x+width<=td.Width&&y+height<=td.Height,"Desktop capture bounds/format mismatch.");
            td.Usage=3;td.Bind=0;td.CpuAccess=0x20000;td.Misc=0;Check(Call<CreateTexture>(device,5)(device,ref td,0,out staging));
            Call<CopyResource>(context,47)(context,staging,texture);Check(Call<Map>(context,14)(context,staging,0,1,0,out var map));mapped=true;
            var bytes=new byte[width*height*4];for(int row=0;row<height;row++)Marshal.Copy(map.Data+(y+row)*(int)map.RowPitch+x*4,bytes,row*width*4,width*4);
            return bytes;
        }
        finally
        {
            if(mapped)Call<Unmap>(context,15)(context,staging,0);
            if(acquired)Check(Call<ReleaseFrame>(duplicate,14)(duplicate));
            foreach(var obj in new[]{staging,texture,resource})if(obj!=0)Marshal.Release(obj);
            Marshal.FreeHGlobal(frame);
        }
    }
    public void Dispose(){foreach(var obj in new[]{duplicate,context,device})if(obj!=0)Marshal.Release(obj);duplicate=context=device=0;}
    static T Call<T>(nint obj,int index)where T:Delegate=>Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(obj),index*IntPtr.Size));
    static nint Query(nint obj,Guid id){Check(Marshal.QueryInterface(obj,in id,out var result));return result;}
    static void Check(int h)=>Marshal.ThrowExceptionForHR(h);
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    [StructLayout(LayoutKind.Sequential)]struct Rect{public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct OutputDescription{[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Device;public Rect Bounds;public int Attached;public int Rotation;public nint Monitor;}
    [StructLayout(LayoutKind.Sequential)]struct TextureDescription{public uint Width,Height,Mips,Array,Format,Samples,Quality,Usage,Bind,CpuAccess,Misc;}
    [StructLayout(LayoutKind.Sequential)]struct Mapped{public nint Data;public uint RowPitch,DepthPitch;}
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int GetObject(nint self,out nint obj);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int EnumOutput(nint self,uint index,out nint obj);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int GetOutputDesc(nint self,ref OutputDescription desc);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int Duplicate(nint self,nint device,out nint duplicate);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int Acquire(nint self,uint timeout,nint frame,out nint resource);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int ReleaseFrame(nint self);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate void GetTextureDesc(nint self,ref TextureDescription desc);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int CreateTexture(nint self,ref TextureDescription desc,nint input,out nint texture);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate void CopyResource(nint self,nint destination,nint source);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate int Map(nint self,nint resource,uint sub,uint type,uint flags,out Mapped map);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate void Unmap(nint self,nint resource,uint sub);
    [DllImport("d3d11.dll")]static extern int D3D11CreateDevice(nint adapter,uint type,nint software,uint flags,nint levels,uint count,uint sdk,out nint device,out uint level,out nint context);
}
