#include "MeshData/OcctSharp.MeshData.MeshAdapters.Generated.hxx"
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRep_Tool.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <Standard_Failure.hxx>
#include <iostream>
#include <cmath>
#include <stdexcept>
using namespace OcctSharpGenerated;
static void Require(bool value){if(!value)throw std::runtime_error("Mesh adapter equivalence differs.");}
int main(){try{
  occ::handle<Poly_Triangulation> a=new Poly_Triangulation(4,2,true,true),b=new Poly_Triangulation(4,2,true,true);
  for(int i=1;i<=4;++i){a->SetNode(i,gp_Pnt(i,0,0));b->SetNode(i,gp_Pnt(i,0,0));MeshSetUVNode(*a,i,gp_Pnt2d(i,.5));b->SetUVNode(i,gp_Pnt2d(i,.5));MeshSetNormal(*a,i,NCollection_Vec3<float>(0,0,1));b->SetNormal(i,NCollection_Vec3<float>(0,0,1));Require(MeshUVNode(*a,i).Distance(b->UVNode(i))<1e-15);Require(MeshNormal(*a,i).IsEqual(b->Normal(i),1e-15));NCollection_Vec3<float> x,y;MeshNormal(*a,i,x);b->Normal(i,y);Require(x==y);}
  MeshSetTriangle(*a,1,Poly_Triangle(1,2,3));b->SetTriangle(1,Poly_Triangle(1,2,3));MeshSetTriangle(*a,2,Poly_Triangle(3,2,4));b->SetTriangle(2,Poly_Triangle(3,2,4));
  auto retained=MeshTriangle(*a,1);int x,y,z,u,v,w;retained.Get(x,y,z);b->Triangle(1).Get(u,v,w);Require(x==u&&y==v&&z==w);
  {PrivatePolyConnect ca(a);Poly_Connect cb(b);for(int i=1;i<=2;++i){ca.Triangles(i,x,y,z);cb.Triangles(i,u,v,w);Require(x==u&&y==v&&z==w);}}
  a.Nullify();b.Nullify();retained.Get(x,y,z);Require(x==1&&y==2&&z==3);
  CopiedCoherentTriangle copied;
  {PrivateCoherentBuilder ca;Poly_CoherentTriangulation cb;for(int i=0;i<4;++i){gp_XYZ p(i,i%2,0);Require(ca.SetNode(p)==cb.SetNode(p));}auto* pa=ca.AddTriangle(0,1,2);auto* pb=cb.AddTriangle(0,1,2);Require(pa&&pb);Require(ca.ReplaceNodes(*pa,0,1,3)==cb.ReplaceNodes(*pb,0,1,3));copied=ca.Triangle(0);for(int i=0;i<3;++i)Require(copied.Node(i)==cb.Triangle(0).Node(i));Require(ca.RemoveTriangle(*pa)==cb.RemoveTriangle(*pb));Require(ca.NTriangles()==cb.NTriangles()&&ca.NTriangles()==0);}
  Require(copied.Node(0)==0&&copied.Node(1)==1&&copied.Node(2)==3);
  {occ::handle<PrivateMergeNodesTool> ca=new PrivateMergeNodesTool(3.141592653589793,.001);occ::handle<Poly_MergeNodesTool> cb=new Poly_MergeNodesTool(3.141592653589793,.001);ca->ClearOutput();cb->ChangeOutput().Nullify();ca->SetDropDegenerative(false);cb->SetDropDegenerative(false);for(int i=0;i<3;++i){gp_XYZ p(i==1?.0001:0,0,0);gp_XYZ nodes[3]={p,p,p};ca->AddTriangle(nodes);cb->AddTriangle(nodes);Require(ca->ElementNodeIndex(0)==cb->ElementNodeIndex(0));}}
  occ::handle<Poly_Polygon3D> poly,polyParameters;occ::handle<Poly_PolygonOnTriangulation> indexed,indexedParameters;
  {NCollection_Array1<gp_Pnt> points(1,3);NCollection_Array1<double> parameters(1,3);NCollection_Array1<int> indices(1,3);for(int i=1;i<=3;++i){points(i)=gp_Pnt(i,0,0);parameters(i)=i*.5;indices(i)=i;}
   poly=MeshPolygon3D(points);polyParameters=MeshPolygon3D(points,parameters);indexed=MeshPolygonOnTriangulation(indices);indexedParameters=MeshPolygonOnTriangulation(indices,parameters);
   Poly_Polygon3D raw(points),rawParameters(points,parameters);Poly_PolygonOnTriangulation rawIndexed(indices),rawIndexedParameters(indices,parameters);
   for(int i=1;i<=3;++i){Require(poly->Nodes()(i).Distance(raw.Nodes()(i))<1e-15);Require(polyParameters->Parameters()(i)==rawParameters.Parameters()(i));Require(indexed->Nodes()(i)==rawIndexed.Nodes()(i));Require(indexedParameters->Parameters()->Value(i)==rawIndexedParameters.Parameters()->Value(i));}points(1)=gp_Pnt(999,0,0);parameters(1)=999;indices(1)=999;}
  Require(poly->Nodes()(1).X()==1&&polyParameters->Parameters()(1)==.5&&indexed->Nodes()(1)==1&&indexedParameters->Parameters()->Value(1)==.5);
  for(int mode=0;mode<2;++mode){auto source=BRepPrimAPI_MakeBox(2,3,4).Shape();BRepBuilderAPI_Copy ac(source,true,false),bc(source,true,false);auto sa=ac.Shape(),sb=bc.Shape();IMeshTools_Parameters parameters;parameters.Deflection=.1;
   if(mode==0){PrivateMeshBuilder ca(sa,.1,false,.5,false);BRepMesh_IncrementalMesh cb(sb,.1,false,.5,false);Require(ca.IsDone()&&ca.IsDone()==cb.IsDone());}
   else{PrivateMeshBuilder ca(sa,parameters);BRepMesh_IncrementalMesh cb(sb,parameters);Require(ca.IsDone()&&ca.IsDone()==cb.IsDone());}
   TopExp_Explorer ea(sa,TopAbs_FACE),eb(sb,TopAbs_FACE);for(;ea.More();ea.Next(),eb.Next()){Require(eb.More());TopLoc_Location la,lb;auto ta=BRep_Tool::Triangulation(TopoDS::Face(ea.Current()),la),tb=BRep_Tool::Triangulation(TopoDS::Face(eb.Current()),lb);Require(!ta.IsNull()&&!tb.IsNull()&&ta->NbNodes()==tb->NbNodes()&&ta->NbTriangles()==tb->NbTriangles());}Require(!eb.More());}
  std::cout<<"Mesh adapters equivalence PASS:24 exact SDK signatures; triangle scalar/polygon storage after owner/input closure, normal/UV/adjacency, scoped coherent mutation/weld and two mesher constructors.\n";return 0;
 }catch(const Standard_Failure& e){std::cerr<<e.what()<<'\n';return 2;}catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 1;}}
