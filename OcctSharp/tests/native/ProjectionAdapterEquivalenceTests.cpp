#include "Geometry/OcctSharp.Geometry.ProjectionAdapters.Generated.hxx"
#include <Geom_Line.hxx>
#include <Geom_Plane.hxx>
#include <Geom_ToroidalSurface.hxx>
#include <Standard_Failure.hxx>
#include <cmath>
#include <iostream>
#include <stdexcept>
namespace {
void Check(bool ok) { if (!ok) throw std::runtime_error("Projection adapter differs from fixed SDK"); }
void Near(double a, double b) { Check(std::abs(a-b)<1e-12); }
void Near(const gp_Pnt& a, const gp_Pnt& b) { Near(a.X(),b.X());Near(a.Y(),b.Y());Near(a.Z(),b.Z()); }
template<class A,class B>void Curve(const A& a,const B& b) {
  Check(a.NbPoints()==b.NbPoints());if(!a.NbPoints())return;
  Near(a.NearestPoint(),b.NearestPoint());Near(a.LowerDistance(),b.LowerDistance());Near(a.LowerDistanceParameter(),b.LowerDistanceParameter());
  for(int i=1;i<=a.NbPoints();++i){Near(a.Point(i),b.Point(i));Near(a.Parameter(i),b.Parameter(i));Near(a.Distance(i),b.Distance(i));}
}
template<class A,class B>void Surface(const A& a,const B& b) {
  Check(a.IsDone()==b.IsDone());Check(a.NbPoints()==b.NbPoints());if(!a.IsDone()||!a.NbPoints())return;
  Near(a.NearestPoint(),b.NearestPoint());Near(a.LowerDistance(),b.LowerDistance());
  double u=0,v=0,x=0,y=0;a.LowerDistanceParameters(u,v);b.LowerDistanceParameters(x,y);Near(u,x);Near(v,y);
  for(int i=1;i<=a.NbPoints();++i){a.Parameters(i,u,v);b.Parameters(i,x,y);Near(u,x);Near(v,y);}
}
template<class F>bool Throws(F&& action){try{action();return false;}catch(const Standard_Failure&){return true;}}
}
int main() {
  occ::handle<Geom_Curve> line=new Geom_Line(gp_Pnt(0,0,0),gp_Dir(1,0,0));
  const gp_Pnt query(1,2,0);
  Curve(OcctSharpGenerated::CurveProjector(query,line),GeomAPI_ProjectPointOnCurve(query,line));
  Curve(OcctSharpGenerated::CurveProjector(query,line,0,2),GeomAPI_ProjectPointOnCurve(query,line,0,2));
  Curve(OcctSharpGenerated::CurveProjector(query,line,3,4),GeomAPI_ProjectPointOnCurve(query,line,3,4));
  OcctSharpGenerated::CurveProjector ca(query,line);GeomAPI_ProjectPointOnCurve cb(query,line);
  Check(Throws([&]{ca.Parameter(0);})&&Throws([&]{cb.Parameter(0);}));
  Check(Throws([&]{ca.Point(2);})&&Throws([&]{cb.Point(2);}));
  occ::handle<Geom_Surface> plane=new Geom_Plane(gp_Pnt(0,0,0),gp_Dir(0,0,1));
  Surface(OcctSharpGenerated::SurfaceProjector(query,plane,1e-7),GeomAPI_ProjectPointOnSurf(query,plane,1e-7));
  Surface(OcctSharpGenerated::SurfaceProjector(query,plane,0,3,0,3,1e-7),GeomAPI_ProjectPointOnSurf(query,plane,0,3,0,3,1e-7));
  occ::handle<Geom_Surface> torus=new Geom_ToroidalSurface(gp_Ax3(),3,1);
  const auto target=torus->Value(6.4,6.5);
  Surface(OcctSharpGenerated::SurfaceProjector(target,torus,6,6.8,6,6.8,1e-7),GeomAPI_ProjectPointOnSurf(target,torus,6,6.8,6,6.8,1e-7));
  OcctSharpGenerated::SurfaceProjector sa(query,plane,1e-7);GeomAPI_ProjectPointOnSurf sb(query,plane,1e-7);
  Check(Throws([&]{double u=0,v=0;sa.Parameters(0,u,v);})&&Throws([&]{double u=0,v=0;sb.Parameters(0,u,v);}));
  std::cout<<"Projection adapters equivalence PASS:17 exact SDK calls, four constructor profiles, copied getters, absent solutions and index errors.\n";
}
