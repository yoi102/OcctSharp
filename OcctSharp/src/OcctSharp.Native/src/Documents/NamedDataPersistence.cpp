#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include "Documents/NamedDataPersistence.hxx"
#include "../../generated/Documents/OcctSharp.Documents.ProjectionSupport.Generated.hxx"
#include "../../generated/Xde/OcctSharp.Xde.ColorPersistenceSupport.Generated.hxx"
#include <BinMDataStd_NamedDataDriver.hxx>
#include <XmlMDataStd_NamedDataDriver.hxx>
#include <XmlMXCAFDoc_NoteCommentDriver.hxx>
#include <XCAFDoc_NoteComment.hxx>
#include <BinMDF_ADriverTable.hxx>
#include <XmlMDF_ADriverTable.hxx>
#include <BinDrivers_DocumentStorageDriver.hxx>
#include <BinDrivers_DocumentRetrievalDriver.hxx>
#include <BinXCAFDrivers_DocumentStorageDriver.hxx>
#include <BinXCAFDrivers_DocumentRetrievalDriver.hxx>
#include <XmlDrivers_DocumentStorageDriver.hxx>
#include <XmlDrivers_DocumentRetrievalDriver.hxx>
#include <XmlXCAFDrivers_DocumentStorageDriver.hxx>
#include <XmlXCAFDrivers_DocumentRetrievalDriver.hxx>
#include <BinObjMgt_Persistent.hxx>
#include <XmlObjMgt_Persistent.hxx>
#include <TDocStd_Application.hxx>
#include <TDF_Attribute.hxx>
#include <TDataStd_NamedData.hxx>
#include <LDOM_Element.hxx>
#include "../../generated/Documents/OcctSharp.Documents.DirectCollectionPersistence.Generated.hxx"
using namespace OcctSharpDocumentProjections;
namespace {
constexpr int BinaryMarker=0x4F534E44;
constexpr const char* XmlMarker="occtsharp_nameddata_v1";
using IntegerArrayMap=NCollection_DataMap<TCollection_ExtendedString,occ::handle<NCollection_HArray1<int>>>;
using RealArrayMap=NCollection_DataMap<TCollection_ExtendedString,occ::handle<NCollection_HArray1<double>>>;
bool NeedsExtension(const occ::handle<TDataStd_NamedData>& data,bool xml){
 // The XML driver splits string keys on spaces and cannot round-trip empty
 // text. Its decimal real encoding also does not promise bitwise fidelity.
 if(xml&&(!data->GetStringsContainer().IsEmpty()||!data->GetRealsContainer().IsEmpty()||!data->GetArraysOfRealsContainer().IsEmpty()))return true;
 for(IntegerArrayMap::Iterator it(data->GetArraysOfIntegersContainer());it.More();it.Next())if(it.Value().IsNull()||it.Value()->Length()==0||(it.Value()->Lower()==0&&it.Value()->Upper()==0)||(xml&&it.Value()->Lower()!=1))return true;
 for(RealArrayMap::Iterator it(data->GetArraysOfRealsContainer());it.More();it.Next())if(it.Value().IsNull()||it.Value()->Length()==0||(it.Value()->Lower()==0&&it.Value()->Upper()==0)||(xml&&it.Value()->Lower()!=1))return true;
 return false;
}
occ::handle<TDataStd_NamedData> SafeCopy(const occ::handle<TDataStd_NamedData>& source,bool xml){
 occ::handle<TDataStd_NamedData> out=new TDataStd_NamedData();out->ChangeIntegers(source->GetIntegersContainer());out->ChangeReals(source->GetRealsContainer());out->ChangeBytes(source->GetBytesContainer());
 NCollection_DataMap<TCollection_ExtendedString,TCollection_ExtendedString> strings;
 for(decltype(strings)::Iterator it(source->GetStringsContainer());it.More();it.Next())if(!xml||(it.Key().Search(" ")==-1&&it.Value().Length()>0))strings.Bind(it.Key(),it.Value());out->ChangeStrings(strings);
 NCollection_DataMap<TCollection_ExtendedString,occ::handle<NCollection_HArray1<int>>> integers;
 for(decltype(integers)::Iterator it(source->GetArraysOfIntegersContainer());it.More();it.Next())if(!it.Value().IsNull()&&it.Value()->Length()>0&&!(it.Value()->Lower()==0&&it.Value()->Upper()==0)&&(!xml||it.Value()->Lower()==1))integers.Bind(it.Key(),it.Value());out->ChangeArraysOfIntegers(integers);
 NCollection_DataMap<TCollection_ExtendedString,occ::handle<NCollection_HArray1<double>>> reals;
 for(decltype(reals)::Iterator it(source->GetArraysOfRealsContainer());it.More();it.Next())if(!it.Value().IsNull()&&it.Value()->Length()>0&&!(it.Value()->Lower()==0&&it.Value()->Upper()==0)&&(!xml||it.Value()->Lower()==1))reals.Bind(it.Key(),it.Value());out->ChangeArraysOfReals(reals);return out;
}
std::string Hex(const std::vector<uint8_t>& bytes){static constexpr char digits[]="0123456789ABCDEF";std::string out;out.reserve(bytes.size()*2);for(uint8_t value:bytes){out.push_back(digits[value>>4]);out.push_back(digits[value&15]);}return out;}
std::vector<uint8_t> Unhex(const char* text){Require(text,"Missing NamedData extension.");size_t size=0;while(text[size]){Require(size<static_cast<size_t>(ByteLimit)*2,"NamedData extension exceeds limit.");++size;}Require(size%2==0,"Invalid NamedData extension length.");std::vector<uint8_t> bytes(size/2);auto nibble=[](char c){int n=c>='0'&&c<='9'?c-'0':c>='A'&&c<='F'?c-'A'+10:-1;Require(n>=0,"Invalid NamedData extension digit.");return n;};for(size_t n=0;n<bytes.size();++n)bytes[n]=static_cast<uint8_t>((nibble(text[n*2])<<4)|nibble(text[n*2+1]));return bytes;}
void Restore(const occ::handle<TDF_Attribute>& target,const char* hex){auto data=occ::down_cast<TDataStd_NamedData>(target);Require(!data.IsNull(),"Invalid NamedData retrieval target.");auto decoded=DecodeNamedData(Unhex(hex));data->Clear();data->Restore(decoded);}
class SafeBinaryNamedData final:public BinMDataStd_NamedDataDriver {
public:
 explicit SafeBinaryNamedData(const occ::handle<Message_Messenger>& messenger):BinMDataStd_NamedDataDriver(messenger){}
 void Paste(const occ::handle<TDF_Attribute>& source,BinObjMgt_Persistent& target,NCollection_IndexedMap<occ::handle<Standard_Transient>>& table)const override{
  auto data=occ::down_cast<TDataStd_NamedData>(source);Require(!data.IsNull(),"Invalid NamedData storage source.");if(!NeedsExtension(data,false)){OcctSharpGenerated::DocumentAdapters::BinMDataStd_NamedDataDriver_Paste(*this,source,target,table);return;}auto safe=SafeCopy(data,false);OcctSharpGenerated::DocumentAdapters::BinMDataStd_NamedDataDriver_Paste(*this,safe,target,table);target<<BinaryMarker<<TCollection_AsciiString(Hex(EncodeNamedData(data)).c_str());
 }
 bool Paste(const BinObjMgt_Persistent& source,const occ::handle<TDF_Attribute>& target,BinObjMgt_RRelocationTable& table)const override{
  if(!OcctSharpGenerated::DocumentAdapters::BinMDataStd_NamedDataDriver_Paste(*this,source,target,table))return false;if(source.Length()-source.Position()<4)return true;int marker=0;TCollection_AsciiString hex;if(!(source>>marker)||marker!=BinaryMarker||!(source>>hex))return false;Restore(target,hex.ToCString());return true;
 }
};
class SafeXmlNamedData final:public XmlMDataStd_NamedDataDriver {
public:
 explicit SafeXmlNamedData(const occ::handle<Message_Messenger>& messenger):XmlMDataStd_NamedDataDriver(messenger){}
 void Paste(const occ::handle<TDF_Attribute>& source,XmlObjMgt_Persistent& target,XmlObjMgt_SRelocationTable& table)const override{
  auto data=occ::down_cast<TDataStd_NamedData>(source);Require(!data.IsNull(),"Invalid NamedData XML storage source.");if(!NeedsExtension(data,true)){OcctSharpGenerated::DocumentAdapters::XmlMDataStd_NamedDataDriver_Paste(*this,source,target,table);return;}auto safe=SafeCopy(data,true);OcctSharpGenerated::DocumentAdapters::XmlMDataStd_NamedDataDriver_Paste(*this,safe,target,table);auto hex=Hex(EncodeNamedData(data));target.Element().setAttribute(XmlMarker,hex.c_str());
 }
 bool Paste(const XmlObjMgt_Persistent& source,const occ::handle<TDF_Attribute>& target,XmlObjMgt_RRelocationTable& table)const override{
  auto text=source.Element().getAttribute(XmlMarker);const char* hex=text.GetString();if(hex&&hex[0]){Restore(target,hex);return true;}return OcctSharpGenerated::DocumentAdapters::XmlMDataStd_NamedDataDriver_Paste(*this,source,target,table);
 }
};
// The pinned XML comment driver writes TimeStamp into "comment" and reads
// UTF-8 bytes through an ASCII ExtendedString constructor. Use the existing
// fields/type identity with correct text and strict UTF-8 decoding instead.
class SafeXmlNoteComment final:public XmlMXCAFDoc_NoteCommentDriver {
public:
 explicit SafeXmlNoteComment(const occ::handle<Message_Messenger>& messenger):XmlMXCAFDoc_NoteCommentDriver(messenger){}
 void Paste(const occ::handle<TDF_Attribute>& source,XmlObjMgt_Persistent& target,XmlObjMgt_SRelocationTable&)const override{
  auto note=occ::down_cast<XCAFDoc_NoteComment>(source);Require(!note.IsNull(),"Invalid comment storage source.");
  auto user=OcctSharp::Native::ExtendedToUtf8(note->UserName()),time=OcctSharp::Native::ExtendedToUtf8(note->TimeStamp()),comment=OcctSharp::Native::ExtendedToUtf8(note->Comment());
  target.Element().setAttribute("user_name",user.c_str());target.Element().setAttribute("time_stamp",time.c_str());target.Element().setAttribute("comment",comment.c_str());
  Writer out(2);out.Text(user);out.Text(time);out.Text(comment);auto hex=Hex(out.bytes);target.Element().setAttribute("occtsharp_note_v1",hex.c_str());
 }
 bool Paste(const XmlObjMgt_Persistent& source,const occ::handle<TDF_Attribute>& target,XmlObjMgt_RRelocationTable&)const override{
  auto note=occ::down_cast<XCAFDoc_NoteComment>(target);Require(!note.IsNull(),"Invalid comment retrieval target.");
  auto extension=source.Element().getAttribute("occtsharp_note_v1");if(extension!=nullptr){auto bytes=Unhex(extension.GetString());Reader input(bytes.data(),static_cast<int>(bytes.size()));Require(input.Number<int32_t>()==1&&input.Number<int32_t>()==2,"Invalid XML note envelope.");auto user=input.Extended(),time=input.Extended(),comment=input.Extended();input.End();OcctSharpGenerated::XdeAdapters::XCAFDoc_Note_Set_Bind(*(note))(user,time);OcctSharpGenerated::XdeAdapters::XCAFDoc_NoteComment_Set_Bind(*(note))(comment);return true;}
  auto read=[&](const char* key){Require(source.Element().getAttributeNode(key)!=nullptr,"Missing XML note field.");auto value=source.Element().getAttribute(key);int integer=0;auto text=value.GetInteger(integer)?std::to_string(integer):std::string(value.GetString());Require(text.size()<=ByteLimit,"XML note exceeds text limit.");Utf8(text);return OcctSharp::Native::MakeExtendedUtf8(text.data(),static_cast<int32_t>(text.size()));};
  auto user=read("user_name"),time=read("time_stamp"),comment=read("comment");OcctSharpGenerated::XdeAdapters::XCAFDoc_Note_Set_Bind(*(note))(user,time);OcctSharpGenerated::XdeAdapters::XCAFDoc_NoteComment_Set_Bind(*(note))(comment);return true;
 }
};
template<class Base>class BinaryDriver final:public Base {
public:
 occ::handle<BinMDF_ADriverTable> AttributeDrivers(const occ::handle<Message_Messenger>& messenger)override{auto table=Base::AttributeDrivers(messenger);table->AddDriver(new SafeBinaryNamedData(messenger));OcctSharpDirectPersistence::Configure(table,messenger);return table;}
};
template<class Base>class XmlReader final:public Base {
public:
 occ::handle<XmlMDF_ADriverTable> AttributeDrivers(const occ::handle<Message_Messenger>& messenger)override{auto table=Base::AttributeDrivers(messenger);table->AddDriver(new SafeXmlNamedData(messenger));table->AddDriver(new SafeXmlNoteComment(messenger));table->AddDriver(new OcctSharpXdeWorkspaces::ExactXmlColor(messenger));OcctSharpDirectPersistence::Configure(table,messenger);return table;}
};
template<class Base>class XmlWriter final:public Base {
public:
 XmlWriter():Base(TCollection_ExtendedString("OcctSharp; OCCT attribute persistence")){
  // The pinned note driver uses its class name as a namespace prefix. Keep
  // that established type name, but declare the prefix so standard XML
  // readers can parse the document as well as OCCT's permissive LDOM reader.
  this->AddNamespace("XCAFDoc_NoteComment","http://www.opencascade.org/OCAF/XML/XCAF");
  this->AddNamespace("XCAFDoc_AssemblyItemRef","http://www.opencascade.org/OCAF/XML/XCAF");
  this->AddNamespace("XCAFDoc_NoteBinData","http://www.opencascade.org/OCAF/XML/XCAF");
 }
 occ::handle<XmlMDF_ADriverTable> AttributeDrivers(const occ::handle<Message_Messenger>& messenger)override{auto table=Base::AttributeDrivers(messenger);table->AddDriver(new SafeXmlNamedData(messenger));table->AddDriver(new SafeXmlNoteComment(messenger));table->AddDriver(new OcctSharpXdeWorkspaces::ExactXmlColor(messenger));OcctSharpDirectPersistence::Configure(table,messenger);return table;}
};
}
namespace OcctSharp::Native {
void ConfigureSafeNamedDataDrivers(const occ::handle<TDocStd_Application>& application){
 // DefineFormat uses IndexedDataMap::Add: an existing format keeps its first
 // driver. Install these drivers before any SDK format registration.
 OcctSharpGenerated::DocumentAdapters::TDocStd_Application_DefineFormat(*(application),"BinOcaf","Binary OCAF Document","cbf",new BinaryDriver<BinDrivers_DocumentRetrievalDriver>(),new BinaryDriver<BinDrivers_DocumentStorageDriver>());
 OcctSharpGenerated::DocumentAdapters::TDocStd_Application_DefineFormat(*(application),"BinXCAF","Binary XCAF Document","xbf",new BinaryDriver<BinXCAFDrivers_DocumentRetrievalDriver>(),new BinaryDriver<BinXCAFDrivers_DocumentStorageDriver>());
 OcctSharpGenerated::DocumentAdapters::TDocStd_Application_DefineFormat(*(application),"XmlOcaf","XML OCAF Document","xml",new XmlReader<XmlDrivers_DocumentRetrievalDriver>(),new XmlWriter<OcctSharpGenerated::DocumentAdapters::PrivateXmlDriversDocumentStorageDriver>());
 OcctSharpGenerated::DocumentAdapters::TDocStd_Application_DefineFormat(*(application),"XmlXCAF","XML XCAF Document","xml",new XmlReader<XmlXCAFDrivers_DocumentRetrievalDriver>(),new XmlWriter<OcctSharpGenerated::XdeAdapters::PrivateXmlXCAFDriversDocumentStorageDriver>());
}
}
