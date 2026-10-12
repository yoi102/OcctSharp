using OcctSharp.BoundedText;
using OcctSharp.DocumentProjections;
using System.Runtime.InteropServices;
using System.Text;
namespace OcctSharp.Tests.Shared;
internal static class BatchBNBoundedTextWorkflow {
 internal static IReadOnlyList<string> Names()=>BoundedTextTransforms.ComputeBatch([
  new("装配",BoundedTextOperation.Append," 🚀"),
  new("齿轮/轴",BoundedTextOperation.ReplaceCharacter,Position:'/',Character:'·'),
  new(" 重复实例 ",BoundedTextOperation.Token," ",Position:1),
  new("部件",BoundedTextOperation.Prepend,"标准 ")
 ]).Select(x=>x.Text).ToArray();
 internal static void Run(){var names=Names();if(names[0]!="装配 🚀"||names[1]!="齿轮·轴"||names[2]!="重复实例"||names[3]!="标准 部件")throw new InvalidOperationException("BN bounded native Unicode transforms failed.");
#if DIRECT_MODULE_CONSUMER
  using var document=DocumentProjectionContext.CreateNativeDocument();using var context=DocumentProjectionContext.Bind(document);var label=context.GetLabel("0:1");document.NewCommand();context.ReplaceCollection(label,DirectCollections.List<string>(names));if(!document.CommitCommand())throw new InvalidOperationException("BN native Unicode document commit failed.");var stored=BatchBICollectionWorkflow.Read<string>(context,label,DirectCollectionKind.StringList);if(!stored.Values.SequenceEqual(names))throw new InvalidOperationException("BN native Unicode document copy failed.");
  RunModulePersistence(names);
#else
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BN."+Guid.NewGuid()+".xbf");try{using(var document=XdeDocument.Create()){var context=document.Projections;var label=context.GetLabel("0:1");using(var command=document.BeginTransaction("BN bounded Unicode document text")){context.ReplaceCollection(label,DirectCollections.List<string>(names));if(!command.Commit())throw new InvalidOperationException("BN Unicode document commit failed.");}document.Save(path,DocumentStorageFormat.BinXcaf);}
   using var restored=XdeDocument.Open(path);var reopened=restored.Projections;var stored=BatchBICollectionWorkflow.Read<string>(reopened,reopened.GetLabel("0:1"),DirectCollectionKind.StringList);if(!stored.Values.SequenceEqual(names))throw new InvalidOperationException("BN actual Unicode document save/reopen failed.");
  }finally{File.Delete(path);}
#endif
 }
#if DIRECT_MODULE_CONSUMER
 // Match the existing direct-module Viewer C ABI workflow: persistence belongs
 // to the native document domain and does not require the Facade assembly.
 static void RunModulePersistence(IReadOnlyList<string> names){
  typeof(OcctRuntime).Assembly.GetType("OcctSharp.Interop.NativeLibraryResolver",true)!.GetMethod("EnsureRegistered",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,[typeof(BatchBNBoundedTextWorkflow).Assembly]);
  string path=Path.Combine(Path.GetTempPath(),"OcctSharp.BN.module."+Guid.NewGuid()+".xbf");nint doc=0,reopened=0,payload=0;
  try{Check(Create(out doc));Check(Begin(doc));using var stream=new MemoryStream();using(var output=new BinaryWriter(stream,Encoding.UTF8,true)){output.Write(1);output.Write(1);output.Write(61);output.Write(1);Text(output,"0:1");output.Write((int)DirectCollectionKind.StringList);Text(output,"");output.Write(1);output.Write(0);output.Write(names.Count);foreach(string name in names)Text(output,name);}byte[] bytes=stream.ToArray();Check(Replace(doc,1,bytes,bytes.Length));Check(Commit(doc,out int changed));if(changed!=1)throw new InvalidOperationException("BN module persistent command had no change.");Check(Save(doc,Utf8(path),1,0));Close(doc);doc=0;Check(Open(Utf8(path),out reopened));Check(Snapshot(reopened,1,Utf8("0:1"),(int)DirectCollectionKind.StringList,Utf8(""),out payload));Check(Size(payload,1,out int length));if(length<20||length>1048576)throw new InvalidDataException("BN module persistent snapshot size.");bytes=new byte[length];Check(Copy(payload,1,bytes,bytes.Length));using var input=new BinaryReader(new MemoryStream(bytes),Encoding.UTF8);if(input.ReadInt32()!=1||input.ReadInt32()!=1||input.ReadInt32()!=60||input.ReadInt32()!=(int)DirectCollectionKind.StringList||input.ReadInt32()!=1)throw new InvalidDataException("BN module persistent snapshot envelope.");_=ReadText(input);if(input.ReadInt32()!=0||input.ReadInt32()!=names.Count)throw new InvalidDataException("BN module persistent list bounds.");foreach(string name in names)if(ReadText(input)!=name)throw new InvalidDataException("BN module XBF Unicode roundtrip.");if(input.BaseStream.Position!=input.BaseStream.Length)throw new InvalidDataException("BN module persistent extra data.");}
  finally{if(payload!=0)ReleasePayload(payload,1);if(reopened!=0)Close(reopened);if(doc!=0)Close(doc);File.Delete(path);}
 }
 static byte[] Utf8(string text)=>Encoding.UTF8.GetBytes(text+"\0");
 static void Text(BinaryWriter output,string text){byte[] bytes=Encoding.UTF8.GetBytes(text);output.Write(bytes.Length);output.Write(bytes);}
 static string ReadText(BinaryReader input){int length=input.ReadInt32();if(length<0||length>1048576||length>input.BaseStream.Length-input.BaseStream.Position)throw new InvalidDataException("BN text length.");return new UTF8Encoding(false,true).GetString(input.ReadBytes(length));}
 static void Check(int status){if(status!=0)throw new InvalidOperationException("BN module native persistence failed: "+status);}
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_create")]static extern int Create(out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_begin_command")]static extern int Begin(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_commit_command")]static extern int Commit(nint doc,out int changed);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_save_format")]static extern int Save(nint doc,byte[] path,int xde,int xml);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open")]static extern int Open(byte[] path,out nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint doc);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_replace")]static extern int Replace(nint doc,int domain,byte[] bytes,int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_snapshot")]static extern int Snapshot(nint doc,int domain,byte[] entry,int kind,byte[] id,out nint payload);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_size")]static extern int Size(nint payload,int kind,out int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_copy")]static extern int Copy(nint payload,int kind,[Out]byte[] bytes,int length);
 [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_release")]static extern void ReleasePayload(nint payload,int kind);
#endif
}
