using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using OcctSharp;

internal static class Program
{
    [DllImport("user32.dll", EntryPoint="CreateWindowExW", CharSet=CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint extended, string className, string name, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(nint window);
    [STAThread]
    public static int Main(string[] args)
    {
        bool old=args[0]=="old", oldPublic=args[0]=="old-public";var version=OcctRuntime.Info.AbiVersion;
        if(version.Major!=1 || (old ? version.Minor!=116 : oldPublic ? version.Minor!=117 : version.Minor<118)) throw new InvalidOperationException("Unexpected actual native ABI: "+version);
        var checks=new List<string>();
        void Check(string name, Action action, Type refusal)
        {
            try { action(); if(old) throw new InvalidOperationException("Old runtime accepted "+name); }
            catch(Exception error) when(old && error.GetType()==refusal) { checks.Add(name); return; }
            if(!old)checks.Add(name);
        }
        using var box=ShapeFactory.CreateBox(1,2,3);
        if (oldPublic)
        {
            void Refused(string name, Action action)
            {
                try { action(); }
                catch (NotSupportedException) { checks.Add(name); return; }
                throw new InvalidOperationException("Public ABI 1.117 accepted a new ABI 1.118 contract: " + name);
            }
            Refused("input copy", () => { using var copy = AuthoringInputCopy.Create([box]); });
            using var snapshot = RepairSnapshot.Create(box);
            var face = snapshot.Topology.First(item => item.Kind == ShapeKind.Face).Selection;
            Refused("defeature history", () => { using var result = DefeatureHistoryModeling.Build(snapshot, [face]); });
            nint guardWindow=CreateWindowEx(0,"STATIC","Preview85 public ABI guard",0x80000000,-32000,-32000,320,320,0,0,0,0);
            if(guardWindow==0)throw new InvalidOperationException("Probe HWND failed");
            try
            {
                using var viewer=OcctViewer.Create(guardWindow);
                Refused("initial display", () => { using var presentation = viewer.DisplayInMode(box, ViewerDisplayMode.Shaded); });
                using var legacy = viewer.Display(box); viewer.FitAll();
                if (viewer.Rendering.CaptureColor(new(64,64)).CopyPixels().Length != 64*64*4)
                    throw new InvalidOperationException("Legacy viewer failed on public ABI 1.117");
            }
            finally { _=DestroyWindow(guardWindow); }
            File.WriteAllText(args[1],JsonSerializer.Serialize(new{state="PASS",oldPublic,actualAbi=version.ToString(),checks}));
            Console.WriteLine("Public ABI 1.117: new ABI 1.118 contracts refused explicitly; legacy rendering PASS.");
            return 0;
        }
        Check("exact sketch",()=>{_ = SketchCurveExpression2d.FromCurve(SketchCurve2d.Segment(default,new(2,0))).Evaluate(.5);},typeof(BadImageFormatException));
        Check("reference graph",()=>{_=BrepReferenceSnapshot.Capture(box);},typeof(NotSupportedException));
        Check("occurrence graph",()=>{_=BrepOccurrenceSnapshot.Capture(box);},typeof(NotSupportedException));
        using var wire=SketchCurveChain2d.Create([SketchCurve2d.Segment(default,new(2,0))]).BuildWire(SketchPlane.XY);
        Check("guided preparation",()=>{using var result=GuidedSweepPlan.PrepareOffsetCurves(wire);_=result.RequireShape();},typeof(NotSupportedException));
        nint window=CreateWindowEx(0,"STATIC","CH ABI guard probe",0x80000000,-32000,-32000,320,320,0,0,0,0);
        if(window==0)throw new InvalidOperationException("Probe HWND failed");
        try { using var viewer=OcctViewer.Create(window);Check("viewer source",()=>{using var source=viewer.CreateSource(box);},typeof(NotSupportedException)); }
        finally { _=DestroyWindow(window); }
        using var process=Process.GetCurrentProcess();
        var modules=process.Modules.Cast<ProcessModule>().Where(m=>m.FileName.Contains("\\occt\\",StringComparison.OrdinalIgnoreCase))
            .Select(m=>new{path=m.FileName,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.FileName)))}).ToArray();
        File.WriteAllText(args[1],JsonSerializer.Serialize(new{state="PASS",old,actualAbi=version.ToString(),checks,modules}));
        Console.WriteLine($"Actual ABI {version}: {checks.Count} consumer prerequisite guards PASS ({(old ? "refused" : "accepted")}).");return 0;
    }
}
