#include "Modeling/OcctSharp.Modeling.TopologyFaceAdapters.Generated.hxx"
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_Plane.hxx>
#include <gp.hxx>
#include <gp_Pln.hxx>
#include <cmath>
#include <iostream>
#include <stdexcept>
void Check(bool ok){if(!ok)throw std::runtime_error("Face adapter equivalence failed");}
void Compare(OcctSharpGenerated::FaceBuilder& a,BRepBuilderAPI_MakeFace& b,double area){
 Check(a.IsDone()==b.IsDone()&&a.IsDone());auto x=a.Face();auto y=b.Face();Check(BRepCheck_Analyzer(x).IsValid()==BRepCheck_Analyzer(y).IsValid());
 GProp_GProps p,q;BRepGProp::SurfaceProperties(x,p);BRepGProp::SurfaceProperties(y,q);Check(std::abs(p.Mass()-q.Mass())<1e-9&&std::abs(p.Mass()-area)<1e-9);Check(a.Shape().IsSame(x)&&b.Shape().IsSame(y));
}
int main(){
 BRepBuilderAPI_MakePolygon p;p.Add(gp_Pnt(0,0,0));p.Add(gp_Pnt(4,0,0));p.Add(gp_Pnt(4,4,0));p.Add(gp_Pnt(0,4,0));p.Close();auto wire=p.Wire();
 NCollection_Array2<gp_Pnt> poles(1,2,1,2);poles(1,1)=gp_Pnt(0,0,0);poles(1,2)=gp_Pnt(0,4,0);poles(2,1)=gp_Pnt(4,0,0);poles(2,2)=gp_Pnt(4,4,0);NCollection_Array1<double> knots(1,2);knots(1)=0;knots(2)=1;NCollection_Array1<int> mults(1,2);mults(1)=mults(2)=2;
 occ::handle<Geom_Surface> surface=new Geom_BSplineSurface(poles,knots,knots,mults,mults,1,1);occ::handle<Geom_Surface> plane=new Geom_Plane(gp::XOY());
 {OcctSharpGenerated::FaceBuilder a;BRepBuilderAPI_MakeFace b;Check(a.IsDone()==b.IsDone()&&!a.IsDone());a.Init(surface,true,1e-7);b.Init(surface,true,1e-7);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a;BRepBuilderAPI_MakeFace b;a.Init(surface,0,1,0,1,1e-7);b.Init(surface,0,1,0,1,1e-7);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(wire);BRepBuilderAPI_MakeFace b(wire);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(wire,true);BRepBuilderAPI_MakeFace b(wire,true);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(gp_Pln(gp::XOY()),wire,true);BRepBuilderAPI_MakeFace b(gp_Pln(gp::XOY()),wire,true);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(surface,1e-7);BRepBuilderAPI_MakeFace b(surface,1e-7);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(surface,0,1,0,1,1e-7);BRepBuilderAPI_MakeFace b(surface,0,1,0,1,1e-7);Compare(a,b,16);}
 {OcctSharpGenerated::FaceBuilder a(plane,wire,true);BRepBuilderAPI_MakeFace b(plane,wire,true);Compare(a,b,16);
  BRepBuilderAPI_MakePolygon hole;hole.Add(gp_Pnt(1,1,0));hole.Add(gp_Pnt(1,3,0));hole.Add(gp_Pnt(3,3,0));hole.Add(gp_Pnt(3,1,0));hole.Close();a.Add(hole.Wire());b.Add(hole.Wire());Compare(a,b,12);}
 TopoDS_Face retained;{OcctSharpGenerated::FaceBuilder a(wire);retained=a.Face();}Check(BRepCheck_Analyzer(retained).IsValid());
 std::cout<<"Topology face adapters equivalence PASS:12 exact SDK calls, initialized profiles, hole area12, owning topology values.\n";
}
