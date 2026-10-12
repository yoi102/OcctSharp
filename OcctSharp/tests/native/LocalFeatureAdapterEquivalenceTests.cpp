#include "Modeling/OcctSharp.Modeling.LocalFeatureAdapters.Generated.hxx"
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepGProp.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <GProp_GProps.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <iostream>
#include <cmath>
#include <stdexcept>
#include <Standard_Failure.hxx>
#include <gp_Pln.hxx>
using namespace OcctSharpGenerated;
static const char* stage="box";
static void Stage(const char* name){stage=name;std::cout<<"Stage "<<stage<<std::endl;}
static void Require(bool value){if(!value)throw std::runtime_error("Local-feature adapter equivalence differs.");}
static double Mass(const TopoDS_Shape& shape){GProp_GProps props;BRepGProp::VolumeProperties(shape,props);return props.Mass();}
static void Same(const TopoDS_Shape& a,const TopoDS_Shape& b){Require(!a.IsNull()&&!b.IsNull());Require(BRepCheck_Analyzer(a).IsValid()==BRepCheck_Analyzer(b).IsValid());Require(std::abs(Mass(a)-Mass(b))<1e-7);}
template<class A,class B>static void Contours(A& a,B& b){Require(a.NbContours()==b.NbContours());for(int i=1;i<=a.NbContours();++i){Require(a.NbEdges(i)==b.NbEdges(i));Require(std::abs(a.Length(i)-b.Length(i))<1e-10);Require(a.Closed(i)==b.Closed(i));Require(a.ClosedAndTangent(i)==b.ClosedAndTangent(i));for(int j=1;j<=a.NbEdges(i);++j)Require(a.Edge(i,j).IsSame(b.Edge(i,j)));}}
int main(){try{
 auto box=BRepPrimAPI_MakeBox(10,10,10).Shape();TopExp_Explorer edges(box,TopAbs_EDGE),faces(box,TopAbs_FACE);auto edge=TopoDS::Edge(edges.Current());auto face=TopoDS::Face(faces.Current());
 TopoDS_Shape retained;
 {Stage("fillet");LocalMakeFilletBuilder a(box);BRepFilletAPI_MakeFillet b(box);a.Add(1.,edge);b.Add(1.,edge);Contours(a,b);a.Build();b.Build();Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());retained=a.Shape();auto copy=a.Modified(face);auto original=copy;a.Generated(face);Require(copy.Size()==original.Size());}
 Require(BRepCheck_Analyzer(retained).IsValid());
 {Stage("chamfer");LocalMakeChamferBuilder a(box);BRepFilletAPI_MakeChamfer b(box);a.Add(1.,edge);b.Add(1.,edge);Contours(a,b);a.Build();b.Build();Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());}
 {Stage("offset-shape");LocalMakeOffsetShapeBuilder a;BRepOffsetAPI_MakeOffsetShape b;a.PerformByJoin(box,.5,1e-6);b.PerformByJoin(box,.5,1e-6);Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());}
 {Stage("thick-solid");NCollection_List<TopoDS_Shape> close;close.Append(face);LocalMakeThickSolidBuilder a;BRepOffsetAPI_MakeThickSolid b;a.MakeThickSolidByJoin(box,close,-.5,1e-6);b.MakeThickSolidByJoin(box,close,-.5,1e-6);Require(a.IsDone()==b.IsDone());if(a.IsDone())Same(a.Shape(),b.Shape());}
 BRepBuilderAPI_MakePolygon polygon;polygon.Add(gp_Pnt(0,0,0));polygon.Add(gp_Pnt(2,0,0));polygon.Add(gp_Pnt(2,2,0));polygon.Add(gp_Pnt(0,2,0));polygon.Close();auto profile=polygon.Wire();
 {Stage("offset-wire");LocalMakeOffsetBuilder a(profile);BRepOffsetAPI_MakeOffset b(profile);a.Perform(.25,0);b.Perform(.25,0);Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());}
 BRepBuilderAPI_MakePolygon path;path.Add(gp_Pnt(0,0,0));path.Add(gp_Pnt(0,0,5));auto spine=path.Wire();
 {Stage("pipe");auto section=BRepBuilderAPI_MakeFace(profile).Face();LocalMakePipeBuilder a(spine,section);BRepOffsetAPI_MakePipe b(spine,section);Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());a.Build();b.Build();Same(a.Shape(),b.Shape());}
 {Stage("pipe-shell");LocalMakePipeShellBuilder a(spine);BRepOffsetAPI_MakePipeShell b(spine);a.SetIsBuildHistory(true);b.SetIsBuildHistory(true);a.SetMode(false);b.SetMode(false);a.Add(profile);b.Add(profile);Require(a.IsReady()==b.IsReady());NCollection_List<TopoDS_Shape> x,y;{LocalMakePipeShellBuilder sa(spine);BRepOffsetAPI_MakePipeShell sb(spine);sa.SetMode(false);sb.SetMode(false);sa.Add(profile);sb.Add(profile);Stage("pipe-shell-simulate-adapter");sa.Simulate(3,x);Stage("pipe-shell-simulate-sdk");sb.Simulate(3,y);}Require(x.Size()==3&&x.Size()==y.Size());for(auto xi=x.begin(),yi=y.begin();xi!=x.end();++xi,++yi)Same(*xi,*yi);Stage("pipe-shell-build-sdk");b.Build();Stage("pipe-shell-build-adapter");a.Build();Require(a.IsDone()==b.IsDone());Stage("pipe-shell-solid");Require(a.MakeSolid()==b.MakeSolid());Same(a.Shape(),b.Shape());Require(a.GetStatus()==b.GetStatus());Require(std::abs(a.ErrorOnSurface()-b.ErrorOnSurface())<1e-10);}
 {Stage("fillet2d");auto plane=BRepBuilderAPI_MakeFace(profile).Face();TopExp_Explorer vertices(plane,TopAbs_VERTEX);LocalMakeFillet2dBuilder a(plane);BRepFilletAPI_MakeFillet2d b(plane);a.AddFillet(TopoDS::Vertex(vertices.Current()),.2);b.AddFillet(TopoDS::Vertex(vertices.Current()),.2);a.Build();b.Build();Require(a.IsDone()==b.IsDone());Same(a.Shape(),b.Shape());}
 {Stage("draft-angle");LocalDraftAngleBuilder a(box);BRepOffsetAPI_DraftAngle b(box);a.Add(face,gp_Dir(0,0,1),.1,gp_Pln(gp_Pnt(0,0,0),gp_Dir(0,0,1)),true);b.Add(face,gp_Dir(0,0,1),.1,gp_Pln(gp_Pnt(0,0,0),gp_Dir(0,0,1)),true);Require(a.AddDone()&&b.AddDone());Require(a.Status()==b.Status());Require(a.ProblematicShape().IsNull()==b.ProblematicShape().IsNull());a.Build();b.Build();Require(a.IsDone()&&b.IsDone());Same(a.Shape(),b.Shape());}
 {Stage("make-draft");LocalMakeDraftBuilder a(profile,gp_Dir(0,0,1),.1);BRepOffsetAPI_MakeDraft b(profile,gp_Dir(0,0,1),.1);a.SetOptions(BRepBuilderAPI_RightCorner,.01,3.);b.SetOptions(BRepBuilderAPI_RightCorner,.01,3.);a.SetDraft(false);b.SetDraft(false);a.Perform(3.);b.Perform(3.);Require(a.IsDone()==b.IsDone());if(a.IsDone())Same(a.Shape(),b.Shape());}
 {Stage("list-copy");NCollection_List<TopoDS_Shape> source;source.Append(edge);source.Append(edge.Reversed());auto copy=CopyLocalFeatureList(source);source.Clear();Require(copy.Size()==2);auto it=copy.begin();Require(it->Orientation()==edge.Orientation());++it;Require(it->Orientation()==edge.Reversed().Orientation());}
 std::cout<<"Local-feature adapters equivalence PASS:ten builder families,contour/shape/state/list and private sweep profiles;owning result after builder closure;original guarded workflow regressions cover additional operations.\n";return 0;
 }catch(const Standard_Failure& error){std::cerr<<stage<<": "<<error.what()<<std::endl;return 2;}catch(const std::exception& error){std::cerr<<stage<<": "<<error.what()<<'\n';return 1;}}
