using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using OcctSharp.StepCafWorkspaces;
using OcctSharp.DocumentProjections;
using OcctSharp.XdeMetadataWorkspaces;
using OcctSharp.XdeAttributeProjections;
namespace OcctSharp.Tests.Shared;

internal static class BatchCCLocatedAssemblyWorkflow
{
 internal sealed record Written(string Directory,double Unit,bool Declared,IReadOnlyList<string> Selected,IReadOnlyList<(string Name,string Source)> Files);
 static void Require(bool ok,string detail){if(!ok)throw new InvalidOperationException("CC located assembly: "+detail);}
 static void Check(int status){if(status!=0)throw new InvalidOperationException("CC native: "+status+": "+Marshal.PtrToStringUTF8(LastError()));}
 static byte[] Utf8(string text)=>Encoding.UTF8.GetBytes(text+"\0");
 static void Text(BinaryWriter writer,string text){byte[] bytes=Encoding.UTF8.GetBytes(text);writer.Write(bytes.Length);writer.Write(bytes);}
 static string Text(BinaryReader reader){int count=reader.ReadInt32();Require(count>=0&&count<=4096,"bounded text");byte[] bytes=reader.ReadBytes(count);Require(bytes.Length==count,"complete text");return new UTF8Encoding(false,true).GetString(bytes);}
 static byte[] Payload(nint token){Check(Size(token,2,out int count));Require(count is >=0 and <=1048576,"payload budget");byte[] data=new byte[count];Check(Copy(token,2,data,count));return data;}
 static byte[] Selection(string[] roots){using var data=new MemoryStream();using(var writer=new BinaryWriter(data,Encoding.UTF8,true)){writer.Write(1);writer.Write(2);writer.Write(81);writer.Write(roots.Length);foreach(string root in roots)Text(writer,root);}return data.ToArray();}
 internal static Written RawExport(string source,string[] roots,string output)
 {
  BatchBPRawSourceDocument.Register();nint doc=0,token=0,payload=0;
  try
  {
   Check(Open(Utf8(source),out doc));byte[] selection=Selection(roots);
   Check(Prepare(doc,1,selection,selection.Length,4,7,1e-4,out token,out payload));
   using var reader=new BinaryReader(new MemoryStream(Payload(payload)),Encoding.UTF8);
   Require(reader.ReadInt32()==1&&reader.ReadInt32()==2&&reader.ReadInt32()==80&&reader.ReadInt32()==4&&reader.ReadInt32()==7&&reader.ReadDouble()==1e-4,"exact private settings");
   double unit=reader.ReadDouble();bool declared=reader.ReadInt32()==1;int count=reader.ReadInt32();Require(count==roots.Length,"selected roots");var selected=new List<string>();
   for(int i=0;i<count;++i){string entry=Text(reader);_=Text(reader);Require(entry==roots[i],"exact source entry");selected.Add(entry);}
   count=reader.ReadInt32();Require(count==3,"canonical file set");var files=new List<(string Name,string Source)>();for(int i=0;i<count;++i)files.Add((Text(reader),Text(reader)));Require(reader.BaseStream.Position==reader.BaseStream.Length,"no trailing diagnostic payload");
   Close(doc);doc=0;Directory.CreateDirectory(output);Check(Write(token,Utf8(output)));Require(Write(token,Utf8(output))!=0,"writer consumed epoch rejection");
   return new(output,unit,declared,selected.AsReadOnly(),files.AsReadOnly());
  }finally{if(payload!=0)ReleasePayload(payload,2);if(token!=0)ReleaseExport(token);if(doc!=0)Close(doc);}
 }
 internal static void Run(Func<string,string[],string,Written>? export=null)
 {
  export??=RawExport;
  string fixture=Path.Combine(AppContext.BaseDirectory,"Fixtures","LocatedAssembly");
  using var oracle=JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture,"oracle.json")));var expected=oracle.RootElement.GetProperty("leaves").EnumerateArray().Select(x=>(Bounds:x.GetProperty("bounds").EnumerateArray().Select(v=>v.GetDouble()).ToArray(),Volume:x.GetProperty("volume").GetDouble())).OrderBy(x=>x.Bounds[0]).ThenBy(x=>x.Bounds[1]).ToArray();
  string[] roots=oracle.RootElement.GetProperty("roots").EnumerateArray().Select(x=>x.GetString()!).ToArray();Require(expected.Length==5&&roots.Length==2,"independent analytic oracle");
  string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.CC."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
  try
  {
   string source=Path.Combine(directory,"原始 装配.xbf");File.Copy(Path.Combine(fixture,"located.xbf"),source);string digest=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)));
   var written=export(source,roots,Path.Combine(directory,"共享 多层 输出🙂"));Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)))==digest,"source file unchanged");File.Delete(source);
   Require(written.Unit==.001&&written.Declared&&written.Selected.SequenceEqual(roots),"exact source units/selection");Require(written.Files.Count==3&&written.Files[0].Name=="assembly.step"&&written.Files.Skip(1).All(f=>f.Name.StartsWith("part-",StringComparison.Ordinal)&&f.Source.Length>0),"canonical metadata identity map");
   var files=written.Files.Select(f=>f.Name).ToArray();var manifest=LocalStepAssemblyWorkspace.CaptureManifest(written.Directory,files);
   using var workspace=LocalStepAssemblyWorkspace.Open(written.Directory,"assembly.step",manifest);Require(workspace.Roots.Count==2,"actual exported roots");
   using var imported=workspace.Transfer(workspace.Roots.Select(r=>r.Id));Require(imported.Completed&&imported.Document is not null&&imported.FreeRoots.Count==2,"actual independent selected multi-file transfer");
   bool consumed=false;try{workspace.Transfer([workspace.Roots[0].Id]);}catch(InvalidOperationException){consumed=true;}Require(consumed,"readback epoch consumed");workspace.Dispose();
   Require(imported.Occurrences.Count==7&&imported.Occurrences.Select(o=>o.Entry).Distinct(StringComparer.Ordinal).Count()==5,"shared nested component identity");Require(imported.Files.Skip(1).All(f=>f.Loaded&&f.LoadStatus==1&&f.NativeTransferSucceeded==true),"complete external status");
   using var context=DocumentProjectionContext.Bind(imported.Document!);using var metadata=XdeMetadataContext.Bind(context);using var layers=XdeAttributeProjectionContext.Bind(context);
   var shared=imported.Occurrences.GroupBy(x=>x.DefinitionEntry).First(g=>g.Count()==3).Key;var target=metadata.Snapshot([context.GetLabel(shared)]).Targets.Single();var color=target.General??target.Surface;Require(color is not null&&Math.Abs(color.Red-.2)<1e-6&&Math.Abs(color.Green-.3)<1e-6&&Math.Abs(color.Blue-.4)<1e-6,"exact shared definition color");Require(layers.Snapshot(context.GetLabel(shared)).Layers.Any(l=>l.Name=="shared-layer"),"shared definition layer");
   var solids=new List<Shape>();var rootShapes=new List<Shape>();
   try
   {
    for(int i=1;i<=2;++i){var shape=imported.CopyFreeRoot(i);rootShapes.Add(shape);Require(shape.IsValid,"root validity");solids.AddRange(shape.GetSubShapes(ShapeKind.Solid));}
    Require(solids.Count==5,"all cumulative leaf solids");var measured=solids.Select(s=>{var b=s.GetBoundingBox();return(Bounds:new[]{b.Minimum.X,b.Minimum.Y,b.Minimum.Z,b.Maximum.X,b.Maximum.Y,b.Maximum.Z},Volume:s.InspectProperties(InspectionPropertyKind.Volume).Mass);}).OrderBy(x=>x.Bounds[0]).ThenBy(x=>x.Bounds[1]).ToArray();
    for(int i=0;i<5;++i){Require(Math.Abs(measured[i].Volume-expected[i].Volume)<1e-5,"leaf volume");for(int j=0;j<6;++j)Require(Math.Abs(measured[i].Bounds[j]-expected[i].Bounds[j])<1e-5,"noncommuting cumulative bounds: "+i+":"+j+" actual="+measured[i].Bounds[j]+" expected="+expected[i].Bounds[j]);}
    Require(Math.Abs(measured.Sum(x=>x.Volume)-84)<1e-5,"complete geometry volume");imported.Dispose();Require(solids.All(s=>s.IsValid),"copied leaf lifetime after parent disposal");
    string viewerInput=Path.Combine(directory,"located-visible.step");ShapeExchange.WriteStep(solids[0],viewerInput);BatchCCRawAssemblyViewer.Run(viewerInput,solids[0].InspectProperties(InspectionPropertyKind.Volume).Mass);
    if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CC_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);foreach(string name in files)File.Copy(Path.Combine(written.Directory,name),Path.Combine(evidence,name),true);File.Copy(Path.Combine(fixture,"oracle.json"),Path.Combine(evidence,"oracle.json"),true);File.WriteAllText(Path.Combine(evidence,"workflow-result.json"),JsonSerializer.Serialize(new{state="PASS",roots=2,sourceUnchanged=true,sourceDeleted=true,readbackEpochRejected=consumed,exactNestedSharedComponents=7,distinctComponentLabels=5,sharedDefinitionColor=true,sharedDefinitionLayer=true,independentLeafLifetime=true,noncommutingBounds=true,leafVolumes=measured.Select(x=>x.Volume),leafBounds=measured.Select(x=>x.Bounds),sourceLengthUnit=written.Unit,leafCount=5,volume=measured.Sum(x=>x.Volume),canonicalFiles=files}));}
   }finally{foreach(var shape in solids)shape.Dispose();foreach(var shape in rootShapes)shape.Dispose();}
  }finally{Directory.Delete(directory,true);}
 }
 internal static void RunRejections()
 {
  BatchBPRawSourceDocument.Register();string fixture=Path.Combine(AppContext.BaseDirectory,"Fixtures","LocatedAssembly");using var oracle=JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture,"oracle.json")));string[] roots=oracle.RootElement.GetProperty("roots").EnumerateArray().Select(x=>x.GetString()!).ToArray();nint doc=0,token=0,payload=0;
  try
  {
   Check(Open(Utf8(Path.Combine(fixture,"located.xbf")),out doc));
   string Query(Func<byte[],int,(int Status,int Count)> action){byte[] bytes=new byte[4096];var result=action(bytes,bytes.Length);Check(result.Status);Require(result.Count>0&&result.Count<bytes.Length,"label query");return Encoding.UTF8.GetString(bytes,0,result.Count);}
   string component=Query((b,n)=>{int status=ComponentEntry(doc,Utf8(roots[0]),1,b,n,out int length);return(status,length);});
   string inner=Query((b,n)=>{int status=Referred(doc,Utf8(component),b,n,out int length);return(status,length);});
   void Reject(string[] entries){byte[] bytes=Selection(entries);int result=Prepare(doc,1,bytes,bytes.Length,4,7,1e-4,out token,out payload);Require(result!=0&&token==0&&payload==0,"rejection before publication");}
   Reject([]);Reject([roots[0],roots[0]]);Reject([component]);Reject([roots[0],inner]);
   Check(Begin(doc));Reject(roots);Check(Abort(doc));
   Close(doc);doc=0;using var budgets=JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture,"budget-oracle.json")));
   Require(budgets.RootElement.GetProperty("state").GetString()=="PASS","budget source oracle");
   foreach(var bad in budgets.RootElement.GetProperty("cases").EnumerateArray()){Check(Open(Utf8(Path.Combine(fixture,bad.GetProperty("file").GetString()!)),out doc));Reject([bad.GetProperty("root").GetString()!]);Close(doc);doc=0;}
   if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CC_EVIDENCE") is {} evidence){Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"rejection-result.json"),JsonSerializer.Serialize(new{state="PASS",empty=true,duplicate=true,occurrence=true,overlap=true,openCommand=true,noPartialOwner=true,completePathDepth=true,expandedOccurrences=true}));}
  }finally{if(payload!=0)ReleasePayload(payload,2);if(token!=0)ReleaseExport(token);if(doc!=0)Close(doc);}
 }
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_get_last_error")]static extern nint LastError();
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open")]static extern int Open(byte[] path,out nint document);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint document);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_begin_command")]static extern int Begin(nint document);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_abort_command")]static extern int Abort(nint document);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_component_entry")]static extern int ComponentEntry(nint document,byte[] entry,int ordinal,[Out]byte[] output,int capacity,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_label_referred_entry")]static extern int Referred(nint document,byte[] entry,[Out]byte[] output,int capacity,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_local_step_export_prepare")]static extern int Prepare(nint document,int domain,byte[] bytes,int count,int schema,int modes,double precision,out nint token,out nint snapshot);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_local_step_export_write")]static extern int Write(nint token,byte[] path);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_local_step_export_release")]static extern void ReleaseExport(nint token);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_size")]static extern int Size(nint token,int kind,out int count);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_copy")]static extern int Copy(nint token,int kind,[Out]byte[] data,int capacity);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_release")]static extern void ReleasePayload(nint token,int kind);
}
