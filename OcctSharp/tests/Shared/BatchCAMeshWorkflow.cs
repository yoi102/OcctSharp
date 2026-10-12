using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using OcctSharp.MeshingWorkspaces;
using OcctSharp.PolyWorkspaces;
using OcctSharp.MeshBoundaryDiagnostics;
namespace OcctSharp.Tests.Shared;

internal static class BatchCAMeshWorkflow
{
 static void Require(bool value,string message){if(!value)throw new InvalidOperationException("CA "+message);}
 static void Rejected(Action action){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Require(rejected,"invalid/stale edit accepted");}
 static double Volume(Shape shape)=>shape.InspectProperties(InspectionPropertyKind.Volume).Mass;
 static void ExactBox(Shape shape){Require(Math.Abs(Volume(shape)-24)<1e-5,"exact source volume changed");using var snapshot=RepairSnapshot.Create(shape);Require(snapshot.Topology.Count(t=>t.Kind==ShapeKind.Solid)==1,"exact solid lost");var b=shape.GetBoundingBox();Require(Math.Abs(b.Minimum.X)<1e-5&&Math.Abs(b.Maximum.X-4)<1e-5&&Math.Abs(b.Maximum.Y-3)<1e-5&&Math.Abs(b.Maximum.Z-2)<1e-5,"exact source bounds changed");}
 internal static void Run(string operation)
 {
  Require(operation is "Remesh" or "Patch","unknown operation");
  string root=Path.Combine(Path.GetTempPath(),"OcctSharp.CA."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  try {
   using var source=ShapeFactory.CreateBox(4,3,2);source.CreateDetailedMesh(.2,.5);
   var original=MeshTopology.SnapshotExisting(source).Mesh;
   string sourceStep=Path.Combine(root,"source.step");ShapeExchange.WriteStep(source,sourceStep);
   using var document=new SourceDocument(sourceStep);
   using var meshing=ControlledMeshing.Run(source,new(Deflection:.05));
   Require(meshing.IsDone&&meshing.HasCompleteTriangulation&&meshing.MissingFaces.Count==0,"controlled mesh incomplete");
   using var meshed=meshing.CopyShape();using var remeshed=MeshTopology.Remesh(meshed,[0],.025,.4);
   Rejected(()=>{using var bad=MeshTopology.Remesh(meshed,[0,0]);});
   Rejected(()=>{using var bad=MeshTopology.Remesh(meshed,[0],double.NaN);});
   var actual=MeshTopology.SnapshotExisting(remeshed).Mesh;
   Require(actual.Triangles.Count>0&&actual.UVs is not null,"remesh copied channels missing");
   ExactBox(source);ExactBox(remeshed);
   var unchanged=MeshTopology.SnapshotExisting(source).Mesh;
   Require(unchanged.Positions.SequenceEqual(original.Positions)&&unchanged.Triangles.SequenceEqual(original.Triangles),"source mesh cache mutated");
   var square=new AuthoredMesh([new(0,0,0),new(2,0,0),new(2,2,0),new(0,2,0)],
    [new(0,1,2)],Enumerable.Repeat(new MeshNormal(0,0,1),4),[new(0,0),new(1,0),new(1,1),new(0,1)]);
   var old=square.SelectTriangles([0]);
   var foreign=new AuthoredMesh(square.Positions,square.Triangles);
   Rejected(()=>MeshEditing.InsertPatch(square,[],foreign.SelectTriangles([0]),[new(0,1,2)]));
   Rejected(()=>MeshEditing.InsertPatch(square,[new(0,1,99)]));
   var mirrored=MeshEditing.Transform(square,new(-1,0,0,2,0,1,0,0,0,0,1,0));
   Require(mirrored.Mesh.Positions[0]==new GpPoint(2,0,0)&&mirrored.Mesh.Triangles[0]==square.Triangles[0].Reversed()&&mirrored.Mesh.UVs!.SequenceEqual(square.UVs!),"transform orientation/coordinates/UV changed");
   Rejected(()=>MeshEditing.Transform(square,AuthoredMeshTransform.Scale(0)));
   var edited=MeshEditing.InsertPatch(square,[new(0,2,3)],old,[new(0,1,2)]);
   Require(edited.Mesh.Triangles.Count==2&&edited.Mesh.Normals is null&&edited.Mesh.UVs!.SequenceEqual(square.UVs!),"patch connectivity/channel contract changed");
   Rejected(()=>MeshEditing.DeleteTriangles(edited.Mesh,old));
   Require(square.Triangles.Count==1&&square.Normals is not null&&square.Revision==old.Revision,"failed/edit path mutated source");
   var welded=MeshEditing.Weld(edited.Mesh);
   Require(welded.Mesh.Positions.SequenceEqual(edited.Mesh.Positions)&&welded.Mesh.UVs!.SequenceEqual(edited.Mesh.UVs!),"weld changed distinct vertices/UVs");
   using var poly=PolyDataWorkspaces.FromMesh(welded.Mesh,doublePrecision:false);
   Require(poly.Neighbors is not null&&poly.IncidentTriangles is not null,"coherent adjacency missing");
   using var triangulation=poly.CopyTriangulation();using var quality=MeshBoundaryComputations.Analyze(triangulation);
   var detached=quality.Diagnostic;using var independent=quality.CopyTriangulation();
   quality.Dispose();triangulation.Dispose();poly.Dispose();
   using var copied=PolyDataWorkspaces.Copy(independent);
   Require(copied.Mesh.Triangles.Count==2&&copied.Mesh.UVs is not null&&detached.DegenerateTriangles==0&&detached.BoundaryEdges==4,"detached copied quality/indices failed");
   var degenerate=new AuthoredMesh([new(0,0,0),new(1,0,0),new(2,0,0)],[new(0,1,2)]);
   Require(MeshEditing.RemoveDegenerate(degenerate).Mesh.Triangles.Count==0&&degenerate.Triangles.Count==1,"degeneration removal mutated/published wrong topology");
   using var openWire=ShapeFactory.CreatePolygonWire([new(0,0,0),new(2,0,0),new(2,2,0),new(0,2,0)],false);
   using var openFace=ShapeFactory.CreatePlanarFace(openWire);using var incomplete=ControlledMeshing.Run(openFace);
   Require(incomplete.IsDone&&!incomplete.HasCompleteTriangulation&&((int)incomplete.StatusFlags&(int)IMeshDataStatus.IMeshData_OpenWire)!=0,"done masked incomplete mesh status");
   bool rootRejected=false;try{using var invalid=incomplete.CopyShape();}catch(InvalidOperationException){rootRejected=true;}
   Require(rootRejected,"incomplete mesh published topology");
   document.VerifyAndPersist(root); // Held throughout native edit/remesh; verifies source document and actual XBF readback.
   using var discrete=MeshTopology.Create(copied.Mesh);using var retained=discrete.CopyShape();discrete.Dispose();
   Require(!MeshTopology.IsSurfaceBacked(retained)&&MeshTopology.SnapshotExisting(retained).Mesh.Triangles.Count==2,"discrete model lost owned result");
   string output=Path.Combine(root,operation=="Patch"?"edited.brep":"edited.step");
   if(operation=="Patch")ShapeExchange.WriteBrep(retained,output);else ShapeExchange.WriteStep(remeshed,output);
   using var reopened=operation=="Patch"?ShapeExchange.ReadBrep(output):ShapeExchange.ReadStep(output);
   if(operation=="Patch")Require(MeshTopology.SnapshotExisting(reopened).Mesh.Triangles.Count==2,"BREP mesh readback failed");else ExactBox(reopened);
   BatchCARawMeshViewer.Run(output,operation,operation=="Remesh"?Volume(reopened):0);
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CA_EVIDENCE") is {} evidence){
    string dest=Path.Combine(evidence,operation.ToLowerInvariant());Directory.CreateDirectory(dest);
    File.Copy(Path.Combine(root,"source.xbf"),Path.Combine(dest,"source.xbf"),true);
    File.WriteAllText(Path.Combine(dest,"workflow-result.json"),JsonSerializer.Serialize(new{state="PASS",operation,sourceGeometryPreserved=true,sourceMeshPreserved=true,sourceDocumentPreserved=true,xbfVolume=24,stepVolume=operation=="Remesh"?Volume(reopened):0,stepSolids=operation=="Remesh"?1:0,foreignRevisionRejected=true,invalidParameterRejected=true,failureAtomic=true,independentLifetime=true,optionalChannelsPreserved=true,orientationTransformPreserved=true,degenerationHandled=true,incompleteRootRejected=true,meshTriangles=2,boundaryEdges=4,qualityDetached=true}));
   }
  }finally{Directory.Delete(root,true);}
 }
 internal static void Run(){Run("Remesh");Run("Patch");}

