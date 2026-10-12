using System.Text.Json.Nodes;
using OcctSharp.ConfiguredMeshExchange;
using OcctSharp.DocumentProjections;
using OcctSharp.Tests.Shared;

namespace OcctSharp.Runtime.Tests;
public sealed class OwnedMeshResourceTests
{
 [Fact]public void IgesAndStlChainPreservesExactGeometryAndReportsFormatMetadataLimit()=>BatchCEExchangeWorkflow.Formats();
 [Theory][InlineData("嵌套.gltf",ConfiguredMeshFormat.Gltf)][InlineData("嵌入.gltf",ConfiguredMeshFormat.Gltf)][InlineData("嵌套.obj",ConfiguredMeshFormat.Obj)]
 public void ActualNestedResourcesSurviveSourceAndResultDisposal(string main,ConfiguredMeshFormat format)=>BatchCEExchangeWorkflow.Resources(main,format);
 [Theory][InlineData("truncated")][InlineData("crc")][InlineData("missing")][InlineData("texture-index")][InlineData("image-index")][InlineData("image-range")][InlineData("escape")][InlineData("jpeg")][InlineData("dual-source")]
 public void InvalidResourcesCannotPublishOwnersOrLeaveStaging(string kind)
 {
  string root=BatchCEExchangeWorkflow.CopyFixture();var before=Directory.GetDirectories(Path.GetTempPath(),"OcctSharp.MeshExchange.*").ToHashSet(StringComparer.OrdinalIgnoreCase);
  try
  {
   string path=Path.Combine(root,"嵌套.gltf"),png=Path.Combine(root,"images","色块.png");var json=JsonNode.Parse(File.ReadAllText(path))!;
   switch(kind){case "truncated":File.WriteAllBytes(png,File.ReadAllBytes(png)[..^6]);break;case "crc":var bytes=File.ReadAllBytes(png);bytes[41]^=1;File.WriteAllBytes(png,bytes);break;case "missing":File.Delete(png);break;case "texture-index":json["materials"]![0]!["pbrMetallicRoughness"]!["baseColorTexture"]!["index"]=8;break;case "image-index":json["textures"]![0]!["source"]=8;break;case "image-range":json["images"]![0]!.AsObject().Remove("uri");json["images"]![0]!["bufferView"]=8;break;case "escape":json["images"]![0]!["uri"]="../outside.png";break;case "jpeg":json["images"]![0]!["mimeType"]="image/jpeg";break;case "dual-source":json["images"]![0]!["bufferView"]=0;break;}
   File.WriteAllText(path,json.ToJsonString());Assert.ThrowsAny<Exception>(()=>ConfiguredMeshFiles.ReadResources(path,ConfiguredMeshFormat.Gltf,new(1,MeshUpAxis.Y,1,MeshUpAxis.Y)));
   Assert.DoesNotContain(Directory.GetDirectories(Path.GetTempPath(),"OcctSharp.MeshExchange.*"),x=>!before.Contains(x));
  }
  finally{Directory.Delete(root,true);}
 }
 [Fact]public void LegacyFlatProfileStillRejectsNestedAndTexturedInput()
 {
  string root=BatchCEExchangeWorkflow.CopyFixture();try{Assert.ThrowsAny<Exception>(()=>ConfiguredMeshFiles.Read(Path.Combine(root,"嵌套.gltf"),ConfiguredMeshFormat.Gltf,new(1,MeshUpAxis.Y,1,MeshUpAxis.Y)));}finally{Directory.Delete(root,true);}
 }
 [Fact]public void TextureSnapshotRejectsForeignLabelsAndDisposedOwner()
 {
  using var a=DocumentProjectionContext.CreateNativeDocument();using var b=DocumentProjectionContext.CreateNativeDocument();using var first=DocumentProjectionContext.Bind(a);using var second=DocumentProjectionContext.Bind(b);
  Assert.Throws<ArgumentException>(()=>ConfiguredMeshFiles.ReadTexture(first,second.GetLabel("0:1"),MeshTextureSlot.BaseColor));
  var label=first.GetLabel("0:1");a.Dispose();Assert.Throws<ObjectDisposedException>(()=>ConfiguredMeshFiles.ReadTexture(first,label,MeshTextureSlot.BaseColor));
 }
 [Fact]public void TexturedDocumentCannotSilentlyUseLegacyTexturelessWriter()
 {
  string root=BatchCEExchangeWorkflow.CopyFixture();try{using var read=ConfiguredMeshFiles.ReadResources(Path.Combine(root,"嵌套.gltf"),ConfiguredMeshFormat.Gltf,new(1,MeshUpAxis.Y,1,MeshUpAxis.Y));using var context=DocumentProjectionContext.Bind(read.Document);string output=Path.Combine(root,"output.gltf");Assert.ThrowsAny<Exception>(()=>ConfiguredMeshFiles.WriteNew(context,output,ConfiguredMeshFormat.Gltf,new(1,MeshUpAxis.Y,1,MeshUpAxis.Y)));Assert.False(File.Exists(output));}finally{Directory.Delete(root,true);}
 }
 [Fact]public void ResourceBudgetRejectsOversizedSparseFileBeforeHashOrNativeAllocation()
 {
  string root=BatchCEExchangeWorkflow.CopyFixture();try{using(var file=new FileStream(Path.Combine(root,"parts","网格.bin"),FileMode.Open,FileAccess.Write))file.SetLength(67108865);Assert.Throws<System.IO.InvalidDataException>(()=>ConfiguredMeshFiles.ReadResources(Path.Combine(root,"嵌套.gltf"),ConfiguredMeshFormat.Gltf,new(1,MeshUpAxis.Y,1,MeshUpAxis.Y)));}finally{Directory.Delete(root,true);}
 }
 [Theory][InlineData(true)][InlineData(false)]public void UnicodeStlStagingPreservesExistingDestinationOnRejectedDeflection(bool binary)
 {
  string root=BatchCEExchangeWorkflow.CopyFixture();var before=Directory.GetFiles(Path.GetTempPath(),"occtsharp-exchange-*.tmp").ToHashSet(StringComparer.OrdinalIgnoreCase);
  try{string path=Path.Combine(root,"保留.stl");File.WriteAllText(path,"keep original");using var box=ShapeFactory.CreateBox(2,3,4);Assert.Throws<ArgumentException>(()=>ShapeExchange.WriteStl(box,path,new(-1,.5,binary)));Assert.Equal("keep original",File.ReadAllText(path));Assert.DoesNotContain(Directory.GetFiles(Path.GetTempPath(),"occtsharp-exchange-*.tmp"),x=>!before.Contains(x));}
  finally{Directory.Delete(root,true);}
 }
}
