#include "OcctSharp.Foundation.NumericResults.Generated.h"
#include <math.h>
#include <stdlib.h>
#include <stdio.h>

static void unchanged(const double* matrix,int rows,int columns,int operation,double threshold,int capacity,OcctSharp_Status expected,int sdkIncomplete) {
 double output[16];for(int i=0;i<16;++i)output[i]=123456.0;
 int32_t done=9,status=9,aux=9,written=9,hasDet=9;double determinant=9;
 OcctSharp_Status result=occtsharp_numeric_result(matrix,rows,columns,NULL,0,operation,threshold,output,capacity,&done,&status,&aux,&written,&determinant,&hasDet);
 if(result!=expected||done!=0||written!=0||hasDet!=0||determinant!=0||aux!=0)abort();
 if(sdkIncomplete&&status==0)abort();
 for(int i=0;i<16;++i)if(output[i]!=123456.0)abort();
}
int main(void) {
 double singular[]={1,2,2,4};
 unchanged(singular,2,2,0,1e-20,16,OCCTSHARP_STATUS_SUCCESS,1);
 double good[]={3,1,1,2};
 unchanged(good,2,2,0,1e-20,1,OCCTSHARP_STATUS_INVALID_ARGUMENT,0);
 double overflow[]={1e308,0,0,1e308};
 unchanged(overflow,2,2,0,1e-20,16,OCCTSHARP_STATUS_INVALID_ARGUMENT,0);
 double invalid[]={NAN};
 unchanged(invalid,1,1,3,1e-20,16,OCCTSHARP_STATUS_INVALID_ARGUMENT,0);
 double output[16]={0};int32_t done=0,status=0,aux=0,written=0,hasDet=0;double determinant=0;
 if(occtsharp_numeric_result(good,2,2,NULL,0,3,1e-20,output,16,&done,&status,&aux,&written,&determinant,&hasDet)!=OCCTSHARP_STATUS_SUCCESS||done!=1||written!=4||hasDet!=1||fabs(determinant-5)>1e-12)abort();
 if(fabs(3*output[0]+output[2]-1)>1e-12||fabs(output[0]+2*output[2])>1e-12)abort();
 puts("Numeric buffer ABI PASS: singular absence, short capacity, nonfinite input/aggregate, unchanged output canaries and complete inverse publication.");
 return 0;
}
