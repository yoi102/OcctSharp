#include "Modeling/OcctSharp.Modeling.BooleanRepairAdapters.Generated.hxx"
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <cmath>
#include <iostream>
#include <sstream>
#include <type_traits>
using namespace OcctSharpGenerated;
static void Require(bool value,const char* message){if(!value)throw std::runtime_error(message);}
static double Volume(const TopoDS_Shape& shape){GProp_GProps p;BRepGProp::VolumeProperties(shape,p);return p.Mass();}
static std::unique_ptr<BRepAlgoAPI_BooleanOperation> Direct(int operation){if(operation==0)return std::make_unique<BRepAlgoAPI_Fuse>();if(operation==1)return std::make_unique<BRepAlgoAPI_Cut>();return std::make_unique<BRepAlgoAPI_Common>();}
static void Configure(BRepAlgoAPI_BooleanOperation& value,const NCollection_List<TopoDS_Shape>& args,const NCollection_List<TopoDS_Shape>& tools){value.SetArguments(args);value.SetTools(tools);value.SetNonDestructive(true);value.SetRunParallel(false);value.SetToFillHistory(true);}
static void Configure(BooleanHistoryBuilder& value,const NCollection_List<TopoDS_Shape>& args,const NCollection_List<TopoDS_Shape>& tools){value.SetArguments(args);value.SetTools(tools);value.SetNonDestructive(true);value.SetRunParallel(false);value.SetToFillHistory(true);}
int main(){try{
 static_assert(std::is_same_v<decltype(std::declval<BooleanHistoryBuilder&>().Shape()),TopoDS_Shape>);
 static_assert(std::is_same_v<decltype(std::declval<BooleanHistoryBuilder&>().Modified(std::declval<const TopoDS_Shape&>())),std::vector<TopoDS_Shape>>);
 const TopoDS_Shape source=BRepPrimAPI_MakeBox(10,10,10).Shape();gp_Trsf placement;placement.SetTranslation(gp_Vec(4,-1,-1));
 const TopoDS_Shape cutter=BRepBuilderAPI_Transform(BRepPrimAPI_MakeBox(2,12,12).Shape(),placement,true).Shape();
 NCollection_List<TopoDS_Shape> args,tools;args.Append(source);tools.Append(cutter);
 NCollection_IndexedMap<TopoDS_Shape,TopTools_ShapeMapHasher> topology;TopExp::MapShapes(source,topology);TopExp::MapShapes(cutter,topology);
 const double expected[]={1088,800,200};
 for(int operation=0;operation<3;++operation){TopoDS_Shape retained;BooleanDiagnosticCopies retainedDiagnostics;
  {BooleanHistoryBuilder generated(operation);auto direct=Direct(operation);Configure(generated,args,tools);Configure(*direct,args,tools);generated.Build();direct->Build();
   Require(generated.IsDone()==direct->IsDone()&&generated.IsDone()&&generated.HasErrors()==direct->HasErrors()&&generated.HasWarnings()==direct->HasWarnings(),"Boolean completion/diagnostic flags differ");
   retained=generated.Shape();Require(BRepCheck_Analyzer(retained).IsValid()&&std::abs(Volume(retained)-expected[operation])<1e-7&&std::abs(Volume(retained)-Volume(direct->Shape()))<1e-7,"Boolean owning geometry differs");
   for(const auto& item:topology){if(!BRepTools_History::IsSupportedType(item))continue;Require(generated.Modified(item).size()==static_cast<size_t>(direct->Modified(item).Size())&&generated.Generated(item).size()==static_cast<size_t>(direct->Generated(item).Size())&&generated.IsDeleted(item)==direct->IsDeleted(item),"Boolean copied history differs");}
   const auto shell=TopExp_Explorer(source,TopAbs_SHELL).Current();bool unsupported=false;try{generated.Modified(shell);}catch(const std::invalid_argument&){unsupported=true;}Require(unsupported,"Unsupported history source reached the SDK");
   std::ostringstream errors,warnings;direct->DumpErrors(errors);direct->DumpWarnings(warnings);retainedDiagnostics=generated.Diagnostics();Require(retainedDiagnostics.errors==errors.str()&&retainedDiagnostics.warnings==warnings.str(),"Boolean diagnostic streams differ");
  }
  Require(BRepCheck_Analyzer(retained).IsValid()&&std::abs(Volume(retained)-expected[operation])<1e-7,"Returned shape depends on destroyed builders");
  BooleanDiagnosticCopies failed;
  {BooleanHistoryBuilder generated(operation);auto direct=Direct(operation);NCollection_List<TopoDS_Shape> empty;Configure(generated,args,empty);Configure(*direct,args,empty);generated.Build();direct->Build();failed=generated.Diagnostics();std::ostringstream text;direct->DumpErrors(text);Require(!generated.IsDone()&&generated.HasErrors()&&failed.hasErrors&&failed.errors==text.str()&&!failed.errors.empty(),"Actual SDK failure text was not copied exactly");}
  Require(!failed.errors.empty(),"Failure text lost its independent lifetime");
 }
 const auto edge=BRepBuilderAPI_MakeEdge(gp_Pnt(0,0,0),gp_Pnt(2,0,0)).Shape();const auto first=BRepBuilderAPI_MakeEdge(gp_Pnt(0,0,0),gp_Pnt(1,0,0)).Shape();const auto second=BRepBuilderAPI_MakeEdge(gp_Pnt(1,0,0),gp_Pnt(2,0,0)).Shape();const auto removed=BRepBuilderAPI_MakeEdge(gp_Pnt(3,0,0),gp_Pnt(4,0,0)).Shape();
 std::vector<TopoDS_Shape> modified,created;
 {BRepTools_History history;history.AddModified(edge,first);history.AddGenerated(edge,second);history.Remove(removed);modified=RepairHistoryModified(history,edge);created=RepairHistoryGenerated(history,edge);Require(modified.size()==1&&modified[0].IsSame(history.Modified(edge).First())&&created.size()==1&&created[0].IsSame(history.Generated(edge).First())&&RepairHistoryRemoved(history,removed)==history.IsRemoved(removed)&&RepairHistoryRemoved(history,removed),"Repair copied history differs");}
 Require(modified[0].IsSame(first)&&created[0].IsSame(second),"Repair lists depend on destroyed history storage");
 {BRepTools_History history;bool unsupported=false;try{RepairHistoryRemoved(history,TopoDS_Shape());}catch(const std::invalid_argument&){unsupported=true;}Require(unsupported,"Null repair history source reached the SDK");}
 NCollection_List<TopoDS_Shape> list;list.Append(edge);list.Append(edge);list.Append(edge.Reversed());auto copy=CopyBooleanRepairList(list);list.Clear();Require(copy.size()==3&&copy[0].IsSame(edge)&&copy[1].IsSame(edge)&&copy[2].Orientation()==edge.Reversed().Orientation(),"List storage/order/duplicates/orientation were not copied");
 BooleanDiagnosticBuffer exact;std::ostream exactStream(&exact);const std::string boundary(BooleanDiagnosticBuffer::Limit,'x');exactStream.write(boundary.data(),static_cast<std::streamsize>(boundary.size()));Require(exact.Copy()==boundary,"Exact diagnostic budget rejected");exactStream.put('y');bool rejected=false;try{exact.Copy(exactStream.good());}catch(const std::length_error&){rejected=true;}Require(rejected,"Single-byte overflow published a partial message");
 BooleanDiagnosticBuffer oversized;std::ostream oversizedStream(&oversized);oversizedStream.exceptions(std::ios::badbit|std::ios::failbit);const std::string huge(BooleanDiagnosticBuffer::Limit+1,'z');rejected=false;try{oversizedStream.write(huge.data(),static_cast<std::streamsize>(huge.size()));}catch(const std::ios_base::failure&){rejected=true;}Require(rejected,"Chunk overflow did not stop the SDK stream");rejected=false;try{oversized.Copy();}catch(const std::length_error&){rejected=true;}Require(rejected,"Failed chunk overflow exposed a partial result");
 rejected=false;try{BooleanHistoryBuilder invalid(99);}catch(const std::invalid_argument&){rejected=true;}Require(rejected,"Unsupported operation initialized a builder");
 std::cout<<"Boolean/repair adapters equivalence PASS:19 exact SDK calls;three operations;owning lists/geometry;detached SDK failure text;65536-byte boundary and atomic overflow rejection.\n";return 0;
}catch(const std::exception& error){std::cerr<<error.what()<<'\n';return 1;}}
