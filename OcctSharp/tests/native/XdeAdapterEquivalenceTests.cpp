#include "Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "Xde/OcctSharp.Xde.LocalStepExportNormalization.Generated.hxx"
#include <XCAFApp_Application.hxx>
#include <BinXCAFDrivers_DocumentStorageDriver.hxx>
#include <BinXCAFDrivers_DocumentRetrievalDriver.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <TDataStd_Name.hxx>
#include <TDF_Tool.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRep_Builder.hxx>
#include <BRepBndLib.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <TopoDS_Compound.hxx>
#include <gp_Ax1.hxx>
#include <gp_Trsf.hxx>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <iomanip>
#include <vector>
#include <cmath>
#include <cstring>
namespace A=OcctSharpGenerated::XdeAdapters;
void Check(bool ok,const char* message){if(!ok)throw std::runtime_error(message);}
std::string Entry(const TDF_Label& label){TCollection_AsciiString text;TDF_Tool::Entry(label,text);return text.ToCString();}
TopLoc_Location Placement(double x,double y,double z,const gp_Dir& axis,double angle){
 gp_Trsf t;t.SetRotation(gp_Ax1(gp_Pnt(0,0,0),axis),angle);t.SetTranslationPart(gp_Vec(x,y,z));return TopLoc_Location(t);
}
void Rebuild(const occ::handle<XCAFDoc_ShapeTool>& tool,const TDF_Label& label,const TopLoc_Location& location){
 TopoDS_Compound compound;BRep_Builder b;b.MakeCompound(compound);NCollection_Sequence<TDF_Label> children;tool->GetComponents(label,children,false);
 for(const auto& child:children)b.Add(compound,tool->GetShape(child));tool->SetShape(label,compound.Located(location));
}
struct Leaf{TDF_Label definition;TopLoc_Location location;};
void Leaves(const TDF_Label& label,const TopLoc_Location& parent,std::vector<Leaf>& leaves){
 if(XCAFDoc_ShapeTool::IsComponent(label)){TDF_Label target;Check(XCAFDoc_ShapeTool::GetReferredShape(label,target),"reference");Leaves(target,parent*XCAFDoc_ShapeTool::GetLocation(label),leaves);return;}
 if(XCAFDoc_ShapeTool::IsAssembly(label)){const auto own=parent*XCAFDoc_ShapeTool::GetLocation(label);NCollection_Sequence<TDF_Label> children;XCAFDoc_ShapeTool::GetComponents(label,children,false);for(const auto& child:children)Leaves(child,own,leaves);}
 else leaves.push_back({label,parent});
}
void SameTransform(const TopLoc_Location& a,const TopLoc_Location& b){
 for(int row=1;row<=3;++row)for(int col=1;col<=4;++col)Check(std::abs(a.Transformation().Value(row,col)-b.Transformation().Value(row,col))<1e-12,"noncommuting placement mismatch");
}
int liveTemporaryMaterials=0;
class TemporaryMaterial final:public XCAFDoc_VisMaterial {
public:
 TemporaryMaterial(){++liveTemporaryMaterials;SetPbrMaterial(XCAFDoc_VisMaterialPBR());}
 ~TemporaryMaterial()override{--liveTemporaryMaterials;}
};
occ::handle<XCAFDoc_VisMaterial> Temporary(){return new TemporaryMaterial();}
void ReceiverOrder(){
 occ::handle<XCAFDoc_VisMaterial> material=new XCAFDoc_VisMaterial();int marker=0;
 auto receiver=[&](){marker=1;return material.get();};auto argument=[&](){return marker==1?.75f:.25f;};
 receiver()->SetAlphaMode(Graphic3d_AlphaMode_Mask,argument());Check(material->AlphaCutOff()==.75f,"SDK receiver-first baseline");
 marker=0;A::XCAFDoc_VisMaterial_SetAlphaMode(*receiver(),Graphic3d_AlphaMode_Mask,argument());
 const bool legacyOrderDiffered=material->AlphaCutOff()!=.75f;
 marker=0;A::XCAFDoc_VisMaterial_SetAlphaMode_Bind(*receiver())(Graphic3d_AlphaMode_Mask,argument());Check(material->AlphaCutOff()==.75f,"bound receiver-before-argument order");
 auto consume=[](const XCAFDoc_VisMaterialPBR& borrowed){Check(liveTemporaryMaterials==1&&borrowed.IsDefined,"temporary borrowed result lifetime");};
 consume(A::XCAFDoc_VisMaterial_PbrMaterial_Bind(*Temporary())());Check(liveTemporaryMaterials==0,"temporary material must die at full-expression end");
 std::cout<<"Receiver-before-argument and temporary borrowed lifetime PASS; legacy unsequenced order difference observed="<<(legacyOrderDiffered?"true":"false")<<'\n';
}
void SharedPathBudgets(const occ::handle<TDocStd_Application>& app,const std::filesystem::path& output){
 occ::handle<TDocStd_Document> doc;app->NewDocument(TCollection_ExtendedString("BinXCAF"),doc);auto tool=XCAFDoc_DocumentTool::ShapeTool(doc->Main());const auto identity=TopLoc_Location();
 auto part=tool->AddShape(BRepPrimAPI_MakeBox(1,1,1).Shape(),false);
 auto assembly=[&](const TDF_Label& child,int count){auto label=tool->NewShape();for(int n=0;n<count;++n)tool->AddComponent(label,child,identity);Rebuild(tool,label,identity);return label;};
 auto subtree=part;for(int n=0;n<6;++n)subtree=assembly(subtree,2);
 auto chain=subtree;for(int n=0;n<6;++n)chain=assembly(chain,1);
 auto root=tool->NewShape();tool->AddComponent(root,subtree,identity);tool->AddComponent(root,chain,identity);Rebuild(tool,root,identity);
 auto legacy=[&](const TDF_Label& selected){NCollection_Map<TDF_Label> seen;int occurrences=0;std::function<void(const TDF_Label&,int)> visit=[&](const TDF_Label& label,int depth){Check(depth<=16,"legacy depth");if(!seen.Add(label))return;if(XCAFDoc_ShapeTool::IsComponent(label)){Check(++occurrences<=4096,"legacy occurrence");TDF_Label target;Check(XCAFDoc_ShapeTool::GetReferredShape(label,target),"legacy target");visit(target,depth+1);}else if(XCAFDoc_ShapeTool::IsAssembly(label)){NCollection_Sequence<TDF_Label> children;tool->GetComponents(label,children,false);for(const auto& child:children)visit(child,depth+1);}};visit(selected,0);};
 legacy(root);NCollection_Sequence<TDF_Label> roots;roots.Append(root);bool rejected=false;
 try{OcctSharpSelectedAssembly::ValidatePaths(roots,doc->GetData());}catch(const std::invalid_argument&){rejected=true;}Check(rejected,"cached shared depth26 must not pass depth16 profile");
 auto save=[&](const char* name){auto text=(output/name).u8string();std::string path(reinterpret_cast<const char*>(text.data()),text.size());Check(app->SaveAs(doc,TCollection_ExtendedString(path.c_str(),true))==PCDM_SS_OK,"budget XBF fixture save");};
 save("bad-depth.xbf");
 auto wide=part;for(int n=0;n<4;++n)wide=assembly(wide,16);legacy(wide);roots.Clear();roots.Append(wide);rejected=false;
 try{OcctSharpSelectedAssembly::ValidatePaths(roots,doc->GetData());}catch(const std::invalid_argument&){rejected=true;}Check(rejected,"expanded shared component visits must not use unique-label count");
 save("bad-expanded.xbf");std::ofstream budget(output/"budget-oracle.json");budget<<"{\"state\":\"PASS\",\"cases\":[{\"file\":\"bad-depth.xbf\",\"root\":\""<<Entry(root)<<"\",\"depth\":26},{\"file\":\"bad-expanded.xbf\",\"root\":\""<<Entry(wide)<<"\",\"leaves\":65536}]}\n";Check(budget.good(),"budget oracle write");
 roots.Clear();roots.Append(part);OcctSharpSelectedAssembly::ValidatePaths(roots,doc->GetData());
 bool foreign=false;try{OcctSharpSelectedAssembly::ValidatePaths(roots,occ::handle<TDF_Data>(new TDF_Data()));}catch(const std::invalid_argument&){foreign=true;}Check(foreign,"path preflight source domain");
 app->Close(doc);std::cout<<"Shared graph complete path depth/budget/domain rejection PASS; legacy unique-label checks accepted depth26 and65536 leaves\n";
}
void Adapters(){
 Quantity_ColorRGBA original(.125f,.25f,.5f,.75f),generated=A::ConstructQuantityColorRGBA(.125f,.25f,.5f,.75f);
 Check(original==generated,"RGBA constructor");Check(&A::Quantity_ColorRGBA_GetRGB(generated)==&generated.GetRGB(),"borrowed RGB identity");
 Check(A::Quantity_ColorRGBA_Alpha(generated)==original.Alpha()&&A::Quantity_Color_Red(generated.GetRGB())==original.GetRGB().Red()&&A::Quantity_Color_Green(generated.GetRGB())==original.GetRGB().Green()&&A::Quantity_Color_Blue(generated.GetRGB())==original.GetRGB().Blue(),"color getters");
 XCAFDoc_VisMaterialPBR sdk,emitted=A::ConstructXCAFDocVisMaterialPBR();Check(sdk.IsEqual(emitted),"PBR defaults");Check(emitted.BaseColor==Quantity_ColorRGBA(1.f,1.f,1.f,1.f)&&emitted.Metallic==1.f&&emitted.Roughness==1.f&&emitted.RefractionIndex==1.5f&&emitted.IsDefined,"exact initialized PBR");
 occ::handle<XCAFDoc_VisMaterial> material=new XCAFDoc_VisMaterial();emitted.BaseColor=generated;A::XCAFDoc_VisMaterial_SetPbrMaterial(*material,emitted);
 XCAFDoc_VisMaterialPBR copied=A::XCAFDoc_VisMaterial_ConvertToPbrMaterial(*material);Check(copied.IsEqual(emitted),"PBR SDK base copy initialization");Check(&A::XCAFDoc_VisMaterial_PbrMaterial(*material)==&material->PbrMaterial(),"borrowed material identity");
 auto id=A::ConstructXCAFDocAssemblyItemId(TCollection_AsciiString("0:1:1/0:1:2"));Check(A::XCAFDoc_AssemblyItemId_ToString(id).IsEqual(id.ToString())&&!A::XCAFDoc_AssemblyItemId_IsNull(id),"assembly id factory");
 XmlXCAFDrivers_DocumentStorageDriver sdkXml(TCollection_ExtendedString("CC"));A::PrivateXmlXCAFDriversDocumentStorageDriver xml(TCollection_ExtendedString("CC"));Check(sdkXml.DynamicType()==xml.DynamicType(),"XML storage SDK type identity");
}
int main(int argc,char** argv){try{
 Check(argc==2,"output fixture directory argument");Adapters();ReceiverOrder();const double halfPi=std::acos(-1.)/2;
 auto output=std::filesystem::absolute(std::filesystem::path(std::u8string(reinterpret_cast<const char8_t*>(argv[1]),std::strlen(argv[1]))));std::filesystem::create_directories(output);
 auto app=XCAFApp_Application::GetApplication();
 app->DefineFormat("BinXCAF","CC independent XCAF fixture","xbf",new BinXCAFDrivers_DocumentRetrievalDriver(),new BinXCAFDrivers_DocumentStorageDriver());
 SharedPathBudgets(app,output);
 occ::handle<TDocStd_Document> doc;app->NewDocument(TCollection_ExtendedString("BinXCAF"),doc);auto tool=XCAFDoc_DocumentTool::ShapeTool(doc->Main());XCAFDoc_DocumentTool::SetLengthUnit(doc,.001);
 auto part=tool->AddShape(BRepPrimAPI_MakeBox(2,3,4).Shape(),false);auto other=tool->AddShape(BRepPrimAPI_MakeBox(1,2,3).Shape(),false);
 TDataStd_Name::Set(part,TCollection_ExtendedString("shared-part"));TDataStd_Name::Set(other,TCollection_ExtendedString("other-part"));
 XCAFDoc_DocumentTool::ColorTool(doc->Main())->SetColor(part,Quantity_ColorRGBA(.2f,.3f,.4f,1.f),XCAFDoc_ColorGen);
 XCAFDoc_DocumentTool::LayerTool(doc->Main())->SetLayer(part,TCollection_ExtendedString("shared-layer"));
 const auto innerLoc=Placement(3,7,2,gp_Dir(0,0,1),halfPi),rootALoc=Placement(20,0,10,gp_Dir(0,1,0),halfPi),rootBLoc=Placement(-20,5,0,gp_Dir(0,0,1),halfPi);
 const auto ca=Placement(10,4,0,gp_Dir(0,0,1),-halfPi),cb=Placement(1,2,3,gp_Dir(0,0,1),0),pa=Placement(5,-1,0,gp_Dir(0,0,1),0),pb=Placement(0,2,3,gp_Dir(1,0,0),halfPi),direct=Placement(-3,1,2,gp_Dir(0,0,1),0);
 auto inner=tool->NewShape();tool->AddComponent(inner,part,pa);tool->AddComponent(inner,other,pb);Rebuild(tool,inner,innerLoc);
 auto rootA=tool->NewShape();tool->AddComponent(rootA,inner,ca);tool->AddComponent(rootA,part,direct);Rebuild(tool,rootA,rootALoc);
 auto rootB=tool->NewShape();tool->AddComponent(rootB,inner,cb);Rebuild(tool,rootB,rootBLoc);
 TDataStd_Name::Set(inner,TCollection_ExtendedString("shared-inner"));TDataStd_Name::Set(rootA,TCollection_ExtendedString("located-root-A"));TDataStd_Name::Set(rootB,TCollection_ExtendedString("located-root-B"));
 const std::vector<TopLoc_Location> expected={rootALoc*ca*innerLoc*pa,rootALoc*ca*innerLoc*pb,rootALoc*direct,rootBLoc*cb*innerLoc*pa,rootBLoc*cb*innerLoc*pb};
 std::vector<Leaf> originals;Leaves(rootA,TopLoc_Location(),originals);Leaves(rootB,TopLoc_Location(),originals);Check(originals.size()==5,"source leaf count");for(size_t n=0;n<5;++n)SameTransform(originals[n].location,expected[n]);
 occ::handle<TDocStd_Document> clonedDoc;app->NewDocument(TCollection_ExtendedString("BinXCAF"),clonedDoc);auto dst=XCAFDoc_DocumentTool::ShapeTool(clonedDoc->Main());NCollection_DataMap<TDF_Label,TDF_Label> labels;
 auto a=A::XCAFDoc_Editor_CloneShapeLabel(rootA,tool,dst,labels),b=A::XCAFDoc_Editor_CloneShapeLabel(rootB,tool,dst,labels);
 std::vector<Leaf> missing;Leaves(a,TopLoc_Location(),missing);Check(missing.size()==3,"SDK clone leaf count");bool lost=false;for(int row=1;row<=3;++row)for(int col=1;col<=4;++col)lost=lost||std::abs(missing[0].location.Transformation().Value(row,col)-expected[0].Transformation().Value(row,col))>1e-6;Check(lost,"pinned SDK clone placement gap reproduction");
 OcctSharpSelectedAssembly::Normalize(tool,dst,labels);dst->UpdateAssemblies();std::vector<Leaf> actual;Leaves(a,TopLoc_Location(),actual);Leaves(b,TopLoc_Location(),actual);Check(actual.size()==5,"normalized leaf count");
 for(size_t n=0;n<5;++n){SameTransform(actual[n].location,expected[n]);Check(actual[n].definition==labels.Find(originals[n].definition),"shared exact definition map");}
 std::vector<Leaf> unchanged;Leaves(rootA,TopLoc_Location(),unchanged);Leaves(rootB,TopLoc_Location(),unchanged);for(size_t n=0;n<5;++n)SameTransform(unchanged[n].location,expected[n]);
 auto rejected=[&](auto action){bool failed=false;try{action();}catch(const std::invalid_argument&){failed=true;}Check(failed,"unsafe normalization accepted");};
 rejected([&]{OcctSharpSelectedAssembly::Normalize(dst,dst,labels);});
 const auto savedRootShape=tool->GetShape(rootA);gp_Trsf scale;scale.SetScale(gp_Pnt(0,0,0),2.);bool scaleRejected=false;
 try{tool->SetShape(rootA,savedRootShape.Located(TopLoc_Location(scale),false));try{OcctSharpSelectedAssembly::Normalize(tool,dst,labels);}catch(const std::invalid_argument&){scaleRejected=true;}}
 catch(const Standard_Failure&){scaleRejected=true;}Check(scaleRejected,"scaled definition accepted");tool->SetShape(rootA,savedRootShape);
 XCAFDoc_Location::Set(rootA,rootBLoc);rejected([&]{OcctSharpSelectedAssembly::Normalize(tool,dst,labels);});rootA.ForgetAttribute(XCAFDoc_Location::GetID());
 const auto file=output/"located.xbf";auto path=file.u8string();std::string pathText(reinterpret_cast<const char*>(path.data()),path.size());Check(app->SaveAs(doc,TCollection_ExtendedString(pathText.c_str(),true))==PCDM_SS_OK,"located XBF fixture save");
 std::ofstream oracle(output/"oracle.json");oracle<<std::setprecision(17)<<"{\"state\":\"PASS\",\"sdkCloneLostPlacement\":true,\"normalizedLeaves\":5,\"sourceUnchanged\":true,\"sharedDefinitions\":2,\"roots\":[\""<<Entry(rootA)<<"\",\""<<Entry(rootB)<<"\"],\"leaves\":[";
 for(size_t n=0;n<5;++n){if(n)oracle<<',';auto shape=tool->GetShape(originals[n].definition).Moved(expected[n]);Bnd_Box box;BRepBndLib::AddOptimal(shape,box,false,false);double x0,y0,z0,x1,y1,z1;box.Get(x0,y0,z0,x1,y1,z1);GProp_GProps props;BRepGProp::VolumeProperties(shape,props);oracle<<"{\"definition\":\""<<Entry(originals[n].definition)<<"\",\"volume\":"<<props.Mass()<<",\"bounds\":["<<x0<<','<<y0<<','<<z0<<','<<x1<<','<<y1<<','<<z1<<"]}";}
 oracle<<"]}\n";Check(oracle.good(),"oracle write");std::cout<<"XDE exact adapters/defaults/borrowed identity/SDK base copy and 5 noncommuting shared located leaves: PASS\n";app->Close(doc);app->Close(clonedDoc);return 0;
 }catch(const Standard_Failure& e){std::cerr<<e.what()<<'\n';return 2;}catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 1;}}
