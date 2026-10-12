using OcctSharp.SelectionWorkspaces;
using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class SelectionWorkspaceTests
{
    [Fact]
    public void PointFrustumCopiesAnalyticRayAndSixPrimitiveProbes()
    {
        using var w = new BatchWRenderingTests.Window(); var v = w.Viewer;
        using var box = ShapeFactory.CreateBox(10, 10, 10); using var p = v.Display(box);
        v.SetProjection(ViewerProjection.Top); v.FitAll();
        using var volume = v.SelectionQueries.CreatePointVolume(160, 160, 20);
        var s = volume.Snapshot; Assert.Equal(8, s.Vertices.Count); Assert.Equal(6, s.Planes.Count);
        Assert.Equal(320, s.Width); Assert.Equal(new GpPoint2d(160, 160), s.Mouse);
        var near = s.Near!.Value; var far = s.Far!.Value; var mid = volume.PointAtDepth(Distance(near, far) * .5);
        Assert.InRange(Distance(near, volume.PointAtDepth(0)), 0, 1e-7);
        Assert.InRange(Distance(far, volume.PointAtDepth(Distance(near, far))), 0, 1e-6);
        Assert.True(volume.ProbePoint(mid).Matched);
        Assert.False(volume.ProbePoint(new(1e8, 1e8, 1e8)).Matched);
        Assert.True(volume.ProbeBox(new(new(mid.X-.001,mid.Y-.001,mid.Z-.001),new(mid.X+.001,mid.Y+.001,mid.Z+.001))).Matched);
        Assert.True(volume.ProbeSphere(mid, .001).Matched);
        Assert.True(volume.ProbeSegment(new(mid.X-.001,mid.Y,mid.Z),new(mid.X+.001,mid.Y,mid.Z)).Matched);
        GpPoint[] triangle = [new(mid.X-.001,mid.Y-.001,mid.Z),new(mid.X+.001,mid.Y-.001,mid.Z),new(mid.X,mid.Y+.001,mid.Z)];
        Assert.True(volume.ProbeTriangle(triangle[0],triangle[1],triangle[2]).Matched);
        Assert.True(volume.ProbePolygon(triangle).Matched);
        Assert.True(double.IsFinite(volume.DistanceToGeometryCenter(mid)));
        foreach(var hit in new[]{volume.ProbePoint(mid),volume.ProbeSphere(mid,.001)})
        { if(hit.Depth is {} depth) Assert.True(double.IsFinite(depth)); if(hit.Point is {} point) Assert.True(double.IsFinite(point.X)); }
        volume.Dispose(); Assert.Equal(8,s.Vertices.Count);
        Assert.Throws<ObjectDisposedException>(()=>volume.ProbePoint(mid));
    }

    [Fact]
    public void BoxWindowAndCrossingSemanticsRejectPartialInclusion()
    {
        using var w=new BatchWRenderingTests.Window();var v=w.Viewer;
        using var box=ShapeFactory.CreateBox(10,10,10);using var p=v.Display(box);v.SetProjection(ViewerProjection.Top);v.FitAll();
        using var window=v.SelectionQueries.CreateBoxVolume(100,100,220,220);
        using var crossing=v.SelectionQueries.CreateBoxVolume(220,220,100,100,true);
        var center=window.Snapshot.Vertices.Aggregate(new GpPoint(0,0,0),(a,b)=>new(a.X+b.X/8,a.Y+b.Y/8,a.Z+b.Z/8));
        var large=new BoundingBox3d(new(center.X-100,center.Y-100,center.Z-.01),new(center.X+100,center.Y+100,center.Z+.01));
        var a=window.ProbeBox(large);var b=crossing.ProbeBox(large);
        Assert.False(a.Matched);Assert.False(a.Inside);Assert.True(b.Matched);Assert.False(b.Inside);
        Assert.Null(window.Snapshot.Near);Assert.Null(window.Snapshot.Mouse);
        Assert.Throws<ArgumentException>(()=>window.PointAtDepth(0));
    }

    [Fact]
    public void NonfiniteDegenerateOutOfRangeAndNonplanarInputsReject()
    {
        using var w=new BatchWRenderingTests.Window();var q=w.Viewer.SelectionQueries;
        Assert.Throws<ArgumentException>(()=>q.CreatePointVolume(-1,0));
        Assert.Throws<ArgumentException>(()=>q.CreatePointVolume(double.NaN,0));
        Assert.Throws<ArgumentException>(()=>q.CreatePointVolume(320,0));
        Assert.Throws<ArgumentException>(()=>q.CreatePointVolume(1,1,101));
        Assert.Throws<ArgumentException>(()=>q.CreateBoxVolume(1,1,1,3));
        using var p=q.CreatePointVolume(160,160);
        Assert.Throws<ArgumentException>(()=>p.PointAtDepth(-1));Assert.Throws<ArgumentException>(()=>p.PointAtDepth(double.MaxValue));
        Assert.Throws<ArgumentException>(()=>p.ProbePoint(new(double.NaN,0,0)));
        Assert.Throws<ArgumentException>(()=>p.ProbeSphere(new(0,0,0),0));
        Assert.Throws<ArgumentException>(()=>p.ProbeSegment(new(0,0,0),new(0,0,0)));
        Assert.Throws<ArgumentException>(()=>p.ProbeTriangle(new(0,0,0),new(1,0,0),new(2,0,0)));
        Assert.Throws<ArgumentException>(()=>p.ProbePolygon([new(0,0,0),new(1,0,0),new(0,1,0),new(1,1,1)]));
        Assert.Throws<ArgumentException>(()=>q.PickAxis(new(0,0,0),new(0,0,0)));
    }

    [Fact]
    public void CameraSceneTransformVisibilityModesFiltersAndRemovalInvalidate()
    {
        using var w=new BatchWRenderingTests.Window();var v=w.Viewer;
        using var box=ShapeFactory.CreateBox(10,10,10);using var p=v.Display(box);v.FitAll();
        void Changes(Action action){using var volume=v.SelectionQueries.CreatePointVolume(160,160);Assert.True(volume.IsCurrent);action();Assert.False(volume.IsCurrent);Assert.Throws<InvalidOperationException>(()=>volume.ProbePoint(new(0,0,0)));}
        Changes(()=>v.Zoom(1.1));
        using var tr=GpTrsf.Create(1,2,3);Changes(()=>p.SetTransform(tr));
        Changes(p.Hide);Changes(p.Show);Changes(()=>p.SetSelectionKind(ShapeKind.Face));
        Changes(()=>v.SetShapeFilter(ShapeKind.Face));Changes(v.ClearFilters);
        Changes(()=>{using var added=v.Display(box);});Changes(p.Dispose);
    }

    [Fact]
    public void WrongThreadKeepsInputGestureAndZeroWheelChecksLifetime()
    {
        using var w=new BatchWRenderingTests.Window();var v=w.Viewer;
        using var box=ShapeFactory.CreateBox(10,10,10);using var p=v.Display(box);v.SetProjection(ViewerProjection.Top);v.FitAll();
        v.Input.PointerPressed(ViewerPointerButton.Left,160,160);
        using var volume=v.SelectionQueries.CreatePointVolume(160,160);
        Exception? error=null;var thread=new Thread(()=>{try{v.Input.PointerReleased(ViewerPointerButton.Left,160,160);}catch(Exception e){error=e;}});thread.Start();thread.Join();Assert.IsType<InvalidOperationException>(error);
        Assert.Same(p,Assert.Single(v.Input.PointerReleased(ViewerPointerButton.Left,160,160)));
        foreach(Action action in new Action[]{()=>v.Input.MouseWheel(0,0,0),()=>volume.ProbePoint(new(0,0,0)),volume.Dispose,()=>v.SelectionQueries.Diagnostics()})
        {error=null;thread=new Thread(()=>{try{action();}catch(Exception e){error=e;}});thread.Start();thread.Join();Assert.IsType<InvalidOperationException>(error);}
        v.Dispose();Assert.Throws<ObjectDisposedException>(()=>v.Input.MouseWheel(0,0,0));Assert.Throws<ObjectDisposedException>(()=>volume.IsCurrent);
        volume.Dispose();volume.Dispose();Assert.Equal(8,volume.Snapshot.Vertices.Count);
    }

    [Fact]
    public void ScenePicksAndDiagnosticCopiesUseActualAis()
    {
        using var w=new BatchWRenderingTests.Window();var v=w.Viewer;
        using var box=ShapeFactory.CreateBox(10,10,10);using var p=v.Display(box);p.SetDisplayMode(ViewerDisplayMode.Shaded);v.SetProjection(ViewerProjection.Top);v.FitAll();
        var hit=v.SelectionQueries.PickPoint(160,160);Assert.True(hit.Hit);Assert.Same(p,hit.Presentation);Assert.InRange(hit.Point!.Value.Z,9.999,10.001);
        var axis=v.SelectionQueries.PickAxis(new(5,5,100),new(0,0,-1));Assert.True(axis.Hit);Assert.Same(p,axis.Presentation);
        Assert.False(v.SelectionQueries.PickPoint(0,0).Hit);
        Assert.True(double.IsFinite(v.SelectionQueries.GravityPoint().X));
        var diagnostic=v.SelectionQueries.Diagnostics();Assert.NotEmpty(diagnostic.Diagnostics);v.Dispose();Assert.NotEmpty(diagnostic.Diagnostics);
        Assert.Equal("OcctSharp.Visualization",typeof(SelectingVolumeSnapshot).Assembly.GetName().Name);
        Assert.DoesNotContain(typeof(SelectingVolumeSnapshot).Assembly.GetReferencedAssemblies(),a=>a.Name=="OcctSharp");
    }

    [Fact]
    public void SixBatchChainAndVisibleHwndSelectionRepaint()=>OcctSharp.Tests.Shared.BatchAUIntegratedWorkflow.Run();

    [Fact]
    public void SelectedSourceIdentityIsFrozenAcrossPresentationChanges()
    {
        using var w=new BatchWRenderingTests.Window();using var box=ShapeFactory.CreateBox(10,10,10);using var document=XdeDocument.Create();
        XdeLabel assembly,component;using(var transaction=document.BeginTransaction()){var part=document.AddShape(box,"part");assembly=document.AddAssembly("assembly");using var location=TopLocLocation.Identity;component=document.AddComponent(assembly,part,location);transaction.Commit();}
        using var p=w.Viewer.Display(assembly.GetOccurrences().Single());w.Viewer.SetProjection(ViewerProjection.Top);w.Viewer.FitAll();w.Viewer.SelectAt(160,160);
        var items=w.Viewer.GetSelectedItems();try{var item=Assert.Single(items);var identity=item.SourceIdentity;Assert.Equal(component.Entry,identity!.OccurrenceEntry);p.UpdateSourceIdentity("replacement");Assert.Same(identity,item.SourceIdentity);p.Dispose();document.Dispose();Assert.Equal(component.Entry,item.SourceIdentity!.OccurrenceEntry);Assert.True(item.Shape.IsValid);}finally{foreach(var item in items)item.Dispose();}
    }

    [Fact]
    public void RawVolumesRejectForeignParentsRetiredTokensAndShortBuffers()
    {
        using var raw=new RawSelection();using var a=new BatchWRenderingTests.Window();using var b=new BatchWRenderingTests.Window();
        Assert.Equal(0,raw.CreateRaw(a.Viewer.Handle.DangerousGetHandle(),0,[160,160],2,2,1,out var token));
        try
        {
            Assert.Equal(0,raw.CurrentRaw(token,b.Viewer.Handle.DangerousGetHandle(),out int current));Assert.Equal(0,current);
            Assert.Equal(0,raw.TextRaw(token,0,null,0,out int size));Assert.True(size>0);
            Assert.NotEqual(0,raw.TextRaw(token,0,new byte[1],1,out _));
            Assert.NotEqual(0,raw.TextRaw(token,1,null,0,out _));
            var bytes=new byte[size];Assert.Equal(0,raw.TextRaw(token,0,bytes,size,out int written));Assert.Equal(size,written);
            a.Viewer.Dispose();Assert.NotEqual(0,raw.CurrentRaw(token,a.Viewer.Handle.DangerousGetHandle(),out _));
        }
        finally{raw.ReleaseRaw(token,0);raw.ReleaseRaw(token,0);}
        Assert.NotEqual(0,raw.TextRaw(token,0,null,0,out _));
    }

    [Fact]
    public void RawCopiedMetadataLeasesSurviveConcurrentRetirement()
    {
        using var raw=new RawSelection();using var window=new BatchWRenderingTests.Window();
        for(int round=0;round<50;round++)
        {
            Assert.Equal(0,raw.CreateRaw(window.Viewer.Handle.DangerousGetHandle(),0,[160,160],2,2,1,out var token));
            using var start=new ManualResetEventSlim();
            Exception? error=null;var reader=new Thread(()=>{try{start.Wait();for(int i=0;i<40;i++){int result=raw.TextRaw(token,0,null,0,out int size);if(result==0)Assert.True(size>0);}}catch(Exception e){error=e;}});reader.Start();
            start.Set();raw.ReleaseRaw(token,0);reader.Join();Assert.Null(error);Assert.NotEqual(0,raw.TextRaw(token,0,null,0,out _));
        }
    }

    sealed class RawSelection:IDisposable
    {
        readonly nint library=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
        internal readonly Create CreateRaw;internal readonly Current CurrentRaw;internal readonly Text TextRaw;internal readonly Release ReleaseRaw;
        internal RawSelection(){CreateRaw=Marshal.GetDelegateForFunctionPointer<Create>(NativeLibrary.GetExport(library,"occtsharp_selection_workspace_create"));CurrentRaw=Marshal.GetDelegateForFunctionPointer<Current>(NativeLibrary.GetExport(library,"occtsharp_selection_workspace_current"));TextRaw=Marshal.GetDelegateForFunctionPointer<Text>(NativeLibrary.GetExport(library,"occtsharp_selection_workspace_text"));ReleaseRaw=Marshal.GetDelegateForFunctionPointer<Release>(NativeLibrary.GetExport(library,"occtsharp_selection_workspace_release"));}
        public void Dispose()=>NativeLibrary.Free(library);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]internal delegate int Create(nint viewer,int kind,double[] coords,int count,int tolerance,int overlap,out nint token);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]internal delegate int Current(nint token,nint viewer,out int current);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]internal delegate int Text(nint token,int kind,[Out]byte[]? output,int capacity,out int length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]internal delegate void Release(nint token,int kind);
    }

    static double Distance(GpPoint a,GpPoint b)=>Math.Sqrt(Math.Pow(a.X-b.X,2)+Math.Pow(a.Y-b.Y,2)+Math.Pow(a.Z-b.Z,2));
}
