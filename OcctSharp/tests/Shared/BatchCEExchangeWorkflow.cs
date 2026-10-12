using System.Security.Cryptography;
using System.Text.Json;
using OcctSharp.ConfiguredMeshExchange;
using OcctSharp.DocumentProjections;
using OcctSharp.XdeMetadataWorkspaces;

namespace OcctSharp.Tests.Shared;

internal static class BatchCEExchangeWorkflow
{
 static readonly JsonSerializerOptions ReportOptions=new(){WriteIndented=true};
 internal static readonly string Fixture=Path.Combine(AppContext.BaseDirectory,"Fixtures","OwnedMeshResources");
 internal static string CopyFixture()
 {
  string root=Path.Combine(Path.GetTempPath(),"OcctSharp.CE."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  foreach(string file in Directory.GetFiles(Fixture,"*",SearchOption.AllDirectories)){string target=Path.Combine(root,Path.GetRelativePath(Fixture,file));Directory.CreateDirectory(Path.GetDirectoryName(target)!);File.Copy(file,target);}
  return root;
 }
 static void Require(bool value,string message){if(!value)throw new InvalidOperationException("CE: "+message);}
 internal static void Resources(string main,ConfiguredMeshFormat format)
 {
  string root=CopyFixture();
  try
  {
   string path=Path.Combine(root,main);var coordinates=new MeshCoordinateConfiguration(1,MeshUpAxis.Y,1,MeshUpAxis.Y);
   using var read=ConfiguredMeshFiles.ReadResources(path,format,coordinates);
   Require(read.Diagnostics.AppliedCoordinates==coordinates&&read.Diagnostics.NativeExtraStatus==0,"complete coordinate/read diagnostics");
   Require(read.Diagnostics.Files.Any(x=>x.FileName.Contains('/'))&&read.Diagnostics.Files.All(x=>x.Sha256.Length==64),"nested exact resource identities");
   using var alias=read.Document.Clone();using var context=DocumentProjectionContext.Bind(alias);using var metadata=XdeMetadataContext.Bind(context);
   var materials=metadata.Snapshot().Materials;Require(materials.Count>0,"imported visual material");
   string material=materials.Single().Entry;
   var slot=format==ConfiguredMeshFormat.Obj?MeshTextureSlot.CommonDiffuse:MeshTextureSlot.BaseColor;
   var snapshot=ConfiguredMeshFiles.ReadTexture(context,context.GetLabel(material),slot);Require(snapshot is not null,"native material-owned texture");
   using var copy=read.Shape.Transformed(ShapeTransform.CreateTranslation(0,0,0));
   Directory.Delete(root,true);read.Dispose();
   // The document alias owns the material graph, independently of result/staging.
   var after=ConfiguredMeshFiles.ReadTexture(context,context.GetLabel(material),slot);Require(after is not null,"texture after source and result disposal");
   Require(after!.Width==2&&after.Height==2&&after.EncodedPng.SequenceEqual(snapshot!.EncodedPng),"exact native encoded PNG bytes");
   using var identity=JsonDocument.Parse(File.ReadAllText(Path.Combine(Fixture,"fixture-identity.json")));
   string hash=Convert.ToHexString(SHA256.HashData(after.EncodedPng.ToArray()));Require(hash==identity.RootElement.GetProperty("pngSha256").GetString(),"encoded PNG source hash");
   double[][] expected=[[1,0,0],[0,1,0],[0,0,1],[1,1,1]];
   for(int n=0;n<4;++n){var p=after.Corners[n];Require(Math.Abs(p.Red-expected[n][0])<1e-6&&Math.Abs(p.Green-expected[n][1])<1e-6&&Math.Abs(p.Blue-expected[n][2])<1e-6&&Math.Abs(p.Alpha-1)<1e-6,"decoded native color corner "+n);}
   alias.Dispose();Require(MeshTopology.SnapshotExisting(copy).Mesh.Triangles.Count==2,"independent shape after all document owners released");
   Require(after.EncodedPng.Count>33&&after.Corners.Count==4,"detached texture snapshot after parent release");
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CE_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,Path.GetFileNameWithoutExtension(main)+"-"+format+".json"),JsonSerializer.Serialize(new{state="PASS",main,format,hash,sourceDeleted=true,resultDisposed=true,documentAliasSurvived=true,shapeSurvived=true,texture=after,resources=read.Diagnostics.Files},ReportOptions));}
  }
  finally{if(Directory.Exists(root))Directory.Delete(root,true);}
 }
 internal static void Formats()
 {
  string root=Path.Combine(Path.GetTempPath(),"OcctSharp.CE.格式."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  try
  {
#if CE_DIRECT_MODULE
   using var box=ShapeFactory.CreateBox(2,3,4);string iges=Path.Combine(root,"solid.iges");ShapeExchange.WriteIges(box,iges);using var restored=ShapeExchange.ReadIges(iges);File.Delete(iges);box.Dispose();
   Require(restored.IsValid&&restored.FaceCount==6&&Math.Abs(restored.InspectProperties(InspectionPropertyKind.Volume).Mass-24)<1e-5,"direct module BRep-mode IGES solid geometry");
   string stl=Path.Combine(root,"离散.stl");ShapeExchange.WriteStl(restored,stl);using var discrete=ShapeExchange.ReadStl(stl);File.Delete(stl);restored.Dispose();_=discrete.CreateDetailedMesh(.01,.5);
   var mesh=MeshTopology.SnapshotExisting(discrete).Mesh;Require(mesh.Triangles.Count==12,"direct module STL independent facets");
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CE_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"format-chain.json"),JsonSerializer.Serialize(new{state="PASS",sourceVolume=24,igesVolume=24,igesFaces=6,profile="geometry-only BRep-mode IGES; no facade metadata API",stlTriangles=mesh.Triangles.Count,sourceDeleted=true,sourceOwnersDisposed=true},ReportOptions));}
#else
   using var box=ShapeFactory.CreateBox(2,3,4);using var document=XdeDocument.Create();
   using(var command=document.BeginTransaction()){var label=document.AddShape(box,"CE part");label.Color=new(.2,.4,.8,1);label.SetLayer("CE layer");Require(command.Commit(),"IGES source command");}
   string iges=Path.Combine(root,"实体.iges");document.WriteIges(iges);
   using var imported=XdeDocument.ReadIges(iges,new(),out var report);var first=imported.GetFreeShapes().Single();
   using var restored=first.Shape;
   // The existing pinned default IGES writer emits surfaces, losing solid topology.
   // Verify every retained geometric measure and assert this loss explicitly.
   var faces=restored.GetFaces();double area;
   try{area=faces.Sum(x=>x.InspectProperties(InspectionPropertyKind.Area).Mass);}finally{foreach(var face in faces)face.Dispose();}
   Require(restored.IsValid&&restored.FaceCount==6&&Math.Abs(area-52)<1e-5&&restored.InspectProperties(InspectionPropertyKind.Volume).Mass==0,"IGES six exact surfaces/area and explicit solid-topology loss");
   Require(report.SourceEntityCount>0&&report.TransferredRootCount==1&&Math.Abs(report.SourceLengthUnitMeters-.001)<1e-12&&report.SystemLengthUnitMillimeters==1,"private IGES exact model/root/unit diagnostics in actual meters");
   Require(first.Name?.Contains("CE part",StringComparison.Ordinal)==true,"IGES supported ASCII name");
   var styles=first.GetPresentationStyles();try{Require(styles.Any(x=>x.EffectiveColor is not null),"IGES supported color");}finally{foreach(var style in styles)style.Dispose();}
   using(var snapshot=imported.CreateSnapshot())Require(snapshot.Labels.SelectMany(x=>imported.GetLabel(x.Entry).Layers).Any(),"IGES supported layer");
   File.Delete(iges);document.Dispose();
   string stl=Path.Combine(root,"离散.stl");ShapeExchange.WriteStl(restored,stl);using var discrete=ShapeExchange.ReadStl(stl);File.Delete(stl);imported.Dispose();
   _=discrete.CreateDetailedMesh(.01,.5);var mesh=MeshTopology.SnapshotExisting(discrete).Mesh;Require(mesh.Triangles.Count==12,"STL bounded independent geometry");
   Require(Math.Abs(mesh.Positions.Max(p=>p.X)-mesh.Positions.Min(p=>p.X)-2)<1e-5&&Math.Abs(mesh.Positions.Max(p=>p.Y)-mesh.Positions.Min(p=>p.Y)-3)<1e-5&&Math.Abs(mesh.Positions.Max(p=>p.Z)-mesh.Positions.Min(p=>p.Z)-4)<1e-5,"STL actual dimension fidelity");
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CE_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"format-chain.json"),JsonSerializer.Serialize(new{state="PASS",sourceVolume=24,igesVolume=0,igesArea=52,igesFaces=6,igesSolidTopology="lost by existing default surface-mode writer",igesName=true,igesColor=true,igesLayer=true,report,stlTriangles=mesh.Triangles.Count,sourceDeleted=true,sourceOwnersDisposed=true,stlMetadata="not carried by STL format",scope="exact fixture; external CAD NOT RUN"},ReportOptions));}
#endif
  }
  finally{Directory.Delete(root,true);}
 }
 internal static void Run(){Resources("嵌套.gltf",ConfiguredMeshFormat.Gltf);Resources("嵌入.gltf",ConfiguredMeshFormat.Gltf);Resources("嵌套.obj",ConfiguredMeshFormat.Obj);Formats();}
}
