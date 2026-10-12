using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using OcctSharp.DocumentProjections;
using OcctSharp.DocumentAssociations;
namespace OcctSharp.Tests.Shared;

// Both clean consumers compile this workflow. Generated module APIs exercise the
// public projection contracts; the existing public C ABI supplies retained XCAF
// storage owners, which have no facade-free managed persistence projection yet.
internal static class BatchCBDocumentWorkflow
{
    static readonly int[] PersistentIntegers=[4,0,7];
    static readonly string[] PersistentStrings=["","数组🙂"];
    static readonly string?[] PersistentReferences=["0:1",null,"0:1"];
    static readonly int[] PersistentKinds=[0,2,5];
    internal static void Require(bool condition,string operation)
    { if(!condition)throw new InvalidOperationException("CB document workflow: "+operation); }
    static void Rejected(Action action)
    {try{action();}catch(ArgumentException){return;}catch(InvalidOperationException){return;}throw new InvalidOperationException("CB expected rejection.");}
    static NamedDataValues Data()=>new(
        integers:new Dictionary<string,int>{{"zero",0},{"value",17}},
        reals:new Dictionary<string,double>{{"real with spaces",.12345678901234567}},
        strings:new Dictionary<string,string>{{"empty key value",""},{"unicode key 空格","持久化🙂"}},
        bytes:new Dictionary<string,byte>{{"byte",255}},
        integerArrays:new Dictionary<string,NamedDataArray<int>?>{{"bounded",new(-3,[4,0,7])},{"empty",new(9,[])},{"null",null}},
        realArrays:new Dictionary<string,NamedDataArray<double>?>{{"bounded",new(7,[.12345678901234567,0])},{"empty",new(-5,[])},{"null",null}});
    static void VerifyData(NamedDataValues? value)
    {
        Require(value is not null,"missing NamedData");
        Require(DataBytes(value!).SequenceEqual(DataBytes(Data())),"exact copied six-category data, Unicode/space/empty/null/bounds/real bits");
    }
    internal static void RunModule()
    {
        Require(typeof(DocumentProjectionContext).Assembly.GetName().Name=="OcctSharp.Documents"
            &&typeof(DocumentAssociationContext).Assembly.GetName().Name=="OcctSharp.Xde","direct module assembly owners");
        using var doc=DocumentAssociationContext.CreateNativeDocument();using var context=DocumentProjectionContext.Bind(doc);
        using var associations=DocumentAssociationContext.Bind(context);var main=context.GetLabel("0:1");
        using var foreignDoc=DocumentAssociationContext.CreateNativeDocument();using var foreign=DocumentProjectionContext.Bind(foreignDoc);
        using var otherView=DocumentProjectionContext.Bind(doc);
        Rejected(()=>context.ReplaceNamedData(main,Data()));
        doc.NewCommand();var child=associations.NewChild(main);var target=associations.NewChild(main);
        context.ReplaceNamedData(child,Data());context.ReplaceCollection(child,DirectCollections.Array<int>(-7,[4,0,7]));
        context.ReplaceCollection(child,DirectCollections.Array<DocumentProjectionLabel?>(-5,[target,null,target]));
        associations.ReplaceReferences([new(child,target)]);
        Rejected(()=>context.ReplaceNamedData([new(child,new()),new(child,null)]));
        Rejected(()=>context.ReplaceCollection(child,DirectCollections.Array<DocumentProjectionLabel?>(1,[foreign.GetLabel("0:1")])));
        Rejected(()=>associations.ReplaceReferences([new(child,otherView.GetLabel("0:1"))]));
        VerifyData(context.Snapshot(child).NamedData);Require(doc.CommitCommand(),"module initial commit");
        var retained=context.Snapshot(child);var references=BatchBICollectionWorkflow.Read<DocumentProjectionLabel?>(context,child,DirectCollectionKind.ReferenceArray);
        doc.NewCommand();context.ReplaceNamedData(child,null);associations.ReplaceReferences([new(child,null)]);doc.AbortCommand();
        VerifyData(context.Snapshot(child).NamedData);Require(associations.Snapshot(child).ReferenceEntry==target.Entry,"abort reference restoration");
        doc.NewCommand();context.ReplaceNamedData(child,null);context.ReplaceCollection(child,DirectCollections.Array<int>(19,[]));
        Require(doc.CommitCommand()&&doc.Undo(),"module edit/undo");VerifyData(context.Snapshot(child).NamedData);
        Require(BatchBICollectionWorkflow.Read<int>(context,child,DirectCollectionKind.IntegerArray).LowerBound==-7,"undo exact bounds");
        Require(doc.Redo()&&context.Snapshot(child).NamedData is null,"module redo absent distinction");
        Require(BatchBICollectionWorkflow.Read<int>(context,child,DirectCollectionKind.IntegerArray).LowerBound==19,"redo empty bounds");
        Require(doc.Undo(),"module restore");context.Dispose();
        bool closed=false;try{context.Snapshot(child);}catch(ObjectDisposedException){closed=true;}Require(closed,"closed context rejection");
        using var fresh=DocumentProjectionContext.Bind(doc);Rejected(()=>fresh.Snapshot(child));doc.Dispose();
        closed=false;try{fresh.GetLabel(child.Entry);}catch(ObjectDisposedException){closed=true;}Require(closed,"closed parent rejection");
        VerifyData(retained.NamedData);Require(references.LowerBound==-5&&references.Values[0]?.Entry==target.Entry
            &&references.Values[1] is null&&references.Values[2]?.Entry==target.Entry,"detached reference ordinals");
    }
    internal static void Run(string format)
    {
        Require(format is "Binary" or "Xml","format input");RunModule();
        string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.CB."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            using(var box=ShapeFactory.CreateBox(2,3,4))ShapeExchange.WriteStep(box,Path.Combine(directory,"seed.step"));
            using var source=new PersistentDocument(Path.Combine(directory,"seed.step"));
            source.Begin();string label=source.NewChild();source.Named(label,Data());source.Direct(label,0,-7,PersistentIntegers);
            source.Direct(label,2,-3,PersistentStrings);source.Direct(label,5,-5,PersistentReferences);source.Reference(label,"0:1");source.Commit();
            source.Verify(label);byte[] named=source.NamedSnapshot(label);var arrays=PersistentKinds.Select(k=>source.DirectSnapshot(label,k)).ToArray();
            source.Begin();source.Named(label,null);source.Direct(label,0,19,Array.Empty<int>());source.Reference(label,"0:");source.Abort();source.Verify(label);
            source.Begin();source.Named(label,null);source.Direct(label,0,19,Array.Empty<int>());source.Commit();source.Undo();source.Verify(label);
            source.Redo();Require(!source.NamedSnapshot(label).SequenceEqual(named),"persistent redo changed data");source.Undo();source.Verify(label);
            source.Begin();Require(source.InvalidDuplicate(label)!=0,"duplicate payload accepted");source.Verify(label);source.Abort();
            string file=Path.Combine(directory,format=="Xml"?"document.xml":"document.xbf");source.Save(file,format=="Xml");
            if(format=="Xml")_ = System.Xml.Linq.XDocument.Load(file);
            using(var reopened=PersistentDocument.Open(file))
            {
                reopened.Verify(label);Require(reopened.NamedSnapshot(label).SequenceEqual(named),"independent six-category persistent payload");
                foreach(var pair in new[]{(Kind:0,Index:0),(Kind:2,Index:1),(Kind:5,Index:2)})
                    Require(reopened.DirectSnapshot(label,pair.Kind).SequenceEqual(arrays[pair.Index]),"exact direct persistence/null slots/Unicode/bounds/GUID");
                reopened.WriteStep(Path.Combine(directory,"reopened.step"));
            }
            using(var shape=ShapeExchange.ReadStep(Path.Combine(directory,"reopened.step")))
                Require(shape.IsValid&&Math.Abs(shape.InspectProperties(InspectionPropertyKind.Volume).Mass-24)<1e-6&&shape.GetBoundingBox().SizeX<2.0001,"actual persisted source geometry");
            source.WriteStep(Path.Combine(directory,"held.step"));
            using(var held=ShapeExchange.ReadStep(Path.Combine(directory,"held.step")))
                Require(held.IsValid&&Math.Abs(held.InspectProperties(InspectionPropertyKind.Volume).Mass-24)<1e-6,"source geometry retained after edit/failure/storage");
            BatchCBRawDocumentViewer.Run(Path.Combine(directory,"reopened.step"),format,24);
            if(Environment.GetEnvironmentVariable("OCCTSHARP_BATCH_CB_EVIDENCE") is {} evidence)
            {
                string dest=Path.Combine(evidence,format.ToLowerInvariant());Directory.CreateDirectory(dest);File.Copy(file,Path.Combine(dest,Path.GetFileName(file)),true);
                File.WriteAllText(Path.Combine(dest,"workflow-result.json"),JsonSerializer.Serialize(new{state="PASS",format,
                    moduleProjection=true,namedCategories=6,directFamilies=3,unicode=true,spaceKeys=true,nullEmptyBounds=true,exactRealBits=true,
                    transactionAbort=true,undoRedo=true,failureAtomic=true,foreignContextRejected=true,closedContextRejected=true,
                    parentLifetimeRejected=true,detachedData=true,referenceIdentity=true,applicationLocalDrivers=true,volume=24,sourceGeometryPreserved=true}));
            }
        }
        finally{Directory.Delete(directory,true);}
    }
    internal static void Run(){Run("Binary");Run("Xml");}
    static byte[] DataBytes(NamedDataValues data)
    {
        using var stream=new MemoryStream();using var writer=new BinaryWriter(stream,Encoding.UTF8,true);WriteData(writer,data);return stream.ToArray();
    }
    static void Text(BinaryWriter output,string value){byte[] bytes=Encoding.UTF8.GetBytes(value);output.Write(bytes.Length);output.Write(bytes);}
    static void WriteData(BinaryWriter output,NamedDataValues? data)
    {
        output.Write(data is null?0:1);if(data is null)return;
        void Map<T>(IReadOnlyDictionary<string,T> map,Action<T> value){output.Write(map.Count);foreach(var item in map.OrderBy(p=>p.Key,StringComparer.Ordinal)){Text(output,item.Key);value(item.Value);}}
        void Array<T>(NamedDataArray<T>? array,Action<T> value)where T:struct{output.Write(array?.LowerBound??1);output.Write(array?.Values.Count??-1);if(array is not null)foreach(var item in array.Values)value(item);}
        Map(data.Integers,output.Write);Map(data.Reals,output.Write);Map(data.Strings,v=>Text(output,v));Map(data.Bytes,v=>output.Write((int)v));
        Map(data.IntegerArrays,v=>Array(v,output.Write));Map(data.RealArrays,v=>Array(v,output.Write));
    }
    sealed class PersistentDocument:IDisposable
    {
        nint document;
        static byte[] Utf8(string value)=>Encoding.UTF8.GetBytes(value+"\0");
        static void Check(int status){if(status!=0)throw new InvalidOperationException("CB retained document ABI: "+status+" "+Marshal.PtrToStringUTF8(LastError()));}
        internal PersistentDocument(string seed){BatchBPRawSourceDocument.Register();Check(Read(Utf8(seed),out document));}
        PersistentDocument(nint token){document=token;}
        internal static PersistentDocument Open(string path){Check(OpenNative(Utf8(path),out nint token));return new(token);}
        internal void Begin()=>Check(BeginNative(document));internal void Abort()=>Check(AbortNative(document));
        internal void Commit(){Check(CommitNative(document,out int changed));Require(changed==1,"retained command commit");}
        internal void Undo(){Check(History(document,0,out int changed));Require(changed==1,"retained undo");}
        internal void Redo(){Check(History(document,1,out int changed));Require(changed==1,"retained redo");}
        internal string NewChild(){Check(Child(document,Utf8("0:1"),out int tag));return "0:1:"+tag;}
        internal void Reference(string entry,string target)=>Check(ReferenceNative(document,Utf8(entry),Utf8(target)));
        internal void Save(string file,bool xml)=>Check(SaveNative(document,Utf8(file),1,xml?1:0));
        internal void WriteStep(string path)=>Check(Write(document,Utf8(path)));
        static byte[] Envelope(Action<BinaryWriter> content){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream,Encoding.UTF8,true);writer.Write(1);writer.Write(1);content(writer);return stream.ToArray();}
        internal void Named(string entry,NamedDataValues? value){byte[] data=Envelope(w=>{w.Write(1);Text(w,entry);WriteData(w,value);});Check(NamedNative(document,1,data,data.Length));}
        internal int InvalidDuplicate(string entry){byte[] data=Envelope(w=>{w.Write(2);for(int i=0;i<2;i++){Text(w,entry);WriteData(w,null);}});return NamedNative(document,1,data,data.Length);}
        internal void Direct<T>(string entry,int kind,int lower,T[] values)
        {
            byte[] data=Envelope(w=>{w.Write(61);w.Write(1);Text(w,entry);w.Write(kind);Text(w,"");w.Write(1);w.Write(lower);w.Write(values.Length);
                foreach(T item in values){if(item is int n)w.Write(n);else Text(w,item?.ToString()??"");}});
            Check(DirectNative(document,1,data,data.Length));
        }
        static byte[] Payload(nint token){try{Check(Size(token,1,out int count));Require(count is >=8 and <=16777216,"payload size");byte[] data=new byte[count];Check(Copy(token,1,data,count));return data;}finally{ReleasePayload(token,1);}}
        internal byte[] DirectSnapshot(string entry,int kind){Check(DirectRead(document,1,Utf8(entry),kind,Utf8(""),out nint token));return Payload(token);}
        internal byte[] NamedSnapshot(string entry)
        {
            Check(Snapshot(document,1,Utf8(entry),out nint token));byte[] data=Payload(token);using var stream=new MemoryStream(data);using var reader=new BinaryReader(stream);
            Require(reader.ReadInt32()==1&&reader.ReadInt32()==1,"snapshot header");int text=reader.ReadInt32();stream.Position+=text;
            int tags=reader.ReadInt32();stream.Position+=tags*4+12;return data[(int)stream.Position..];
        }
        internal void Verify(string entry)
        {
            Require(NamedSnapshot(entry).SequenceEqual(DataBytes(Data())),"retained exact NamedData values");
            Check(ReferenceEntry(document,Utf8(entry),0,1,new byte[256],256,out int written));Require(written==3,"retained reference to main");
            byte[] integers=DirectSnapshot(entry,0);using var stream=new MemoryStream(integers);using var reader=new BinaryReader(stream);
            Require(reader.ReadInt32()==1&&reader.ReadInt32()==1&&reader.ReadInt32()==60&&reader.ReadInt32()==0,"direct integer header");
            Require(reader.ReadInt32()==1,"direct integer presence");int size=reader.ReadInt32();stream.Position+=size;
            Require(reader.ReadInt32()==-7&&reader.ReadInt32()==3&&reader.ReadInt32()==4&&reader.ReadInt32()==0&&reader.ReadInt32()==7,"direct integer exact bounds/contents");
        }
        public void Dispose(){if(document!=0){Close(document);document=0;}}
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_get_last_error")]static extern nint LastError();
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_read_step")]static extern int Read(byte[] path,out nint doc);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_open")]static extern int OpenNative(byte[] path,out nint doc);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_release")]static extern void Close(nint doc);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_begin_command")]static extern int BeginNative(nint doc);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_abort_command")]static extern int AbortNative(nint doc);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_document_commit_command")]static extern int CommitNative(nint doc,out int changed);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_history_action")]static extern int History(nint doc,int action,out int changed);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_ocaf_label_add_child")]static extern int Child(nint doc,byte[] entry,out int tag);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_reference_set")]static extern int ReferenceNative(nint doc,byte[] entry,byte[] target);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_reference_entry")]static extern int ReferenceEntry(nint doc,byte[] entry,int array,int index,[Out]byte[] data,int capacity,out int written);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_save_format")]static extern int SaveNative(nint doc,byte[] path,int xde,int xml);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_xde_document_write_step")]static extern int Write(nint doc,byte[] path);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_replace")]static extern int NamedNative(nint doc,int domain,byte[] data,int size);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_replace")]static extern int DirectNative(nint doc,int domain,byte[] data,int size);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_snapshot")]static extern int Snapshot(nint doc,int domain,byte[] entry,out nint token);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_direct_collection_snapshot")]static extern int DirectRead(nint doc,int domain,byte[] entry,int kind,byte[] guid,out nint token);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_size")]static extern int Size(nint token,int kind,out int size);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_copy")]static extern int Copy(nint token,int kind,[Out]byte[] data,int size);
        [DllImport("OcctSharp.Native",EntryPoint="occtsharp_document_projection_payload_release")]static extern void ReleasePayload(nint token,int kind);
    }
}
