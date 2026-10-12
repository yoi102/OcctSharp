#include "OcctSharp.Modeling.RayResults.Generated.h"
#include "OcctSharp.Geometry.SurfaceIntersections.Generated.h"
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#define require(c) do {if(!(c)){fprintf(stderr,"Intersection ABI assertion failed at %d\n",__LINE__);exit(1);}} while(0)
int main(void){
 OcctSharp_ShapeHandle* box=NULL;require(occtsharp_shape_create_box(2,2,2,&box)==OCCTSHARP_STATUS_SUCCESS);
 const double xray[6]={-1,1,1,1,0,0},zray[6]={1,1,-1,0,0,1};
 void* ray=(void*)1;int32_t done=9,count=9;
 require(occtsharp_ray_result_create(box,xray,6,0,INFINITY,1e-7,4096,&ray,&done,&count)==OCCTSHARP_STATUS_INVALID_ARGUMENT&&ray==NULL&&done==0&&count==0);
 require(occtsharp_ray_result_create(box,xray,6,0,5,1e-7,4096,&ray,&done,&count)==OCCTSHARP_STATUS_SUCCESS&&done==1&&count==2);
 double values[14];int32_t info[10];for(int i=0;i<14;++i)values[i]=987654;for(int i=0;i<10;++i)info[i]=987654;
 require(occtsharp_ray_result_copy(ray,values,12,info,7)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 for(int i=0;i<14;++i)require(values[i]==987654);for(int i=0;i<10;++i)require(info[i]==987654);
 require(occtsharp_ray_result_copy(ray,values,11,info,8)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 for(int i=0;i<14;++i)require(values[i]==987654);for(int i=0;i<10;++i)require(info[i]==987654);
 require(occtsharp_ray_result_copy(ray,values,12,info,8)==OCCTSHARP_STATUS_SUCCESS&&values[5]==1&&values[11]==3&&values[12]==987654&&info[8]==987654);
 OcctSharp_ShapeHandle* xface=NULL;require(occtsharp_ray_result_face(ray,info[0],&xface)==OCCTSHARP_STATUS_SUCCESS);
 OcctSharp_GeomSurfaceHandle* a=NULL;require(occtsharp_ray_surface_copy(xface,&a)==OCCTSHARP_STATUS_SUCCESS);
 occtsharp_ray_result_release(ray);
 require(occtsharp_ray_result_copy(ray,values,12,info,8)==OCCTSHARP_STATUS_INVALID_ARGUMENT&&values[5]==1&&info[8]==987654);
 require(occtsharp_ray_result_create(box,zray,6,0,5,1e-7,4096,&ray,&done,&count)==OCCTSHARP_STATUS_SUCCESS&&count==2);
 require(occtsharp_ray_result_copy(ray,values,12,info,8)==OCCTSHARP_STATUS_SUCCESS);
 OcctSharp_ShapeHandle* zface=NULL;require(occtsharp_ray_result_face(ray,info[0],&zface)==OCCTSHARP_STATUS_SUCCESS);
 OcctSharp_GeomSurfaceHandle* b=NULL;require(occtsharp_ray_surface_copy(zface,&b)==OCCTSHARP_STATUS_SUCCESS);
 const double config[1]={1e-7};void* result=NULL;int32_t lines=0,boundaries=0,points=0;
 require(occtsharp_surface_intersection_create(a,0,b,0,config,1,1,1,1,4096,&result,&done,&lines,&boundaries,&points)==OCCTSHARP_STATUS_SUCCESS&&done==1&&lines==1&&boundaries==0&&points==0);
 double tolerance[4]={987654,987654,987654,987654},domain[4]={987654,987654,987654,987654};int32_t presence[6]={987654,987654,987654,987654,987654,987654};
 require(occtsharp_surface_intersection_snapshot(result,tolerance,2,domain,2,presence,3)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 for(int i=0;i<4;++i)require(tolerance[i]==987654&&domain[i]==987654);for(int i=0;i<6;++i)require(presence[i]==987654);
 require(occtsharp_surface_intersection_snapshot(result,tolerance,2,domain,1,presence,4)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 for(int i=0;i<4;++i)require(tolerance[i]==987654&&domain[i]==987654);for(int i=0;i<6;++i)require(presence[i]==987654);
 require(occtsharp_surface_intersection_snapshot(result,tolerance,2,domain,2,presence,4)==OCCTSHARP_STATUS_SUCCESS);
 require(tolerance[2]==987654&&domain[2]==987654&&presence[4]==987654&&presence[0]==0&&presence[1]==0&&presence[2]==0&&presence[3]==0);
 OcctSharp_Geom2dCurveHandle* pc=(OcctSharp_Geom2dCurveHandle*)1;require(occtsharp_surface_intersection_pcurve(result,1,0,&pc)==OCCTSHARP_STATUS_SUCCESS&&pc==NULL);
 OcctSharp_GeomCurveHandle* curve=NULL;require(occtsharp_surface_intersection_curve(result,1,0,&curve)==OCCTSHARP_STATUS_SUCCESS&&curve!=NULL);
 occtsharp_surface_intersection_release(result);double saved=tolerance[0];require(occtsharp_surface_intersection_snapshot(result,tolerance,2,domain,2,presence,4)==OCCTSHARP_STATUS_INVALID_ARGUMENT&&tolerance[0]==saved);
 occtsharp_generated_geom_curve_release(curve);occtsharp_generated_geom_surface_release(a);occtsharp_generated_geom_surface_release(b);occtsharp_ray_result_release(ray);occtsharp_shape_release(xface);occtsharp_shape_release(zface);occtsharp_shape_release(box);
 puts("Intersection result buffer ABI PASS: failure publication, atomic aggregate canaries, finite hit data, optional pcurves/domains, copied geometry and stale token rejection.");return 0;
}
