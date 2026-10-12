#include "Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include <TDocStd_Document.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <TDF_Data.hxx>
#include <Message_Messenger.hxx>
#include <BinObjMgt_Persistent.hxx>
#include <BinObjMgt_RRelocationTable.hxx>
#include <XmlDrivers_DocumentRetrievalDriver.hxx>
#include <Standard_Failure.hxx>
#include <iostream>
#include <stdexcept>
#include <vector>
#include <string>
namespace D=OcctSharpGenerated::DocumentAdapters;
static void Require(bool v,const char* operation){if(!v)throw std::runtime_error(operation);}
#define M(owner,member,receiver,...) (G ? D::owner##_##member(receiver __VA_OPT__(,) __VA_ARGS__) : (receiver).member(__VA_ARGS__))
#define S(owner,member,...) (G ? D::owner##_##member(__VA_ARGS__) : owner::member(__VA_ARGS__))
template<bool G> static std::vector<std::string> Exercise(const TopoDS_Shape& shape)
{
  std::vector<std::string> snapshot;
  auto number=[&](auto value){snapshot.push_back(std::to_string(value));};
  occ::handle<TDocStd_Application> app=new TDocStd_Application;
  using Storage=std::conditional_t<G,D::PrivateXmlDriversDocumentStorageDriver,XmlDrivers_DocumentStorageDriver>;
  M(TDocStd_Application,DefineFormat,*app,TCollection_AsciiString("XmlOcaf"),TCollection_AsciiString("probe XML"),TCollection_AsciiString("xml"),
    new XmlDrivers_DocumentRetrievalDriver,new Storage(TCollection_ExtendedString("probe")));
  occ::handle<TDocStd_Document> doc;app->NewDocument(TCollection_ExtendedString("BinOcaf"),doc);doc->SetUndoLimit(10);
  auto root=doc->Main();doc->NewCommand();
  auto label=M(TDF_Label,FindChild,root,11,true),target=M(TDF_Label,FindChild,root,12,true);
  number(M(TDF_Label,Tag,label));number(M(TDF_Label,Depth,label));
  Require(M(TDF_Label,Father,label)==root&&M(TDF_Label,Root,label)==doc->GetData()->Root(),"parent/root identity");
  Require(!M(TDF_Label,IsRoot,label)&&!M(TDF_Label,IsNull,label)&&M(TDF_Label,IsDescendant,label,root),"label domain");
  auto name=S(TDataStd_Name,Set,label,TCollection_ExtendedString("unicode-name"));
  auto comment=S(TDataStd_Comment,Set,label,TCollection_ExtendedString("comment"));
  auto ascii=S(TDataStd_AsciiString,Set,label,TCollection_AsciiString("ascii"));
  auto integer=S(TDataStd_Integer,Set,label,17);auto real=S(TDataStd_Real,Set,label,2.5);
  number(integer->Get());number(real->Get());
  number(M(TDataStd_GenericExtString,Get,*name).Length());number(M(TDataStd_GenericExtString,Get,*comment).Length());
  snapshot.push_back(M(TDataStd_AsciiString,Get,*ascii).ToCString());
  auto ia=S(TDataStd_IntegerArray,Set,label,-3,-1,false);ia->SetValue(-3,42);
  auto ra=S(TDataStd_RealArray,Set,label,7,8,false);ra->SetValue(7,1.125);
  const Standard_GUID extId("d3a0c36e-7044-40d1-a2bb-c621a29a0c64");
  auto strings=S(TDataStd_ExtStringArray,Set,label,extId,-2,-1,false);
  M(TDataStd_ExtStringArray,SetValue,*strings,-2,TCollection_ExtendedString(""));
  M(TDataStd_ExtStringArray,SetValue,*strings,-1,TCollection_ExtendedString("kept"));
  number(M(TDataStd_ExtStringArray,Value,*strings,-2).Length());number(M(TDataStd_ExtStringArray,Value,*strings,-1).Length());
  auto reference=S(TDF_Reference,Set,label,target);Require(M(TDF_Reference,Get,*reference)==target,"direct reference");
  auto refs=S(TDataStd_ReferenceArray,Set,label,3,4);M(TDataStd_ReferenceArray,SetValue,*refs,3,target);
  M(TDataStd_ReferenceArray,SetValue,*refs,4,TDF_Label());
  Require(M(TDataStd_ReferenceArray,Value,*refs,3)==target&&M(TDF_Label,IsNull,M(TDataStd_ReferenceArray,Value,*refs,4)),"null reference slots");
  auto tree=S(TDataStd_TreeNode,Set,target);Require(tree->ID()==S(TDataStd_TreeNode,GetDefaultTreeID),"tree GUID");
  auto named=S(TDataStd_NamedData,Set,label);
  TCollection_ExtendedString key("key");M(TDataStd_NamedData,SetInteger,*named,key,17);
  M(TDataStd_NamedData,SetReal,*named,key,2.125);M(TDataStd_NamedData,SetString,*named,key,TCollection_ExtendedString("value"));
  occ::handle<NCollection_HArray1<int>> integers=new NCollection_HArray1<int>(-3,-1);integers->SetValue(-3,5);
  occ::handle<NCollection_HArray1<double>> reals=new NCollection_HArray1<double>(7,8);reals->SetValue(7,.125);
  M(TDataStd_NamedData,SetArrayOfIntegers,*named,key,integers);M(TDataStd_NamedData,SetArrayOfReals,*named,key,reals);
  Require(M(TDataStd_NamedData,HasInteger,*named,key)&&M(TDataStd_NamedData,HasReal,*named,key)&&M(TDataStd_NamedData,HasString,*named,key),"scalar presence");
  Require(M(TDataStd_NamedData,HasArrayOfIntegers,*named,key)&&M(TDataStd_NamedData,HasArrayOfReals,*named,key),"array presence");
  number(M(TDataStd_NamedData,GetInteger,*named,key));number(M(TDataStd_NamedData,GetReal,*named,key));number(M(TDataStd_NamedData,GetString,*named,key).Length());
  number(M(TDataStd_NamedData,GetArrayOfIntegers,*named,key)->Lower());number(M(TDataStd_NamedData,GetArrayOfIntegers,*named,key)->Value(-3));
  number(M(TDataStd_NamedData,GetArrayOfReals,*named,key)->Lower());number(M(TDataStd_NamedData,GetArrayOfReals,*named,key)->Value(7));
  // Missing typed lookup retains its old handle; wrong-type lookup clears it.
  occ::handle<TDataStd_Name> retained=name;
  Standard_GUID missing("ca51dbcb-f4d8-4c4b-981a-74eb0f1ff4be");
  Require(!M(TDF_Label,FindAttribute,label,missing,retained)&&retained==name,"missing typed lookup retention");
  Require(!M(TDF_Label,FindAttribute,label,S(TDataStd_Integer,GetID),retained)&&retained.IsNull(),"wrong typed lookup clearing");
  occ::handle<TDF_Attribute> attr;
  for(const auto& id:{S(TDataStd_Name,GetID),S(TDataStd_Comment,GetID),S(TDataStd_AsciiString,GetID),S(TDataStd_Integer,GetID),S(TDataStd_Real,GetID),S(TDataStd_IntegerArray,GetID),S(TDataStd_RealArray,GetID),S(TDataStd_NamedData,GetID),S(TDF_Reference,GetID),S(TDataStd_ReferenceArray,GetID)}){
    Require(M(TDF_Label,FindAttribute,label,id,attr)&&M(TDF_Attribute,Label,*attr)==label,"nongeneric attribute identity");}
  using AI=std::conditional_t<G,D::PrivateTDFAttributeIterator,TDF_AttributeIterator>;
  using CI=std::conditional_t<G,D::PrivateTDFChildIterator,TDF_ChildIterator>;
  int attributes=0;for(AI it(label,false);M(TDF_AttributeIterator,More,it);M(TDF_AttributeIterator,Next,it)){
    Require(M(TDF_AttributeIterator,Value,it)->Label()==label,"attribute iterator parent");++attributes;}
  number(attributes);int children=0;for(CI it(root,true);M(TDF_ChildIterator,More,it);M(TDF_ChildIterator,Next,it)){number(M(TDF_Label,Tag,M(TDF_ChildIterator,Value,it)));++children;}number(children);
  auto scope=S(TFunction_Scope,Set,label);auto graph=S(TFunction_GraphNode,Set,label);
  S(TFunction_Function,Set,label,missing);Require(M(TFunction_Scope,AddFunction,*scope,label),"function registration");
  int function=M(TFunction_Scope,GetFunction,*scope,label);Require(M(TFunction_Scope,GetFunction,*scope,function)==label&&M(TFunction_Scope,HasFunction,*scope,label),"scope lookup");
  graph->AddNext(9);graph->AddPrevious(8);number(M(TFunction_GraphNode,GetNext,*graph).Extent());number(M(TFunction_GraphNode,GetPrevious,*graph).Extent());
  auto log=S(TFunction_Logbook,Set,label);log->Backup();M(TFunction_Logbook,SetTouched,*log,label);M(TFunction_Logbook,SetImpacted,*log,target,false);M(TFunction_Logbook,SetValid,*log,label,false);
  Require(M(TFunction_Logbook,GetTouched,*log).Contains(label)&&M(TFunction_Logbook,GetImpacted,*log).Contains(target)&&M(TFunction_Logbook,GetValid,*log).Contains(label),"logbook maps");
  for(const auto& id:{S(TFunction_Function,GetID),S(TFunction_GraphNode,GetID)})Require(M(TDF_Label,FindAttribute,label,id,attr),"function IDs");
  Require(M(TDF_Label,FindAttribute,M(TDF_Label,Root,label),S(TFunction_Scope,GetID),attr)&&M(TDF_Label,FindAttribute,M(TDF_Label,Root,label),S(TFunction_Logbook,GetID),attr),"root scoped IDs");
  Require(doc->CommitCommand(),"commit");number(M(TDocStd_Document,GetUndos,*doc).Extent());
  auto delta=M(TDocStd_Document,GetUndos,*doc).Last();M(TDF_Delta,SetName,*delta,TCollection_ExtendedString("adapter-command"));
  number(M(TDF_Delta,Name,*delta).Length());int deltas=0;
  for(NCollection_List<occ::handle<TDF_AttributeDelta>>::Iterator it(M(TDF_Delta,AttributeDeltas,*delta));it.More();it.Next()){
    Require(!M(TDF_Label,IsNull,M(TDF_AttributeDelta,Label,*it.Value())),"delta label");++deltas;}number(deltas);
  Require(doc->Undo(),"undo");number(M(TDocStd_Document,GetRedos,*doc).Extent());Require(doc->Redo(),"redo");
  doc->NewCommand();Require(M(TFunction_Scope,RemoveFunction,*scope,label),"remove function");
  Require(M(TDF_Label,ForgetAttribute,label,S(TDataStd_Comment,GetID)),"forget comment");doc->AbortCommand();
  Require(M(TDF_Label,FindAttribute,label,S(TDataStd_Comment,GetID),attr),"abort restored comment");
  // Exact naming and temporary receiver forwarding, with no shape correspondence guessed.
  doc->NewCommand();using NB=std::conditional_t<G,D::PrivateTNamingBuilder,TNaming_Builder>;
  using NI=std::conditional_t<G,D::PrivateTNamingIterator,TNaming_Iterator>;
  auto shapeLabel=M(TDF_Label,FindChild,root,20,true);M(TNaming_Builder,Generated,NB(shapeLabel),shape);
  occ::handle<TNaming_NamedShape> ns;Require(M(TDF_Label,FindAttribute,shapeLabel,S(TNaming_NamedShape,GetID),ns),"named shape lookup");
  Require(S(TNaming_Tool,GetShape,ns).IsSame(shape),"named shape identity");
  for(NI it(shapeLabel,-1);M(TNaming_Iterator,More,it);M(TNaming_Iterator,Next,it)){number(M(TNaming_Iterator,Evolution,it));Require(M(TNaming_Iterator,NewShape,it).IsSame(shape)&&M(TNaming_Iterator,OldShape,it).IsNull(),"naming generated path");}
  auto selected=M(TDF_Label,FindChild,root,21,true);using NS=std::conditional_t<G,D::PrivateTNamingSelector,TNaming_Selector>;NS selector(selected);
  number(M(TNaming_Selector,Select,selector,shape,shape,false,false));NCollection_Map<TDF_Label> valid;number(M(TNaming_Selector,Solve,selector,valid));
  auto changed=M(TDF_Label,FindChild,root,22,true);M(TNaming_Builder,Modify,NB(changed),shape,shape);
  auto generated=M(TDF_Label,FindChild,root,23,true);M(TNaming_Builder,Generated,NB(generated),shape,shape);
  auto deleted=M(TDF_Label,FindChild,root,24,true);M(TNaming_Builder,Delete,NB(deleted),shape);
  // Relocation must map references, not borrow their original mutable attribute.
  auto from=M(TDF_Label,FindChild,root,30,true),to=M(TDF_Label,FindChild,root,31,true);
  S(TDataStd_Name,Set,from,TCollection_ExtendedString("relocated"));S(TDF_Reference,Set,from,from);
  occ::handle<TDF_DataSet> data=new TDF_DataSet;M(TDF_DataSet,AddLabel,*data,from);M(TDF_DataSet,AddRoot,*data,from);
  for(AI it(from,false);M(TDF_AttributeIterator,More,it);M(TDF_AttributeIterator,Next,it))data->AddAttribute(M(TDF_AttributeIterator,Value,it));
  number(M(TDF_DataSet,Attributes,*data).Extent());number(M(TDF_DataSet,Labels,*data).Extent());Require(M(TDF_DataSet,ContainsLabel,*data,from),"dataset labels");
  occ::handle<TDF_RelocationTable> reloc=new TDF_RelocationTable(false);M(TDF_RelocationTable,SetRelocation,*reloc,from,to);S(TDF_CopyTool,Copy,data,reloc);
  occ::handle<TDF_Reference> copiedRef;Require(M(TDF_Label,FindAttribute,to,S(TDF_Reference,GetID),copiedRef)&&M(TDF_Reference,Get,*copiedRef)==to,"relocated self reference");
  M(TDF_Label,ForgetAllAttributes,from,true);Require(M(TDF_Label,HasAttribute,to)&&!M(TDF_Label,HasAttribute,from)&&M(TDF_Label,HasChild,root),"copy remains independent");
  doc->AbortCommand();M(TDocStd_Document,ChangeStorageFormat,*doc,TCollection_ExtendedString("XmlOcaf"));snapshot.push_back("XmlOcaf");
  app->Close(doc);return snapshot;
}
class BinaryOverride final:public BinMDataStd_NamedDataDriver{
public:explicit BinaryOverride(const occ::handle<Message_Messenger>& m):BinMDataStd_NamedDataDriver(m){}
 void Paste(const occ::handle<TDF_Attribute>&,BinObjMgt_Persistent&,NCollection_IndexedMap<occ::handle<Standard_Transient>>&)const override{throw std::runtime_error("recursive virtual storage dispatch");}
 bool Paste(const BinObjMgt_Persistent&,const occ::handle<TDF_Attribute>&,BinObjMgt_RRelocationTable&)const override{throw std::runtime_error("recursive virtual retrieval dispatch");}
};
static void QualifiedBinaryDispatch(){
 occ::handle<Message_Messenger> messenger=new Message_Messenger;BinaryOverride driver(messenger);
 occ::handle<TDataStd_NamedData> source=new TDataStd_NamedData,target=new TDataStd_NamedData;
 source->SetInteger(TCollection_ExtendedString("key"),37);BinObjMgt_Persistent persistent;
 const int payloadStart=persistent.Position();
 NCollection_IndexedMap<occ::handle<Standard_Transient>> stored;D::BinMDataStd_NamedDataDriver_Paste(driver,source,persistent,stored);
 Require(persistent.SetPosition(payloadStart),"binary payload start");BinObjMgt_RRelocationTable retrieved;
 Require(D::BinMDataStd_NamedDataDriver_Paste(driver,persistent,target,retrieved)&&target->GetInteger(TCollection_ExtendedString("key"))==37,"qualified binary base roundtrip");
 D::PrivateXmlDriversDocumentStorageDriver generated(TCollection_ExtendedString("copyright"));XmlDrivers_DocumentStorageDriver raw(TCollection_ExtendedString("copyright"));
 Require(generated.DynamicType()==raw.DynamicType(),"XML storage SDK runtime type identity");
}
int main(){try{
 auto shape=BRepPrimAPI_MakeBox(2,3,4).Shape();std::cout<<"SDK path"<<std::endl;auto raw=Exercise<false>(shape);
 std::cout<<"Generated path"<<std::endl;auto generated=Exercise<true>(shape);
 Require(raw==generated,"document adapter snapshots differ from exact SDK path");std::cout<<"Qualified driver path"<<std::endl;QualifiedBinaryDispatch();
 std::cout<<"Document adapters equivalence PASS:125 exact SDK signatures; independent SDK/generated label/attribute/typed lookup/array/reference/function/logbook/naming/relocation/undo/redo snapshots, temporary builder, qualified binary dispatch and XML runtime type identity.\n";return 0;
}catch(const Standard_Failure& e){std::cerr<<e.what()<<'\n';return 2;}catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 1;}}
