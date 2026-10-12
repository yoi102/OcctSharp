#include "OcctSharp.Geometry.FiniteExtrema.Generated.h"
#include "OcctSharp.Modeling.ParameterizedConstruction.Generated.h"
#include "OcctSharp.Geometry.SharedHandles.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
static OcctSharp_GeomCurveHandle* make_curve(int second){
 double data[11]={0,0,0,0,0,1,1,0,0,1,1};if(second){data[2]=2;data[6]=0;data[7]=1;}
 void* construction=NULL;OcctSharp_ShapeHandle* edge=NULL;OcctSharp_GeomCurveHandle* curve=NULL;
 assert(occtsharp_parameterized_edge(5,0,NULL,NULL,NULL,NULL,NULL,data,11,-3,3,&construction)==OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_parameterized_shape(construction,0,&edge)==OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_finite_extrema_copy_curve(edge,&curve)==OCCTSHARP_STATUS_SUCCESS&&curve);
 occtsharp_shape_release(edge);occtsharp_parameterized_release(construction);return curve;
}
int main(void){
 OcctSharp_GeomCurveHandle* a=make_curve(0);OcctSharp_GeomCurveHandle* b=make_curve(1);double bounds[8]={-3,3,0,0,-3,3,0,0};void* result=(void*)(uintptr_t)1;
 assert(occtsharp_finite_extrema_compute(0,a,b,NULL,NULL,bounds,7,1e-7,1e-7,4096,0,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 assert(occtsharp_finite_extrema_compute(0,a,b,NULL,NULL,bounds,8,1e-7,1e-7,4096,0,&result)==OCCTSHARP_STATUS_SUCCESS&&result);
 occtsharp_generated_geom_curve_release(a);occtsharp_generated_geom_curve_release(b);
 int32_t info[8];for(int i=0;i<8;++i)info[i]=999;
 assert(occtsharp_finite_extrema_snapshot(result,info+1,5)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<8;++i)assert(info[i]==999);
 assert(occtsharp_finite_extrema_snapshot(result,info+1,6)==OCCTSHARP_STATUS_SUCCESS&&info[0]==999&&info[7]==999&&info[2]==1&&info[5]==1&&info[6]==4);
 double row[14];for(int i=0;i<14;++i)row[i]=777;
 assert(occtsharp_finite_extrema_row(result,0,0,row+1,11)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<14;++i)assert(row[i]==777);
 assert(occtsharp_finite_extrema_row(result,0,0,row+1,12)==OCCTSHARP_STATUS_SUCCESS&&row[0]==777&&row[13]==777&&row[1]==4&&row[2]==1);
 for(int i=0;i<14;++i)row[i]=777;assert(occtsharp_finite_extrema_row(result,0,1,row+1,12)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<14;++i)assert(row[i]==777);
 double sample[5]={888,888,888,888,888};assert(occtsharp_finite_extrema_sample(result,0,0,0,sample+1,2)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<5;++i)assert(sample[i]==888);
 assert(occtsharp_finite_extrema_sample(result,1,0,0,sample+1,3)==OCCTSHARP_STATUS_SUCCESS&&sample[0]==888&&sample[4]==888&&sample[3]==2);
 occtsharp_finite_extrema_release(result);assert(occtsharp_finite_extrema_snapshot(result,info+1,6)!=OCCTSHARP_STATUS_SUCCESS);
 puts("Finite extrema C ABI PASS: coupled snapshot/row/sample canaries, no partial writes, stale tokens, copied input lifetime and cleared failed outputs.");return 0;
}
