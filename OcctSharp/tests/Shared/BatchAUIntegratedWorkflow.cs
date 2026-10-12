using System.Runtime.InteropServices;
using System.Text.Json;
using OcctSharp.CopiedCollections;
using OcctSharp.DocumentProjections;
using OcctSharp.ExchangeSessions;
using OcctSharp.HatchingWorkspaces;
using OcctSharp.MeshingWorkspaces;
using OcctSharp.ModelingWorkspaces;
using OcctSharp.SelectionWorkspaces;
using OcctSharp.Values;

namespace OcctSharp.Tests.Shared;

internal static class BatchAUIntegratedWorkflow
{
    internal static void Run()
    {
        var owners=new List<IDisposable>();string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.AU."+Guid.NewGuid());Directory.CreateDirectory(directory);
        try
        {
            using var line=BatchAQHatchingWorkflow.Line(0,0);
            using var scan=GeometricHatching.Scan(BatchAQHatchingWorkflow.Rectangle(owners),[new(line,-10,10)]);
            using var lifted=scan.TrimmedCurve3d(0,0,new Plane(new(new(0,0,0),new(0,0,1),new(1,0,0),new(0,1,0))))!;
            var interval=scan.Curves[0].Domains.Single();using var a=ModelingBuilders.Edge(lifted,interval.ClippedFirst,interval.ClippedLast);
            using var b=ModelingBuilders.Edge(new(4,0,0),new(4,3,0));using var c=ModelingBuilders.Edge(new(4,3,0),new(-4,3,0));using var d=ModelingBuilders.Edge(new(-4,3,0),new(-4,0,0));
            using var e1=a.Shape;using var e2=b.Shape;using var e3=c.Shape;using var e4=d.Shape;using var set=OwnedShapeSet.Create(e1,e2,e3,e4);using var wireResult=ModelingBuilders.Wire(set);using var wire=wireResult.Shape;
            using var face=ShapeFactory.CreatePlanarFace(wire);using var extrusion=GpVec.Create(0,0,3);using var solid=face.Extrude(extrusion);
            using var mesh=ControlledMeshing.Run(solid,new(Deflection:.05));using var meshed=mesh.CopyShape();
            Require(meshed.IsValid&&Math.Abs(meshed.InspectProperties(InspectionPropertyKind.Volume).Mass-72)<1e-6,"AQ/AP/AR geometry chain");
            var topology=MeshTopology.SnapshotExisting(meshed);Require(topology.Mesh.Triangles.Count>0,"AR copied mesh");
            string source=Path.Combine(directory,"source.step"),output=Path.Combine(directory,"retained.step");ShapeExchange.WriteStep(meshed,source);
            using var session=ExchangeFileSession.ReadStep(source);File.Delete(source);
            Require(session.WriteStep(output,new(name:"AU integrated model",authors:["AP AQ AR AS AT AU"])).Done,"AT retained write");
            using var readSession=ExchangeFileSession.ReadStep(output);using var restored=ShapeExchange.ReadStep(output);
            Require(readSession.Snapshot().StepHeader!.Name=="AU integrated model"&&Math.Abs(restored.InspectProperties(InspectionPropertyKind.Volume).Mass-72)<1e-5,"AT actual readback");
#if DIRECT_MODULE_CONSUMER
            // Friendly XDE/Viewer belong to Facade. Validate module AS projection and AU C ABI
            // explicitly, without introducing a Facade reference or duplicating its implementation.
            using var document=DocumentProjectionContext.CreateNativeDocument();using var attributes=DocumentProjectionContext.Bind(document);
            document.NewCommand();var label=attributes.GetLabel("0:1");attributes.ReplaceNamedData(label,new(integers:new Dictionary<string,int>{{"round",6}}));
            Require(attributes.Snapshot(label).NamedData!.Integers["round"]==6,"AS direct module metadata");
            RunRawViewer(output);
#else
            using var document=XdeDocument.Create();using var transaction=document.BeginTransaction("AU integrated metadata");var part=document.AddShape(restored,"six batch part");var assembly=document.AddAssembly("six batch assembly");
            using var identity=TopLocLocation.Identity;var component=document.AddComponent(assembly,part,identity);
            document.Projections.ReplaceNamedData(document.Projections.GetLabel(component.Entry),new(integers:new Dictionary<string,int>{{"round",6}}));
            Require(transaction.Commit(),"AS transaction");string xbf=Path.Combine(directory,"model.xbf");document.Save(xbf);using var reopened=XdeDocument.Open(xbf);
            Require(reopened.Projections.Snapshot(reopened.Projections.GetLabel(component.Entry)).NamedData!.Integers["round"]==6,"AS saved occurrence metadata");
            var occurrence=reopened.GetLabel(assembly.Entry).GetOccurrences().Single();
            using var window=new BatchAUVisibleWindow();using var viewer=OcctViewer.Create(window.Handle);window.Viewer=viewer;
            using var presentation=viewer.Display(occurrence);presentation.SetDisplayMode(ViewerDisplayMode.Shaded);viewer.SetProjection(ViewerProjection.Top);viewer.FitAll();viewer.Redraw();
            var hit=viewer.SelectionQueries.PickPoint(180,180);Require(hit.Hit&&ReferenceEquals(hit.Presentation,presentation)&&hit.SourceIdentity?.OccurrenceEntry==component.Entry,"AU exact XDE identity");
            // Validate actual HWND inputs and independent selection metadata before the
            // separately enforced desktop-pixel gate, so capture failures retain semantic evidence.
            window.Mouse(0x201,180,180,true);window.Mouse(0x202,180,180);window.Mouse(0x200,0,0);
            Require(viewer.GetSelection().Count==1,"actual HWND selection");
            var copied=viewer.GetSelectedItems();try{var item=copied.Single();var frozen=item.SourceIdentity;Require(item.SourceIdentity?.OccurrenceEntry==component.Entry,"copied selection identity");}finally{foreach(var item in copied)item.Dispose();}
            viewer.ClearSelection();viewer.Input.RectangleSelectionEnabled=true;
            window.Mouse(0x201,0,0,true);window.Mouse(0x200,359,359,true);window.Mouse(0x202,359,359);window.Mouse(0x200,0,0);
            Require(viewer.GetSelection().Count==1,"actual HWND rectangle input");viewer.ClearSelection();viewer.Input.RectangleSelectionEnabled=false;
            using(var revision=viewer.SelectionQueries.CreatePointVolume(180,180)){window.Resize(380,360);viewer.Resize();Require(!revision.IsCurrent,"resized client source stamp");window.Resize(360,360);viewer.Resize();}
            if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE") is {} semantic){Directory.CreateDirectory(semantic);File.WriteAllText(Path.Combine(semantic,"semantic-result.json"),JsonSerializer.Serialize(new{state="PASS",aqApArAtVolume=72,asSavedRound=6,exactOccurrence=component.Entry,hwndClick=true,hwndRectangle=true,frozenSelectionIdentity=true,resizeStale=true,visibleFrame="REQUIRED_SEPARATE_GATE"}));}
            viewer.ClearSelection();viewer.MoveTo(0,0);viewer.Redraw();if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE") is {} debug){Directory.CreateDirectory(debug);File.WriteAllText(Path.Combine(debug,"driver.json"),JsonSerializer.Serialize(viewer.SelectionQueries.Diagnostics()));}var before=window.Capture();
            window.Mouse(0x201,180,180,true);window.Mouse(0x202,180,180);window.Mouse(0x200,0,0);
            var selected=viewer.GetSelection();Require(selected.Count==1&&ReferenceEquals(selected[0],presentation),"actual HWND click selection");
            var after=window.Capture();int clickChanged=BatchAUVisibleWindow.Changed(before,after);if(clickChanged<=100){string fail=Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE")??directory;Directory.CreateDirectory(fail);BatchAUVisibleWindow.SavePng(Path.Combine(fail,"failed-before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(fail,"failed-after.png"),after);}Require(clickChanged>100,"visible screen repaint after click: "+clickChanged);
            var items=viewer.GetSelectedItems();try{Require(items.Single().SourceIdentity?.OccurrenceEntry==component.Entry,"copied selection source identity");}finally{foreach(var item in items)item.Dispose();}
            viewer.ClearSelection();window.Mouse(0x200,0,0);viewer.Redraw();var cleared=window.Capture();
            viewer.Input.RectangleSelectionEnabled=true;window.Mouse(0x201,0,0,true);window.Mouse(0x200,359,359,true);window.Mouse(0x202,359,359);window.Mouse(0x200,0,0);
            Require(viewer.GetSelection().Count==1,"actual HWND rectangle selection");var rectangle=window.Capture();int rectangleChanged=BatchAUVisibleWindow.Changed(cleared,rectangle);Require(rectangleChanged>100,"visible screen rectangle repaint");
            using var volume=viewer.SelectionQueries.CreatePointVolume(180,180);window.Resize(380,360);viewer.Resize();Require(!volume.IsCurrent,"client resize invalidates selection volume");window.Resize(360,360);viewer.Resize();
            string? evidence=Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_AU_EVIDENCE");if(!string.IsNullOrWhiteSpace(evidence)){Directory.CreateDirectory(evidence);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"before.png"),before);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"click.png"),after);BatchAUVisibleWindow.SavePng(Path.Combine(evidence,"rectangle.png"),rectangle);File.WriteAllText(Path.Combine(evidence,"visible-result.json"),JsonSerializer.Serialize(new{state="PASS",capture="DXGI composited desktop pixels at actual client rectangle",input="SendMessage -> WndProc -> ViewerInputController",clickChanged,rectangleChanged,occurrence=component.Entry,width=360,height=360}));}
#endif
            Require(typeof(SelectingVolumeSnapshot).Assembly.GetName().Name=="OcctSharp.Visualization","AU copied data module");
        }
        finally{foreach(var owner in owners)owner.Dispose();Directory.Delete(directory,true);}
    }
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException("AU: "+message);}
#if DIRECT_MODULE_CONSUMER
    static void RunRawViewer(string path)
    {
        using var window=new BatchAUVisibleWindow();nint viewer=0,shape=0,volume=0;
        try{Require(Create(window.Handle,out viewer)==0&&ReadStep(System.Text.Encoding.UTF8.GetBytes(path+"\0"),out shape)==0,"raw viewer/shape create");Require(Display(viewer,shape,out long id)==0&&Projection(viewer,2)==0&&Fit(viewer)==0,"raw AIS display");
            Require(Capture(viewer,0,new double[]{180,180},2,2,1,out volume)==0,"AU raw point volume");Require(Current(volume,viewer,out int current)==0&&current==1,"AU raw current");
            Require(Pick(viewer,0,new double[]{180,180},2,0,out int hit,new double[3],out long picked)==0&&hit==1&&picked==id,"AU raw exact presentation pick");
            Require(Text(volume,0,null,0,out int length)==0&&length>0,"AU raw copied volume size");var bytes=new byte[length];Require(Text(volume,0,bytes,length,out _)==0,"AU raw copied volume bytes");using var json=JsonDocument.Parse(bytes);Require(json.RootElement.GetProperty("Vertices").GetArrayLength()==8,"AU raw eight copied vertices");
        }finally{if(volume!=0)Release(volume,0);if(viewer!=0)ReleaseViewer(viewer);if(shape!=0)ReleaseShape(shape);}
    }
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_create")]static extern int Create(nint hwnd,out nint viewer);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_read_step")]static extern int ReadStep(byte[] path,out nint shape);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_display_shape")]static extern int Display(nint viewer,nint shape,out long id);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_set_projection")]static extern int Projection(nint viewer,int projection);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_fit_all")]static extern int Fit(nint viewer);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_create")]static extern int Capture(nint viewer,int kind,double[] coords,int count,int tolerance,int overlap,out nint token);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_current")]static extern int Current(nint token,nint viewer,out int current);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_scene_pick")]static extern int Pick(nint viewer,int kind,double[] input,int count,int stick,out int hit,[Out]double[] point,out long id);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_text")]static extern int Text(nint token,int kind,[Out]byte[]? bytes,int capacity,out int size);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_selection_workspace_release")]static extern void Release(nint token,int kind);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_viewer_release")]static extern void ReleaseViewer(nint viewer);
    [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_release")]static extern void ReleaseShape(nint shape);
#endif
}
