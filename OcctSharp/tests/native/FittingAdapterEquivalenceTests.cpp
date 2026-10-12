#include "Geometry/OcctSharp.Geometry.FittingAdapters.Generated.hxx"
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <cmath>
#include <iostream>
#include <stdexcept>
namespace {
void Check(bool condition){if(!condition)throw std::runtime_error("Fitting helper differs from fixed SDK");}
template<class A,class B>void Curves(const A& a,const B& b){Check(!a.IsNull()&&!b.IsNull());Check(a->Degree()==b->Degree()&&a->NbPoles()==b->NbPoles());for(int i=0;i<=10;++i){double u=a->FirstParameter()+(a->LastParameter()-a->FirstParameter())*i/10.;Check(a->Value(u).Distance(b->Value(u))<1e-12);}}
template<class A,class B>void Surfaces(const A& a,const B& b){Check(!a.IsNull()&&!b.IsNull());double u0=0,u1=0,v0=0,v1=0;a->Bounds(u0,u1,v0,v1);for(int i=0;i<=4;++i)for(int j=0;j<=4;++j){double u=u0+(u1-u0)*i/4.,v=v0+(v1-v0)*j/4.;Check(a->Value(u,v).Distance(b->Value(u,v))<1e-12);}}
occ::handle<Geom_BSplineCurve> BS(const gp_Pnt& a,const gp_Pnt& b){NCollection_Array1<gp_Pnt> p(1,2);p(1)=a;p(2)=b;NCollection_Array1<double> k(1,2);k(1)=0;k(2)=1;NCollection_Array1<int> m(1,2);m(1)=2;m(2)=2;return new Geom_BSplineCurve(p,k,m,1);}
occ::handle<Geom_BezierCurve> BZ(const gp_Pnt& a,const gp_Pnt& b){NCollection_Array1<gp_Pnt> p(1,2);p(1)=a;p(2)=b;return new Geom_BezierCurve(p);}
}
int main(){
 NCollection_Array1<gp_Pnt> points(1,5);for(int i=1;i<=5;++i)points(i)=gp_Pnt(i,std::sin(i),0);
 OcctSharpGenerated::CurveFitter a(points,1,5,GeomAbs_C2,1e-6);GeomAPI_PointsToBSpline b(points,1,5,GeomAbs_C2,1e-6);Check(a.IsDone()==b.IsDone());Curves(a.Curve(),b.Curve());
 NCollection_Array2<gp_Pnt> grid(1,4,1,4);for(int i=1;i<=4;++i)for(int j=1;j<=4;++j)grid(i,j)=gp_Pnt(i,j,.1*i*j);
 OcctSharpGenerated::SurfaceFitter sa;GeomAPI_PointsToBSplineSurface sb;
 sa.Init(grid,1,5,GeomAbs_C2,1e-6);sb.Init(grid,1,5,GeomAbs_C2,1e-6);Check(sa.IsDone()==sb.IsDone());Surfaces(sa.Surface(),sb.Surface());
 sa.Interpolate(grid);sb.Interpolate(grid);Check(sa.IsDone()==sb.IsDone());Surfaces(sa.Surface(),sb.Surface());
 auto c0=BS(gp_Pnt(0,0,0),gp_Pnt(1,0,0)),c1=BS(gp_Pnt(1,0,0),gp_Pnt(1,1,0)),c2=BS(gp_Pnt(1,1,0),gp_Pnt(0,1,0)),c3=BS(gp_Pnt(0,1,0),gp_Pnt(0,0,0));
 auto d0=BZ(gp_Pnt(0,0,0),gp_Pnt(1,0,0)),d1=BZ(gp_Pnt(1,0,0),gp_Pnt(1,1,0)),d2=BZ(gp_Pnt(1,1,0),gp_Pnt(0,1,0)),d3=BZ(gp_Pnt(0,1,0),gp_Pnt(0,0,0));
 OcctSharpGenerated::BSplinePatchFitter ba;GeomFill_BSplineCurves bb;OcctSharpGenerated::BezierPatchFitter za;GeomFill_BezierCurves zb;
 ba.Init(c0,c2,GeomFill_StretchStyle);bb.Init(c0,c2,GeomFill_StretchStyle);Surfaces(ba.Surface(),bb.Surface());
 ba.Init(c0,c1,c2,GeomFill_StretchStyle);bb.Init(c0,c1,c2,GeomFill_StretchStyle);Surfaces(ba.Surface(),bb.Surface());
 ba.Init(c0,c1,c2,c3,GeomFill_StretchStyle);bb.Init(c0,c1,c2,c3,GeomFill_StretchStyle);Surfaces(ba.Surface(),bb.Surface());
 za.Init(d0,d2,GeomFill_StretchStyle);zb.Init(d0,d2,GeomFill_StretchStyle);Surfaces(za.Surface(),zb.Surface());
 za.Init(d0,d1,d2,GeomFill_StretchStyle);zb.Init(d0,d1,d2,GeomFill_StretchStyle);Surfaces(za.Surface(),zb.Surface());
 za.Init(d0,d1,d2,d3,GeomFill_StretchStyle);zb.Init(d0,d1,d2,d3,GeomFill_StretchStyle);Surfaces(za.Surface(),zb.Surface());
 occ::handle<Geom_BSplineCurve> retained;{OcctSharpGenerated::CurveFitter owned(points,1,5,GeomAbs_C2,1e-6);retained=owned.Curve();}Check(!retained.IsNull());Check(std::isfinite(retained->Value(retained->FirstParameter()).X()));
 std::cout<<"Fitting adapters equivalence PASS:18 exact SDK calls, all curve/surface/patch profiles, owning handle copies after receiver destruction.\n";
}
