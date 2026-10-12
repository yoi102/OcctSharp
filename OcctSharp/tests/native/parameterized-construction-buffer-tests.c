#include "OcctSharp.Modeling.ParameterizedConstruction.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
int main(void){
 double data[11]={0,0,0,0,0,1,1,0,0,2,1},bounds[4]={0,3,0,4};
 void* token=(void*)(uintptr_t)1;
 assert(occtsharp_parameterized_edge(99,0,NULL,NULL,NULL,NULL,NULL,data,11,0,3,&token)!=OCCTSHARP_STATUS_SUCCESS && token==NULL);
 assert(occtsharp_parameterized_edge(5,0,NULL,NULL,NULL,NULL,NULL,data,11,0,3,&token)==OCCTSHARP_STATUS_SUCCESS && token);
 int32_t info[8];double ranges[6];for(int i=0;i<8;++i)info[i]=0xABC;for(int i=0;i<6;++i)ranges[i]=999;
 assert(occtsharp_parameterized_snapshot(token,info+1,4,ranges+1,4)!=OCCTSHARP_STATUS_SUCCESS);
 for(int i=0;i<8;++i)assert(info[i]==0xABC);for(int i=0;i<6;++i)assert(ranges[i]==999);
 assert(occtsharp_parameterized_snapshot(token,info+1,6,ranges+1,4)==OCCTSHARP_STATUS_SUCCESS);
 assert(info[0]==0xABC && info[7]==0xABC && info[1]==1 && ranges[0]==999 && ranges[5]==999 && ranges[1]==0 && ranges[2]==3);
 double values[7];for(int i=0;i<7;++i)values[i]=888;
 assert(occtsharp_parameterized_sample(token,2,0,0,values+1,4)!=OCCTSHARP_STATUS_SUCCESS);
 for(int i=0;i<7;++i)assert(values[i]==888);
 assert(occtsharp_parameterized_sample(token,2,0,0,values+1,5)==OCCTSHARP_STATUS_SUCCESS && values[0]==888 && values[6]==888 && values[1]==2);
 OcctSharp_ShapeHandle* shape=(OcctSharp_ShapeHandle*)(uintptr_t)1;
 assert(occtsharp_parameterized_shape(token,99,&shape)!=OCCTSHARP_STATUS_SUCCESS && shape==NULL);
 assert(occtsharp_parameterized_shape(token,0,&shape)==OCCTSHARP_STATUS_SUCCESS && shape);
 occtsharp_parameterized_release(token);
 assert(occtsharp_parameterized_snapshot(token,info+1,6,ranges+1,4)!=OCCTSHARP_STATUS_SUCCESS);
 occtsharp_shape_release(shape);
 token=(void*)(uintptr_t)1;assert(occtsharp_parameterized_face(0,data,11,bounds,3,&token)!=OCCTSHARP_STATUS_SUCCESS && token==NULL);
 assert(occtsharp_parameterized_face(0,data,11,bounds,4,&token)==OCCTSHARP_STATUS_SUCCESS && token);
 assert(occtsharp_parameterized_sample(token,2,2,1,values+1,5)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_parameterized_sample(token,2,2,0,values+1,5)==OCCTSHARP_STATUS_SUCCESS && values[1]==2 && values[2]==2);
 occtsharp_parameterized_release(token);
 puts("Parameterized construction C ABI PASS: copied snapshot/sample canaries, no partial writes, stale tokens, missing representations and cleared failed outputs.");return 0;
}
