#include "OcctSharp.Geometry.FillingWorkspaces.Generated.h"
#include <assert.h>
#include <math.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
int main(void){
 const void* inputs[2]={NULL,NULL};int32_t options[7]={3,8,5,4,2,1,64};double values[6]={1e-6,1e-6,.4,.2,.4,0};
 double parameters[4]={99,0,1,88},saved[4];memcpy(saved,parameters,sizeof(parameters));void* result=(void*)(uintptr_t)1;
 assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,2,NULL)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_filling_create_parameterized(NULL,2,options,7,values,6,parameters+1,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,NULL,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,1,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,3,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 options[5]=2;result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);options[5]=1;
 parameters[2]=0;result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 parameters[2]=NAN;result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 parameters[2]=1e-12;result=(void*)(uintptr_t)1;assert(occtsharp_filling_create_parameterized(inputs,2,options,7,values,6,parameters+1,2,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 assert(parameters[0]==saved[0]&&parameters[1]==saved[1]&&parameters[2]==1e-12&&parameters[3]==saved[3]);
 puts("Nonuniform filling C ABI PASS: cleared failed outputs, null/mismatched buffers, finite ascending domain, ignored smoothing rejection and input canaries.");return 0;
}