 // Existing public C ABI also lets the facade-free consumer verify XBF storage.
 sealed class SourceDocument:IDisposable
 {
  nint document;
  static byte[] Utf8(string s)=>Encoding.UTF8.GetBytes(s+"\0");
  static void Check(int status){if(status!=0)throw new InvalidOperationException("CA XBF status "+status);}
  internal SourceDocument(string step){Check(ReadStep(Utf8(step),out document));}
  static void Verify(nint owner,string path){Check(Count(owner,out int count));Require(count==1,"document free-root count changed");byte[] entry=new byte[256];Check(Entry(owner,1,entry,entry.Length,out int written));Require(written>0,"document root entry absent");Check(CopyShape(owner,entry,out nint shape));try{Check(WriteStep(shape,Utf8(path)));}finally{ReleaseShape(shape);}using var result=ShapeExchange.ReadStep(path);ExactBox(result);}
  internal void VerifyAndPersist(string root){Verify(document,Path.Combine(root,"held.step"));string xbf=Path.Combine(root,"source.xbf");Check(Save(document,Utf8(xbf)));Check(Open(Utf8(xbf),out nint reopened));try{Verify(reopened,Path.Combine(root,"reopened.step"));}finally{Release(reopened);}}
  public void Dispose(){if(document!=0){Release(document);document=0;}}
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_read_step")]static extern int ReadStep(byte[] path,out nint document);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_save")]static extern int Save(nint document,byte[] path);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open")]static extern int Open(byte[] path,out nint document);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Release(nint document);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_count")]static extern int Count(nint document,out int count);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_free_shape_entry")]static extern int Entry(nint document,int index,[Out]byte[] buffer,int capacity,out int written);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_get_shape")]static extern int CopyShape(nint document,byte[] entry,out nint shape);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_write_step")]static extern int WriteStep(nint shape,byte[] path);
  [DllImport("OcctSharp.Native",EntryPoint="occtsharp_shape_release")]static extern void ReleaseShape(nint shape);
 }
}
